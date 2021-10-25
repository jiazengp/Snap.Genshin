using DGP.Genshin.Common.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Common.Extensions.System.Collections.Generic
{
    public static class ListExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="shouldRemovePredicate"></param>
        /// <returns></returns>
        public static bool RemoveFirstWhere<T>(this IList<T> list, Func<T, bool> shouldRemovePredicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (shouldRemovePredicate.Invoke(list[i]))
                {
                    list.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public static List<T> ClonePartially<T>(this List<T> listToClone) where T : IPartiallyCloneable<T>
        {
            return listToClone.Select(item => item.ClonePartially()).ToList();
        }
    }
}
