using Microsoft.CodeAnalysis;
using System;

namespace DGP.Genshin.SourceGeneration
{
    [Generator]
    public class AppCenterGenerator : ISourceGenerator
    {
        /// <summary>
        /// 需要在系统环境变量中添加
        /// key: SGReleaseAppCenterSecret value: secret
        /// </summary>
        /// <param name="context"></param>
        public void Execute(GeneratorExecutionContext context)
        {
            string secret = Environment.GetEnvironmentVariable("SGReleaseAppCenterSecret", EnvironmentVariableTarget.User);
            string name = this.GetGeneratorType().Namespace;
            string version = this.GetGeneratorType().Assembly.ImageRuntimeVersion;
            string sourceCode = $@"using DGP.Genshin.Helper;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Snap.Core.Logging;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;

namespace DGP.Genshin
{{
    [DebuggerNonUserCode]
    [GeneratedCode(""{name}"",""{version}"")]
    public partial class App : Application
    {{
        partial void ConfigureAppCenter(bool enabled)
        {{
            if (enabled)
            {{
                AppCenter.SetUserId(User.Id);
#if DEBUG
                //DEBUG INFO should send to Snap Genshin Debug kanban
                AppCenter.Start(""2e4fa440-132e-42a7-a288-22ab1a8606ef"", typeof(Analytics), typeof(Crashes));
#else
                //开发测试人员请不要生成 Release 版本
                if (!System.Diagnostics.Debugger.IsAttached)
                {{
                    //RELEASE INFO should send to Snap Genshin kanban
                    AppCenter.Start(""{secret}"", typeof(Analytics), typeof(Crashes));
                }}
                else
                {{
                    throw Microsoft.Verify.FailOperation(""请不要生成 Release 版本"");
                }}
#endif
                this.Log(""AppCenter Initialized"");
            }}
        }}
    }}
}}";
            context.AddSource("AppCenterConfiguration.g.cs", sourceCode);
        }

        public void Initialize(GeneratorInitializationContext context)
        {

        }
    }
}
