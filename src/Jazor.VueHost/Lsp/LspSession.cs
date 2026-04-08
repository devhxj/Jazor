using System.Text.Json;
using Jazor.VueHost.Workspace;
using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Lsp;

internal sealed class LspSession
{
    private readonly IVueHostWorkspaceStore _workspaceStore;
    private readonly JazorLspDocumentService _documentService;
    private readonly LspMessageWriter _writer;

    public LspSession(
        IVueHostWorkspaceStore workspaceStore,
        JazorLspDocumentService documentService,
        LspMessageWriter writer)
    {
        _workspaceStore = workspaceStore ?? throw new ArgumentNullException(nameof(workspaceStore));
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    public async ValueTask<LspResponseMessage?> HandleRequestAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return request.Method switch
        {
            "initialize" => CreateSuccessResponse(
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
                        RenameProvider = true,
                        CodeActionProvider = true,
                        CompletionProvider = new LspCompletionOptions
                        {
                            ResolveProvider = false,
                            TriggerCharacters = ["@", "<", "/"]
                        }
                    },
                    ServerInfo = new LspServerInfo
                    {
                        Name = "Jazor.VueHost",
                        Version = "0.1"
                    }
                }),
            "shutdown" => CreateSuccessResponse(request.Id, result: null),
            "textDocument/hover" => await HandleHoverAsync(request, cancellationToken),
            "textDocument/completion" => await HandleCompletionAsync(request, cancellationToken),
            "textDocument/definition" => await HandleDefinitionAsync(request, cancellationToken),
            "textDocument/references" => await HandleReferencesAsync(request, cancellationToken),
            "textDocument/rename" => await HandleRenameAsync(request, cancellationToken),
            "textDocument/codeAction" => await HandleCodeActionAsync(request, cancellationToken),
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
            default:
                return true;
        }
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
        return CreateSuccessResponse(
            request.Id,
            await _documentService.GetHoverAsync(document, parameters.Position, cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleCompletionAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspCompletionParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await _documentService.GetCompletionItemsAsync(document, parameters.Position, cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleDefinitionAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspDefinitionParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await _documentService.GetDefinitionAsync(document, parameters.Position, cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleReferencesAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspReferenceParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await _documentService.GetReferencesAsync(
                document,
                parameters.Position,
                parameters.Context?.IncludeDeclaration ?? true,
                cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleRenameAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspRenameParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await _documentService.GetRenameAsync(document, parameters.Position, parameters.NewName, cancellationToken));
    }

    private async ValueTask<LspResponseMessage> HandleCodeActionAsync(
        LspRequestMessage request,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspCodeActionParams>(request.Params);
        var document = await GetRequiredDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        return CreateSuccessResponse(
            request.Id,
            await _documentService.GetCodeActionsAsync(document, parameters.Context?.Diagnostics ?? [], cancellationToken));
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
        await PublishDiagnosticsAsync(document, cancellationToken);
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
        await PublishDiagnosticsAsync(document, cancellationToken);
    }

    private async ValueTask HandleDidCloseAsync(
        LspRequestMessage notification,
        CancellationToken cancellationToken)
    {
        var parameters = DeserializeParams<LspDidCloseTextDocumentParams>(notification.Params);
        var documentPath = LspProtocolHelpers.ToDocumentPath(parameters.TextDocument.Uri);
        await _workspaceStore.RemoveDocumentAsync(documentPath, cancellationToken);
        await PublishDiagnosticsAsync(
            new DocumentSnapshot(documentPath, MapDocumentKind(languageId: null, documentPath), string.Empty, null),
            cancellationToken,
            diagnostics: Array.Empty<LspDiagnostic>());
    }

    private async ValueTask PublishDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken,
        IReadOnlyList<LspDiagnostic>? diagnostics = null)
    {
        if (document.DocumentKind != DocumentKind.Jazor)
        {
            return;
        }

        diagnostics ??= await _documentService.GetDiagnosticsAsync(document, cancellationToken);
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

    private static DocumentKind MapDocumentKind(string? languageId, string documentPath)
        => languageId?.ToLowerInvariant() switch
        {
            "jazor" => DocumentKind.Jazor,
            "vue" => DocumentKind.Vue,
            "javascript" => DocumentKind.JavaScript,
            "typescript" => DocumentKind.TypeScript,
            _ => Path.GetExtension(documentPath).ToLowerInvariant() switch
            {
                ".jazor" => DocumentKind.Jazor,
                ".vue" => DocumentKind.Vue,
                ".js" => DocumentKind.JavaScript,
                ".ts" => DocumentKind.TypeScript,
                _ => DocumentKind.Unknown
            }
        };
}
