using DGP.Genshin.Helper;
using DGP.Genshin.Helper.Converter;
using DGP.Genshin.Message;
using DGP.Genshin.Service.Abstratcion;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using Octokit;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Exception;
using Snap.Net.Download;
using Snap.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace DGP.Genshin.Service
{
    [Service(typeof(IUpdateService), InjectAs.Singleton)]
    internal class UpdateService : IUpdateService
    {
        private const string UpdateNotificationTag = "snap_genshin_update";
        private NotificationUpdateResult lastNotificationUpdateResult = NotificationUpdateResult.Succeeded;
        private readonly ISettingService settingService;

        public Uri? PackageUri { get; set; }
        public Version? NewVersion { get; set; }
        public Release? Release { get; set; }
        public Version? CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version;

        private Downloader? InnerDownloader { get; set; }

        public UpdateService(ISettingService settingService)
        {
            this.settingService = settingService;
        }

        public async Task<UpdateState> CheckUpdateStateAsync()
        {
            try
            {
                GitHubClient client = new(new ProductHeaderValue("SnapGenshin"))
                {
                    Credentials = new Credentials(GithubToken.GetToken()),
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

        private readonly TaskPreventer updateTaskPreventer = new();

        public async Task DownloadAndInstallPackageAsync()
        {
            if (updateTaskPreventer.ShouldExecute)
            {
                string destinationPath = PathContext.Locate("Package.zip");

                //unlikely to happen,unless a new release with no package is published
                _ = PackageUri ?? throw new SnapGenshinInternalException("未找到更新包的下载地址");

                if (settingService.GetOrDefault(Setting.UpdateUseFastGit, false))
                {
                    //replace host with fastgit
                    PackageUri = new UriBuilder(PackageUri) { Host = "download.fastgit.org" }.Uri;
                }

                InnerDownloader = new(PackageUri, destinationPath);
                InnerDownloader.ProgressChanged += OnProgressChanged;
                //toast can only be shown & updated by main thread
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
                finally
                {
                    App.Messenger.Send(UpdateProgressedMessage.Default);
                }
                if (!caught)
                {
                    StartInstallUpdate();
                }
                updateTaskPreventer.Release();
            }
        }

        /// <summary>
        /// 显示下载进度通知
        /// </summary>
        private void ShowDownloadToastNotification()
        {
            lastNotificationUpdateResult = NotificationUpdateResult.Succeeded;

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
                    toast.Data =
                    new(new Dictionary<string, string>()
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
            //message will be sent anyway.
            string valueString = $@"{percent:P2} - {bytesReceived * 1.0 / 1024 / 1024:F2}MB / {totalBytesToReceive * 1.0 / 1024 / 1024:F2}MB";
            App.Messenger.Send(new UpdateProgressedMessage(percent ?? 0, valueString, percent <= 1));
            //if user has dismissed the notification, we don't update it anymore
            if (lastNotificationUpdateResult is NotificationUpdateResult.Succeeded)
            {
                if (percent is not null)
                {
                    //notification could only be updated by main thread.
                    App.Current.Dispatcher.Invoke(() => UpdateNotificationValue(totalBytesToReceive, bytesReceived, percent));
                }
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

            data.Values["progressValue"] = $"{percent ?? 0}";
            data.Values["progressValueString"] =
                $@"{percent:P2} - {bytesReceived * 1.0 / 1024 / 1024:F2}MB / {totalBytesToReceive * 1.0 / 1024 / 1024:F2}MB";
            if (percent >= 1)
            {
                data.Values["progressStatus"] = "下载完成";
            }

            // Update the existing notification's data
            lastNotificationUpdateResult = ToastNotificationManagerCompat.CreateToastNotifier().Update(data, UpdateNotificationTag);
            this.Log("UpdateNotificationValue called");
        }

        /// <summary>
        /// 开始安装更新
        /// </summary>
        [Conditional("RELEASE")]
        private void StartInstallUpdate()
        {
            Directory.CreateDirectory("Updater");
            PathContext.MoveToFolderOrIgnore("DGP.Genshin.Updater.exe", "Updater");
            //Updater自带工作路径纠正
            Process.Start(new ProcessStartInfo()
            {
                //fix auth exception
                Verb = "runas",
                UseShellExecute = true,
                FileName = @"Updater/DGP.Genshin.Updater.exe",
                Arguments = "UpdateInstall"
            });

            App.Current.Dispatcher.Invoke(() => App.Current.Shutdown());
        }
    }
}
