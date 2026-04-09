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

    public ValueTask<IReadOnlyList<LspDocumentSymbol>> GetDocumentSymbolsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (document.DocumentKind != DocumentKind.Jazor)
        {
            return ValueTask.FromResult<IReadOnlyList<LspDocumentSymbol>>(Array.Empty<LspDocumentSymbol>());
        }

        var parsed = _parser.Parse(document.DocumentPath, document.Text);
        var symbols = new List<LspDocumentSymbol>();

        if (parsed.TemplateStartIndex >= 0 && parsed.TemplateLength > 0)
        {
            var templateRange = LspProtocolHelpers.ToRange(document.Text, parsed.TemplateStartIndex, parsed.TemplateLength);
            var componentSymbols = CreateTemplateComponentSymbols(document, parsed);
            symbols.Add(new LspDocumentSymbol
            {
                Name = "Template",
                Kind = 2,
                Range = templateRange,
                SelectionRange = templateRange,
                Children = componentSymbols.Length == 0 ? null : componentSymbols
            });
        }

        if (parsed.CodeStartIndex >= 0 && parsed.CodeLength > 0)
        {
            var codeRange = LspProtocolHelpers.ToRange(document.Text, parsed.CodeStartIndex, parsed.CodeLength);
            symbols.Add(new LspDocumentSymbol
            {
                Name = "Code",
                Kind = 2,
                Range = codeRange,
                SelectionRange = codeRange
            });
        }

        return ValueTask.FromResult<IReadOnlyList<LspDocumentSymbol>>(symbols);
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
            foreach (var importPath in VueHostWorkspaceResolver.GetImportPathCandidates(document.DocumentPath, import.Source))
            {
                await AddRelatedDocumentAsync(importPath, openDocuments, relatedDocuments, seen, cancellationToken);
            }
        }

        var referencedComponentNames = TagPattern.Matches(document.Text)
            .Select(static match => match.Groups["name"].Value)
            .Distinct(StringComparer.Ordinal);
        foreach (var componentName in referencedComponentNames)
        {
            if (VueHostWorkspaceResolver.TryResolveTrackedNearbyVueComponent(document.DocumentPath, componentName, openDocuments, out var trackedNearby))
            {
                await AddRelatedDocumentAsync(trackedNearby.AbsolutePath, openDocuments, relatedDocuments, seen, cancellationToken);
                continue;
            }

            if (VueHostWorkspaceResolver.TryResolveNearbyVueComponent(document.DocumentPath, componentName, out var componentPath, out _))
            {
                await AddRelatedDocumentAsync(componentPath, openDocuments, relatedDocuments, seen, cancellationToken);
                continue;
            }

            if (VueHostWorkspaceResolver.TryResolveTrackedVueComponent(document.DocumentPath, componentName, openDocuments, out var tracked))
            {
                await AddRelatedDocumentAsync(tracked.AbsolutePath, openDocuments, relatedDocuments, seen, cancellationToken);
                continue;
            }

            if (VueHostWorkspaceResolver.ResolveWorkspaceVueComponent(document.DocumentPath, componentName, openDocuments, cancellationToken) is not { } workspaceResolved)
            {
                continue;
            }

            await AddRelatedDocumentAsync(workspaceResolved.AbsolutePath, openDocuments, relatedDocuments, seen, cancellationToken);
        }

        foreach (var assetPath in VueHostWorkspaceResolver.GetCoLocatedAssetPaths(document.DocumentPath))
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
        => VueHostWorkspaceResolver.MapDocumentKind(documentPath);

    private static string NormalizeDocumentPath(string documentPath)
        => VueHostWorkspaceResolver.NormalizePath(documentPath);

    private async ValueTask AddRelatedDocumentAsync(
        string documentPath,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        List<DocumentSnapshot> relatedDocuments,
        HashSet<string> seen,
        CancellationToken cancellationToken)
    {
        var document = await VueHostWorkspaceResolver.ResolveDocumentAsync(documentPath, openDocuments, cancellationToken);
        if (document is null)
        {
            return;
        }

        var normalizedPath = NormalizeDocumentPath(document.DocumentPath);
        if (!seen.Add(normalizedPath))
        {
            return;
        }

        relatedDocuments.Add(document);
    }

    private async ValueTask<ResolvedVueComponent?> ResolveVueComponentAsync(
        string documentPath,
        string componentName,
        CancellationToken cancellationToken)
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

        if (VueHostWorkspaceResolver.ResolveWorkspaceVueComponent(documentPath, componentName, openDocuments, cancellationToken) is { } workspaceResolved)
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
            if (seenPaths.Add(NormalizeDocumentPath(tracked.AbsolutePath)))
            {
                suggestions.Add(new ResolvedVueComponent(tracked.ComponentName, tracked.AbsolutePath, tracked.ImportPath));
            }
        }

        foreach (var nearby in VueHostWorkspaceResolver.EnumerateNearbyVueComponents(documentPath))
        {
            if (seenPaths.Add(NormalizeDocumentPath(nearby.AbsolutePath)))
            {
                suggestions.Add(new ResolvedVueComponent(nearby.ComponentName, nearby.AbsolutePath, nearby.ImportPath));
            }
        }

        foreach (var workspace in VueHostWorkspaceResolver.EnumerateWorkspaceVueComponents(documentPath, openDocuments, cancellationToken))
        {
            if (seenPaths.Add(NormalizeDocumentPath(workspace.AbsolutePath)))
            {
                suggestions.Add(new ResolvedVueComponent(workspace.ComponentName, workspace.AbsolutePath, workspace.ImportPath));
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

    private static ResolvedVueComponent? ResolveWorkspaceVueComponent(
        string documentPath,
        string componentName,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        CancellationToken cancellationToken)
    {
        var documentDirectory = Path.GetDirectoryName(documentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory))
        {
            return null;
        }

        foreach (var filePath in EnumerateWorkspaceFiles(
                     GetWorkspaceSearchRoots(documentPath, declarationDocumentPath: null, openDocuments),
                     componentName + ".vue",
                     cancellationToken))
        {
            return new ResolvedVueComponent(
                componentName,
                filePath,
                ToImportPath(documentDirectory, filePath));
        }

        return null;
    }

    private static IEnumerable<ResolvedVueComponent> EnumerateWorkspaceVueComponents(
        string documentPath,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        CancellationToken cancellationToken)
    {
        var documentDirectory = Path.GetDirectoryName(documentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory))
        {
            yield break;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var filePath in EnumerateWorkspaceFiles(
                     GetWorkspaceSearchRoots(documentPath, declarationDocumentPath: null, openDocuments),
                     "*.vue",
                     cancellationToken))
        {
            var componentName = Path.GetFileNameWithoutExtension(filePath);
            if (string.IsNullOrWhiteSpace(componentName) || !char.IsUpper(componentName[0]))
            {
                continue;
            }

            var normalizedPath = NormalizeDocumentPath(filePath);
            if (!seen.Add(normalizedPath))
            {
                continue;
            }

            yield return new ResolvedVueComponent(
                componentName,
                normalizedPath,
                ToImportPath(documentDirectory, normalizedPath));
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

    private static IEnumerable<string> GetWorkspaceSearchRoots(
        string documentPath,
        string? declarationDocumentPath,
        IReadOnlyList<DocumentSnapshot> openDocuments)
    {
        var directories = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in new[] { documentPath, declarationDocumentPath }
                     .Concat(openDocuments
                         .Where(static document => document.DocumentKind is DocumentKind.Jazor or DocumentKind.Vue)
                         .Select(static document => document.DocumentPath)))
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

            var normalizedDirectory = Path.GetFullPath(directory);
            if (seen.Add(normalizedDirectory))
            {
                directories.Add(normalizedDirectory);
            }
        }

        if (directories.Count == 0)
        {
            yield break;
        }

        if (directories.Count == 1)
        {
            foreach (var ancestor in EnumerateSearchAncestors(directories[0]))
            {
                yield return ancestor;
            }

            yield break;
        }

        if (TryGetCommonSearchAncestor(directories) is { } commonAncestor)
        {
            yield return commonAncestor;
            yield break;
        }

        var emitted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var directory in directories)
        {
            foreach (var ancestor in EnumerateSearchAncestors(directory))
            {
                if (emitted.Add(ancestor))
                {
                    yield return ancestor;
                }
            }
        }
    }

    private static IEnumerable<string> EnumerateSearchAncestors(string directory)
    {
        var current = Path.GetFullPath(directory);
        var depth = 0;
        while (!string.IsNullOrWhiteSpace(current) && depth < 3)
        {
            if (string.Equals(current, Path.GetPathRoot(current), StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }

            yield return current;
            depth++;

            var parent = Directory.GetParent(current)?.FullName;
            if (string.IsNullOrWhiteSpace(parent)
                || string.Equals(parent, current, StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }

            current = parent;
        }
    }

    private static string? TryGetCommonSearchAncestor(IReadOnlyList<string> directories)
    {
        if (directories.Count == 0)
        {
            return null;
        }

        var current = directories[0];
        for (var index = 1; index < directories.Count; index++)
        {
            current = GetCommonAncestor(current, directories[index]);
            if (string.IsNullOrWhiteSpace(current))
            {
                return null;
            }
        }

        return string.Equals(current, Path.GetPathRoot(current), StringComparison.OrdinalIgnoreCase)
            ? null
            : current;
    }

    private static string? GetCommonAncestor(string left, string right)
    {
        var candidate = Path.GetFullPath(left);
        var normalizedRight = NormalizeDocumentPath(right);
        while (!string.IsNullOrWhiteSpace(candidate)
               && !string.Equals(candidate, Path.GetPathRoot(candidate), StringComparison.OrdinalIgnoreCase))
        {
            var normalizedCandidate = NormalizeDocumentPath(candidate);
            if (normalizedRight.StartsWith(normalizedCandidate + "/", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalizedRight, normalizedCandidate, StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }

            candidate = Directory.GetParent(candidate)?.FullName;
        }

        return null;
    }

    private static IEnumerable<string> EnumerateWorkspaceFiles(
        IEnumerable<string> searchRoots,
        string searchPattern,
        CancellationToken cancellationToken)
    {
        var visitedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visitedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var searchRoot in searchRoots)
        {
            if (!Directory.Exists(searchRoot))
            {
                continue;
            }

            var pendingDirectories = new Stack<string>();
            pendingDirectories.Push(searchRoot);

            while (pendingDirectories.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var currentDirectory = pendingDirectories.Pop();
                var normalizedDirectory = NormalizeDocumentPath(currentDirectory);
                if (!visitedDirectories.Add(normalizedDirectory) || ShouldSkipWorkspaceDirectory(currentDirectory))
                {
                    continue;
                }

                IEnumerable<string> files;
                try
                {
                    files = Directory.EnumerateFiles(currentDirectory, searchPattern, SearchOption.TopDirectoryOnly);
                }
                catch (IOException)
                {
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                foreach (var filePath in files)
                {
                    var normalizedPath = NormalizeDocumentPath(filePath);
                    if (visitedFiles.Add(normalizedPath))
                    {
                        yield return normalizedPath;
                    }
                }

                IEnumerable<string> directories;
                try
                {
                    directories = Directory.EnumerateDirectories(currentDirectory, "*", SearchOption.TopDirectoryOnly);
                }
                catch (IOException)
                {
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                foreach (var childDirectory in directories)
                {
                    if (!ShouldSkipWorkspaceDirectory(childDirectory))
                    {
                        pendingDirectories.Push(childDirectory);
                    }
                }
            }
        }
    }

    private static bool ShouldSkipWorkspaceDirectory(string directoryPath)
    {
        var directoryName = Path.GetFileName(directoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        return directoryName switch
        {
            ".git" => true,
            ".hg" => true,
            ".svn" => true,
            ".vs" => true,
            ".idea" => true,
            "bin" => true,
            "obj" => true,
            "node_modules" => true,
            ".deno" => true,
            _ => false
        };
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
                // Workspace scans are a best-effort IntelliSense heuristic.
                // If a file is transiently unavailable, skip it instead of
                // failing the whole reference/rename request path.
            }
            catch (UnauthorizedAccessException)
            {
                // The host should degrade gracefully when a scanned file is not readable.
            }
        }
    }

    private static IEnumerable<string> GetJazorSearchDirectories(
        string documentPath,
        string? declarationDocumentPath,
        IReadOnlyList<DocumentSnapshot> openDocuments)
    {
        return GetWorkspaceSearchRoots(documentPath, declarationDocumentPath, openDocuments);
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

    private static LspDocumentSymbol[] CreateTemplateComponentSymbols(
        DocumentSnapshot document,
        JazorVueDocument parsed)
    {
        var symbols = new List<LspDocumentSymbol>();
        foreach (Match match in TagPattern.Matches(parsed.Template))
        {
            var group = match.Groups["name"];
            if (!group.Success)
            {
                continue;
            }

            var sourceIndex = parsed.TemplateStartIndex + group.Index;
            var range = LspProtocolHelpers.ToRange(document.Text, sourceIndex, group.Length);
            symbols.Add(new LspDocumentSymbol
            {
                Name = group.Value,
                Kind = 5,
                Range = range,
                SelectionRange = range
            });
        }

        return symbols
            .GroupBy(
                static symbol => $"{symbol.Name}:{symbol.Range.Start.Line}:{symbol.Range.Start.Character}:{symbol.Range.End.Line}:{symbol.Range.End.Character}",
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    private readonly record struct ResolvedVueComponent(string ComponentName, string AbsolutePath, string ImportPath);

    private sealed record ResolvedImportSymbol(
        string SymbolName,
        LspRange DeclarationRange,
        string? DeclarationDocumentPath,
        ImportDescriptor Import);
}
