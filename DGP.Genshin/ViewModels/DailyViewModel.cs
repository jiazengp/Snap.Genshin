using DGP.Genshin.DataModels.Characters;
using DGP.Genshin.DataModels.Helpers;
using DGP.Genshin.DataModels.Materials.Talents;
using DGP.Genshin.DataModels.Weapons;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using Snap.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using WeaponMaterial = DGP.Genshin.DataModels.Materials.Weapons.Weapon;

namespace DGP.Genshin.ViewModels
{
    /// <summary>
    /// 日常材料服务
    /// </summary>
    [ViewModel(InjectAs.Singleton)]
    public class DailyViewModel : ObservableObject2
    {
        private readonly MetadataViewModel dataViewModel;

        public List<NamedValue<DayOfWeek>> DayOfWeeks { get; } = new()
        {
            new("星期一", DayOfWeek.Monday),
            new("星期二", DayOfWeek.Tuesday),
            new("星期三", DayOfWeek.Wednesday),
            new("星期四", DayOfWeek.Thursday),
            new("星期五", DayOfWeek.Friday),
            new("星期六", DayOfWeek.Saturday),
            new("星期日", DayOfWeek.Sunday)
        };

        private NamedValue<DayOfWeek>? selectedDayOfWeek;
        public NamedValue<DayOfWeek>? SelectedDayOfWeek
        {
            get => selectedDayOfWeek;
            set => SetPropertyAndCallbackOnCompletion(ref selectedDayOfWeek, value, () => TriggerPropertyChanged("Mondstadt", "Liyue", "Inazuma"));
        }

        public ICommand OpenUICommand { get; }

        private async void TriggerPropertyChanged(params string[] cities)
        {
            foreach(string city in cities)
            {
                ClearFieldValueOf($"today{city}Talent");
                ClearFieldValueOf($"today{city}WeaponAscension");
                ClearFieldValueOf($"today{city}Character5");
                ClearFieldValueOf($"today{city}Character4");
                ClearFieldValueOf($"today{city}Weapon5");
                ClearFieldValueOf($"today{city}Weapon4");

                OnPropertyChanged($"Today{city}Talent");
                await Task.Delay(100);
                OnPropertyChanged($"Today{city}Character5");
                await Task.Delay(100);
                OnPropertyChanged($"Today{city}Character4");
                await Task.Delay(100);
                OnPropertyChanged($"Today{city}WeaponAscension");
                await Task.Delay(100);
                OnPropertyChanged($"Today{city}Weapon5");
                await Task.Delay(100);
                OnPropertyChanged($"Today{city}Weapon4");
            }
        }
        private void ClearFieldValueOf(string name)
        {
            FieldInfo? fieldInfo = typeof(DailyViewModel).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            fieldInfo?.SetValue(this, null);
        }

        public DailyViewModel(MetadataViewModel metadataViewModel)
        {
            dataViewModel = metadataViewModel;

            OpenUICommand = new AsyncRelayCommand(OpenUIAsync);
        }

        private async Task OpenUIAsync()
        {
            await Task.Delay(1000);
            SelectedDayOfWeek = DayOfWeeks.First(d => d.Value == DateTime.Now.DayOfWeek);
        }

        #region Mondstadt
        private IEnumerable<Talent>? todayMondstadtTalent;
        public IEnumerable<Talent>? TodayMondstadtTalent
        {
            get
            {
                if (todayMondstadtTalent == null)
                {
                    todayMondstadtTalent = dataViewModel.DailyTalents?
                        .Where(i => i.IsTodaysTalent(SelectedDayOfWeek?.Value) && i.IsMondstadt());
                }
                return todayMondstadtTalent;
            }
        }

        private IEnumerable<WeaponMaterial>? todayMondstadtWeaponAscension;
        public IEnumerable<WeaponMaterial>? TodayMondstadtWeaponAscension
        {
            get
            {
                if (todayMondstadtWeaponAscension == null)
                {
                    todayMondstadtWeaponAscension = dataViewModel.DailyWeapons?
                        .Where(i => i.IsTodaysWeapon(SelectedDayOfWeek?.Value) && i.IsMondstadt());
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
                    todayMondstadtCharacter5 = dataViewModel.Characters?
                        .Where(c => c.Star.ToRank() == 5 && c.Talent is not null && c.Talent.IsMondstadt() && c.Talent.IsTodaysTalent(SelectedDayOfWeek?.Value));
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
                    todayMondstadtCharacter4 = dataViewModel.Characters?
                        .Where(c => c.Star.ToRank() == 4 && c.Talent is not null && c.Talent.IsMondstadt() && c.Talent.IsTodaysTalent(SelectedDayOfWeek?.Value));
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
                    todayMondstadtWeapon5 = dataViewModel.Weapons?
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 5 && w.Ascension.IsMondstadt() && w.Ascension.IsTodaysWeapon(SelectedDayOfWeek?.Value));
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
                    todayMondstadtWeapon4 = dataViewModel.Weapons?
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 4 && w.Ascension.IsMondstadt() && w.Ascension.IsTodaysWeapon(SelectedDayOfWeek?.Value));
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
                    todayLiyueTalent = dataViewModel.DailyTalents?
                        .Where(i => i.IsTodaysTalent(SelectedDayOfWeek?.Value) && i.IsLiyue());
                }
                return todayLiyueTalent;
            }
        }

        private IEnumerable<WeaponMaterial>? todayLiyueWeaponAscension;
        public IEnumerable<WeaponMaterial>? TodayLiyueWeaponAscension
        {
            get
            {
                if (todayLiyueWeaponAscension == null)
                {
                    todayLiyueWeaponAscension = dataViewModel.DailyWeapons?
                        .Where(i => i.IsTodaysWeapon(SelectedDayOfWeek?.Value) && i.IsLiyue());
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
                    todayLiyueCharacter5 = dataViewModel.Characters?
                        .Where(c => c.Star.ToRank() == 5 && c.Talent is not null && c.Talent.IsLiyue() && c.Talent.IsTodaysTalent(SelectedDayOfWeek?.Value));
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
                    todayLiyueCharacter4 = dataViewModel.Characters?
                        .Where(c => c.Star.ToRank() == 4 && c.Talent is not null && c.Talent.IsLiyue() && c.Talent.IsTodaysTalent(SelectedDayOfWeek?.Value));
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
                    todayLiyueWeapon5 = dataViewModel.Weapons?
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 5 && w.Ascension.IsLiyue() && w.Ascension.IsTodaysWeapon(SelectedDayOfWeek?.Value));
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
                    todayLiyueWeapon4 = dataViewModel.Weapons?
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 4 && w.Ascension.IsLiyue() && w.Ascension.IsTodaysWeapon(SelectedDayOfWeek?.Value));
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
                    todayInazumaTalent = dataViewModel.DailyTalents?
                        .Where(i => i.IsTodaysTalent(SelectedDayOfWeek?.Value) && i.IsInazuma());
                }
                return todayInazumaTalent;
            }
        }

        private IEnumerable<WeaponMaterial>? todayInazumaWeaponAscension;
        public IEnumerable<WeaponMaterial>? TodayInazumaWeaponAscension
        {
            get
            {
                if (todayInazumaWeaponAscension == null)
                {
                    todayInazumaWeaponAscension = dataViewModel.DailyWeapons?
                        .Where(i => i.IsTodaysWeapon(SelectedDayOfWeek?.Value) && i.IsInazuma());
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
                    todayInazumaCharacter5 = dataViewModel.Characters?
                        .Where(c => c.Star.ToRank() == 5 && c.Talent is not null && c.Talent.IsInazuma() && c.Talent.IsTodaysTalent(SelectedDayOfWeek?.Value));
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
                    todayInazumaCharacter4 = dataViewModel.Characters?
                        .Where(c => c.Star.ToRank() == 4 && c.Talent is not null && c.Talent.IsInazuma() && c.Talent.IsTodaysTalent(SelectedDayOfWeek?.Value));
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
                    todayInazumaWeapon5 = dataViewModel.Weapons?
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 5 && w.Ascension.IsInazuma() && w.Ascension.IsTodaysWeapon(SelectedDayOfWeek?.Value));
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
                    todayInazumaWeapon4 = dataViewModel.Weapons?
                        .Where(w => w.Ascension != null && w.Star.ToRank() == 4 && w.Ascension.IsInazuma() && w.Ascension.IsTodaysWeapon(SelectedDayOfWeek?.Value));
                }
                return todayInazumaWeapon4;
            }
        }
        #endregion
    }
}
