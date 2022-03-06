using ModernWpf.Controls;
using System.Threading.Tasks;

namespace DGP.Genshin.Control.GenshinElement.GachaStatistic
{
    public sealed partial class GachaLogUrlDialog : ContentDialog
    {
        public GachaLogUrlDialog()
        {
            InitializeComponent();
        }
        public async Task<(bool isOk, string url)> GetInputUrlAsync()
        {
            bool isOk = await ShowAsync() == ContentDialogResult.Primary;
            return (isOk, isOk ? InputText.Text : string.Empty);
        }
    }
}
