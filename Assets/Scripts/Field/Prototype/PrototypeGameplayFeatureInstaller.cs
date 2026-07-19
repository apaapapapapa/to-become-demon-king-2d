using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// InteractionとCombatの最小プレイ可能ループを確認するため、試作NPCと訓練用ダミーを配置します。
    /// 恒久機能のロジックは持たず、Prototypeシーン向けの組み立てだけを担当します。
    /// </summary>
    internal sealed class PrototypeGameplayFeatureInstaller
    {
        public void Install(Transform parent)
        {
            CreateNpc(parent);
            CreateCombatDummy(parent);
        }

        private static void CreateNpc(Transform parent)
        {
            GameObject npc = new("見習い魔術師");
            npc.transform.SetParent(parent, false);
            npc.transform.localPosition = new Vector3(-0.85f, 0.35f, 0f);
            npc.AddComponent<PrototypeNpcInteractable>();
        }

        private static void CreateCombatDummy(Transform parent)
        {
            GameObject dummy = new("訓練用スライム");
            dummy.transform.SetParent(parent, false);
            dummy.transform.localPosition = new Vector3(1.45f, -0.45f, 0f);
            dummy.AddComponent<PrototypeCombatDummy>();
        }
    }
}
