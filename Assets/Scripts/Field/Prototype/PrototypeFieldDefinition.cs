using System;
using System.Collections.Generic;
using DemonKing.Field.Composition;
using DemonKing.Field.Prototype.Configuration;
using DemonKing.Gameplay.Characters.Configuration;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Field内に配置する出口の静的定義です。
    /// 遷移先はScene名ではなくStable Field / Entry Point IDで保持します。
    /// </summary>
    internal readonly struct PrototypeFieldTransitionDefinition
    {
        public PrototypeFieldTransitionDefinition(
            string transitionId,
            string displayName,
            Vector3 position,
            FieldLocation destination)
        {
            if (string.IsNullOrWhiteSpace(transitionId))
            {
                throw new ArgumentException("Transition IDは必須です。", nameof(transitionId));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Transition表示名は必須です。", nameof(displayName));
            }

            if (!destination.IsValid)
            {
                throw new ArgumentException("遷移先Field Locationが不正です。", nameof(destination));
            }

            TransitionId = transitionId;
            DisplayName = displayName;
            Position = position;
            Destination = destination;
        }

        public string TransitionId { get; }
        public string DisplayName { get; }
        public Vector3 Position { get; }
        public FieldLocation Destination { get; }
    }

    /// <summary>
    /// Prototype Fieldの静的定義です。
    /// Field ID、Scene、Entry Point、Scenario / Content、World Asset参照をRuntime Compositionから分離します。
    /// </summary>
    internal sealed class PrototypeFieldDefinition
    {
        public const string DefaultFieldId = "field.prototype.training_ground";
        public const string DefaultSceneName = "Prototype";
        public const string DefaultEntryPointId = "entry.default";
        public const string DefaultDisplayName = "夕映えの学園草原";

        public const string SecondaryFieldId = "field.prototype.forest_gate";
        public const string SecondarySceneName = "PrototypeForestGate";
        public const string SecondaryEntryPointId = "entry.from_training_ground";
        public const string SecondaryDisplayName = "薄明の森門";
        public const string ReturnFromSecondaryEntryPointId = "entry.from_forest_gate";

        private readonly Dictionary<string, FieldEntryPoint> entryPoints;
        private readonly PrototypeFieldTransitionDefinition[] transitions;
        private readonly bool includesTrainingScenario;
        private readonly bool includesProgressionPickups;

        public PrototypeFieldDefinition(
            string fieldId,
            string sceneName,
            string displayName,
            string defaultEntryPointId,
            IEnumerable<FieldEntryPoint> entryPoints,
            int playableTileRadius,
            PrototypeProjectAssets projectAssets)
            : this(
                fieldId,
                sceneName,
                displayName,
                defaultEntryPointId,
                entryPoints,
                playableTileRadius,
                projectAssets,
                Array.Empty<PrototypeFieldTransitionDefinition>(),
                includesTrainingScenario: true,
                includesProgressionPickups: true)
        {
        }

        public PrototypeFieldDefinition(
            string fieldId,
            string sceneName,
            string displayName,
            string defaultEntryPointId,
            IEnumerable<FieldEntryPoint> entryPoints,
            int playableTileRadius,
            PrototypeProjectAssets projectAssets,
            IEnumerable<PrototypeFieldTransitionDefinition> fieldTransitions,
            bool includesTrainingScenario,
            bool includesProgressionPickups)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
            {
                throw new ArgumentException("Field IDは必須です。", nameof(fieldId));
            }

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                throw new ArgumentException("Scene名は必須です。", nameof(sceneName));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Field表示名は必須です。", nameof(displayName));
            }

            if (string.IsNullOrWhiteSpace(defaultEntryPointId))
            {
                throw new ArgumentException("Default Entry Point IDは必須です。", nameof(defaultEntryPointId));
            }

            if (entryPoints == null)
            {
                throw new ArgumentNullException(nameof(entryPoints));
            }

            if (fieldTransitions == null)
            {
                throw new ArgumentNullException(nameof(fieldTransitions));
            }

            ProjectAssets = projectAssets != null
                ? projectAssets
                : throw new ArgumentNullException(nameof(projectAssets));
            FieldId = fieldId;
            SceneName = sceneName;
            DisplayName = displayName;
            ConfiguredDefaultEntryPointId = defaultEntryPointId;
            PlayableTileRadius = Mathf.Max(4, playableTileRadius);
            this.includesTrainingScenario = includesTrainingScenario;
            this.includesProgressionPickups = includesProgressionPickups;

            this.entryPoints = new Dictionary<string, FieldEntryPoint>(StringComparer.Ordinal);
            foreach (FieldEntryPoint entryPoint in entryPoints)
            {
                if (this.entryPoints.ContainsKey(entryPoint.EntryPointId))
                {
                    throw new ArgumentException(
                        $"Entry Point IDが重複しています: {entryPoint.EntryPointId}",
                        nameof(entryPoints));
                }

                this.entryPoints.Add(entryPoint.EntryPointId, entryPoint);
            }

            if (!this.entryPoints.ContainsKey(defaultEntryPointId))
            {
                throw new ArgumentException(
                    $"Default Entry Pointが定義されていません: {defaultEntryPointId}",
                    nameof(defaultEntryPointId));
            }

            var transitionList = new List<PrototypeFieldTransitionDefinition>();
            var transitionIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (PrototypeFieldTransitionDefinition transition in fieldTransitions)
            {
                if (!transitionIds.Add(transition.TransitionId))
                {
                    throw new ArgumentException(
                        $"Transition IDが重複しています: {transition.TransitionId}",
                        nameof(fieldTransitions));
                }

                transitionList.Add(transition);
            }

            transitions = transitionList.ToArray();
        }

        public string FieldId { get; }
        public string SceneName { get; }
        public string DisplayName { get; }
        public string ConfiguredDefaultEntryPointId { get; }
        public int PlayableTileRadius { get; }
        public PrototypeProjectAssets ProjectAssets { get; }
        public TrainingScenarioDefinition TrainingScenario =>
            includesTrainingScenario ? ProjectAssets.TrainingScenario : null;
        public IReadOnlyList<PrototypeProgressionPickupDefinition> ProgressionPickups =>
            includesProgressionPickups
                ? ProjectAssets.ProgressionPickups
                : Array.Empty<PrototypeProgressionPickupDefinition>();
        public CharacterDefinition PlayerCharacter => ProjectAssets.PlayerCharacter;
        public FieldLocation DefaultLocation => new(FieldId, ConfiguredDefaultEntryPointId);
        public IReadOnlyCollection<FieldEntryPoint> EntryPoints => entryPoints.Values;
        public IReadOnlyList<PrototypeFieldTransitionDefinition> Transitions => transitions;

        public bool TryResolveEntryPoint(string entryPointId, out FieldEntryPoint entryPoint)
        {
            string resolvedId = string.IsNullOrWhiteSpace(entryPointId)
                ? ConfiguredDefaultEntryPointId
                : entryPointId;
            return entryPoints.TryGetValue(resolvedId, out entryPoint);
        }

        public FieldEntryPoint ResolveEntryPoint(string entryPointId)
        {
            if (TryResolveEntryPoint(entryPointId, out FieldEntryPoint entryPoint))
            {
                return entryPoint;
            }

            throw new KeyNotFoundException(
                $"Field '{FieldId}' にEntry Point '{entryPointId}' は定義されていません。");
        }

        public static PrototypeFieldDefinition CreateInitial(
            PrototypeApplicationSettings settings,
            PrototypeProjectAssets projectAssets)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return new PrototypeFieldDefinition(
                settings.InitialFieldId,
                settings.InitialSceneName,
                settings.InitialFieldDisplayName,
                settings.DefaultEntryPointId,
                new[]
                {
                    new FieldEntryPoint(
                        settings.DefaultEntryPointId,
                        settings.PlayerSpawnPosition),
                    new FieldEntryPoint(
                        ReturnFromSecondaryEntryPointId,
                        new Vector3(-5.25f, 3.15f, -1f))
                },
                settings.PlayableTileRadius,
                projectAssets,
                new[]
                {
                    new PrototypeFieldTransitionDefinition(
                        "transition.training_to_forest_gate",
                        "森門へ",
                        new Vector3(5.25f, 3.15f, -1f),
                        new FieldLocation(SecondaryFieldId, SecondaryEntryPointId))
                },
                includesTrainingScenario: true,
                includesProgressionPickups: true);
        }

        public static PrototypeFieldDefinition CreateSecondary(PrototypeProjectAssets projectAssets)
        {
            return new PrototypeFieldDefinition(
                SecondaryFieldId,
                SecondarySceneName,
                SecondaryDisplayName,
                SecondaryEntryPointId,
                new[]
                {
                    new FieldEntryPoint(
                        SecondaryEntryPointId,
                        new Vector3(-4.5f, -1.5f, -1f))
                },
                12,
                projectAssets,
                new[]
                {
                    new PrototypeFieldTransitionDefinition(
                        "transition.forest_gate_to_training",
                        "訓練場へ戻る",
                        new Vector3(4.5f, 2.5f, -1f),
                        new FieldLocation(DefaultFieldId, ReturnFromSecondaryEntryPointId))
                },
                includesTrainingScenario: false,
                includesProgressionPickups: false);
        }

        public static PrototypeFieldDefinition CreateLegacy(
            Vector3 playerSpawnPosition,
            int playableTileRadius,
            PrototypeProjectAssets projectAssets)
        {
            return new PrototypeFieldDefinition(
                DefaultFieldId,
                DefaultSceneName,
                DefaultDisplayName,
                DefaultEntryPointId,
                new[] { new FieldEntryPoint(DefaultEntryPointId, playerSpawnPosition) },
                playableTileRadius,
                projectAssets);
        }
    }

    /// <summary>
    /// Stable Field IDからField Definitionを解決します。
    /// Field追加時はDefinitionを登録するだけで、Game SessionやWorld Composerを複製しません。
    /// </summary>
    internal sealed class PrototypeFieldCatalog
    {
        private readonly Dictionary<string, PrototypeFieldDefinition> definitions;

        public PrototypeFieldCatalog(
            IEnumerable<PrototypeFieldDefinition> definitions,
            string initialFieldId)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            this.definitions = new Dictionary<string, PrototypeFieldDefinition>(StringComparer.Ordinal);
            foreach (PrototypeFieldDefinition definition in definitions)
            {
                if (definition == null)
                {
                    throw new ArgumentException("Field Definitionにnullを含めることはできません。", nameof(definitions));
                }

                if (this.definitions.ContainsKey(definition.FieldId))
                {
                    throw new ArgumentException(
                        $"Field IDが重複しています: {definition.FieldId}",
                        nameof(definitions));
                }

                this.definitions.Add(definition.FieldId, definition);
            }

            if (!this.definitions.TryGetValue(initialFieldId, out PrototypeFieldDefinition initialField))
            {
                throw new ArgumentException(
                    $"Initial FieldがCatalogへ登録されていません: {initialFieldId}",
                    nameof(initialFieldId));
            }

            InitialField = initialField;
        }

        public PrototypeFieldDefinition InitialField { get; }
        public IReadOnlyCollection<PrototypeFieldDefinition> Definitions => definitions.Values;

        public bool TryResolve(
            FieldLocation location,
            out PrototypeFieldDefinition definition,
            out FieldEntryPoint entryPoint)
        {
            entryPoint = default;
            if (!location.IsValid ||
                !definitions.TryGetValue(location.FieldId, out definition))
            {
                definition = null;
                return false;
            }

            return definition.TryResolveEntryPoint(location.EntryPointId, out entryPoint);
        }

        public bool TryGetDefinition(string fieldId, out PrototypeFieldDefinition definition)
        {
            return definitions.TryGetValue(fieldId ?? string.Empty, out definition);
        }

        public static PrototypeFieldCatalog CreateInitial(
            PrototypeApplicationSettings settings,
            PrototypeProjectAssets projectAssets)
        {
            PrototypeFieldDefinition initialField =
                PrototypeFieldDefinition.CreateInitial(settings, projectAssets);
            PrototypeFieldDefinition secondaryField =
                PrototypeFieldDefinition.CreateSecondary(projectAssets);
            return new PrototypeFieldCatalog(
                new[] { initialField, secondaryField },
                initialField.FieldId);
        }
    }
}
