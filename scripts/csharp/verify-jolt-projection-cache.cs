#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Jazor.NodeHost;

if (args is not [var executable])
    throw new ArgumentException("Usage: verify-jolt-projection-cache.cs <Jolt CLI apphost>");

var cli = Path.GetFullPath(executable);
var root = Directory.GetCurrentDirectory();
var fixture = Path.Combine(root, "scripts", "csharp", "fixtures", "jolt-monaco-browser");
var project = Directory.CreateDirectory(Path.Combine(root, ".tmp", $"projection-cache-{Guid.NewGuid():N}"));
var generated = Path.Combine(project.FullName, "obj", "Jolt", "projections");
var metadata = Path.Combine(project.FullName, ".jazor", "cache");
var client = project.CreateSubdirectory("client");
var shared = project.CreateSubdirectory("shared");
var declaration = Path.Combine(client.FullName, "client.d.ts");
var sharedDeclaration = Path.Combine(shared.FullName, "index.d.ts");
const string sharedSource = """
    import type { Ref } from 'vue';
    export type NamedRef<T> = Ref<T> & { clear(): void };
    export interface Registry {
        matches<S extends string, K extends 'reserved' = 'reserved'>(key: S | K, locale?: string): boolean;
    }
    """;
var output = Path.Combine(project.FullName, "Client.g.cs");
var app = Path.Combine(project.FullName, "App.jazor");
const string source = """
    @using Client = Npm.ProjectionCacheFixture.Exports
    <p>@Label</p>
    @code {
        private string Label => Client.Select("ready", value => value.Length) > 0 ? Client.Format("ready") + Client.Count() : "";
        public static string ReadSelection() => Client.SelectMode("ready");
        public static string ReadRecord() => Client.ApplyPatch("ready", new());
        public static string ReadComputed() => Client.ConsumeComputed<string>(Client.Computed());
        public static bool ReadRegistry() => Client.OpenRegistry().Matches("ready");
        public static string ReadSharedRef()
        {
            var reference = Client.GetSharedRef();
            reference.Clear();
            return reference.Value;
        }
        public static string ReadModel() =>
            Client.BindModelWithPassiveTrue(Client.Snapshot("ready"), item => item.Value,
                new() { DefaultValue = "fallback", ShouldEmit = value => value.Length > 0 }).Value;

        public static string ReadRefs()
        {
            var writable = Client.WideRef();
            var snapshot = Client.Snapshot("updated");
            snapshot.Edited = snapshot.Value;
            writable.Value = snapshot.Edited;
            return writable.Value + "/" + Client.NarrowRef().Value;
        }
    }
    """;
try
{
    File.Copy(Path.Combine(fixture, "pnpm-lock.yaml"), Path.Combine(project.FullName, "pnpm-lock.yaml"));
    var manifest = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(Path.Combine(fixture, "package.json")))!;
    manifest["dependencies"]!["projection-cache-fixture"] = "link:./client";
    manifest["dependencies"]!["projection-cache-ref"] = "link:./shared";
    File.WriteAllText(Path.Combine(project.FullName, "package.json"), manifest.ToJsonString());
    File.WriteAllText(Path.Combine(shared.FullName, "package.json"),
        """{"name":"projection-cache-ref","version":"1.0.0","types":"./index.d.ts"}""");
    File.WriteAllText(sharedDeclaration, sharedSource);
    File.WriteAllText(Path.Combine(client.FullName, "package.json"),
        """{"name":"projection-cache-fixture","version":"1.0.0","type":"module","exports":"./client.js"}""");
    File.WriteAllText(Path.Combine(client.FullName, "client.js"),
        """
        import { computed as vueComputed, ref } from 'vue';
        export const format = String;
        export const count = () => 2;
        export const select = (value, selector) => selector(value);
        export const selectMode = (label, enabled = false) => enabled ? true : label;
        export const applyPatch = (value, patch) => patch.value ?? value;
        export const snapshot = value => ({ value, edited: value });
        export const wideRef = () => ({ value: 'initial' });
        export const narrowRef = () => ({ value: 'initial' });
        export const computed = () => vueComputed(() => 'computed');
        export const consumeComputed = source => source.value;
        export const openRegistry = () => ({ matches: key => key === 'ready' });
        export const getSharedRef = () => {
            const result = ref('initial');
            result.clear = () => result.value = 'cleared';
            return result;
        };
        export const bindModel = (props, key, emit, options) => {
            if (emit !== undefined || options?.passive !== true ||
                options.defaultValue !== 'fallback' || options.shouldEmit(props[key]) !== true)
                throw Error('model argument order or option projection');
            return ref(props[key]);
        };
        """);
    File.WriteAllText(declaration, """
        import type { ComputedRef, Ref, WritableComputedRef } from 'vue';
        import type { NamedRef, Registry } from 'projection-cache-ref';
        export interface Snapshot<TValue> { readonly value: TValue; edited: TValue }
        export interface ValuePatch<T> { value?: T }
        export declare function format(value: string): string;
        export declare function count(): number;
        export declare function select<S, R>(value: S, selector: (value: S) => R): R;
        export declare function selectMode<B extends boolean = false>(label: string, enabled?: B): B extends true ? boolean : string;
        export declare function applyPatch<T>(value: T, patch: ValuePatch<T>): T;
        export declare function snapshot<TValue>(value: TValue): Snapshot<TValue>;
        export declare function wideRef(): Ref<string, string | null>;
        export declare function narrowRef(): Ref<string, 'initial'>;
        export declare function computed(): ComputedRef<string>;
        export declare function consumeComputed<T>(source: ComputedRef<T>): T;
        export declare function getSharedRef(): NamedRef<string>;
        export declare function openRegistry(): Registry;
        interface ModelOptions<T, Passive extends boolean> { passive?: Passive; defaultValue?: T; shouldEmit?: (value: T) => boolean }
        export declare function bindModel<P extends object, K extends keyof P>(props: P, key?: K, emit?: (event: string, value: P[K]) => void, options?: ModelOptions<P[K], false>): WritableComputedRef<P[K]>;
        export declare function bindModel<P extends object, K extends keyof P>(props: P, key?: K, emit?: (event: string, value: P[K]) => void, options?: ModelOptions<P[K], true>): Ref<P[K]>;
        """);
    File.WriteAllText(app, source);
    var store = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.Combine(root, ".tmp", "nodeapi-store");
    await NodeToolchain.RestoreAsync(project.FullName, store, CancellationToken.None, Console.Out);

    string[] Bindings(string first, string second, string? destination = null) =>
        ["bindings", ".", "--module", "projection-cache-fixture", "--export", first, "--export", "select", "--export", second,
            "--export", "snapshot", "--export", "wideRef", "--export", "narrowRef", "--export", "selectMode",
            "--export", "applyPatch", "--export", "computed", "--export", "consumeComputed", "--export", "bindModel",
            "--export", "getSharedRef", "--export", "openRegistry",
            "--output", destination ?? output];

    var parallelOutput = Path.Combine(project.FullName, "Concurrent.g.cs");
    await Task.WhenAll(Run(Bindings("format", "count")), Run(Bindings("count", "format", parallelOutput)));
    var expected = File.ReadAllText(output);
    if (!expected.Contains("extern string @SelectMode(string @label);", StringComparison.Ordinal))
        throw new InvalidOperationException("Omitting the default-false flag must produce a string public binding.");
    if (!expected.Contains("@HasValue {", StringComparison.Ordinal))
        throw new InvalidOperationException("The public binding must allow omission of the generic patch value.");
    if (!expected.Contains("VueComputedRef<string> @Computed()", StringComparison.Ordinal))
        throw new InvalidOperationException("The public binding must preserve Vue's computed ref type.");
    if (!expected.Contains("extern bool @Matches(string @key);", StringComparison.Ordinal))
        throw new InvalidOperationException("The public binding must allow the defaulted registry call.");
    if (File.ReadAllText(parallelOutput) != expected)
        throw new InvalidOperationException("Concurrent publishers must produce identical binding source.");
    File.Delete(parallelOutput);
    var bindings = Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories).Single();
    var bindingsStamp = Mark(bindings);
    File.Delete(output);
    await Run(Bindings("count", "format"));
    if (File.ReadAllText(output) != expected || File.GetLastWriteTimeUtc(bindings) != bindingsStamp ||
        Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories).Length != 1)
        throw new InvalidOperationException("A reordered bindings request must reuse cached text and restore its output.");

    File.Delete(output);
    await Run(["build", "."]);
    var projection = Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories).Single();
    Check(Path.GetDirectoryName(projection)!, "symbols.json");
    var projectionStamp = Mark(projection);
    var componentOutput = Path.Combine(project.FullName, ".jolt", "App.mjs");
    var componentText = File.ReadAllText(componentOutput);
    File.Delete(componentOutput);
    await Run(["build", "."]);
    if (File.GetLastWriteTimeUtc(projection) != projectionStamp || File.ReadAllText(componentOutput) != componentText)
        throw new InvalidOperationException("A second build must reuse its projection and restore component output.");
    var symbols = Path.Combine(Path.GetDirectoryName(projection)!, "symbols.json");
    File.Delete(symbols);
    File.Delete(componentOutput);
    // Keep the original source open while the missing sidecar forces publication.
    using (var reader = new FileStream(projection, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
        await Run(["build", "."]);
    if (!File.Exists(symbols) || File.GetLastWriteTimeUtc(projection) != projectionStamp ||
        File.ReadAllText(componentOutput) != componentText)
        throw new InvalidOperationException("Regenerating symbols must reuse the published source and restore component output.");
    var verification = Path.Combine(project.FullName, "verify.mjs");
    File.WriteAllText(verification, """
        import assert from 'node:assert/strict';
        import { ReadRefs, ReadSelection, ReadRecord, ReadComputed, ReadModel, ReadSharedRef, ReadRegistry } from './.jolt/App.mjs';
        assert.equal(ReadRefs(), 'updated/initial');
        assert.equal(ReadSelection(), 'ready');
        assert.equal(ReadRecord(), 'ready');
        assert.equal(ReadComputed(), 'computed');
        assert.equal(ReadModel(), 'ready');
        assert.equal(ReadSharedRef(), 'cleared');
        assert.equal(ReadRegistry(), true);
        """);
    await NodeModuleRunner.RunAsync(project.FullName, verification);
    Check(metadata, "*");
    var metadataEntries = Directory.GetFiles(metadata, "*.json", SearchOption.AllDirectories);
    var metadataStamps = metadataEntries.ToDictionary(path => path, File.GetLastWriteTimeUtc);
    await VerifyDefinition(() =>
    {
        if (File.GetLastWriteTimeUtc(projection) != projectionStamp ||
            metadataStamps.Any(entry => File.GetLastWriteTimeUtc(entry.Key) != entry.Value))
            throw new InvalidOperationException("LSP rewrote cached projection or metadata.");
    });

    var declarations = File.ReadAllText(declaration);
    File.WriteAllText(declaration, "// Public documentation moved.\n" + declarations);
    await Run(Bindings("format", "count"));
    if (File.ReadAllText(output) != expected || File.GetLastWriteTimeUtc(bindings) != bindingsStamp ||
        Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories).Length != 1)
        throw new InvalidOperationException("Changing declaration offsets must reuse the public C# cache entry.");
    File.Delete(output);
    await Run(["build", "."]);
    if (File.GetLastWriteTimeUtc(projection) != projectionStamp || File.ReadAllText(componentOutput) != componentText)
        throw new InvalidOperationException("Moving the declaration must reuse the component projection.");
    var previousProjections = Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories);
    File.WriteAllText(declaration, declarations.Replace("=> R): R;", "=> R): S;", StringComparison.Ordinal));
    await Run(Bindings("format", "count"));
    if (File.ReadAllText(output) == expected || Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories).Length != 2)
        throw new InvalidOperationException("Changing the declaration must regenerate the public signature.");
    File.Delete(output);
    await Run(["build", "."], "CS0019");
    if (Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories).Except(previousProjections).Count() != 1)
        throw new InvalidOperationException("Changing the generic result must regenerate the component projection.");

    var priorBindings = Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories);
    previousProjections = Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories);
    File.WriteAllText(declaration, declarations.Replace("Ref<string, string | null>", "Ref<string, 'initial'>", StringComparison.Ordinal));
    await Run(Bindings("format", "count"));
    var readonlyBindings = File.ReadLines(output).Single(line => line.Contains("@WideRef()", StringComparison.Ordinal));
    if (!readonlyBindings.Contains("VueReadonlyRef<string>", StringComparison.Ordinal) ||
        Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories).Except(priorBindings).Count() != 1)
        throw new InvalidOperationException("Changing the permitted Ref writes must generate a new readonly public binding.");
    File.Delete(output);
    await Run(["build", "."], "CS0200");
    if (Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories).Except(previousProjections).Count() != 1)
        throw new InvalidOperationException("Changing the Ref write policy must regenerate the component projection.");

    priorBindings = Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories);
    previousProjections = Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories);
    File.WriteAllText(declaration, declarations.Replace("computed(): ComputedRef<string>", "computed(): Readonly<Ref<string>>",
        StringComparison.Ordinal));
    await Run(Bindings("format", "count"));
    if (!File.ReadAllText(output).Contains("VueReadonlyRef<string> @Computed()", StringComparison.Ordinal) ||
        Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories).Except(priorBindings).Count() != 1)
        throw new InvalidOperationException("Removing computed identity must generate a plain readonly public binding.");
    File.Delete(output);
    await Run(["build", "."], "CS1503");
    if (Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories).Except(previousProjections).Count() != 1)
        throw new InvalidOperationException("Removing computed identity must regenerate the component projection.");

    priorBindings = Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories);
    previousProjections = Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories);
    File.WriteAllText(declaration, declarations.Replace("B extends boolean = false", "B extends boolean = true", StringComparison.Ordinal));
    await Run(Bindings("format", "count"));
    if (!File.ReadAllText(output).Contains("extern bool @SelectMode(string @label);", StringComparison.Ordinal) ||
        Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories).Except(priorBindings).Count() != 1)
        throw new InvalidOperationException("Changing the omitted flag's default must generate a new boolean public binding.");
    File.Delete(output);
    await Run(["build", "."], "CS0029");
    if (Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories).Except(previousProjections).Count() != 1)
        throw new InvalidOperationException("Changing the omitted flag's default must regenerate the component projection.");

    priorBindings = Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories);
    previousProjections = Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories);
    File.WriteAllText(declaration, declarations.Replace("value?: T", "value: T", StringComparison.Ordinal));
    await Run(Bindings("format", "count"));
    var requiredBindings = File.ReadAllText(output);
    if (requiredBindings.Contains("@HasValue {", StringComparison.Ordinal) ||
        !requiredBindings.Split('\n').Any(line => line.Contains("public required ", StringComparison.Ordinal) &&
            line.Contains(" @Value {", StringComparison.Ordinal)) ||
        Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories).Except(priorBindings).Count() != 1)
        throw new InvalidOperationException("Making the generic patch value required must generate a new public binding.");
    File.Delete(output);
    await Run(["build", "."], "CS9035");
    if (Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories).Except(previousProjections).Count() != 1)
        throw new InvalidOperationException("Making the generic patch value required must regenerate the component projection.");

    priorBindings = Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories);
    previousProjections = Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories);
    File.WriteAllText(declaration, declarations.Replace("shouldEmit?:", "canEmit?:", StringComparison.Ordinal));
    await Run(Bindings("format", "count"));
    var updatedModel = File.ReadAllText(output);
    if (!updatedModel.Contains("@CanEmit", StringComparison.Ordinal) ||
        updatedModel.Contains("@ShouldEmit", StringComparison.Ordinal) ||
        Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories).Except(priorBindings).Count() != 1)
        throw new InvalidOperationException("Renaming the dependent callback must regenerate the public binding.");
    File.Delete(output);
    await Run(["build", "."], "CS0117");
    if (Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories).Except(previousProjections).Count() != 1)
        throw new InvalidOperationException("Renaming the dependent callback must regenerate the component projection.");

    File.WriteAllText(declaration, declarations);
    priorBindings = Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories);
    previousProjections = Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories);
    File.WriteAllText(sharedDeclaration, sharedSource.Replace("clear()", "reset()", StringComparison.Ordinal));
    await Run(Bindings("format", "count"));
    var updatedRef = File.ReadAllText(output);
    var newBindingCount = Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories).Except(priorBindings).Count();
    var hasReset = updatedRef.Contains("\"@#reset\"", StringComparison.Ordinal);
    var hasClear = updatedRef.Contains("\"@#clear\"", StringComparison.Ordinal);
    if (!hasReset || hasClear || newBindingCount != 1)
        throw new InvalidOperationException($"Changing the dependency ref method must regenerate the public binding. " +
            $"Reset present: {hasReset}, Clear present: {hasClear}, new entries: {newBindingCount}.");
    File.Delete(output);
    await Run(["build", "."], "CS1061");
    if (Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories).Except(previousProjections).Count() != 1)
        throw new InvalidOperationException("Changing the dependency ref method must regenerate the component projection.");

    priorBindings = Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories);
    previousProjections = Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories);
    File.WriteAllText(sharedDeclaration, sharedSource.Replace("locale?: string): boolean", "locale?: string): number", StringComparison.Ordinal));
    await Run(Bindings("format", "count"));
    var registryBindings = File.ReadAllText(output);
    if (!registryBindings.Contains("extern double @Matches(string @key);", StringComparison.Ordinal) ||
        registryBindings.Contains("extern bool @Matches(", StringComparison.Ordinal))
        throw new InvalidOperationException("Changing the linked registry result must regenerate the public signature.");
    var registryBindingsPath = Directory.GetFiles(generated, "Bindings.g.cs", SearchOption.AllDirectories).Except(priorBindings).Single();
    var registryBindingsStamp = Mark(registryBindingsPath);
    File.Delete(output);
    await Run(Bindings("count", "format"));
    if (File.ReadAllText(output) != registryBindings || File.GetLastWriteTimeUtc(registryBindingsPath) != registryBindingsStamp)
        throw new InvalidOperationException("The updated registry binding must restore its output from cache.");
    File.Delete(output);
    await Run(["build", "."], "CS0029");
    var registryProjection = Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories).Except(previousProjections).Single();
    var registryProjectionStamp = Mark(registryProjection);
    File.WriteAllText(app, source.Replace("bool ReadRegistry", "double ReadRegistry", StringComparison.Ordinal));
    var clientScript = Path.Combine(client.FullName, "client.js");
    File.WriteAllText(clientScript, File.ReadAllText(clientScript).Replace("key => key === 'ready'", "key => key.length", StringComparison.Ordinal));
    await Run(["build", "."]);
    var updatedComponent = File.ReadAllText(componentOutput);
    File.Delete(componentOutput);
    await Run(["build", "."]);
    if (File.ReadAllText(componentOutput) != updatedComponent || File.GetLastWriteTimeUtc(registryProjection) != registryProjectionStamp)
        throw new InvalidOperationException("The updated registry projection must restore cached component output.");
    File.WriteAllText(verification, File.ReadAllText(verification).Replace("ReadRegistry(), true", "ReadRegistry(), 5", StringComparison.Ordinal));
    await NodeModuleRunner.RunAsync(project.FullName, verification);
    Console.WriteLine("Verified concurrent publication, cross-process binding and component reuse, output restoration, LSP navigation, and declaration invalidation.");
}
finally
{
    project.Delete(recursive: true);
}

async Task Run(string[] arguments, string? diagnostic = null)
{
    using var process = Start(arguments);
    using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
    var stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
    var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
    try
    {
        await process.WaitForExitAsync(timeout.Token);
        var text = await stdout + await stderr;
        if (diagnostic is null ? process.ExitCode != 0 :
            process.ExitCode == 0 || !text.Contains(diagnostic, StringComparison.Ordinal))
            throw new InvalidOperationException($"jolt {string.Join(' ', arguments)} returned {process.ExitCode}\n{text}");
    }
    finally
    {
        await Stop(process);
    }
}

Process Start(string[] arguments)
{
    var start = new ProcessStartInfo(cli)
    {
        WorkingDirectory = project.FullName,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
    return Process.Start(start) ?? throw new InvalidOperationException($"Could not start {cli}.");
}

async Task VerifyDefinition(Action verifyCacheReuse)
{
    using var process = Start(["lsp"]);
    using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
    var stop = timeout.Token;
    var input = process.StandardInput.BaseStream;
    var output = process.StandardOutput.BaseStream;
    var errors = process.StandardError.ReadToEndAsync(stop);
    try
    {
        var uri = new Uri(app).AbsoluteUri;
        await Send("initialize", new { rootUri = new Uri(project.FullName).AbsoluteUri, capabilities = new { } }, 1);
        await Result(1);
        await Send("initialized", new { });
        await Send("textDocument/didOpen", new { textDocument = new { uri, languageId = "jazor", version = 1, text = source } });
        while (true)
        {
            var notification = await ReadMessage();
            if (notification.TryGetProperty("method", out var method) &&
                method.GetString() == "textDocument/publishDiagnostics" &&
                notification.GetProperty("params").GetProperty("uri").GetString() == uri &&
                notification.GetProperty("params").GetProperty("diagnostics").GetArrayLength() == 0)
                break;
        }
        var lines = source.Split('\n');
        var line = Array.FindIndex(lines, text => text.Contains("Client.Format", StringComparison.Ordinal));
        var character = lines[line].IndexOf("Format", StringComparison.Ordinal) + 2;
        await Send("textDocument/definition", new { textDocument = new { uri }, position = new { line, character } }, 2);
        var result = await Result(2);
        if (result.ValueKind != JsonValueKind.Object ||
            new Uri(result.GetProperty("uri").GetString()!).LocalPath != declaration)
            throw new InvalidOperationException($"LSP definition must navigate to the cached TypeScript declaration. Received {result}");
        verifyCacheReuse();
        var reexport = Path.Combine(client.FullName, "formatted.d.ts");
        var original = File.ReadAllText(declaration);
        try
        {
            File.WriteAllText(reexport, "// Moved declaration.\nexport declare function format(value: string): string;\n");
            File.WriteAllText(declaration, original.Replace("export declare function format(value: string): string;",
                "export { format } from './formatted.js';", StringComparison.Ordinal));
            await Send("textDocument/didSave", new { textDocument = new { uri = new Uri(declaration).AbsoluteUri } });
            await WaitDiagnostics(null);
            await Send("textDocument/definition", new { textDocument = new { uri }, position = new { line, character } }, 3);
            var updated = await Result(3);
            if (updated.ValueKind != JsonValueKind.Object || new Uri(updated.GetProperty("uri").GetString()!).LocalPath != reexport ||
                updated.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() != 1)
                throw new InvalidOperationException($"LSP definition must follow the saved re-export. Received {updated}");
            if (Directory.GetFiles(generated, "Projection.g.cs", SearchOption.AllDirectories).Length != 1)
                throw new InvalidOperationException("A moved definition must reuse the same component C# source.");

            File.WriteAllText(reexport, "export declare function format(value: number): string;\n");
            await Send("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = new Uri(reexport).AbsoluteUri, type = 2 } } });
            await WaitDiagnostics("CS1503");
        }
        finally
        {
            File.WriteAllText(declaration, original);
            File.Delete(reexport);
        }
        await Send("shutdown", new { }, 4);
        await Result(4);
        await Send("exit", new { });
        await process.WaitForExitAsync(stop);
        if (process.ExitCode != 0)
            throw new InvalidOperationException(await errors);

        async Task WaitDiagnostics(string? code)
        {
            while (true)
            {
                var notification = await ReadMessage();
                if (!notification.TryGetProperty("method", out var method) ||
                    method.GetString() != "textDocument/publishDiagnostics")
                    continue;
                var publication = notification.GetProperty("params");
                if (publication.GetProperty("uri").GetString() != uri)
                    continue;
                var diagnostics = publication.GetProperty("diagnostics");
                if (code is null ? diagnostics.GetArrayLength() == 0 :
                    diagnostics.EnumerateArray().Any(item => item.GetProperty("code").GetString() == code))
                    return;
            }
        }
    }
    finally
    {
        await Stop(process);
    }

    async Task Send(string method, object parameters, int? id = null)
    {
        var message = new Dictionary<string, object> { ["jsonrpc"] = "2.0", ["method"] = method, ["params"] = parameters };
        if (id is int request)
            message["id"] = request;
        var body = JsonSerializer.SerializeToUtf8Bytes(message);
        await input.WriteAsync(Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n"), stop);
        await input.WriteAsync(body, stop);
        await input.FlushAsync(stop);
    }

    async Task<JsonElement> Result(int id)
    {
        while (true)
        {
            var response = await ReadMessage();
            if (response.TryGetProperty("id", out var request) && request.GetInt32() == id)
                return response.GetProperty("result");
        }
    }

    async Task<JsonElement> ReadMessage()
    {
        var next = new byte[1];
        var header = new StringBuilder();
        while (!header.ToString().EndsWith("\r\n\r\n", StringComparison.Ordinal))
        {
            await output.ReadExactlyAsync(next, stop);
            header.Append((char)next[0]);
        }
        var length = header.ToString().Split("\r\n").Single(line =>
            line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase));
        var body = new byte[int.Parse(length.AsSpan("Content-Length:".Length), System.Globalization.CultureInfo.InvariantCulture)];
        await output.ReadExactlyAsync(body, stop);
        using var message = JsonDocument.Parse(body);
        return message.RootElement.Clone();
    }
}

static DateTime Mark(string path)
{
    File.SetLastWriteTimeUtc(path, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    return File.GetLastWriteTimeUtc(path);
}

static void Check(string directory, string pattern)
{
    if (!Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories).Any())
        throw new InvalidOperationException($"Expected {pattern} under {directory}.");
}

static async Task Stop(Process process)
{
    if (process.HasExited)
        return;
    process.Kill(entireProcessTree: true);
    await process.WaitForExitAsync();
}
