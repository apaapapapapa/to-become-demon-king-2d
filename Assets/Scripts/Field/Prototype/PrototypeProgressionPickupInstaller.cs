using System.Collections.Generic;
using DemonKing.Domain.Progression;
using DemonKing.Field.Prototype.Configuration;
using DemonKing.Gameplay.Progression;
using DemonKing.Presentation.Rendering;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// Prototype固有のProgression取得物をフィールドへ配置します。
    /// Grant実行はGameplay側のProgressionGrantInteractableへ委譲します。
    /// </summary>
    internal sealed class PrototypeProgressionPickupInstaller
    {
        public void Install(
            Transform world,
            ProgressionAcquisitionService acquisitionService,
            ProgressionGrantConsumptionState consumptionState,
            IReadOnlyList<PrototypeProgressionPickupDefinition> definitions)
        {
            if (world == null ||
                acquisitionService == null ||
                consumptionState == null ||
                definitions == null)
            {
                return;
            }

            foreach (PrototypeProgressionPickupDefinition definition in definitions)
            {
                if (definition == null || !definition.IsConfigured)
                {
                    Debug.LogError("Progression取得物の定義が正しく設定されていません。");
                    continue;
                }

                CreatePickup(world, acquisitionService, consumptionState, definition);
            }
        }

        private static void CreatePickup(
            Transform world,
            ProgressionAcquisitionService acquisitionService,
            ProgressionGrantConsumptionState consumptionState,
            PrototypeProgressionPickupDefinition definition)
        {
            GameObject root = new(definition.DisplayName);
            root.transform.SetParent(world, false);
            root.transform.localPosition = definition.Position;

            SphereCollider collider = root.AddComponent<SphereCollider>();
            collider.isTrigger = true;

            GroupYSorter sorter = root.AddComponent<GroupYSorter>();
            var shapes = new RuntimeShapeFactory();
            shapes.CreateEllipse(
                "取得物の影",
                new Vector2(0f, -0.18f),
                new Vector2(0.82f, 0.22f),
                new Color(0.04f, 0.07f, 0.10f, 0.55f),
                -2,
                root.transform);
            shapes.CreateEllipse(
                "魔力の輝き",
                new Vector2(0f, 0.24f),
                new Vector2(0.78f, 0.78f),
                new Color(definition.Color.r, definition.Color.g, definition.Color.b, 0.24f),
                -1,
                root.transform);
            shapes.CreateDiamond(
                "取得物",
                new Vector2(0f, 0.24f),
                new Vector2(0.50f, 0.72f),
                definition.Color,
                0,
                root.transform);

            ProgressionGrantInteractable interactable =
                root.AddComponent<ProgressionGrantInteractable>();
            interactable.Initialize(
                definition.GrantDefinition,
                acquisitionService,
                consumptionState);
            sorter.RefreshRenderers();
        }
    }
}
