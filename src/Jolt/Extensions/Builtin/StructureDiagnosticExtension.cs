using System.Text.RegularExpressions;
using Jazor.VueContracts.Protocol;
using Jolt.Lsp;

namespace Jolt.Extensions.Builtin;

internal sealed class StructureDiagnosticExtension : IExtension, ILspDiagnosticProvider
{
    private const string Source = "Jolt.Extension.Builtin";
    private const string MultipleTemplateDiagnosticCode = "JAZORVUEEXTSTR001";
    private const string TemplateMismatchDiagnosticCode = "JAZORVUEEXTSTR002";
    private const string MissingCodeBlockStartDiagnosticCode = "JAZORVUEEXTSTR003";
    private const string UnbalancedCodeBlockDiagnosticCode = "JAZORVUEEXTSTR004";
    private const string MissingTemplateWrapperDiagnosticCode = "JAZORVUEEXTSTR005";

    private static readonly Regex TemplateOpenPattern = new(
        @"<template\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex TemplateClosePattern = new(
        @"</template\s*>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex MarkupTagPattern = new(
        @"<\s*[A-Za-z][A-Za-z0-9:_-]*\b",
        RegexOptions.Compiled);

    private static readonly Regex CodeDirectivePattern = new(
        @"@code\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public ExtensionMetadata Metadata { get; } = new(
        Id: "builtin.structure-diagnostic",
        Name: "Builtin Structure Diagnostic",
        Version: "1.0.0",
        Description: "Validates Jazor template/code structural contracts.");

    public string Name => "BuiltinStructureDiagnosticProvider";

    public int Priority => 200;

    public ValueTask InitializeAsync(ExtensionContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask ActivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask DeactivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask<IReadOnlyList<LspDiagnostic>> ProvideDiagnosticsAsync(
        LspDiagnosticProviderContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var document = context.Document;
        if (document.DocumentKind != DocumentKind.Jazor)
        {
            return ValueTask.FromResult<IReadOnlyList<LspDiagnostic>>(Array.Empty<LspDiagnostic>());
        }

        var existingCodes = context.ExistingDiagnostics
            .Select(static diagnostic => diagnostic.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var diagnostics = new List<LspDiagnostic>();

        AddTemplateDiagnostics(document.Text, existingCodes, diagnostics);
        AddCodeDirectiveDiagnostics(document.Text, existingCodes, diagnostics);
        AddTemplateWrapperDiagnostic(document.Text, existingCodes, diagnostics);

        return ValueTask.FromResult<IReadOnlyList<LspDiagnostic>>(diagnostics);
    }

    private static void AddTemplateDiagnostics(
        string text,
        IReadOnlySet<string> existingCodes,
        List<LspDiagnostic> diagnostics)
    {
        var templateOpenMatches = TemplateOpenPattern.Matches(text).Cast<Match>().ToArray();
        var templateCloseMatches = TemplateClosePattern.Matches(text).Cast<Match>().ToArray();

        if (templateOpenMatches.Length > 1)
        {
            var secondaryTemplate = templateOpenMatches[1];
            TryAddDiagnostic(
                diagnostics,
                existingCodes,
                MultipleTemplateDiagnosticCode,
                severity: 2,
                message: "Only one <template> block is supported.",
                range: LspProtocolHelpers.ToRange(text, secondaryTemplate.Index, secondaryTemplate.Length));
        }

        if (templateOpenMatches.Length > templateCloseMatches.Length)
        {
            var unmatchedTemplate = templateOpenMatches[^1];
            TryAddDiagnostic(
                diagnostics,
                existingCodes,
                TemplateMismatchDiagnosticCode,
                severity: 1,
                message: "Missing </template> closing tag.",
                range: LspProtocolHelpers.ToRange(text, unmatchedTemplate.Index, unmatchedTemplate.Length));
        }
        else if (templateCloseMatches.Length > templateOpenMatches.Length)
        {
            var unmatchedClose = templateCloseMatches[templateOpenMatches.Length];
            TryAddDiagnostic(
                diagnostics,
                existingCodes,
                TemplateMismatchDiagnosticCode,
                severity: 1,
                message: "Found </template> without a matching <template> opening tag.",
                range: LspProtocolHelpers.ToRange(text, unmatchedClose.Index, unmatchedClose.Length));
        }
    }

    private static void AddCodeDirectiveDiagnostics(
        string text,
        IReadOnlySet<string> existingCodes,
        List<LspDiagnostic> diagnostics)
    {
        var codeDirective = CodeDirectivePattern.Match(text);
        if (!codeDirective.Success)
        {
            return;
        }

        var codeStartIndex = codeDirective.Index;
        var openingBraceIndex = text.IndexOf('{', codeDirective.Index + codeDirective.Length);
        if (openingBraceIndex < 0)
        {
            TryAddDiagnostic(
                diagnostics,
                existingCodes,
                MissingCodeBlockStartDiagnosticCode,
                severity: 1,
                message: "@code directive requires a '{' block body.",
                range: LspProtocolHelpers.ToRange(text, codeStartIndex, "@code".Length));
            return;
        }

        var depth = 0;
        var closed = false;
        for (var index = openingBraceIndex; index < text.Length; index++)
        {
            switch (text[index])
            {
                case '{':
                    depth++;
                    break;
                case '}':
                    depth--;
                    if (depth == 0)
                    {
                        closed = true;
                        break;
                    }

                    break;
            }

            if (closed)
            {
                break;
            }
        }

        if (!closed)
        {
            TryAddDiagnostic(
                diagnostics,
                existingCodes,
                UnbalancedCodeBlockDiagnosticCode,
                severity: 1,
                message: "@code block has unbalanced braces.",
                range: LspProtocolHelpers.ToRange(text, openingBraceIndex, length: 1));
        }
    }

    private static void AddTemplateWrapperDiagnostic(
        string text,
        IReadOnlySet<string> existingCodes,
        List<LspDiagnostic> diagnostics)
    {
        if (TemplateOpenPattern.IsMatch(text))
        {
            return;
        }

        var codeDirectiveMatch = CodeDirectivePattern.Match(text);
        if (!codeDirectiveMatch.Success)
        {
            return;
        }

        var markupSegment = codeDirectiveMatch.Success
            ? text[..codeDirectiveMatch.Index]
            : text;
        if (string.IsNullOrWhiteSpace(markupSegment))
        {
            return;
        }

        var markupMatch = MarkupTagPattern.Match(markupSegment);
        if (!markupMatch.Success)
        {
            return;
        }

        TryAddDiagnostic(
            diagnostics,
            existingCodes,
            MissingTemplateWrapperDiagnosticCode,
            severity: 2,
            message: "Markup should be wrapped in <template>...</template> for stable projection.",
            range: LspProtocolHelpers.ToRange(text, markupMatch.Index, markupMatch.Length));
    }

    private static void TryAddDiagnostic(
        List<LspDiagnostic> diagnostics,
        IReadOnlySet<string> existingCodes,
        string code,
        int severity,
        string message,
        LspRange range)
    {
        if (existingCodes.Contains(code)
            || diagnostics.Any(diagnostic => string.Equals(diagnostic.Code, code, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        diagnostics.Add(new LspDiagnostic
        {
            Range = range,
            Severity = severity,
            Code = code,
            Source = Source,
            Message = message
        });
    }
}
