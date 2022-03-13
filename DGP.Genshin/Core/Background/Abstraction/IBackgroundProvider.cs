using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DGP.Genshin.Core.Background.Abstraction
{
    /// <summary>
    /// 背景提供器接口
    /// 实现此接口的对象必须包含公共无参构造器
    /// </summary>
    public interface IBackgroundProvider
    {
        Task<BitmapImage?> GetNextBitmapImageAsync();
    }
}
