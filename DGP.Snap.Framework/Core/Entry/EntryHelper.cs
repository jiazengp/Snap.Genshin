using System;
using System.Collections.Generic;
using System.Reflection;

namespace DGP.Snap.Framework.Core.Entry
{
    /// <summary>
    /// entry point assembly manager
    /// </summary>
    internal class EntryHelper
    {
        public static Assembly EntryAssembly => Assembly.GetEntryAssembly();
        public static Assembly FrameworkAssembly => Assembly.GetExecutingAssembly();

        public static IEnumerable<Type> GetCurrentTypes()
        {
            foreach (Type t in EntryAssembly.GetTypes())
                yield return t;
            foreach (Type t in FrameworkAssembly.GetTypes())
                yield return t;
        }
    }
}
