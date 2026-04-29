using Jolt.Lsp;
using ECMAScript.Internal.VueContracts.Protocol;

namespace Jolt.Extensions;

internal interface ILspWorkspaceSymbolProvider
{
    string Name { get; }

    int Priority { get; }

    ValueTask<IReadOnlyList<LspWorkspaceSymbol>> ProvideWorkspaceSymbolsAsync(
        LspWorkspaceSymbolProviderContext context,
        CancellationToken cancellationToken);
}

internal sealed record LspWorkspaceSymbolProviderContext(
    string Query,
    IReadOnlyList<DocumentSnapshot> OpenDocuments,
    IReadOnlyList<LspWorkspaceSymbol> ExistingSymbols,
    IReadOnlyList<LspWorkspaceFolder>? WorkspaceFolders = null);
