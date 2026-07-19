using UnityEngine;

namespace DemonKing.Core.Input
{
    /// <summary>
    /// 既存の移動系コンポーネントへMove入力だけを公開する互換用アダプターです。
    /// Input Actionsアセットの所有とライフサイクルはPlayerInputReaderへ集約します。
    /// </summary>
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class MoveInputReader : MonoBehaviour
    {
        private PlayerInputReader playerInput;

        public Vector2 Move => playerInput == null ? Vector2.zero : playerInput.Move;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInputReader>();
        }
    }
}
