using Jazor.Vue;
using Jazor.VueContracts.Protocol;
using Jolt.Frontend;
using Jolt.Frontend.Deno.Hosting;
using Jolt.Lsp.Coordination;
using Jolt.Lsp.Routing;
using Jolt.VirtualDocuments.Mapping;
using Jolt.VirtualDocuments.Models;
using Jolt.VirtualDocuments.Registry;
using Jolt.Workspace;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace Jolt.Lsp.Lanes;

internal sealed class VolarLaneService : ILspLane
{
    private const string MissingTemplateImportDiagnosticCode = "JAZORVUEFRONTEND001";
    private const int DiagnosticSeverityWarning = 2;
    private const int MaxDenoFailureSnapshots = 64;
    private static readonly ConcurrentDictionary<string, DenoFailureSnapshot> DenoFailureSnapshots = new(StringComparer.OrdinalIgnoreCase);
    private static readonly AsyncLocal<ConcurrentDictionary<string, DenoFailureSnapshot>?> TestDenoFailureSnapshots = new();
    private readonly IFrontendContextProvider? _frontendContextProvider;
    private readonly IVirtualDocumentRegistry? _virtualDocumentRegistry;
    private readonly IDenoVolarHost? _denoVolarHost;
    private readonly MarkupComponentBridgeService _markupComponentBridge;

    public VolarLaneService(
        IJoltWorkspaceStore workspaceStore,
        IFrontendContextProvider? frontendContextProvider = null,
        IVirtualDocumentRegistry? virtualDocumentRegistry = null,
        IDenoVolarHost? denoVolarHost = null,
        MarkupComponentBridgeService? markupComponentBridge = null)
    {
        ArgumentNullException.ThrowIfNull(workspaceStore);
        _frontendContextProvider = frontendContextProvider;
        _virtualDocumentRegistry = virtualDocumentRegistry;
        _denoVolarHost = denoVolarHost;
        _markupComponentBridge = markupComponentBridge ?? new MarkupComponentBridgeService(workspaceStore);
    }

    public LaneKind LaneKind => LaneKind.Volar;

    internal static IReadOnlyList<DenoFailureSnapshot> GetDenoFailureSnapshots()
        => GetDenoFailureSnapshotStore().Values
            .OrderBy(static snapshot => snapshot.Operation, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    internal static void ResetDenoFailureSnapshotsForTests()
        => TestDenoFailureSnapshots.Value = new ConcurrentDictionary<string, DenoFailureSnapshot>(StringComparer.OrdinalIgnoreCase);

    public async ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        var diagnostics = new List<LspDiagnostic>();
        var frontendDocument = await ResolveFrontendDocumentAsync(document, projectionTarget: null, cancellationToken);
        if (document.DocumentKind != DocumentKind.Vue || frontendDocument.ProjectionMap is not null)
        {
            var frontendContext = await GetVolarIntelliSenseContextAsync(document, cancellationToken);
            var denoDiagnostics = await TryGetDenoDiagnosticsAsync(frontendDocument.RequestDocument, frontendContext, cancellationToken);
            diagnostics.AddRange(await FilterDenoDiagnosticsAsync(document, MapDiagnostics(document, frontendDocument, denoDiagnostics), cancellationToken));
        }

        if (CanUseWorkspaceGraph()
            && document.DocumentKind is DocumentKind.Jazor or DocumentKind.Vue)
        {
            diagnostics.AddRange(await CreateUnresolvedMarkupComponentDiagnosticsAsync(document, cancellationToken));
        }

        return diagnostics
            .GroupBy(static diagnostic =>
                $"{diagnostic.Code}:{diagnostic.Range.Start.Line}:{diagnostic.Range.Start.Character}:{diagnostic.Range.End.Line}:{diagnostic.Range.End.Character}",
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    public async ValueTask<LspHoverResult?> GetHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return null;
        }

        var frontendDocument = await ResolveFrontendDocumentAsync(document, projectionTarget, cancellationToken);
        var requestPosition = frontendDocument.MapPosition(position, projectionTarget.ProjectedPosition);
        if (CanUseWorkspaceGraph()
            && TryGetComponentTagNameAtPosition(document.Text, position, out _))
        {
            var bridgeHover = await _markupComponentBridge.GetHoverAsync(document, position, allowWorkspaceScan: true, cancellationToken);
            if (bridgeHover is not null)
            {
                return bridgeHover;
            }
        }

        var frontendContext = await GetVolarIntelliSenseContextAsync(document, cancellationToken);
        var denoResult = await TryGetDenoHoverAsync(frontendDocument.RequestDocument, requestPosition, frontendContext, cancellationToken);
        if (denoResult is not null)
        {
            return MapHover(document, frontendDocument, denoResult);
        }

        if (!CanUseWorkspaceGraph())
        {
            return null;
        }

        return await _markupComponentBridge.GetHoverAsync(document, position, allowWorkspaceScan: true, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return Array.Empty<LspCompletionItem>();
        }

        var frontendDocument = await ResolveFrontendDocumentAsync(document, projectionTarget, cancellationToken);
        var requestPosition = frontendDocument.MapPosition(position, projectionTarget.ProjectedPosition);
        var frontendContext = await GetVolarIntelliSenseContextAsync(document, cancellationToken);
        var items = new List<LspCompletionItem>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in await TryGetDenoCompletionItemsAsync(frontendDocument.RequestDocument, requestPosition, frontendContext, cancellationToken))
        {
            if (seen.Add($"{item.Label}|{item.Kind}|{item.Detail}"))
            {
                items.Add(item);
            }
        }

        if (CanUseWorkspaceGraph()
            && TryGetTagCompletionPrefix(document.Text, position, out var tagPrefix))
        {
            foreach (var component in await _markupComponentBridge.GetComponentSuggestionsAsync(
                         document.DocumentPath,
                         allowWorkspaceScan: document.DocumentKind == DocumentKind.Jazor,
                         cancellationToken))
            {
                if (!component.ComponentName.StartsWith(tagPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var item = new LspCompletionItem
                {
                    Label = component.ComponentName,
                    Kind = 7,
                    Detail = component.ImportPath,
                    Documentation = $"Vue component available in the workspace graph at `{component.ImportPath}`."
                };
                if (seen.Add($"{item.Label}|{item.Kind}|{item.Detail}"))
                {
                    items.Add(item);
                }
            }
        }

        return items;
    }

    public ValueTask<IReadOnlyList<LspDocumentSymbol>> GetDocumentSymbolsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
        => GetDocumentSymbolsCoreAsync(document, cancellationToken);

    public async ValueTask<IReadOnlyList<LspSemanticToken>> GetSemanticTokensAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        if (document.DocumentKind is not (DocumentKind.Jazor or DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript or DocumentKind.Css))
        {
            return Array.Empty<LspSemanticToken>();
        }

        var frontendDocument = await ResolveFrontendDocumentAsync(document, projectionTarget: null, cancellationToken);
        var frontendContext = await GetVolarIntelliSenseContextAsync(document, cancellationToken);
        var tokens = await TryGetDenoSemanticTokensAsync(frontendDocument.RequestDocument, frontendContext, cancellationToken);
        var mappedTokens = MapSemanticTokens(document, frontendDocument, tokens);
        if (mappedTokens.Count > 0)
        {
            return mappedTokens;
        }

        if (document.DocumentKind == DocumentKind.Jazor
            && frontendDocument.ProjectionMap is not null
            && CanUseWorkspaceGraph())
        {
            return CreateConservativeProjectedTemplateSemanticTokens(document);
        }

        return mappedTokens;
    }

    private async ValueTask<IReadOnlyList<LspDocumentSymbol>> GetDocumentSymbolsCoreAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var frontendDocument = await ResolveFrontendDocumentAsync(document, projectionTarget: null, cancellationToken);
        var frontendContext = await GetVolarIntelliSenseContextAsync(document, cancellationToken);
        var denoResult = await TryGetDenoDocumentSymbolsAsync(frontendDocument.RequestDocument, frontendContext, cancellationToken);
        if (denoResult is { Count: > 0 })
        {
            return MapDocumentSymbols(document, frontendDocument, denoResult);
        }

        return Array.Empty<LspDocumentSymbol>();
    }

    public ValueTask<LspSignatureHelp?> GetSignatureHelpAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<LspSignatureHelp?>(null);

    public async ValueTask<IReadOnlyList<LspDocumentLink>> GetDocumentLinksAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        if (document.DocumentKind is not (DocumentKind.Jazor or DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript or DocumentKind.Css))
        {
            return Array.Empty<LspDocumentLink>();
        }

        var frontendDocument = await ResolveFrontendDocumentAsync(document, projectionTarget: null, cancellationToken);
        var frontendContext = await GetVolarIntelliSenseContextAsync(document, cancellationToken);
        var links = await TryGetDenoDocumentLinksAsync(frontendDocument.RequestDocument, frontendContext, cancellationToken);
        return MapDocumentLinks(document, frontendDocument, links);
    }

    public async ValueTask<IReadOnlyList<LspInlayHint>> GetInlayHintsAsync(
        DocumentSnapshot document,
        LspRange range,
        CancellationToken cancellationToken)
    {
        if (document.DocumentKind is not (DocumentKind.Jazor or DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript or DocumentKind.Css))
        {
            return Array.Empty<LspInlayHint>();
        }

        var frontendDocument = await ResolveFrontendDocumentAsync(document, projectionTarget: null, cancellationToken);
        var requestRange = range;
        if (frontendDocument.ProjectionMap is not null
            && !frontendDocument.ProjectionMap.TryMapToProjectedRange(
                document.Text,
                range,
                frontendDocument.RequestDocument.Text,
                out requestRange))
        {
            return Array.Empty<LspInlayHint>();
        }

        var frontendContext = await GetVolarIntelliSenseContextAsync(document, cancellationToken);
        var hints = await TryGetDenoInlayHintsAsync(frontendDocument.RequestDocument, requestRange, frontendContext, cancellationToken);
        return MapInlayHints(document, frontendDocument, hints);
    }

    public async ValueTask<IReadOnlyList<LspFoldingRange>> GetFoldingRangesAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        if (document.DocumentKind is not (DocumentKind.Jazor or DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript or DocumentKind.Css))
        {
            return Array.Empty<LspFoldingRange>();
        }

        var frontendDocument = await ResolveFrontendDocumentAsync(document, projectionTarget: null, cancellationToken);
        var frontendContext = await GetVolarIntelliSenseContextAsync(document, cancellationToken);
        var ranges = await TryGetDenoFoldingRangesAsync(frontendDocument.RequestDocument, frontendContext, cancellationToken);
        return MapFoldingRanges(document, frontendDocument, ranges);
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return Array.Empty<LspLocation>();
        }

        var frontendDocument = await ResolveFrontendDocumentAsync(document, projectionTarget, cancellationToken);
        var requestPosition = frontendDocument.MapPosition(position, projectionTarget.ProjectedPosition);
        var frontendContext = await GetVolarIntelliSenseContextAsync(document, cancellationToken);
        var locations = new List<LspLocation>();
        var denoResult = await TryGetDenoDefinitionsAsync(frontendDocument.RequestDocument, requestPosition, frontendContext, cancellationToken);
        if (denoResult is { Count: > 0 })
        {
            locations.AddRange(MapLocations(document, frontendDocument, denoResult));
        }

        if (CanUseWorkspaceGraph()
            && document.DocumentKind == DocumentKind.Jazor
            && !ContainsLocationOutsideDocument(locations, document.DocumentPath))
        {
            var bridgeSymbol = await _markupComponentBridge.ResolveBridgeSymbolAsync(
                document,
                position,
                locations,
                allowWorkspaceScan: true,
                cancellationToken);
            if (bridgeSymbol is { } resolved)
            {
                if (LocationsOnlyTargetDocument(locations, document.DocumentPath))
                {
                    // Projected `.g.vue` definitions can legitimately map back onto the original
                    // `.jazor` tag usage. That round-trip is accurate but not useful to users, so
                    // prefer the resolved Vue declaration whenever the native result only echoes the
                    // current source document.
                    locations.Clear();
                }

                if (!ContainsLocationForDocument(locations, resolved.AbsolutePath))
                {
                    locations.Add(CreateDocumentStartLocation(resolved.AbsolutePath));
                }
            }
        }

        return locations.Count == 0
            ? Array.Empty<LspLocation>()
            : locations
                .GroupBy(static location =>
                    $"{location.Uri}:{location.Range.Start.Line}:{location.Range.Start.Character}:{location.Range.End.Line}:{location.Range.End.Character}",
                    StringComparer.Ordinal)
                .Select(static group => group.First())
                .ToArray();
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetImplementationAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return Array.Empty<LspLocation>();
        }

        var frontendDocument = await ResolveFrontendDocumentAsync(document, projectionTarget, cancellationToken);
        var requestPosition = frontendDocument.MapPosition(position, projectionTarget.ProjectedPosition);
        var frontendContext = await GetVolarIntelliSenseContextAsync(document, cancellationToken);
        var denoLocations = await TryGetDenoImplementationsAsync(
            frontendDocument.RequestDocument,
            requestPosition,
            frontendContext,
            cancellationToken);

        var locations = MapLocations(
            document,
            frontendDocument,
            denoLocations);
        return locations
            .GroupBy(static location =>
                $"{location.Uri}:{location.Range.Start.Line}:{location.Range.Start.Character}:{location.Range.End.Line}:{location.Range.End.Character}",
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    private static LspLocation CreateDocumentStartLocation(string documentPath)
        => new()
        {
            Uri = NormalizeFileUri(LspProtocolHelpers.ToDocumentUri(documentPath)),
            Range = new LspRange
            {
                Start = new LspPosition { Line = 0, Character = 0 },
                End = new LspPosition { Line = 0, Character = 0 }
            }
        };

    private static bool ContainsLocationForDocument(
        IReadOnlyList<LspLocation> locations,
        string documentPath)
    {
        if (locations.Count == 0)
        {
            return false;
        }

        var normalizedDocumentUri = NormalizeFileUri(LspProtocolHelpers.ToDocumentUri(documentPath));
        return locations.Any(location =>
            string.Equals(
                NormalizeFileUri(location.Uri),
                normalizedDocumentUri,
                StringComparison.Ordinal));
    }

    private static bool ContainsLocationOutsideDocument(
        IReadOnlyList<LspLocation> locations,
        string documentPath)
    {
        if (locations.Count == 0)
        {
            return false;
        }

        var normalizedDocumentUri = NormalizeFileUri(LspProtocolHelpers.ToDocumentUri(documentPath));
        return locations.Any(location =>
            !string.Equals(
                NormalizeFileUri(location.Uri),
                normalizedDocumentUri,
                StringComparison.Ordinal));
    }

    private static bool LocationsOnlyTargetDocument(
        IReadOnlyList<LspLocation> locations,
        string documentPath)
    {
        if (locations.Count == 0)
        {
            return false;
        }

        var normalizedDocumentUri = NormalizeFileUri(LspProtocolHelpers.ToDocumentUri(documentPath));
        return locations.All(location =>
            string.Equals(
                NormalizeFileUri(location.Uri),
                normalizedDocumentUri,
                StringComparison.Ordinal));
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return Array.Empty<LspLocation>();
        }

        var frontendDocument = await ResolveFrontendDocumentAsync(document, projectionTarget, cancellationToken);
        var requestPosition = frontendDocument.MapPosition(position, projectionTarget.ProjectedPosition);
        var frontendContext = await GetVolarIntelliSenseContextAsync(document, cancellationToken);
        var locations = new List<LspLocation>();
        var denoLocations = await TryGetDenoReferencesAsync(
            frontendDocument.RequestDocument,
            requestPosition,
            includeDeclaration,
            frontendContext,
            cancellationToken);
        locations.AddRange(MapLocations(
            document,
            frontendDocument,
            denoLocations));
        return locations
            .GroupBy(static location => $"{location.Uri}:{location.Range.Start.Line}:{location.Range.Start.Character}:{location.Range.End.Line}:{location.Range.End.Character}", StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    public async ValueTask<IReadOnlyList<LspDocumentHighlight>> GetDocumentHighlightsAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return Array.Empty<LspDocumentHighlight>();
        }

        var locations = await GetReferencesAsync(
            document,
            position,
            includeDeclaration: true,
            projectionTarget,
            cancellationToken);
        if (locations.Count == 0)
        {
            return Array.Empty<LspDocumentHighlight>();
        }

        return CreateDocumentHighlightsFromLocations(document, locations);
    }

    public async ValueTask<LspWorkspaceEdit?> GetRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return null;
        }

        var frontendDocument = await ResolveFrontendDocumentAsync(document, projectionTarget, cancellationToken);
        var requestPosition = frontendDocument.MapPosition(position, projectionTarget.ProjectedPosition);
        var frontendContext = await GetVolarIntelliSenseContextAsync(document, cancellationToken);
        var changes = new Dictionary<string, List<LspTextEdit>>(StringComparer.Ordinal);
        var denoResult = MapWorkspaceEdit(
            document,
            frontendDocument,
            await TryGetDenoRenameAsync(frontendDocument.RequestDocument, requestPosition, newName, frontendContext, cancellationToken));
        if (denoResult is not null)
        {
            foreach (var change in denoResult.Changes)
            {
                if (!changes.TryGetValue(change.Key, out var edits))
                {
                    edits = [];
                    changes.Add(change.Key, edits);
                }

                edits.AddRange(change.Value);
            }
        }

        if (changes.Count == 0)
        {
            return null;
        }

        return new LspWorkspaceEdit
        {
            Changes = changes.ToDictionary(
                static pair => pair.Key,
                static pair => pair.Value
                    .GroupBy(static edit => $"{edit.Range.Start.Line}:{edit.Range.Start.Character}:{edit.Range.End.Line}:{edit.Range.End.Character}:{edit.NewText}", StringComparer.Ordinal)
                    .Select(static group => group.First())
                    .OrderByDescending(static edit => edit.Range.Start.Line)
                    .ThenByDescending(static edit => edit.Range.Start.Character)
                    .ToArray(),
                StringComparer.Ordinal)
        };
    }

    public ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(
        DocumentSnapshot document,
        LspRange range,
        IReadOnlyList<LspDiagnostic> diagnostics,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(Array.Empty<LspCodeAction>());

    private static bool IsTemplateTarget(ProjectionTarget projectionTarget)
        => projectionTarget.LaneKind == LaneKind.Volar
            || projectionTarget.RegionKind == DocumentRegionKind.Template;

    private static bool TryGetTagCompletionPrefix(string text, LspPosition position, out string tagPrefix)
    {
        var offset = LspProtocolHelpers.GetOffset(text, position);
        var cursor = Math.Min(offset, text.Length);
        var nameStart = cursor;
        while (nameStart > 0 && IsTagNameCharacter(text[nameStart - 1]))
        {
            nameStart--;
        }

        var tagStart = nameStart;
        if (tagStart > 0 && text[tagStart - 1] == '/')
        {
            tagStart--;
        }

        if (tagStart <= 0 || text[tagStart - 1] != '<')
        {
            tagPrefix = string.Empty;
            return false;
        }

        tagPrefix = text[nameStart..cursor];
        return true;
    }

    private static bool IsTagNameCharacter(char character)
        => char.IsLetterOrDigit(character) || character == '_';

    private static bool TryGetComponentTagNameAtPosition(string text, LspPosition position, out string componentName)
    {
        var offset = LspProtocolHelpers.GetOffset(text, position);
        foreach (Match match in JazorMarkupPatterns.ComponentTagPattern.Matches(text))
        {
            var group = match.Groups["name"];
            if (!group.Success)
            {
                continue;
            }

            if (offset < group.Index || offset > group.Index + group.Length)
            {
                continue;
            }

            componentName = group.Value;
            return true;
        }

        componentName = string.Empty;
        return false;
    }

    private ValueTask<IReadOnlyList<LspCompletionItem>> TryGetDenoCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
        => ExecuteDenoRequestAsync(
            document,
            operation: "completion",
            fallbackValue: Array.Empty<LspCompletionItem>(),
            requestAsync: (host, token) => host.GetTemplateCompletionItemsAsync(document, position, context, token),
            cancellationToken);

    private ValueTask<LspHoverResult?> TryGetDenoHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
        => ExecuteDenoRequestAsync(
            document,
            operation: "hover",
            fallbackValue: null,
            requestAsync: (host, token) => host.GetTemplateHoverAsync(document, position, context, token),
            cancellationToken);

    private ValueTask<IReadOnlyList<LspLocation>> TryGetDenoDefinitionsAsync(
        DocumentSnapshot document,
        LspPosition position,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
        => ExecuteDenoRequestAsync(
            document,
            operation: "definition",
            fallbackValue: Array.Empty<LspLocation>(),
            requestAsync: (host, token) => host.GetTemplateDefinitionAsync(document, position, context, token),
            cancellationToken);

    private ValueTask<IReadOnlyList<LspLocation>> TryGetDenoImplementationsAsync(
        DocumentSnapshot document,
        LspPosition position,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
        => ExecuteDenoRequestAsync(
            document,
            operation: "implementation",
            fallbackValue: Array.Empty<LspLocation>(),
            requestAsync: (host, token) => host.GetTemplateImplementationAsync(document, position, context, token),
            cancellationToken);

    private ValueTask<IReadOnlyList<LspLocation>> TryGetDenoReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
        => ExecuteDenoRequestAsync(
            document,
            operation: "references",
            fallbackValue: Array.Empty<LspLocation>(),
            requestAsync: (host, token) => host.GetTemplateReferencesAsync(
                document,
                position,
                includeDeclaration,
                context,
                token),
            cancellationToken);

    private ValueTask<LspWorkspaceEdit?> TryGetDenoRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
        => ExecuteDenoRequestAsync(
            document,
            operation: "rename",
            fallbackValue: null,
            requestAsync: (host, token) => host.GetTemplateRenameAsync(document, position, newName, context, token),
            cancellationToken);

    private ValueTask<IReadOnlyList<LspDocumentSymbol>> TryGetDenoDocumentSymbolsAsync(
        DocumentSnapshot document,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
        => ExecuteDenoRequestAsync(
            document,
            operation: "documentSymbol",
            fallbackValue: Array.Empty<LspDocumentSymbol>(),
            requestAsync: (host, token) => host.GetTemplateDocumentSymbolsAsync(document, context, token),
            cancellationToken);

    private ValueTask<IReadOnlyList<LspDiagnostic>> TryGetDenoDiagnosticsAsync(
        DocumentSnapshot document,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
        => ExecuteDenoRequestAsync(
            document,
            operation: "diagnostics",
            fallbackValue: Array.Empty<LspDiagnostic>(),
            requestAsync: (host, token) => host.GetTemplateDiagnosticsAsync(document, context, token),
            cancellationToken);

    private ValueTask<IReadOnlyList<LspSemanticToken>> TryGetDenoSemanticTokensAsync(
        DocumentSnapshot document,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
        => ExecuteDenoRequestAsync(
            document,
            operation: "semanticTokens",
            fallbackValue: Array.Empty<LspSemanticToken>(),
            requestAsync: (host, token) => host.GetTemplateSemanticTokensAsync(document, context, token),
            cancellationToken);

    private ValueTask<IReadOnlyList<LspDocumentLink>> TryGetDenoDocumentLinksAsync(
        DocumentSnapshot document,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
        => ExecuteDenoRequestAsync(
            document,
            operation: "documentLink",
            fallbackValue: Array.Empty<LspDocumentLink>(),
            requestAsync: (host, token) => host.GetTemplateDocumentLinksAsync(document, context, token),
            cancellationToken);

    private ValueTask<IReadOnlyList<LspInlayHint>> TryGetDenoInlayHintsAsync(
        DocumentSnapshot document,
        LspRange range,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
        => ExecuteDenoRequestAsync(
            document,
            operation: "inlayHint",
            fallbackValue: Array.Empty<LspInlayHint>(),
            requestAsync: (host, token) => host.GetTemplateInlayHintsAsync(document, range, context, token),
            cancellationToken);

    private ValueTask<IReadOnlyList<LspFoldingRange>> TryGetDenoFoldingRangesAsync(
        DocumentSnapshot document,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
        => ExecuteDenoRequestAsync(
            document,
            operation: "foldingRange",
            fallbackValue: Array.Empty<LspFoldingRange>(),
            requestAsync: (host, token) => host.GetTemplateFoldingRangesAsync(document, context, token),
            cancellationToken);

    private ValueTask<DenoVolarIntelliSenseContext?> GetVolarIntelliSenseContextAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        if (document.DocumentKind != DocumentKind.Jazor || _frontendContextProvider is null)
        {
            return ValueTask.FromResult<DenoVolarIntelliSenseContext?>(null);
        }

        return ExecuteWithFailureCaptureAsync(
            operation: "frontendContext",
            documentPath: document.DocumentPath,
            fallbackValue: default(DenoVolarIntelliSenseContext?),
            operationAsync: async token =>
            {
                var response = await _frontendContextProvider.GetFrontendContextAsync(
                    new GetFrontendContextRequest(document.DocumentPath, Array.Empty<string>()),
                    token);
                return new DenoVolarIntelliSenseContext(response.SemanticContext, response.Artifacts);
            },
            cancellationToken);
    }

    private async ValueTask<T> ExecuteDenoRequestAsync<T>(
        DocumentSnapshot document,
        string operation,
        T fallbackValue,
        Func<IDenoVolarHost, CancellationToken, ValueTask<T>> requestAsync,
        CancellationToken cancellationToken)
    {
        var denoHost = _denoVolarHost;
        if (denoHost is null)
        {
            return fallbackValue;
        }

        return await ExecuteWithFailureCaptureAsync(
            operation,
            document.DocumentPath,
            fallbackValue,
            token => requestAsync(denoHost, token),
            cancellationToken);
    }

    private async ValueTask<T> ExecuteWithFailureCaptureAsync<T>(
        string operation,
        string documentPath,
        T fallbackValue,
        Func<CancellationToken, ValueTask<T>> operationAsync,
        CancellationToken cancellationToken)
    {
        try
        {
            return await operationAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            RecordDenoFailure(operation, documentPath, exception);
            return fallbackValue;
        }
    }

    private async ValueTask<VolarRequestDocument> ResolveFrontendDocumentAsync(
        DocumentSnapshot sourceDocument,
        ProjectionTarget? projectionTarget,
        CancellationToken cancellationToken)
    {
        if (_virtualDocumentRegistry is not null)
        {
            VirtualDocument? projectedDocument = null;
            if (projectionTarget is not null
                && !string.IsNullOrWhiteSpace(projectionTarget.ProjectedDocumentPath))
            {
                projectedDocument = await _virtualDocumentRegistry.GetByProjectedDocumentAsync(
                    projectionTarget.ProjectedDocumentPath,
                    cancellationToken);
            }

            if (projectedDocument is null && sourceDocument.DocumentKind == DocumentKind.Jazor)
            {
                var virtualDocuments = await _virtualDocumentRegistry.GetBySourceDocumentAsync(
                    sourceDocument.DocumentPath,
                    cancellationToken);
                projectedDocument = FindPrimaryVueProjection(
                    sourceDocument.DocumentPath,
                    projectionTarget?.ProjectedDocumentPath,
                    virtualDocuments);
            }

            if (projectedDocument is not null)
            {
                return new VolarRequestDocument(
                    new DocumentSnapshot(
                        projectedDocument.Identity.ProjectedDocumentPath,
                        MapProjectedDocumentKind(projectedDocument.Identity.DocumentKind),
                        projectedDocument.Text,
                        projectedDocument.Version),
                    projectedDocument.ProjectionMap);
            }
        }

        return new VolarRequestDocument(sourceDocument, ProjectionMap: null);
    }

    private static VirtualDocument? FindPrimaryVueProjection(
        string sourceDocumentPath,
        string? preferredProjectedPath,
        IReadOnlyList<VirtualDocument> virtualDocuments)
    {
        if (!string.IsNullOrWhiteSpace(preferredProjectedPath))
        {
            var preferredDocument = virtualDocuments.FirstOrDefault(candidate =>
                candidate.Identity.DocumentKind == VirtualDocumentKind.Vue
                && string.Equals(
                    NormalizePath(candidate.Identity.ProjectedDocumentPath),
                    NormalizePath(preferredProjectedPath),
                    StringComparison.OrdinalIgnoreCase));
            if (preferredDocument is not null)
            {
                return preferredDocument;
            }
        }

        var expectedProjectedPath = NormalizePath("virtual:" + sourceDocumentPath + ".g.vue");
        return virtualDocuments.FirstOrDefault(candidate =>
            candidate.Identity.DocumentKind == VirtualDocumentKind.Vue
            && string.Equals(
                NormalizePath(candidate.Identity.ProjectedDocumentPath),
                expectedProjectedPath,
                StringComparison.OrdinalIgnoreCase));
    }

    private static DocumentKind MapProjectedDocumentKind(VirtualDocumentKind documentKind)
        => documentKind switch
        {
            VirtualDocumentKind.Vue => DocumentKind.Vue,
            VirtualDocumentKind.CSharp => DocumentKind.Unknown,
            _ => DocumentKind.Unknown
        };

    private static IReadOnlyList<LspDiagnostic> MapDiagnostics(
        DocumentSnapshot sourceDocument,
        VolarRequestDocument requestDocument,
        IReadOnlyList<LspDiagnostic> diagnostics)
    {
        if (requestDocument.ProjectionMap is null)
        {
            return diagnostics;
        }

        return diagnostics
            .Select(diagnostic =>
            {
                if (!requestDocument.ProjectionMap.TryMapToOriginalRange(
                        requestDocument.RequestDocument.Text,
                        diagnostic.Range,
                        sourceDocument.Text,
                        out var sourceRange))
                {
                    return null;
                }

                return new LspDiagnostic
                {
                    Range = sourceRange,
                    Severity = diagnostic.Severity,
                    Code = diagnostic.Code,
                    Source = diagnostic.Source,
                    Message = diagnostic.Message
                };
            })
            .Where(static diagnostic => diagnostic is not null)
            .Cast<LspDiagnostic>()
            .ToArray();
    }

    private static LspHoverResult? MapHover(
        DocumentSnapshot sourceDocument,
        VolarRequestDocument requestDocument,
        LspHoverResult? hover)
    {
        if (hover is null || requestDocument.ProjectionMap is null || hover.Range is null)
        {
            return hover;
        }

        if (!requestDocument.ProjectionMap.TryMapToOriginalRange(
                requestDocument.RequestDocument.Text,
                hover.Range,
                sourceDocument.Text,
                out var sourceRange))
        {
            return new LspHoverResult
            {
                Contents = hover.Contents,
                Range = null
            };
        }

        return new LspHoverResult
        {
            Contents = hover.Contents,
            Range = sourceRange
        };
    }

    private static IReadOnlyList<LspLocation> MapLocations(
        DocumentSnapshot sourceDocument,
        VolarRequestDocument requestDocument,
        IReadOnlyList<LspLocation> locations)
    {
        if (requestDocument.ProjectionMap is null)
        {
            return NormalizeLocationUris(locations);
        }

        var projectedUri = LspProtocolHelpers.ToDocumentUri(requestDocument.RequestDocument.DocumentPath);
        var projectedPath = NormalizePath(requestDocument.RequestDocument.DocumentPath);
        return NormalizeLocationUris(locations
            .Select(location =>
            {
                if (!LocationTargetsProjectedDocument(location.Uri, projectedUri, projectedPath))
                {
                    return location;
                }

                if (!requestDocument.ProjectionMap.TryMapToOriginalRange(
                        requestDocument.RequestDocument.Text,
                        location.Range,
                        sourceDocument.Text,
                        out var sourceRange))
                {
                    return null;
                }

                return new LspLocation
                {
                    Uri = LspProtocolHelpers.ToDocumentUri(sourceDocument.DocumentPath),
                    Range = sourceRange
                };
            })
            .Where(static location => location is not null)
            .Cast<LspLocation>()
            .ToArray());
    }

    private static LspWorkspaceEdit? MapWorkspaceEdit(
        DocumentSnapshot sourceDocument,
        VolarRequestDocument requestDocument,
        LspWorkspaceEdit? workspaceEdit)
    {
        if (workspaceEdit is null || requestDocument.ProjectionMap is null)
        {
            return workspaceEdit;
        }

        var projectedUri = NormalizeFileUri(LspProtocolHelpers.ToDocumentUri(requestDocument.RequestDocument.DocumentPath));
        var sourceUri = NormalizeFileUri(LspProtocolHelpers.ToDocumentUri(sourceDocument.DocumentPath));
        var mappedChanges = new Dictionary<string, LspTextEdit[]>(StringComparer.Ordinal);
        foreach (var change in workspaceEdit.Changes)
        {
            var normalizedKey = NormalizeFileUri(change.Key);
            if (!string.Equals(normalizedKey, projectedUri, StringComparison.Ordinal))
            {
                mappedChanges[normalizedKey] = change.Value;
                continue;
            }

            var edits = change.Value
                .Select(edit =>
                {
                    if (!requestDocument.ProjectionMap.TryMapToOriginalRange(
                            requestDocument.RequestDocument.Text,
                            edit.Range,
                            sourceDocument.Text,
                            out var sourceRange))
                    {
                        return null;
                    }

                    return new LspTextEdit
                    {
                        Range = sourceRange,
                        NewText = edit.NewText
                    };
                })
                .Where(static edit => edit is not null)
                .Cast<LspTextEdit>()
                .ToArray();

            if (edits.Length > 0)
            {
                mappedChanges[sourceUri] = edits;
            }
        }

        return mappedChanges.Count == 0
            ? null
            : new LspWorkspaceEdit
            {
                Changes = mappedChanges
            };
    }

    private static IReadOnlyList<LspDocumentSymbol> MapDocumentSymbols(
        DocumentSnapshot sourceDocument,
        VolarRequestDocument requestDocument,
        IReadOnlyList<LspDocumentSymbol> symbols)
    {
        if (requestDocument.ProjectionMap is null)
        {
            return symbols;
        }

        return symbols
            .Select(symbol => MapDocumentSymbol(sourceDocument, requestDocument, symbol))
            .Where(static symbol => symbol is not null)
            .Cast<LspDocumentSymbol>()
            .ToArray();
    }

    private static LspDocumentSymbol? MapDocumentSymbol(
        DocumentSnapshot sourceDocument,
        VolarRequestDocument requestDocument,
        LspDocumentSymbol symbol)
    {
        var projectionMap = requestDocument.ProjectionMap;
        if (projectionMap is null)
        {
            return symbol;
        }

        if (!projectionMap.TryMapToOriginalRange(
                requestDocument.RequestDocument.Text,
                symbol.Range,
                sourceDocument.Text,
                out var sourceRange))
        {
            return null;
        }

        var sourceSelectionRange = projectionMap.TryMapToOriginalRange(
            requestDocument.RequestDocument.Text,
            symbol.SelectionRange,
            sourceDocument.Text,
            out var mappedSelectionRange)
            ? mappedSelectionRange
            : sourceRange;

        LspDocumentSymbol[]? mappedChildren = null;
        if (symbol.Children is { Length: > 0 })
        {
            mappedChildren = symbol.Children
                .Select(child => MapDocumentSymbol(sourceDocument, requestDocument, child))
                .Where(static child => child is not null)
                .Cast<LspDocumentSymbol>()
                .ToArray();
        }

        return new LspDocumentSymbol
        {
            Name = symbol.Name,
            Detail = symbol.Detail,
            Kind = symbol.Kind,
            Range = sourceRange,
            SelectionRange = sourceSelectionRange,
            Children = mappedChildren
        };
    }

    private static IReadOnlyList<LspSemanticToken> MapSemanticTokens(
        DocumentSnapshot sourceDocument,
        VolarRequestDocument requestDocument,
        IReadOnlyList<LspSemanticToken> tokens)
    {
        if (requestDocument.ProjectionMap is null)
        {
            return tokens;
        }

        var mappedTokens = new List<LspSemanticToken>(tokens.Count);
        foreach (var token in tokens)
        {
            var projectedRange = new LspRange
            {
                Start = new LspPosition
                {
                    Line = token.Line,
                    Character = token.Character
                },
                End = new LspPosition
                {
                    Line = token.Line,
                    Character = token.Character + token.Length
                }
            };

            if (!requestDocument.ProjectionMap.TryMapToOriginalRange(
                    requestDocument.RequestDocument.Text,
                    projectedRange,
                    sourceDocument.Text,
                    out var sourceRange))
            {
                continue;
            }

            if (sourceRange.Start.Line != sourceRange.End.Line)
            {
                continue;
            }

            mappedTokens.Add(new LspSemanticToken
            {
                Line = sourceRange.Start.Line,
                Character = sourceRange.Start.Character,
                Length = Math.Max(0, sourceRange.End.Character - sourceRange.Start.Character),
                TokenType = token.TokenType,
                TokenModifiers = token.TokenModifiers
            });
        }

        return mappedTokens;
    }

    private static IReadOnlyList<LspDocumentLink> MapDocumentLinks(
        DocumentSnapshot sourceDocument,
        VolarRequestDocument requestDocument,
        IReadOnlyList<LspDocumentLink> links)
    {
        if (requestDocument.ProjectionMap is null)
        {
            return links
                .Select(link => new LspDocumentLink
                {
                    Range = link.Range,
                    Target = NormalizeDocumentLinkTarget(link.Target),
                    Tooltip = link.Tooltip
                })
                .ToArray();
        }

        return links
            .Select(link =>
            {
                if (!requestDocument.ProjectionMap.TryMapToOriginalRange(
                        requestDocument.RequestDocument.Text,
                        link.Range,
                        sourceDocument.Text,
                        out var sourceRange))
                {
                    return null;
                }

                return new LspDocumentLink
                {
                    Range = sourceRange,
                    Target = NormalizeDocumentLinkTarget(link.Target),
                    Tooltip = link.Tooltip
                };
            })
            .Where(static link => link is not null)
            .Cast<LspDocumentLink>()
            .ToArray();
    }

    private static IReadOnlyList<LspInlayHint> MapInlayHints(
        DocumentSnapshot sourceDocument,
        VolarRequestDocument requestDocument,
        IReadOnlyList<LspInlayHint> hints)
    {
        if (requestDocument.ProjectionMap is null)
        {
            return hints;
        }

        return hints
            .Select(hint =>
            {
                if (!requestDocument.ProjectionMap.TryMapToOriginalPosition(
                        requestDocument.RequestDocument.Text,
                        hint.Position,
                        sourceDocument.Text,
                        out var sourcePosition))
                {
                    return null;
                }

                return new LspInlayHint
                {
                    Position = sourcePosition,
                    Label = hint.Label,
                    Kind = hint.Kind
                };
            })
            .Where(static hint => hint is not null)
            .Cast<LspInlayHint>()
            .ToArray();
    }

    private static IReadOnlyList<LspFoldingRange> MapFoldingRanges(
        DocumentSnapshot sourceDocument,
        VolarRequestDocument requestDocument,
        IReadOnlyList<LspFoldingRange> ranges)
    {
        if (requestDocument.ProjectionMap is null)
        {
            return ranges;
        }

        return ranges
            .Select(range =>
            {
                var projectedStart = new LspPosition
                {
                    Line = Math.Max(0, range.StartLine),
                    Character = Math.Max(0, range.StartCharacter ?? 0)
                };
                var projectedEnd = new LspPosition
                {
                    Line = Math.Max(0, range.EndLine),
                    Character = Math.Max(0, range.EndCharacter ?? 0)
                };
                if (!requestDocument.ProjectionMap.TryMapToOriginalPosition(
                        requestDocument.RequestDocument.Text,
                        projectedStart,
                        sourceDocument.Text,
                        out var sourceStart)
                    || !requestDocument.ProjectionMap.TryMapToOriginalPosition(
                        requestDocument.RequestDocument.Text,
                        projectedEnd,
                        sourceDocument.Text,
                        out var sourceEnd))
                {
                    return null;
                }

                if (sourceEnd.Line < sourceStart.Line
                    || (sourceEnd.Line == sourceStart.Line && sourceEnd.Character < sourceStart.Character))
                {
                    return null;
                }

                return new LspFoldingRange
                {
                    StartLine = sourceStart.Line,
                    StartCharacter = range.StartCharacter is null ? null : sourceStart.Character,
                    EndLine = sourceEnd.Line,
                    EndCharacter = range.EndCharacter is null ? null : sourceEnd.Character,
                    Kind = range.Kind
                };
            })
            .Where(static range => range is not null)
            .Cast<LspFoldingRange>()
            .ToArray();
    }

    private async ValueTask<IReadOnlyList<LspDiagnostic>> CreateUnresolvedMarkupComponentDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        var diagnostics = new List<LspDiagnostic>();
        foreach (Match match in JazorMarkupPatterns.ComponentTagPattern.Matches(document.Text))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var group = match.Groups["name"];
            if (!group.Success)
            {
                continue;
            }

            var isResolvable = await _markupComponentBridge.ResolveComponentAsync(
                    document.DocumentPath,
                    group.Value,
                    allowWorkspaceScan: document.DocumentKind == DocumentKind.Jazor,
                    cancellationToken)
                is not null;
            if (isResolvable)
            {
                continue;
            }

            diagnostics.Add(new LspDiagnostic
            {
                Range = new LspRange
                {
                    Start = LspProtocolHelpers.GetPosition(document.Text, group.Index),
                    End = LspProtocolHelpers.GetPosition(document.Text, group.Index + group.Length)
                },
                Severity = DiagnosticSeverityWarning,
                Code = MissingTemplateImportDiagnosticCode,
                Source = "Jolt.Frontend",
                Message = $"Razor component '{group.Value}' could not be resolved to a nearby Vue file."
            });
        }

        return diagnostics;
    }

    private async ValueTask<IReadOnlyList<LspDiagnostic>> FilterDenoDiagnosticsAsync(
        DocumentSnapshot document,
        IReadOnlyList<LspDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var filtered = new List<LspDiagnostic>();
        foreach (var diagnostic in diagnostics)
        {
            if (!string.Equals(diagnostic.Code, MissingTemplateImportDiagnosticCode, StringComparison.Ordinal))
            {
                filtered.Add(diagnostic);
                continue;
            }

            var componentName = TryGetComponentName(document.Text, diagnostic.Range);
            if (componentName is not null
                && await _markupComponentBridge.ResolveComponentAsync(
                        document.DocumentPath,
                        componentName,
                        allowWorkspaceScan: document.DocumentKind == DocumentKind.Jazor,
                        cancellationToken) is not null)
            {
                continue;
            }

            filtered.Add(diagnostic);
        }

        return filtered;
    }

    private static void RecordDenoFailure(
        string operation,
        string documentPath,
        Exception exception)
    {
        var timestamp = DateTimeOffset.UtcNow;
        var snapshots = GetDenoFailureSnapshotStore();
        var snapshot = snapshots.AddOrUpdate(
            operation,
            static (op, state) => new DenoFailureSnapshot(
                Operation: op,
                FailureCount: 1,
                LastFailureAt: state.Timestamp,
                LastErrorType: state.Exception.GetType().FullName ?? state.Exception.GetType().Name,
                LastErrorMessage: state.Exception.Message),
            static (op, current, state) => current with
            {
                FailureCount = current.FailureCount + 1,
                LastFailureAt = state.Timestamp,
                LastErrorType = state.Exception.GetType().FullName ?? state.Exception.GetType().Name,
                LastErrorMessage = state.Exception.Message
            },
            (Timestamp: timestamp, Exception: exception));
        TrimDenoFailureSnapshots();

        var payload = new
        {
            eventType = "volarDenoLaneDegraded",
            operation,
            documentPath,
            failureCount = snapshot.FailureCount,
            errorType = snapshot.LastErrorType,
            message = snapshot.LastErrorMessage,
            timestamp = snapshot.LastFailureAt
        };
        Console.Error.WriteLine(JsonSerializer.Serialize(payload));
    }

    private static void TrimDenoFailureSnapshots()
    {
        var snapshots = GetDenoFailureSnapshotStore();
        while (snapshots.Count > MaxDenoFailureSnapshots)
        {
            var oldest = snapshots
                .OrderBy(static entry => entry.Value.LastFailureAt)
                .Select(static entry => entry.Key)
                .FirstOrDefault();
            if (oldest is null || !snapshots.TryRemove(oldest, out _))
            {
                return;
            }
        }
    }

    private static ConcurrentDictionary<string, DenoFailureSnapshot> GetDenoFailureSnapshotStore()
        => TestDenoFailureSnapshots.Value ?? DenoFailureSnapshots;

    private static string? TryGetComponentName(string text, LspRange range)
    {
        var start = LspProtocolHelpers.GetOffset(text, range.Start);
        var length = Math.Max(0, LspProtocolHelpers.GetOffset(text, range.End) - start);
        if (start < 0 || start >= text.Length || length <= 0)
        {
            return null;
        }

        return text.Substring(start, Math.Min(length, text.Length - start));
    }

    private bool CanUseWorkspaceGraph()
        => _denoVolarHost?.IsEnabled == true;

    private static IReadOnlyList<LspSemanticToken> CreateConservativeProjectedTemplateSemanticTokens(
        DocumentSnapshot document)
    {
        var tokens = new List<LspSemanticToken>();
        foreach (Match match in JazorMarkupPatterns.ComponentTagPattern.Matches(document.Text))
        {
            var group = match.Groups["name"];
            if (!group.Success)
            {
                continue;
            }

            var position = LspProtocolHelpers.GetPosition(document.Text, group.Index);
            tokens.Add(new LspSemanticToken
            {
                Line = position.Line,
                Character = position.Character,
                Length = group.Length,
                TokenType = "class"
            });
        }

        return tokens;
    }

    internal sealed record DenoFailureSnapshot(
        string Operation,
        int FailureCount,
        DateTimeOffset LastFailureAt,
        string LastErrorType,
        string LastErrorMessage);

    private readonly record struct VolarRequestDocument(
        DocumentSnapshot RequestDocument,
        ProjectionMap? ProjectionMap)
    {
        public LspPosition MapPosition(LspPosition sourcePosition, LspPosition? projectedPosition)
            => ProjectionMap is null
                ? sourcePosition
                : projectedPosition ?? sourcePosition;
    }

    private static string NormalizePath(string documentPath)
        => documentPath.Replace('\\', '/');

    private static bool LocationTargetsProjectedDocument(
        string locationUri,
        string projectedUri,
        string normalizedProjectedPath)
    {
        if (string.Equals(locationUri, projectedUri, StringComparison.Ordinal))
        {
            return true;
        }

        var locationPath = NormalizePath(GetComparableLocationPath(locationUri));
        return string.Equals(locationPath, normalizedProjectedPath, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetComparableLocationPath(string locationUri)
    {
        const string virtualFileUriPrefix = "file://virtual:";
        if (locationUri.StartsWith(virtualFileUriPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return "virtual:" + locationUri[virtualFileUriPrefix.Length..];
        }

        return LspProtocolHelpers.ToDocumentPath(locationUri);
    }

    private static IReadOnlyList<LspLocation> NormalizeLocationUris(
        IReadOnlyList<LspLocation> locations)
    {
        if (locations.Count == 0)
        {
            return locations;
        }

        return locations
            .Select(location => new LspLocation
            {
                Uri = NormalizeFileUri(location.Uri),
                Range = location.Range
            })
            .ToArray();
    }

    private static IReadOnlyList<LspDocumentHighlight> CreateDocumentHighlightsFromLocations(
        DocumentSnapshot sourceDocument,
        IReadOnlyList<LspLocation> locations)
    {
        var sourceUri = NormalizeFileUri(LspProtocolHelpers.ToDocumentUri(sourceDocument.DocumentPath));
        return locations
            .Where(location => string.Equals(NormalizeFileUri(location.Uri), sourceUri, StringComparison.Ordinal))
            .Select(static location => new LspDocumentHighlight
            {
                Range = location.Range,
                Kind = 1
            })
            .GroupBy(static highlight =>
                $"{highlight.Range.Start.Line}:{highlight.Range.Start.Character}:{highlight.Range.End.Line}:{highlight.Range.End.Character}:{highlight.Kind}",
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    private static string NormalizeFileUri(string uri)
    {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed) || !parsed.IsFile)
        {
            return uri;
        }

        var localPath = parsed.LocalPath;
        if (localPath.Length >= 2 && localPath[1] == ':')
        {
            localPath = char.ToUpperInvariant(localPath[0]) + localPath[1..];
        }

        return new Uri(localPath).AbsoluteUri;
    }

    private static string? NormalizeDocumentLinkTarget(string? target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return target;
        }

        return Uri.TryCreate(target, UriKind.Absolute, out var parsed) && parsed.IsFile
            ? NormalizeFileUri(target)
            : target;
    }
}
