using System.Collections.Immutable;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Jolt.Lsp;
using Jolt.Lsp.Routing;
using Jolt.Razor.InProc;
using Jolt.VirtualDocuments.Mapping;
using Jolt.Workspace;
using Jazor.Vue;
using ECMAScript.Contract.VueContracts.Protocol;

namespace Jolt.Roslyn.InProc;

internal sealed partial class InProcRoslynCodeService
{
    private static readonly ConcurrentDictionary<string, string> ContainerNamesByPath = new(
        OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal);

    private static bool TryMapToProjectedPositionWithBoundaryFallback(
        ProjectionMap projectionMap,
        string sourceText,
        LspPosition sourcePosition,
        string projectedText,
        out LspPosition projectedPosition)
    {
        if (projectionMap.TryMapToProjectedPosition(sourceText, sourcePosition, projectedText, out projectedPosition))
        {
            return true;
        }

        var sourceOffset = LspProtocolHelpers.GetOffset(sourceText, sourcePosition);
        if (sourceOffset <= 0)
        {
            projectedPosition = new LspPosition();
            return false;
        }

        var maxDelta = sourceOffset;
        for (var delta = 1; delta <= maxDelta; delta++)
        {
            var probeSourceOffset = sourceOffset - delta;
            if (!projectionMap.TryMapToProjectedOffset(probeSourceOffset, out var probeProjectedOffset))
            {
                continue;
            }

            var adjustedProjectedOffset = Math.Min(probeProjectedOffset + delta, projectedText.Length);
            projectedPosition = LspProtocolHelpers.GetPosition(projectedText, adjustedProjectedOffset);
            return true;
        }

        projectedPosition = new LspPosition();
        return false;
    }

    private List<ProjectedDocumentContext> BuildProjectedDocuments(
        DocumentSnapshot primaryDocument,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken,
        out ProjectedDocumentContext? primaryProjectedDocument)
    {
        var projectedDocuments = new List<ProjectedDocumentContext>();
        var seenPaths = new HashSet<string>(PathComparer);

        primaryProjectedDocument = null;
        foreach (var sourceDocument in EnumerateRoslynSourceDocuments(primaryDocument, openDocuments, cancellationToken))
        {
            AddProjectedDocument(sourceDocument, projectedDocuments, seenPaths, out var projectedDocument);
            if (projectedDocument is not null && PathsEqual(sourceDocument.DocumentPath, primaryDocument.DocumentPath))
            {
                primaryProjectedDocument = projectedDocument;
            }
        }

        return projectedDocuments;
    }

    private void AddProjectedDocument(
        DocumentSnapshot document,
        ICollection<ProjectedDocumentContext> projectedDocuments,
        ISet<string> seenPaths,
        out ProjectedDocumentContext? projectedDocument)
    {
        projectedDocument = null;
        if (!seenPaths.Add(GetComparablePath(document.DocumentPath)))
            return;

        if (document.DocumentKind == DocumentKind.CSharp)
        {
            var projectionMap = ProjectionMap.CreateWholeDocument(
                document.DocumentPath,
                document.DocumentPath,
                document.Text.Length,
                document.Text.Length);
            var csharpSyntaxTree = CSharpSyntaxTree.ParseText(
                document.Text,
                ParseOptions,
                path: document.DocumentPath,
                encoding: Encoding.UTF8);
            projectedDocument = new ProjectedDocumentContext(
                document,
                document.Text,
                projectionMap,
                csharpSyntaxTree,
                SemanticModel: CreatePlaceholderSemanticModel(csharpSyntaxTree));
            projectedDocuments.Add(projectedDocument);
            return;
        }

        if (TryCreateRazorProjectedDocument(document, out projectedDocument))
        {
            projectedDocuments.Add(projectedDocument);
            return;
        }

        var parsed = JazorVueParser.Parse(document.DocumentPath, document.Text);
        if (string.IsNullOrWhiteSpace(parsed.Code) || parsed.CodeStartIndex < 0)
            return;

        var projection = CreateFallbackProjection(document, parsed);
        var syntaxTree = CSharpSyntaxTree.ParseText(
            projection.SourceText,
            ParseOptions,
            path: projection.ProjectedDocumentPath,
            encoding: Encoding.UTF8);
        projectedDocument = new ProjectedDocumentContext(
            document,
            projection.SourceText,
            projection.ProjectionMap,
            syntaxTree,
            SemanticModel: CreatePlaceholderSemanticModel(syntaxTree));
        projectedDocuments.Add(projectedDocument);
    }

    private bool TryCreateRazorProjectedDocument(
        DocumentSnapshot document,
        [NotNullWhen(true)] out ProjectedDocumentContext? projectedDocument)
    {
        if (!_razorProjectionService.TryCreateProjection(document, out var razorProjection))
        {
            projectedDocument = null;
            return false;
        }

        var syntaxTree = CSharpSyntaxTree.ParseText(
            razorProjection.SourceText,
            ParseOptions,
            path: razorProjection.ProjectedDocumentPath,
            encoding: Encoding.UTF8);
        projectedDocument = new ProjectedDocumentContext(
            document,
            razorProjection.SourceText,
            razorProjection.ProjectionMap,
            syntaxTree,
            SemanticModel: CreatePlaceholderSemanticModel(syntaxTree));
        return true;
    }

    private bool TryCreateFallbackProjectedDocument(
        DocumentSnapshot document,
        [NotNullWhen(true)] out ProjectedDocumentContext? projectedDocument)
    {
        var parsed = JazorVueParser.Parse(document.DocumentPath, document.Text);
        if (string.IsNullOrWhiteSpace(parsed.Code) || parsed.CodeStartIndex < 0)
        {
            projectedDocument = null;
            return false;
        }

        var projection = CreateFallbackProjection(document, parsed);
        var syntaxTree = CSharpSyntaxTree.ParseText(
            projection.SourceText,
            ParseOptions,
            path: projection.ProjectedDocumentPath,
            encoding: Encoding.UTF8);
        projectedDocument = new ProjectedDocumentContext(
            document,
            projection.SourceText,
            projection.ProjectionMap,
            syntaxTree,
            SemanticModel: CreatePlaceholderSemanticModel(syntaxTree));
        return true;
    }

    internal (string ProjectedDocumentPath, string SourceText, ProjectionMap ProjectionMap) CreateProjection(DocumentSnapshot document, JazorVueDocument parsed)
    {
        if (_razorProjectionService.TryCreateProjection(document, out var razorProjection))
        {
            return (
                razorProjection.ProjectedDocumentPath,
                razorProjection.SourceText,
                razorProjection.ProjectionMap);
        }

        return CreateFallbackProjection(document, parsed);
    }

    internal ValueTask<IReadOnlyList<DocumentSnapshot>> GetSourceDocumentsAsync(
        DocumentSnapshot primaryDocument,
        IReadOnlyList<DocumentSnapshot>? openDocuments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<IReadOnlyList<DocumentSnapshot>>(
            EnumerateRoslynSourceDocuments(primaryDocument, openDocuments, cancellationToken).ToArray());
    }

    internal static (string ProjectedDocumentPath, string SourceText, ProjectionMap ProjectionMap) CreateFallbackProjection(DocumentSnapshot document, JazorVueDocument parsed)
    {
        var projectedPath = "virtual:" + document.DocumentPath + ".inproc.g.cs";
        var sourceText = BuildProjectedSource(document.DocumentPath, document.Text, parsed);
        var projectionMap = new ProjectionMap(
            document.DocumentPath,
            projectedPath,
            TryCreateCodeProjectionSegment(parsed, sourceText, out var segment)
                && segment is not null
                ? [segment]
                : Array.Empty<ProjectionSegment>());

        return (projectedPath, sourceText, projectionMap);
    }

    internal static string BuildProjectedSource(string documentPath, string sourceText, JazorVueDocument parsed)
    {
        var builder = new StringBuilder();
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using System.Linq;");
        builder.AppendLine("using System.Threading.Tasks;");
        foreach (var import in UsingDirectivePattern.Matches(sourceText)
                     .Select(static match => match.Groups["ns"].Value.Trim())
                     .Where(static value => !string.IsNullOrWhiteSpace(value))
                     .Distinct(StringComparer.Ordinal))
        {
            builder.Append("using ")
                .Append(import.TrimEnd(';'))
                .AppendLine(";");
        }

        builder.AppendLine("#nullable enable");
        builder.AppendLine("namespace Jolt.RoslynProjection;");
        builder.AppendLine("[global::System.AttributeUsage(global::System.AttributeTargets.Property | global::System.AttributeTargets.Field | global::System.AttributeTargets.Method)]");
        builder.AppendLine("internal sealed class PropAttribute : global::System.Attribute { }");
        builder.AppendLine("[global::System.AttributeUsage(global::System.AttributeTargets.Property | global::System.AttributeTargets.Field | global::System.AttributeTargets.Method)]");
        builder.AppendLine("internal sealed class StateAttribute : global::System.Attribute { }");
        builder.AppendLine("[global::System.AttributeUsage(global::System.AttributeTargets.Property | global::System.AttributeTargets.Field | global::System.AttributeTargets.Method)]");
        builder.AppendLine("internal sealed class ComputedAttribute : global::System.Attribute { }");
        builder.Append("internal partial class ")
            .Append(CreateContainerName(documentPath))
            .AppendLine();
        builder.AppendLine("{");
        builder.AppendLine(parsed.Code);
        builder.AppendLine("}");
        return builder.ToString();
    }

    internal static bool TryCreateCodeProjectionSegment(
        JazorVueDocument parsed,
        string projectedSource,
        [NotNullWhen(true)] out ProjectionSegment? segment)
    {
        if (parsed.CodeStartIndex < 0 || parsed.CodeLength <= 0 || string.IsNullOrWhiteSpace(parsed.Code))
        {
            segment = null;
            return false;
        }

        var projectedCodeStart = projectedSource.IndexOf(parsed.Code, StringComparison.Ordinal);
        if (projectedCodeStart < 0)
        {
            segment = null;
            return false;
        }

        segment = new ProjectionSegment(
            parsed.CodeStartIndex,
            parsed.Code.Length,
            projectedCodeStart,
            parsed.Code.Length);
        return true;
    }

    private SemanticModel CreatePlaceholderSemanticModel(SyntaxTree syntaxTree)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "__JoltRoslynPlaceholder",
            syntaxTrees: [syntaxTree],
            references: _metadataReferences,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        return compilation.GetSemanticModel(syntaxTree, ignoreAccessibility: true);
    }

    private CachedCompilationContext GetOrCreateCompilationContext(
        IReadOnlyList<ProjectedDocumentContext> projectedDocuments)
    {
        var cacheKey = CreateCompilationCacheKey(projectedDocuments);
        lock (_compilationCacheGate)
        {
            if (_compilationCache.TryGetValue(cacheKey, out var cachedContext))
            {
                cachedContext.LastUsedTick = ++_compilationCacheClock;
                return cachedContext;
            }

            var compilation = CSharpCompilation.Create(
                assemblyName: "__JoltRoslyn",
                syntaxTrees: projectedDocuments.Select(static projectedDocument => projectedDocument.SyntaxTree),
                references: _metadataReferences,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var projectedContexts = projectedDocuments
                .Select(projectedDocument => projectedDocument with
                {
                    SemanticModel = compilation.GetSemanticModel(projectedDocument.SyntaxTree, ignoreAccessibility: true)
                })
                .ToArray();
            var contextsByTree = projectedContexts.ToDictionary(
                static projectedDocument => projectedDocument.SyntaxTree,
                static projectedDocument => projectedDocument);
            var context = new CachedCompilationContext(
                compilation,
                projectedContexts,
                contextsByTree,
                ++_compilationCacheClock);
            _compilationCache[cacheKey] = context;
            TrimCompilationCache();
            return context;
        }
    }

    private void TrimCompilationCache()
    {
        while (_compilationCache.Count > MaxCompilationCacheEntries)
        {
            var oldestKey = _compilationCache
                .OrderBy(static entry => entry.Value.LastUsedTick)
                .Select(static entry => entry.Key)
                .FirstOrDefault();
            if (oldestKey is null)
            {
                return;
            }

            _compilationCache.Remove(oldestKey);
        }
    }

    private static string CreateCompilationCacheKey(
        IEnumerable<ProjectedDocumentContext> projectedDocuments)
    {
        var builder = new StringBuilder();
        foreach (var projectedDocument in projectedDocuments
                     .OrderBy(static projectedDocument => projectedDocument.SyntaxTree.FilePath, PathComparer))
        {
            builder.Append(projectedDocument.SyntaxTree.FilePath)
                .Append('\u001f')
                .Append(projectedDocument.ProjectedText.Length)
                .Append('\u001f')
                .Append(ComputeSha256(projectedDocument.ProjectedText))
                .Append('\u001e');
        }

        return builder.ToString();
    }

    private static string CreateContainerName(string documentPath)
        => ContainerNamesByPath.GetOrAdd(documentPath, static path =>
        {
        var fileName = Path.GetFileNameWithoutExtension(path);
        var sanitized = string.Concat((fileName ?? "Document").Select(character =>
            char.IsLetterOrDigit(character) || character == '_' ? character : '_'));
        if (string.IsNullOrWhiteSpace(sanitized) || !char.IsLetter(sanitized[0]) && sanitized[0] != '_')
            sanitized = "_" + sanitized;

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(path));
        var hash = Convert.ToHexString(bytes.AsSpan(0, 8));
        return "__JazorDocument_" + sanitized + "_" + hash;
        });

    private static string ComputeSha256(string text)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
}
