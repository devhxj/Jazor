using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

internal static class RazorSgGeneratedCSharpBinder
{
    private const string RenderTreeBuilderMetadataName = "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder";

    public static bool TryBind(
        RazorSgTailBatch batch,
        out RazorSgGeneratedCSharpBinding? binding,
        out string? failure)
    {
        if (batch is null)
            throw new ArgumentNullException(nameof(batch));

        binding = null;
        failure = null;
        if (batch.Documents.IsDefaultOrEmpty)
        {
            failure = "The Razor SG generated-C# binder did not receive any documents.";
            return false;
        }

        var parseOptions = batch.HookCompilation.SyntaxTrees
            .Select(static tree => tree.Options)
            .OfType<CSharpParseOptions>()
            .FirstOrDefault();
        if (parseOptions is null)
        {
            failure = "The Razor SG hook compilation did not expose C# parse options for generated documents.";
            return false;
        }

        var orderedDocuments = batch.Documents
            .OrderBy(static document => document.Identity.SourcePath, StringComparer.Ordinal)
            .ThenBy(static document => document.Identity.HintName, StringComparer.Ordinal)
            .ThenBy(static document => document.ContentHash, StringComparer.Ordinal)
            .ToImmutableArray();
        if (!TryValidateDocumentSet(orderedDocuments, out failure))
            return false;

        var matchedTrees = new Dictionary<RazorSgGeneratedDocument, SyntaxTree>();
        var missingTrees = ImmutableArray.CreateBuilder<SyntaxTree>();
        var reusedTreeCount = 0;
        foreach (var document in orderedDocuments)
        {
            var candidates = batch.HookCompilation.SyntaxTrees
                .Where(tree => string.Equals(
                    NormalizeTreePath(tree.FilePath),
                    NormalizeTreePath(document.HintName),
                    StringComparison.Ordinal))
                .ToImmutableArray();
            if (candidates.Length > 1)
            {
                failure = "The Razor SG hook compilation contained multiple syntax trees for hint name '" +
                          document.HintName +
                          "'.";
                return false;
            }

            if (candidates.Length == 1)
            {
                var existingTree = candidates[0];
                if (!existingTree.GetText().ContentEquals(document.GeneratedCSharp))
                {
                    failure = "The Razor SG hook compilation contained a stale or conflicting tree for hint name '" +
                              document.HintName +
                              "'.";
                    return false;
                }

                matchedTrees.Add(document, existingTree);
                reusedTreeCount++;
                continue;
            }

            var missingTree = CSharpSyntaxTree.ParseText(
                document.GeneratedCSharp,
                options: parseOptions,
                path: document.HintName);
            matchedTrees.Add(document, missingTree);
            missingTrees.Add(missingTree);
        }

        var boundCompilation = missingTrees.Count == 0
            ? batch.HookCompilation
            : batch.HookCompilation.AddSyntaxTrees(missingTrees.ToImmutable());
        if (!TryGetGeneratedTreeDiagnostics(boundCompilation, matchedTrees.Values, out failure))
            return false;

        var components = ImmutableArray.CreateBuilder<RazorSgBoundComponent>();
        var componentIdentities = new HashSet<string>(StringComparer.Ordinal);
        foreach (var document in orderedDocuments)
        {
            var generatedTree = matchedTrees[document];
            if (!TryBindComponent(
                    boundCompilation,
                    generatedTree,
                    document,
                    out var component,
                    out failure))
            {
                return false;
            }

            var componentIdentity = component!.ComponentSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (!componentIdentities.Add(componentIdentity))
            {
                failure = "The Razor SG generated-C# batch contained duplicate component identity '" +
                          componentIdentity +
                          "'.";
                return false;
            }

            components.Add(component);
        }

        binding = new RazorSgGeneratedCSharpBinding(
            boundCompilation,
            missingTrees.Count == 0
                ? RazorSgCompilationBindingMode.ReusedHookCompilation
                : RazorSgCompilationBindingMode.DerivedHookCompilation,
            orderedDocuments,
            components.ToImmutable(),
            reusedTreeCount,
            missingTrees.Count);
        return true;
    }

    private static bool TryValidateDocumentSet(
        ImmutableArray<RazorSgGeneratedDocument> documents,
        out string? failure)
    {
        failure = null;
        var identities = new HashSet<RazorSgGeneratedDocumentIdentity>();
        var hints = new HashSet<string>(StringComparer.Ordinal);
        foreach (var document in documents)
        {
            if (!identities.Add(document.Identity))
            {
                failure = "The Razor SG generated-C# batch contained duplicate document identity '" +
                          document.Identity.SourcePath +
                          "' / '" +
                          document.Identity.HintName +
                          "'.";
                return false;
            }

            if (!hints.Add(document.HintName))
            {
                failure = "The Razor SG generated-C# batch contained duplicate hint name '" +
                          document.HintName +
                          "'.";
                return false;
            }
        }

        return true;
    }

    private static bool TryGetGeneratedTreeDiagnostics(
        Compilation compilation,
        IEnumerable<SyntaxTree> generatedTrees,
        out string? failure)
    {
        failure = null;
        var treeSet = new HashSet<SyntaxTree>(generatedTrees);
        var errors = compilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error &&
                                 diagnostic.Location.SourceTree is not null &&
                                 treeSet.Contains(diagnostic.Location.SourceTree))
            .ToImmutableArray();
        if (errors.IsDefaultOrEmpty)
            return true;

        failure = "The Razor SG generated C# did not bind: " +
                  string.Join(
                      Environment.NewLine,
                      errors.Select(static diagnostic => diagnostic.ToString()));
        return false;
    }

    private static bool TryBindComponent(
        Compilation compilation,
        SyntaxTree generatedTree,
        RazorSgGeneratedDocument document,
        out RazorSgBoundComponent? component,
        out string? failure)
    {
        component = null;
        failure = null;
        var semanticModel = compilation.GetSemanticModel(generatedTree);
        var candidates = generatedTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Select(declaration => new
            {
                Declaration = declaration,
                Symbol = semanticModel.GetDeclaredSymbol(declaration)
            })
            .Where(static candidate => IsBuildRenderTree(candidate.Symbol))
            .ToImmutableArray();
        if (candidates.Length == 0)
        {
            failure = "The Razor SG generated document '" +
                      document.HintName +
                      "' did not declare BuildRenderTree(RenderTreeBuilder).";
            return false;
        }

        if (candidates.Length > 1)
        {
            failure = "The Razor SG generated document '" +
                      document.HintName +
                      "' declared multiple BuildRenderTree(RenderTreeBuilder) methods.";
            return false;
        }

        var candidate = candidates[0];
        if (candidate.Symbol?.ContainingType is not INamedTypeSymbol componentSymbol)
        {
            failure = "The Razor SG generated document '" +
                      document.HintName +
                      "' did not bind BuildRenderTree to a component type.";
            return false;
        }

        if (candidate.Declaration.Body is null ||
            semanticModel.GetOperation(candidate.Declaration.Body) is not IBlockOperation body)
        {
            failure = "The Razor SG generated document '" +
                      document.HintName +
                      "' did not provide a bindable BuildRenderTree block operation.";
            return false;
        }

        component = new RazorSgBoundComponent(document, componentSymbol, candidate.Symbol, body);
        return true;
    }

    private static bool IsBuildRenderTree(IMethodSymbol? method)
        => method is not null &&
           string.Equals(method.Name, "BuildRenderTree", StringComparison.Ordinal) &&
           method.Parameters.Length == 1 &&
           string.Equals(
               method.Parameters[0].Type.ToDisplayString(),
               RenderTreeBuilderMetadataName,
               StringComparison.Ordinal);

    private static string NormalizeTreePath(string path)
        => (path ?? string.Empty).Replace('\\', '/');
}

internal enum RazorSgCompilationBindingMode
{
    ReusedHookCompilation,
    DerivedHookCompilation
}

internal sealed record RazorSgGeneratedCSharpBinding(
    Compilation Compilation,
    RazorSgCompilationBindingMode BindingMode,
    ImmutableArray<RazorSgGeneratedDocument> Documents,
    ImmutableArray<RazorSgBoundComponent> Components,
    int ReusedGeneratedTreeCount,
    int DerivedGeneratedTreeCount);

internal sealed record RazorSgBoundComponent(
    RazorSgGeneratedDocument Document,
    INamedTypeSymbol ComponentSymbol,
    IMethodSymbol BuildRenderTreeMethod,
    IBlockOperation BuildRenderTreeBody);
