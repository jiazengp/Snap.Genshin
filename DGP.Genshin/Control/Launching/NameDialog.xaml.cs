using DGP.Genshin.Control.Infrastructure.Observable;
using DGP.Genshin.DataModel.Launching;
using System.Threading.Tasks;

namespace DGP.Genshin.Control.Launching
{
    /// <summary>
    /// 启动游戏账号命名对话框
    /// </summary>
    public sealed partial class NameDialog : ObservableContentDialog
    {
        private string? input;
        private GenshinAccount? targetAccount;

        /// <summary>
        /// 构造一个新的命名对话框
        /// </summary>
        public NameDialog()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        /// <summary>
        /// 输入
        /// </summary>
        public string? Input
        {
            get => this.input;

            set => this.Set(ref this.input, value);
        }

        /// <summary>
        /// 目标账号
        /// </summary>
        public GenshinAccount? TargetAccount
        {
            get => this.targetAccount;

            set => this.Set(ref this.targetAccount, value);
        }

        /// <summary>
        /// 获取用户输入
        /// </summary>
        /// <returns>用户输入的字符串或 <see langword="null"/></returns>
        public async Task<string?> GetInputAsync()
        {
            await this.ShowAsync();
            return this.Input;
        }
    }
}