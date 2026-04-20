using Jazor.VueContracts.Protocol;
using Jolt.Lsp;
using Jolt.Lsp.Routing;

namespace Jolt.Extensions;

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
