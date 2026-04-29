using Jolt.Lsp;
using ECMAScript.Contract.VueContracts.Protocol;

namespace Jolt.Extensions;

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
