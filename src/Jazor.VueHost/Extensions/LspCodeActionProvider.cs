using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Routing;
using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Extensions;

internal interface ILspCodeActionProvider
{
    string Name { get; }

    int Priority { get; }

    ValueTask<IReadOnlyList<LspCodeAction>> ProvideCodeActionsAsync(
        LspCodeActionProviderContext context,
        CancellationToken cancellationToken);
}

internal sealed record LspCodeActionProviderContext(
    DocumentSnapshot Document,
    LspRange Range,
    IReadOnlyList<LspDiagnostic> Diagnostics,
    ProjectionTarget ProjectionTarget,
    IReadOnlyList<LspCodeAction> ExistingActions);
