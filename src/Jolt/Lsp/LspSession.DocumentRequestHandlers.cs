using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text.Json;
using Jolt.Extensions;
using Jazor.VueContracts.Protocol;
using Jolt.Jazor.Projection;
using Jolt.Lsp.Aggregation;
using Jolt.Lsp.Coordination;
using Jolt.Lsp.Lanes;
using Jolt.Lsp.Routing;
using Jolt.VirtualDocuments.Registry;
using Jolt.Workspace;

namespace Jolt.Lsp;

internal sealed partial class LspSession
{
    private async ValueTask<LspResponseMessage> HandleHoverAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspHoverParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
        var projectionTarget = await _projectionResolver.ResolveAsync(document, parameters.Position, cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await CollectHoverAsync(document, parameters.Position, projectionTarget, cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleDocumentHighlightsAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspDocumentHighlightParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
        var projectionTarget = await _projectionResolver.ResolveAsync(document, parameters.Position, cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await CollectDocumentHighlightsAsync(document, parameters.Position, projectionTarget, cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleDocumentLinksAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspDocumentLinkParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await CollectDocumentLinksAsync(document, cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleCompletionAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspCompletionParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
        var projectionTarget = await _projectionResolver.ResolveAsync(document, parameters.Position, cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await CollectCompletionItemsAsync(document, parameters.Position, projectionTarget, cancellationToken));
    }

    private LspResponseMessage HandleCompletionItemResolve(LspRequestMessage request)
    {
        var item = DeserializeParams<LspCompletionItem>(request.Params);
        return CreateSuccessResponse(request.Id, item);
    }

    private async ValueTask<LspResponseMessage> HandleDocumentSymbolsAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspDocumentSymbolParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await CollectDocumentSymbolsAsync(document, cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleSemanticTokensAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspSemanticTokensParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
        var tokens = new List<LspSemanticToken>();

        foreach (var lane in GetSemanticTokenLanes(document))
        {
            var laneTokens = await lane.GetSemanticTokensAsync(document, cancellationToken);
            if (laneTokens.Count > 0)
            {
                tokens.AddRange(laneTokens);
            }
        }

        return CreateSuccessResponse(request.Id, _resultAggregator.AggregateSemanticTokens(tokens));
    }

    private async ValueTask<LspResponseMessage> HandleDefinitionAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspDefinitionParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
        var projectionTarget = await _projectionResolver.ResolveAsync(document, parameters.Position, cancellationToken);
        var locations = new List<LspLocation>();

        foreach (var lane in GetOrderedLanes(projectionTarget))
        {
            var laneLocations = await lane.GetDefinitionAsync(document, parameters.Position, projectionTarget, cancellationToken);
            if (laneLocations.Count > 0)
            {
                locations.AddRange(laneLocations);
            }
        }

        return CreateSuccessResponse(
            request.Id,
            await _markupBridgeFanoutCoordinator.CoordinateDefinitionAsync(
                document,
                parameters.Position,
                locations,
                allowMarkupFallback: !(document.DocumentKind == DocumentKind.Jazor
                    && projectionTarget.RegionKind == DocumentRegionKind.Template),
                cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleTypeDefinitionAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspTypeDefinitionParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
        var projectionTarget = await _projectionResolver.ResolveAsync(document, parameters.Position, cancellationToken);
        if (TryGetRoslynLaneService(out var roslynLane)
            && IsRoslynSemanticTarget(document, projectionTarget))
        {
            var typeLocations = await roslynLane.GetTypeDefinitionAsync(document, parameters.Position, cancellationToken);
            return CreateSuccessResponse(
                request.Id,
                _resultAggregator.AggregateLocations(typeLocations));
        }

        var locations = new List<LspLocation>();
        foreach (var lane in GetOrderedLanes(projectionTarget))
        {
            var laneLocations = await lane.GetDefinitionAsync(document, parameters.Position, projectionTarget, cancellationToken);
            if (laneLocations.Count > 0)
            {
                locations.AddRange(laneLocations);
            }
        }

        return CreateSuccessResponse(
            request.Id,
            _resultAggregator.AggregateLocations(locations));
    }

    private async ValueTask<LspResponseMessage> HandleImplementationAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspImplementationParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
        var projectionTarget = await _projectionResolver.ResolveAsync(document, parameters.Position, cancellationToken);
        var locations = new List<LspLocation>();

        foreach (var lane in GetOrderedLanes(projectionTarget))
        {
            var laneLocations = await lane.GetImplementationAsync(document, parameters.Position, projectionTarget, cancellationToken);
            if (laneLocations.Count > 0)
            {
                locations.AddRange(laneLocations);
            }
        }

        return CreateSuccessResponse(
            request.Id,
            _resultAggregator.AggregateLocations(locations));
    }

    private async ValueTask<LspResponseMessage> HandleSelectionRangeAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspSelectionRangeParams>(request.Params);
        var positions = parameters.Positions ?? [];
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);

        var results = new List<LspSelectionRange>(positions.Length);
        foreach (var position in positions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            results.Add(CreateSelectionRange(document.Text, position));
        }

        return CreateSuccessResponse(request.Id, results);
    }

    private async ValueTask<LspResponseMessage> HandleLinkedEditingRangeAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspLinkedEditingRangeParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
        var ranges = CollectLinkedEditingRanges(document.Text, parameters.Position);
        if (ranges.Count == 0)
        {
            return CreateSuccessResponse(request.Id, result: null);
        }

        return CreateSuccessResponse(
            request.Id,
            new LspLinkedEditingRanges
            {
                Ranges = ranges.ToArray(),
                WordPattern = @"[A-Za-z][A-Za-z0-9_\-:]*"
            });
    }

    private async ValueTask<LspResponseMessage> HandleDocumentFormattingAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspDocumentFormattingParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
        var formattedText = FormatText(document.Text, parameters.Options, ensureFinalNewline: false);
        if (string.Equals(formattedText, document.Text, StringComparison.Ordinal))
        {
            return CreateSuccessResponse(request.Id, Array.Empty<LspTextEdit>());
        }

        return CreateSuccessResponse(
            request.Id,
            new[]
            {
                new LspTextEdit
                {
                    Range = LspProtocolHelpers.ToRange(document.Text, 0, document.Text.Length),
                    NewText = formattedText
                }
            });
    }

    private async ValueTask<LspResponseMessage> HandleDocumentRangeFormattingAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspDocumentRangeFormattingParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
        var startOffset = LspProtocolHelpers.GetOffset(document.Text, parameters.Range.Start);
        var endOffset = LspProtocolHelpers.GetOffset(document.Text, parameters.Range.End);
        if (startOffset < 0 || endOffset < startOffset || startOffset > document.Text.Length)
        {
            return CreateSuccessResponse(request.Id, Array.Empty<LspTextEdit>());
        }

        endOffset = Math.Min(endOffset, document.Text.Length);
        var length = endOffset - startOffset;
        var originalText = document.Text.Substring(startOffset, length);
        var formattedText = FormatText(originalText, parameters.Options, ensureFinalNewline: false);
        if (string.Equals(formattedText, originalText, StringComparison.Ordinal))
        {
            return CreateSuccessResponse(request.Id, Array.Empty<LspTextEdit>());
        }

        return CreateSuccessResponse(
            request.Id,
            new[]
            {
                new LspTextEdit
                {
                    Range = parameters.Range,
                    NewText = formattedText
                }
            });
    }

    private async ValueTask<LspResponseMessage> HandleCodeLensAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspCodeLensParams>(request.Params);
        _ = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
        return CreateSuccessResponse(request.Id, Array.Empty<LspCodeLens>());
    }
}
