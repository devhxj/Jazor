namespace Jolt.Lsp.Aggregation;

internal sealed class LspResultAggregator
{
    public IReadOnlyList<LspDiagnostic> AggregateDiagnostics(
        IReadOnlyList<LspDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        return diagnostics
            .GroupBy(static diagnostic => string.Join(
                '|',
                diagnostic.Range.Start.Line,
                diagnostic.Range.Start.Character,
                diagnostic.Range.End.Line,
                diagnostic.Range.End.Character,
                diagnostic.Code,
                diagnostic.Message,
                diagnostic.Source),
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    public IReadOnlyList<LspCompletionItem> AggregateCompletionItems(
        IReadOnlyList<LspCompletionItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items
            .GroupBy(static item => string.Join('|', item.Label, item.Kind, item.Detail), StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    public IReadOnlyList<LspLocation> AggregateLocations(
        IReadOnlyList<LspLocation> locations)
    {
        ArgumentNullException.ThrowIfNull(locations);

        return locations
            .GroupBy(static location => string.Join(
                '|',
                location.Uri,
                location.Range.Start.Line,
                location.Range.Start.Character,
                location.Range.End.Line,
                location.Range.End.Character),
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    public IReadOnlyList<LspDocumentHighlight> AggregateDocumentHighlights(
        IReadOnlyList<LspDocumentHighlight> highlights)
    {
        ArgumentNullException.ThrowIfNull(highlights);

        return highlights
            .GroupBy(static highlight => string.Join(
                '|',
                highlight.Range.Start.Line,
                highlight.Range.Start.Character,
                highlight.Range.End.Line,
                highlight.Range.End.Character,
                highlight.Kind),
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .OrderBy(static highlight => highlight.Range.Start.Line)
            .ThenBy(static highlight => highlight.Range.Start.Character)
            .ToArray();
    }

    public IReadOnlyList<LspDocumentLink> AggregateDocumentLinks(
        IReadOnlyList<LspDocumentLink> links)
    {
        ArgumentNullException.ThrowIfNull(links);

        return links
            .GroupBy(static link => string.Join(
                    '|',
                    link.Range.Start.Line,
                    link.Range.Start.Character,
                    link.Range.End.Line,
                    link.Range.End.Character,
                    link.Target,
                    link.Tooltip),
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .OrderBy(static link => link.Range.Start.Line)
            .ThenBy(static link => link.Range.Start.Character)
            .ToArray();
    }

    public IReadOnlyList<LspCodeAction> AggregateCodeActions(
        IReadOnlyList<LspCodeAction> actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        return actions
            .GroupBy(static action => string.Join('|', action.Title, action.Kind), StringComparer.Ordinal)
            .Select(static group => group.First())
            .ToArray();
    }

    public IReadOnlyList<LspDocumentSymbol> AggregateDocumentSymbols(
        IReadOnlyList<LspDocumentSymbol> symbols)
    {
        ArgumentNullException.ThrowIfNull(symbols);

        return symbols
            .GroupBy(static symbol => string.Join(
                '|',
                symbol.Name,
                symbol.Kind,
                symbol.Range.Start.Line,
                symbol.Range.Start.Character,
                symbol.Range.End.Line,
                symbol.Range.End.Character),
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .OrderBy(static symbol => symbol.Range.Start.Line)
            .ThenBy(static symbol => symbol.Range.Start.Character)
            .ToArray();
    }

    public LspSemanticTokensResult AggregateSemanticTokens(
        IReadOnlyList<LspSemanticToken> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        var normalizedTokens = tokens
            .Where(static token => token.Length > 0)
            .GroupBy(static token => string.Join(
                '|',
                token.Line,
                token.Character,
                token.Length,
                token.TokenType,
                string.Join(",", token.TokenModifiers)),
                StringComparer.Ordinal)
            .Select(static group => group.First())
            .OrderBy(static token => token.Line)
            .ThenBy(static token => token.Character)
            .ThenBy(static token => token.Length)
            .ThenBy(static token => token.TokenType, StringComparer.Ordinal)
            .ToArray();

        return LspSemanticTokenLegend.Encode(normalizedTokens);
    }

    public LspWorkspaceEdit? AggregateWorkspaceEdits(
        IReadOnlyList<LspWorkspaceEdit> edits)
    {
        ArgumentNullException.ThrowIfNull(edits);

        if (edits.Count == 0)
        {
            return null;
        }

        var mergedChanges = new Dictionary<string, List<LspTextEdit>>(StringComparer.Ordinal);
        foreach (var edit in edits)
        {
            foreach (var change in edit.Changes)
            {
                if (!mergedChanges.TryGetValue(change.Key, out var bucket))
                {
                    bucket = [];
                    mergedChanges.Add(change.Key, bucket);
                }

                bucket.AddRange(change.Value);
            }
        }

        return new LspWorkspaceEdit
        {
            Changes = mergedChanges.ToDictionary(
                static entry => entry.Key,
                static entry => entry.Value
                    .GroupBy(static edit => string.Join(
                        '|',
                        edit.Range.Start.Line,
                        edit.Range.Start.Character,
                        edit.Range.End.Line,
                        edit.Range.End.Character,
                        edit.NewText),
                        StringComparer.Ordinal)
                    .Select(static group => group.First())
                    .OrderByDescending(static edit => edit.Range.Start.Line)
                    .ThenByDescending(static edit => edit.Range.Start.Character)
                    .ToArray(),
                StringComparer.Ordinal)
        };
    }
}
