using DemonKing.Field.Prototype;
using UnityEngine;

namespace DemonKing.Field
{
    /// <summary>
    /// プロトタイプフィールド起動時の最小構成ルートです。
    /// シーン初期設定とワールド構築の開始だけを担当し、具体的な生成処理は各Builderへ委譲します。
    /// </summary>
    public sealed class FieldBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            PrototypeSceneConfigurator.Configure(Camera.main);
            new PrototypeWorldBuilder().Build();
        }
    }
}
