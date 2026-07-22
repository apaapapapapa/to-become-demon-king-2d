using System;
using System.Collections.Generic;
using DemonKing.Field.Composition;
using DemonKing.Field.Prototype;
using NUnit.Framework;
using UnityEngine;

namespace DemonKing.Tests.EditMode
{
    public sealed class FieldCompositionTests
    {
        [Test]
        public void Pipeline_Installerを定義順に実行する()
        {
            var executionOrder = new List<string>();
            var pipeline = new FieldCompositionPipeline<List<string>>(
                new IFieldInstaller<List<string>>[]
                {
                    new RecordingInstaller("terrain"),
                    new RecordingInstaller("gameplay"),
                    new RecordingInstaller("camera")
                });

            pipeline.Install(executionOrder);

            Assert.That(
                executionOrder,
                Is.EqualTo(new[] { "terrain", "gameplay", "camera" }));
        }

        [Test]
        public void FieldDefinition_StableIdとDefaultEntryPointを解決できる()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            PrototypeFieldDefinition definition =
                PrototypeFieldDefinition.CreateInitial(
                    projectAssets.ApplicationSettings,
                    projectAssets);

            FieldEntryPoint entryPoint =
                definition.ResolveEntryPoint(definition.DefaultEntryPointId);

            Assert.That(definition.FieldId, Is.Not.Empty);
            Assert.That(definition.SceneName, Is.Not.Empty);
            Assert.That(definition.TrainingScenario, Is.SameAs(projectAssets.TrainingScenario));
            Assert.That(definition.ProgressionPickups, Is.SameAs(projectAssets.ProgressionPickups));
            Assert.That(entryPoint.EntryPointId, Is.EqualTo(definition.DefaultEntryPointId));
            Assert.That(entryPoint.Position, Is.EqualTo(projectAssets.ApplicationSettings.PlayerSpawnPosition));
            Assert.That(
                definition.DefaultLocation,
                Is.EqualTo(new FieldLocation(definition.FieldId, definition.DefaultEntryPointId)));
        }

        [Test]
        public void FieldCatalog_StableFieldIdとEntryPointIdでDefinitionを解決する()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            PrototypeFieldCatalog catalog = PrototypeFieldCatalog.CreateInitial(
                projectAssets.ApplicationSettings,
                projectAssets);

            bool resolved = catalog.TryResolve(
                catalog.InitialField.DefaultLocation,
                out PrototypeFieldDefinition definition,
                out FieldEntryPoint entryPoint);

            Assert.That(resolved, Is.True);
            Assert.That(definition, Is.SameAs(catalog.InitialField));
            Assert.That(entryPoint.EntryPointId, Is.EqualTo(definition.DefaultEntryPointId));
        }

        [Test]
        public void FieldDefinition_EntryPointId重複を拒否する()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");

            Assert.That(
                () => new PrototypeFieldDefinition(
                    "field.test",
                    "TestScene",
                    "Test Field",
                    "entry.same",
                    new[]
                    {
                        new FieldEntryPoint("entry.same", Vector3.zero),
                        new FieldEntryPoint("entry.same", Vector3.one)
                    },
                    8,
                    projectAssets),
                Throws.TypeOf<ArgumentException>());
        }

        private sealed class RecordingInstaller : IFieldInstaller<List<string>>
        {
            private readonly string name;

            public RecordingInstaller(string name)
            {
                this.name = name;
            }

            public void Install(List<string> context)
            {
                context.Add(name);
            }
        }
    }
}
