using DemonKing.Core.Math;
using UnityEngine;

namespace DemonKing.Field.Prototype
{
    /// <summary>
    /// プロトタイプフィールド固有の座標変換と配置計算をまとめます。
    /// Builder側は数式の詳細を持たず、配置の意図に集中します。
    /// </summary>
    internal static class PrototypeWorldMath
    {
        private const float IsoYScale = 0.5f;
        private const int SortingPrecision = 100;

        public const int GroundOrder = -1000;

        public static int PathY(int x)
        {
            return Mathf.RoundToInt(Mathf.Sin(x * 0.38f) * 1.65f);
        }

        public static Vector2 Iso(int x, int y)
        {
            return new Vector2((x - y) * 0.5f, (x + y) * 0.5f * IsoYScale);
        }

        public static int SortOrder(float worldY)
        {
            return WorldSortOrder.FromWorldY(worldY, SortingPrecision);
        }

        public static float Next(System.Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }
    }
}
