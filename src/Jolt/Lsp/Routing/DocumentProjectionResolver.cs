using Jazor.VueContracts.Protocol;
using Jolt.VirtualDocuments.Models;
using Jolt.VirtualDocuments.Registry;
using Jolt.VirtualDocuments.Mapping;

namespace Jolt.Lsp.Routing;

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
            if (document.DocumentKind == DocumentKind.CSharp)
            {
                return new ProjectionTarget(
                    LaneKind.Roslyn,
                    DocumentRegionKind.Code,
                    document.DocumentPath,
                    document.DocumentPath,
                    position,
                    null,
                    IsProjected: false);
            }

            if (document.DocumentKind is DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript or DocumentKind.Css)
            {
                return new ProjectionTarget(
                    LaneKind.Volar,
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
        if (regionKind == DocumentRegionKind.Template)
        {
            var virtualDocuments = await _virtualDocumentRegistry.GetBySourceDocumentAsync(document.DocumentPath, cancellationToken);
            var projectedDocument = FindPrimaryVueProjection(
                document.DocumentPath,
                virtualDocuments);

            if (projectedDocument is not null)
            {
                var projectedPosition = TryMapPosition(projectedDocument.ProjectionMap, document.Text, position, projectedDocument.Text);
                return new ProjectionTarget(
                    LaneKind.Volar,
                    regionKind,
                    projectedDocument.Identity.ProjectedDocumentPath,
                    projectedDocument.Identity.SourceDocumentPath,
                    projectedPosition ?? position,
                    null,
                    IsProjected: projectedPosition is not null);
            }

            return new ProjectionTarget(
                LaneKind.Volar,
                regionKind,
                document.DocumentPath,
                document.DocumentPath,
                position,
                null,
                IsProjected: false);
        }

        if (regionKind == DocumentRegionKind.Code)
        {
            var virtualDocuments = await _virtualDocumentRegistry.GetBySourceDocumentAsync(document.DocumentPath, cancellationToken);
            var projectedDocument = virtualDocuments.FirstOrDefault(candidate =>
                candidate.Identity.DocumentKind == VirtualDocumentKind.CSharp);

            if (projectedDocument is not null)
            {
                var projectedPosition = TryMapPosition(projectedDocument.ProjectionMap, document.Text, position, projectedDocument.Text);
                return new ProjectionTarget(
                    LaneKind.Roslyn,
                    regionKind,
                    projectedDocument.Identity.ProjectedDocumentPath,
                    projectedDocument.Identity.SourceDocumentPath,
                    projectedPosition ?? position,
                    null,
                    IsProjected: projectedPosition is not null);
            }

            // Code-lane requests already execute against the source snapshot. Keep routing
            // them into Roslyn even if the virtual C# document has not been materialized yet.
            return new ProjectionTarget(
                LaneKind.Roslyn,
                regionKind,
                document.DocumentPath,
                document.DocumentPath,
                position,
                null,
                IsProjected: false);
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

    private static VirtualDocument? FindPrimaryVueProjection(
        string sourceDocumentPath,
        IReadOnlyList<VirtualDocument> virtualDocuments)
    {
        var expectedProjectedPath = NormalizePath("virtual:" + sourceDocumentPath + ".g.vue");
        return virtualDocuments.FirstOrDefault(candidate =>
            candidate.Identity.DocumentKind == VirtualDocumentKind.Vue
            && string.Equals(
                NormalizePath(candidate.Identity.ProjectedDocumentPath),
                expectedProjectedPath,
                StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizePath(string documentPath)
        => documentPath.Replace('\\', '/');
}
