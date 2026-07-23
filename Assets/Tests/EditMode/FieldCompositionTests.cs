using System;
using System.Collections.Generic;
using System.Linq;
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
                definition.ResolveEntryPoint(definition.ConfiguredDefaultEntryPointId);

            Assert.That(definition.FieldId, Is.Not.Empty);
            Assert.That(definition.SceneName, Is.Not.Empty);
            Assert.That(definition.TrainingScenario, Is.SameAs(projectAssets.TrainingScenario));
            Assert.That(definition.ProgressionPickups, Is.SameAs(projectAssets.ProgressionPickups));
            Assert.That(entryPoint.EntryPointId, Is.EqualTo(definition.ConfiguredDefaultEntryPointId));
            Assert.That(entryPoint.Position, Is.EqualTo(projectAssets.ApplicationSettings.PlayerSpawnPosition));
            Assert.That(
                definition.DefaultLocation,
                Is.EqualTo(new FieldLocation(definition.FieldId, definition.ConfiguredDefaultEntryPointId)));
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
            Assert.That(entryPoint.EntryPointId, Is.EqualTo(definition.ConfiguredDefaultEntryPointId));
        }

        [Test]
        public void FieldCatalog_2つ目のFieldとEntryPointをStableIdで解決する()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            PrototypeFieldCatalog catalog = PrototypeFieldCatalog.CreateInitial(
                projectAssets.ApplicationSettings,
                projectAssets);
            var secondaryLocation = new FieldLocation(
                PrototypeFieldDefinition.SecondaryFieldId,
                PrototypeFieldDefinition.SecondaryEntryPointId);

            bool resolved = catalog.TryResolve(
                secondaryLocation,
                out PrototypeFieldDefinition definition,
                out FieldEntryPoint entryPoint);

            Assert.That(catalog.Definitions.Count, Is.EqualTo(2));
            Assert.That(resolved, Is.True);
            Assert.That(definition.FieldId, Is.EqualTo(PrototypeFieldDefinition.SecondaryFieldId));
            Assert.That(definition.SceneName, Is.EqualTo(PrototypeFieldDefinition.SecondarySceneName));
            Assert.That(entryPoint.EntryPointId, Is.EqualTo(PrototypeFieldDefinition.SecondaryEntryPointId));
            Assert.That(definition.TrainingScenario, Is.Null);
            Assert.That(definition.ProgressionPickups, Is.Empty);
        }

        [Test]
        public void FieldCatalog_FieldAとFieldBの出口が相互のEntryPointを指す()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            PrototypeFieldCatalog catalog = PrototypeFieldCatalog.CreateInitial(
                projectAssets.ApplicationSettings,
                projectAssets);
            PrototypeFieldDefinition initial = catalog.InitialField;
            Assert.That(
                catalog.TryGetDefinition(
                    PrototypeFieldDefinition.SecondaryFieldId,
                    out PrototypeFieldDefinition secondary),
                Is.True);

            PrototypeFieldTransitionDefinition toSecondary = initial.Transitions.Single();
            PrototypeFieldTransitionDefinition toInitial = secondary.Transitions.Single();

            Assert.That(
                toSecondary.Destination,
                Is.EqualTo(new FieldLocation(
                    PrototypeFieldDefinition.SecondaryFieldId,
                    PrototypeFieldDefinition.SecondaryEntryPointId)));
            Assert.That(
                toInitial.Destination,
                Is.EqualTo(new FieldLocation(
                    initial.FieldId,
                    PrototypeFieldDefinition.ReturnFromSecondaryEntryPointId)));
            Assert.That(
                initial.TryResolveEntryPoint(
                    PrototypeFieldDefinition.ReturnFromSecondaryEntryPointId,
                    out _),
                Is.True);
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
