using DGP.Genshin.Control.Infrastructure.Observable;
using Snap.Win32;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace DGP.Genshin.Control
{
    /// <summary>
    /// 异常提示窗口
    /// </summary>
    public sealed partial class ExceptionWindow : ObservableWindow
    {
        private Exception exceptionObject;

        /// <summary>
        /// 构造一个新的异常窗体
        /// </summary>
        /// <param name="ex">要展示的异常</param>
        public ExceptionWindow(Exception ex)
        {
            this.ExceptionObject = ex;
            this.InitializeComponent();
        }

        /// <summary>
        /// 异常对象
        /// </summary>
        public Exception ExceptionObject
        {
            get => this.exceptionObject;

            [MemberNotNull(nameof(exceptionObject))]
            set => this.Set(ref this.exceptionObject, value);
        }

        /// <summary>
        /// 异常类型字符串
        /// </summary>
        public string ExceptionType
        {
            get => this.ExceptionObject.GetType().ToString();
        }

        private void CopyInfoAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            string stackTrace = this.ExceptionObject.ToString();

            // clear before copy
            Clipboard.Clear();
            try
            {
                Clipboard.SetText(stackTrace);
            }
            catch
            {
                try
                {
                    Clipboard2.SetText(stackTrace);
                }
                catch
                {
                }
            }
        }
    }
}