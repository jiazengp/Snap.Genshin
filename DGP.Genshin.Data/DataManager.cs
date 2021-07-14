using DGP.Genshin.Data.Characters;
using DGP.Genshin.Data.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DGP.Genshin.Data
{
    public class DataManager
    {
        #region Characters
        private ResourceDictionary characterResources;
        public ResourceDictionary CharacterResources
        {
            get
            {
                if (this.characterResources == null)
                {
                    this.characterResources = new ResourceDictionary 
                    {
                        Source = new Uri($"pack://application:,,,/DGP.Genshin.Data;component/Characters/Characters.xaml")
                    };
                }

                return this.characterResources;
            }
        }
        public IEnumerable<Character> GetAllCharacters() => CharacterResources.Values.OfType<Character>();
        #endregion
        #region Weapons
        private ResourceDictionary weaponResources;
        public ResourceDictionary WeaponResources
        {
            get
            {
                if (this.weaponResources == null)
                {
                    this.weaponResources = new ResourceDictionary
                    {
                        Source = new Uri($"pack://application:,,,/DGP.Genshin.Data;component/Weapons/Weapons.xaml")
                    };
                }

                return this.weaponResources;
            }
        }
        public IEnumerable<Weapon> GetAllWeapons() => WeaponResources.Values.OfType<Weapon>();
        #endregion
        #region 单例
        private static DataManager instance;
        private static readonly object _lock = new object();
        private DataManager()
        {
        }
        public static DataManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new DataManager();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion
    }
}
