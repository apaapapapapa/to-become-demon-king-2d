using DemonKing.Core.Input;
using DemonKing.Domain.Progression;
using DemonKing.Field.Composition;
using DemonKing.Field.Prototype;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Quests;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DemonKing.Tests.EditMode
{
    public sealed class FieldTransitionSaveEditModeTests
    {
        [Test]
        public void SnapshotProvider_Field切替中もLoadoutを保持し再Bind後は新FieldLocationを保存する()
        {
            PrototypeProjectAssets projectAssets =
                Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
            CharacterProgressionState progressionState =
                CharacterProgressionState.CreateInitial(projectAssets.PlayerCharacter.CharacterId);
            var questService = new QuestProgressionService(projectAssets.QuestDefinitions);
            ProgressionGrantConsumptionState grantState =
                ProgressionGrantConsumptionState.CreateInitial();

            GameObject firstPlayer = new("Transition Snapshot Player A");
            GameObject secondPlayer = new("Transition Snapshot Player B");
            try
            {
                AbilityLoadoutController firstLoadout =
                    firstPlayer.AddComponent<AbilityLoadoutController>();
                var firstRuntimeLoadout = new AbilityLoadout();
                firstRuntimeLoadout.Assign(AbilitySlot.Action3, "ability.test.transition");
                firstLoadout.Initialize(firstRuntimeLoadout);

                var initialLocation = new FieldLocation(
                    PrototypeFieldDefinition.DefaultFieldId,
                    PrototypeFieldDefinition.DefaultEntryPointId);
                var provider = new PrototypeGameSaveSnapshotProvider(
                    progressionState,
                    firstLoadout,
                    questService,
                    grantState,
                    initialLocation);

                provider.PrepareForFieldTransition();
                Object.DestroyImmediate(firstPlayer);

                var transitionSnapshot = provider.CreateSnapshot();
                Assert.That(transitionSnapshot.world.currentFieldId, Is.EqualTo(initialLocation.FieldId));
                Assert.That(transitionSnapshot.player.abilityLoadout.slots, Has.Count.EqualTo(1));
                Assert.That(
                    transitionSnapshot.player.abilityLoadout.slots[0].abilityId,
                    Is.EqualTo("ability.test.transition"));

                AbilityLoadoutController secondLoadout =
                    secondPlayer.AddComponent<AbilityLoadoutController>();
                var secondRuntimeLoadout = new AbilityLoadout();
                secondRuntimeLoadout.Assign(AbilitySlot.Action3, "ability.test.transition");
                secondLoadout.Initialize(secondRuntimeLoadout);
                var secondaryLocation = new FieldLocation(
                    PrototypeFieldDefinition.SecondaryFieldId,
                    PrototypeFieldDefinition.SecondaryEntryPointId);

                provider.BindWorld(secondLoadout, secondaryLocation);
                var secondarySnapshot = provider.CreateSnapshot();

                Assert.That(
                    secondarySnapshot.world.currentFieldId,
                    Is.EqualTo(PrototypeFieldDefinition.SecondaryFieldId));
                Assert.That(
                    secondarySnapshot.world.entryPointId,
                    Is.EqualTo(PrototypeFieldDefinition.SecondaryEntryPointId));
                Assert.That(secondarySnapshot.player.abilityLoadout.slots, Has.Count.EqualTo(1));
                Assert.That(
                    secondarySnapshot.player.abilityLoadout.slots[0].abilityId,
                    Is.EqualTo("ability.test.transition"));
            }
            finally
            {
                if (firstPlayer != null)
                {
                    Object.DestroyImmediate(firstPlayer);
                }

                if (secondPlayer != null)
                {
                    Object.DestroyImmediate(secondPlayer);
                }
            }
        }
    }
}
