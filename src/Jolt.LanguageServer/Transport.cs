using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Jolt.LanguageServer;

internal sealed class Transport(Stream input, Stream output)
{
    private readonly SemaphoreSlim _writer = new(1);
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public async Task<JsonElement?> ReadAsync(CancellationToken cancellationToken)
    {
        var header = new StringBuilder();
        var next = new byte[1];
        int length = -1;
        while (true)
        {
            if (await input.ReadAsync(next, cancellationToken) == 0)
                return null;

            if (next[0] == '\n')
            {
                var line = header.ToString().TrimEnd('\r');
                header.Clear();
                if (line.Length == 0)
                    break;
                if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                    length = int.Parse(line.AsSpan("Content-Length:".Length).Trim(), CultureInfo.InvariantCulture);
            }
            else
                header.Append((char)next[0]);
        }

        if (length < 0)
            throw new InvalidDataException("Missing LSP Content-Length.");
        var body = new byte[length];
        await input.ReadExactlyAsync(body, cancellationToken);
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
