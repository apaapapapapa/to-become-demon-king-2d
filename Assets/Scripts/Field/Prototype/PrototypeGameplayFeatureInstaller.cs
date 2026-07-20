using System;
using DemonKing.Gameplay.Rewards;
using DemonKing.Gameplay.Rewards.Configuration;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// InteractionとCombatの最小プレイ可能ループを確認するため、試作NPCと訓練用ダミーを配置します。
    /// 恒久機能のロジックは持たず、Prototypeシーン向けの組み立てだけを担当します。
    /// </summary>
    internal sealed class PrototypeGameplayFeatureInstaller
    {
        public void Install(
            Transform parent,
            RewardService rewardService,
            RewardDefinition trainingDummyReward)
        {
            if (rewardService == null)
            {
                throw new ArgumentNullException(nameof(rewardService));
            }

            if (trainingDummyReward == null || !trainingDummyReward.IsConfigured)
            {
                throw new ArgumentException(
                    "訓練用ダミーの報酬定義が正しく設定されていません。",
                    nameof(trainingDummyReward));
            }

            CreateNpc(parent);
            PrototypeCombatDummy dummy = CreateCombatDummy(parent);
            dummy.ConfigureReward(trainingDummyReward);
            dummy.gameObject.AddComponent<PrototypeMonsterDefeatEffect>();
            dummy.Defeated += context =>
            {
                RewardGrantResult result = rewardService.GrantDefeatReward(
                    context,
                    trainingDummyReward);
                if (!result.WasGranted)
                {
                    Debug.LogWarning($"撃破報酬を付与できませんでした: {result.FailureReason}");
                }
            };

            rewardService.RewardGranted += LogGrantedReward;
        }

        private static void CreateNpc(Transform parent)
        {
            GameObject npc = new("見習い魔術師");
            npc.transform.SetParent(parent, false);
            npc.transform.localPosition = new Vector3(-0.85f, 0.35f, 0f);
            npc.AddComponent<PrototypeNpcInteractable>();
        }

        private static PrototypeCombatDummy CreateCombatDummy(Transform parent)
        {
            GameObject dummy = new("訓練用スライム");
            dummy.transform.SetParent(parent, false);
            dummy.transform.localPosition = new Vector3(1.45f, -0.45f, 0f);
            return dummy.AddComponent<PrototypeCombatDummy>();
        }

        private static void LogGrantedReward(RewardGrantResult result)
        {
            Debug.Log(
                $"経験値を{result.GrantedExperience}獲得。" +
                $" レベル {result.LevelUpResult.PreviousLevel} → {result.LevelUpResult.CurrentLevel}、" +
                $"累積経験値 {result.LevelUpResult.CurrentExperience}");
        }
    }
}
