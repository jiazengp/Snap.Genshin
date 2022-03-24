using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace DGP.Genshin.Helper
{
    /// <summary>
    /// https://github.com/HandyOrg/HandyControl/blob/master/src/Shared/HandyControl_Shared/Tools/Helper/AnimationHelper.cs
    /// </summary>
    internal class AnimationHelper
    {
        /// <summary>
        /// 创建一个Double动画
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

        public static DoubleAnimation CreateAnimation<TEasingFunction>(double toValue, double milliseconds=200,FillBehavior fillBehavior = FillBehavior.Stop) where TEasingFunction : EasingFunctionBase, new()
        {
            return new(toValue, new Duration(TimeSpan.FromMilliseconds(milliseconds)), fillBehavior)
            {
                EasingFunction = new TEasingFunction { EasingMode = EasingMode.EaseInOut }
            };
        }
    }
}
