using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// 実行時生成プロトタイプで共有する色定義です。
    /// 見た目の調整箇所を各Builderから分離し、色変更の影響範囲を限定します。
    /// </summary>
    internal static class PrototypePalette
    {
        public static readonly Color Grass = new(0.31f, 0.52f, 0.28f);
        public static readonly Color GrassLight = new(0.43f, 0.66f, 0.34f);
        public static readonly Color DeepGreen = new(0.10f, 0.22f, 0.20f);
        public static readonly Color Path = new(0.72f, 0.52f, 0.31f);
        public static readonly Color PathLight = new(0.86f, 0.69f, 0.43f);
        public static readonly Color Wood = new(0.32f, 0.20f, 0.16f);
        public static readonly Color Roof = new(0.49f, 0.22f, 0.34f);
        public static readonly Color RoofLight = new(0.68f, 0.32f, 0.40f);
        public static readonly Color Wall = new(0.84f, 0.72f, 0.52f);
    }
}
