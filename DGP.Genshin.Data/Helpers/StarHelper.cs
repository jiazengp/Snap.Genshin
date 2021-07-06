using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Genshin.Data.Helpers
{
    public class StarHelper
    {
        public string FromRank(int rank) => rank >= 1 && rank <= 5 ? $@"https://genshin.honeyhunterworld.com/img/back/item/{rank}star.png" : null;

        public Uri RankToUri(int rank) => new Uri(FromRank(rank));
    }
}
