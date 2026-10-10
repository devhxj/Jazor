using System.Collections.Immutable;
using Jazor.RazorVue.Generation;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.Sg.Test;

/// <summary>Executes generated route catalogs with Vue's real async loader, renderer and scheduler.</summary>
[TestClass]
public sealed class RazorVueLazyRouteRuntimeTests
{
    private const string RealVueModule = "npm:vue@3.5.42/dist/vue.runtime.esm-browser.prod.js";

    [TestMethod]
    public async Task RouteCatalog_ImportAndNavigation_EvaluateOnlyRequestedPageDependencies()
        => await RunRuntimeTestAsync(
            "lazy-route-navigation.test.mjs",
            """
            Deno.test("catalog import keeps pages cold and navigation loads only the selected page", async () => {
                assert.deepEqual(observation.evaluated, ["Shell"]);
                assert.deepEqual(observation.mounted, []);
                const app = mountRoute("/app/home");
                try {
                    await settle(() => content(app.root) === "Home", "initial home render");
                    assert.deepEqual(observation.evaluated, ["Shell", "Home"]);
                    assert.deepEqual(observation.mounted, ["Home"]);

                    app.navigate("/app/editor");
                    await settle(() => content(app.root) === "Editor:editor dependency", "editor navigation");
                    assert.deepEqual(observation.evaluated, ["Shell", "Home", "HeavyEditor", "Editor"]);
                    assert.deepEqual(observation.mounted, ["Home", "Editor"]);
                    assert.ok(!observation.evaluated.includes("Items"));
                    assert.ok(!observation.evaluated.includes("Slow"));

                    app.navigate("/app/home");
                    await settle(() => content(app.root) === "Home", "return to home");
                    assert.equal(observation.evaluated.filter(name => name === "Home").length, 1);
                    assert.deepEqual(observation.mounted, ["Home", "Editor", "Home"]);
                    assert.deepEqual(app.errors, []);
                } finally {
                    app.dispose();
                    await nextTick();
                }
            });
            """);

    [TestMethod]
    public async Task RouteCatalog_NavigateAwayWhileLoading_DoesNotMountAbandonedPage()
        => await RunRuntimeTestAsync(
            "lazy-route-abandoned-load.test.mjs",
            """
            Deno.test("an unresolved page cannot mount after navigation has selected another page", async () => {
                const slow = routes.find(route => route.template === "/slow");
                const app = mountRoute("/app/slow");
                try {
                    await settle(() => observation.evaluated.includes("Slow"), "slow import started");
                    assert.equal(content(app.root), "");
                    assert.ok(!observation.setups.includes("Slow"));

                    app.navigate("/app/home");
                    await settle(() => content(app.root) === "Home", "leave pending page");
                    releasePage();
                    // Await the actual wrapper's pending loader, then Vue's scheduler. A timer
                    // delay would make the assertion depend on machine speed rather than completion.
                    await slow.component.__asyncLoader();
                    await nextTick();
                    assert.equal(content(app.root), "Home");
                    assert.ok(!observation.setups.includes("Slow"));
                    assert.ok(!observation.mounted.includes("Slow"));

                    app.navigate("/app/slow");
                    await settle(() => content(app.root) === "Slow", "explicit return to resolved page");
                    assert.equal(observation.evaluated.filter(name => name === "Slow").length, 1);
                    assert.equal(observation.mounted.filter(name => name === "Slow").length, 1);
                    assert.deepEqual(app.errors, []);
                } finally {
                    releasePage();
                    app.dispose();
                    await nextTick();
                }
            });
            """,
            gatedPage: "Slow");

    [TestMethod]
    public async Task RouteCatalog_RepeatedRoutesAndParameterNavigation_ShareComponentAndPendingLoader()
        => await RunRuntimeTestAsync(
            "lazy-route-shared-loader.test.mjs",
            """
            Deno.test("route aliases and parameter changes share one wrapper and one module evaluation", async () => {
                const primary = routes.find(route => route.template === "/items/{id:int}");
                const alias = routes.find(route => route.template === "/items-alias/{id:int}");
                assert.strictEqual(primary.component, alias.component);
                assert.strictEqual(primary.component.__asyncLoader, alias.component.__asyncLoader);
                const app = mountRoute("/app/items/1?term=first");
                try {
                    await settle(() => observation.evaluated.includes("Items"), "items import started");
                    app.navigate("/app/items-alias/2?term=second");
                    await nextTick();
                    assert.equal(content(app.root), "");
                    assert.equal(observation.evaluated.filter(name => name === "Items").length, 1);
                    releasePage();
                    await settle(() => content(app.root) === "Items:2:second", "latest props after pending load");
                    assert.equal(observation.setups.filter(name => name === "Items").length, 1);
                    assert.equal(observation.mounted.filter(name => name === "Items").length, 1);

                    app.navigate("/app/items/3?term=third");
                    await settle(() => content(app.root) === "Items:3:third", "parameter-only navigation");
                    assert.equal(observation.evaluated.filter(name => name === "Items").length, 1);
                    assert.equal(observation.setups.filter(name => name === "Items").length, 1);
                    assert.equal(observation.mounted.filter(name => name === "Items").length, 1);
                    assert.deepEqual(observation.parameterTypes, ["number", "number"]);
                    assert.deepEqual(app.errors, []);
                } finally {
                    releasePage();
                    app.dispose();
                    await nextTick();
                }
            });
            """,
            gatedPage: "Items");

    private static async Task RunRuntimeTestAsync(string testFileName, string testSource, string? gatedPage = null)
    {
        var binding = CreateBinding();
        var artifacts = binding.Components.Select(component => CreateArtifact(
            component.ComponentSymbol,
            component.ComponentSymbol.Name == "Shell" ? "Layouts/Shell.mjs" : "Pages/" + component.ComponentSymbol.Name + ".mjs",
            component.ComponentSymbol.Name == "Shell"
                ? ShellModule
                : CreatePageModule(component.ComponentSymbol.Name, component.ComponentSymbol.Name == gatedPage)))
            .ToImmutableArray();
        var catalog = RazorVueRouteCatalogBuilder.Build(binding, artifacts);
        var supportingModules = artifacts.ToDictionary(static artifact => artifact.RelativePath, static artifact => artifact.ModuleText);
        supportingModules.Add("Pages/heavy-editor.mjs", """
            globalThis.__routeObservation.evaluated.push("HeavyEditor");
            export const description = "editor dependency";
            """);

        // Route metadata and import paths come from bound C# symbols and the production builder.
        // Only page business fixtures are handwritten, so observing module evaluation cannot
        // accidentally replace or rewrite the generated lazy-route protocol under test.
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            catalog.RelativePath,
            catalog.ModuleText,
            testFileName,
            RuntimeHarness + Environment.NewLine + testSource,
            supportingModules,
            vueModuleSpecifier: RealVueModule,
            restoreNpmDependencies: true);
    }

    private static string CreatePageModule(string name, bool gated)
        => $$"""
            import { defineComponent, h, onMounted } from "vue";
            {{(name == "Editor" ? "import { description } from \"./heavy-editor.mjs\";" : "")}}
            const observation = globalThis.__routeObservation;
            observation.evaluated.push("{{name}}");
            {{(gated ? "await globalThis.__routePageReady;" : "")}}
            export default defineComponent({
                name: "{{name}}",
                props: ["Id", "Term"],
                setup(props) {
                    observation.setups.push("{{name}}");
                    onMounted(() => observation.mounted.push("{{name}}"));
                    return () => {
                        {{(name == "Items" ? "observation.parameterTypes.push(typeof props.Id);" : "")}}
                        return h("p", {}, {{(name switch
                        {
                            "Items" => "\"Items:\" + props.Id + \":\" + props.Term",
                            "Editor" => "\"Editor:\" + description",
                            _ => "\"" + name + "\""
                        })}});
                    };
                }
            });
            """;

    private const string ShellModule = """
        import { defineComponent, h } from "vue";
        globalThis.__routeObservation.evaluated.push("Shell");
        export default defineComponent({
            name: "Shell",
            setup(_props, { slots }) {
                return () => h("section", {}, slots.default?.());
            }
        });
        """;

    private const string RuntimeHarness = """
        import assert from "node:assert/strict";
        import { createRenderer, h, nextTick } from "vue";

        const observation = globalThis.__routeObservation = {
            evaluated: [], setups: [], mounted: [], parameterTypes: []
        };
        let releasePage;
        globalThis.__routePageReady = new Promise(resolve => releasePage = resolve);
        const { routes } = await import("./runtime/vue/routes.js");
        const { createNavigationHost } = await import("./runtime/vue/blazor-routing.js");

        function makeNode(kind, text = "") {
            return { kind, text, parent: null, children: [], props: {} };
        }
        function remove(node) {
            if (!node.parent) return;
            const siblings = node.parent.children;
            siblings.splice(siblings.indexOf(node), 1);
            node.parent = null;
        }
        // This is Vue's supported custom-renderer host; Vue owns component setup, patching,
        // async wrapper resolution, unmount checks and scheduling throughout the tests.
        const renderer = createRenderer({
            createElement: tag => makeNode(tag),
            createText: text => makeNode("text", text),
            createComment: text => makeNode("comment", text),
            setText(node, text) { node.text = text; },
            setElementText(node, text) {
                for (const child of node.children) child.parent = null;
                node.children = [];
                node.text = text;
            },
            parentNode: node => node.parent,
            nextSibling(node) {
                return node.parent?.children[node.parent.children.indexOf(node) + 1] ?? null;
            },
            patchProp(node, name, _previous, value) { node.props[name] = value; },
            insert(node, parent, anchor = null) {
                remove(node);
                node.parent = parent;
                if (anchor === null) parent.children.push(node);
                else parent.children.splice(parent.children.indexOf(anchor), 0, node);
            },
            remove
        });
        function content(node) {
            return node.kind === "comment" ? "" : node.text + node.children.map(content).join("");
        }
        function installBrowser(path) {
            const setLocation = uri => {
                globalThis.location = new URL(uri, globalThis.location ?? "https://example.test");
                globalThis.window.location = globalThis.location;
            };
            const history = {
                state: null,
                pushState(state, _title, uri) { this.state = state; setLocation(uri); },
                replaceState(state, _title, uri) { this.state = state; setLocation(uri); }
            };
            globalThis.window = { history };
            globalThis.history = history;
            setLocation(path);
            globalThis.document = { querySelector: () => ({ getAttribute: () => "/app/" }) };
            const listeners = new Map();
            globalThis.addEventListener = (name, callback) => {
                if (!listeners.has(name)) listeners.set(name, new Set());
                listeners.get(name).add(callback);
            };
            globalThis.removeEventListener = (name, callback) => listeners.get(name)?.delete(callback);
        }
        function mountRoute(path) {
            installBrowser(path);
            let routing;
            const root = makeNode("root");
            const errors = [];
            const app = renderer.createApp({
                setup() {
                    routing = createNavigationHost();
                    return () => {
                        const route = routing.resolveRoute();
                        if (!route) return null;
                        const page = h(route.component, route.parameters);
                        return route.layout ? h(route.layout, {}, { default: () => page }) : page;
                    };
                }
            });
            app.config.errorHandler = error => errors.push(String(error));
            app.mount(root);
            return { root, errors, navigate: uri => routing.navigation.navigateTo(uri), dispose: () => app.unmount() };
        }
        async function settle(predicate, message) {
            for (let attempt = 0; attempt < 100; attempt++) {
                await nextTick();
                if (predicate()) return;
                await new Promise(resolve => setTimeout(resolve, 0));
            }
            assert.fail(message);
        }
        """;

    private static GeneratedCSharpBinding CreateBinding()
    {
        const string source = """
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            namespace LazyRouteContracts;
            public sealed class Shell : ComponentBase
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder) { }
            }
            [Route("/home"), Layout(typeof(Shell))]
            public sealed class Home : ComponentBase
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder) { }
            }
            [Route("/editor"), Layout(typeof(Shell))]
            public sealed class Editor : ComponentBase
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder) { }
            }
            [Route("/slow"), Layout(typeof(Shell))]
            public sealed class Slow : ComponentBase
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder) { }
            }
            [Route("/items/{id:int}"), Route("/items-alias/{id:int}"), Layout(typeof(Shell))]
            public sealed class Items : ComponentBase
            {
                [Parameter] public int Id { get; set; }
                [Parameter, SupplyParameterFromQuery(Name = "term")] public string? Term { get; set; }
                protected override void BuildRenderTree(RenderTreeBuilder builder) { }
            }
            """;
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), "Pages/LazyRoutes.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.LazyRoute.Contracts",
            [tree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        var document = new GeneratedDocument(
            "LazyRoutes.razor.g.cs", "Pages/LazyRoutes.razor", SourceText.From(source), ImmutableArray<RazorSourceMap>.Empty);
        var model = compilation.GetSemanticModel(tree);
        var components = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Select(declaration =>
        {
            var symbol = model.GetDeclaredSymbol(declaration)!;
            var methodDeclaration = declaration.Members.OfType<MethodDeclarationSyntax>().Single();
            var method = model.GetDeclaredSymbol(methodDeclaration)!;
            var body = model.GetOperation(methodDeclaration.Body!) as IBlockOperation;
            Assert.IsNotNull(body);
            return new BoundComponent(document, symbol, method, body!);
        }).ToImmutableArray();
        return new GeneratedCSharpBinding(compilation, ImmutableArray.Create(document), components);
    }

    private static VueModuleArtifact CreateArtifact(INamedTypeSymbol symbol, string path, string moduleText)
        => new(
            symbol.ToDisplayString(), path, moduleText, "content:" + symbol.Name, path + ".map", "{}", "map:" + symbol.Name,
            ImmutableArray<string>.Empty, ImmutableArray<VueAsset>.Empty,
            new VueHmrMetadata("test:" + symbol.Name, "descriptor", "template", "logic", VueHmrBoundaryKind.LogicSafe));
}
