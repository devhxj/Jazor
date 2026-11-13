#!/usr/bin/env dotnet run
#:property AssemblyName=ECMAScript.WebIDL.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/ECMAScript.WebIDL.Generator/ECMAScript.WebIDL.Generator.csproj

using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using ECMAScript.WebIDL.Generator;
using Jazor.NodeHost;
using Microsoft.JavaScript.NodeApi;

// This single .NET process hosts the native installer, transpiler and collector.
// No Node, pnpm, tsc, shell, or generator child process is launched.
var layout = RepositoryLayout.Discover(Directory.GetCurrentDirectory());
var reportRoot = Path.Combine(layout.RepositoryRoot, "artifacts", "webidl-nodeapi", Guid.NewGuid().ToString("N"));
var outputRoot = Path.Combine(reportRoot, "collected");
Directory.CreateDirectory(reportRoot);
var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var options = GeneratorOptions.Parse(["--out", outputRoot, "--skip-spec-prose"], layout);
var lockPath = Path.Combine(Path.GetDirectoryName(options.NodeProjectPath)!, "pnpm-lock.yaml");
var lockBefore = File.Exists(lockPath) ? SHA256.HashData(await File.ReadAllBytesAsync(lockPath)) : null;
await WebIdlGeneratorApplication.RunAsync(options, CancellationToken.None);

var testResult = await new NodeWebIdlCollector(options).TestAsync(CancellationToken.None);
using var testDocument = JsonDocument.Parse(testResult);
if (testDocument.RootElement.GetProperty("pid").GetInt32() != Environment.ProcessId)
    throw new InvalidOperationException("Collector tests did not run in the C# host process.");
var lockAfter = SHA256.HashData(await File.ReadAllBytesAsync(lockPath));
if (lockBefore is not null && !lockBefore.AsSpan().SequenceEqual(lockAfter))
    throw new InvalidOperationException("Frozen restoration changed pnpm-lock.yaml.");

// Observe an asynchronous JS rejection after a second environment initialization.
// This also catches references incorrectly retained across closed JS callback scopes.
var failedWorker = Path.Combine(reportRoot, "failed-worker.ts");
await File.WriteAllTextAsync(failedWorker, "export async function collectInventory() { throw new Error('webidl-nodeapi-error-propagation'); }");
try
{
    await new NodeWebIdlCollector(options with { WorkerPath = failedWorker }).CollectAsync(CancellationToken.None);
    throw new InvalidOperationException("A rejected collector unexpectedly succeeded.");
}
catch (JSException exception) when (exception.ToString().Contains("webidl-nodeapi-error-propagation", StringComparison.Ordinal))
{
    Console.WriteLine("Asynchronous JS error propagated to C#.");
}

var inventory = JsonSerializer.Deserialize<WebIdlInventory>(
    await File.ReadAllTextAsync(Path.Combine(outputRoot, "webidl.inventory.json")), jsonOptions)!;
var parity = "";
if (args.Length == 0)
{
    // The collector fingerprint stays fixed to the pre-migration input. Intentional emitter
    // changes update generatedFiles from that same inventory and record emissionRevision;
    // a fresh collection must never silently reset the inventory fingerprint.
    using var baseline = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(layout.RepositoryRoot,
        "src", "ECMAScript.WebIDL.GeneratorTest", "Fixtures", "nodeapi-collector-baseline.json")));
    var fingerprint = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(
        inventory with { GeneratedAt = DateTimeOffset.UnixEpoch })));
    if (fingerprint != baseline.RootElement.GetProperty("inventorySha256").GetString())
        throw new InvalidOperationException("Inventory differs from the fixed Deno collector fingerprint beyond generatedAt.");
    var actualFiles = Directory.GetFiles(outputRoot, "*.cs", SearchOption.AllDirectories)
        .ToDictionary(file => Path.GetRelativePath(outputRoot, file).Replace('\\', '/'),
            file => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))));
    var expected = baseline.RootElement.GetProperty("generatedFiles");
    if (actualFiles.Count != expected.EnumerateObject().Count() || expected.EnumerateObject().Any(file =>
        !actualFiles.TryGetValue(file.Name, out var hash) || hash != file.Value.GetString()))
        throw new InvalidOperationException("Generated C# differs from the reviewed emitter baseline.");
    parity = $"Fixed collector inventory fingerprint matches; {actualFiles.Count} generated C# files match the reviewed emitter baseline.";
}
if (args.Length > 0)
{
    var baselinePath = Path.GetFullPath(args[0]);
    var baseline = JsonSerializer.Deserialize<WebIdlInventory>(await File.ReadAllTextAsync(baselinePath), jsonOptions)!;
    if (!JsonNode.DeepEquals(JsonSerializer.SerializeToNode(baseline),
        JsonSerializer.SerializeToNode(inventory with { GeneratedAt = baseline.GeneratedAt })))
        throw new InvalidOperationException("Inventory differs from the baseline beyond generatedAt.");

    var baselineOutput = Path.Combine(reportRoot, "baseline");
    await WebIdlGeneratorApplication.RunAsync(
        options with { OutputDirectory = baselineOutput, InputInventoryPath = baselinePath }, CancellationToken.None);
    var actualFiles = Directory.GetFiles(outputRoot, "*.cs", SearchOption.AllDirectories)
        .ToDictionary(file => Path.GetRelativePath(outputRoot, file), File.ReadAllBytes);
    var expectedFiles = Directory.GetFiles(baselineOutput, "*.cs", SearchOption.AllDirectories);
    if (actualFiles.Count != expectedFiles.Length || expectedFiles.Any(file =>
        !actualFiles.TryGetValue(Path.GetRelativePath(baselineOutput, file), out var actual) ||
        !actual.AsSpan().SequenceEqual(File.ReadAllBytes(file))))
        throw new InvalidOperationException("Generated C# differs from the fixed inventory baseline.");
    parity = $"Inventory equal except generatedAt; {expectedFiles.Length} generated C# files are byte-identical.";
}

var report = $"""
    # WebIDL NodeApi verification

    - .NET/JS PID: {Environment.ProcessId}
    - pnpm engine: {PnpmBootstrap.Version}
    - Collector tests: {testDocument.RootElement.GetProperty("tests").GetInt32()}
    - Files / declarations: {inventory.Stats.FileCount} / {inventory.Stats.DeclarationCount}
    - Frozen lock unchanged: {lockBefore is not null}
    - Asynchronous JS error propagation: passed
    - {parity}
    """;
await File.WriteAllTextAsync(Path.Combine(reportRoot, "report.md"), report + Environment.NewLine);
Console.WriteLine(report);
Console.WriteLine("Report: " + Path.Combine(reportRoot, "report.md"));
