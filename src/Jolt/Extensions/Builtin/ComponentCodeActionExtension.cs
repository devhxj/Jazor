using System.Text.RegularExpressions;
using Jazor.Vue;
using Jolt.Lsp;
using ECMAScript.Contract.VueContracts.Protocol;
using Jolt.Workspace;

namespace Jolt.Extensions.Builtin;

internal sealed class ComponentCodeActionExtension : IExtension, ILspCodeActionProvider
{
    private const string MissingComponentDiagnosticCode = "JAZORVUEFRONTEND001";

    private static readonly Regex QuotedComponentNamePattern = new(
        @"['""](?<name>[A-Z][A-Za-z0-9_]*)['""]",
        RegexOptions.Compiled);

    public ExtensionMetadata Metadata { get; } = new(
        Id: "builtin.component-code-action",
        Name: "Builtin Component Code Action",
        Version: "1.0.0",
        Description: "Provides component import quick fixes for unresolved template components.");

    public string Name => "BuiltinComponentCodeActionProvider";

    public int Priority => 200;

    public ValueTask InitializeAsync(ExtensionContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask ActivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask DeactivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask<IReadOnlyList<LspCodeAction>> ProvideCodeActionsAsync(
        LspCodeActionProviderContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context.Document.DocumentKind != DocumentKind.Jazor)
        {
            return ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(Array.Empty<LspCodeAction>());
        }

        var actions = new List<LspCodeAction>();
        var seenComponentNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var diagnostic in context.Diagnostics.Where(IsMissingComponentDiagnostic))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!TryResolveComponentName(context.Document.Text, diagnostic, out var componentName))
            {
                continue;
            }

            if (!seenComponentNames.Add(componentName))
            {
                continue;
            }

            if (HasImportDirective(context.Document.Text, componentName))
            {
                continue;
            }

            if (!TryResolveImportPath(context.Document.DocumentPath, componentName, out var importPath))
            {
                continue;
            }

            actions.Add(CreateImportAction(context.Document, componentName, importPath));
        }

        return ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(actions);
    }

    private static bool IsMissingComponentDiagnostic(LspDiagnostic diagnostic)
    {
        if (string.Equals(diagnostic.Code, MissingComponentDiagnosticCode, StringComparison.Ordinal))
        {
            return true;
        }

        if (!string.Equals(diagnostic.Source, "Jolt.Volar", StringComparison.Ordinal))
        {
            return false;
        }

        return diagnostic.Message.Contains("component", StringComparison.OrdinalIgnoreCase)
               && diagnostic.Message.Contains("resolve", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryResolveComponentName(
        string text,
        LspDiagnostic diagnostic,
        out string componentName)
    {
        var diagnosticStartOffset = LspProtocolHelpers.GetOffset(text, diagnostic.Range.Start);
        var diagnosticEndOffset = LspProtocolHelpers.GetOffset(text, diagnostic.Range.End);

        foreach (Match match in JazorMarkupPatterns.ComponentTagPattern.Matches(text))
        {
            var nameGroup = match.Groups["name"];
            if (!nameGroup.Success)
            {
                continue;
            }

            if (!RangesOverlap(
                    match.Index,
                    match.Length,
                    diagnosticStartOffset,
                    Math.Max(0, diagnosticEndOffset - diagnosticStartOffset)))
            {
                continue;
            }

            componentName = nameGroup.Value;
            return true;
        }

        var quotedComponentMatch = QuotedComponentNamePattern.Match(diagnostic.Message);
        if (quotedComponentMatch.Success)
        {
            componentName = quotedComponentMatch.Groups["name"].Value;
            return true;
        }

        componentName = string.Empty;
        return false;
    }

    private static bool HasImportDirective(string text, string componentName)
        => JazorImportDirectiveLocator.EnumerateModuleDirectives(text)
            .Any(match => JazorImportDirectiveLocator.HasLocalBinding(match.Clause, componentName));

    private static bool TryResolveImportPath(
        string documentPath,
        string componentName,
        out string importPath)
    {
        if (JoltWorkspaceResolver.TryResolveNearbyVueComponent(
                documentPath,
                componentName,
                out _,
                out importPath))
        {
            return true;
        }

        importPath = string.Empty;
        return false;
    }

    private static LspCodeAction CreateImportAction(
        DocumentSnapshot document,
        string componentName,
        string importPath)
    {
        var text = document.Text;
        var newline = text.Contains("\r\n", StringComparison.Ordinal)
            ? "\r\n"
            : "\n";
        var importLine = $"@module {componentName} from \"{importPath}\"";
        var (insertOffset, newText) = DetermineInsertion(text, importLine, newline);

        var uri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath);
        return new LspCodeAction
        {
            Title = $"Add @module for {componentName}",
            Kind = "quickfix",
            Edit = new LspWorkspaceEdit
            {
                Changes = new Dictionary<string, LspTextEdit[]>
                {
                    [uri] =
                    [
                        new LspTextEdit
                        {
                            Range = LspProtocolHelpers.ToRange(text, insertOffset, length: 0),
                            NewText = newText
                        }
                    ]
                }
            }
        };
    }

    private static (int InsertOffset, string NewText) DetermineInsertion(
        string text,
        string importLine,
        string newline)
    {
        var importMatches = JazorImportDirectiveLocator.EnumerateDirectiveLines(text).ToArray();
        if (importMatches.Length == 0)
        {
            return (0, importLine + newline);
        }

        var lastImportDirective = importMatches[^1];
        return (
            lastImportDirective.LineStartIndex + lastImportDirective.LineLength,
            newline + importLine);
    }

    private static bool RangesOverlap(
        int leftStart,
        int leftLength,
        int rightStart,
        int rightLength)
    {
        var leftEnd = leftStart + leftLength;
        var rightEnd = rightStart + rightLength;
        return leftStart <= rightEnd && rightStart <= leftEnd;
    }
}
