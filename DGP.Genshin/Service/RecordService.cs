using DGP.Genshin.DataModel.Reccording;
using DGP.Genshin.Message;
using DGP.Genshin.MiHoYoAPI.Record;
using DGP.Genshin.MiHoYoAPI.Record.Avatar;
using DGP.Genshin.MiHoYoAPI.Record.SpiralAbyss;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.Messaging;
using Snap.Core.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace DGP.Genshin.Service
{
    /// <summary>
    /// 玩家记录服务的默认实现
    /// </summary>
    [Service(typeof(IRecordService), InjectAs.Transient)]
    internal class RecordService : IRecordService
    {
        private readonly ICookieService cookieService;
        private readonly IMessenger messenger;

        public RecordService(ICookieService cookieService, IMessenger messenger)
        {
            this.messenger = messenger;
            this.cookieService = cookieService;
        }

        public async Task<Record> GetRecordAsync(string? uid)
        {
            Record? result = await Task.Run(async () =>
            {
                if (uid is null)
                {
                    return new Record("请输入Uid");
                }

                RecordProvider recordProvider = new(this.cookieService.CurrentCookie);

                //figure out the server
                string? server = recordProvider.EvaluateUidRegion(uid);
                if (server is null)
                {
                    this.messenger.Send(new RecordProgressChangedMessage(null));
                    return new Record("不支持查询此UID");
                }

                this.messenger.Send(new RecordProgressChangedMessage("正在获取 玩家基础统计信息 (1/4)"));
                PlayerInfo? playerInfo = await recordProvider.GetPlayerInfoAsync(uid, server);
                if (playerInfo is null)
                {
                    this.messenger.Send(new RecordProgressChangedMessage(null));
                    return new Record($"获取玩家基本信息失败");
                }

                this.messenger.Send(new RecordProgressChangedMessage("正在获取 本期深境螺旋信息 (2/4)"));
                SpiralAbyss? spiralAbyss = await recordProvider.GetSpiralAbyssAsync(uid, server, 1);
                if (spiralAbyss is null)
                {
                    this.messenger.Send(new RecordProgressChangedMessage(null));
                    return new Record($"获取本期深境螺旋信息失败");
                }

                this.messenger.Send(new RecordProgressChangedMessage("正在获取 上期深境螺旋信息 (3/4)"));
                SpiralAbyss? lastSpiralAbyss = await recordProvider.GetSpiralAbyssAsync(uid, server, 2);
                if (lastSpiralAbyss is null)
                {
                    this.messenger.Send(new RecordProgressChangedMessage(null));
                    return new Record($"获取上期深境螺旋信息失败");
                }

                this.messenger.Send(new RecordProgressChangedMessage("正在获取 详细角色信息 (4/4)"));
                DetailedAvatarWrapper? detailedAvatarInfo = await recordProvider.GetDetailAvaterInfoAsync(uid, server, playerInfo);
                if (detailedAvatarInfo is null)
                {
                    this.messenger.Send(new RecordProgressChangedMessage(null));
                    return new Record($"获取详细角色信息失败");
                }

                this.messenger.Send(new RecordProgressChangedMessage(null));
                //return
                return new Record
                {
                    Success = true,
                    UserId = uid,
                    Server = server,
                    PlayerInfo = playerInfo,
                    SpiralAbyss = spiralAbyss,
                    LastSpiralAbyss = lastSpiralAbyss,
                    DetailedAvatars = detailedAvatarInfo.Avatars
                };
            });
            return result;
        }
    }
}