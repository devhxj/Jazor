using System.Diagnostics;
using System.Text.Json;
using Jazor.VueHost.Extensions;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Jazor.Projection;
using Jazor.VueHost.Lsp.Aggregation;
using Jazor.VueHost.Lsp.Coordination;
using Jazor.VueHost.Lsp.Lanes;
using Jazor.VueHost.Lsp.Routing;
using Jazor.VueHost.VirtualDocuments.Registry;
using Jazor.VueHost.Workspace;

namespace Jazor.VueHost.Lsp;

internal sealed class LspSession
{
    private readonly IVueHostWorkspaceStore _workspaceStore;
    private readonly IReadOnlyDictionary<LaneKind, ILspLane> _lanes;
    private readonly ILspLaneRouter _laneRouter;
    private readonly LspMessageWriter _writer;
    private readonly JazorProjectionService _projectionService;
    private readonly IVirtualDocumentRegistry _virtualDocumentRegistry;
    private readonly DocumentProjectionResolver _projectionResolver;
    private readonly LspResultAggregator _resultAggregator;
    private readonly MarkupBridgeFanoutCoordinator _markupBridgeFanoutCoordinator;
    private readonly ReferenceCoordinator _referenceCoordinator;
    private readonly RenameCoordinator _renameCoordinator;
    private readonly CodeActionCoordinator _codeActionCoordinator;
    private readonly IWorkspaceDocumentChangeSink _workspaceDocumentChangeSink;
    private readonly IExtensionRegistry _extensionRegistry;
    private readonly TimeSpan _extensionProviderTimeout;
    private readonly int _extensionProviderIsolationFailureThreshold;
    private readonly TimeSpan _extensionProviderIsolationDuration;
    private readonly Lock _providerIsolationGate = new();
    private readonly Dictionary<string, ProviderIsolationState> _providerIsolationByKey = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _workspaceFoldersGate = new();
    private readonly Dictionary<string, LspWorkspaceFolder> _workspaceFoldersByUri = new(StringComparer.OrdinalIgnoreCase);

    public LspSession(
        IVueHostWorkspaceStore workspaceStore,
        IEnumerable<ILspLane> lanes,
        ILspLaneRouter laneRouter,
        LspMessageWriter writer,
        JazorProjectionService projectionService,
        IVirtualDocumentRegistry virtualDocumentRegistry,
        DocumentProjectionResolver projectionResolver,
        LspResultAggregator resultAggregator,
        MarkupBridgeFanoutCoordinator markupBridgeFanoutCoordinator,
        ReferenceCoordinator referenceCoordinator,
        RenameCoordinator renameCoordinator,
        CodeActionCoordinator codeActionCoordinator,
        IWorkspaceDocumentChangeSink? workspaceDocumentChangeSink = null,
        IExtensionRegistry? extensionRegistry = null,
        TimeSpan? extensionProviderTimeout = null,
        int extensionProviderIsolationFailureThreshold = 2,
        TimeSpan? extensionProviderIsolationDuration = null)
    {
        _workspaceStore = workspaceStore ?? throw new ArgumentNullException(nameof(workspaceStore));
        ArgumentNullException.ThrowIfNull(lanes);
        _lanes = lanes.ToDictionary(static lane => lane.LaneKind);
        _laneRouter = laneRouter ?? throw new ArgumentNullException(nameof(laneRouter));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _projectionService = projectionService ?? throw new ArgumentNullException(nameof(projectionService));
        _virtualDocumentRegistry = virtualDocumentRegistry ?? throw new ArgumentNullException(nameof(virtualDocumentRegistry));
        _projectionResolver = projectionResolver ?? throw new ArgumentNullException(nameof(projectionResolver));
        _resultAggregator = resultAggregator ?? throw new ArgumentNullException(nameof(resultAggregator));
        _markupBridgeFanoutCoordinator = markupBridgeFanoutCoordinator ?? throw new ArgumentNullException(nameof(markupBridgeFanoutCoordinator));
        _referenceCoordinator = referenceCoordinator ?? throw new ArgumentNullException(nameof(referenceCoordinator));
        _renameCoordinator = renameCoordinator ?? throw new ArgumentNullException(nameof(renameCoordinator));
        _codeActionCoordinator = codeActionCoordinator ?? throw new ArgumentNullException(nameof(codeActionCoordinator));
        _workspaceDocumentChangeSink = workspaceDocumentChangeSink ?? NullWorkspaceDocumentChangeSink.Instance;
        _extensionRegistry = extensionRegistry ?? NullExtensionRegistry.Instance;
        _extensionProviderTimeout = extensionProviderTimeout.HasValue
            && extensionProviderTimeout.Value > TimeSpan.Zero
                ? extensionProviderTimeout.Value
                : TimeSpan.FromSeconds(2);
        _extensionProviderIsolationFailureThreshold = Math.Max(1, extensionProviderIsolationFailureThreshold);
        _extensionProviderIsolationDuration = extensionProviderIsolationDuration.HasValue
            && extensionProviderIsolationDuration.Value > TimeSpan.Zero
                ? extensionProviderIsolationDuration.Value
                : TimeSpan.FromSeconds(10);
    }

    public async ValueTask<LspResponseMessage?> HandleRequestAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return request.Method switch
        {
            "initialize" => HandleInitialize(request),
            "shutdown" => CreateSuccessResponse(request.Id, result: null),
            "textDocument/hover" => await HandleHoverAsync(request, cancellationToken),
            "textDocument/completion" => await HandleCompletionAsync(request, cancellationToken),
            "textDocument/documentSymbol" => await HandleDocumentSymbolsAsync(request, cancellationToken),
            "textDocument/semanticTokens/full" => await HandleSemanticTokensAsync(request, cancellationToken),
            "textDocument/signatureHelp" => await HandleSignatureHelpAsync(request, cancellationToken),
            "textDocument/inlayHint" => await HandleInlayHintAsync(request, cancellationToken),
            "workspace/symbol" => await HandleWorkspaceSymbolAsync(request, cancellationToken),
            "textDocument/foldingRange" => await HandleFoldingRangeAsync(request, cancellationToken),
            "textDocument/definition" => await HandleDefinitionAsync(request, cancellationToken),
            "textDocument/references" => await HandleReferencesAsync(request, cancellationToken),
            "textDocument/rename" => await HandleRenameAsync(request, cancellationToken),
            "textDocument/prepareRename" => await HandlePrepareRenameAsync(request, cancellationToken),
            "textDocument/codeAction" => await HandleCodeActionAsync(request, cancellationToken),
            "jazor/extensionProviderHealth" => CreateSuccessResponse(
                request.Id,
                _extensionRegistry.GetProviderHealth()),
            "jazor/extensionLoadHealth" => CreateSuccessResponse(
                request.Id,
                _extensionRegistry.GetExtensionLoadHealth()),
            "jazor/extensionObservabilityDashboard" => CreateSuccessResponse(
                request.Id,
                CreateObservabilityDashboard()),
            _ => CreateErrorResponse(request.Id, -32601, $"Unsupported LSP method '{request.Method}'.")
        };
    }

    public async ValueTask<bool> HandleNotificationAsync(
        LspRequestMessage notification,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        switch (notification.Method)
        {
            case "initialized":
                return true;
            case "exit":
                return false;
            case "textDocument/didOpen":
                await HandleDidOpenAsync(notification, cancellationToken);
                return true;
            case "textDocument/didChange":
                await HandleDidChangeAsync(notification, cancellationToken);
                return true;
            case "textDocument/didClose":
                await HandleDidCloseAsync(notification, cancellationToken);
                return true;
            case "workspace/didChangeWorkspaceFolders":
                HandleDidChangeWorkspaceFolders(notification);
                return true;
            default:
                return true;
        }
    }

    private LspResponseMessage HandleInitialize(LspRequestMessage request)
    {
        var parameters = TryDeserializeParams<LspInitializeParams>(request.Params);
        ApplyInitializeWorkspaceFolders(parameters);
        return CreateSuccessResponse(
            request.Id,
            new LspInitializeResult
            {
                Capabilities = new LspServerCapabilities
                {
                    TextDocumentSync = new LspTextDocumentSyncOptions
                    {
                        OpenClose = true,
                        Change = 1
                    },
                    HoverProvider = true,
                    DefinitionProvider = true,
                    ReferencesProvider = true,
                    RenameProvider = new LspRenameOptions
                    {
                        PrepareProvider = true
                    },
                    CodeActionProvider = true,
                    DocumentSymbolProvider = true,
                    SignatureHelpProvider = new LspSignatureHelpOptions
                    {
                        TriggerCharacters = ["(", ","],
                        RetriggerCharacters = [")"]
                    },
                    WorkspaceSymbolProvider = true,
                    FoldingRangeProvider = true,
                    InlayHintProvider = true,
                    CompletionProvider = new LspCompletionOptions
                    {
                        ResolveProvider = false,
                        TriggerCharacters = ["@", "<", "/"]
                    },
                    SemanticTokensProvider = new LspSemanticTokensOptions
                    {
                        Legend = LspSemanticTokenLegend.CreateDescriptor(),
                        Full = true,
                        Range = false
                    },
                    Workspace = new LspWorkspaceServerCapabilities
                    {
                        WorkspaceFolders = new LspWorkspaceFoldersServerCapabilities
                        {
                            Supported = true,
                            ChangeNotifications = true
                        }
                    }
                },
                ServerInfo = new LspServerInfo
                {
                    Name = "Jazor.VueHost",
                    Version = "0.1"
                }
            });
    }

    private static LspResponseMessage CreateSuccessResponse(object? id, object? result)
        => new()
        {
            Id = id,
            Result = result
        };

    private static LspResponseMessage CreateErrorResponse(object? id, int code, string message)
        => new()
        {
            Id = id,
            Error = new LspResponseError
            {
                Code = code,
                Message = message
            }
        };

    private async ValueTask<LspResponseMessage> HandleHoverAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspHoverParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        var projectionTarget = await _projectionResolver.ResolveAsync(document, parameters.Position, cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await CollectHoverAsync(document, parameters.Position, projectionTarget, cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleCompletionAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspCompletionParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        var projectionTarget = await _projectionResolver.ResolveAsync(document, parameters.Position, cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await CollectCompletionItemsAsync(document, parameters.Position, projectionTarget, cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleDocumentSymbolsAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspDocumentSymbolParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await CollectDocumentSymbolsAsync(document, cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleSemanticTokensAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspSemanticTokensParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
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
        var document = await GetRequiredDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
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

    private async ValueTask<LspResponseMessage> HandleSignatureHelpAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspSignatureHelpParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        var projectionTarget = await _projectionResolver.ResolveAsync(document, parameters.Position, cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await CollectSignatureHelpAsync(
                document,
                parameters.Position,
                projectionTarget,
                cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleInlayHintAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspInlayHintParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await CollectInlayHintsAsync(document, parameters.Range, cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleWorkspaceSymbolAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspWorkspaceSymbolParams>(request.Params);
        return CreateSuccessResponse(
            request.Id,
            await CollectWorkspaceSymbolsAsync(parameters.Query, cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleFoldingRangeAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspFoldingRangeParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await CollectFoldingRangesAsync(document, cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleReferencesAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspReferenceParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        var projectionTarget = await _projectionResolver.ResolveAsync(document, parameters.Position, cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await CollectReferencesAsync(
                document,
                parameters.Position,
                parameters.Context?.IncludeDeclaration ?? true,
                projectionTarget,
                cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleRenameAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspRenameParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        var projectionTarget = await _projectionResolver.ResolveAsync(document, parameters.Position, cancellationToken);

        return CreateSuccessResponse(
            request.Id,
            await CollectRenameEditAsync(
                document,
                parameters.Position,
                parameters.NewName,
                projectionTarget,
                cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandlePrepareRenameAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspPrepareRenameParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        var projectionTarget = await _projectionResolver.ResolveAsync(document, parameters.Position, cancellationToken);

        // Try each lane to see if the position is renamable
        foreach (var lane in GetOrderedLanes(projectionTarget))
        {
            var renameEdit = await lane.GetRenameAsync(
                document,
                parameters.Position,
                "__prepare__",
                projectionTarget,
                cancellationToken);

            if (renameEdit is not null)
            {
                // A lane confirmed the position is renamable — return the placeholder range
                var offset = LspProtocolHelpers.GetOffset(document.Text, parameters.Position);
                var wordRange = GetWordRangeAtPosition(document.Text, offset);
                return CreateSuccessResponse(request.Id, new LspPrepareRenameResult
                {
                    Range = wordRange,
                    Placeholder = ExtractWord(document.Text, offset)
                });
            }
        }

        return CreateSuccessResponse(request.Id, result: null);
    }

    private static LspRange GetWordRangeAtPosition(string text, int offset)
    {
        var (start, length) = GetWordBounds(text, offset);
        return LspProtocolHelpers.ToRange(text, start, length);
    }

    private static string ExtractWord(string text, int offset)
    {
        var (start, length) = GetWordBounds(text, offset);
        return text.Substring(start, length);
    }

    private static (int start, int length) GetWordBounds(string text, int offset)
    {
        if (offset < 0 || offset >= text.Length)
        {
            return (Math.Max(0, offset), 0);
        }

        var start = offset;
        while (start > 0 && IsWordCharacter(text[start - 1]))
        {
            start--;
        }

        var end = offset;
        while (end < text.Length && IsWordCharacter(text[end]))
        {
            end++;
        }

        return (start, end - start);
    }

    private static bool IsWordCharacter(char c)
        => char.IsLetterOrDigit(c) || c == '_';

    private async ValueTask<LspResponseMessage> HandleCodeActionAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspCodeActionParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        var projectionTarget = await _projectionResolver.ResolveAsync(document, parameters.Range.Start, cancellationToken);

        return CreateSuccessResponse(
            request.Id,
            await CollectCodeActionsAsync(
                document,
                parameters.Range,
                parameters.Context?.Diagnostics ?? [],
                projectionTarget,
                cancellationToken));
    }

    private async ValueTask HandleDidOpenAsync(
        LspRequestMessage notification,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspDidOpenTextDocumentParams>(notification.Params);
        var documentPath = LspProtocolHelpers.ToDocumentPath(parameters.TextDocument.Uri);
        var document = new DocumentSnapshot(
            documentPath,
            MapDocumentKind(parameters.TextDocument.LanguageId, documentPath),
            parameters.TextDocument.Text,
            parameters.TextDocument.Version?.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await _workspaceStore.UpsertDocumentAsync(document, cancellationToken);
        VueHostWorkspaceResolver.InvalidatePath(documentPath);
        await UpdateProjectionStateAsync(document, cancellationToken);
        await PublishDiagnosticsAsync(document, cancellationToken);
        await RefreshOpenJazorDiagnosticsAsync(document, cancellationToken);
    }

    private async ValueTask HandleDidChangeAsync(
        LspRequestMessage notification,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspDidChangeTextDocumentParams>(notification.Params);
        var documentPath = LspProtocolHelpers.ToDocumentPath(parameters.TextDocument.Uri);
        var existing = await _workspaceStore.GetDocumentAsync(documentPath, cancellationToken);
        var document = new DocumentSnapshot(
            documentPath,
            existing?.DocumentKind ?? MapDocumentKind(languageId: null, documentPath),
            parameters.ContentChanges.LastOrDefault()?.Text ?? string.Empty,
            parameters.TextDocument.Version?.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await _workspaceStore.UpsertDocumentAsync(document, cancellationToken);
        VueHostWorkspaceResolver.InvalidatePath(documentPath);
        await UpdateProjectionStateAsync(document, cancellationToken);
        await PublishDiagnosticsAsync(document, cancellationToken);
        await RefreshOpenJazorDiagnosticsAsync(document, cancellationToken);
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        await NotifyWorkspaceDocumentChangedAsync(document, openDocuments, cancellationToken);
    }

    private async ValueTask HandleDidCloseAsync(
        LspRequestMessage notification,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspDidCloseTextDocumentParams>(notification.Params);
        var documentPath = LspProtocolHelpers.ToDocumentPath(parameters.TextDocument.Uri);
        await _workspaceStore.RemoveDocumentAsync(documentPath, cancellationToken);
        VueHostWorkspaceResolver.InvalidatePath(documentPath);
        await _virtualDocumentRegistry.RemoveBySourceDocumentAsync(documentPath, cancellationToken);
        await PublishDiagnosticsAsync(
            new DocumentSnapshot(documentPath, MapDocumentKind(languageId: null, documentPath), string.Empty, null),
            cancellationToken,
            diagnostics: Array.Empty<LspDiagnostic>());
        await RefreshOpenJazorDiagnosticsAsync(
            new DocumentSnapshot(documentPath, MapDocumentKind(languageId: null, documentPath), string.Empty, null),
            cancellationToken);
    }

    private void HandleDidChangeWorkspaceFolders(LspRequestMessage notification)
    {
        var parameters = TryDeserializeParams<LspDidChangeWorkspaceFoldersParams>(notification.Params);
        if (parameters?.Event is null)
        {
            return;
        }

        ApplyWorkspaceFolderChanges(parameters.Event);
    }

    private async ValueTask PublishDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken,
        IReadOnlyList<LspDiagnostic>? diagnostics = null)
    {
        diagnostics ??= await CollectDiagnosticsAsync(document, cancellationToken);
        diagnostics = _resultAggregator.AggregateDiagnostics(diagnostics);
        await _writer.WriteMessageAsync(
            LspJsonSerializer.Serialize(new LspNotificationMessage
            {
                Method = "textDocument/publishDiagnostics",
                Params = new LspPublishDiagnosticsParams
                {
                    Uri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath),
                    Diagnostics = diagnostics.ToArray()
                }
            }),
            cancellationToken);
    }

    private async ValueTask UpdateProjectionStateAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        if (document.DocumentKind != DocumentKind.Jazor)
        {
            return;
        }

        var virtualDocuments = await _projectionService.ProjectAsync(document, cancellationToken);
        await _virtualDocumentRegistry.UpsertAsync(virtualDocuments, cancellationToken);
    }

    private async ValueTask NotifyWorkspaceDocumentChangedAsync(
        DocumentSnapshot document,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        CancellationToken cancellationToken)
    {
        try
        {
            await _workspaceDocumentChangeSink.OnWorkspaceDocumentChangedAsync(document, openDocuments, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // LSP diagnostics/projection updates must keep working even if dev-server HMR coordination fails.
        }
    }

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

    private void ApplyInitializeWorkspaceFolders(LspInitializeParams? parameters)
    {
        var workspaceFolders = (parameters?.WorkspaceFolders ?? [])
            .Where(static folder => !string.IsNullOrWhiteSpace(folder.Uri))
            .Select(CloneWorkspaceFolder)
            .ToArray();

        if (workspaceFolders.Length == 0)
        {
            var fallbackRootUri = parameters?.RootUri;
            if (string.IsNullOrWhiteSpace(fallbackRootUri)
                && !string.IsNullOrWhiteSpace(parameters?.RootPath))
            {
                fallbackRootUri = new Uri(Path.GetFullPath(parameters.RootPath!)).AbsoluteUri;
            }

            if (!string.IsNullOrWhiteSpace(fallbackRootUri))
            {
                workspaceFolders =
                [
                    new LspWorkspaceFolder
                    {
                        Uri = fallbackRootUri!,
                        Name = Path.GetFileName(LspProtocolHelpers.ToDocumentPath(fallbackRootUri!))
                    }
                ];
            }
        }

        lock (_workspaceFoldersGate)
        {
            _workspaceFoldersByUri.Clear();
            foreach (var folder in workspaceFolders)
            {
                _workspaceFoldersByUri[folder.Uri] = folder;
            }
        }
    }

    private void ApplyWorkspaceFolderChanges(LspWorkspaceFoldersChangeEvent changeEvent)
    {
        lock (_workspaceFoldersGate)
        {
            foreach (var removed in changeEvent.Removed ?? [])
            {
                if (string.IsNullOrWhiteSpace(removed.Uri))
                {
                    continue;
                }

                _workspaceFoldersByUri.Remove(removed.Uri);
            }

            foreach (var added in changeEvent.Added ?? [])
            {
                if (string.IsNullOrWhiteSpace(added.Uri))
                {
                    continue;
                }

                _workspaceFoldersByUri[added.Uri] = CloneWorkspaceFolder(added);
            }
        }
    }

    private IReadOnlyList<LspWorkspaceFolder> GetWorkspaceFoldersSnapshot()
    {
        lock (_workspaceFoldersGate)
        {
            return _workspaceFoldersByUri.Values
                .OrderBy(static folder => folder.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static folder => folder.Uri, StringComparer.OrdinalIgnoreCase)
                .Select(CloneWorkspaceFolder)
                .ToArray();
        }
    }

    private static LspWorkspaceFolder CloneWorkspaceFolder(LspWorkspaceFolder folder)
        => new()
        {
            Uri = folder.Uri,
            Name = string.IsNullOrWhiteSpace(folder.Name)
                ? folder.Uri
                : folder.Name
        };

    private async ValueTask<IReadOnlyList<LspFoldingRange>> CollectFoldingRangesAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        var ranges = new List<LspFoldingRange>();
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

    private async ValueTask<ProviderInvocationResult<TResult>> InvokeProviderAsync<TResult>(
        string capability,
        string providerName,
        Func<CancellationToken, ValueTask<TResult>> invocation,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var startedTimestamp = Stopwatch.GetTimestamp();
        if (TryGetProviderIsolationWindow(capability, providerName, out var isolationRemaining))
        {
            _extensionRegistry.ReportProviderInvocation(new ExtensionProviderInvocation(
                ProviderName: providerName,
                Capability: capability,
                Duration: Stopwatch.GetElapsedTime(startedTimestamp),
                Succeeded: false,
                TimedOut: false,
                Skipped: true,
                ErrorMessage: $"Provider isolated for {isolationRemaining.TotalMilliseconds:F0} ms due to recent failures."));
            return ProviderInvocationResult<TResult>.Isolated();
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        Task<TResult> invocationTask;
        try
        {
            invocationTask = invocation(timeoutCts.Token).AsTask();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            RecordProviderFailure(capability, providerName);
            _extensionRegistry.ReportProviderInvocation(new ExtensionProviderInvocation(
                ProviderName: providerName,
                Capability: capability,
                Duration: Stopwatch.GetElapsedTime(startedTimestamp),
                Succeeded: false,
                TimedOut: false,
                Skipped: false,
                ErrorMessage: ex.Message));
            return ProviderInvocationResult<TResult>.Failure();
        }

        try
        {
            var result = await invocationTask.WaitAsync(_extensionProviderTimeout, cancellationToken);
            RecordProviderSuccess(capability, providerName);
            _extensionRegistry.ReportProviderInvocation(new ExtensionProviderInvocation(
                ProviderName: providerName,
                Capability: capability,
                Duration: Stopwatch.GetElapsedTime(startedTimestamp),
                Succeeded: true,
                TimedOut: false,
                Skipped: false,
                ErrorMessage: null));
            return ProviderInvocationResult<TResult>.Success(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TimeoutException)
        {
            RecordProviderFailure(capability, providerName);
            timeoutCts.Cancel();
            _ = ObserveProviderCompletionAsync(invocationTask);
            _extensionRegistry.ReportProviderInvocation(new ExtensionProviderInvocation(
                ProviderName: providerName,
                Capability: capability,
                Duration: Stopwatch.GetElapsedTime(startedTimestamp),
                Succeeded: false,
                TimedOut: true,
                Skipped: false,
                ErrorMessage: $"Provider timed out after {_extensionProviderTimeout.TotalMilliseconds:F0} ms."));
            return ProviderInvocationResult<TResult>.Timeout();
        }
        catch (Exception ex)
        {
            RecordProviderFailure(capability, providerName);
            _extensionRegistry.ReportProviderInvocation(new ExtensionProviderInvocation(
                ProviderName: providerName,
                Capability: capability,
                Duration: Stopwatch.GetElapsedTime(startedTimestamp),
                Succeeded: false,
                TimedOut: false,
                Skipped: false,
                ErrorMessage: ex.Message));
            return ProviderInvocationResult<TResult>.Failure();
        }
    }

    private bool TryGetProviderIsolationWindow(
        string capability,
        string providerName,
        out TimeSpan remaining)
    {
        var now = DateTimeOffset.UtcNow;
        var key = CreateProviderIsolationKey(capability, providerName);

        lock (_providerIsolationGate)
        {
            if (!_providerIsolationByKey.TryGetValue(key, out var state)
                || state.IsolatedUntil is null)
            {
                remaining = TimeSpan.Zero;
                return false;
            }

            var isolatedUntil = state.IsolatedUntil.Value;
            if (isolatedUntil <= now)
            {
                _providerIsolationByKey[key] = state with { IsolatedUntil = null };
                remaining = TimeSpan.Zero;
                return false;
            }

            remaining = isolatedUntil - now;
            return true;
        }
    }

    private void RecordProviderSuccess(string capability, string providerName)
    {
        var key = CreateProviderIsolationKey(capability, providerName);
        lock (_providerIsolationGate)
        {
            _providerIsolationByKey[key] = new ProviderIsolationState(
                ConsecutiveFailureCount: 0,
                IsolatedUntil: null);
        }
    }

    private void RecordProviderFailure(string capability, string providerName)
    {
        var key = CreateProviderIsolationKey(capability, providerName);
        var now = DateTimeOffset.UtcNow;

        lock (_providerIsolationGate)
        {
            _providerIsolationByKey.TryGetValue(key, out var currentState);

            var currentFailures = currentState.IsolatedUntil is { } isolatedUntil && isolatedUntil > now
                ? 0
                : currentState.ConsecutiveFailureCount;
            var nextFailureCount = currentFailures + 1;
            var nextIsolatedUntil = nextFailureCount >= _extensionProviderIsolationFailureThreshold
                ? now + _extensionProviderIsolationDuration
                : (DateTimeOffset?)null;
            var persistedFailures = nextIsolatedUntil is not null
                ? 0
                : nextFailureCount;

            _providerIsolationByKey[key] = new ProviderIsolationState(
                persistedFailures,
                nextIsolatedUntil);
        }
    }

    private static string CreateProviderIsolationKey(string capability, string providerName)
        => capability.Trim() + "|" + providerName.Trim();

    private static async Task ObserveProviderCompletionAsync<TResult>(Task<TResult> task)
    {
        try
        {
            await task;
        }
        catch
        {
            // Swallow fault/cancel from timed-out provider calls to avoid unobserved exceptions.
        }
    }

    private ExtensionObservabilityDashboard CreateObservabilityDashboard()
        => new(
            LoadHealth: _extensionRegistry.GetExtensionLoadHealth(),
            ProviderHealth: _extensionRegistry.GetProviderHealth(),
            RecentLoadEvents: _extensionRegistry.GetRecentExtensionLoadInvocations(maxCount: 200),
            RecentProviderEvents: _extensionRegistry.GetRecentProviderInvocations(maxCount: 500),
            GeneratedAt: DateTimeOffset.UtcNow);

    private async ValueTask RefreshOpenJazorDiagnosticsAsync(
        DocumentSnapshot triggeringDocument,
        CancellationToken cancellationToken)
    {
        if (triggeringDocument.DocumentKind == DocumentKind.Jazor)
        {
            return;
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        foreach (var openDocument in openDocuments.Where(static candidate => candidate.DocumentKind == DocumentKind.Jazor))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await PublishDiagnosticsAsync(openDocument, cancellationToken);
        }
    }

    private IReadOnlyList<ILspLane> GetOrderedLanes(ProjectionTarget projectionTarget)
    {
        var orderedLanes = new List<ILspLane>();
        foreach (var laneKind in _laneRouter.GetOrderedLanes(projectionTarget))
        {
            if (_lanes.TryGetValue(laneKind, out var lane))
            {
                orderedLanes.Add(lane);
            }
        }

        return orderedLanes;
    }

    private IReadOnlyList<ILspLane> GetDocumentSymbolLanes(DocumentSnapshot document)
    {
        LaneKind[] laneKinds = document.DocumentKind switch
        {
            DocumentKind.Jazor => [LaneKind.Jazor, LaneKind.Roslyn],
            DocumentKind.CSharp => [LaneKind.Roslyn],
            DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript or DocumentKind.Css => [LaneKind.Volar],
            _ => [LaneKind.Jazor]
        };

        var orderedLanes = new List<ILspLane>();
        foreach (var laneKind in laneKinds)
        {
            if (_lanes.TryGetValue(laneKind, out var lane))
            {
                orderedLanes.Add(lane);
            }
        }

        return orderedLanes;
    }

    private IReadOnlyList<ILspLane> GetSemanticTokenLanes(DocumentSnapshot document)
    {
        var orderedLanes = new List<ILspLane>();
        foreach (var laneKind in _laneRouter.GetSemanticTokenLanes(document))
        {
            if (_lanes.TryGetValue(laneKind, out var lane))
            {
                orderedLanes.Add(lane);
            }
        }

        return orderedLanes;
    }

    private async ValueTask<DocumentSnapshot> GetRequiredDocumentAsync(
        string documentUri,
        CancellationToken cancellationToken)
    {
        var documentPath = LspProtocolHelpers.ToDocumentPath(documentUri);
        var document = await _workspaceStore.GetDocumentAsync(documentPath, cancellationToken);
        if (document is not null)
        {
            return document;
        }

        if (!File.Exists(documentPath))
        {
            throw new InvalidOperationException($"Document '{documentPath}' is not tracked and does not exist on disk.");
        }

        document = new DocumentSnapshot(
            documentPath,
            MapDocumentKind(languageId: null, documentPath),
            await File.ReadAllTextAsync(documentPath, cancellationToken),
            version: null);
        await _workspaceStore.UpsertDocumentAsync(document, cancellationToken);
        await UpdateProjectionStateAsync(document, cancellationToken);
        return document;
    }

    private static TParams DeserializeParams<TParams>(object? payload)
    {
        if (payload is JsonElement element)
        {
            return element.Deserialize<TParams>() ?? throw new InvalidOperationException("Invalid LSP params payload.");
        }

        if (payload is TParams typed)
        {
            return typed;
        }

        return LspJsonSerializer.Deserialize<TParams>(LspJsonSerializer.Serialize(payload))
            ?? throw new InvalidOperationException("Invalid LSP params payload.");
    }

    private static TParams? TryDeserializeParams<TParams>(object? payload)
    {
        if (payload is null)
        {
            return default;
        }

        try
        {
            return DeserializeParams<TParams>(payload);
        }
        catch
        {
            return default;
        }
    }

    private static DocumentKind MapDocumentKind(string? languageId, string documentPath)
        => languageId?.ToLowerInvariant() switch
        {
            "jazor" => DocumentKind.Jazor,
            "csharp" => DocumentKind.CSharp,
            "cs" => DocumentKind.CSharp,
            "vue" => DocumentKind.Vue,
            "javascript" => DocumentKind.JavaScript,
            "typescript" => DocumentKind.TypeScript,
            "css" => DocumentKind.Css,
            _ => Path.GetExtension(documentPath).ToLowerInvariant() switch
            {
                ".jazor" => DocumentKind.Jazor,
                ".cs" => DocumentKind.CSharp,
                ".vue" => DocumentKind.Vue,
                ".js" => DocumentKind.JavaScript,
                ".ts" => DocumentKind.TypeScript,
                ".css" => DocumentKind.Css,
                _ => DocumentKind.Unknown
            }
        };

    private readonly record struct ProviderInvocationResult<TResult>(
        bool IsSuccess,
        bool TimedOut,
        bool Skipped,
        TResult? Result)
    {
        public static ProviderInvocationResult<TResult> Success(TResult result)
            => new(true, false, false, result);

        public static ProviderInvocationResult<TResult> Timeout()
            => new(false, true, false, default);

        public static ProviderInvocationResult<TResult> Isolated()
            => new(false, false, true, default);

        public static ProviderInvocationResult<TResult> Failure()
            => new(false, false, false, default);
    }

    private readonly record struct ProviderIsolationState(
        int ConsecutiveFailureCount,
        DateTimeOffset? IsolatedUntil);
}
