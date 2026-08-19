namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialNestedRuntimeClassClosureRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorReactiveNestedRuntimeClass_UsesProxySafePrivateStorage()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReactiveNestedRuntimeClass.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <section>
                <article>@Formatter.Format(Title) @Revision</article>
                <button type="button" @onclick="Promote">Promote</button>
            </section>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/reactive-nested-runtime-class")]
            public partial class ReactiveNestedRuntimeClass : ComponentBase, IVueComponent
            {
                [Parameter]
                public string Title { get; set; } = string.Empty;

                private ReleaseFormatter Formatter { get; set; } = new("release");

                private int Revision { get; set; }

                private void Promote()
                {
                    Formatter.Prefix = "promoted";
                    Revision++;
                }

                private sealed class ReleaseFormatter
                {
                    public ReleaseFormatter(string prefix)
                    {
                        Prefix = prefix;
                    }

                    public string Prefix { get; set; }

                    public string Format(string title)
                    {
                        return Prefix + ": " + title;
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReactiveNestedRuntimeClass");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "$jazor$private$", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("this.#", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/reactive-nested-runtime-class.mjs",
            observation.ModuleText,
            "official-reactive-nested-runtime-class.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/reactive-nested-runtime-class.mjs";
            import { reactiveWrites } from "vue";

            function findNode(node, name) {
                if (node == null) return null;
                if (Array.isArray(node)) {
                    for (const child of node) {
                        const found = findNode(child, name);
                        if (found) return found;
                    }
                    return null;
                }
                if (node.name === name) return node;
                return findNode(node.children, name);
            }

            function text(node) {
                if (node == null) return "";
                if (typeof node === "string") return node;
                if (typeof node === "number") return String(node);
                if (Array.isArray(node)) return node.map(text).join("");
                return text(node.children);
            }

            test("Vue deep Proxy can invoke nested runtime-class auto-properties without private-brand failures", () => {
                const render = component.setup({ Title: "Deploy API" }, { slots: {} });
                assert.match(text(render()), /release: Deploy API\s*0/);

                const button = findNode(render(), "button");
                assert.ok(button);
                button.props.onClick();

                assert.match(text(render()), /promoted: Deploy API\s*1/);
                assert.ok(reactiveWrites.length >= 2);
            });
            """,
            vueRuntimeSource:
            """
            export function defineComponent(options) {
                return options;
            }

            export const Fragment = Symbol("Fragment");
            export const reactiveWrites = [];

            export function reactive(value) {
                const proxies = new WeakMap();
                const wrap = (candidate) => {
                    if (candidate === null || typeof candidate !== "object") return candidate;
                    const existing = proxies.get(candidate);
                    if (existing) return existing;
                    const proxy = new Proxy(candidate, {
                        get(target, key, receiver) {
                            return wrap(Reflect.get(target, key, receiver));
                        },
                        set(target, key, next, receiver) {
                            reactiveWrites.push(key);
                            return Reflect.set(target, key, next, receiver);
                        }
                    });
                    proxies.set(candidate, proxy);
                    return proxy;
                };
                return wrap(value);
            }

            export function h(name, props, children) {
                return { name, props, children };
            }

            export function createStaticVNode(html, count) {
                return { name: "__static", props: { html, count }, children: html };
            }

            export function createTextVNode(children, patchFlag) {
                return { name: "__text", children, patchFlag };
            }
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorNestedRuntimeClass_ProjectsFieldPropertyAndHelperInvocationOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/NestedRuntimeClassClosureRuntime.razor"),
            documentText:
            """
            <article data-release="@formatter.Key">@formatter.Format(Title)</article>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/nested-runtime-class-closure-runtime")]
            public partial class NestedRuntimeClassClosureRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public string Title { get; set; } = string.Empty;

                private readonly ReleaseFormatter formatter = new("release");

                private sealed class ReleaseFormatter
                {
                    private readonly string prefix;

                    public ReleaseFormatter(string prefix)
                    {
                        this.prefix = prefix;
                    }

                    public string Key => prefix + "-key";

                    public string Format(string title)
                    {
                        return Combine(title);
                    }

                    private string Combine(string title)
                    {
                        return prefix + ": " + title;
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NestedRuntimeClassClosureRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "formatter.Key", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "formatter.Format(Title)", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "class ReleaseFormatter", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "new ReleaseFormatter(\"release\")", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "Combine(title)", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/nested-runtime-class-closure-runtime.mjs",
            observation.ModuleText,
            "official-nested-runtime-class-closure-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/nested-runtime-class-closure-runtime.mjs";

            test("official Razor nested runtime classes retain reachable field, property, and helper semantics", () => {
                const render = component.setup({ Title: "Deploy API" }, { slots: {} });
                const article = render();

                assert.equal(article.name, "article");
                assert.equal(article.props["data-release"], "release-key");
                assert.equal(article.children, "release: Deploy API");
            });
            """);
    }
}
