using System.Globalization;

namespace StripeSample.Infrastructure
{
    public static class IntegerExtensions
    {
        public static string CentsAsDollars(this int amount, string currencyCode)
        {
            var tmp = amount / 100;

            if (string.IsNullOrEmpty(currencyCode))
            {
                return $"{tmp.ToString("C2", CultureInfo.CurrentUICulture)}";
            }
            else
            {
                return $"{tmp:C2}";
            }

        }
    }
}
