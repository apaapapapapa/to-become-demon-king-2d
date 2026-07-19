using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// ワールド構築後にアプリケーション層へ公開する最小の実行時参照です。
    /// </summary>
    internal readonly struct PrototypeWorldBuildResult
    {
        public PrototypeWorldBuildResult(Transform worldRoot, GameObject player)
        {
            WorldRoot = worldRoot;
            Player = player;
        }

        public Transform WorldRoot { get; }
        public GameObject Player { get; }
    }
}
