using System.Text.RegularExpressions;
using Jazor.Vue;
using Jolt.Analysis;
using Jolt.Lsp.Coordination;
using Jolt.Workspace;
using Jazor.VueContracts.Protocol;

namespace Jolt.Lsp;

internal sealed class JazorLspDocumentService
{
    private static readonly Regex TagPattern = new(@"<(?<name>[A-Z][A-Za-z0-9_]*)\b", RegexOptions.Compiled);
    private static readonly Regex TagCompletionPrefixPattern = new(@"</?(?<name>[A-Za-z0-9_]*)$", RegexOptions.Compiled);
    private static readonly Regex PrivateMethodPattern = new(@"(?<modifier>\bprivate\b)\s+(?<signature>(?:async\s+)?[\w<>\.\?]+\s+\w+\s*\()", RegexOptions.Compiled);
    private readonly IVueAnalysisClient _analysisClient;
    private readonly MarkupComponentBridgeService _markupComponentBridge;
    private readonly JazorRelatedDocumentResolver _relatedDocumentResolver;
    private readonly FallbackJazorAnalysisService _fallbackAnalysisService = new();
    private readonly JazorVueParser _parser = new();

    public JazorLspDocumentService(
        IJoltWorkspaceStore workspaceStore,
        IVueAnalysisClient analysisClient,
        MarkupComponentBridgeService? markupComponentBridge = null)
    {
        ArgumentNullException.ThrowIfNull(workspaceStore);
        _analysisClient = analysisClient ?? throw new ArgumentNullException(nameof(analysisClient));
        _markupComponentBridge = markupComponentBridge ?? new MarkupComponentBridgeService(workspaceStore);
        _relatedDocumentResolver = new JazorRelatedDocumentResolver(workspaceStore);
    }

    public async ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        var response = await AnalyzeAsync(document, cancellationToken);
        var diagnostics = response.Diagnostics
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
                Source = "Jolt",
                Message = diagnostic.Message
            })
            .ToList();

        return diagnostics
            .GroupBy(
                static diagnostic => $"{diagnostic.Code}:{GetRangeKey(diagnostic.Range)}:{diagnostic.Message}",
                StringComparer.Ordinal)
            .Select(static group => group.First())
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

    public async ValueTask<IReadOnlyList<LspDocumentHighlight>> GetDocumentHighlightsAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var symbol = await _markupComponentBridge.ResolveBridgeSymbolAsync(
            document,
            position,
            locationHints: null,
            allowWorkspaceScan: true,
            cancellationToken);
        var componentName = symbol?.ComponentName;
        var highlightRanges = new List<LspRange>();

        if (symbol is not null)
        {
            var referenceLocations = await _markupComponentBridge.FindJazorReferencesAsync(
                document,
                symbol.Value.ComponentName,
                symbol.Value.AbsolutePath,
                includeDeclaration: true,
                cancellationToken);
            var currentDocumentPath = NormalizeDocumentPath(document.DocumentPath);
            highlightRanges.AddRange(referenceLocations
                .Where(location =>
                {
                    var referencePath = NormalizeDocumentPath(LspProtocolHelpers.ToDocumentPath(location.Uri));
                    return string.Equals(referencePath, currentDocumentPath, StringComparison.OrdinalIgnoreCase);
                })
                .Select(static location => location.Range));
        }

        if (string.IsNullOrWhiteSpace(componentName)
            && TryGetComponentTagNameAtPosition(document.Text, position, out var positionComponentName))
        {
            componentName = positionComponentName;
        }

        if (!string.IsNullOrWhiteSpace(componentName))
        {
            foreach (Match match in TagPattern.Matches(document.Text))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var nameGroup = match.Groups["name"];
                if (!nameGroup.Success
                    || !string.Equals(nameGroup.Value, componentName, StringComparison.Ordinal))
                {
                    continue;
                }

                highlightRanges.Add(LspProtocolHelpers.ToRange(document.Text, nameGroup.Index, nameGroup.Length));
            }
        }

        return highlightRanges
            .Select(static range => new LspDocumentHighlight
            {
                Range = range,
                Kind = 1
            })
            .GroupBy(
                static highlight =>
                    $"{highlight.Range.Start.Line}:{highlight.Range.Start.Character}:{highlight.Range.End.Line}:{highlight.Range.End.Character}:{highlight.Kind}",
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    public ValueTask<IReadOnlyList<LspDocumentLink>> GetDocumentLinksAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (document.DocumentKind != DocumentKind.Jazor)
        {
            return ValueTask.FromResult<IReadOnlyList<LspDocumentLink>>(Array.Empty<LspDocumentLink>());
        }

        var links = new List<LspDocumentLink>();
        foreach (var match in JazorImportDirectiveLocator.EnumerateModuleDirectives(document.Text))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (match.SourceIndex < 0
                || match.SourceLength <= 0
                || !TryResolveImportLinkTargetPath(document.DocumentPath, match.Source, out var targetPath))
            {
                continue;
            }

            links.Add(new LspDocumentLink
            {
                Range = LspProtocolHelpers.ToRange(document.Text, match.SourceIndex, match.SourceLength),
                Target = LspProtocolHelpers.ToDocumentUri(targetPath),
                Tooltip = "Open import target"
            });
        }

        return ValueTask.FromResult<IReadOnlyList<LspDocumentLink>>(links
            .GroupBy(
                static link => $"{link.Range.Start.Line}:{link.Range.Start.Character}:{link.Range.End.Line}:{link.Range.End.Character}:{link.Target}",
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray());
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

    public ValueTask<IReadOnlyList<LspSemanticToken>> GetSemanticTokensAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (document.DocumentKind != DocumentKind.Jazor)
        {
            return ValueTask.FromResult<IReadOnlyList<LspSemanticToken>>(Array.Empty<LspSemanticToken>());
        }

        var parsed = _parser.Parse(document.DocumentPath, document.Text);
        var tokens = new List<LspSemanticToken>();

        // Template wrapper tags: <template> and </template>
        AddTemplateWrapperTokens(document.Text, tokens);

        // @code directive keyword
        AddCodeDirectiveTokens(document.Text, tokens);

        // Import directives: canonical @module plus unsupported legacy forms for highlighting.
        AddImportDirectiveTokens(document.Text, tokens);

        // Component tags (PascalCase) in template region
        if (parsed.TemplateStartIndex >= 0 && parsed.TemplateLength > 0)
        {
            AddComponentTagTokens(document.Text, parsed, tokens);
        }

        return ValueTask.FromResult<IReadOnlyList<LspSemanticToken>>(tokens);
    }

    private static void AddTemplateWrapperTokens(string text, List<LspSemanticToken> tokens)
    {
        var openMatch = Regex.Match(text, @"<template\b", RegexOptions.IgnoreCase);
        if (openMatch.Success)
        {
            var pos = LspProtocolHelpers.GetPosition(text, openMatch.Index);
            tokens.Add(new LspSemanticToken
            {
                Line = pos.Line,
                Character = pos.Character,
                Length = "template".Length,
                TokenType = "decorator"
            });
        }

        var closeMatch = Regex.Match(text, @"</template\s*>", RegexOptions.IgnoreCase);
        if (closeMatch.Success)
        {
            var pos = LspProtocolHelpers.GetPosition(text, closeMatch.Index + 2);
            tokens.Add(new LspSemanticToken
            {
                Line = pos.Line,
                Character = pos.Character,
                Length = "template".Length,
                TokenType = "decorator"
            });
        }
    }

    private static void AddCodeDirectiveTokens(string text, List<LspSemanticToken> tokens)
    {
        foreach (var match in JazorCodeDirectiveLocator.EnumerateCodeDirectives(text))
        {
            if (!match.HasBlockBody)
            {
                continue;
            }

            var pos = LspProtocolHelpers.GetPosition(text, match.DirectiveIndex);
            tokens.Add(new LspSemanticToken
            {
                Line = pos.Line,
                Character = pos.Character,
                Length = match.DirectiveLength,
                TokenType = "keyword"
            });
        }
    }

    private static void AddImportDirectiveTokens(string text, List<LspSemanticToken> tokens)
    {
        foreach (var match in JazorImportDirectiveLocator.EnumerateDirectiveLines(text))
        {
            var atPos = LspProtocolHelpers.GetPosition(text, match.DirectiveIndex);
            tokens.Add(new LspSemanticToken
            {
                Line = atPos.Line,
                Character = atPos.Character,
                Length = match.DirectiveLength,
                TokenType = "keyword"
            });
        }
    }

    private static void AddComponentTagTokens(
        string text,
        JazorVueDocument parsed,
        List<LspSemanticToken> tokens)
    {
        foreach (Match match in TagPattern.Matches(parsed.Template))
        {
            var group = match.Groups["name"];
            if (!group.Success)
            {
                continue;
            }

            var sourceIndex = parsed.TemplateStartIndex + group.Index;
            var pos = LspProtocolHelpers.GetPosition(text, sourceIndex);
            tokens.Add(new LspSemanticToken
            {
                Line = pos.Line,
                Character = pos.Character,
                Length = group.Length,
                TokenType = "class",
                TokenModifiers = ["declaration"]
            });
        }
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

    public async ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var symbol = await _markupComponentBridge.ResolveBridgeSymbolAsync(
            document,
            position,
            locationHints: null,
            allowWorkspaceScan: true,
            cancellationToken);

        if (symbol is null)
        {
            return Array.Empty<LspLocation>();
        }

        return
        [
            new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(symbol.Value.AbsolutePath),
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
        cancellationToken.ThrowIfCancellationRequested();

        var symbol = await _markupComponentBridge.ResolveBridgeSymbolAsync(
            document,
            position,
            locationHints: null,
            allowWorkspaceScan: true,
            cancellationToken);

        if (symbol is null)
        {
            return Array.Empty<LspLocation>();
        }

        return await _markupComponentBridge.FindJazorReferencesAsync(
            document,
            symbol.Value.ComponentName,
            symbol.Value.AbsolutePath,
            includeDeclaration,
            cancellationToken);
    }

    public async ValueTask<LspWorkspaceEdit?> GetRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var symbol = await _markupComponentBridge.ResolveBridgeSymbolAsync(
            document,
            position,
            locationHints: null,
            allowWorkspaceScan: true,
            cancellationToken);

        if (symbol is null)
        {
            return null;
        }

        var changes = await _markupComponentBridge.FindJazorRenameChangesAsync(
            document,
            symbol.Value.ComponentName,
            symbol.Value.AbsolutePath,
            newName,
            cancellationToken);

        if (changes.Count == 0)
        {
            return null;
        }

        return new LspWorkspaceEdit
        {
            Changes = changes
        };
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

        if (diagnostics.Count == 0)
        {
            return ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(Array.Empty<LspCodeAction>());
        }

        var actions = new List<LspCodeAction>();

        if (TryFindPrivateMethodModifierForDiagnostic(document, diagnostics, out var privateMethodModifier))
        {
            actions.Add(new LspCodeAction
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
                                Range = LspProtocolHelpers.ToRange(document.Text, privateMethodModifier.Index, privateMethodModifier.Length),
                                NewText = "public"
                            }
                        ]
                    }
                }
            });
        }

        var legacyDirectiveDiagnostics = diagnostics
            .Where(static diagnostic => string.Equals(diagnostic.Code, LegacyImportDirectiveCatalog.DiagnosticCode, StringComparison.Ordinal))
            .ToArray();
        if (legacyDirectiveDiagnostics.Length > 0)
        {
            var rangeKeys = legacyDirectiveDiagnostics
                .Select(static diagnostic => GetRangeKey(diagnostic.Range))
                .ToHashSet(StringComparer.Ordinal);
            actions.AddRange(CreateLegacyImportDirectiveCodeActions(document, rangeKeys));
        }

        if (actions.Count == 0)
        {
            return ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(Array.Empty<LspCodeAction>());
        }

        return ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(actions);
    }

    private static IReadOnlyList<LspCodeAction> CreateLegacyImportDirectiveCodeActions(
        DocumentSnapshot document,
        HashSet<string> rangeKeys)
    {
        if (document.DocumentKind != DocumentKind.Jazor)
        {
            return Array.Empty<LspCodeAction>();
        }

        var actions = new List<LspCodeAction>();
        foreach (var occurrence in LegacyImportDirectiveCatalog.FindOccurrences(document.Text))
        {
            var range = LspProtocolHelpers.ToRange(document.Text, occurrence.Start, occurrence.Length);
            var rangeKey = GetRangeKey(range);
            if (rangeKeys.Count > 0 && !rangeKeys.Contains(rangeKey))
            {
                continue;
            }

            var legacyDirective = "@" + occurrence.Kind;
            actions.Add(new LspCodeAction
            {
                Title = $"Replace {legacyDirective} with @module",
                Kind = "quickfix",
                Edit = new LspWorkspaceEdit
                {
                    Changes = new Dictionary<string, LspTextEdit[]>
                    {
                        [LspProtocolHelpers.ToDocumentUri(document.DocumentPath)] =
                        [
                            new LspTextEdit
                            {
                                Range = range,
                                NewText = "@module"
                            }
                        ]
                    }
                }
            });
        }

        return actions;
    }

    private static string GetRangeKey(LspRange range)
        => $"{range.Start.Line}:{range.Start.Character}:{range.End.Line}:{range.End.Character}";

    private static bool TryFindPrivateMethodModifierForDiagnostic(
        DocumentSnapshot document,
        IReadOnlyList<LspDiagnostic> diagnostics,
        out Group privateMethodModifier)
    {
        privateMethodModifier = default!;
        var privateMethodMatches = PrivateMethodPattern
            .Matches(document.Text)
            .Where(static match => match.Success && match.Groups["modifier"].Success)
            .ToArray();
        if (privateMethodMatches.Length == 0)
        {
            return false;
        }

        foreach (var diagnostic in diagnostics.Where(static diagnostic =>
                     string.Equals(diagnostic.Code, "JAZORVUE001", StringComparison.Ordinal)))
        {
            if (!TryGetRangeOffsets(document.Text, diagnostic.Range, out var diagnosticStart, out var diagnosticEnd))
            {
                continue;
            }

            foreach (var match in privateMethodMatches)
            {
                var candidateStart = match.Groups["modifier"].Index;
                var candidateEnd = FindMethodDeclarationEnd(document.Text, match);
                if (RangesOverlapOrTouch(diagnosticStart, diagnosticEnd, candidateStart, candidateEnd))
                {
                    privateMethodModifier = match.Groups["modifier"];
                    return true;
                }
            }
        }

        if (privateMethodMatches.Length == 1)
        {
            privateMethodModifier = privateMethodMatches[0].Groups["modifier"];
            return true;
        }

        return false;
    }

    private static bool TryGetRangeOffsets(
        string text,
        LspRange range,
        out int startOffset,
        out int endOffset)
    {
        startOffset = default;
        endOffset = default;
        try
        {
            startOffset = LspProtocolHelpers.GetOffset(text, range.Start);
            endOffset = LspProtocolHelpers.GetOffset(text, range.End);
            if (endOffset < startOffset)
            {
                endOffset = startOffset;
            }

            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    private static int FindMethodDeclarationEnd(string text, Match privateMethodMatch)
    {
        var braceIndex = text.IndexOf('{', privateMethodMatch.Index + privateMethodMatch.Length);
        return braceIndex >= 0
            ? braceIndex
            : privateMethodMatch.Index + privateMethodMatch.Length;
    }

    private static bool RangesOverlapOrTouch(
        int leftStart,
        int leftEnd,
        int rightStart,
        int rightEnd)
    {
        if (leftStart == leftEnd)
        {
            return leftStart >= rightStart && leftStart <= rightEnd;
        }

        return leftStart <= rightEnd && rightStart <= leftEnd;
    }

    private async ValueTask<AnalyzeJazorResponse> AnalyzeAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
    {
        var request = new AnalyzeJazorRequest(
            document,
            relatedDocuments: await _relatedDocumentResolver.ResolveAsync(
                document,
                explicitPaths: Array.Empty<string>(),
                cancellationToken),
            frontendContext: null);
        return await _analysisClient.AnalyzeWithFallbackAsync(
            _fallbackAnalysisService,
            request,
            cancellationToken);
    }

    private static string NormalizeDocumentPath(string documentPath)
        => JoltWorkspaceResolver.NormalizePath(documentPath);

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

    private static bool TryGetComponentTagNameAtPosition(
        string text,
        LspPosition position,
        out string componentName)
    {
        var offset = LspProtocolHelpers.GetOffset(text, position);
        foreach (Match match in TagPattern.Matches(text))
        {
            var nameGroup = match.Groups["name"];
            if (!nameGroup.Success)
            {
                continue;
            }

            if (offset < nameGroup.Index || offset > nameGroup.Index + nameGroup.Length)
            {
                continue;
            }

            componentName = nameGroup.Value;
            return true;
        }

        componentName = string.Empty;
        return false;
    }

    private static bool TryResolveImportLinkTargetPath(
        string documentPath,
        string importSource,
        out string targetPath)
    {
        foreach (var candidate in JoltWorkspaceResolver.GetImportPathCandidates(documentPath, importSource))
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            var fullPath = Path.IsPathRooted(candidate)
                ? Path.GetFullPath(candidate)
                : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(documentPath) ?? string.Empty, candidate));
            if (!File.Exists(fullPath))
            {
                continue;
            }

            targetPath = fullPath;
            return true;
        }

        targetPath = string.Empty;
        return false;
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
