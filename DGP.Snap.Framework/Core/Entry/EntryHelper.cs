using System;
using System.Reflection;

namespace DGP.Snap.Framework.Core.Entry
{
    internal class EntryHelper
    {
        public static Assembly EntryAssembly => Assembly.GetEntryAssembly();
        
        public static Type[] GetEntryTypes()
        {
            return EntryAssembly.GetTypes();
        }
    }
}
