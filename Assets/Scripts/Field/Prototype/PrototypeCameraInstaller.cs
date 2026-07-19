using DemonKing.Presentation.CameraSystem;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// プロトタイプシーンのカメラへ追従機能を接続します。
    /// カメラ側とプレイヤー側を直接依存させず、構成ルートで追従対象を結び付けます。
    /// </summary>
    internal static class PrototypeCameraInstaller
    {
        public static CameraFollow2D Configure(Camera camera, Transform target)
        {
            if (camera == null || target == null)
            {
                return null;
            }

            CameraFollow2D follow = camera.GetComponent<CameraFollow2D>();
            if (follow == null)
            {
                follow = camera.gameObject.AddComponent<CameraFollow2D>();
            }

            follow.SetTarget(target, snapImmediately: true);
            return follow;
        }
    }
}
