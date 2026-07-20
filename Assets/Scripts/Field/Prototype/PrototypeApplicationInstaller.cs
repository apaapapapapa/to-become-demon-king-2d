using DemonKing.Core.Application;
using DemonKing.Core.Input;
using DemonKing.Field.Prototype.Configuration;
using DemonKing.Gameplay.Dialogue;
using DemonKing.Gameplay.Progression;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// プロトタイプ起動時のアプリケーション構成を組み立てます。
    /// FieldBootstrapを薄いエントリーポイントに保ち、Scene / World / Pause / UIの初期化順序をここへ集約します。
    /// </summary>
    internal sealed class PrototypeApplicationInstaller
    {
        private readonly PrototypeProjectAssets projectAssets;

        public PrototypeApplicationInstaller(PrototypeProjectAssets projectAssets)
        {
            this.projectAssets = projectAssets;
        }

        public GameObject Install()
        {
            PrototypeApplicationSettings settings = projectAssets.ApplicationSettings;
            if (settings == null)
            {
                Debug.LogError("PrototypeApplicationSettingsが設定されていません。");
                return null;
            }

            PrototypeSceneConfigurator.Configure(Camera.main);
            PrototypeSortingConfigurator.Configure();

            var dialogueLog = new DialogueLog();
            PrototypeWorldBuildResult worldResult = new PrototypeWorldBuilder(
                    settings.PlayerSpawnPosition,
                    settings.PlayableTileRadius,
                    projectAssets,
                    dialogueLog)
                .Build();

            GameObject applicationRoot = new("Application Runtime");
            GamePauseController pauseController = applicationRoot.AddComponent<GamePauseController>();

            PlayerInputReader inputReader = worldResult.Player == null
                ? null
                : worldResult.Player.GetComponent<PlayerInputReader>();

            if (inputReader == null)
            {
                Debug.LogError("PlayerInputReaderが見つからないため、Pause入力を初期化できません。");
            }

            pauseController.Initialize(inputReader, settings.PausedTimeScale);

            EvolutionSelectionController evolutionSelectionController = null;
            EvolutionProgressionController evolutionProgressionController =
                worldResult.Player == null
                    ? null
                    : worldResult.Player.GetComponent<EvolutionProgressionController>();
            if (inputReader != null && evolutionProgressionController != null)
            {
                evolutionSelectionController =
                    applicationRoot.AddComponent<EvolutionSelectionController>();
                evolutionSelectionController.Initialize(
                    inputReader,
                    evolutionProgressionController);
            }
            else
            {
                Debug.LogError("Evolution選択を初期化するためのPlayerコンポーネントが見つかりません。");
            }

            PrototypeUiInstaller.Create(
                projectAssets.UiFont,
                pauseController,
                dialogueLog,
                evolutionSelectionController);
            return applicationRoot;
        }
    }
}
