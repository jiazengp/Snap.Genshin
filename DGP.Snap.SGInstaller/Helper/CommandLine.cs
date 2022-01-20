using System.Collections.Generic;
using System.Text;

namespace DGP.Snap.SGInstaller.Helper
{
    public class CommandLine
    {
        private const char WhiteSpace = ' ';
        private readonly Dictionary<string, string?> options = new();

        public CommandLine WithIf(bool condition, string name, object? value = null)
        {
            return condition ? With(name, value) : this;
        }

        public CommandLine With(string name, object? value = null)
        {
            options.Add(name, value?.ToString());
            return this;
        }

        public string Build()
        {
            return ToString();
        }

        public override string ToString()
        {
            StringBuilder s = new();
            foreach (KeyValuePair<string, string?> option in options)
            {
                s.Append(WhiteSpace);
                s.Append(option.Key);
                if (!string.IsNullOrEmpty(option.Value))
                {
                    s.Append(WhiteSpace);
                    s.Append(option.Value);
                }
            }
            return s.ToString();
        }
    }
}
