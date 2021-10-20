using System;
using System.IO;

namespace DGP.Snap.Framework.Net.Download
{
    internal static class FileUtils
    {
        //private static readonly ILogger Logger = LoggerFacade.GetCurrentClassLogger();

        public static bool TryGetFileSize(string filename, out long filesize)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filename);
                filesize = fileInfo.Length;
            }
            catch (Exception)
            {
                filesize = 0;
                return false;
            }
            return true;
        }

        public static bool TryFileDelete(string? filename)
        {
            if (filename is not null)
            {
                try
                {

                    File.Delete(filename);
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public static bool ReplaceFile(string source, string destination)
        {
            if (!destination.Equals(source, StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    File.Delete(destination);
                    File.Move(source, destination);
                }
                catch (Exception)
                {
                    //Logger.Warn("Unable replace local file {0} with cached resource {1}, {2}", destination, source, e.Message);
                    return false;
                }
            }
            return true;
        }
    }
}