using DGP.Genshin.Control.Infrastructure.Observable;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace DGP.Genshin.Control
{
    public sealed partial class ExceptionWindow : ObservableWindow
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

        public string ExceptionType
        {
            get => ExceptionObject.GetType().ToString();
        }

        private void CopyInfoAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ExceptionObject.ToString());
        }
    }
}
