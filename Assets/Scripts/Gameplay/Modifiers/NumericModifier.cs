using System;

namespace DemonKing.Gameplay.Modifiers
{
    /// <summary>
    /// 複数の取得元から集めた加算値と割合補正を、適用順に依存せず合成します。
    /// </summary>
    public readonly struct NumericModifier
    {
        public NumericModifier(double flatAmount, double additiveRate)
        {
            FlatAmount = flatAmount;
            AdditiveRate = additiveRate;
        }

        public double FlatAmount { get; }
        public double AdditiveRate { get; }
        public static NumericModifier Identity => default;

        public NumericModifier Combine(NumericModifier other)
        {
            return new NumericModifier(
                FlatAmount + other.FlatAmount,
                AdditiveRate + other.AdditiveRate);
        }

        public float Apply(float baseValue, float minimumValue = 0f)
        {
            double result = Calculate(baseValue);
            if (double.IsNaN(result))
            {
                return minimumValue;
            }

            return (float)Math.Max(minimumValue, Math.Min(float.MaxValue, result));
        }

        public int Apply(int baseValue, int minimumValue = 0)
        {
            double result = Math.Round(Calculate(baseValue), MidpointRounding.AwayFromZero);
            if (double.IsNaN(result))
            {
                return minimumValue;
            }

            return (int)Math.Max(minimumValue, Math.Min(int.MaxValue, result));
        }

        public long Apply(long baseValue, long minimumValue = 0)
        {
            double result = Math.Round(Calculate(baseValue), MidpointRounding.AwayFromZero);
            if (double.IsNaN(result))
            {
                return minimumValue;
            }

            if (result >= long.MaxValue)
            {
                return long.MaxValue;
            }

            return (long)Math.Max(minimumValue, result);
        }

        private double Calculate(double baseValue)
        {
            return (baseValue + FlatAmount) * Math.Max(0d, 1d + AdditiveRate);
        }
    }
}
