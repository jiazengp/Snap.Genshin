using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Snap.Framework.Attributes
{
    public class AttributeService
    {
        private Assembly operatingAssemby;

        public void SetAssembly(Assembly assembly)
        {
            operatingAssemby = assembly;
        }
    }
}
