using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp;

namespace Jazor.VueHost.Frontend.Deno.Hosting;

internal interface IDenoFrontendHost : IAsyncDisposable
{
    bool IsRunning { get; }

    ValueTask StartAsync(CancellationToken cancellationToken);

    ValueTask StopAsync(CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspDiagnostic>> GetTemplateDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspCompletionItem>> GetTemplateCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken);

    ValueTask<LspHoverResult?> GetTemplateHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspLocation>> GetTemplateDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspLocation>> GetTemplateReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        CancellationToken cancellationToken);

    ValueTask<LspWorkspaceEdit?> GetTemplateRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        CancellationToken cancellationToken);
}
