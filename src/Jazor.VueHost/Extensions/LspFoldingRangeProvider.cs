using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp;

namespace Jazor.VueHost.Extensions;

internal interface ILspFoldingRangeProvider
{
    string Name { get; }

    int Priority { get; }

    ValueTask<IReadOnlyList<LspFoldingRange>> ProvideFoldingRangesAsync(
        LspFoldingRangeProviderContext context,
        CancellationToken cancellationToken);
}

internal sealed record LspFoldingRangeProviderContext(
    DocumentSnapshot Document,
    IReadOnlyList<LspFoldingRange> ExistingRanges);
