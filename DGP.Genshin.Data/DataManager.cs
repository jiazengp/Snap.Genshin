using DGP.Genshin.Data.Characters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DGP.Genshin.Data
{
    public class DataManager
    {
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
