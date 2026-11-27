using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Jolt.LanguageServer;

internal sealed class Transport(Stream input, Stream output)
{
    private readonly SemaphoreSlim _writer = new(1);
    private readonly byte[] _readBuffer = new byte[4096];
    private int _readOffset;
    private int _readEnd;
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public async Task<JsonElement?> ReadAsync(CancellationToken cancellationToken)
    {
        var header = new StringBuilder();
        var started = false;
        int length = -1;
        while (true)
        {
            if (_readOffset == _readEnd)
            {
                _readEnd = await input.ReadAsync(_readBuffer, cancellationToken);
                _readOffset = 0;
                if (_readEnd == 0)
                    return started ? throw new EndOfStreamException("The stream ended inside an LSP header.") : null;
            }

            started = true;
            var next = _readBuffer[_readOffset++];
            if (next == '\n')
            {
                var line = header.ToString().TrimEnd('\r');
                header.Clear();
                if (line.Length == 0)
                    break;
                if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                    length = int.Parse(line.AsSpan("Content-Length:".Length).Trim(), CultureInfo.InvariantCulture);
            }
            else
                header.Append((char)next);
        }

        if (length < 0)
            throw new InvalidDataException("Missing LSP Content-Length.");
        var body = new byte[length];
        // Keep any following frame in the buffer and read a large body remainder directly.
        var buffered = Math.Min(_readEnd - _readOffset, length);
        _readBuffer.AsSpan(_readOffset, buffered).CopyTo(body);
        _readOffset += buffered;
        await input.ReadExactlyAsync(body.AsMemory(buffered), cancellationToken);
        return JsonSerializer.Deserialize<JsonElement>(body);
    }

    public async Task WriteAsync(object message, CancellationToken cancellationToken)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(message, Options);
        var header = Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n");
        await _writer.WaitAsync(cancellationToken);
        try
        {
            // Once a frame begins, finish writing it even if its request is canceled.
            await output.WriteAsync(header);
            await output.WriteAsync(body);
            await output.FlushAsync();
        }
        finally
        {
            _writer.Release();
        }
    }
}
