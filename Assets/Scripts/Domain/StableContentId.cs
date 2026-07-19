using System;

namespace DemonKing.Domain
{
    /// <summary>
    /// Definitionと保存データを結び付ける安定IDの共通検証を提供します。
    /// 表示名やUnityアセット名を保存キーとして使用しないための境界です。
    /// </summary>
    public static class StableContentId
    {
        public static bool IsValid(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static string Normalize(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        public static string Require(string value, string parameterName)
        {
            string normalized = Normalize(value);
            if (normalized.Length == 0)
            {
                throw new ArgumentException("安定IDは空にできません。", parameterName);
            }

            return normalized;
        }
    }
}
