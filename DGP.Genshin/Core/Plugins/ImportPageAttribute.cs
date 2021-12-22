using ModernWpf.Controls;
using System;

namespace DGP.Genshin.Core.Plugins
{
    /// <summary>
    /// 指示插件需要申请额外的可导航页面, 一个插件可以申请多个页面
    /// <para/>
    /// 如果第二个页面与第一个页面使用了相同的页面类型, 则后加入的页面无法被导航
    /// <para/>
    /// 必须将此特性添加在实现了 <see cref="IPlugin"/> 的主类上
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =true,Inherited =false)]
    public class ImportPageAttribute : Attribute
    {
        public Type PageType { get; set; }
        public string Label { get; set; }
        public IconElement Icon { get; set; }

        /// <summary>
        /// 指示插件需要申请额外的可导航页面,
        /// 一个插件可以申请多个页面
        /// </summary>
        /// <param name="pageType">导航页面的类型</param>
        /// <param name="label">导航页面的标签字符串</param>
        /// <param name="icon">图标</param>
        public ImportPageAttribute(Type pageType,string label, IconElement icon)
        {
            PageType = pageType;
            Label = label;
            Icon = icon;
        }

        /// <summary>
        /// 指示插件需要申请额外的可导航页面,
        /// 一个插件可以申请多个页面
        /// </summary>
        /// <param name="pageType">导航页面的类型</param>
        /// <param name="label">导航页面的标签字符串</param>
        /// <param name="glyph">图标字符串 详见 <seealso cref="https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-fluent-icons-font"/></param>
        public ImportPageAttribute(Type pageType, string label, string glyph)
        {
            PageType = pageType;
            Label = label;
            Icon = new FontIcon { Glyph = glyph };
        }
    }
}