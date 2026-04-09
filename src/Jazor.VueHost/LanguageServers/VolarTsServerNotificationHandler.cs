using System.Text.Json;

namespace Jazor.VueHost.LanguageServers;

internal sealed class VolarTsServerNotificationHandler : ILspServerNotificationHandler
{
    private ExternalLspClient? _volarClient;
    private readonly TypeScriptServerClient _tsServerClient;

    public VolarTsServerNotificationHandler(TypeScriptServerClient tsServerClient)
    {
        _tsServerClient = tsServerClient ?? throw new ArgumentNullException(nameof(tsServerClient));
    }

    public void AttachClient(ExternalLspClient volarClient)
        => _volarClient = volarClient ?? throw new ArgumentNullException(nameof(volarClient));

    public async ValueTask<bool> HandleNotificationAsync(
        string method,
        object? parameters,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(method, "tsserver/request", StringComparison.Ordinal))
        {
            return false;
        }

        if (parameters is not JsonElement element
            || element.ValueKind != JsonValueKind.Array
            || element.GetArrayLength() < 3)
        {
            return true;
        }

        var requestId = element[0].Clone();
        var command = element[1].GetString();
        if (string.IsNullOrWhiteSpace(command))
        {
            return true;
        }

        var arguments = element[2].Clone();
        var responseBody = await _tsServerClient.SendRequestAsync(command, arguments, cancellationToken);
        if (_volarClient is null)
        {
            return true;
        }

        await _volarClient.SendNotificationAsync(
            "tsserver/response",
            new object?[]
            {
                requestId,
                responseBody
            },
            cancellationToken);
        return true;
    }
}
