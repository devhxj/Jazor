using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Lsp.Routing;

internal sealed record ProjectionTarget(
    LaneKind LaneKind,
    DocumentRegionKind RegionKind,
    string ProjectedDocumentPath,
    string MappingId,
    LspPosition? ProjectedPosition = null,
    LspRange? ProjectedRange = null,
    bool IsProjected = false);
