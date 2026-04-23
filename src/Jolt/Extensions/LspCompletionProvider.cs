using Jolt.Lsp;
using Jolt.Lsp.Routing;
using Jazor.Common.VueContracts.Protocol;

namespace Jolt.Extensions;

internal interface ILspCompletionProvider
{
    string Name { get; }

    int Priority { get; }

    ValueTask<IReadOnlyList<LspCompletionItem>> ProvideCompletionItemsAsync(
        LspCompletionProviderContext context,
        CancellationToken cancellationToken);
}

internal sealed record LspCompletionProviderContext(
    DocumentSnapshot Document,
    LspPosition Position,
    ProjectionTarget ProjectionTarget,
    IReadOnlyList<LspCompletionItem> ExistingItems);
