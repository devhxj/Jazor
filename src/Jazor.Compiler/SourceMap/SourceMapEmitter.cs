using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Acornima;
using Acornima.Ast;

namespace Jazor.Compiler;

internal static class SourceMapEmitter
{
    public static GeneratedJavaScriptArtifact Emit(
        Node node,
        JavaScriptTextWriterOptions writerOptions,
        AstToJavaScriptOptions astOptions,
        string generatedFileName,
        bool includeSourcesContent,
        string? sourceRootPath,
        Func<string, string?>? readSourceContent)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node));
        if (writerOptions is null)
            throw new ArgumentNullException(nameof(writerOptions));
        if (astOptions is null)
            throw new ArgumentNullException(nameof(astOptions));
        if (string.IsNullOrWhiteSpace(generatedFileName))
            throw new ArgumentException("Generated file name cannot be null or whitespace.", nameof(generatedFileName));

        var textWriter = new TrackingStringWriter();
        var collector = new SourceMapCaptureCollector(textWriter);
        var sourceMapAstOptions = new SourceMapAstToJavaScriptOptions(astOptions, collector);

        AstToJavaScript.WriteJavaScript(node, textWriter, writerOptions, sourceMapAstOptions);
        var content = textWriter.ToString();

        var sourceMap = BuildSourceMap(
            generatedFileName,
            collector.Captures,
            includeSourcesContent,
            sourceRootPath,
            readSourceContent);
        var sourceMapContent = new GeneratedSourceMapWriter().Write(sourceMap);

        return new GeneratedJavaScriptArtifact(
            Content: content,
            SourceMapContent: sourceMapContent,
            JsHash: ComputeSha256Hex(content),
            MapHash: ComputeSha256Hex(sourceMapContent));
    }

    private static GeneratedSourceMap BuildSourceMap(
        string generatedFileName,
        IReadOnlyList<CapturedSourceSegment> captures,
        bool includeSourcesContent,
        string? sourceRootPath,
        Func<string, string?>? readSourceContent)
    {
        var normalizedRootPath = NormalizeRootPath(sourceRootPath);

        var sourceIndexByPath = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var sourceEntries = new List<(string NormalizedPath, string OriginalPath)>();
        var candidateSegments = new List<CandidateSegment>();

        for (var captureOrder = 0; captureOrder < captures.Count; captureOrder++)
        {
            var capture = captures[captureOrder];
            var origin = capture.Origin;
            if (origin.IsSynthetic || string.IsNullOrWhiteSpace(origin.SourcePath))
                continue;

            var normalizedPath = NormalizeSourcePath(origin.SourcePath!, normalizedRootPath);
            if (string.IsNullOrWhiteSpace(normalizedPath))
                continue;
            var normalizedSourcePath = normalizedPath!;

            if (!sourceIndexByPath.ContainsKey(normalizedSourcePath))
            {
                sourceIndexByPath.Add(normalizedSourcePath, sourceIndexByPath.Count);
                sourceEntries.Add((normalizedSourcePath, origin.SourcePath!));
            }

            candidateSegments.Add(new CandidateSegment(
                GeneratedLine: Math.Max(capture.GeneratedLine, 0),
                GeneratedColumn: Math.Max(capture.GeneratedColumn, 0),
                NormalizedPath: normalizedSourcePath,
                SourceLine: Math.Max(origin.StartLine, 0),
                SourceColumn: Math.Max(origin.StartColumn, 0),
                SourceEndLine: Math.Max(origin.EndLine, 0),
                SourceEndColumn: Math.Max(origin.EndColumn, 0),
                CaptureOrder: captureOrder));
        }

        sourceEntries.Sort(static (left, right) => StringComparer.OrdinalIgnoreCase.Compare(left.NormalizedPath, right.NormalizedPath));
        sourceIndexByPath.Clear();

        var sources = new List<GeneratedSourceMapSource>(sourceEntries.Count);
        for (var index = 0; index < sourceEntries.Count; index++)
        {
            var entry = sourceEntries[index];
            sourceIndexByPath.Add(entry.NormalizedPath, index);
            sources.Add(new GeneratedSourceMapSource(
                Path: entry.NormalizedPath,
                Content: includeSourcesContent && readSourceContent is not null
                    ? TryReadSourceContent(readSourceContent, entry.OriginalPath)
                    : null));
        }

        candidateSegments.Sort(static (left, right) =>
        {
            var line = left.GeneratedLine.CompareTo(right.GeneratedLine);
            if (line != 0)
                return line;

            var column = left.GeneratedColumn.CompareTo(right.GeneratedColumn);
            if (column != 0)
                return column;

            // Same generated position prefers innermost capture.
            var order = right.CaptureOrder.CompareTo(left.CaptureOrder);
            if (order != 0)
                return order;

            var sourceLine = left.SourceLine.CompareTo(right.SourceLine);
            if (sourceLine != 0)
                return sourceLine;

            var sourceColumn = left.SourceColumn.CompareTo(right.SourceColumn);
            if (sourceColumn != 0)
                return sourceColumn;

            var sourceEndLine = left.SourceEndLine.CompareTo(right.SourceEndLine);
            if (sourceEndLine != 0)
                return sourceEndLine;

            var sourceEndColumn = left.SourceEndColumn.CompareTo(right.SourceEndColumn);
            if (sourceEndColumn != 0)
                return sourceEndColumn;

            return StringComparer.OrdinalIgnoreCase.Compare(left.NormalizedPath, right.NormalizedPath);
        });

        var seenGeneratedPositions = new HashSet<(int GeneratedLine, int GeneratedColumn)>();
        var segments = new List<GeneratedSourceMapSegment>(candidateSegments.Count);

        foreach (var candidate in candidateSegments)
        {
            if (!sourceIndexByPath.TryGetValue(candidate.NormalizedPath, out var sourceIndex))
                continue;

            if (!seenGeneratedPositions.Add((candidate.GeneratedLine, candidate.GeneratedColumn)))
                continue;

            segments.Add(new GeneratedSourceMapSegment(
                GeneratedLine: candidate.GeneratedLine,
                GeneratedColumn: candidate.GeneratedColumn,
                SourceIndex: sourceIndex,
                SourceLine: candidate.SourceLine,
                SourceColumn: candidate.SourceColumn));
        }

        return new GeneratedSourceMap(generatedFileName, sources, segments);
    }

    private static string? NormalizeRootPath(string? rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
            return null;

        try
        {
            return Path.GetFullPath(rootPath);
        }
        catch
        {
            return null;
        }
    }

    private static string? NormalizeSourcePath(string sourcePath, string? sourceRootPath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            return null;

        if (Uri.TryCreate(sourcePath, UriKind.Absolute, out var sourceUri) && sourceUri.IsFile)
            sourcePath = sourceUri.LocalPath;

        if (!Path.IsPathRooted(sourcePath))
            return NormalizeRelativePath(sourcePath);

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(sourcePath);
        }
        catch
        {
            fullPath = sourcePath;
        }

        if (!string.IsNullOrWhiteSpace(sourceRootPath) &&
            IsPathWithinRoot(fullPath, sourceRootPath!) &&
            TryMakeRelativePath(sourceRootPath!, fullPath, out var relativePath))
        {
            if (!string.IsNullOrWhiteSpace(relativePath))
                return NormalizeRelativePath(relativePath);
        }

        return NormalizeAbsolutePath(fullPath);
    }

    private static string? NormalizeRelativePath(string path)
    {
        var normalized = path.Replace('\\', '/').Trim();
        if (normalized.Length == 0)
            return null;

        if (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized.Substring(2);

        normalized = normalized.TrimStart('/');
        return normalized.Length == 0 ? null : normalized;
    }

    private static string? NormalizeAbsolutePath(string path)
    {
        var normalized = path.Replace('\\', '/').Trim();
        return normalized.Length == 0 ? null : normalized;
    }

    private static bool IsPathWithinRoot(string path, string rootPath)
    {
        var normalizedPath = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        var normalizedRoot = rootPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        if (string.Equals(normalizedPath, normalizedRoot, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!normalizedRoot.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            normalizedRoot += Path.DirectorySeparatorChar;

        return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryMakeRelativePath(string rootPath, string fullPath, out string relativePath)
    {
        relativePath = string.Empty;
        try
        {
            var rootUri = new Uri(AppendDirectorySeparator(rootPath));
            var fullUri = new Uri(fullPath);
            if (!string.Equals(rootUri.Scheme, fullUri.Scheme, StringComparison.OrdinalIgnoreCase))
                return false;

            var relativeUri = rootUri.MakeRelativeUri(fullUri);
            if (relativeUri.IsAbsoluteUri)
                return false;

            var decoded = Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
            if (decoded.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
                string.Equals(decoded, "..", StringComparison.Ordinal))
            {
                return false;
            }

            relativePath = decoded;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string AppendDirectorySeparator(string path)
    {
        var normalized = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        if (!normalized.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            normalized += Path.DirectorySeparatorChar;

        return normalized;
    }

    private static string? TryReadSourceContent(Func<string, string?> readSourceContent, string sourcePath)
    {
        try
        {
            return readSourceContent(sourcePath);
        }
        catch
        {
            return null;
        }
    }

    private static string ComputeSha256Hex(string value)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        var hashBytes = sha.ComputeHash(bytes);
        var builder = new StringBuilder(hashBytes.Length * 2);
        foreach (var hashByte in hashBytes)
            builder.Append(hashByte.ToString("X2"));

        return builder.ToString();
    }

    private readonly record struct CapturedSourceSegment(
        int GeneratedLine,
        int GeneratedColumn,
        SourceOrigin Origin);

    private readonly record struct CandidateSegment(
        int GeneratedLine,
        int GeneratedColumn,
        string NormalizedPath,
        int SourceLine,
        int SourceColumn,
        int SourceEndLine,
        int SourceEndColumn,
        int CaptureOrder);

    private sealed class SourceMapCaptureCollector
    {
        private readonly TrackingStringWriter _writer;
        private readonly List<CapturedSourceSegment> _captures = [];
        private SourceOrigin? _currentOrigin;

        public SourceMapCaptureCollector(TrackingStringWriter writer)
            => _writer = writer ?? throw new ArgumentNullException(nameof(writer));

        public IReadOnlyList<CapturedSourceSegment> Captures => _captures;

        public SourceOrigin? Enter(Node? node)
        {
            var previousOrigin = _currentOrigin;
            if (node?.UserData is SourceOrigin nodeOrigin)
            {
                // Synthetic origins intentionally suppress mapping at this node,
                // but should not erase the inherited non-synthetic context.
                if (!nodeOrigin.IsSynthetic)
                {
                    _currentOrigin = nodeOrigin;
                    CaptureCurrent(nodeOrigin);
                }

                return previousOrigin;
            }

            CaptureCurrent(_currentOrigin);
            return previousOrigin;
        }

        public void Exit(SourceOrigin? previousOrigin)
            => _currentOrigin = previousOrigin;

        private void CaptureCurrent(SourceOrigin? origin)
        {
            if (origin is null || origin.IsSynthetic || string.IsNullOrWhiteSpace(origin.SourcePath))
                return;

            _captures.Add(new CapturedSourceSegment(
                GeneratedLine: _writer.Line,
                GeneratedColumn: _writer.Column,
                Origin: origin));
        }
    }

    private sealed record SourceMapAstToJavaScriptOptions : AstToJavaScriptOptions
    {
        private readonly SourceMapCaptureCollector _collector;

        public SourceMapAstToJavaScriptOptions(AstToJavaScriptOptions original, SourceMapCaptureCollector collector)
            : base(original ?? throw new ArgumentNullException(nameof(original)))
            => _collector = collector ?? throw new ArgumentNullException(nameof(collector));

        protected override AstToJavaScriptConverter CreateConverter(JavaScriptTextWriter writer)
            => new SourceMapAstToJavaScriptConverter(writer, this, _collector);
    }

    private sealed class SourceMapAstToJavaScriptConverter : AstToJavaScriptConverter
    {
        private readonly SourceMapCaptureCollector _collector;

        public SourceMapAstToJavaScriptConverter(
            JavaScriptTextWriter writer,
            AstToJavaScriptOptions options,
            SourceMapCaptureCollector collector)
            : base(writer, options)
            => _collector = collector ?? throw new ArgumentNullException(nameof(collector));

        public override object? Visit(Node node)
        {
            var previousOrigin = _collector.Enter(node);
            try
            {
                return base.Visit(node);
            }
            finally
            {
                _collector.Exit(previousOrigin);
            }
        }
    }

    private sealed class TrackingStringWriter : StringWriter
    {
        private bool _lastWasCarriageReturn;

        public int Line { get; private set; }

        public int Column { get; private set; }

        public override void Write(char value)
        {
            base.Write(value);
            Advance(value);
        }

        public override void Write(string? value)
        {
            if (value is null)
                return;

            base.Write(value);
            for (var index = 0; index < value.Length; index++)
                Advance(value[index]);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            base.Write(buffer, index, count);
            for (var i = index; i < index + count; i++)
                Advance(buffer[i]);
        }

        private void Advance(char ch)
        {
            switch (ch)
            {
                case '\r':
                    Line++;
                    Column = 0;
                    _lastWasCarriageReturn = true;
                    break;
                case '\n':
                    if (!_lastWasCarriageReturn)
                        Line++;

                    Column = 0;
                    _lastWasCarriageReturn = false;
                    break;
                default:
                    Column++;
                    _lastWasCarriageReturn = false;
                    break;
            }
        }
    }
}
