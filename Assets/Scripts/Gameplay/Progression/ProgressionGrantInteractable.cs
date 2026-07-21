using System;
using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Interaction;
using DemonKing.Gameplay.Progression.Configuration;
using UnityEngine;

namespace DemonKing.Gameplay.Progression
{
    /// <summary>
    /// Interactionを一度だけProgression Grantへ変換する汎用取得オブジェクトです。
    /// 取得元固有の条件や見た目を持たず、付与処理はProgressionAcquisitionServiceへ委譲します。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    public sealed class ProgressionGrantInteractable : MonoBehaviour, IInteractable
    {
        private ProgressionGrantDefinition grantDefinition;
        private ProgressionAcquisitionService acquisitionService;
        private ProgressionGrantConsumptionState consumptionState;

        public event Action<ProgressionGrantResult> InteractionCompleted;

        public bool IsInitialized =>
            grantDefinition != null && acquisitionService != null && consumptionState != null;
        public bool IsConsumed =>
            grantDefinition != null &&
            consumptionState != null &&
            consumptionState.IsConsumed(grantDefinition.GrantId);
        public string GrantId => grantDefinition == null ? string.Empty : grantDefinition.GrantId;

        /// <summary>
        /// Save状態を必要としないテスト・単独利用向けの互換初期化です。
        /// </summary>
        public void Initialize(
            ProgressionGrantDefinition definition,
            ProgressionAcquisitionService service)
        {
            Initialize(definition, service, ProgressionGrantConsumptionState.CreateInitial());
        }

        public void Initialize(
            ProgressionGrantDefinition definition,
            ProgressionAcquisitionService service,
            ProgressionGrantConsumptionState restoredConsumptionState)
        {
            if (definition == null || !definition.IsConfigured)
            {
                throw new ArgumentException(
                    "正しく設定されたProgressionGrantDefinitionが必要です。",
                    nameof(definition));
            }

            grantDefinition = definition;
            acquisitionService = service ?? throw new ArgumentNullException(nameof(service));
            consumptionState = restoredConsumptionState ??
                throw new ArgumentNullException(nameof(restoredConsumptionState));

            SphereCollider interactionCollider = GetComponent<SphereCollider>();
            interactionCollider.isTrigger = true;
            interactionCollider.radius = 0.6f;
            interactionCollider.center = new Vector3(0f, 0.2f, 0.45f);
            interactionCollider.enabled = !IsConsumed;

            if (IsConsumed)
            {
                gameObject.SetActive(false);
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            return IsInitialized &&
                   !IsConsumed &&
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

            ProgressionGrantResult result = acquisitionService.Grant(grantDefinition);
            consumptionState.TryConsume(grantDefinition.GrantId);
            InteractionCompleted?.Invoke(result);

            // Grantの付与結果が既取得であっても、このフィールド取得物自体は一度きりとして消費します。
            gameObject.SetActive(false);
        }
    }
}
