using Jolt.Lsp;
using Jazor.Common.VueContracts.Protocol;

namespace Jolt.Extensions;

internal interface ILspDocumentSymbolProvider
{
    string Name { get; }

    int Priority { get; }

    ValueTask<IReadOnlyList<LspDocumentSymbol>> ProvideDocumentSymbolsAsync(
        LspDocumentSymbolProviderContext context,
        CancellationToken cancellationToken);
}

internal sealed record LspDocumentSymbolProviderContext(
    DocumentSnapshot Document,
    IReadOnlyList<LspDocumentSymbol> ExistingSymbols);
