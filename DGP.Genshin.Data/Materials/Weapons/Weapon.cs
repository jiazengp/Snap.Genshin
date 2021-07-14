using DGP.Genshin.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Genshin.Data.Materials.Weapons
{
    public class Weapon : Material
    {
        public Weapon()
        {
            Star = StarHelper.FromRank(5);
        }
    }
}
