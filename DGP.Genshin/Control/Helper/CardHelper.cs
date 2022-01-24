using System.Windows;
using WPFUI.Controls;

namespace DGP.Genshin.Control.Helper
{
    internal class CardHelper
    {
        #region CardAction Attach
        public static string GetActionTitle(CardAction obj)
        {
            return (string)obj.GetValue(ActionTitleProperty);
        }

        public static void SetActionTitle(CardAction obj, string value)
        {
            obj.SetValue(ActionTitleProperty, value);
        }
        public static readonly DependencyProperty ActionTitleProperty =
            DependencyProperty.RegisterAttached("ActionTitle", typeof(string), typeof(CardHelper), new PropertyMetadata(""));

        public static string GetActionSubtitle(CardAction obj)
        {
            return (string)obj.GetValue(ActionSubtitleProperty);
        }

        public static void SetActionSubtitle(CardAction obj, string value)
        {
            obj.SetValue(ActionSubtitleProperty, value);
        }
        public static readonly DependencyProperty ActionSubtitleProperty =
            DependencyProperty.RegisterAttached("ActionSubtitle", typeof(string), typeof(CardHelper), new PropertyMetadata(""));
        #endregion
    }
}
