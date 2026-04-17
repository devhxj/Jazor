using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Routing;

namespace Jazor.VueHost.Extensions;

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
