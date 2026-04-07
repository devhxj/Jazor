using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Jazor.RazorVue;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Discovery;
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
        RazorVueDiagnosticDescriptors.DuplicateSlotValue,
        RazorVueDiagnosticDescriptors.InvalidLibraryComponentDeclaration,
        RazorVueDiagnosticDescriptors.InvalidLibraryStyleDependencyDeclaration,
        RazorVueDiagnosticDescriptors.InvalidLibraryPluginRequirementDeclaration
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static startContext =>
        {
            var compilationContext = RazorVueCompilationContext.TryCreate(startContext.Compilation);
            if (compilationContext is null)
                return;

            startContext.RegisterSymbolAction(
                symbolContext => AnalyzeLibraryComponentDeclaration(symbolContext, compilationContext.Symbols),
                SymbolKind.NamedType);
        });
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
            {
                if (exception.Issue.Code == RazorVueIssueCode.InvalidLibraryComponentDeclaration)
                    continue;

                if (!TryGetDescriptor(exception.Issue.Code, out var descriptor))
                    continue;

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

    private static void AnalyzeLibraryComponentDeclaration(
        SymbolAnalysisContext context,
        RazorVueCompilationSymbols symbols)
    {
        if (context.Symbol is not INamedTypeSymbol symbol ||
            !RazorVueEntryClassifier.IsLibraryComponent(symbol, symbols))
        {
            return;
        }

        if (!TryGetLibraryMetadataDiagnostic(symbol, symbols, out var descriptor, out var message))
            return;

        context.ReportDiagnostic(Diagnostic.Create(
            descriptor,
            symbol.Locations.FirstOrDefault(static location => location.IsInSource) ?? Location.None,
            message));
    }

    private static bool TryGetLibraryMetadataDiagnostic(
        INamedTypeSymbol symbol,
        RazorVueCompilationSymbols symbols,
        out DiagnosticDescriptor descriptor,
        out string message)
    {
        descriptor = null!;
        message = string.Empty;

        if (!HasValidLibraryComponentAttribute(symbol, symbols))
        {
            descriptor = RazorVueDiagnosticDescriptors.InvalidLibraryComponentDeclaration;
            message = $"Library component '{symbol.ToDisplayString()}' must declare [VueLibraryComponent(importSpecifier, exportName)].";
            return true;
        }

        if (TryGetInvalidMetadataValue(
                symbol,
                symbols.VueLibraryStyleAttribute,
                "[VueLibraryStyle(styleSpecifier)]",
                "style dependency",
                out var styleMessage))
        {
            descriptor = RazorVueDiagnosticDescriptors.InvalidLibraryStyleDependencyDeclaration;
            message = styleMessage;
            return true;
        }

        if (TryGetInvalidMetadataValue(
                symbol,
                symbols.VueLibraryPluginRequirementAttribute,
                "[VueLibraryPluginRequirement(requirementId)]",
                "plugin requirement",
                out var pluginMessage))
        {
            descriptor = RazorVueDiagnosticDescriptors.InvalidLibraryPluginRequirementDeclaration;
            message = pluginMessage;
            return true;
        }

        return false;
    }

    private static bool HasValidLibraryComponentAttribute(
        INamedTypeSymbol symbol,
        RazorVueCompilationSymbols symbols)
    {
        if (symbols.VueLibraryComponentAttribute is null)
            return false;

        foreach (var attribute in symbol.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, symbols.VueLibraryComponentAttribute) ||
                attribute.ConstructorArguments.Length < 2 ||
                attribute.ConstructorArguments[0].Value is not string importSpecifier ||
                string.IsNullOrWhiteSpace(importSpecifier) ||
                attribute.ConstructorArguments[1].Value is not string exportName ||
                string.IsNullOrWhiteSpace(exportName))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static bool TryGetInvalidMetadataValue(
        INamedTypeSymbol symbol,
        INamedTypeSymbol? attributeSymbol,
        string attributeDisplayName,
        string valueKind,
        out string message)
    {
        message = string.Empty;
        if (attributeSymbol is null)
            return false;

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var attribute in symbol.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeSymbol))
                continue;

            if (attribute.ConstructorArguments.Length != 1 ||
                attribute.ConstructorArguments[0].Value is not string rawValue ||
                string.IsNullOrWhiteSpace(rawValue))
            {
                message = $"Library component '{symbol.ToDisplayString()}' has an invalid {attributeDisplayName} declaration.";
                return true;
            }

            var normalizedValue = rawValue.Trim();
            if (!seen.Add(normalizedValue))
            {
                message = $"Library component '{symbol.ToDisplayString()}' declares duplicate {valueKind} '{normalizedValue}'.";
                return true;
            }
        }

        return false;
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
            case RazorVueIssueCode.InvalidLibraryComponentDeclaration:
                descriptor = RazorVueDiagnosticDescriptors.InvalidLibraryComponentDeclaration;
                return true;
            case RazorVueIssueCode.InvalidLibraryStyleDependencyDeclaration:
                descriptor = RazorVueDiagnosticDescriptors.InvalidLibraryStyleDependencyDeclaration;
                return true;
            case RazorVueIssueCode.InvalidLibraryPluginRequirementDeclaration:
                descriptor = RazorVueDiagnosticDescriptors.InvalidLibraryPluginRequirementDeclaration;
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
