using DGP.Genshin.DataModel.MiHoYo2;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Abstratcions
{
    public interface IRecordService
    {
        List<string> QueryHistory { get; set; }
        event Action<string?>? RecordProgressed;

        void AddQueryHistory(string? uid);
        Task<Record> GetRecordAsync(string? uid);
        void UnInitialize();
    }
}