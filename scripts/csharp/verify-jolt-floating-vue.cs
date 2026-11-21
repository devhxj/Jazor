#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:package Microsoft.CodeAnalysis.CSharp@5.12.0-1.26529.118
#:project ../../src/Jolt.Compiler/Jolt.Compiler.csproj

using Jazor.NodeHost;
using Jolt.Compiler;
using Jolt.Npm;

var real = Path.GetFullPath("scripts/csharp/fixtures/jolt-binding-projection");
var directory = Directory.CreateDirectory(Path.Combine(real, $".floating-vue-{Guid.NewGuid():N}"));
try
{
    var project = directory.FullName;
    var dom = Path.GetFullPath("scripts/csharp/fixtures/jolt-npm-runtime");
    var contract = Directory.CreateDirectory(Path.Combine(project, "contract")).FullName;
    var fixture = Directory.CreateDirectory(Path.Combine(contract, "fixture")).FullName;
    File.WriteAllText(Path.Combine(contract, "package.json"), """
        {"private":true,"dependencies":{"vue":"3.5.42","ref-fixture":"file:./fixture"}}
        """);
    File.WriteAllText(Path.Combine(fixture, "package.json"), """
        {"name":"ref-fixture","version":"1.0.0","type":"module","main":"./index.js","types":"./index.d.ts"}
        """);
    File.WriteAllText(Path.Combine(fixture, "index.js"), """
        export function peek() { return true; }
        export function replace(ref) { ref.value = 'other'; }
        """);
    File.WriteAllText(Path.Combine(fixture, "index.d.ts"), """
        import type { Ref } from 'vue';
        type Target = HTMLElement | string | null;
        export function peek(source: Readonly<Ref<Target>>): boolean;
        export function replace(source: Ref<Target>): void;
        """);
    var provider = new NpmVueMetadataProvider(Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory")
        ?? Path.GetFullPath(".tmp/nodeapi-store"));
    var document = Path.Combine(real, "Probe.jazor");
    var floating = await provider.ReadValueAsync(document, "@floating-ui/vue", "useFloating");
    var autoUpdate = await provider.ReadValueAsync(document, "@floating-ui/dom", "autoUpdate");
    if (floating.Signatures is not [{ Parameters.Count: 2 }, { Parameters.Count: 3 }])
        throw new InvalidOperationException("Expected both read-only ref overloads.");
    await provider.ListValueExportsAsync(Path.Combine(dom, "Floating.jazor"), "@floating-ui/dom");
    var independent = await provider.ReadValuesAsync(Path.Combine(contract, "Probe.jazor"), "ref-fixture", ["peek", "replace"]);
    if (independent is not [{ ExportName: "peek", Signatures: [{ Parameters: [
        { Type: { Kind: "ref", ReadOnly: true, ElementType.ElementType.Name: "HTMLElement" } }] }] }])
        throw new InvalidOperationException("Expected a DOM arm only on the read-only ref.");

    var source = File.ReadAllText(Path.Combine(real, "FloatingVue.jazor"));
    var middleware = new List<NpmValueExport>();
    foreach (var name in new[] { "arrow", "limitShift", "shift" })
        middleware.Add(await provider.ReadValueAsync(document, "@floating-ui/vue", name));
    var offset = await provider.ReadValueAsync(document, "@floating-ui/dom", "offset");
    var options = new JoltCompileOptions { NpmValues = [floating, autoUpdate, offset, .. middleware] };
    var directSource = source[..source.IndexOf("WhileElementsMounted =", StringComparison.Ordinal)] +
        "WhileElementsMounted = FloatingDom.AutoUpdate,\n" +
        source[source.IndexOf("        });", source.IndexOf("WhileElementsMounted =", StringComparison.Ordinal), StringComparison.Ordinal)..];
    var direct = await JoltProjectCompiler.CompileAsync([new("Direct.jazor", directSource)], options);
    if (direct.Diagnostics.Count != 0)
        throw new InvalidOperationException(string.Join(Environment.NewLine, direct.Diagnostics));
    var wrongMiddleware = source.Replace("FloatingDom.Offset(4),", "Floating.LimitShift(),", StringComparison.Ordinal);
    var rejected = await JoltProjectCompiler.CompileAsync([new("WrongMiddleware.jazor", wrongMiddleware)], options);
    if (!rejected.Diagnostics.Any(diagnostic => diagnostic.Id is "CS0826" or "CS1503"))
        throw new InvalidOperationException("An opaque limiter cannot substitute for middleware.");
    var result = await JoltProjectCompiler.CompileAsync([new("Probe.jazor", source)], options);
    if (result.Diagnostics.Count != 0)
        throw new InvalidOperationException(string.Join(Environment.NewLine, result.Diagnostics));
    File.WriteAllText(Path.Combine(project, "Probe.mjs"), result.Modules.Single().Content);
    var verify = Path.Combine(project, "verify.mjs");
    File.WriteAllText(verify, """
        import assert from 'node:assert/strict';
        const { JSDOM } = await import(globalThis.__jazorArguments[0]);
        const { window } = new JSDOM('<main></main><div style="overflow:auto"><button></button></div><div style="overflow:auto"><button></button></div><aside><i></i></aside>');
        try {
            for (const name of ['window', 'document', 'Node', 'Element', 'HTMLElement', 'SVGElement'])
                globalThis[name] = window[name];
            globalThis.getComputedStyle = window.getComputedStyle.bind(window);
            const { createApp, h, shallowRef, nextTick } = await import('vue');
            const { default: Probe } = await import('./Probe.mjs');
            const [first, second] = document.querySelectorAll('button');
            const listeners = new Map([first, second].map(button => {
                const container = button.parentElement;
                const active = new Set();
                const add = container.addEventListener.bind(container);
                const remove = container.removeEventListener.bind(container);
                container.addEventListener = (type, listener, options) => {
                    if (type === 'scroll') active.add(listener);
                    add(type, listener, options);
                };
                container.removeEventListener = (type, listener, options) => {
                    if (type === 'scroll') assert.equal(active.delete(listener), true);
                    remove(type, listener, options);
                };
                return [button, active];
            }));
            const panel = document.querySelector('aside');
            const arrow = panel.firstElementChild;
            const rect = (x, y) => ({ x, y, left: x, top: y, right: x + 20, bottom: y + 10, width: 20, height: 10 });
            let measured = 0;
            let arrowMeasured = 0;
            Object.defineProperty(arrow, 'offsetWidth', { get: () => { arrowMeasured++; return 4; } });
            Object.defineProperty(arrow, 'offsetHeight', { value: 4 });
            first.getBoundingClientRect = () => { measured++; return rect(20, 10); };
            second.getBoundingClientRect = () => { measured++; return rect(60, 40); };
            panel.getBoundingClientRect = () => rect(0, 0);
            const reference = shallowRef(null);
            const host = document.querySelector('main');
            const app = createApp({ render: () => h(Probe, { Reference: reference.value, Panel: panel, ArrowElement: arrow }) });
            app.mount(host);
            reference.value = first;
            async function expect(text) {
                for (let i = 0; i < 20 && host.textContent !== text; i++) {
                    await new Promise(resolve => setTimeout(resolve, 5));
                    await nextTick();
                }
                assert.equal(host.textContent, text);
            }
            await expect('20/24/true');
            assert.ok(arrowMeasured > 0);
            assert.equal(listeners.get(first).size, 1);
            reference.value = second;
            await expect('60/54/true');
            assert.equal(listeners.get(first).size, 0);
            assert.equal(listeners.get(second).size, 1);
            app.unmount();
            assert.equal(listeners.get(second).size, 0);
            const before = measured;
            reference.value = first;
            await nextTick();
            await new Promise(resolve => setTimeout(resolve, 10));
            assert.equal(measured, before);
            assert.equal(host.childElementCount, 0);
            console.log('Vue Floating UI mounted, repositioned against the new element, and unmounted.');
        } finally {
            window.close();
        }
        """);
    Console.WriteLine(await NodeModuleRunner.RunAsync(project, verify, [
        new Uri(Path.Combine(dom, "node_modules/jsdom/lib/api.js")).AbsoluteUri,
    ]));
}
finally
{
    directory.Delete(recursive: true);
}
