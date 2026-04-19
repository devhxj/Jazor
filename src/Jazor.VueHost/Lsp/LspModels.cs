using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jazor.VueHost.Lsp;

internal sealed class LspRequestMessage
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public object? Id { get; set; }

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public object? Params { get; set; }
}

internal sealed class LspResponseMessage
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public object? Id { get; set; }

    [JsonPropertyName("result")]
    public object? Result { get; set; }

    [JsonPropertyName("error")]
    public LspResponseError? Error { get; set; }
}

internal sealed class LspResponseError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

internal sealed class LspNotificationMessage
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public object? Params { get; set; }
}

internal sealed class LspInitializeResult
{
    [JsonPropertyName("capabilities")]
    public required LspServerCapabilities Capabilities { get; init; }

    [JsonPropertyName("serverInfo")]
    public required LspServerInfo ServerInfo { get; init; }
}

internal sealed class LspServerInfo
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }
}

internal sealed class LspServerCapabilities
{
    [JsonPropertyName("textDocumentSync")]
    public required LspTextDocumentSyncOptions TextDocumentSync { get; init; }

    [JsonPropertyName("hoverProvider")]
    public bool HoverProvider { get; init; }

    [JsonPropertyName("documentHighlightProvider")]
    public bool DocumentHighlightProvider { get; init; }

    [JsonPropertyName("documentLinkProvider")]
    public bool DocumentLinkProvider { get; init; }

    [JsonPropertyName("definitionProvider")]
    public bool DefinitionProvider { get; init; }

    [JsonPropertyName("typeDefinitionProvider")]
    public bool TypeDefinitionProvider { get; init; }

    [JsonPropertyName("implementationProvider")]
    public bool ImplementationProvider { get; init; }

    [JsonPropertyName("selectionRangeProvider")]
    public bool SelectionRangeProvider { get; init; }

    [JsonPropertyName("linkedEditingRangeProvider")]
    public bool LinkedEditingRangeProvider { get; init; }

    [JsonPropertyName("referencesProvider")]
    public bool ReferencesProvider { get; init; }

    [JsonPropertyName("renameProvider")]
    public LspRenameOptions? RenameProvider { get; init; }

    [JsonPropertyName("codeActionProvider")]
    public bool CodeActionProvider { get; init; }

    [JsonPropertyName("codeLensProvider")]
    public bool CodeLensProvider { get; init; }

    [JsonPropertyName("documentSymbolProvider")]
    public bool DocumentSymbolProvider { get; init; }

    [JsonPropertyName("documentFormattingProvider")]
    public bool DocumentFormattingProvider { get; init; }

    [JsonPropertyName("documentRangeFormattingProvider")]
    public bool DocumentRangeFormattingProvider { get; init; }

    [JsonPropertyName("signatureHelpProvider")]
    public LspSignatureHelpOptions? SignatureHelpProvider { get; init; }

    [JsonPropertyName("workspaceSymbolProvider")]
    public bool WorkspaceSymbolProvider { get; init; }

    [JsonPropertyName("foldingRangeProvider")]
    public bool FoldingRangeProvider { get; init; }

    [JsonPropertyName("inlayHintProvider")]
    public bool InlayHintProvider { get; init; }

    [JsonPropertyName("callHierarchyProvider")]
    public bool CallHierarchyProvider { get; init; }

    [JsonPropertyName("typeHierarchyProvider")]
    public bool TypeHierarchyProvider { get; init; }

    [JsonPropertyName("completionProvider")]
    public required LspCompletionOptions CompletionProvider { get; init; }

    [JsonPropertyName("semanticTokensProvider")]
    public LspSemanticTokensOptions? SemanticTokensProvider { get; init; }

    [JsonPropertyName("workspace")]
    public LspWorkspaceServerCapabilities? Workspace { get; init; }
}

internal sealed class LspTextDocumentSyncOptions
{
    [JsonPropertyName("openClose")]
    public bool OpenClose { get; init; }

    [JsonPropertyName("change")]
    public int Change { get; init; }

    [JsonPropertyName("save")]
    public bool Save { get; init; }
}

internal sealed class LspCompletionOptions
{
    [JsonPropertyName("resolveProvider")]
    public bool ResolveProvider { get; init; }

    [JsonPropertyName("triggerCharacters")]
    public required string[] TriggerCharacters { get; init; }
}

internal sealed class LspSignatureHelpOptions
{
    [JsonPropertyName("triggerCharacters")]
    public required string[] TriggerCharacters { get; init; }

    [JsonPropertyName("retriggerCharacters")]
    public string[]? RetriggerCharacters { get; init; }
}

internal sealed class LspRenameOptions
{
    [JsonPropertyName("prepareProvider")]
    public bool PrepareProvider { get; init; }
}

internal sealed class LspSemanticTokensOptions
{
    [JsonPropertyName("legend")]
    public required LspSemanticTokensLegendDescriptor Legend { get; init; }

    [JsonPropertyName("full")]
    public bool Full { get; init; }

    [JsonPropertyName("range")]
    public bool Range { get; init; }
}

internal sealed class LspWorkspaceServerCapabilities
{
    [JsonPropertyName("workspaceFolders")]
    public LspWorkspaceFoldersServerCapabilities? WorkspaceFolders { get; init; }
}

internal sealed class LspWorkspaceFoldersServerCapabilities
{
    [JsonPropertyName("supported")]
    public bool Supported { get; init; }

    [JsonPropertyName("changeNotifications")]
    public bool ChangeNotifications { get; init; }
}

internal sealed class LspInitializeParams
{
    [JsonPropertyName("rootUri")]
    public string? RootUri { get; init; }

    [JsonPropertyName("rootPath")]
    public string? RootPath { get; init; }

    [JsonPropertyName("workspaceFolders")]
    public LspWorkspaceFolder[]? WorkspaceFolders { get; init; }
}

internal sealed class LspWorkspaceFolder
{
    [JsonPropertyName("uri")]
    public string Uri { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}

internal sealed class LspDidChangeWorkspaceFoldersParams
{
    [JsonPropertyName("event")]
    public LspWorkspaceFoldersChangeEvent? Event { get; init; }
}

internal sealed class LspWorkspaceFoldersChangeEvent
{
    [JsonPropertyName("added")]
    public LspWorkspaceFolder[]? Added { get; init; }

    [JsonPropertyName("removed")]
    public LspWorkspaceFolder[]? Removed { get; init; }
}

internal sealed class LspCancelRequestParams
{
    [JsonPropertyName("id")]
    public object? Id { get; init; }
}

internal sealed class LspSemanticTokensLegendDescriptor
{
    [JsonPropertyName("tokenTypes")]
    public required string[] TokenTypes { get; init; }

    [JsonPropertyName("tokenModifiers")]
    public required string[] TokenModifiers { get; init; }
}

internal class LspTextDocumentIdentifier
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
}

internal class LspVersionedTextDocumentIdentifier : LspTextDocumentIdentifier
{
    [JsonPropertyName("version")]
    public int? Version { get; set; }
}

internal class LspTextDocumentItem : LspVersionedTextDocumentIdentifier
{
    [JsonPropertyName("languageId")]
    public string LanguageId { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

internal sealed class LspDidOpenTextDocumentParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentItem TextDocument { get; init; }
}

internal sealed class LspTextDocumentContentChangeEvent
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

internal sealed class LspDidChangeTextDocumentParams
{
    [JsonPropertyName("textDocument")]
    public required LspVersionedTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("contentChanges")]
    public required LspTextDocumentContentChangeEvent[] ContentChanges { get; init; }
}

internal sealed class LspDidCloseTextDocumentParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }
}

internal sealed class LspHoverParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required LspPosition Position { get; init; }
}

internal sealed class LspDocumentHighlightParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required LspPosition Position { get; init; }
}

internal sealed class LspCompletionParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required LspPosition Position { get; init; }
}

internal sealed class LspDocumentLinkParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }
}

internal sealed class LspSignatureHelpParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required LspPosition Position { get; init; }
}

internal sealed class LspDefinitionParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required LspPosition Position { get; init; }
}

internal sealed class LspImplementationParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required LspPosition Position { get; init; }
}

internal sealed class LspTypeDefinitionParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required LspPosition Position { get; init; }
}

internal sealed class LspSelectionRangeParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("positions")]
    public required LspPosition[] Positions { get; init; }
}

internal sealed class LspSelectionRange
{
    [JsonPropertyName("range")]
    public required LspRange Range { get; init; }

    [JsonPropertyName("parent")]
    public LspSelectionRange? Parent { get; init; }
}

internal sealed class LspLinkedEditingRangeParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required LspPosition Position { get; init; }
}

internal sealed class LspLinkedEditingRanges
{
    [JsonPropertyName("ranges")]
    public required LspRange[] Ranges { get; init; }

    [JsonPropertyName("wordPattern")]
    public string? WordPattern { get; init; }
}

internal sealed class LspFormattingOptions
{
    [JsonPropertyName("tabSize")]
    public int TabSize { get; init; } = 4;

    [JsonPropertyName("insertSpaces")]
    public bool InsertSpaces { get; init; } = true;

    [JsonPropertyName("trimTrailingWhitespace")]
    public bool? TrimTrailingWhitespace { get; init; }

    [JsonPropertyName("insertFinalNewline")]
    public bool? InsertFinalNewline { get; init; }
}

internal sealed class LspDocumentFormattingParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("options")]
    public LspFormattingOptions? Options { get; init; }
}

internal sealed class LspDocumentRangeFormattingParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("range")]
    public required LspRange Range { get; init; }

    [JsonPropertyName("options")]
    public LspFormattingOptions? Options { get; init; }
}

internal sealed class LspCodeLensParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }
}

internal sealed class LspCallHierarchyPrepareParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required LspPosition Position { get; init; }
}

internal sealed class LspTypeHierarchyPrepareParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required LspPosition Position { get; init; }
}

internal sealed class LspTypeHierarchyParams
{
    [JsonPropertyName("item")]
    public required LspTypeHierarchyItem Item { get; init; }
}

internal sealed class LspCallHierarchyIncomingCallsParams
{
    [JsonPropertyName("item")]
    public required LspCallHierarchyItem Item { get; init; }
}

internal sealed class LspCallHierarchyOutgoingCallsParams
{
    [JsonPropertyName("item")]
    public required LspCallHierarchyItem Item { get; init; }
}

internal sealed class LspDidSaveTextDocumentParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }
}

internal sealed class LspWillSaveTextDocumentParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("reason")]
    public int Reason { get; init; }
}

internal sealed class LspDidChangeConfigurationParams
{
    [JsonPropertyName("settings")]
    public JsonElement? Settings { get; init; }
}

internal sealed class LspDidChangeWatchedFilesParams
{
    [JsonPropertyName("changes")]
    public LspFileEvent[]? Changes { get; init; }
}

internal sealed class LspFileEvent
{
    [JsonPropertyName("uri")]
    public string Uri { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public int Type { get; init; }
}

internal sealed class LspReferenceContext
{
    [JsonPropertyName("includeDeclaration")]
    public bool IncludeDeclaration { get; init; } = true;
}

internal sealed class LspReferenceParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required LspPosition Position { get; init; }

    [JsonPropertyName("context")]
    public LspReferenceContext? Context { get; init; }
}

internal sealed class LspRenameParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required LspPosition Position { get; init; }

    [JsonPropertyName("newName")]
    public required string NewName { get; init; }
}

internal sealed class LspPrepareRenameParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("position")]
    public required LspPosition Position { get; init; }
}

internal sealed class LspPrepareRenameResult
{
    [JsonPropertyName("range")]
    public required LspRange Range { get; init; }

    [JsonPropertyName("placeholder")]
    public required string Placeholder { get; init; }
}

internal sealed class LspCodeActionContext
{
    [JsonPropertyName("diagnostics")]
    public LspDiagnostic[] Diagnostics { get; init; } = [];
}

internal sealed class LspCodeActionParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("range")]
    public required LspRange Range { get; init; }

    [JsonPropertyName("context")]
    public LspCodeActionContext? Context { get; init; }
}

internal sealed class LspDocumentSymbolParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }
}

internal sealed class LspSemanticTokensParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }
}

internal sealed class LspFoldingRangeParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }
}

internal sealed class LspInlayHintParams
{
    [JsonPropertyName("textDocument")]
    public required LspTextDocumentIdentifier TextDocument { get; init; }

    [JsonPropertyName("range")]
    public required LspRange Range { get; init; }
}

internal sealed class LspWorkspaceSymbolParams
{
    [JsonPropertyName("query")]
    public required string Query { get; init; }
}

internal sealed class LspPublishDiagnosticsParams
{
    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    [JsonPropertyName("diagnostics")]
    public required LspDiagnostic[] Diagnostics { get; init; }
}

internal sealed class LspDiagnostic
{
    [JsonPropertyName("range")]
    public required LspRange Range { get; init; }

    [JsonPropertyName("severity")]
    public int Severity { get; init; }

    [JsonPropertyName("code")]
    public required string Code { get; init; }

    [JsonPropertyName("source")]
    public required string Source { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}

internal sealed class LspHoverResult
{
    [JsonPropertyName("contents")]
    public required LspMarkupContent Contents { get; init; }

    [JsonPropertyName("range")]
    public LspRange? Range { get; init; }
}

internal sealed class LspMarkupContent
{
    [JsonPropertyName("kind")]
    public required string Kind { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }
}

internal sealed class LspCompletionItem
{
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    [JsonPropertyName("kind")]
    public int Kind { get; init; }

    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    [JsonPropertyName("documentation")]
    public string? Documentation { get; init; }
}

internal sealed class LspSignatureHelp
{
    [JsonPropertyName("signatures")]
    public required LspSignatureInformation[] Signatures { get; init; }

    [JsonPropertyName("activeSignature")]
    public int ActiveSignature { get; init; }

    [JsonPropertyName("activeParameter")]
    public int ActiveParameter { get; init; }
}

internal sealed class LspSignatureInformation
{
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    [JsonPropertyName("documentation")]
    public string? Documentation { get; init; }

    [JsonPropertyName("parameters")]
    public LspParameterInformation[]? Parameters { get; init; }
}

internal sealed class LspParameterInformation
{
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    [JsonPropertyName("documentation")]
    public string? Documentation { get; init; }
}

internal sealed class LspLocation
{
    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    [JsonPropertyName("range")]
    public required LspRange Range { get; init; }
}

internal sealed class LspDocumentHighlight
{
    [JsonPropertyName("range")]
    public required LspRange Range { get; init; }

    [JsonPropertyName("kind")]
    public int Kind { get; init; }
}

internal sealed class LspDocumentLink
{
    [JsonPropertyName("range")]
    public required LspRange Range { get; init; }

    [JsonPropertyName("target")]
    public string? Target { get; init; }

    [JsonPropertyName("tooltip")]
    public string? Tooltip { get; init; }
}

internal sealed class LspTextEdit
{
    [JsonPropertyName("range")]
    public required LspRange Range { get; init; }

    [JsonPropertyName("newText")]
    public required string NewText { get; init; }
}

internal sealed class LspWorkspaceEdit
{
    [JsonPropertyName("changes")]
    public required Dictionary<string, LspTextEdit[]> Changes { get; init; }
}

internal sealed class LspCodeAction
{
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("kind")]
    public string? Kind { get; init; }

    [JsonPropertyName("edit")]
    public LspWorkspaceEdit? Edit { get; init; }
}

internal sealed class LspCodeLens
{
    [JsonPropertyName("range")]
    public required LspRange Range { get; init; }
}

internal sealed class LspCallHierarchyItem
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("kind")]
    public int Kind { get; init; }

    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    [JsonPropertyName("range")]
    public required LspRange Range { get; init; }

    [JsonPropertyName("selectionRange")]
    public required LspRange SelectionRange { get; init; }

    [JsonPropertyName("detail")]
    public string? Detail { get; init; }
}

internal sealed class LspCallHierarchyIncomingCall
{
    [JsonPropertyName("from")]
    public required LspCallHierarchyItem From { get; init; }

    [JsonPropertyName("fromRanges")]
    public required LspRange[] FromRanges { get; init; }
}

internal sealed class LspCallHierarchyOutgoingCall
{
    [JsonPropertyName("to")]
    public required LspCallHierarchyItem To { get; init; }

    [JsonPropertyName("fromRanges")]
    public required LspRange[] FromRanges { get; init; }
}

internal sealed class LspTypeHierarchyItem
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("kind")]
    public int Kind { get; init; }

    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    [JsonPropertyName("range")]
    public required LspRange Range { get; init; }

    [JsonPropertyName("selectionRange")]
    public required LspRange SelectionRange { get; init; }

    [JsonPropertyName("detail")]
    public string? Detail { get; init; }
}

internal sealed class LspDocumentSymbol
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    [JsonPropertyName("kind")]
    public int Kind { get; init; }

    [JsonPropertyName("range")]
    public required LspRange Range { get; init; }

    [JsonPropertyName("selectionRange")]
    public required LspRange SelectionRange { get; init; }

    [JsonPropertyName("children")]
    public LspDocumentSymbol[]? Children { get; init; }
}

internal sealed class LspInlayHint
{
    [JsonPropertyName("position")]
    public required LspPosition Position { get; init; }

    [JsonPropertyName("label")]
    public required string Label { get; init; }

    [JsonPropertyName("kind")]
    public int? Kind { get; init; }
}

internal sealed class LspWorkspaceSymbol
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("kind")]
    public int Kind { get; init; }

    [JsonPropertyName("location")]
    public required LspLocation Location { get; init; }

    [JsonPropertyName("containerName")]
    public string? ContainerName { get; init; }
}

internal sealed class LspFoldingRange
{
    [JsonPropertyName("startLine")]
    public int StartLine { get; init; }

    [JsonPropertyName("startCharacter")]
    public int? StartCharacter { get; init; }

    [JsonPropertyName("endLine")]
    public int EndLine { get; init; }

    [JsonPropertyName("endCharacter")]
    public int? EndCharacter { get; init; }

    [JsonPropertyName("kind")]
    public string? Kind { get; init; }
}

internal sealed class LspSemanticTokensResult
{
    [JsonPropertyName("data")]
    public required int[] Data { get; init; }
}

internal sealed class LspSemanticToken
{
    [JsonPropertyName("line")]
    public int Line { get; init; }

    [JsonPropertyName("character")]
    public int Character { get; init; }

    [JsonPropertyName("length")]
    public int Length { get; init; }

    [JsonPropertyName("tokenType")]
    public required string TokenType { get; init; }

    [JsonPropertyName("tokenModifiers")]
    public string[] TokenModifiers { get; init; } = [];
}

internal sealed class LspRange
{
    [JsonPropertyName("start")]
    public required LspPosition Start { get; init; }

    [JsonPropertyName("end")]
    public required LspPosition End { get; init; }
}

internal sealed class LspPosition
{
    [JsonPropertyName("line")]
    public int Line { get; init; }

    [JsonPropertyName("character")]
    public int Character { get; init; }
}
