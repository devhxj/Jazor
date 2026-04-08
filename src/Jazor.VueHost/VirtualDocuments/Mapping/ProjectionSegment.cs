namespace Jazor.VueHost.VirtualDocuments.Mapping;

public sealed record ProjectionSegment(
    int OriginalStart,
    int OriginalLength,
    int ProjectedStart,
    int ProjectedLength);
