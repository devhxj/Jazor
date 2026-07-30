using System.Collections.Generic;
using System.Linq;

namespace Jazor.Common.SourceMaps;

/// <summary>
/// 将 bundle SourceMap 逐段追溯到模块 SourceMap，构造最终源文件映射。
/// </summary>
/// <remarks>
/// 链式合并只沿 bundle 中已有的源路径和模块映射查找，不创造不存在的源位置；路径查找会
/// 规范化斜杠和相对前缀。循环或无法解析的映射必须保持可诊断，而不是无限递归。
/// </remarks>
public sealed class SourceMapChainBuilder
{
    private static readonly SourceMapReader Reader = new();

    public SourceMapDocument Chain(SourceMapDocument bundleMap, IReadOnlyDictionary<string, SourceMapDocument> moduleMapsByPath)
    {
        if (bundleMap is null)
            throw new ArgumentNullException(nameof(bundleMap));
        if (moduleMapsByPath is null)
            throw new ArgumentNullException(nameof(moduleMapsByPath));

        var moduleMaps = NormalizeLookup(moduleMapsByPath);
        var sources = new List<SourceMapSource>();
        var sourceIndexByPath = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var segments = new List<SourceMapSegment>();

        foreach (var bundleSegment in bundleMap.Segments)
        {
            if (bundleSegment.SourceIndex < 0 || bundleSegment.SourceIndex >= bundleMap.Sources.Count)
                continue;

            var bundleSource = bundleMap.Sources[bundleSegment.SourceIndex];
            var resolved = ResolveSource(
                bundleSource.Path,
                bundleSource.Content,
                bundleSegment.SourceLine,
                bundleSegment.SourceColumn,
                moduleMaps,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            var sourceIndex = GetOrAddSourceIndex(sources, sourceIndexByPath, resolved.Path, resolved.Content);

            segments.Add(new SourceMapSegment(
                bundleSegment.GeneratedLine,
                bundleSegment.GeneratedColumn,
                sourceIndex,
                resolved.SourceLine,
                resolved.SourceColumn));
        }

        var orderedSegments = segments
            .GroupBy(static segment => (segment.GeneratedLine, segment.GeneratedColumn, segment.SourceIndex, segment.SourceLine, segment.SourceColumn))
            .Select(static group => group.First())
            .OrderBy(static segment => segment.GeneratedLine)
            .ThenBy(static segment => segment.GeneratedColumn)
            .ToArray();

        return new SourceMapDocument(bundleMap.File, sources, orderedSegments);
    }

    public SourceMapDocument Chain(string bundleMapJson, IReadOnlyDictionary<string, string> moduleMapJsonByPath)
    {
        if (string.IsNullOrWhiteSpace(bundleMapJson))
            throw new ArgumentException("Bundle source map json is required.", nameof(bundleMapJson));
        if (moduleMapJsonByPath is null)
            throw new ArgumentNullException(nameof(moduleMapJsonByPath));

        var bundleMap = Reader.Read(bundleMapJson);
        var moduleMaps = moduleMapJsonByPath.ToDictionary(
            static pair => pair.Key,
            static pair => Reader.Read(pair.Value),
            StringComparer.OrdinalIgnoreCase);

        return Chain(bundleMap, moduleMaps);
    }

    private static IReadOnlyDictionary<string, SourceMapDocument> NormalizeLookup(IReadOnlyDictionary<string, SourceMapDocument> moduleMapsByPath)
    {
        var lookup = new Dictionary<string, SourceMapDocument>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in moduleMapsByPath)
        {
            lookup[NormalizePath(pair.Key)] = pair.Value;
            if (!string.IsNullOrWhiteSpace(pair.Value.File))
                lookup[NormalizePath(pair.Value.File)] = pair.Value;
        }

        return lookup;
    }

    private static ResolvedSource ResolveSource(
        string path,
        string? content,
        int sourceLine,
        int sourceColumn,
        IReadOnlyDictionary<string, SourceMapDocument> moduleMapsByPath,
        HashSet<string> visited)
    {
        var normalizedPath = NormalizePath(path);
        if (!TryGetModuleMap(normalizedPath, moduleMapsByPath, out var moduleMap) || !visited.Add(normalizedPath))
            return new ResolvedSource(path, content, sourceLine, sourceColumn);

        try
        {
            var chainedSegment = FindLastSegmentAtOrBefore(moduleMap.Segments, sourceLine, sourceColumn);
            if (chainedSegment is null || chainedSegment.SourceIndex < 0 || chainedSegment.SourceIndex >= moduleMap.Sources.Count)
                return new ResolvedSource(path, content, sourceLine, sourceColumn);

            var moduleSource = moduleMap.Sources[chainedSegment.SourceIndex];
            return ResolveSource(
                moduleSource.Path,
                moduleSource.Content,
                chainedSegment.SourceLine,
                chainedSegment.SourceColumn,
                moduleMapsByPath,
                visited);
        }
        finally
        {
            visited.Remove(normalizedPath);
        }
    }

    private static bool TryGetModuleMap(string normalizedPath, IReadOnlyDictionary<string, SourceMapDocument> moduleMapsByPath, out SourceMapDocument moduleMap)
    {
        if (moduleMapsByPath.TryGetValue(normalizedPath, out moduleMap!))
            return true;

        foreach (var pair in moduleMapsByPath)
        {
            if (PathMatches(pair.Key, normalizedPath))
            {
                moduleMap = pair.Value;
                return true;
            }
        }

        moduleMap = null!;
        return false;
    }

    private static bool PathMatches(string left, string right)
    {
        if (string.Equals(left, right, StringComparison.OrdinalIgnoreCase))
            return true;
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            return false;

        return left.EndsWith("/" + right, StringComparison.OrdinalIgnoreCase)
            || right.EndsWith("/" + left, StringComparison.OrdinalIgnoreCase);
    }

    private static int GetOrAddSourceIndex(List<SourceMapSource> sources, Dictionary<string, int> sourceIndexByPath, string path, string? content)
    {
        var normalizedPath = NormalizePath(path);
        if (sourceIndexByPath.TryGetValue(normalizedPath, out var existingIndex))
        {
            if (sources[existingIndex].Content is null && content is not null)
                sources[existingIndex] = sources[existingIndex] with { Content = content };

            return existingIndex;
        }

        var index = sources.Count;
        sources.Add(new SourceMapSource(path, content));
        sourceIndexByPath[normalizedPath] = index;
        return index;
    }

    private static SourceMapSegment? FindLastSegmentAtOrBefore(IReadOnlyList<SourceMapSegment> segments, int generatedLine, int generatedColumn)
    {
        SourceMapSegment? candidate = null;
        foreach (var segment in segments)
        {
            if (segment.GeneratedLine > generatedLine)
                break;
            if (segment.GeneratedLine == generatedLine && segment.GeneratedColumn > generatedColumn)
                break;

            candidate = segment;
        }

        return candidate;
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var normalized = path.Trim();
        if (Uri.TryCreate(normalized, UriKind.Absolute, out var uri) && uri.IsFile)
            normalized = uri.LocalPath;

        normalized = normalized.Replace('\\', '/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized.Substring(2);

        return normalized;
    }

    private sealed record ResolvedSource(string Path, string? Content, int SourceLine, int SourceColumn);
}
