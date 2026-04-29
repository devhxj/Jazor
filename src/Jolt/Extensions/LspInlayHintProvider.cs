using Jolt.Lsp;
using ECMAScript.Internal.VueContracts.Protocol;

namespace Jolt.Extensions;

internal interface ILspInlayHintProvider
{
    string Name { get; }

    int Priority { get; }

    ValueTask<IReadOnlyList<LspInlayHint>> ProvideInlayHintsAsync(
        LspInlayHintProviderContext context,
        CancellationToken cancellationToken);
}

internal sealed record LspInlayHintProviderContext(
    DocumentSnapshot Document,
    LspRange Range,
    IReadOnlyList<LspInlayHint> ExistingHints);
