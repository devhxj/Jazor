using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Workspace;

internal interface IWorkspaceDocumentChangeSink
{
    ValueTask OnWorkspaceDocumentChangedAsync(
        DocumentSnapshot document,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        CancellationToken cancellationToken);
}

internal sealed class NullWorkspaceDocumentChangeSink : IWorkspaceDocumentChangeSink
{
    public static NullWorkspaceDocumentChangeSink Instance { get; } = new();

    private NullWorkspaceDocumentChangeSink()
    {
    }

    public ValueTask OnWorkspaceDocumentChangedAsync(
        DocumentSnapshot document,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}
