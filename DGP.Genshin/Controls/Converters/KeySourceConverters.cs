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

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return App.GetViewModel<MetadataViewModel>().Cities?.First(i => i.Source == (string)value);
        }
    }
    public class ElementStringConverter : IValueConverter
    {
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            KeySource item = (KeySource)value;
            return item?.Source;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return App.GetViewModel<MetadataViewModel>().Elements?.First(i => i.Source == (string)value);
        }
    }
    public class StarStringConverter : IValueConverter
    {
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            KeySource item = (KeySource)value;
            return item?.Source;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return App.GetViewModel<MetadataViewModel>().Stars?.First(i => i.Source == (string)value);
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
            return App.GetViewModel<MetadataViewModel>().WeaponTypes?.FirstOrDefault(i => i.Source == (string)value);
        }
    }

    public class DailyWeaponsConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null
                ? App.GetViewModel<MetadataViewModel>().DailyWeapons?.First()
                : App.GetViewModel<MetadataViewModel>().DailyWeapons?.First(i => i.Source == ((DataModel.Materials.Weapons.Weapon)value).Source);
        }
    }
    public class ElitesConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null
                ? App.GetViewModel<MetadataViewModel>().Elites?.First()
                : App.GetViewModel<MetadataViewModel>().Elites?.First(i => i.Source == ((Elite)value).Source);
        }
    }
    public class MonstersConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null
                ? App.GetViewModel<MetadataViewModel>().Monsters?.First()
                : App.GetViewModel<MetadataViewModel>().Monsters?.First(i => i.Source == ((Monster)value).Source);
        }
    }
    public class DailyTalentsConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return App.GetViewModel<MetadataViewModel>().DailyTalents?.First(i => i.Source == ((Talent)value).Source);
        }
    }
    public class WeeklyTalentsConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return App.GetViewModel<MetadataViewModel>().WeeklyTalents?.First(i => i.Source == ((Weekly)value).Source);
        }
    }
    public class BossesConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return App.GetViewModel<MetadataViewModel>().Bosses?.First(i => i.Source == ((Boss)value).Source);
        }
    }
    public class GemStonesConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return App.GetViewModel<MetadataViewModel>().GemStones?.First(i => i.Source == ((GemStone)value).Source);
        }
    }
    public class LocalsConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return App.GetViewModel<MetadataViewModel>().Locals?.First(i => i.Source == ((Local)value).Source);
        }
    }
}
