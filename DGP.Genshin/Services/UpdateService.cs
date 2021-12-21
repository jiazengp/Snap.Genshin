using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Common.Net.Download;
using DGP.Genshin.Helpers;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Uwp.Notifications;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace DGP.Genshin.Services
{
    [Service(typeof(IUpdateService), ServiceType.Singleton)]
    internal class UpdateService : IUpdateService
    {
        private const string UpdateNotificationTag = "snap_genshin_update";
        private NotificationUpdateResult LastNotificationUpdateResult = NotificationUpdateResult.Succeeded;

        public Uri? PackageUri { get; set; }
        public Version? NewVersion { get; set; }
        public Release? Release { get; set; }
        public Version? CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version;

        private Downloader? InnerDownloader { get; set; }

        public async Task<UpdateState> CheckUpdateStateAsync()
        {
            try
            {
                GitHubClient client = new(new ProductHeaderValue("SnapGenshin"))
                {
                    Credentials = new Credentials(TokenHelper.GetToken()),
                };
                Release = await client.Repository.Release.GetLatest("DGP-Studio", "Snap.Genshin");
                PackageUri = new Uri(Release.Assets[0].BrowserDownloadUrl);
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

        public async Task DownloadAndInstallPackageAsync()
        {
            string destinationPath = AppDomain.CurrentDomain.BaseDirectory + @"\Package.zip";

            if (PackageUri is null)
            {
                //unlikely to happen,unless a new release with no package is published
                throw new InvalidOperationException("未找到更新包的下载地址");
            }

            InnerDownloader = new(PackageUri, destinationPath);
            InnerDownloader.ProgressChanged += OnProgressChanged;
            App.Current.Dispatcher.Invoke(ShowDownloadToastNotification);
            bool caught = false;
            try
            {
                await InnerDownloader.DownloadAsync();
            }
            catch
            {
                caught = true;
            }
            if (!caught)
            {
                StartInstallUpdate();
            }
        }

        /// <summary>
        /// 显示下载进度通知
        /// </summary>
        private void ShowDownloadToastNotification()
        {
            LastNotificationUpdateResult = NotificationUpdateResult.Succeeded;

            new ToastContentBuilder()
                .AddText("下载更新中...")
                .AddVisualChild(new AdaptiveProgressBar()
                {
                    Title = Release?.Name,
                    Value = new BindableProgressBarValue("progressValue"),
                    ValueStringOverride = new BindableString("progressValueString"),
                    Status = new BindableString("progressStatus")
                })
                .Show(toast =>
                {
                    toast.Tag = UpdateNotificationTag;
                    toast.Data = new(new Dictionary<string, string>()
                    {
                        {"progressValue", "0" },
                        {"progressValueString", "0% - 0MB / 0MB" },
                        {"progressStatus", "下载中..." }
                    })
                    {
                        //always update when it's 0
                        SequenceNumber = 0
                    };
                });
        }

        /// <summary>
        /// 进度更新
        /// </summary>
        /// <param name="totalBytesToReceive">总大小</param>
        /// <param name="bytesReceived">下载的大小</param>
        /// <param name="percent">进度</param>
        private void OnProgressChanged(long? totalBytesToReceive, long bytesReceived, double? percent)
        {
            this.Log(percent);
            this.Log(LastNotificationUpdateResult);
            //user has dismissed the notification so we don't update it anymore
            if (LastNotificationUpdateResult is not NotificationUpdateResult.Succeeded)
            {
                return;
            }
            if (percent is not null)
            {
                //notification could only be updated by same thread.
                App.Current.Dispatcher.Invoke(() => UpdateNotificationValue(totalBytesToReceive, bytesReceived, percent));
            }
        }

        /// <summary>
        /// 更新下载进度通知上的进度条
        /// </summary>
        /// <param name="totalBytesToReceive">总大小</param>
        /// <param name="bytesReceived">下载的大小</param>
        /// <param name="percent">进度</param>
        private void UpdateNotificationValue(long? totalBytesToReceive, long bytesReceived, double? percent)
        {
            NotificationData data = new() { SequenceNumber = 0 };

            data.Values["progressValue"] = $"{(percent is null ? 0 : percent.Value)}";
            data.Values["progressValueString"] = $@"{percent:p2}% - {bytesReceived * 1.0 / 1024 / 1024}MB / {totalBytesToReceive * 1.0 / 1024 / 1024}MB";
            if (percent >= 1)
            {
                data.Values["progressStatus"] = "下载完成";
            }

            // Update the existing notification's data
            LastNotificationUpdateResult = ToastNotificationManagerCompat.CreateToastNotifier().Update(data, UpdateNotificationTag);
            this.Log("UpdateNotificationValue called");
        }

        /// <summary>
        /// 开始安装更新
        /// </summary>
        private void StartInstallUpdate()
        {
            Directory.CreateDirectory("Updater");
            File.Move("DGP.Genshin.Updater.exe", @"Updater/DGP.Genshin.Updater.exe", true);

            //Updater自带工作路径纠正
            Process.Start(new ProcessStartInfo()
            {
                FileName = @"Updater/DGP.Genshin.Updater.exe",
                Arguments = "UpdateInstall"
            });
        }
    }
}
