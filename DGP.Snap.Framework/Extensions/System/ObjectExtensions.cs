using DGP.Snap.Framework.Core.Logging;
using System;
using System.Diagnostics;

namespace DGP.Snap.Framework.Extensions.System
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="debugString"></param>
        public static void DebugWriteLine(this object obj, string debugString = null)
        {
            Debug.WriteLine(obj);
            if (debugString != null)
            {
                Debug.WriteLine(debugString);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="info"></param>
        /// <param name="formatter"></param>
        public static void Log(this object obj, object info, Func<object, string> formatter = null) => Logger.Instance.Log(obj, info, formatter);
    }
}
