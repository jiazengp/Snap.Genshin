using System.Windows.Forms;

namespace DGP.Genshin.DataViewer.Services
{
    internal static class WorkingFolderService
    {
        public static void SelectWorkingFolder()
        {
            FolderBrowserDialog folder = new FolderBrowserDialog
            {
                Description = "选择数据所在文件夹",
            };
            if (folder.ShowDialog() == DialogResult.OK)
            {
                WorkingFolderPath = folder.SelectedPath;
            }
        }

        public static string WorkingFolderPath { get; set; }
    }
}
