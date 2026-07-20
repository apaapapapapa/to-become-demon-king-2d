using DemonKing.Gameplay.Abilities.Configuration;

namespace DemonKing.Gameplay.Abilities
{
    /// <summary>
    /// Ability固有の効果を発生させる実装境界です。入力元やSkill、Evolutionを知りません。
    /// </summary>
    public interface IAbilityExecutor
    {
        bool Supports(AbilityDefinition definition);
        bool CanExecute(AbilityExecutionRequest request);
        AbilityExecutionResult Execute(AbilityExecutionRequest request);
    }

    /// <summary>
    /// Abilityの汎用コストをキャラクター固有リソースへ接続する任意境界です。
    /// </summary>
    public interface IAbilityCostSource
    {
        bool CanSpend(AbilityDefinition definition, AbilityCost cost);
        bool TrySpend(AbilityDefinition definition, AbilityCost cost);
    }
}
