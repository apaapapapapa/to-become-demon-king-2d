# 敵AI仕様

## 目的

敵個体がプレイヤーを索敵し、距離と高度差に応じて追跡・攻撃する基本行動を定義します。

AIはCombatや3D Physicsの具体処理を再実装せず、既存のGameplay境界を利用します。

```text
EnemyAiController
  ├ CharacterPhysicsBody3D -> 追跡移動
  └ AbilityController      -> 攻撃Ability
```

## 状態

敵AIは次の3状態を持ちます。

- `Idle`: Targetを追跡していない
- `Chasing`: Targetへ向かってX/Y平面を移動する
- `Attacking`: 攻撃範囲内でPrimary Attack Abilityの使用を試みる

状態遷移は距離・高度差・Target生存状態から毎Physics Step評価します。

## 索敵と離脱

未交戦時は`DetectionRange`以内へTargetが入ると交戦を開始します。

一度交戦した後は`DisengageRange`まで追跡を継続します。`DisengageRange`を`DetectionRange`以上にすることで、境界付近でIdleとChasingが頻繁に往復することを防ぎます。

TargetとのElevation差が`MaxChaseElevationDifference`を超えた場合は追跡を解除します。現在の訓練用スライムは地上敵として扱うため、高く飛行したプレイヤーを追跡しません。

## 追跡

追跡方向はTargetとのX/Y平面差分から求めます。

`EnemyAiController`はRigidbodyを直接操作せず、`CharacterPhysicsBody3D.QueuePlanarDelta`へ移動要求を渡します。これにより建物などの3D Colliderとの衝突解決は既存の3D Physicsへ委譲します。

現段階ではNavMeshや経路探索は使用せず、Targetへの直接追跡とします。障害物を迂回する経路探索は、フィールド構造が複雑化して必要性が発生した段階で追加します。

## 攻撃

次の条件を満たす場合、状態を`Attacking`へ変更します。

- X/Y平面距離が`AttackRange`以内
- Elevation差が`MaxAttackElevationDifference`以内

攻撃処理は`EnemyAiController`自身では実装しません。

```text
EnemyAiController
  -> AbilityController.TryUse
      -> MeleeAttackExecutor
          -> IDamageable
              -> Health
```

AbilityのCooldown、DamageRequest、命中判定は既存Ability / Combat実装をSource of Truthとします。AIはCooldownを独自管理せず、攻撃可能なPhysics StepごとにAbility使用を試み、`AbilityController`の結果に従います。

## 訓練用スライム

Prototypeの訓練用スライムへ基本敵AIを接続します。

`TrainingSlimeAi.asset`が次のAI調整値とPrimary Attack参照を所有します。

- 索敵距離
- 離脱距離
- 攻撃距離
- 追跡速度
- 追跡可能な最大Elevation差
- 攻撃可能な最大Elevation差
- Primary Attack Ability

訓練用スライムが撃破後に再生成された場合も、`SpawnLifecycle`から生成された新個体へ同じAI DefinitionとプレイヤーTargetを再注入します。

## 責務境界

- AI状態・Target判断: `EnemyAiController`
- AI静的調整値: `EnemyAiDefinition`
- 平面移動と3D衝突: `CharacterPhysicsBody3D`
- 攻撃使用可否・Cooldown: `AbilityController`
- 攻撃効果・命中判定: Ability Executor / Combat
- HP・死亡: `Health`
- 再生成・復元: `SpawnLifecycle<T>`
- PrototypeでのTarget注入: `PrototypeGameplayFeatureInstaller`
