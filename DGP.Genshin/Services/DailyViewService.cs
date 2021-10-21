using DGP.Genshin.Data.Characters;
using DGP.Genshin.Data.Helpers;
using DGP.Genshin.Data.Materials.Talents;
using DGP.Genshin.Data.Weapons;
using DGP.Snap.Framework.Extensions.System;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Services
{
    /// <summary>
    /// no need to update the view,so we don't make it observable
    /// </summary>
    public class DailyViewService
    {
        private readonly MetaDataService dataService = MetaDataService.Instance;

        #region Mondstadt
        private IEnumerable<Talent>? todayMondstadtTalent;
        public IEnumerable<Talent> TodayMondstadtTalent
        {
            get
            {
                if (this.todayMondstadtTalent == null)
                {
                    this.todayMondstadtTalent = this.dataService.DailyTalents
                        .Where(i => i.IsTodaysTalent() && i.IsMondstadt());
                }
                return this.todayMondstadtTalent;
            }
        }

        private IEnumerable<Data.Materials.Weapons.Weapon>? todayMondstadtWeaponAscension;
        public IEnumerable<Data.Materials.Weapons.Weapon> TodayMondstadtWeaponAscension
        {
            get
            {
                if (this.todayMondstadtWeaponAscension == null)
                {
                    this.todayMondstadtWeaponAscension = this.dataService.DailyWeapons
                        .Where(i => i.IsTodaysWeapon() && i.IsMondstadt());
                }
                return this.todayMondstadtWeaponAscension;
            }
        }

        private IEnumerable<Character>? todayMondstadtCharacter5;
        public IEnumerable<Character> TodayMondstadtCharacter5
        {
            get
            {
                if (this.todayMondstadtCharacter5 == null)
                {
                    this.todayMondstadtCharacter5 = this.dataService.Characters
                        .Where(c => c.Star.ToRank() == 5 && c.Talent is not null && c.Talent.IsMondstadt() && c.Talent.IsTodaysTalent());
                }
                return this.todayMondstadtCharacter5;
            }
        }

        private IEnumerable<Character>? todayMondstadtCharacter4;
        public IEnumerable<Character> TodayMondstadtCharacter4
        {
            get
            {
                if (this.todayMondstadtCharacter4 == null)
                {
                    this.todayMondstadtCharacter4 = this.dataService.Characters
                        .Where(c => c.Star.ToRank() == 4 && c.Talent is not null && c.Talent.IsMondstadt() && c.Talent.IsTodaysTalent());
                }
                return this.todayMondstadtCharacter4;
            }
        }

        private IEnumerable<Weapon>? todayMondstadtWeapon5;
        public IEnumerable<Weapon> TodayMondstadtWeapon5
        {
            get
            {
                if (this.todayMondstadtWeapon5 == null)
                {
                    this.todayMondstadtWeapon5 = this.dataService.Weapons
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 5 && w.Ascension.IsMondstadt() && w.Ascension.IsTodaysWeapon());
                }
                return this.todayMondstadtWeapon5;
            }
        }

        private IEnumerable<Weapon>? todayMondstadtWeapon4;
        public IEnumerable<Weapon> TodayMondstadtWeapon4
        {
            get
            {
                if (this.todayMondstadtWeapon4 == null)
                {
                    this.todayMondstadtWeapon4 = this.dataService.Weapons
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 4 && w.Ascension.IsMondstadt() && w.Ascension.IsTodaysWeapon());
                }
                return this.todayMondstadtWeapon4;
            }
        }
        #endregion

        #region Liyue
        private IEnumerable<Talent>? todayLiyueTalent;
        public IEnumerable<Talent> TodayLiyueTalent
        {
            get
            {
                if (this.todayLiyueTalent == null)
                {
                    this.todayLiyueTalent = this.dataService.DailyTalents
                        .Where(i => i.IsTodaysTalent() && i.IsLiyue());
                }
                return this.todayLiyueTalent;
            }
        }

        private IEnumerable<Data.Materials.Weapons.Weapon>? todayLiyueWeaponAscension;
        public IEnumerable<Data.Materials.Weapons.Weapon> TodayLiyueWeaponAscension
        {
            get
            {
                if (this.todayLiyueWeaponAscension == null)
                {
                    this.todayLiyueWeaponAscension = this.dataService.DailyWeapons
                        .Where(i => i.IsTodaysWeapon() && i.IsLiyue());
                }
                return this.todayLiyueWeaponAscension;
            }
        }

        private IEnumerable<Character>? todayLiyueCharacter5;
        public IEnumerable<Character> TodayLiyueCharacter5
        {
            get
            {
                if (this.todayLiyueCharacter5 == null)
                {
                    this.todayLiyueCharacter5 = this.dataService.Characters
                        .Where(c => c.Star.ToRank() == 5 && c.Talent is not null && c.Talent.IsLiyue() && c.Talent.IsTodaysTalent());
                }
                return this.todayLiyueCharacter5;
            }
        }

        private IEnumerable<Character>? todayLiyueCharacter4;
        public IEnumerable<Character> TodayLiyueCharacter4
        {
            get
            {
                if (this.todayLiyueCharacter4 == null)
                {
                    this.todayLiyueCharacter4 = this.dataService.Characters
                        .Where(c => c.Star.ToRank() == 4 && c.Talent is not null && c.Talent.IsLiyue() && c.Talent.IsTodaysTalent());
                }
                return this.todayLiyueCharacter4;
            }
        }

        private IEnumerable<Weapon>? todayLiyueWeapon5;
        public IEnumerable<Weapon> TodayLiyueWeapon5
        {
            get
            {
                if (this.todayLiyueWeapon5 == null)
                {
                    this.todayLiyueWeapon5 = this.dataService.Weapons
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 5 && w.Ascension.IsLiyue() && w.Ascension.IsTodaysWeapon());
                }
                return this.todayLiyueWeapon5;
            }
        }

        private IEnumerable<Weapon>? todayLiyueWeapon4;
        public IEnumerable<Weapon> TodayLiyueWeapon4
        {
            get
            {
                if (this.todayLiyueWeapon4 == null)
                {
                    this.todayLiyueWeapon4 = this.dataService.Weapons
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 4 && w.Ascension.IsLiyue() && w.Ascension.IsTodaysWeapon());
                }
                return this.todayLiyueWeapon4;
            }
        }
        #endregion

        #region Inazuma
        private IEnumerable<Talent>? todayInazumaTalent;
        public IEnumerable<Talent> TodayInazumaTalent
        {
            get
            {
                if (this.todayInazumaTalent == null)
                {
                    this.todayInazumaTalent = this.dataService.DailyTalents
                        .Where(i => i.IsTodaysTalent() && i.IsInazuma());
                }
                return this.todayInazumaTalent;
            }
        }

        private IEnumerable<Data.Materials.Weapons.Weapon>? todayInazumaWeaponAscension;
        public IEnumerable<Data.Materials.Weapons.Weapon> TodayInazumaWeaponAscension
        {
            get
            {
                if (this.todayInazumaWeaponAscension == null)
                {
                    this.todayInazumaWeaponAscension = this.dataService.DailyWeapons
                        .Where(i => i.IsTodaysWeapon() && i.IsInazuma());
                }
                return this.todayInazumaWeaponAscension;
            }
        }

        private IEnumerable<Character>? todayInazumaCharacter5;
        public IEnumerable<Character> TodayInazumaCharacter5
        {
            get
            {
                if (this.todayInazumaCharacter5 == null)
                {
                    this.todayInazumaCharacter5 = this.dataService.Characters
                        .Where(c => c.Star.ToRank() == 5 && c.Talent is not null && c.Talent.IsInazuma() && c.Talent.IsTodaysTalent());
                }
                return this.todayInazumaCharacter5;
            }
        }

        private IEnumerable<Character>? todayInazumaCharacter4;
        public IEnumerable<Character> TodayInazumaCharacter4
        {
            get
            {
                if (this.todayInazumaCharacter4 == null)
                {
                    this.todayInazumaCharacter4 = this.dataService.Characters
                        .Where(c => c.Star.ToRank() == 4 && c.Talent is not null && c.Talent.IsInazuma() && c.Talent.IsTodaysTalent());
                }
                return this.todayInazumaCharacter4;
            }
        }

        private IEnumerable<Weapon>? todayInazumaWeapon5;
        public IEnumerable<Weapon> TodayInazumaWeapon5
        {
            get
            {
                if (this.todayInazumaWeapon5 == null)
                {
                    this.todayInazumaWeapon5 = this.dataService.Weapons
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 5 && w.Ascension.IsInazuma() && w.Ascension.IsTodaysWeapon());
                }
                return this.todayInazumaWeapon5;
            }
        }

        private IEnumerable<Weapon>? todayInazumaWeapon4;
        public IEnumerable<Weapon> TodayInazumaWeapon4
        {
            get
            {
                if (this.todayInazumaWeapon4 == null)
                {
                    this.todayInazumaWeapon4 = this.dataService.Weapons
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 4 && w.Ascension.IsInazuma() && w.Ascension.IsTodaysWeapon());
                }
                return this.todayInazumaWeapon4;
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
