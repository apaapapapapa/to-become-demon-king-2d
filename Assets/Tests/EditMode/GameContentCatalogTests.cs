using System;
using System.Collections.Generic;
using DemonKing.Gameplay.Content;
using NUnit.Framework;
using UnityEngine;

namespace DemonKing.Tests.EditMode
{
    public sealed class GameContentCatalogTests
    {
        [Test]
        public void TryGet_RegisteredContent_ReturnsDefinition()
        {
            var definition = new TestContentDefinition("skill.test.visible", visible: true);
            var catalog = new GameContentCatalog(new[] { definition });

            bool found = catalog.TryGet(definition.ContentId, out IGameContentDefinition actual);

            Assert.That(found, Is.True);
            Assert.That(actual, Is.SameAs(definition));
        }

        [Test]
        public void Constructor_RecursivelyCollectsChildDefinitions()
        {
            var grandchild = new TestContentDefinition("ability.test.child", visible: true);
            var child = new TestContentContainer(
                "art.test.child",
                new IGameContentDefinition[] { grandchild });
            var root = new TestContentContainer(
                "character.test.root",
                new IGameContentDefinition[] { child });

            var catalog = new GameContentCatalog(new IGameContentDefinition[] { root });

            Assert.That(catalog.Definitions, Has.Count.EqualTo(3));
            Assert.That(catalog.TryGet(grandchild.ContentId, out IGameContentDefinition actual), Is.True);
            Assert.That(actual, Is.SameAs(grandchild));
        }

        [Test]
        public void Constructor_SameDefinitionReferencedFromMultiplePaths_RegistersOnce()
        {
            var shared = new TestContentDefinition("ability.test.shared", visible: true);
            var first = new TestContentContainer(
                "art.test.first",
                new IGameContentDefinition[] { shared });
            var second = new TestContentContainer(
                "art.test.second",
                new IGameContentDefinition[] { shared });
            var root = new TestContentContainer(
                "character.test.root",
                new IGameContentDefinition[] { first, second });

            var catalog = new GameContentCatalog(new IGameContentDefinition[] { root });

            Assert.That(catalog.Definitions, Has.Count.EqualTo(4));
            Assert.That(catalog.TryGet(shared.ContentId, out IGameContentDefinition actual), Is.True);
            Assert.That(actual, Is.SameAs(shared));
        }

        [Test]
        public void Constructor_DifferentDefinitionsWithSameStableContentId_ThrowsArgumentException()
        {
            var first = new TestContentDefinition("art.test.duplicate", visible: true);
            var second = new TestContentDefinition("art.test.duplicate", visible: true);

            Assert.Throws<ArgumentException>(() =>
                new GameContentCatalog(new IGameContentDefinition[] { first, second }));
        }

        [Test]
        public void GetVisibleEncyclopediaEntries_HiddenContent_IsExcluded()
        {
            var visible = new TestContentDefinition("character.test.visible", visible: true);
            var hidden = new TestContentDefinition("character.test.hidden", visible: false);
            var catalog = new GameContentCatalog(new IGameContentDefinition[] { visible, hidden });

            var entries = catalog.GetVisibleEncyclopediaEntries();

            Assert.That(entries, Has.Count.EqualTo(1));
            Assert.That(entries[0], Is.SameAs(visible));
        }

        private class TestContentDefinition : IGameContentDefinition
        {
            public TestContentDefinition(string contentId, bool visible)
            {
                ContentId = contentId;
                VisibleInEncyclopedia = visible;
            }

            public string ContentId { get; }
            public string DisplayName => ContentId;
            public string Description => string.Empty;
            public string EncyclopediaDescription => string.Empty;
            public Sprite Icon => null;
            public bool VisibleInEncyclopedia { get; }
        }

        private sealed class TestContentContainer : TestContentDefinition, IGameContentContainer
        {
            public TestContentContainer(
                string contentId,
                IEnumerable<IGameContentDefinition> children)
                : base(contentId, visible: true)
            {
                ChildContentDefinitions = children;
            }

            public IEnumerable<IGameContentDefinition> ChildContentDefinitions { get; }
        }
    }
}
