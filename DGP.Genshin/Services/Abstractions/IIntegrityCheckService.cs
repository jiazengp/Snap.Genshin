using System;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Abstratcions
{
    /// <summary>
    /// 完整性检查服务
    /// </summary>
    public interface IIntegrityCheckService
    {
        /// <summary>
        /// 完整性检查是否完成
        /// 出于线程安全的考虑请勿在检查完整性的过程中途访问
        /// </summary>
        bool IntegrityCheckCompleted { get; }

        /// <summary>
        /// 检查基础缓存图片完整性，不完整的自动下载补全
        /// </summary>
        Task CheckMetadataIntegrityAsync(Action<IIntegrityCheckState> progressedCallback);

        /// <summary>
        /// 封装完整性检查进度报告
        /// </summary>
        public interface IIntegrityCheckState
        {
            /// <summary>
            /// 当前检查进度
            /// </summary>
            int CurrentCount { get; set; }

            /// <summary>
            /// 总进度
            /// </summary>
            int TotalCount { get; set; }

            /// <summary>
            /// 描述
            /// </summary>
            string? Info { get; set; }
        }
    }

    /// <summary>
    /// 指示此属性需要受到完整性检查
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IntegrityAwareAttribute : Attribute
    {
        /// <summary>
        /// 该属性是否为角色容器
        /// </summary>
        public bool IsCharacter { get; set; } = false;
    }
}
