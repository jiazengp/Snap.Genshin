using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace Snap.Genshin.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SGAPI0001MethodUnstableAnalyzer : DiagnosticAnalyzer
    {
        public const string Id = "SGAPI0001";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            Id,
            "使用了可能会发生改动的SG API",
            "方法 {0} 在后续的版本中可能会发生改动，不应继续使用此方法",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get => ImmutableArray.Create(Rule);
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterOperationAction(Analyze, OperationKind.MethodReference);
        }

        private static void Analyze(OperationAnalysisContext context)
        {
            IMethodReferenceOperation methodOperation = (IMethodReferenceOperation)context.Operation;

            ImmutableArray<AttributeData> attrs = methodOperation.Method.GetAttributes();
            foreach (AttributeData attr in attrs)
            {
                if (attr.AttributeClass.Name == "DGP.Genshin.StableAttribute")
                {
                    // For all such symbols, produce a diagnostic.
                    Diagnostic diagnostic = Diagnostic.Create(Rule, methodOperation.Method.Locations[0]);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
