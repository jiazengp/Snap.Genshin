using DGP.Genshin.Core;
using DGP.Genshin.Core.LifeCycle;
using DGP.Genshin.Core.Plugins;
using System;
using System.Windows.Media;

[assembly: SnapGenshinPlugin]

namespace DGP.Genshin.Skin.Pink;

/// <summary>
/// 粉色皮肤插件
/// </summary>
public class PinkSkinPlugin : IPlugin, IAppStartUp
{
    /// <inheritdoc/>
    public bool IsEnabled { get => true; }

    /// <inheritdoc/>
    public string Name { get => "Pink Skin"; }

    /// <inheritdoc/>
    public string Description { get => "可以让你的 Snap Genshin 看起来粉粉的。"; }

    /// <inheritdoc/>
    public string Author { get => "希尔"; }

    /// <inheritdoc/>
    public Version Version { get => new(1, 0, 0, 0); }

    /// <inheritdoc/>
    public void Happen(IContainer container)
    {
        Color pinkColorLight2 = Color.FromArgb(255, 255, 199, 190);
        Color pinkColorLight1 = Color.FromArgb(255, 255, 194, 190);
        Color pinkColor = Color.FromArgb(255, 230, 172, 172);
        Color pinkColorDark1 = Color.FromArgb(255, 255, 199, 190);
        Color pinkColorDark2 = Color.FromArgb(255, 255, 194, 190);

        // SolidColorBrush pinkBrushLight2 = new(pinkColorLight2);
        // SolidColorBrush pinkBrushLight1 = new(pinkColorLight1);
        // SolidColorBrush pinkBrush = new(pinkColor);
        // SolidColorBrush pinkBrushDark1 = new(Color.FromArgb(255, 179, 133, 133));
        // SolidColorBrush pinkBrushDark2 = new(Color.FromArgb(255, 153, 114, 114));

        // WPFUI
        App.Current.Resources["TextFillColorPrimary"] = pinkColor;
        App.Current.Resources["TextFillColorSecondary"] = pinkColorLight1;
        App.Current.Resources["TextFillColorTertiary"] = pinkColorLight2;

        // ModernWpf
        App.Current.Resources["SystemBaseHighColor"] = pinkColor;
        App.Current.Resources["SystemBaseMediumHighColor"] = pinkColor;
        App.Current.Resources["SystemBaseMediumColor"] = pinkColor;
        App.Current.Resources["SystemBaseMediumLowColor"] = pinkColor;
        App.Current.Resources["SystemBaseLowColor"] = pinkColor;

        App.Current.Resources.MergedDictionaries.Add(new()
        {
            Source = new("pack://application:,,,/DGP.Genshin.Skin.Pink;component/Dark.xaml"),
        });

        // <Color x:Key="SystemBaseHighColor">#FFFFFFFF</Color>
        // <Color x:Key="SystemBaseMediumHighColor">#CCFFFFFF</Color>
        // <Color x:Key="SystemBaseMediumColor">#99FFFFFF</Color>
        // <Color x:Key="SystemBaseMediumLowColor">#66FFFFFF</Color>
        // <Color x:Key="SystemBaseLowColor">#33FFFFFF</Color>

        // <Color x:Key="SystemAltHighColor">#FF000000</Color>
        // <Color x:Key="SystemAltLowColor">#33000000</Color>
        // <Color x:Key="SystemAltMediumColor">#99000000</Color>
        // <Color x:Key="SystemAltMediumHighColor">#CC000000</Color>
        // <Color x:Key="SystemAltMediumLowColor">#66000000</Color>

        // <Color x:Key="SystemChromeAltLowColor">#FFF2F2F2</Color>
        // <Color x:Key="SystemChromeBlackHighColor">#FF000000</Color>
        // <Color x:Key="SystemChromeBlackLowColor">#33000000</Color>
        // <Color x:Key="SystemChromeBlackMediumLowColor">#66000000</Color>
        // <Color x:Key="SystemChromeBlackMediumColor">#CC000000</Color>
        // <Color x:Key="SystemChromeDisabledHighColor">#FF333333</Color>
        // <Color x:Key="SystemChromeDisabledLowColor">#FF858585</Color>
        // <Color x:Key="SystemChromeHighColor">#FF767676</Color>
        // <Color x:Key="SystemChromeLowColor">#FF171717</Color>
        // <Color x:Key="SystemChromeMediumColor">#FF1F1F1F</Color>
        // <Color x:Key="SystemChromeMediumHighColor">#FF323232</Color>
        // <Color x:Key="SystemChromeMediumLowColor">#FF2B2B2B</Color>
        // <Color x:Key="SystemChromeWhiteColor">#FFFFFFFF</Color>
        // <Color x:Key="SystemChromeGrayColor">#FF767676</Color>
        // <Color x:Key="SystemListLowColor">#19FFFFFF</Color>
        // <Color x:Key="SystemListMediumColor">#33FFFFFF</Color>
    }
}