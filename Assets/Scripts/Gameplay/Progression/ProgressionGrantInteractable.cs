using System;
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

        public event Action<ProgressionGrantResult> InteractionCompleted;

        public bool IsInitialized => grantDefinition != null && acquisitionService != null;
        public bool IsConsumed { get; private set; }
        public string GrantId => grantDefinition == null ? string.Empty : grantDefinition.GrantId;

        public void Initialize(
            ProgressionGrantDefinition definition,
            ProgressionAcquisitionService service)
        {
            if (definition == null || !definition.IsConfigured)
            {
                throw new ArgumentException(
                    "正しく設定されたProgressionGrantDefinitionが必要です。",
                    nameof(definition));
            }

            grantDefinition = definition;
            acquisitionService = service ?? throw new ArgumentNullException(nameof(service));
            IsConsumed = false;

            SphereCollider interactionCollider = GetComponent<SphereCollider>();
            interactionCollider.isTrigger = true;
            interactionCollider.radius = 0.6f;
            interactionCollider.center = new Vector3(0f, 0.2f, 0.45f);
            interactionCollider.enabled = true;
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
            IsConsumed = true;
            InteractionCompleted?.Invoke(result);

            // 一度きりの取得物として、取得済みコンテンツだった場合も再Interactionさせません。
            // Save復元時の配置制御はローカルSave実装フェーズで永続状態と接続します。
            gameObject.SetActive(false);
        }
    }
}
