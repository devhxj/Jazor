using System.Text.RegularExpressions;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Workspace;

namespace Jazor.VueHost.Lsp.Coordination;

internal sealed class MarkupComponentBridgeService
{
    private static readonly Regex ComponentTagPattern = new(
        @"<(?<name>[A-Z][A-Za-z0-9_]*)\b",
        RegexOptions.Compiled);
    private static readonly Regex ScriptImportPattern = new(
        @"^\s*import\s+(?<clause>.+?)\s+from\s+[""'](?<path>[^""']+)[""']",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private readonly IVueHostWorkspaceStore _workspaceStore;

    public MarkupComponentBridgeService(IVueHostWorkspaceStore workspaceStore)
    {
        _workspaceStore = workspaceStore ?? throw new ArgumentNullException(nameof(workspaceStore));
    }

    public bool TryFindComponentTagSymbol(string text, LspPosition position, out MarkupComponentSymbol symbol)
    {
        var offset = LspProtocolHelpers.GetOffset(text, position);
        foreach (Match match in ComponentTagPattern.Matches(text))
        {
            var group = match.Groups["name"];
            if (offset < group.Index || offset > group.Index + group.Length)
            {
                continue;
            }

            symbol = new MarkupComponentSymbol(
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

    public async ValueTask<MarkupBridgeSymbol?> ResolveBridgeSymbolAsync(
        string documentPath,
        string componentName,
        bool allowWorkspaceScan,
        CancellationToken cancellationToken)
    {
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        if (VueHostWorkspaceResolver.TryResolveTrackedNearbyVueComponent(documentPath, componentName, openDocuments, out var trackedNearby))
        {
            return new MarkupBridgeSymbol(trackedNearby.ComponentName, trackedNearby.AbsolutePath, trackedNearby.ImportPath);
        }

        if (VueHostWorkspaceResolver.TryResolveNearbyVueComponent(documentPath, componentName, out var componentPath, out var importPath))
        {
            return new MarkupBridgeSymbol(componentName, componentPath, importPath);
        }

        if (VueHostWorkspaceResolver.TryResolveTrackedVueComponent(documentPath, componentName, openDocuments, out var tracked))
        {
            return new MarkupBridgeSymbol(tracked.ComponentName, tracked.AbsolutePath, tracked.ImportPath);
        }

        if (allowWorkspaceScan
            && VueHostWorkspaceResolver.ResolveWorkspaceVueComponent(documentPath, componentName, openDocuments, cancellationToken) is { } workspaceResolved)
        {
            return new MarkupBridgeSymbol(workspaceResolved.ComponentName, workspaceResolved.AbsolutePath, workspaceResolved.ImportPath);
        }

        return null;
    }

    public async ValueTask<MarkupComponentResolution?> ResolveComponentAsync(
        string documentPath,
        string componentName,
        bool allowWorkspaceScan,
        CancellationToken cancellationToken)
    {
        var resolved = await ResolveBridgeSymbolAsync(documentPath, componentName, allowWorkspaceScan, cancellationToken);
        return resolved is null
            ? null
            : new MarkupComponentResolution(
                resolved.Value.ComponentName,
                resolved.Value.AbsolutePath,
                resolved.Value.ImportPath);
    }

    public async ValueTask<MarkupBridgeSymbol?> ResolveBridgeSymbolAsync(
        DocumentSnapshot document,
        LspPosition position,
        IReadOnlyList<LspLocation>? locationHints,
        bool allowWorkspaceScan,
        CancellationToken cancellationToken)
    {
        if (TryFindComponentTagSymbol(document.Text, position, out var tagSymbol))
        {
            return await ResolveBridgeSymbolAsync(
                document.DocumentPath,
                tagSymbol.ComponentName,
                allowWorkspaceScan,
                cancellationToken);
        }

        if (locationHints is { Count: > 0 }
            && TryResolveBridgeSymbolFromLocations(locationHints) is { } resolvedFromLocations)
        {
            return resolvedFromLocations;
        }

        return await TryResolveImportedBridgeSymbolAsync(document, position, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<MarkupComponentResolution>> GetComponentSuggestionsAsync(
        string documentPath,
        bool allowWorkspaceScan,
        CancellationToken cancellationToken)
    {
        var suggestions = new List<MarkupComponentResolution>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);

        foreach (var tracked in VueHostWorkspaceResolver.EnumerateTrackedVueComponents(documentPath, openDocuments))
        {
            if (seenPaths.Add(VueHostWorkspaceResolver.NormalizePath(tracked.AbsolutePath)))
            {
                suggestions.Add(new MarkupComponentResolution(tracked.ComponentName, tracked.AbsolutePath, tracked.ImportPath));
            }
        }

        foreach (var nearby in VueHostWorkspaceResolver.EnumerateNearbyVueComponents(documentPath))
        {
            if (seenPaths.Add(VueHostWorkspaceResolver.NormalizePath(nearby.AbsolutePath)))
            {
                suggestions.Add(new MarkupComponentResolution(nearby.ComponentName, nearby.AbsolutePath, nearby.ImportPath));
            }
        }

        if (allowWorkspaceScan)
        {
            foreach (var workspace in VueHostWorkspaceResolver.EnumerateWorkspaceVueComponents(documentPath, openDocuments, cancellationToken))
            {
                if (seenPaths.Add(VueHostWorkspaceResolver.NormalizePath(workspace.AbsolutePath)))
                {
                    suggestions.Add(new MarkupComponentResolution(workspace.ComponentName, workspace.AbsolutePath, workspace.ImportPath));
                }
            }
        }

        return suggestions;
    }

    public async ValueTask<IReadOnlyList<ResolvedVueComponent>> GetVueComponentSuggestionsAsync(
        string documentPath,
        bool allowWorkspaceScan,
        CancellationToken cancellationToken)
    {
        var suggestions = await GetComponentSuggestionsAsync(documentPath, allowWorkspaceScan, cancellationToken);
        return suggestions
            .Select(static component => new ResolvedVueComponent(
                component.ComponentName,
                component.AbsolutePath,
                component.ImportPath))
            .ToArray();
    }

    public MarkupBridgeSymbol? TryResolveBridgeSymbolFromLocations(IReadOnlyList<LspLocation> locations)
    {
        foreach (var location in locations)
        {
            var documentPath = LspProtocolHelpers.ToDocumentPath(location.Uri);
            if (VueHostWorkspaceResolver.MapDocumentKind(documentPath) != DocumentKind.Vue)
            {
                continue;
            }

            var componentName = Path.GetFileNameWithoutExtension(documentPath);
            if (string.IsNullOrWhiteSpace(componentName) || !char.IsUpper(componentName[0]))
            {
                continue;
            }

            var normalizedPath = VueHostWorkspaceResolver.NormalizePath(documentPath);
            return new MarkupBridgeSymbol(componentName, normalizedPath, normalizedPath);
        }

        return null;
    }

    public async ValueTask<MarkupBridgeSymbol?> TryResolveImportedBridgeSymbolAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        if (!TryFindImportedComponentSymbol(document.Text, position, out var importSymbol))
        {
            return null;
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        foreach (var candidatePath in VueHostWorkspaceResolver.GetImportPathCandidates(document.DocumentPath, importSymbol.ImportPath))
        {
            var normalizedPath = VueHostWorkspaceResolver.NormalizePath(candidatePath);
            if (VueHostWorkspaceResolver.MapDocumentKind(normalizedPath) != DocumentKind.Vue)
            {
                continue;
            }

            if (!openDocuments.Any(candidate =>
                    string.Equals(
                        VueHostWorkspaceResolver.NormalizePath(candidate.DocumentPath),
                        normalizedPath,
                        StringComparison.OrdinalIgnoreCase))
                && !File.Exists(normalizedPath))
            {
                continue;
            }

            var componentName = Path.GetFileNameWithoutExtension(normalizedPath);
            if (string.IsNullOrWhiteSpace(componentName))
            {
                continue;
            }

            var documentDirectory = Path.GetDirectoryName(document.DocumentPath) ?? string.Empty;
            return new MarkupBridgeSymbol(
                componentName,
                normalizedPath,
                VueHostWorkspaceResolver.ToImportPath(documentDirectory, normalizedPath));
        }

        return null;
    }

    public async ValueTask<LspHoverResult?> GetHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool allowWorkspaceScan,
        CancellationToken cancellationToken)
    {
        if (!TryFindComponentTagSymbol(document.Text, position, out var symbol))
        {
            return null;
        }

        var resolvedComponent = await ResolveComponentAsync(document.DocumentPath, symbol.ComponentName, allowWorkspaceScan, cancellationToken);
        if (resolvedComponent is null)
        {
            return null;
        }

        return new LspHoverResult
        {
            Contents = new LspMarkupContent
            {
                Kind = "markdown",
                Value = $"`{symbol.ComponentName}` resolved from Razor/Volar markup to `{resolvedComponent.Value.ImportPath}`\n\nkind: `VueComponent`"
            },
            Range = symbol.Range
        };
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool allowWorkspaceScan,
        CancellationToken cancellationToken)
    {
        if (!TryFindComponentTagSymbol(document.Text, position, out var symbol))
        {
            return Array.Empty<LspLocation>();
        }

        var resolvedComponent = await ResolveComponentAsync(document.DocumentPath, symbol.ComponentName, allowWorkspaceScan, cancellationToken);
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
        CancellationToken cancellationToken)
    {
        if (!TryFindComponentTagSymbol(document.Text, position, out var symbol))
        {
            return Array.Empty<LspLocation>();
        }

        var resolvedComponent = await ResolveComponentAsync(document.DocumentPath, symbol.ComponentName, allowWorkspaceScan: true, cancellationToken);
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

        var candidateDocuments = await GetReferenceCandidateDocumentsAsync(document, resolvedComponent.Value.AbsolutePath, cancellationToken);
        foreach (var candidateDocument in candidateDocuments)
        {
            foreach (var location in FindComponentTagLocations(candidateDocument, symbol.ComponentName))
            {
                if (!includeDeclaration
                    && string.Equals(location.Uri, LspProtocolHelpers.ToDocumentUri(document.DocumentPath), StringComparison.Ordinal)
                    && RangesEqual(location.Range, symbol.Range))
                {
                    continue;
                }

                locations.Add(location);
            }
        }

        return locations
            .GroupBy(static location =>
                $"{location.Uri}:{location.Range.Start.Line}:{location.Range.Start.Character}:{location.Range.End.Line}:{location.Range.End.Character}",
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    public async ValueTask<LspWorkspaceEdit?> GetRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(newName)
            || !TryFindComponentTagSymbol(document.Text, position, out var symbol))
        {
            return null;
        }

        var resolvedComponent = await ResolveComponentAsync(document.DocumentPath, symbol.ComponentName, allowWorkspaceScan: true, cancellationToken);
        if (resolvedComponent is null)
        {
            return null;
        }

        var candidateDocuments = await GetReferenceCandidateDocumentsAsync(document, resolvedComponent.Value.AbsolutePath, cancellationToken);
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

        return changes.Count == 0
            ? null
            : new LspWorkspaceEdit
            {
                Changes = changes
            };
    }

    public async ValueTask<IReadOnlyList<LspLocation>> FindJazorReferencesAsync(
        DocumentSnapshot document,
        string componentName,
        string? declarationDocumentPath,
        bool includeDeclaration,
        CancellationToken cancellationToken)
    {
        var locations = new List<LspLocation>();
        if (includeDeclaration && !string.IsNullOrWhiteSpace(declarationDocumentPath))
        {
            locations.Add(new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(declarationDocumentPath),
                Range = new LspRange
                {
                    Start = new LspPosition { Line = 0, Character = 0 },
                    End = new LspPosition { Line = 0, Character = 0 }
                }
            });
        }

        var candidateDocuments = await GetJazorReferenceCandidateDocumentsAsync(document, declarationDocumentPath, cancellationToken);
        foreach (var candidateDocument in candidateDocuments)
        {
            locations.AddRange(FindComponentTagLocations(candidateDocument, componentName));
        }

        return locations
            .GroupBy(static location =>
                $"{location.Uri}:{location.Range.Start.Line}:{location.Range.Start.Character}:{location.Range.End.Line}:{location.Range.End.Character}",
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    public async ValueTask<Dictionary<string, LspTextEdit[]>> FindJazorRenameChangesAsync(
        DocumentSnapshot document,
        string componentName,
        string? declarationDocumentPath,
        string newName,
        CancellationToken cancellationToken)
    {
        var candidateDocuments = await GetJazorReferenceCandidateDocumentsAsync(document, declarationDocumentPath, cancellationToken);
        var changes = new Dictionary<string, LspTextEdit[]>(StringComparer.Ordinal);
        foreach (var candidateDocument in candidateDocuments)
        {
            var edits = FindComponentTagLocations(candidateDocument, componentName)
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
            if (openDocument.DocumentKind is not (DocumentKind.Jazor or DocumentKind.Vue)
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
            await AddDocumentsFromDirectoryAsync(directory, "*.jazor", openDocuments, documents, seen, cancellationToken);
            await AddDocumentsFromDirectoryAsync(directory, "*.vue", openDocuments, documents, seen, cancellationToken);
        }

        return documents;
    }

    private async ValueTask<IReadOnlyList<DocumentSnapshot>> GetJazorReferenceCandidateDocumentsAsync(
        DocumentSnapshot document,
        string? declarationDocumentPath,
        CancellationToken cancellationToken)
    {
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var documents = new List<DocumentSnapshot>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var openDocument in openDocuments.Where(static candidate => candidate.DocumentKind == DocumentKind.Jazor))
        {
            AddDocumentCandidate(openDocument, documents, seen);
        }

        if (document.DocumentKind is DocumentKind.Jazor or DocumentKind.Vue)
        {
            AddDocumentCandidate(document, documents, seen);
        }

        foreach (var directory in VueHostWorkspaceResolver.GetWorkspaceSearchRoots(document.DocumentPath, declarationDocumentPath, openDocuments))
        {
            await AddDocumentsFromDirectoryAsync(directory, "*.jazor", openDocuments, documents, seen, cancellationToken);
        }

        return documents;
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

    private static bool RangesEqual(LspRange left, LspRange right)
        => left.Start.Line == right.Start.Line
            && left.Start.Character == right.Start.Character
            && left.End.Line == right.End.Line
            && left.End.Character == right.End.Character;

    private static bool TryFindImportedComponentSymbol(
        string text,
        LspPosition position,
        out ImportedComponentSymbol symbol)
    {
        var offset = LspProtocolHelpers.GetOffset(text, position);
        foreach (Match match in ScriptImportPattern.Matches(text))
        {
            var clauseGroup = match.Groups["clause"];
            var pathGroup = match.Groups["path"];
            if (!clauseGroup.Success || !pathGroup.Success)
            {
                continue;
            }

            foreach (var candidate in EnumerateImportBindings(clauseGroup))
            {
                if (offset < candidate.StartOffset || offset > candidate.EndOffset)
                {
                    continue;
                }

                symbol = new ImportedComponentSymbol(candidate.Name, pathGroup.Value);
                return true;
            }
        }

        symbol = default;
        return false;
    }

    private static IEnumerable<ImportBindingCandidate> EnumerateImportBindings(Group clauseGroup)
    {
        var clause = clauseGroup.Value;
        var defaultMatch = Regex.Match(clause, @"^(?<name>[A-Za-z_$][A-Za-z0-9_$]*)");
        if (defaultMatch.Success && defaultMatch.Groups["name"] is { Success: true } defaultGroup)
        {
            yield return new ImportBindingCandidate(
                defaultGroup.Value,
                clauseGroup.Index + defaultGroup.Index,
                clauseGroup.Index + defaultGroup.Index + defaultGroup.Length);
        }

        var namespaceMatch = Regex.Match(clause, @"\*\s+as\s+(?<name>[A-Za-z_$][A-Za-z0-9_$]*)");
        if (namespaceMatch.Success && namespaceMatch.Groups["name"] is { Success: true } namespaceGroup)
        {
            yield return new ImportBindingCandidate(
                namespaceGroup.Value,
                clauseGroup.Index + namespaceGroup.Index,
                clauseGroup.Index + namespaceGroup.Index + namespaceGroup.Length);
        }

        var namedClauseMatch = Regex.Match(clause, @"\{(?<names>[^}]+)\}");
        if (!namedClauseMatch.Success || namedClauseMatch.Groups["names"] is not { Success: true } namesGroup)
        {
            yield break;
        }

        foreach (Match nameMatch in Regex.Matches(
                     namesGroup.Value,
                     @"(?<imported>[A-Za-z_$][A-Za-z0-9_$]*)(?:\s+as\s+(?<local>[A-Za-z_$][A-Za-z0-9_$]*))?"))
        {
            var localGroup = nameMatch.Groups["local"];
            var importedGroup = nameMatch.Groups["imported"];
            var effectiveGroup = localGroup.Success ? localGroup : importedGroup;
            if (!effectiveGroup.Success)
            {
                continue;
            }

            yield return new ImportBindingCandidate(
                effectiveGroup.Value,
                namesGroup.Index + effectiveGroup.Index,
                namesGroup.Index + effectiveGroup.Index + effectiveGroup.Length);
        }
    }

    private static async ValueTask AddDocumentsFromDirectoryAsync(
        string directory,
        string searchPattern,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        List<DocumentSnapshot> documents,
        HashSet<string> seen,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var filePath in VueHostWorkspaceResolver.EnumerateWorkspaceFiles(new[] { directory }, searchPattern, cancellationToken))
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
                var documentKind = VueHostWorkspaceResolver.MapDocumentKind(filePath);
                if (documentKind is not (DocumentKind.Jazor or DocumentKind.Vue))
                {
                    continue;
                }

                documents.Add(new DocumentSnapshot(
                    normalizedPath,
                    documentKind,
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
}

internal readonly record struct MarkupComponentSymbol(
    string ComponentName,
    LspRange Range);

internal readonly record struct MarkupBridgeSymbol(
    string ComponentName,
    string AbsolutePath,
    string ImportPath);

internal readonly record struct MarkupComponentResolution(
    string ComponentName,
    string AbsolutePath,
    string ImportPath);

internal readonly record struct ResolvedVueComponent(
    string ComponentName,
    string AbsolutePath,
    string ImportPath);

internal readonly record struct ImportedComponentSymbol(
    string LocalName,
    string ImportPath);

internal readonly record struct ImportBindingCandidate(
    string Name,
    int StartOffset,
    int EndOffset);
