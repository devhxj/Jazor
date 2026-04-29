using System.Diagnostics;
using System.Text.Json;
using Jolt.Extensions;
using Jolt.Jazor.Projection;
using Jolt.Lsp.Aggregation;
using Jolt.Lsp.Coordination;
using Jolt.Lsp.Lanes;
using Jolt.Lsp.Routing;
using Jolt.VirtualDocuments.Registry;
using Jolt.Workspace;
using ECMAScript.Contract.VueContracts.Protocol;

namespace Jolt.Lsp;

internal sealed partial class LspSession
{
    private const int InvalidParamsErrorCode = -32602;
    private readonly IJoltWorkspaceStore _workspaceStore;
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
        IJoltWorkspaceStore workspaceStore,
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
        using var workspaceFolderScope = JoltWorkspaceResolver.PushWorkspaceFolderRoots(GetWorkspaceFolderRootPaths());

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
        using var workspaceFolderScope = JoltWorkspaceResolver.PushWorkspaceFolderRoots(GetWorkspaceFolderRootPaths());

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
                    Name = "Jolt",
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
        if (!IsValidRenameIdentifier(parameters.NewName))
        {
            throw CreateInvalidParamsException(
                "textDocument/rename newName must be a valid identifier containing only letters, digits, and underscores, and must start with a letter or underscore.");
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

    private static bool IsValidRenameIdentifier(string? newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return false;
        }

        if (!(char.IsLetter(newName[0]) || newName[0] == '_'))
        {
            return false;
        }

        for (var index = 1; index < newName.Length; index++)
        {
            var character = newName[index];
            if (!(char.IsLetterOrDigit(character) || character == '_'))
            {
                return false;
            }
        }

        return true;
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
        var probeOffset = GetRenameProbeOffset(document.Text, LspProtocolHelpers.GetOffset(document.Text, parameters.Position));
        var probePosition = LspProtocolHelpers.GetPosition(document.Text, probeOffset);
        var (rangeStart, rangeLength) = GetRenameTokenBounds(document.Text, probeOffset);
        if (rangeLength == 0)
        {
            return CreateSuccessResponse(request.Id, result: null);
        }

        // Try each lane to see if the position is renamable
        foreach (var lane in GetOrderedLanes(projectionTarget))
        {
            var renameEdit = await lane.GetRenameAsync(
                document,
                probePosition,
                "__prepare__",
                projectionTarget,
                cancellationToken);

            if (renameEdit is not null)
            {
                // A lane confirmed the position is renamable — return the placeholder range
                return CreateSuccessResponse(request.Id, new LspPrepareRenameResult
                {
                    Range = LspProtocolHelpers.ToRange(document.Text, rangeStart, rangeLength),
                    Placeholder = document.Text.Substring(rangeStart, rangeLength)
                });
            }
        }

        return CreateSuccessResponse(request.Id, result: null);
    }


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
        var documentPath = GetWorkspaceScopedDocumentPath(GetRequiredTextDocumentUri(textDocument));
        var document = new DocumentSnapshot(
            documentPath,
            MapDocumentKind(textDocument.LanguageId, documentPath),
            textDocument.Text,
            textDocument.Version?.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await _workspaceStore.UpsertDocumentAsync(document, cancellationToken);
        JoltWorkspaceResolver.InvalidatePath(documentPath);
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

        var documentPath = GetWorkspaceScopedDocumentPath(GetRequiredTextDocumentUri(textDocument));
        var existing = await _workspaceStore.GetDocumentAsync(documentPath, cancellationToken);
        var document = new DocumentSnapshot(
            documentPath,
            existing?.DocumentKind ?? MapDocumentKind(languageId: null, documentPath),
            parameters.ContentChanges.LastOrDefault()?.Text ?? string.Empty,
            textDocument.Version?.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await _workspaceStore.UpsertDocumentAsync(document, cancellationToken);
        JoltWorkspaceResolver.InvalidatePath(documentPath);
        await UpdateProjectionStateAsync(document, cancellationToken);
        await PublishDiagnosticsAsync(document, cancellationToken);
        await RefreshOpenJazorDiagnosticsAsync(document, cancellationToken);
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        await NotifyWorkspaceDocumentChangedAsync(
            document,
            FilterOpenDocumentsToProjectScope(document, openDocuments),
            cancellationToken);
    }

    private async ValueTask HandleDidCloseAsync(
        LspRequestMessage notification,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspDidCloseTextDocumentParams>(notification.Params);
        var textDocument = parameters.TextDocument
            ?? throw CreateInvalidParamsException("textDocument/didClose textDocument is required.");
        var documentPath = GetWorkspaceScopedDocumentPath(GetRequiredTextDocumentUri(textDocument));
        await _workspaceStore.RemoveDocumentAsync(documentPath, cancellationToken);
        JoltWorkspaceResolver.InvalidatePath(documentPath);
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
        var documentPath = GetWorkspaceScopedDocumentPath(GetRequiredTextDocumentUri(parameters.TextDocument));
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

        JoltWorkspaceResolver.InvalidatePath(documentPath);
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

        var documentPath = GetWorkspaceScopedDocumentPath(GetRequiredTextDocumentUri(parameters.TextDocument));
        JoltWorkspaceResolver.InvalidatePath(documentPath);
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
            .Select(uri => TryNormalizeWorkspaceDocumentPath(uri, out var path) ? path : null)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Select(static path => path!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        foreach (var path in affectedPaths)
        {
            JoltWorkspaceResolver.InvalidatePath(path);
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
        catch (Exception exception)
        {
            WriteWorkspaceSinkWarning(document.DocumentPath, exception);
            // LSP diagnostics/projection updates must keep working even if dev-server HMR coordination fails.
        }
    }

    private static IReadOnlyList<DocumentSnapshot> FilterOpenDocumentsToProjectScope(
        DocumentSnapshot document,
        IReadOnlyList<DocumentSnapshot> openDocuments)
    {
        // 共享 Jolt 进程可以同时服务多个项目，但工作区变更广播只应该看到
        // 当前文档所属项目内的打开文档，避免兄弟项目被无关刷新拖进来。
        var filtered = openDocuments
            .Where(candidate => JoltWorkspaceResolver.IsInSameProjectScope(document.DocumentPath, candidate.DocumentPath))
            .ToArray();
        return filtered.Length == openDocuments.Count
            ? openDocuments
            : filtered;
    }

    private string GetWorkspaceScopedDocumentPath(string documentUri)
    {
        string documentPath;
        try
        {
            documentPath = LspProtocolHelpers.ToDocumentPath(documentUri);
        }
        catch (UriFormatException)
        {
            throw CreateInvalidParamsException("textDocument.uri must be a valid file URI or path.");
        }
        catch (ArgumentException)
        {
            throw CreateInvalidParamsException("textDocument.uri must be a valid file URI or path.");
        }
        catch (InvalidOperationException)
        {
            throw CreateInvalidParamsException("textDocument.uri must be a valid file URI or path.");
        }
        catch (NotSupportedException)
        {
            throw CreateInvalidParamsException("textDocument.uri must be a valid file URI or path.");
        }
        catch (PathTooLongException)
        {
            throw CreateInvalidParamsException("textDocument.uri must be a valid file URI or path.");
        }

        if (!IsInsideWorkspaceRoots(documentPath))
        {
            throw new InvalidOperationException($"Document '{documentPath}' is outside the configured workspace folders.");
        }

        return documentPath;
    }

    private bool TryNormalizeWorkspaceDocumentPath(string? documentUri, out string? documentPath)
    {
        documentPath = null;
        if (string.IsNullOrWhiteSpace(documentUri))
        {
            return false;
        }

        try
        {
            documentPath = LspProtocolHelpers.ToDocumentPath(documentUri);
        }
        catch (UriFormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
        catch (PathTooLongException)
        {
            return false;
        }

        return IsInsideWorkspaceRoots(documentPath);
    }

    private static void WriteWorkspaceSinkWarning(string documentPath, Exception exception)
    {
        try
        {
            Console.Error.WriteLine(JsonSerializer.Serialize(new
            {
                eventType = "lspWorkspaceDocumentChangeSinkFailed",
                documentPath,
                errorType = exception.GetType().FullName ?? exception.GetType().Name,
                message = exception.Message,
                timestamp = DateTimeOffset.UtcNow
            }));
        }
        catch (Exception)
        {
            // Keep diagnostics/projection work isolated from observability failures.
        }
    }

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
