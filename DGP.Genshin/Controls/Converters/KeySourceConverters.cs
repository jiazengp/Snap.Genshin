using DGP.Genshin.Data;
using DGP.Genshin.Data.Materials.GemStones;
using DGP.Genshin.Data.Materials.Locals;
using DGP.Genshin.Data.Materials.Monsters;
using DGP.Genshin.Data.Materials.Talents;
using DGP.Genshin.Data.Materials.Weeklys;
using DGP.Genshin.Services;
using DGP.Snap.Framework.Core.LifeCycling;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace DGP.Genshin.Controls.Converters
{
    public class CityStringConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            KeySource item = (KeySource)value;
            return item.Source;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LifeCycle.InstanceOf<DataService>().Cities.First(i => i.Source == (string)value);
        }
    }
    public class ElementStringConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            KeySource item = (KeySource)value;
            return item.Source;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LifeCycle.InstanceOf<DataService>().Elements.First(i => i.Source == (string)value);
        }
    }
    public class StarStringConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            KeySource item = (KeySource)value;
            return item.Source;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LifeCycle.InstanceOf<DataService>().Stars.First(i => i.Source == (string)value);
        }
    }
    public class WeaponTypeStringConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            KeySource item = (KeySource)value;
            return item.Source;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LifeCycle.InstanceOf<DataService>().WeaponTypes.FirstOrDefault(i => i.Source == (string)value);
        }
    }

    public class DailyWeaponsConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LifeCycle.InstanceOf<DataService>().DailyWeapons.First(i => i.Source == ((Data.Materials.Weapons.Weapon)value).Source);
        }
    }
    public class ElitesConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LifeCycle.InstanceOf<DataService>().Elites.First(i => i.Source == ((Elite)value).Source);
        }
    }
    public class MonstersConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LifeCycle.InstanceOf<DataService>().Monsters.First(i => i.Source == ((Monster)value).Source);
        }
    }
    public class DailyTalentsConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LifeCycle.InstanceOf<DataService>().DailyTalents.First(i => i.Source == ((Talent)value).Source);
        }
    }
    public class WeeklyTalentsConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LifeCycle.InstanceOf<DataService>().WeeklyTalents.First(i => i.Source == ((Weekly)value).Source);
        }
    }
    public class BossesConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LifeCycle.InstanceOf<DataService>().Bosses.First(i => i.Source == ((Boss)value).Source);
        }
    }
    public class GemStonesConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LifeCycle.InstanceOf<DataService>().GemStones.First(i => i.Source == ((GemStone)value).Source);
        }
    }
    public class LocalsConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LifeCycle.InstanceOf<DataService>().Locals.First(i => i.Source == ((Local)value).Source);
        }
    }

}
