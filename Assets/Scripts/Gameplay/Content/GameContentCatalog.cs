using System;
using System.Collections.Generic;
using DemonKing.Domain;

namespace DemonKing.Gameplay.Content
{
    /// <summary>
    /// Stable Content IDから静的コンテンツDefinitionを解決する読み取り専用カタログです。
    /// Root Definitionから到達可能な子DefinitionはGameContentDefinitionCollectorで再帰収集します。
    /// </summary>
    public sealed class GameContentCatalog
    {
        private readonly Dictionary<string, IGameContentDefinition> definitionsById =
            new(StringComparer.Ordinal);

        public GameContentCatalog(IEnumerable<IGameContentDefinition> rootDefinitions)
        {
            IReadOnlyList<IGameContentDefinition> definitions =
                GameContentDefinitionCollector.Collect(rootDefinitions);

            foreach (IGameContentDefinition definition in definitions)
            {
                string contentId = StableContentId.Normalize(definition.ContentId);
                definitionsById.Add(contentId, definition);
            }
        }

        public IReadOnlyCollection<IGameContentDefinition> Definitions => definitionsById.Values;

        public bool TryGet(string contentId, out IGameContentDefinition definition)
        {
            return definitionsById.TryGetValue(StableContentId.Normalize(contentId), out definition);
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
