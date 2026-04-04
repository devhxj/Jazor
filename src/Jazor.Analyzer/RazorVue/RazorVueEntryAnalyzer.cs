using Jazor.RazorVue.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Jazor.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RazorVueEntryAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        RazorVueDiagnosticDescriptors.InvalidEntryInheritance,
        RazorVueDiagnosticDescriptors.DirectComponentBaseEntry
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol symbol || symbol.TypeKind != TypeKind.Class)
            return;

        var knownSymbols = RazorVueKnownSymbols.TryCreate(context.Compilation);
        if (knownSymbols is null || !knownSymbols.HasECMAScriptModuleAttribute(symbol))
            return;

        var location = symbol.Locations.FirstOrDefault(static x => x.IsInSource) ?? Location.None;
        // Direct ComponentBase inheritance gets its own diagnostic because it is
        // the most likely migration mistake and deserves a clearer message.
        if (knownSymbols.IsDirectComponentBaseEntry(symbol))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                RazorVueDiagnosticDescriptors.DirectComponentBaseEntry,
                location,
                symbol.ToDisplayString()));
            return;
        }

        if (knownSymbols.Classify(symbol) == RazorVueEntryKind.Invalid)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                RazorVueDiagnosticDescriptors.InvalidEntryInheritance,
                location,
                symbol.ToDisplayString()));
        }
    }
}

