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
    private const int DefaultMaxMessageBytes = 4 * 1024 * 1024;
    private readonly WebSocket _webSocket;
    private readonly ClientWebSocket? _clientWebSocket;
    private readonly bool _ownsWebSocket;
    private readonly int _maxMessageBytes;
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    public CdpConnection(int? maxMessageBytes = null)
        : this(
            new ClientWebSocket(),
            ownsWebSocket: true,
            maxMessageBytes: maxMessageBytes)
    {
    }

    internal CdpConnection(
        WebSocket webSocket,
        bool ownsWebSocket,
        int? maxMessageBytes = null)
    {
        _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
        _clientWebSocket = webSocket as ClientWebSocket;
        _ownsWebSocket = ownsWebSocket;
        _maxMessageBytes = maxMessageBytes is > 0
            ? maxMessageBytes.Value
            : DefaultMaxMessageBytes;
    }

    public bool IsConnected => _webSocket.State == WebSocketState.Open;

    public async Task ConnectAsync(Uri endpoint, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        if (_clientWebSocket is null)
        {
            throw new InvalidOperationException("CDP connection test transport does not support ConnectAsync.");
        }

        await _clientWebSocket.ConnectAsync(endpoint, cancellationToken);
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
        var decoder = Encoding.UTF8.GetDecoder();
        var chars = new char[Encoding.UTF8.GetMaxCharCount(buffer.Length)];
        var receivedPartialMessage = false;
        var totalBytes = 0;
        while (true)
        {
            var result = await _webSocket.ReceiveAsync(buffer.AsMemory(), cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                if (receivedPartialMessage)
                {
                    throw new IOException("CDP WebSocket closed before completing the current message.");
                }

                return null;
            }

            receivedPartialMessage = true;
            totalBytes += result.Count;
            if (totalBytes > _maxMessageBytes)
            {
                throw new IOException(
                    $"CDP WebSocket message exceeds the configured {_maxMessageBytes} byte limit.");
            }

            var completedChars = decoder.GetChars(
                buffer,
                0,
                result.Count,
                chars,
                0,
                flush: result.EndOfMessage);
            text.Append(chars, 0, completedChars);
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

        if (_ownsWebSocket)
        {
            _webSocket.Dispose();
        }

        _sendLock.Dispose();
    }
}
