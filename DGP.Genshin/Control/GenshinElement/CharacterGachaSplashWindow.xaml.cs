using DGP.Genshin.Helper;
using System.Windows;

namespace DGP.Genshin.Control.GenshinElement
{
    /// <summary>
    /// 角色卡池立绘展示窗口
    /// 不支持中途更改展示内容
    /// </summary>
    public sealed partial class CharacterGachaSplashWindow : Window
    {
        private static readonly DependencyProperty SourceProperty = Property<CharacterGachaSplashWindow>.Depend<string>(nameof(Source));

        /// <summary>
        /// 构造一个新的角色卡池立绘窗口
        /// </summary>
        public CharacterGachaSplashWindow()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        /// <summary>
        /// 待展示的图片地址
        /// </summary>
        public string? Source
        {
            get => (string)this.GetValue(SourceProperty);

            set => this.SetValue(SourceProperty, value);
        }
    }
}