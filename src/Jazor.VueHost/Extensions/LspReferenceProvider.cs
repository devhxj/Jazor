using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Routing;

namespace Jazor.VueHost.Extensions;

internal interface ILspReferenceProvider
{
    string Name { get; }

    int Priority { get; }

    ValueTask<IReadOnlyList<LspLocation>> ProvideReferencesAsync(
        LspReferenceProviderContext context,
        CancellationToken cancellationToken);
}

internal sealed record LspReferenceProviderContext(
    DocumentSnapshot Document,
    LspPosition Position,
    bool IncludeDeclaration,
    ProjectionTarget ProjectionTarget,
    IReadOnlyList<LspLocation> ExistingLocations);
