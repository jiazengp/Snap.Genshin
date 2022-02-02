using Snap.Exception;
using System;
using System.Windows.Media;

namespace DGP.Genshin.DataModel.Helper
{
    public static class StarHelper
    {
        public static string FromInt32Rank(int rank)
        {
            if (rank == 105)
            {
                rank = 5;
            }
            return rank is >= 1 and <= 5
                ? $@"https://genshin.honeyhunterworld.com/img/back/item/{rank}star.png"
                : throw new SnapGenshinInternalException($"稀有度不存在：{rank}");
        }

        public static int ToInt32Rank(this string? starurl)
        {
            return starurl is null ? 1 : int.Parse(starurl[51].ToString());
        }

        public static bool IsOfRank(this string? starurl, int rank)
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
                _ => Color.FromArgb(0, 0, 0, 0),
            };
            return new SolidColorBrush(color);

        }
        public static SolidColorBrush? ToSolid(string? starurl)
        {
            return starurl is not null ? ToSolid(ToInt32Rank(starurl)) : null;
        }

        public static Uri? RankToUri(int rank)
        {
            string? rankUrl = FromInt32Rank(rank);
            return rankUrl is not null ? new Uri(rankUrl) : null;
        }

        //rgb(235, 229, 215)

    }
}
