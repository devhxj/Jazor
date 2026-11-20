#!/usr/bin/env dotnet run
#:sdk Microsoft.NET.Sdk.Web
#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj

using System.Diagnostics;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Jazor.NodeHost;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

if (args is not [var cli, var chromium, var report, .. var comparison] || comparison.Length > 1)
    throw new ArgumentException("Usage: benchmark-keyed-rows.cs <Jolt apphost> <Chromium executable> <report.json> [baseline Jolt apphost]");

cli = Path.GetFullPath(cli);
chromium = Path.GetFullPath(chromium);
report = Path.GetFullPath(report);
var sample = Path.GetDirectoryName(ScriptPath())!;
var repo = Path.GetFullPath(Path.Combine(sample, "..", ".."));
var fixture = Path.Combine(sample, ".fixtures", "keyed-rows");
var root = Directory.CreateDirectory(Path.Combine(repo, ".tmp", $"keyed-rows-{Guid.NewGuid():N}"));
var variantsToBuild = new List<(string Name, string? Cli)> { ("jolt", cli), ("sfc", null) };
if (comparison is [var baseline])
    variantsToBuild.Add(("baseline", Path.GetFullPath(baseline)));
int[][] orders = variantsToBuild.Count == 2
    ? [[0, 1], [1, 0]]
    : [[0, 1, 2], [1, 2, 0], [2, 0, 1], [2, 1, 0], [1, 0, 2], [0, 2, 1]];
var steps = new[] { "mount", "update", "insert", "reverse" };
var store = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.Combine(repo, ".tmp", "nodeapi-store");
const int warmup = 6;
const int cycles = 24;
using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
var token = timeout.Token;
try
{
    foreach (var (name, apphost) in variantsToBuild)
    {
        var project = root.CreateSubdirectory(name).FullName;
        var jolt = apphost is not null;
        foreach (var file in new[] { "package.json", "pnpm-lock.yaml", "index.html", "vite.config.js" })
            File.Copy(Path.Combine(sample, file), Path.Combine(project, file == "vite.config.js" ? "vite.shared.config.js" : file));
        File.Copy(Path.Combine(fixture, "KeyedRows.js"), Path.Combine(project, "main.js"));
        File.Copy(Path.Combine(fixture, "KeyedRows.css"), Path.Combine(project, "KeyedRows.css"));
        var component = jolt ? "KeyedRows.jazor" : "KeyedRows.vue";
        File.Copy(Path.Combine(fixture, component), Path.Combine(project, component));
        await File.WriteAllTextAsync(Path.Combine(project, "component.js"),
            jolt ? "export { default } from './.jolt/KeyedRows.mjs';" : "export { default } from './KeyedRows.vue';", token);
        var sfcCompiler = jolt ? "" : """
            import { compileScript, parse } from 'vue/compiler-sfc';
            const plugins = [{
                name: 'keyed-rows-sfc',
                transform(source, id) {
                    if (!id.endsWith('/KeyedRows.vue')) return;
                    const { descriptor, errors } = parse(source, { filename: id });
                    if (errors.length) throw new Error(errors.join('\n'));
                    const compiled = compileScript(descriptor, { id: 'keyed-rows', inlineTemplate: true, isProd: true });
                    return { code: compiled.content, map: compiled.map };
                }
            }];
            """;
        await File.WriteAllTextAsync(Path.Combine(project, "vite.config.js"), $$"""
            import config from './vite.shared.config.js';
            {{sfcCompiler}}
            export default { ...config, base: './'{{(jolt ? "" : ", plugins")}} };
            """, token);
        if (apphost is not null)
        {
            await RunAsync(apphost, project, "bundle", ".");
        }
        else
        {
            await NodeToolchain.RestoreAsync(project, store, token, Console.Out);
            await NodeToolchain.BuildAsync(project, ssr: false, token, Console.Out);
        }
    }

    var outputs = variantsToBuild.ToDictionary(variant => variant.Name, variant => Path.Combine(root.FullName, variant.Name, "dist"));
    var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { ContentRootPath = root.FullName });
    builder.Logging.ClearProviders();
    builder.WebHost.UseUrls("http://127.0.0.1:0");
    await using var app = builder.Build();
    foreach (var (name, output) in outputs)
    {
        foreach (var file in Directory.EnumerateFiles(output, "*", SearchOption.AllDirectories))
        {
            var contentType = Path.GetExtension(file) switch
            {
                ".html" => "text/html",
                ".js" => "text/javascript",
                ".css" => "text/css",
                _ => "application/octet-stream",
            };
            var relative = Path.GetRelativePath(output, file).Replace('\\', '/');
            app.MapGet($"/{name}/{(relative == "index.html" ? "" : relative)}", () => Results.File(file, contentType));
        }
    }
    await app.StartAsync(token);
    var origin = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    var profile = root.CreateSubdirectory("chrome");
    using var browser = Start(chromium, root.FullName, "--headless", "--disable-gpu",
        "--remote-debugging-port=0", $"--user-data-dir={profile.FullName}", "about:blank");
    string browserVersion;
    var results = new List<JsonElement>[variantsToBuild.Count];
    try
    {
        var portFile = Path.Combine(profile.FullName, "DevToolsActivePort");
        while (!File.Exists(portFile))
        {
            if (browser.HasExited)
                throw new InvalidOperationException($"Chromium exited with {browser.ExitCode}.");
            await Task.Delay(25, token);
        }
        var port = (await File.ReadAllLinesAsync(portFile, token))[0];
        using var http = new HttpClient();
        using var version = JsonDocument.Parse(await http.GetStringAsync($"http://127.0.0.1:{port}/json/version", token));
        browserVersion = version.RootElement.GetProperty("Browser").GetString()!;
        using var targets = JsonDocument.Parse(await http.GetStringAsync($"http://127.0.0.1:{port}/json/list", token));
        var page = targets.RootElement.EnumerateArray().First(target => target.GetProperty("type").GetString() == "page");
        using var cdp = new ClientWebSocket();
        await cdp.ConnectAsync(new Uri(page.GetProperty("webSocketDebuggerUrl").GetString()!), token);
        var command = 0;
        for (var variant = 0; variant < results.Length; variant++)
            results[variant] = [];
        Console.WriteLine($"Measuring on {browserVersion} with {warmup} warm-up and {cycles} measured cycles.");
        for (var cycle = -warmup; cycle < cycles; cycle++)
        {
            // Rotate every ordering so each build visits each slot equally often.
            foreach (var variant in orders[(cycle + warmup) % orders.Length])
            {
                var url = $"{origin}/{variantsToBuild[variant].Name}/?cycle={cycle}";
                await SendAsync("Page.navigate", new { url });
                var condition = $"location.href === {JsonSerializer.Serialize(url)} && typeof window.measureKeyedRows === 'function'";
                while (!(await EvaluateAsync(condition)).GetBoolean())
                    await Task.Delay(10, token);
                var measured = await EvaluateAsync("window.measureKeyedRows()");
                if (cycle >= 0)
                    results[variant].Add(measured);
            }
        }

        async Task<JsonElement> EvaluateAsync(string expression)
        {
            var result = await SendAsync("Runtime.evaluate", new { expression, awaitPromise = true, returnByValue = true });
            if (result.TryGetProperty("exceptionDetails", out var exception))
                throw new InvalidOperationException(exception.GetRawText());
            return result.GetProperty("result").GetProperty("value");
        }

        async Task<JsonElement> SendAsync(string method, object parameters)
        {
            var id = ++command;
            var request = JsonSerializer.SerializeToUtf8Bytes(new { id, method, @params = parameters });
            await cdp.SendAsync(request.AsMemory(), WebSocketMessageType.Text, true, token);
            var buffer = new byte[8192];
            while (true)
            {
                using var message = new MemoryStream();
                ValueWebSocketReceiveResult part;
                do
                {
                    part = await cdp.ReceiveAsync(buffer.AsMemory(), token);
                    if (part.MessageType == WebSocketMessageType.Close)
                        throw new InvalidOperationException("Chromium closed DevTools.");
                    message.Write(buffer, 0, part.Count);
                } while (!part.EndOfMessage);
                using var reply = JsonDocument.Parse(message.GetBuffer().AsMemory(0, (int)message.Length));
                if (!reply.RootElement.TryGetProperty("id", out var replyId) || replyId.GetInt32() != id)
                    continue;
                if (reply.RootElement.TryGetProperty("error", out var error))
                    throw new InvalidOperationException(error.GetRawText());
                return reply.RootElement.GetProperty("result").Clone();
            }
        }
    }
    finally
    {
        if (!browser.HasExited)
            browser.Kill(entireProcessTree: true);
        await browser.WaitForExitAsync();
    }

    using var revision = Start("git", repo, "rev-parse", "HEAD");
    var variants = variantsToBuild.Select((build, index) => new
    {
        name = build.Name,
        cli = build.Cli is { } apphost ? new
        {
            apphost,
            apphostSha256 = FileSha256(apphost),
            cliSha256 = FileSha256(Path.Combine(Path.GetDirectoryName(apphost)!, "Jolt.Cli.dll")),
            compilerSha256 = FileSha256(Path.Combine(Path.GetDirectoryName(apphost)!, "Jolt.Compiler.dll"))
        } : null,
        jsBytes = Files(outputs[build.Name]).Sum(file => new FileInfo(file).Length),
        gzipBytes = Files(outputs[build.Name]).Sum(GzipBytes),
        jsSha256 = Files(outputs[build.Name]).Order(StringComparer.Ordinal).ToDictionary(
            file => Path.GetRelativePath(outputs[build.Name], file).Replace('\\', '/'), FileSha256),
        timings = steps.ToDictionary(step => step, step =>
        {
            var timings = results[index].Select(result => result.GetProperty(step).GetProperty("time").GetDouble()).Order().ToArray();
            return new { p50 = Percentile(timings, 0.50), p95 = Percentile(timings, 0.95),
                mutatedRows = results[index][0].GetProperty(step).GetProperty("mutated").GetInt32() };
        }),
        samples = results[index]
    }).ToArray();
    var metadata = new
    {
        date = DateTimeOffset.UtcNow,
        revision = (await revision.StandardOutput.ReadToEndAsync(token)).Trim(),
        platform = RuntimeInformation.OSDescription,
        architecture = RuntimeInformation.ProcessArchitecture.ToString(),
        logicalProcessors = Environment.ProcessorCount,
        processor = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"),
        runtime = RuntimeInformation.FrameworkDescription,
        browserVersion,
        lockSha256 = FileSha256(Path.Combine(sample, "pnpm-lock.yaml")),
        fixtureSha256 = Directory.GetFiles(fixture).Order(StringComparer.Ordinal).ToDictionary(file => Path.GetFileName(file)!, FileSha256),
        order = orders.Select(order => order.Select(index => variantsToBuild[index].Name)),
        warmup,
        cycles,
        variants,
    };
    await File.WriteAllTextAsync(report, JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }), token);
    foreach (var variant in variants)
    {
        Console.WriteLine($"{variant.name}: {variant.jsBytes} JS bytes, {variant.gzipBytes} gzip bytes");
        foreach (var (step, timing) in variant.timings)
            Console.WriteLine(FormattableString.Invariant($"{step}: p50 {timing.p50:F2} ms, p95 {timing.p95:F2} ms, {timing.mutatedRows} rows"));
    }
}
finally
{
    root.Delete(recursive: true);
}

async Task RunAsync(string file, string workingDirectory, params string[] arguments)
{
    var start = CreateStartInfo(file, workingDirectory, arguments);
    start.Environment["JazorPnpmStoreDirectory"] = store;
    using var process = Process.Start(start) ?? throw new InvalidOperationException($"Could not launch {file}.");
    var output = process.StandardOutput.ReadToEndAsync(token);
    try
    {
        await process.WaitForExitAsync(token);
        Console.Write(await output);
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"{file} returned {process.ExitCode}.");
    }
    finally
    {
        if (!process.HasExited)
            process.Kill(entireProcessTree: true);
    }
}

static Process Start(string file, string workingDirectory, params string[] arguments)
    => Process.Start(CreateStartInfo(file, workingDirectory, arguments)) ?? throw new InvalidOperationException($"Could not launch {file}.");

static ProcessStartInfo CreateStartInfo(string file, string workingDirectory, string[] arguments)
{
    var start = new ProcessStartInfo(file) { WorkingDirectory = workingDirectory, RedirectStandardOutput = true, UseShellExecute = false };
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
    return start;
}

static IEnumerable<string> Files(string directory) => Directory.EnumerateFiles(directory, "*.js", SearchOption.AllDirectories);
static string FileSha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

static long GzipBytes(string path)
{
    using var compressed = new MemoryStream();
    using (var gzip = new GZipStream(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
    using (var source = File.OpenRead(path))
        source.CopyTo(gzip);
    return compressed.Length;
}

static double Percentile(double[] ordered, double quantile) => ordered[(int)Math.Ceiling(quantile * ordered.Length) - 1];

static string ScriptPath([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;
