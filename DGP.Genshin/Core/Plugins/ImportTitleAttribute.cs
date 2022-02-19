using System;

namespace DGP.Genshin.Core.Plugins
{
    /// <summary>
    /// 指示插件需要申请的标题栏按钮
    /// 必须将此特性添加在
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ImportTitleAttribute : Attribute
    {
        public ImportTitleAttribute(Type buttonType, uint order)
        {
            ButtonType = buttonType;
            Order = order;
        }

        /// <summary>
        /// 待添加的标题栏按钮的类型
        /// </summary>
        public Type ButtonType { get; set; }

        /// <summary>
        /// 获取或设置标题栏按钮的优先级
        /// 越小的数值会被放置在越靠近右侧的位置
        /// </summary>
        public uint Order { get; set; }
    }
}