using Microsoft.CodeAnalysis;
using System;

namespace DGP.Genshin.SourceGeneration
{
    [Generator]
    public class AppVersionGenerator : ISourceGenerator
    {
        private const string AutoVersionKey = "SGAppAutoVersion";

        public void Execute(GeneratorExecutionContext context)
        {
            string version = Environment.GetEnvironmentVariable(AutoVersionKey, EnvironmentVariableTarget.User);

            string sourceCode= $@"using System.Reflection;
[assembly: AssemblyVersion(""{version}"")]";

            context.AddSource("AssemblyInfo.g.cs", sourceCode);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            string currentVersion = $"{DateTime.Now:yyyy.M.d}.0";
            //环境变量中存在版本号
            if(Environment.GetEnvironmentVariable(AutoVersionKey,EnvironmentVariableTarget.User) is string oldVersionString)
            {
                if (string.Compare(currentVersion, oldVersionString) <= 0)
                {
                    currentVersion = oldVersionString;
                }
            }
            //递增版本号
            string[] versionSegments = currentVersion.Split('.');

            versionSegments[3] = (int.Parse(versionSegments[3]) + 1).ToString();
            currentVersion = string.Join(".", versionSegments);

            Environment.SetEnvironmentVariable(AutoVersionKey, currentVersion, EnvironmentVariableTarget.User);
        }
    }
}
