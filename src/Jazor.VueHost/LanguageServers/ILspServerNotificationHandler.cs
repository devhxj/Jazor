namespace Jazor.VueHost.LanguageServers;

internal interface ILspServerNotificationHandler
{
    ValueTask<bool> HandleNotificationAsync(
        string method,
        object? parameters,
        CancellationToken cancellationToken);
}
