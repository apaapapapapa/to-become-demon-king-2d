using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DemonKing.Domain;

namespace DemonKing.Gameplay.Content
{
    /// <summary>
    /// Root Definitionから到達可能なContent Definitionを再帰収集します。
    /// 同一インスタンスの共有参照は一度だけ登録し、異なるインスタンスによるStable Content ID衝突は設定エラーとして扱います。
    /// </summary>
    public static class GameContentDefinitionCollector
    {
        public static IReadOnlyList<IGameContentDefinition> Collect(
            IEnumerable<IGameContentDefinition> rootDefinitions)
        {
            if (rootDefinitions == null)
            {
                throw new ArgumentNullException(nameof(rootDefinitions));
            }

            var collected = new List<IGameContentDefinition>();
            var visited = new HashSet<IGameContentDefinition>(ReferenceComparer.Instance);
            var definitionsById = new Dictionary<string, IGameContentDefinition>(StringComparer.Ordinal);

            foreach (IGameContentDefinition definition in rootDefinitions)
            {
                Visit(definition, collected, visited, definitionsById, nameof(rootDefinitions));
            }

            return collected;
        }

        private static void Visit(
            IGameContentDefinition definition,
            ICollection<IGameContentDefinition> collected,
            ISet<IGameContentDefinition> visited,
            IDictionary<string, IGameContentDefinition> definitionsById,
            string parameterName)
        {
            if (definition == null)
            {
                throw new ArgumentException(
                    "Content Definitionにnullを登録することはできません。",
                    parameterName);
            }

            if (!visited.Add(definition))
            {
                return;
            }

            string contentId = StableContentId.Normalize(definition.ContentId);
            if (!StableContentId.IsValid(contentId))
            {
                throw new ArgumentException(
                    $"Stable Content IDが不正です: {definition.ContentId}",
                    parameterName);
            }

            if (definitionsById.TryGetValue(contentId, out IGameContentDefinition existing))
            {
                if (!ReferenceEquals(existing, definition))
                {
                    throw new ArgumentException(
                        $"異なるContent Definitionが同じStable Content IDを使用しています: {contentId}",
                        parameterName);
                }

                return;
            }

            definitionsById.Add(contentId, definition);
            collected.Add(definition);

            if (definition is not IGameContentContainer container ||
                container.ChildContentDefinitions == null)
            {
                return;
            }

            foreach (IGameContentDefinition child in container.ChildContentDefinitions)
            {
                Visit(child, collected, visited, definitionsById, parameterName);
            }
        }

        private sealed class ReferenceComparer : IEqualityComparer<IGameContentDefinition>
        {
            public static ReferenceComparer Instance { get; } = new();

            public bool Equals(IGameContentDefinition x, IGameContentDefinition y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(IGameContentDefinition obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
