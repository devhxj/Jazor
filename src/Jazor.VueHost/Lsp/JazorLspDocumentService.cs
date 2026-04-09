using System.Text.RegularExpressions;
using Jazor.Vue;
using Jazor.VueHost.Analysis;
using Jazor.VueHost.Workspace;
using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Lsp;

internal sealed class JazorLspDocumentService
{
    private static readonly Regex TagPattern = new(@"<(?<name>[A-Z][A-Za-z0-9_]*)\b", RegexOptions.Compiled);
    private static readonly Regex TagCompletionPrefixPattern = new(@"</?(?<name>[A-Za-z0-9_]*)$", RegexOptions.Compiled);
    private static readonly Regex PrivateMethodPattern = new(@"(?<modifier>\bprivate\b)\s+(?<signature>(?:async\s+)?[\w<>\.\?]+\s+\w+\s*\()", RegexOptions.Compiled);
    private readonly IVueHostWorkspaceStore _workspaceStore;
    private readonly IVueAnalysisClient _analysisClient;
    private readonly FallbackJazorAnalysisService _fallbackAnalysisService = new();
    private readonly JazorVueParser _parser = new();

    public JazorLspDocumentService(
        IVueHostWorkspaceStore workspaceStore,
        IVueAnalysisClient analysisClient)
    {
        _workspaceStore = workspaceStore ?? throw new ArgumentNullException(nameof(workspaceStore));
        _analysisClient = analysisClient ?? throw new ArgumentNullException(nameof(analysisClient));
    }

    public async ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        var response = await AnalyzeAsync(document, cancellationToken);
        return response.Diagnostics
            .Where(diagnostic => string.Equals(
                NormalizeDocumentPath(diagnostic.DocumentPath),
                NormalizeDocumentPath(document.DocumentPath),
                StringComparison.OrdinalIgnoreCase))
            .Select(diagnostic => new LspDiagnostic
            {
                Range = LspProtocolHelpers.ToRange(document.Text, diagnostic.Start, diagnostic.Length),
                Severity = diagnostic.Severity switch
                {
                    DiagnosticSeverityKind.Error => 1,
                    DiagnosticSeverityKind.Warning => 2,
                    _ => 3
                },
                Code = diagnostic.Id,
                Source = "Jazor.VueHost",
                Message = diagnostic.Message
            })
            .ToArray();
    }

    public ValueTask<LspHoverResult?> GetHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
        => GetHoverCoreAsync(document, position, cancellationToken);

    private async ValueTask<LspHoverResult?> GetHoverCoreAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var symbol = TryFindSymbol(document.Text, position);
        if (symbol is null)
        {
            return null;
        }

        if (await ResolveVueComponentAsync(document.DocumentPath, symbol.SymbolName, cancellationToken) is not ResolvedVueComponent resolvedComponent)
        {
            return null;
        }

        return new LspHoverResult
        {
            Contents = new LspMarkupContent
            {
                Kind = "markdown",
                Value = $"`{symbol.SymbolName}` resolved from Razor markup to `{resolvedComponent.ImportPath}`\n\nkind: `VueComponent`"
            },
            Range = symbol.Range
        };
    }

    public ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
        => GetCompletionItemsCoreAsync(document, position, cancellationToken);

    private async ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsCoreAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var offset = LspProtocolHelpers.GetOffset(document.Text, position);
        var prefix = document.Text[..Math.Min(offset, document.Text.Length)];
        var items = new List<LspCompletionItem>();

        if (EndsWithDirectivePrefix(prefix))
        {
            items.Add(new LspCompletionItem
            {
                Label = "@code",
                Kind = 14,
                Detail = "Razor code block",
                Documentation = "Start the C# code block for the current .jazor component."
            });
        }

        if (TryGetTagCompletionPrefix(prefix, out var tagPrefix))
        {
            var seenLabels = new HashSet<string>(items.Select(static item => item.Label), StringComparer.Ordinal);
            foreach (var suggestion in await GetVueComponentSuggestionsAsync(document.DocumentPath, cancellationToken))
            {
                if (!suggestion.ComponentName.StartsWith(tagPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!seenLabels.Add(suggestion.ComponentName))
                {
                    continue;
                }

                items.Add(new LspCompletionItem
                {
                    Label = suggestion.ComponentName,
                    Kind = 7,
                    Detail = suggestion.ImportPath,
                    Documentation = $"Vue component available to `.jazor` from `{suggestion.ImportPath}`."
                });
            }
        }

        return items;
    }

    public ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
        => GetDefinitionCoreAsync(document, position, cancellationToken);

    private async ValueTask<IReadOnlyList<LspLocation>> GetDefinitionCoreAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var symbol = TryFindSymbol(document.Text, position);
        if (symbol is null)
        {
            return Array.Empty<LspLocation>();
        }

        if (await ResolveVueComponentAsync(document.DocumentPath, symbol.SymbolName, cancellationToken) is not ResolvedVueComponent resolvedComponent)
        {
            return Array.Empty<LspLocation>();
        }

        return
        [
            new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(resolvedComponent.AbsolutePath),
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
        var symbol = await ResolveImportSymbolAsync(document, position, cancellationToken);
        if (symbol is null)
        {
            return Array.Empty<LspLocation>();
        }

        var locations = await FindSymbolLocationsAsync(document, symbol, includeDeclaration, cancellationToken);
        return locations;
    }

    public async ValueTask<bool> IsVueComponentResolvableAsync(
        DocumentSnapshot document,
        string componentName,
        CancellationToken cancellationToken)
        => await ResolveVueComponentAsync(document.DocumentPath, componentName, cancellationToken) is not null;

    public async ValueTask<LspWorkspaceEdit?> GetRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return null;
        }

        var symbol = await ResolveImportSymbolAsync(document, position, cancellationToken);
        if (symbol is null)
        {
            return null;
        }

        var changes = await FindRenameChangesAsync(document, symbol, newName, cancellationToken);
        if (changes.Count == 0)
        {
            return null;
        }

        return new LspWorkspaceEdit
        {
            Changes = changes
        };
    }

    public ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(
        DocumentSnapshot document,
        IReadOnlyList<LspDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!diagnostics.Any(static diagnostic => string.Equals(diagnostic.Code, "JAZORVUE001", StringComparison.Ordinal)))
        {
            return ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(Array.Empty<LspCodeAction>());
        }

        var privateMethodMatch = PrivateMethodPattern.Match(document.Text);
        if (!privateMethodMatch.Success)
        {
            return ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(Array.Empty<LspCodeAction>());
        }

        IReadOnlyList<LspCodeAction> actions =
        [
            new LspCodeAction
            {
                Title = "Make method public for bridge lowering",
                Kind = "quickfix",
                Edit = new LspWorkspaceEdit
                {
                    Changes = new Dictionary<string, LspTextEdit[]>
                    {
                        [LspProtocolHelpers.ToDocumentUri(document.DocumentPath)] =
                        [
                            new LspTextEdit
                            {
                                Range = LspProtocolHelpers.ToRange(document.Text, privateMethodMatch.Groups["modifier"].Index, privateMethodMatch.Groups["modifier"].Length),
                                NewText = "public"
                            }
                        ]
                    }
                }
            }
        ];

        return ValueTask.FromResult(actions);
    }

    private async ValueTask<AnalyzeJazorResponse> AnalyzeAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        var request = new AnalyzeJazorRequest(
            document,
            relatedDocuments: await ResolveRelatedDocumentsAsync(document, cancellationToken),
            frontendContext: null);
        var response = await _analysisClient.AnalyzeJazorAsync(request, cancellationToken);
        if (response.Artifacts.Count > 0 || response.Imports.Count > 0 || response.Diagnostics.Count > 0)
        {
            return response;
        }

        return await _fallbackAnalysisService.AnalyzeJazorAsync(request, cancellationToken);
    }

    private async ValueTask<IReadOnlyList<DocumentSnapshot>> ResolveRelatedDocumentsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        var parsed = _parser.Parse(document.DocumentPath, document.Text);
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var relatedDocuments = new List<DocumentSnapshot>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var import in parsed.Imports)
        {
            var importPath = ResolveImportPath(document.DocumentPath, import.Source);
            await AddRelatedDocumentAsync(importPath, openDocuments, relatedDocuments, seen, cancellationToken);
        }

        var referencedComponentNames = TagPattern.Matches(document.Text)
            .Select(static match => match.Groups["name"].Value)
            .Distinct(StringComparer.Ordinal);
        foreach (var componentName in referencedComponentNames)
        {
            if (!TryResolveNearbyVueComponent(document.DocumentPath, componentName, out var componentPath, out _))
            {
                continue;
            }

            await AddRelatedDocumentAsync(componentPath, openDocuments, relatedDocuments, seen, cancellationToken);
        }

        foreach (var assetPath in GetCoLocatedAssetPaths(document.DocumentPath))
        {
            await AddRelatedDocumentAsync(assetPath, openDocuments, relatedDocuments, seen, cancellationToken);
        }

        return relatedDocuments;
    }

    private static bool EndsWithDirectivePrefix(string prefix)
    {
        return prefix.EndsWith("@", StringComparison.Ordinal)
            || prefix.EndsWith("@c", StringComparison.Ordinal);
    }

    private async ValueTask<ResolvedImportSymbol?> ResolveImportSymbolAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        var symbol = TryFindSymbol(document.Text, position);
        if (symbol is null)
        {
            return null;
        }

        if (await ResolveVueComponentAsync(document.DocumentPath, symbol.SymbolName, cancellationToken) is ResolvedVueComponent resolvedComponent)
        {
            return new ResolvedImportSymbol(
                symbol.SymbolName,
                symbol.Range,
                resolvedComponent.AbsolutePath,
                new ImportDescriptor(
                    symbol.SymbolName,
                    resolvedComponent.ImportPath,
                    ImportKind.VueImport,
                    ImportBindingKind.Default,
                    importedName: null,
                    templateVisible: true));
        }

        return null;
    }

    private async ValueTask<IReadOnlyList<LspLocation>> FindSymbolLocationsAsync(
        DocumentSnapshot document,
        ResolvedImportSymbol symbol,
        bool includeDeclaration,
        CancellationToken cancellationToken)
    {
        var locations = new List<LspLocation>();
        if (includeDeclaration)
        {
            if (!string.IsNullOrWhiteSpace(symbol.DeclarationDocumentPath))
            {
                locations.Add(new LspLocation
                {
                    Uri = LspProtocolHelpers.ToDocumentUri(symbol.DeclarationDocumentPath),
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 0 },
                        End = new LspPosition { Line = 0, Character = 0 }
                    }
                });
            }
            else
            {
                locations.Add(new LspLocation
                {
                    Uri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath),
                    Range = symbol.DeclarationRange
                });
            }
        }

        var candidateDocuments = await GetReferenceCandidateDocumentsAsync(
            document,
            symbol.DeclarationDocumentPath,
            cancellationToken);
        foreach (var candidateDocument in candidateDocuments)
        {
            foreach (var location in FindComponentTagLocations(candidateDocument, symbol.SymbolName))
            {
                if (!includeDeclaration
                    && string.Equals(location.Uri, LspProtocolHelpers.ToDocumentUri(document.DocumentPath), StringComparison.Ordinal)
                    && RangesEqual(location.Range, symbol.DeclarationRange))
                {
                    continue;
                }

                locations.Add(location);
            }
        }

        return locations
            .GroupBy(static location => $"{location.Uri}:{location.Range.Start.Line}:{location.Range.Start.Character}:{location.Range.End.Line}:{location.Range.End.Character}", StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    private async ValueTask<Dictionary<string, LspTextEdit[]>> FindRenameChangesAsync(
        DocumentSnapshot document,
        ResolvedImportSymbol symbol,
        string newName,
        CancellationToken cancellationToken)
    {
        var candidateDocuments = await GetReferenceCandidateDocumentsAsync(
            document,
            symbol.DeclarationDocumentPath,
            cancellationToken);
        var changes = new Dictionary<string, LspTextEdit[]>(StringComparer.Ordinal);

        foreach (var candidateDocument in candidateDocuments)
        {
            var edits = FindComponentTagLocations(candidateDocument, symbol.SymbolName)
                .Select(location => new LspTextEdit
                {
                    Range = location.Range,
                    NewText = newName
                })
                .OrderByDescending(edit => LspProtocolHelpers.GetOffset(candidateDocument.Text, edit.Range.Start))
                .ToArray();

            if (edits.Length == 0)
            {
                continue;
            }

            changes[LspProtocolHelpers.ToDocumentUri(candidateDocument.DocumentPath)] = edits;
        }

        return changes;
    }

    private static bool RangesEqual(LspRange left, LspRange right)
        => left.Start.Line == right.Start.Line
            && left.Start.Character == right.Start.Character
            && left.End.Line == right.End.Line
            && left.End.Character == right.End.Character;

    private FoundSymbol? TryFindSymbol(string text, LspPosition position)
    {
        var offset = LspProtocolHelpers.GetOffset(text, position);
        foreach (Match match in TagPattern.Matches(text))
        {
            var group = match.Groups["name"];
            if (offset >= group.Index && offset <= group.Index + group.Length)
            {
                return new FoundSymbol(group.Value, LspProtocolHelpers.ToRange(text, group.Index, group.Length));
            }
        }

        var document = _parser.Parse("virtual.jazor", text);
        foreach (var import in document.Imports)
        {
            foreach (var binding in import.Bindings)
            {
                var bindingIndex = text.IndexOf(binding.LocalName, StringComparison.Ordinal);
                if (bindingIndex < 0)
                {
                    continue;
                }

                if (offset >= bindingIndex && offset <= bindingIndex + binding.LocalName.Length)
                {
                    return new FoundSymbol(
                        binding.LocalName,
                        LspProtocolHelpers.ToRange(text, bindingIndex, binding.LocalName.Length));
                }
            }
        }

        return null;
    }

    private static string ResolveImportPath(string documentPath, string importSource)
    {
        if (Path.IsPathRooted(importSource))
        {
            return NormalizeDocumentPath(importSource);
        }

        var baseDirectory = Path.GetDirectoryName(documentPath);
        if (string.IsNullOrWhiteSpace(baseDirectory))
        {
            return NormalizeDocumentPath(importSource);
        }

        var combined = Path.Combine(baseDirectory, importSource);
        if (Path.HasExtension(combined))
        {
            return NormalizeDocumentPath(combined);
        }

        foreach (var candidate in new[]
                 {
                     combined + ".vue",
                     combined + ".ts",
                     combined + ".js",
                     combined
                 })
        {
            if (File.Exists(candidate))
            {
                return NormalizeDocumentPath(candidate);
            }
        }

        return NormalizeDocumentPath(combined + ".vue");
    }

    private static DocumentKind MapDocumentKind(string documentPath)
        => Path.GetExtension(documentPath).ToLowerInvariant() switch
        {
            ".jazor" => DocumentKind.Jazor,
            ".vue" => DocumentKind.Vue,
            ".js" => DocumentKind.JavaScript,
            ".ts" => DocumentKind.TypeScript,
            _ => DocumentKind.Unknown
        };

    private static string NormalizeDocumentPath(string documentPath)
    {
        return Path.IsPathRooted(documentPath)
            ? Path.GetFullPath(documentPath).Replace('\\', '/')
            : documentPath.Replace('\\', '/');
    }

    private async ValueTask AddRelatedDocumentAsync(
        string documentPath,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        List<DocumentSnapshot> relatedDocuments,
        HashSet<string> seen,
        CancellationToken cancellationToken)
    {
        var normalizedPath = NormalizeDocumentPath(documentPath);
        if (!seen.Add(normalizedPath))
        {
            return;
        }

        var openDocument = openDocuments.FirstOrDefault(candidate =>
            string.Equals(
                NormalizeDocumentPath(candidate.DocumentPath),
                normalizedPath,
                StringComparison.OrdinalIgnoreCase));
        if (openDocument is not null)
        {
            relatedDocuments.Add(openDocument);
            return;
        }

        if (!File.Exists(documentPath))
        {
            return;
        }

        relatedDocuments.Add(new DocumentSnapshot(
            documentPath,
            MapDocumentKind(documentPath),
            await File.ReadAllTextAsync(documentPath, cancellationToken),
            version: null));
    }

    private async ValueTask<ResolvedVueComponent?> ResolveVueComponentAsync(
        string documentPath,
        string componentName,
        CancellationToken cancellationToken)
    {
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        if (TryResolveTrackedNearbyVueComponent(documentPath, componentName, openDocuments, out var trackedNearby))
        {
            return trackedNearby;
        }

        if (TryResolveNearbyVueComponent(documentPath, componentName, out var componentPath, out var importPath))
        {
            return new ResolvedVueComponent(componentName, componentPath, importPath);
        }

        if (TryResolveTrackedVueComponent(documentPath, componentName, openDocuments, out var tracked))
        {
            return tracked;
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

        foreach (var tracked in EnumerateTrackedVueComponents(documentPath, openDocuments))
        {
            if (seenPaths.Add(NormalizeDocumentPath(tracked.AbsolutePath)))
            {
                suggestions.Add(tracked);
            }
        }

        foreach (var nearby in EnumerateNearbyVueComponents(documentPath))
        {
            if (seenPaths.Add(NormalizeDocumentPath(nearby.AbsolutePath)))
            {
                suggestions.Add(nearby);
            }
        }

        return suggestions;
    }

    private static bool TryResolveNearbyVueComponent(
        string documentPath,
        string componentName,
        out string componentPath,
        out string importPath)
    {
        componentPath = string.Empty;
        importPath = string.Empty;

        var documentDirectory = Path.GetDirectoryName(documentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory))
        {
            return false;
        }

        foreach (var directory in GetSearchDirectories(documentDirectory))
        {
            var candidate = Path.Combine(directory, componentName + ".vue");
            if (!File.Exists(candidate))
            {
                continue;
            }

            componentPath = NormalizeDocumentPath(candidate);
            importPath = ToImportPath(documentDirectory, candidate);
            return true;
        }

        return false;
    }

    private static bool TryResolveTrackedNearbyVueComponent(
        string documentPath,
        string componentName,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        out ResolvedVueComponent resolvedComponent)
    {
        var documentDirectory = Path.GetDirectoryName(documentPath);
        if (!string.IsNullOrWhiteSpace(documentDirectory))
        {
            foreach (var candidate in GetSearchDirectories(documentDirectory))
            {
                var expectedPath = NormalizeDocumentPath(Path.Combine(candidate, componentName + ".vue"));
                var tracked = openDocuments.FirstOrDefault(openDocument =>
                    openDocument.DocumentKind == DocumentKind.Vue
                    && string.Equals(
                        NormalizeDocumentPath(openDocument.DocumentPath),
                        expectedPath,
                        StringComparison.OrdinalIgnoreCase));
                if (tracked is not null)
                {
                    resolvedComponent = new ResolvedVueComponent(
                        componentName,
                        NormalizeDocumentPath(tracked.DocumentPath),
                        ToImportPath(documentDirectory, tracked.DocumentPath));
                    return true;
                }
            }
        }

        resolvedComponent = default;
        return false;
    }

    private static bool TryResolveTrackedVueComponent(
        string documentPath,
        string componentName,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        out ResolvedVueComponent resolvedComponent)
    {
        var documentDirectory = Path.GetDirectoryName(documentPath);
        var tracked = openDocuments.FirstOrDefault(openDocument =>
            openDocument.DocumentKind == DocumentKind.Vue
            && string.Equals(
                Path.GetFileNameWithoutExtension(openDocument.DocumentPath),
                componentName,
                StringComparison.Ordinal));
        if (tracked is not null && !string.IsNullOrWhiteSpace(documentDirectory))
        {
            resolvedComponent = new ResolvedVueComponent(
                componentName,
                NormalizeDocumentPath(tracked.DocumentPath),
                ToImportPath(documentDirectory, tracked.DocumentPath));
            return true;
        }

        resolvedComponent = default;
        return false;
    }

    private static IEnumerable<ResolvedVueComponent> EnumerateTrackedVueComponents(
        string documentPath,
        IReadOnlyList<DocumentSnapshot> openDocuments)
    {
        var documentDirectory = Path.GetDirectoryName(documentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory))
        {
            yield break;
        }

        foreach (var openDocument in openDocuments.Where(static candidate => candidate.DocumentKind == DocumentKind.Vue))
        {
            var componentName = Path.GetFileNameWithoutExtension(openDocument.DocumentPath);
            if (string.IsNullOrWhiteSpace(componentName) || !char.IsUpper(componentName[0]))
            {
                continue;
            }

            yield return new ResolvedVueComponent(
                componentName,
                NormalizeDocumentPath(openDocument.DocumentPath),
                ToImportPath(documentDirectory, openDocument.DocumentPath));
        }
    }

    private static IEnumerable<ResolvedVueComponent> EnumerateNearbyVueComponents(string documentPath)
    {
        var documentDirectory = Path.GetDirectoryName(documentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory))
        {
            yield break;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var directory in GetSearchDirectories(documentDirectory))
        {
            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (var filePath in Directory.EnumerateFiles(directory, "*.vue", SearchOption.TopDirectoryOnly))
            {
                var normalizedPath = NormalizeDocumentPath(filePath);
                if (!seen.Add(normalizedPath))
                {
                    continue;
                }

                var componentName = Path.GetFileNameWithoutExtension(filePath);
                if (string.IsNullOrWhiteSpace(componentName) || !char.IsUpper(componentName[0]))
                {
                    continue;
                }

                yield return new ResolvedVueComponent(
                    componentName,
                    normalizedPath,
                    ToImportPath(documentDirectory, normalizedPath));
            }
        }
    }

    private static IEnumerable<string> GetSearchDirectories(string documentDirectory)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parentDirectory = Directory.GetParent(documentDirectory)?.FullName;
        foreach (var directory in new[]
                 {
                     documentDirectory,
                     Path.Combine(documentDirectory, "Components"),
                     Path.Combine(documentDirectory, "components"),
                     parentDirectory,
                     parentDirectory is null ? null : Path.Combine(parentDirectory, "Components"),
                     parentDirectory is null ? null : Path.Combine(parentDirectory, "components")
                 })
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                continue;
            }

            var fullPath = Path.GetFullPath(directory);
            if (seen.Add(fullPath))
            {
                yield return fullPath;
            }
        }
    }

    private static string ToImportPath(string documentDirectory, string absolutePath)
    {
        var relativePath = Path.GetRelativePath(documentDirectory, absolutePath)
            .Replace('\\', '/');
        if (relativePath.StartsWith(".", StringComparison.Ordinal))
        {
            return relativePath;
        }

        return "./" + relativePath;
    }

    private static IEnumerable<string> GetCoLocatedAssetPaths(string documentPath)
    {
        var documentDirectory = Path.GetDirectoryName(documentPath);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(documentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory) || string.IsNullOrWhiteSpace(fileNameWithoutExtension))
        {
            yield break;
        }

        foreach (var extension in new[] { ".css", ".js", ".ts" })
        {
            yield return Path.Combine(documentDirectory, fileNameWithoutExtension + extension);
        }
    }

    private sealed record FoundSymbol(string SymbolName, LspRange Range);

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
                    NormalizeDocumentPath(openDocument.DocumentPath),
                    NormalizeDocumentPath(document.DocumentPath),
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            AddDocumentCandidate(openDocument, documents, seen);
        }

        AddDocumentCandidate(document, documents, seen);

        foreach (var directory in GetJazorSearchDirectories(document.DocumentPath, declarationDocumentPath))
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

        foreach (var filePath in Directory.EnumerateFiles(directory, "*.jazor", SearchOption.TopDirectoryOnly))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var normalizedPath = NormalizeDocumentPath(filePath);
            var openDocument = openDocuments.FirstOrDefault(candidate =>
                string.Equals(
                    NormalizeDocumentPath(candidate.DocumentPath),
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

            documents.Add(new DocumentSnapshot(
                normalizedPath,
                DocumentKind.Jazor,
                await File.ReadAllTextAsync(filePath, cancellationToken),
                version: null));
            seen.Add(normalizedPath);
        }
    }

    private static IEnumerable<string> GetJazorSearchDirectories(
        string documentPath,
        string? declarationDocumentPath)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in new[] { documentPath, declarationDocumentPath })
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            var directory = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(directory))
            {
                continue;
            }

            foreach (var searchDirectory in GetSearchDirectories(directory))
            {
                if (seen.Add(searchDirectory))
                {
                    yield return searchDirectory;
                }
            }
        }
    }

    private static void AddDocumentCandidate(
        DocumentSnapshot document,
        List<DocumentSnapshot> documents,
        HashSet<string> seen)
    {
        var normalizedPath = NormalizeDocumentPath(document.DocumentPath);
        if (!seen.Add(normalizedPath))
        {
            return;
        }

        documents.Add(document);
    }

    private static bool TryGetTagCompletionPrefix(string prefix, out string tagPrefix)
    {
        var match = TagCompletionPrefixPattern.Match(prefix);
        if (!match.Success)
        {
            tagPrefix = string.Empty;
            return false;
        }

        tagPrefix = match.Groups["name"].Value;
        return true;
    }

    private static IReadOnlyList<LspLocation> FindComponentTagLocations(DocumentSnapshot document, string componentName)
    {
        var locations = new List<LspLocation>();
        foreach (Match match in TagPattern.Matches(document.Text))
        {
            var group = match.Groups["name"];
            if (!string.Equals(group.Value, componentName, StringComparison.Ordinal))
            {
                continue;
            }

            locations.Add(new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath),
                Range = LspProtocolHelpers.ToRange(document.Text, group.Index, group.Length)
            });
        }

        return locations;
    }

    private readonly record struct ResolvedVueComponent(string ComponentName, string AbsolutePath, string ImportPath);

    private sealed record ResolvedImportSymbol(
        string SymbolName,
        LspRange DeclarationRange,
        string? DeclarationDocumentPath,
        ImportDescriptor Import);
}
