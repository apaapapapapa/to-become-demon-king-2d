using UnityEngine;

namespace DemonKing.Gameplay.Interaction
{
    /// <summary>
    /// プレイヤーなどから相互作用を受け取れるオブジェクトの共通契約です。
    /// 会話、調査、扉、宝箱などの具体的な処理は各実装側へ閉じ込めます。
    /// </summary>
    public interface IInteractable
    {
        bool CanInteract(GameObject interactor);

        void Interact(GameObject interactor);
    }
}
