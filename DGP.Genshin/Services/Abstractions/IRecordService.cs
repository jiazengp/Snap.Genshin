using DGP.Genshin.DataModels.MiHoYo2;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Abstratcions
{
    public interface IRecordService
    {
        Task<Record> GetRecordAsync(string? uid);
    }
}