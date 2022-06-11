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

        // WPFUI
        App.Current.Resources["TextFillColorPrimary"] = pinkColor;
        App.Current.Resources["TextFillColorSecondary"] = pinkColorLight1;
        App.Current.Resources["TextFillColorTertiary"] = pinkColorLight2;

        App.Current.Resources.MergedDictionaries.Add(new()
        {
            Source = new("pack://application:,,,/DGP.Genshin.Skin.Pink;component/Dark.xaml"),
        });
    }
}