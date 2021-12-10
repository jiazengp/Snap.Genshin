using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.YoungMoeAPI.Collocation;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.DataModel.YoungMoe2
{
    public class DetailedAvatarInfo2 : DetailedAvatarInfo
    {
        public new IEnumerable<CollocationWeapon2>? CollocationWeapon { get; set; }
        public new IEnumerable<CollocationAvatar2>? CollocationAvatar { get; set; }
    }
}
