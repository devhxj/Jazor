using Jolt.Lsp;
using ECMAScript.Contract.VueContracts.Protocol;

namespace Jolt.Extensions;

internal interface ILspDiagnosticProvider
{
    string Name { get; }

    int Priority { get; }

    ValueTask<IReadOnlyList<LspDiagnostic>> ProvideDiagnosticsAsync(
        LspDiagnosticProviderContext context,
        CancellationToken cancellationToken);
}

internal sealed record LspDiagnosticProviderContext(
    DocumentSnapshot Document,
    IReadOnlyList<LspDiagnostic> ExistingDiagnostics);
