namespace Jolt.VirtualDocuments.Mapping;

public sealed record ProjectionSegment(
    int OriginalStart,
    int OriginalLength,
    int ProjectedStart,
    int ProjectedLength,
    bool IsBidirectional = true)
{
    public int OriginalEnd => OriginalStart + OriginalLength;

    public int ProjectedEnd => ProjectedStart + ProjectedLength;

    public bool ContainsOriginalOffset(int offset)
        => offset >= OriginalStart && offset < OriginalEnd;

    public bool ContainsProjectedOffset(int offset)
        => offset >= ProjectedStart && offset < ProjectedEnd;
}
