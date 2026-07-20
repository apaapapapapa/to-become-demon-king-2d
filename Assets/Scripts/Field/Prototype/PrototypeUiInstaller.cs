using DemonKing.Core.Application;
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
    /// UIはプレイヤーPrefabから独立したシーンライフサイクルで管理し、必要なRuntime Serviceを外側から注入します。
    /// </summary>
    internal static class PrototypeUiInstaller
    {
        public static GameObject Create(
            Font uiFont,
            GamePauseController pauseController,
            DialogueLog dialogueLog,
            EvolutionSelectionController evolutionSelectionController,
            QuestProgressionService questProgressionService)
        {
            GameObject uiRoot = new("UI Root", typeof(RectTransform));

            Canvas canvas = uiRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = uiRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;

            uiRoot.AddComponent<GraphicRaycaster>();

            GameHudView hudView = uiRoot.AddComponent<GameHudView>();
            hudView.Initialize(uiFont);

            DialogueLogView dialogueLogView = uiRoot.AddComponent<DialogueLogView>();
            dialogueLogView.Initialize(uiFont, dialogueLog);

            QuestTrackerView questTrackerView = uiRoot.AddComponent<QuestTrackerView>();
            questTrackerView.Initialize(uiFont, questProgressionService);

            PauseMenuView pauseMenuView = uiRoot.AddComponent<PauseMenuView>();
            pauseMenuView.Initialize(uiFont, pauseController);

            EvolutionMenuView evolutionMenuView = uiRoot.AddComponent<EvolutionMenuView>();
            evolutionMenuView.Initialize(uiFont, evolutionSelectionController);
            return uiRoot;
        }
    }
}
