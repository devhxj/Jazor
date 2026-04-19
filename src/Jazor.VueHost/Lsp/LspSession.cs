using System.Diagnostics;
using System.Text.RegularExpressions;
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
    private const int InvalidParamsErrorCode = -32602;
    private static readonly Regex TagNamePattern = new(
        @"</?(?<name>[A-Za-z][A-Za-z0-9_\-:]*)\b",
        RegexOptions.Compiled);

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
        using var workspaceFolderScope = VueHostWorkspaceResolver.PushWorkspaceFolderRoots(GetWorkspaceFolderRootPaths());

        return request.Method switch
        {
            "initialize" => HandleInitialize(request),
            "shutdown" => CreateSuccessResponse(request.Id, result: null),
            "textDocument/hover" => await HandleHoverAsync(request, cancellationToken),
            "textDocument/documentHighlight" => await HandleDocumentHighlightsAsync(request, cancellationToken),
            "textDocument/documentLink" => await HandleDocumentLinksAsync(request, cancellationToken),
            "textDocument/completion" => await HandleCompletionAsync(request, cancellationToken),
            "completionItem/resolve" => HandleCompletionItemResolve(request),
            "textDocument/documentSymbol" => await HandleDocumentSymbolsAsync(request, cancellationToken),
            "textDocument/semanticTokens/full" => await HandleSemanticTokensAsync(request, cancellationToken),
            "textDocument/signatureHelp" => await HandleSignatureHelpAsync(request, cancellationToken),
            "textDocument/inlayHint" => await HandleInlayHintAsync(request, cancellationToken),
            "workspace/symbol" => await HandleWorkspaceSymbolAsync(request, cancellationToken),
            "textDocument/foldingRange" => await HandleFoldingRangeAsync(request, cancellationToken),
            "textDocument/definition" => await HandleDefinitionAsync(request, cancellationToken),
            "textDocument/typeDefinition" => await HandleTypeDefinitionAsync(request, cancellationToken),
            "textDocument/implementation" => await HandleImplementationAsync(request, cancellationToken),
            "textDocument/selectionRange" => await HandleSelectionRangeAsync(request, cancellationToken),
            "textDocument/linkedEditingRange" => await HandleLinkedEditingRangeAsync(request, cancellationToken),
            "textDocument/formatting" => await HandleDocumentFormattingAsync(request, cancellationToken),
            "textDocument/rangeFormatting" => await HandleDocumentRangeFormattingAsync(request, cancellationToken),
            "textDocument/codeLens" => await HandleCodeLensAsync(request, cancellationToken),
            "textDocument/prepareCallHierarchy" => await HandlePrepareCallHierarchyAsync(request, cancellationToken),
            "callHierarchy/incomingCalls" => await HandleIncomingCallsAsync(request, cancellationToken),
            "callHierarchy/outgoingCalls" => await HandleOutgoingCallsAsync(request, cancellationToken),
            "textDocument/prepareTypeHierarchy" => await HandlePrepareTypeHierarchyAsync(request, cancellationToken),
            "typeHierarchy/supertypes" => await HandleTypeHierarchySuperTypesAsync(request, cancellationToken),
            "typeHierarchy/subtypes" => await HandleTypeHierarchySubTypesAsync(request, cancellationToken),
            "textDocument/willSaveWaitUntil" => CreateSuccessResponse(request.Id, Array.Empty<LspTextEdit>()),
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
        using var workspaceFolderScope = VueHostWorkspaceResolver.PushWorkspaceFolderRoots(GetWorkspaceFolderRootPaths());

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
            case "textDocument/didSave":
                await HandleDidSaveAsync(notification, cancellationToken);
                return true;
            case "textDocument/willSave":
                HandleWillSave(notification);
                return true;
            case "workspace/didChangeWorkspaceFolders":
                HandleDidChangeWorkspaceFolders(notification);
                return true;
            case "workspace/didChangeConfiguration":
                await HandleDidChangeConfigurationAsync(cancellationToken);
                return true;
            case "workspace/didChangeWatchedFiles":
                await HandleDidChangeWatchedFilesAsync(notification, cancellationToken);
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
                        Change = 1,
                        Save = true
                    },
                    HoverProvider = true,
                    DocumentHighlightProvider = true,
                    DocumentLinkProvider = true,
                    DefinitionProvider = true,
                    TypeDefinitionProvider = true,
                    ImplementationProvider = true,
                    SelectionRangeProvider = true,
                    LinkedEditingRangeProvider = true,
                    ReferencesProvider = true,
                    RenameProvider = new LspRenameOptions
                    {
                        PrepareProvider = true
                    },
                    CodeActionProvider = true,
                    CodeLensProvider = true,
                    DocumentSymbolProvider = true,
                    DocumentFormattingProvider = true,
                    DocumentRangeFormattingProvider = true,
                    SignatureHelpProvider = new LspSignatureHelpOptions
                    {
                        TriggerCharacters = ["(", ","],
                        RetriggerCharacters = [")"]
                    },
                    WorkspaceSymbolProvider = true,
                    FoldingRangeProvider = true,
                    InlayHintProvider = true,
                    CallHierarchyProvider = true,
                    TypeHierarchyProvider = true,
                    CompletionProvider = new LspCompletionOptions
                    {
                        ResolveProvider = true,
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

    private async ValueTask<LspResponseMessage> HandlePrepareCallHierarchyAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspCallHierarchyPrepareParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
        var projectionTarget = await _projectionResolver.ResolveAsync(document, parameters.Position, cancellationToken);
        if (TryGetRoslynLaneService(out var roslynLane)
            && IsRoslynSemanticTarget(document, projectionTarget))
        {
            var semanticItems = await roslynLane.PrepareCallHierarchyAsync(document, parameters.Position, cancellationToken);
            if (semanticItems.Count > 0)
            {
                return CreateSuccessResponse(request.Id, semanticItems);
            }
        }

        var range = GetWordRangeAtPosition(
            document.Text,
            LspProtocolHelpers.GetOffset(document.Text, parameters.Position));
        var label = document.Text.Substring(
            LspProtocolHelpers.GetOffset(document.Text, range.Start),
            Math.Max(0, LspProtocolHelpers.GetOffset(document.Text, range.End) - LspProtocolHelpers.GetOffset(document.Text, range.Start)));
        if (string.IsNullOrWhiteSpace(label))
        {
            return CreateSuccessResponse(request.Id, Array.Empty<LspCallHierarchyItem>());
        }

        return CreateSuccessResponse(
            request.Id,
            new[]
            {
                new LspCallHierarchyItem
                {
                    Name = label,
                    Kind = 12,
                    Uri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath),
                    Range = range,
                    SelectionRange = range,
                    Detail = Path.GetFileName(document.DocumentPath)
                }
            });
    }

    private async ValueTask<LspResponseMessage> HandleIncomingCallsAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspCallHierarchyIncomingCallsParams>(request.Params);
        var item = parameters.Item;
        var document = await GetRequiredDocumentAsync(item.Uri, cancellationToken);
        var position = item.SelectionRange.Start;
        var projectionTarget = await _projectionResolver.ResolveAsync(document, position, cancellationToken);
        if (TryGetRoslynLaneService(out var roslynLane)
            && IsRoslynSemanticTarget(document, projectionTarget))
        {
            var incomingCalls = await roslynLane.GetIncomingCallsAsync(document, position, cancellationToken);
            return CreateSuccessResponse(request.Id, incomingCalls);
        }

        return CreateSuccessResponse(request.Id, Array.Empty<LspCallHierarchyIncomingCall>());
    }

    private async ValueTask<LspResponseMessage> HandleOutgoingCallsAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspCallHierarchyOutgoingCallsParams>(request.Params);
        var item = parameters.Item;
        var document = await GetRequiredDocumentAsync(item.Uri, cancellationToken);
        var position = item.SelectionRange.Start;
        var projectionTarget = await _projectionResolver.ResolveAsync(document, position, cancellationToken);
        if (TryGetRoslynLaneService(out var roslynLane)
            && IsRoslynSemanticTarget(document, projectionTarget))
        {
            var outgoingCalls = await roslynLane.GetOutgoingCallsAsync(document, position, cancellationToken);
            return CreateSuccessResponse(request.Id, outgoingCalls);
        }

        return CreateSuccessResponse(request.Id, Array.Empty<LspCallHierarchyOutgoingCall>());
    }

    private async ValueTask<LspResponseMessage> HandlePrepareTypeHierarchyAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspTypeHierarchyPrepareParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
        var projectionTarget = await _projectionResolver.ResolveAsync(document, parameters.Position, cancellationToken);
        if (TryGetRoslynLaneService(out var roslynLane)
            && IsRoslynSemanticTarget(document, projectionTarget))
        {
            var semanticItems = await roslynLane.PrepareTypeHierarchyAsync(document, parameters.Position, cancellationToken);
            if (semanticItems.Count > 0)
            {
                return CreateSuccessResponse(request.Id, semanticItems);
            }
        }

        var range = GetWordRangeAtPosition(
            document.Text,
            LspProtocolHelpers.GetOffset(document.Text, parameters.Position));
        var label = document.Text.Substring(
            LspProtocolHelpers.GetOffset(document.Text, range.Start),
            Math.Max(0, LspProtocolHelpers.GetOffset(document.Text, range.End) - LspProtocolHelpers.GetOffset(document.Text, range.Start)));
        if (string.IsNullOrWhiteSpace(label))
        {
            return CreateSuccessResponse(request.Id, Array.Empty<LspTypeHierarchyItem>());
        }

        return CreateSuccessResponse(
            request.Id,
            new[]
            {
                new LspTypeHierarchyItem
                {
                    Name = label,
                    Kind = 5,
                    Uri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath),
                    Range = range,
                    SelectionRange = range,
                    Detail = Path.GetFileName(document.DocumentPath)
                }
            });
    }

    private async ValueTask<LspResponseMessage> HandleTypeHierarchySuperTypesAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspTypeHierarchyParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.Item.Uri, cancellationToken);
        var position = parameters.Item.SelectionRange.Start;
        var projectionTarget = await _projectionResolver.ResolveAsync(document, position, cancellationToken);
        if (TryGetRoslynLaneService(out var roslynLane)
            && IsRoslynSemanticTarget(document, projectionTarget))
        {
            var superTypes = await roslynLane.GetTypeHierarchySuperTypesAsync(document, position, cancellationToken);
            return CreateSuccessResponse(request.Id, superTypes);
        }

        return CreateSuccessResponse(request.Id, Array.Empty<LspTypeHierarchyItem>());
    }

    private async ValueTask<LspResponseMessage> HandleTypeHierarchySubTypesAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspTypeHierarchyParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.Item.Uri, cancellationToken);
        var position = parameters.Item.SelectionRange.Start;
        var projectionTarget = await _projectionResolver.ResolveAsync(document, position, cancellationToken);
        if (TryGetRoslynLaneService(out var roslynLane)
            && IsRoslynSemanticTarget(document, projectionTarget))
        {
            var subTypes = await roslynLane.GetTypeHierarchySubTypesAsync(document, position, cancellationToken);
            return CreateSuccessResponse(request.Id, subTypes);
        }

        return CreateSuccessResponse(request.Id, Array.Empty<LspTypeHierarchyItem>());
    }

    private async ValueTask<LspResponseMessage> HandleSignatureHelpAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspSignatureHelpParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
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
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await CollectInlayHintsAsync(document, parameters.Range, cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleWorkspaceSymbolAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspWorkspaceSymbolParams>(request.Params);
        if (parameters.Query is null)
        {
            throw CreateInvalidParamsException("workspace/symbol query is required.");
        }

        return CreateSuccessResponse(
            request.Id,
            await CollectWorkspaceSymbolsAsync(parameters.Query, cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleFoldingRangeAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspFoldingRangeParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await CollectFoldingRangesAsync(document, cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleReferencesAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspReferenceParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
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
        if (string.IsNullOrWhiteSpace(parameters.NewName))
        {
            throw CreateInvalidParamsException("textDocument/rename newName is required.");
        }

        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
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
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
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

    private static LspSelectionRange CreateSelectionRange(string text, LspPosition position)
    {
        var offset = LspProtocolHelpers.GetOffset(text, position);
        var wordRange = GetWordRangeAtPosition(text, offset);
        var lineRange = GetLineRangeAtOffset(text, offset);
        var documentRange = LspProtocolHelpers.ToRange(text, 0, text.Length);

        var lineSelection = new LspSelectionRange
        {
            Range = lineRange,
            Parent = new LspSelectionRange
            {
                Range = documentRange
            }
        };

        return new LspSelectionRange
        {
            Range = wordRange,
            Parent = lineSelection
        };
    }

    private static LspRange GetLineRangeAtOffset(string text, int offset)
    {
        var boundedOffset = Math.Clamp(offset, 0, text.Length);
        var lineStart = boundedOffset;
        while (lineStart > 0 && text[lineStart - 1] != '\n')
        {
            lineStart--;
        }

        var lineEnd = boundedOffset;
        while (lineEnd < text.Length && text[lineEnd] != '\n')
        {
            lineEnd++;
        }

        return LspProtocolHelpers.ToRange(text, lineStart, Math.Max(0, lineEnd - lineStart));
    }

    private static IReadOnlyList<LspRange> CollectLinkedEditingRanges(string text, LspPosition position)
    {
        if (!TryFindTagNameAtPosition(text, position, out var tagName))
        {
            return Array.Empty<LspRange>();
        }

        var ranges = new List<LspRange>();
        foreach (Match match in TagNamePattern.Matches(text))
        {
            var nameGroup = match.Groups["name"];
            if (!nameGroup.Success
                || !string.Equals(nameGroup.Value, tagName, StringComparison.Ordinal))
            {
                continue;
            }

            ranges.Add(LspProtocolHelpers.ToRange(text, nameGroup.Index, nameGroup.Length));
        }

        return ranges
            .GroupBy(
                static range => $"{range.Start.Line}:{range.Start.Character}:{range.End.Line}:{range.End.Character}",
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    private static bool TryFindTagNameAtPosition(
        string text,
        LspPosition position,
        out string tagName)
    {
        var offset = LspProtocolHelpers.GetOffset(text, position);
        foreach (Match match in TagNamePattern.Matches(text))
        {
            var nameGroup = match.Groups["name"];
            if (!nameGroup.Success)
            {
                continue;
            }

            if (offset < nameGroup.Index || offset > nameGroup.Index + nameGroup.Length)
            {
                continue;
            }

            tagName = nameGroup.Value;
            return true;
        }

        tagName = string.Empty;
        return false;
    }

    private static string FormatText(
        string text,
        LspFormattingOptions? options,
        bool ensureFinalNewline)
    {
        var newline = text.Contains("\r\n", StringComparison.Ordinal)
            ? "\r\n"
            : "\n";
        var lines = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n')
            .Select(static line => line.TrimEnd(' ', '\t'))
            .ToArray();
        var formatted = string.Join(newline, lines);
        var shouldInsertFinalNewline = options?.InsertFinalNewline ?? ensureFinalNewline;
        if (shouldInsertFinalNewline
            && !formatted.EndsWith(newline, StringComparison.Ordinal))
        {
            formatted += newline;
        }

        return formatted;
    }

    private static bool IsWordCharacter(char c)
        => char.IsLetterOrDigit(c) || c == '_';

    private async ValueTask<LspResponseMessage> HandleCodeActionAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspCodeActionParams>(request.Params);
        var document = await GetRequiredDocumentAsync(
            GetRequiredTextDocumentUri(parameters.TextDocument),
            cancellationToken);
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
        var textDocument = parameters.TextDocument
            ?? throw CreateInvalidParamsException("textDocument/didOpen textDocument is required.");
        var documentPath = LspProtocolHelpers.ToDocumentPath(GetRequiredTextDocumentUri(textDocument));
        var document = new DocumentSnapshot(
            documentPath,
            MapDocumentKind(textDocument.LanguageId, documentPath),
            textDocument.Text,
            textDocument.Version?.ToString(System.Globalization.CultureInfo.InvariantCulture));
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
        var textDocument = parameters.TextDocument
            ?? throw CreateInvalidParamsException("textDocument/didChange textDocument is required.");
        if (parameters.ContentChanges is null || parameters.ContentChanges.Length == 0)
        {
            throw CreateInvalidParamsException("textDocument/didChange contentChanges is required.");
        }

        var documentPath = LspProtocolHelpers.ToDocumentPath(GetRequiredTextDocumentUri(textDocument));
        var existing = await _workspaceStore.GetDocumentAsync(documentPath, cancellationToken);
        var document = new DocumentSnapshot(
            documentPath,
            existing?.DocumentKind ?? MapDocumentKind(languageId: null, documentPath),
            parameters.ContentChanges.LastOrDefault()?.Text ?? string.Empty,
            textDocument.Version?.ToString(System.Globalization.CultureInfo.InvariantCulture));
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
        var textDocument = parameters.TextDocument
            ?? throw CreateInvalidParamsException("textDocument/didClose textDocument is required.");
        var documentPath = LspProtocolHelpers.ToDocumentPath(GetRequiredTextDocumentUri(textDocument));
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

    private async ValueTask HandleDidSaveAsync(
        LspRequestMessage notification,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspDidSaveTextDocumentParams>(notification.Params);
        var documentPath = LspProtocolHelpers.ToDocumentPath(GetRequiredTextDocumentUri(parameters.TextDocument));
        var document = await _workspaceStore.GetDocumentAsync(documentPath, cancellationToken);
        if (document is null)
        {
            return;
        }

        if (parameters.Text is not null && !string.Equals(parameters.Text, document.Text, StringComparison.Ordinal))
        {
            document = new DocumentSnapshot(
                document.DocumentPath,
                document.DocumentKind,
                parameters.Text,
                document.Version);
            await _workspaceStore.UpsertDocumentAsync(document, cancellationToken);
        }

        VueHostWorkspaceResolver.InvalidatePath(documentPath);
        await PublishDiagnosticsAsync(document, cancellationToken);
        await RefreshOpenJazorDiagnosticsAsync(document, cancellationToken);
    }

    private void HandleWillSave(LspRequestMessage notification)
    {
        var parameters = TryDeserializeParams<LspWillSaveTextDocumentParams>(notification.Params);
        if (parameters?.TextDocument is null)
        {
            return;
        }

        var documentPath = LspProtocolHelpers.ToDocumentPath(GetRequiredTextDocumentUri(parameters.TextDocument));
        VueHostWorkspaceResolver.InvalidatePath(documentPath);
    }

    private async ValueTask HandleDidChangeConfigurationAsync(CancellationToken cancellationToken)
    {
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        foreach (var document in openDocuments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await PublishDiagnosticsAsync(document, cancellationToken);
        }
    }

    private async ValueTask HandleDidChangeWatchedFilesAsync(
        LspRequestMessage notification,
        CancellationToken cancellationToken)
    {
        var parameters = TryDeserializeParams<LspDidChangeWatchedFilesParams>(notification.Params);
        if (parameters?.Changes is null || parameters.Changes.Length == 0)
        {
            return;
        }

        var affectedPaths = parameters.Changes
            .Select(static change => change.Uri)
            .Where(static uri => !string.IsNullOrWhiteSpace(uri))
            .Select(LspProtocolHelpers.ToDocumentPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        foreach (var path in affectedPaths)
        {
            VueHostWorkspaceResolver.InvalidatePath(path);
            var openDocument = await _workspaceStore.GetDocumentAsync(path, cancellationToken);
            if (openDocument is null)
            {
                continue;
            }

            await PublishDiagnosticsAsync(openDocument, cancellationToken);
            await RefreshOpenJazorDiagnosticsAsync(openDocument, cancellationToken);
        }
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
        catch (Exception) {
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

    private IReadOnlyList<string> GetWorkspaceFolderRootPaths()
    {
        lock (_workspaceFoldersGate)
        {
            return _workspaceFoldersByUri.Values
                .Select(static folder => TryResolveWorkspaceFolderRootPath(folder.Uri))
                .Where(static path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(static path => path!)
                .ToArray();
        }
    }

    private static string? TryResolveWorkspaceFolderRootPath(string? workspaceFolderUri)
    {
        if (string.IsNullOrWhiteSpace(workspaceFolderUri))
        {
            return null;
        }

        try
        {
            return Path.GetFullPath(LspProtocolHelpers.ToDocumentPath(workspaceFolderUri));
        }
        catch (Exception) {
            return null;
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
        catch (Exception) {
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

    private IReadOnlyList<ILspLane> GetDocumentLinkLanes(DocumentSnapshot document)
    {
        LaneKind[] laneKinds = document.DocumentKind switch
        {
            DocumentKind.Jazor => [LaneKind.Jazor, LaneKind.Volar],
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

    private IReadOnlyList<ILspLane> GetInlayAndFoldingLanes(DocumentSnapshot document)
    {
        LaneKind[] laneKinds = document.DocumentKind switch
        {
            DocumentKind.Jazor => [LaneKind.Jazor, LaneKind.Volar, LaneKind.Roslyn],
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

    private bool TryGetRoslynLaneService(out RoslynLaneService roslynLane)
    {
        if (_lanes.TryGetValue(LaneKind.Roslyn, out var lane)
            && lane is RoslynLaneService typedLane)
        {
            roslynLane = typedLane;
            return true;
        }

        roslynLane = null!;
        return false;
    }

    private static bool IsRoslynSemanticTarget(
        DocumentSnapshot document,
        ProjectionTarget projectionTarget)
        => document.DocumentKind == DocumentKind.CSharp
            || projectionTarget.LaneKind == LaneKind.Roslyn
            || projectionTarget.RegionKind == DocumentRegionKind.Code;

    private async ValueTask<DocumentSnapshot> GetRequiredDocumentAsync(
        string documentUri,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentUri))
        {
            throw CreateInvalidParamsException("textDocument.uri is required.");
        }

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
        try
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
        catch (LspRequestException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new LspRequestException(
                InvalidParamsErrorCode,
                $"Invalid LSP params payload for '{typeof(TParams).Name}'.",
                exception);
        }
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
        catch (Exception) {
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

    private static string GetRequiredTextDocumentUri(LspTextDocumentIdentifier? textDocument)
    {
        if (textDocument is null)
        {
            throw CreateInvalidParamsException("textDocument is required.");
        }

        if (string.IsNullOrWhiteSpace(textDocument.Uri))
        {
            throw CreateInvalidParamsException("textDocument.uri is required.");
        }

        return textDocument.Uri;
    }

    private static LspRequestException CreateInvalidParamsException(string message)
        => new(InvalidParamsErrorCode, message);

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
