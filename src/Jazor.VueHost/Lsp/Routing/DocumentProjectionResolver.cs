using Jazor.VueContracts.Protocol;
using Jazor.VueHost.VirtualDocuments.Models;
using Jazor.VueHost.VirtualDocuments.Registry;

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
                    document.DocumentPath);
            }

            return new ProjectionTarget(
                LaneKind.Jazor,
                DocumentRegionKind.Unknown,
                document.DocumentPath,
                document.DocumentPath);
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
                return new ProjectionTarget(
                    regionKind == DocumentRegionKind.Template ? LaneKind.Frontend : LaneKind.Roslyn,
                    regionKind,
                    projectedDocument.Identity.ProjectedDocumentPath,
                    projectedDocument.Identity.ProjectedDocumentPath);
            }
        }

        return new ProjectionTarget(
            LaneKind.Jazor,
            regionKind,
            document.DocumentPath,
            document.DocumentPath);
    }
}
