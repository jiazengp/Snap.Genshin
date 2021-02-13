using System.IO;

namespace DGP.Genshin.DataViewer.Helper
{
    public class FileEx
    {
        public FileEx(string fullPath) => this.FullPath = fullPath;
        public string FullPath { get; set; }
        public string FullFileName => Path.GetFileNameWithoutExtension(this.FullPath);
        public string FileName => Path.GetFileNameWithoutExtension(this.FullPath)
            .Replace("Excel", "").Replace("Config", "").Replace("Data", "");
        public override string ToString() => this.FileName;

        public string ReadFile()
        {
            string json;
            using (StreamReader sr = new StreamReader(this.FullPath))
            {
                json = sr.ReadToEnd();
            }
            return json;
        }
    }
}
