#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:package Microsoft.CodeAnalysis.CSharp@5.12.0-1.26529.118
#:project ../../src/Jolt.Compiler/Jolt.Compiler.csproj

using Jazor.NodeHost;
using Jolt.Compiler;
using Jolt.Npm;

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

var directory = Directory.CreateDirectory(Path.Combine(project, $".async-vue-refs-{Guid.NewGuid():N}"));
try
{
    var contract = Directory.CreateDirectory(Path.Combine(directory.FullName, "contract"));
    var package = Directory.CreateDirectory(Path.Combine(contract.FullName, "fixture"));
    File.WriteAllText(Path.Combine(contract.FullName, "package.json"),
        """{"private":true,"dependencies":{"ref-fixture":"file:./fixture","vue":"3.5.42"}}""");
    File.WriteAllText(Path.Combine(package.FullName, "package.json"),
        """{"name":"ref-fixture","version":"1.0.0","type":"module","main":"./index.js","types":"./index.d.ts"}""");
    File.WriteAllText(Path.Combine(package.FullName, "index.js"), "export function createSwitch() {}");
    File.WriteAllText(Path.Combine(package.FullName, "index.d.ts"), """
        import type { Ref } from 'vue';
        export declare function createSwitch<T>(register: Ref<T>): (desired?: T) => T;
        export declare function createSwitch<T = boolean>(initial?: T): [status: Ref<T>, execute: (desired?: T) => T];
        """);
    var renamed = await provider.ReadValueAsync(Path.Combine(contract.FullName, "Probe.jazor"), "ref-fixture", "createSwitch");
    if (renamed.Signatures is not [
        { ReturnType.Kind: "function" },
        { Parameters: [], ReturnType: { Kind: "tupleView", Members: [{ Name: "Item1", Type.Kind: "ref" }, { Name: "Item2", Type.Kind: "function" }] } },
        { Parameters: [{ Name: "initial", Type.Kind: "boolean" }], ReturnType.Kind: "tupleView" }])
        throw new InvalidOperationException("Expected positional tuple bindings from the renamed declaration.");
    File.WriteAllText(Path.Combine(directory.FullName, "App.mjs"), result.Modules.Single().Content);
    var verify = Path.Combine(directory.FullName, "verify.mjs");
    File.WriteAllText(verify, """
        import assert from 'node:assert/strict';
        import { createSSRApp, effectScope, ref } from 'vue';
        import { renderToString } from 'vue/server-renderer';
        import App, { LoadState, ReadNumber, ReadText, SortText, Toggle, ToggleDefault, ToggleSeed } from './App.mjs';
        const scope = effectScope();
        try {
            const [numeric, text, state] = scope.run(() => [ReadNumber(), ReadText(), LoadState()]);
            assert.equal(numeric.value, 10);
            assert.equal(text.value, 'pending');
            assert.equal(state.value, 11);
            await new Promise(resolve => setImmediate(resolve));
            assert.equal(numeric.value, 42);
            assert.equal(text.value, 'done');
            assert.equal(state.value, 23);
            assert.deepEqual(scope.run(SortText).value, ['1', '2', '3']);
            const toggled = ref(false);
            assert.equal(Toggle(toggled), true);
            assert.equal(toggled.value, true);
            assert.equal(Toggle(toggled), false);
            assert.equal(ToggleDefault().value, true);
            assert.equal(ToggleSeed().value, false);
            const states = [];
            await renderToString(createSSRApp(App, { observe: (loading, label) => states.push([loading, label]) }));
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
