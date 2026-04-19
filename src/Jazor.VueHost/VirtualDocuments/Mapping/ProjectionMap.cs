using Jazor.VueHost.Lsp;
using System.Diagnostics.CodeAnalysis;

namespace Jazor.VueHost.VirtualDocuments.Mapping;

public sealed class ProjectionMap
{
    public ProjectionMap(
        string sourceDocumentPath,
        string projectedDocumentPath,
        IReadOnlyList<ProjectionSegment> segments)
    {
        SourceDocumentPath = sourceDocumentPath ?? throw new ArgumentNullException(nameof(sourceDocumentPath));
        ProjectedDocumentPath = projectedDocumentPath ?? throw new ArgumentNullException(nameof(projectedDocumentPath));
        Segments = segments ?? throw new ArgumentNullException(nameof(segments));
    }

    public string SourceDocumentPath { get; }

    public string ProjectedDocumentPath { get; }

    public IReadOnlyList<ProjectionSegment> Segments { get; }

    internal bool TryMapToProjectedPosition(string sourceText, LspPosition sourcePosition, string projectedText, out LspPosition projectedPosition)
    {
        var sourceOffset = LspProtocolHelpers.GetOffset(sourceText, sourcePosition);
        if (!TryMapToProjectedOffset(sourceOffset, out var projectedOffset))
        {
            projectedPosition = new LspPosition();
            return false;
        }

        projectedPosition = LspProtocolHelpers.GetPosition(projectedText, projectedOffset);
        return true;
    }

    internal bool TryMapToProjectedRange(
        string sourceText,
        LspRange sourceRange,
        string projectedText,
        [NotNullWhen(true)] out LspRange? projectedRange)
    {
        if (!TryMapToProjectedPosition(sourceText, sourceRange.Start, projectedText, out var projectedStart)
            || !TryMapToProjectedPosition(sourceText, sourceRange.End, projectedText, out var projectedEnd))
        {
            projectedRange = null;
            return false;
        }

        projectedRange = new LspRange
        {
            Start = projectedStart,
            End = projectedEnd
        };
        return true;
    }

    internal bool TryMapToOriginalPosition(string projectedText, LspPosition projectedPosition, string sourceText, out LspPosition originalPosition)
    {
        var projectedOffset = LspProtocolHelpers.GetOffset(projectedText, projectedPosition);
        if (!TryMapToOriginalOffset(projectedOffset, out var originalOffset))
        {
            originalPosition = new LspPosition();
            return false;
        }

        originalPosition = LspProtocolHelpers.GetPosition(sourceText, originalOffset);
        return true;
    }

    internal bool TryMapToOriginalRange(
        string projectedText,
        LspRange projectedRange,
        string sourceText,
        [NotNullWhen(true)] out LspRange? originalRange)
    {
        if (!TryMapToOriginalPosition(projectedText, projectedRange.Start, sourceText, out var originalStart)
            || !TryMapToOriginalPosition(projectedText, projectedRange.End, sourceText, out var originalEnd))
        {
            originalRange = null;
            return false;
        }

        originalRange = new LspRange
        {
            Start = originalStart,
            End = originalEnd
        };
        return true;
    }

    public bool TryMapToProjectedOffset(int sourceOffset, out int projectedOffset)
    {
        foreach (var segment in Segments)
        {
            if (!segment.IsBidirectional || !segment.ContainsOriginalOffset(sourceOffset))
            {
                continue;
            }

            projectedOffset = segment.ProjectedStart + Math.Min(sourceOffset - segment.OriginalStart, segment.ProjectedLength);
            return true;
        }

        projectedOffset = default;
        return false;
    }

    public bool TryMapToOriginalOffset(int projectedOffset, out int originalOffset)
    {
        foreach (var segment in Segments)
        {
            if (!segment.IsBidirectional || !segment.ContainsProjectedOffset(projectedOffset))
            {
                continue;
            }

            originalOffset = segment.OriginalStart + Math.Min(projectedOffset - segment.ProjectedStart, segment.OriginalLength);
            return true;
        }

        originalOffset = default;
        return false;
    }

    public static ProjectionMap CreateWholeDocument(
        string sourceDocumentPath,
        string projectedDocumentPath,
        int sourceLength,
        int projectedLength)
        => new(
            sourceDocumentPath,
            projectedDocumentPath,
            [
                new ProjectionSegment(0, sourceLength, 0, projectedLength)
            ]);

}
