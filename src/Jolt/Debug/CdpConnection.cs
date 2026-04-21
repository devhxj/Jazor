using System.Net.WebSockets;
using System.Text;

namespace Jolt.Debug;

internal interface ICdpConnection : IAsyncDisposable
{
    bool IsConnected { get; }

    Task ConnectAsync(Uri endpoint, CancellationToken cancellationToken);

    Task SendAsync(string payloadJson, CancellationToken cancellationToken);

    Task<string?> ReceiveAsync(CancellationToken cancellationToken);
}

internal sealed class CdpConnection : ICdpConnection
{
    private readonly ClientWebSocket _webSocket = new();
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    public bool IsConnected => _webSocket.State == WebSocketState.Open;

    public async Task ConnectAsync(Uri endpoint, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        await _webSocket.ConnectAsync(endpoint, cancellationToken);
    }

    public async Task SendAsync(string payloadJson, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadJson);
        if (!IsConnected)
        {
            throw new InvalidOperationException("CDP WebSocket is not connected.");
        }

        var payload = Encoding.UTF8.GetBytes(payloadJson);
        await _sendLock.WaitAsync(cancellationToken);
        try
        {
            await _webSocket.SendAsync(
                payload.AsMemory(),
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public async Task<string?> ReceiveAsync(CancellationToken cancellationToken)
    {
        if (_webSocket.State == WebSocketState.Closed
            || _webSocket.State == WebSocketState.Aborted)
        {
            return null;
        }

        var buffer = new byte[16 * 1024];
        var text = new StringBuilder();
        while (true)
        {
            var result = await _webSocket.ReceiveAsync(buffer.AsMemory(), cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                return null;
            }

            text.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            if (result.EndOfMessage)
            {
                break;
            }
        }

        return text.ToString();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_webSocket.State == WebSocketState.Open
                || _webSocket.State == WebSocketState.CloseReceived)
            {
                await _webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Closing CDP connection.",
                    CancellationToken.None);
            }
        }
        catch (WebSocketException)
        {
            // Best effort close.
        }

        _webSocket.Dispose();
        _sendLock.Dispose();
    }
}
