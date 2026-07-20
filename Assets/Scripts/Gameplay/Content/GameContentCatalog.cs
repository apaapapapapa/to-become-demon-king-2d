using System;
using System.Collections.Generic;
using DemonKing.Domain;

namespace DemonKing.Gameplay.Content
{
    /// <summary>
    /// Stable Content IDから静的コンテンツDefinitionを解決する読み取り専用カタログです。
    /// Composition Rootが利用可能なDefinitionを登録し、図鑑UI等の参照基盤として使用します。
    /// </summary>
    public sealed class GameContentCatalog
    {
        private readonly Dictionary<string, IGameContentDefinition> definitionsById =
            new(StringComparer.Ordinal);

        public GameContentCatalog(IEnumerable<IGameContentDefinition> definitions)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            foreach (IGameContentDefinition definition in definitions)
            {
                if (definition == null)
                {
                    throw new ArgumentException(
                        "Content Definitionにnullを登録することはできません。",
                        nameof(definitions));
                }

                if (!StableContentId.IsValid(definition.ContentId))
                {
                    throw new ArgumentException(
                        $"Stable Content IDが不正です: {definition.ContentId}",
                        nameof(definitions));
                }

                if (definitionsById.ContainsKey(definition.ContentId))
                {
                    throw new ArgumentException(
                        $"Stable Content IDが重複しています: {definition.ContentId}",
                        nameof(definitions));
                }

                definitionsById.Add(definition.ContentId, definition);
            }
        }

        public IReadOnlyCollection<IGameContentDefinition> Definitions => definitionsById.Values;

        public bool TryGet(string contentId, out IGameContentDefinition definition)
        {
            return definitionsById.TryGetValue(contentId, out definition);
        }

        public IReadOnlyList<IGameContentDefinition> GetVisibleEncyclopediaEntries()
        {
            var entries = new List<IGameContentDefinition>();
            foreach (IGameContentDefinition definition in definitionsById.Values)
            {
                if (definition.VisibleInEncyclopedia)
                {
                    entries.Add(definition);
                }
            }

            return entries;
        }

        public IReadOnlyList<TDefinition> GetVisibleEncyclopediaEntries<TDefinition>()
            where TDefinition : class, IGameContentDefinition
        {
            var entries = new List<TDefinition>();
            foreach (IGameContentDefinition definition in definitionsById.Values)
            {
                if (definition is TDefinition typedDefinition &&
                    typedDefinition.VisibleInEncyclopedia)
                {
                    entries.Add(typedDefinition);
                }
            }

            return entries;
        }
    }
}
