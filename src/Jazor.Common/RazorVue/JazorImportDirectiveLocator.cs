using System.Text.RegularExpressions;

namespace ECMAScript.Contract.RazorVue;

public static class JazorImportDirectiveLocator
{
    private const string ModuleDirective = "@module";
    private static readonly string[] CodeBlockDirectives = ["@code", "@functions"];

    private static readonly Regex ModuleDirectivePattern = new(
        @"^@module\s+(?<clause>.+?)\s+from\s+(?<quote>[""'])(?<source>[^""']+)\k<quote>\s*$",
        RegexOptions.Compiled);

    private static readonly Regex LegacyDirectivePattern = new(
        @"^@(?<kind>import|jsimport|vueimport)\b.*$",
        RegexOptions.Compiled);

    private static readonly Regex NamespaceBindingPattern = new(
        @"^\*\s+as\s+(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\s*$",
        RegexOptions.Compiled);

    private static readonly Regex NamedBindingPattern = new(
        @"^(?<imported>[A-Za-z_$][A-Za-z0-9_$]*)(?:\s+as\s+(?<local>[A-Za-z_$][A-Za-z0-9_$]*))?\s*$",
        RegexOptions.Compiled);

    public static IEnumerable<JazorImportDirectiveMatch> EnumerateModuleDirectives(string text)
    {
        foreach (var match in EnumerateDirectiveLines(text))
        {
            if (match.Kind == JazorImportDirectiveKind.Module)
            {
                yield return match;
            }
        }
    }

    public static IEnumerable<JazorImportDirectiveMatch> EnumerateLegacyDirectives(string text)
    {
        foreach (var match in EnumerateDirectiveLines(text))
        {
            if (match.IsLegacy)
            {
                yield return match;
            }
        }
    }

    public static IEnumerable<JazorImportDirectiveMatch> EnumerateDirectiveLines(string text)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        var templateRange = FindTagBlock(text, "<template", "</template>");
        var codeRanges = RazorBlockDirectiveLocator.EnumerateDirectiveBlocks(text, CodeBlockDirectives)
            .Where(static match => match.HasBlockBody)
            .Select(match => (Start: match.DirectiveIndex, End: match.IsClosed ? match.ClosingBraceIndex + 1 : text.Length))
            .ToArray();

        var codeRangeIndex = 0;
        var inBlockComment = false;
        var inRazorComment = false;
        var lineStart = 0;

        while (lineStart < text.Length)
        {
            var lineEnd = lineStart;
            while (lineEnd < text.Length
                   && text[lineEnd] != '\r'
                   && text[lineEnd] != '\n')
            {
                lineEnd++;
            }

            while (codeRangeIndex < codeRanges.Length && codeRanges[codeRangeIndex].End <= lineStart)
            {
                codeRangeIndex++;
            }

            if (InRange(templateRange, lineStart)
                || (codeRangeIndex < codeRanges.Length && InRange(codeRanges[codeRangeIndex], lineStart)))
            {
                lineStart = GetNextLineStart(text, lineEnd);
                continue;
            }

            var contentStart = GetLineContentStart(text, lineStart, lineEnd);
            if (contentStart >= lineEnd)
            {
                lineStart = GetNextLineStart(text, lineEnd);
                continue;
            }

            var lineContent = text.AsSpan(contentStart, lineEnd - contentStart);
            if (inBlockComment)
            {
                if (ContainsTerminatingCommentOnly(lineContent, "*/", out _))
                {
                    inBlockComment = false;
                    lineStart = GetNextLineStart(text, lineEnd);
                    continue;
                }

                inBlockComment = true;
                lineStart = GetNextLineStart(text, lineEnd);
                continue;
            }

            if (inRazorComment)
            {
                if (ContainsTerminatingCommentOnly(lineContent, "*@", out _))
                {
                    inRazorComment = false;
                    lineStart = GetNextLineStart(text, lineEnd);
                    continue;
                }

                inRazorComment = true;
                lineStart = GetNextLineStart(text, lineEnd);
                continue;
            }

            if (StartsWith(lineContent, "//"))
            {
                lineStart = GetNextLineStart(text, lineEnd);
                continue;
            }

            if (StartsWith(lineContent, "/*"))
            {
                inBlockComment = !ContainsTerminatingCommentOnly(lineContent, "*/", out _);
                lineStart = GetNextLineStart(text, lineEnd);
                continue;
            }

            if (StartsWith(lineContent, "@*"))
            {
                inRazorComment = !ContainsTerminatingCommentOnly(lineContent, "*@", out _);
                lineStart = GetNextLineStart(text, lineEnd);
                continue;
            }

            var lineText = text.Substring(lineStart, lineEnd - lineStart);
            var trimmedText = text.Substring(contentStart, lineEnd - contentStart);

            var moduleMatch = ModuleDirectivePattern.Match(trimmedText);
            if (moduleMatch.Success)
            {
                var clauseGroup = moduleMatch.Groups["clause"];
                var sourceGroup = moduleMatch.Groups["source"];
                yield return new JazorImportDirectiveMatch(
                    JazorImportDirectiveKind.Module,
                    lineStart,
                    lineText.Length,
                    contentStart,
                    ModuleDirective.Length,
                    sourceGroup.Index >= 0 ? contentStart + sourceGroup.Index : -1,
                    sourceGroup.Length,
                    clauseGroup.Value.Trim(),
                    sourceGroup.Value.Trim(),
                    lineText);
                lineStart = GetNextLineStart(text, lineEnd);
                continue;
            }

            var legacyMatch = LegacyDirectivePattern.Match(trimmedText);
            if (legacyMatch.Success)
            {
                var kindGroup = legacyMatch.Groups["kind"];
                yield return new JazorImportDirectiveMatch(
                    ParseLegacyKind(kindGroup.Value),
                    lineStart,
                    lineText.Length,
                    contentStart,
                    kindGroup.Length + 1,
                    sourceIndex: -1,
                    sourceLength: 0,
                    clause: string.Empty,
                    source: string.Empty,
                    lineText);
            }

            lineStart = GetNextLineStart(text, lineEnd);
        }
    }

    public static IReadOnlyList<JazorImportBinding> ParseBindings(string clause)
    {
        if (clause is null)
        {
            throw new ArgumentNullException(nameof(clause));
        }

        var trimmedClause = clause.Trim();
        if (trimmedClause.Length == 0)
        {
            return Array.Empty<JazorImportBinding>();
        }

        var clauseParts = SplitTopLevelCommaSeparatedClause(trimmedClause);
        if (clauseParts.Count > 1)
        {
            var bindings = new List<JazorImportBinding>();
            var defaultBinding = clauseParts[0].Trim();
            if (!string.IsNullOrWhiteSpace(defaultBinding))
            {
                bindings.Add(new JazorImportBinding(defaultBinding, null, JazorImportBindingKind.Default));
            }

            var trailingClause = string.Join(",", clauseParts.Skip(1));
            bindings.AddRange(ParseBindings(trailingClause));
            return bindings;
        }

        if (trimmedClause.StartsWith("{", StringComparison.Ordinal)
            && trimmedClause.EndsWith("}", StringComparison.Ordinal))
        {
            var content = trimmedClause.Substring(1, trimmedClause.Length - 2);
            var bindings = new List<JazorImportBinding>();
            foreach (var rawPart in SplitTopLevelCommaSeparatedClause(content))
            {
                var part = rawPart.Trim();
                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }

                var bindingMatch = NamedBindingPattern.Match(part);
                if (bindingMatch.Success)
                {
                    var importedGroup = bindingMatch.Groups["imported"];
                    var localGroup = bindingMatch.Groups["local"];
                    var localName = localGroup.Success ? localGroup.Value : importedGroup.Value;
                    var importedName = importedGroup.Value;
                    if (!string.IsNullOrWhiteSpace(localName))
                    {
                        bindings.Add(new JazorImportBinding(localName, importedName, JazorImportBindingKind.Named));
                        continue;
                    }
                }

                bindings.Add(new JazorImportBinding(part, part, JazorImportBindingKind.Named));
            }

            return bindings;
        }

        var namespaceMatch = NamespaceBindingPattern.Match(trimmedClause);
        if (namespaceMatch.Success
            && namespaceMatch.Groups["name"] is { Success: true } namespaceGroup)
        {
            return [new JazorImportBinding(namespaceGroup.Value, null, JazorImportBindingKind.Namespace)];
        }

        return [new JazorImportBinding(trimmedClause, null, JazorImportBindingKind.Default)];
    }

    public static bool HasLocalBinding(string clause, string localName)
    {
        if (localName is null)
        {
            throw new ArgumentNullException(nameof(localName));
        }

        return ParseBindings(clause)
            .Any(binding => string.Equals(binding.LocalName, localName, StringComparison.Ordinal));
    }

    private static JazorImportDirectiveKind ParseLegacyKind(string kind)
        => kind switch
        {
            "import" => JazorImportDirectiveKind.Import,
            "jsimport" => JazorImportDirectiveKind.JsImport,
            "vueimport" => JazorImportDirectiveKind.VueImport,
            _ => throw new InvalidOperationException($"Unsupported import directive kind '{kind}'.")
        };

    private static bool ContainsTerminatingCommentOnly(
        ReadOnlySpan<char> lineContent,
        string terminator,
        out bool endsComment)
    {
        var index = lineContent.IndexOf(terminator, StringComparison.Ordinal);
        if (index < 0)
        {
            endsComment = false;
            return false;
        }

        var trailing = lineContent.Slice(index + terminator.Length);
        endsComment = trailing.Trim().IsEmpty;
        return true;
    }

    private static bool StartsWith(ReadOnlySpan<char> span, string value)
        => span.StartsWith(value, StringComparison.Ordinal);

    private static IReadOnlyList<string> SplitTopLevelCommaSeparatedClause(string clause)
    {
        var parts = new List<string>();
        var builder = new System.Text.StringBuilder(clause.Length);
        var braceDepth = 0;
        for (var index = 0; index < clause.Length; index++)
        {
            var character = clause[index];
            switch (character)
            {
                case '{':
                    braceDepth++;
                    builder.Append(character);
                    break;
                case '}':
                    if (braceDepth > 0)
                    {
                        braceDepth--;
                    }

                    builder.Append(character);
                    break;
                case ',' when braceDepth == 0:
                    parts.Add(builder.ToString());
                    builder.Clear();
                    break;
                default:
                    builder.Append(character);
                    break;
            }
        }

        if (builder.Length > 0)
        {
            parts.Add(builder.ToString());
        }

        return parts;
    }

    private static int GetLineContentStart(string text, int lineStart, int lineEnd)
    {
        var index = lineStart;
        while (index < lineEnd && char.IsWhiteSpace(text[index]))
        {
            index++;
        }

        return index;
    }

    private static int GetNextLineStart(string text, int lineEnd)
    {
        if (lineEnd >= text.Length)
        {
            return text.Length;
        }

        if (text[lineEnd] == '\r'
            && lineEnd + 1 < text.Length
            && text[lineEnd + 1] == '\n')
        {
            return lineEnd + 2;
        }

        return lineEnd + 1;
    }

    private static (int Start, int End) FindTagBlock(string text, string openTag, string closeTag)
    {
        var openIndex = text.IndexOf(openTag, StringComparison.OrdinalIgnoreCase);
        if (openIndex < 0)
        {
            return (-1, -1);
        }

        var closeIndex = text.IndexOf(closeTag, openIndex, StringComparison.OrdinalIgnoreCase);
        if (closeIndex < 0)
        {
            return (openIndex, text.Length);
        }

        return (openIndex, closeIndex + closeTag.Length);
    }

    private static bool InRange((int Start, int End) range, int offset)
        => range.Start >= 0
            && range.End > range.Start
            && offset >= range.Start
            && offset < range.End;
}

public enum JazorImportDirectiveKind
{
    Module,
    Import,
    JsImport,
    VueImport
}

public readonly struct JazorImportDirectiveMatch
{
    public JazorImportDirectiveMatch(
        JazorImportDirectiveKind kind,
        int lineStartIndex,
        int lineLength,
        int directiveIndex,
        int directiveLength,
        int sourceIndex,
        int sourceLength,
        string clause,
        string source,
        string rawText)
    {
        Kind = kind;
        LineStartIndex = lineStartIndex;
        LineLength = lineLength;
        DirectiveIndex = directiveIndex;
        DirectiveLength = directiveLength;
        SourceIndex = sourceIndex;
        SourceLength = sourceLength;
        Clause = clause;
        Source = source;
        RawText = rawText;
    }

    public JazorImportDirectiveKind Kind { get; }

    public int LineStartIndex { get; }

    public int LineLength { get; }

    public int DirectiveIndex { get; }

    public int DirectiveLength { get; }

    public int SourceIndex { get; }

    public int SourceLength { get; }

    public string Clause { get; }

    public string Source { get; }

    public string RawText { get; }

    public bool IsLegacy => Kind != JazorImportDirectiveKind.Module;

    public string LegacyKind => Kind switch
    {
        JazorImportDirectiveKind.Import => "import",
        JazorImportDirectiveKind.JsImport => "jsimport",
        JazorImportDirectiveKind.VueImport => "vueimport",
        _ => string.Empty
    };
}
