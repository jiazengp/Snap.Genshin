using HtmlAgilityPack;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DGP.Genshin2.Data.GHHW
{
    internal sealed class HtmlParser
    {
        private const string GHHWUrl = @"https://genshin.honeyhunterworld.com/";

        public async Task<Node> GetBaseContainerAsync()
        {
            HtmlDocument htmlDoc = await new HtmlWeb().LoadFromWebAsync(GHHWUrl);
            HtmlNode container = htmlDoc.DocumentNode.SelectSingleNode(@"/html/body/div[1]");
            return new Node(container.Name, GetContentOf(container));
        }
        private List<Node> GetContentOf(HtmlNode node)
        {
            List<Node> nodes = new List<Node>();
            foreach(var n in node.ChildNodes)
            {
                nodes.Add(new Node(n.Name, GetContentOf(n)));
            }
            return nodes;
        }
    }
}
