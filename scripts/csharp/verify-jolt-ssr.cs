#!/usr/bin/env dotnet run
#:property PublishAot=false

using System.Diagnostics;
using System.Net;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

var benchmark = args is [_, _, "--benchmark"];
if (args.Length != 2 && !benchmark)
    throw new ArgumentException("Usage: verify-jolt-ssr.cs <Jolt CLI apphost> <Chromium executable> [--benchmark]");

var cli = Path.GetFullPath(args[0]);
var browser = Path.GetFullPath(args[1]);
var repo = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ScriptPath())!, "..", ".."));
var sample = Path.Combine(repo, "samples", "Jolt.Mixed.Ssr");
var browserCheck = Path.Combine(repo, "samples", "Jolt.Counter", "verify-browser.cs");
var store = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.Combine(repo, ".tmp", "nodeapi-store");
using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(4));
var stop = deadline.Token;
var work = Directory.CreateDirectory(Path.Combine(repo, ".tmp", $"jolt-ssr-{Guid.NewGuid():N}"));
try
{
    CopyInputs(sample, work.FullName);
    if (benchmark)
    {
        var entry = Path.Combine(work.FullName, "frontend", "entry-client.ts");
        var source = await File.ReadAllTextAsync(entry, stop);
        const string mount = "app.mount(container);";
        Require(source.Contains(mount, StringComparison.Ordinal), "Missing client hydration mount for benchmark marks.");
        await File.WriteAllTextAsync(entry, source.Replace(mount, """
            performance.mark('jolt-hydration-start');
            app.mount(container);
            performance.mark('jolt-hydration-end');
            """, StringComparison.Ordinal), stop);
    }
    await File.WriteAllTextAsync(Path.Combine(work.FullName, "frontend", "vite.config.js"), """
        import { appendFileSync } from 'node:fs';
        import { defineConfig } from 'vite';
        export default defineConfig({
            plugins: [{ name: 'fixture-build-count', buildStart() {
                appendFileSync(new URL('../vite-builds.log', import.meta.url), this.environment.name + '\n');
            } }]
        });
        """, stop);
    await Run(cli, "bundle", "frontend", "--ssr", "entry-server.ts");

    var published = Path.Combine(work.FullName, "published");
    await Run("dotnet", "publish", "Jolt.Mixed.Ssr.csproj", "--configuration", benchmark ? "Release" : "Debug", "--output", published);
    var builds = await File.ReadAllLinesAsync(Path.Combine(work.FullName, "vite-builds.log"), stop);
    Require(builds.SequenceEqual(["client", "ssr"]), $"Expected one client and one server build. Observed: {string.Join(", ", builds)}.");
    Console.WriteLine("Vite built the SSR client and server once each across Jolt bundle and .NET publish.");
    var host = Path.Combine(published, OperatingSystem.IsWindows() ? "Jolt.Mixed.Ssr.exe" : "Jolt.Mixed.Ssr");
    var origin = new TaskCompletionSource<Uri>(TaskCreationOptions.RunContinuationsAsynchronously);
    using var server = Start(host, ["--urls", "http://127.0.0.1:0"]);
    try
    {
        var output = Pump(server.StandardOutput, line =>
        {
            const string listening = "Now listening on: ";
            var offset = line.IndexOf(listening, StringComparison.Ordinal);
            if (offset >= 0 && Uri.TryCreate(line[(offset + listening.Length)..], UriKind.Absolute, out var uri))
                origin.TrySetResult(uri);
        });
        var errors = Pump(server.StandardError);
        var finished = server.WaitForExitAsync(stop);
        if (await Task.WhenAny(origin.Task, finished) == finished)
        {
            await finished;
            await Task.WhenAll(output, errors);
            throw new InvalidOperationException($"SSR host exited with code {server.ExitCode} before starting.");
        }

        using var http = new HttpClient { BaseAddress = await origin.Task };
        var editor = new Uri(http.BaseAddress, "/editor?name=Alice").AbsoluteUri;
        if (benchmark)
        {
            const int warmups = 5;
            const int samples = 20;
            for (var i = -warmups; i < samples; i++)
            {
                var watch = Stopwatch.StartNew();
                using var response = await http.GetAsync(editor, HttpCompletionOption.ResponseHeadersRead, stop);
                var headers = watch.Elapsed.TotalMilliseconds;
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsByteArrayAsync(stop);
                var completed = watch.Elapsed.TotalMilliseconds;
                Require(body.Length > 0, "Empty benchmark response.");
                if (i >= 0)
                    Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"SSR_HTTP {i + 1} {headers:F3} {completed:F3}"));
            }
            await Run("dotnet", "run", "--file", browserCheck, "--", browser, editor, "--ssr-benchmark");
            return;
        }
        var first = ReadEditor("Alice", "Bob");
        var second = ReadEditor("Bob", "Alice");
        await Task.WhenAll(first, second);

        var rendered = await first;
        foreach (var (tag, contentType) in new[]
        {
            ("""script\b(?=[^>]*\btype="module")[^>]+src""", "javascript"),
            ("""link\b(?=[^>]*\brel="stylesheet")[^>]+href""", "text/css"),
        })
        {
            var asset = Regex.Match(rendered, $"""<{tag}="(?<url>/assets/[^"]+)"[^>]*>""");
            Require(asset.Success, $"Missing production {contentType} reference in the SSR document.");
            using var response = await http.GetAsync(asset.Groups["url"].Value, stop);
            Require(response.IsSuccessStatusCode &&
                response.Content.Headers.ContentType?.MediaType?.Contains(contentType, StringComparison.Ordinal) == true &&
                (await response.Content.ReadAsByteArrayAsync(stop)).Length > 0,
                $"Expected served {contentType} asset at {asset.Groups["url"].Value}.");
        }

        using (var failure = await http.GetAsync("/editor?fail=1", stop))
        {
            Require(failure.StatusCode == HttpStatusCode.InternalServerError, "Rejected SSR prefetch must return HTTP 500.");
            var body = await failure.Content.ReadAsStringAsync(stop);
            Require(!body.Contains("id=\"app\"", StringComparison.Ordinal), "Failed SSR returned an application document.");
        }
        await ReadEditor("Alice", "Bob");
        var generatedRoute = await http.GetStringAsync("/drafts/42?name=Alice", stop);
        Require(HasText(generatedRoute, "route-heading", "Draft details") && HasText(generatedRoute, "draft-number", "43") &&
            generatedRoute.Contains("id=\"draft-layout\"", StringComparison.Ordinal),
            "The generated parameter route did not render typed draft details inside its layout.");
        Console.WriteLine("Verified isolated concurrent SSR responses, state, teleport anchors, assets, and prefetch error handling.");

        await Run("dotnet", "run", "--file", browserCheck, "--", browser, editor, "--ssr");

        async Task<string> ReadEditor(string name, string other)
        {
            using var response = await http.GetAsync($"/editor?name={name}", stop);
            var body = await response.Content.ReadAsStringAsync(stop);
            Require(response.IsSuccessStatusCode &&
                HasText(body, "route-heading", "Editor") && HasText(body, "request-name", name) &&
                HasText(body, "prefetch-count", "1") && HasText(body, "counter", "1") &&
                !body.Contains(other, StringComparison.Ordinal),
                $"Expected isolated server-rendered state for {name}, HTTP {(int)response.StatusCode}.\n{body}");

            var state = Regex.Match(body, """<script id="__jazor_ssr_state" type="application/json">(?<state>.*?)</script>""", RegexOptions.Singleline);
            Require(state.Success, "Missing serialized application state in the SSR document.");
            using var stateJson = JsonDocument.Parse(state.Groups["state"].Value);
            var serialized = stateJson.RootElement;
            var provider = serialized.GetProperty("providers").EnumerateArray().Single();
            var workspace = serialized.GetProperty("props").GetProperty("pinia").GetProperty("workspace").GetProperty("state");
            Require(provider.GetProperty("key").GetString() == "workspace:request-name" &&
                provider.GetProperty("value").GetString() == name &&
                workspace.GetProperty("prefetchCount").GetInt32() == 1 && workspace.GetProperty("count").GetInt32() == 1,
                $"Serialized state does not contain the request seed and prefetched editor values for {name}.");
            Require(Regex.IsMatch(body, """id="overlays"[^>]*>\s*<!--teleport start anchor-->.*?id="teleported-notice".*?<!--teleport anchor-->""", RegexOptions.Singleline),
                "The SSR overlay is missing its teleported content or Vue anchors.");
            return body;
        }
    }
    finally
    {
        await Stop(server);
    }
}
finally
{
    work.Delete(recursive: true);
}

Process Start(string file, string[] arguments)
{
    var start = new ProcessStartInfo(file)
    {
        WorkingDirectory = work.FullName,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
    };
    start.Environment["JazorPnpmStoreDirectory"] = store;
    start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
    start.Environment["DOTNET_ENVIRONMENT"] = "Production";
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
    return Process.Start(start) ?? throw new InvalidOperationException($"Cannot start {file}.");
}

async Task Run(string file, params string[] arguments)
{
    using var process = Start(file, arguments);
    var output = Pump(process.StandardOutput);
    var errors = Pump(process.StandardError);
    try
    {
        await process.WaitForExitAsync(stop);
        await Task.WhenAll(output, errors);
        Require(process.ExitCode == 0, $"{file} {string.Join(' ', arguments)} exited with code {process.ExitCode}.");
    }
    finally
    {
        await Stop(process);
    }
}

async Task Pump(StreamReader reader, Action<string>? observe = null)
{
    while (await reader.ReadLineAsync(stop) is { } line)
    {
        Console.WriteLine(line);
        observe?.Invoke(line);
    }
}

static async Task Stop(Process process)
{
    if (!process.HasExited)
        process.Kill(entireProcessTree: true);
    await process.WaitForExitAsync();
}

static void CopyInputs(string source, string destination)
{
    foreach (var file in Directory.EnumerateFiles(source))
        File.Copy(file, Path.Combine(destination, Path.GetFileName(file)));
    foreach (var child in Directory.EnumerateDirectories(source))
    {
        var name = Path.GetFileName(child);
        if (name is "bin" or "obj" or "dist" or "node_modules" or ".jazor" or ".jolt" or ".vite")
            continue;
        var directory = Directory.CreateDirectory(Path.Combine(destination, name));
        CopyInputs(child, directory.FullName);
    }
}

static bool HasText(string html, string id, string text) =>
    Regex.IsMatch(html, $"""id="{id}"[^>]*>\s*{Regex.Escape(text)}\s*</""");

static void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

static string ScriptPath([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;
