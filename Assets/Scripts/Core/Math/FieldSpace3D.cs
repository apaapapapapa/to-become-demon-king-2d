using UnityEngine;

namespace DemonKing.Core.Math
{
    /// <summary>
    /// 既存のアイソメトリック表示座標を維持したまま3D Physicsを利用するための軸規約です。
    /// X/Yをフィールド平面、ZをElevation（高さ）として扱います。
    /// </summary>
    public static class FieldSpace3D
    {
        public const float GroundElevation = 0f;

        public static Vector3 Planar(Vector2 value, float elevation = GroundElevation)
        {
            return new Vector3(value.x, value.y, elevation);
        }

        public static Vector3 PlanarDelta(Vector2 value)
        {
            return new Vector3(value.x, value.y, 0f);
        }

        public static Vector2 ToPlanar(Vector3 value)
        {
            return new Vector2(value.x, value.y);
        }

        public static Vector3 WithElevation(Vector3 value, float elevation)
        {
            value.z = elevation;
            return value;
        }
    }
}
