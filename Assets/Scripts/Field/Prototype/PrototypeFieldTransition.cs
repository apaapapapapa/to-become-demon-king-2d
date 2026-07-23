using System;
using DemonKing.Field.Composition;
using DemonKing.Gameplay.Interaction;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Field内の出口からApplication側へStable Field Locationで遷移を要求する境界です。
    /// InteractableはScene名、Save、Game Sessionの詳細を知りません。
    /// </summary>
    internal interface IPrototypeFieldTransitionRequester
    {
        bool IsTransitioning { get; }
        bool TryTransition(FieldLocation destination);
    }

    /// <summary>
    /// Field出口をInteractionへ接続する薄いAdapterです。
    /// 実際のScene切替とRuntime再構築はIPrototypeFieldTransitionRequesterへ委譲します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    internal sealed class PrototypeFieldTransitionInteractable : MonoBehaviour, IInteractable
    {
        private IPrototypeFieldTransitionRequester transitionRequester;
        private FieldLocation destination;
        private bool initialized;

        public string DisplayName { get; private set; } = string.Empty;
        public FieldLocation Destination => destination;
        public bool IsInitialized => initialized;

        public void Initialize(
            string displayName,
            FieldLocation targetLocation,
            IPrototypeFieldTransitionRequester requester)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Field出口の表示名は必須です。", nameof(displayName));
            }

            if (!targetLocation.IsValid)
            {
                throw new ArgumentException("遷移先Field Locationが不正です。", nameof(targetLocation));
            }

            transitionRequester = requester ?? throw new ArgumentNullException(nameof(requester));
            DisplayName = displayName;
            destination = targetLocation;
            initialized = true;

            SphereCollider interactionCollider = GetComponent<SphereCollider>();
            interactionCollider.isTrigger = true;
            interactionCollider.radius = 0.7f;
            interactionCollider.center = new Vector3(0f, 0.2f, 0.35f);
        }

        public bool CanInteract(GameObject interactor)
        {
            return initialized &&
                   transitionRequester != null &&
                   !transitionRequester.IsTransitioning &&
                   isActiveAndEnabled &&
                   gameObject.activeInHierarchy &&
                   interactor != null;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                return;
            }

            transitionRequester.TryTransition(destination);
        }
    }
}
