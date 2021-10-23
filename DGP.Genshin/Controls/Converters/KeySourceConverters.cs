using DGP.Genshin.DataModel;
using DGP.Genshin.DataModel.Materials.GemStones;
using DGP.Genshin.DataModel.Materials.Locals;
using DGP.Genshin.DataModel.Materials.Monsters;
using DGP.Genshin.DataModel.Materials.Talents;
using DGP.Genshin.DataModel.Materials.Weeklys;
using DGP.Genshin.Services;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace DGP.Genshin.Controls.Converters
{
    public class CityStringConverter : IValueConverter
    {
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            KeySource item = (KeySource)value;
            return item?.Source;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MetaDataService.Instance.Cities.First(i => i.Source == (string)value);
        }
    }
    public class ElementStringConverter : IValueConverter
    {
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            KeySource item = (KeySource)value;
            return item?.Source;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MetaDataService.Instance.Elements.First(i => i.Source == (string)value);
        }
    }
    public class StarStringConverter : IValueConverter
    {
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            KeySource item = (KeySource)value;
            return item?.Source;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MetaDataService.Instance.Stars.First(i => i.Source == (string)value);
        }
    }
    public class WeaponTypeStringConverter : IValueConverter
    {
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            KeySource item = (KeySource)value;
            return item?.Source;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MetaDataService.Instance.WeaponTypes.FirstOrDefault(i => i.Source == (string)value);
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
            return value == null
                ? MetaDataService.Instance.DailyWeapons.First()
                : MetaDataService.Instance.DailyWeapons.First(i => i.Source == ((DataModel.Materials.Weapons.Weapon)value).Source);
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
            return value == null
                ? MetaDataService.Instance.Elites.First()
                : MetaDataService.Instance.Elites.First(i => i.Source == ((Elite)value).Source);
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
            return value == null
                ? MetaDataService.Instance.Monsters.First()
                : MetaDataService.Instance.Monsters.First(i => i.Source == ((Monster)value).Source);
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
            return MetaDataService.Instance.DailyTalents.First(i => i.Source == ((Talent)value).Source);
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
            return MetaDataService.Instance.WeeklyTalents.First(i => i.Source == ((Weekly)value).Source);
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
            return MetaDataService.Instance.Bosses.First(i => i.Source == ((Boss)value).Source);
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
            return MetaDataService.Instance.GemStones.First(i => i.Source == ((GemStone)value).Source);
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
            return MetaDataService.Instance.Locals.First(i => i.Source == ((Local)value).Source);
        }
    }
}
