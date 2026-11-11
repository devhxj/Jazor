#!/usr/bin/env dotnet run
#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj
#:sdk Microsoft.NET.Sdk.Web
#:project ../../src/Jazor.AspNetCore/Jazor.AspNetCore.csproj

using Jazor.NodeHost;
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
ConfigureRepositoryEnvironment(repoRoot);
var outputRoot = Path.GetFullPath(
    options.OutputDirectory ?? Path.Combine(repoRoot, ".tmp", "razorvue-ssr-benchmark"));
var workspace = Path.Combine(outputRoot, "workspace-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(outputRoot);

try
{
    var artifactRoot = await MaterializeArtifactGraphAsync(repoRoot, workspace);
    var coldSamples = new List<double>(options.Samples);
    var coldEnvironmentIds = new List<string>(options.Samples);
    for (var index = 0; index < options.Samples; index++)
    {
        await using var app = CreateApplication(workspace, artifactRoot, options.WorkerCount);
        var renderer = app.Services.GetRequiredService<IJazorSsrRenderer>();
        var started = Stopwatch.GetTimestamp();
        var result = await renderer.RenderAsync(
            new JazorSsrRequest("components/benchmark.mjs", new { Sequence = index }));
        coldSamples.Add(Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        coldEnvironmentIds.Add(ParseEnvironmentId(result.Html));
    }

    await using var warmApp = CreateApplication(workspace, artifactRoot, options.WorkerCount);
    var warmRenderer = warmApp.Services.GetRequiredService<IJazorSsrRenderer>();
    var warmup = await warmRenderer.RenderAsync(
        new JazorSsrRequest("components/benchmark.mjs", new { Sequence = -1 }));
    var warmupEnvironmentId = ParseEnvironmentId(warmup.Html);
    var warmSamples = new List<double>(options.Iterations);
    var warmEnvironmentIds = new HashSet<string>();
    for (var index = 0; index < options.Iterations; index++)
    {
        var started = Stopwatch.GetTimestamp();
        var result = await warmRenderer.RenderAsync(
            new JazorSsrRequest("components/benchmark.mjs", new { Sequence = index }));
        warmSamples.Add(Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        warmEnvironmentIds.Add(ParseEnvironmentId(result.Html));
    }

    var concurrentStarted = Stopwatch.GetTimestamp();
    var concurrentResults = await Task.WhenAll(
        Enumerable.Range(0, options.ConcurrentRequests)
            .Select(index => warmRenderer.RenderAsync(
                new JazorSsrRequest("components/benchmark.mjs", new { Sequence = index }))));
    var concurrentElapsed = Stopwatch.GetElapsedTime(concurrentStarted);
    var concurrentEnvironmentIds = concurrentResults
        .Select(static result => ParseEnvironmentId(result.Html))
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
        warmupEnvironmentId,
        warmEnvironmentIds.Order().ToArray(),
        concurrentEnvironmentIds,
        coldEnvironmentIds,
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
          "main": "./entry.js",
          "exports": {
            ".": "./entry.js",
            "./ssr": "./ssr-entry.js"
          },
          "dependencies": {
            "vue": "3.5.42",
            "@vue/server-renderer": "3.5.42"
          }
        }
        """.Replace("\r\n", "\n", StringComparison.Ordinal));
    WriteText(Path.Combine(artifactRoot, "entry.js"), "export {};\n");
    File.Copy(
        Path.Combine(repoRoot, "src", "Jazor.Emit", "tooling", "ssr-runner.js"),
        Path.Combine(artifactRoot, "ssr-entry.js"));
    WriteText(
        Path.Combine(artifactRoot, "components", "benchmark.mjs"),
        """
        import { defineComponent, h } from "vue";
        import { randomUUID } from "node:crypto";
        const environmentId = randomUUID();

        export default defineComponent({
          props: ["Sequence"],
          setup(props) {
            return () => h("main", null, `${environmentId}|${props.Sequence}`);
          }
        });
        """);
    await NodeToolchain.RestoreAsync(artifactRoot, Path.Combine(repoRoot, ".tmp", "nodeapi-store"),
        CancellationToken.None, Console.Out);
    if (!File.Exists(Path.Combine(artifactRoot, "pnpm-lock.yaml")) ||
        !File.Exists(Path.Combine(artifactRoot, "node_modules", "vue", "package.json")) ||
        !File.Exists(Path.Combine(artifactRoot, "node_modules", "@vue", "server-renderer", "package.json")))
    {
        throw new InvalidOperationException("SSR benchmark did not produce the frozen package graph.");
    }

    return artifactRoot;
}

static string ParseEnvironmentId(string html)
{
    const string prefix = "<main>";
    const string suffix = "</main>";
    if (!html.StartsWith(prefix, StringComparison.Ordinal) ||
        !html.EndsWith(suffix, StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Unexpected SSR benchmark payload: " + html);
    }

    var separator = html.IndexOf('|', prefix.Length);
    if (separator < 0)
    {
        throw new InvalidOperationException("SSR benchmark payload has no environment id: " + html);
    }

    return html[prefix.Length..separator];
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

static void ConfigureRepositoryEnvironment(string repoRoot)
{
    var temp = Path.Combine(repoRoot, ".tmp", "agent-temp");
    var dotnet = Path.Combine(repoRoot, ".dotnet");
    var nuget = Path.Combine(dotnet, ".nuget", "packages") + Path.DirectorySeparatorChar;
    var nugetHttp = Path.Combine(repoRoot, ".tmp", "nuget-http-cache");
    var npm = Path.Combine(repoRoot, ".tmp", "npm-cache");
    foreach (var directory in new[] { temp, dotnet, nuget, nugetHttp, npm })
        Directory.CreateDirectory(directory);

    Environment.SetEnvironmentVariable("TEMP", temp);
    Environment.SetEnvironmentVariable("TMP", temp);
    Environment.SetEnvironmentVariable("DOTNET_CLI_HOME", dotnet);
    Environment.SetEnvironmentVariable("NUGET_PACKAGES", nuget);
    Environment.SetEnvironmentVariable("NUGET_HTTP_CACHE_PATH", nugetHttp);
    Environment.SetEnvironmentVariable("JazorPnpmStoreDirectory", Path.Combine(repoRoot, ".tmp", "nodeapi-store"));
    Environment.SetEnvironmentVariable("NPM_CONFIG_CACHE", npm);
    Environment.SetEnvironmentVariable("npm_config_cache", npm);
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
    string WarmupEnvironmentId,
    IReadOnlyList<string> WarmEnvironmentIds,
    IReadOnlyList<string> ConcurrentEnvironmentIds,
    IReadOnlyList<string> ColdEnvironmentIds,
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
            - Warm environment ids: `{string.Join(",", WarmEnvironmentIds)}` (warmup `{WarmupEnvironmentId}`)
            - Concurrent environment ids: `{string.Join(",", ConcurrentEnvironmentIds)}`
            - Per-request temporary JSON files: {RequestTempFileCount}

            Cold creates and disposes a real ASP.NET Core renderer for each sample. Warm and concurrent lanes reuse the production `IJazorSsrRenderer`; the fixture uses the in-process NodeApi runtime and production Vue/server-renderer assets.
            """;
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SsrBenchmarkReport))]
internal sealed partial class BenchmarkJsonContext : JsonSerializerContext;

internal sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
