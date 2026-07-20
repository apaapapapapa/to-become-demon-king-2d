using System;
using System.Collections.Generic;
using DemonKing.Domain;
using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Modifiers;
using DemonKing.Gameplay.Modifiers.Configuration;
using DemonKing.Gameplay.Progression.Configuration;

namespace DemonKing.Gameplay.Progression
{
    public delegate bool EvolutionArtRankResolver(string artId, out int rank);

    public enum EvolutionEvaluationStatus
    {
        Available = 0,
        InvalidEvolutionId = 1,
        DefinitionNotFound = 2,
        AlreadyUnlocked = 3,
        RequirementsNotMet = 4
    }

    public enum EvolutionRequirementKind
    {
        Character = 0,
        Level = 1,
        Skill = 2,
        ArtRank = 3,
        EvolutionNode = 4,
        ExclusiveChoice = 5
    }

    public readonly struct EvolutionRequirementFailure
    {
        internal EvolutionRequirementFailure(
            EvolutionRequirementKind kind,
            string contentId,
            int requiredValue = 0,
            int currentValue = 0)
        {
            Kind = kind;
            ContentId = contentId ?? string.Empty;
            RequiredValue = requiredValue;
            CurrentValue = currentValue;
        }

        public EvolutionRequirementKind Kind { get; }
        public string ContentId { get; }
        public int RequiredValue { get; }
        public int CurrentValue { get; }
    }

    public readonly struct EvolutionEvaluationResult
    {
        internal EvolutionEvaluationResult(
            EvolutionEvaluationStatus status,
            string evolutionNodeId,
            IReadOnlyList<EvolutionRequirementFailure> failures)
        {
            Status = status;
            EvolutionNodeId = evolutionNodeId ?? string.Empty;
            Failures = failures ?? Array.Empty<EvolutionRequirementFailure>();
        }

        public EvolutionEvaluationStatus Status { get; }
        public string EvolutionNodeId { get; }
        public IReadOnlyList<EvolutionRequirementFailure> Failures { get; }
        public bool IsAvailable => Status == EvolutionEvaluationStatus.Available;
    }

    public enum EvolutionApplyStatus
    {
        Succeeded = 0,
        Rejected = 1
    }

    public readonly struct EvolutionApplyResult
    {
        internal EvolutionApplyResult(
            EvolutionApplyStatus status,
            EvolutionEvaluationResult evaluation)
        {
            Status = status;
            Evaluation = evaluation;
        }

        public EvolutionApplyStatus Status { get; }
        public EvolutionEvaluationResult Evaluation { get; }
        public string EvolutionNodeId => Evaluation.EvolutionNodeId;
        public bool Succeeded => Status == EvolutionApplyStatus.Succeeded;
    }

    /// <summary>
    /// Evolution条件、排他性、不可逆なNode取得を一か所で評価・実行します。
    /// UIや取得元は評価結果を利用し、成長状態を直接書き換えません。
    /// </summary>
    public sealed class EvolutionProgressionService
    {
        private const string EvolutionIdPrefix = "evolution.";

        private readonly CharacterProgressionState progressionState;
        private readonly EvolutionArtRankResolver artRankResolver;
        private readonly Dictionary<string, EvolutionDefinition> definitions =
            new(StringComparer.Ordinal);
        private readonly List<EvolutionDefinition> definitionOrder = new();

        public EvolutionProgressionService(
            CharacterProgressionState progressionState,
            IEnumerable<EvolutionDefinition> evolutionDefinitions,
            EvolutionArtRankResolver artRankResolver = null)
        {
            this.progressionState = progressionState ??
                throw new ArgumentNullException(nameof(progressionState));
            this.artRankResolver = artRankResolver;
            RegisterDefinitions(evolutionDefinitions);
            ValidatePrerequisiteDefinitions();
            ValidateAcyclicPrerequisites();
            ValidateUnlockedExclusiveGroups();
        }

        public event Action<EvolutionApplyResult> EvolutionApplied;

        public IReadOnlyList<EvolutionDefinition> Definitions => definitionOrder;
        public IReadOnlyList<string> UnlockedEvolutionNodeIds =>
            progressionState.UnlockedEvolutionNodeIds;

        public bool TryGetDefinition(
            string evolutionNodeId,
            out EvolutionDefinition definition)
        {
            return definitions.TryGetValue(
                StableContentId.Normalize(evolutionNodeId),
                out definition);
        }

        public EvolutionEvaluationResult Evaluate(string evolutionNodeId)
        {
            string normalizedId = StableContentId.Normalize(evolutionNodeId);
            if (!IsValidEvolutionId(normalizedId))
            {
                return Result(EvolutionEvaluationStatus.InvalidEvolutionId, normalizedId);
            }

            if (!definitions.TryGetValue(normalizedId, out EvolutionDefinition definition))
            {
                return Result(EvolutionEvaluationStatus.DefinitionNotFound, normalizedId);
            }

            if (progressionState.IsEvolutionNodeUnlocked(normalizedId))
            {
                return Result(EvolutionEvaluationStatus.AlreadyUnlocked, normalizedId);
            }

            var failures = new List<EvolutionRequirementFailure>();
            EvaluateCharacter(definition, failures);
            EvaluateLevel(definition, failures);
            EvaluateSkills(definition, failures);
            EvaluateArtRanks(definition, failures);
            EvaluateEvolutionPrerequisites(definition, failures);
            EvaluateExclusiveGroup(definition, failures);

            return failures.Count == 0
                ? Result(EvolutionEvaluationStatus.Available, normalizedId)
                : new EvolutionEvaluationResult(
                    EvolutionEvaluationStatus.RequirementsNotMet,
                    normalizedId,
                    failures);
        }

        public EvolutionApplyResult Evolve(string evolutionNodeId)
        {
            EvolutionEvaluationResult evaluation = Evaluate(evolutionNodeId);
            if (!evaluation.IsAvailable ||
                !progressionState.TryUnlockEvolutionNode(evaluation.EvolutionNodeId))
            {
                return new EvolutionApplyResult(EvolutionApplyStatus.Rejected, evaluation);
            }

            var result = new EvolutionApplyResult(EvolutionApplyStatus.Succeeded, evaluation);
            EvolutionApplied?.Invoke(result);
            return result;
        }

        public NumericModifier GetModifier(GameplayModifierTarget target, string contentId)
        {
            string normalizedContentId = StableContentId.Normalize(contentId);
            NumericModifier result = NumericModifier.Identity;
            foreach (string nodeId in progressionState.UnlockedEvolutionNodeIds)
            {
                if (!definitions.TryGetValue(nodeId, out EvolutionDefinition definition))
                {
                    continue;
                }

                foreach (GameplayModifierEntry entry in definition.Modifiers)
                {
                    if (entry.AppliesTo(target, normalizedContentId))
                    {
                        result = result.Combine(entry.CreateModifier());
                    }
                }
            }

            return result;
        }

        private void EvaluateCharacter(
            EvolutionDefinition definition,
            ICollection<EvolutionRequirementFailure> failures)
        {
            if (!string.Equals(
                    definition.CharacterDefinitionId,
                    progressionState.CharacterDefinitionId,
                    StringComparison.Ordinal))
            {
                failures.Add(new EvolutionRequirementFailure(
                    EvolutionRequirementKind.Character,
                    definition.CharacterDefinitionId));
            }
        }

        private void EvaluateLevel(
            EvolutionDefinition definition,
            ICollection<EvolutionRequirementFailure> failures)
        {
            if (progressionState.Level < definition.RequiredLevel)
            {
                failures.Add(new EvolutionRequirementFailure(
                    EvolutionRequirementKind.Level,
                    string.Empty,
                    definition.RequiredLevel,
                    progressionState.Level));
            }
        }

        private void EvaluateSkills(
            EvolutionDefinition definition,
            ICollection<EvolutionRequirementFailure> failures)
        {
            foreach (string skillId in definition.RequiredSkillIds)
            {
                if (!progressionState.IsSkillUnlocked(skillId))
                {
                    failures.Add(new EvolutionRequirementFailure(
                        EvolutionRequirementKind.Skill,
                        skillId));
                }
            }
        }

        private void EvaluateArtRanks(
            EvolutionDefinition definition,
            ICollection<EvolutionRequirementFailure> failures)
        {
            foreach (EvolutionArtRankRequirement requirement in definition.RequiredArtRanks)
            {
                int currentRank = 0;
                if (artRankResolver == null ||
                    !artRankResolver.Invoke(requirement.ArtId, out currentRank) ||
                    currentRank < requirement.RequiredRank)
                {
                    failures.Add(new EvolutionRequirementFailure(
                        EvolutionRequirementKind.ArtRank,
                        requirement.ArtId,
                        requirement.RequiredRank,
                        currentRank));
                }
            }
        }

        private void EvaluateEvolutionPrerequisites(
            EvolutionDefinition definition,
            ICollection<EvolutionRequirementFailure> failures)
        {
            foreach (string requiredNodeId in definition.RequiredEvolutionNodeIds)
            {
                if (!progressionState.IsEvolutionNodeUnlocked(requiredNodeId))
                {
                    failures.Add(new EvolutionRequirementFailure(
                        EvolutionRequirementKind.EvolutionNode,
                        requiredNodeId));
                }
            }
        }

        private void EvaluateExclusiveGroup(
            EvolutionDefinition definition,
            ICollection<EvolutionRequirementFailure> failures)
        {
            foreach (string unlockedNodeId in progressionState.UnlockedEvolutionNodeIds)
            {
                if (definitions.TryGetValue(
                        unlockedNodeId,
                        out EvolutionDefinition unlockedDefinition) &&
                    string.Equals(
                        unlockedDefinition.ExclusiveGroupId,
                        definition.ExclusiveGroupId,
                        StringComparison.Ordinal))
                {
                    failures.Add(new EvolutionRequirementFailure(
                        EvolutionRequirementKind.ExclusiveChoice,
                        unlockedNodeId));
                    return;
                }
            }
        }

        private void RegisterDefinitions(IEnumerable<EvolutionDefinition> evolutionDefinitions)
        {
            if (evolutionDefinitions == null)
            {
                throw new ArgumentNullException(nameof(evolutionDefinitions));
            }

            foreach (EvolutionDefinition definition in evolutionDefinitions)
            {
                if (definition == null || !definition.IsConfigured)
                {
                    throw new ArgumentException(
                        "正しく設定されたEvolutionDefinitionだけを登録できます。",
                        nameof(evolutionDefinitions));
                }

                if (!definitions.TryAdd(definition.EvolutionNodeId, definition))
                {
                    throw new ArgumentException(
                        $"Evolution Node IDが重複しています: {definition.EvolutionNodeId}",
                        nameof(evolutionDefinitions));
                }

                definitionOrder.Add(definition);
            }
        }

        private void ValidatePrerequisiteDefinitions()
        {
            foreach (EvolutionDefinition definition in definitions.Values)
            {
                foreach (string requiredNodeId in definition.RequiredEvolutionNodeIds)
                {
                    if (!definitions.ContainsKey(requiredNodeId))
                    {
                        throw new ArgumentException(
                            $"前提Evolution Node Definitionが見つかりません: {requiredNodeId}");
                    }
                }
            }
        }

        private void ValidateAcyclicPrerequisites()
        {
            var visiting = new HashSet<string>(StringComparer.Ordinal);
            var visited = new HashSet<string>(StringComparer.Ordinal);
            foreach (string nodeId in definitions.Keys)
            {
                VisitPrerequisites(nodeId, visiting, visited);
            }
        }

        private void VisitPrerequisites(
            string nodeId,
            ISet<string> visiting,
            ISet<string> visited)
        {
            if (visited.Contains(nodeId))
            {
                return;
            }

            if (!visiting.Add(nodeId))
            {
                throw new ArgumentException(
                    $"Evolution Nodeの前提関係が循環しています: {nodeId}");
            }

            foreach (string requiredNodeId in definitions[nodeId].RequiredEvolutionNodeIds)
            {
                VisitPrerequisites(requiredNodeId, visiting, visited);
            }

            visiting.Remove(nodeId);
            visited.Add(nodeId);
        }

        private void ValidateUnlockedExclusiveGroups()
        {
            var selectedGroups = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string nodeId in progressionState.UnlockedEvolutionNodeIds)
            {
                if (!definitions.TryGetValue(nodeId, out EvolutionDefinition definition))
                {
                    continue;
                }

                if (selectedGroups.TryGetValue(
                        definition.ExclusiveGroupId,
                        out string selectedNodeId))
                {
                    throw new InvalidOperationException(
                        $"同じ排他グループのEvolution Nodeが複数選択されています: " +
                        $"{selectedNodeId}, {nodeId}");
                }

                selectedGroups.Add(definition.ExclusiveGroupId, nodeId);
            }
        }

        private static bool IsValidEvolutionId(string evolutionNodeId)
        {
            return StableContentId.IsValid(evolutionNodeId) &&
                   evolutionNodeId.StartsWith(EvolutionIdPrefix, StringComparison.Ordinal);
        }

        private static EvolutionEvaluationResult Result(
            EvolutionEvaluationStatus status,
            string evolutionNodeId)
        {
            return new EvolutionEvaluationResult(
                status,
                evolutionNodeId,
                Array.Empty<EvolutionRequirementFailure>());
        }
    }
}
