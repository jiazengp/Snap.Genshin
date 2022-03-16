using ModernWpf.Controls;
using Snap.Data.Primitive;
using System.Threading.Tasks;

namespace DGP.Genshin.Control.GenshinElement.GachaStatistic
{
    public sealed partial class GachaLogUrlDialog : ContentDialog
    {
        public GachaLogUrlDialog()
        {
            InitializeComponent();
        }
        public async Task<Result<bool,string>> GetInputUrlAsync()
        {
            bool isOk = await ShowAsync() != ContentDialogResult.Secondary;
            return new(isOk, isOk ? InputText.Text : string.Empty);
        }
    }
}
