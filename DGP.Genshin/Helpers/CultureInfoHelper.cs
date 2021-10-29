using System.Globalization;

namespace DGP.Genshin.Helpers
{
    public static class CultureInfoHelper
    {
        private static readonly CultureInfo cultureInfo = new("zh-cn");
        public static CultureInfo Default => cultureInfo;
    }
}
