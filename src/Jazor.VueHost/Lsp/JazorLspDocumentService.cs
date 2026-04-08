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

    public async ValueTask<LspHoverResult?> GetHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        var symbol = TryFindSymbol(document.Text, position);
        if (symbol is null)
        {
            return null;
        }

        var response = await AnalyzeAsync(document, cancellationToken);
        var importMatch = response.Imports.FirstOrDefault(import =>
            string.Equals(import.LocalName, symbol.SymbolName, StringComparison.Ordinal));
        if (importMatch is null)
        {
            return null;
        }

        return new LspHoverResult
        {
            Contents = new LspMarkupContent
            {
                Kind = "markdown",
                Value = $"`{importMatch.LocalName}` from `{importMatch.Source}`\n\nkind: `{importMatch.ImportKind}`"
            },
            Range = symbol.Range
        };
    }

    public async ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        var offset = LspProtocolHelpers.GetOffset(document.Text, position);
        var prefix = document.Text[..Math.Min(offset, document.Text.Length)];
        var response = await AnalyzeAsync(document, cancellationToken);
        var items = new List<LspCompletionItem>();

        if (EndsWithDirectivePrefix(prefix))
        {
            items.Add(new LspCompletionItem
            {
                Label = "@vueimport",
                Kind = 14,
                Detail = "Jazor Vue import directive",
                Documentation = "Import a Vue component into the .jazor document."
            });
            items.Add(new LspCompletionItem
            {
                Label = "@jsimport",
                Kind = 14,
                Detail = "Jazor JavaScript import directive",
                Documentation = "Import a JavaScript or TypeScript symbol into the .jazor document."
            });
            items.Add(new LspCompletionItem
            {
                Label = "@code",
                Kind = 14,
                Detail = "Jazor code block",
                Documentation = "Start the C# code block for the current .jazor component."
            });
        }

        if (EndsWithTagPrefix(prefix))
        {
            items.AddRange(response.Imports
                .Where(static import => import.TemplateVisible)
                .Select(import => new LspCompletionItem
                {
                    Label = import.LocalName,
                    Kind = 7,
                    Detail = import.Source,
                    Documentation = $"Vue component imported from `{import.Source}`."
                }));
        }

        return items;
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        var symbol = TryFindSymbol(document.Text, position);
        if (symbol is null)
        {
            return Array.Empty<LspLocation>();
        }

        var response = await AnalyzeAsync(document, cancellationToken);
        var importMatch = response.Imports.FirstOrDefault(import =>
            string.Equals(import.LocalName, symbol.SymbolName, StringComparison.Ordinal));
        if (importMatch is null)
        {
            return Array.Empty<LspLocation>();
        }

        return
        [
            new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(ResolveImportPath(document.DocumentPath, importMatch.Source)),
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
            var openDocument = openDocuments.FirstOrDefault(candidate =>
                string.Equals(
                    NormalizeDocumentPath(candidate.DocumentPath),
                    NormalizeDocumentPath(importPath),
                    StringComparison.OrdinalIgnoreCase));
            if (openDocument is not null)
            {
                if (seen.Add(NormalizeDocumentPath(openDocument.DocumentPath)))
                {
                    relatedDocuments.Add(openDocument);
                }

                continue;
            }

            if (!File.Exists(importPath))
            {
                continue;
            }

            if (!seen.Add(NormalizeDocumentPath(importPath)))
            {
                continue;
            }

            relatedDocuments.Add(new DocumentSnapshot(
                importPath,
                MapDocumentKind(importPath),
                await File.ReadAllTextAsync(importPath, cancellationToken),
                version: null));
        }

        return relatedDocuments;
    }

    private static bool EndsWithDirectivePrefix(string prefix)
    {
        return prefix.EndsWith("@", StringComparison.Ordinal)
            || prefix.EndsWith("@v", StringComparison.Ordinal)
            || prefix.EndsWith("@j", StringComparison.Ordinal)
            || prefix.EndsWith("@c", StringComparison.Ordinal);
    }

    private static bool EndsWithTagPrefix(string prefix)
    {
        return prefix.EndsWith("<", StringComparison.Ordinal)
            || prefix.EndsWith("</", StringComparison.Ordinal);
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

        var response = await AnalyzeAsync(document, cancellationToken);
        var importMatch = response.Imports.FirstOrDefault(import =>
            string.Equals(import.LocalName, symbol.SymbolName, StringComparison.Ordinal));
        if (importMatch is null)
        {
            return null;
        }

        return new ResolvedImportSymbol(symbol.SymbolName, symbol.Range, importMatch);
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

    private sealed record FoundSymbol(string SymbolName, LspRange Range);

    private sealed record ResolvedImportSymbol(string SymbolName, LspRange DeclarationRange, ImportDescriptor Import);
}
