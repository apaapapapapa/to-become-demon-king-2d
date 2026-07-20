# Spawning仕様

## SpawnLifecycle

`SpawnLifecycle<T>` はSpawn対象の生成、再利用可否判定、復元を調停します。

- 現在個体が存在し、`canRestore` が真なら同じ個体を `restore` する。
- 現在個体が利用できない場合はFactory Delegateで新しい個体を生成する。
- 新規生成と復元はそれぞれ通知する。
- `Forget` された現在個体は次回の要求で新規生成する。
- Lifecycle自体は生成位置、Prefab、敵種別、報酬を知らない。

## Prototype訓練対象

Prototypeの訓練用スライムは `PrototypeCombatDummyFactory` が具体生成を担当し、`SpawnLifecycle<PrototypeCombatDummy>` が再生成・復元判断を担当します。

見習い魔術師へのInteractとSpawn / Restoreの接続はCompositionの責務です。Feature間の接続方向は [Feature間の責務境界](../design/feature-boundaries.md) を参照してください。
