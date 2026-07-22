using DemonKing.Core.Application;
using DemonKing.Gameplay.Abilities;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Progression;
using DemonKing.Gameplay.Quests;
using DemonKing.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// プロトタイプシーンへCanvas（uGUI）ベースのUIルートを構築します。
    /// HUD系の補助UIはRuntime Compositionを維持し、主要Modal画面はPrefabから生成します。
    /// </summary>
    internal static class PrototypeUiInstaller
    {
        public static GameObject Create(
            Font uiFont,
            GameObject pauseMenuPrefab,
            GameObject evolutionMenuPrefab,
            GameObject abilityLoadoutMenuPrefab,
            GamePauseController pauseController,
            DialogueLog dialogueLog,
            EvolutionSelectionController evolutionSelectionController,
            AbilityLoadoutSelectionController abilityLoadoutSelectionController,
            QuestProgressionService questProgressionService)
        {
            GameObject uiRoot = new("UI Root", typeof(RectTransform));

            Canvas canvas = uiRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = uiRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScaleMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;

            uiRoot.AddComponent<GraphicRaycaster>();

            GameHudView hudView = uiRoot.AddComponent<GameHudView>();
            hudView.Initialize(uiFont);

            DialogueLogView dialogueLogView = uiRoot.AddComponent<DialogueLogView>();
            dialogueLogView.Initialize(uiFont, dialogueLog);

            QuestNotificationView questNotificationView =
                uiRoot.AddComponent<QuestNotificationView>();
            questNotificationView.Initialize(uiFont);

            QuestTrackerView questTrackerView = uiRoot.AddComponent<QuestTrackerView>();
            questTrackerView.Initialize(
                uiFont,
                questProgressionService,
                questNotificationView);

            PauseMenuLayout pauseLayout = InstantiateLayout<PauseMenuLayout>(
                pauseMenuPrefab,
                uiRoot.transform,
                "Pause Menu");
            EvolutionMenuLayout evolutionLayout = InstantiateLayout<EvolutionMenuLayout>(
                evolutionMenuPrefab,
                uiRoot.transform,
                "Evolution Menu");
            AbilityLoadoutMenuLayout loadoutLayout = InstantiateLayout<AbilityLoadoutMenuLayout>(
                abilityLoadoutMenuPrefab,
                uiRoot.transform,
                "Ability Loadout Menu");

            PauseMenuView pauseMenuView = uiRoot.AddComponent<PauseMenuView>();
            pauseMenuView.Initialize(uiFont, pauseController, pauseLayout);

            EvolutionMenuView evolutionMenuView = uiRoot.AddComponent<EvolutionMenuView>();
            evolutionMenuView.Initialize(uiFont, evolutionSelectionController, evolutionLayout);

            AbilityLoadoutMenuView loadoutMenuView = uiRoot.AddComponent<AbilityLoadoutMenuView>();
            loadoutMenuView.Initialize(
                uiFont,
                abilityLoadoutSelectionController,
                loadoutLayout);
            return uiRoot;
        }

        private static TLayout InstantiateLayout<TLayout>(
            GameObject prefab,
            Transform parent,
            string displayName)
            where TLayout : Component
        {
            if (prefab == null)
            {
                Debug.LogError($"{displayName} Prefabが設定されていません。");
                return null;
            }

            GameObject instance = Object.Instantiate(prefab, parent, false);
            TLayout layout = instance.GetComponent<TLayout>();
            if (layout == null)
            {
                Debug.LogError(
                    $"{displayName} Prefabに{typeof(TLayout).Name}がありません。",
                    instance);
            }

            return layout;
        }
    }
}
