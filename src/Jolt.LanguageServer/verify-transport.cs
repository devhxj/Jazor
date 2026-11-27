#!/usr/bin/env dotnet run
#:exclude **/*.cs
#:include Transport.cs
#:property PublishAot=false
using System.Text;
using Jolt.LanguageServer;

using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var token = timeout.Token;
const string message = "中文消息";
var small = Frame(message);
var large = Frame(new string('中', 5000));
var batch = small.Concat(small).Concat(large).ToArray();

await VerifyAsync(new CountingInput(batch), expectedReads: 3);
await VerifyAsync(new CountingInput(batch, maximumChunk: 1), expectedReads: batch.Length + 1);
await VerifyTruncationAsync("Content-Len"u8.ToArray());
await VerifyTruncationAsync("Content-Length: 14\r\n"u8.ToArray());
await VerifyTruncationAsync(small[..^1]);
Console.WriteLine("Verified UTF-8, coalesced frames, byte fragmentation and EOF. Batch input reads: 3; byte-fragmented input reads: " + (batch.Length + 1) + ".");

async Task VerifyAsync(CountingInput stream, int expectedReads)
{
    using (stream)
    {
        var transport = new Transport(stream, Stream.Null);
        foreach (var expected in new[] { message, message, new string('中', 5000) })
            if ((await transport.ReadAsync(token))?.GetProperty("message").GetString() != expected)
                throw new InvalidOperationException("The frame body was corrupted.");
        if (await transport.ReadAsync(token) is not null || stream.Reads != expectedReads)
            throw new InvalidOperationException($"Expected EOF after {expectedReads} reads, saw {stream.Reads} reads.");
    }
}

async Task VerifyTruncationAsync(byte[] bytes)
{
    using var input = new CountingInput(bytes);
    try
    {
        await new Transport(input, Stream.Null).ReadAsync(token);
    }
    catch (EndOfStreamException)
    {
        return;
    }
    throw new InvalidOperationException("A truncated frame must raise EndOfStreamException.");
}

static byte[] Frame(string message)
{
    var body = Encoding.UTF8.GetBytes("{\"message\":\"" + message + "\"}");
    return Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n").Concat(body).ToArray();
}

sealed class CountingInput(byte[] contents, int maximumChunk = int.MaxValue) : MemoryStream(contents)
{
    public int Reads { get; private set; }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        Reads++;
        return base.ReadAsync(buffer[..Math.Min(buffer.Length, maximumChunk)], cancellationToken);
    }
}
