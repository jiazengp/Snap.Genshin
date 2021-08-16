using Newtonsoft.Json;

namespace DGP.Genshin.Models.MiHoYo.Sign
{
    public class SignInResult
    {
        [JsonProperty("code")] public string Code { get; set; }
    }
}
