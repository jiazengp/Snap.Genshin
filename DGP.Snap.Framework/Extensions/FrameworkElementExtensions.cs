using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DGP.Snap.Framework.Extensions
{
    public static class FrameworkElementExtensions
    {
        public static bool GoToElementState(this FrameworkElement element,string stateName,bool useTransions)
        {
            return VisualStateManager.GoToElementState(element, stateName, useTransions);
        }
    }
}
