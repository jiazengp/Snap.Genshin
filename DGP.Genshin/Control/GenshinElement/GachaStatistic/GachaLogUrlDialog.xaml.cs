using ModernWpf.Controls;
using Snap.Core.Logging;
using System.Threading.Tasks;

namespace DGP.Genshin.Control.GenshinElement.GachaStatistic
{
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
