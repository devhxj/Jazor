namespace Jolt.VirtualDocuments.Mapping;

public sealed record ProjectionSegment
{
    public ProjectionSegment(
        int originalStart,
        int originalLength,
        int projectedStart,
        int projectedLength,
        bool isBidirectional = true)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(originalStart);
        ArgumentOutOfRangeException.ThrowIfNegative(originalLength);
        ArgumentOutOfRangeException.ThrowIfNegative(projectedStart);
        ArgumentOutOfRangeException.ThrowIfNegative(projectedLength);

        OriginalStart = originalStart;
        OriginalLength = originalLength;
        ProjectedStart = projectedStart;
        ProjectedLength = projectedLength;
        IsBidirectional = isBidirectional;
    }

    public int OriginalStart { get; }

    public int OriginalLength { get; }

    public int ProjectedStart { get; }

    public int ProjectedLength { get; }

    public bool IsBidirectional { get; }

    public int OriginalEnd => checked(OriginalStart + OriginalLength);

    public int ProjectedEnd => checked(ProjectedStart + ProjectedLength);

    public bool ContainsOriginalOffset(int offset)
        => offset >= OriginalStart && offset < OriginalEnd;

    public bool ContainsProjectedOffset(int offset)
        => offset >= ProjectedStart && offset < ProjectedEnd;
}
