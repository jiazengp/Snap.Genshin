using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DGP.Genshin.Controls
{
    public partial class ExceptionWindow : Window, INotifyPropertyChanged
    {
        private Exception exceptionObject;

        public ExceptionWindow(Exception ex)
        {
            ExceptionObject = ex;
            DataContext = this;
            InitializeComponent();
        }

        public Exception ExceptionObject
        {
            get => exceptionObject;
            [MemberNotNull(nameof(exceptionObject))]
            set => Set(ref exceptionObject, value);
        }

        public string ExceptionType => ExceptionObject.GetType().ToString();

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void Set<T>([NotNullIfNotNull("value")] ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void CopyInfoAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ExceptionObject.ToString());
        }

        private void GithubAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/DGP-Studio/Snap.Genshin/issues/new/choose");
        }

        private void QQChatAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://qm.qq.com/cgi-bin/qm/qr?k=K1OglMXZGd-ulewzRDdFOYnSfMBOoNiT&amp;jump_from=webapi");
        }
    }
}
