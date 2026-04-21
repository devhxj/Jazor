using System.Text;
using System.Text.Json;
using Jolt.Debug;
using Jolt.Lsp;
using Jolt.SourceMap;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JoltDebugDapServerTests
{
    [TestMethod]
    public async Task DapServer_RunAsync_WithMalformedRequest_KeepsServingLaterMessages()
    {
        var server = new DapServer(CreateHandler());
        using var input = new MemoryStream();
        using var output = new MemoryStream();

        WriteMessage(input, "{not-json");
        WriteMessage(
            input,
            """
            {"seq":1,"type":"request","command":"initialize"}
            """);
        input.Position = 0;

        await server.RunAsync(input, output, CancellationToken.None);

        output.Position = 0;
        var reader = new LspMessageReader(output);
        var responseJson = await reader.ReadMessageAsync(CancellationToken.None);
        Assert.IsNotNull(responseJson);

        using var responseDocument = JsonDocument.Parse(responseJson);
        Assert.AreEqual("response", responseDocument.RootElement.GetProperty("type").GetString());
        Assert.AreEqual("initialize", responseDocument.RootElement.GetProperty("command").GetString());
    }

    private static DapRequestHandler CreateHandler()
        => new(
            new DapSession(cdpClient: null),
            new BreakpointManager(new InMemorySourceMapService()),
            new CallStackMapper(new InMemorySourceMapService()));

    private static void WriteMessage(Stream stream, string json)
    {
        var body = Encoding.UTF8.GetBytes(json);
        var header = Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n");
        stream.Write(header, 0, header.Length);
        stream.Write(body, 0, body.Length);
    }
}
