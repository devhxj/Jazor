using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Frontend.Deno.Hosting;
using Jazor.VueHost.Lsp.Routing;
using Jazor.VueHost.Workspace;
using System.Text.RegularExpressions;

namespace Jazor.VueHost.Lsp.Lanes;

internal sealed class FrontendLaneService : ILspLane
{
    private const string MissingTemplateImportDiagnosticCode = "JAZORVUEFRONTEND001";
    private static readonly Regex ComponentTagPattern = new(
        @"<(?<name>[A-Z][A-Za-z0-9_]*)\b",
        RegexOptions.Compiled);
    private static readonly Regex TagCompletionPrefixPattern = new(
        @"</?(?<name>[A-Za-z0-9_]*)$",
        RegexOptions.Compiled);
    private readonly IVueHostWorkspaceStore _workspaceStore;
    private readonly IDenoFrontendHost? _denoFrontendHost;

    public FrontendLaneService(
        IVueHostWorkspaceStore workspaceStore,
        IDenoFrontendHost? denoFrontendHost = null)
    {
        _workspaceStore = workspaceStore ?? throw new ArgumentNullException(nameof(workspaceStore));
        _denoFrontendHost = denoFrontendHost;
    }

    public LaneKind LaneKind => LaneKind.Frontend;

    public async ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        var diagnostics = new List<LspDiagnostic>();
        diagnostics.AddRange(await FilterDenoDiagnosticsAsync(document, await TryGetDenoDiagnosticsAsync(document, cancellationToken), cancellationToken));

        diagnostics.AddRange(await CreateUnresolvedMarkupComponentDiagnosticsAsync(document, cancellationToken));
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

        var denoResult = await TryGetDenoHoverAsync(document, position, cancellationToken);
        if (denoResult is not null)
        {
            return denoResult;
        }

        if (!CanUseWorkspaceGraph())
        {
            return null;
        }

        if (!TryFindComponentTagSymbol(document.Text, position, out var symbol))
        {
            return null;
        }

        var resolvedComponent = await ResolveVueComponentAsync(document.DocumentPath, symbol.ComponentName, cancellationToken, allowWorkspaceScan: true);
        if (resolvedComponent is null)
        {
            return null;
        }

        return new LspHoverResult
        {
            Contents = new LspMarkupContent
            {
                Kind = "markdown",
                Value = $"`{symbol.ComponentName}` resolved from Razor markup to `{resolvedComponent.Value.ImportPath}`\n\nkind: `VueComponent`"
            },
            Range = symbol.Range
        };
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

        var items = new List<LspCompletionItem>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in await TryGetDenoCompletionItemsAsync(document, position, cancellationToken))
        {
            if (seen.Add($"{item.Label}|{item.Kind}|{item.Detail}"))
            {
                items.Add(item);
            }
        }

        if (CanUseWorkspaceGraph() && TryGetTagCompletionPrefix(document.Text, position, out var tagPrefix))
        {
            foreach (var component in await GetVueComponentSuggestionsAsync(document.DocumentPath, cancellationToken))
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
        if (document.DocumentKind is not (DocumentKind.Jazor or DocumentKind.Vue))
        {
            return Array.Empty<LspSemanticToken>();
        }

        return await TryGetDenoSemanticTokensAsync(document, cancellationToken);
    }

    private async ValueTask<IReadOnlyList<LspDocumentSymbol>> GetDocumentSymbolsCoreAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var denoResult = await TryGetDenoDocumentSymbolsAsync(document, cancellationToken);
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

        var denoResult = await TryGetDenoDefinitionsAsync(document, position, cancellationToken);
        if (denoResult is { Count: > 0 })
        {
            return denoResult;
        }

        if (!CanUseWorkspaceGraph())
        {
            return Array.Empty<LspLocation>();
        }

        if (!TryFindComponentTagSymbol(document.Text, position, out var symbol))
        {
            return Array.Empty<LspLocation>();
        }

        var resolvedComponent = await ResolveVueComponentAsync(document.DocumentPath, symbol.ComponentName, cancellationToken, allowWorkspaceScan: true);
        if (resolvedComponent is null)
        {
            return Array.Empty<LspLocation>();
        }

        return
        [
            new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(resolvedComponent.Value.AbsolutePath),
                Range = new LspRange
                {
                    Start = new LspPosition { Line = 0, Character = 0 },
                    End = new LspPosition { Line = 0, Character = 0 }
                }
            }
        ];
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

        if (!TryFindComponentTagSymbol(document.Text, position, out var symbol))
        {
            return Array.Empty<LspLocation>();
        }

        var locations = new List<LspLocation>();
        locations.AddRange(await TryGetDenoReferencesAsync(document, position, includeDeclaration, cancellationToken));
        if (CanUseWorkspaceGraph())
        {
            locations.AddRange(await FindWorkspaceReferencesAsync(document, symbol, includeDeclaration, cancellationToken));
        }

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

        if (!TryFindComponentTagSymbol(document.Text, position, out var symbol))
        {
            return null;
        }

        var changes = new Dictionary<string, List<LspTextEdit>>(StringComparer.Ordinal);
        var denoResult = await TryGetDenoRenameAsync(document, position, newName, cancellationToken);
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

        if (CanUseWorkspaceGraph())
        {
            foreach (var change in await FindWorkspaceRenameChangesAsync(document, symbol, newName, cancellationToken))
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
        => projectionTarget.LaneKind == LaneKind.Frontend
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

    private static bool TryFindComponentTagSymbol(string text, LspPosition position, out ComponentTagSymbol symbol)
    {
        var offset = LspProtocolHelpers.GetOffset(text, position);
        foreach (Match match in ComponentTagPattern.Matches(text))
        {
            var group = match.Groups["name"];
            if (offset < group.Index || offset > group.Index + group.Length)
            {
                continue;
            }

            symbol = new ComponentTagSymbol(
                group.Value,
                new LspRange
                {
                    Start = LspProtocolHelpers.GetPosition(text, group.Index),
                    End = LspProtocolHelpers.GetPosition(text, group.Index + group.Length)
                });
            return true;
        }

        symbol = default;
        return false;
    }

    private async ValueTask<IReadOnlyList<LspCompletionItem>> TryGetDenoCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null)
        {
            return Array.Empty<LspCompletionItem>();
        }

        try
        {
            return await _denoFrontendHost.GetTemplateCompletionItemsAsync(document, position, cancellationToken);
        }
        catch
        {
            return Array.Empty<LspCompletionItem>();
        }
    }

    private async ValueTask<LspHoverResult?> TryGetDenoHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null)
        {
            return null;
        }

        try
        {
            return await _denoFrontendHost.GetTemplateHoverAsync(document, position, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async ValueTask<IReadOnlyList<LspLocation>> TryGetDenoDefinitionsAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null)
        {
            return Array.Empty<LspLocation>();
        }

        try
        {
            return await _denoFrontendHost.GetTemplateDefinitionAsync(document, position, cancellationToken);
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
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null)
        {
            return Array.Empty<LspLocation>();
        }

        try
        {
            return await _denoFrontendHost.GetTemplateReferencesAsync(
                document,
                position,
                includeDeclaration,
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
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null)
        {
            return null;
        }

        try
        {
            return await _denoFrontendHost.GetTemplateRenameAsync(document, position, newName, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async ValueTask<IReadOnlyList<LspDocumentSymbol>> TryGetDenoDocumentSymbolsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null)
        {
            return Array.Empty<LspDocumentSymbol>();
        }

        try
        {
            return await _denoFrontendHost.GetTemplateDocumentSymbolsAsync(document, cancellationToken);
        }
        catch
        {
            return Array.Empty<LspDocumentSymbol>();
        }
    }

    private async ValueTask<IReadOnlyList<LspDiagnostic>> TryGetDenoDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null)
        {
            return Array.Empty<LspDiagnostic>();
        }

        try
        {
            return await _denoFrontendHost.GetTemplateDiagnosticsAsync(document, cancellationToken);
        }
        catch
        {
            return Array.Empty<LspDiagnostic>();
        }
    }

    private async ValueTask<IReadOnlyList<LspSemanticToken>> TryGetDenoSemanticTokensAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null)
        {
            return Array.Empty<LspSemanticToken>();
        }

        try
        {
            return await _denoFrontendHost.GetTemplateSemanticTokensAsync(document, cancellationToken);
        }
        catch
        {
            return Array.Empty<LspSemanticToken>();
        }
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

            var isResolvable = await ResolveVueComponentAsync(
                    document.DocumentPath,
                    group.Value,
                    cancellationToken,
                    allowWorkspaceScan: document.DocumentKind == DocumentKind.Jazor)
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
                && await ResolveVueComponentAsync(
                        document.DocumentPath,
                        componentName,
                        cancellationToken,
                        allowWorkspaceScan: document.DocumentKind == DocumentKind.Jazor) is not null)
            {
                continue;
            }

            filtered.Add(diagnostic);
        }

        return filtered;
    }

    private static IReadOnlyList<LspLocation> FindComponentTagLocations(
        DocumentSnapshot document,
        string componentName)
    {
        var locations = new List<LspLocation>();
        foreach (Match match in ComponentTagPattern.Matches(document.Text))
        {
            var group = match.Groups["name"];
            if (!string.Equals(group.Value, componentName, StringComparison.Ordinal))
            {
                continue;
            }

            locations.Add(new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath),
                Range = new LspRange
                {
                    Start = LspProtocolHelpers.GetPosition(document.Text, group.Index),
                    End = LspProtocolHelpers.GetPosition(document.Text, group.Index + group.Length)
                }
            });
        }

        return locations;
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

    private async ValueTask<ResolvedVueComponent?> ResolveVueComponentAsync(
        string documentPath,
        string componentName,
        CancellationToken cancellationToken,
        bool allowWorkspaceScan)
    {
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        if (VueHostWorkspaceResolver.TryResolveTrackedNearbyVueComponent(documentPath, componentName, openDocuments, out var trackedNearby))
        {
            return new ResolvedVueComponent(trackedNearby.ComponentName, trackedNearby.AbsolutePath, trackedNearby.ImportPath);
        }

        if (VueHostWorkspaceResolver.TryResolveNearbyVueComponent(documentPath, componentName, out var componentPath, out var importPath))
        {
            return new ResolvedVueComponent(componentName, componentPath, importPath);
        }

        if (VueHostWorkspaceResolver.TryResolveTrackedVueComponent(documentPath, componentName, openDocuments, out var tracked))
        {
            return new ResolvedVueComponent(tracked.ComponentName, tracked.AbsolutePath, tracked.ImportPath);
        }

        if (allowWorkspaceScan
            && VueHostWorkspaceResolver.ResolveWorkspaceVueComponent(documentPath, componentName, openDocuments, cancellationToken) is { } workspaceResolved)
        {
            return new ResolvedVueComponent(workspaceResolved.ComponentName, workspaceResolved.AbsolutePath, workspaceResolved.ImportPath);
        }

        return null;
    }

    private async ValueTask<IReadOnlyList<ResolvedVueComponent>> GetVueComponentSuggestionsAsync(
        string documentPath,
        CancellationToken cancellationToken)
    {
        var suggestions = new List<ResolvedVueComponent>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);

        foreach (var tracked in VueHostWorkspaceResolver.EnumerateTrackedVueComponents(documentPath, openDocuments))
        {
            if (seenPaths.Add(VueHostWorkspaceResolver.NormalizePath(tracked.AbsolutePath)))
            {
                suggestions.Add(new ResolvedVueComponent(tracked.ComponentName, tracked.AbsolutePath, tracked.ImportPath));
            }
        }

        foreach (var nearby in VueHostWorkspaceResolver.EnumerateNearbyVueComponents(documentPath))
        {
            if (seenPaths.Add(VueHostWorkspaceResolver.NormalizePath(nearby.AbsolutePath)))
            {
                suggestions.Add(new ResolvedVueComponent(nearby.ComponentName, nearby.AbsolutePath, nearby.ImportPath));
            }
        }

        if (VueHostWorkspaceResolver.MapDocumentKind(documentPath) == DocumentKind.Jazor)
        {
            foreach (var workspace in VueHostWorkspaceResolver.EnumerateWorkspaceVueComponents(documentPath, openDocuments, cancellationToken))
            {
                if (seenPaths.Add(VueHostWorkspaceResolver.NormalizePath(workspace.AbsolutePath)))
                {
                    suggestions.Add(new ResolvedVueComponent(workspace.ComponentName, workspace.AbsolutePath, workspace.ImportPath));
                }
            }
        }

        return suggestions;
    }

    private bool CanUseWorkspaceGraph()
        => _denoFrontendHost?.IsRunning == true;

    private async ValueTask<IReadOnlyList<LspLocation>> FindWorkspaceReferencesAsync(
        DocumentSnapshot document,
        ComponentTagSymbol symbol,
        bool includeDeclaration,
        CancellationToken cancellationToken)
    {
        var resolvedComponent = await ResolveVueComponentAsync(document.DocumentPath, symbol.ComponentName, cancellationToken, allowWorkspaceScan: true);
        if (resolvedComponent is null)
        {
            return Array.Empty<LspLocation>();
        }

        var locations = new List<LspLocation>();
        if (includeDeclaration)
        {
            locations.Add(new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(resolvedComponent.Value.AbsolutePath),
                Range = new LspRange
                {
                    Start = new LspPosition { Line = 0, Character = 0 },
                    End = new LspPosition { Line = 0, Character = 0 }
                }
            });
        }

        var candidateDocuments = await GetReferenceCandidateDocumentsAsync(
            document,
            resolvedComponent.Value.AbsolutePath,
            cancellationToken);
        foreach (var candidateDocument in candidateDocuments)
        {
            locations.AddRange(FindComponentTagLocations(candidateDocument, symbol.ComponentName));
        }

        return locations;
    }

    private async ValueTask<Dictionary<string, LspTextEdit[]>> FindWorkspaceRenameChangesAsync(
        DocumentSnapshot document,
        ComponentTagSymbol symbol,
        string newName,
        CancellationToken cancellationToken)
    {
        var resolvedComponent = await ResolveVueComponentAsync(document.DocumentPath, symbol.ComponentName, cancellationToken, allowWorkspaceScan: true);
        if (resolvedComponent is null)
        {
            return [];
        }

        var candidateDocuments = await GetReferenceCandidateDocumentsAsync(
            document,
            resolvedComponent.Value.AbsolutePath,
            cancellationToken);
        var changes = new Dictionary<string, LspTextEdit[]>(StringComparer.Ordinal);
        foreach (var candidateDocument in candidateDocuments)
        {
            var edits = FindComponentTagLocations(candidateDocument, symbol.ComponentName)
                .Select(location => new LspTextEdit
                {
                    Range = location.Range,
                    NewText = newName
                })
                .OrderByDescending(edit => LspProtocolHelpers.GetOffset(candidateDocument.Text, edit.Range.Start))
                .ToArray();
            if (edits.Length > 0)
            {
                changes[LspProtocolHelpers.ToDocumentUri(candidateDocument.DocumentPath)] = edits;
            }
        }

        return changes;
    }

    private async ValueTask<IReadOnlyList<DocumentSnapshot>> GetReferenceCandidateDocumentsAsync(
        DocumentSnapshot document,
        string? declarationDocumentPath,
        CancellationToken cancellationToken)
    {
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var documents = new List<DocumentSnapshot>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var openDocument in openDocuments)
        {
            if (openDocument.DocumentKind != DocumentKind.Jazor
                && !string.Equals(
                    VueHostWorkspaceResolver.NormalizePath(openDocument.DocumentPath),
                    VueHostWorkspaceResolver.NormalizePath(document.DocumentPath),
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            AddDocumentCandidate(openDocument, documents, seen);
        }

        AddDocumentCandidate(document, documents, seen);

        foreach (var directory in VueHostWorkspaceResolver.GetWorkspaceSearchRoots(document.DocumentPath, declarationDocumentPath, openDocuments))
        {
            await AddJazorDocumentsFromDirectoryAsync(directory, openDocuments, documents, seen, cancellationToken);
        }

        return documents;
    }

    private async ValueTask AddJazorDocumentsFromDirectoryAsync(
        string directory,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        List<DocumentSnapshot> documents,
        HashSet<string> seen,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var filePath in VueHostWorkspaceResolver.EnumerateWorkspaceFiles(new[] { directory }, "*.jazor", cancellationToken))
        {
            var normalizedPath = VueHostWorkspaceResolver.NormalizePath(filePath);
            var openDocument = openDocuments.FirstOrDefault(candidate =>
                string.Equals(
                    VueHostWorkspaceResolver.NormalizePath(candidate.DocumentPath),
                    normalizedPath,
                    StringComparison.OrdinalIgnoreCase));
            if (openDocument is not null)
            {
                AddDocumentCandidate(openDocument, documents, seen);
                continue;
            }

            if (seen.Contains(normalizedPath))
            {
                continue;
            }

            try
            {
                documents.Add(new DocumentSnapshot(
                    normalizedPath,
                    DocumentKind.Jazor,
                    await File.ReadAllTextAsync(filePath, cancellationToken),
                    version: null));
                seen.Add(normalizedPath);
            }
            catch (FileNotFoundException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    private static void AddDocumentCandidate(
        DocumentSnapshot document,
        List<DocumentSnapshot> documents,
        HashSet<string> seen)
    {
        var normalizedPath = VueHostWorkspaceResolver.NormalizePath(document.DocumentPath);
        if (!seen.Add(normalizedPath))
        {
            return;
        }

        documents.Add(document);
    }

    private readonly record struct ComponentTagSymbol(
        string ComponentName,
        LspRange Range);

    private readonly record struct ResolvedVueComponent(
        string ComponentName,
        string AbsolutePath,
        string ImportPath);
}
