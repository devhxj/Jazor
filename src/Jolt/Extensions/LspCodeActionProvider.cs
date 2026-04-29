using Jolt.Lsp;
using Jolt.Lsp.Routing;
using ECMAScript.Internal.VueContracts.Protocol;

namespace Jolt.Extensions;

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
