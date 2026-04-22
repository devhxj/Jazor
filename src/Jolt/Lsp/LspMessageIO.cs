using System.Text;

namespace Jolt.Lsp;

internal sealed class LspMessageReader
{
    private const int MaxHeaderBytes = 64 * 1024;
    private const int MaxHeaderLineBytes = 8 * 1024;
    private const int MaxContentBytes = 16 * 1024 * 1024;

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

        if (contentLength.Value > MaxContentBytes)
        {
            throw new InvalidDataException($"LSP message body exceeds the {MaxContentBytes} byte safety limit.");
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
        var headerBytes = 0;
        while ((line = await ReadHeaderLineAsync(cancellationToken)) is not null)
        {
            headerBytes += line.Length + 2;
            if (headerBytes > MaxHeaderBytes)
            {
                throw new InvalidDataException("LSP headers exceed the safety limit.");
            }

            if (line.Length == 0)
            {
                if (!sawHeader)
                {
                    return null;
                }

                return contentLength
                    ?? throw new InvalidDataException("LSP message is missing the Content-Length header.");
            }

            sawHeader = true;
            if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
            {
                var rawValue = line["Content-Length:".Length..].Trim();
                if (!int.TryParse(rawValue, out var parsed))
                {
                    throw new InvalidDataException("LSP message Content-Length header is invalid.");
                }

                if (parsed < 0)
                {
                    throw new InvalidDataException("LSP message Content-Length must be non-negative.");
                }

                contentLength = parsed;
            }
        }

        return null;
    }

    private async ValueTask<string?> ReadHeaderLineAsync(CancellationToken cancellationToken)
    {
        var bytes = new List<byte>(64);
        var singleByte = new byte[1];
        while (true)
        {
            var read = await _input.ReadAsync(singleByte.AsMemory(0, 1), cancellationToken);
            if (read == 0)
            {
                if (bytes.Count == 0)
                {
                    return null;
                }

                throw new InvalidDataException("Unexpected EOF while reading LSP message headers.");
            }

            if (singleByte[0] == '\n')
            {
                return Encoding.ASCII.GetString(bytes.ToArray()).TrimEnd('\r');
            }

            bytes.Add(singleByte[0]);
            if (bytes.Count > MaxHeaderLineBytes)
            {
                throw new InvalidDataException("LSP header line exceeds the safety limit.");
            }
        }
    }
}

internal sealed class LspMessageWriter
{
    private readonly Stream _output;
    private readonly SemaphoreSlim _writeGate = new(1, 1);

    public LspMessageWriter(Stream output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    public async ValueTask WriteMessageAsync(string json, CancellationToken cancellationToken)
    {
        var body = Encoding.UTF8.GetBytes(json);
        var header = Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n");
        await _writeGate.WaitAsync(cancellationToken);
        try
        {
            await _output.WriteAsync(header.AsMemory(), cancellationToken);
            await _output.WriteAsync(body.AsMemory(), cancellationToken);
            await _output.FlushAsync(cancellationToken);
        }
        finally
        {
            _writeGate.Release();
        }
    }
}
