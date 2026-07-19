using System.Collections.Generic;
using DemonKing.Core.Input;
using UnityEngine;

namespace DemonKing.Gameplay.Interaction
{
    /// <summary>
    /// プレイヤーのInteract入力を受け取り、近傍で最も近いIInteractableへ相互作用を委譲します。
    /// 対象ごとの会話や調査ロジックは持たず、探索と実行だけを担当します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float interactionRadius = 1.1f;
        [SerializeField] private Vector2 interactionOffset = new(0f, 0.15f);
        [SerializeField] private LayerMask interactionLayers = ~0;

        private readonly HashSet<IInteractable> visited = new();
        private PlayerInputReader inputReader;

        private void Awake()
        {
            inputReader = GetComponent<PlayerInputReader>();
        }

        private void OnEnable()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            if (inputReader != null)
            {
                inputReader.InteractPressed += HandleInteractPressed;
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.InteractPressed -= HandleInteractPressed;
            }
        }

        private void HandleInteractPressed()
        {
            IInteractable target = FindNearestInteractable();
            target?.Interact(gameObject);
        }

        private IInteractable FindNearestInteractable()
        {
            Vector2 center = (Vector2)transform.position + interactionOffset;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(center, interactionRadius, interactionLayers);

            visited.Clear();
            IInteractable nearest = null;
            float nearestDistance = float.PositiveInfinity;

            foreach (Collider2D collider in colliders)
            {
                if (collider == null || collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                MonoBehaviour[] behaviours = collider.GetComponentsInParent<MonoBehaviour>(false);
                foreach (MonoBehaviour behaviour in behaviours)
                {
                    IInteractable interactable = behaviour as IInteractable;
                    if (interactable == null || !visited.Add(interactable))
                    {
                        continue;
                    }

                    if (!interactable.CanInteract(gameObject))
                    {
                        continue;
                    }

                    float distance = ((Vector2)behaviour.transform.position - center).sqrMagnitude;
                    if (distance >= nearestDistance)
                    {
                        continue;
                    }

                    nearest = interactable;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere((Vector2)transform.position + interactionOffset, interactionRadius);
        }
#endif
    }
}
