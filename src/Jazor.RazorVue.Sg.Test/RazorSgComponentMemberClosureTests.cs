using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using ECMAScript;
using Jazor.Compiler;
using Jazor.RazorVue.RazorSdk;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgComponentMemberClosureTests
{
    [TestMethod]
    public async Task Build_OfficialRazorCounter_CompilesStateThroughCompilerClosureAndHost()
    {
        var fixture = CreateOfficialCounterFixture();

        var built = RazorSgComponentMemberClosureBuilder.TryBuild(
            fixture.Binding,
            fixture.Component,
            out var closure,
            out var failure);

        Assert.IsTrue(built, failure);
        Assert.IsNotNull(closure);
        Assert.AreEqual("Counter", closure!.ComponentSymbol.Name);
        Assert.AreEqual("BuildRenderTree", closure.BuildRenderTreeMethod.Name);
        AssertHasField(closure, "count");
        Assert.IsFalse(
            closure.ReachableMethods.Any(static method => method.ContainingType.Name == "ComponentBase"),
            "Current-component closure must not copy ComponentBase methods.");

        var syntaxTree = fixture.Component.BuildRenderTreeMethod.DeclaringSyntaxReferences.Single().SyntaxTree;
        var semanticModel = fixture.Binding.Compilation.GetSemanticModel(syntaxTree);
        var converter = new AstConverter(
            fixture.Component.ComponentSymbol,
            semanticModel,
            closure.CreateAstConverterOptions());
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, ".addContent(state.count);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("this.count", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void Build_CurrentComponentClosure_IsDeterministicAndExcludesUnusedMembers()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    private int count = Seed();

                    private string label => Format(count);

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "button");
                        builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, Increment));
                        builder.AddContent(2, label);
                        builder.CloseElement();
                    }

                    private void Increment()
                    {
                        count = Normalize(count + 1);
                    }

                    private int Normalize(int value) => value;

                    private static int Seed() => 0;

                    private string Format(int value) => value.ToString();

                    private void Unused()
                    {
                        count = -1;
                    }
                }
            }
            """);

        var first = BuildClosure(fixture);
        var second = BuildClosure(fixture);

        CollectionAssert.AreEqual(
            first.OrderedMembers.Select(static symbol => symbol.ToDisplayString()).ToArray(),
            second.OrderedMembers.Select(static symbol => symbol.ToDisplayString()).ToArray());
        AssertHasField(first, "count");
        AssertHasProperty(first, "label");
        AssertHasMethod(first, "Increment");
        AssertHasMethod(first, "Normalize");
        AssertHasMethod(first, "Seed");
        AssertHasMethod(first, "Format");
        AssertNoMember(first, "Unused");
        Assert.IsInstanceOfType(first.CompilerClosure, typeof(CurrentComponentMemberClosure));
        Assert.IsTrue(first.CreateMemberFilter()(first.BuildRenderTreeMethod));
        Assert.IsFalse(first.CreateMemberFilter()(fixture.Component.ComponentSymbol.BaseType!.GetMembers("StateHasChanged").Single()));
    }

    [TestMethod]
    public async Task Build_CreateAstConverterOptions_UsesCompilerClosureAndCurrentComponentHost()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string Title { get; set; } = "";

                    private int count = 1;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "button");
                        builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, Increment));
                        builder.AddContent(2, Title);
                        builder.AddContent(3, count);
                        builder.CloseElement();
                    }

                    private void Increment()
                    {
                        count++;
                    }

                    private void Unused()
                    {
                        count = -1;
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var syntaxTree = fixture.Component.BuildRenderTreeMethod.DeclaringSyntaxReferences.Single().SyntaxTree;
        var semanticModel = fixture.Binding.Compilation.GetSemanticModel(syntaxTree);
        var options = closure.CreateAstConverterOptions();

        Assert.AreEqual(AstConverterProfile.RazorVueRuntime, options.Profile);
        Assert.IsNotNull(options.MemberFilter);
        Assert.IsInstanceOfType(options.Host, typeof(CurrentComponentSemanticWalkerHost));

        var converter = new AstConverter(fixture.Component.ComponentSymbol, semanticModel, options);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "function buildRenderTree(builder)", StringComparison.Ordinal);
        StringAssert.Contains(script!, "builder.addAttribute(\"onclick\", increment);", StringComparison.Ordinal);
        StringAssert.Contains(script!, "builder.addContent(props.title);", StringComparison.Ordinal);
        StringAssert.Contains(script!, "builder.addContent(state.count);", StringComparison.Ordinal);
        StringAssert.Contains(script!, "function increment()", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("function unused()", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("this.", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains(".bind(", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_EmitsSetupScopedRenderFunction()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string Title { get; set; } = "";

                    private int count = Seed();

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "button");
                        builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, Increment));
                        builder.AddContent(2, Title);
                        builder.AddContent(3, count);
                        builder.CloseElement();
                    }

                    private void Increment()
                    {
                        count++;
                    }

                    private static int Seed() => 1;
                }
            }
            """);
        var closure = BuildClosure(fixture);

        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var rebuilt = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        Assert.AreEqual("Demo.Pages.Counter", artifact.ComponentId);
        Assert.AreEqual("components/counter.mjs", artifact.RelativePath);
        Assert.AreEqual("components/counter.mjs.map", artifact.SourceMapRelativePath);
        Assert.AreEqual(artifact.ModuleText, rebuilt.ModuleText);
        Assert.AreEqual(artifact.ContentHash, rebuilt.ContentHash);
        Assert.AreEqual(artifact.SourceMapContent, rebuilt.SourceMapContent);
        Assert.AreEqual(artifact.MapHash, rebuilt.MapHash);
        Assert.IsTrue(IsSha256Hash(artifact.ContentHash), artifact.ContentHash);
        Assert.IsTrue(IsSha256Hash(artifact.MapHash), artifact.MapHash);
        Assert.IsFalse(artifact.ModuleText.Contains("\r", StringComparison.Ordinal), artifact.ModuleText);
        Assert.IsFalse(artifact.SourceMapContent.Contains("\r", StringComparison.Ordinal), artifact.SourceMapContent);
        StringAssert.Contains(artifact.SourceMapContent, "\"file\": \"components/counter.mjs\"", StringComparison.Ordinal);
        StringAssert.Contains(artifact.SourceMapContent, "\"sources\": [", StringComparison.Ordinal);
        StringAssert.Contains(script, "import { defineComponent, h, reactive } from \"vue\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "function createCounterSetupScope(props) {", StringComparison.Ordinal);
        StringAssert.Contains(script, "const state = reactive({", StringComparison.Ordinal);
        StringAssert.Contains(script, "count: seed()", StringComparison.Ordinal);
        StringAssert.Contains(script, "function buildRenderTree(builder)", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.addAttribute(\"onclick\", increment);", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.addContent(props.title);", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.addContent(state.count);", StringComparison.Ordinal);
        StringAssert.Contains(script, "return { buildRenderTree };", StringComparison.Ordinal);
        StringAssert.Contains(script, "props: [", StringComparison.Ordinal);
        StringAssert.Contains(script, "\"title\"", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("let invalidate = null;", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("let pendingInvalidations = 0;", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("const stateHasChanged = () => {", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("if (invalidate === null) {", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("pendingInvalidations++;", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("invalidate.tick++;", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "const scope = createCounterSetupScope(props);", StringComparison.Ordinal);
        StringAssert.Contains(script, "setup(props) {", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("{ slots }", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("const invokeAsync = ", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("invalidate = reactive({ tick: pendingInvalidations });", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("invalidate.tick;", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "const builder = createRenderContext(h);", StringComparison.Ordinal);
        StringAssert.Contains(script, "scope.buildRenderTree(builder);", StringComparison.Ordinal);
        StringAssert.Contains(script, "return builder.finish();", StringComparison.Ordinal);
        StringAssert.Contains(script, "export default defineComponent({", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("watch(", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("onMounted(", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("onUpdated(", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("onUnmounted(", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("cachedVNode", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("let count", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("this.", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains(".bind(", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_OmitsPropsWhenComponentDoesNotUseProps()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/plain-text")]
                public partial class PlainText : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, "Hello");
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);

        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "import { defineComponent, h } from \"vue\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "function createPlainTextSetupScope() {", StringComparison.Ordinal);
        StringAssert.Contains(script, "setup() {", StringComparison.Ordinal);
        StringAssert.Contains(script, "const scope = createPlainTextSetupScope();", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("setup(props", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("props.", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("{ slots }", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("reactive", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_EmitsOnParametersSetWatchHook()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string Title { get; set; } = "";

                    private int count;

                    protected override void OnParametersSet()
                    {
                        count = Title.Length;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "div");
                        builder.AddContent(1, Title);
                        builder.AddContent(2, count);
                        builder.CloseElement();
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);

        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);

        var script = artifact.ModuleText.ReplaceLineEndings("\n");
        StringAssert.Contains(script, "import { defineComponent, h, reactive, watch } from \"vue\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "scope.onParametersSet();", StringComparison.Ordinal);
        StringAssert.Contains(script, "watch(", StringComparison.Ordinal);
        StringAssert.Contains(script, "() => props,", StringComparison.Ordinal);
        StringAssert.Contains(script, "{ deep: true }", StringComparison.Ordinal);
        StringAssert.Contains(script, "function onParametersSet()", StringComparison.Ordinal);
        StringAssert.Contains(script, "state.count = props.title.length;", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_EmitsOnAfterRenderHooks()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    private int afterRenderCount;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "div");
                        builder.AddContent(1, afterRenderCount);
                        builder.CloseElement();
                    }

                    protected override void OnAfterRender(bool firstRender)
                    {
                        afterRenderCount = firstRender ? 1 : afterRenderCount + 1;
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "import { defineComponent, h, onMounted, onUpdated, reactive } from \"vue\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "onMounted(() => {", StringComparison.Ordinal);
        StringAssert.Contains(script, "scope.onAfterRender(true);", StringComparison.Ordinal);
        StringAssert.Contains(script, "onUpdated(() => {", StringComparison.Ordinal);
        StringAssert.Contains(script, "scope.onAfterRender(false);", StringComparison.Ordinal);
        StringAssert.Contains(script, "function onAfterRender(firstRender)", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_EmitsDisposeOnUnmountedHook()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using System;
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent, IDisposable
                {
                    private int disposeCount;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "div");
                        builder.AddContent(1, disposeCount);
                        builder.CloseElement();
                    }

                    public void Dispose()
                    {
                        disposeCount++;
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "import { defineComponent, h, onUnmounted, reactive } from \"vue\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "onUnmounted(() => {", StringComparison.Ordinal);
        StringAssert.Contains(script, "scope.dispose();", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("scope.disposeAsync();", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "function dispose()", StringComparison.Ordinal);
        StringAssert.Contains(script, "state.disposeCount++;", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_EmitsShouldRenderGate()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    private int count;

                    protected override bool ShouldRender()
                    {
                        return count % 2 == 0;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "div");
                        builder.AddContent(1, count);
                        builder.CloseElement();
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "let hasRendered = false;", StringComparison.Ordinal);
        StringAssert.Contains(script, "let cachedVNode = null;", StringComparison.Ordinal);
        StringAssert.Contains(script, "if (hasRendered && !scope.shouldRender()) {", StringComparison.Ordinal);
        StringAssert.Contains(script, "return cachedVNode;", StringComparison.Ordinal);
        StringAssert.Contains(script, "function shouldRender()", StringComparison.Ordinal);
        StringAssert.Contains(script, "return state.count % 2 === 0;", StringComparison.Ordinal);
        StringAssert.Contains(script, "cachedVNode = builder.finish();", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_EmitsOnInitializedAsyncHook()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using System.Threading.Tasks;
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    private int count;

                    protected override Task OnInitializedAsync()
                    {
                        count = 1;
                        return Task.CompletedTask;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "div");
                        builder.AddContent(1, count);
                        builder.CloseElement();
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "Promise.resolve(scope.onInitializedAsync()).then(", StringComparison.Ordinal);
        StringAssert.Contains(script, "stateHasChanged();", StringComparison.Ordinal);
        StringAssert.Contains(script, "function onInitializedAsync()", StringComparison.Ordinal);
        StringAssert.Contains(script, "state.count = 1;", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_EmitsInvokeAsyncDispatcherAndRuntimeDispatchesSynchronously()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    private int count;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "button");
                        builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, Refresh));
                        builder.AddContent(2, count);
                        builder.CloseElement();
                    }

                    private void Refresh()
                    {
                        _ = InvokeAsync(Increment);
                    }

                    private void Increment()
                    {
                        count++;
                        StateHasChanged();
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "function createCounterSetupScope(stateHasChanged, invokeAsync) {", StringComparison.Ordinal);
        StringAssert.Contains(script, "const invokeAsync = (workItem) => {", StringComparison.Ordinal);
        StringAssert.Contains(script, "return Promise.resolve(workItem());", StringComparison.Ordinal);
        StringAssert.Contains(script, "return Promise.reject(error);", StringComparison.Ordinal);
        StringAssert.Contains(script, "setup() {", StringComparison.Ordinal);
        StringAssert.Contains(script, "const scope = createCounterSetupScope(stateHasChanged, invokeAsync);", StringComparison.Ordinal);
        StringAssert.Contains(script, "invokeAsync(increment);", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("InvokeAsync", StringComparison.Ordinal), script);

        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "Jazor.RazorVue.Sg.Test",
            Guid.NewGuid().ToString("N"));
        try
        {
            WriteFile(Path.Combine(tempRoot, artifact.RelativePath), artifact.ModuleText);
            WriteFile(
                Path.Combine(tempRoot, "package.json"),
                """{"type":"module"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "vue", "package.json"),
                """{"type":"module","exports":"./index.mjs"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "vue", "index.mjs"),
                """
                export const Fragment = Symbol("Fragment");
                export function createStaticVNode(html, count) {
                    return { html, count };
                }
                export function defineComponent(options) {
                    return options;
                }
                export function reactive(value) {
                    return value;
                }
                export function watch() {
                    return () => {};
                }
                export function onMounted() {}
                export function onUpdated() {}
                export function onUnmounted() {}
                export function h(name, props, children) {
                    return { name, props, children };
                }
                """);
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "package.json"),
                """{"type":"module"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "render-context-core.mjs"),
                System.IO.File.ReadAllText(FindRepositoryFile("src/Jazor.RazorVue/Runtime/render-context-core.mjs")));
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "render-context.mjs"),
                """
                import { Fragment, createStaticVNode } from "vue";
                import { createRenderContextCore } from "./render-context-core.mjs";

                export function createRenderContext(h) {
                    return createRenderContextCore(h, Fragment, createStaticVNode);
                }
                """);
            var testFile = Path.Combine(tempRoot, "invoke-async-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/counter.mjs";

                test("invokeAsync dispatches the work item synchronously before any microtask", () => {
                    const render = component.setup({}, { slots: {} });
                    const first = render();
                    assert.deepEqual(first.children, [0]);

                    first.props.onClick();

                    const second = render();
                    assert.deepEqual(second.children, [1]);
                });
                """);

            await RunNodeTestAsync(testFile, tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_EmitsOnParametersSetAsyncSerializedWatch()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using System.Threading.Tasks;
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string Title { get; set; } = "";

                    private int count;

                    protected override Task OnParametersSetAsync()
                    {
                        count = Title.Length;
                        return Task.CompletedTask;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "div");
                        builder.AddContent(1, Title);
                        builder.AddContent(2, count);
                        builder.CloseElement();
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "import { defineComponent, h, reactive, watch } from \"vue\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "let parametersSetAsyncGen = 0;", StringComparison.Ordinal);
        StringAssert.Contains(script, "let parametersSetAsyncTail = Promise.resolve();", StringComparison.Ordinal);
        StringAssert.Contains(script, "const runOnParametersSetAsync = () => {", StringComparison.Ordinal);
        StringAssert.Contains(script, "const gen = ++parametersSetAsyncGen;", StringComparison.Ordinal);
        StringAssert.Contains(script, "parametersSetAsyncTail = parametersSetAsyncTail", StringComparison.Ordinal);
        StringAssert.Contains(script, "if (gen !== parametersSetAsyncGen) {", StringComparison.Ordinal);
        StringAssert.Contains(script, "return Promise.resolve(scope.onParametersSetAsync()).then(", StringComparison.Ordinal);
        StringAssert.Contains(script, "if (gen === parametersSetAsyncGen) {", StringComparison.Ordinal);
        StringAssert.Contains(script, "runOnParametersSetAsync();", StringComparison.Ordinal);
        StringAssert.Contains(script, "function onParametersSetAsync()", StringComparison.Ordinal);
        StringAssert.Contains(script, "state.count = props.title.length;", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_EmitsOnAfterRenderAsyncHooks()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using System.Threading.Tasks;
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    private int afterRenderCount;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "div");
                        builder.AddContent(1, afterRenderCount);
                        builder.CloseElement();
                    }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        afterRenderCount = firstRender ? 1 : afterRenderCount + 1;
                        return Task.CompletedTask;
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "import { defineComponent, h, onMounted, onUpdated, reactive } from \"vue\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "void Promise.resolve(scope.onAfterRenderAsync(true));", StringComparison.Ordinal);
        StringAssert.Contains(script, "void Promise.resolve(scope.onAfterRenderAsync(false));", StringComparison.Ordinal);
        StringAssert.Contains(script, "function onAfterRenderAsync(firstRender)", StringComparison.Ordinal);
        // Completion must not auto-invalidate/render.
        Assert.IsFalse(
            script.Contains("onAfterRenderAsync(true)).then", StringComparison.Ordinal) ||
            script.Contains("onAfterRenderAsync(false)).then", StringComparison.Ordinal),
            script);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_EmitsChildContentDefaultSlotBridge()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/panel")]
                public partial class Panel : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string Title { get; set; } = "";

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Title);
                        if (ChildContent != null)
                        {
                            builder.AddContent(2, ChildContent);
                        }
                        builder.CloseElement();
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "props: [", StringComparison.Ordinal);
        StringAssert.Contains(script, "\"title\"", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("\"childContent\"", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "setup(props, { slots }) {", StringComparison.Ordinal);
        StringAssert.Contains(script, "if (typeof slots.default === \"function\") {", StringComparison.Ordinal);
        StringAssert.Contains(script, "props.childContent = (builder) => {", StringComparison.Ordinal);
        StringAssert.Contains(script, "const content = slots.default();", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.addContent(content);", StringComparison.Ordinal);
        StringAssert.Contains(script, "props.childContent", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeBridgesNamedRenderFragmentSlot()
    {
        var childFixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/child")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? Header { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "before");
                        if (Header != null)
                        {
                            builder.AddContent(2, Header);
                        }
                        builder.CloseElement();
                    }
                }
            }
            """);
        var childClosure = BuildClosure(childFixture);
        var childArtifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            childFixture.Binding,
            childFixture.Component,
            childClosure);
        var childScript = childArtifact.ModuleText.ReplaceLineEndings("\n");
        StringAssert.Contains(childScript, "if (typeof slots.header === \"function\") {", StringComparison.Ordinal);
        StringAssert.Contains(childScript, "props.header = (builder) => {", StringComparison.Ordinal);
        Assert.IsFalse(childScript.Contains("\"header\"", StringComparison.Ordinal), childScript);

        var parentFixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/child")]
                public partial class Child : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? Header { get; set; }
                }

                [ECMAScriptModule("./components/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment header = child =>
                        {
                            child.OpenElement(0, "h1");
                            child.AddContent(1, "Named header");
                            child.CloseElement();
                        };

                        builder.OpenComponent<Child>(2);
                        builder.AddComponentParameter(3, "Header", header);
                        builder.CloseComponent();
                    }
                }
            }
            """);
        var parentClosure = BuildClosure(parentFixture);
        var parentArtifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            parentFixture.Binding,
            parentFixture.Component,
            parentClosure);
        var parentScript = parentArtifact.ModuleText.ReplaceLineEndings("\n");
        StringAssert.Contains(parentScript, "builder.addComponentSlot(\"Header\", header);", StringComparison.Ordinal);
        Assert.IsFalse(parentScript.Contains("builder.addComponentParameter(\"Header\"", StringComparison.Ordinal), parentScript);

        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "Jazor.RazorVue.Sg.Test",
            Guid.NewGuid().ToString("N"));
        try
        {
            WriteFile(Path.Combine(tempRoot, childArtifact.RelativePath), childArtifact.ModuleText);
            WriteFile(Path.Combine(tempRoot, parentArtifact.RelativePath), parentArtifact.ModuleText);
            WriteFile(
                Path.Combine(tempRoot, "package.json"),
                """{"type":"module"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "vue", "package.json"),
                """{"type":"module","exports":"./index.mjs"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "vue", "index.mjs"),
                """
                export const Fragment = Symbol("Fragment");
                export function createStaticVNode(html, count) {
                    return { html, count };
                }
                export function defineComponent(options) {
                    return options;
                }
                export function reactive(value) {
                    return value;
                }
                export function watch() {
                    return () => {};
                }
                export function onMounted() {}
                export function onUpdated() {}
                export function onUnmounted() {}
                export function h(name, props, children) {
                    return { name, props, children };
                }
                """);
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "package.json"),
                """{"type":"module"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "render-context-core.mjs"),
                System.IO.File.ReadAllText(FindRepositoryFile("src/Jazor.RazorVue/Runtime/render-context-core.mjs")));
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "render-context.mjs"),
                """
                import { Fragment, createStaticVNode } from "vue";
                import { createRenderContextCore } from "./render-context-core.mjs";

                export function createRenderContext(h) {
                    return createRenderContextCore(h, Fragment, createStaticVNode);
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-named-slot-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import parent from "./components/parent.mjs";

                test("named RenderFragment parameter is transported as a Vue named slot", () => {
                    const parentRender = parent.setup({}, { slots: {} });
                    const childVNode = parentRender();

                    assert.equal(typeof childVNode.children.header, "function");
                    assert.equal(childVNode.props?.header, undefined);

                    const childRender = childVNode.name.setup(childVNode.props ?? {}, { slots: childVNode.children });
                    const rendered = childRender();

                    assert.equal(rendered.name, "section");
                    assert.deepEqual(rendered.children[0], "before");
                    assert.equal(rendered.children[1].name, "h1");
                    assert.deepEqual(rendered.children[1].children, ["Named header"]);
                });
                """);

            await RunNodeTestAsync(testFile, tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RejectsTypedRenderFragmentSlotUntilDescriptorLoweringExists()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/child")]
                public partial class Child : ComponentBase, IVueComponent
                {
                }

                [ECMAScriptModule("./components/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment<string> header = value => child =>
                        {
                            child.AddContent(0, value);
                        };

                        builder.OpenComponent<Child>(1);
                        builder.AddComponentParameter(2, "Header", header);
                        builder.CloseComponent();
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);

        OperationTransformationException? exception = null;
        try
        {
            await RazorSgVueComponentModuleBuilder.BuildAsync(
                fixture.Binding,
                fixture.Component,
                closure);
        }
        catch (OperationTransformationException ex)
        {
            exception = ex;
        }

        Assert.IsNotNull(exception);
        StringAssert.Contains(exception.Message, "Header", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "RenderFragment<T>", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "typed slot descriptor", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_UsesCSharpDefaultsForUninitializedState()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    private int count;
                    private bool enabled;
                    private string? label;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, count);
                        builder.AddContent(1, enabled);
                        builder.AddContent(2, label);
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);

        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "count: 0", StringComparison.Ordinal);
        StringAssert.Contains(script, "enabled: false", StringComparison.Ordinal);
        StringAssert.Contains(script, "label: null", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("count: undefined", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("enabled: undefined", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("label: undefined", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_SourceMapChainsCompilerSegmentsToOriginalRazor()
    {
        var fixture = CreateOfficialCounterFixture();
        var closure = BuildClosure(fixture);

        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);

        var sources = ReadSourceMapSources(artifact.SourceMapContent);
        var segments = DecodeSourceMapSegments(artifact.SourceMapContent);
        Assert.IsTrue(
            sources.Any(static source => source.EndsWith("Counter.razor", StringComparison.Ordinal)),
            "RazorVue component maps should resolve to the original .razor source: " + string.Join(", ", sources));
        Assert.IsFalse(
            sources.Any(static source => source.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)),
            "RazorVue component maps should not stop at the generated Razor C# source: " + string.Join(", ", sources));
        Assert.IsTrue(
            segments.Count > 0,
            "Expected RazorVue component maps to emit at least one resolved source-map segment.");
    }

    [TestMethod]
    public async Task BuildVueComponentModule_SourceMapPreservesMultipleRazorSourceMappingSegments()
    {
        const string documentPath = @"D:\repo\Demo\Pages\Counter.razor";
        const string generatedSource = """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string Title { get; set; } = "";

                    private int count = 1;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Title);
                        builder.AddContent(2, count);
                        builder.CloseElement();
                    }
                }
            }
            """;
        var titleGeneratedLine = GetLineIndexContaining(generatedSource, "builder.AddContent(1, Title);");
        var countGeneratedLine = GetLineIndexContaining(generatedSource, "builder.AddContent(2, count);");
        var sourceMappings = ImmutableArray.Create(
            CreateSourceMapping(
                documentPath,
                originalLine: 0,
                originalColumn: 4,
                generatedSource,
                titleGeneratedLine,
                GetColumnIndexContaining(generatedSource, titleGeneratedLine, "builder.AddContent"),
                "builder.AddContent(1, Title);".Length),
            CreateSourceMapping(
                documentPath,
                originalLine: 1,
                originalColumn: 3,
                generatedSource,
                countGeneratedLine,
                GetColumnIndexContaining(generatedSource, countGeneratedLine, "builder.AddContent"),
                "builder.AddContent(2, count);".Length));
        var fixture = CreateManualGeneratedFixture(
            generatedSource,
            documentPath,
            sourceMappings);
        var closure = BuildClosure(fixture);

        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var sources = ReadSourceMapSources(artifact.SourceMapContent);
        var segments = DecodeSourceMapSegments(artifact.SourceMapContent);
        var razorSourceIndex = Array.FindIndex(
            sources,
            static source => string.Equals(source, "Pages/Counter.razor", StringComparison.Ordinal));

        Assert.IsTrue(razorSourceIndex >= 0, "Expected source map to contain Pages/Counter.razor.");
        Assert.IsTrue(
            segments.Any(segment => segment.SourceIndex == razorSourceIndex && segment.SourceLine == 0 && segment.SourceColumn == 4),
            "Expected generated Title segment to map to the first Razor source mapping.");
        Assert.IsTrue(
            segments.Any(segment => segment.SourceIndex == razorSourceIndex && segment.SourceLine == 1 && segment.SourceColumn == 3),
            "Expected generated count segment to map to the second Razor source mapping.");
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeKeepsStateInitializerOnceAndStableHandler()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string Title { get; set; } = "";

                    private int count = Seed();

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "button");
                        builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, Increment));
                        builder.AddContent(2, Title);
                        builder.AddContent(3, count);
                        builder.CloseElement();
                    }

                    private void Increment()
                    {
                        count++;
                    }

                    private static int Seed() => 1;
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);

        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "Jazor.RazorVue.Sg.Test",
            Guid.NewGuid().ToString("N"));
        try
        {
            WriteFile(Path.Combine(tempRoot, artifact.RelativePath), artifact.ModuleText);
            WriteFile(
                Path.Combine(tempRoot, "package.json"),
                """{"type":"module"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "vue", "package.json"),
                """{"type":"module","exports":"./index.mjs"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "vue", "index.mjs"),
                """
                export const Fragment = Symbol("Fragment");
                export function createStaticVNode(html, count) {
                    return { html, count };
                }
                export const reactiveCalls = [];
                export function defineComponent(options) {
                    return options;
                }
                export function reactive(value) {
                    reactiveCalls.push(value);
                    return value;
                }
                export function watch() {
                    return () => {};
                }
                export function onMounted() {}
                export function onUpdated() {}
                export function onUnmounted() {}
                export function h(name, props, children) {
                    return { name, props, children };
                }
                """);
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "package.json"),
                """{"type":"module"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "render-context-core.mjs"),
                System.IO.File.ReadAllText(FindRepositoryFile("src/Jazor.RazorVue/Runtime/render-context-core.mjs")));
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "render-context.mjs"),
                """
                import { Fragment, createStaticVNode } from "vue";
                import { createRenderContextCore } from "./render-context-core.mjs";

                export function createRenderContext(h) {
                    return createRenderContextCore(h, Fragment, createStaticVNode);
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/counter.mjs";
                import { reactiveCalls } from "vue";

                test("state initializer runs once per setup and handler identity is stable", () => {
                    const render = component.setup({ title: "Count: " }, { slots: {} });

                    // One reactive(state) per setup.
                    console.error("DBG reactiveCalls:", JSON.stringify(reactiveCalls));
                    assert.equal(reactiveCalls.length, 1);
                    assert.ok("count" in reactiveCalls[0]);
                    assert.deepEqual(component.props, ["title"]);

                    const first = render();
                    const handler = first.props.onClick;
                    assert.equal(first.name, "button");
                    assert.deepEqual(first.children, ["Count: ", 1]);

                    const second = render();
                    assert.equal(second.props.onClick, handler);
                    assert.deepEqual(second.children, ["Count: ", 1]);

                    handler();

                    const third = render();
                    assert.equal(third.props.onClick, handler);
                    assert.deepEqual(third.children, ["Count: ", 2]);
                    assert.equal(reactiveCalls.length, 1);

                    const otherRender = component.setup({ title: "Other: " }, { slots: {} });
                    const other = otherRender();
                    assert.equal(reactiveCalls.length, 2);
                    assert.notEqual(other.props.onClick, handler);
                    assert.deepEqual(other.children, ["Other: ", 1]);
                });
                """);

            await RunNodeTestAsync(testFile, tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeNormalizesChildComponentParameterProps()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/child")]
                public partial class Child : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string Title { get; set; } = "";

                    [Parameter]
                    public EventCallback<string> OnValueChanged { get; set; }
                }

                [ECMAScriptModule("./components/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string Title { get; set; } = "";

                    private string last = "";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Child>(0);
                        builder.AddComponentParameter(1, "Title", Title);
                        builder.AddComponentParameter(2, "OnValueChanged", EventCallback.Factory.Create<string>(this, HandleValueChanged));
                        builder.CloseComponent();
                    }

                    private void HandleValueChanged(string value)
                    {
                        last = value;
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        StringAssert.Contains(
            artifact.ModuleText.ReplaceLineEndings("\n"),
            "from \"./child.mjs\";",
            StringComparison.Ordinal);

        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "Jazor.RazorVue.Sg.Test",
            Guid.NewGuid().ToString("N"));
        try
        {
            WriteFile(Path.Combine(tempRoot, artifact.RelativePath), artifact.ModuleText);
            WriteFile(Path.Combine(tempRoot, "components", "child.mjs"), "export default { name: \"Child\" };\n");
            WriteFile(
                Path.Combine(tempRoot, "package.json"),
                """{"type":"module"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "vue", "package.json"),
                """{"type":"module","exports":"./index.mjs"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "vue", "index.mjs"),
                """
                export const Fragment = Symbol("Fragment");
                export function createStaticVNode(html, count) {
                    return { html, count };
                }
                export function defineComponent(options) {
                    return options;
                }
                export function reactive(value) {
                    return value;
                }
                export function watch() {
                    return () => {};
                }
                export function onMounted() {}
                export function onUpdated() {}
                export function onUnmounted() {}
                export function h(name, props, children) {
                    return { name, props, children };
                }
                """);
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "package.json"),
                """{"type":"module"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "render-context-core.mjs"),
                System.IO.File.ReadAllText(FindRepositoryFile("src/Jazor.RazorVue/Runtime/render-context-core.mjs")));
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "render-context.mjs"),
                """
                import { Fragment, createStaticVNode } from "vue";
                import { createRenderContextCore } from "./render-context-core.mjs";

                export function createRenderContext(h) {
                    return createRenderContextCore(h, Fragment, createStaticVNode);
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-child-parameter-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/parent.mjs";

                test("component parameters normalize to generated child runtime prop names", () => {
                    const render = component.setup({ title: "Hello" }, { slots: {} });
                    const vnode = render();

                    assert.equal(vnode.name.name, "Child");
                    assert.equal(vnode.props.title, "Hello");
                    assert.equal(typeof vnode.props.onValueChanged, "function");
                    assert.equal(vnode.props.Title, undefined);
                    assert.equal(vnode.props.OnValueChanged, undefined);
                });
                """);

            await RunNodeTestAsync(testFile, tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeLowersComponentBindValuePair()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/child")]
                public partial class Child : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string Value { get; set; } = "";

                    [Parameter]
                    public EventCallback<string> ValueChanged { get; set; }
                }

                [ECMAScriptModule("./components/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    private string text = "initial";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Child>(0);
                        builder.AddComponentParameter(1, "Value", text);
                        builder.AddComponentParameter(2, "ValueChanged", EventCallback.Factory.CreateBinder(this, __value => text = __value, text));
                        builder.CloseComponent();
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "builder.addComponentParameter(\"Value\", state.text);", StringComparison.Ordinal);
        StringAssert.Contains(script, "builder.addComponentParameter(\"ValueChanged\", __value => state.text = __value);", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("modelValue", StringComparison.Ordinal), script);

        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "Jazor.RazorVue.Sg.Test",
            Guid.NewGuid().ToString("N"));
        try
        {
            WriteFile(Path.Combine(tempRoot, artifact.RelativePath), artifact.ModuleText);
            WriteFile(Path.Combine(tempRoot, "components", "child.mjs"), "export default { name: \"Child\" };\n");
            WriteFile(
                Path.Combine(tempRoot, "package.json"),
                """{"type":"module"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "vue", "package.json"),
                """{"type":"module","exports":"./index.mjs"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "vue", "index.mjs"),
                """
                export const Fragment = Symbol("Fragment");
                export function createStaticVNode(html, count) {
                    return { html, count };
                }
                export function defineComponent(options) {
                    return options;
                }
                export function reactive(value) {
                    return value;
                }
                export function watch() {
                    return () => {};
                }
                export function onMounted() {}
                export function onUpdated() {}
                export function onUnmounted() {}
                export function h(name, props, children) {
                    return { name, props, children };
                }
                """);
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "package.json"),
                """{"type":"module"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "render-context-core.mjs"),
                System.IO.File.ReadAllText(FindRepositoryFile("src/Jazor.RazorVue/Runtime/render-context-core.mjs")));
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "render-context.mjs"),
                """
                import { Fragment, createStaticVNode } from "vue";
                import { createRenderContextCore } from "./render-context-core.mjs";

                export function createRenderContext(h) {
                    return createRenderContextCore(h, Fragment, createStaticVNode);
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-component-bind-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/parent.mjs";

                test("component bind-X uses X and XChanged instead of modelValue", () => {
                    const render = component.setup({}, { slots: {} });
                    const first = render();

                    assert.equal(first.name.name, "Child");
                    assert.equal(first.props.value, "initial");
                    assert.equal(typeof first.props.valueChanged, "function");
                    assert.equal(first.props.modelValue, undefined);
                    assert.equal(first.props["onUpdate:modelValue"], undefined);
                    assert.equal(first.props.Value, undefined);
                    assert.equal(first.props.ValueChanged, undefined);

                    first.props.valueChanged("updated");
                    const second = render();

                    assert.equal(second.props.value, "updated");
                    assert.equal(typeof second.props.valueChanged, "function");
                    assert.equal(second.props.modelValue, undefined);
                    assert.equal(second.props["onUpdate:modelValue"], undefined);
                });
                """);

            await RunNodeTestAsync(testFile, tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeUsesComponentBindDescriptorNames()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/child")]
                [VueProp(nameof(Value), VuePropKind.Model, Name = "modelValue", AcceptsBinding = true)]
                [VueLibraryEmit(nameof(ValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
                public partial class Child : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string Value { get; set; } = "";

                    [Parameter]
                    public EventCallback<string> ValueChanged { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, Value);
                    }
                }

                [ECMAScriptModule("./components/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    private string text = "initial";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Child>(0);
                        builder.AddComponentParameter(1, "Value", text);
                        builder.AddComponentParameter(2, "ValueChanged", EventCallback.Factory.CreateBinder(this, __value => text = __value, text));
                        builder.CloseComponent();
                    }
                }
            }
            """);
        var child = fixture.Binding.Components.Single(component => component.ComponentSymbol.Name == "Child");
        var childClosure = BuildClosure(fixture, child.ComponentSymbol.Name);
        var childArtifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            child,
            childClosure);
        var childScript = childArtifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(childScript, "\"modelValue\"", StringComparison.Ordinal);
        StringAssert.Contains(childScript, "\"update:modelValue\"", StringComparison.Ordinal);
        StringAssert.Contains(childScript, "props.modelValue", StringComparison.Ordinal);
        Assert.IsFalse(childScript.Contains("props.value", StringComparison.Ordinal), childScript);

        var parent = fixture.Binding.Components.Single(component => component.ComponentSymbol.Name == "Counter");
        var parentClosure = BuildClosure(fixture, parent.ComponentSymbol.Name);
        var parentArtifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            parent,
            parentClosure);
        var parentScript = parentArtifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(parentScript, "builder.openComponent(", StringComparison.Ordinal);
        StringAssert.Contains(parentScript, "\"Value\": \"modelValue\"", StringComparison.Ordinal);
        StringAssert.Contains(parentScript, "\"ValueChanged\": \"onUpdate:modelValue\"", StringComparison.Ordinal);

        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "Jazor.RazorVue.Sg.Test",
            Guid.NewGuid().ToString("N"));
        try
        {
            WriteFile(Path.Combine(tempRoot, childArtifact.RelativePath), childArtifact.ModuleText);
            WriteFile(Path.Combine(tempRoot, parentArtifact.RelativePath), parentArtifact.ModuleText);
            WriteFile(
                Path.Combine(tempRoot, "package.json"),
                """{"type":"module"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "vue", "package.json"),
                """{"type":"module","exports":"./index.mjs"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "vue", "index.mjs"),
                """
                export const Fragment = Symbol("Fragment");
                export function createStaticVNode(html, count) {
                    return { html, count };
                }
                export function defineComponent(options) {
                    return options;
                }
                export function reactive(value) {
                    return value;
                }
                export function watch() {
                    return () => {};
                }
                export function onMounted() {}
                export function onUpdated() {}
                export function onUnmounted() {}
                export function h(name, props, children) {
                    return { name, props, children };
                }
                """);
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "package.json"),
                """{"type":"module"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "render-context-core.mjs"),
                System.IO.File.ReadAllText(FindRepositoryFile("src/Jazor.RazorVue/Runtime/render-context-core.mjs")));
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "@jazor", "vue-runtime", "render-context.mjs"),
                """
                import { Fragment, createStaticVNode } from "vue";
                import { createRenderContextCore } from "./render-context-core.mjs";

                export function createRenderContext(h) {
                    return createRenderContextCore(h, Fragment, createStaticVNode);
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-component-bind-descriptor-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/parent.mjs";

                test("component bind descriptor maps Razor pair to Vue model prop and update listener", () => {
                    const render = component.setup({}, { slots: {} });
                    const first = render();

                    assert.equal(first.props.modelValue, "initial");
                    assert.equal(typeof first.props["onUpdate:modelValue"], "function");
                    assert.equal(first.props.value, undefined);
                    assert.equal(first.props.valueChanged, undefined);

                    first.props["onUpdate:modelValue"]("updated");
                    const second = render();

                    assert.equal(second.props.modelValue, "updated");
                    assert.equal(typeof second.props["onUpdate:modelValue"], "function");
                });
                """);

            await RunNodeTestAsync(testFile, tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_EmitsEventCallbackMetadataFromParameters()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/callbacks")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback OnClick { get; set; }

                    [Parameter]
                    public EventCallback<string> ValueChanged { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        _ = OnClick.InvokeAsync();
                        _ = ValueChanged.InvokeAsync("ready");
                        builder.AddContent(0, "callbacks");
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "props: [", StringComparison.Ordinal);
        StringAssert.Contains(script, "\"onClick\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "\"valueChanged\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "emits: [", StringComparison.Ordinal);
        StringAssert.Contains(script, "\"click\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "\"valueChanged\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "props.onClick?.();", StringComparison.Ordinal);
        StringAssert.Contains(script, "props.valueChanged?.(\"ready\");", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("stateHasChanged();", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("Promise.resolve(props.", StringComparison.Ordinal), script);
        Assert.IsTrue(
            script.IndexOf("props: [", StringComparison.Ordinal) <
            script.IndexOf("emits: [", StringComparison.Ordinal),
            script);
        var emitsIndex = script.IndexOf("emits: [", StringComparison.Ordinal);
        Assert.IsTrue(
            script.IndexOf("\"click\"", emitsIndex, StringComparison.Ordinal) <
            script.IndexOf("\"valueChanged\"", emitsIndex, StringComparison.Ordinal),
            script);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_EventCallbackAwaitUsesSourceAwaitAndLifecycleInvalidationOnly()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using System.Threading.Tasks;
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/callbacks")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<string> Ready { get; set; }

                    protected override async Task OnInitializedAsync()
                    {
                        await Ready.InvokeAsync("ready");
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, "callbacks");
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "props.ready?.(\"ready\");", StringComparison.Ordinal);
        StringAssert.Contains(script, "async function onInitializedAsync()", StringComparison.Ordinal);
        StringAssert.Contains(script, "await props.ready?.(\"ready\");", StringComparison.Ordinal);
        StringAssert.Contains(script, "Promise.resolve(scope.onInitializedAsync()).then(", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("try {", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("catch (", StringComparison.Ordinal), script);
    }

    private static bool IsSha256Hash(string value)
        => value.Length == "sha256:".Length + 64 &&
           value.StartsWith("sha256:", StringComparison.Ordinal) &&
           value["sha256:".Length..].All(static ch =>
               ch is >= '0' and <= '9' ||
               ch is >= 'a' and <= 'f');

    private static string[] ReadSourceMapSources(string sourceMapContent)
    {
        using var document = System.Text.Json.JsonDocument.Parse(sourceMapContent);
        return document.RootElement
            .GetProperty("sources")
            .EnumerateArray()
            .Select(static source => source.GetString() ?? string.Empty)
            .ToArray();
    }

    private static IReadOnlyList<DecodedSourceMapSegment> DecodeSourceMapSegments(string sourceMapContent)
    {
        using var document = System.Text.Json.JsonDocument.Parse(sourceMapContent);
        var mappings = document.RootElement.GetProperty("mappings").GetString() ?? string.Empty;
        var segments = new List<DecodedSourceMapSegment>();
        var generatedLine = 0;
        var previousGeneratedColumn = 0;
        var previousSourceIndex = 0;
        var previousSourceLine = 0;
        var previousSourceColumn = 0;
        var position = 0;

        while (position < mappings.Length)
        {
            var current = mappings[position];
            if (current == ';')
            {
                generatedLine++;
                previousGeneratedColumn = 0;
                position++;
                continue;
            }

            if (current == ',')
            {
                position++;
                continue;
            }

            previousGeneratedColumn += DecodeSourceMapVlq(mappings, ref position);
            if (position >= mappings.Length || mappings[position] == ',' || mappings[position] == ';')
                continue;

            previousSourceIndex += DecodeSourceMapVlq(mappings, ref position);
            previousSourceLine += DecodeSourceMapVlq(mappings, ref position);
            previousSourceColumn += DecodeSourceMapVlq(mappings, ref position);
            segments.Add(new DecodedSourceMapSegment(
                generatedLine,
                previousGeneratedColumn,
                previousSourceIndex,
                previousSourceLine,
                previousSourceColumn));

            if (position < mappings.Length && mappings[position] != ',' && mappings[position] != ';')
                _ = DecodeSourceMapVlq(mappings, ref position);
        }

        return segments;
    }

    private static int DecodeSourceMapVlq(string mappings, ref int position)
    {
        const string base64Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        var result = 0;
        var shift = 0;
        var continuation = true;

        while (continuation)
        {
            Assert.IsTrue(position < mappings.Length, "Unexpected end of source-map VLQ mapping.");
            var digit = base64Digits.IndexOf(mappings[position], StringComparison.Ordinal);
            Assert.IsTrue(digit >= 0, $"Invalid source-map VLQ digit '{mappings[position]}'.");
            position++;

            continuation = (digit & 32) != 0;
            digit &= 31;
            result += digit << shift;
            shift += 5;
        }

        var isNegative = (result & 1) == 1;
        result >>= 1;
        return isNegative ? -result : result;
    }

    private readonly record struct DecodedSourceMapSegment(
        int GeneratedLine,
        int GeneratedColumn,
        int SourceIndex,
        int SourceLine,
        int SourceColumn);

    private static async Task RunNodeTestAsync(string testFile, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "node",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory
        };
        startInfo.ArgumentList.Add("--test");
        startInfo.ArgumentList.Add(testFile);

        Process process;
        try
        {
            process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Failed to start node test process.");
        }
        catch (Win32Exception ex)
        {
            Assert.Inconclusive("Node.js was not available to run the RazorVue generated module runtime test: " + ex.Message);
            return;
        }

        using (process)
        {
            var standardOutput = process.StandardOutput.ReadToEndAsync();
            var standardError = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException)
                {
                }

                Assert.Fail("Node.js runtime test timed out after 30 seconds.");
            }

            var output = await standardOutput;
            var error = await standardError;
            if (process.ExitCode != 0)
            {
                Assert.Fail(
                    "Node.js runtime test failed with exit code " +
                    process.ExitCode +
                    Environment.NewLine +
                    output +
                    Environment.NewLine +
                    error);
            }
        }
    }

    private static string FindRepositoryFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate repository file.", relativePath);
    }

    private static void WriteFile(string path, string contents)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        System.IO.File.WriteAllText(path, contents.ReplaceLineEndings("\n"));
    }

    private static RazorSgComponentMemberClosure BuildClosure(ClosureFixture fixture)
    {
        var built = RazorSgComponentMemberClosureBuilder.TryBuild(
            fixture.Binding,
            fixture.Component,
            out var closure,
            out var failure);

        Assert.IsTrue(built, failure);
        Assert.IsNotNull(closure);
        return closure!;
    }

    private static RazorSgComponentMemberClosure BuildClosure(ClosureFixture fixture, string componentName)
    {
        var component = fixture.Binding.Components.Single(component =>
            string.Equals(component.ComponentSymbol.Name, componentName, StringComparison.Ordinal));
        var built = RazorSgComponentMemberClosureBuilder.TryBuild(
            fixture.Binding,
            component,
            out var closure,
            out var failure);

        Assert.IsTrue(built, failure);
        Assert.IsNotNull(closure);
        return closure!;
    }

    private static void AssertHasField(RazorSgComponentMemberClosure closure, string name)
        => Assert.IsTrue(
            closure.StateFields.Any(field => field.Name == name),
            $"Expected field '{name}' in closure: {Describe(closure)}");

    private static void AssertHasProperty(RazorSgComponentMemberClosure closure, string name)
        => Assert.IsTrue(
            closure.StateProperties.Concat(closure.ParameterProperties).Any(property => property.Name == name),
            $"Expected property '{name}' in closure: {Describe(closure)}");

    private static void AssertHasMethod(RazorSgComponentMemberClosure closure, string name)
        => Assert.IsTrue(
            closure.ReachableMethods.Any(method => method.Name == name),
            $"Expected method '{name}' in closure: {Describe(closure)}");

    private static void AssertNoMember(RazorSgComponentMemberClosure closure, string name)
        => Assert.IsFalse(
            closure.OrderedMembers.Any(member => member.Name == name),
            $"Did not expect member '{name}' in closure: {Describe(closure)}");

    private static string Describe(RazorSgComponentMemberClosure closure)
        => string.Join(", ", closure.OrderedMembers.Select(static member => member.ToDisplayString()));

    private static ClosureFixture CreateOfficialCounterFixture()
    {
        const string documentPath = @"D:\repo\Demo\Pages\Counter.razor";
        const string hintName = "Counter.razor.g.cs";
        const string documentText = """
            <button @onclick="Increment">@count</button>
            """;
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var baseCompilation = CSharpCompilation.Create(
            assemblyName: "RazorSg.ComponentMemberClosure.Official.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    global using Microsoft.AspNetCore.Components;
                    global using Microsoft.AspNetCore.Components.Web;
                    global using Microsoft.AspNetCore.Components.Rendering;
                    global using ECMAScript;
                    global using static ECMAScript.Vue3;
                    """,
                    options: parseOptions,
                    path: "GlobalUsings.g.cs"),
                CSharpSyntaxTree.ParseText(
                    """
                    namespace Demo.Pages
                    {
                        [ECMAScriptModule("./components/counter")]
                        public partial class Counter : ComponentBase, IVueComponent
                        {
                            private int count;

                            private void Increment()
                            {
                                count++;
                            }
                        }
                    }
                    """,
                    options: parseOptions,
                    path: "Counter.razor.cs")
            ],
            references: RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var projectEngine = RazorSgTestDocumentFactory.CreateProjectEngine(
            documentPath,
            parseOptions,
            rootNamespace: "Demo.Pages");
        var tagHelpers = RazorSgTestDocumentFactory.DiscoverTagHelpers(projectEngine, baseCompilation);
        var codeDocument = projectEngine.Process(
            RazorSgTestDocumentFactory.CreateSourceDocument(documentPath, SourceText.From(documentText)),
            RazorFileKind.Component,
            ImmutableArray<RazorSourceDocument>.Empty,
            tagHelpers.Length == 0 ? null : TagHelperCollection.Create(tagHelpers));
        var csharpDocument = RazorSgTestDocumentFactory.GetRequiredCSharpDocument(codeDocument);
        var generatedTree = CSharpSyntaxTree.ParseText(
            csharpDocument.Text,
            options: parseOptions,
            path: hintName);
        var compilation = baseCompilation.AddSyntaxTrees(generatedTree);
        AssertNoCompilationErrors(compilation);

        var adapted = RazorSgFinalDocumentAdapter.TryCreateBatch(
            compilation,
            ImmutableArray.Create(new RazorSgTailDocumentInput(hintName, codeDocument, csharpDocument)),
            out var batch,
            out var adaptationFailure);
        Assert.IsTrue(adapted, adaptationFailure);

        var bound = RazorSgGeneratedCSharpBinder.TryBind(
            batch!,
            out var binding,
            out var bindingFailure);
        Assert.IsTrue(bound, bindingFailure);

        var component = binding!.Components.Single();
        return new ClosureFixture(binding, component);
    }

    private static ClosureFixture CreateManualGeneratedFixture(
        string source,
        string documentSourcePath = @"D:\repo\Demo\Pages\Counter.razor",
        ImmutableArray<RazorSgSourceMapping> sourceMappings = default)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var generatedTree = CSharpSyntaxTree.ParseText(source, parseOptions, "Counter.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorSg.ComponentMemberClosure.Manual.Tests",
            syntaxTrees: [generatedTree],
            references: RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        AssertNoCompilationErrors(compilation);

        var semanticModel = compilation.GetSemanticModel(generatedTree);
        var buildRenderTreeDeclarations = generatedTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(static method => method.Identifier.ValueText == "BuildRenderTree")
            .ToArray();
        var document = new RazorSgGeneratedDocument(
            "Counter.razor.g.cs",
            documentSourcePath,
            SourceText.From(source),
            sourceMappings.IsDefault ? ImmutableArray<RazorSgSourceMapping>.Empty : sourceMappings);
        var components = buildRenderTreeDeclarations
            .Select(declaration =>
            {
                var method = semanticModel.GetDeclaredSymbol(declaration)
                    ?? throw new InvalidOperationException("BuildRenderTree symbol was not available.");
                var body = semanticModel.GetOperation(declaration.Body!) as IBlockOperation
                    ?? throw new InvalidOperationException("BuildRenderTree body operation was not available.");
                return new RazorSgBoundComponent(document, method.ContainingType, method, body);
            })
            .ToImmutableArray();
        var component = components.FirstOrDefault(static candidate => candidate.ComponentSymbol.Name == "Counter")
            ?? components.Single();
        var binding = new RazorSgGeneratedCSharpBinding(
            compilation,
            RazorSgCompilationBindingMode.ReusedHookCompilation,
            ImmutableArray.Create(document),
            components,
            ReusedGeneratedTreeCount: 1,
            DerivedGeneratedTreeCount: 0);

        return new ClosureFixture(binding, component);
    }

    private static RazorSgSourceMapping CreateSourceMapping(
        string originalPath,
        int originalLine,
        int originalColumn,
        string generatedSource,
        int generatedLine,
        int generatedColumn,
        int generatedLength)
        => new(
            new RazorSgSourceSpan(
                originalPath,
                AbsoluteIndex: 0,
                Length: 1,
                originalLine,
                originalColumn),
            new RazorSgSourceSpan(
                "Counter.razor.g.cs",
                GetAbsoluteIndex(generatedSource, generatedLine, generatedColumn),
                generatedLength,
                generatedLine,
                generatedColumn));

    private static int GetLineIndexContaining(string source, string value)
    {
        var lines = source.ReplaceLineEndings("\n").Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            if (lines[index].Contains(value, StringComparison.Ordinal))
                return index;
        }

        throw new InvalidOperationException("Could not find source line containing: " + value);
    }

    private static int GetColumnIndexContaining(string source, int line, string value)
    {
        var lines = source.ReplaceLineEndings("\n").Split('\n');
        if (line < 0 || line >= lines.Length)
            throw new InvalidOperationException("Source line index was out of range: " + line);

        var column = lines[line].IndexOf(value, StringComparison.Ordinal);
        if (column < 0)
            throw new InvalidOperationException("Could not find source column containing: " + value);

        return column;
    }

    private static int GetAbsoluteIndex(string source, int line, int column)
    {
        var normalized = source.ReplaceLineEndings("\n");
        var absolute = 0;
        for (var currentLine = 0; currentLine < line; currentLine++)
        {
            var newline = normalized.IndexOf('\n', absolute);
            if (newline < 0)
                throw new InvalidOperationException("Source line index was out of range: " + line);

            absolute = newline + 1;
        }

        return absolute + column;
    }

    private static void AssertNoCompilationErrors(Compilation compilation)
    {
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(
            0,
            errors.Length,
            string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));
    }

    private sealed record ClosureFixture(
        RazorSgGeneratedCSharpBinding Binding,
        RazorSgBoundComponent Component);
}
