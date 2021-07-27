using System;
using System.Windows.Media;

namespace DGP.Genshin.Data.Helpers
{
    public static class StarHelper
    {
        public static string FromRank(int rank) => rank >= 1 && rank <= 5 ? $@"https://genshin.honeyhunterworld.com/img/back/item/{rank}star.png" : null;
        public static int ToRank(this string starurl) => Int32.Parse(starurl[51].ToString());
        public static SolidColorBrush ToSolid(int rank)
        {
            switch (rank)
            {
                case 1:
                    return new SolidColorBrush(Color.FromRgb(114, 119, 139));
                case 2:
                    return new SolidColorBrush(Color.FromRgb(42, 143, 114));
                case 3:
                    return new SolidColorBrush(Color.FromRgb(81, 128, 203));
                case 4:
                    return new SolidColorBrush(Color.FromRgb(161, 86, 224));
                case 5:
                    return new SolidColorBrush(Color.FromRgb(188, 105, 50));
                default:
                    return null;
            }
        }
        public static SolidColorBrush ToSolid(string starurl) => ToSolid(ToRank(starurl));
        public static Uri RankToUri(int rank) => new Uri(FromRank(rank));

        //rgb(235, 229, 215)

    }
}
