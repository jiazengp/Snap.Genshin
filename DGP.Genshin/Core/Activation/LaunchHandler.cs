using Snap.Core.Logging;
using System.Text.RegularExpressions;

namespace DGP.Genshin.Core.Activation
{
    /// <summary>
    /// 启动处理器
    /// </summary>
    internal sealed class LaunchHandler
    {
        /// <summary>
        /// 处理启动参数
        /// </summary>
        /// <param name="argument">参数</param>
        public void Handle(string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                this.Log($"无参数");
                return;
            }

            this.Log($"Activated with [{argument}]");

            if (Uri.TryCreate(argument, UriKind.Absolute, out Uri? argumentUri))
            {
                UriBuilder uriBuilder = new(argumentUri);
                if (uriBuilder.Scheme == "snapgenshin")
                {
                    // switch page portion
                    switch (uriBuilder.Host.ToLowerInvariant())
                    {
                        case "achievement":
                            {
                                // switch action
                                switch (uriBuilder.Path)
                                {
                                    case "/import/file":
                                        {
                                            string path = Uri.UnescapeDataString(uriBuilder.Query);
                                            Match match = new Regex("(?<=\\?path=\")(.*?)(?=\")").Match(path);
                                            path = match.Success ? match.Value : string.Empty;

                                            // TODO: add import handler
                                            break;
                                        }

                                    default:
                                        break;
                                }

                                break;
                            }

                        default:
                            break;
                    }
                }
                else
                {
                    // TODO: notify not support
                }
            }
        }
    }
}