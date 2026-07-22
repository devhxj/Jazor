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
                builder.Add(requiredContext.CreateSemanticSnapshot(
                    candidate,
                    null,
                    sourceGeneratorDocument,
                    CollectImportedNamespaces(sourceGeneratorDocument)));
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

        var typeParameterList = classDeclaration.TypeParameterList is null
            ? string.Empty
            : "<" + string.Join(
                ", ",
                classDeclaration.TypeParameterList.Parameters.Select(static parameter => parameter.Identifier.ValueText)) + ">";

        typeName = string.IsNullOrWhiteSpace(namespaceName)
            ? "global::" + classDeclaration.Identifier.ValueText + typeParameterList
            : "global::" + namespaceName + "." + classDeclaration.Identifier.ValueText + typeParameterList;
        return true;
    }

    private static ImmutableArray<string> CollectImportedNamespaces(RazorVueRazorSourceGeneratorDocument document)
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        AddImportedNamespaces(document.CSharpText.ToString());
        foreach (var importDocument in document.ImportDocuments)
            AddRazorUsingDirectives(importDocument.Text.ToString());

        return builder.ToImmutable();

        void AddImportedNamespaces(string csharpText)
        {
            if (string.IsNullOrWhiteSpace(csharpText))
                return;

            var root = CSharpSyntaxTree.ParseText(csharpText).GetRoot();
            foreach (var directive in root.DescendantNodes()
                         .OfType<UsingDirectiveSyntax>()
                         .Where(static directive =>
                             directive.Alias is null &&
                             !directive.StaticKeyword.IsKind(SyntaxKind.StaticKeyword) &&
                             directive.Name is not null))
            {
                Add(directive.Name!.ToString());
            }
        }

        void AddRazorUsingDirectives(string razorText)
        {
            if (string.IsNullOrWhiteSpace(razorText))
                return;

            foreach (var line in razorText.Split(["\r\n", "\n", "\r"], StringSplitOptions.None))
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith("@using ", StringComparison.Ordinal))
                    continue;

                var namespaceText = trimmed.Substring("@using ".Length).Trim();
                if (namespaceText.StartsWith("static ", StringComparison.Ordinal))
                    continue;

                var semicolonIndex = namespaceText.IndexOf(';');
                if (semicolonIndex >= 0)
                    namespaceText = namespaceText.Substring(0, semicolonIndex).Trim();

                Add(namespaceText);
            }
        }

        void Add(string importedNamespace)
        {
            if (string.IsNullOrWhiteSpace(importedNamespace))
                return;

            if (seen.Add(importedNamespace))
                builder.Add(importedNamespace);
        }
    }

    private static Jazor.RazorVue.RazorVueCompilationContext GetRequiredContext(Jazor.RazorVue.RazorVueCompilationContext context)
        => context ?? throw new InvalidOperationException("The Razor SDK semantic frontend expected a valid RazorVue compilation context.");
}
