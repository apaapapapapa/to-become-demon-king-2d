using UnityEngine;

namespace DemonKing.Core.Math
{
    /// <summary>
    /// ワールド座標から2D描画順を算出する共通ルールです。
    /// 画面下側にいるオブジェクトほど手前に描画される規則を、複数コンポーネントで共有します。
    /// </summary>
    public static class WorldSortOrder
    {
        public static int FromWorldY(float worldY, int precision, int offset = 0)
        {
            return -Mathf.RoundToInt(worldY * precision) + offset;
        }
    }
}
