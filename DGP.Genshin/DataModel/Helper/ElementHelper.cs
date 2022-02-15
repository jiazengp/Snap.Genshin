namespace DGP.Genshin.DataModel.Helper
{
    public class ElementHelper
    {
        public static string FromENGName(string? element)
        {
            return $@"https://genshin.honeyhunterworld.com/img/icons/element/{element?.ToLower()}.png";
        }
    }
}
