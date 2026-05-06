using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Components;
using Jolt.Razor.Toolset;
using Jolt.VirtualDocuments.Mapping;
using Jazor.Vue;
using Jazor.RazorVue.Protocol;

namespace Jolt.Razor.InProc;

internal sealed class RazorDesignTimeCodeProjectionService
{
    private const string ProjectionNamespace = "Jolt.RazorProjection";
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
				[],
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
            var projectionMap = CreateProjectionMap(document.DocumentPath, document.Text, projectedDocumentPath, sourceMappings);
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
        catch (TargetInvocationException)
        {
            return TryCreateFallbackProjection(document, out projection);
        }
        catch (SystemException)
        {
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
                // Keep the design-time component pipeline local to Jolt so Razor
                // can produce official source mappings without requiring another host.
                builder.SetRootNamespace(ProjectionNamespace);
                builder.SetSupportLocalizedComponentNames();

                ComponentCodeDirective.Register(builder);
            });
    }

    private static ProjectionMap CreateProjectionMap(
        string sourceDocumentPath,
        string sourceText,
        string projectedDocumentPath,
        IEnumerable<SourceMapping> sourceMappings)
    {
        var normalizedSourceDocumentPath = NormalizeComparablePath(sourceDocumentPath);
        var excludedDirectiveRanges = GetExcludedDirectiveRanges(sourceText);
        var segments = sourceMappings
            .Select(static mapping => TryCreateSegment(mapping))
            .Where(static segment => segment is not null)
            .Select(static segment => segment!)
            .Where(segment => segment.OriginalLength > 0 && segment.ProjectedLength > 0)
            .Where(segment => IsRelevantMappingSegment(segment, normalizedSourceDocumentPath))
            // Keep Roslyn mapped to semantic/code regions. Top-level Jolt import directives are
            // handled by the Jazor lane and must not be reinterpreted as C#.
            .Where(segment => !OverlapsExcludedDirectiveRange(segment, excludedDirectiveRanges))
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

    private static (int Start, int End)[] GetExcludedDirectiveRanges(string sourceText)
    {
        if (string.IsNullOrWhiteSpace(sourceText))
        {
            return [];
        }

        return JazorImportDirectiveLocator.EnumerateDirectiveLines(sourceText)
            .Select(static match => (Start: match.LineStartIndex, End: match.LineStartIndex + match.LineLength))
            .Where(static range => range.End > range.Start)
            .ToArray();
    }

    private static bool OverlapsExcludedDirectiveRange(
        SourceMappingSegment segment,
        IReadOnlyList<(int Start, int End)> excludedDirectiveRanges)
    {
        foreach (var range in excludedDirectiveRanges)
        {
            if (segment.OriginalStart < range.End
                && segment.OriginalStart + segment.OriginalLength > range.Start)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsRelevantMappingSegment(
        SourceMappingSegment segment,
        string normalizedSourceDocumentPath)
        => string.IsNullOrWhiteSpace(segment.FilePath)
            || PathComparer.Equals(
                NormalizeComparablePath(segment.FilePath),
                normalizedSourceDocumentPath);

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
        catch (ArgumentException)
        {
            return path.Replace('\\', '/');
        }
        catch (NotSupportedException)
        {
            return path.Replace('\\', '/');
        }
        catch (PathTooLongException)
        {
            return path.Replace('\\', '/');
        }
        catch (IOException)
        {
            return path.Replace('\\', '/');
        }
    }

    private static bool TryCreateCodeBlockFallbackProjectionMap(
        DocumentSnapshot document,
        string projectedDocumentPath,
        string generatedCode,
        out ProjectionMap projectionMap)
    {
        var parsed = JazorVueParser.Parse(document.DocumentPath, document.Text);
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
        var parsed = JazorVueParser.Parse(document.DocumentPath, document.Text);
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
        catch (TargetInvocationException) when (TryGetGeneratedCodeDocumentByUnsafeFallback(
                     codeDocument,
                     out generatedCode,
                     out sourceMappings))
        {
            return true;
        }
        catch (SystemException) when (TryGetGeneratedCodeDocumentByUnsafeFallback(
                     codeDocument,
                     out generatedCode,
                     out sourceMappings))
        {
            return true;
        }
        catch (TargetInvocationException)
        {
            generatedCode = string.Empty;
            sourceMappings = [];
            return false;
        }
        catch (SystemException)
        {
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
        catch (TargetInvocationException)
        {
            generatedCode = string.Empty;
            sourceMappings = [];
            return false;
        }
        catch (SystemException)
        {
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

    private static SourceMapping[] GetSourceMappings(RazorCSharpDocument csharpDocument)
    {
        try
        {
            var property = typeof(RazorCSharpDocument).GetProperty(
                "SourceMappings",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property?.GetValue(csharpDocument) is IEnumerable<SourceMapping> sourceMappings)
            {
                return [.. sourceMappings];
            }
        }
        catch (ArgumentException ex)
        {
            WriteSourceMappingFallbackWarning(ex);
        }
        catch (TargetException ex)
        {
            WriteSourceMappingFallbackWarning(ex);
        }
        catch (TargetInvocationException ex)
        {
            WriteSourceMappingFallbackWarning(ex);
        }
        catch (MemberAccessException ex)
        {
            WriteSourceMappingFallbackWarning(ex);
        }
        catch (NotSupportedException ex)
        {
            WriteSourceMappingFallbackWarning(ex);
        }
        catch (SystemException ex)
        {
            WriteSourceMappingFallbackWarning(ex);
        }

        return [];
    }

    private static void WriteSourceMappingFallbackWarning(Exception exception)
    {
        try
        {
            Console.Error.WriteLine(
                $"[jolt][razor][warning] Falling back without Razor SourceMappings after {exception.GetType().Name}: {exception.Message}");
        }
        catch
        {
        }
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

