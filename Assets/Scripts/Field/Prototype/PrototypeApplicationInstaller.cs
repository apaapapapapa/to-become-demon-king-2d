using DemonKing.Core.Application;
using DemonKing.Core.Input;
using DemonKing.Field.Prototype.Configuration;
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

            PrototypeWorldBuildResult worldResult = new PrototypeWorldBuilder(
                    settings.PlayerSpawnPosition,
                    settings.PlayableTileRadius,
                    projectAssets)
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
            PrototypeUiInstaller.Create(projectAssets.UiFont, pauseController);
            return applicationRoot;
        }
    }
}
