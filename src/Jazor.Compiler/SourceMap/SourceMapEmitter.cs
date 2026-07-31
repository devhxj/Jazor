using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Acornima;
using Acornima.Ast;

namespace Jazor.Compiler;

/// <summary>
/// 将 Acornima AST 发射为 JavaScript 文本，并同步建立源码映射和内容 hash。
/// </summary>
/// <remarks>
/// SourceMapEmitter 只负责文本、映射和 artifact carrier，不负责模块语义或文件落盘。
/// 节点上的 SourceOrigin 是映射锚点；合成节点也必须有明确的来源策略，不能在这里猜测源位置。
/// </remarks>
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
        ValidateArtifactArguments(node, writerOptions, astOptions, generatedFileName);
        var output = WriteJavaScript(
            node,
            writerOptions,
            astOptions,
            captureSourceOrigins: true,
            captureNodePositions: false);
        return BuildArtifact(
            output.Content,
            generatedFileName,
            output.Collector.Captures,
            includeSourcesContent,
            sourceRootPath,
            readSourceContent);
    }

    internal static GeneratedJavaScriptLayout EmitWithNodePositions(
        Node node,
        JavaScriptTextWriterOptions writerOptions,
        AstToJavaScriptOptions astOptions,
        string generatedFileName,
        bool includeSourcesContent,
        string? sourceRootPath,
        Func<string, string?>? readSourceContent)
    {
        ValidateArtifactArguments(node, writerOptions, astOptions, generatedFileName);
        var output = WriteJavaScript(
            node,
            writerOptions,
            astOptions,
            captureSourceOrigins: true,
            captureNodePositions: true);
        return new GeneratedJavaScriptLayout(
            BuildArtifact(
                output.Content,
                generatedFileName,
                output.Collector.Captures,
                includeSourcesContent,
                sourceRootPath,
                readSourceContent),
            output.Collector.NodePositions);
    }

    internal static GeneratedJavaScriptNodeLayout EmitNodeLayout(
        Node node,
        JavaScriptTextWriterOptions writerOptions,
        AstToJavaScriptOptions astOptions)
    {
        ValidateWriterArguments(node, writerOptions, astOptions);
        var output = WriteJavaScript(
            node,
            writerOptions,
            astOptions,
            captureSourceOrigins: false,
            captureNodePositions: true);
        return new GeneratedJavaScriptNodeLayout(output.Content, output.Collector.NodePositions);
    }

    private static GeneratedJavaScriptArtifact BuildArtifact(
        string content,
        string generatedFileName,
        IReadOnlyList<CapturedSourceSegment> captures,
        bool includeSourcesContent,
        string? sourceRootPath,
        Func<string, string?>? readSourceContent)
    {
        var sourceMap = BuildSourceMap(
            generatedFileName,
            captures,
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

    private static JavaScriptWriteResult WriteJavaScript(
        Node node,
        JavaScriptTextWriterOptions writerOptions,
        AstToJavaScriptOptions astOptions,
        bool captureSourceOrigins,
        bool captureNodePositions)
    {
        var textWriter = new TrackingStringWriter();
        var collector = new SourceMapCaptureCollector(
            textWriter,
            captureSourceOrigins,
            captureNodePositions);
        var captureOptions = new SourceMapAstToJavaScriptOptions(astOptions, collector);

        AstToJavaScript.WriteJavaScript(node, textWriter, writerOptions, captureOptions);
        return new JavaScriptWriteResult(textWriter.ToString(), collector);
    }

    private static void ValidateArtifactArguments(
        Node node,
        JavaScriptTextWriterOptions writerOptions,
        AstToJavaScriptOptions astOptions,
        string generatedFileName)
    {
        ValidateWriterArguments(node, writerOptions, astOptions);
        if (string.IsNullOrWhiteSpace(generatedFileName))
            throw new ArgumentException("Generated file name cannot be null or whitespace.", nameof(generatedFileName));
    }

    private static void ValidateWriterArguments(
        Node node,
        JavaScriptTextWriterOptions writerOptions,
        AstToJavaScriptOptions astOptions)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node));
        if (writerOptions is null)
            throw new ArgumentNullException(nameof(writerOptions));
        if (astOptions is null)
            throw new ArgumentNullException(nameof(astOptions));
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

        // Captures are admitted only by SourceMapCaptureCollector, which excludes
        // synthetic origins and origins without a source path before this stage.
        for (var captureOrder = 0; captureOrder < captures.Count; captureOrder++)
        {
            var capture = captures[captureOrder];
            var origin = capture.Origin;
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
            // CaptureOrder is the unique index assigned while captures are collected.
            // Equal order therefore means the comparer received the same candidate;
            // no secondary source-coordinate fallback can ever affect ordering.
            return right.CaptureOrder.CompareTo(left.CaptureOrder);
        });

        var seenGeneratedPositions = new HashSet<(int GeneratedLine, int GeneratedColumn)>();
        var segments = new List<GeneratedSourceMapSegment>(candidateSegments.Count);

        foreach (var candidate in candidateSegments)
        {
            var sourceIndex = sourceIndexByPath[candidate.NormalizedPath];

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
            IsPathWithinRoot(fullPath, sourceRootPath!))
        {
            var relativePath = MakeRelativePath(sourceRootPath!, fullPath);
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

    private static string NormalizeAbsolutePath(string path)
        => path.Replace('\\', '/').Trim();

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

    private static string MakeRelativePath(string rootPath, string fullPath)
    {
        // NormalizeSourcePath calls this only after both paths are normalized and
        // IsPathWithinRoot has established the same-root containment contract.
        var rootUri = new Uri(AppendDirectorySeparator(rootPath));
        var fullUri = new Uri(fullPath);
        return Uri.UnescapeDataString(rootUri.MakeRelativeUri(fullUri).ToString())
            .Replace('/', Path.DirectorySeparatorChar);
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
        var bytes = Encoding.UTF8.GetBytes(value);
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

    private sealed record JavaScriptWriteResult(
        string Content,
        SourceMapCaptureCollector Collector);

    private sealed class SourceMapCaptureCollector
    {
        private readonly TrackingStringWriter _writer;
        private readonly List<CapturedSourceSegment>? _captures;
        private readonly Dictionary<Node, GeneratedNodePosition>? _nodePositions;
        private SourceOrigin? _currentOrigin;

        public SourceMapCaptureCollector(
            TrackingStringWriter writer,
            bool captureSourceOrigins,
            bool captureNodePositions)
        {
            _writer = writer;
            if (captureSourceOrigins)
                _captures = [];
            if (captureNodePositions)
                _nodePositions = [];
        }

        public IReadOnlyList<CapturedSourceSegment> Captures
            => _captures ?? throw new InvalidOperationException("Source-origin capture was not enabled for this writer.");

        public IReadOnlyDictionary<Node, GeneratedNodePosition> NodePositions
            => _nodePositions ?? throw new InvalidOperationException("Node-position capture was not enabled for this writer.");

        public SourceOrigin? Enter(Node node)
        {
            var previousOrigin = _currentOrigin;
            if (_nodePositions is not null &&
                !_nodePositions.ContainsKey(node))
            {
                _nodePositions.Add(node, new GeneratedNodePosition(_writer.Line, _writer.Column));
            }

            if (_captures is null)
                return previousOrigin;

            if (node.UserData is SourceOrigin nodeOrigin)
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
            if (origin is null ||
                string.IsNullOrWhiteSpace(origin.SourcePath))
            {
                return;
            }

            _captures!.Add(new CapturedSourceSegment(
                GeneratedLine: _writer.Line,
                GeneratedColumn: _writer.Column,
                Origin: origin));
        }
    }

    private sealed record SourceMapAstToJavaScriptOptions : AstToJavaScriptOptions
    {
        private readonly SourceMapCaptureCollector _collector;

        public SourceMapAstToJavaScriptOptions(AstToJavaScriptOptions original, SourceMapCaptureCollector collector)
            : base(original)
            => _collector = collector;

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
            => _collector = collector;

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

        public TrackingStringWriter()
            => NewLine = "\n";

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

/// <summary>
/// 带节点位置索引的 JavaScript 发射结果，供测试和诊断使用。
/// </summary>
/// <remarks>
/// NodePositions 是本轮 AST 节点到生成文本坐标的索引，不属于最终 SourceMap 公共格式，
/// 也不能替代 SourceOrigin 到源文件坐标的映射。
/// </remarks>
public sealed record GeneratedJavaScriptLayout(
    GeneratedJavaScriptArtifact Artifact,
    IReadOnlyDictionary<Node, GeneratedNodePosition> NodePositions);

/// <summary>
/// 内部文本 writer 输出及其 AST 节点位置索引。
/// </summary>
public sealed record GeneratedJavaScriptNodeLayout(
    string Content,
    IReadOnlyDictionary<Node, GeneratedNodePosition> NodePositions);

/// <summary>
/// 生成 JavaScript 文本中的零基行列坐标。
/// </summary>
public readonly record struct GeneratedNodePosition(int Line, int Column);
