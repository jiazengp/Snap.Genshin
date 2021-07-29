using DGP.Snap.Framework.Data.Regex;
using DGP.Snap.Framework.Extensions.System;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DGP.Genshin.Helpers
{
    public class AnimationHelper
    {
        /// <summary>
        ///     创建一个Thickness动画
        /// </summary>
        /// <param name="thickness"></param>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static ThicknessAnimation CreateAnimation(Thickness thickness = default, double milliseconds = 200)
        {
            return new(thickness, new Duration(TimeSpan.FromMilliseconds(milliseconds)))
            {
                EasingFunction = new PowerEase { EasingMode = EasingMode.EaseInOut }
            };
        }

        /// <summary>
        ///     创建一个Double动画
        /// </summary>
        /// <param name="toValue"></param>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static DoubleAnimation CreateAnimation(double toValue, double milliseconds = 200)
        {
            return new(toValue, new Duration(TimeSpan.FromMilliseconds(milliseconds)))
            {
                EasingFunction = new PowerEase { EasingMode = EasingMode.EaseInOut }
            };
        }

        internal static void DecomposeGeometryStr(string geometryStr, out double[] arr)
        {
            MatchCollection collection = Regex.Matches(geometryStr, RegexPatterns.DigitsPattern);
            arr = new double[collection.Count];
            for (int i = 0; i < collection.Count; i++)
            {
                arr[i] = collection[i].Value.Value<double>();
            }
        }

        internal static Geometry ComposeGeometry(string[] strings, double[] arr)
        {
            StringBuilder builder = new StringBuilder(strings[0]);
            for (int i = 0; i < arr.Length; i++)
            {
                string s = strings[i + 1];
                double n = arr[i];
                if (!Double.IsNaN(n))
                {
                    builder.Append(n).Append(s);
                }
            }

            return Geometry.Parse(builder.ToString());
        }

        internal static Geometry InterpolateGeometry(double[] from, double[] to, double progress, string[] strings)
        {
            double[] accumulated = new double[to.Length];
            for (int i = 0; i < to.Length; i++)
            {
                double fromValue = from[i];
                accumulated[i] = fromValue + (to[i] - fromValue) * progress;
            }

            return ComposeGeometry(strings, accumulated);
        }

        internal static double[] InterpolateGeometryValue(double[] from, double[] to, double progress)
        {
            double[] accumulated = new double[to.Length];
            for (int i = 0; i < to.Length; i++)
            {
                double fromValue = from[i];
                accumulated[i] = fromValue + (to[i] - fromValue) * progress;
            }

            return accumulated;
        }
    }
}
