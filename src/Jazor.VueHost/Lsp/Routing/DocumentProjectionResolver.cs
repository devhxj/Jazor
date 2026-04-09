using Jazor.VueContracts.Protocol;
using Jazor.VueHost.VirtualDocuments.Models;
using Jazor.VueHost.VirtualDocuments.Registry;
using Jazor.VueHost.VirtualDocuments.Mapping;

namespace Jazor.VueHost.Lsp.Routing;

internal sealed class DocumentProjectionResolver
{
    private readonly DocumentRegionClassifier _classifier;
    private readonly IVirtualDocumentRegistry _virtualDocumentRegistry;

    public DocumentProjectionResolver(
        DocumentRegionClassifier classifier,
        IVirtualDocumentRegistry virtualDocumentRegistry)
    {
        _classifier = classifier ?? throw new ArgumentNullException(nameof(classifier));
        _virtualDocumentRegistry = virtualDocumentRegistry ?? throw new ArgumentNullException(nameof(virtualDocumentRegistry));
    }

    public async ValueTask<ProjectionTarget> ResolveAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        cancellationToken.ThrowIfCancellationRequested();

        if (document.DocumentKind != DocumentKind.Jazor)
        {
            if (document.DocumentKind is DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript)
            {
                return new ProjectionTarget(
                    LaneKind.Frontend,
                    DocumentRegionKind.Unknown,
                    document.DocumentPath,
                    document.DocumentPath,
                    position,
                    null,
                    IsProjected: false);
            }

            return new ProjectionTarget(
                LaneKind.Jazor,
                DocumentRegionKind.Unknown,
                document.DocumentPath,
                document.DocumentPath,
                position,
                IsProjected: false);
        }

        var offset = LspProtocolHelpers.GetOffset(document.Text, position);
        var regionKind = _classifier.Classify(document.Text, offset);
        if (regionKind == DocumentRegionKind.Template || regionKind == DocumentRegionKind.Code)
        {
            var virtualDocuments = await _virtualDocumentRegistry.GetBySourceDocumentAsync(document.DocumentPath, cancellationToken);
            var projectedDocument = virtualDocuments.FirstOrDefault(candidate =>
                candidate.Identity.DocumentKind == (regionKind == DocumentRegionKind.Template
                    ? VirtualDocumentKind.Vue
                    : VirtualDocumentKind.CSharp));

            if (projectedDocument is not null)
            {
                var projectedPosition = TryMapPosition(projectedDocument.ProjectionMap, document.Text, position, projectedDocument.Text);
                return new ProjectionTarget(
                    regionKind == DocumentRegionKind.Template ? LaneKind.Frontend : LaneKind.Roslyn,
                    regionKind,
                    projectedDocument.Identity.ProjectedDocumentPath,
                    projectedDocument.Identity.SourceDocumentPath,
                    projectedPosition ?? position,
                    null,
                    IsProjected: projectedPosition is not null);
            }
        }

        return new ProjectionTarget(
            LaneKind.Jazor,
            regionKind,
            document.DocumentPath,
            document.DocumentPath,
            position,
            IsProjected: false);
    }

    private static LspPosition? TryMapPosition(
        ProjectionMap projectionMap,
        string sourceText,
        LspPosition sourcePosition,
        string projectedText)
    {
        var sourceOffset = LspProtocolHelpers.GetOffset(sourceText, sourcePosition);
        if (!projectionMap.TryMapToProjectedOffset(sourceOffset, out var projectedOffset))
        {
            return null;
        }

        return LspProtocolHelpers.GetPosition(projectedText, projectedOffset);
    }
}
