using System.Collections.Generic;

namespace DemonKing.Gameplay.Content
{
    /// <summary>
    /// 自身から到達可能な子Content Definitionを公開する契約です。
    /// Catalog構築側が具体的なDefinition型や内部構造を知らずに再帰走査するために使用します。
    /// </summary>
    public interface IGameContentContainer
    {
        IEnumerable<IGameContentDefinition> ChildContentDefinitions { get; }
    }
}
