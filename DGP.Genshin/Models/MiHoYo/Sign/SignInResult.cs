using DGP.Snap.Framework.Attributes.DataModel;
using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.Sign
{
    [JsonModel]
    public class SignInResult
    {
        [JsonProperty("code")] public string Code { get; set; }
    }
}
