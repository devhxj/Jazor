using Jazor.VueHost.Lsp;

namespace Jazor.VueHost.Extensions.Builtin;

internal sealed class WorkspaceSymbolExtension : IExtension, ILspWorkspaceSymbolProvider
{
    private readonly WorkspaceSymbolIndex _symbolIndex = new();

    public ExtensionMetadata Metadata { get; } = new(
        Id: "builtin.workspace-symbol",
        Name: "Builtin Workspace Symbol",
        Version: "1.0.0",
        Description: "Indexes open workspace documents for workspace/symbol requests.");

    public string Name => "BuiltinWorkspaceSymbolProvider";

    public int Priority => 200;

    public ValueTask InitializeAsync(ExtensionContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask ActivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask DeactivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask<IReadOnlyList<LspWorkspaceSymbol>> ProvideWorkspaceSymbolsAsync(
        LspWorkspaceSymbolProviderContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var indexedSymbols = _symbolIndex.Search(context.Query, context.OpenDocuments);
        if (context.ExistingSymbols.Count == 0)
        {
            return ValueTask.FromResult<IReadOnlyList<LspWorkspaceSymbol>>(indexedSymbols);
        }

        var existingKeys = context.ExistingSymbols
            .Select(CreateSymbolIdentity)
            .ToHashSet(StringComparer.Ordinal);
        var newSymbols = indexedSymbols
            .Where(symbol => !existingKeys.Contains(CreateSymbolIdentity(symbol)))
            .ToArray();
        return ValueTask.FromResult<IReadOnlyList<LspWorkspaceSymbol>>(newSymbols);
    }

    private static string CreateSymbolIdentity(LspWorkspaceSymbol symbol)
        => string.Join(
            '|',
            symbol.Name,
            symbol.Location.Uri,
            symbol.Location.Range.Start.Line,
            symbol.Location.Range.Start.Character,
            symbol.Kind,
            symbol.ContainerName);
}
