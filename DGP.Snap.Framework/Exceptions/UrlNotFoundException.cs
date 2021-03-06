using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Snap.Framework.Exceptions
{
    public class UrlNotFoundException:Exception
    {
        public UrlNotFoundException(string message) : base(message) { }

    }
}
