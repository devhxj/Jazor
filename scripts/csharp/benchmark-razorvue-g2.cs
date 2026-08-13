#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

var options = BenchmarkOptions.Parse(args);
if (options.DryRun)
{
    Console.WriteLine(BenchmarkProtocol.Create(options).ToMarkdown());
    return;
}

var repoRoot = RequireRepositoryRoot();
var outputDirectory = Path.GetFullPath(options.OutputDirectory ?? Path.Combine(repoRoot, ".tmp", "razorvue-g2-benchmark"));
Directory.CreateDirectory(outputDirectory);

var protocol = BenchmarkProtocol.Create(options);
WriteText(Path.Combine(outputDirectory, "razorvue-g2-benchmark-protocol.md"), protocol.ToMarkdown());
WriteText(Path.Combine(outputDirectory, "razorvue-g2-benchmark-protocol.json"), JsonSerializer.Serialize(protocol, new JsonSerializerOptions { WriteIndented = true }));

if (options.MeasureRuntime)
{
    var report = await RunNodeBenchmarkAsync(repoRoot, options);
    WriteText(Path.Combine(outputDirectory, "razorvue-g2-direct-runtime-report.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
    WriteText(Path.Combine(outputDirectory, "razorvue-g2-direct-runtime-report.md"), report.ToMarkdown());
    Console.WriteLine(report.ToMarkdown());
}
else
{
    Console.WriteLine(protocol.ToMarkdown());
}

if (options.MeasureBrowser)
{
    var browserReport = await RunBrowserProbeAsync(repoRoot, options);
    WriteText(Path.Combine(outputDirectory, "razorvue-g2-direct-browser-report.json"), JsonSerializer.Serialize(browserReport, new JsonSerializerOptions { WriteIndented = true }));
    WriteText(Path.Combine(outputDirectory, "razorvue-g2-direct-browser-report.md"), browserReport.ToMarkdown());
    Console.WriteLine(browserReport.ToMarkdown());
}

static string RequireRepositoryRoot()
{
    var directory = new DirectoryInfo(Environment.CurrentDirectory);
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;
        directory = directory.Parent;
    }

    throw new InvalidOperationException("Could not locate Jazor.slnx from the current directory.");
}

static void WriteText(string path, string content)
{
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, content, new UTF8Encoding(false));
}

static async Task<DirectRuntimeReport> RunNodeBenchmarkAsync(string repoRoot, BenchmarkOptions options)
{
    var directory = Path.Combine(repoRoot, ".tmp", "razorvue-g2-benchmark", "node-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(directory);
    var scriptPath = Path.Combine(directory, "direct-benchmark.mjs");
    WriteText(scriptPath, CreateNodeBenchmark(options));

    var result = await RunProcessAsync("node", "--expose-gc \"" + scriptPath + "\"");
    if (result.ExitCode != 0)
        throw new InvalidOperationException("Direct render Node benchmark failed." + Environment.NewLine + result.StandardError);

    // Node emits camelCase JSON while the C# report keeps PascalCase record members.
    // Keep this boundary explicit so protocol naming does not affect benchmark parsing.
    return JsonSerializer.Deserialize<DirectRuntimeReport>(
        result.StandardOutput,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new InvalidOperationException("Node benchmark returned no report.");
}

static string CreateNodeBenchmark(BenchmarkOptions options)
{
    var samples = options.Samples.ToString(CultureInfo.InvariantCulture);
    var iterations = options.Iterations.ToString(CultureInfo.InvariantCulture);
    return $$"""
        import { performance } from "node:perf_hooks";
        import { gzipSync } from "node:zlib";

        const samples = {{samples}};
        const iterations = {{iterations}};

        // These helpers model the final Vue render-function calls emitted by RazorVue.
        // They intentionally do not mount DOM, run patch(), hydrate, or load a Vue runtime.
        const h = (type, props, children) => ({ type, props: props ?? null, children });
        const openBlock = () => null;
        const createElementBlock = (type, props, children, patchFlag, dynamicProps) =>
          ({ type, props: props ?? null, children, patchFlag, dynamicProps });
        const createStaticVNode = (html, count) => ({ type: "static", html, count });

        const generatedPlain = () => "Hello RazorVue";
        const handwrittenPlain = () => "Hello RazorVue";

        function createGeneratedCounter() {
          const state = { count: 0 };
          const handlerCache = [];
          return {
            state,
            render() {
              return openBlock(), createElementBlock(
                "button",
                { onClick: handlerCache[0] || (handlerCache[0] = () => state.count++) },
                ["Clicks: ", state.count],
                8,
                ["onClick"]);
            }
          };
        }

        function createHandwrittenCounter() {
          const state = { count: 0 };
          const onClick = () => state.count++;
          return {
            state,
            render() {
              return h("button", { onClick }, ["Clicks: ", state.count]);
            }
          };
        }

        function createGeneratedList() {
          const items = Array.from({ length: 100 }, (_, index) => "Item " + index);
          return () => (
            openBlock(),
            createElementBlock(
              "ul",
              null,
              items.map(item => h("li", { key: item }, item)))
          );
        }

        function createHandwrittenList() {
          const items = Array.from({ length: 100 }, (_, index) => "Item " + index);
          return () => h("ul", null, items.map(item => h("li", { key: item }, item)));
        }

        const generatedBodies = {
          "plain-text": "export const render = () => \"Hello RazorVue\";",
          counter: "openBlock(), createElementBlock(\"button\", { onClick: cache[0] || (cache[0] = handler) }, children, 8, [\"onClick\"]);",
          "keyed-list-100": "openBlock(), createElementBlock(\"ul\", null, items.map(item => h(\"li\", { key: item }, item)));"
        };
        const handwrittenBodies = {
          "plain-text": "export const render = () => \"Hello RazorVue\";",
          counter: "h(\"button\", { onClick: handler }, children);",
          "keyed-list-100": "h(\"ul\", null, items.map(item => h(\"li\", { key: item }, item)));"
        };

        const measure = operation => {
          operation();
          const values = [];
          for (let sample = 0; sample < samples; sample++) {
            const started = performance.now();
            let sink;
            for (let index = 0; index < iterations; index++) sink = operation(index);
            if (sink === undefined) throw new Error("benchmark operation returned undefined");
            values.push(iterations / Math.max((performance.now() - started) / 1000, 0.000001));
          }
          values.sort((a, b) => a - b);
          return { median: values[Math.floor(values.length / 2)], min: values[0], max: values[values.length - 1], samples: values };
        };

        const retained = operation => {
          if (typeof globalThis.gc === "function") globalThis.gc();
          const before = process.memoryUsage().heapUsed;
          let sink;
          for (let index = 0; index < iterations; index++) sink = operation(index);
          if (sink === undefined) throw new Error("retained operation returned undefined");
          if (typeof globalThis.gc === "function") globalThis.gc();
          return process.memoryUsage().heapUsed - before;
        };

        const generatedCounter = createGeneratedCounter();
        const handwrittenCounter = createHandwrittenCounter();
        const generatedList = createGeneratedList();
        const handwrittenList = createHandwrittenList();
        // Module hoists are created once when the generated artifact is evaluated. Returning
        // the same vnode here models RazorVue's createStaticVNode lowering rather than timing
        // the helper allocation on every render.
        const generatedStaticVNode = createStaticVNode("<strong>static</strong>", 1);
        const generatedStatic = () => generatedStaticVNode;
        const handwrittenStatic = () => h("strong", null, "static");

        const fixtures = [
          {
            id: "plain-text",
            generatedRender: measure(generatedPlain),
            handwrittenRender: measure(handwrittenPlain),
            generatedUpdate: measure(generatedPlain),
            handwrittenUpdate: measure(handwrittenPlain),
            generatedRetained: retained(generatedPlain),
            handwrittenRetained: retained(handwrittenPlain),
            generatedGzipBytes: gzipSync(Buffer.from(generatedBodies["plain-text"])).byteLength,
            handwrittenGzipBytes: gzipSync(Buffer.from(handwrittenBodies["plain-text"])).byteLength
          },
          {
            id: "counter",
            generatedRender: measure(() => generatedCounter.render()),
            handwrittenRender: measure(() => handwrittenCounter.render()),
            generatedUpdate: measure(() => { generatedCounter.state.count++; return generatedCounter.render(); }),
            handwrittenUpdate: measure(() => { handwrittenCounter.state.count++; return handwrittenCounter.render(); }),
            generatedRetained: retained(() => generatedCounter.render()),
            handwrittenRetained: retained(() => handwrittenCounter.render()),
            generatedGzipBytes: gzipSync(Buffer.from(generatedBodies.counter)).byteLength,
            handwrittenGzipBytes: gzipSync(Buffer.from(handwrittenBodies.counter)).byteLength
          },
          {
            id: "keyed-list-100",
            generatedRender: measure(generatedList),
            handwrittenRender: measure(handwrittenList),
            generatedUpdate: measure(generatedList),
            handwrittenUpdate: measure(handwrittenList),
            generatedRetained: retained(generatedList),
            handwrittenRetained: retained(handwrittenList),
            generatedGzipBytes: gzipSync(Buffer.from(generatedBodies["keyed-list-100"])).byteLength,
            handwrittenGzipBytes: gzipSync(Buffer.from(handwrittenBodies["keyed-list-100"])).byteLength
          },
          {
            id: "static-vnode",
            generatedRender: measure(generatedStatic),
            handwrittenRender: measure(handwrittenStatic),
            generatedUpdate: measure(generatedStatic),
            handwrittenUpdate: measure(handwrittenStatic),
            generatedRetained: retained(generatedStatic),
            handwrittenRetained: retained(handwrittenStatic),
            generatedGzipBytes: gzipSync(Buffer.from("createStaticVNode(\\\"<strong>static</strong>\\\", 1)")).byteLength,
            handwrittenGzipBytes: gzipSync(Buffer.from("h(\\\"strong\\\", null, \\\"static\\\")")).byteLength
          }
        ];

        const report = {
          schemaVersion: "razorvue-g2-direct-runtime-v1",
          status: "measured",
          nodeVersion: process.version,
          samples,
          iterations,
          scope: "direct render-function call shape; no DOM patch, hydration, or compile timing",
          fixtures
        };
        process.stdout.write(JSON.stringify(report));
        """;
}

static async Task<BrowserProbeReport> RunBrowserProbeAsync(string repoRoot, BenchmarkOptions options)
{
    var browser = ResolveBrowserExecutable();
    if (browser is null)
    {
        return new BrowserProbeReport(
            "razorvue-g2-direct-browser-v1",
            "unavailable",
            "",
            "No Edge/Chrome executable was found. Set RAZORVUE_BROWSER_EXE to enable this lane.",
            "direct render-function calls only; no DOM patch or hydration");
    }

    var directory = Path.Combine(repoRoot, ".tmp", "razorvue-g2-benchmark", "browser-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(directory);
    var htmlPath = Path.Combine(directory, "direct-benchmark.html");
    WriteText(htmlPath, CreateBrowserBenchmark(options));
    var result = await RunProcessAsync(browser, "--headless --disable-gpu --no-sandbox --allow-file-access-from-files --dump-dom \"" + htmlPath + "\"");
    if (result.ExitCode != 0)
        return new BrowserProbeReport("razorvue-g2-direct-browser-v1", "failed", browser, result.StandardError, "direct render-function calls only; no DOM patch or hydration");

    return new BrowserProbeReport("razorvue-g2-direct-browser-v1", "measured", browser, result.StandardOutput, "direct render-function calls only; no DOM patch or hydration");
}

static string CreateBrowserBenchmark(BenchmarkOptions options)
    => $$"""
        <!doctype html>
        <meta charset="utf-8">
        <body><pre id="result"></pre>
        <script>
        const iterations = {{options.Iterations.ToString(CultureInfo.InvariantCulture)}};
        const h = (type, props, children) => ({ type, props: props ?? null, children });
        const openBlock = () => null;
        const createElementBlock = (type, props, children, patchFlag, dynamicProps) => ({ type, props, children, patchFlag, dynamicProps });
        const createStaticVNode = (html, count) => ({ type: "static", html, count });
        const generatedStaticVNode = createStaticVNode("<strong>static</strong>", 1);
        const generated = () => (
          openBlock(),
          createElementBlock("div", { class: "direct" }, [generatedStaticVNode], 0, null)
        );
        const handwritten = () => h("div", { class: "direct" }, [h("strong", null, "static")]);
        const run = operation => { let sink; const start = performance.now(); for (let i = 0; i < iterations; i++) sink = operation(); return { ops: iterations / Math.max((performance.now() - start) / 1000, 0.000001), sample: sink }; };
        document.querySelector("#result").textContent = JSON.stringify({ schemaVersion: "razorvue-g2-direct-browser-v1", generated: run(generated).ops, handwritten: run(handwritten).ops, scope: "direct render-function calls only; no DOM patch or hydration" });
        </script>
        """;

static string? ResolveBrowserExecutable()
{
    var explicitPath = Environment.GetEnvironmentVariable("RAZORVUE_BROWSER_EXE")?.Trim();
    if (!string.IsNullOrWhiteSpace(explicitPath) && File.Exists(explicitPath))
        return explicitPath;

    var candidates = OperatingSystem.IsWindows()
        ? new[] { @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe", @"C:\Program Files\Microsoft\Edge\Application\msedge.exe", @"C:\Program Files\Google\Chrome\Application\chrome.exe" }
        : OperatingSystem.IsMacOS()
            ? new[] { "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge", "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" }
            : new[] { "microsoft-edge", "google-chrome", "chromium" };
    return candidates.FirstOrDefault(File.Exists);
}

static async Task<ProcessResult> RunProcessAsync(string fileName, string arguments)
{
    using var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        }
    };
    process.Start();
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    return new ProcessResult(process.ExitCode, await stdout, await stderr);
}

internal sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);

internal sealed record BenchmarkOptions(
    bool DryRun,
    bool MeasureRuntime,
    bool MeasureBrowser,
    string? OutputDirectory,
    int Samples,
    int Iterations)
{
    public static BenchmarkOptions Parse(string[] args)
    {
        var result = new BenchmarkOptions(true, false, false, null, 5, 10_000);
        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--dry-run": result = result with { DryRun = true }; break;
                case "--measure-runtime": result = result with { DryRun = false, MeasureRuntime = true }; break;
                case "--measure-browser": result = result with { DryRun = false, MeasureBrowser = true }; break;
                case "--out": result = result with { OutputDirectory = Next(args, ref index, "--out") }; break;
                case "--samples": result = result with { Samples = PositiveInt(Next(args, ref index, "--samples"), "--samples") }; break;
                case "--iterations": result = result with { Iterations = PositiveInt(Next(args, ref index, "--iterations"), "--iterations") }; break;
                case "--help":
                    Console.WriteLine("Usage: dotnet run --file scripts/csharp/benchmark-razorvue-g2.cs -- [--dry-run] [--measure-runtime] [--measure-browser] [--out DIR] [--samples N] [--iterations N]");
                    Environment.Exit(0);
                    break;
                default: throw new InvalidOperationException("Unknown benchmark argument: " + args[index]);
            }
        }
        return result with { DryRun = !result.MeasureRuntime && !result.MeasureBrowser && result.DryRun };
    }

    private static string Next(string[] args, ref int index, string name)
        => ++index < args.Length ? args[index] : throw new InvalidOperationException("Missing value for " + name);

    private static int PositiveInt(string value, string name)
        => int.TryParse(value, out var parsed) && parsed > 0 ? parsed : throw new InvalidOperationException(name + " must be a positive integer.");
}

internal sealed record BenchmarkProtocol(
    string SchemaVersion,
    string Status,
    int Samples,
    int Iterations,
    IReadOnlyList<string> Fixtures,
    IReadOnlyList<string> Metrics,
    IReadOnlyList<string> Notes)
{
    public static BenchmarkProtocol Create(BenchmarkOptions options)
        => new(
            "razorvue-g2-direct-v1",
            "pending-measurement",
            options.Samples,
            options.Iterations,
            ["plain-text", "counter", "keyed-list-100", "static-vnode"],
            ["render ops/s", "update ops/s", "retained heap delta", "gzip body bytes"],
            [
                "Input is the final direct Vue render-function shape emitted by RazorVue.",
                "Handwritten h() is the comparison baseline on the same machine and Node process.",
                "This lane does not measure Vue DOM patching, hydration, browser layout, or compiler cold/incremental time.",
                "Static props/VNodes and setup-local handler identity are evaluated as artifact semantics, not as a runtime bridge protocol."
            ]);

    public string ToMarkdown()
        => $"""
# RazorVue Direct Render Performance Benchmark

- Schema: {SchemaVersion}
- Status: {Status}
- Samples: {Samples}
- Iterations: {Iterations}

## Fixtures

{string.Join(Environment.NewLine, Fixtures.Select(static fixture => "- `" + fixture + "`"))}

## Metrics

{string.Join(Environment.NewLine, Metrics.Select(static metric => "- " + metric))}

## Scope

{string.Join(Environment.NewLine, Notes.Select(static note => "- " + note))}
""";
}

internal sealed record DirectRuntimeReport(
    string SchemaVersion,
    string Status,
    string NodeVersion,
    int Samples,
    int Iterations,
    string Scope,
    IReadOnlyList<DirectFixtureReport> Fixtures)
{
    public string ToMarkdown()
        => $"""
# RazorVue Direct Render Runtime Report

- Schema: {SchemaVersion}
- Status: {Status}
- Node: `{NodeVersion}`
- Scope: {Scope}

| Fixture | Generated render | Handwritten render | Generated update | Handwritten update | Generated gzip | Handwritten gzip |
|---|---:|---:|---:|---:|---:|---:|
{string.Join(Environment.NewLine, Fixtures.Select(static fixture => $"| `{fixture.Id}` | {fixture.GeneratedRender.Median:0.##} | {fixture.HandwrittenRender.Median:0.##} | {fixture.GeneratedUpdate.Median:0.##} | {fixture.HandwrittenUpdate.Median:0.##} | {fixture.GeneratedGzipBytes} | {fixture.HandwrittenGzipBytes} |"))}
""";
}

internal sealed record DirectFixtureReport(
    string Id,
    MetricSummary GeneratedRender,
    MetricSummary HandwrittenRender,
    MetricSummary GeneratedUpdate,
    MetricSummary HandwrittenUpdate,
    long GeneratedRetained,
    long HandwrittenRetained,
    int GeneratedGzipBytes,
    int HandwrittenGzipBytes);

internal sealed record MetricSummary(double Median, double Min, double Max, IReadOnlyList<double> Samples);

internal sealed record BrowserProbeReport(string SchemaVersion, string Status, string Browser, string Details, string Scope)
{
    public string ToMarkdown()
        => $"""
# RazorVue Direct Render Browser Probe

- Schema: {SchemaVersion}
- Status: {Status}
- Browser: `{Browser}`
- Scope: {Scope}

{Details}
""";
}
