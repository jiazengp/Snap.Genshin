using DGP.Genshin.DataModel.Characters;
using DGP.Genshin.DataModel.Helpers;
using DGP.Genshin.DataModel.Materials.Talents;
using DGP.Genshin.DataModel.Weapons;
using DGP.Snap.Framework.Extensions.System;
using System.Collections.Generic;
using System.Linq;

using MaterialWeapon = DGP.Genshin.DataModel.Materials.Weapons.Weapon;

namespace DGP.Genshin.Services
{
    /// <summary>
    /// no need to update the view,so we don't make it observable
    /// 如果在0点前初始化并在0点后呈现，需要重新初始化
    /// </summary>
    public class DailyViewService
    {
        private readonly MetaDataService dataService = MetaDataService.Instance;

        #region Mondstadt
        private IEnumerable<Talent>? todayMondstadtTalent;
        public IEnumerable<Talent>? TodayMondstadtTalent
        {
            get
            {
                if (todayMondstadtTalent == null)
                {
                    todayMondstadtTalent = dataService.DailyTalents?
                        .Where(i => i.IsTodaysTalent() && i.IsMondstadt());
                }
                return todayMondstadtTalent;
            }
        }

        private IEnumerable<MaterialWeapon>? todayMondstadtWeaponAscension;
        public IEnumerable<MaterialWeapon>? TodayMondstadtWeaponAscension
        {
            get
            {
                if (todayMondstadtWeaponAscension == null)
                {
                    todayMondstadtWeaponAscension = dataService.DailyWeapons?
                        .Where(i => i.IsTodaysWeapon() && i.IsMondstadt());
                }
                return todayMondstadtWeaponAscension;
            }
        }

        private IEnumerable<Character>? todayMondstadtCharacter5;
        public IEnumerable<Character>? TodayMondstadtCharacter5
        {
            get
            {
                if (todayMondstadtCharacter5 == null)
                {
                    todayMondstadtCharacter5 = dataService.Characters?
                        .Where(c => c.Star.ToRank() == 5 && c.Talent is not null && c.Talent.IsMondstadt() && c.Talent.IsTodaysTalent());
                }
                return todayMondstadtCharacter5;
            }
        }

        private IEnumerable<Character>? todayMondstadtCharacter4;
        public IEnumerable<Character>? TodayMondstadtCharacter4
        {
            get
            {
                if (todayMondstadtCharacter4 == null)
                {
                    todayMondstadtCharacter4 = dataService.Characters?
                        .Where(c => c.Star.ToRank() == 4 && c.Talent is not null && c.Talent.IsMondstadt() && c.Talent.IsTodaysTalent());
                }
                return todayMondstadtCharacter4;
            }
        }

        private IEnumerable<Weapon>? todayMondstadtWeapon5;
        public IEnumerable<Weapon>? TodayMondstadtWeapon5
        {
            get
            {
                if (todayMondstadtWeapon5 == null)
                {
                    todayMondstadtWeapon5 = dataService.Weapons?
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 5 && w.Ascension.IsMondstadt() && w.Ascension.IsTodaysWeapon());
                }
                return todayMondstadtWeapon5;
            }
        }

        private IEnumerable<Weapon>? todayMondstadtWeapon4;
        public IEnumerable<Weapon>? TodayMondstadtWeapon4
        {
            get
            {
                if (todayMondstadtWeapon4 == null)
                {
                    todayMondstadtWeapon4 = dataService.Weapons?
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 4 && w.Ascension.IsMondstadt() && w.Ascension.IsTodaysWeapon());
                }
                return todayMondstadtWeapon4;
            }
        }
        #endregion

        #region Liyue
        private IEnumerable<Talent>? todayLiyueTalent;
        public IEnumerable<Talent>? TodayLiyueTalent
        {
            get
            {
                if (todayLiyueTalent == null)
                {
                    todayLiyueTalent = dataService.DailyTalents?
                        .Where(i => i.IsTodaysTalent() && i.IsLiyue());
                }
                return todayLiyueTalent;
            }
        }

        private IEnumerable<MaterialWeapon>? todayLiyueWeaponAscension;
        public IEnumerable<MaterialWeapon>? TodayLiyueWeaponAscension
        {
            get
            {
                if (todayLiyueWeaponAscension == null)
                {
                    todayLiyueWeaponAscension = dataService.DailyWeapons?
                        .Where(i => i.IsTodaysWeapon() && i.IsLiyue());
                }
                return todayLiyueWeaponAscension;
            }
        }

        private IEnumerable<Character>? todayLiyueCharacter5;
        public IEnumerable<Character>? TodayLiyueCharacter5
        {
            get
            {
                if (todayLiyueCharacter5 == null)
                {
                    todayLiyueCharacter5 = dataService.Characters?
                        .Where(c => c.Star.ToRank() == 5 && c.Talent is not null && c.Talent.IsLiyue() && c.Talent.IsTodaysTalent());
                }
                return todayLiyueCharacter5;
            }
        }

        private IEnumerable<Character>? todayLiyueCharacter4;
        public IEnumerable<Character>? TodayLiyueCharacter4
        {
            get
            {
                if (todayLiyueCharacter4 == null)
                {
                    todayLiyueCharacter4 = dataService.Characters?
                        .Where(c => c.Star.ToRank() == 4 && c.Talent is not null && c.Talent.IsLiyue() && c.Talent.IsTodaysTalent());
                }
                return todayLiyueCharacter4;
            }
        }

        private IEnumerable<Weapon>? todayLiyueWeapon5;
        public IEnumerable<Weapon>? TodayLiyueWeapon5
        {
            get
            {
                if (todayLiyueWeapon5 == null)
                {
                    todayLiyueWeapon5 = dataService.Weapons?
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 5 && w.Ascension.IsLiyue() && w.Ascension.IsTodaysWeapon());
                }
                return todayLiyueWeapon5;
            }
        }

        private IEnumerable<Weapon>? todayLiyueWeapon4;
        public IEnumerable<Weapon>? TodayLiyueWeapon4
        {
            get
            {
                if (todayLiyueWeapon4 == null)
                {
                    todayLiyueWeapon4 = dataService.Weapons?
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 4 && w.Ascension.IsLiyue() && w.Ascension.IsTodaysWeapon());
                }
                return todayLiyueWeapon4;
            }
        }
        #endregion

        #region Inazuma
        private IEnumerable<Talent>? todayInazumaTalent;
        public IEnumerable<Talent>? TodayInazumaTalent
        {
            get
            {
                if (todayInazumaTalent == null)
                {
                    todayInazumaTalent = dataService.DailyTalents?
                        .Where(i => i.IsTodaysTalent() && i.IsInazuma());
                }
                return todayInazumaTalent;
            }
        }

        private IEnumerable<MaterialWeapon>? todayInazumaWeaponAscension;
        public IEnumerable<MaterialWeapon>? TodayInazumaWeaponAscension
        {
            get
            {
                if (todayInazumaWeaponAscension == null)
                {
                    todayInazumaWeaponAscension = dataService.DailyWeapons?
                        .Where(i => i.IsTodaysWeapon() && i.IsInazuma());
                }
                return todayInazumaWeaponAscension;
            }
        }

        private IEnumerable<Character>? todayInazumaCharacter5;
        public IEnumerable<Character>? TodayInazumaCharacter5
        {
            get
            {
                if (todayInazumaCharacter5 == null)
                {
                    todayInazumaCharacter5 = dataService.Characters?
                        .Where(c => c.Star.ToRank() == 5 && c.Talent is not null && c.Talent.IsInazuma() && c.Talent.IsTodaysTalent());
                }
                return todayInazumaCharacter5;
            }
        }

        private IEnumerable<Character>? todayInazumaCharacter4;
        public IEnumerable<Character>? TodayInazumaCharacter4
        {
            get
            {
                if (todayInazumaCharacter4 == null)
                {
                    todayInazumaCharacter4 = dataService.Characters?
                        .Where(c => c.Star.ToRank() == 4 && c.Talent is not null && c.Talent.IsInazuma() && c.Talent.IsTodaysTalent());
                }
                return todayInazumaCharacter4;
            }
        }

        private IEnumerable<Weapon>? todayInazumaWeapon5;
        public IEnumerable<Weapon>? TodayInazumaWeapon5
        {
            get
            {
                if (todayInazumaWeapon5 == null)
                {
                    todayInazumaWeapon5 = dataService.Weapons?
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 5 && w.Ascension.IsInazuma() && w.Ascension.IsTodaysWeapon());
                }
                return todayInazumaWeapon5;
            }
        }

        private IEnumerable<Weapon>? todayInazumaWeapon4;
        public IEnumerable<Weapon>? TodayInazumaWeapon4
        {
            get
            {
                if (todayInazumaWeapon4 == null)
                {
                    todayInazumaWeapon4 = dataService.Weapons?
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 4 && w.Ascension.IsInazuma() && w.Ascension.IsTodaysWeapon());
                }
                return todayInazumaWeapon4;
            }
        }
        #endregion

        #region 单例
        private static DailyViewService? instance;
        private static readonly object _lock = new();
        private DailyViewService()
        {
            this.Log("initialized");
        }
        public static DailyViewService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new DailyViewService();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion
    }
}
