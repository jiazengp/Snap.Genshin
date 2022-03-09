using Snap.Data.Primitive;
using System;
using System.Threading.Tasks;

namespace DGP.Genshin.Service.Abstraction
{
    /// <summary>
    /// 完整性检查服务
    /// </summary>
    public interface IIntegrityCheckService
    {
        /// <summary>
        /// 完整性检查监视器
        /// </summary>
        WorkWatcher IntegrityChecking { get; }

        /// <summary>
        /// 检查基础缓存图片完整性，不完整的自动下载补全
        /// </summary>
        Task CheckMetadataIntegrityAsync(IProgress<IIntegrityCheckState> progress);

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
    }
}
