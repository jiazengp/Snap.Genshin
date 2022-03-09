using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DGP.Genshin.Core.Background.Abstraction
{
    public interface IBackgroundProvider
    {
        Task<BitmapImage?> GetNextBitmapImageAsync();
    }
}
