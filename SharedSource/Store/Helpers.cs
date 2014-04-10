namespace MarkerMetro.Unity.WinIntegration.Store
{
    internal static class Helpers
    {
        public static float GetValueFromFormattedPrice(string price)
        {
            if (string.IsNullOrEmpty(price.Trim())) return 0;

            int start = -1;
            int len = 1;
            for (int i = 0; i < price.Length; i++)
            {
                if (start < 0)
                {
                    // Find start
                    if (char.IsNumber(price[i]))
                        start = i;
                }
                else
                {
                    // Find length
                    if (char.IsNumber(price[i]) || price[i] == '.' || price[i] == ',')
                        len++;
                    else
                        break;
                }
            }

            if (start < 0) return 0;

            var valStr = price.Substring(start, len);
            return float.Parse(valStr);
        }
    }
}