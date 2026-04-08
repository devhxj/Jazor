using System.Text.RegularExpressions;
using Jazor.Vue;
using Jazor.VueHost.Analysis;
using Jazor.VueHost.Workspace;
using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Lsp;

internal sealed class JazorLspDocumentService
{
    private static readonly Regex TagPattern = new(@"<(?<name>[A-Z][A-Za-z0-9_]*)\b", RegexOptions.Compiled);
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
    {
        cancellationToken.ThrowIfCancellationRequested();

        var symbol = TryFindSymbol(document.Text, position);
        if (symbol is null)
        {
            return ValueTask.FromResult<LspHoverResult?>(null);
        }

        if (!TryResolveNearbyVueComponent(document.DocumentPath, symbol.SymbolName, out _, out var resolvedImportPath))
        {
            return ValueTask.FromResult<LspHoverResult?>(null);
        }

        return ValueTask.FromResult<LspHoverResult?>(new LspHoverResult
        {
            Contents = new LspMarkupContent
            {
                Kind = "markdown",
                Value = $"`{symbol.SymbolName}` resolved from Razor markup to `{resolvedImportPath}`\n\nkind: `VueComponent`"
            },
            Range = symbol.Range
        });
    }

    public ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
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

        return ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(items);
    }

    public ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var symbol = TryFindSymbol(document.Text, position);
        if (symbol is null)
        {
            return ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());
        }

        if (!TryResolveNearbyVueComponent(document.DocumentPath, symbol.SymbolName, out var targetPath, out _))
        {
            return ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());
        }

        return ValueTask.FromResult<IReadOnlyList<LspLocation>>(
        [
            new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(targetPath),
                Range = new LspRange
                {
                    Start = new LspPosition { Line = 0, Character = 0 },
                    End = new LspPosition { Line = 0, Character = 0 }
                }
            }
        ]);
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

        var locations = FindSymbolLocations(document, symbol.SymbolName, includeDeclaration, symbol.DeclarationRange);
        return locations;
    }

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

        var locations = FindSymbolLocations(document, symbol.SymbolName, includeDeclaration: true, symbol.DeclarationRange);
        if (locations.Count == 0)
        {
            return null;
        }

        return new LspWorkspaceEdit
        {
            Changes = new Dictionary<string, LspTextEdit[]>
            {
                [LspProtocolHelpers.ToDocumentUri(document.DocumentPath)] = locations
                    .Select(location => new LspTextEdit
                    {
                        Range = location.Range,
                        NewText = newName
                    })
                    .OrderByDescending(edit => LspProtocolHelpers.GetOffset(document.Text, edit.Range.Start))
                    .ToArray()
            }
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

        return relatedDocuments;
    }

    private static bool EndsWithDirectivePrefix(string prefix)
    {
        return prefix.EndsWith("@", StringComparison.Ordinal)
            || prefix.EndsWith("@c", StringComparison.Ordinal);
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

        if (TryResolveNearbyVueComponent(document.DocumentPath, symbol.SymbolName, out _, out var nearbyImportPath))
        {
            return new ResolvedImportSymbol(
                symbol.SymbolName,
                symbol.Range,
                new ImportDescriptor(
                    symbol.SymbolName,
                    nearbyImportPath,
                    ImportKind.VueImport,
                    ImportBindingKind.Default,
                    importedName: null,
                    templateVisible: true));
        }

        return null;
    }

    private IReadOnlyList<LspLocation> FindSymbolLocations(
        DocumentSnapshot document,
        string symbolName,
        bool includeDeclaration,
        LspRange declarationRange)
    {
        var locations = new List<LspLocation>();
        if (includeDeclaration)
        {
            locations.Add(new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath),
                Range = declarationRange
            });
        }

        var wordPattern = new Regex(@"\b" + Regex.Escape(symbolName) + @"\b", RegexOptions.Compiled);
        foreach (Match match in wordPattern.Matches(document.Text))
        {
            var range = LspProtocolHelpers.ToRange(document.Text, match.Index, match.Length);
            if (!includeDeclaration && RangesEqual(range, declarationRange))
            {
                continue;
            }

            if (includeDeclaration || !RangesEqual(range, declarationRange))
            {
                locations.Add(new LspLocation
                {
                    Uri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath),
                    Range = range
                });
            }
        }

        return locations
            .GroupBy(static location => $"{location.Uri}:{location.Range.Start.Line}:{location.Range.Start.Character}:{location.Range.End.Line}:{location.Range.End.Character}", StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
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

    private sealed record FoundSymbol(string SymbolName, LspRange Range);

    private sealed record ResolvedImportSymbol(string SymbolName, LspRange DeclarationRange, ImportDescriptor Import);
}
