#!/usr/bin/env dotnet run
#:sdk Microsoft.NET.Sdk.Web
#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Jazor.NodeHost;

if (args is ["--build-consumer", var directory])
{
    // Exit this apphost before cleanup so Windows releases Vite's native modules.
    await NodeToolchain.BuildAsync(directory, ssr: false, CancellationToken.None, Console.Out);
    return;
}

var rounds = args is [_, _, "--benchmark", var count] && int.TryParse(count, out var parsed) ? parsed : 0;
if (args.Length != 2 && (rounds is < 1 or > 100))
    throw new ArgumentException("Usage: verify-jolt-mixed.cs <Jolt CLI apphost> <Chromium executable> [--benchmark <1-100 rounds>]");

var appHost = Path.GetFullPath(args[0]);
var browser = Path.GetFullPath(args[1]);
var scriptDirectory = Path.GetDirectoryName(ScriptPath())!;
var repo = Path.GetFullPath(Path.Combine(scriptDirectory, "..", ".."));
var sample = Path.Combine(repo, "samples", "Jolt.Mixed");
var browserCheck = Path.Combine(repo, "samples", "Jolt.Counter", "verify-browser.cs");
var bridge = Path.Combine(scriptDirectory, "fixtures", "vue-typecheck.cjs");
var store = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.Combine(repo, ".tmp", "nodeapi-store");
using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(240 + 20 * rounds));
var stop = deadline.Token;
var scratchRoot = Directory.CreateDirectory(Path.Combine(repo, ".tmp"));
var project = Directory.CreateDirectory(Path.Combine(scratchRoot.FullName, $"jolt-mixed-{Guid.NewGuid():N}"));
try
{
    // Copy only authored inputs so a previous local dev/build cannot seed this run.
    foreach (var relative in new[] { "package.json", "pnpm-lock.yaml", "index.html", "main.ts", "probe.tsx", "App.vue",
        "tsconfig.json", "editors/MixedEditor.jazor", "editors/NameInput.vue", "editors/draft.svg" })
    {
        var destination = Path.Combine(project.FullName, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(Path.Combine(sample, relative), destination);
    }

    var restoreStarted = Stopwatch.GetTimestamp();
    await NodeToolchain.RestoreAsync(project.FullName, store, stop, Console.Out);
    if (rounds != 0)
    {
        var restoreMs = Stopwatch.GetElapsedTime(restoreStarted).TotalMilliseconds;
        var startupStarted = Stopwatch.GetTimestamp();
        using var server = new ObservedProcess(Start(appHost, "dev", ".", "--port", AvailablePort()));
        var origin = await server.WaitFor(IsLocalOrigin, stop);
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            scenario = "mixed-workspace-preparation",
            restoreMs,
            devReadyMs = Stopwatch.GetElapsedTime(startupStarted).TotalMilliseconds,
        }));
        await RequireSuccess("dotnet", "run", "--file", browserCheck, "--", browser, origin,
            project.FullName, $"--mixed-benchmark={rounds}");
        return;
    }

    await RequireSuccess(appHost, "build");
    await RequireVueTypeCheck();

    var app = Path.Combine(project.FullName, "App.vue");
    var jsx = Path.Combine(project.FullName, "probe.tsx");
    var editor = Path.Combine(project.FullName, "editors", "MixedEditor.jazor");
    var input = Path.Combine(project.FullName, "editors", "NameInput.vue");
    var appSource = await File.ReadAllTextAsync(app, stop);
    var jsxSource = await File.ReadAllTextAsync(jsx, stop);
    var editorSource = await File.ReadAllTextAsync(editor, stop);
    var inputSource = await File.ReadAllTextAsync(input, stop);

    await Replace(app, appSource, ":Limit=\"24\"", ":Limit=\"name\"");
    await Replace(jsx, jsxSource, "text=\"Draft\"", "text={24}");
    var typeErrors = await CheckVue();
    Require(typeErrors.ExitCode != 0 &&
        typeErrors.Diagnostics.Contains("App.vue(", StringComparison.Ordinal) &&
        typeErrors.Diagnostics.Contains("probe.tsx(", StringComparison.Ordinal) &&
        typeErrors.Diagnostics.Contains("TS2322", StringComparison.Ordinal), $"Expected wrong prop types in Vue and TSX.\n{typeErrors.Diagnostics}");
    await File.WriteAllTextAsync(app, appSource, stop);
    await File.WriteAllTextAsync(jsx, jsxSource, stop);
    Console.WriteLine("Vue SFC and TSX both rejected the invalid typed component props.");

    await Replace(editor, editorSource, "Limit=\"@Limit\"", "Limit=\"@Text\"");
    var badBuild = await Run(appHost, "build");
    Require(badBuild.ExitCode != 0 && InvalidCaller(badBuild.Output), "Expected a typed SFC prop error in the authored Jolt caller.");
    await File.WriteAllTextAsync(editor, editorSource, stop);

    await RequireSuccess(appHost, "bundle");
    await CheckProduction();
    await RequireTypeScriptDefinition(jsx, editor, 0);

    using (var server = new ObservedProcess(Start(appHost, "dev", ".", "--port", AvailablePort())))
    {
        var origin = await server.WaitFor(IsLocalOrigin, stop);

        // Leave the Jolt caller untouched. A script contract edit must recheck it on the running dev server.
        var recompilation = server.WaitFor(InvalidCaller, stop);
        await Replace(input, inputSource, "limit: number", "limit: string");
        await recompilation;
        var recovery = server.WaitFor(line => line.StartsWith("Compiled ", StringComparison.Ordinal), stop);
        await File.WriteAllTextAsync(input, inputSource, stop);
        await recovery;
        Console.WriteLine("The running server rechecked the Jolt caller after both SFC contract edits.");

        // Vue infers its slot contract from template expressions. The literal
        // text cache must still rebind a slot payload when its expression changes.
        var slotInput = inputSource.Replace("</label>", "<slot :limit=\"12345\" /></label>", StringComparison.Ordinal);
        var slotEditor = editorSource.Replace(" />", "><template #default context=\"slotScope\"><p>@(slotScope.limit + 1)</p></template></NameInput>", StringComparison.Ordinal);
        var slotCompiled = server.WaitFor(line => line.StartsWith("Compiled ", StringComparison.Ordinal), stop);
        await File.WriteAllTextAsync(input, slotInput, stop);
        await File.WriteAllTextAsync(editor, slotEditor, stop);
        await slotCompiled;
        var slotRejected = server.WaitFor(line => line.Contains("CS0019", StringComparison.Ordinal), stop);
        await Replace(input, slotInput, ":limit=\"12345\"", ":limit=\"false\"");
        await slotRejected;
        var slotRecovered = server.WaitFor(line => line.StartsWith("Compiled ", StringComparison.Ordinal), stop);
        await File.WriteAllTextAsync(editor, editorSource, stop);
        await File.WriteAllTextAsync(input, inputSource, stop);
        await slotRecovered;
        Console.WriteLine("The running server rechecked the inferred Vue slot type after a template expression edit.");

        await RequireSuccess("dotnet", "run", "--file", browserCheck, "--", browser, origin, project.FullName, "--mixed");
    }

    // Reuse the same application, but resolve the component through an installed package.
    await File.WriteAllTextAsync(input, inputSource, stop);
    await File.WriteAllTextAsync(editor, editorSource, stop);
    var library = Path.Combine(project.FullName, ".jazor", "mixed-package");
    await RequireSuccess(appHost, "library", ".", "--output", library);
    var manifestPath = Path.Combine(project.FullName, "package.json");
    var manifest = JsonNode.Parse(await File.ReadAllTextAsync(manifestPath, stop))!;
    var packageName = manifest["name"]!.GetValue<string>();
    var libraryManifest = JsonNode.Parse(await File.ReadAllTextAsync(Path.Combine(library, "package.json"), stop))!;
    Require(JsonNode.DeepEquals(libraryManifest["peerDependencies"]?["vue"], manifest["dependencies"]!["vue"]),
        "Expected Vue to remain a peer dependency of the library.");
    var scripts = Directory.EnumerateFiles(library, "*.mjs", SearchOption.AllDirectories).Select(File.ReadAllText).ToArray();
    Require(scripts.Any(code => Regex.IsMatch(code, """\bfrom\s*["']vue["']""")) &&
        scripts.All(code => !Regex.IsMatch(code, """\b(?:from|import)\s*["'][^"']+\.vue(?:\?[^"']*)?["']""")) &&
        !Directory.EnumerateFiles(library, "*.vue", SearchOption.AllDirectories).Any() &&
        Directory.EnumerateFiles(library, "*.css", SearchOption.AllDirectories).Any(),
        "Expected library chunks to import Vue and include compiled CSS and SFC modules.");

    // The fixture's source imports and installed-package imports use the same Vue app.
    // Use a distinct consumer package name so Node resolves the installed dependency.
    manifest["name"] = "mixed-editor-consumer";
    manifest["dependencies"]![packageName] = "file:.jazor/mixed-package";
    await File.WriteAllTextAsync(manifestPath, manifest.ToJsonString(), stop);
    await Replace(app, appSource, "import MixedEditor from './editors/MixedEditor.jazor';",
        $"import {{ MixedEditor }} from '{packageName}';");
    await Replace(jsx, jsxSource, "import MixedEditor from './editors/MixedEditor.jazor';",
        $"import {{ MixedEditor }} from '{packageName}';");
    await File.WriteAllTextAsync(Path.Combine(project.FullName, "vite.config.js"), """
        import vue from '@vitejs/plugin-vue';
        export default { plugins: [vue()] };
        """, stop);
    await NodeToolchain.RestoreAsync(project.FullName, store, stop, Console.Out);
    await RequireVueTypeCheck();
    await RequireSuccess(Environment.ProcessPath!, "--build-consumer", project.FullName);
    await CheckProduction();
    Console.WriteLine("Verified mixed source and installed mixed library in Chromium.");
}
finally
{
    project.Delete(recursive: true);
}

ProcessStartInfo Start(string file, params string[] arguments)
{
    var start = new ProcessStartInfo(file)
    {
        WorkingDirectory = project.FullName,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
    };
    start.Environment["JazorPnpmStoreDirectory"] = store;
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
    return start;
}

async Task<(int ExitCode, string Output)> Run(string file, params string[] arguments)
{
    using var process = Process.Start(Start(file, arguments)) ?? throw new InvalidOperationException($"Cannot start {file}.");
    var stdout = process.StandardOutput.ReadToEndAsync(stop);
    var stderr = process.StandardError.ReadToEndAsync(stop);
    try
    {
        await process.WaitForExitAsync(stop);
        var output = (await stdout) + (await stderr);
        Console.Write(output);
        return (process.ExitCode, output);
    }
    finally
    {
        if (!process.HasExited)
            process.Kill(entireProcessTree: true);
        await process.WaitForExitAsync();
    }
}

async Task RequireSuccess(string file, params string[] arguments)
{
    var result = await Run(file, arguments);
    Require(result.ExitCode == 0, $"{file} {string.Join(' ', arguments)} exited with code {result.ExitCode}.");
}

async Task CheckProduction()
{
    var dist = Path.Combine(project.FullName, "dist");
    var assets = Path.Combine(dist, "assets");
    Require(Directory.EnumerateFiles(assets, "*.js").Any() && Directory.EnumerateFiles(assets, "*.css").Any(),
        "Expected Vite to produce JavaScript and stylesheet assets.");
    var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { WebRootPath = dist });
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
    await using var production = builder.Build();
    production.Urls.Add("http://127.0.0.1:0");
    production.UseDefaultFiles();
    production.UseStaticFiles();
    await production.StartAsync(stop);
    await RequireSuccess("dotnet", "run", "--file", browserCheck, "--", browser, production.Urls.Single(), "--mixed");
}

async Task<(int ExitCode, string Diagnostics)> CheckVue()
{
    using var node = new NodeEnvironment(project.FullName, Console.Out);
    var result = await node.InvokeAsync(bridge, "typeCheck", stop, Path.Combine(project.FullName, "tsconfig.json"));
    using var json = JsonDocument.Parse(result);
    return (json.RootElement.GetProperty("exitCode").GetInt32(), json.RootElement.GetProperty("diagnostics").GetString()!);
}

async Task RequireTypeScriptDefinition(string source, string target, int offset)
{
    using var node = new NodeEnvironment(project.FullName, Console.Out);
    var result = await node.InvokeAsync(bridge, "typeScriptDefinition", stop,
        Path.Combine(project.FullName, "tsconfig.json"), source, "<MixedEditor", "2");
    using var definitions = JsonDocument.Parse(result);
    if (!definitions.RootElement.EnumerateArray().Any(definition =>
        Path.GetRelativePath(target, definition.GetProperty("fileName").GetString()!) == "." &&
        definition.GetProperty("pos").GetInt32() == offset))
        throw new InvalidOperationException($"Expected TSX definition at {target} offset {offset}. Received {result}.");
    Console.WriteLine("TypeScript resolved the TSX component definition to the authored Jolt source.");
}

async Task RequireVueTypeCheck()
{
    var result = await CheckVue();
    Require(result.ExitCode == 0, result.Diagnostics);
}

async Task Replace(string path, string source, string previous, string replacement)
{
    Require(source.Contains(previous, StringComparison.Ordinal), $"Missing fixture edit in {path}.");
    await File.WriteAllTextAsync(path, source.Replace(previous, replacement, StringComparison.Ordinal), stop);
}

static bool InvalidCaller(string text) => text.Contains("editors/MixedEditor.jazor", StringComparison.Ordinal) &&
    text.Contains("error CS1503", StringComparison.Ordinal);

static bool IsLocalOrigin(string line) =>
    Uri.TryCreate(line, UriKind.Absolute, out var uri) && uri.Scheme == "http" && uri.IsLoopback;

static string AvailablePort()
{
    using var probe = new TcpListener(IPAddress.Loopback, 0);
    probe.Start();
    return ((IPEndPoint)probe.LocalEndpoint).Port.ToString(System.Globalization.CultureInfo.InvariantCulture);
}

static void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

static string ScriptPath([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;

sealed class ObservedProcess : IDisposable
{
    private readonly Process _process;
    private readonly Channel<string> _events = Channel.CreateUnbounded<string>();

    public ObservedProcess(ProcessStartInfo start)
    {
        _process = Process.Start(start) ?? throw new InvalidOperationException($"Cannot start {start.FileName}.");
        _process.OutputDataReceived += OnLine;
        _process.ErrorDataReceived += OnLine;
        _process.Exited += (_, _) => _events.Writer.TryComplete();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
        _process.EnableRaisingEvents = true;
    }

    public async Task<string> WaitFor(Func<string, bool> predicate, CancellationToken stop)
    {
        await foreach (var line in _events.Reader.ReadAllAsync(stop))
            if (predicate(line))
                return line;
        throw new InvalidOperationException($"Server exited with code {_process.ExitCode} before reporting the expected event.");
    }

    public void Dispose()
    {
        if (!_process.HasExited)
            _process.Kill(entireProcessTree: true);
        _process.WaitForExit();
        _process.Dispose();
    }

    private void OnLine(object sender, DataReceivedEventArgs message)
    {
        if (message.Data is not null)
        {
            Console.WriteLine(message.Data);
            _events.Writer.TryWrite(message.Data);
        }
    }
}
