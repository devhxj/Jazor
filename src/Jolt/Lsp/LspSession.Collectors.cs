using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text.Json;
using Jolt.Extensions;
using Jolt.Jazor.Projection;
using Jolt.Lsp.Aggregation;
using Jolt.Lsp.Coordination;
using Jolt.Lsp.Lanes;
using Jolt.Lsp.Routing;
using Jolt.VirtualDocuments.Registry;
using Jolt.Workspace;
using Jazor.Common.VueContracts.Protocol;

namespace Jolt.Lsp;

internal sealed partial class LspSession
{
    private async ValueTask<LspHoverResult?> CollectHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        LspHoverResult? hover = null;
        foreach (var lane in GetOrderedLanes(projectionTarget))
        {
            // Template requests still enter lanes with the source snapshot, but the
            // Volar lane now resolves the real projected `.g.vue` document from the
            // target metadata when that projection is available.
            var laneHover = await lane.GetHoverAsync(document, position, projectionTarget, cancellationToken);
            if (laneHover is not null)
            {
                hover = laneHover;
                break;
            }
        }

        foreach (var provider in _extensionRegistry.GetLspHoverProviders())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var invocation = await InvokeProviderAsync(
                capability: "hover",
                providerName: provider.Name,
                invocation: token => provider.ProvideHoverAsync(
                    new LspHoverProviderContext(
                        document,
                        position,
                        projectionTarget,
                        hover),
                    token),
                cancellationToken);
            if (invocation.TimedOut)
            {
                continue;
            }

            if (invocation.Result is not null)
            {
                hover = invocation.Result;
            }
        }

        return hover;
    }

    private async ValueTask<IReadOnlyList<LspDocumentHighlight>> CollectDocumentHighlightsAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        var highlights = new List<LspDocumentHighlight>();
        foreach (var lane in GetOrderedLanes(projectionTarget))
        {
            var laneHighlights = await lane.GetDocumentHighlightsAsync(document, position, projectionTarget, cancellationToken);
            if (laneHighlights.Count > 0)
            {
                highlights.AddRange(laneHighlights);
            }
        }

        if (highlights.Count == 0
            && document.DocumentKind == DocumentKind.Jazor
            && projectionTarget.LaneKind == LaneKind.Volar
            && _lanes.TryGetValue(LaneKind.Jazor, out var jazorLane))
        {
            var fallbackHighlights = await jazorLane.GetDocumentHighlightsAsync(
                document,
                position,
                projectionTarget,
                cancellationToken);
            if (fallbackHighlights.Count > 0)
            {
                highlights.AddRange(fallbackHighlights);
            }
        }

        return _resultAggregator.AggregateDocumentHighlights(highlights);
    }

    private async ValueTask<IReadOnlyList<LspDocumentLink>> CollectDocumentLinksAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        var links = new List<LspDocumentLink>();
        foreach (var lane in GetDocumentLinkLanes(document))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var laneLinks = await lane.GetDocumentLinksAsync(document, cancellationToken);
            if (laneLinks.Count > 0)
            {
                links.AddRange(laneLinks);
            }
        }

        return _resultAggregator.AggregateDocumentLinks(links);
    }

    private async ValueTask<IReadOnlyList<LspCompletionItem>> CollectCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        var items = new List<LspCompletionItem>();
        foreach (var lane in GetOrderedLanes(projectionTarget))
        {
            var laneItems = await lane.GetCompletionItemsAsync(document, position, projectionTarget, cancellationToken);
            if (laneItems.Count > 0)
            {
                items.AddRange(laneItems);
            }
        }

        foreach (var provider in _extensionRegistry.GetLspCompletionProviders())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var invocation = await InvokeProviderAsync(
                capability: "completion",
                providerName: provider.Name,
                invocation: token => provider.ProvideCompletionItemsAsync(
                    new LspCompletionProviderContext(
                        document,
                        position,
                        projectionTarget,
                        items),
                    token),
                cancellationToken);
            if (invocation.Result is { Count: > 0 } providedItems)
            {
                items.AddRange(providedItems);
            }
        }

        return _resultAggregator.AggregateCompletionItems(items);
    }

    private async ValueTask<IReadOnlyList<LspDocumentSymbol>> CollectDocumentSymbolsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        var symbols = new List<LspDocumentSymbol>();
        foreach (var lane in GetDocumentSymbolLanes(document))
        {
            var laneSymbols = await lane.GetDocumentSymbolsAsync(document, cancellationToken);
            if (laneSymbols.Count > 0)
            {
                symbols.AddRange(laneSymbols);
            }
        }

        foreach (var provider in _extensionRegistry.GetLspDocumentSymbolProviders())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var invocation = await InvokeProviderAsync(
                capability: "documentSymbol",
                providerName: provider.Name,
                invocation: token => provider.ProvideDocumentSymbolsAsync(
                    new LspDocumentSymbolProviderContext(document, symbols),
                    token),
                cancellationToken);
            if (invocation.Result is { Count: > 0 } providedSymbols)
            {
                symbols.AddRange(providedSymbols);
            }
        }

        return _resultAggregator.AggregateDocumentSymbols(symbols);
    }

    private async ValueTask<LspSignatureHelp?> CollectSignatureHelpAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        LspSignatureHelp? signatureHelp = null;
        foreach (var lane in GetOrderedLanes(projectionTarget))
        {
            var laneSignatureHelp = await lane.GetSignatureHelpAsync(document, position, projectionTarget, cancellationToken);
            if (laneSignatureHelp is not null)
            {
                signatureHelp = laneSignatureHelp;
                break;
            }
        }

        foreach (var provider in _extensionRegistry.GetLspSignatureHelpProviders())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var invocation = await InvokeProviderAsync(
                capability: "signatureHelp",
                providerName: provider.Name,
                invocation: token => provider.ProvideSignatureHelpAsync(
                    new LspSignatureHelpProviderContext(
                        document,
                        position,
                        projectionTarget,
                        signatureHelp),
                    token),
                cancellationToken);
            if (invocation.TimedOut)
            {
                continue;
            }

            if (invocation.Result is not null)
            {
                signatureHelp = invocation.Result;
            }
        }

        return signatureHelp;
    }

    private async ValueTask<IReadOnlyList<LspInlayHint>> CollectInlayHintsAsync(
        DocumentSnapshot document,
        LspRange range,
        CancellationToken cancellationToken)
    {
        var hints = new List<LspInlayHint>();
        foreach (var lane in GetInlayAndFoldingLanes(document))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var laneHints = await lane.GetInlayHintsAsync(document, range, cancellationToken);
            if (laneHints.Count > 0)
            {
                hints.AddRange(laneHints);
            }
        }

        foreach (var provider in _extensionRegistry.GetLspInlayHintProviders())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var invocation = await InvokeProviderAsync(
                capability: "inlayHint",
                providerName: provider.Name,
                invocation: token => provider.ProvideInlayHintsAsync(
                    new LspInlayHintProviderContext(document, range, hints),
                    token),
                cancellationToken);
            if (invocation.Result is { Count: > 0 } providedHints)
            {
                hints.AddRange(providedHints);
            }
        }

        return hints
            .GroupBy(static hint =>
                $"{hint.Position.Line}:{hint.Position.Character}:{hint.Label}:{hint.Kind}",
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    private async ValueTask<IReadOnlyList<LspWorkspaceSymbol>> CollectWorkspaceSymbolsAsync(
        string query,
        CancellationToken cancellationToken)
    {
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var workspaceFolders = GetWorkspaceFoldersSnapshot();
        var symbols = new List<LspWorkspaceSymbol>();
        foreach (var provider in _extensionRegistry.GetLspWorkspaceSymbolProviders())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var invocation = await InvokeProviderAsync(
                capability: "workspaceSymbol",
                providerName: provider.Name,
                invocation: token => provider.ProvideWorkspaceSymbolsAsync(
                    new LspWorkspaceSymbolProviderContext(query, openDocuments, symbols, workspaceFolders),
                    token),
                cancellationToken);
            if (invocation.Result is { Count: > 0 } providedSymbols)
            {
                symbols.AddRange(providedSymbols);
            }
        }

        return symbols
            .GroupBy(static symbol =>
                $"{symbol.Name}:{symbol.Location.Uri}:{symbol.Location.Range.Start.Line}:{symbol.Location.Range.Start.Character}",
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    private async ValueTask<IReadOnlyList<LspFoldingRange>> CollectFoldingRangesAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        var ranges = new List<LspFoldingRange>();
        foreach (var lane in GetInlayAndFoldingLanes(document))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var laneRanges = await lane.GetFoldingRangesAsync(document, cancellationToken);
            if (laneRanges.Count > 0)
            {
                ranges.AddRange(laneRanges);
            }
        }

        foreach (var provider in _extensionRegistry.GetLspFoldingRangeProviders())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var invocation = await InvokeProviderAsync(
                capability: "foldingRange",
                providerName: provider.Name,
                invocation: token => provider.ProvideFoldingRangesAsync(
                    new LspFoldingRangeProviderContext(document, ranges),
                    token),
                cancellationToken);
            if (invocation.Result is { Count: > 0 } providedRanges)
            {
                ranges.AddRange(providedRanges);
            }
        }

        return ranges
            .GroupBy(static range =>
                $"{range.StartLine}:{range.StartCharacter}:{range.EndLine}:{range.EndCharacter}:{range.Kind}",
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    private async ValueTask<IReadOnlyList<LspLocation>> CollectReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        var locations = (await _referenceCoordinator.CoordinateAsync(
            document,
            position,
            includeDeclaration,
            projectionTarget,
            cancellationToken))
            .ToList();

        foreach (var provider in _extensionRegistry.GetLspReferenceProviders())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var invocation = await InvokeProviderAsync(
                capability: "references",
                providerName: provider.Name,
                invocation: token => provider.ProvideReferencesAsync(
                    new LspReferenceProviderContext(
                        document,
                        position,
                        includeDeclaration,
                        projectionTarget,
                        locations),
                    token),
                cancellationToken);
            if (invocation.Result is { Count: > 0 } providedLocations)
            {
                locations.AddRange(providedLocations);
            }
        }

        return _resultAggregator.AggregateLocations(locations);
    }

    private async ValueTask<LspWorkspaceEdit?> CollectRenameEditAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        var edits = new List<LspWorkspaceEdit>();
        var laneEdit = await _renameCoordinator.CoordinateAsync(
            document,
            position,
            newName,
            projectionTarget,
            cancellationToken);
        if (laneEdit is not null)
        {
            edits.Add(laneEdit);
        }

        var mergedEdit = _resultAggregator.AggregateWorkspaceEdits(edits);
        foreach (var provider in _extensionRegistry.GetLspRenameProviders())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var invocation = await InvokeProviderAsync(
                capability: "rename",
                providerName: provider.Name,
                invocation: token => provider.ProvideRenameAsync(
                    new LspRenameProviderContext(
                        document,
                        position,
                        newName,
                        projectionTarget,
                        mergedEdit),
                    token),
                cancellationToken);
            if (invocation.Result is not null)
            {
                edits.Add(invocation.Result);
                mergedEdit = _resultAggregator.AggregateWorkspaceEdits(edits);
            }
        }

        return mergedEdit;
    }

    private async ValueTask<IReadOnlyList<LspDiagnostic>> CollectDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        var diagnostics = new List<LspDiagnostic>();
        foreach (var laneKind in _laneRouter.GetDiagnosticLanes(document))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_lanes.TryGetValue(laneKind, out var lane))
            {
                continue;
            }

            var laneDiagnostics = await lane.GetDiagnosticsAsync(document, cancellationToken);
            if (laneDiagnostics.Count > 0)
            {
                diagnostics.AddRange(laneDiagnostics);
            }
        }

        return await CollectExtensionDiagnosticsAsync(document, diagnostics, cancellationToken);
    }

    private async ValueTask<IReadOnlyList<LspCodeAction>> CollectCodeActionsAsync(
        DocumentSnapshot document,
        LspRange range,
        IReadOnlyList<LspDiagnostic> diagnostics,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        var actions = (await _codeActionCoordinator.CoordinateAsync(
            document,
            range,
            diagnostics,
            projectionTarget,
            cancellationToken))
            .ToList();

        foreach (var provider in _extensionRegistry.GetLspCodeActionProviders())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var invocation = await InvokeProviderAsync(
                capability: "codeAction",
                providerName: provider.Name,
                invocation: token => provider.ProvideCodeActionsAsync(
                    new LspCodeActionProviderContext(
                        document,
                        range,
                        diagnostics,
                        projectionTarget,
                        actions),
                    token),
                cancellationToken);
            if (invocation.Result is { Count: > 0 } providedActions)
            {
                actions.AddRange(providedActions);
            }
        }

        return _resultAggregator.AggregateCodeActions(actions);
    }

    private async ValueTask<IReadOnlyList<LspDiagnostic>> CollectExtensionDiagnosticsAsync(
        DocumentSnapshot document,
        IReadOnlyList<LspDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var merged = diagnostics.ToList();
        foreach (var provider in _extensionRegistry.GetLspDiagnosticProviders())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var invocation = await InvokeProviderAsync(
                capability: "diagnostic",
                providerName: provider.Name,
                invocation: token => provider.ProvideDiagnosticsAsync(
                    new LspDiagnosticProviderContext(document, merged),
                    token),
                cancellationToken);
            if (invocation.Result is { Count: > 0 } providedDiagnostics)
            {
                merged.AddRange(providedDiagnostics);
            }
        }

        return merged;
    }
}
