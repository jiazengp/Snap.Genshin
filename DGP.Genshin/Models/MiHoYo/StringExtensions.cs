using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace DGP.Genshin.Models.MiHoYo
{
    public static class StringExtensions
    {
        /// <summary>
        /// 将html字符串转换为普通字符串
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string RemoveHtmlFormat(this string s)
        {
            //color
            s = new Regex(@"<color=.*?>").Replace(s, "");
            s = s.Replace("</color>", "");
            //important mark
            s = s.Replace("<i>", "").Replace("</i>", "");
            //apply \n & \r char
            s = s.Replace(@"\n", "\n").Replace(@"\r", "\r");
            return s;
        }
    }

    public class HtmlStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            ((JValue)value).Value.ToString().RemoveHtmlFormat();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
