using Jazor.VueContracts.Protocol;
using Jolt.Lsp;

namespace Jolt.Extensions.Builtin;

internal sealed class DirectiveCompletionExtension : IExtension, ILspCompletionProvider
{
    private static readonly DirectiveDescriptor[] Directives =
    [
        new("@attribute", "Razor directive", "Apply attributes to the generated component class."),
        new("@functions", "Razor directive", "Define legacy C# members on the component."),
        new("@implements", "Razor directive", "Declare implemented interfaces for the component."),
        new("@inherits", "Razor directive", "Declare the component base type."),
        new("@inject", "Razor directive", "Inject a service into the component."),
        new("@layout", "Razor directive", "Set the layout component."),
        new("@model", "Razor directive", "Declare a model type for compatibility scenarios."),
        new("@namespace", "Razor directive", "Override generated namespace."),
        new("@page", "Razor directive", "Declare component route."),
        new("@preservewhitespace", "Razor directive", "Keep template whitespace."),
        new("@rendermode", "Razor directive", "Select render mode."),
        new("@typeparam", "Razor directive", "Declare generic type parameters."),
        new("@using", "Razor directive", "Import namespaces into generated code."),
        new("@code", "Razor directive", "Open the primary C# code block.")
    ];

    public ExtensionMetadata Metadata { get; } = new(
        Id: "builtin.directive-completion",
        Name: "Builtin Directive Completion",
        Version: "1.0.0",
        Description: "Provides directive completions for .jazor documents.");

    public string Name => "BuiltinDirectiveCompletionProvider";

    public int Priority => 200;

    public ValueTask InitializeAsync(ExtensionContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask ActivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask DeactivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask<IReadOnlyList<LspCompletionItem>> ProvideCompletionItemsAsync(
        LspCompletionProviderContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context.Document.DocumentKind != DocumentKind.Jazor)
        {
            return ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(Array.Empty<LspCompletionItem>());
        }

        if (!TryReadDirectivePrefix(context.Document.Text, context.Position, out var typedPrefix))
        {
            return ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(Array.Empty<LspCompletionItem>());
        }

        var existingLabels = context.ExistingItems
            .Select(static item => item.Label)
            .ToHashSet(StringComparer.Ordinal);
        var expectedPrefix = "@" + typedPrefix;
        var items = Directives
            .Where(directive => directive.Label.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            .Where(directive => !existingLabels.Contains(directive.Label))
            .Select(static directive => new LspCompletionItem
            {
                Label = directive.Label,
                Kind = 14,
                Detail = directive.Detail,
                Documentation = directive.Documentation
            })
            .ToArray();

        return ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(items);
    }

    private static bool TryReadDirectivePrefix(
        string text,
        LspPosition position,
        out string typedPrefix)
    {
        var offset = Math.Max(0, Math.Min(LspProtocolHelpers.GetOffset(text, position), text.Length));
        var lineStart = text.LastIndexOf('\n', Math.Max(0, offset - 1));
        if (lineStart < 0)
        {
            lineStart = 0;
        }
        else
        {
            lineStart++;
        }

        var linePrefix = text[lineStart..offset];
        var trimmedPrefix = linePrefix.TrimStart();
        if (!trimmedPrefix.StartsWith('@'))
        {
            typedPrefix = string.Empty;
            return false;
        }

        var directiveToken = trimmedPrefix[1..];
        var tokenLength = 0;
        while (tokenLength < directiveToken.Length
               && char.IsLetter(directiveToken[tokenLength]))
        {
            tokenLength++;
        }

        if (tokenLength == 0 && directiveToken.Length > 0 && !char.IsWhiteSpace(directiveToken[0]))
        {
            typedPrefix = string.Empty;
            return false;
        }

        if (directiveToken.Length > tokenLength)
        {
            typedPrefix = string.Empty;
            return false;
        }

        typedPrefix = directiveToken[..tokenLength];
        return true;
    }

    private readonly record struct DirectiveDescriptor(
        string Label,
        string Detail,
        string Documentation);
}
