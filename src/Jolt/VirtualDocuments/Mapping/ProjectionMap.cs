using Jolt.Lsp;
using System.Diagnostics.CodeAnalysis;

namespace Jolt.VirtualDocuments.Mapping;

public sealed class ProjectionMap
{
    public ProjectionMap(
        string sourceDocumentPath,
        string projectedDocumentPath,
        IReadOnlyList<ProjectionSegment> segments)
    {
        SourceDocumentPath = sourceDocumentPath ?? throw new ArgumentNullException(nameof(sourceDocumentPath));
        ProjectedDocumentPath = projectedDocumentPath ?? throw new ArgumentNullException(nameof(projectedDocumentPath));
        Segments = ValidateSegments(segments);
    }

    public string SourceDocumentPath { get; }

    public string ProjectedDocumentPath { get; }

    public IReadOnlyList<ProjectionSegment> Segments { get; }

    internal bool TryMapToProjectedPosition(string sourceText, LspPosition sourcePosition, string projectedText, out LspPosition projectedPosition)
    {
        var sourceOffset = LspProtocolHelpers.GetOffset(sourceText, sourcePosition);
        if (!TryMapToProjectedOffsetCore(sourceOffset, preferSegmentEnd: sourceOffset == sourceText.Length, out var projectedOffset))
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
        var sourceStartOffset = LspProtocolHelpers.GetOffset(sourceText, sourceRange.Start);
        var sourceEndOffset = LspProtocolHelpers.GetOffset(sourceText, sourceRange.End);
        if (!TryMapToProjectedOffsetCore(sourceStartOffset, preferSegmentEnd: false, out var projectedStartOffset)
            || !TryMapToProjectedOffsetCore(sourceEndOffset, preferSegmentEnd: true, out var projectedEndOffset))
        {
            projectedRange = null;
            return false;
        }

        projectedRange = new LspRange
        {
            Start = LspProtocolHelpers.GetPosition(projectedText, projectedStartOffset),
            End = LspProtocolHelpers.GetPosition(projectedText, projectedEndOffset)
        };
        return true;
    }

    internal bool TryMapToOriginalPosition(string projectedText, LspPosition projectedPosition, string sourceText, out LspPosition originalPosition)
    {
        var projectedOffset = LspProtocolHelpers.GetOffset(projectedText, projectedPosition);
        if (!TryMapToOriginalOffsetCore(projectedOffset, preferSegmentEnd: projectedOffset == projectedText.Length, out var originalOffset))
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
        var projectedStartOffset = LspProtocolHelpers.GetOffset(projectedText, projectedRange.Start);
        var projectedEndOffset = LspProtocolHelpers.GetOffset(projectedText, projectedRange.End);
        if (!TryMapToOriginalOffsetCore(projectedStartOffset, preferSegmentEnd: false, out var originalStartOffset)
            || !TryMapToOriginalOffsetCore(projectedEndOffset, preferSegmentEnd: true, out var originalEndOffset))
        {
            originalRange = null;
            return false;
        }

        originalRange = new LspRange
        {
            Start = LspProtocolHelpers.GetPosition(sourceText, originalStartOffset),
            End = LspProtocolHelpers.GetPosition(sourceText, originalEndOffset)
        };
        return true;
    }

    public bool TryMapToProjectedOffset(int sourceOffset, out int projectedOffset)
        => TryMapToProjectedOffsetCore(sourceOffset, preferSegmentEnd: false, out projectedOffset);

    public bool TryMapToOriginalOffset(int projectedOffset, out int originalOffset)
        => TryMapToOriginalOffsetCore(projectedOffset, preferSegmentEnd: false, out originalOffset);

    private bool TryMapToProjectedOffsetCore(int sourceOffset, bool preferSegmentEnd, out int projectedOffset)
    {
        if (sourceOffset < 0)
        {
            projectedOffset = default;
            return false;
        }

        if (preferSegmentEnd && TryMapBoundaryToProjectedOffset(sourceOffset, preferSegmentEnd: true, out projectedOffset))
        {
            return true;
        }

        foreach (var segment in Segments)
        {
            if (!segment.IsBidirectional || !segment.ContainsOriginalOffset(sourceOffset))
            {
                continue;
            }

            projectedOffset = segment.ProjectedStart + Math.Min(sourceOffset - segment.OriginalStart, segment.ProjectedLength);
            return true;
        }

        if (!preferSegmentEnd && TryMapBoundaryToProjectedOffset(sourceOffset, preferSegmentEnd: false, out projectedOffset))
        {
            return true;
        }

        projectedOffset = default;
        return false;
    }

    private bool TryMapToOriginalOffsetCore(int projectedOffset, bool preferSegmentEnd, out int originalOffset)
    {
        if (projectedOffset < 0)
        {
            originalOffset = default;
            return false;
        }

        if (preferSegmentEnd && TryMapBoundaryToOriginalOffset(projectedOffset, preferSegmentEnd: true, out originalOffset))
        {
            return true;
        }

        foreach (var segment in Segments)
        {
            if (!segment.IsBidirectional || !segment.ContainsProjectedOffset(projectedOffset))
            {
                continue;
            }

            originalOffset = segment.OriginalStart + Math.Min(projectedOffset - segment.ProjectedStart, segment.OriginalLength);
            return true;
        }

        if (!preferSegmentEnd && TryMapBoundaryToOriginalOffset(projectedOffset, preferSegmentEnd: false, out originalOffset))
        {
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

    private bool TryMapBoundaryToProjectedOffset(int sourceOffset, bool preferSegmentEnd, out int projectedOffset)
    {
        if (preferSegmentEnd)
        {
            if (TryMapProjectedSegmentEnd(sourceOffset, out projectedOffset))
            {
                return true;
            }

            if (TryMapProjectedSegmentStart(sourceOffset, out projectedOffset))
            {
                return true;
            }

            projectedOffset = default;
            return false;
        }

        if (TryMapProjectedSegmentStart(sourceOffset, out projectedOffset))
        {
            return true;
        }

        if (TryMapProjectedSegmentEnd(sourceOffset, out projectedOffset))
        {
            return true;
        }

        projectedOffset = default;
        return false;
    }

    private bool TryMapBoundaryToOriginalOffset(int projectedOffset, bool preferSegmentEnd, out int originalOffset)
    {
        if (preferSegmentEnd)
        {
            if (TryMapOriginalSegmentEnd(projectedOffset, out originalOffset))
            {
                return true;
            }

            if (TryMapOriginalSegmentStart(projectedOffset, out originalOffset))
            {
                return true;
            }

            originalOffset = default;
            return false;
        }

        if (TryMapOriginalSegmentStart(projectedOffset, out originalOffset))
        {
            return true;
        }

        if (TryMapOriginalSegmentEnd(projectedOffset, out originalOffset))
        {
            return true;
        }

        originalOffset = default;
        return false;
    }

    private bool TryMapProjectedSegmentStart(int sourceOffset, out int projectedOffset)
    {
        foreach (var segment in Segments)
        {
            if (!segment.IsBidirectional)
            {
                continue;
            }

            if (segment.OriginalStart == sourceOffset)
            {
                projectedOffset = segment.ProjectedStart;
                return true;
            }
        }

        projectedOffset = default;
        return false;
    }

    private bool TryMapProjectedSegmentEnd(int sourceOffset, out int projectedOffset)
    {
        for (var index = Segments.Count - 1; index >= 0; index--)
        {
            var segment = Segments[index];
            if (!segment.IsBidirectional)
            {
                continue;
            }

            if (segment.OriginalEnd == sourceOffset)
            {
                projectedOffset = segment.ProjectedEnd;
                return true;
            }
        }

        projectedOffset = default;
        return false;
    }

    private bool TryMapOriginalSegmentStart(int projectedOffset, out int originalOffset)
    {
        foreach (var segment in Segments)
        {
            if (!segment.IsBidirectional)
            {
                continue;
            }

            if (segment.ProjectedStart == projectedOffset)
            {
                originalOffset = segment.OriginalStart;
                return true;
            }
        }

        originalOffset = default;
        return false;
    }

    private bool TryMapOriginalSegmentEnd(int projectedOffset, out int originalOffset)
    {
        for (var index = Segments.Count - 1; index >= 0; index--)
        {
            var segment = Segments[index];
            if (!segment.IsBidirectional)
            {
                continue;
            }

            if (segment.ProjectedEnd == projectedOffset)
            {
                originalOffset = segment.OriginalEnd;
                return true;
            }
        }

        originalOffset = default;
        return false;
    }

    private static IReadOnlyList<ProjectionSegment> ValidateSegments(IReadOnlyList<ProjectionSegment> segments)
    {
        ArgumentNullException.ThrowIfNull(segments);

        var copiedSegments = new ProjectionSegment[segments.Count];
        ProjectionSegment? previousSegment = null;
        for (var index = 0; index < segments.Count; index++)
        {
            var segment = segments[index] ?? throw new ArgumentException("Projection segments cannot contain null items.", nameof(segments));
            if (previousSegment is not null)
            {
                if (segment.OriginalStart < previousSegment.OriginalStart
                    || segment.OriginalStart < previousSegment.OriginalEnd)
                {
                    throw new ArgumentException("Projection segments must be sorted and non-overlapping in source order.", nameof(segments));
                }
            }

            copiedSegments[index] = segment;
            previousSegment = segment;
        }

        return copiedSegments;
    }

}
