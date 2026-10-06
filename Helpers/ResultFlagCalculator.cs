using System.Globalization;
using Backend.Constants;

namespace Backend.Helpers
{
    public static class ResultFlagCalculator
    {
        // Low/High/Normal when the value and at least one reference bound are numeric; otherwise N/A
        // (qualitative results such as "Positive" are left for the pathologist to interpret).
        public static string Calculate(string resultValue, string referenceLow, string referenceHigh)
        {
            if (!TryParse(resultValue, out var value))
            {
                return ResultFlags.NotApplicable;
            }

            var hasLow=TryParse(referenceLow, out var low);
            var hasHigh=TryParse(referenceHigh, out var high);

            if (!hasLow && !hasHigh)
            {
                return ResultFlags.NotApplicable;
            }

            if (hasLow && value < low)
            {
                return ResultFlags.Low;
            }

            if (hasHigh && value > high)
            {
                return ResultFlags.High;
            }

            return ResultFlags.Normal;
        }

        private static bool TryParse(string? text, out decimal value)
        {
            return decimal.TryParse(text?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }
    }
}
