using Snap.Exception;
using System;
using System.Windows.Media;

namespace DGP.Genshin.DataModel.Helper
{
    public static class StarHelper
    {
        private const string star1Url = @"https://genshin.honeyhunterworld.com/img/back/item/1star.png";
        private const string star2Url = @"https://genshin.honeyhunterworld.com/img/back/item/2star.png";
        private const string star3Url = @"https://genshin.honeyhunterworld.com/img/back/item/3star.png";
        private const string star4Url = @"https://genshin.honeyhunterworld.com/img/back/item/4star.png";
        private const string star5Url = @"https://genshin.honeyhunterworld.com/img/back/item/5star.png";

        public static string FromInt32Rank(int rank)
        {
            return rank switch
            {
                1 => star1Url,
                2 => star2Url,
                3 => star3Url,
                4 => star4Url,
                5 or 105 => star5Url,
                _ => throw new SnapGenshinInternalException($"稀有度不存在：{rank}")
            };
        }

        public static int ToInt32Rank(this string? starurl)
        {
            return starurl is null ? 1 : int.Parse(starurl[51].ToString());
        }

        public static bool IsRankAs(this string? starurl, int rank)
        {
            return starurl.ToInt32Rank() == rank;
        }

        public static SolidColorBrush? ToSolid(int rank)
        {
            Color color = rank switch
            {
                1 => Color.FromRgb(114, 119, 139),
                2 => Color.FromRgb(42, 143, 114),
                3 => Color.FromRgb(81, 128, 203),
                4 => Color.FromRgb(161, 86, 224),
                5 or 105 => Color.FromRgb(188, 105, 50),
                _ => Color.FromRgb(114, 119, 139),
            };
            return new SolidColorBrush(color);

        }
        public static SolidColorBrush? ToSolid(string? starurl)
        {
            return starurl is null ? null : ToSolid(ToInt32Rank(starurl));
        }

        public static Uri? ToUri(int rank)
        {
            string? rankUrl = FromInt32Rank(rank);
            return rankUrl is null ? null : new Uri(rankUrl);
        }
    }
}
