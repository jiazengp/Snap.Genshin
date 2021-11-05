using DGP.Genshin.Cookie;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Record.DailyNote;
using ModernWpf.Controls.Primitives;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
            get => (List<DailyNote>)GetValue(DailyNotesProperty);
            set => SetValue(DailyNotesProperty, value);
        }
        public static readonly DependencyProperty DailyNotesProperty =
            DependencyProperty.Register("DailyNotes", typeof(List<DailyNote>), typeof(DailyNoteTitleBarButton), new PropertyMetadata(null));

        private async void DailyNoteTitleBarButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender.ShowAttachedFlyout<Grid>(this))
            {
                await RefreshAsync();
            }
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
            UserGameRoleInfo? roles = await new UserGameRoleProvider(CookieManager.CurrentCookie).GetUserGameRolesAsync();

            if (roles?.List is not null)
            {
                foreach (UserGameRole role in roles.List)
                {
                    if (role.Region is not null && role.GameUid is not null)
                    {
                        DailyNote? note = await new DailyNoteProvider(CookieManager.CurrentCookie).GetDailyNoteAsync(role.Region, role.GameUid);
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
