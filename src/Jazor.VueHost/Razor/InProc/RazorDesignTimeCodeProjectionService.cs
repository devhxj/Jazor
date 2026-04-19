using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Components;
using Jazor.Vue;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Razor.Toolset;
using Jazor.VueHost.VirtualDocuments.Mapping;

namespace Jazor.VueHost.Razor.InProc;

internal sealed class RazorDesignTimeCodeProjectionService
{
    private const string ProjectionNamespace = "Jazor.VueHost.RazorProjection";
    private readonly bool _requireSdkAlignedProjection;
    private readonly RazorSdkToolset? _resolvedToolset;

    public RazorDesignTimeCodeProjectionService(RazorSdkToolsetHost? toolsetHost = null)
    {
        _requireSdkAlignedProjection = toolsetHost is not null;
        _resolvedToolset = toolsetHost?.ResolveToolset();
    }

    public bool TryCreateProjection(
        DocumentSnapshot document,
        out RazorDesignTimeCodeProjection projection)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document.DocumentKind != DocumentKind.Jazor || string.IsNullOrWhiteSpace(document.Text))
        {
            projection = default;
            return false;
        }

        if (_requireSdkAlignedProjection && _resolvedToolset is null)
        {
            projection = default;
            return false;
        }

        try
        {
            var sourceDocument = RazorSourceDocument.Create(document.Text, document.DocumentPath);
            var projectEngine = CreateProjectEngine(document.DocumentPath);
            var codeDocument = projectEngine.ProcessDesignTime(
                sourceDocument,
                RazorFileKind.Component,
                ImmutableArray<RazorSourceDocument>.Empty,
                tagHelpers: null);
            if (!TryGetGeneratedCodeDocument(
                    codeDocument,
                    out var generatedCode,
                    out var sourceMappings)
                || string.IsNullOrWhiteSpace(generatedCode))
            {
                return TryCreateFallbackProjection(document, out projection);
            }

            var projectedDocumentPath = "virtual:" + document.DocumentPath + ".razor.g.cs";
            var projectionMap = CreateProjectionMap(document.DocumentPath, projectedDocumentPath, sourceMappings);
            if (projectionMap.Segments.Count == 0)
            {
                // Some design-time Razor toolchains produce generated source while omitting
                // granular source mappings. Prefer a code-block segment fallback so Roslyn
                // can still map @code operations accurately; only then degrade to whole-doc.
                if (!TryCreateCodeBlockFallbackProjectionMap(
                        document,
                        projectedDocumentPath,
                        generatedCode,
                        out projectionMap))
                {
                    projectionMap = ProjectionMap.CreateWholeDocument(
                        document.DocumentPath,
                        projectedDocumentPath,
                        document.Text.Length,
                        generatedCode.Length);
                }
            }

            projection = new RazorDesignTimeCodeProjection(
                projectedDocumentPath,
                generatedCode,
                projectionMap);
            return true;
        }
        catch (Exception) {
            return TryCreateFallbackProjection(document, out projection);
        }
    }

    private static RazorProjectEngine CreateProjectEngine(string documentPath)
    {
        var rootPath = Path.GetDirectoryName(documentPath);
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            rootPath = Directory.GetCurrentDirectory();
        }

        return RazorProjectEngine.Create(
            RazorConfiguration.Default,
            RazorProjectFileSystem.Create(rootPath),
            builder =>
            {
                // Keep the design-time component pipeline local to VueHost so Razor
                // can produce official source mappings without requiring another host.
                builder.SetRootNamespace(ProjectionNamespace);
                builder.SetSupportLocalizedComponentNames();

                ComponentCodeDirective.Register(builder);
            });
    }

    private static ProjectionMap CreateProjectionMap(
        string sourceDocumentPath,
        string projectedDocumentPath,
        IEnumerable<SourceMapping> sourceMappings)
    {
        var normalizedSourceDocumentPath = NormalizeComparablePath(sourceDocumentPath);
        var segments = sourceMappings
            .Select(static mapping => TryCreateSegment(mapping))
            .Where(static segment => segment is not null)
            .Select(static segment => segment!)
            .Where(segment => segment.OriginalLength > 0 && segment.ProjectedLength > 0)
            .Where(segment => string.IsNullOrWhiteSpace(segment.FilePath)
                || PathComparer.Equals(
                    NormalizeComparablePath(segment.FilePath),
                    normalizedSourceDocumentPath))
            .Select(static segment => new ProjectionSegment(
                segment.OriginalStart,
                segment.OriginalLength,
                segment.ProjectedStart,
                segment.ProjectedLength))
            .OrderBy(static segment => segment.OriginalStart)
            .ThenBy(static segment => segment.ProjectedStart)
            .ToArray();

        return new ProjectionMap(sourceDocumentPath, projectedDocumentPath, segments);
    }

    private static string NormalizeComparablePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        try
        {
            var fullPath = Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : path;
            return fullPath.Replace('\\', '/');
        }
        catch (Exception) {
            return path.Replace('\\', '/');
        }
    }

    private static bool TryCreateCodeBlockFallbackProjectionMap(
        DocumentSnapshot document,
        string projectedDocumentPath,
        string generatedCode,
        out ProjectionMap projectionMap)
    {
        var parsed = new JazorVueParser().Parse(document.DocumentPath, document.Text);
        if (parsed.CodeStartIndex < 0
            || parsed.CodeLength <= 0
            || string.IsNullOrWhiteSpace(parsed.Code))
        {
            projectionMap = default!;
            return false;
        }

        var projectedCodeStart = generatedCode.IndexOf(parsed.Code, StringComparison.Ordinal);
        if (projectedCodeStart < 0)
        {
            projectionMap = default!;
            return false;
        }

        projectionMap = new ProjectionMap(
            document.DocumentPath,
            projectedDocumentPath,
            [
                new ProjectionSegment(
                    parsed.CodeStartIndex,
                    parsed.Code.Length,
                    projectedCodeStart,
                    parsed.Code.Length)
            ]);
        return true;
    }

    private static bool TryCreateFallbackProjection(
        DocumentSnapshot document,
        out RazorDesignTimeCodeProjection projection)
    {
        var parsed = new JazorVueParser().Parse(document.DocumentPath, document.Text);
        if (parsed.CodeStartIndex < 0
            || parsed.CodeLength <= 0
            || string.IsNullOrWhiteSpace(parsed.Code))
        {
            projection = default;
            return false;
        }

        var projectedDocumentPath = "virtual:" + document.DocumentPath + ".razor.g.cs";
        var generatedCode = BuildFallbackProjectedSource(document.DocumentPath, parsed);
        if (!TryCreateCodeBlockFallbackProjectionMap(document, projectedDocumentPath, generatedCode, out var projectionMap))
        {
            projectionMap = ProjectionMap.CreateWholeDocument(
                document.DocumentPath,
                projectedDocumentPath,
                document.Text.Length,
                generatedCode.Length);
        }

        projection = new RazorDesignTimeCodeProjection(
            projectedDocumentPath,
            generatedCode,
            projectionMap);
        return true;
    }

    private static string BuildFallbackProjectedSource(string documentPath, JazorVueDocument parsed)
    {
        var builder = new StringBuilder();
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using System.Linq;");
        builder.AppendLine("using System.Threading.Tasks;");
        builder.AppendLine("#nullable enable");
        builder.Append("namespace ")
            .Append(ProjectionNamespace)
            .AppendLine(";");
        builder.Append("public partial class ")
            .Append(CreateFallbackContainerName(documentPath))
            .AppendLine();
        builder.AppendLine("{");
        builder.AppendLine(parsed.Code);
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string CreateFallbackContainerName(string documentPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(documentPath);
        var sanitized = string.Concat((fileName ?? "Document").Select(character =>
            char.IsLetterOrDigit(character) || character == '_' ? character : '_'));
        if (string.IsNullOrWhiteSpace(sanitized) || !char.IsLetter(sanitized[0]) && sanitized[0] != '_')
        {
            sanitized = "_" + sanitized;
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(documentPath));
        var hash = Convert.ToHexString(bytes.AsSpan(0, 4));
        return "__JazorDocument_" + sanitized + "_" + hash;
    }

    private static bool TryGetGeneratedCodeDocument(
        RazorCodeDocument codeDocument,
        out string generatedCode,
        out IReadOnlyList<SourceMapping> sourceMappings)
    {
        try
        {
            var csharpDocument = RazorCodeDocumentUnsafeAccessor.GetRequiredCSharpDocument(codeDocument);
            generatedCode = csharpDocument.Text.ToString();
            sourceMappings = GetSourceMappings(csharpDocument);
            return true;
        }
        catch (Exception) when (TryGetGeneratedCodeDocumentByUnsafeFallback(
                     codeDocument,
                     out generatedCode,
                     out sourceMappings))
        {
            return true;
        }
        catch (Exception) {
            generatedCode = string.Empty;
            sourceMappings = [];
            return false;
        }
    }

    private static bool TryGetGeneratedCodeDocumentByUnsafeFallback(
        RazorCodeDocument codeDocument,
        out string generatedCode,
        out IReadOnlyList<SourceMapping> sourceMappings)
    {
        try
        {
            var csharpDocument = RazorCodeDocumentUnsafeAccessor.GetCSharpDocument(codeDocument);
            if (csharpDocument is null)
            {
                generatedCode = string.Empty;
                sourceMappings = [];
                return false;
            }

            generatedCode = csharpDocument.Text.ToString();
            sourceMappings = GetSourceMappings(csharpDocument);
            return !string.IsNullOrWhiteSpace(generatedCode);
        }
        catch (Exception) {
            generatedCode = string.Empty;
            sourceMappings = [];
            return false;
        }
    }

    private static SourceMappingSegment? TryCreateSegment(SourceMapping mapping)
    {
        return new SourceMappingSegment(
            mapping.OriginalSpan.AbsoluteIndex,
            mapping.OriginalSpan.Length,
            mapping.GeneratedSpan.AbsoluteIndex,
            mapping.GeneratedSpan.Length,
            mapping.OriginalSpan.FilePath);
    }

    private static IReadOnlyList<SourceMapping> GetSourceMappings(RazorCSharpDocument csharpDocument)
    {
        var property = typeof(RazorCSharpDocument).GetProperty(
            "SourceMappings",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property?.GetValue(csharpDocument) is IEnumerable<SourceMapping> sourceMappings)
        {
            return sourceMappings.ToArray();
        }

        return [];
    }

    private static StringComparer PathComparer
        => OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    private sealed record SourceMappingSegment(
        int OriginalStart,
        int OriginalLength,
        int ProjectedStart,
        int ProjectedLength,
        string? FilePath);

    private static class RazorCodeDocumentUnsafeAccessor
    {
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetRequiredCSharpDocument")]
        internal static extern RazorCSharpDocument GetRequiredCSharpDocument(RazorCodeDocument codeDocument);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetCSharpDocument")]
        internal static extern RazorCSharpDocument? GetCSharpDocument(RazorCodeDocument codeDocument);
    }
}

internal readonly record struct RazorDesignTimeCodeProjection(
    string ProjectedDocumentPath,
    string SourceText,
    ProjectionMap ProjectionMap);
