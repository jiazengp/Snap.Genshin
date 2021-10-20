using DGP.Genshin.Data.Characters;
using DGP.Snap.Framework.Extensions.System;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Services
{
    public class WeeklyViewService
    {
        private readonly MetaDataService dataService = MetaDataService.Instance;

        #region 风魔龙
        private IEnumerable<Character>? dvalinsPlume;
        public IEnumerable<Character> DvalinsPlume
        {
            get
            {
                if (this.dvalinsPlume == null)
                {
                    this.dvalinsPlume = this.dataService.Characters
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_461.png").ToList();
                }
                return this.dvalinsPlume;
            }
        }
        private IEnumerable<Character>? dvalinsClaw;
        public IEnumerable<Character> DvalinsClaw
        {
            get
            {
                if (this.dvalinsClaw == null)
                {
                    this.dvalinsClaw = this.dataService.Characters
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_462.png").ToList();
                }
                return this.dvalinsClaw;
            }
        }
        private IEnumerable<Character>? dvalinsSigh;
        public IEnumerable<Character> DvalinsSigh
        {
            get
            {
                if (this.dvalinsSigh == null)
                {
                    this.dvalinsSigh = this.dataService.Characters
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_463.png").ToList();
                }
                return this.dvalinsSigh;
            }
        }
        #endregion

        #region 北风的王狼
        private IEnumerable<Character>? tailofBoreas;
        public IEnumerable<Character> TailofBoreas
        {
            get
            {
                if (this.tailofBoreas == null)
                {
                    this.tailofBoreas = this.dataService.Characters
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_464.png").ToList();
                }
                return this.tailofBoreas;
            }
        }
        private IEnumerable<Character>? ringofBoreas;
        public IEnumerable<Character> RingofBoreas
        {
            get
            {
                if (this.ringofBoreas == null)
                {
                    this.ringofBoreas = this.dataService.Characters
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_465.png").ToList();
                }
                return this.ringofBoreas;
            }
        }
        private IEnumerable<Character>? spiritLocketofBoreas;
        public IEnumerable<Character> SpiritLocketofBoreas
        {
            get
            {
                if (this.spiritLocketofBoreas == null)
                {
                    this.spiritLocketofBoreas = this.dataService.Characters
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_466.png").ToList();
                }
                return this.spiritLocketofBoreas;
            }
        }
        #endregion

        #region 公子
        private IEnumerable<Character>? tuskofMonocerosCaeli;
        public IEnumerable<Character> TuskofMonocerosCaeli
        {
            get
            {
                if (this.tuskofMonocerosCaeli == null)
                {
                    this.tuskofMonocerosCaeli = this.dataService.Characters
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_467.png").ToList();
                }
                return this.tuskofMonocerosCaeli;
            }
        }
        private IEnumerable<Character>? shardofaFoulLegacy;
        public IEnumerable<Character> ShardofaFoulLegacy
        {
            get
            {
                if (this.shardofaFoulLegacy == null)
                {
                    this.shardofaFoulLegacy = this.dataService.Characters
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_468.png").ToList();
                }
                return this.shardofaFoulLegacy;
            }
        }
        private IEnumerable<Character>? shadowoftheWarrior;
        public IEnumerable<Character> ShadowoftheWarrior
        {
            get
            {
                if (this.shadowoftheWarrior == null)
                {
                    this.shadowoftheWarrior = this.dataService.Characters
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_469.png").ToList();
                }
                return this.shadowoftheWarrior;
            }
        }
        #endregion

        #region 若陀龙王
        private IEnumerable<Character>? dragonLordsCrown;
        public IEnumerable<Character> DragonLordsCrown
        {
            get
            {
                if (this.dragonLordsCrown == null)
                {
                    this.dragonLordsCrown = this.dataService.Characters
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_470.png").ToList();
                }
                return this.dragonLordsCrown;
            }
        }
        private IEnumerable<Character>? bloodjadeBranch;
        public IEnumerable<Character> BloodjadeBranch
        {
            get
            {
                if (this.bloodjadeBranch == null)
                {
                    this.bloodjadeBranch = this.dataService.Characters
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_471.png").ToList();
                }
                return this.bloodjadeBranch;
            }
        }
        private IEnumerable<Character>? gildedScale;
        public IEnumerable<Character> GildedScale
        {
            get
            {
                if (this.gildedScale == null)
                {
                    this.gildedScale = this.dataService.Characters
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_472.png").ToList();
                }
                return this.gildedScale;
            }
        }
        #endregion

        #region 女士
        private IEnumerable<Character>? moltenMoment;
        public IEnumerable<Character> MoltenMoment
        {
            get
            {
                if (this.moltenMoment == null)
                {
                    this.moltenMoment = this.dataService.Characters
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_480.png").ToList();
                }
                return this.moltenMoment;
            }
        }
        private IEnumerable<Character>? hellfireButterfly;
        public IEnumerable<Character> HellfireButterfly
        {
            get
            {
                if (this.hellfireButterfly == null)
                {
                    this.hellfireButterfly = this.dataService.Characters
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_481.png").ToList();
                }
                return this.hellfireButterfly;
            }
        }
        private IEnumerable<Character>? ashenHeart;
        public IEnumerable<Character> AshenHeart
        {
            get
            {
                if (this.ashenHeart == null)
                {
                    this.ashenHeart = this.dataService.Characters
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_482.png").ToList();
                }
                return this.ashenHeart;
            }
        }
        #endregion

        #region 单例
        private static WeeklyViewService? instance;
        private static readonly object _lock = new();
        private WeeklyViewService()
        {
            this.Log("initialized");
        }
        public static WeeklyViewService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new WeeklyViewService();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion
    }
}
