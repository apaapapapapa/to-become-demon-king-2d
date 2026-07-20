using System.Collections.Generic;
using DemonKing.Core.Input;
using DemonKing.Core.Math;
using UnityEngine;

namespace DemonKing.Gameplay.Interaction
{
    /// <summary>
    /// プレイヤーのInteract入力を受け取り、3D Physics空間の近傍で最も近いIInteractableへ相互作用を委譲します。
    /// X/Yをフィールド平面、ZをElevationとして扱うため、高さが離れた対象は自然に検索範囲外になります。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        private const float GroundElevationTolerance = 0.001f;

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
            Vector3 center = transform.position + FieldSpace3D.PlanarDelta(interactionOffset);
            visited.Clear();
            IInteractable nearest = null;
            float nearestDistance = float.PositiveInfinity;

            Collider[] colliders = Physics.OverlapSphere(
                center,
                interactionRadius,
                interactionLayers,
                QueryTriggerInteraction.Collide);
            foreach (Collider collider in colliders)
            {
                if (collider == null || collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                ConsiderBehaviours(
                    collider.GetComponentsInParent<MonoBehaviour>(false),
                    center,
                    ref nearest,
                    ref nearestDistance);
            }

            // 既存の地上PlayModeテストfixtureを段階移行するための互換Queryです。
            // Runtime Actorは3D Colliderへ移行済みのため正式な探索は上の3D Queryです。
            if (nearest == null && Mathf.Abs(center.z) <= GroundElevationTolerance)
            {
                Collider2D[] legacyColliders = Physics2D.OverlapCircleAll(
                    FieldSpace3D.ToPlanar(center),
                    interactionRadius,
                    interactionLayers);
                foreach (Collider2D collider in legacyColliders)
                {
                    if (collider == null || collider.transform.IsChildOf(transform))
                    {
                        continue;
                    }

                    ConsiderBehaviours(
                        collider.GetComponentsInParent<MonoBehaviour>(false),
                        center,
                        ref nearest,
                        ref nearestDistance);
                }
            }

            return nearest;
        }

        private void ConsiderBehaviours(
            MonoBehaviour[] behaviours,
            Vector3 center,
            ref IInteractable nearest,
            ref float nearestDistance)
        {
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is not IInteractable interactable || !visited.Add(interactable))
                {
                    continue;
                }

                if (!interactable.CanInteract(gameObject))
                {
                    continue;
                }

                float distance = (behaviour.transform.position - center).sqrMagnitude;
                if (distance >= nearestDistance)
                {
                    continue;
                }

                nearest = interactable;
                nearestDistance = distance;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(
                transform.position + FieldSpace3D.PlanarDelta(interactionOffset),
                interactionRadius);
        }
#endif
    }
}
