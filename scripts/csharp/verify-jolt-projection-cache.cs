#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Jazor.NodeHost;

if (args is not [var executable])
    throw new ArgumentException("Usage: verify-jolt-projection-cache.cs <Jolt CLI apphost>");

var cli = Path.GetFullPath(executable);
var root = Directory.GetCurrentDirectory();
var fixture = Path.Combine(root, "scripts", "csharp", "fixtures", "jolt-monaco-browser");
var project = Directory.CreateDirectory(Path.Combine(root, ".tmp", $"projection-cache-{Guid.NewGuid():N}"));
var generated = Path.Combine(project.FullName, "obj", "Jolt", "projections");
var metadata = Path.Combine(project.FullName, ".jazor", "cache");
var client = project.CreateSubdirectory("client");
var declaration = Path.Combine(client.FullName, "client.d.ts");
var output = Path.Combine(project.FullName, "Client.g.cs");
var app = Path.Combine(project.FullName, "App.jazor");
const string source = """
    @using Client = Npm.ProjectionCacheFixture.Exports
    <p>@Label</p>
    @code {
        private string Label => Client.Format("ready") + Client.Count();
    }
    """;
try
{
    File.Copy(Path.Combine(fixture, "pnpm-lock.yaml"), Path.Combine(project.FullName, "pnpm-lock.yaml"));
    var manifest = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(Path.Combine(fixture, "package.json")))!;
    manifest["dependencies"]!["projection-cache-fixture"] = "link:./client";
    File.WriteAllText(Path.Combine(project.FullName, "package.json"), manifest.ToJsonString());
    File.WriteAllText(Path.Combine(client.FullName, "package.json"),
        """{"name":"projection-cache-fixture","version":"1.0.0","type":"module","exports":"./client.js"}""");
    File.WriteAllText(Path.Combine(client.FullName, "client.js"), "export const format = String; export const count = () => 2;");
    File.WriteAllText(declaration, "export declare function format(value: string): string;\nexport declare function count(): number;\n");
    File.WriteAllText(app, source);
    var store = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.Combine(root, ".tmp", "nodeapi-store");
    await NodeToolchain.RestoreAsync(project.FullName, store, CancellationToken.None, Console.Out);

    string[] Bindings(string first, string second, string? destination = null) =>
        ["bindings", ".", "--module", "projection-cache-fixture", "--export", first, "--export", second, "--output", destination ?? output];

    var parallelOutput = Path.Combine(project.FullName, "Concurrent.g.cs");
    await Task.WhenAll(Run(Bindings("format", "count")), Run(Bindings("count", "format", parallelOutput)));
    var expected = File.ReadAllText(output);
    if (File.ReadAllText(parallelOutput) != expected)
        throw new InvalidOperationException("Concurrent publishers must produce identical binding source.");
    File.Delete(parallelOutput);
    var bindings = Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories).Single();
    var bindingsStamp = Mark(bindings);
    File.Delete(output);
    await Run(Bindings("count", "format"));
    if (File.ReadAllText(output) != expected || File.GetLastWriteTimeUtc(bindings) != bindingsStamp ||
        Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories).Length != 1)
        throw new InvalidOperationException("A reordered bindings request must reuse cached text and restore its output.");

    File.Delete(output);
    await Run(["build", "."]);
    var projection = Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories).Single();
    Check(Path.GetDirectoryName(projection)!, "symbols.json");
    var projectionStamp = Mark(projection);
    var componentOutput = Path.Combine(project.FullName, ".jolt", "App.mjs");
    var componentText = File.ReadAllText(componentOutput);
    File.Delete(componentOutput);
    await Run(["build", "."]);
    if (File.GetLastWriteTimeUtc(projection) != projectionStamp || File.ReadAllText(componentOutput) != componentText)
        throw new InvalidOperationException("A second build must reuse its projection and restore component output.");
    Check(metadata, "*");
    var metadataEntries = Directory.GetFiles(metadata, "*.json", SearchOption.AllDirectories);
    var metadataStamps = metadataEntries.ToDictionary(path => path, File.GetLastWriteTimeUtc);
    await VerifyDefinition(() =>
    {
        if (File.GetLastWriteTimeUtc(projection) != projectionStamp ||
            metadataStamps.Any(entry => File.GetLastWriteTimeUtc(entry.Key) != entry.Value))
            throw new InvalidOperationException("LSP rewrote cached projection or metadata.");
    });

    var declarations = File.ReadAllText(declaration);
    File.WriteAllText(declaration, "// Public documentation moved.\n" + declarations);
    await Run(Bindings("format", "count"));
    if (File.ReadAllText(output) != expected || File.GetLastWriteTimeUtc(bindings) != bindingsStamp ||
        Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories).Length != 1)
        throw new InvalidOperationException("Changing declaration offsets must reuse the public C# cache entry.");
    File.Delete(output);
    File.WriteAllText(declaration, "export declare function format(value: number): string;\nexport declare function count(): number;\n");
    await Run(Bindings("format", "count"));
    if (File.ReadAllText(output) == expected || !File.ReadAllText(output).Contains("Format(double", StringComparison.Ordinal))
        throw new InvalidOperationException("Changing the declaration must regenerate the public signature.");
    File.Delete(output);
    await Run(["build", "."], "CS1503");
    Console.WriteLine("Verified concurrent publication, cross-process binding and component reuse, output restoration, LSP navigation, and declaration invalidation.");
}
finally
{
    project.Delete(recursive: true);
}

async Task Run(string[] arguments, string? diagnostic = null)
{
    using var process = Start(arguments);
    using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
    var stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
    var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
    try
    {
        await process.WaitForExitAsync(timeout.Token);
        var text = await stdout + await stderr;
        if (diagnostic is null ? process.ExitCode != 0 :
            process.ExitCode == 0 || !text.Contains(diagnostic, StringComparison.Ordinal))
            throw new InvalidOperationException($"jolt {string.Join(' ', arguments)} returned {process.ExitCode}\n{text}");
    }
    finally
    {
        await Stop(process);
    }
}

Process Start(string[] arguments)
{
    var start = new ProcessStartInfo(cli)
    {
        WorkingDirectory = project.FullName,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
    return Process.Start(start) ?? throw new InvalidOperationException($"Could not start {cli}.");
}

async Task VerifyDefinition(Action verifyCacheReuse)
{
    using var process = Start(["lsp"]);
    using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
    var stop = timeout.Token;
    var input = process.StandardInput.BaseStream;
    var output = process.StandardOutput.BaseStream;
    var errors = process.StandardError.ReadToEndAsync(stop);
    try
    {
        var uri = new Uri(app).AbsoluteUri;
        await Send("initialize", new { rootUri = new Uri(project.FullName).AbsoluteUri, capabilities = new { } }, 1);
        await Result(1);
        await Send("initialized", new { });
        await Send("textDocument/didOpen", new { textDocument = new { uri, languageId = "jazor", version = 1, text = source } });
        while (true)
        {
            var notification = await ReadMessage();
            if (notification.TryGetProperty("method", out var method) &&
                method.GetString() == "textDocument/publishDiagnostics" &&
                notification.GetProperty("params").GetProperty("uri").GetString() == uri &&
                notification.GetProperty("params").GetProperty("diagnostics").GetArrayLength() == 0)
                break;
        }
        var lines = source.Split('\n');
        var line = Array.FindIndex(lines, text => text.Contains("Client.Format", StringComparison.Ordinal));
        var character = lines[line].IndexOf("Format", StringComparison.Ordinal) + 2;
        await Send("textDocument/definition", new { textDocument = new { uri }, position = new { line, character } }, 2);
        var result = await Result(2);
        if (result.ValueKind != JsonValueKind.Object ||
            new Uri(result.GetProperty("uri").GetString()!).LocalPath != declaration)
            throw new InvalidOperationException($"LSP definition must navigate to the cached TypeScript declaration. Received {result}");
        verifyCacheReuse();
        var reexport = Path.Combine(client.FullName, "formatted.d.ts");
        var original = File.ReadAllText(declaration);
        try
        {
            File.WriteAllText(reexport, "// Moved declaration.\nexport declare function format(value: string): string;\n");
            File.WriteAllText(declaration, "export { format } from './formatted.js';\nexport declare function count(): number;\n");
            await Send("textDocument/didSave", new { textDocument = new { uri = new Uri(declaration).AbsoluteUri } });
            await WaitDiagnostics(null);
            await Send("textDocument/definition", new { textDocument = new { uri }, position = new { line, character } }, 3);
            var updated = await Result(3);
            if (updated.ValueKind != JsonValueKind.Object || new Uri(updated.GetProperty("uri").GetString()!).LocalPath != reexport ||
                updated.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() != 1)
                throw new InvalidOperationException($"LSP definition must follow the saved re-export. Received {updated}");
            if (Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories).Length != 1)
                throw new InvalidOperationException("A moved definition must reuse the same component C# source.");

            File.WriteAllText(reexport, "export declare function format(value: number): string;\n");
            await Send("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = new Uri(reexport).AbsoluteUri, type = 2 } } });
            await WaitDiagnostics("CS1503");
        }
        finally
        {
            File.WriteAllText(declaration, original);
            File.Delete(reexport);
        }
        await Send("shutdown", new { }, 4);
        await Result(4);
        await Send("exit", new { });
        await process.WaitForExitAsync(stop);
        if (process.ExitCode != 0)
            throw new InvalidOperationException(await errors);

        async Task WaitDiagnostics(string? code)
        {
            while (true)
            {
                var notification = await ReadMessage();
                if (!notification.TryGetProperty("method", out var method) ||
                    method.GetString() != "textDocument/publishDiagnostics")
                    continue;
                var publication = notification.GetProperty("params");
                if (publication.GetProperty("uri").GetString() != uri)
                    continue;
                var diagnostics = publication.GetProperty("diagnostics");
                if (code is null ? diagnostics.GetArrayLength() == 0 :
                    diagnostics.EnumerateArray().Any(item => item.GetProperty("code").GetString() == code))
                    return;
            }
        }
    }
    finally
    {
        await Stop(process);
    }

    async Task Send(string method, object parameters, int? id = null)
    {
        var message = new Dictionary<string, object> { ["jsonrpc"] = "2.0", ["method"] = method, ["params"] = parameters };
        if (id is int request)
            message["id"] = request;
        var body = JsonSerializer.SerializeToUtf8Bytes(message);
        await input.WriteAsync(Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n"), stop);
        await input.WriteAsync(body, stop);
        await input.FlushAsync(stop);
    }

    async Task<JsonElement> Result(int id)
    {
        while (true)
        {
            var response = await ReadMessage();
            if (response.TryGetProperty("id", out var request) && request.GetInt32() == id)
                return response.GetProperty("result");
        }
    }

    async Task<JsonElement> ReadMessage()
    {
        var next = new byte[1];
        var header = new StringBuilder();
        while (!header.ToString().EndsWith("\r\n\r\n", StringComparison.Ordinal))
        {
            await output.ReadExactlyAsync(next, stop);
            header.Append((char)next[0]);
        }
        var length = header.ToString().Split("\r\n").Single(line =>
            line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase));
        var body = new byte[int.Parse(length.AsSpan("Content-Length:".Length), System.Globalization.CultureInfo.InvariantCulture)];
        await output.ReadExactlyAsync(body, stop);
        using var message = JsonDocument.Parse(body);
        return message.RootElement.Clone();
    }
}

static DateTime Mark(string path)
{
    File.SetLastWriteTimeUtc(path, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    return File.GetLastWriteTimeUtc(path);
}

static void Check(string directory, string pattern)
{
    if (!Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories).Any())
        throw new InvalidOperationException($"Expected {pattern} under {directory}.");
}

static async Task Stop(Process process)
{
    if (process.HasExited)
        return;
    process.Kill(entireProcessTree: true);
    await process.WaitForExitAsync();
}
