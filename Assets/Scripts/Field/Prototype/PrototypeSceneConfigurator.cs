using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// プロトタイプ起動時に必要なアプリケーション設定とカメラ設定を適用します。
    /// ワールド生成ロジックからシーン全体の初期設定を切り離します。
    /// </summary>
    internal static class PrototypeSceneConfigurator
    {
        public static void Configure(Camera camera)
        {
            Application.targetFrameRate = 60;
            QualitySettings.antiAliasing = 0;

            if (camera == null)
            {
                return;
            }

            camera.backgroundColor = new Color(0.075f, 0.16f, 0.18f);
            camera.orthographicSize = 5.8f;
            camera.allowMSAA = false;
            camera.transform.position = new Vector3(0f, 0.35f, -10f);
        }
    }
}
