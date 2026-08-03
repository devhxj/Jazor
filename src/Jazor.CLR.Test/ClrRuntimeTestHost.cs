using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DenoHost.Core;

namespace Jazor.CLR.Test;

internal sealed record ClrRuntimeInvocation(
    string Id,
    string ModulePath,
    string ExportName,
    IReadOnlyList<ClrRuntimeValue> Arguments,
    bool CaptureArguments);

internal sealed record ClrRuntimeExecutionResult(
    string Id,
    bool Succeeded,
    ClrRuntimeValue? Value,
    IReadOnlyList<ClrRuntimeValue>? Arguments,
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
                        scenario.Arguments,
                        scenario.ExpectedArguments is not null);
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

        async function decodeAll(values, references) {
          const decoded = [];
          for (const value of values)
            decoded.push(await decode(value, references));
          return decoded;
        }

        async function decodeEntries(items, references) {
          if (items.length % 2 !== 0)
            throw new Error("CLR runtime map values require key/value pairs");
          const entries = [];
          for (let index = 0; index < items.length; index += 2)
            entries.push([await decode(items[index], references), await decode(items[index + 1], references)]);
          return entries;
        }

        function createTrackedWeakMap(entries) {
          const snapshot = new Map(entries);
          const weakMap = new WeakMap(entries);
          const set = weakMap.set.bind(weakMap);
          const remove = weakMap.delete.bind(weakMap);
          weakMap.set = (key, value) => {
            snapshot.set(key, value);
            return set(key, value);
          };
          weakMap.delete = key => {
            snapshot.delete(key);
            return remove(key);
          };
          Object.defineProperty(weakMap, "__clrRuntimeEntries", { value: snapshot });
          return weakMap;
        }

        async function decode(value, references) {
          switch (value.kind) {
            case "null": return null;
            case "string": return value.scalar;
            case "number": return Number(value.scalar);
            case "boolean": return value.scalar === "true";
            case "bigInt": return BigInt(value.scalar);
            case "array": return await decodeAll(value.items, references);
            case "arrayElement": {
              const array = await decode(value.items[0], references);
              return array[Number(value.scalar)];
            }
            case "set": return new Set(await decodeAll(value.items, references));
            case "map": return new Map(await decodeEntries(value.items, references));
            case "weakMap": return createTrackedWeakMap(await decodeEntries(value.items, references));
            case "reference": {
              if (references.has(value.scalar)) return references.get(value.scalar);
              const resolved = await decode(value.items[0], references);
              references.set(value.scalar, resolved);
              return resolved;
            }
            case "sequence": {
              let result;
              for (const step of value.items)
                result = await decode(step, references);
              return result;
            }
            case "record": {
              const entries = [];
              for (const [name, item] of Object.entries(value.properties))
                entries.push([name, await decode(item, references)]);
              return Object.fromEntries(entries);
            }
            case "callable": {
              let fn;
              switch (value.scalar) {
                case "IsEven": fn = item => typeof item === "number" && item % 2 === 0; break;
                case "IsEvenIndex": fn = (_, index) => index % 2 === 0; break;
                case "IsPositive": fn = item => typeof item === "number" && item > 0; break;
                case "DoubleNumber": fn = item => item * 2; break;
                case "AddIndex": fn = (item, index) => item + index; break;
                case "ExpandNumber": fn = item => [item, item * 10]; break;
                case "ExpandWithIndex": fn = (item, index) => [item + index]; break;
                case "CombineOuterInner": fn = (outer, inner) => outer * 100 + inner; break;
                case "CombineOuterGroupCount": fn = (outer, group) => outer * 10 + Array.from(group).length; break;
                case "GroupKeyAndSum": fn = (key, group) => (key ? 100 : 0) + Array.from(group).reduce((sum, item) => sum + item, 0); break;
                case "CompareDescending": fn = (left, right) => right - left; break;
                case "AddNumbers": fn = (left, right) => left + right; break;
                case "ToBigInt": fn = item => BigInt(item); break;
                case "ToDecimalText": fn = item => String(item); break;
                case "ReturnFactoryText": fn = _ => "factory"; break;
                case "ReturnFactoryArgument": fn = (_, argument) => argument; break;
                case "ReturnHashCode": fn = () => 713; break;
                case "Identity": fn = item => item; break;
                case "SameParity": fn = (left, right) => Math.abs(left) % 2 === Math.abs(right) % 2; break;
                case "ParityHash": fn = item => Math.abs(item) % 2; break;
                default: throw new Error(`Unsupported CLR runtime callable kind: ${value.scalar}`);
              }
              Object.defineProperty(fn, "__clrRuntimeCallable", { value: value.scalar });
              return fn;
            }
            case "disposable": {
              const disposable = {
                disposeCount: Number(value.scalar),
                dispose() { this.disposeCount += 1; }
              };
              Object.defineProperty(disposable, "__clrRuntimeCarrier", { value: "disposable" });
              return disposable;
            }
            case "asyncDisposable": {
              const disposable = {
                disposeCount: Number(value.scalar),
                async disposeAsync() { this.disposeCount += 1; }
              };
              Object.defineProperty(disposable, "__clrRuntimeCarrier", { value: "asyncDisposable" });
              return disposable;
            }
            case "runtimeInvocation": {
              const invocation = value.invocation;
              const runtimeModule = await import(`./${invocation.modulePath}`);
              const runtimeFunction = runtimeModule[invocation.exportName];
              if (typeof runtimeFunction !== "function")
                throw new Error(
                  `Missing nested runtime export ${invocation.exportName} in ${invocation.modulePath}`);
              const args = await decodeAll(invocation.arguments, references);
              return await runtimeFunction(...args);
            }
            case "error": {
              const cause = await decode(value.items[0], references);
              return new Error(value.scalar, { cause });
            }
            case "undefined": return undefined;
            default: throw new Error(`Unsupported CLR runtime value kind: ${value.kind}`);
          }
        }

        function encode(value) {
          if (value === null) return { kind: "null" };
          if (value === undefined) return { kind: "undefined" };
          if (Array.isArray(value)) return { kind: "array", items: value.map(encode) };
          if (value instanceof Set) return { kind: "set", items: Array.from(value, encode) };
          if (value instanceof Map) return { kind: "map", items: Array.from(value).flatMap(([key, item]) => [encode(key), encode(item)]) };
          if (value instanceof WeakMap) return { kind: "weakMap", items: Array.from(value.__clrRuntimeEntries ?? []).flatMap(([key, item]) => [encode(key), encode(item)]) };
          // JQueue/JStack state is exposed through prototype getters, not enumerable own fields.
          if (value?.constructor?.name === "JQueue") {
            return { kind: "record", properties: { head: encode(value.head), items: encode(value.items) } };
          }
          if (value?.constructor?.name === "JStack") {
            return { kind: "record", properties: { items: encode(value.items) } };
          }
          // Index/Range carriers expose their state through generated prototype getters. Encode
          // the CLR-facing value fields explicitly so Deno scenarios verify their contracts.
          if (value?.constructor?.name === "JIndex") {
            return { kind: "record", properties: { value: encode(value.value), fromEnd: encode(value.fromEnd) } };
          }
          if (value?.constructor?.name === "JRange") {
            return { kind: "record", properties: { start: encode(value.start), end: encode(value.end) } };
          }
          if (value?.__clrRuntimeCarrier === "disposable") {
            return { kind: "disposable", scalar: String(value.disposeCount) };
          }
          if (value?.__clrRuntimeCarrier === "asyncDisposable") {
            return { kind: "asyncDisposable", scalar: String(value.disposeCount) };
          }
          if (value instanceof Error) {
            const cause = Object.hasOwn(value, "cause") ? value.cause : null;
            return { kind: "error", scalar: value.message, items: [encode(cause)] };
          }
          if (typeof value === "object" && Object.hasOwn(value, Symbol.toPrimitive)) {
            const primitive = value[Symbol.toPrimitive]("string");
            if (primitive === value)
              throw new Error("Runtime carrier returned itself from Symbol.toPrimitive");
            return encode(primitive);
          }
          switch (typeof value) {
            case "string": return { kind: "string", scalar: value };
            case "number": return {
              kind: "number",
              scalar: Object.is(value, -0) ? "-0" : String(value)
            };
            case "boolean": return { kind: "boolean", scalar: String(value) };
            case "bigint": return { kind: "bigInt", scalar: String(value) };
            case "function": {
              if (typeof value.__clrRuntimeCallable !== "string")
                throw new Error("Cannot encode an unregistered CLR runtime callable");
              return { kind: "callable", scalar: value.__clrRuntimeCallable };
            }
            case "object": return {
              kind: "record",
              properties: Object.fromEntries(
                Object.entries(value)
                  .sort(([left], [right]) => left.localeCompare(right))
                  .map(([name, item]) => [name, encode(item)]))
            };
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
            const args = await decodeAll(scenario.arguments, new Map());
            const value = await runtimeFunction(...args);
            results.push({
              id: scenario.id,
              succeeded: true,
              value: encode(value),
              arguments: scenario.captureArguments ? args.map(encode) : null,
              error: null
            });
          } catch (error) {
            results.push({
              id: scenario.id,
              succeeded: false,
              value: null,
              arguments: null,
              error: String(error?.stack ?? error)
            });
          }
        }

        await Deno.writeTextFile(resultPath, JSON.stringify(results));
        """;
}
