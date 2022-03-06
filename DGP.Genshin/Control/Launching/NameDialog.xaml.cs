using DGP.Genshin.Control.Infrastructure.Observable;
using DGP.Genshin.DataModel.Launching;
using System.Threading.Tasks;

namespace DGP.Genshin.Control.Launching
{
    public sealed partial class NameDialog : ObservableContentDialog
    {
        private string? input;
        private GenshinAccount? targetAccount;

        public string? Input
        {
            get => input;

            set => Set(ref input, value);
        }
        public GenshinAccount? TargetAccount
        {
            get => targetAccount;

            set => Set(ref targetAccount, value);
        }

        public NameDialog()
        {
            DataContext = this;
            InitializeComponent();
        }

        public async Task<string?> GetInputAsync()
        {
            await ShowAsync();
            return Input;
        }
    }
}
