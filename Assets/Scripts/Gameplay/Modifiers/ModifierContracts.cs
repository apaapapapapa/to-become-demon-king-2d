using DemonKing.Gameplay.Abilities.Configuration;
using DemonKing.Gameplay.Progression.Configuration;

namespace DemonKing.Gameplay.Modifiers
{
    /// <summary>
    /// Abilityの実行基盤が、補正の取得元をSkillなどの成長要素から分離して参照する契約です。
    /// </summary>
    public interface IAbilityCooldownModifierSource
    {
        NumericModifier GetAbilityCooldownModifier(AbilityDefinition definition);
    }

    /// <summary>
    /// Combat効果が、補正の取得元を知らずに使用者の与ダメージ補正を参照する契約です。
    /// </summary>
    public interface IOutgoingDamageModifierSource
    {
        NumericModifier GetOutgoingDamageModifier(AbilityDefinition definition);
    }

    /// <summary>
    /// Art成長処理が、補正の取得元を知らずに熟練ポイント補正を参照する契約です。
    /// </summary>
    public interface IArtMasteryModifierSource
    {
        NumericModifier GetArtMasteryModifier(ArtDefinition definition);
    }
}
