using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Common.Net.Download;
using DGP.Genshin.Helpers;
using Octokit;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Updating
{
    internal class UpdateService
    {
        public Uri? PackageUri { get; set; }
        public Version? NewVersion { get; set; }
        public Release? Release { get; set; }
        public UpdateInfo? UpdateInfo { get; set; }
        public Version? CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version;

        private IFileDownloader? InnerFileDownloader { get; set; }

        public async Task<UpdateState> CheckUpdateStateAsync()
        {
            try
            {
                //use token to increase github rate limit
                GitHubClient client = new(new ProductHeaderValue("SnapGenshin"))
                {
                    Credentials = new Credentials(TokenHelper.GetToken()),
                };
                Release = await client.Repository.Release.GetLatest("DGP-Studio", "Snap.Genshin");

                PackageUri = new Uri(Release.Assets[0].BrowserDownloadUrl);
                UpdateInfo = new UpdateInfo
                {
                    Title = Release.Name,
                    Detail = Release.Body
                };
                string newVersion = Release.TagName;
                NewVersion = new Version(Release.TagName);

                return NewVersion > CurrentVersion
                    ? UpdateState.NeedUpdate
                    : NewVersion == CurrentVersion
                           ? UpdateState.IsNewestRelease
                           : UpdateState.IsInsiderVersion;
            }
            catch
            {
                return UpdateState.NotAvailable;
            }
        }

        public void DownloadAndInstallPackage()
        {
            InnerFileDownloader = new FileDownloader();
            InnerFileDownloader.DownloadProgressChanged += OnDownloadProgressChanged;
            InnerFileDownloader.DownloadFileCompleted += OnDownloadFileCompleted;

            string destinationPath = AppDomain.CurrentDomain.BaseDirectory + @"\Package.zip";
            if (PackageUri is null)
            {
                throw new InvalidOperationException("未找到更新包的下载地址");
            }
            InnerFileDownloader.DownloadFileAsync(PackageUri, destinationPath);
        }
        public void CancelUpdate()
        {
            InnerFileDownloader?.CancelDownloadAsync();
            InnerFileDownloader?.Dispose();
        }

        internal void OnDownloadProgressChanged(object? sender, DownloadFileProgressChangedArgs args)
        {
            double percent = Math.Round((double)args.BytesReceived / args.TotalBytesToReceive, 2);
            this.Log(percent);
            if (UpdateInfo is not null)
            {
                UpdateInfo.Progress = percent;
                UpdateInfo.ProgressText = $@"{percent * 100}% - {args.BytesReceived / 1024}KB / {args.TotalBytesToReceive / 1024}KB";
            }
        }
        internal void OnDownloadFileCompleted(object? sender, DownloadFileCompletedArgs eventArgs)
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
            //those files are needed to start process successufully
            File.Move("DGP.Genshin.Updater.dll", "OldUpdater.dll");
            File.Move("DGP.Genshin.Updater.exe", "OldUpdater.exe");
            File.Move("DGP.Genshin.Updater.deps.json", "OldUpdater.deps.json");
            File.Move("DGP.Genshin.Updater.runtimeconfig.json", "OldUpdater.runtimeconfig.json");

            Process.Start(new ProcessStartInfo()
            {
                FileName = "OldUpdater.exe",
                Arguments = "UpdateInstall"
            });
        }

        #region 单例
        private static volatile UpdateService? instance;
        [SuppressMessage("", "IDE0044")]
        private static object _locker = new();
        private UpdateService() { }
        public static UpdateService Instance
        {
            get
            {
                if (instance is null)
                {
                    lock (_locker)
                    {
                        instance ??= new();
                    }
                }
                return instance;
            }
        }
        #endregion
    }
}
