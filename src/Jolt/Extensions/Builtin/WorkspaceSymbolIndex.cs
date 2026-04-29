using System.Text;
using System.Text.RegularExpressions;
using Jolt.Lsp;
using ECMAScript.Contract.VueContracts.Protocol;

namespace Jolt.Extensions.Builtin;

internal sealed class WorkspaceSymbolIndex
{
    private static readonly Regex CSharpTypePattern = new(
        @"\b(class|record|interface|struct)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Compiled);

    private static readonly Regex CSharpMethodPattern = new(
        @"\b(?:public|private|protected|internal|static|virtual|override|async|sealed|partial|\s)+[\w<>\[\]\.\?,]+\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\(",
        RegexOptions.Compiled);

    private static readonly Regex TagComponentPattern = new(
        @"<(?<name>[A-Z][A-Za-z0-9_]*)\b",
        RegexOptions.Compiled);

    private static readonly Regex JavaScriptExportPattern = new(
        @"\bexport\s+(?:default\s+)?(?:async\s+)?(?:function|class|const|let|var)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Compiled);

    private readonly Lock _gate = new();
    private readonly Dictionary<string, IndexedDocumentSymbols> _symbolsByDocumentPath = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<LspWorkspaceSymbol> Search(
        string query,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        int maxResults = 256)
    {
        ArgumentNullException.ThrowIfNull(openDocuments);
        if (maxResults <= 0)
        {
            maxResults = 256;
        }

        Refresh(openDocuments);
        var normalizedQuery = query?.Trim() ?? string.Empty;
        List<LspWorkspaceSymbol> symbols;
        lock (_gate)
        {
            symbols = _symbolsByDocumentPath.Values
                .SelectMany(static entry => entry.Symbols)
                .ToList();
        }

        var filtered = string.IsNullOrWhiteSpace(normalizedQuery)
            ? symbols
            : symbols
                .Where(symbol =>
                    symbol.Name.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    || (symbol.ContainerName?.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

        return filtered
            .OrderBy(static symbol => symbol.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static symbol => symbol.Location.Uri, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static symbol => symbol.Location.Range.Start.Line)
            .Take(maxResults)
            .ToArray();
    }

    private void Refresh(IReadOnlyList<DocumentSnapshot> openDocuments)
    {
        var normalizedOpenPaths = openDocuments
            .Select(static document => Path.GetFullPath(document.DocumentPath))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        lock (_gate)
        {
            foreach (var stalePath in _symbolsByDocumentPath.Keys
                         .Where(path => !normalizedOpenPaths.Contains(path))
                         .ToArray())
            {
                _symbolsByDocumentPath.Remove(stalePath);
            }

            foreach (var openDocument in openDocuments)
            {
                var fullPath = Path.GetFullPath(openDocument.DocumentPath);
                var fingerprint = CreateFingerprint(openDocument);
                if (_symbolsByDocumentPath.TryGetValue(fullPath, out var existing)
                    && string.Equals(existing.Fingerprint, fingerprint, StringComparison.Ordinal))
                {
                    continue;
                }

                _symbolsByDocumentPath[fullPath] = new IndexedDocumentSymbols(
                    fullPath,
                    fingerprint,
                    ExtractSymbols(openDocument));
            }
        }
    }

    private static string CreateFingerprint(DocumentSnapshot document)
    {
        var version = document.Version?.Trim();
        if (!string.IsNullOrWhiteSpace(version))
        {
            return "version:" + version;
        }

        var textBytes = Encoding.UTF8.GetBytes(document.Text);
        var textHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(textBytes));
        return "text:" + textHash;
    }

    private static IReadOnlyList<LspWorkspaceSymbol> ExtractSymbols(DocumentSnapshot document)
    {
        var symbols = new List<LspWorkspaceSymbol>();
        var uri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath);
        var containerName = Path.GetFileNameWithoutExtension(document.DocumentPath);

        switch (document.DocumentKind)
        {
            case DocumentKind.CSharp:
                AddSymbolsFromPattern(document.Text, uri, containerName, CSharpTypePattern, kind: 5, symbols);
                AddSymbolsFromPattern(document.Text, uri, containerName, CSharpMethodPattern, kind: 6, symbols);
                break;

            case DocumentKind.Jazor:
            case DocumentKind.Vue:
                AddSymbolsFromPattern(document.Text, uri, containerName, TagComponentPattern, kind: 5, symbols);
                AddSymbolsFromPattern(document.Text, uri, containerName, CSharpMethodPattern, kind: 6, symbols);
                break;

            case DocumentKind.JavaScript:
            case DocumentKind.TypeScript:
                AddSymbolsFromPattern(document.Text, uri, containerName, JavaScriptExportPattern, kind: 12, symbols);
                break;
        }

        return symbols
            .GroupBy(static symbol => string.Join(
                '|',
                symbol.Name,
                symbol.Kind,
                symbol.Location.Uri,
                symbol.Location.Range.Start.Line,
                symbol.Location.Range.Start.Character),
                StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .ToArray();
    }

    private static void AddSymbolsFromPattern(
        string text,
        string uri,
        string? containerName,
        Regex pattern,
        int kind,
        List<LspWorkspaceSymbol> symbols)
    {
        foreach (Match match in pattern.Matches(text))
        {
            var nameGroup = match.Groups["name"];
            if (!nameGroup.Success || string.IsNullOrWhiteSpace(nameGroup.Value))
            {
                continue;
            }

            var range = LspProtocolHelpers.ToRange(text, nameGroup.Index, nameGroup.Length);
            symbols.Add(new LspWorkspaceSymbol
            {
                Name = nameGroup.Value,
                Kind = kind,
                ContainerName = containerName,
                Location = new LspLocation
                {
                    Uri = uri,
                    Range = range
                }
            });
        }
    }
}

internal sealed record IndexedDocumentSymbols(
    string DocumentPath,
    string Fingerprint,
    IReadOnlyList<LspWorkspaceSymbol> Symbols);
