namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialNativePropNormalizationRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_NativeBlocks_NormalizeClassAndStyleOnMountAndUpdate()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/NativeProps.razor"),
            documentText: """
                <section id="direct" class="@CssClass" style="@CssStyle">direct</section>
                <section @attributes="Additional">splat</section>
                """,
            codeBehindSource: """
                using System.Collections.Generic;
                namespace Demo.Pages;
                [ECMAScriptModule("./components/native-props")]
                public partial class NativeProps : ComponentBase, IVueComponent
                {
                    [Parameter] public VueClassValue CssClass { get; set; }
                    [Parameter] public VueStyleValue CssStyle { get; set; }
                    [Parameter] public Dictionary<string, object> Additional { get; set; } = new();
                }
                """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NativeProps");
        await RazorSgOfficialNodeRuntimeTestHost.RunModuleTestAsync(
            "components/native-props.js", observation.ModuleText, "native-props.test.mjs", """
                import assert from 'node:assert/strict';
                import { Window } from 'happy-dom';
                const window = new Window();
                for (const name of ['window', 'document', 'Node', 'Element', 'HTMLElement', 'SVGElement'])
                  globalThis[name] = name === 'window' ? window : window[name];
                const { createApp, h, nextTick, reactive } = await import('vue');
                const { default: component } = await import('./components/native-props.js');
                globalThis.__jazorTest('native block class/style normalization', async () => {
                  const host = document.createElement('main');
                  document.body.append(host);
                  const state = reactive({
                    css: ['ja-error', { ready: true, hidden: false }],
                    style: [{ color: 'red' }, { backgroundColor: 'blue' }],
                    attrs: { id: 'splat', class: ['splat', { active: true }], style: [{ fontSize: '12px' }] }
                  });
                  const app = createApp({ setup: () => () => h(component, {
                    CssClass: state.css, CssStyle: state.style, Additional: state.attrs
                  }) });
                  try {
                    app.mount(host);
                    await nextTick();
                    const direct = host.querySelector('#direct');
                    const splat = host.querySelector('#splat');
                    assert.equal(direct.className, 'ja-error ready');
                    assert.equal(direct.style.color, 'red');
                    assert.equal(direct.style.backgroundColor, 'blue');
                    assert.equal(splat.className, 'splat active');
                    assert.equal(splat.style.fontSize, '12px');
                    state.css = { updated: true };
                    state.style = { color: 'green' };
                    state.attrs = { id: 'splat', class: ['again'], style: { fontSize: '20px' } };
                    await nextTick();
                    assert.equal(direct.className, 'updated');
                    assert.equal(direct.style.color, 'green');
                    assert.equal(direct.style.backgroundColor, '');
                    assert.equal(splat.className, 'again');
                    assert.equal(splat.style.fontSize, '20px');
                  } finally {
                    app.unmount();
                    await window.happyDOM.abort();
                  }
                });
                """,
            vueModuleSpecifier: "vue/dist/vue.runtime.esm-browser.prod.js",
            packageDependencies: new Dictionary<string, string> { ["vue"] = "3.5.42", ["happy-dom"] = "20.10.6" });
    }
}
