using DGP.Snap.Framework.Extensions.System;
using ModernWpf.Controls;
using System;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.GachaStatistic
{
    /// <summary>
    /// GachaLogUrlDialog.xaml 的交互逻辑
    /// </summary>
    public partial class GachaLogUrlDialog : ContentDialog
    {
        public GachaLogUrlDialog()
        {
            this.InitializeComponent();
            this.Log("initialized");
        }
        public async Task<string> GetInputUrlAsync() =>
            await this.ShowAsync() == ContentDialogResult.Primary ? this.InputText.Text : String.Empty;
    }
}
