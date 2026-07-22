using DemonKing.Domain.Progression;
using DemonKing.Gameplay.Characters;
using DemonKing.Gameplay.Characters.Configuration;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// CharacterDefinitionからPlayer Prefabを生成し、配置とRuntime Context注入を開始します。
    /// FeatureごとのComponent構築・初期化はPrototypePlayerRuntimeInstallerへ委譲します。
    /// </summary>
    internal sealed class PrototypePlayerSpawner
    {
        private readonly Vector3 spawnPosition;
        private readonly CharacterDefinition characterDefinition;
        private readonly CharacterProgressionState progressionState;
        private readonly PrototypePlayerRuntimeInstaller runtimeInstaller;

        public PrototypePlayerSpawner(
            Vector3 spawnPosition,
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState = null)
            : this(
                spawnPosition,
                characterDefinition,
                progressionState,
                new PrototypePlayerRuntimeInstaller())
        {
        }

        internal PrototypePlayerSpawner(
            Vector3 spawnPosition,
            CharacterDefinition characterDefinition,
            CharacterProgressionState progressionState,
            PrototypePlayerRuntimeInstaller runtimeInstaller)
        {
            this.spawnPosition = spawnPosition;
            this.characterDefinition = characterDefinition;
            this.progressionState = progressionState;
            this.runtimeInstaller = runtimeInstaller;
        }

        public GameObject Spawn(Transform parent)
        {
            if (characterDefinition == null || !characterDefinition.IsConfigured)
            {
                Debug.LogError("プレイヤーのCharacterDefinitionが正しく設定されていません。");
                return null;
            }

            CharacterProgressionState state = progressionState ??
                CharacterProgressionState.CreateInitial(characterDefinition.CharacterId);
            var runtimeContext = new CharacterRuntimeContext(characterDefinition, state);

            GameObject root = Object.Instantiate(characterDefinition.Prefab, parent, false);
            root.transform.localPosition = spawnPosition;

            CharacterRuntimeContextHost contextHost = root.GetComponent<CharacterRuntimeContextHost>();
            if (contextHost == null)
            {
                contextHost = root.AddComponent<CharacterRuntimeContextHost>();
            }

            contextHost.Initialize(runtimeContext);

            try
            {
                runtimeInstaller.Install(root, characterDefinition, state);
            }
            catch (System.Exception exception)
            {
                Debug.LogError(
                    $"Player Runtime Compositionに失敗しました: {exception.Message}",
                    root);
                Object.Destroy(root);
                return null;
            }

            return root;
        }
    }
}
