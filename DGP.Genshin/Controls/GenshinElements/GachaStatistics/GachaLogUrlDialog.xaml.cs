using DGP.Genshin.Common.Extensions.System;
using ModernWpf.Controls;
using System.Threading.Tasks;

namespace DGP.Genshin.Controls.GenshinElements.GachaStatistics
{
    /// <summary>
    /// GachaLogUrlDialog.xaml 的交互逻辑
    /// </summary>
    public partial class GachaLogUrlDialog : ContentDialog
    {
        public GachaLogUrlDialog()
        {
            InitializeComponent();
            this.Log("initialized");
        }
        public async Task<(bool isOk, string url)> GetInputUrlAsync()
        {
            bool isOk = await ShowAsync() == ContentDialogResult.Primary;
            return (isOk, isOk ? InputText.Text : string.Empty);
        }
    }
}
