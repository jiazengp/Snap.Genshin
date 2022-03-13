using DGP.Genshin.DataModel.Character;
using DGP.Genshin.DataModel.Helper;
using DGP.Genshin.DataModel.Material;
using DGP.Genshin.Factory.Abstraction;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Core.Mvvm;
using Snap.Data.Primitive;
using Snap.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DataModelWeapon = DGP.Genshin.DataModel.Weapon;
using WeaponMaterial = DGP.Genshin.DataModel.Material.Weapon;

namespace DGP.Genshin.ViewModel
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

        [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "<挂起>")]
        private async void TriggerPropertyChanged(params string[] cities)
        {
            foreach (string city in cities)
            {
                this.SetPrivateFieldValueByName($"today{city}Talent", null);
                this.SetPrivateFieldValueByName($"today{city}WeaponAscension", null);
                this.SetPrivateFieldValueByName($"today{city}Character5", null);
                this.SetPrivateFieldValueByName($"today{city}Character4", null);
                this.SetPrivateFieldValueByName($"today{city}Weapon5", null);
                this.SetPrivateFieldValueByName($"today{city}Weapon4", null);

                OnPropertyChanged($"Today{city}Talent");
                await Task.Delay(50);
                OnPropertyChanged($"Today{city}Character5");
                await Task.Delay(50);
                OnPropertyChanged($"Today{city}Character4");
                await Task.Delay(50);
                OnPropertyChanged($"Today{city}WeaponAscension");
                await Task.Delay(50);
                OnPropertyChanged($"Today{city}Weapon5");
                await Task.Delay(50);
                OnPropertyChanged($"Today{city}Weapon4");
            }
        }

        public DailyViewModel(MetadataViewModel metadataViewModel, IAsyncRelayCommandFactory asyncRelayCommandFactory)
        {
            dataViewModel = metadataViewModel;

            OpenUICommand = asyncRelayCommandFactory.Create(OpenUIAsync);
        }

        private async Task OpenUIAsync()
        {
            await Task.Delay(500);
            DateTime day = DateTime.UtcNow + TimeSpan.FromHours(4);
            this.Log($"current dailyview date:{day}");
            SelectedDayOfWeek = DayOfWeeks
                .First(d => d.Value == day.DayOfWeek);
        }

        #region Mondstadt
        private IEnumerable<Talent>? todayMondstadtTalent;
        public IEnumerable<Talent>? TodayMondstadtTalent
        {
            get
            {
                todayMondstadtTalent ??= dataViewModel.DailyTalents?
                    .Where(i => i.IsTodaysTalent(SelectedDayOfWeek?.Value) && i.IsMondstadt());
                return todayMondstadtTalent;
            }
        }

        private IEnumerable<WeaponMaterial>? todayMondstadtWeaponAscension;
        public IEnumerable<WeaponMaterial>? TodayMondstadtWeaponAscension
        {
            get
            {
                todayMondstadtWeaponAscension ??= dataViewModel.DailyWeapons?
                    .Where(i => i.IsTodaysWeapon(SelectedDayOfWeek?.Value) && i.IsMondstadt());
                return todayMondstadtWeaponAscension;
            }
        }

        private IEnumerable<Character>? todayMondstadtCharacter5;
        public IEnumerable<Character>? TodayMondstadtCharacter5
        {
            get
            {
                todayMondstadtCharacter5 ??= dataViewModel.Characters?
                    .Where(c => c.Star.ToInt32Rank() == 5
                    && c.Talent is not null
                    && c.Talent.IsMondstadt()
                    && c.Talent.IsTodaysTalent(SelectedDayOfWeek?.Value));
                return todayMondstadtCharacter5;
            }
        }

        private IEnumerable<Character>? todayMondstadtCharacter4;
        public IEnumerable<Character>? TodayMondstadtCharacter4
        {
            get
            {
                todayMondstadtCharacter4 ??= dataViewModel.Characters?
                    .Where(c => c.Star.ToInt32Rank() == 4
                    && c.Talent is not null
                    && c.Talent.IsMondstadt()
                    && c.Talent.IsTodaysTalent(SelectedDayOfWeek?.Value));
                return todayMondstadtCharacter4;
            }
        }

        private IEnumerable<DataModelWeapon>? todayMondstadtWeapon5;
        public IEnumerable<DataModelWeapon>? TodayMondstadtWeapon5
        {
            get
            {
                todayMondstadtWeapon5 ??= dataViewModel.Weapons?
                    .Where(w => w.Ascension != null
                    && w.Star.ToInt32Rank() == 5
                    && w.Ascension.IsMondstadt()
                    && w.Ascension.IsTodaysWeapon(SelectedDayOfWeek?.Value));
                return todayMondstadtWeapon5;
            }
        }

        private IEnumerable<DataModelWeapon>? todayMondstadtWeapon4;
        public IEnumerable<DataModelWeapon>? TodayMondstadtWeapon4
        {
            get
            {
                todayMondstadtWeapon4 ??= dataViewModel.Weapons?
                    .Where(w => w.Ascension != null
                    && w.Star.ToInt32Rank() == 4
                    && w.Ascension.IsMondstadt()
                    && w.Ascension.IsTodaysWeapon(SelectedDayOfWeek?.Value));
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
                todayLiyueTalent ??= dataViewModel.DailyTalents?
                    .Where(i => i.IsTodaysTalent(SelectedDayOfWeek?.Value) && i.IsLiyue());
                return todayLiyueTalent;
            }
        }

        private IEnumerable<WeaponMaterial>? todayLiyueWeaponAscension;
        public IEnumerable<WeaponMaterial>? TodayLiyueWeaponAscension
        {
            get
            {
                todayLiyueWeaponAscension ??= dataViewModel.DailyWeapons?
                    .Where(i => i.IsTodaysWeapon(SelectedDayOfWeek?.Value) && i.IsLiyue());
                return todayLiyueWeaponAscension;
            }
        }

        private IEnumerable<Character>? todayLiyueCharacter5;
        public IEnumerable<Character>? TodayLiyueCharacter5
        {
            get
            {
                todayLiyueCharacter5 ??= dataViewModel.Characters?
                    .Where(c => c.Star.ToInt32Rank() == 5
                    && c.Talent is not null
                    && c.Talent.IsLiyue()
                    && c.Talent.IsTodaysTalent(SelectedDayOfWeek?.Value));
                return todayLiyueCharacter5;
            }
        }

        private IEnumerable<Character>? todayLiyueCharacter4;
        public IEnumerable<Character>? TodayLiyueCharacter4
        {
            get
            {
                todayLiyueCharacter4 ??= dataViewModel.Characters?
                    .Where(c => c.Star.ToInt32Rank() == 4
                    && c.Talent is not null
                    && c.Talent.IsLiyue()
                    && c.Talent.IsTodaysTalent(SelectedDayOfWeek?.Value));
                return todayLiyueCharacter4;
            }
        }

        private IEnumerable<DataModelWeapon>? todayLiyueWeapon5;
        public IEnumerable<DataModelWeapon>? TodayLiyueWeapon5
        {
            get
            {
                todayLiyueWeapon5 ??= dataViewModel.Weapons?
                    .Where(w => w.Ascension != null
                    && w.Star.ToInt32Rank() == 5
                    && w.Ascension.IsLiyue()
                    && w.Ascension.IsTodaysWeapon(SelectedDayOfWeek?.Value));
                return todayLiyueWeapon5;
            }
        }

        private IEnumerable<DataModelWeapon>? todayLiyueWeapon4;
        public IEnumerable<DataModelWeapon>? TodayLiyueWeapon4
        {
            get
            {
                todayLiyueWeapon4 ??= dataViewModel.Weapons?
                    .Where(w => w.Ascension != null
                    && w.Star.ToInt32Rank() == 4
                    && w.Ascension.IsLiyue()
                    && w.Ascension.IsTodaysWeapon(SelectedDayOfWeek?.Value));
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
                todayInazumaTalent ??= dataViewModel.DailyTalents?
                    .Where(i => i.IsTodaysTalent(SelectedDayOfWeek?.Value) && i.IsInazuma());
                return todayInazumaTalent;
            }
        }

        private IEnumerable<WeaponMaterial>? todayInazumaWeaponAscension;
        public IEnumerable<WeaponMaterial>? TodayInazumaWeaponAscension
        {
            get
            {
                todayInazumaWeaponAscension ??= dataViewModel.DailyWeapons?
                    .Where(i => i.IsTodaysWeapon(SelectedDayOfWeek?.Value) && i.IsInazuma());
                return todayInazumaWeaponAscension;
            }
        }

        private IEnumerable<Character>? todayInazumaCharacter5;
        public IEnumerable<Character>? TodayInazumaCharacter5
        {
            get
            {
                todayInazumaCharacter5 ??= dataViewModel.Characters?
                    .Where(c => c.Star.ToInt32Rank() == 5
                    && c.Talent is not null
                    && c.Talent.IsInazuma()
                    && c.Talent.IsTodaysTalent(SelectedDayOfWeek?.Value));
                return todayInazumaCharacter5;
            }
        }

        private IEnumerable<Character>? todayInazumaCharacter4;
        public IEnumerable<Character>? TodayInazumaCharacter4
        {
            get
            {
                todayInazumaCharacter4 ??= dataViewModel.Characters?
                    .Where(c => c.Star.ToInt32Rank() == 4
                    && c.Talent is not null
                    && c.Talent.IsInazuma()
                    && c.Talent.IsTodaysTalent(SelectedDayOfWeek?.Value));
                return todayInazumaCharacter4;
            }
        }

        private IEnumerable<DataModelWeapon>? todayInazumaWeapon5;
        public IEnumerable<DataModelWeapon>? TodayInazumaWeapon5
        {
            get
            {
                todayInazumaWeapon5 ??= dataViewModel.Weapons?
                    .Where(w => w.Ascension != null
                    && w.Star.ToInt32Rank() == 5
                    && w.Ascension.IsInazuma()
                    && w.Ascension.IsTodaysWeapon(SelectedDayOfWeek?.Value));
                return todayInazumaWeapon5;
            }
        }

        private IEnumerable<DataModelWeapon>? todayInazumaWeapon4;


        public IEnumerable<DataModelWeapon>? TodayInazumaWeapon4
        {
            get
            {
                todayInazumaWeapon4 ??= dataViewModel.Weapons?
                    .Where(w => w.Ascension != null
                    && w.Star.ToInt32Rank() == 4
                    && w.Ascension.IsInazuma()
                    && w.Ascension.IsTodaysWeapon(SelectedDayOfWeek?.Value));
                return todayInazumaWeapon4;
            }
        }
        #endregion
    }
}
