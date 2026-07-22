using DemonKing.Core.Application;
using UnityEngine;

namespace DemonKing.Presentation.UI
{
    /// <summary>
    /// GamePauseControllerの状態をPrefabベースのuGUIへ反映します。
    /// Hierarchy構築、ポーズ状態の決定、Time.timeScale操作は行いません。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class PauseMenuView : MonoBehaviour
    {
        private GamePauseController pauseController;
        private PauseMenuLayout layout;

        public void Initialize(
            Font font,
            GamePauseController controller,
            PauseMenuLayout menuLayout)
        {
            Unbind();
            pauseController = controller;
            layout = menuLayout;
            if (layout == null)
            {
                Debug.LogError("PauseMenuLayoutが設定されていません。", this);
                return;
            }

            Font resolvedFont = font != null
                ? font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            layout.ApplyFont(resolvedFont);

            if (pauseController != null)
            {
                pauseController.PauseStateChanged += HandlePauseStateChanged;
                SetVisible(pauseController.IsPaused);
            }
            else
            {
                SetVisible(false);
            }
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void Unbind()
        {
            if (pauseController != null)
            {
                pauseController.PauseStateChanged -= HandlePauseStateChanged;
                pauseController = null;
            }
        }

        private void HandlePauseStateChanged(bool paused)
        {
            SetVisible(paused);
        }

        private void SetVisible(bool visible)
        {
            if (layout != null)
            {
                layout.Root.SetActive(visible);
            }
        }
    }
}
