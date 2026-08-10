#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

var options = BenchmarkArguments.Parse(args);
var repoRoot = ScriptHelpers.RequireRepoRoot();
var protocol = BenchmarkProtocol.Create(repoRoot, options);

if (options.DryRun)
{
    Console.WriteLine(protocol.ToMarkdown());
    return;
}

var outputDirectory = options.OutputDirectory is { Length: > 0 }
    ? Path.GetFullPath(options.OutputDirectory)
    : Path.Combine(repoRoot, ".tmp", "razorvue-g2-benchmark");
Directory.CreateDirectory(outputDirectory);

var jsonPath = Path.Combine(outputDirectory, "razorvue-g2-benchmark-protocol.json");
var markdownPath = Path.Combine(outputDirectory, "razorvue-g2-benchmark-protocol.md");

File.WriteAllText(
    jsonPath,
    BenchmarkJson.Write(protocol));
File.WriteAllText(
    markdownPath,
    protocol.ToMarkdown());

Console.WriteLine("Wrote RazorVue G2 benchmark protocol:");
Console.WriteLine("  " + Path.GetRelativePath(repoRoot, jsonPath));
Console.WriteLine("  " + Path.GetRelativePath(repoRoot, markdownPath));

RuntimeBenchmarkReport? runtimeReport = null;
GeneratedArtifactBenchmarkReport? generatedArtifactReport = null;
BrowserBenchmarkReport? browserReport = null;

if (options.MeasureRuntime)
{
    runtimeReport = await RuntimeBenchmarkRunner.RunAsync(repoRoot, options);
    var runtimeJsonPath = Path.Combine(outputDirectory, "razorvue-g2-runtime-report.json");
    var runtimeMarkdownPath = Path.Combine(outputDirectory, "razorvue-g2-runtime-report.md");
    File.WriteAllText(runtimeJsonPath, RuntimeBenchmarkReportJson.Write(runtimeReport));
    File.WriteAllText(runtimeMarkdownPath, runtimeReport.ToMarkdown());

    Console.WriteLine("Wrote RazorVue G2 runtime benchmark report:");
    Console.WriteLine("  " + Path.GetRelativePath(repoRoot, runtimeJsonPath));
    Console.WriteLine("  " + Path.GetRelativePath(repoRoot, runtimeMarkdownPath));
}

if (options.MeasureGeneratedArtifacts)
{
    generatedArtifactReport = await GeneratedArtifactBenchmarkRunner.RunAsync(repoRoot, outputDirectory, options);
    var generatedJsonPath = Path.Combine(outputDirectory, "razorvue-g2-generated-artifacts-report.json");
    var generatedMarkdownPath = Path.Combine(outputDirectory, "razorvue-g2-generated-artifacts-report.md");
    File.WriteAllText(generatedJsonPath, GeneratedArtifactBenchmarkReportJson.Write(generatedArtifactReport));
    File.WriteAllText(generatedMarkdownPath, generatedArtifactReport.ToMarkdown());

    Console.WriteLine("Wrote RazorVue G2 generated artifact benchmark report:");
    Console.WriteLine("  " + Path.GetRelativePath(repoRoot, generatedJsonPath));
    Console.WriteLine("  " + Path.GetRelativePath(repoRoot, generatedMarkdownPath));
}

if (options.MeasureBrowser)
{
    browserReport = await BrowserBenchmarkRunner.RunAsync(repoRoot, options);
    var browserJsonPath = Path.Combine(outputDirectory, "razorvue-g2-browser-report.json");
    var browserMarkdownPath = Path.Combine(outputDirectory, "razorvue-g2-browser-report.md");
    File.WriteAllText(browserJsonPath, BrowserBenchmarkReportJson.Write(browserReport));
    File.WriteAllText(browserMarkdownPath, browserReport.ToMarkdown());

    Console.WriteLine("Wrote RazorVue G2 browser benchmark report:");
    Console.WriteLine("  " + Path.GetRelativePath(repoRoot, browserJsonPath));
    Console.WriteLine("  " + Path.GetRelativePath(repoRoot, browserMarkdownPath));
}

if (options.WriteReleaseReport)
{
    if (runtimeReport is null || generatedArtifactReport is null || browserReport is null)
        throw new InvalidOperationException("--write-release-report requires --measure-runtime, --measure-generated-artifacts, and --measure-browser.");

    var releaseReport = ReleasePerformanceReport.Create(
        repoRoot,
        options,
        runtimeReport,
        generatedArtifactReport,
        browserReport,
        OldLineBaselineProbe.Probe(repoRoot, options.RetiredLineRef));
    var releaseJsonPath = Path.Combine(outputDirectory, "razorvue-g2-release-performance-report.json");
    var releaseMarkdownPath = Path.Combine(outputDirectory, "razorvue-g2-release-performance-report.md");
    File.WriteAllText(releaseJsonPath, ReleasePerformanceReportJson.Write(releaseReport));
    File.WriteAllText(releaseMarkdownPath, releaseReport.ToMarkdown());

    Console.WriteLine("Wrote RazorVue G2 release performance report:");
    Console.WriteLine("  " + Path.GetRelativePath(repoRoot, releaseJsonPath));
    Console.WriteLine("  " + Path.GetRelativePath(repoRoot, releaseMarkdownPath));
}

internal sealed record BenchmarkArguments
{
    public bool DryRun { get; init; }

    public bool WriteProtocol { get; init; }

    public bool MeasureRuntime { get; init; }

    public bool MeasureGeneratedArtifacts { get; init; }

    public bool MeasureBrowser { get; init; }

    public bool WriteReleaseReport { get; init; }

    public string? OutputDirectory { get; init; }

    public string RetiredLineRef { get; init; } = "v0.1.26";

    public int Samples { get; init; } = 5;

    public int RenderIterations { get; init; } = 10_000;

    public int MountIterations { get; init; } = 100;

    public static BenchmarkArguments Parse(string[] args)
    {
        var result = new BenchmarkArguments();
        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            switch (argument)
            {
                case "--dry-run":
                    result = result with { DryRun = true };
                    break;
                case "--write-protocol":
                    result = result with { WriteProtocol = true };
                    break;
                case "--measure-runtime":
                    result = result with { MeasureRuntime = true };
                    break;
                case "--measure-generated-artifacts":
                    result = result with { MeasureGeneratedArtifacts = true };
                    break;
                case "--measure-browser":
                    result = result with { MeasureBrowser = true };
                    break;
                case "--write-release-report":
                    result = result with { WriteReleaseReport = true };
                    break;
                case "--out":
                    result = result with { OutputDirectory = GetValue(args, ref index, argument) };
                    break;
                case "--retired-line-ref":
                    result = result with { RetiredLineRef = GetValue(args, ref index, argument) };
                    break;
                case "--samples":
                    result = result with { Samples = GetPositiveInt(args, ref index, argument) };
                    break;
                case "--render-iterations":
                    result = result with { RenderIterations = GetPositiveInt(args, ref index, argument) };
                    break;
                case "--mount-iterations":
                    result = result with { MountIterations = GetPositiveInt(args, ref index, argument) };
                    break;
                case "--help":
                case "-h":
                    WriteUsage();
                    Environment.Exit(0);
                    break;
                default:
                    throw new InvalidOperationException("Unknown argument: " + argument);
            }
        }

        if (result.WriteReleaseReport)
            result = result with { WriteProtocol = true, MeasureRuntime = true, MeasureGeneratedArtifacts = true, MeasureBrowser = true };

        if (result.DryRun && (result.WriteProtocol || result.MeasureRuntime || result.MeasureGeneratedArtifacts || result.MeasureBrowser || result.WriteReleaseReport))
            throw new InvalidOperationException("--dry-run cannot be combined with --write-protocol, --measure-runtime, or --measure-generated-artifacts.");

        if (!result.DryRun && !result.WriteProtocol && !result.MeasureRuntime && !result.MeasureGeneratedArtifacts && !result.MeasureBrowser && !result.WriteReleaseReport)
            result = result with { DryRun = true };

        return result;
    }

    private static string GetValue(string[] args, ref int index, string argumentName)
    {
        if (index + 1 >= args.Length)
            throw new InvalidOperationException("Missing value for " + argumentName);

        index++;
        return args[index];
    }

    private static int GetPositiveInt(string[] args, ref int index, string argumentName)
    {
        var value = GetValue(args, ref index, argumentName);
        if (!int.TryParse(value, out var parsed) || parsed <= 0)
            throw new InvalidOperationException(argumentName + " must be a positive integer.");

        return parsed;
    }

    private static void WriteUsage()
    {
        Console.WriteLine("Usage: dotnet run --file scripts/csharp/benchmark-razorvue-g2.cs -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --dry-run                         Print the benchmark protocol without writing files. Default.");
        Console.WriteLine("  --write-protocol                  Write pending JSON/Markdown protocol files.");
        Console.WriteLine("  --measure-runtime                 Write protocol plus runtime-protocol benchmark JSON/Markdown report.");
        Console.WriteLine("  --measure-generated-artifacts     Build an official Razor SG three-fixture consumer and report per-fixture plus full artifact size/hash data.");
        Console.WriteLine("  --measure-browser                 Run the render-context benchmark inside a headless browser and report JS heap deltas.");
        Console.WriteLine("  --write-release-report            Run all measurement lanes and write a combined release performance report.");
        Console.WriteLine("  --out <directory>                 Output directory for written protocol/report files.");
        Console.WriteLine("  --retired-line-ref <git-ref>      Fixed retired-line baseline ref for availability probing. Default: v0.1.26.");
        Console.WriteLine("  --samples <count>                 Repeated samples per metric. Default: 5.");
        Console.WriteLine("  --render-iterations <count>       Render/update loop iterations. Default: 10000.");
        Console.WriteLine("  --mount-iterations <count>        Mount/unmount loop iterations. Default: 100.");
    }
}

internal sealed record BenchmarkProtocol(
    string SchemaVersion,
    string Status,
    string RepositoryRootName,
    int Samples,
    int RenderIterations,
    int MountIterations,
    IReadOnlyList<BenchmarkFixture> Fixtures,
    IReadOnlyList<BenchmarkMetric> Metrics,
    IReadOnlyList<BenchmarkThreshold> Thresholds,
    IReadOnlyList<string> RequiredBaselines,
    IReadOnlyList<string> Notes)
{
    public static BenchmarkProtocol Create(string repoRoot, BenchmarkArguments options)
        => new(
            SchemaVersion: "razorvue-g2-benchmark-protocol-v1",
            Status: "pending-measurement",
            RepositoryRootName: Path.GetFileName(repoRoot),
            Samples: options.Samples,
            RenderIterations: options.RenderIterations,
            MountIterations: options.MountIterations,
            Fixtures:
            [
                new(
                    Id: "plain-text",
                    Description: "Static Razor component rendering plain text.",
                    RequiredComparisons:
                    [
                        "generated Vue render-function .mjs",
                        "handwritten Vue h() baseline",
                        "retired-line baseline from fixed commit/worktree"
                    ]),
                new(
                    Id: "counter",
                    Description: "Interactive Counter component with state, event handler, and re-render.",
                    RequiredComparisons:
                    [
                        "generated Vue render-function .mjs",
                        "handwritten Vue h() baseline",
                        "retired-line baseline from fixed commit/worktree"
                    ]),
                new(
                    Id: "keyed-list-100",
                    Description: "100-item keyed list fixture covering repeat render/update behavior.",
                    RequiredComparisons:
                    [
                        "generated Vue render-function .mjs",
                        "handwritten Vue h() baseline",
                        "retired-line baseline from fixed commit/worktree"
                    ])
            ],
            Metrics:
            [
                new("compiler-cold-ms", "Cold compile elapsed time in milliseconds.", "pending"),
                new("compiler-incremental-p95-ms", "Incremental compile p95 elapsed time in milliseconds.", "pending"),
                new("generated-module-gzip-bytes", "Generated module plus protocol gzip size, shared Vue dependency excluded.", "pending"),
                new("handwritten-module-gzip-bytes", "Handwritten h() baseline gzip size, shared Vue dependency excluded.", "pending"),
                new("render-throughput-ops-per-second", "Render loop throughput across configured iterations.", "pending"),
                new("update-throughput-ops-per-second", "State update plus render throughput across configured iterations.", "pending"),
                new("retained-after-mount-cycle", "Retained handler/object count after configured mount/unmount cycles.", "pending"),
                new("runtime-protocol-count", "Runtime protocol surface count used by the fixture.", "pending"),
                new("compiler-special-case-count", "Compiler host special-case count used by the fixture.", "pending")
            ],
            Thresholds:
            [
                new("gzip", "generated module+protocol gzip <= 2x handwritten h() baseline"),
                new("throughput", "render/update throughput >= 70% of handwritten h() baseline"),
                new("incremental-compile", "incremental compiler p95 <= 1.5x retired-line baseline"),
                new("retained", "100 mount/unmount cycles must not show sustained retained handler/object growth")
            ],
            RequiredBaselines:
            [
                "same-machine generated RazorVue render-function artifact",
                "same-machine handwritten Vue h() artifact",
                "fixed retired-line commit/worktree baseline"
            ],
            Notes:
            [
                "Default dry-run/write-protocol records the G2 benchmark protocol only; --measure-runtime and --measure-generated-artifacts record partial measurement slices but do not claim complete G2 evidence.",
                "Measurements must use official Razor SG generated C# -> Roslyn IOperation -> Jazor.Compiler/SemanticWalker -> Vue render-function .mjs.",
                "Do not add Razor IR, SFC output, Jolt, or wrapper-marker fallback paths to satisfy this benchmark."
            ]);

    public string ToMarkdown()
    {
        var writer = new StringWriter();
        writer.WriteLine("# RazorVue G2 Benchmark Protocol");
        writer.WriteLine();
        writer.WriteLine("- Schema: " + SchemaVersion);
        writer.WriteLine("- Status: " + Status);
        writer.WriteLine("- Samples: " + Samples);
        writer.WriteLine("- Render iterations: " + RenderIterations);
        writer.WriteLine("- Mount iterations: " + MountIterations);
        writer.WriteLine();
        writer.WriteLine("## Fixtures");
        writer.WriteLine();
        foreach (var fixture in Fixtures)
        {
            writer.WriteLine("- `" + fixture.Id + "`: " + fixture.Description);
            foreach (var comparison in fixture.RequiredComparisons)
                writer.WriteLine("  - " + comparison);
        }

        writer.WriteLine();
        writer.WriteLine("## Metrics");
        writer.WriteLine();
        foreach (var metric in Metrics)
            writer.WriteLine("- `" + metric.Id + "`: " + metric.Description + " (" + metric.Status + ")");

        writer.WriteLine();
        writer.WriteLine("## Thresholds");
        writer.WriteLine();
        foreach (var threshold in Thresholds)
            writer.WriteLine("- `" + threshold.Id + "`: " + threshold.Rule);

        writer.WriteLine();
        writer.WriteLine("## Notes");
        writer.WriteLine();
        foreach (var note in Notes)
            writer.WriteLine("- " + note);

        return writer.ToString();
    }
}

internal sealed record BenchmarkFixture(
    string Id,
    string Description,
    IReadOnlyList<string> RequiredComparisons);

internal sealed record BenchmarkMetric(
    string Id,
    string Description,
    string Status);

internal sealed record BenchmarkThreshold(
    string Id,
    string Rule);

internal static class BenchmarkJson
{
    public static string Write(BenchmarkProtocol protocol)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteString("schemaVersion", protocol.SchemaVersion);
            writer.WriteString("status", protocol.Status);
            writer.WriteString("repositoryRootName", protocol.RepositoryRootName);
            writer.WriteNumber("samples", protocol.Samples);
            writer.WriteNumber("renderIterations", protocol.RenderIterations);
            writer.WriteNumber("mountIterations", protocol.MountIterations);

            writer.WritePropertyName("fixtures");
            writer.WriteStartArray();
            foreach (var fixture in protocol.Fixtures)
            {
                writer.WriteStartObject();
                writer.WriteString("id", fixture.Id);
                writer.WriteString("description", fixture.Description);
                WriteStringArray(writer, "requiredComparisons", fixture.RequiredComparisons);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();

            writer.WritePropertyName("metrics");
            writer.WriteStartArray();
            foreach (var metric in protocol.Metrics)
            {
                writer.WriteStartObject();
                writer.WriteString("id", metric.Id);
                writer.WriteString("description", metric.Description);
                writer.WriteString("status", metric.Status);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();

            writer.WritePropertyName("thresholds");
            writer.WriteStartArray();
            foreach (var threshold in protocol.Thresholds)
            {
                writer.WriteStartObject();
                writer.WriteString("id", threshold.Id);
                writer.WriteString("rule", threshold.Rule);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();

            WriteStringArray(writer, "requiredBaselines", protocol.RequiredBaselines);
            WriteStringArray(writer, "notes", protocol.Notes);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteStringArray(
        Utf8JsonWriter writer,
        string propertyName,
        IReadOnlyList<string> values)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartArray();
        foreach (var value in values)
            writer.WriteStringValue(value);

        writer.WriteEndArray();
    }
}

internal sealed record RuntimeBenchmarkReport(
    string SchemaVersion,
    string Status,
    string RuntimePath,
    string NodeVersion,
    int Samples,
    int RenderIterations,
    int MountIterations,
    IReadOnlyList<RuntimeFixtureMeasurement> Fixtures,
    IReadOnlyList<string> Notes)
{
    public string ToMarkdown()
    {
        var writer = new StringWriter();
        writer.WriteLine("# RazorVue G2 Runtime Benchmark Report");
        writer.WriteLine();
        writer.WriteLine("- Schema: " + SchemaVersion);
        writer.WriteLine("- Status: " + Status);
        writer.WriteLine("- Runtime: `" + RuntimePath + "`");
        writer.WriteLine("- Node: `" + NodeVersion + "`");
        writer.WriteLine("- Samples: " + Samples);
        writer.WriteLine("- Render/update iterations per sample: " + RenderIterations);
        writer.WriteLine("- Mount/unmount cycles: " + MountIterations);
        writer.WriteLine();
        writer.WriteLine("## Fixture measurements");
        writer.WriteLine();
        writer.WriteLine("| Fixture | Protocol render ops/s | Handwritten render ops/s | Render ratio | Protocol update ops/s | Handwritten update ops/s | Update ratio | Protocol gzip | Handwritten gzip | Gzip ratio | Heap delta median | Heap delta min | Heap delta max |");
        writer.WriteLine("|---------|-----------------------|--------------------------|--------------|-----------------------|--------------------------|--------------|---------------|------------------|------------|-------------------|----------------|----------------|");

        foreach (var fixture in Fixtures)
        {
            writer.WriteLine(
                "| `" + fixture.Id + "`" +
                " | " + FormatNumber(fixture.ProtocolRenderOpsPerSecond.Median) +
                " | " + FormatNumber(fixture.HandwrittenRenderOpsPerSecond.Median) +
                " | " + FormatRatio(fixture.RenderThroughputRatio) +
                " | " + FormatNumber(fixture.ProtocolUpdateOpsPerSecond.Median) +
                " | " + FormatNumber(fixture.HandwrittenUpdateOpsPerSecond.Median) +
                " | " + FormatRatio(fixture.UpdateThroughputRatio) +
                " | " + fixture.ProtocolBodyGzipBytes.ToString(CultureInfo.InvariantCulture) +
                " | " + fixture.HandwrittenBodyGzipBytes.ToString(CultureInfo.InvariantCulture) +
                " | " + FormatRatio(fixture.GzipRatio) +
                " | " + fixture.RetainedHeapDeltaBytes.Median.ToString(CultureInfo.InvariantCulture) +
                " | " + fixture.RetainedHeapDeltaBytes.Min.ToString(CultureInfo.InvariantCulture) +
                " | " + fixture.RetainedHeapDeltaBytes.Max.ToString(CultureInfo.InvariantCulture) +
                " |");
        }

        writer.WriteLine();
        writer.WriteLine("## Notes");
        writer.WriteLine();
        foreach (var note in Notes)
            writer.WriteLine("- " + note);

        return writer.ToString();
    }

    private static string FormatNumber(double value)
        => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string FormatRatio(double value)
        => value.ToString("0.000", CultureInfo.InvariantCulture);
}

internal sealed record RuntimeFixtureMeasurement(
    string Id,
    RuntimeMetricSummary ProtocolRenderOpsPerSecond,
    RuntimeMetricSummary HandwrittenRenderOpsPerSecond,
    double RenderThroughputRatio,
    RuntimeMetricSummary ProtocolUpdateOpsPerSecond,
    RuntimeMetricSummary HandwrittenUpdateOpsPerSecond,
    double UpdateThroughputRatio,
    int ProtocolBodyGzipBytes,
    int HandwrittenBodyGzipBytes,
    double GzipRatio,
    RuntimeMetricSummary RetainedHeapDeltaBytes,
    IReadOnlyList<string> Notes);

internal sealed record RuntimeMetricSummary(
    double Median,
    double Min,
    double Max,
    IReadOnlyList<double> Samples);

internal static class RuntimeBenchmarkReportJson
{
    public static string Write(RuntimeBenchmarkReport report)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteString("schemaVersion", report.SchemaVersion);
            writer.WriteString("status", report.Status);
            writer.WriteString("runtimePath", report.RuntimePath);
            writer.WriteString("nodeVersion", report.NodeVersion);
            writer.WriteNumber("samples", report.Samples);
            writer.WriteNumber("renderIterations", report.RenderIterations);
            writer.WriteNumber("mountIterations", report.MountIterations);

            writer.WritePropertyName("fixtures");
            writer.WriteStartArray();
            foreach (var fixture in report.Fixtures)
            {
                writer.WriteStartObject();
                writer.WriteString("id", fixture.Id);
                WriteSummary(writer, "protocolRenderOpsPerSecond", fixture.ProtocolRenderOpsPerSecond);
                WriteSummary(writer, "handwrittenRenderOpsPerSecond", fixture.HandwrittenRenderOpsPerSecond);
                writer.WriteNumber("renderThroughputRatio", fixture.RenderThroughputRatio);
                WriteSummary(writer, "protocolUpdateOpsPerSecond", fixture.ProtocolUpdateOpsPerSecond);
                WriteSummary(writer, "handwrittenUpdateOpsPerSecond", fixture.HandwrittenUpdateOpsPerSecond);
                writer.WriteNumber("updateThroughputRatio", fixture.UpdateThroughputRatio);
                writer.WriteNumber("protocolBodyGzipBytes", fixture.ProtocolBodyGzipBytes);
                writer.WriteNumber("handwrittenBodyGzipBytes", fixture.HandwrittenBodyGzipBytes);
                writer.WriteNumber("gzipRatio", fixture.GzipRatio);
                WriteSummary(writer, "retainedHeapDeltaBytes", fixture.RetainedHeapDeltaBytes);
                WriteStringArray(writer, "notes", fixture.Notes);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            WriteStringArray(writer, "notes", report.Notes);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static RuntimeBenchmarkReport Read(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        return new RuntimeBenchmarkReport(
            SchemaVersion: root.GetProperty("schemaVersion").GetString() ?? "",
            Status: root.GetProperty("status").GetString() ?? "",
            RuntimePath: root.GetProperty("runtimePath").GetString() ?? "",
            NodeVersion: root.GetProperty("nodeVersion").GetString() ?? "",
            Samples: root.GetProperty("samples").GetInt32(),
            RenderIterations: root.GetProperty("renderIterations").GetInt32(),
            MountIterations: root.GetProperty("mountIterations").GetInt32(),
            Fixtures: ReadFixtures(root.GetProperty("fixtures")),
            Notes: ReadStringArray(root.GetProperty("notes")));
    }

    private static RuntimeFixtureMeasurement[] ReadFixtures(JsonElement fixtures)
        => fixtures
            .EnumerateArray()
            .Select(static fixture => new RuntimeFixtureMeasurement(
                Id: fixture.GetProperty("id").GetString() ?? "",
                ProtocolRenderOpsPerSecond: ReadSummary(fixture.GetProperty("protocolRenderOpsPerSecond")),
                HandwrittenRenderOpsPerSecond: ReadSummary(fixture.GetProperty("handwrittenRenderOpsPerSecond")),
                RenderThroughputRatio: fixture.GetProperty("renderThroughputRatio").GetDouble(),
                ProtocolUpdateOpsPerSecond: ReadSummary(fixture.GetProperty("protocolUpdateOpsPerSecond")),
                HandwrittenUpdateOpsPerSecond: ReadSummary(fixture.GetProperty("handwrittenUpdateOpsPerSecond")),
                UpdateThroughputRatio: fixture.GetProperty("updateThroughputRatio").GetDouble(),
                ProtocolBodyGzipBytes: fixture.GetProperty("protocolBodyGzipBytes").GetInt32(),
                HandwrittenBodyGzipBytes: fixture.GetProperty("handwrittenBodyGzipBytes").GetInt32(),
                GzipRatio: fixture.GetProperty("gzipRatio").GetDouble(),
                RetainedHeapDeltaBytes: ReadSummary(fixture.GetProperty("retainedHeapDeltaBytes")),
                Notes: ReadStringArray(fixture.GetProperty("notes"))))
            .ToArray();

    private static RuntimeMetricSummary ReadSummary(JsonElement element)
        => new(
            Median: element.GetProperty("median").GetDouble(),
            Min: element.GetProperty("min").GetDouble(),
            Max: element.GetProperty("max").GetDouble(),
            Samples: element
                .GetProperty("samples")
                .EnumerateArray()
                .Select(static sample => sample.GetDouble())
                .ToArray());

    private static string[] ReadStringArray(JsonElement element)
        => element
            .EnumerateArray()
            .Select(static item => item.GetString() ?? "")
            .ToArray();

    private static void WriteSummary(
        Utf8JsonWriter writer,
        string propertyName,
        RuntimeMetricSummary summary)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartObject();
        writer.WriteNumber("median", summary.Median);
        writer.WriteNumber("min", summary.Min);
        writer.WriteNumber("max", summary.Max);
        writer.WritePropertyName("samples");
        writer.WriteStartArray();
        foreach (var sample in summary.Samples)
            writer.WriteNumberValue(sample);

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteStringArray(
        Utf8JsonWriter writer,
        string propertyName,
        IReadOnlyList<string> values)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartArray();
        foreach (var value in values)
            writer.WriteStringValue(value);

        writer.WriteEndArray();
    }
}

internal static class RuntimeBenchmarkRunner
{
    public static async Task<RuntimeBenchmarkReport> RunAsync(
        string repoRoot,
        BenchmarkArguments options)
    {
        var runtimeCorePath = Path.Combine(repoRoot, "src", "Jazor.RazorVue", "Runtime", "render-context-core.mjs");
        if (!File.Exists(runtimeCorePath))
            throw new FileNotFoundException("RazorVue render-context runtime core was not found.", runtimeCorePath);

        var tempDirectory = Path.Combine(repoRoot, ".tmp", "razorvue-g2-benchmark", "runtime-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var runnerPath = Path.Combine(tempDirectory, "runtime-benchmark.mjs");
        await File.WriteAllTextAsync(
            runnerPath,
            CreateNodeRunner(new Uri(runtimeCorePath).AbsoluteUri, options),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        var result = await RunNodeAsync(repoRoot, runnerPath);
        if (result.ExitCode != 0)
            throw new InvalidOperationException("Runtime benchmark failed." + Environment.NewLine + result);

        return RuntimeBenchmarkReportJson.Read(result.StandardOutput);
    }

    private static string CreateNodeRunner(
        string runtimeCoreUri,
        BenchmarkArguments options)
    {
        var runtimeRelativePath = "src/Jazor.RazorVue/Runtime/render-context-core.mjs";
        return $$"""
            import { performance } from "node:perf_hooks";
            import { gzipSync } from "node:zlib";
            import { createRenderContextCore } from "{{runtimeCoreUri}}";

            const samples = {{options.Samples.ToString(CultureInfo.InvariantCulture)}};
            const renderIterations = {{options.RenderIterations.ToString(CultureInfo.InvariantCulture)}};
            const mountIterations = {{options.MountIterations.ToString(CultureInfo.InvariantCulture)}};
            const fragment = Symbol.for("Jazor.Fragment");

            function h(type, props, children) {
              return {
                type,
                props: props ?? null,
                children: Array.isArray(children) ? children : (children === undefined ? [] : [children])
              };
            }

            function createStaticVNode(html, rootCount) {
              return {
                type: "static",
                html: html ?? "",
                rootCount: rootCount ?? 1
              };
            }

            function createContext() {
              return createRenderContextCore(h, fragment, createStaticVNode);
            }

            function gzipBytes(value) {
              return gzipSync(Buffer.from(value, "utf8")).byteLength;
            }

            function summarize(values) {
              const ordered = [...values].sort((left, right) => left - right);
              return {
                median: ordered[Math.floor(ordered.length / 2)],
                min: ordered[0],
                max: ordered[ordered.length - 1],
                samples: values
              };
            }

            function opsPerSecond(operation, iterations) {
              let sink;
              const started = performance.now();
              for (let index = 0; index < iterations; index++) {
                sink = operation(index);
              }

              if (sink === undefined) {
                throw new Error("benchmark operation returned undefined");
              }

              const elapsedMs = Math.max(performance.now() - started, 0.000001);
              return iterations / (elapsedMs / 1000);
            }

            function sample(operation, iterations) {
              operation(0);
              const values = [];
              for (let index = 0; index < samples; index++) {
                values.push(opsPerSecond(operation, iterations));
              }

              return summarize(values);
            }

            function heapUsed() {
              if (typeof globalThis.gc === "function") {
                globalThis.gc();
              }

              return process.memoryUsage().heapUsed;
            }

            function retainedHeapDelta(operation) {
              const before = heapUsed();
              let sink;
              for (let index = 0; index < mountIterations; index++) {
                sink = operation(index);
              }

              if (sink === undefined) {
                throw new Error("retained measurement returned undefined");
              }

              return heapUsed() - before;
            }

            function sampleRetainedHeapDelta(operation) {
              operation(0);
              const values = [];
              for (let index = 0; index < samples; index++) {
                values.push(retainedHeapDelta(operation));
              }

              return summarize(values);
            }

            function createItems(seed) {
              return Array.from({ length: 100 }, (_, index) => ({
                id: index,
                title: `Item ${index}`,
                active: ((index + seed) % 2) === 0
              }));
            }

            let counterState = 0;
            const counterHandler = () => {
              counterState++;
            };

            function renderProtocolPlainText() {
              const builder = createContext();
              builder.addContent("Hello RazorVue");
              return builder.finish();
            }

            function renderHandwrittenPlainText() {
              return "Hello RazorVue";
            }

            function updateProtocolPlainText(index) {
              const builder = createContext();
              builder.addContent(index % 2 === 0 ? "Hello RazorVue" : "Hello Jazor");
              return builder.finish();
            }

            function updateHandwrittenPlainText(index) {
              return index % 2 === 0 ? "Hello RazorVue" : "Hello Jazor";
            }

            function renderProtocolCounter() {
              const builder = createContext();
              builder.openElement("button");
              builder.addAttribute("onclick", counterHandler);
              builder.addContent("Clicks: ");
              builder.addContent(counterState);
              builder.closeElement();
              return builder.finish();
            }

            function renderHandwrittenCounter() {
              return h("button", { onClick: counterHandler }, ["Clicks: ", counterState]);
            }

            function updateProtocolCounter() {
              counterState++;
              return renderProtocolCounter();
            }

            function updateHandwrittenCounter() {
              counterState++;
              return renderHandwrittenCounter();
            }

            function renderProtocolKeyedList(index) {
              const builder = createContext();
              builder.openElement("ul");
              for (const item of createItems(index)) {
                builder.openElement("li");
                builder.addAttribute("key", item.id);
                builder.addContent(item.title);
                if (item.active) {
                  builder.addContent(" active");
                }
                builder.closeElement();
              }

              builder.closeElement();
              return builder.finish();
            }

            function renderHandwrittenKeyedList(index) {
              return h("ul", null, createItems(index).map(item => h(
                "li",
                { key: item.id },
                item.active ? [item.title, " active"] : [item.title])));
            }

            const fixtures = [
              {
                id: "plain-text",
                protocolRender: renderProtocolPlainText,
                handwrittenRender: renderHandwrittenPlainText,
                protocolUpdate: updateProtocolPlainText,
                handwrittenUpdate: updateHandwrittenPlainText,
                protocolSource: renderProtocolPlainText.toString() + updateProtocolPlainText.toString(),
                handwrittenSource: renderHandwrittenPlainText.toString() + updateHandwrittenPlainText.toString()
              },
              {
                id: "counter",
                protocolRender: renderProtocolCounter,
                handwrittenRender: renderHandwrittenCounter,
                protocolUpdate: updateProtocolCounter,
                handwrittenUpdate: updateHandwrittenCounter,
                protocolSource: renderProtocolCounter.toString() + updateProtocolCounter.toString(),
                handwrittenSource: renderHandwrittenCounter.toString() + updateHandwrittenCounter.toString()
              },
              {
                id: "keyed-list-100",
                protocolRender: renderProtocolKeyedList,
                handwrittenRender: renderHandwrittenKeyedList,
                protocolUpdate: renderProtocolKeyedList,
                handwrittenUpdate: renderHandwrittenKeyedList,
                protocolSource: createItems.toString() + renderProtocolKeyedList.toString(),
                handwrittenSource: createItems.toString() + renderHandwrittenKeyedList.toString()
              }
            ];

            const measurements = fixtures.map(fixture => {
              const protocolRender = sample(fixture.protocolRender, renderIterations);
              const handwrittenRender = sample(fixture.handwrittenRender, renderIterations);
              const protocolUpdate = sample(fixture.protocolUpdate, renderIterations);
              const handwrittenUpdate = sample(fixture.handwrittenUpdate, renderIterations);
              const protocolBodyGzipBytes = gzipBytes(fixture.protocolSource);
              const handwrittenBodyGzipBytes = gzipBytes(fixture.handwrittenSource);

              return {
                id: fixture.id,
                protocolRenderOpsPerSecond: protocolRender,
                handwrittenRenderOpsPerSecond: handwrittenRender,
                renderThroughputRatio: protocolRender.median / handwrittenRender.median,
                protocolUpdateOpsPerSecond: protocolUpdate,
                handwrittenUpdateOpsPerSecond: handwrittenUpdate,
                updateThroughputRatio: protocolUpdate.median / handwrittenUpdate.median,
                protocolBodyGzipBytes,
                handwrittenBodyGzipBytes,
                gzipRatio: protocolBodyGzipBytes / handwrittenBodyGzipBytes,
                retainedHeapDeltaBytes: sampleRetainedHeapDelta(fixture.protocolUpdate),
                notes: [
                  "This measurement compares the shared render-context runtime protocol against direct handwritten h() calls.",
                  "It does not include official Razor SG build time, generated .mjs size, browser DOM patching, or retired-line baseline."
                ]
              };
            });

            const report = {
              schemaVersion: "razorvue-g2-runtime-benchmark-v1",
              status: "partial-runtime-measurement",
              runtimePath: "{{runtimeRelativePath}}",
              nodeVersion: process.version,
              samples,
              renderIterations,
              mountIterations,
              fixtures: measurements,
              notes: [
                "Partial G2 evidence only: runtime protocol numbers are useful for trend tracking but do not satisfy the full G2 gate.",
                "Full G2 still requires official Razor SG generated artifact measurements, compiler cold/incremental timings, browser/heap retained-object methodology, and fixed retired-line baseline comparison.",
                "The benchmark uses the active render-context runtime and does not introduce Razor IR, SFC, Jolt, or wrapper-marker fallback paths."
              ]
            };

            process.stdout.write(JSON.stringify(report));
            """;
    }

    private static async Task<ProcessResult> RunNodeAsync(string repoRoot, string runnerPath)
    {
        var startInfo = new ProcessStartInfo("node")
        {
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("--expose-gc");
        startInfo.ArgumentList.Add(runnerPath);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start Node.js for RazorVue G2 runtime benchmark.");
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new ProcessResult(
            process.ExitCode,
            await standardOutput,
            await standardError);
    }
}

internal sealed record BrowserBenchmarkReport(
    string SchemaVersion,
    string Status,
    string BrowserPath,
    int Samples,
    int RenderIterations,
    int MountIterations,
    IReadOnlyList<BrowserFixtureMeasurement> Fixtures,
    IReadOnlyList<string> Notes)
{
    public string ToMarkdown()
    {
        var writer = new StringWriter();
        writer.WriteLine("# RazorVue G2 Browser Benchmark Report");
        writer.WriteLine();
        writer.WriteLine("- Schema: " + SchemaVersion);
        writer.WriteLine("- Status: " + Status);
        writer.WriteLine("- Browser: `" + BrowserPath + "`");
        writer.WriteLine("- Samples: " + Samples);
        writer.WriteLine("- Render/update iterations per sample: " + RenderIterations);
        writer.WriteLine("- Mount/unmount cycles: " + MountIterations);
        writer.WriteLine();
        writer.WriteLine("## Fixture measurements");
        writer.WriteLine();
        writer.WriteLine("| Fixture | Protocol render ops/s | Handwritten render ops/s | Render ratio | Protocol update ops/s | Handwritten update ops/s | Update ratio | Heap available | Heap delta median | Heap delta min | Heap delta max |");
        writer.WriteLine("|---------|-----------------------|--------------------------|--------------|-----------------------|--------------------------|--------------|----------------|-------------------|----------------|----------------|");

        foreach (var fixture in Fixtures)
        {
            writer.WriteLine(
                "| `" + fixture.Id + "`" +
                " | " + FormatNumber(fixture.ProtocolRenderOpsPerSecond.Median) +
                " | " + FormatNumber(fixture.HandwrittenRenderOpsPerSecond.Median) +
                " | " + FormatRatio(fixture.RenderThroughputRatio) +
                " | " + FormatNumber(fixture.ProtocolUpdateOpsPerSecond.Median) +
                " | " + FormatNumber(fixture.HandwrittenUpdateOpsPerSecond.Median) +
                " | " + FormatRatio(fixture.UpdateThroughputRatio) +
                " | " + (fixture.HeapMeasurementAvailable ? "yes" : "no") +
                " | " + fixture.RetainedHeapDeltaBytes.Median.ToString(CultureInfo.InvariantCulture) +
                " | " + fixture.RetainedHeapDeltaBytes.Min.ToString(CultureInfo.InvariantCulture) +
                " | " + fixture.RetainedHeapDeltaBytes.Max.ToString(CultureInfo.InvariantCulture) +
                " |");
        }

        writer.WriteLine();
        writer.WriteLine("## Notes");
        writer.WriteLine();
        foreach (var note in Notes)
            writer.WriteLine("- " + note);

        return writer.ToString();
    }

    private static string FormatNumber(double value)
        => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string FormatRatio(double value)
        => value.ToString("0.000", CultureInfo.InvariantCulture);
}

internal sealed record BrowserFixtureMeasurement(
    string Id,
    RuntimeMetricSummary ProtocolRenderOpsPerSecond,
    RuntimeMetricSummary HandwrittenRenderOpsPerSecond,
    double RenderThroughputRatio,
    RuntimeMetricSummary ProtocolUpdateOpsPerSecond,
    RuntimeMetricSummary HandwrittenUpdateOpsPerSecond,
    double UpdateThroughputRatio,
    bool HeapMeasurementAvailable,
    RuntimeMetricSummary RetainedHeapDeltaBytes,
    IReadOnlyList<string> Notes);

internal static class BrowserBenchmarkReportJson
{
    public static string Write(BrowserBenchmarkReport report)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteString("schemaVersion", report.SchemaVersion);
            writer.WriteString("status", report.Status);
            writer.WriteString("browserPath", report.BrowserPath);
            writer.WriteNumber("samples", report.Samples);
            writer.WriteNumber("renderIterations", report.RenderIterations);
            writer.WriteNumber("mountIterations", report.MountIterations);
            writer.WritePropertyName("fixtures");
            writer.WriteStartArray();
            foreach (var fixture in report.Fixtures)
            {
                writer.WriteStartObject();
                writer.WriteString("id", fixture.Id);
                WriteSummary(writer, "protocolRenderOpsPerSecond", fixture.ProtocolRenderOpsPerSecond);
                WriteSummary(writer, "handwrittenRenderOpsPerSecond", fixture.HandwrittenRenderOpsPerSecond);
                writer.WriteNumber("renderThroughputRatio", fixture.RenderThroughputRatio);
                WriteSummary(writer, "protocolUpdateOpsPerSecond", fixture.ProtocolUpdateOpsPerSecond);
                WriteSummary(writer, "handwrittenUpdateOpsPerSecond", fixture.HandwrittenUpdateOpsPerSecond);
                writer.WriteNumber("updateThroughputRatio", fixture.UpdateThroughputRatio);
                writer.WriteBoolean("heapMeasurementAvailable", fixture.HeapMeasurementAvailable);
                WriteSummary(writer, "retainedHeapDeltaBytes", fixture.RetainedHeapDeltaBytes);
                WriteStringArray(writer, "notes", fixture.Notes);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            WriteStringArray(writer, "notes", report.Notes);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static BrowserBenchmarkReport Read(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        return new BrowserBenchmarkReport(
            SchemaVersion: root.GetProperty("schemaVersion").GetString() ?? "",
            Status: root.GetProperty("status").GetString() ?? "",
            BrowserPath: root.GetProperty("browserPath").GetString() ?? "",
            Samples: root.GetProperty("samples").GetInt32(),
            RenderIterations: root.GetProperty("renderIterations").GetInt32(),
            MountIterations: root.GetProperty("mountIterations").GetInt32(),
            Fixtures: root.GetProperty("fixtures")
                .EnumerateArray()
                .Select(static fixture => new BrowserFixtureMeasurement(
                    Id: fixture.GetProperty("id").GetString() ?? "",
                    ProtocolRenderOpsPerSecond: ReadSummary(fixture.GetProperty("protocolRenderOpsPerSecond")),
                    HandwrittenRenderOpsPerSecond: ReadSummary(fixture.GetProperty("handwrittenRenderOpsPerSecond")),
                    RenderThroughputRatio: fixture.GetProperty("renderThroughputRatio").GetDouble(),
                    ProtocolUpdateOpsPerSecond: ReadSummary(fixture.GetProperty("protocolUpdateOpsPerSecond")),
                    HandwrittenUpdateOpsPerSecond: ReadSummary(fixture.GetProperty("handwrittenUpdateOpsPerSecond")),
                    UpdateThroughputRatio: fixture.GetProperty("updateThroughputRatio").GetDouble(),
                    HeapMeasurementAvailable: fixture.GetProperty("heapMeasurementAvailable").GetBoolean(),
                    RetainedHeapDeltaBytes: ReadSummary(fixture.GetProperty("retainedHeapDeltaBytes")),
                    Notes: ReadStringArray(fixture.GetProperty("notes"))))
                .ToArray(),
            Notes: ReadStringArray(root.GetProperty("notes")));
    }

    private static RuntimeMetricSummary ReadSummary(JsonElement element)
        => new(
            Median: element.GetProperty("median").GetDouble(),
            Min: element.GetProperty("min").GetDouble(),
            Max: element.GetProperty("max").GetDouble(),
            Samples: element.GetProperty("samples").EnumerateArray().Select(static sample => sample.GetDouble()).ToArray());

    private static string[] ReadStringArray(JsonElement element)
        => element.EnumerateArray().Select(static item => item.GetString() ?? "").ToArray();

    private static void WriteSummary(Utf8JsonWriter writer, string propertyName, RuntimeMetricSummary summary)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartObject();
        writer.WriteNumber("median", summary.Median);
        writer.WriteNumber("min", summary.Min);
        writer.WriteNumber("max", summary.Max);
        writer.WritePropertyName("samples");
        writer.WriteStartArray();
        foreach (var sample in summary.Samples)
            writer.WriteNumberValue(sample);

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteStringArray(Utf8JsonWriter writer, string propertyName, IReadOnlyList<string> values)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartArray();
        foreach (var value in values)
            writer.WriteStringValue(value);

        writer.WriteEndArray();
    }
}

internal static class BrowserBenchmarkRunner
{
    public static async Task<BrowserBenchmarkReport> RunAsync(string repoRoot, BenchmarkArguments options)
    {
        var browserPath = ResolveBrowserExecutable();
        if (browserPath is null)
        {
            return new BrowserBenchmarkReport(
                SchemaVersion: "razorvue-g2-browser-benchmark-v1",
                Status: "browser-unavailable",
                BrowserPath: "",
                Samples: options.Samples,
                RenderIterations: options.RenderIterations,
                MountIterations: options.MountIterations,
                Fixtures: [],
                Notes:
                [
                    "Headless browser benchmark requires Microsoft Edge, Chrome, or Chromium.",
                    "Set RAZORVUE_BROWSER_EXE or RAZORVUE_BROWSER_PATH to the browser executable path."
                ]);
        }

        var runtimeCorePath = Path.Combine(repoRoot, "src", "Jazor.RazorVue", "Runtime", "render-context-core.mjs");
        if (!File.Exists(runtimeCorePath))
            throw new FileNotFoundException("RazorVue render-context runtime core was not found.", runtimeCorePath);

        var harnessRoot = Path.Combine(repoRoot, ".tmp", "razorvue-g2-benchmark", "browser-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(harnessRoot);
        var indexPath = Path.Combine(harnessRoot, "index.html");
        await File.WriteAllTextAsync(
            indexPath,
            CreateBrowserHarness(new Uri(runtimeCorePath).AbsoluteUri, options),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        var browser = await RunBrowserDumpDomAsync(browserPath, indexPath);
        if (browser.ExitCode != 0)
            throw new InvalidOperationException("Browser benchmark failed." + Environment.NewLine + browser);

        var json = ReadBrowserPayload(browser.StandardOutput);
        var report = BrowserBenchmarkReportJson.Read(json);
        return report with { BrowserPath = browserPath };
    }

    private static string CreateBrowserHarness(string runtimeCoreUri, BenchmarkArguments options)
        => $$"""
            <!doctype html>
            <html lang="en">
              <head>
                <meta charset="utf-8">
                <title>RazorVue G2 Browser Benchmark</title>
              </head>
              <body>
                <script type="module">
                  import { createRenderContextCore } from "{{runtimeCoreUri}}";

                  const samples = {{options.Samples.ToString(CultureInfo.InvariantCulture)}};
                  const renderIterations = {{options.RenderIterations.ToString(CultureInfo.InvariantCulture)}};
                  const mountIterations = {{options.MountIterations.ToString(CultureInfo.InvariantCulture)}};
                  const fragment = Symbol.for("Jazor.Fragment");

                  function encodeUtf8Base64(value) {
                    const bytes = new TextEncoder().encode(value);
                    let binary = "";
                    for (const byte of bytes) {
                      binary += String.fromCharCode(byte);
                    }

                    return btoa(binary);
                  }

                  function finish(payload) {
                    document.documentElement.setAttribute(
                      "data-jazor-benchmark",
                      encodeUtf8Base64(JSON.stringify(payload)));
                  }

                  function h(type, props, children) {
                    return {
                      type,
                      props: props ?? null,
                      children: Array.isArray(children) ? children : (children === undefined ? [] : [children])
                    };
                  }

                  function createStaticVNode(html, rootCount) {
                    return { type: "static", html: html ?? "", rootCount: rootCount ?? 1 };
                  }

                  function createContext() {
                    return createRenderContextCore(h, fragment, createStaticVNode);
                  }

                  function summarize(values) {
                    const ordered = [...values].sort((left, right) => left - right);
                    return {
                      median: ordered[Math.floor(ordered.length / 2)] ?? 0,
                      min: ordered[0] ?? 0,
                      max: ordered[ordered.length - 1] ?? 0,
                      samples: values
                    };
                  }

                  function opsPerSecond(operation, iterations) {
                    let sink;
                    const started = performance.now();
                    for (let index = 0; index < iterations; index++) {
                      sink = operation(index);
                    }

                    if (sink === undefined) {
                      throw new Error("benchmark operation returned undefined");
                    }

                    const elapsedMs = Math.max(performance.now() - started, 0.000001);
                    return iterations / (elapsedMs / 1000);
                  }

                  function sample(operation, iterations) {
                    operation(0);
                    const values = [];
                    for (let index = 0; index < samples; index++) {
                      values.push(opsPerSecond(operation, iterations));
                    }

                    return summarize(values);
                  }

                  function heapUsed() {
                    if (typeof globalThis.gc === "function") {
                      globalThis.gc();
                    }

                    const memory = performance.memory;
                    return memory && typeof memory.usedJSHeapSize === "number"
                      ? memory.usedJSHeapSize
                      : null;
                  }

                  function retainedHeapDelta(operation) {
                    const before = heapUsed();
                    if (before === null) {
                      return null;
                    }

                    let sink;
                    for (let index = 0; index < mountIterations; index++) {
                      sink = operation(index);
                    }

                    if (sink === undefined) {
                      throw new Error("retained measurement returned undefined");
                    }

                    const after = heapUsed();
                    return after === null ? null : after - before;
                  }

                  function sampleRetainedHeapDelta(operation) {
                    operation(0);
                    const values = [];
                    for (let index = 0; index < samples; index++) {
                      const delta = retainedHeapDelta(operation);
                      if (delta === null) {
                        return { available: false, summary: summarize([]) };
                      }

                      values.push(delta);
                    }

                    return { available: true, summary: summarize(values) };
                  }

                  function createItems(seed) {
                    return Array.from({ length: 100 }, (_, index) => ({
                      id: index,
                      title: `Item ${index}`,
                      active: ((index + seed) % 2) === 0
                    }));
                  }

                  let counterState = 0;
                  const counterHandler = () => {
                    counterState++;
                  };

                  function renderProtocolPlainText() {
                    const builder = createContext();
                    builder.addContent("Hello RazorVue");
                    return builder.finish();
                  }

                  function renderHandwrittenPlainText() {
                    return "Hello RazorVue";
                  }

                  function updateProtocolPlainText(index) {
                    const builder = createContext();
                    builder.addContent(index % 2 === 0 ? "Hello RazorVue" : "Hello Jazor");
                    return builder.finish();
                  }

                  function updateHandwrittenPlainText(index) {
                    return index % 2 === 0 ? "Hello RazorVue" : "Hello Jazor";
                  }

                  function renderProtocolCounter() {
                    const builder = createContext();
                    builder.openElement("button");
                    builder.addAttribute("onclick", counterHandler);
                    builder.addContent("Clicks: ");
                    builder.addContent(counterState);
                    builder.closeElement();
                    return builder.finish();
                  }

                  function renderHandwrittenCounter() {
                    return h("button", { onClick: counterHandler }, ["Clicks: ", counterState]);
                  }

                  function updateProtocolCounter() {
                    counterState++;
                    return renderProtocolCounter();
                  }

                  function updateHandwrittenCounter() {
                    counterState++;
                    return renderHandwrittenCounter();
                  }

                  function renderProtocolKeyedList(index) {
                    const builder = createContext();
                    builder.openElement("ul");
                    for (const item of createItems(index)) {
                      builder.openElement("li");
                      builder.addAttribute("key", item.id);
                      builder.addContent(item.title);
                      if (item.active) {
                        builder.addContent(" active");
                      }
                      builder.closeElement();
                    }

                    builder.closeElement();
                    return builder.finish();
                  }

                  function renderHandwrittenKeyedList(index) {
                    return h("ul", null, createItems(index).map(item => h(
                      "li",
                      { key: item.id },
                      item.active ? [item.title, " active"] : [item.title])));
                  }

                  const fixtures = [
                    ["plain-text", renderProtocolPlainText, renderHandwrittenPlainText, updateProtocolPlainText, updateHandwrittenPlainText],
                    ["counter", renderProtocolCounter, renderHandwrittenCounter, updateProtocolCounter, updateHandwrittenCounter],
                    ["keyed-list-100", renderProtocolKeyedList, renderHandwrittenKeyedList, renderProtocolKeyedList, renderHandwrittenKeyedList]
                  ];

                  try {
                    const measurements = fixtures.map(([id, protocolRender, handwrittenRender, protocolUpdate, handwrittenUpdate]) => {
                      const protocolRenderSummary = sample(protocolRender, renderIterations);
                      const handwrittenRenderSummary = sample(handwrittenRender, renderIterations);
                      const protocolUpdateSummary = sample(protocolUpdate, renderIterations);
                      const handwrittenUpdateSummary = sample(handwrittenUpdate, renderIterations);
                      const retained = sampleRetainedHeapDelta(protocolUpdate);
                      return {
                        id,
                        protocolRenderOpsPerSecond: protocolRenderSummary,
                        handwrittenRenderOpsPerSecond: handwrittenRenderSummary,
                        renderThroughputRatio: protocolRenderSummary.median / handwrittenRenderSummary.median,
                        protocolUpdateOpsPerSecond: protocolUpdateSummary,
                        handwrittenUpdateOpsPerSecond: handwrittenUpdateSummary,
                        updateThroughputRatio: protocolUpdateSummary.median / handwrittenUpdateSummary.median,
                        heapMeasurementAvailable: retained.available,
                        retainedHeapDeltaBytes: retained.summary,
                        notes: [
                          "This browser measurement runs the active render-context runtime in a real ESM-capable browser.",
                          "It measures VNode construction protocol cost and browser JS heap deltas, not Vue DOM patch cost."
                        ]
                      };
                    });

                    finish({
                      schemaVersion: "razorvue-g2-browser-benchmark-v1",
                      status: "browser-measurement",
                      browserPath: "",
                      samples,
                      renderIterations,
                      mountIterations,
                      fixtures: measurements,
                      notes: [
                        "Headless browser benchmark uses file:// ESM loading with --allow-file-access-from-files.",
                        "Heap deltas depend on browser support for performance.memory.usedJSHeapSize and are recorded as unavailable when the browser does not expose it.",
                        "This lane complements Node protocol numbers and generated artifact measurements; it does not introduce any alternate RazorVue lowering path."
                      ]
                    });
                  } catch (error) {
                    finish({
                      schemaVersion: "razorvue-g2-browser-benchmark-v1",
                      status: "browser-measurement-failed",
                      browserPath: "",
                      samples,
                      renderIterations,
                      mountIterations,
                      fixtures: [],
                      notes: [
                        error instanceof Error ? (error.stack || error.message) : String(error)
                      ]
                    });
                  }
                </script>
              </body>
            </html>
            """;

    private static string? ResolveBrowserExecutable()
    {
        var explicitPath = Environment.GetEnvironmentVariable("RAZORVUE_BROWSER_EXE")?.Trim();
        if (string.IsNullOrWhiteSpace(explicitPath))
            explicitPath = Environment.GetEnvironmentVariable("RAZORVUE_BROWSER_PATH")?.Trim();

        if (!string.IsNullOrWhiteSpace(explicitPath))
            return File.Exists(explicitPath) ? explicitPath : null;

        var candidates = OperatingSystem.IsWindows()
            ? new[]
            {
                @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                "msedge.exe",
                "chrome.exe"
            }
            : OperatingSystem.IsMacOS()
                ? new[]
                {
                    "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge",
                    "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
                    "microsoft-edge",
                    "google-chrome",
                    "chromium"
                }
                : new[]
                {
                    "microsoft-edge",
                    "microsoft-edge-stable",
                    "google-chrome",
                    "google-chrome-stable",
                    "chromium",
                    "chromium-browser"
                };

        foreach (var candidate in candidates)
        {
            var resolved = TryResolveBrowserExecutable(candidate);
            if (resolved is not null)
                return resolved;
        }

        return null;
    }

    private static string? TryResolveBrowserExecutable(string candidate)
    {
        if (candidate.Contains(Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
            candidate.Contains(Path.AltDirectorySeparatorChar, StringComparison.Ordinal) ||
            candidate.Contains(':', StringComparison.Ordinal))
        {
            return File.Exists(candidate) ? candidate : null;
        }

        var path = Environment.GetEnvironmentVariable("PATH") ?? "";
        var extensions = OperatingSystem.IsWindows()
            ? (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT").Split(';', StringSplitOptions.RemoveEmptyEntries)
            : [""];

        foreach (var directory in path.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(directory))
                continue;

            foreach (var extension in extensions)
            {
                var fileName = candidate.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                    ? candidate
                    : candidate + extension;
                var fullPath = Path.Combine(directory, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
        }

        return null;
    }

    private static async Task<ProcessResult> RunBrowserDumpDomAsync(string browserPath, string indexPath)
    {
        var harnessRoot = Path.GetDirectoryName(indexPath)!;
        var userDataRoot = Path.Combine(harnessRoot, ".browser-profile-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(userDataRoot);

        try
        {
            return await RunProcessAsync(
                browserPath,
                harnessRoot,
                [
                    "--headless=new",
                    "--disable-gpu",
                    "--disable-dev-shm-usage",
                    "--no-first-run",
                    "--no-default-browser-check",
                    "--no-sandbox",
                    "--allow-file-access-from-files",
                    "--enable-precise-memory-info",
                    "--js-flags=--expose-gc",
                    "--run-all-compositor-stages-before-draw",
                    "--virtual-time-budget=60000",
                    "--dump-dom",
                    $"--user-data-dir={userDataRoot}",
                    new Uri(Path.GetFullPath(indexPath)).AbsoluteUri
                ],
                TimeSpan.FromSeconds(90));
        }
        finally
        {
            try
            {
                if (Directory.Exists(userDataRoot))
                    Directory.Delete(userDataRoot, recursive: true);
            }
            catch
            {
            }
        }
    }

    private static string ReadBrowserPayload(string standardOutput)
    {
        var match = Regex.Match(
            standardOutput,
            "data-jazor-benchmark=\"(?<payload>[A-Za-z0-9+/=]+)\"",
            RegexOptions.CultureInvariant);
        if (!match.Success)
            throw new InvalidOperationException("Browser DOM did not contain the RazorVue G2 benchmark result marker.");

        return Encoding.UTF8.GetString(Convert.FromBase64String(match.Groups["payload"].Value));
    }

    private static async Task<ProcessResult> RunProcessAsync(
        string fileName,
        string workingDirectory,
        IReadOnlyList<string> arguments,
        TimeSpan timeout)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();

        var timedOut = false;
        using var timeoutSource = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
        }
        catch (OperationCanceledException)
        {
            timedOut = true;
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
            }

            await process.WaitForExitAsync();
        }

        var output = await standardOutput;
        var error = await standardError;
        return timedOut
            ? new ProcessResult(-1, output, "Process timed out after " + timeout + "." + Environment.NewLine + error)
            : new ProcessResult(process.ExitCode, output, error);
    }
}

internal sealed record GeneratedArtifactBenchmarkReport(
    string SchemaVersion,
    string Status,
    IReadOnlyList<GeneratedArtifactFixtureMeasurement> Fixtures,
    string DotNetSdkVersion,
    string PackageVersion,
    string WorkspaceRelativePath,
    string OutputRootRelativePath,
    long PackageElapsedMilliseconds,
    long CleanBuildElapsedMilliseconds,
    IReadOnlyList<long> IncrementalBuildElapsedMilliseconds,
    long IncrementalBuildP95Milliseconds,
    IReadOnlyList<GeneratedArtifactMeasurement> Artifacts,
    IReadOnlyList<string> Notes)
{
    public string ToMarkdown()
    {
        var writer = new StringWriter();
        writer.WriteLine("# RazorVue G2 Generated Artifact Report");
        writer.WriteLine();
        writer.WriteLine("- Schema: " + SchemaVersion);
        writer.WriteLine("- Status: " + Status);
        writer.WriteLine("- Fixtures: " + string.Join(", ", Fixtures.Select(static fixture => "`" + fixture.Id + "`")));
        writer.WriteLine("- .NET SDK: `" + DotNetSdkVersion + "`");
        writer.WriteLine("- Jazor package version: `" + PackageVersion + "`");
        writer.WriteLine("- Workspace: `" + WorkspaceRelativePath + "`");
        writer.WriteLine("- Artifact root: `" + OutputRootRelativePath + "`");
        writer.WriteLine("- Package elapsed: " + PackageElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) + " ms");
        writer.WriteLine("- Clean build elapsed: " + CleanBuildElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) + " ms");
        writer.WriteLine("- Incremental build samples: " + string.Join(", ", IncrementalBuildElapsedMilliseconds.Select(static value => value.ToString(CultureInfo.InvariantCulture))) + " ms");
        writer.WriteLine("- Incremental build p95: " + IncrementalBuildP95Milliseconds.ToString(CultureInfo.InvariantCulture) + " ms");
        writer.WriteLine();
        writer.WriteLine("## Fixture modules");
        writer.WriteLine();
        writer.WriteLine("| Fixture | Component path | Component bytes | Component gzip | Handwritten bytes | Handwritten gzip | Gzip ratio | Source map path | Source map bytes | Source map gzip |");
        writer.WriteLine("|---------|----------------|-----------------|----------------|-------------------|------------------|------------|-----------------|------------------|-----------------|");

        foreach (var fixture in Fixtures)
        {
            writer.WriteLine(
                "| `" + fixture.Id + "`" +
                " | `" + fixture.ComponentRelativePath + "`" +
                " | " + fixture.ComponentBytes.ToString(CultureInfo.InvariantCulture) +
                " | " + fixture.ComponentGzipBytes.ToString(CultureInfo.InvariantCulture) +
                " | " + fixture.HandwrittenBytes.ToString(CultureInfo.InvariantCulture) +
                " | " + fixture.HandwrittenGzipBytes.ToString(CultureInfo.InvariantCulture) +
                " | " + fixture.GzipRatio.ToString("0.###", CultureInfo.InvariantCulture) +
                " | `" + fixture.SourceMapRelativePath + "`" +
                " | " + fixture.SourceMapBytes.ToString(CultureInfo.InvariantCulture) +
                " | " + fixture.SourceMapGzipBytes.ToString(CultureInfo.InvariantCulture) +
                " |");
        }

        writer.WriteLine();
        writer.WriteLine("## Artifacts");
        writer.WriteLine();
        writer.WriteLine("| Path | Kind | Bytes | Gzip bytes | SHA256 |");
        writer.WriteLine("|------|------|-------|------------|--------|");

        foreach (var artifact in Artifacts)
        {
            writer.WriteLine(
                "| `" + artifact.RelativePath + "`" +
                " | " + artifact.Kind +
                " | " + artifact.Bytes.ToString(CultureInfo.InvariantCulture) +
                " | " + artifact.GzipBytes.ToString(CultureInfo.InvariantCulture) +
                " | `" + artifact.Sha256 + "` |");
        }

        writer.WriteLine();
        writer.WriteLine("## Notes");
        writer.WriteLine();
        foreach (var note in Notes)
            writer.WriteLine("- " + note);

        return writer.ToString();
    }
}

internal sealed record GeneratedArtifactMeasurement(
    string RelativePath,
    string Kind,
    long Bytes,
    int GzipBytes,
    string Sha256);

internal sealed record GeneratedArtifactFixtureMeasurement(
    string Id,
    string Description,
    string ComponentRelativePath,
    long ComponentBytes,
    int ComponentGzipBytes,
    string ComponentSha256,
    string SourceMapRelativePath,
    long SourceMapBytes,
    int SourceMapGzipBytes,
    string SourceMapSha256,
    long HandwrittenBytes,
    int HandwrittenGzipBytes,
    double GzipRatio);

internal sealed record GeneratedArtifactFixtureDefinition(
    string Id,
    string Description,
    string ComponentRelativePath,
    string SourceMapRelativePath);

internal static class GeneratedArtifactBenchmarkReportJson
{
    public static string Write(GeneratedArtifactBenchmarkReport report)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteString("schemaVersion", report.SchemaVersion);
            writer.WriteString("status", report.Status);
            writer.WritePropertyName("fixtures");
            writer.WriteStartArray();
            foreach (var fixture in report.Fixtures)
            {
                writer.WriteStartObject();
                writer.WriteString("id", fixture.Id);
                writer.WriteString("description", fixture.Description);
                writer.WriteString("componentRelativePath", fixture.ComponentRelativePath);
                writer.WriteNumber("componentBytes", fixture.ComponentBytes);
                writer.WriteNumber("componentGzipBytes", fixture.ComponentGzipBytes);
                writer.WriteString("componentSha256", fixture.ComponentSha256);
                writer.WriteString("sourceMapRelativePath", fixture.SourceMapRelativePath);
                writer.WriteNumber("sourceMapBytes", fixture.SourceMapBytes);
                writer.WriteNumber("sourceMapGzipBytes", fixture.SourceMapGzipBytes);
                writer.WriteString("sourceMapSha256", fixture.SourceMapSha256);
                writer.WriteNumber("handwrittenBytes", fixture.HandwrittenBytes);
                writer.WriteNumber("handwrittenGzipBytes", fixture.HandwrittenGzipBytes);
                writer.WriteNumber("gzipRatio", fixture.GzipRatio);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteString("dotNetSdkVersion", report.DotNetSdkVersion);
            writer.WriteString("packageVersion", report.PackageVersion);
            writer.WriteString("workspaceRelativePath", report.WorkspaceRelativePath);
            writer.WriteString("outputRootRelativePath", report.OutputRootRelativePath);
            writer.WriteNumber("packageElapsedMilliseconds", report.PackageElapsedMilliseconds);
            writer.WriteNumber("cleanBuildElapsedMilliseconds", report.CleanBuildElapsedMilliseconds);
            writer.WritePropertyName("incrementalBuildElapsedMilliseconds");
            writer.WriteStartArray();
            foreach (var elapsed in report.IncrementalBuildElapsedMilliseconds)
                writer.WriteNumberValue(elapsed);

            writer.WriteEndArray();
            writer.WriteNumber("incrementalBuildP95Milliseconds", report.IncrementalBuildP95Milliseconds);

            writer.WritePropertyName("artifacts");
            writer.WriteStartArray();
            foreach (var artifact in report.Artifacts)
            {
                writer.WriteStartObject();
                writer.WriteString("relativePath", artifact.RelativePath);
                writer.WriteString("kind", artifact.Kind);
                writer.WriteNumber("bytes", artifact.Bytes);
                writer.WriteNumber("gzipBytes", artifact.GzipBytes);
                writer.WriteString("sha256", artifact.Sha256);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            WriteStringArray(writer, "notes", report.Notes);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteStringArray(
        Utf8JsonWriter writer,
        string propertyName,
        IReadOnlyList<string> values)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartArray();
        foreach (var value in values)
            writer.WriteStringValue(value);

        writer.WriteEndArray();
    }
}

internal static class GeneratedArtifactBenchmarkRunner
{
    private static readonly GeneratedArtifactFixtureDefinition[] FixtureDefinitions =
    [
        new(
            Id: "plain-text",
            Description: "Static Razor component rendering text through RenderTreeBuilder.AddContent.",
            ComponentRelativePath: "components/plain-text.mjs",
            SourceMapRelativePath: "components/plain-text.mjs.map"),
        new(
            Id: "counter",
            Description: "Interactive Counter component with state, event handler, and re-render.",
            ComponentRelativePath: "components/counter.mjs",
            SourceMapRelativePath: "components/counter.mjs.map"),
        new(
            Id: "keyed-list-100",
            Description: "100-item keyed list fixture using a Razor foreach loop and a stable key attribute.",
            ComponentRelativePath: "components/keyed-list-100.mjs",
            SourceMapRelativePath: "components/keyed-list-100.mjs.map")
    ];

    public static async Task<GeneratedArtifactBenchmarkReport> RunAsync(
        string repoRoot,
        string outputDirectory,
        BenchmarkArguments options)
    {
        var workspaceRoot = Path.Combine(
            outputDirectory,
            "generated-artifacts-workspace-" + Guid.NewGuid().ToString("N"));
        var packageOutputDirectory = Path.Combine(workspaceRoot, "nupkg");
        var restorePackagesPath = Path.Combine(workspaceRoot, "restore-packages");
        var packageBuildOutputRoot = Path.Combine(workspaceRoot, "package-out");
        var packageBuildIntermediateRoot = Path.Combine(workspaceRoot, "package-obj");

        Directory.CreateDirectory(packageOutputDirectory);
        Directory.CreateDirectory(restorePackagesPath);
        Directory.CreateDirectory(packageBuildOutputRoot);
        Directory.CreateDirectory(packageBuildIntermediateRoot);

        var dotNetVersionResult = await RunDotNetAsync(repoRoot, ["--version"]);
        if (dotNetVersionResult.ExitCode != 0)
            throw new InvalidOperationException("Failed to read .NET SDK version." + Environment.NewLine + dotNetVersionResult);

        var packageStopwatch = Stopwatch.StartNew();
        var pack = await RunDotNetAsync(
            repoRoot,
            [
                "pack",
                Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj"),
                "-c",
                "Debug",
                "-o",
                packageOutputDirectory,
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestorePackagesPath={restorePackagesPath}",
                $"-p:NuGetPackageRoot={ScriptHelpers.EnsureTrailingDirectorySeparator(restorePackagesPath)}",
                $"-p:JazorIsolatedBaseOutputRoot={ScriptHelpers.EnsureTrailingDirectorySeparator(packageBuildOutputRoot)}",
                $"-p:JazorIsolatedBaseIntermediateOutputRoot={ScriptHelpers.EnsureTrailingDirectorySeparator(packageBuildIntermediateRoot)}",
                "/nr:false",
                "/p:UseSharedCompilation=false"
            ]);
        if (pack.ExitCode != 0)
            throw new InvalidOperationException("Failed to pack local Jazor package for generated artifact benchmark." + Environment.NewLine + pack);
        if (pack.ToString().Contains("NU5118", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Local Jazor package emitted duplicate pack warnings." + Environment.NewLine + pack);

        var packageVersion = DiscoverPackageVersion(packageOutputDirectory, "Jazor");
        var jazorVuePack = await RunDotNetAsync(
            repoRoot,
            [
                "pack",
                Path.Combine(repoRoot, "src", "Jazor.Vue", "Jazor.Vue.csproj"),
                "-c",
                "Debug",
                "-o",
                packageOutputDirectory,
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestorePackagesPath={restorePackagesPath}",
                $"-p:NuGetPackageRoot={ScriptHelpers.EnsureTrailingDirectorySeparator(restorePackagesPath)}",
                $"-p:JazorIsolatedBaseOutputRoot={ScriptHelpers.EnsureTrailingDirectorySeparator(packageBuildOutputRoot)}",
                $"-p:JazorIsolatedBaseIntermediateOutputRoot={ScriptHelpers.EnsureTrailingDirectorySeparator(packageBuildIntermediateRoot)}",
                "/nr:false",
                "/p:UseSharedCompilation=false"
            ]);
        if (jazorVuePack.ExitCode != 0)
            throw new InvalidOperationException("Failed to pack local Jazor.Vue package for generated artifact benchmark." + Environment.NewLine + jazorVuePack);
        if (jazorVuePack.ToString().Contains("NU5118", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Local Jazor.Vue package emitted duplicate pack warnings." + Environment.NewLine + jazorVuePack);

        var jazorVuePackageVersion = DiscoverPackageVersion(packageOutputDirectory, "Jazor.Vue");
        if (!string.Equals(packageVersion, jazorVuePackageVersion, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Jazor and Jazor.Vue package versions must match for the generated artifact benchmark. " +
                $"Jazor={packageVersion}, Jazor.Vue={jazorVuePackageVersion}.");
        }

        packageStopwatch.Stop();

        var projectRoot = Path.Combine(workspaceRoot, "ExternalRazorSgGeneratedArtifactConsumer");
        var projectPath = CreateExternalRazorSgGeneratedArtifactConsumerProject(projectRoot);

        var buildStopwatch = Stopwatch.StartNew();
        var build = await RunDotNetAsync(
            repoRoot,
            [
                "build",
                projectPath,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={packageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={restorePackagesPath}",
                $"-p:JazorPackageVersion={packageVersion}",
                "/nr:false",
                "/p:UseSharedCompilation=false"
            ]);
        buildStopwatch.Stop();
        if (build.ExitCode != 0)
            throw new InvalidOperationException("Failed to build external Razor SG three-fixture consumer for generated artifact benchmark." + Environment.NewLine + build);

        var incrementalBuildElapsedMilliseconds = new List<long>(options.Samples);
        for (var sample = 0; sample < options.Samples; sample++)
        {
            var incrementalStopwatch = Stopwatch.StartNew();
            var incrementalBuild = await RunDotNetAsync(
                repoRoot,
                [
                    "build",
                    projectPath,
                    "/m:1",
                    "/p:BuildInParallel=false",
                    $"-p:RestoreSources={packageOutputDirectory}",
                    "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                    $"-p:RestorePackagesPath={restorePackagesPath}",
                    $"-p:JazorPackageVersion={packageVersion}",
                    "/nr:false",
                    "/p:UseSharedCompilation=false"
                ]);
            incrementalStopwatch.Stop();
            if (incrementalBuild.ExitCode != 0)
                throw new InvalidOperationException("Failed incremental build sample " + sample.ToString(CultureInfo.InvariantCulture) + " for generated artifact benchmark." + Environment.NewLine + incrementalBuild);

            incrementalBuildElapsedMilliseconds.Add(incrementalStopwatch.ElapsedMilliseconds);
        }

        var outputRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        var artifacts = ReadArtifactMeasurements(outputRoot);
        AssertRequiredArtifacts(artifacts, FixtureDefinitions);
        AssertCanonicalArtifacts(outputRoot, projectRoot);
        var fixtureMeasurements = BuildFixtureMeasurements(FixtureDefinitions, artifacts);

        return new GeneratedArtifactBenchmarkReport(
            SchemaVersion: "razorvue-g2-generated-artifacts-v1",
            Status: "partial-generated-artifact-measurement",
            Fixtures: fixtureMeasurements,
            DotNetSdkVersion: dotNetVersionResult.StandardOutput.Trim(),
            PackageVersion: packageVersion,
            WorkspaceRelativePath: ScriptHelpers.NormalizeRelativePath(repoRoot, workspaceRoot),
            OutputRootRelativePath: ScriptHelpers.NormalizeRelativePath(repoRoot, outputRoot),
            PackageElapsedMilliseconds: packageStopwatch.ElapsedMilliseconds,
            CleanBuildElapsedMilliseconds: buildStopwatch.ElapsedMilliseconds,
            IncrementalBuildElapsedMilliseconds: incrementalBuildElapsedMilliseconds,
            IncrementalBuildP95Milliseconds: CalculateP95(incrementalBuildElapsedMilliseconds),
            Artifacts: artifacts,
            Notes:
            [
                "This report uses an external Microsoft.NET.Sdk.Razor consumer with the SDK Razor source generator.",
                "It records generated artifact size, gzip size, and SHA256 for the active official Razor SG -> IOperation -> Jazor.Compiler -> Vue render-function .mjs path.",
                "Each fixture includes a same-machine handwritten Vue h() .mjs baseline (shared Vue/runtime dependencies excluded) and generated/handwritten gzip ratio.",
                "Runtime and CLR module artifacts are reported separately from per-fixture component modules so later G2 threshold comparisons can isolate shared dependencies.",
                "Clean build elapsed is the cold official Razor SG consumer build after package creation; incremental p95 is computed from no-op builds of the same consumer.",
                "Partial G2 evidence only: this does not cover browser DOM patching, retained heap snapshots, or fixed retired-line worktree baseline."
            ]);
    }

    private static long CalculateP95(IReadOnlyList<long> values)
    {
        if (values.Count == 0)
            return 0;

        var ordered = values.OrderBy(static value => value).ToArray();
        var index = (int)Math.Ceiling(ordered.Length * 0.95) - 1;
        return ordered[Math.Clamp(index, 0, ordered.Length - 1)];
    }

    private static async Task<ProcessResult> RunDotNetAsync(
        string workingDirectory,
        IReadOnlyList<string> arguments)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        var repoRoot = ScriptHelpers.RequireRepoRoot();
        var dotnetHome = Path.Combine(repoRoot, ".dotnet");
        Directory.CreateDirectory(dotnetHome);
        startInfo.Environment["DOTNET_CLI_HOME"] = dotnetHome;
        startInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        startInfo.Environment["UseSharedCompilation"] = "false";

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start dotnet.");
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new ProcessResult(
            process.ExitCode,
            await standardOutput,
            await standardError);
    }

    private static string CreateExternalRazorSgGeneratedArtifactConsumerProject(string projectRoot)
    {
        Directory.CreateDirectory(projectRoot);

        var projectPath = Path.Combine(projectRoot, "ExternalRazorSgGeneratedArtifactConsumer.csproj");
        File.WriteAllText(
            projectPath,
            """
            <Project Sdk="Microsoft.NET.Sdk.Razor">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <RazorLangVersion>11.0</RazorLangVersion>
                <UseRazorSourceGenerator>true</UseRazorSourceGenerator>
                <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
                <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
                <JazorMode>debug</JazorMode>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
                <PackageReference Include="Jazor.Vue" Version="$(JazorPackageVersion)" PrivateAssets="all" />
              </ItemGroup>

              <ItemGroup>
                <FrameworkReference Include="Microsoft.AspNetCore.App" />
                <CompilerVisibleProperty Include="JazorRazorVueTestHook" />
              </ItemGroup>
            </Project>
            """);

        File.WriteAllText(
            Path.Combine(projectRoot, "Program.cs"),
            """
            namespace ExternalRazorSgGeneratedArtifactConsumer;

            internal static class Program
            {
                private static void Main()
                {
                }
            }
            """);

        File.WriteAllText(
            Path.Combine(projectRoot, "Counter.razor.cs"),
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;

            namespace ExternalRazorSgGeneratedArtifactConsumer;

            [ECMAScriptModule("./components/counter")]
            public partial class Counter : ComponentBase, IVueComponent
            {
                private int count;

                private void Increment()
                {
                    count++;
                }
            }
            """);

        File.WriteAllText(
            Path.Combine(projectRoot, "_Imports.razor"),
            """
            @using Microsoft.AspNetCore.Components.Web
            """);

        File.WriteAllText(
            Path.Combine(projectRoot, "Counter.razor"),
            """
            <button @onclick="Increment">Clicks: @count</button>
            """);

        File.WriteAllText(
            Path.Combine(projectRoot, "PlainText.razor.cs"),
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;

            namespace ExternalRazorSgGeneratedArtifactConsumer;

            [ECMAScriptModule("./components/plain-text")]
            public partial class PlainText : ComponentBase, IVueComponent
            {
                private string text = "Hello RazorVue";
            }
            """);

        File.WriteAllText(
            Path.Combine(projectRoot, "PlainText.razor"),
            """
            @text
            """);

        File.WriteAllText(
            Path.Combine(projectRoot, "KeyedList100.razor.cs"),
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;

            namespace ExternalRazorSgGeneratedArtifactConsumer;

            [ECMAScriptModule("./components/keyed-list-100")]
            public partial class KeyedList100 : ComponentBase, IVueComponent
            {
                private string[] items = new[]
                {
            """ +
            string.Join(
                "," + Environment.NewLine,
                Enumerable
                    .Range(0, 100)
                    .Select(static index => "        \"Item " + index.ToString(CultureInfo.InvariantCulture) + "\"")) +
            Environment.NewLine +
            """
                };
            }
            """);

        File.WriteAllText(
            Path.Combine(projectRoot, "KeyedList100.razor"),
            """
            <ul>
            @foreach (var item in items)
            {
                <li key="@item">@item</li>
            }
            </ul>
            """);

        return projectPath;
    }

    private static string DiscoverPackageVersion(string packageOutputDirectory, string packageId)
    {
        var prefix = packageId + ".";
        var nupkg = Directory
            .GetFiles(packageOutputDirectory, $"{packageId}.*.nupkg")
            .Where(static path => !path.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(static path => File.GetLastWriteTimeUtc(path))
            .FirstOrDefault();

        if (nupkg is null)
            throw new FileNotFoundException("Packed package was not found for package id '" + packageId + "'.", packageOutputDirectory);

        var fileName = Path.GetFileName(nupkg);
        if (!fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            !fileName.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Unexpected package file name: " + fileName);

        return fileName[prefix.Length..^".nupkg".Length];
    }

    private static GeneratedArtifactMeasurement[] ReadArtifactMeasurements(string outputRoot)
    {
        if (!Directory.Exists(outputRoot))
            throw new DirectoryNotFoundException("Generated artifact output root was not found: " + outputRoot);

        return Directory
            .EnumerateFiles(outputRoot, "*", SearchOption.AllDirectories)
            .Select(filePath =>
            {
                var relativePath = ScriptHelpers.NormalizeRelativePath(outputRoot, filePath);
                var bytes = File.ReadAllBytes(filePath);
                var sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
                return new GeneratedArtifactMeasurement(
                    relativePath,
                    ClassifyArtifact(relativePath),
                    bytes.LongLength,
                    CountGzipBytes(bytes),
                    sha256);
            })
            .OrderBy(static artifact => artifact.RelativePath, StringComparer.Ordinal)
            .ToArray();
    }

    private static string ClassifyArtifact(string relativePath)
    {
        if (string.Equals(relativePath, "jazor-manifest.json", StringComparison.Ordinal))
            return "manifest";
        if (relativePath.StartsWith("@jazor/vue-runtime/", StringComparison.Ordinal))
            return "runtime";
        if (relativePath.StartsWith("System/", StringComparison.Ordinal))
            return "clr-runtime";
        if (relativePath.EndsWith(".mjs.map", StringComparison.Ordinal))
            return "source-map";
        if (relativePath.EndsWith(".mjs", StringComparison.Ordinal))
            return "component-module";

        return "other";
    }

    private static int CountGzipBytes(byte[] bytes)
    {
        using var stream = new MemoryStream();
        using (var gzip = new GZipStream(stream, CompressionLevel.SmallestSize, leaveOpen: true))
            gzip.Write(bytes, 0, bytes.Length);

        return checked((int)stream.Length);
    }

    private static GeneratedArtifactFixtureMeasurement[] BuildFixtureMeasurements(
        IReadOnlyList<GeneratedArtifactFixtureDefinition> fixtures,
        IReadOnlyList<GeneratedArtifactMeasurement> artifacts)
        => fixtures
            .Select(fixture =>
            {
                var component = RequireArtifact(artifacts, fixture.ComponentRelativePath);
                var sourceMap = RequireArtifact(artifacts, fixture.SourceMapRelativePath);
                var handwritten = CreateHandwrittenBaseline(fixture.Id);
                var handwrittenBytes = Encoding.UTF8.GetByteCount(handwritten);
                var handwrittenGzip = CountGzipBytes(Encoding.UTF8.GetBytes(handwritten));
                var gzipRatio = handwrittenGzip == 0
                    ? 0d
                    : (double)component.GzipBytes / handwrittenGzip;
                return new GeneratedArtifactFixtureMeasurement(
                    fixture.Id,
                    fixture.Description,
                    fixture.ComponentRelativePath,
                    component.Bytes,
                    component.GzipBytes,
                    component.Sha256,
                    fixture.SourceMapRelativePath,
                    sourceMap.Bytes,
                    sourceMap.GzipBytes,
                    sourceMap.Sha256,
                    handwrittenBytes,
                    handwrittenGzip,
                    gzipRatio);
            })
            .ToArray();

    private static string CreateHandwrittenBaseline(string fixtureId)
        => fixtureId switch
        {
            "plain-text" =>
                """
                export default {
                  setup() {
                    return () => "Hello RazorVue";
                  }
                };
                """,
            "counter" =>
                """
                import { h, ref } from "vue";

                export default {
                  setup() {
                    const count = ref(0);
                    const increment = () => {
                      count.value++;
                    };

                    return () => h("button", { onClick: increment }, ["Clicks: ", count.value]);
                  }
                };
                """,
            "keyed-list-100" =>
                """
                import { h } from "vue";

                const items = Array.from({ length: 100 }, (_, index) => `Item ${index}`);

                export default {
                  setup() {
                    return () => h(
                      "ul",
                      null,
                      items.map((item) => h("li", { key: item }, item)));
                  }
                };
                """,
            _ => throw new InvalidOperationException("Unknown generated-artifact fixture id: " + fixtureId)
        };

    private static void AssertRequiredArtifacts(
        IReadOnlyList<GeneratedArtifactMeasurement> artifacts,
        IReadOnlyList<GeneratedArtifactFixtureDefinition> fixtures)
    {
        var paths = artifacts.Select(static artifact => artifact.RelativePath).ToArray();
        RequireArtifact(paths, "jazor-manifest.json");
        foreach (var fixture in fixtures)
        {
            RequireArtifact(paths, fixture.ComponentRelativePath);
            RequireArtifact(paths, fixture.SourceMapRelativePath);
        }
    }

    private static void RequireArtifact(IReadOnlyList<string> paths, string relativePath)
    {
        if (!paths.Contains(relativePath, StringComparer.Ordinal))
            throw new FileNotFoundException("Expected generated artifact was not produced: " + relativePath, relativePath);
    }

    private static GeneratedArtifactMeasurement RequireArtifact(
        IReadOnlyList<GeneratedArtifactMeasurement> artifacts,
        string relativePath)
        => artifacts.FirstOrDefault(artifact => string.Equals(artifact.RelativePath, relativePath, StringComparison.Ordinal))
            ?? throw new FileNotFoundException("Expected generated artifact was not produced: " + relativePath, relativePath);

    private static void AssertCanonicalArtifacts(string outputRoot, string projectRoot)
    {
        var manifestPath = Path.Combine(outputRoot, "jazor-manifest.json");
        var manifestText = File.ReadAllText(manifestPath);
        if (manifestText.Contains("generatedAtUtc", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Generated manifest must not contain generatedAtUtc.");
        if (manifestText.Contains("rootAssemblyPath", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Generated manifest must not contain rootAssemblyPath.");

        foreach (var filePath in Directory.EnumerateFiles(outputRoot, "*", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(filePath);
            if (text.Contains(projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                var relativePath = ScriptHelpers.NormalizeRelativePath(outputRoot, filePath);
                throw new InvalidOperationException(
                    "Generated artifact must not persist the external consumer absolute project path: " + relativePath);
            }
        }
    }
}

internal sealed record ReleasePerformanceReport(
    string SchemaVersion,
    string Status,
    string Commit,
    string RetiredLineRef,
    string RetiredLineCommit,
    bool RetiredLineHasHomogeneousProtocol,
    string RetiredLineBaselineStatus,
    RuntimeBenchmarkReport Runtime,
    GeneratedArtifactBenchmarkReport GeneratedArtifacts,
    BrowserBenchmarkReport Browser,
    IReadOnlyList<ThresholdEvaluation> Thresholds,
    IReadOnlyList<string> Notes)
{
    public static ReleasePerformanceReport Create(
        string repoRoot,
        BenchmarkArguments options,
        RuntimeBenchmarkReport runtime,
        GeneratedArtifactBenchmarkReport generatedArtifacts,
        BrowserBenchmarkReport browser,
        OldLineBaselineProbeResult oldLine)
    {
        var thresholds = BuildThresholds(runtime, generatedArtifacts, browser, oldLine);
        var status = thresholds.Any(static threshold => threshold.Status == "warn")
            ? "baseline-recorded-with-warnings"
            : "baseline-recorded";

        return new ReleasePerformanceReport(
            SchemaVersion: "razorvue-g2-release-performance-v1",
            Status: status,
            Commit: ReadGit(repoRoot, ["rev-parse", "HEAD"]).Trim(),
            RetiredLineRef: oldLine.Ref,
            RetiredLineCommit: oldLine.Commit,
            RetiredLineHasHomogeneousProtocol: oldLine.HasHomogeneousProtocol,
            RetiredLineBaselineStatus: oldLine.Status,
            Runtime: runtime,
            GeneratedArtifacts: generatedArtifacts,
            Browser: browser,
            Thresholds: thresholds,
            Notes:
            [
                "This report is the first G2 performance baseline for the active RazorVue render-function path.",
                "Runtime measurements compare render-context protocol calls with same-machine handwritten h() baselines.",
                "Generated artifact measurements use an external official Razor SG consumer built from the local Jazor package.",
                "Browser measurements run the render-context protocol in a real browser and record JS heap deltas when performance.memory is available.",
                "Old-line comparison is recorded as an availability probe. The default v0.1.26 ref predates this G2 protocol, so numeric old-line thresholds remain unavailable until a fixed old-line worktree exposes the same measurement contract."
            ]);
    }

    public string ToMarkdown()
    {
        var writer = new StringWriter();
        writer.WriteLine("# RazorVue G2 Release Performance Report");
        writer.WriteLine();
        writer.WriteLine("- Schema: " + SchemaVersion);
        writer.WriteLine("- Status: " + Status);
        writer.WriteLine("- Commit: `" + Commit + "`");
        writer.WriteLine("- Retired-line ref: `" + RetiredLineRef + "`");
        writer.WriteLine("- Retired-line commit: `" + RetiredLineCommit + "`");
        writer.WriteLine("- Retired-line protocol available: " + (RetiredLineHasHomogeneousProtocol ? "yes" : "no"));
        writer.WriteLine("- Retired-line baseline status: " + RetiredLineBaselineStatus);
        writer.WriteLine();
        writer.WriteLine("## Threshold Summary");
        writer.WriteLine();
        writer.WriteLine("| Threshold | Status | Detail |");
        writer.WriteLine("|-----------|--------|--------|");
        foreach (var threshold in Thresholds)
            writer.WriteLine("| `" + threshold.Id + "` | " + threshold.Status + " | " + threshold.Detail + " |");

        writer.WriteLine();
        writer.WriteLine("## Runtime Protocol");
        writer.WriteLine();
        writer.WriteLine("| Fixture | Render ratio | Update ratio | Node heap delta median |");
        writer.WriteLine("|---------|--------------|--------------|------------------------|");
        foreach (var fixture in Runtime.Fixtures)
        {
            writer.WriteLine(
                "| `" + fixture.Id + "`" +
                " | " + fixture.RenderThroughputRatio.ToString("0.000", CultureInfo.InvariantCulture) +
                " | " + fixture.UpdateThroughputRatio.ToString("0.000", CultureInfo.InvariantCulture) +
                " | " + fixture.RetainedHeapDeltaBytes.Median.ToString(CultureInfo.InvariantCulture) +
                " |");
        }

        writer.WriteLine();
        writer.WriteLine("## Generated Artifacts");
        writer.WriteLine();
        writer.WriteLine("- Package version: `" + GeneratedArtifacts.PackageVersion + "`");
        writer.WriteLine("- Clean build: " + GeneratedArtifacts.CleanBuildElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) + " ms");
        writer.WriteLine("- Incremental p95: " + GeneratedArtifacts.IncrementalBuildP95Milliseconds.ToString(CultureInfo.InvariantCulture) + " ms");
        writer.WriteLine();
        writer.WriteLine("| Fixture | Component gzip | Handwritten gzip | Gzip ratio |");
        writer.WriteLine("|---------|----------------|------------------|------------|");
        foreach (var fixture in GeneratedArtifacts.Fixtures)
        {
            writer.WriteLine(
                "| `" + fixture.Id + "`" +
                " | " + fixture.ComponentGzipBytes.ToString(CultureInfo.InvariantCulture) +
                " | " + fixture.HandwrittenGzipBytes.ToString(CultureInfo.InvariantCulture) +
                " | " + fixture.GzipRatio.ToString("0.000", CultureInfo.InvariantCulture) +
                " |");
        }

        writer.WriteLine();
        writer.WriteLine("## Browser");
        writer.WriteLine();
        writer.WriteLine("- Status: " + Browser.Status);
        writer.WriteLine("- Browser: `" + Browser.BrowserPath + "`");
        writer.WriteLine();
        writer.WriteLine("| Fixture | Render ratio | Update ratio | Heap available | Heap delta median |");
        writer.WriteLine("|---------|--------------|--------------|----------------|-------------------|");
        foreach (var fixture in Browser.Fixtures)
        {
            writer.WriteLine(
                "| `" + fixture.Id + "`" +
                " | " + fixture.RenderThroughputRatio.ToString("0.000", CultureInfo.InvariantCulture) +
                " | " + fixture.UpdateThroughputRatio.ToString("0.000", CultureInfo.InvariantCulture) +
                " | " + (fixture.HeapMeasurementAvailable ? "yes" : "no") +
                " | " + fixture.RetainedHeapDeltaBytes.Median.ToString(CultureInfo.InvariantCulture) +
                " |");
        }

        writer.WriteLine();
        writer.WriteLine("## Notes");
        writer.WriteLine();
        foreach (var note in Notes)
            writer.WriteLine("- " + note);

        return writer.ToString();
    }

    private static ThresholdEvaluation[] BuildThresholds(
        RuntimeBenchmarkReport runtime,
        GeneratedArtifactBenchmarkReport generatedArtifacts,
        BrowserBenchmarkReport browser,
        OldLineBaselineProbeResult oldLine)
    {
        var generatedMaxGzip = generatedArtifacts.Fixtures.Count == 0
            ? 0
            : generatedArtifacts.Fixtures.Max(static fixture => fixture.GzipRatio);
        var runtimeMinRatio = runtime.Fixtures.Count == 0
            ? 0
            : runtime.Fixtures.Min(static fixture => Math.Min(fixture.RenderThroughputRatio, fixture.UpdateThroughputRatio));
        var browserMinRatio = browser.Fixtures.Count == 0
            ? 0
            : browser.Fixtures.Min(static fixture => Math.Min(fixture.RenderThroughputRatio, fixture.UpdateThroughputRatio));
        var browserHeapAvailable = browser.Fixtures.Count > 0 && browser.Fixtures.All(static fixture => fixture.HeapMeasurementAvailable);

        return
        [
            new(
                "generated-gzip",
                generatedMaxGzip <= 2.0 ? "pass" : "warn",
                "max generated/handwritten fixture gzip ratio = " + generatedMaxGzip.ToString("0.000", CultureInfo.InvariantCulture) + " (limit 2.000)"),
            new(
                "node-throughput",
                runtimeMinRatio >= 0.70 ? "pass" : "warn",
                "min Node render/update ratio = " + runtimeMinRatio.ToString("0.000", CultureInfo.InvariantCulture) + " (limit 0.700)"),
            new(
                "browser-throughput",
                browser.Fixtures.Count == 0 ? "unavailable" : "observed",
                browser.Fixtures.Count == 0
                    ? "headless browser measurement did not run"
                    : "min browser render/update ratio = " + browserMinRatio.ToString("0.000", CultureInfo.InvariantCulture) + "; this lane is used for real-browser heap/timer trend evidence, not a hard throughput threshold"),
            new(
                "browser-heap",
                browser.Fixtures.Count == 0 || !browserHeapAvailable ? "unavailable" : "observed",
                browser.Fixtures.Count == 0
                    ? "headless browser measurement did not run"
                    : browserHeapAvailable
                        ? "all browser fixtures reported JS heap deltas"
                        : "browser did not expose performance.memory.usedJSHeapSize for every fixture"),
            new(
                "incremental-compile-old-line",
                oldLine.HasHomogeneousProtocol ? "observed" : "unavailable",
                oldLine.HasHomogeneousProtocol
                    ? "retired-line ref exposes the G2 protocol; compare incremental p95 externally"
                    : "retired-line ref does not expose the same G2 measurement protocol"),
            new(
                "node-retained-heap",
                "observed",
                "Node retained heap delta medians recorded for " + runtime.Fixtures.Count.ToString(CultureInfo.InvariantCulture) + " fixtures")
        ];
    }

    private static string ReadGit(string repoRoot, IReadOnlyList<string> arguments)
    {
        var result = OldLineBaselineProbe.RunGit(repoRoot, arguments);
        return result.ExitCode == 0 ? result.StandardOutput : "";
    }
}

internal sealed record ThresholdEvaluation(
    string Id,
    string Status,
    string Detail);

internal sealed record OldLineBaselineProbeResult(
    string Ref,
    string Commit,
    bool HasHomogeneousProtocol,
    string Status,
    IReadOnlyList<string> Notes);

internal static class OldLineBaselineProbe
{
    public static OldLineBaselineProbeResult Probe(string repoRoot, string retiredLineRef)
    {
        var commitResult = RunGit(repoRoot, ["rev-parse", retiredLineRef]);
        if (commitResult.ExitCode != 0)
        {
            return new OldLineBaselineProbeResult(
                retiredLineRef,
                "",
                false,
                "ref-unavailable",
                [commitResult.StandardError.Trim()]);
        }

        var benchmarkScriptResult = RunGit(repoRoot, ["cat-file", "-e", retiredLineRef + ":scripts/csharp/benchmark-razorvue-g2.cs"]);
        var hasProtocol = benchmarkScriptResult.ExitCode == 0;
        return new OldLineBaselineProbeResult(
            retiredLineRef,
            commitResult.StandardOutput.Trim(),
            hasProtocol,
            hasProtocol ? "homogeneous-protocol-available" : "homogeneous-protocol-unavailable",
            hasProtocol
                ? ["The retired-line ref contains scripts/csharp/benchmark-razorvue-g2.cs and can be sampled separately."]
                : ["The retired-line ref does not contain the G2 benchmark script, so numeric threshold comparison would not be same-protocol evidence."]);
    }

    public static ProcessResult RunGit(string repoRoot, IReadOnlyList<string> arguments)
    {
        var startInfo = new ProcessStartInfo("git")
        {
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start git.");
        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return new ProcessResult(process.ExitCode, standardOutput, standardError);
    }
}

internal static class ReleasePerformanceReportJson
{
    public static string Write(ReleasePerformanceReport report)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteString("schemaVersion", report.SchemaVersion);
            writer.WriteString("status", report.Status);
            writer.WriteString("commit", report.Commit);
            writer.WriteString("retiredLineRef", report.RetiredLineRef);
            writer.WriteString("retiredLineCommit", report.RetiredLineCommit);
            writer.WriteBoolean("retiredLineHasHomogeneousProtocol", report.RetiredLineHasHomogeneousProtocol);
            writer.WriteString("retiredLineBaselineStatus", report.RetiredLineBaselineStatus);
            writer.WritePropertyName("thresholds");
            writer.WriteStartArray();
            foreach (var threshold in report.Thresholds)
            {
                writer.WriteStartObject();
                writer.WriteString("id", threshold.Id);
                writer.WriteString("status", threshold.Status);
                writer.WriteString("detail", threshold.Detail);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            WriteStringArray(writer, "notes", report.Notes);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteStringArray(Utf8JsonWriter writer, string propertyName, IReadOnlyList<string> values)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartArray();
        foreach (var value in values)
            writer.WriteStringValue(value);

        writer.WriteEndArray();
    }
}

internal sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
{
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.AppendLine("ExitCode: " + ExitCode.ToString(CultureInfo.InvariantCulture));

        if (!string.IsNullOrWhiteSpace(StandardOutput))
        {
            builder.AppendLine("STDOUT:");
            builder.AppendLine(StandardOutput);
        }

        if (!string.IsNullOrWhiteSpace(StandardError))
        {
            builder.AppendLine("STDERR:");
            builder.AppendLine(StandardError);
        }

        return builder.ToString();
    }
}

internal static class ScriptHelpers
{
    public static string RequireRepoRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root containing Jazor.slnx was not found from the current directory upward.");
    }

    public static string EnsureTrailingDirectorySeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;

    public static string NormalizeRelativePath(string root, string path)
        => Path.GetRelativePath(root, path).Replace('\\', '/');
}
