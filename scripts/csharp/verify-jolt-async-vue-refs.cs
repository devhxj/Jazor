#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:package Microsoft.CodeAnalysis.CSharp@5.12.0-1.26529.118
#:project ../../src/Jolt.Compiler/Jolt.Compiler.csproj

using Jazor.NodeHost;
using Jolt.Compiler;
using Jazor.Npm;
using Jolt.Npm;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

var project = Path.GetFullPath(Path.Combine("scripts", "csharp", "fixtures", "jolt-binding-projection"));
var provider = new NpmVueMetadataProvider(Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory")
    ?? Path.GetFullPath(".tmp/nodeapi-store"));
var values = await provider.ReadValuesAsync(Path.Combine(project, "Probe.jazor"), "@vueuse/core",
    ["computedAsync", "useAsyncState", "useSorted", "useToggle"]);
var source = File.ReadAllText(Path.Combine(project, "AsyncVueRefs.jazor"));
var options = new JoltCompileOptions { NpmValues = values };
var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
if (result.Diagnostics.Count != 0)
    throw new InvalidOperationException(string.Join(Environment.NewLine, result.Diagnostics));
var invalid = source.Replace("Resolve(42), 10", "Resolve(42), false", StringComparison.Ordinal);
var rejected = await JoltProjectCompiler.CompileAsync([new("App.jazor", invalid)], options);
if (!rejected.Diagnostics.Any(diagnostic => diagnostic.Id == "CS0411"))
    throw new InvalidOperationException(string.Join(Environment.NewLine, rejected.Diagnostics));

var bindings = NpmBindings.GenerateSource([], values);
var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
    .Split(Path.PathSeparator).Select(path => MetadataReference.CreateFromFile(path));
var consumer = """
    #nullable enable
    using ECMAScript;
    using VueUse = Npm.Vueuse.Core.Exports;
    public static class Consumer
    {
        public static Vue.VueReadonlyOptionalRef<UnknownValue?> Load(IPromise<double> pending) =>
            VueUse.UseAsyncState(pending, 11).Error;
        public static string? ReadErrorMessage(UnknownValue? value) => value?.AsError?.Message;
    }
    """;
var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
var library = CSharpCompilation.Create("VueUseConsumer",
    [CSharpSyntaxTree.ParseText(bindings, parseOptions), CSharpSyntaxTree.ParseText(consumer, parseOptions)],
    references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
using var image = new MemoryStream();
var emitted = library.Emit(image);
if (!emitted.Success)
    throw new InvalidOperationException(string.Join(Environment.NewLine, emitted.Diagnostics));
var invalidConsumer = consumer.Replace("value?.AsError?.Message", "value?.Message", StringComparison.Ordinal) +
    """

    public static class InvalidConsumer
    {
        public static void Write(IPromise<double> pending) => Consumer.Load(pending).Value = null;
    }
    """;
var errors = library.ReplaceSyntaxTree(library.SyntaxTrees.Last(),
    CSharpSyntaxTree.ParseText(invalidConsumer, parseOptions)).GetDiagnostics();
if (!errors.Any(diagnostic => diagnostic.Id == "CS1061") || !errors.Any(diagnostic => diagnostic.Id == "CS0200"))
    throw new InvalidOperationException(string.Join(Environment.NewLine, errors));

var directory = Directory.CreateDirectory(Path.Combine(project, $".async-vue-refs-{Guid.NewGuid():N}"));
try
{
    var contract = Directory.CreateDirectory(Path.Combine(directory.FullName, "contract"));
    var package = Directory.CreateDirectory(Path.Combine(contract.FullName, "fixture"));
    File.WriteAllText(Path.Combine(contract.FullName, "package.json"),
        """{"private":true,"dependencies":{"ref-fixture":"file:./fixture","vue":"3.5.42"}}""");
    File.WriteAllText(Path.Combine(package.FullName, "package.json"),
        """{"name":"ref-fixture","version":"1.0.0","type":"module","main":"./index.js","types":"./index.d.ts"}""");
    File.WriteAllText(Path.Combine(package.FullName, "index.js"), """
        import { isReactive, ref, shallowReactive, shallowRef, toRaw } from 'vue';
        export function createSwitch() {}
        export function publish() {}
        export function publishDeep() {}
        export function publishConditional() {}
        export function createConcrete() {
            const entry = () => ({ enabled: ref(true), nested: { count: ref(1) },
                items: [ref(1)], immutable: 7 });
            const deepSource = entry();
            const shallowSource = entry();
            const reactiveSource = entry();
            const deep = ref(deepSource);
            const shallow = shallowRef(shallowSource);
            const reactive = shallowReactive(reactiveSource);
            const deepProxy = deep.value;
            const leaves = [deepSource, shallowSource, reactiveSource]
                .map(source => [source.enabled, source.nested.count, source.items[0]]);
            const check = () => {
                if (deep.value !== deepProxy || deep.value.nested === deepSource.nested ||
                    shallow.value !== shallowSource || reactive.nested !== reactiveSource.nested)
                    throw new Error('Vue wrapper identity changed');
                for (const [i, source] of [deepSource, shallowSource, reactiveSource].entries()) {
                    const [enabled, count, item] = leaves[i];
                    if (source.enabled !== enabled || source.nested.count !== count || source.items[0] !== item ||
                        enabled.value !== false || count.value !== 0 || item.value !== 0)
                        throw new Error('Expected the original leaf refs with false and zero');
                }
            };
            const replacement = { enabled: ref(true), nested: { count: ref(2) },
                items: [ref(3)], immutable: 8 };
            const replacementRefs = [replacement.enabled, replacement.nested.count, replacement.items[0]];
            const valueAccessor = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(deep), 'value');
            let written;
            Object.defineProperty(deep, 'value', {
                get: () => valueAccessor.get.call(deep),
                set: value => {
                    written = value;
                    valueAccessor.set.call(deep, value);
                },
            });
            const checkReplacement = () => {
                if (!isReactive(deep.value) || deep.value === deepProxy || written === deep.value ||
                    toRaw(deep.value) !== written || written === replacement || written.immutable !== 8)
                    throw new Error('Expected a new proxy over the supplied raw object');
                const [enabled, count, item] = replacementRefs;
                if (written.enabled !== enabled || written.nested.count !== count ||
                    written.items[0] !== item || enabled.value !== false || count.value !== 4 || item.value !== 3)
                    throw new Error('Expected the replacement refs to retain identity and receive proxy writes');
                const [oldEnabled, oldCount, oldItem] = leaves[0];
                if (toRaw(deepProxy) !== deepSource || deepSource.enabled !== oldEnabled ||
                    deepSource.nested.count !== oldCount || deepSource.items[0] !== oldItem ||
                    oldEnabled.value !== false || oldCount.value !== 0 || oldItem.value !== 0)
                    throw new Error('Expected the previous object and refs to retain their values');
            };
            return { deep, shallow, reactive, check, replacement, checkReplacement };
        }
        """);
    File.WriteAllText(Path.Combine(package.FullName, "index.d.ts"), """
        import type { Ref, ShallowReactive, ShallowRef, UnwrapRef } from 'vue';
        export declare function createSwitch<T>(register: Ref<T>): (desired?: T) => T;
        export declare function createSwitch<T = boolean>(initial?: T): [status: Ref<T>, execute: (desired?: T) => T];
        interface Concrete {
            enabled: Ref<boolean>;
            nested: { count: Ref<number> };
            items: Ref<number>[];
            readonly immutable: number;
        }
        export declare function createConcrete(): {
            deep: Ref<UnwrapRef<Concrete>, Concrete | UnwrapRef<Concrete>>;
            shallow: ShallowRef<Concrete>;
            reactive: ShallowReactive<Concrete>;
            check: () => void;
            replacement: Concrete;
            checkReplacement: () => void;
        };
        interface PublishOptions<S extends boolean> { preserve?: S; start?: boolean }
        export declare function publish<T, S extends boolean = false>(
            load: Promise<T> | (() => Promise<T>), seed: T, options?: PublishOptions<S>
        ): { current: Ref<S extends true ? T : UnwrapRef<T>> };
        export declare function publishDeep<T>(
            load: Promise<T> | (() => Promise<T>), seed: T
        ): { current: Ref<UnwrapRef<T>> };
        export declare function publishConditional<T>(
            load: Promise<T> | (() => Promise<T>), seed: T
        ): { current: Ref<T extends { balance: number } ? string : T> };
        """);
    var renamed = await provider.ReadValueAsync(Path.Combine(contract.FullName, "Probe.jazor"), "ref-fixture", "createSwitch");
    if (renamed.Signatures is not [
        { ReturnType.Kind: "function" },
        { Parameters: [], ReturnType: { Kind: "tupleView", Members: [{ Name: "Item1", Type.Kind: "ref" }, { Name: "Item2", Type.Kind: "function" }] } },
        { Parameters: [{ Name: "initial", Type.Kind: "boolean" }], ReturnType.Kind: "tupleView" }])
        throw new InvalidOperationException("Expected positional tuple bindings from the renamed declaration.");
    var concrete = await provider.ReadValueAsync(Path.Combine(contract.FullName, "Probe.jazor"), "ref-fixture", "createConcrete");
    var concreteSource = File.ReadAllText(Path.Combine(project, "ConcreteVueRefs.jazor"));
    var concreteOptions = new JoltCompileOptions { NpmValues = [concrete] };
    var concreteResult = await JoltProjectCompiler.CompileAsync([new("Concrete.jazor", concreteSource)], concreteOptions);
    if (concreteResult.Diagnostics.Count != 0)
        throw new InvalidOperationException(string.Join(Environment.NewLine, concreteResult.Diagnostics));
    var invalidConcrete = concreteSource.Replace("fixture.Check();",
        "deep.Value.Immutable = 1; shallow.Value.Immutable = 1; reactive.Immutable = 1;", StringComparison.Ordinal);
    var readonlyErrors = (await JoltProjectCompiler.CompileAsync([new("Concrete.jazor", invalidConcrete)], concreteOptions)).Diagnostics;
    if (readonlyErrors.Count(diagnostic => diagnostic.Id == "CS0200") != 3)
        throw new InvalidOperationException(string.Join(Environment.NewLine, readonlyErrors));
    var invalidWrite = concreteSource.Replace("Enabled = replacement.Enabled,", "Enabled = false,", StringComparison.Ordinal);
    var writeErrors = (await JoltProjectCompiler.CompileAsync([new("Concrete.jazor", invalidWrite)], concreteOptions)).Diagnostics;
    if (!writeErrors.Any(diagnostic => diagnostic.Id == "CS0029"))
        throw new InvalidOperationException(string.Join(Environment.NewLine, writeErrors));
    File.WriteAllText(Path.Combine(contract.FullName, "Concrete.mjs"), concreteResult.Modules.Single().Content);
    var concreteVerify = Path.Combine(contract.FullName, "verify-concrete.mjs");
    File.WriteAllText(concreteVerify, """
        import assert from 'node:assert/strict';
        import { WriteConcrete } from './Concrete.mjs';
        assert.equal(WriteConcrete(), true);
        """);
    await NodeModuleRunner.RunAsync(contract.FullName, concreteVerify);
    var makers = await provider.ReadValuesAsync(Path.Combine(contract.FullName, "Probe.jazor"),
        "ref-fixture", ["publish", "publishDeep", "publishConditional"]);
    var correlated = makers.Single(value => value.ExportName == "publish").Signatures
        .Where(signature => signature.ScalarResultTypeParameter == "T").ToArray();
    if (correlated is not [{ ManagedNameSuffix: "WithPreserveTrue", Parameters: [_, _, { Type.Members: { } config }] }] ||
        !config.Any(member => member is { Name: "preserve", FixedBoolean: true }) ||
        makers.Any(value => value.ExportName is "publishDeep" or "publishConditional" &&
            value.Signatures.Any(signature => signature.ScalarResultTypeParameter == "T")))
        throw new InvalidOperationException("Expected typed state only when the declared mode preserves nested refs.");
    File.WriteAllText(Path.Combine(directory.FullName, "App.mjs"), result.Modules.Single().Content);
    var verify = Path.Combine(directory.FullName, "verify.mjs");
    File.WriteAllText(verify, """
        import assert from 'node:assert/strict';
        import { createSSRApp, effectScope, ref } from 'vue';
        import { renderToString } from 'vue/server-renderer';
        import App, {
            LoadError, LoadState, ReadError, ReadNumber, ReadRejectedBoolean,
            ReadRejectedNumber, ReadRejectedText, ReadText, SortText, Toggle, ToggleDefault, ToggleSeed,
        } from './App.mjs';
        const scope = effectScope();
        try {
            const [numeric, text, state] = scope.run(() => [ReadNumber(), ReadText(), LoadState()]);
            assert.equal(numeric.value, 10);
            assert.equal(text.value, 'pending');
            assert.equal(state.value, 11);
            const nested = ref(8);
            await new Promise(resolve => setImmediate(resolve));
            assert.equal(numeric.value, 42);
            assert.equal(text.value, 'done');
            assert.equal(state.value, 23);
            const originalError = new Error('network failure');
            const [error, rejectedText, rejectedBoolean, rejectedNumber] = scope.run(() =>
                [originalError, 'offline', false, 0].map(reason => LoadError(Promise.reject(reason))));
            assert.equal(error.value, undefined);
            await new Promise(resolve => setImmediate(resolve));
            assert.strictEqual(ReadError(error.value), originalError);
            assert.equal(ReadRejectedText(rejectedText.value), 'offline');
            assert.equal(ReadRejectedBoolean(rejectedBoolean.value), false);
            assert.equal(ReadRejectedNumber(rejectedNumber.value), 0);
            assert.equal(ReadError(rejectedText.value), null);
            assert.deepEqual(scope.run(SortText).value, ['1', '2', '3']);
            const toggled = ref(false);
            assert.equal(Toggle(toggled), true);
            assert.equal(toggled.value, true);
            assert.equal(Toggle(toggled), false);
            assert.equal(ToggleDefault().value, true);
            assert.equal(ToggleSeed().value, false);
            const states = [];
            const observed = [];
            await renderToString(createSSRApp(App, {
                nested, observeNested: value => observed.push(value), observe: (loading, label) => states.push([loading, label]),
            }));
            assert.strictEqual(observed.length, 1);
            assert.strictEqual(observed[0], nested);
            await new Promise(resolve => setImmediate(resolve));
            assert.deepEqual(states, [[false, 'initial'], [true, 'initial'], [false, 'ready']]);
        } finally {
            scope.stop();
        }
        console.log('VueUse computed refs, async state, sorting and toggling passed.');
        """);
    Console.WriteLine(await NodeModuleRunner.RunAsync(project, verify));
}
finally
{
    directory.Delete(recursive: true);
}
