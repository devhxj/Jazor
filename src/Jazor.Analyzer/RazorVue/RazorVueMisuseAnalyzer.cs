using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;

namespace Jazor.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RazorVueMisuseAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        RazorVueDiagnosticDescriptors.StateHasChangedNotSupported,
        RazorVueDiagnosticDescriptors.ShouldRenderNotSupported,
        RazorVueDiagnosticDescriptors.SetParametersAsyncNotSupported
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation invocation)
            return;

        var containingType = context.ContainingSymbol?.ContainingType;
        if (containingType is null)
            return;

        var knownSymbols = RazorVueKnownSymbols.TryCreate(context.Compilation);
        if (knownSymbols is null || !knownSymbols.IsRazorVueComponent(containingType))
            return;

        if (!knownSymbols.IsStateHasChanged(invocation.TargetMethod))
            return;

        // Vue drives update scheduling in RazorVue, so StateHasChanged must not
        // silently survive as a hidden Blazor semantic.
        context.ReportDiagnostic(Diagnostic.Create(
            RazorVueDiagnosticDescriptors.StateHasChangedNotSupported,
            invocation.Syntax.GetLocation()));
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        if (context.Symbol is not IMethodSymbol method || method.MethodKind is not MethodKind.Ordinary and not MethodKind.ReducedExtension)
            return;

        var containingType = method.ContainingType;
        if (containingType is null)
            return;

        var knownSymbols = RazorVueKnownSymbols.TryCreate(context.Compilation);
        if (knownSymbols is null || !knownSymbols.IsRazorVueComponent(containingType))
            return;

        var location = method.Locations.FirstOrDefault(static x => x.IsInSource) ?? Location.None;
        if (knownSymbols.IsShouldRender(method))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                RazorVueDiagnosticDescriptors.ShouldRenderNotSupported,
                location));
            return;
        }

        if (knownSymbols.IsSetParametersAsync(method))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                RazorVueDiagnosticDescriptors.SetParametersAsyncNotSupported,
                location));
        }
    }
}
