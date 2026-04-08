namespace Jazor.VueHost.Lsp.Routing;

internal sealed record ProjectionTarget(
    LaneKind LaneKind,
    DocumentRegionKind RegionKind,
    string ProjectedDocumentPath,
    string MappingId);
