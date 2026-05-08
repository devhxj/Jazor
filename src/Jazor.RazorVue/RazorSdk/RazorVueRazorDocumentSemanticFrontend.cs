using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Extensibility;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.RazorVue.RazorSdk;

internal sealed class RazorVueRazorDocumentSemanticFrontend : IRazorSemanticFrontend
{
    public static RazorVueRazorDocumentSemanticFrontend Instance { get; } = new();

    private RazorVueRazorDocumentSemanticFrontend()
    {
    }

    public string Name => "Jazor.RazorVue.RazorSdk.RazorVueRazorDocumentSemanticFrontend";

    public bool CanHandle(Jazor.RazorVue.RazorVueCompilationContext context)
        => context is not null;

    public RazorVueEntryKind ClassifyEntry(Jazor.RazorVue.RazorVueCompilationContext context, INamedTypeSymbol symbol)
        => GetRequiredContext(context).ClassifyEntry(symbol);

    public ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots(Jazor.RazorVue.RazorVueCompilationContext context)
        => CreateSemanticSnapshots(context, ImmutableArray<RazorVueRazorSourceGeneratorDocument>.Empty);

    public ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots(
        Jazor.RazorVue.RazorVueCompilationContext context,
        ImmutableArray<RazorVueRazorSourceGeneratorDocument> sourceGeneratorDocuments)
    {
        var requiredContext = GetRequiredContext(context);
        var builder = ImmutableArray.CreateBuilder<RazorVueSemanticSnapshot>();
        var documentsByTypeName = CreateDocumentMap(sourceGeneratorDocuments);

        foreach (var candidate in requiredContext.DiscoverComponentCandidates())
        {
            var componentSymbol = candidate.ComponentSymbol;
            documentsByTypeName.TryGetValue(
                componentSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                out var sourceGeneratorDocument);

            if (sourceGeneratorDocument is not null)
            {
                builder.Add(requiredContext.CreateSemanticSnapshot(candidate, null, sourceGeneratorDocument));
                continue;
            }

            if (RazorVueRazorIrCarrier.TryResolve(componentSymbol, out var carrier))
            {
                builder.Add(requiredContext.CreateSemanticSnapshot(candidate, carrier, null));
                continue;
            }

            builder.Add(requiredContext.CreateSemanticSnapshot(candidate, null, sourceGeneratorDocument));
        }

        return builder.ToImmutable();
    }

    private static Dictionary<string, RazorVueRazorSourceGeneratorDocument> CreateDocumentMap(
        ImmutableArray<RazorVueRazorSourceGeneratorDocument> sourceGeneratorDocuments)
    {
        var map = new Dictionary<string, RazorVueRazorSourceGeneratorDocument>(StringComparer.Ordinal);
        if (sourceGeneratorDocuments.IsDefaultOrEmpty)
            return map;

        foreach (var document in sourceGeneratorDocuments)
        {
            if (TryGetGeneratedTypeName(document, out var typeName))
                map[typeName] = document;
        }

        return map;
    }

    private static bool TryGetGeneratedTypeName(
        RazorVueRazorSourceGeneratorDocument document,
        out string typeName)
    {
        typeName = string.Empty;
        var root = CSharpSyntaxTree.ParseText(document.CSharpText).GetRoot();
        var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classDeclaration is null)
            return false;

        var namespaceName = classDeclaration.Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault()
            ?.Name
            .ToString();

        typeName = string.IsNullOrWhiteSpace(namespaceName)
            ? "global::" + classDeclaration.Identifier.ValueText
            : "global::" + namespaceName + "." + classDeclaration.Identifier.ValueText;
        return true;
    }

    private static Jazor.RazorVue.RazorVueCompilationContext GetRequiredContext(Jazor.RazorVue.RazorVueCompilationContext context)
        => context ?? throw new InvalidOperationException("The Razor SDK semantic frontend expected a valid RazorVue compilation context.");
}
