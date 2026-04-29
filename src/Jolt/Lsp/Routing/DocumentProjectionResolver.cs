using ECMAScript.Internal.VueContracts.Protocol;
using Jolt.VirtualDocuments.Models;
using Jolt.VirtualDocuments.Registry;

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
        var virtualDocuments = await _virtualDocumentRegistry.GetBySourceDocumentAsync(
            document.DocumentPath,
            cancellationToken);

        if (regionKind == DocumentRegionKind.Directive
            && IsModuleDirectivePosition(document.Text, offset))
        {
            return new ProjectionTarget(
                LaneKind.Jazor,
                regionKind,
                document.DocumentPath,
                document.DocumentPath,
                position,
                IsProjected: false);
        }

        if (regionKind != DocumentRegionKind.Template
            && TryResolveProjectedTarget(
                document.Text,
                position,
                regionKind,
                LaneKind.Roslyn,
                FindCSharpProjection(virtualDocuments),
                out var projectedCodeTarget))
        {
            return projectedCodeTarget;
        }

        if (regionKind == DocumentRegionKind.Template)
        {
            var projectedDocument = FindPrimaryVueProjection(
                document.DocumentPath,
                virtualDocuments);
            if (TryResolveProjectedTarget(
                    document.Text,
                    position,
                    regionKind,
                    LaneKind.Volar,
                    projectedDocument,
                    out var projectedTemplateTarget))
            {
                return projectedTemplateTarget;
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

        if (regionKind is DocumentRegionKind.Code or DocumentRegionKind.Directive)
        {
            // Standard Razor directives and code-lane requests should stay on Roslyn even when
            // the registry does not have an exact projection mapping yet. The Roslyn lane can
            // still build or fall back to its own projection from the source snapshot.
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

    private static bool TryResolveProjectedTarget(
        string sourceText,
        LspPosition sourcePosition,
        DocumentRegionKind regionKind,
        LaneKind laneKind,
        VirtualDocument? projectedDocument,
        out ProjectionTarget projectionTarget)
    {
        if (projectedDocument is null
            || !projectedDocument.ProjectionMap.TryMapToProjectedPosition(
                sourceText,
                sourcePosition,
                projectedDocument.Text,
                out var projectedPosition))
        {
            projectionTarget = default!;
            return false;
        }

        projectionTarget = new ProjectionTarget(
            laneKind,
            regionKind,
            projectedDocument.Identity.ProjectedDocumentPath,
            projectedDocument.Identity.SourceDocumentPath,
            projectedPosition,
            null,
            IsProjected: true);
        return true;
    }

    private static VirtualDocument? FindCSharpProjection(
        IReadOnlyList<VirtualDocument> virtualDocuments)
        => virtualDocuments.FirstOrDefault(candidate =>
            candidate.Identity.DocumentKind == VirtualDocumentKind.CSharp);

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

    private static bool IsModuleDirectivePosition(string text, int offset)
    {
        if (text.Length == 0)
        {
            return false;
        }

        var clampedOffset = Math.Max(0, Math.Min(offset, text.Length - 1));
        var lineStart = clampedOffset;
        while (lineStart > 0
               && text[lineStart - 1] != '\r'
               && text[lineStart - 1] != '\n')
        {
            lineStart--;
        }

        var lineEnd = clampedOffset;
        while (lineEnd < text.Length
               && text[lineEnd] != '\r'
               && text[lineEnd] != '\n')
        {
            lineEnd++;
        }

        var line = text.AsSpan(lineStart, lineEnd - lineStart).TrimStart();
        if (line.IsEmpty || line[0] != '@')
        {
            return false;
        }

        var directiveLength = 0;
        while (directiveLength < line.Length
               && !char.IsWhiteSpace(line[directiveLength]))
        {
            directiveLength++;
        }

        if (directiveLength == 0)
        {
            return false;
        }

        var directive = line[..directiveLength];
        var moduleDirective = "@module".AsSpan();
        return directive.Equals(moduleDirective, StringComparison.OrdinalIgnoreCase)
            || moduleDirective.StartsWith(directive, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string documentPath)
        => documentPath.Replace('\\', '/');
}
