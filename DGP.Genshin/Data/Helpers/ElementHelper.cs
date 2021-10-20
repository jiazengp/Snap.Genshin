namespace DGP.Genshin.Data.Helpers
{
    public class ElementHelper
    {
        public static string FromENGName(string? element) => $@"https://genshin.honeyhunterworld.com/img/icons/element/{element?.ToLower()}.png";

        public static string? ConvertCHSToENG(string s)
        {
            switch (s)
            {
                case "草":
                    return "dendro";
                case "风":
                    return "dendro";
                case "火":
                    return "dendro";
                case "冰":
                    return "dendro";
                case "水":
                    return "dendro";
                case "雷":
                    return "dendro";
                case "岩":
                    return "dendro";
                default:
                    return null;
            }
        }
    }
}
