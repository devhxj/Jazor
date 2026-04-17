using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Frontend;
using Jazor.VueHost.Frontend.Deno.Hosting;
using Jazor.VueHost.Lsp.Coordination;
using Jazor.VueHost.Lsp.Routing;
using Jazor.VueHost.VirtualDocuments.Mapping;
using Jazor.VueHost.VirtualDocuments.Models;
using Jazor.VueHost.VirtualDocuments.Registry;
using Jazor.VueHost.Workspace;
using System.Text.RegularExpressions;

namespace Jazor.VueHost.Lsp.Lanes;

internal sealed class VolarLaneService : ILspLane
{
    private const string MissingTemplateImportDiagnosticCode = "JAZORVUEFRONTEND001";
    private static readonly Regex ComponentTagPattern = new(
        @"<(?<name>[A-Z][A-Za-z0-9_]*)\b",
        RegexOptions.Compiled);
    private static readonly Regex TagCompletionPrefixPattern = new(
        @"</?(?<name>[A-Za-z0-9_]*)$",
        RegexOptions.Compiled);
    private readonly IFrontendContextProvider? _frontendContextProvider;
    private readonly IVirtualDocumentRegistry? _virtualDocumentRegistry;
    private readonly IDenoVolarHost? _denoVolarHost;
    private readonly MarkupComponentBridgeService _markupComponentBridge;

    public VolarLaneService(
        IVueHostWorkspaceStore workspaceStore,
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

        if (document.DocumentKind is DocumentKind.Jazor or DocumentKind.Vue
            && frontendDocument.ProjectionMap is null)
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
        if (frontendDocument.ProjectionMap is null
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

        if (frontendDocument.ProjectionMap is not null || !CanUseWorkspaceGraph())
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

        if (frontendDocument.ProjectionMap is null
            && CanUseWorkspaceGraph()
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

        var frontendContext = await GetVolarIntelliSenseContextAsync(document, cancellationToken);
        var denoResult = await TryGetDenoDocumentSymbolsAsync(document, frontendContext, cancellationToken);
        if (denoResult is { Count: > 0 })
        {
            return denoResult;
        }

        return Array.Empty<LspDocumentSymbol>();
    }

    public ValueTask<LspSignatureHelp?> GetSignatureHelpAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<LspSignatureHelp?>(null);

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
        var denoResult = await TryGetDenoDefinitionsAsync(frontendDocument.RequestDocument, requestPosition, frontendContext, cancellationToken);
        if (denoResult is { Count: > 0 })
        {
            return MapLocations(document, frontendDocument, denoResult);
        }

        return Array.Empty<LspLocation>();
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
        var prefix = text[..Math.Min(offset, text.Length)];
        var match = TagCompletionPrefixPattern.Match(prefix);
        if (!match.Success)
        {
            tagPrefix = string.Empty;
            return false;
        }

        tagPrefix = match.Groups["name"].Value;
        return true;
    }

    private static bool TryGetComponentTagNameAtPosition(string text, LspPosition position, out string componentName)
    {
        var offset = LspProtocolHelpers.GetOffset(text, position);
        foreach (Match match in ComponentTagPattern.Matches(text))
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

    private async ValueTask<IReadOnlyList<LspCompletionItem>> TryGetDenoCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        if (_denoVolarHost is null)
        {
            return Array.Empty<LspCompletionItem>();
        }

        try
        {
            return await _denoVolarHost.GetTemplateCompletionItemsAsync(document, position, context, cancellationToken);
        }
        catch
        {
            return Array.Empty<LspCompletionItem>();
        }
    }

    private async ValueTask<LspHoverResult?> TryGetDenoHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        if (_denoVolarHost is null)
        {
            return null;
        }

        try
        {
            return await _denoVolarHost.GetTemplateHoverAsync(document, position, context, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async ValueTask<IReadOnlyList<LspLocation>> TryGetDenoDefinitionsAsync(
        DocumentSnapshot document,
        LspPosition position,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        if (_denoVolarHost is null)
        {
            return Array.Empty<LspLocation>();
        }

        try
        {
            return await _denoVolarHost.GetTemplateDefinitionAsync(document, position, context, cancellationToken);
        }
        catch
        {
            return Array.Empty<LspLocation>();
        }
    }

    private async ValueTask<IReadOnlyList<LspLocation>> TryGetDenoReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        if (_denoVolarHost is null)
        {
            return Array.Empty<LspLocation>();
        }

        try
        {
            return await _denoVolarHost.GetTemplateReferencesAsync(
                document,
                position,
                includeDeclaration,
                context,
                cancellationToken);
        }
        catch
        {
            return Array.Empty<LspLocation>();
        }
    }

    private async ValueTask<LspWorkspaceEdit?> TryGetDenoRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        if (_denoVolarHost is null)
        {
            return null;
        }

        try
        {
            return await _denoVolarHost.GetTemplateRenameAsync(document, position, newName, context, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async ValueTask<IReadOnlyList<LspDocumentSymbol>> TryGetDenoDocumentSymbolsAsync(
        DocumentSnapshot document,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        if (_denoVolarHost is null)
        {
            return Array.Empty<LspDocumentSymbol>();
        }

        try
        {
            return await _denoVolarHost.GetTemplateDocumentSymbolsAsync(document, context, cancellationToken);
        }
        catch
        {
            return Array.Empty<LspDocumentSymbol>();
        }
    }

    private async ValueTask<IReadOnlyList<LspDiagnostic>> TryGetDenoDiagnosticsAsync(
        DocumentSnapshot document,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        if (_denoVolarHost is null)
        {
            return Array.Empty<LspDiagnostic>();
        }

        try
        {
            return await _denoVolarHost.GetTemplateDiagnosticsAsync(document, context, cancellationToken);
        }
        catch
        {
            return Array.Empty<LspDiagnostic>();
        }
    }

    private async ValueTask<IReadOnlyList<LspSemanticToken>> TryGetDenoSemanticTokensAsync(
        DocumentSnapshot document,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
    {
        if (_denoVolarHost is null)
        {
            return Array.Empty<LspSemanticToken>();
        }

        try
        {
            return await _denoVolarHost.GetTemplateSemanticTokensAsync(document, context, cancellationToken);
        }
        catch
        {
            return Array.Empty<LspSemanticToken>();
        }
    }

    private async ValueTask<DenoVolarIntelliSenseContext?> GetVolarIntelliSenseContextAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        if (document.DocumentKind != DocumentKind.Jazor || _frontendContextProvider is null)
        {
            return null;
        }

        try
        {
            var response = await _frontendContextProvider.GetFrontendContextAsync(
                new GetFrontendContextRequest(document.DocumentPath, Array.Empty<string>()),
                cancellationToken);
            return new DenoVolarIntelliSenseContext(response.SemanticContext, response.Artifacts);
        }
        catch
        {
            return null;
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
            return locations;
        }

        var projectedUri = LspProtocolHelpers.ToDocumentUri(requestDocument.RequestDocument.DocumentPath);
        return locations
            .Select(location =>
            {
                if (!string.Equals(location.Uri, projectedUri, StringComparison.Ordinal))
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
            .ToArray();
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

        var projectedUri = LspProtocolHelpers.ToDocumentUri(requestDocument.RequestDocument.DocumentPath);
        var sourceUri = LspProtocolHelpers.ToDocumentUri(sourceDocument.DocumentPath);
        var mappedChanges = new Dictionary<string, LspTextEdit[]>(StringComparer.Ordinal);
        foreach (var change in workspaceEdit.Changes)
        {
            if (!string.Equals(change.Key, projectedUri, StringComparison.Ordinal))
            {
                mappedChanges[change.Key] = change.Value;
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

    private async ValueTask<IReadOnlyList<LspDiagnostic>> CreateUnresolvedMarkupComponentDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        var diagnostics = new List<LspDiagnostic>();
        foreach (Match match in ComponentTagPattern.Matches(document.Text))
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
                Severity = 2,
                Code = MissingTemplateImportDiagnosticCode,
                Source = "Jazor.VueHost.Frontend",
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
        => _denoVolarHost?.IsRunning == true;

    private static IReadOnlyList<LspSemanticToken> CreateConservativeProjectedTemplateSemanticTokens(
        DocumentSnapshot document)
    {
        var tokens = new List<LspSemanticToken>();
        foreach (Match match in ComponentTagPattern.Matches(document.Text))
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
}
