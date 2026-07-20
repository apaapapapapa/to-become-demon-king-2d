using UnityEngine;

namespace DemonKing.Gameplay.Content
{
    /// <summary>
    /// ゲーム内図鑑と外部Knowledge Baseが共通参照する静的コンテンツ契約です。
    /// プレイヤーごとの発見状態はこのDefinitionへ持たせず、Runtime Stateへ分離します。
    /// </summary>
    public interface IGameContentDefinition
    {
        string ContentId { get; }
        string DisplayName { get; }
        string Description { get; }
        string EncyclopediaDescription { get; }
        Sprite Icon { get; }
        bool VisibleInEncyclopedia { get; }
    }
}
