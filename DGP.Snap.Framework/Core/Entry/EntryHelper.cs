using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Snap.Framework.Core.Entry
{
    internal class EntryHelper
    {
        public static Assembly EntryAssembly => Assembly.GetEntryAssembly();
        
        public static Type[] GetEntryTypeInfos()
        {
            return EntryAssembly.GetTypes();
        }
    }
}
