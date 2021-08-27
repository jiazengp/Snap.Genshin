using DGP.Snap.Framework.Extensions.System;
using ModernWpf.Controls;
using System.Windows;

namespace DGP.Genshin.Services.Updating
{
    /// <summary>
    /// UpdateDialog.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateDialog : ContentDialog
    {
        public UpdateDialog()
        {
            UpdateService.Instance.UpdateInfo = this.UpdateInfo;
            InitializeComponent();
            this.Log("initialized");
        }

        private void UpdateCancellationRequested(ContentDialog sender, ContentDialogButtonClickEventArgs args) => UpdateService.Instance.CancelUpdate();

        public UpdateInfo UpdateInfo
        {
            get => (UpdateInfo)GetValue(UpdateInfoProperty);
            set => SetValue(UpdateInfoProperty, value);
        }
        public static readonly DependencyProperty UpdateInfoProperty =
            DependencyProperty.Register("UpdateInfo", typeof(UpdateInfo), typeof(UpdateDialog), new PropertyMetadata(new UpdateInfo()));

    }
}
