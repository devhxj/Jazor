#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.Npm/Jazor.Npm.csproj
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Jazor.NodeHost;
using Jazor.Npm;
using Microsoft.CodeAnalysis.CSharp;

if (args is not ([_] or [_, "--external-link" or "--workspace-lock" or "--global-cache"]))
    throw new ArgumentException("Usage: verify-jolt-projection-cache.cs <Jolt CLI apphost> [--external-link | --workspace-lock | --global-cache]");

var cli = Path.GetFullPath(args[0]);
var root = Directory.GetCurrentDirectory();
var store = Path.GetFullPath(Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.Combine(root, ".tmp", "nodeapi-store"));
var fixture = Path.Combine(root, "scripts", "csharp", "fixtures", "jolt-monaco-browser");
var project = Directory.CreateDirectory(Path.Combine(root, ".tmp", $"projection-cache-{Guid.NewGuid():N}"));
if (args.Length == 2)
{
    try
    {
        if (args[1] == "--global-cache")
            await VerifyGlobalCache();
        else if (args[1] == "--workspace-lock")
            await VerifyWorkspaceLock();
        else
            await VerifyExternalLink();
    }
    finally
    {
        project.Delete(recursive: true);
    }
    return;
}
var generated = NpmProjectionCache.ProjectionDirectory;
var metadata = Path.Combine(project.FullName, ".jazor", "cache");
var client = project.CreateSubdirectory("client");
var shared = project.CreateSubdirectory("shared");
var declaration = Path.Combine(client.FullName, "client.d.ts");
var sharedDeclaration = Path.Combine(shared.FullName, "index.d.ts");
const string sharedSource = """
    import type { Ref, ShallowRef, UnwrapRef } from 'vue';
    export interface Ledger { amount: Ref<number> }
    export type LedgerRef = ShallowRef<Ledger>;
    export type NamedRef<T> = Ref<T> & { clear(): void };
    export interface Registry {
        matches<S extends string, K extends 'reserved' = 'reserved'>(key: S | K, locale?: string): boolean;
    }
    """;
const string shallowLedger = "export type LedgerRef = ShallowRef<Ledger>;";
const string localeSource = """
    export declare const messages: {
        readonly en: { readonly welcome: 'Hello {guest}'; readonly cart: 'Item {0}'; readonly noticeStyle?: 'brief' | 'formal' };
        readonly fr: { readonly welcome: 'Bonjour {guest}' };
    };
    export declare const writableMessages: { readonly en: { readonly oldNotice: string; readonly noticeStyle?: 'compact' | 'expanded' } };
    """;
var localeDeclaration = Path.Combine(shared.FullName, "messages.d.ts");
var pluralLocaleSource = localeSource.Replace("'Item {0}'", "'None | {0} {1} | {1} {0}'", StringComparison.Ordinal);
var visitorLocaleSource = pluralLocaleSource.Replace("{guest}", "{visitor}", StringComparison.Ordinal);
var updatedWriteLocaleSource = visitorLocaleSource
    .Replace("oldNotice: string", "newNotice: string", StringComparison.Ordinal)
    .Replace("'compact' | 'expanded'", "'compact' | 'expanded' | 'detailed'", StringComparison.Ordinal);
const string deepLedger = "export type LedgerRef = Ref<UnwrapRef<Ledger>>;";
var deepSharedSource = sharedSource.Replace(shallowLedger, deepLedger, StringComparison.Ordinal);
var output = Path.Combine(project.FullName, "Client.g.cs");
var app = Path.Combine(project.FullName, "App.jazor");
var fixturePackage = "projection-cache-fixture-x" + Guid.NewGuid().ToString("N");
var fixtureNamespace = NpmNames.NamespaceFor(fixturePackage);
var source = $$"""
    @using Client = {{fixtureNamespace}}.Exports
    <p>@Label</p>
    @code {
        private string Label => Client.Select("ready", value => value.Length) > 0 ? Client.Format("ready") + Client.Count() : "";
        public static string ReadSelection() => Client.SelectMode("ready");
        public static string ReadRecord() => Client.ApplyPatch("ready", new());
        public static string ReadComputed() => Client.ConsumeComputed<string>(Client.Computed());
        public static bool ReadRegistry() => Client.OpenRegistry().Matches("ready");
        public static void ReadLedger() { _ = Client.OpenLedger().Value.Amount; }
        public static string Welcome() => Client.OpenComposer(false).TWelcome(new() { Guest = "Ada" });
        public static string Cart() => Client.OpenComposer(true).TCart("A");
        public static void WriteMessages()
        {
            var composer = Client.OpenWritableComposer();
            composer.SetLocaleMessageEn(new() { OldNotice = "set" });
            composer.MergeLocaleMessageEn(new() { OldNotice = "merge" });
        }
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
    manifest["dependencies"]![fixturePackage] = "link:./client";
    manifest["dependencies"]!["projection-cache-ref"] = "link:./shared";
    manifest["dependencies"]!["vue-i18n"] = "11.4.12";
    File.WriteAllText(Path.Combine(project.FullName, "package.json"), manifest.ToJsonString());
    File.WriteAllText(Path.Combine(shared.FullName, "package.json"),
        """{"name":"projection-cache-ref","version":"1.0.0","types":"./index.d.ts"}""");
    File.WriteAllText(sharedDeclaration, sharedSource);
    File.WriteAllText(localeDeclaration, localeSource);
    File.WriteAllText(Path.Combine(client.FullName, "package.json"),
        $$"""{"name":"{{fixturePackage}}","version":"1.0.0","type":"module","exports":"./client.js"}""");
    File.WriteAllText(Path.Combine(client.FullName, "client.js"),
        """
        import { computed as vueComputed, ref, shallowRef } from 'vue';
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
        export const openLedger = () => shallowRef({ amount: ref(3) });
        export const openComposer = () => ({
            t: (key, values, plural) => key === 'cart'
                ? JSON.stringify([values, plural ?? null])
                : 'Hello ' + (values.guest ?? values.visitor),
        });
        export const openWritableComposer = () => {
            const composer = {
                setLocaleMessage(locale, messages) { check(this, locale, messages, 'set'); },
                mergeLocaleMessage(locale, messages) { check(this, locale, messages, 'merge'); },
            };
            function check(receiver, locale, messages, expected) {
                if (receiver !== composer || locale !== 'en' || Object.values(messages)[0] !== expected)
                    throw Error('locale write argument order or receiver');
            }
            return composer;
        };
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
        import type { Composer } from 'vue-i18n';
        import type { messages, writableMessages } from '../shared/messages.js';
        import type { LedgerRef, NamedRef, Registry } from 'projection-cache-ref';
        export declare function openComposer(fallback: boolean): Composer<typeof messages>;
        export declare function openWritableComposer(): Composer<typeof writableMessages>;
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
        export declare function openLedger(): LedgerRef;
        interface ModelOptions<T, Passive extends boolean> { passive?: Passive; defaultValue?: T; shouldEmit?: (value: T) => boolean }
        export declare function bindModel<P extends object, K extends keyof P>(props: P, key?: K, emit?: (event: string, value: P[K]) => void, options?: ModelOptions<P[K], false>): WritableComputedRef<P[K]>;
        export declare function bindModel<P extends object, K extends keyof P>(props: P, key?: K, emit?: (event: string, value: P[K]) => void, options?: ModelOptions<P[K], true>): Ref<P[K]>;
        """);
    File.WriteAllText(app, source);
    await NodeToolchain.RestoreAsync(project.FullName, store, CancellationToken.None, Console.Out);

    var publicCache = NpmProjectionCache.ProjectionDirectory;
    var cancellationValues = (await new NpmValueMetadataProvider(store).ReadValueProjectionsAsync(
        app, fixturePackage, ["format", "count"])).Values;
    if (cancellationValues.Count != 2)
        throw new InvalidOperationException("The cancellation fixture must supply both npm bindings.");
    var cancellationCache = Path.Combine(project.FullName, "cancellation-cache");
    foreach (var standalone in new[] { true, false })
    {
        var lane = Path.Combine(cancellationCache, standalone ? "public" : "component");
        string Load(CancellationToken token, IReadOnlyList<string>? namespaces = null) => standalone
            ? NpmProjectionCache.LoadBindings(lane, namespaces ?? [], cancellationValues, token)
            : NpmProjectionCache.LoadComponent(lane, namespaces ?? [], cancellationValues, CSharpParseOptions.Default, token)
                .Tree.GetText(token).ToString();
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        ExpectCancellation(cancelled);
        using var duringKey = new CancellationTokenSource();
        ExpectCancellation(duringKey, new CancelOnEnumeration(duringKey));
        if (Directory.Exists(lane))
            throw new InvalidOperationException("A cancelled cold request published a cache entry.");

        var cold = Load(CancellationToken.None);
        var files = Directory.GetFiles(lane, "*", SearchOption.AllDirectories)
            .ToDictionary(file => Path.GetFileName(file)!, file => File.ReadAllText(file));
        var codeName = standalone ? "Bindings.g.cs" : "Projection.g.cs";
        if (files.Count != (standalone ? 1 : 2) || files[codeName] != cold ||
            (!standalone && (JsonSerializer.Deserialize<NpmValueProjection.Declaration[]>(files["symbols.json"])
                is not { Length: > 0 } indexEntries || indexEntries.Any(item => item.Start >= cold.Length))))
            throw new InvalidOperationException("A successful retry must publish complete C# and symbol index files.");
        ExpectCancellation(cancelled);
        if (Load(CancellationToken.None) != cold || Directory.GetFiles(lane, "*", SearchOption.AllDirectories)
            .Any(file => File.ReadAllText(file) != files[Path.GetFileName(file)]))
            throw new InvalidOperationException("Cancellation must preserve the warm snapshot and allow a retry.");

        void ExpectCancellation(CancellationTokenSource source, IReadOnlyList<string>? namespaces = null)
        {
            try { Load(source.Token, namespaces); }
            catch (OperationCanceledException error) when (error.CancellationToken == source.Token) { return; }
            throw new InvalidOperationException("The projection ignored cancellation.");
        }
    }

    async Task Bindings(string first, string second, string? destination = null)
    {
        var provider = new NpmValueMetadataProvider(store);
        var projection = await provider.ReadValueProjectionsAsync(app, fixturePackage,
            [first, "select", second, "snapshot", "wideRef", "narrowRef", "selectMode", "applyPatch",
             "computed", "consumeComputed", "bindModel", "getSharedRef", "openRegistry", "openLedger",
             "openComposer", "openWritableComposer"]);
        if (projection.Errors.Count != 0)
            throw new InvalidOperationException(string.Join(Environment.NewLine, projection.Errors.Values));
        var text = NpmBindings.GenerateCachedSource([], projection.Values);
        await File.WriteAllTextAsync(destination ?? output, text);
    }

    var parallelOutput = Path.Combine(project.FullName, "Concurrent.g.cs");
    await Task.WhenAll(Bindings("format", "count"), Bindings("count", "format", parallelOutput));
    var expected = File.ReadAllText(output);
    if (!expected.Contains("extern string @SelectMode(string @label);", StringComparison.Ordinal))
        throw new InvalidOperationException("Omitting the default-false flag must produce a string public binding.");
    if (!expected.Contains("@HasValue {", StringComparison.Ordinal))
        throw new InvalidOperationException("The public binding must allow omission of the generic patch value.");
    if (!expected.Contains("VueComputedRef<string> @Computed()", StringComparison.Ordinal))
        throw new InvalidOperationException("The public binding must preserve Vue's computed ref type.");
    if (!expected.Contains("extern bool @Matches(string @key);", StringComparison.Ordinal))
        throw new InvalidOperationException("The public binding must allow the defaulted registry call.");
    if (!expected.Contains("IVueRef<double> @Amount", StringComparison.Ordinal))
        throw new InvalidOperationException("A shallow ref must keep the linked DTO's nested ref.");
    if (File.ReadAllText(parallelOutput) != expected)
        throw new InvalidOperationException("Concurrent publishers must produce identical binding source.");
    File.Delete(parallelOutput);
    var bindings = FixtureFiles(publicCache, "Bindings.g.cs").Single();
    var bindingsStamp = Mark(bindings);
    File.Delete(output);
    await Bindings("count", "format");
    if (File.ReadAllText(output) != expected || File.GetLastWriteTimeUtc(bindings) != bindingsStamp ||
        FixtureFiles(publicCache, "Bindings.g.cs").Length != 1)
        throw new InvalidOperationException("A reordered bindings request must reuse cached text and restore its output.");

    File.Delete(output);
    await Run(["build", "."]);
    var projection = FixtureFiles(generated, "Projection.g.cs").SingleOrDefault() ??
        throw new InvalidOperationException($"No component projection for {fixturePackage} in {generated}. " +
            $"Fixture outputs: {string.Join(", ", Directory.GetFiles(Path.Combine(project.FullName, ".jazor")))}");
    Check(Path.GetDirectoryName(projection)!, "symbols.json");
    var projectionStamp = Mark(projection);
    var componentOutput = Path.Combine(project.FullName, ".jazor", "App.mjs");
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
    var expectedSymbols = File.ReadAllBytes(symbols);
    File.WriteAllText(symbols, "{broken");
    File.Delete(componentOutput);
    await Run(["build", "."]);
    if (!File.ReadAllBytes(symbols).SequenceEqual(expectedSymbols) ||
        File.GetLastWriteTimeUtc(projection) != projectionStamp ||
        File.ReadAllText(componentOutput) != componentText)
        throw new InvalidOperationException("Regenerating invalid JSON must repair the index and reuse the published source.");
    var repairedSymbolStamp = Mark(symbols);
    File.Delete(componentOutput);
    await Run(["build", "."]);
    if (File.GetLastWriteTimeUtc(symbols) != repairedSymbolStamp ||
        File.GetLastWriteTimeUtc(projection) != projectionStamp ||
        File.ReadAllText(componentOutput) != componentText)
        throw new InvalidOperationException("A warm build must reuse the repaired index and published source.");
    var verification = Path.Combine(project.FullName, "verify.mjs");
    File.WriteAllText(verification, """
        import assert from 'node:assert/strict';
        import { ReadRefs, ReadSelection, ReadRecord, ReadComputed, ReadModel, ReadSharedRef, ReadRegistry, Welcome, Cart, WriteMessages } from './.jazor/App.mjs';
        assert.equal(ReadRefs(), 'updated/initial');
        assert.equal(ReadSelection(), 'ready');
        assert.equal(ReadRecord(), 'ready');
        assert.equal(ReadComputed(), 'computed');
        assert.equal(ReadModel(), 'ready');
        assert.equal(ReadSharedRef(), 'cleared');
        assert.equal(ReadRegistry(), true);
        assert.equal(Welcome(), 'Hello Ada');
        assert.equal(Cart(), '[["A"],null]');
        WriteMessages();
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
    await Bindings("format", "count");
    if (File.ReadAllText(output) != expected || File.GetLastWriteTimeUtc(bindings) != bindingsStamp ||
        FixtureFiles(publicCache, "Bindings.g.cs").Length != 1)
        throw new InvalidOperationException("Changing declaration offsets must reuse the public C# cache entry.");
    File.Delete(output);
    await Run(["build", "."]);
    if (File.GetLastWriteTimeUtc(projection) != projectionStamp || File.ReadAllText(componentOutput) != componentText)
        throw new InvalidOperationException("Moving the declaration must reuse the component projection.");
    var previousProjections = FixtureFiles(generated, "Projection.g.cs");
    File.WriteAllText(declaration, declarations.Replace("=> R): R;", "=> R): S;", StringComparison.Ordinal));
    await Bindings("format", "count");
    if (File.ReadAllText(output) == expected || FixtureFiles(publicCache, "Bindings.g.cs").Length != 2)
        throw new InvalidOperationException("Changing the declaration must regenerate the public signature.");
    File.Delete(output);
    await Run(["build", "."], "CS0019");
    if (FixtureFiles(generated, "Projection.g.cs").Except(previousProjections).Count() != 1)
        throw new InvalidOperationException("Changing the generic result must regenerate the component projection.");

    var priorBindings = FixtureFiles(publicCache, "Bindings.g.cs");
    previousProjections = FixtureFiles(generated, "Projection.g.cs");
    File.WriteAllText(declaration, declarations.Replace("Ref<string, string | null>", "Ref<string, 'initial'>", StringComparison.Ordinal));
    await Bindings("format", "count");
    var readonlyBindings = File.ReadLines(output).Single(line => line.Contains("@WideRef()", StringComparison.Ordinal));
    if (!readonlyBindings.Contains("VueReadonlyRef<string>", StringComparison.Ordinal) ||
        FixtureFiles(publicCache, "Bindings.g.cs").Except(priorBindings).Count() != 1)
        throw new InvalidOperationException("Changing the permitted Ref writes must generate a new readonly public binding.");
    File.Delete(output);
    await Run(["build", "."], "CS0200");
    if (FixtureFiles(generated, "Projection.g.cs").Except(previousProjections).Count() != 1)
        throw new InvalidOperationException("Changing the Ref write policy must regenerate the component projection.");

    priorBindings = FixtureFiles(publicCache, "Bindings.g.cs");
    previousProjections = FixtureFiles(generated, "Projection.g.cs");
    File.WriteAllText(declaration, declarations.Replace("computed(): ComputedRef<string>", "computed(): Readonly<Ref<string>>",
        StringComparison.Ordinal));
    await Bindings("format", "count");
    if (!File.ReadAllText(output).Contains("VueReadonlyRef<string> @Computed()", StringComparison.Ordinal) ||
        FixtureFiles(publicCache, "Bindings.g.cs").Except(priorBindings).Count() != 1)
        throw new InvalidOperationException("Removing computed identity must generate a plain readonly public binding.");
    File.Delete(output);
    await Run(["build", "."], "CS1503");
    if (FixtureFiles(generated, "Projection.g.cs").Except(previousProjections).Count() != 1)
        throw new InvalidOperationException("Removing computed identity must regenerate the component projection.");

    priorBindings = FixtureFiles(publicCache, "Bindings.g.cs");
    previousProjections = FixtureFiles(generated, "Projection.g.cs");
    File.WriteAllText(declaration, declarations.Replace("B extends boolean = false", "B extends boolean = true", StringComparison.Ordinal));
    await Bindings("format", "count");
    if (!File.ReadAllText(output).Contains("extern bool @SelectMode(string @label);", StringComparison.Ordinal) ||
        FixtureFiles(publicCache, "Bindings.g.cs").Except(priorBindings).Count() != 1)
        throw new InvalidOperationException("Changing the omitted flag's default must generate a new boolean public binding.");
    File.Delete(output);
    await Run(["build", "."], "CS0029");
    if (FixtureFiles(generated, "Projection.g.cs").Except(previousProjections).Count() != 1)
        throw new InvalidOperationException("Changing the omitted flag's default must regenerate the component projection.");

    priorBindings = FixtureFiles(publicCache, "Bindings.g.cs");
    previousProjections = FixtureFiles(generated, "Projection.g.cs");
    File.WriteAllText(declaration, declarations.Replace("value?: T", "value: T", StringComparison.Ordinal));
    await Bindings("format", "count");
    var requiredBindings = File.ReadAllText(output);
    if (requiredBindings.Contains("@HasValue {", StringComparison.Ordinal) ||
        !requiredBindings.Split('\n').Any(line => line.Contains("public required ", StringComparison.Ordinal) &&
            line.Contains(" @Value {", StringComparison.Ordinal)) ||
        FixtureFiles(publicCache, "Bindings.g.cs").Except(priorBindings).Count() != 1)
        throw new InvalidOperationException("Making the generic patch value required must generate a new public binding.");
    File.Delete(output);
    await Run(["build", "."], "CS9035");
    if (FixtureFiles(generated, "Projection.g.cs").Except(previousProjections).Count() != 1)
        throw new InvalidOperationException("Making the generic patch value required must regenerate the component projection.");

    priorBindings = FixtureFiles(publicCache, "Bindings.g.cs");
    previousProjections = FixtureFiles(generated, "Projection.g.cs");
    File.WriteAllText(declaration, declarations.Replace("shouldEmit?:", "canEmit?:", StringComparison.Ordinal));
    await Bindings("format", "count");
    var updatedModel = File.ReadAllText(output);
    if (!updatedModel.Contains("@CanEmit", StringComparison.Ordinal) ||
        updatedModel.Contains("@ShouldEmit", StringComparison.Ordinal) ||
        FixtureFiles(publicCache, "Bindings.g.cs").Except(priorBindings).Count() != 1)
        throw new InvalidOperationException("Renaming the dependent callback must regenerate the public binding.");
    File.Delete(output);
    await Run(["build", "."], "CS0117");
    if (FixtureFiles(generated, "Projection.g.cs").Except(previousProjections).Count() != 1)
        throw new InvalidOperationException("Renaming the dependent callback must regenerate the component projection.");

    File.WriteAllText(declaration, declarations);
    priorBindings = FixtureFiles(publicCache, "Bindings.g.cs");
    previousProjections = FixtureFiles(generated, "Projection.g.cs");
    File.WriteAllText(sharedDeclaration, sharedSource.Replace("clear()", "reset()", StringComparison.Ordinal));
    await Bindings("format", "count");
    var updatedRef = File.ReadAllText(output);
    var newBindingCount = FixtureFiles(publicCache, "Bindings.g.cs").Except(priorBindings).Count();
    var hasReset = updatedRef.Contains("\"@#reset\"", StringComparison.Ordinal);
    var hasClear = updatedRef.Contains("\"@#clear\"", StringComparison.Ordinal);
    if (!hasReset || hasClear || newBindingCount != 1)
        throw new InvalidOperationException($"Changing the dependency ref method must regenerate the public binding. " +
            $"Reset present: {hasReset}, Clear present: {hasClear}, new entries: {newBindingCount}.");
    File.Delete(output);
    await Run(["build", "."], "CS1061");
    if (FixtureFiles(generated, "Projection.g.cs").Except(previousProjections).Count() != 1)
        throw new InvalidOperationException("Changing the dependency ref method must regenerate the component projection.");

    priorBindings = FixtureFiles(publicCache, "Bindings.g.cs");
    previousProjections = FixtureFiles(generated, "Projection.g.cs");
    File.WriteAllText(sharedDeclaration, sharedSource.Replace("locale?: string): boolean", "locale?: string): number", StringComparison.Ordinal));
    await Bindings("format", "count");
    var registryBindings = File.ReadAllText(output);
    if (!registryBindings.Contains("extern double @Matches(string @key);", StringComparison.Ordinal) ||
        registryBindings.Contains("extern bool @Matches(", StringComparison.Ordinal))
        throw new InvalidOperationException("Changing the linked registry result must regenerate the public signature.");
    var registryBindingsPath = FixtureFiles(publicCache, "Bindings.g.cs").Except(priorBindings).Single();
    var registryBindingsStamp = Mark(registryBindingsPath);
    File.Delete(output);
    await Bindings("count", "format");
    if (File.ReadAllText(output) != registryBindings || File.GetLastWriteTimeUtc(registryBindingsPath) != registryBindingsStamp)
        throw new InvalidOperationException("The updated registry binding must restore its output from cache.");
    File.Delete(output);
    await Run(["build", "."], "CS0029");
    var registryProjection = FixtureFiles(generated, "Projection.g.cs").Except(previousProjections).Single();
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
    priorBindings = FixtureFiles(publicCache, "Bindings.g.cs");
    previousProjections = FixtureFiles(generated, "Projection.g.cs");
    File.WriteAllText(sharedDeclaration, deepSharedSource.Replace("locale?: string): boolean", "locale?: string): number", StringComparison.Ordinal));
    await Bindings("format", "count");
    var deepBindings = File.ReadAllText(output);
    if (!deepBindings.Contains("double @Amount", StringComparison.Ordinal) ||
        deepBindings.Contains("IVueRef<double> @Amount", StringComparison.Ordinal))
        throw new InvalidOperationException("The linked deep ref must expose the unwrapped numeric property.");
    var deepBindingsPath = FixtureFiles(publicCache, "Bindings.g.cs").Except(priorBindings).Single();
    var deepBindingsStamp = Mark(deepBindingsPath);
    File.Delete(output);
    await Bindings("count", "format");
    if (File.ReadAllText(output) != deepBindings || File.GetLastWriteTimeUtc(deepBindingsPath) != deepBindingsStamp)
        throw new InvalidOperationException("The deep-ref binding must restore its cached source.");
    File.Delete(output);
    await Run(["build", "."]);
    var deepProjection = FixtureFiles(generated, "Projection.g.cs").Except(previousProjections).Single();
    var deepProjectionStamp = Mark(deepProjection);
    await Run(["build", "."]);
    if (File.GetLastWriteTimeUtc(deepProjection) != deepProjectionStamp)
        throw new InvalidOperationException("The deep-ref component must reuse its cached projection.");
    await Bindings("format", "count");
    var guestBindings = File.ReadAllText(output);
    if (!guestBindings.Contains("@Guest {", StringComparison.Ordinal))
        throw new InvalidOperationException("Expected a typed Guest argument in the initial locale binding.");
    File.WriteAllText(localeDeclaration, visitorLocaleSource);
    var beforeVisitor = FixtureFiles(publicCache, "Bindings.g.cs");
    await Bindings("format", "count");
    var visitorBindings = File.ReadAllText(output);
    if (!visitorBindings.Contains("@Visitor {", StringComparison.Ordinal) || visitorBindings.Contains("@Guest {", StringComparison.Ordinal))
        throw new InvalidOperationException("The linked locale edit must replace Guest with Visitor in the public source.");
    var visitorBinding = FixtureFiles(publicCache, "Bindings.g.cs").Except(beforeVisitor).Single();
    var visitorBindingStamp = Mark(visitorBinding);
    File.Delete(output);
    await Bindings("count", "format");
    if (File.ReadAllText(output) != visitorBindings || File.GetLastWriteTimeUtc(visitorBinding) != visitorBindingStamp)
        throw new InvalidOperationException("The updated locale binding must restore its cached source.");
    File.Delete(output);
    await Run(["build", "."], "CS0117");
    File.WriteAllText(app, File.ReadAllText(app).Replace("Guest =", "Visitor =", StringComparison.Ordinal));
    await Run(["build", "."], "CS7036");
    File.WriteAllText(app, File.ReadAllText(app).Replace("TCart(\"A\")", "TCart(\"A\", false, 2d)", StringComparison.Ordinal));
    await Run(["build", "."]);
    File.WriteAllText(verification, File.ReadAllText(verification)
        .Replace("""[["A"],null]""", """[["A",false],2]""", StringComparison.Ordinal));
    await NodeModuleRunner.RunAsync(project.FullName, verification);
    var localeProjections = FixtureFiles(generated, "Projection.g.cs");
    var localeProjection = localeProjections.Single(path => File.ReadAllText(path).Contains("@Visitor", StringComparison.Ordinal) &&
        File.ReadAllText(path).Contains("double @Amount", StringComparison.Ordinal));
    var localeProjectionStamp = Mark(localeProjection);
    await Run(["build", "."]);
    if (File.GetLastWriteTimeUtc(localeProjection) != localeProjectionStamp ||
        FixtureFiles(generated, "Projection.g.cs").Length != localeProjections.Length)
        throw new InvalidOperationException("The updated locale component must reuse its cached projection.");
    var beforeWrite = FixtureFiles(publicCache, "Bindings.g.cs");
    File.WriteAllText(localeDeclaration, updatedWriteLocaleSource);
    await Bindings("format", "count");
    var writeBindings = File.ReadAllText(output);
    if (!visitorBindings.Contains("string @OldNotice", StringComparison.Ordinal) ||
        !writeBindings.Contains("string @NewNotice", StringComparison.Ordinal) ||
        writeBindings.Contains("string @OldNotice", StringComparison.Ordinal))
        throw new InvalidOperationException("The linked locale edit must replace OldNotice with NewNotice in the public source.");
    var originalNoticeDomain = ReadNoticeStyleDomain(visitorBindings, "compact");
    var updatedNoticeDomain = ReadNoticeStyleDomain(writeBindings, "compact");
    var otherNoticeDomain = ReadNoticeStyleDomain(visitorBindings, "brief");
    if (originalNoticeDomain.Name != updatedNoticeDomain.Name ||
        !originalNoticeDomain.Values.SetEquals(["compact", "expanded"]) ||
        !updatedNoticeDomain.Values.SetEquals(["compact", "expanded", "detailed"]) ||
        otherNoticeDomain.Name == originalNoticeDomain.Name ||
        !otherNoticeDomain.Values.SetEquals(["brief", "formal"]))
        throw new InvalidOperationException("Locale enum names must survive a domain edit and distinguish same-path fields from different schemas.");
    var writeBinding = FixtureFiles(publicCache, "Bindings.g.cs").Except(beforeWrite).Single();
    var writeBindingStamp = Mark(writeBinding);
    File.Delete(output);
    await Bindings("count", "format");
    if (File.ReadAllText(output) != writeBindings || File.GetLastWriteTimeUtc(writeBinding) != writeBindingStamp)
        throw new InvalidOperationException("The updated locale write binding must restore its cached source.");
    File.Delete(output);
    await Run(["build", "."], "CS0117");
    File.WriteAllText(app, File.ReadAllText(app).Replace("OldNotice =", "NewNotice =", StringComparison.Ordinal));
    await Run(["build", "."]);
    await NodeModuleRunner.RunAsync(project.FullName, verification);
    Console.WriteLine("Verified concurrent publication, cross-process binding and component reuse, output restoration, LSP navigation, and declaration invalidation.");
}
finally
{
    if (Directory.Exists(generated))
        foreach (var entry in FixtureFiles(generated, "Bindings.g.cs")
                     .Concat(FixtureFiles(generated, "Projection.g.cs")).Select(Path.GetDirectoryName).Distinct())
        {
            // Cache entries are direct children of the global projection directory.
            if (entry is not null && string.Equals(Path.GetDirectoryName(entry), generated, StringComparison.OrdinalIgnoreCase))
                Directory.Delete(entry, recursive: true);
        }
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
    start.Environment["JazorPnpmStoreDirectory"] = store;
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
        var amountLine = Array.FindIndex(lines, text => text.Contains(".Value.Amount", StringComparison.Ordinal));
        var amountCharacter = lines[amountLine].IndexOf("Amount", StringComparison.Ordinal) + 2;
        var amountPosition = new { line = amountLine, character = amountCharacter };
        await VerifyAmountHover(5, "IVueRef<double>");
        try
        {
            File.WriteAllText(sharedDeclaration, deepSharedSource);
            await Send("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = new Uri(sharedDeclaration).AbsoluteUri, type = 2 } } });
            await WaitDiagnostics(null);
            await VerifyAmountHover(6, "double");
        }
        finally
        {
            File.WriteAllText(sharedDeclaration, sharedSource);
            await Send("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = new Uri(sharedDeclaration).AbsoluteUri, type = 2 } } });
            await WaitDiagnostics(null);
        }
        var projectionCount = FixtureFiles(generated, "Projection.g.cs").Length;
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
            if (FixtureFiles(generated, "Projection.g.cs").Length != projectionCount)
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
        File.WriteAllText(localeDeclaration, pluralLocaleSource);
        await Send("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = new Uri(localeDeclaration).AbsoluteUri, type = 2 } } });
        await WaitDiagnostics("CS7036");
        File.WriteAllText(localeDeclaration, visitorLocaleSource);
        await Send("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = new Uri(localeDeclaration).AbsoluteUri, type = 2 } } });
        await WaitDiagnostics("CS0117");
        var visitorSource = source.Replace("Guest =", "Visitor =", StringComparison.Ordinal)
            .Replace("TCart(\"A\")", "TCart(\"A\", false, 2d)", StringComparison.Ordinal);
        await ChangeComponent(2, visitorSource);
        await WaitDiagnostics(null);
        File.WriteAllText(localeDeclaration, updatedWriteLocaleSource);
        await Send("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = new Uri(localeDeclaration).AbsoluteUri, type = 2 } } });
        await WaitDiagnostics("CS0117", "OldNotice");
        await ChangeComponent(3, visitorSource.Replace("OldNotice =", "NewNotice =", StringComparison.Ordinal));
        await WaitDiagnostics(null);
        File.WriteAllText(localeDeclaration, localeSource);
        await Send("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = new Uri(localeDeclaration).AbsoluteUri, type = 2 } } });
        await ChangeComponent(4, source);
        await WaitDiagnostics(null);
        await Send("shutdown", new { }, 4);
        await Result(4);
        await Send("exit", new { });
        await process.WaitForExitAsync(stop);
        if (process.ExitCode != 0)
            throw new InvalidOperationException(await errors);

        async Task VerifyAmountHover(int id, string type)
        {
            await Send("textDocument/hover", new { textDocument = new { uri }, position = amountPosition }, id);
            var hover = await Result(id);
            var value = hover.ValueKind == JsonValueKind.Object ? hover.GetProperty("contents").GetProperty("value").GetString() : null;
            if (value is null || !value.StartsWith(type + " ", StringComparison.Ordinal) &&
                !value.StartsWith("ECMAScript.Vue." + type + " ", StringComparison.Ordinal))
                throw new InvalidOperationException($"LSP hover for the linked leaf must show {type}. Received {hover}");
        }

        async Task WaitDiagnostics(string? code, string? member = null)
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
                    diagnostics.EnumerateArray().Any(item => item.GetProperty("code").GetString() == code &&
                        (member is null || item.GetProperty("message").GetString()!.Contains(member, StringComparison.Ordinal))))
                    return;
            }
        }

        Task ChangeComponent(int version, string text) => Send("textDocument/didChange", new
        {
            textDocument = new { uri, version },
            contentChanges = new[] { new { text } },
        });
    }
    finally
    {
        await Stop(process);
    }

    Task Send(string method, object parameters, int? id = null) =>
        SendLsp(input, method, parameters, id, stop);

    async Task<JsonElement> Result(int id)
    {
        while (true)
        {
            var response = await ReadMessage();
            if (response.TryGetProperty("id", out var request) && request.GetInt32() == id)
                return response.GetProperty("result");
        }
    }

    Task<JsonElement> ReadMessage() => ReadLsp(output, stop);
}

string[] FixtureFiles(string cache, string name) =>
    Directory.EnumerateFiles(cache, name, SearchOption.AllDirectories)
        .Where(path =>
        {
            var source = File.ReadAllText(path);
            return source.Contains(fixturePackage, StringComparison.Ordinal) ||
                source.Contains(fixtureNamespace, StringComparison.Ordinal);
        }).ToArray();

async Task VerifyGlobalCache()
{
    using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
    var stop = timeout.Token;
    var portable = project.CreateSubdirectory("portable").FullName;
    var generatedCache = project.CreateSubdirectory("generated").FullName;
    var lockSource = File.ReadAllText(Path.Combine(root, "scripts", "csharp", "fixtures", "jolt-npm-runtime", "pnpm-lock.yaml"));
    var lockedPackage = Regex.Match(lockSource, @"date-fns@4\.4\.0:\s+resolution: \{integrity: (sha512-[A-Za-z0-9+/=]+)\}").Groups[1].Value;
    if (lockedPackage.Length == 0)
        throw new InvalidOperationException("Missing the date-fns 4.4.0 integrity.");
    var one = await Create("first");
    var two = await Create("second");
    var firstChecks = 0;
    var first = await Read(one, () => firstChecks++);
    if (firstChecks == 0)
        throw new InvalidOperationException("Cold workspace did not exercise the TypeScript checker.");
    var paths = Directory.GetFiles(portable, "*.json", SearchOption.AllDirectories);
    if (paths.Length == 0)
        throw new InvalidOperationException("Cold registry package did not publish portable metadata.");
    var firstBindings = NpmProjectionCache.LoadBindings(generatedCache, [], [first], stop);
    var firstComponent = NpmProjectionCache.LoadComponent(generatedCache, [], [first], CSharpParseOptions.Default, stop);
    var files = Directory.GetFiles(generatedCache, "*", SearchOption.AllDirectories);
    var times = files.Select(File.GetLastWriteTimeUtc).ToArray();
    var second = await Read(two, () => throw new InvalidOperationException("Warm second workspace invoked the TypeScript checker."));
    if (first.Definition is not { } localFirst || second.Definition is not { } localSecond ||
        !Path.GetFullPath(localFirst.Path).StartsWith(one + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
        !Path.GetFullPath(localSecond.Path).StartsWith(two + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Portable metadata did not rebase definition paths to each local workspace.");
    var secondBindings = NpmProjectionCache.LoadBindings(generatedCache, [], [second], stop);
    var secondComponent = NpmProjectionCache.LoadComponent(generatedCache, [], [second], CSharpParseOptions.Default, stop);
    if (firstBindings != secondBindings || firstComponent.Tree.ToString() != secondComponent.Tree.ToString() ||
        Directory.GetFiles(generatedCache, "*", SearchOption.AllDirectories).Length != files.Length ||
        !files.Select(File.GetLastWriteTimeUtc).SequenceEqual(times))
        throw new InvalidOperationException("The second workspace did not reuse the immutable C# and component source.");

    var firstModule = await BuildComponent(one);
    var secondModule = await BuildComponent(two);
    if (firstModule != secondModule || !secondModule.Contains("from 'date-fns'", StringComparison.Ordinal) &&
        !secondModule.Contains("from \"date-fns\"", StringComparison.Ordinal))
        throw new InvalidOperationException("Both Jolt projects must emit the same registry-backed component module.");

    var declaration = localSecond.Path;
    var declarationText = await File.ReadAllTextAsync(declaration, stop);
    await ReplaceDeclaration(declaration, "// Local declaration offset\n" + declarationText);
    var changedChecks = 0;
    var changed = await Read(two, () => changedChecks++);
    if (changedChecks == 0 || changed.Definition is not { } refreshed || refreshed.Start <= localSecond.Start)
        throw new InvalidOperationException("Changed local declaration reused stale portable metadata.");
    var firstAgain = await Read(one, () => throw new InvalidOperationException("The modified declaration displaced the original portable metadata."));
    if (firstAgain.Definition != first.Definition)
        throw new InvalidOperationException("The original workspace lost its local definition after a declaration variant was published.");
    Console.WriteLine($"Independent locked date-fns workspaces reused metadata and generated C# with zero warm checker callbacks. Cold callbacks: {firstChecks}. Modified declaration callbacks: {changedChecks}. Both definition variants remained valid for their workspaces.");

    await SetPeer(one, localFirst.Path, "string");
    await SetPeer(two, localSecond.Path, "number");
    var peerA = await Read(one, () => { });
    var peerChecks = 0;
    var peerB = await Read(two, () => peerChecks++);
    if (peerChecks == 0 || peerA.Signatures.Single().ReturnType.Kind != "string" ||
        peerB.Signatures.Single().ReturnType.Kind != "number")
        throw new InvalidOperationException($"Distinct peer declarations must yield distinct projections. A: {peerA.Signatures.Single().ReturnType.Kind}, B: {peerB.Signatures.Single().ReturnType.Kind}, callbacks: {peerChecks}.");
    var peerAAgain = await Read(one, () => throw new InvalidOperationException("The second peer displaced the first peer's snapshot."));
    if (peerA.Signatures.Single().ReturnType != peerAAgain.Signatures.Single().ReturnType)
        throw new InvalidOperationException("The first peer lost its return type after the second peer was queried.");
    Console.WriteLine("Distinct installed peer declarations produced distinct return types and preserved both snapshots.");

    const string documentedDeclarations = """
        /**
         * Text overload <with markup> & detail.
         * @param label Text to read.
         * @returns Text result.
         */
        export declare function addDays(label: string): string;
        /**
         * Numeric overload.
         * @param amount Numeric input.
         * @returns Numeric result.
         */
        export declare function addDays(amount: number, base?: number): number;
        """;
    await ReplaceDeclaration(localFirst.Path, documentedDeclarations);
    await ReplaceDeclaration(localSecond.Path, documentedDeclarations);
    var documented = await Read(one, () => { });
    var documentedSources = Sources(documented);
    CheckDocumentation(documentedSources, "Text result.");
    var documentedWarm = await Read(two, () => throw new InvalidOperationException("The matching workspace did not reuse documented metadata."));
    if (Sources(documentedWarm) != documentedSources)
        throw new InvalidOperationException("The warm workspaces emitted different documentation.");

    await ReplaceDeclaration(localSecond.Path, documentedDeclarations.Replace("Text result.", "Revised text result.", StringComparison.Ordinal));
    var documentationChecks = 0;
    var edited = await Read(two, () => documentationChecks++);
    var editedSources = Sources(edited);
    if (documentationChecks != 1 || editedSources.Bindings == documentedSources.Bindings ||
        editedSources.Component == documentedSources.Component)
        throw new InvalidOperationException("A docs-only edit did not refresh both generated source caches.");
    CheckDocumentation(editedSources, "Revised text result.");

    await ReplaceDeclaration(localSecond.Path, documentedDeclarations);
    var restored = await Read(two, () => throw new InvalidOperationException("Restored declaration bytes did not reuse the original metadata."));
    if (Sources(restored) != documentedSources)
        throw new InvalidOperationException("Restored declarations emitted stale documentation.");
    Console.WriteLine("Both generated C# projections kept overload-specific documentation, refreshed a docs-only edit, and reused the restored text across workspaces.");

    (string Bindings, string Component) Sources(NpmValueExport value) => (
        NpmProjectionCache.LoadBindings(generatedCache, [], [value], stop),
        NpmProjectionCache.LoadComponent(generatedCache, [], [value], CSharpParseOptions.Default, stop).Tree.ToString());

    void CheckDocumentation((string Bindings, string Component) sources, string textResult)
    {
        foreach (var generatedSource in new[] { sources.Bindings, sources.Component })
        {
            var methods = CSharpSyntaxTree.ParseText(generatedSource, cancellationToken: stop).GetRoot(stop).DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().Where(method => method.Identifier.ValueText == "AddDays")
                .ToLookup(method => method.ParameterList.Parameters.First().Identifier.ValueText);
            CheckMethod("label", "Text overload <with markup> & detail.", "Text to read.", textResult);
            CheckMethod("amount", "Numeric overload.", "Numeric input.", "Numeric result.");
            if (!methods["amount"].Any(method => method.ParameterList.Parameters.Any(parameter => parameter.Identifier.ValueText == "base")))
                throw new InvalidOperationException("The generated numeric overload omitted the undocumented optional parameter.");
            if (generatedSource.Contains(one, StringComparison.OrdinalIgnoreCase) ||
                generatedSource.Contains(two, StringComparison.OrdinalIgnoreCase) ||
                generatedSource.Contains("file://", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Shared generated source contains an absolute declaration link.");

            void CheckMethod(string name, string summary, string parameter, string returns)
            {
                if (!methods.Contains(name))
                    throw new InvalidOperationException($"Missing the overload for AddDays({name}).");
                foreach (var method in methods[name])
                {
                    var lines = method.GetLeadingTrivia().ToFullString().Split('\n')
                        .Select(line => line.Trim()).Where(line => line.StartsWith("///", StringComparison.Ordinal))
                        .Select(line => line[3..]);
                    var xml = XElement.Parse("<doc>" + string.Join('\n', lines) + "</doc>");
                    var param = xml.Elements("param").SingleOrDefault();
                    if (xml.Element("summary")?.Value.Trim() != summary ||
                        param?.Attribute("name")?.Value != name || param?.Value.Trim() != parameter ||
                        xml.Element("returns")?.Value.Trim() != returns)
                        throw new InvalidOperationException($"Unexpected documentation for AddDays({name}): {xml}");
                }
            }
        }
    }

    async Task ReplaceDeclaration(string path, string contents)
    {
        // An installed declaration may be hard-linked into the shared pnpm store.
        await File.WriteAllTextAsync(path + ".next", contents, stop);
        File.Move(path + ".next", path, overwrite: true);
    }

    async Task SetPeer(string directory, string path, string returnType)
    {
        // Install beside the package in pnpm's virtual store, the same resolution
        // location a registry package's declared peer uses.
        var scope = Path.GetDirectoryName(Path.GetDirectoryName(path))!;
        var peer = Directory.CreateDirectory(Path.Combine(scope, "projection-cache-peer")).FullName;
        await File.WriteAllTextAsync(Path.Combine(peer, "package.json"),
            """{"name":"projection-cache-peer","version":"1.0.0","types":"index.d.ts"}""", stop);
        await File.WriteAllTextAsync(Path.Combine(peer, "index.d.ts"), $"export declare function getPeer(): {returnType};", stop);
        await ReplaceDeclaration(path, """
            import { getPeer } from 'projection-cache-peer';
            export { getPeer as addDays };
            """);
    }

    async Task<string> BuildComponent(string directory)
    {
        await File.WriteAllTextAsync(Path.Combine(directory, "App.jazor"), """
            @using Dates = Npm.DateFns.Exports
            <p>Calendar</p>
            @code {
                public static ECMAScript.Date Tomorrow(ECMAScript.Date today) => Dates.AddDays(today, 1);
            }
            """, stop);
        var start = new ProcessStartInfo(cli) { WorkingDirectory = directory, UseShellExecute = false };
        start.ArgumentList.Add("build");
        start.ArgumentList.Add(".");
        start.Environment["JazorPnpmStoreDirectory"] = store;
        using var process = Process.Start(start) ?? throw new InvalidOperationException($"Could not start {cli}.");
        try
        {
            await process.WaitForExitAsync(stop);
            if (process.ExitCode != 0)
                throw new InvalidOperationException($"Jolt build in {directory} exited with {process.ExitCode}.");
        }
        finally
        {
            await Stop(process);
        }
        return await File.ReadAllTextAsync(Path.Combine(directory, ".jazor", "App.mjs"), stop);
    }

    async Task<string> Create(string name)
    {
        var directory = project.CreateSubdirectory(name).FullName;
        await File.WriteAllTextAsync(Path.Combine(directory, "package.json"),
            $$$"""{"name":"cache-fixture-{{{name}}}","private":true,"type":"module","dependencies":{"date-fns":"4.4.0"}}""", stop);
        await File.WriteAllTextAsync(Path.Combine(directory, "pnpm-lock.yaml"), $$"""
            lockfileVersion: '9.0'
            settings:
              autoInstallPeers: true
              excludeLinksFromLockfile: false
            importers:
              .:
                dependencies:
                  date-fns:
                    specifier: 4.4.0
                    version: 4.4.0
            packages:
              date-fns@4.4.0:
                resolution: {integrity: {{lockedPackage}}}
            snapshots:
              date-fns@4.4.0: {}
            """, stop);
        return directory;
    }

    Task<NpmValueExport> Read(string directory, Action observe) =>
        new NpmValueMetadataProvider(store) { PortableCacheDirectory = portable, ObserveChecker = observe }
            .ReadValueAsync(Path.Combine(directory, "App.jazor"), "date-fns", "addDays", stop);
}

async Task VerifyWorkspaceLock()
{
    var child = project.CreateSubdirectory("app").FullName;
    await File.WriteAllTextAsync(Path.Combine(project.FullName, "pnpm-workspace.yaml"), "packages:\n  - app\n");
    await File.WriteAllTextAsync(Path.Combine(child, "package.json"), """
        {"private":true,"type":"module","dependencies":{"date-fns":"4.4.0","vue":"3.5.42"},"devDependencies":{"vite":"8.2.0"}}
        """);
    const string source = """
        @using Dates = Npm.DateFns.Exports
        <p>@(Dates.AddDays(new ECMAScript.Date(2024, 0, 31), 2).GetDate())</p>
        """;
    var component = Path.Combine(child, "App.jazor");
    await File.WriteAllTextAsync(component, source);
    var start = new ProcessStartInfo(cli)
    {
        WorkingDirectory = child,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };
    start.Environment["JazorPnpmStoreDirectory"] = store;
    foreach (var arg in new[] { "dev", ".", "--port", "0" })
        start.ArgumentList.Add(arg);
    using var server = Process.Start(start)!;
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(50));
    var stop = timeout.Token;
    var events = System.Threading.Channels.Channel.CreateUnbounded<string>();
    server.OutputDataReceived += (_, line) => Report(line.Data);
    server.ErrorDataReceived += (_, line) => Report(line.Data);
    server.BeginOutputReadLine();
    server.BeginErrorReadLine();
    try
    {
        await WaitFor(line => line.StartsWith("http://", StringComparison.Ordinal));
        var path = Path.Combine(project.FullName, "pnpm-lock.yaml");
        var original = await File.ReadAllTextAsync(path, stop);
        if (File.Exists(Path.Combine(child, "pnpm-lock.yaml")))
            throw new InvalidOperationException("Expected the pnpm lockfile only at the workspace root.");
        await Replace("not: [valid");
        await WaitFor(line => line.Contains("JOLT1001", StringComparison.Ordinal) &&
            line.Contains(path, StringComparison.OrdinalIgnoreCase) && line.Contains("unclosed bracket", StringComparison.Ordinal));
        await Replace(original);
        await WaitFor(line => line.StartsWith("Compiled ", StringComparison.Ordinal));
        if (await File.ReadAllTextAsync(component, stop) != source)
            throw new InvalidOperationException("The workspace lock check modified the component.");
        Console.WriteLine("Jolt dev diagnosed and recovered a broken parent workspace lock without a component edit.");

        async Task Replace(string contents)
        {
            await File.WriteAllTextAsync(path + ".next", contents, stop);
            File.Move(path + ".next", path, overwrite: true);
        }
    }
    finally
    {
        if (!server.HasExited)
            server.Kill(entireProcessTree: true);
        await server.WaitForExitAsync();
    }

    void Report(string? line)
    {
        if (line is null)
            return;
        Console.WriteLine(line);
        events.Writer.TryWrite(line);
    }

    async Task WaitFor(Func<string, bool> predicate)
    {
        await foreach (var line in events.Reader.ReadAllAsync(stop))
            if (predicate(line))
                return;
    }
}

async Task VerifyExternalLink()
{
    var link = Directory.CreateDirectory(project.FullName + "-linked");
    try
    {
        File.WriteAllText(Path.Combine(project.FullName, "package.json"),
            $$$"""{"private":true,"dependencies":{"external-probe":"link:../{{{link.Name}}}"}}""");
        File.WriteAllText(Path.Combine(link.FullName, "package.json"),
            """{"name":"external-probe","version":"1.0.0","type":"module","exports":{".":{"types":"./index.d.ts","import":"./index.js"}}}""");
        File.WriteAllText(Path.Combine(link.FullName, "index.js"),
            "export function format(value) { return String(value); }\n");
        var index = Path.Combine(link.FullName, "index.d.ts");
        var initial = Path.Combine(link.FullName, "initial.d.ts");
        var updated = Path.Combine(link.FullName, "updated.d.ts");
        const string original = "export { format } from './initial.js';\n";
        File.WriteAllText(index, original);
        File.WriteAllText(initial, "export declare function format(value: string): string;\n");
        File.WriteAllText(updated, "// New declaration\nexport declare function format(value: number): string;\n");
        var file = Path.Combine(project.FullName, "External.jazor");
        const string disk = "<p>Disk source</p>";
        File.WriteAllText(file, disk);
        const string buffer = "@using External = Npm.ExternalProbe.Exports\n<p>@External.Format(default)</p>\n<p>@External.Format(\"editor\")</p>\n";
        var uri = new Uri(file).AbsoluteUri;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        var stop = timeout.Token;
        await NodeToolchain.RestoreAsync(project.FullName, store, stop, Console.Out);
        using var process = Start(["lsp"]);
        var input = process.StandardInput.BaseStream;
        var output = process.StandardOutput.BaseStream;
        var errors = process.StandardError.ReadToEndAsync(stop);
        var phase = "starting LSP";
        JsonElement lastDiagnostics = default;
        try
        {
            await Send("initialize", new { rootUri = new Uri(project.FullName).AbsoluteUri, capabilities = new { } }, 1);
            await ReadUntil(m => m.TryGetProperty("id", out var id) && id.GetInt32() == 1);
            await Send("initialized", new { });
            await Send("textDocument/didOpen", new { textDocument = new { uri, languageId = "jazor", version = 1, text = buffer } });
            phase = "binding the original declaration";
            await Diagnostics(null);
            await Definition(2, initial, 0);

            // External files receive no LSP edit or watched-file notification.
            phase = "observing the external re-export change";
            File.WriteAllText(index, "export { format } from './updated.js';\n");
            await Diagnostics("CS1503");
            await Definition(3, updated, 1);
            phase = "observing a change to the transitive declaration itself";
            File.WriteAllText(updated, "// Same export, moved and corrected\n\nexport declare function format(value: string): string;\n");
            await Diagnostics(null);
            await Definition(4, updated, 2);
            phase = "observing the next signature of the active declaration";
            File.WriteAllText(updated, "// Numeric signature\n\nexport declare function format(value: number): string;\n");
            await Diagnostics("CS1503");
            phase = "observing restoration of the original external re-export";
            File.WriteAllText(index, original);
            await Diagnostics(null);
            await Definition(5, initial, 0);
            if (File.ReadAllText(file) != disk)
                throw new InvalidOperationException("The external declaration test changed the on-disk .jazor source.");
            await Send("shutdown", new { }, 6);
            await ReadUntil(m => m.TryGetProperty("id", out var id) && id.GetInt32() == 6);
            await Send("exit", new { });
            await process.WaitForExitAsync(stop);
            if (process.ExitCode != 0)
                throw new InvalidOperationException(await errors);
            Console.WriteLine("Jolt LSP watched the external link declaration, updated diagnostics and F12, and recovered with the same unsaved buffer.");
        }
        catch (OperationCanceledException ex) when (stop.IsCancellationRequested)
        {
            throw new InvalidOperationException($"Timed out while {phase}. Last diagnostics: {lastDiagnostics}.", ex);
        }
        finally
        {
            await Stop(process);
        }

        async Task Definition(int id, string expected, int line)
        {
            await Send("textDocument/definition", new { textDocument = new { uri }, position = new { line = 1, character = 14 } }, id);
            var result = (await ReadUntil(m => m.TryGetProperty("id", out var request) && request.GetInt32() == id)).GetProperty("result");
            if (result.ValueKind != JsonValueKind.Object ||
                new Uri(result.GetProperty("uri").GetString()!).LocalPath != expected ||
                result.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() != line)
                throw new InvalidOperationException($"Expected F12 at {expected}:{line + 1}. Received {result}.");
        }

        Task<JsonElement> Diagnostics(string? code) => ReadUntil(message =>
        {
            if (!message.TryGetProperty("method", out var method) || method.GetString() != "textDocument/publishDiagnostics")
                return false;
            var args = message.GetProperty("params");
            if (args.GetProperty("uri").GetString() != uri)
                return false;
            var diagnostics = args.GetProperty("diagnostics");
            lastDiagnostics = diagnostics;
            return code is null ? diagnostics.GetArrayLength() == 0 :
                diagnostics.EnumerateArray().Any(item => item.GetProperty("code").GetString() == code);
        });

        Task Send(string method, object parameters, int? id = null) =>
            SendLsp(input, method, parameters, id, stop);

        async Task<JsonElement> ReadUntil(Func<JsonElement, bool> matches)
        {
            while (true)
            {
                var message = await ReadLsp(output, stop);
                if (matches(message))
                    return message;
            }
        }
    }
    finally
    {
        link.Delete(recursive: true);
    }
}

static async Task SendLsp(Stream input, string method, object parameters, int? id, CancellationToken stop)
{
    var message = new Dictionary<string, object> { ["jsonrpc"] = "2.0", ["method"] = method, ["params"] = parameters };
    if (id is int request)
        message["id"] = request;
    var body = JsonSerializer.SerializeToUtf8Bytes(message);
    await input.WriteAsync(Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n"), stop);
    await input.WriteAsync(body, stop);
    await input.FlushAsync(stop);
}

static async Task<JsonElement> ReadLsp(Stream output, CancellationToken stop)
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

static (string Name, HashSet<string> Values) ReadNoticeStyleDomain(string bindings, string existingValue)
{
    var enums = Regex.Matches(bindings, @"public enum (?<name>\w+) \{(?<body>[^}]*)\}");
    var domains = enums.Select(match => (
        Name: match.Groups["name"].Value,
        Values: Regex.Matches(match.Groups["body"].Value, """ECMAScriptName\("(?<value>[a-z]+)"\)""")
            .Select(literal => literal.Groups["value"].Value).ToHashSet(StringComparer.Ordinal)));
    return domains.Single(domain => domain.Values.Contains(existingValue));
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

sealed class CancelOnEnumeration(CancellationTokenSource cancellation) : IReadOnlyList<string>
{
    public int Count => 0;
    public string this[int index] => throw new ArgumentOutOfRangeException(nameof(index));

    public IEnumerator<string> GetEnumerator()
    {
        cancellation.Cancel();
        return Enumerable.Empty<string>().GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
