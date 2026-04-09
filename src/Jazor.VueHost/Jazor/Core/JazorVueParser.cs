using System.Text.RegularExpressions;

namespace Jazor.Vue;

public sealed partial class JazorVueParser
{
    private static readonly Regex LegacyImportDirectivePattern = new Regex(
        @"^\s*@(?<kind>jsimport|vueimport)\s+(?<clause>.+?)\s+from\s+[""'](?<source>[^""']+)[""']\s*$",
        RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex ComponentTagPattern = new Regex(
        @"<(?<name>[A-Z][A-Za-z0-9_]*)\b",
        RegexOptions.Compiled);
    private static readonly Regex TemplatePattern = new Regex(
        @"<template>(?<content>[\s\S]*?)</template>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CodeStartPattern = new Regex(
        @"@code\s*\{",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public JazorVueDocument Parse(string filePath, string sourceText)
    {
        if (filePath is null)
            throw new ArgumentNullException(nameof(filePath));
        if (sourceText is null)
            throw new ArgumentNullException(nameof(sourceText));

        var codeParseResult = ParseCode(sourceText);
        var template = ParseTemplate(sourceText, codeParseResult);
        var imports = BuildImports(filePath, sourceText, template);

        return new JazorVueDocument(filePath, sourceText, imports, template, codeParseResult.Code, codeParseResult.StartIndex);
    }

    private static IReadOnlyList<JazorImportDirective> BuildImports(
        string filePath,
        string sourceText,
        string template)
    {
        var legacyImports = ParseLegacyImports(sourceText);
        var inferredVueImports = InferVueImports(filePath, template);
        if (inferredVueImports.Count == 0)
        {
            return legacyImports;
        }

        var inferredComponentNames = inferredVueImports
            .SelectMany(static import => import.Bindings)
            .Select(static binding => binding.LocalName)
            .ToHashSet(StringComparer.Ordinal);
        var compatibilityImports = legacyImports
            .Where(import => import.Kind != JazorImportKind.VueImport
                || import.Bindings.All(binding => !inferredComponentNames.Contains(binding.LocalName)));

        return compatibilityImports
            .Concat(inferredVueImports)
            .ToArray();
    }

    private static IReadOnlyList<JazorImportDirective> ParseLegacyImports(string sourceText)
    {
        var imports = new List<JazorImportDirective>();
        foreach (Match match in LegacyImportDirectivePattern.Matches(sourceText))
        {
            var kind = string.Equals(match.Groups["kind"].Value, "vueimport", StringComparison.Ordinal)
                ? JazorImportKind.VueImport
                : JazorImportKind.JSImport;
            var clause = match.Groups["clause"].Value.Trim();
            var source = match.Groups["source"].Value.Trim();

            imports.Add(new JazorImportDirective(kind, source, ParseBindings(clause), match.Value));
        }

        return imports;
    }

    private static IReadOnlyList<JazorImportDirective> InferVueImports(
        string filePath,
        string template)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return Array.Empty<JazorImportDirective>();
        }

        var imports = new List<JazorImportDirective>();
        foreach (var componentName in ComponentTagPattern.Matches(template)
                     .Select(static match => match.Groups["name"].Value)
                     .Distinct(StringComparer.Ordinal))
        {
            var importPath = ResolveVueComponentImportPath(filePath, componentName);

            imports.Add(new JazorImportDirective(
                JazorImportKind.VueImport,
                importPath,
                [new JazorImportBinding(componentName, null, JazorImportBindingKind.Default)],
                $"/* inferred vue import {componentName} from \"{importPath}\" */"));
        }

        return imports;
    }

    private static IReadOnlyList<JazorImportBinding> ParseBindings(string clause)
    {
        var clauseParts = SplitTopLevelCommaSeparatedClause(clause);
        if (clauseParts.Count > 1)
        {
            var bindings = new List<JazorImportBinding>();
            var defaultBinding = clauseParts[0].Trim();
            if (!string.IsNullOrWhiteSpace(defaultBinding))
                bindings.Add(new JazorImportBinding(defaultBinding, null, JazorImportBindingKind.Default));

            var trailingClause = clauseParts[1].Trim();
            bindings.AddRange(ParseBindings(trailingClause));
            return bindings;
        }

        if (clause.StartsWith("{", StringComparison.Ordinal) && clause.EndsWith("}", StringComparison.Ordinal))
        {
            var content = clause.Substring(1, clause.Length - 2);
            var bindings = new List<JazorImportBinding>();
            foreach (var rawPart in content.Split(','))
            {
                var part = rawPart.Trim();
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                var aliasParts = part.Split(new[] { " as " }, StringSplitOptions.None);
                if (aliasParts.Length == 2)
                    bindings.Add(new JazorImportBinding(aliasParts[1].Trim(), aliasParts[0].Trim(), JazorImportBindingKind.Named));
                else
                    bindings.Add(new JazorImportBinding(part, part, JazorImportBindingKind.Named));
            }

            return bindings;
        }

        if (clause.StartsWith("* as ", StringComparison.Ordinal))
        {
            var name = clause.Substring("* as ".Length).Trim();
            return new[] { new JazorImportBinding(name, null, JazorImportBindingKind.Namespace) };
        }

        return new[] { new JazorImportBinding(clause.Trim(), null, JazorImportBindingKind.Default) };
    }

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
                        braceDepth--;
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
            parts.Add(builder.ToString());

        return parts;
    }

    private static string ParseTemplate(string sourceText, CodeParseResult codeParseResult)
    {
        var match = TemplatePattern.Match(sourceText);
        if (match.Success)
            return match.Groups["content"].Value.Trim();

        var markupEnd = codeParseResult.DirectiveIndex >= 0
            ? codeParseResult.DirectiveIndex
            : sourceText.Length;
        var markup = sourceText[..markupEnd];
        markup = LegacyImportDirectivePattern.Replace(markup, string.Empty);
        return markup.Trim();
    }

    private static string ResolveVueComponentImportPath(
        string filePath,
        string componentName)
    {
        var documentDirectory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(documentDirectory))
        {
            return "./" + componentName + ".vue";
        }

        foreach (var directory in GetSearchDirectories(documentDirectory))
        {
            var candidate = Path.Combine(directory, componentName + ".vue");
            if (!File.Exists(candidate))
            {
                continue;
            }

            return ToImportPath(documentDirectory, candidate);
        }

        return "./" + componentName + ".vue";
    }

    private static IEnumerable<string> GetSearchDirectories(string documentDirectory)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parentDirectory = GetParentDirectoryPath(documentDirectory);
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

    private static string? GetParentDirectoryPath(string documentDirectory)
    {
        if (Path.IsPathRooted(documentDirectory))
        {
            return Directory.GetParent(documentDirectory)?.FullName;
        }

        var normalized = documentDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (normalized.Length == 0)
        {
            return null;
        }

        return Path.GetDirectoryName(normalized);
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

    private static CodeParseResult ParseCode(string sourceText)
    {
        var codeStart = CodeStartPattern.Match(sourceText);
        if (!codeStart.Success)
            return new CodeParseResult(string.Empty, -1, -1);

        var startIndex = codeStart.Index + codeStart.Length;
        var depth = 1;
        for (var i = startIndex; i < sourceText.Length; i++)
        {
            switch (sourceText[i])
            {
                case '{':
                    depth++;
                    break;
                case '}':
                    depth--;
                    if (depth == 0)
                    {
                        var codeText = sourceText.Substring(startIndex, i - startIndex);
                        var trimmedCodeText = codeText.Trim();
                        if (trimmedCodeText.Length == 0)
                            return new CodeParseResult(string.Empty, startIndex, codeStart.Index);

                        var relativeStart = codeText.IndexOf(trimmedCodeText, StringComparison.Ordinal);
                        return new CodeParseResult(trimmedCodeText, startIndex + relativeStart, codeStart.Index);
                    }
                    break;
            }
        }

        throw new FormatException("The .jazor document contains an unterminated @code block.");
    }

    private sealed class CodeParseResult
    {
        public CodeParseResult(string code, int startIndex, int directiveIndex)
        {
            Code = code;
            StartIndex = startIndex;
            DirectiveIndex = directiveIndex;
        }

        public string Code { get; }

        public int StartIndex { get; }

        public int DirectiveIndex { get; }
    }
}
