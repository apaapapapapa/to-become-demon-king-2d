using System;
using System.Collections;
using System.IO;
using DemonKing.Core.Application;
using DemonKing.Core.Input;
using DemonKing.Domain.Save;
using DemonKing.Field.Prototype;
using DemonKing.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DemonKing.Tests.PlayMode
{
    public sealed class TitleScreenPlayModeTests
    {
        [UnityTest]
        public IEnumerator TitleScreen_NewGameで空Slotを選びFreshStartを要求する()
        {
            string directory = CreateTemporaryDirectory();
            GameObject root = null;

            try
            {
                PrototypeProjectAssets projectAssets =
                    Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
                var store = new LocalSaveSlotStore(directory, () => 0d, () => DateTime.UtcNow);
                PrototypeGameStartRequest? request = null;

                PrototypeTitleScreenController controller = CreateController(
                    projectAssets,
                    store,
                    value => request = value,
                    out root,
                    out PlayerInputReader inputReader);

                Assert.That(controller.CurrentPage, Is.EqualTo(PrototypeTitlePage.Main));
                Assert.That(controller.ContinueAvailable, Is.False);
                Assert.That(inputReader.CurrentContext, Is.EqualTo(PlayerInputContext.UI));

                controller.SubmitSelection();
                Assert.That(controller.CurrentPage, Is.EqualTo(PrototypeTitlePage.NewGameSlots));
                Assert.That(controller.SlotSelectionIndex, Is.EqualTo(0));

                controller.SubmitSelection();

                Assert.That(request.HasValue, Is.True);
                Assert.That(request.Value.SlotId, Is.EqualTo(SaveSlotId.Slot1));
                Assert.That(request.Value.IsNewGame, Is.True);
                Assert.That(inputReader.CurrentContext, Is.EqualTo(PlayerInputContext.Disabled));
            }
            finally
            {
                if (root != null)
                {
                    Object.Destroy(root);
                }

                DeleteTemporaryDirectory(directory);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator TitleScreen_Continueは最終保存日時が新しい有効Slotを開始する()
        {
            string directory = CreateTemporaryDirectory();
            GameObject root = null;
            double realtimeSeconds = 10d;
            DateTime utcNow = new DateTime(2026, 7, 23, 10, 0, 0, DateTimeKind.Utc);

            try
            {
                var store = new LocalSaveSlotStore(
                    directory,
                    () => realtimeSeconds,
                    () => utcNow);
                store.CreateSaveService(SaveSlotId.Slot1)
                    .Save(CreateSaveData(level: 2, fieldId: "field.old"));

                utcNow = utcNow.AddHours(2);
                store.CreateSaveService(SaveSlotId.Slot2)
                    .Save(CreateSaveData(level: 5, fieldId: "field.latest"));

                PrototypeProjectAssets projectAssets =
                    Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
                PrototypeGameStartRequest? request = null;
                PrototypeTitleScreenController controller = CreateController(
                    projectAssets,
                    store,
                    value => request = value,
                    out root,
                    out _);

                Assert.That(controller.ContinueAvailable, Is.True);
                Assert.That(controller.MoveSelection(1), Is.True);
                Assert.That(controller.MainSelectionIndex, Is.EqualTo(1));

                controller.SubmitSelection();

                Assert.That(request.HasValue, Is.True);
                Assert.That(request.Value.SlotId, Is.EqualTo(SaveSlotId.Slot2));
                Assert.That(request.Value.IsNewGame, Is.False);
            }
            finally
            {
                if (root != null)
                {
                    Object.Destroy(root);
                }

                DeleteTemporaryDirectory(directory);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator TitleScreen_LoadGameは破損Slotを開始しない()
        {
            string directory = CreateTemporaryDirectory();
            GameObject root = null;

            try
            {
                var store = new LocalSaveSlotStore(directory, () => 0d, () => DateTime.UtcNow);
                File.WriteAllText(store.GetSaveFilePath(SaveSlotId.Slot1), string.Empty);
                PrototypeProjectAssets projectAssets =
                    Resources.Load<PrototypeProjectAssets>("Settings/PrototypeProjectAssets");
                PrototypeGameStartRequest? request = null;
                PrototypeTitleScreenController controller = CreateController(
                    projectAssets,
                    store,
                    value => request = value,
                    out root,
                    out _);

                controller.MoveSelection(1);
                controller.MoveSelection(1);
                Assert.That(controller.MainSelectionIndex, Is.EqualTo(2));
                controller.SubmitSelection();

                Assert.That(controller.CurrentPage, Is.EqualTo(PrototypeTitlePage.LoadGameSlots));
                Assert.That(
                    controller.SlotMetadata[0].Status,
                    Is.EqualTo(SaveSlotStatus.Corrupted));

                controller.SubmitSelection();

                Assert.That(request.HasValue, Is.False);
                Assert.That(controller.Cancel(), Is.True);
                Assert.That(controller.CurrentPage, Is.EqualTo(PrototypeTitlePage.Main));
            }
            finally
            {
                if (root != null)
                {
                    Object.Destroy(root);
                }

                DeleteTemporaryDirectory(directory);
            }

            yield return null;
        }

        private static PrototypeTitleScreenController CreateController(
            PrototypeProjectAssets projectAssets,
            LocalSaveSlotStore store,
            Action<PrototypeGameStartRequest> onStart,
            out GameObject root,
            out PlayerInputReader inputReader)
        {
            root = new GameObject("Title Screen Test", typeof(RectTransform));
            root.AddComponent<Canvas>();
            PrototypeTitleScreenView view = root.AddComponent<PrototypeTitleScreenView>();
            view.Initialize(projectAssets.UiFont);
            inputReader = root.AddComponent<PlayerInputReader>();
            PrototypeTitleScreenController controller =
                root.AddComponent<PrototypeTitleScreenController>();
            controller.Initialize(
                projectAssets,
                store,
                inputReader,
                view,
                onStart,
                () => { });
            return controller;
        }

        private static GameSaveData CreateSaveData(int level, string fieldId)
        {
            return new GameSaveData
            {
                version = GameSaveData.CurrentVersion,
                player = new PlayerSaveData
                {
                    characterDefinitionId = "character.player.slime",
                    level = level
                },
                world = new WorldSaveData
                {
                    currentFieldId = fieldId,
                    entryPointId = "entry.default"
                }
            };
        }

        private static string CreateTemporaryDirectory()
        {
            string directory = Path.Combine(
                Path.GetTempPath(),
                "demon-king-title-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            return directory;
        }

        private static void DeleteTemporaryDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
