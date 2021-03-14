using DGP.Genshin.Services;
using DGP.Snap.Framework.Core.LifeCycle;
using System;
using System.Linq;
using System.Windows;

namespace DGP.Genshin.Data.Character
{
    public class CharacterManager
    {
        private readonly ResourceDictionary characterDictionary = new()
        {
            Source = new Uri("/Data/Character/CharacterDictionary.xaml", UriKind.Relative)
        };
        public CharacterCollection Characters =>
            new(this.characterDictionary.Values.OfType<Character>().Where(i => UnreleasedPolicyFilter(i)).Where(i => i.IsPresent));
        public Character this[string key]
        {
            get => (Character)this.characterDictionary[key];
            set => this.characterDictionary[key] = value;
        }

        public static bool UnreleasedPolicyFilter(Character item) =>
            item.IsReleased || LifeCycle.InstanceOf<SettingService>().GetOrDefault(Setting.ShowUnreleasedData, false);

        #region 单例
        private static CharacterManager instance;
        private static readonly object _lock = new object();
        private CharacterManager()
        {

        }
        public static CharacterManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new CharacterManager();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion
    }

}
