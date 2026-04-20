using Jazor.VueContracts.Protocol;
using Jolt.Lsp;
using Jolt.Lsp.Routing;

namespace Jolt.Extensions;

internal interface ILspHoverProvider
{
    string Name { get; }

    int Priority { get; }

    ValueTask<LspHoverResult?> ProvideHoverAsync(
        LspHoverProviderContext context,
        CancellationToken cancellationToken);
}

internal sealed record LspHoverProviderContext(
    DocumentSnapshot Document,
    LspPosition Position,
    ProjectionTarget ProjectionTarget,
    LspHoverResult? ExistingHover);
