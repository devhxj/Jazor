using System.Text.Json;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Routing;
using Jazor.VueHost.VirtualDocuments.Mapping;
using Jazor.VueHost.VirtualDocuments.Registry;

namespace Jazor.VueHost.LanguageServers;

internal sealed class ProjectedLanguageServerLaneHost : IAsyncDisposable
{
    private readonly string _rootPath;
    private readonly string _languageId;
    private readonly IVirtualDocumentRegistry _virtualDocumentRegistry;
    private readonly ExternalLspClient _client;
    private readonly Dictionary<string, DocumentSyncState> _synchronizedDocuments =
        new(StringComparer.OrdinalIgnoreCase);

    public ProjectedLanguageServerLaneHost(
        string rootPath,
        string languageId,
        IVirtualDocumentRegistry virtualDocumentRegistry,
        ExternalLspClient client)
    {
        _rootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
        _languageId = languageId ?? throw new ArgumentNullException(nameof(languageId));
        _virtualDocumentRegistry = virtualDocumentRegistry ?? throw new ArgumentNullException(nameof(virtualDocumentRegistry));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask<LspHoverResult?> GetHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        var context = await PrepareContextAsync(document, projectionTarget, cancellationToken);
        if (context is null)
        {
            return null;
        }

        await EnsureInitializedAndSynchronizedAsync(context, cancellationToken);
        var result = await _client.SendRequestAsync<JsonElement?>(
            "textDocument/hover",
            new
            {
                textDocument = new { uri = context.ProjectedUri },
                position = context.ProjectedPosition
            },
            cancellationToken);

        return result is null ? null : MapHover(context, result.Value);
    }

    public async ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        var context = await PrepareContextAsync(document, projectionTarget, cancellationToken);
        if (context is null)
        {
            return Array.Empty<LspCompletionItem>();
        }

        await EnsureInitializedAndSynchronizedAsync(context, cancellationToken);
        var result = await _client.SendRequestAsync<JsonElement?>(
            "textDocument/completion",
            new
            {
                textDocument = new { uri = context.ProjectedUri },
                position = context.ProjectedPosition
            },
            cancellationToken);

        return NormalizeCompletionItems(result);
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        var context = await PrepareContextAsync(document, projectionTarget, cancellationToken);
        if (context is null)
        {
            return Array.Empty<LspLocation>();
        }

        await EnsureInitializedAndSynchronizedAsync(context, cancellationToken);
        var result = await _client.SendRequestAsync<JsonElement?>(
            "textDocument/definition",
            new
            {
                textDocument = new { uri = context.ProjectedUri },
                position = context.ProjectedPosition
            },
            cancellationToken);

        return MapLocations(context, NormalizeLocations(result));
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        var context = await PrepareContextAsync(document, projectionTarget, cancellationToken);
        if (context is null)
        {
            return Array.Empty<LspLocation>();
        }

        await EnsureInitializedAndSynchronizedAsync(context, cancellationToken);
        var result = await _client.SendRequestAsync<JsonElement?>(
            "textDocument/references",
            new
            {
                textDocument = new { uri = context.ProjectedUri },
                position = context.ProjectedPosition,
                context = new
                {
                    includeDeclaration
                }
            },
            cancellationToken);

        return MapLocations(context, NormalizeLocations(result));
    }

    public async ValueTask<LspWorkspaceEdit?> GetRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        var context = await PrepareContextAsync(document, projectionTarget, cancellationToken);
        if (context is null)
        {
            return null;
        }

        await EnsureInitializedAndSynchronizedAsync(context, cancellationToken);
        var result = await _client.SendRequestAsync<JsonElement?>(
            "textDocument/rename",
            new
            {
                textDocument = new { uri = context.ProjectedUri },
                position = context.ProjectedPosition,
                newName
            },
            cancellationToken);

        return result is null
            ? null
            : MapWorkspaceEdit(context, Deserialize<LspWorkspaceEdit>(result.Value));
    }

    public async ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(
        DocumentSnapshot document,
        LspRange range,
        IReadOnlyList<LspDiagnostic> diagnostics,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        var context = await PrepareContextAsync(document, projectionTarget, cancellationToken);
        if (context is null)
        {
            return Array.Empty<LspCodeAction>();
        }

        if (!TryGetProjectedRange(context, range, out var projectedRange))
        {
            return Array.Empty<LspCodeAction>();
        }

        await EnsureInitializedAndSynchronizedAsync(context, cancellationToken);
        var result = await _client.SendRequestAsync<JsonElement?>(
            "textDocument/codeAction",
            new
            {
                textDocument = new { uri = context.ProjectedUri },
                range = projectedRange,
                context = new
                {
                    diagnostics = Array.Empty<object>()
                }
            },
            cancellationToken);

        var actions = NormalizeCodeActions(result);
        return actions
            .Select(action => action.Edit is null
                ? action
                : new LspCodeAction
                {
                    Title = action.Title,
                    Kind = action.Kind,
                    Edit = MapWorkspaceEdit(context, action.Edit)
                })
            .ToArray();
    }

    private async ValueTask EnsureInitializedAndSynchronizedAsync(
        ProjectedDocumentContext context,
        CancellationToken cancellationToken)
    {
        await _client.InitializeAsync(_rootPath, cancellationToken);
        await SynchronizeDocumentAsync(context, cancellationToken);
    }

    private async ValueTask SynchronizeDocumentAsync(
        ProjectedDocumentContext context,
        CancellationToken cancellationToken)
    {
        if (!_synchronizedDocuments.TryGetValue(context.ProjectedUri, out var existing))
        {
            await _client.SendNotificationAsync(
                "textDocument/didOpen",
                new
                {
                    textDocument = new
                    {
                        uri = context.ProjectedUri,
                        languageId = _languageId,
                        version = ParseVersion(context.Version),
                        text = context.ProjectedText
                    }
                },
                cancellationToken);
            _synchronizedDocuments[context.ProjectedUri] = new DocumentSyncState(context.ProjectedText, context.Version);
            return;
        }

        if (string.Equals(existing.Text, context.ProjectedText, StringComparison.Ordinal)
            && string.Equals(existing.Version, context.Version, StringComparison.Ordinal))
        {
            return;
        }

        await _client.SendNotificationAsync(
            "textDocument/didChange",
            new
            {
                textDocument = new
                {
                    uri = context.ProjectedUri,
                    version = ParseVersion(context.Version)
                },
                contentChanges = new[]
                {
                    new
                    {
                        text = context.ProjectedText
                    }
                }
            },
            cancellationToken);
        _synchronizedDocuments[context.ProjectedUri] = new DocumentSyncState(context.ProjectedText, context.Version);
    }

    private async ValueTask<ProjectedDocumentContext?> PrepareContextAsync(
        DocumentSnapshot document,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (projectionTarget.IsProjected)
        {
            var virtualDocument = await _virtualDocumentRegistry.GetByProjectedDocumentAsync(
                projectionTarget.ProjectedDocumentPath,
                cancellationToken);
            if (virtualDocument is null || projectionTarget.ProjectedPosition is null)
            {
                return null;
            }

            return new ProjectedDocumentContext(
                document,
                LspProtocolHelpers.ToDocumentUri(virtualDocument.Identity.ProjectedDocumentPath),
                virtualDocument.Text,
                virtualDocument.Version ?? document.Version,
                projectionTarget.ProjectedPosition,
                virtualDocument.ProjectionMap);
        }

        return new ProjectedDocumentContext(
            document,
            LspProtocolHelpers.ToDocumentUri(document.DocumentPath),
            document.Text,
            document.Version,
            projectionTarget.ProjectedPosition ?? new LspPosition
            {
                Line = 0,
                Character = 0
            },
            ProjectionMap: null);
    }

    private static int ParseVersion(string? version)
        => int.TryParse(version, out var parsed) ? parsed : 1;

    private static bool TryGetProjectedRange(
        ProjectedDocumentContext context,
        LspRange sourceRange,
        out LspRange projectedRange)
    {
        if (context.ProjectionMap is null)
        {
            projectedRange = sourceRange;
            return true;
        }

        return context.ProjectionMap.TryMapToProjectedRange(
            context.SourceDocument.Text,
            sourceRange,
            context.ProjectedText,
            out projectedRange);
    }

    private static LspHoverResult? MapHover(ProjectedDocumentContext context, JsonElement result)
    {
        if (result.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var hover = Deserialize<LspHoverResult>(result);
        if (hover is null || hover.Range is null || context.ProjectionMap is null)
        {
            return hover;
        }

        if (!context.ProjectionMap.TryMapToOriginalRange(
            context.ProjectedText,
            hover.Range,
            context.SourceDocument.Text,
            out var originalRange))
        {
            return hover;
        }

        return new LspHoverResult
        {
            Contents = hover.Contents,
            Range = originalRange
        };
    }

    private static IReadOnlyList<LspCompletionItem> NormalizeCompletionItems(JsonElement? result)
    {
        if (result is null || result.Value.ValueKind == JsonValueKind.Null)
        {
            return Array.Empty<LspCompletionItem>();
        }

        if (result.Value.ValueKind == JsonValueKind.Array)
        {
            return Deserialize<LspCompletionItem[]>(result.Value) ?? Array.Empty<LspCompletionItem>();
        }

        if (result.Value.TryGetProperty("items", out var itemsElement))
        {
            return Deserialize<LspCompletionItem[]>(itemsElement) ?? Array.Empty<LspCompletionItem>();
        }

        return Array.Empty<LspCompletionItem>();
    }

    private static IReadOnlyList<LspLocation> NormalizeLocations(JsonElement? result)
    {
        if (result is null || result.Value.ValueKind == JsonValueKind.Null)
        {
            return Array.Empty<LspLocation>();
        }

        if (result.Value.ValueKind == JsonValueKind.Array)
        {
            return Deserialize<LspLocation[]>(result.Value) ?? Array.Empty<LspLocation>();
        }

        if (result.Value.ValueKind == JsonValueKind.Object)
        {
            var location = Deserialize<LspLocation>(result.Value);
            return location is null ? Array.Empty<LspLocation>() : [location];
        }

        return Array.Empty<LspLocation>();
    }

    private static IReadOnlyList<LspCodeAction> NormalizeCodeActions(JsonElement? result)
    {
        if (result is null || result.Value.ValueKind == JsonValueKind.Null)
        {
            return Array.Empty<LspCodeAction>();
        }

        if (result.Value.ValueKind == JsonValueKind.Array)
        {
            return Deserialize<LspCodeAction[]>(result.Value) ?? Array.Empty<LspCodeAction>();
        }

        return Array.Empty<LspCodeAction>();
    }

    private static IReadOnlyList<LspLocation> MapLocations(
        ProjectedDocumentContext context,
        IReadOnlyList<LspLocation> locations)
        => locations
            .Select(location =>
            {
                if (context.ProjectionMap is null
                    || !string.Equals(location.Uri, context.ProjectedUri, StringComparison.OrdinalIgnoreCase)
                    || !context.ProjectionMap.TryMapToOriginalRange(
                        context.ProjectedText,
                        location.Range,
                        context.SourceDocument.Text,
                        out var originalRange))
                {
                    return location;
                }

                return new LspLocation
                {
                    Uri = LspProtocolHelpers.ToDocumentUri(context.SourceDocument.DocumentPath),
                    Range = originalRange
                };
            })
            .ToArray();

    private static LspWorkspaceEdit? MapWorkspaceEdit(
        ProjectedDocumentContext context,
        LspWorkspaceEdit? edit)
    {
        if (edit is null || context.ProjectionMap is null)
        {
            return edit;
        }

        var projectedUri = context.ProjectedUri;
        var sourceUri = LspProtocolHelpers.ToDocumentUri(context.SourceDocument.DocumentPath);
        var changes = new Dictionary<string, LspTextEdit[]>(StringComparer.Ordinal);
        foreach (var pair in edit.Changes)
        {
            if (!string.Equals(pair.Key, projectedUri, StringComparison.OrdinalIgnoreCase))
            {
                changes[pair.Key] = pair.Value;
                continue;
            }

            var mappedEdits = pair.Value
                .Select(textEdit =>
                {
                    if (!context.ProjectionMap.TryMapToOriginalRange(
                        context.ProjectedText,
                        textEdit.Range,
                        context.SourceDocument.Text,
                        out var originalRange))
                    {
                        return null;
                    }

                    return new LspTextEdit
                    {
                        Range = originalRange,
                        NewText = textEdit.NewText
                    };
                })
                .Where(static candidate => candidate is not null)
                .Cast<LspTextEdit>()
                .ToArray();

            changes[sourceUri] = mappedEdits;
        }

        return new LspWorkspaceEdit
        {
            Changes = changes
        };
    }

    private static T? Deserialize<T>(JsonElement element)
        => JsonSerializer.Deserialize<T>(
            element.GetRawText(),
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            });

    public ValueTask DisposeAsync()
        => _client.DisposeAsync();

    private sealed record ProjectedDocumentContext(
        DocumentSnapshot SourceDocument,
        string ProjectedUri,
        string ProjectedText,
        string? Version,
        LspPosition ProjectedPosition,
        ProjectionMap? ProjectionMap);

    private sealed record DocumentSyncState(string Text, string? Version);
}
