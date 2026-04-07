using System;
using System.Collections.Immutable;
using System.Linq;
using Jazor.RazorVue;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Lowering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RazorVueAuthoringAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        RazorVueDiagnosticDescriptors.UnknownParameter,
        RazorVueDiagnosticDescriptors.InvalidBindTarget,
        RazorVueDiagnosticDescriptors.UnknownSlot,
        RazorVueDiagnosticDescriptors.SlotContextMisuse,
        RazorVueDiagnosticDescriptors.DuplicateSlotValue
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        var compilationContext = RazorVueCompilationContext.TryCreate(context.Compilation);
        if (compilationContext is null)
            return;

        var lowerer = new RazorVueArtifactFactory();
        foreach (var snapshot in compilationContext.CreateSemanticSnapshots())
        {
            try
            {
                lowerer.Lower(compilationContext, snapshot);
            }
            catch (RazorVueCompilationIssueException exception)
                when (TryGetDescriptor(exception.Issue.Code, out var descriptor))
            {
                // Reuse the lowering pipeline as the single source of truth for
                // library authoring contracts so analyzer and generator diagnostics
                // stay aligned on the same slot/parameter rules.
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    GetDiagnosticLocation(snapshot, exception.Origin),
                    exception.Issue.Message));
            }
        }
    }

    private static bool TryGetDescriptor(RazorVueIssueCode issueCode, out DiagnosticDescriptor descriptor)
    {
        switch (issueCode)
        {
            case RazorVueIssueCode.UnknownParameter:
                descriptor = RazorVueDiagnosticDescriptors.UnknownParameter;
                return true;
            case RazorVueIssueCode.InvalidBindTarget:
                descriptor = RazorVueDiagnosticDescriptors.InvalidBindTarget;
                return true;
            case RazorVueIssueCode.UnknownSlot:
                descriptor = RazorVueDiagnosticDescriptors.UnknownSlot;
                return true;
            case RazorVueIssueCode.SlotContextMisuse:
                descriptor = RazorVueDiagnosticDescriptors.SlotContextMisuse;
                return true;
            case RazorVueIssueCode.DuplicateSlotValue:
                descriptor = RazorVueDiagnosticDescriptors.DuplicateSlotValue;
                return true;
            default:
                descriptor = null!;
                return false;
        }
    }

    private static Location GetDiagnosticLocation(
        RazorVueSemanticSnapshot snapshot,
        RazorVueSourceOrigin? origin)
    {
        var sourceTree = snapshot.ComponentSymbol.DeclaringSyntaxReferences
            .Select(static reference => reference.SyntaxTree)
            .FirstOrDefault();
        if (origin is not null &&
            sourceTree is not null &&
            origin.SourceSpanStart >= 0 &&
            origin.SourceSpanLength >= 0 &&
            origin.SourceSpanStart + origin.SourceSpanLength <= sourceTree.Length)
        {
            return Location.Create(sourceTree, new TextSpan(origin.SourceSpanStart, origin.SourceSpanLength));
        }

        return snapshot.BuildRenderTreeMethod?.Locations.FirstOrDefault(static location => location.IsInSource) ??
               snapshot.ComponentSymbol.Locations.FirstOrDefault(static location => location.IsInSource) ??
               Location.None;
    }
}
