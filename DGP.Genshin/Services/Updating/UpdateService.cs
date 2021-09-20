using DGP.Snap.Framework.Attributes;
using DGP.Snap.Framework.Extensions.System;
using DGP.Snap.Framework.Net.Download;
using Octokit;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Updating
{
    internal class UpdateService
    {
        public Uri PackageUri { get; set; }
        public Version NewVersion { get; set; }
        public Release Release { get; set; }
        public UpdateInfo UpdateInfo { get; set; }
        public Version CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version;

        private IFileDownloader InnerFileDownloader { get; set; }

        private const string GithubUrl = @"https://api.github.com/repos/DGP-Studio/Snap.Genshin/releases/latest";

        [Github("https://github.com/settings/tokens")]
        public async Task<UpdateState> CheckUpdateStateAsync()
        {
            try
            {
                //use token to increase github rate limit
                GitHubClient client = new GitHubClient(new ProductHeaderValue("SnapGenshin"))
                {
                    Credentials = new Credentials("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")
                };
                this.Release = await client.Repository.Release.GetLatest("DGP-Studio", "Snap.Genshin");

                this.PackageUri = new Uri(this.Release.Assets[0].BrowserDownloadUrl);
                this.UpdateInfo = new UpdateInfo
                {
                    Title = this.Release.Name,
                    Detail = this.Release.Body
                };
                string newVersion = this.Release.TagName;
                this.NewVersion = new Version(this.Release.TagName);

                return new Version(newVersion) > this.CurrentVersion
                    ? UpdateState.NeedUpdate
                    : new Version(newVersion) == this.CurrentVersion
                           ? UpdateState.IsNewestRelease
                           : UpdateState.IsInsiderVersion;
            }
            catch (Exception)
            {
                return UpdateState.NotAvailable;
            }
        }

        public void DownloadAndInstallPackage()
        {
            this.InnerFileDownloader = new FileDownloader();
            this.InnerFileDownloader.DownloadProgressChanged += OnDownloadProgressChanged;
            this.InnerFileDownloader.DownloadFileCompleted += OnDownloadFileCompleted;

            string destinationPath = AppDomain.CurrentDomain.BaseDirectory + @"\Package.zip";
            this.InnerFileDownloader.DownloadFileAsync(this.PackageUri, destinationPath);
        }
        public void CancelUpdate() =>
            this.InnerFileDownloader.CancelDownloadAsync();

        internal void OnDownloadProgressChanged(object sender, DownloadFileProgressChangedArgs args)
        {
            double percent = Math.Round((double)args.BytesReceived / args.TotalBytesToReceive, 2);
            this.Log(percent);
            this.UpdateInfo.Progress = percent;
            this.UpdateInfo.ProgressText = $@"{percent * 100}% - {args.BytesReceived / 1024}KB / {args.TotalBytesToReceive / 1024}KB";
        }
        internal void OnDownloadFileCompleted(object sender, DownloadFileCompletedArgs eventArgs)
        {
            if (eventArgs.State == CompletedState.Succeeded)
            {
                StartInstallUpdate();
            }
        }
        public static void StartInstallUpdate()
        {
            //rename to oldupdater to avoid package extraction error
            if (File.Exists("OldUpdater.exe"))
            {
                File.Delete("OldUpdater.exe");
            }
            File.Move("DGP.Snap.Updater.exe", "OldUpdater.exe");
            Process.Start(new ProcessStartInfo()
            {
                FileName = "OldUpdater.exe",
                Arguments = "UpdateInstall"
            });
        }

        #region 单例
        private static UpdateService instance;
        private static readonly object _lock = new object();
        private UpdateService()
        {
            this.Log("initialized");
        }
        public static UpdateService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new UpdateService();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion
    }
}
