using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    /// <summary>
    /// 角色评分页面
    /// </summary>
    [View(InjectAs.Transient)]
    public partial class AvatarRankPage : System.Windows.Controls.Page
    {
        public AvatarRankPage()
        {
            InitializeComponent();
        }
    }
}