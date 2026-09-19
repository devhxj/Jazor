#!/usr/bin/env dotnet run
#:sdk Microsoft.NET.Sdk.Web
#:project ../../src/Jazor.AspNetCore/Jazor.AspNetCore.csproj

using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jazor.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var options = BenchmarkOptions.Parse(args);
if (options.ShowHelp)
    return;

var repoRoot = RequireRepositoryRoot();
var outputRoot = Path.GetFullPath(
    options.OutputDirectory ?? Path.Combine(repoRoot, ".tmp", "razorvue-ssr-benchmark"));
var workspace = Path.Combine(outputRoot, "workspace-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(outputRoot);

try
{
    var artifactRoot = await MaterializeArtifactGraphAsync(repoRoot, workspace);
    var coldSamples = new List<double>(options.Samples);
    var coldProcessIds = new List<int>(options.Samples);
    for (var index = 0; index < options.Samples; index++)
    {
        await using var app = CreateApplication(workspace, artifactRoot, options.WorkerCount);
        var renderer = app.Services.GetRequiredService<IJazorSsrRenderer>();
        var started = Stopwatch.GetTimestamp();
        var result = await renderer.RenderAsync(
            new JazorSsrRequest("components/benchmark.mjs", new { Sequence = index }));
        coldSamples.Add(Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        coldProcessIds.Add(ParseProcessId(result.Html));
    }

    await using var warmApp = CreateApplication(workspace, artifactRoot, options.WorkerCount);
    var warmRenderer = warmApp.Services.GetRequiredService<IJazorSsrRenderer>();
    var warmup = await warmRenderer.RenderAsync(
        new JazorSsrRequest("components/benchmark.mjs", new { Sequence = -1 }));
    var warmupProcessId = ParseProcessId(warmup.Html);
    var warmSamples = new List<double>(options.Iterations);
    var warmProcessIds = new HashSet<int>();
    for (var index = 0; index < options.Iterations; index++)
    {
        var started = Stopwatch.GetTimestamp();
        var result = await warmRenderer.RenderAsync(
            new JazorSsrRequest("components/benchmark.mjs", new { Sequence = index }));
        warmSamples.Add(Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        warmProcessIds.Add(ParseProcessId(result.Html));
    }

    var concurrentStarted = Stopwatch.GetTimestamp();
    var concurrentResults = await Task.WhenAll(
        Enumerable.Range(0, options.ConcurrentRequests)
            .Select(index => warmRenderer.RenderAsync(
                new JazorSsrRequest("components/benchmark.mjs", new { Sequence = index }))));
    var concurrentElapsed = Stopwatch.GetElapsedTime(concurrentStarted);
    var concurrentProcessIds = concurrentResults
        .Select(static result => ParseProcessId(result.Html))
        .Distinct()
        .Order()
        .ToArray();

    var report = new SsrBenchmarkReport(
        "razorvue-ssr-worker-pool-v1",
        "measured",
        DateTimeOffset.UtcNow,
        Environment.MachineName,
        Environment.OSVersion.ToString(),
        Environment.Version.ToString(),
        options.WorkerCount,
        options.Samples,
        options.Iterations,
        options.ConcurrentRequests,
        Summarize(coldSamples),
        Summarize(warmSamples),
        coldSamples.Count == 0 || warmSamples.Count == 0
            ? 0
            : Median(coldSamples) / Math.Max(Median(warmSamples), 0.000001),
        options.ConcurrentRequests / Math.Max(concurrentElapsed.TotalSeconds, 0.000001),
        concurrentElapsed.TotalMilliseconds,
        warmupProcessId,
        warmProcessIds.Order().ToArray(),
        concurrentProcessIds,
        coldProcessIds,
        Directory.GetFiles(artifactRoot, "ssr-request-*.json", SearchOption.AllDirectories).Length);

    WriteText(
        Path.Combine(outputRoot, "razorvue-ssr-benchmark.json"),
        JsonSerializer.Serialize(report, BenchmarkJsonContext.Default.SsrBenchmarkReport));
    WriteText(Path.Combine(outputRoot, "razorvue-ssr-benchmark.md"), report.ToMarkdown());
    Console.WriteLine(report.ToMarkdown());
}
finally
{
    if (Directory.Exists(workspace))
        Directory.Delete(workspace, recursive: true);
}

static WebApplication CreateApplication(string contentRoot, string artifactRoot, int workerCount)
{
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        ContentRootPath = contentRoot,
        WebRootPath = Path.Combine(contentRoot, "wwwroot"),
        EnvironmentName = Environments.Production
    });
    builder.Services.AddJazorSsr(options =>
    {
        options.ArtifactRootPath = artifactRoot;
        options.WorkerCount = workerCount;
    });
    return builder.Build();
}

static async Task<string> MaterializeArtifactGraphAsync(string repoRoot, string workspace)
{
    var artifactRoot = Path.Combine(workspace, "jazor");
    WriteText(
        Path.Combine(artifactRoot, "package.json"),
        """
        {
          "name": "@jazor/ssr-benchmark",
          "private": true,
          "type": "module",
          "dependencies": {
            "vue": "3.5.42",
            "@vue/server-renderer": "3.5.42"
          }
        }
        """.Replace("\r\n", "\n", StringComparison.Ordinal));
    // Bare package imports resolve through the restored package.json exports map.
    WriteText(Path.Combine(artifactRoot, "importmap.json"), "{\"imports\":{}}\n");
    WriteText(Path.Combine(artifactRoot, "ssr-importmap.json"), "{\"imports\":{}}\n");
    WriteText(Path.Combine(artifactRoot, "manifest.json"), "{\"styles\":[]}");
    WriteText(Path.Combine(artifactRoot, "jazor-manifest.json"), "{\"generation\":\"benchmark-v1\"}");
    WriteText(
        Path.Combine(artifactRoot, "components", "benchmark.mjs"),
        """
        import { defineComponent, h } from "vue";

        export default defineComponent({
          props: ["Sequence"],
          setup(props) {
            return () => h("main", null, `${Deno.pid}|${props.Sequence}`);
          }
        });
        """);
    var deno = ResolveDenoExecutable(repoRoot);
    var install = await RunProcessAsync(
        deno,
        ["install", "--package-json", "--node-modules-dir=manual", "--node-modules-linker=hoisted", "--frozen=false"],
        artifactRoot);
    if (install.ExitCode != 0)
        throw new InvalidOperationException("SSR benchmark Deno restore failed." + Environment.NewLine + install.StandardError);

    var check = await RunProcessAsync(
        deno,
        ["check", "--node-modules-dir=manual", "--no-remote", "--frozen-lockfile", "components/benchmark.mjs"],
        artifactRoot);
    if (check.ExitCode != 0)
        throw new InvalidOperationException("SSR benchmark Deno offline check failed." + Environment.NewLine + check.StandardError);

    if (!File.Exists(Path.Combine(artifactRoot, "deno.lock")) ||
        !File.Exists(Path.Combine(artifactRoot, "node_modules", "vue", "package.json")) ||
        !File.Exists(Path.Combine(artifactRoot, "node_modules", "@vue", "server-renderer", "package.json")))
    {
        throw new InvalidOperationException("SSR benchmark did not produce the frozen package graph.");
    }

    return artifactRoot;
}

static string ResolveDenoExecutable(string repoRoot)
{
    var explicitPath = Environment.GetEnvironmentVariable("JAZOR_DENO_PATH");
    if (!string.IsNullOrWhiteSpace(explicitPath) && File.Exists(explicitPath))
        return Path.GetFullPath(explicitPath);

    var runtimeName = OperatingSystem.IsWindows() ? "deno.exe" : "deno";
    var rid = OperatingSystem.IsWindows()
        ? "win-x64"
        : OperatingSystem.IsMacOS()
            ? "osx-x64"
            : "linux-x64";
    foreach (var project in new[] { "Jazor.EmitTest", "Jazor.AspNetCore" })
    {
        var candidate = Path.Combine(repoRoot, "src", project, "bin", "Debug", "net11.0", "runtimes", rid, "native", runtimeName);
        if (File.Exists(candidate))
            return candidate;
    }

    return runtimeName;
}

static async Task<ProcessResult> RunProcessAsync(
    string fileName,
    IReadOnlyList<string> arguments,
    string workingDirectory)
{
    using var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        }
    };
    foreach (var argument in arguments)
        process.StartInfo.ArgumentList.Add(argument);
    process.Start();
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    return new ProcessResult(process.ExitCode, await stdout, await stderr);
}

static int ParseProcessId(string html)
{
    const string prefix = "<main>";
    const string suffix = "</main>";
    if (!html.StartsWith(prefix, StringComparison.Ordinal) ||
        !html.EndsWith(suffix, StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Unexpected SSR benchmark payload: " + html);
    }

    var separator = html.IndexOf('|', prefix.Length);
    if (separator < 0 ||
        !int.TryParse(html.AsSpan(prefix.Length, separator - prefix.Length), out var processId))
    {
        throw new InvalidOperationException("SSR benchmark payload has no Deno process id: " + html);
    }

    return processId;
}

static MetricSummary Summarize(IReadOnlyList<double> samples)
{
    var ordered = samples.Order().ToArray();
    if (ordered.Length == 0)
        return new MetricSummary(0, 0, 0, 0, []);
    return new MetricSummary(
        Percentile(ordered, 0.5),
        Percentile(ordered, 0.95),
        ordered[0],
        ordered[^1],
        ordered);
}

static double Median(IReadOnlyList<double> samples)
    => Percentile(samples.Order().ToArray(), 0.5);

static double Percentile(IReadOnlyList<double> ordered, double percentile)
{
    if (ordered.Count == 0)
        return 0;
    var rank = Math.Clamp((int)Math.Ceiling(percentile * ordered.Count) - 1, 0, ordered.Count - 1);
    return ordered[rank];
}

static string RequireRepositoryRoot()
{
    for (var directory = new DirectoryInfo(Environment.CurrentDirectory);
         directory is not null;
         directory = directory.Parent)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;
    }

    throw new DirectoryNotFoundException("Could not locate the Jazor repository root.");
}

static void WriteText(string path, string content)
{
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, content, new UTF8Encoding(false));
}

internal sealed record BenchmarkOptions(
    bool ShowHelp,
    string? OutputDirectory,
    int Samples,
    int Iterations,
    int ConcurrentRequests,
    int WorkerCount)
{
    public static BenchmarkOptions Parse(string[] args)
    {
        var result = new BenchmarkOptions(false, null, 5, 50, 20, Math.Clamp(Environment.ProcessorCount, 1, 4));
        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--out": result = result with { OutputDirectory = Next(args, ref index, "--out") }; break;
                case "--samples": result = result with { Samples = PositiveInt(Next(args, ref index, "--samples"), "--samples") }; break;
                case "--iterations": result = result with { Iterations = PositiveInt(Next(args, ref index, "--iterations"), "--iterations") }; break;
                case "--concurrent-requests": result = result with { ConcurrentRequests = PositiveInt(Next(args, ref index, "--concurrent-requests"), "--concurrent-requests") }; break;
                case "--workers": result = result with { WorkerCount = PositiveInt(Next(args, ref index, "--workers"), "--workers") }; break;
                case "--help":
                    Console.WriteLine("Usage: dotnet run --file scripts/csharp/benchmark-razorvue-ssr.cs -- [--out DIR] [--samples N] [--iterations N] [--concurrent-requests N] [--workers N]");
                    return result with { ShowHelp = true };
                default: throw new InvalidOperationException("Unknown benchmark argument: " + args[index]);
            }
        }
        return result;
    }

    private static string Next(string[] args, ref int index, string name)
        => ++index < args.Length
            ? args[index]
            : throw new InvalidOperationException("Missing value for " + name);

    private static int PositiveInt(string value, string name)
        => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) && parsed > 0
            ? parsed
            : throw new InvalidOperationException(name + " must be a positive integer.");
}

internal sealed record MetricSummary(
    double MedianMilliseconds,
    double P95Milliseconds,
    double MinMilliseconds,
    double MaxMilliseconds,
    IReadOnlyList<double> Samples);

internal sealed record ImportMapDocument(
    [property: JsonPropertyName("imports")] IReadOnlyDictionary<string, string> Imports);

internal sealed record SsrBenchmarkReport(
    string SchemaVersion,
    string Status,
    DateTimeOffset MeasuredAtUtc,
    string Machine,
    string OperatingSystem,
    string DotNetVersion,
    int WorkerCount,
    int ColdSampleCount,
    int WarmIterationCount,
    int ConcurrentRequestCount,
    MetricSummary Cold,
    MetricSummary Warm,
    double ColdToWarmMedianRatio,
    double ConcurrentRequestsPerSecond,
    double ConcurrentElapsedMilliseconds,
    int WarmupProcessId,
    IReadOnlyList<int> WarmProcessIds,
    IReadOnlyList<int> ConcurrentProcessIds,
    IReadOnlyList<int> ColdProcessIds,
    int RequestTempFileCount)
{
    public string ToMarkdown()
        => $"""
            # RazorVue Persistent SSR Benchmark

            - Schema: {SchemaVersion}
            - Status: {Status}
            - Machine: `{Machine}`
            - OS: `{OperatingSystem}`
            - .NET: `{DotNetVersion}`
            - Workers: {WorkerCount}
            - Cold samples: {ColdSampleCount}
            - Warm iterations: {WarmIterationCount}

            | Metric | Median | p95 | Min | Max |
            |---|---:|---:|---:|---:|
            | Cold render | {Cold.MedianMilliseconds:0.###} ms | {Cold.P95Milliseconds:0.###} ms | {Cold.MinMilliseconds:0.###} ms | {Cold.MaxMilliseconds:0.###} ms |
            | Warm render | {Warm.MedianMilliseconds:0.###} ms | {Warm.P95Milliseconds:0.###} ms | {Warm.MinMilliseconds:0.###} ms | {Warm.MaxMilliseconds:0.###} ms |

            - Cold/warm median ratio: {ColdToWarmMedianRatio:0.##}x
            - Concurrent lane: {ConcurrentRequestCount} requests in {ConcurrentElapsedMilliseconds:0.###} ms ({ConcurrentRequestsPerSecond:0.##} req/s)
            - Warm process ids: `{string.Join(",", WarmProcessIds)}` (warmup `{WarmupProcessId}`)
            - Concurrent process ids: `{string.Join(",", ConcurrentProcessIds)}`
            - Per-request temporary JSON files: {RequestTempFileCount}

            Cold creates and disposes a real ASP.NET Core renderer for each sample. Warm and concurrent lanes reuse the production `IJazorSsrRenderer`; the fixture uses the packaged DenoHost runtime and production Vue/server-renderer assets.
            """;
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ImportMapDocument))]
[JsonSerializable(typeof(SsrBenchmarkReport))]
internal sealed partial class BenchmarkJsonContext : JsonSerializerContext;

internal sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
