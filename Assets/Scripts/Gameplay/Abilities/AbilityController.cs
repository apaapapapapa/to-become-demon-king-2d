using System;
using System.Collections.Generic;
using DemonKing.Domain;
using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Modifiers;
using UnityEngine;

namespace DemonKing.Gameplay.Abilities
{
    /// <summary>
    /// キャラクター個体のAbility使用可否、Runtime State、Executorへの委譲を管理します。
    /// プレイヤー入力にもAIにも依存しない共通の実行入口です。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AbilityController : MonoBehaviour
    {
        [SerializeField] private AbilityDefinition[] initialAbilities = Array.Empty<AbilityDefinition>();

        private readonly Dictionary<string, AbilityDefinition> definitions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, AbilityRuntimeState> runtimeStates = new(StringComparer.Ordinal);
        private readonly List<IAbilityExecutor> executors = new();

        public event Action<AbilityUseResult> AbilityUseResolved;
        public event Action<AbilityEffectResolved> EffectResolved;

        private void Awake()
        {
            RefreshExecutors();

            if (initialAbilities != null && initialAbilities.Length > 0)
            {
                Configure(initialAbilities);
            }
        }

        private void Update()
        {
            foreach (AbilityRuntimeState state in runtimeStates.Values)
            {
                state.Advance(Time.deltaTime);
            }
        }

        public void Configure(IEnumerable<AbilityDefinition> abilityDefinitions)
        {
            if (abilityDefinitions == null)
            {
                throw new ArgumentNullException(nameof(abilityDefinitions));
            }

            definitions.Clear();
            runtimeStates.Clear();

            foreach (AbilityDefinition definition in abilityDefinitions)
            {
                if (!TryAddDefinition(definition))
                {
                    throw new ArgumentException(
                        $"Ability IDが重複しています: {definition.AbilityId}",
                        nameof(abilityDefinitions));
                }
            }

            RefreshExecutors();
        }

        public bool GrantAbility(AbilityDefinition definition)
        {
            ValidateDefinition(definition, nameof(definition));

            if (definitions.TryGetValue(definition.AbilityId, out AbilityDefinition existing))
            {
                if (ReferenceEquals(existing, definition))
                {
                    return false;
                }

                throw new InvalidOperationException(
                    $"同じAbility IDへ異なるDefinitionを付与できません: {definition.AbilityId}");
            }

            definitions.Add(definition.AbilityId, definition);
            runtimeStates.Add(
                definition.AbilityId,
                new AbilityRuntimeState(definition.AbilityId));
            return true;
        }

        public bool HasAbility(string abilityId)
        {
            string normalizedId = StableContentId.Normalize(abilityId);
            return definitions.ContainsKey(normalizedId);
        }

        public void RefreshExecutors()
        {
            executors.Clear();

            MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IAbilityExecutor executor)
                {
                    executors.Add(executor);
                }
            }
        }

        public AbilityUseResult CanUse(
            string abilityId,
            GameObject user,
            AbilityExecutionInput input)
        {
            AbilityUseStatus status = EvaluateUse(
                abilityId,
                user,
                input,
                Guid.Empty,
                out AbilityExecutionRequest request,
                out AbilityRuntimeState state,
                out IAbilityExecutor executor,
                out IAbilityCostSource costSource);

            return new AbilityUseResult(status, request.Definition?.AbilityId ?? abilityId, state);
        }

        public AbilityUseResult TryUse(
            string abilityId,
            GameObject user,
            AbilityExecutionInput input)
        {
            Guid executionId = Guid.NewGuid();
            AbilityUseStatus status = EvaluateUse(
                abilityId,
                user,
                input,
                executionId,
                out AbilityExecutionRequest request,
                out AbilityRuntimeState state,
                out IAbilityExecutor executor,
                out IAbilityCostSource costSource);

            if (status != AbilityUseStatus.Succeeded)
            {
                return Notify(new AbilityUseResult(
                    status,
                    request.Definition?.AbilityId ?? abilityId,
                    state,
                    executionId));
            }

            AbilityCost cost = request.Definition.Cost;
            if (!cost.IsFree && !costSource.TrySpend(request.Definition, cost))
            {
                return Notify(new AbilityUseResult(
                    AbilityUseStatus.CostUnavailable,
                    request.Definition.AbilityId,
                    state,
                    executionId));
            }

            state.BeginExecution();

            try
            {
                AbilityExecutionResult executionResult = executor.Execute(request);
                state.CommitUse(
                    ResolveCooldown(request.Definition),
                    executionResult.IsComplete);
            }
            catch
            {
                state.CancelExecution();
                throw;
            }

            return Notify(new AbilityUseResult(
                AbilityUseStatus.Succeeded,
                request.Definition.AbilityId,
                state,
                executionId));
        }

        public bool TryGetRuntimeState(string abilityId, out AbilityRuntimeState state)
        {
            string normalizedId = StableContentId.Normalize(abilityId);
            return runtimeStates.TryGetValue(normalizedId, out state);
        }

        public bool CompleteExecution(string abilityId, GameObject user)
        {
            if (user != gameObject || !TryGetRuntimeState(abilityId, out AbilityRuntimeState state))
            {
                return false;
            }

            state.CompleteExecution();
            return true;
        }

        private AbilityUseStatus EvaluateUse(
            string abilityId,
            GameObject user,
            AbilityExecutionInput input,
            Guid executionId,
            out AbilityExecutionRequest request,
            out AbilityRuntimeState state,
            out IAbilityExecutor executor,
            out IAbilityCostSource costSource)
        {
            request = default;
            state = null;
            executor = null;
            costSource = null;

            if (user == null)
            {
                return AbilityUseStatus.InvalidUser;
            }

            if (user != gameObject)
            {
                return AbilityUseStatus.UserMismatch;
            }

            string normalizedId = StableContentId.Normalize(abilityId);
            if (!StableContentId.IsValid(normalizedId) ||
                !normalizedId.StartsWith("ability.", StringComparison.Ordinal))
            {
                return AbilityUseStatus.InvalidAbilityId;
            }

            if (!definitions.TryGetValue(normalizedId, out AbilityDefinition definition) ||
                !runtimeStates.TryGetValue(normalizedId, out state))
            {
                return AbilityUseStatus.AbilityNotGranted;
            }

            request = new AbilityExecutionRequest(
                executionId,
                user,
                definition,
                input,
                ReportEffect);

            if (state.IsExecuting)
            {
                return AbilityUseStatus.AlreadyExecuting;
            }

            if (state.IsOnCooldown)
            {
                return AbilityUseStatus.CooldownActive;
            }

            executor = FindExecutor(definition);
            if (executor == null)
            {
                return AbilityUseStatus.ExecutorUnavailable;
            }

            if (!executor.CanExecute(request))
            {
                return AbilityUseStatus.ExecutorRejected;
            }

            AbilityCost cost = definition.Cost;
            if (!cost.IsFree)
            {
                costSource = FindCostSource(user, definition, cost);
                if (costSource == null)
                {
                    return AbilityUseStatus.CostUnavailable;
                }
            }

            return AbilityUseStatus.Succeeded;
        }

        private IAbilityExecutor FindExecutor(AbilityDefinition definition)
        {
            foreach (IAbilityExecutor executor in executors)
            {
                if (executor is Behaviour behaviour && !behaviour.isActiveAndEnabled)
                {
                    continue;
                }

                if (executor.Supports(definition))
                {
                    return executor;
                }
            }

            return null;
        }

        private static IAbilityCostSource FindCostSource(
            GameObject user,
            AbilityDefinition definition,
            AbilityCost cost)
        {
            MonoBehaviour[] behaviours = user.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IAbilityCostSource source && source.CanSpend(definition, cost))
                {
                    return source;
                }
            }

            return null;
        }

        private float ResolveCooldown(AbilityDefinition definition)
        {
            NumericModifier modifier = NumericModifier.Identity;
            MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IAbilityCooldownModifierSource source)
                {
                    modifier = modifier.Combine(
                        source.GetAbilityCooldownModifier(definition));
                }
            }

            return modifier.Apply(definition.CooldownSeconds);
        }

        private AbilityUseResult Notify(AbilityUseResult result)
        {
            AbilityUseResolved?.Invoke(result);
            return result;
        }

        private bool TryAddDefinition(AbilityDefinition definition)
        {
            ValidateDefinition(definition, nameof(definition));
            if (!definitions.TryAdd(definition.AbilityId, definition))
            {
                return false;
            }

            runtimeStates.Add(
                definition.AbilityId,
                new AbilityRuntimeState(definition.AbilityId));
            return true;
        }

        private static void ValidateDefinition(AbilityDefinition definition, string parameterName)
        {
            if (definition == null || !definition.IsConfigured)
            {
                throw new ArgumentException(
                    "正しく設定されたAbilityDefinitionだけを登録できます。",
                    parameterName);
            }
        }

        private void ReportEffect(AbilityEffectResolved result)
        {
            EffectResolved?.Invoke(result);
        }
    }
}
