using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DenoHost.Core;

namespace Jazor.CLR.Test;

internal sealed record ClrRuntimeInvocation(
    string Id,
    string ModulePath,
    string ExportName,
    IReadOnlyList<ClrRuntimeValue> Arguments);

internal sealed record ClrRuntimeExecutionResult(
    string Id,
    bool Succeeded,
    ClrRuntimeValue? Value,
    string? Error);

internal sealed record DenoImportMap(IReadOnlyDictionary<string, string> Imports);

internal static class ClrRuntimeTestHost
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    private static readonly Lazy<Task<IReadOnlyDictionary<string, ClrRuntimeExecutionResult>>> Results =
        new(RunCoreAsync, LazyThreadSafetyMode.ExecutionAndPublication);

    public static Task<IReadOnlyDictionary<string, ClrRuntimeExecutionResult>> RunAsync() => Results.Value;

    private static async Task<IReadOnlyDictionary<string, ClrRuntimeExecutionResult>> RunCoreAsync()
    {
        var root = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "jazor-clr-runtime-" + Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(root);

        try
        {
            await MaterializeCatalogAsync(root);
            var invocations = ClrRuntimeScenarioCatalog.All
                .Select(static scenario =>
                {
                    var mapping = ClrRuntimeMappingCatalog.GetImport(scenario.Member);
                    return new ClrRuntimeInvocation(
                        scenario.Id,
                        scenario.ModulePath,
                        mapping.ExportName,
                        scenario.Arguments);
                })
                .ToArray();

            var invocationPath = Path.Combine(root, "scenarios.json");
            var resultPath = Path.Combine(root, "results.json");
            var runnerPath = Path.Combine(root, "runner.mjs");
            var configPath = Path.Combine(root, "deno.json");
            await File.WriteAllTextAsync(invocationPath, JsonSerializer.Serialize(invocations, JsonOptions), Utf8WithoutBom);
            await File.WriteAllTextAsync(runnerPath, RunnerSource, Utf8WithoutBom);
            await File.WriteAllTextAsync(
                configPath,
                JsonSerializer.Serialize(
                    new DenoImportMap(new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["System/"] = "./System/"
                    }),
                    JsonOptions),
                Utf8WithoutBom);

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                [
                    "run",
                    "--config",
                    configPath,
                    "--quiet",
                    "--allow-read",
                    "--allow-write",
                    runnerPath,
                    invocationPath,
                    resultPath
                ],
                timeout.Token);

            await using var stream = File.OpenRead(resultPath);
            var results = await JsonSerializer.DeserializeAsync<ClrRuntimeExecutionResult[]>(
                stream,
                JsonOptions,
                CancellationToken.None)
                ?? throw new InvalidOperationException("Deno CLR runtime runner returned no result array.");
            return results.ToDictionary(static result => result.Id, StringComparer.Ordinal);
        }
        finally
        {
            DeleteOwnedTempDirectory(root);
        }
    }

    private static async Task MaterializeCatalogAsync(string root)
    {
        foreach (var module in ClrRuntimeCatalog.All)
        {
            var relativePath = module.RelativePath.Replace('/', Path.DirectorySeparatorChar);
            var outputPath = Path.GetFullPath(Path.Combine(root, relativePath));
            if (!outputPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Catalog module path escapes the runtime workspace: {module.RelativePath}");

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllTextAsync(outputPath, module.Content, Utf8WithoutBom);
        }
    }

    private static void DeleteOwnedTempDirectory(string root)
    {
        var tempRoot = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar);
        if (!root.StartsWith(tempRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Refusing to delete a non-temporary CLR runtime workspace: {root}");

        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    private const string RunnerSource = """
        const [scenarioPath, resultPath] = Deno.args;
        const scenarios = JSON.parse(await Deno.readTextFile(scenarioPath));

        function decode(value) {
          switch (value.kind) {
            case "null": return null;
            case "string": return value.scalar;
            case "number": return Number(value.scalar);
            case "boolean": return value.scalar === "true";
            case "bigInt": return BigInt(value.scalar);
            case "array": return value.items.map(decode);
            case "undefined": return undefined;
            default: throw new Error(`Unsupported CLR runtime value kind: ${value.kind}`);
          }
        }

        function encode(value) {
          if (value === null) return { kind: "null" };
          if (value === undefined) return { kind: "undefined" };
          if (Array.isArray(value)) return { kind: "array", items: value.map(encode) };
          switch (typeof value) {
            case "string": return { kind: "string", scalar: value };
            case "number": return { kind: "number", scalar: String(value) };
            case "boolean": return { kind: "boolean", scalar: String(value) };
            case "bigint": return { kind: "bigInt", scalar: String(value) };
            default: throw new Error(`Unsupported CLR runtime result type: ${typeof value}`);
          }
        }

        const results = [];
        for (const scenario of scenarios) {
          try {
            const runtimeModule = await import(`./${scenario.modulePath}`);
            const runtimeFunction = runtimeModule[scenario.exportName];
            if (typeof runtimeFunction !== "function")
              throw new Error(`Missing runtime export ${scenario.exportName} in ${scenario.modulePath}`);
            const value = await runtimeFunction(...scenario.arguments.map(decode));
            results.push({ id: scenario.id, succeeded: true, value: encode(value), error: null });
          } catch (error) {
            results.push({
              id: scenario.id,
              succeeded: false,
              value: null,
              error: String(error?.stack ?? error)
            });
          }
        }

        await Deno.writeTextFile(resultPath, JSON.stringify(results));
        """;
}
