using DGP.Genshin.Helper;
using DGP.Genshin.Models.Github;
using DGP.Snap.Framework.Net.Download;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace DGP.Genshin.Service.Update
{
    internal class UpdateService
    {
        public Uri PackageUri { get; set; }
        public Version NewVersion { get; set; }
        public Release ReleaseInfo { get; set; }
        public UpdateInfo UpdateInfo { get; set; }
        public Version CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version;

        private IFileDownloader InnerFileDownloader { get; set; }

        private const string GithubUrl = @"https://api.github.com/repos/DGP-Studio/Snap.Genshin/releases/latest";
        private const string GiteeUrl = @"https://gitee.com/api/v5/repos/Lightczx/Snap.Genshin/releases/latest";
        public UpdateState CheckUpdateState(string url)
        {
            try
            {
                this.ReleaseInfo = Json.GetWebRequestObject<Release>(url);
                this.UpdateInfo.Title = this.ReleaseInfo.Name;
                this.UpdateInfo.Detail = this.ReleaseInfo.Body;
                string newVersion = this.ReleaseInfo.TagName;
                this.NewVersion = new Version(this.ReleaseInfo.TagName);

                if (new Version(newVersion) > this.CurrentVersion)//有新版本
                {
                    this.PackageUri = new Uri(this.ReleaseInfo.Assets[0].BrowserDownloadUrl);
                    return UpdateState.NeedUpdate;
                }
                else
                {
                    if (new Version(newVersion) == this.CurrentVersion)
                    {
                        return UpdateState.IsNewestRelease;
                    }
                    else
                    {
                        return UpdateState.IsInsiderVersion;
                    }
                }
            }
            catch (Exception)
            {
                return UpdateState.NotAvailable;
            }
        }
        public UpdateState CheckUpdateStateViaGitee() => this.CheckUpdateState(GiteeUrl);
        public UpdateState CheckUpdateStateViaGithub() => this.CheckUpdateState(GithubUrl);

        public void DownloadAndInstallPackage()
        {
            this.InnerFileDownloader = new FileDownloader();
            this.InnerFileDownloader.DownloadProgressChanged += this.OnDownloadProgressChanged;
            this.InnerFileDownloader.DownloadFileCompleted += this.OnDownloadFileCompleted;

            string destinationPath = AppDomain.CurrentDomain.BaseDirectory + @"\Package.zip";
            this.InnerFileDownloader.DownloadFileAsync(this.PackageUri, destinationPath);
        }
        public void CancelUpdate() => this.InnerFileDownloader.CancelDownloadAsync();

        internal void OnDownloadProgressChanged(object sender, DownloadFileProgressChangedArgs args)
        {
            double percent = Math.Round((double)args.BytesReceived / args.TotalBytesToReceive, 2);
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
            if (File.Exists("OldUpdater.exe"))
            {
                File.Delete("OldUpdater.exe");
            }
            //rename to oldupdater to avoid package extraction error
            File.Move("DGP.Snap.Updater.exe", "OldUpdater.exe");

            Process.Start(new ProcessStartInfo()
            {
                FileName = "OldUpdater.exe",
                Arguments = "UpdateInstall",
                CreateNoWindow = true
            });
            App.Current.Shutdown();
        }
        #region 单例
        private static UpdateService instance;
        private static readonly object _lock = new object();
        private UpdateService()
        {

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
