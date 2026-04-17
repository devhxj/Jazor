using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Routing;

namespace Jazor.VueHost.Extensions;

internal interface ILspSignatureHelpProvider
{
    string Name { get; }

    int Priority { get; }

    ValueTask<LspSignatureHelp?> ProvideSignatureHelpAsync(
        LspSignatureHelpProviderContext context,
        CancellationToken cancellationToken);
}

internal sealed record LspSignatureHelpProviderContext(
    DocumentSnapshot Document,
    LspPosition Position,
    ProjectionTarget ProjectionTarget,
    LspSignatureHelp? ExistingSignatureHelp);
