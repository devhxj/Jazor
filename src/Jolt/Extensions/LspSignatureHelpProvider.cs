using Jolt.Lsp;
using Jolt.Lsp.Routing;
using Jazor.RazorVue.Protocol;

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

