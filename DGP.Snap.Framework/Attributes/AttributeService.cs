using System.Reflection;

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
