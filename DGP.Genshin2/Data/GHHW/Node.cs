using System.Collections.Generic;
using System.Text;

namespace DGP.Genshin2.Data.GHHW
{
    public class Node
    {
        public Node ParentNode { get; private set; }
        public NodeType NodeType { get; private set; }
        public List<Node> ChildNodes { get; private set; }
        public int Depth { get; private set; } = 0;

        public Node(NodeType nodeType, List<Node> childNodes)
        {
            InitializeInternal(nodeType, childNodes);
        }
        public Node(string nodeType, List<Node> childNodes)
        {
            NodeType type = Parse(nodeType);
            InitializeInternal(type, childNodes);
        }
        private void InitializeInternal(NodeType nodeType, List<Node> childNodes)
        {
            NodeType = nodeType;
            ChildNodes = childNodes;
            foreach (Node child in ChildNodes)
            {
                child.ParentNode = this;
                child.IncreaseDepth();
            }
        }

        private NodeType Parse(string type)
        {
            switch (type)
            {
                case "div":
                    return NodeType.Div;
                case "nav":
                    return NodeType.Nav;
                case "#text":
                    return NodeType.Text;
                case "button":
                    return NodeType.Button;
                case "span":
                    return NodeType.Span;
                case "p":
                    return NodeType.P;
                case "ul":
                    return NodeType.Ul;
                case "li":
                    return NodeType.Li;
                case "a":
                    return NodeType.A;
                case "i":
                    return NodeType.I;
                case "input":
                    return NodeType.Input;
                case "label":
                    return NodeType.Label;
                case "h1":
                    return NodeType.H1;
                case "table":
                    return NodeType.Table;
                case "tr":
                    return NodeType.Tr;
                case "th":
                    return NodeType.Th;
                case "td":
                    return NodeType.Td;
                case "img":
                    return NodeType.Img;
                case "h3":
                    return NodeType.H3;
                case "style":
                    return NodeType.Style;
                case "form":
                    return NodeType.Form;
                default:
                    return NodeType.Other;
            }
        }
        private void IncreaseDepth()
        {
            this.Depth++;
            foreach(Node node in ChildNodes)
            {
                node.IncreaseDepth();
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(new string('	', Depth)).Append(NodeType).Append("\n");
            foreach(Node n in ChildNodes)
            {
                sb.Append(n);
            }
            return sb.ToString();
        }
    }
}
