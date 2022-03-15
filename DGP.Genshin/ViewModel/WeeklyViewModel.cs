using DGP.Genshin.DataModel.Character;
using Snap.Core.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    internal class WeeklyViewModel
    {
        private readonly MetadataViewModel dataService;

        #region 风魔龙
        private IEnumerable<Character>? dvalinsPlume;
        public IEnumerable<Character>? DvalinsPlume
        {
            get
            {
                if (dvalinsPlume == null)
                {
                    dvalinsPlume = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_461.png").ToList();
                }
                return dvalinsPlume;
            }
        }

        private IEnumerable<Character>? dvalinsClaw;
        public IEnumerable<Character>? DvalinsClaw
        {
            get
            {
                if (dvalinsClaw == null)
                {
                    dvalinsClaw = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_462.png").ToList();
                }
                return dvalinsClaw;
            }
        }

        private IEnumerable<Character>? dvalinsSigh;
        public IEnumerable<Character>? DvalinsSigh
        {
            get
            {
                if (dvalinsSigh == null)
                {
                    dvalinsSigh = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_463.png").ToList();
                }
                return dvalinsSigh;
            }
        }
        #endregion

        #region 北风的王狼
        private IEnumerable<Character>? tailofBoreas;
        public IEnumerable<Character>? TailofBoreas
        {
            get
            {
                if (tailofBoreas == null)
                {
                    tailofBoreas = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_464.png").ToList();
                }
                return tailofBoreas;
            }
        }

        private IEnumerable<Character>? ringofBoreas;
        public IEnumerable<Character>? RingofBoreas
        {
            get
            {
                if (ringofBoreas == null)
                {
                    ringofBoreas = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_465.png").ToList();
                }
                return ringofBoreas;
            }
        }

        private IEnumerable<Character>? spiritLocketofBoreas;
        public IEnumerable<Character>? SpiritLocketofBoreas
        {
            get
            {
                if (spiritLocketofBoreas == null)
                {
                    spiritLocketofBoreas = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_466.png").ToList();
                }
                return spiritLocketofBoreas;
            }
        }
        #endregion

        #region 公子
        private IEnumerable<Character>? tuskofMonocerosCaeli;
        public IEnumerable<Character>? TuskofMonocerosCaeli
        {
            get
            {
                if (tuskofMonocerosCaeli == null)
                {
                    tuskofMonocerosCaeli = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_467.png").ToList();
                }
                return tuskofMonocerosCaeli;
            }
        }

        private IEnumerable<Character>? shardofaFoulLegacy;
        public IEnumerable<Character>? ShardofaFoulLegacy
        {
            get
            {
                if (shardofaFoulLegacy == null)
                {
                    shardofaFoulLegacy = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_468.png").ToList();
                }
                return shardofaFoulLegacy;
            }
        }

        private IEnumerable<Character>? shadowoftheWarrior;
        public IEnumerable<Character>? ShadowoftheWarrior
        {
            get
            {
                if (shadowoftheWarrior == null)
                {
                    shadowoftheWarrior = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_469.png").ToList();
                }
                return shadowoftheWarrior;
            }
        }
        #endregion

        #region 若陀龙王
        private IEnumerable<Character>? dragonLordsCrown;
        public IEnumerable<Character>? DragonLordsCrown
        {
            get
            {
                if (dragonLordsCrown == null)
                {
                    dragonLordsCrown = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_470.png").ToList();
                }
                return dragonLordsCrown;
            }
        }

        private IEnumerable<Character>? bloodjadeBranch;
        public IEnumerable<Character>? BloodjadeBranch
        {
            get
            {
                if (bloodjadeBranch == null)
                {
                    bloodjadeBranch = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_471.png").ToList();
                }
                return bloodjadeBranch;
            }
        }

        private IEnumerable<Character>? gildedScale;
        public IEnumerable<Character>? GildedScale
        {
            get
            {
                if (gildedScale == null)
                {
                    gildedScale = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_472.png").ToList();
                }
                return gildedScale;
            }
        }
        #endregion

        #region 女士
        private IEnumerable<Character>? moltenMoment;
        public IEnumerable<Character>? MoltenMoment
        {
            get
            {
                if (moltenMoment == null)
                {
                    moltenMoment = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_480.png").ToList();
                }
                return moltenMoment;
            }
        }

        private IEnumerable<Character>? hellfireButterfly;
        public IEnumerable<Character>? HellfireButterfly
        {
            get
            {
                if (hellfireButterfly == null)
                {
                    hellfireButterfly = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_481.png").ToList();
                }
                return hellfireButterfly;
            }
        }

        private IEnumerable<Character>? ashenHeart;
        public IEnumerable<Character>? AshenHeart
        {
            get
            {
                if (ashenHeart == null)
                {
                    ashenHeart = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_482.png").ToList();
                }
                return ashenHeart;
            }
        }
        #endregion

        #region 雷电将军
        private IEnumerable<Character>? mudraoftheMaleficGeneral;
        public IEnumerable<Character>? MudraoftheMaleficGeneral
        {
            get
            {
                if (mudraoftheMaleficGeneral == null)
                {
                    mudraoftheMaleficGeneral = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_483.png").ToList();
                }
                return mudraoftheMaleficGeneral;
            }
        }

        private IEnumerable<Character>? tearsoftheCalamitousGod;
        public IEnumerable<Character>? TearsoftheCalamitousGod
        {
            get
            {
                if (tearsoftheCalamitousGod == null)
                {
                    tearsoftheCalamitousGod = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_484.png").ToList();
                }
                return tearsoftheCalamitousGod;
            }
        }

        private IEnumerable<Character>? theMeaningofAeons;
        public IEnumerable<Character>? TheMeaningofAeons
        {
            get
            {
                if (theMeaningofAeons == null)
                {
                    theMeaningofAeons = dataService.Characters?
                        .Where(c => c.Weekly?.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_485.png").ToList();
                }
                return theMeaningofAeons;
            }
        }
        #endregion

        public WeeklyViewModel(MetadataViewModel dataService)
        {
            this.dataService = dataService;
        }
    }
}
