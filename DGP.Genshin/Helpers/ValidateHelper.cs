using System;

namespace DGP.Genshin.Helpers
{
    /// <summary>
    ///     验证帮助类
    /// </summary>
    public class ValidateHelper
    {
        /// <summary>
        /// 是否在浮点数范围内
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInRangeOfDouble(object value)
        {
            double v = (double)value;
            return !(Double.IsNaN(v) || Double.IsInfinity(v));
        }

        /// <summary>
        /// 是否在正浮点数范围内
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInRangeOfPosDouble(object value)
        {
            double v = (double)value;
            return !(Double.IsNaN(v) || Double.IsInfinity(v)) && v > 0;
        }

        /// <summary>
        /// 是否在正浮点数范围内（包括0）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInRangeOfPosDoubleIncludeZero(object value)
        {
            double v = (double)value;
            return !(Double.IsNaN(v) || Double.IsInfinity(v)) && v >= 0;
        }

        /// <summary>
        /// 是否在负浮点数范围内
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInRangeOfNegDouble(object value)
        {
            double v = (double)value;
            return !(Double.IsNaN(v) || Double.IsInfinity(v)) && v < 0;
        }

        /// <summary>
        /// 是否在负浮点数范围内（包括0）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInRangeOfNegDoubleIncludeZero(object value)
        {
            double v = (double)value;
            return !(Double.IsNaN(v) || Double.IsInfinity(v)) && v <= 0;
        }

        /// <summary>
        /// 是否在正整数范围内
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInRangeOfPosInt(object value)
        {
            int v = (int)value;
            return v > 0;
        }

        /// <summary>
        /// 是否在正整数范围内（包括0）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInRangeOfPosIntIncludeZero(object value)
        {
            int v = (int)value;
            return v >= 0;
        }

        /// <summary>
        /// 是否在负整数范围内
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInRangeOfNegInt(object value)
        {
            int v = (int)value;
            return v < 0;
        }

        /// <summary>
        /// 是否在负整数范围内（包括0）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInRangeOfNegIntIncludeZero(object value)
        {
            int v = (int)value;
            return v <= 0;
        }
    }
}
