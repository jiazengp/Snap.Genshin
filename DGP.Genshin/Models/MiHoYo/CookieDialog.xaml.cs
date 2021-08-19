using ModernWpf.Controls;
using System.Threading.Tasks;

namespace DGP.Genshin.Models.MiHoYo
{
    /// <summary>
    /// CookieDialog.xaml 的交互逻辑
    /// </summary>
    public partial class CookieDialog : ContentDialog
    {
        public CookieDialog()
        {
            InitializeComponent();
        }

        public async Task<string> GetInputCookieAsync()
        {
            if (await ShowAsync() == ContentDialogResult.Primary)
            {
                return this.InputText.Text;
            }
            else
            {
                return System.String.Empty;
            }
        }
    }
}
