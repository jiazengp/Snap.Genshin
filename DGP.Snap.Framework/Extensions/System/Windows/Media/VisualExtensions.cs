using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DGP.Snap.Framework.Extensions.System.Windows.Media
{
    public static class VisualExtensions
    {
        /// <summary> 
        /// 获取父可视对象中第一个指定类型的子可视对象 
        /// </summary> 
        /// <typeparam name="T">可视对象类型</typeparam> 
        /// <param name="parent">父可视对象</param> 
        /// <returns>第一个指定类型的子可视对象</returns> 
        public static T FirstVisualChild<T>(this Visual parent) where T : Visual
        {
            T child = default(T);
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = FirstVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
    }
}
