using DGP.Genshin.Cookie;
using DGP.Genshin.MiHoYoAPI.Record.DailyNote;
using DGP.Genshin.MiHoYoAPI.User;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DGP.Genshin.Controls.TitleBarButtons
{
    /// <summary>
    /// DailyNoteTitleBarButton.xaml 的交互逻辑
    /// </summary>
    public partial class DailyNoteTitleBarButton : TitleBarButton
    {
        public DailyNoteTitleBarButton()
        {
            InitializeComponent();
        }

        public List<DailyNote> DailyNotes
        {
            get { return (List<DailyNote>)GetValue(DailyNotesProperty); }
            set { SetValue(DailyNotesProperty, value); }
        }
        public static readonly DependencyProperty DailyNotesProperty =
            DependencyProperty.Register("DailyNotes", typeof(List<DailyNote>), typeof(DailyNoteTitleBarButton), new PropertyMetadata(null));

        private async void DailyNoteTitleBarButtonClick(object sender, RoutedEventArgs e)
        {
            if (FlyoutBase.GetAttachedFlyout((TitleBarButton)sender) is Flyout flyout)
            {
                if (flyout.Content is Grid grid)
                {
                    grid.DataContext = this;
                    FlyoutBase.ShowAttachedFlyout((TitleBarButton)sender);
                }
            }
            await RefreshAsync();
        }

        private bool isRefreshing;
        public async Task RefreshAsync()
        {
            if (isRefreshing)
            {
                return;
            }
            isRefreshing = true;
            List<DailyNote> list = new();
            UserGameRoleInfo? roles = await Task.Run(new UserGameRoleProvider(CookieManager.Cookie).GetUserGameRoles);

            if (roles?.List is not null)
            {
                foreach (UserGameRole role in roles.List)
                {
                    if (role.Region is not null && role.GameUid is not null)
                    {
                        DailyNote? note = new DailyNoteProvider(CookieManager.Cookie).GetDailyNote(role.Region, role.GameUid);
                        if (note is not null)
                        {
                            list.Add(note);
                        }
                    }
                }
            }
            DailyNotes = list;
            isRefreshing = false;
        }
    }
}
