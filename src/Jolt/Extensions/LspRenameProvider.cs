using Jolt.Lsp;
using Jolt.Lsp.Routing;
using Jazor.RazorVue.Protocol;

namespace Jolt.Extensions;

internal interface ILspRenameProvider
{
    string Name { get; }

    int Priority { get; }

    ValueTask<LspWorkspaceEdit?> ProvideRenameAsync(
        LspRenameProviderContext context,
        CancellationToken cancellationToken);
}

internal sealed record LspRenameProviderContext(
    DocumentSnapshot Document,
    LspPosition Position,
    string NewName,
    ProjectionTarget ProjectionTarget,
    LspWorkspaceEdit? ExistingEdit);

