using System.Text.RegularExpressions;
using Jazor.Vue;
using Jazor.VueHost.Analysis;
using Jazor.VueHost.Lsp.Coordination;
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
    private readonly MarkupComponentBridgeService _markupComponentBridge;
    private readonly FallbackJazorAnalysisService _fallbackAnalysisService = new();
    private readonly JazorVueParser _parser = new();

    public JazorLspDocumentService(
        IVueHostWorkspaceStore workspaceStore,
        IVueAnalysisClient analysisClient,
        MarkupComponentBridgeService? markupComponentBridge = null)
    {
        _workspaceStore = workspaceStore ?? throw new ArgumentNullException(nameof(workspaceStore));
        _analysisClient = analysisClient ?? throw new ArgumentNullException(nameof(analysisClient));
        _markupComponentBridge = markupComponentBridge ?? new MarkupComponentBridgeService(workspaceStore);
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
        => await _markupComponentBridge.GetHoverAsync(document, position, allowWorkspaceScan: true, cancellationToken);

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
            foreach (var suggestion in await _markupComponentBridge.GetComponentSuggestionsAsync(
                         document.DocumentPath,
                         allowWorkspaceScan: true,
                         cancellationToken))
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

    public async ValueTask<bool> IsVueComponentResolvableAsync(
        DocumentSnapshot document,
        string componentName,
        CancellationToken cancellationToken)
        => await _markupComponentBridge.ResolveComponentAsync(
            document.DocumentPath,
            componentName,
            allowWorkspaceScan: true,
            cancellationToken) is not null;

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
}
