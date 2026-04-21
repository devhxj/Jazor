using System.Text.RegularExpressions;

namespace Jazor.Vue;

public sealed partial class JazorVueParser
{
    private static readonly Regex TemplatePattern = new Regex(
        @"<template>(?<content>[\s\S]*?)</template>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public JazorVueDocument Parse(string filePath, string sourceText)
    {
        if (filePath is null)
            throw new ArgumentNullException(nameof(filePath));
        if (sourceText is null)
            throw new ArgumentNullException(nameof(sourceText));

        var codeParseResult = ParseCode(sourceText);
        var templateParseResult = ParseTemplate(sourceText, codeParseResult);
        var imports = BuildImports(filePath, sourceText, templateParseResult.Template);

        return new JazorVueDocument(
            filePath,
            sourceText,
            imports,
            templateParseResult.Template,
            codeParseResult.Code,
            codeParseResult.StartIndex,
            templateParseResult.StartIndex,
            templateParseResult.Length,
            codeParseResult.Code.Length);
    }

    private static IReadOnlyList<JazorImportDirective> BuildImports(
        string filePath,
        string sourceText,
        string template)
    {
        var declaredImports = ParseImportDirectives(sourceText);
        var inferredVueImports = InferVueImports(filePath, template);
        if (inferredVueImports.Count == 0)
        {
            return declaredImports;
        }

        var inferredComponentNames = inferredVueImports
            .SelectMany(static import => import.Bindings)
            .Select(static binding => binding.LocalName)
            .ToHashSet(StringComparer.Ordinal);
        var compatibilityImports = declaredImports
            .Where(import => import.Kind != JazorImportKind.VueImport
                || import.Bindings.All(binding => !inferredComponentNames.Contains(binding.LocalName)));

        return compatibilityImports
            .Concat(inferredVueImports)
            .ToArray();
    }

    private static IReadOnlyList<JazorImportDirective> ParseImportDirectives(string sourceText)
    {
        var imports = new List<JazorImportDirective>();
        foreach (var match in JazorImportDirectiveLocator.EnumerateModuleDirectives(sourceText))
        {
            var kind = ResolveImportKind(match.Source);

            imports.Add(new JazorImportDirective(
                kind,
                match.Source,
                JazorImportDirectiveLocator.ParseBindings(match.Clause),
                match.RawText));
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
        foreach (var componentName in JazorMarkupPatterns.ComponentTagPattern.Matches(template)
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

    private static TemplateParseResult ParseTemplate(string sourceText, CodeParseResult codeParseResult)
    {
        var match = TemplatePattern.Match(sourceText);
        if (match.Success)
        {
            return CreateTrimmedTemplateResult(
                match.Groups["content"].Value,
                match.Groups["content"].Index);
        }

        var markupEnd = codeParseResult.DirectiveIndex >= 0
            ? codeParseResult.DirectiveIndex
            : sourceText.Length;
        var markup = sourceText[..markupEnd];
        var sanitizedMarkup = RemoveTopLevelImportDirectiveLines(markup);
        var trimmedMarkup = sanitizedMarkup.Trim();
        if (trimmedMarkup.Length == 0)
        {
            return new TemplateParseResult(string.Empty, -1, 0);
        }

        var sourceStartIndex = markup.IndexOf(trimmedMarkup, StringComparison.Ordinal);
        return sourceStartIndex < 0
            ? new TemplateParseResult(trimmedMarkup, -1, trimmedMarkup.Length)
            : new TemplateParseResult(trimmedMarkup, sourceStartIndex, trimmedMarkup.Length);
    }

    private static TemplateParseResult CreateTrimmedTemplateResult(string rawTemplate, int rawStartIndex)
    {
        var trimmedTemplate = rawTemplate.Trim();
        if (trimmedTemplate.Length == 0)
        {
            return new TemplateParseResult(string.Empty, -1, 0);
        }

        var relativeStartIndex = rawTemplate.IndexOf(trimmedTemplate, StringComparison.Ordinal);
        return relativeStartIndex < 0
            ? new TemplateParseResult(trimmedTemplate, -1, trimmedTemplate.Length)
            : new TemplateParseResult(trimmedTemplate, rawStartIndex + relativeStartIndex, trimmedTemplate.Length);
    }

    private static string RemoveTopLevelImportDirectiveLines(string text)
    {
        var directiveLines = JazorImportDirectiveLocator.EnumerateDirectiveLines(text)
            .Select(static match => (match.LineStartIndex, match.LineLength))
            .ToArray();
        if (directiveLines.Length == 0)
        {
            return text;
        }

        var builder = new System.Text.StringBuilder(text.Length);
        var cursor = 0;
        foreach (var directiveLine in directiveLines)
        {
            if (directiveLine.LineStartIndex > cursor)
            {
                builder.Append(text, cursor, directiveLine.LineStartIndex - cursor);
            }

            cursor = directiveLine.LineStartIndex + directiveLine.LineLength;
        }

        if (cursor < text.Length)
        {
            builder.Append(text, cursor, text.Length - cursor);
        }

        return builder.ToString();
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
        if (!JazorCodeDirectiveLocator.TryFindCodeDirectiveWithBlockBody(sourceText, out var codeDirective))
        {
            return new CodeParseResult(string.Empty, -1, -1);
        }

        if (!codeDirective.IsClosed)
        {
            throw new FormatException("The .jazor document contains an unterminated @code block.");
        }

        var codeStartIndex = codeDirective.OpeningBraceIndex + 1;
        var codeText = sourceText.Substring(codeStartIndex, codeDirective.ClosingBraceIndex - codeStartIndex);
        var trimmedCodeText = codeText.Trim();
        if (trimmedCodeText.Length == 0)
        {
            return new CodeParseResult(string.Empty, codeStartIndex, codeDirective.DirectiveIndex);
        }

        var relativeStart = codeText.IndexOf(trimmedCodeText, StringComparison.Ordinal);
        return new CodeParseResult(trimmedCodeText, codeStartIndex + relativeStart, codeDirective.DirectiveIndex);
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

    private sealed class TemplateParseResult
    {
        public TemplateParseResult(string template, int startIndex, int length)
        {
            Template = template;
            StartIndex = startIndex;
            Length = length;
        }

        public string Template { get; }

        public int StartIndex { get; }

        public int Length { get; }
    }

    private static JazorImportKind ResolveImportKind(string source)
    {
        return IsVueImportSource(source)
            ? JazorImportKind.VueImport
            : JazorImportKind.JSImport;
    }

    private static bool IsVueImportSource(string source)
        => source.EndsWith(".vue", StringComparison.OrdinalIgnoreCase);
}
