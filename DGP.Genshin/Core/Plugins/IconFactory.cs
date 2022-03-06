using ModernWpf.Controls;

namespace DGP.Genshin.Core.Plugins
{
    /// <summary>
    /// 图表工厂类
    /// </summary>
    public abstract class IconFactory
    {
        public abstract IconElement? GetIcon();
    }
}