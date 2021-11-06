using System;
using System.Windows.Media;

namespace DGP.Genshin.DataModel.Helpers
{
    public static class StarHelper
    {
        public static string FromRank(int rank)
        {
            if (rank == 105)
            {
                rank = 5;
            }
            return rank is >= 1 and <= 5
                ? $@"https://genshin.honeyhunterworld.com/img/back/item/{rank}star.png"
                : throw new Exception($"稀有度不存在：{rank}");
        }

        public static int ToRank(this string? starurl)
        {
            return starurl is null ? 1 : int.Parse(starurl[51].ToString());
        }

        public static bool IsOfRank(this string? starurl, int rank)
        {
            return starurl.ToRank() == rank;
        }

        public static SolidColorBrush? ToSolid(int rank)
        {
            return rank switch
            {
                1 => new SolidColorBrush(Color.FromRgb(114, 119, 139)),
                2 => new SolidColorBrush(Color.FromRgb(42, 143, 114)),
                3 => new SolidColorBrush(Color.FromRgb(81, 128, 203)),
                4 => new SolidColorBrush(Color.FromRgb(161, 86, 224)),
                5 or 105 => new SolidColorBrush(Color.FromRgb(188, 105, 50)),
                _ => null,
            };
        }
        public static SolidColorBrush? ToSolid(string? starurl)
        {
            return starurl is not null ? ToSolid(ToRank(starurl)) : null;
        }

        public static Uri? RankToUri(int rank)
        {
            string? rankUrl = FromRank(rank);
            return rankUrl is not null ? new Uri(rankUrl) : null;
        }

        //rgb(235, 229, 215)

    }
}
