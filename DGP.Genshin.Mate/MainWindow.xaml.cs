using DGP.Genshin.Common.NativeMethods;
using DGP.Genshin.Mate.Services;
using System;
using System.Windows;
using System.Windows.Interop;

namespace DGP.Genshin.Mate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = ResinService.Instance;
            InitializeComponent();
        }

        public void SetOnDesktop()
        {
            IntPtr hWnd = new WindowInteropHelper(this).Handle;
            IntPtr hWndWorkerW = User32.FindWindow("WorkerW", "");
            User32.SetParent(hWnd, hWndWorkerW);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetOnDesktop();
            await ResinService.Instance.RefreshAsync();
        }

        private void DragGrid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private async void DragGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            await ResinService.Instance.RefreshAsync();
        }
    }
}
