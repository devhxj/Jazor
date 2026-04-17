using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Routing;

namespace Jazor.VueHost.Extensions;

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
