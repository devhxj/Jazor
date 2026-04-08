using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Frontend.Deno.Hosting;

public interface IDenoFrontendHost : IAsyncDisposable
{
    bool IsRunning { get; }

    ValueTask StartAsync(CancellationToken cancellationToken);

    ValueTask StopAsync(CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<object>> GetTemplateCompletionItemsAsync(
        DocumentSnapshot document,
        object position,
        CancellationToken cancellationToken);

    ValueTask<object?> GetTemplateHoverAsync(
        DocumentSnapshot document,
        object position,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<object>> GetTemplateDefinitionAsync(
        DocumentSnapshot document,
        object position,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<object>> GetTemplateReferencesAsync(
        DocumentSnapshot document,
        object position,
        bool includeDeclaration,
        CancellationToken cancellationToken);

    ValueTask<object?> GetTemplateRenameAsync(
        DocumentSnapshot document,
        object position,
        string newName,
        CancellationToken cancellationToken);
}
