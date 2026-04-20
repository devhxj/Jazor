using System.Text;

namespace Jolt.Lsp;

internal sealed class LspMessageReader
{
    private readonly Stream _input;

    public LspMessageReader(Stream input)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
    }

    public async ValueTask<string?> ReadMessageAsync(CancellationToken cancellationToken)
    {
        var contentLength = await ReadContentLengthAsync(cancellationToken);
        if (contentLength is null)
        {
            return null;
        }

        var buffer = new byte[contentLength.Value];
        var read = 0;
        while (read < buffer.Length)
        {
            var chunk = await _input.ReadAsync(buffer.AsMemory(read, buffer.Length - read), cancellationToken);
            if (chunk == 0)
            {
                return null;
            }

            read += chunk;
        }

        return Encoding.UTF8.GetString(buffer);
    }

    private async ValueTask<int?> ReadContentLengthAsync(CancellationToken cancellationToken)
    {
        string? line;
        var contentLength = default(int?);
        var sawHeader = false;
        while ((line = await ReadHeaderLineAsync(cancellationToken)) is not null)
        {
            if (line.Length == 0)
            {
                return sawHeader ? contentLength : null;
            }

            sawHeader = true;
            if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
            {
                var rawValue = line["Content-Length:".Length..].Trim();
                if (int.TryParse(rawValue, out var parsed))
                {
                    contentLength = parsed;
                }
            }
        }

        return null;
    }

    private async ValueTask<string?> ReadHeaderLineAsync(CancellationToken cancellationToken)
    {
        var bytes = new List<byte>();
        while (true)
        {
            var buffer = new byte[1];
            var read = await _input.ReadAsync(buffer.AsMemory(0, 1), cancellationToken);
            if (read == 0)
            {
                return bytes.Count == 0 ? null : Encoding.ASCII.GetString(bytes.ToArray()).TrimEnd('\r');
            }

            if (buffer[0] == '\n')
            {
                return Encoding.ASCII.GetString(bytes.ToArray()).TrimEnd('\r');
            }

            bytes.Add(buffer[0]);
        }
    }
}

internal sealed class LspMessageWriter
{
    private readonly Stream _output;

    public LspMessageWriter(Stream output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    public async ValueTask WriteMessageAsync(string json, CancellationToken cancellationToken)
    {
        var body = Encoding.UTF8.GetBytes(json);
        var header = Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n");
        await _output.WriteAsync(header.AsMemory(), cancellationToken);
        await _output.WriteAsync(body.AsMemory(), cancellationToken);
        await _output.FlushAsync(cancellationToken);
    }
}
