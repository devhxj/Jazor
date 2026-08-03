using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
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
    private static readonly Lazy<string> DenoExecutable = new(ResolveDenoExecutable);

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
            using System;
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
    public async Task Build_CurrentComponentClosure_IncludesConstructedNestedRuntimeClass()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./format.mjs")]
                public static class FormatModule
                {
                    public static string Compose(string prefix, string suffix)
                    {
                        return prefix + suffix;
                    }
                }

                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        var state = BuildState();
                        builder.OpenElement(0, "p");
                        builder.AddContent(1, state.Text);
                        builder.CloseElement();
                    }

                    private HeaderState BuildState()
                    {
                        return new HeaderState("ready");
                    }

                    private sealed class HeaderState
                    {
                        private static readonly PrefixFormatter Formatter = new PrefixFormatter("header:");

                        public HeaderState(string text)
                        {
                            Text = Formatter.Format(text);
                        }

                        public string Text { get; }

                        private sealed class PrefixFormatter
                        {
                            private readonly string prefix;

                            public PrefixFormatter(string value)
                            {
                                prefix = value;
                            }

                            public string Format(string suffix)
                            {
                                return FormatModule.Compose(prefix, suffix);
                            }
                        }
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);

        CollectionAssert.AreEqual(
            new[] { "HeaderState", "PrefixFormatter" },
            closure.OrderedMembers
                .OfType<INamedTypeSymbol>()
                .Select(static type => type.Name)
                .OrderBy(static name => name, StringComparer.Ordinal)
                .ToArray());

        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "class HeaderState", StringComparison.Ordinal);
        StringAssert.Contains(script, "class PrefixFormatter", StringComparison.Ordinal);
        StringAssert.Contains(script, "import { compose } from \"../format.mjs\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "return new HeaderState(\"ready\");", StringComparison.Ordinal);
        StringAssert.Contains(script, "new PrefixFormatter(\"header:\")", StringComparison.Ordinal);
        StringAssert.Contains(script, "return compose(this.#prefix, suffix);", StringComparison.Ordinal);
        var importIndex = script.IndexOf("import { compose } from \"../format.mjs\";", StringComparison.Ordinal);
        var formatterIndex = script.IndexOf("class PrefixFormatter", StringComparison.Ordinal);
        var headerIndex = script.IndexOf("class HeaderState", StringComparison.Ordinal);
        Assert.IsTrue(
            importIndex < formatterIndex && formatterIndex < headerIndex,
            script);
        Assert.IsFalse(script.Contains("from \"./components/counter", StringComparison.Ordinal), script);
        _ = new Acornima.Parser().ParseModule(script);
    }

    [TestMethod]
    public async Task Build_CurrentComponentClosure_UsesVueDescriptorNamesForPropsAndListeners()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using System.Threading.Tasks;
            using ECMAScript;
            using ECMAScript.VueContract;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                [VueProp(nameof(Title), Name = "data-title")]
                [VueLibraryEmit(nameof(OnSave), Name = "saved")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string Title { get; set; } = "";

                    [Parameter]
                    public EventCallback<string> OnSave { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "button");
                        builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, Notify));
                        builder.AddContent(2, ReadTitle());
                        builder.CloseElement();
                    }

                    private string ReadTitle()
                    {
                        return Title;
                    }

                    private async Task Notify()
                    {
                        await OnSave.InvokeAsync(Title);
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

        StringAssert.Contains(script, "return props[\"data-title\"];", StringComparison.Ordinal);
        StringAssert.Contains(script, "props.onSaved?.(props[\"data-title\"]);", StringComparison.Ordinal);
        StringAssert.Contains(script, "props: [\"data-title\", \"onSave\"]", StringComparison.Ordinal);
        StringAssert.Contains(script, "emits: [\"saved\"]", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("props.title", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("props.onSave?.", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public async Task Build_CreateAstConverterOptions_UsesCompilerClosureAndCurrentComponentHost()
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

        Assert.AreEqual(AstConverterProfile.Standard, options.Profile);
        Assert.IsNotNull(options.MemberFilter);
        Assert.IsInstanceOfType(options.Host, typeof(RazorVueSemanticWalkerHost));
        Assert.AreSame(RazorVueModulePolicy.Instance, options.ModulePolicy);

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
    public async Task Build_RenderTreeBuilderFullSurfaceSlice_LowersThroughCurrentComponentHost()
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
                public sealed class Child : ComponentBase, IVueComponent
                {
                }

                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    private string ReadMarkup() => "<em>raw</em>";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "input");
                        builder.AddAttribute(1, "value", "before");
                        builder.SetAttributeValue(2, "after");
                        builder.AddContent(3, new MarkupString(ReadMarkup()));
                        builder.CloseElement();
                        builder.OpenComponent(4, typeof(Child));
                        builder.AddAttribute(5, "Title", "from attribute");
                        builder.AddComponentRenderMode(RenderMode(null));
                        builder.CloseComponent();
                        builder.Clear();
                        builder.Dispose();
                        var nested = new RenderTreeBuilder();
                        nested.AddMarkupContent(0, ReadMarkup());
                    }

                    private IComponentRenderMode RenderMode(IComponentRenderMode mode) => mode;
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var syntaxTree = fixture.Component.BuildRenderTreeMethod.DeclaringSyntaxReferences.Single().SyntaxTree;
        var semanticModel = fixture.Binding.Compilation.GetSemanticModel(syntaxTree);
        var converter = new AstConverter(
            fixture.Component.ComponentSymbol,
            semanticModel,
            closure.CreateAstConverterOptions());
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "builder.setAttributeValue(\"after\");", StringComparison.Ordinal);
        StringAssert.Contains(script!, "builder.addMarkupContent(readMarkup());", StringComparison.Ordinal);
        StringAssert.Contains(script!, "builder.openComponent(", StringComparison.Ordinal);
        StringAssert.Contains(script!, "builder.addAttribute(\"Title\", \"from attribute\");", StringComparison.Ordinal);
        StringAssert.Contains(script!, "builder.addComponentRenderMode(renderMode(null));", StringComparison.Ordinal);
        StringAssert.Contains(script!, "builder.clear();", StringComparison.Ordinal);
        StringAssert.Contains(script!, "builder.dispose();", StringComparison.Ordinal);
        StringAssert.Contains(script!, "let nested = createRenderContext(h);", StringComparison.Ordinal);
        StringAssert.Contains(script!, "nested.addMarkupContent(readMarkup());", StringComparison.Ordinal);
        StringAssert.Contains(script!, "from \"./components/child.mjs\";", StringComparison.Ordinal);
        StringAssert.Contains(script!, "@jazor/vue-runtime/render-context.mjs", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("MarkupString", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("from \"./components/child\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("new RenderTreeBuilder", StringComparison.Ordinal), script);

        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var moduleText = artifact.ModuleText.ReplaceLineEndings("\n");
        Assert.IsFalse(moduleText.Contains("createRenderContext", StringComparison.Ordinal), moduleText);
        Assert.IsFalse(moduleText.Contains(".addComponentRenderMode(", StringComparison.Ordinal), moduleText);
        Assert.IsFalse(moduleText.Contains(".clear()", StringComparison.Ordinal), moduleText);
        Assert.IsFalse(moduleText.Contains(".dispose()", StringComparison.Ordinal), moduleText);
        StringAssert.Contains(moduleText, "function $renderDirect()", StringComparison.Ordinal);
        StringAssert.Contains(moduleText, "return null;", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeAppliesMarkupAndAttributeValueSurface()
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
                    private string ReadMarkup() => "<strong>dynamic</strong>";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "article");
                        builder.AddAttribute(1, "data-state", "before");
                        builder.SetAttributeValue(2, "after");
                        builder.AddMarkupContent(3, ReadMarkup());
                        builder.AddContent(4, new MarkupString(ReadMarkup()));
                        builder.AddContent(5, new MarkupString("<em>literal</em>"));
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

        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.finish();", StringComparison.Ordinal), script);

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
                    return { kind: "static", html, count };
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
            var testFile = Path.Combine(tempRoot, "module-markup-attribute-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/counter.mjs";

                test("markup and SetAttributeValue materialize through render-context", () => {
                    const render = component.setup({}, { slots: {} });
                    const vnode = render();

                    assert.equal(vnode.name, "article");
                    assert.deepEqual(vnode.props, { "data-state": "after" });
                    assert.deepEqual(vnode.children, [
                        { kind: "static", html: "<strong>dynamic</strong>", count: 1 },
                        { kind: "static", html: "<strong>dynamic</strong>", count: 1 },
                        { kind: "static", html: "<em>literal</em>", count: 1 }
                    ]);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectLowersRegionAndMarkup()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/region-markup")]
                public partial class RegionMarkup : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenRegion(0);
                        builder.AddContent(1, "region");
                        builder.AddMarkupContent(2, "<strong>raw</strong>");
                        builder.CloseRegion();
                        builder.AddContent(3, "tail");
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

        StringAssert.Contains(script, "import { Fragment, createStaticVNode, defineComponent, h } from \"vue\";", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.finish();", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "h(Fragment, null, [h(Fragment, null, [\"region\", createStaticVNode(\"<strong>raw</strong>\", 1)]), \"tail\"])", StringComparison.Ordinal);

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
                    return { kind: "static", html, count };
                }
                export function defineComponent(options) {
                    return options;
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-region-markup-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/region-markup.mjs";

                test("region and markup lower directly to Fragment and static VNode", () => {
                    const render = component.setup({}, { slots: {} });
                    const vnode = render();

                    assert.equal(typeof vnode.name, "symbol");
                    assert.equal(typeof vnode.children[0].name, "symbol");
                    assert.deepEqual(vnode.children[0].children, [
                        "region",
                        { kind: "static", html: "<strong>raw</strong>", count: 1 }
                    ]);
                    assert.equal(vnode.children[1], "tail");
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectLowersStructuredChildrenAndMultipleAttributes()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using System.Collections.Generic;
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/structured")]
                public partial class StructuredChildren : ComponentBase, IVueComponent
                {
                    [Parameter(CaptureUnmatchedValues = true)]
                    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

                    private bool Show => true;

                    private string[] Items => ["one", "two"];

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddAttribute(1, "class", "structured");
                        builder.AddMultipleAttributes(2, AdditionalAttributes);
                        if (Show)
                        {
                            foreach (var item in Items)
                            {
                                builder.OpenElement(3, "span");
                                builder.AddContent(4, item);
                                builder.CloseElement();
                            }
                        }
                        else
                        {
                            builder.AddContent(5, "empty");
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

        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.finish();", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "function $renderDirect() {", StringComparison.Ordinal);
        StringAssert.Contains(script, "mergeProps(", StringComparison.Ordinal);
        StringAssert.Contains(script, "Array.from(", StringComparison.Ordinal);

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
                export function defineComponent(options) {
                    return options;
                }
                export function mergeProps(...values) {
                    return Object.assign({}, ...values.filter(Boolean));
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                export function reactive(value) {
                    return value;
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-structured-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/structured.mjs";

                test("structured children lower directly to VNodes", () => {
                    const render = component.setup({ additionalAttributes: { id: "root" } }, { slots: {} });
                    const vnode = render();

                    assert.equal(vnode.name, "section");
                    assert.equal(vnode.props.class, "structured");
                    assert.equal(vnode.props.id, "root");
                    assert.equal(vnode.children[0][0].name, "span");
                    assert.deepEqual(vnode.children[0].map((child) => child.children[0]), ["one", "two"]);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectPreservesConditionalAttributeGroupOrder()
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
                public sealed class Child : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Mode { get; set; }

                    [Parameter]
                    public string? TrueOnly { get; set; }

                    [Parameter]
                    public string? FalseOnly { get; set; }
                }

                [ECMAScriptModule("./components/conditional-props")]
                public partial class ConditionalProps : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Selected { get; set; }

                    private int _callCount;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Child>(0);
                        builder.AddAttribute(1, nameof(Child.Mode), Track("before"));
                        if (Selected)
                        {
                            builder.AddAttribute(2, nameof(Child.Mode), Track("true"));
                            builder.AddAttribute(3, nameof(Child.TrueOnly), Track("true-only"));
                        }
                        else
                        {
                            builder.AddAttribute(4, nameof(Child.Mode), Track("false"));
                            builder.AddAttribute(5, nameof(Child.FalseOnly), Track("false-only"));
                        }
                        builder.AddAttribute(6, nameof(Child.Mode), Track("after"));
                        builder.CloseComponent();
                    }

                    private string Track(string value)
                    {
                        _callCount++;
                        return value + ":" + _callCount;
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

        Assert.IsFalse(script.Contains("createRenderContext", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder)", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.finish()", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "mergeProps(", StringComparison.Ordinal);

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
                export function defineComponent(options) {
                    return options;
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                export function mergeProps(...values) {
                    return Object.assign({}, ...values);
                }
                export function reactive(value) {
                    return value;
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-conditional-attribute-order.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/conditional-props.mjs";

                test("conditional attribute groups preserve branch and overwrite order", () => {
                    const renderTrue = component.setup({ selected: true }, { slots: {} });
                    const trueVNode = renderTrue();
                    assert.deepEqual(trueVNode.props, {
                        mode: "after:4",
                        trueOnly: "true-only:3"
                    });

                    const renderFalse = component.setup({ selected: false }, { slots: {} });
                    const falseVNode = renderFalse();
                    assert.deepEqual(falseVNode.props, {
                        mode: "after:4",
                        falseOnly: "false-only:3"
                    });
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectResolvesComputedRenderFragmentProperty()
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
                public sealed class Child : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? Navigation { get; set; }
                }

                [ECMAScriptModule("./components/navigation")]
                public sealed class Navigation : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Horizontal { get; set; }
                }

                [ECMAScriptModule("./components/computed-slot")]
                public partial class ComputedSlot : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Top { get; set; }

                    private RenderFragment? HeaderNavigation => Top
                        ? BuildNavigation
                        : null;

                    private static void BuildNavigation(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Navigation>(0);
                        builder.AddComponentParameter(1, nameof(Navigation.Horizontal), true);
                        builder.CloseComponent();
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Child>(0);
                        builder.AddComponentParameter(1, nameof(Child.Navigation), HeaderNavigation);
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

        Assert.IsFalse(script.Contains("createRenderContext", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("typeof slots.headerNavigation", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "...props.top ? { navigation:", StringComparison.Ordinal);
        StringAssert.Contains(script, "horizontal: true", StringComparison.Ordinal);

        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "Jazor.RazorVue.Sg.Test",
            Guid.NewGuid().ToString("N"));
        try
        {
            WriteFile(Path.Combine(tempRoot, artifact.RelativePath), artifact.ModuleText);
            WriteFile(Path.Combine(tempRoot, "components", "child.mjs"), "export default { name: \"Child\" };\n");
            WriteFile(Path.Combine(tempRoot, "components", "navigation.mjs"), "export default { name: \"Navigation\" };\n");
            WriteFile(
                Path.Combine(tempRoot, "package.json"),
                """{"type":"module"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "vue", "package.json"),
                """{"type":"module","exports":"./index.mjs"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "vue", "index.mjs"),
                """
                export function defineComponent(options) {
                    return options;
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                export function reactive(value) {
                    return value;
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-computed-slot-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/computed-slot.mjs";

                test("computed RenderFragment properties lower to direct Vue slots", () => {
                    let topReads = 0;
                    const topProps = {};
                    Object.defineProperty(topProps, "top", {
                        get() {
                            topReads++;
                            return true;
                        }
                    });
                    const renderTop = component.setup(topProps, { slots: {} });
                    const topChild = renderTop();
                    assert.equal(topReads, 1);
                    const navigation = topChild.children.navigation();
                    assert.equal(topReads, 1);
                    assert.equal(navigation[0].name.name, "Navigation");
                    assert.equal(navigation[0].props.horizontal, true);

                    let sidebarReads = 0;
                    const sidebarProps = {};
                    Object.defineProperty(sidebarProps, "top", {
                        get() {
                            sidebarReads++;
                            return false;
                        }
                    });
                    const renderSidebar = component.setup(sidebarProps, { slots: {} });
                    const sidebarChild = renderSidebar();
                    assert.equal(sidebarReads, 1);
                    assert.equal("navigation" in sidebarChild.children, false);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectSharesRecursiveRenderFragmentHelperAcrossSiblingScopes()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/recursive-fragment")]
                public partial class RecursiveFragment : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool ShowFirst { get; set; }

                    [Parameter]
                    public bool ShowSecond { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        if (ShowFirst)
                        {
                            builder.AddContent(1, RenderItem(2));
                        }
                        if (ShowSecond)
                        {
                            builder.AddContent(2, RenderItem(1));
                        }
                        builder.CloseElement();
                    }

                    private RenderFragment RenderItem(int value) => child =>
                    {
                        child.OpenElement(0, "span");
                        child.AddContent(1, value);
                        if (value > 0)
                        {
                            child.AddContent(2, RenderItem(value - 1));
                        }
                        child.CloseElement();
                    };
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        Assert.AreEqual(
            1,
            script.Split("function renderRenderItem(", StringSplitOptions.None).Length - 1,
            script);
        Assert.IsTrue(
            script.IndexOf("function $renderDirect()", StringComparison.Ordinal) <
            script.IndexOf("function renderRenderItem(", StringComparison.Ordinal) &&
            script.IndexOf("function renderRenderItem(", StringComparison.Ordinal) <
            script.IndexOf("props.showFirst ? renderRenderItem", StringComparison.Ordinal),
            script);

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
                export function defineComponent(options) {
                    return options;
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                export function reactive(value) {
                    return value;
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-recursive-fragment-sibling-scopes.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/recursive-fragment.mjs";

                test("recursive RenderFragment helper is shared by sibling scopes", () => {
                    const render = component.setup({ showFirst: true, showSecond: true }, { slots: {} });
                    const root = render();
                    const first = root.children[0];
                    const second = root.children[1];

                    assert.equal(first.children[0], 2);
                    assert.equal(first.children[1].children[0], 1);
                    assert.equal(first.children[1].children[1].children[0], 0);
                    assert.equal(second.children[0], 1);
                    assert.equal(second.children[1].children[0], 0);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeLowersTypeOpenComponentToImportedChild()
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
                public sealed class Child : ComponentBase, IVueComponent
                {
                }

                [ECMAScriptModule("./components/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.OpenComponent(1, typeof(Child));
                        builder.AddComponentParameter(2, "Title", "direct");
                        builder.CloseComponent();
                        var childType = typeof(Child);
                        builder.OpenComponent(3, childType);
                        builder.AddComponentParameter(4, "Title", "alias");
                        builder.CloseComponent();
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

        StringAssert.Contains(script, "from \"./child.mjs\";", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("childType", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("typeof", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.finish();", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "return h(\"section\", null, [h(", StringComparison.Ordinal);
        StringAssert.Contains(script, "{ title: \"direct\" }", StringComparison.Ordinal);
        StringAssert.Contains(script, "{ title: \"alias\" }", StringComparison.Ordinal);

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
            var testFile = Path.Combine(tempRoot, "module-type-open-component-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/parent.mjs";

                test("OpenComponent(int, typeof(T)) and local typeof alias render imported child components", () => {
                    const render = component.setup({}, { slots: {} });
                    const vnode = render();

                    assert.equal(vnode.name, "section");
                    assert.equal(vnode.children[0].name.name, "Child");
                    assert.deepEqual(vnode.children[0].props, { title: "direct" });
                    assert.equal(vnode.children[1].name.name, "Child");
                    assert.deepEqual(vnode.children[1].props, { title: "alias" });
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectLowersInlineRenderFragmentParameterAsSlot()
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
                    public RenderFragment? ChildContent { get; set; }
                }

                [ECMAScriptModule("./components/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    private string title = "direct";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Child>(0);
                        builder.AddComponentParameter(1, "ChildContent", (RenderFragment)(child =>
                        {
                            child.OpenElement(0, "span");
                            AddText(child, title);
                            child.CloseElement();
                        }));
                        builder.CloseComponent();
                    }

                    private static void AddText(RenderTreeBuilder builder, string text)
                    {
                        builder.AddContent(0, text);
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

        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "function $renderDirect() {", StringComparison.Ordinal);
        StringAssert.Contains(script, "default: () => [].concat(h(\"span\", null, [state.title]) ?? [])", StringComparison.Ordinal);

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
                export function defineComponent(options) {
                    return options;
                }
                export function reactive(value) {
                    return value;
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-inline-slot-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";
                import { h } from "vue";

                import component from "./components/parent.mjs";

                test("inline RenderFragment parameter lowers to a Vue slot function", () => {
                    const render = component.setup({}, { slots: {} });
                    const child = render();
                    const slot = child.children.default();

                    assert.equal(child.name.name, "Child");
                    assert.equal(slot[0].name, "span");
                    assert.deepEqual(slot[0].children, ["direct"]);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectKeepsComputedPropertyNameAlignedWithPatternLocalCollision()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using ECMAScript.VueContract;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/theme-frame")]
                public sealed class ThemeFrame : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Theme { get; set; }
                }

                [ECMAScriptModule("./components/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    private string Theme => "dark";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<ThemeFrame>(0);
                        builder.AddComponentParameter(1, nameof(ThemeFrame.Theme), Theme);
                        builder.CloseComponent();
                        builder.AddContent(2, Normalize(null));
                    }

                    private static string Normalize(string? value)
                        => value is { } theme ? theme : string.Empty;
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "function theme()", StringComparison.Ordinal);
        StringAssert.Contains(script, "{ theme: theme() }", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("function Theme()", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectLowersAddAttributeLibraryComponentSlotAndDeduplicatesImport()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using ECMAScript.VueContract;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [VueLibraryComponent("npm:demo-links@1.mjs", "DemoLink")]
                [VueSlot(nameof(ChildContent), IsDefault = true)]
                public sealed class DemoLink : ComponentBase, IVueLibraryComponent
                {
                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }

                [ECMAScriptModule("./components/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<DemoLink>(0);
                        builder.AddAttribute(1, nameof(DemoLink.ChildContent), (RenderFragment)(childBuilder =>
                            childBuilder.AddContent(0, "linked")));
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

        Assert.AreEqual(
            1,
            script.Split("from \"npm:demo-links@1.mjs\";", StringSplitOptions.None).Length - 1,
            script);
        StringAssert.Contains(script, "default: () => [].concat(\"linked\" ?? [])", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("childBuilder =>", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("childBuilder.addContent", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectResolvesVueInjectImportAndRuntimeNames()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using ECMAScript.VueContract;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            [assembly: VueInject(typeof(Demo.Pages.ContractShell), typeof(Demo.Pages.InjectedShell))]

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/contract-shell")]
                public partial class ContractShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }

                [ECMAScriptModule("./components/injected-shell")]
                [VueProp(nameof(Title), Name = "injectedTitle")]
                [VueSlot(nameof(ChildContent), Name = "injected-content")]
                public partial class InjectedShell : ComponentBase, IVueComponent, IVueContainerImplementation<ContractShell>
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddAttribute(1, "data-injected-shell", true);
                        builder.AddContent(2, ChildContent);
                        builder.CloseElement();
                    }
                }

                [ECMAScriptModule("./components/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<ContractShell>(0);
                        builder.AddComponentParameter(1, nameof(ContractShell.Title), "Injected title");
                        builder.AddComponentParameter(2, nameof(ContractShell.ChildContent),
                            (RenderFragment)(childBuilder => childBuilder.AddContent(0, "Injected content")));
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

        var injectedImport = script.Split('\n').Single(static line =>
            line.EndsWith("from \"./injected-shell.mjs\";", StringComparison.Ordinal));
        Assert.IsTrue(injectedImport.StartsWith("import i$", StringComparison.Ordinal), injectedImport);
        Assert.IsFalse(injectedImport.StartsWith("import {", StringComparison.Ordinal), injectedImport);
        StringAssert.Contains(script, "injectedTitle: \"Injected title\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "\"injected-content\": () => [].concat(\"Injected content\" ?? [])", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("contract-shell", StringComparison.Ordinal), script);

        var injectedComponent = fixture.Binding.Components.Single(static component =>
            component.ComponentSymbol.Name == "InjectedShell");
        var injectedClosure = BuildClosure(fixture, "InjectedShell");
        var injectedArtifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            injectedComponent,
            injectedClosure);
        var injectedScript = injectedArtifact.ModuleText.ReplaceLineEndings("\n");
        StringAssert.Contains(injectedScript, "slots[\"injected-content\"]", StringComparison.Ordinal);
        Assert.IsFalse(injectedScript.Contains("slots.injected-content", StringComparison.Ordinal), injectedScript);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectForwardsConditionalRenderFragmentSlot()
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
                    public RenderFragment? Logo { get; set; }
                }

                [ECMAScriptModule("./components/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? Logo { get; set; }

                    private bool SuppressLogo => false;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        var logo = Logo;
                        var forwardedLogo = SuppressLogo ? null : logo;

                        builder.OpenComponent<Child>(0);
                        builder.AddComponentParameter(1, "Logo", forwardedLogo);
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

        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "function $renderDirect() {", StringComparison.Ordinal);
        StringAssert.Contains(script, "function createCounterSetupScope(slots)", StringComparison.Ordinal);
        StringAssert.Contains(
            script,
            "...suppressLogo() ? {} : typeof slots.logo === \"function\" ? { logo: () => [].concat(slots.logo() ?? []) } : {}",
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
                export function defineComponent(options) {
                    return options;
                }
                export function reactive(value) {
                    return value;
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-conditional-slot-forward-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";
                import { h } from "vue";

                import component from "./components/parent.mjs";

                test("conditional RenderFragment local projects slot presence", () => {
                    const child = component.setup({}, { slots: { logo: () => h("span", null, ["brand"]) } })();
                    const withoutLogo = component.setup({}, { slots: {} })();
                    const logo = child.children.logo();

                    assert.equal(child.name.name, "Child");
                    assert.equal(logo[0].name, "span");
                    assert.deepEqual(logo[0].children, ["brand"]);
                    assert.equal("logo" in withoutLogo.children, false);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectOmitsMissingForwardedRenderFragmentSlot()
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
                    public RenderFragment? Logo { get; set; }
                }

                [ECMAScriptModule("./components/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? Logo { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Child>(0);
                        builder.AddComponentParameter(1, "Logo", Logo);
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

        StringAssert.Contains(
            script,
            "...typeof slots.logo === \"function\" ? { logo: () => [].concat(slots.logo() ?? []) } : {}",
            StringComparison.Ordinal);
        Assert.IsFalse(
            script.Contains("logo: () => typeof slots.logo", StringComparison.Ordinal),
            script);

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
                export function defineComponent(options) {
                    return options;
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-optional-slot-forward-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";
                import { h } from "vue";

                import component from "./components/parent.mjs";

                test("missing forwarded RenderFragment omits the target Vue slot", () => {
                    const withLogo = component.setup({}, { slots: { logo: () => h("span", null, ["brand"]) } })();
                    const withoutLogo = component.setup({}, { slots: {} })();

                    assert.equal(typeof withLogo.children.logo, "function");
                    assert.equal(withLogo.children.logo()[0].name, "span");
                    assert.equal("logo" in withoutLogo.children, false);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectLowersRenderFragmentHelperResult()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/helper-fragment")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    private string?[] Items => ["one", null, "two"];

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "ul");
                        foreach (var item in Items)
                        {
                            builder.AddContent(1, RenderItem(item));
                        }
                        builder.CloseElement();
                    }

                    private RenderFragment RenderItem(string? entry) => child =>
                    {
                        if (entry is not { } text)
                        {
                            return;
                        }

                        child.OpenElement(0, "li");
                        child.AddContent(1, text);
                        child.CloseElement();
                    };
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "function $renderDirect() {", StringComparison.Ordinal);
        StringAssert.Contains(script, "let text;", StringComparison.Ordinal);
        StringAssert.Contains(script, "typeof item === \"string\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "text = item", StringComparison.Ordinal);

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
                export function defineComponent(options) {
                    return options;
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                export function reactive(value) {
                    return value;
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-helper-fragment-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/helper-fragment.mjs";

                test("RenderFragment helper result lowers directly inside foreach", () => {
                    const render = component.setup({}, { slots: {} });
                    const vnode = render();

                    assert.equal(vnode.name, "ul");
                    assert.deepEqual(vnode.children[0].filter(Boolean).map((child) => child.children[0]), ["one", "two"]);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectLowersObjectCarriedRenderFragmentProperty()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/object-carried-fragment")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment? Extra { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        var state = BuildState();
                        if (state.HasExtra)
                        {
                            builder.OpenElement(0, "section");
                            builder.AddContent(1, state.Extra);
                            builder.CloseElement();
                        }
                    }

                    private State BuildState()
                    {
                        var extra = Extra;
                        return new(extra, extra is not null);
                    }

                    private sealed class State
                    {
                        public State(RenderFragment? extra, bool hasExtra)
                        {
                            Extra = extra;
                            HasExtra = hasExtra;
                        }

                        public RenderFragment? Extra { get; }

                        public bool HasExtra { get; }
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

        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
        StringAssert.Contains(script, "function $renderDirect() {", StringComparison.Ordinal);
        StringAssert.Contains(script, "slots.extra", StringComparison.Ordinal);

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
                export function defineComponent(options) {
                    return options;
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                export function reactive(value) {
                    return value;
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-object-carried-fragment-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/object-carried-fragment.mjs";

                test("object-carried RenderFragment property lowers to the original Vue slot", () => {
                    const render = component.setup({}, {
                        slots: {
                            extra: () => ({ name: "strong", props: null, children: ["tools"] })
                        }
                    });
                    const vnode = render();

                    assert.equal(vnode.name, "section");
                    assert.equal(vnode.children[0].name, "strong");
                    assert.deepEqual(vnode.children[0].children, ["tools"]);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectScopesSiblingRenderFragmentHelperLocals()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/sibling-helper-locals")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "div");
                        builder.AddContent(1, RenderFirst(true));
                        builder.AddContent(2, RenderSecond(true));
                        builder.CloseElement();
                    }

                    private RenderFragment RenderFirst(bool enabled) => child =>
                    {
                        var label = "first";
                        var flag = enabled;
                        if (flag)
                        {
                            child.OpenElement(0, "span");
                            child.AddContent(1, label);
                            child.CloseElement();
                        }
                    };

                    private RenderFragment RenderSecond(bool enabled) => child =>
                    {
                        var label = "second";
                        var flag = enabled;
                        if (flag)
                        {
                            child.OpenElement(0, "strong");
                            child.AddContent(1, label);
                            child.CloseElement();
                        }
                    };
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "function $renderDirect() {", StringComparison.Ordinal);

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
                export function defineComponent(options) {
                    return options;
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-sibling-helper-locals-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/sibling-helper-locals.mjs";

                test("sibling RenderFragment helper local names stay scoped", () => {
                    const render = component.setup({}, { slots: {} });
                    const vnode = render();

                    assert.equal(vnode.name, "div");
                    assert.equal(vnode.children[0].name, "span");
                    assert.deepEqual(vnode.children[0].children, ["first"]);
                    assert.equal(vnode.children[1].name, "strong");
                    assert.deepEqual(vnode.children[1].children, ["second"]);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectGuardsSiblingsAfterOutputReturnBranch()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/output-return-branches")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "div");
                        builder.AddContent(1, RenderItem(true, true));
                        builder.AddContent(2, RenderItem(false, true));
                        builder.AddContent(3, RenderItem(false, false));
                        builder.CloseElement();
                    }

                    private RenderFragment RenderItem(bool route, bool href) => child =>
                    {
                        if (route)
                        {
                            child.OpenElement(0, "router-link");
                            child.AddContent(1, "route");
                            child.CloseElement();
                            return;
                        }

                        if (href)
                        {
                            child.OpenElement(10, "a");
                            child.AddContent(11, "href");
                            child.CloseElement();
                            return;
                        }

                        child.OpenElement(20, "span");
                        child.AddContent(21, "plain");
                        child.CloseElement();
                    };
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "function $renderDirect() {", StringComparison.Ordinal);

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
                export function defineComponent(options) {
                    return options;
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-output-return-branches-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/output-return-branches.mjs";

                test("output branches ending in return guard later sibling output", () => {
                    const render = component.setup({}, { slots: {} });
                    const vnode = render();
                    const rendered = vnode.children.map((child) => child.children.filter(Boolean)[0]);

                    assert.equal(vnode.name, "div");
                    assert.deepEqual(rendered.map((child) => child.name), ["router-link", "a", "span"]);
                    assert.deepEqual(rendered.map((child) => child.children[0]), ["route", "href", "plain"]);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_ReferencedVueSfcComponentDeclaresFrontendAsset()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/widgets/LocalCard.vue")]
                public sealed class LocalCard : ComponentBase, IVueComponent
                {
                }

                [ECMAScriptModule("./components/pages/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<LocalCard>(0);
                        builder.CloseComponent();
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture, "Counter");
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Binding.Components.Single(component => component.ComponentSymbol.Name == "Counter"),
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "from \"../widgets/LocalCard.vue.mjs\";", StringComparison.Ordinal);
        Assert.HasCount(1, artifact.FrontendAssets);
        Assert.AreEqual("components/widgets/LocalCard.vue", artifact.FrontendAssets[0].SourcePath);
        Assert.AreEqual("components/widgets/LocalCard.vue", artifact.FrontendAssets[0].ArtifactPath);
        Assert.AreEqual("vue-sfc", artifact.FrontendAssets[0].Kind);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_DynamicTypeOpenComponentFailsWithDiagnostic()
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
                [ECMAScriptModule("./components/child")]
                public sealed class Child : ComponentBase, IVueComponent
                {
                }

                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public Type ChildType { get; set; } = typeof(Child);

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent(0, ChildType);
                        builder.CloseComponent();
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture);

        OperationTransformationException? exception = null;
        try
        {
            _ = await RazorSgVueComponentModuleBuilder.BuildAsync(
                fixture.Binding,
                fixture.Component,
                closure);
        }
        catch (OperationTransformationException ex)
        {
            exception = ex;
        }

        Assert.IsNotNull(exception);
        StringAssert.Contains(exception.Message, "Dynamic Type OpenComponent", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "render-context v1", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeAppliesMetadataAndBulkAttributeSurface()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using Microsoft.AspNetCore.Components.Web;
            using System.Collections.Generic;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/child")]
                public sealed class Child : ComponentBase, IVueComponent
                {
                }

                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "form");
                        builder.AddMultipleAttributes(1, new Dictionary<string, object>
                        {
                            ["onsubmit"] = OnSubmit,
                            ["class"] = "checkout"
                        });
                        builder.SetKey("form-key");
                        builder.SetUpdatesAttributeName("value");
                        builder.AddNamedEvent("onsubmit", "checkout");
                        builder.AddEventPreventDefaultAttribute(2, "onsubmit", true);
                        builder.AddEventStopPropagationAttribute(3, "onsubmit", true);
                        builder.OpenRegion(4);
                        builder.AddContent(5, "region");
                        builder.CloseRegion();
                        builder.CloseElement();

                        builder.OpenComponent(6, typeof(Child));
                        builder.AddMultipleAttributes(7, new Dictionary<string, object>
                        {
                            ["Title"] = "bulk",
                            ["Count"] = 2
                        });
                        builder.SetAttributeValue(8, 3);
                        builder.AddComponentRenderMode(RenderMode(null));
                        builder.CloseComponent();
                    }

                    private string OnSubmit() => "handled";

                    private IComponentRenderMode RenderMode(IComponentRenderMode mode) => mode;
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        Assert.IsFalse(script.Contains("createRenderContext", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains(".addMultipleAttributes(", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains(".addComponentRenderMode(", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "class: \"checkout\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "key: \"form-key\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "event?.preventDefault?.();", StringComparison.Ordinal);
        StringAssert.Contains(script, "event?.stopPropagation?.();", StringComparison.Ordinal);
        StringAssert.Contains(script, "{ title: \"bulk\", count: 3 }", StringComparison.Ordinal);

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
            var testFile = Path.Combine(tempRoot, "module-metadata-bulk-attributes-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/counter.mjs";

                test("metadata and bulk attributes materialize through the direct render function", () => {
                    const render = component.setup({}, { slots: {} });
                    const vnode = render();

                    assert.equal(typeof vnode.name, "symbol");
                    const form = vnode.children[0];
                    const child = vnode.children[1];

                    assert.equal(form.name, "form");
                    assert.equal(form.props.class, "checkout");
                    assert.equal(form.props.key, "form-key");
                    assert.equal(form.children[0], "region");

                    let prevented = false;
                    let stopped = false;
                    assert.equal(form.props.onSubmit({ preventDefault: () => prevented = true, stopPropagation: () => stopped = true }), "handled");
                    assert.equal(prevented, true);
                    assert.equal(stopped, true);

                    assert.equal(child.name.name, "Child");
                    assert.deepEqual(child.props, { title: "bulk", count: 3 });
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectAvoidsLocalAndComponentMemberNameCollisions()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/local-member-name-collision")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    private string[] Columns { get; } = ["name"];

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "div");
                        foreach (var column in Columns)
                        {
                            var columnIsSorted = ColumnIsSorted(column);
                            builder.OpenElement(1, "span");
                            builder.AddContent(2, columnIsSorted ? "sorted" : "plain");
                            builder.CloseElement();
                        }
                        builder.CloseElement();
                    }

                    private bool ColumnIsSorted(string column)
                        => column == "name";
                }
            }
            """);
        var closure = BuildClosure(fixture);
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        Assert.IsFalse(script.Contains("const columnIsSorted = columnIsSorted(", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "function ColumnIsSorted(column)", StringComparison.Ordinal);
        StringAssert.Contains(script, "const columnIsSorted = ColumnIsSorted(column);", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);

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
                export function defineComponent(options) {
                    return options;
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                export function reactive(value) {
                    return value;
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-local-member-name-collision-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/local-member-name-collision.mjs";

                test("direct render local names do not shadow current component helpers", () => {
                    const render = component.setup({}, { slots: {} });
                    const vnode = render();

                    assert.equal(vnode.name, "div");
                    assert.equal(vnode.children[0][0].name, "span");
                    assert.deepEqual(vnode.children[0][0].children, ["sorted"]);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectFoldsConstFieldConditions()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/const-field-condition")]
                public partial class ConstFieldCondition : ComponentBase, IVueComponent
                {
                    private const string ReleasesKey = "operations.releases";
                    private string selectedKey = ReleasesKey;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        if (selectedKey == ReleasesKey)
                        {
                            builder.OpenElement(0, "strong");
                            builder.AddContent(1, "release page");
                            builder.CloseElement();
                            return;
                        }

                        builder.OpenElement(2, "span");
                        builder.AddContent(3, "dashboard");
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

        Assert.IsFalse(script.Contains("from \"./const-field-condition.mjs\"", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "state.selectedKey === \"operations.releases\"", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("state.selectedKey === releasesKey", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);

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
                export function defineComponent(options) {
                    return options;
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                export function reactive(value) {
                    return value;
                }
                """);
            var vueStubPath = Path.Combine(tempRoot, "node_modules", "vue", "index.mjs");
            var vueStub = System.IO.File.ReadAllText(vueStubPath);
            if (!vueStub.Contains("export const Fragment", StringComparison.Ordinal))
            {
                WriteFile(
                    vueStubPath,
                    "export const Fragment = Symbol(\"Fragment\");" + Environment.NewLine + vueStub);
            }

            var testFile = Path.Combine(tempRoot, "module-const-field-condition-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/const-field-condition.mjs";

                test("direct render folds current component const field conditions", () => {
                    const render = component.setup({}, { slots: {} });
                    const vnode = render();

                    assert.equal(typeof vnode.name, "symbol");
                    assert.equal(vnode.children[0].name, "strong");
                    assert.deepEqual(vnode.children[0].children, ["release page"]);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectLowersBulkAttributes()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using System;
            using System.Collections.Generic;
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/child")]
                public sealed class Child : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string Title { get; set; } = "";

                    [Parameter]
                    public int Count { get; set; }
                }

                [ECMAScriptModule("./components/direct-bulk")]
                public partial class DirectBulk : ComponentBase, IVueComponent
                {
                    private string submitted = "idle";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "form");
                        builder.AddMultipleAttributes(1, new Dictionary<string, object>
                        {
                            ["class"] = "checkout",
                            ["onsubmit"] = (Action)Submit
                        });
                        builder.AddContent(2, submitted);
                        builder.CloseElement();

                        builder.OpenComponent(3, typeof(Child));
                        builder.AddMultipleAttributes(4, new Dictionary<string, object>
                        {
                            ["Title"] = "bulk",
                            ["Count"] = 2
                        });
                        builder.SetAttributeValue(5, 3);
                        builder.CloseComponent();
                    }

                    private void Submit()
                    {
                        submitted = "submitted";
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

        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.finish();", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains(".addMultipleAttributes(", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "h(Fragment, null, [h(\"form\", { class: \"checkout\", onSubmit: submit }, [state.submitted])", StringComparison.Ordinal);
        StringAssert.Contains(script, "{ title: \"bulk\", count: 3 })", StringComparison.Ordinal);

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
                export function defineComponent(options) {
                    return options;
                }
                export function reactive(value) {
                    return value;
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-direct-bulk-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/direct-bulk.mjs";

                test("bulk attributes lower directly", () => {
                    const render = component.setup({}, { slots: {} });
                    const first = render();
                    const form = first.children[0];
                    const child = first.children[1];

                    assert.equal(typeof first.name, "symbol");
                    assert.equal(form.name, "form");
                    assert.equal(form.props.class, "checkout");
                    assert.equal(typeof form.props.onSubmit, "function");
                    assert.equal(form.children[0], "idle");
                    assert.deepEqual(child.props, { title: "bulk", count: 3 });

                    form.props.onSubmit();
                    assert.equal(render().children[0].children[0], "submitted");
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeDirectLowersElementMetadataAndBind()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using Microsoft.AspNetCore.Components.Web;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/direct-bind")]
                public partial class DirectBind : ComponentBase, IVueComponent
                {
                    private string text = "initial";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "input");
                        builder.AddAttribute(1, "class", "before");
                        builder.SetAttributeValue(2, "after");
                        builder.SetKey("input-key");
                        builder.AddAttribute(3, "value", text);
                        builder.AddAttribute(4, "onchange", EventCallback.Factory.CreateBinder<string>(this, __value => text = __value, text));
                        builder.SetUpdatesAttributeName("value");
                        builder.AddNamedEvent("onchange", "changed");
                        builder.AddEventPreventDefaultAttribute(5, "onchange", true);
                        builder.AddEventStopPropagationAttribute(6, "onchange", true);
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

        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.finish();", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains(".addNamedEvent(", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "key: \"input-key\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "class: \"after\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "onChange: (event, ...args) => {", StringComparison.Ordinal);
        StringAssert.Contains(script, "event?.preventDefault?.();", StringComparison.Ordinal);
        StringAssert.Contains(script, "event?.stopPropagation?.();", StringComparison.Ordinal);
        StringAssert.Contains(script, "return ((eventOrValue, ...args) =>", StringComparison.Ordinal);

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
                export function defineComponent(options) {
                    return options;
                }
                export function reactive(value) {
                    return value;
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                """);
            var testFile = Path.Combine(tempRoot, "module-direct-bind-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/direct-bind.mjs";

                test("element metadata and DOM bind lower directly", () => {
                    const render = component.setup({}, { slots: {} });
                    const first = render();

                    assert.equal(first.name, "input");
                    assert.equal(first.props.class, "after");
                    assert.equal(first.props.key, "input-key");
                    assert.equal(first.props.value, "initial");
                    assert.equal(typeof first.props.onChange, "function");

                    let prevented = false;
                    let stopped = false;
                    first.props.onChange({
                        target: { value: "updated" },
                        preventDefault: () => prevented = true,
                        stopPropagation: () => stopped = true
                    });

                    assert.equal(prevented, true);
                    assert.equal(stopped, true);
                    assert.equal(render().props.value, "updated");
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
    }

    [TestMethod]
    public async Task EcmaScriptStyle_BuildVueComponentModule_UsesGeneratedClassAsOrdinaryStringProp()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using ECMAScript.Style;
            using static ECMAScript.Style.css;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/styled-button")]
                public partial class StyledButton : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        var className = css.style(new CssRule
                        {
                            Display = inlineFlex,
                            Color = color("white"),
                            BackgroundColor = hex("1769aa")
                        });
                        builder.OpenElement(0, "button");
                        builder.AddAttribute(1, "class", className);
                        builder.AddContent(2, "Save");
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

        StringAssert.Contains(script, "from \"style.mjs\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "const className = style({", StringComparison.Ordinal);
        StringAssert.Contains(script, "display: inlineFlex", StringComparison.Ordinal);
        StringAssert.Contains(script, "color: color(\"white\")", StringComparison.Ordinal);
        StringAssert.Contains(script, "\"background-color\": hex(\"1769aa\")", StringComparison.Ordinal);
        StringAssert.Contains(script, "class: className", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("VueClassValue", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("EcmaScriptStyle", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_BrowserMountsAndUpdatesRenderContextDom()
    {
        var browserPath = ResolveBrowserExecutable();
        if (browserPath is null)
            Assert.Inconclusive("RazorVue generated module browser smoke requires Microsoft Edge, Chrome, or Chromium.");

        var fixture = CreateManualGeneratedFixture(
            """
            using System;
            using System.Collections.Generic;
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using Microsoft.AspNetCore.Components.Web;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/child")]
                public partial class Child : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string Title { get; set; } = "";

                    [Parameter]
                    public EventCallback<string> OnValueChanged { get; set; }

                    [Parameter]
                    public RenderFragment<string>? Detail { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "button");
                        builder.AddAttribute(1, "onclick", (Action)Notify);
                        builder.AddContent(2, "Child: ");
                        builder.AddContent(3, Title);
                        builder.CloseElement();
                        builder.AddContent(4, Detail, Title);
                    }

                    private void Notify()
                    {
                        _ = OnValueChanged.InvokeAsync("updated");
                    }
                }

                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent, IDisposable
                {
                    private string last = "initial";
                    private string captured = "none";
                    private string submitted = "idle";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment<string> detail = value => childBuilder =>
                        {
                            childBuilder.OpenElement(0, "span");
                            childBuilder.AddContent(1, "Slot: ");
                            childBuilder.AddContent(2, value);
                            childBuilder.CloseElement();
                        };

                        builder.OpenElement(0, "input");
                        builder.AddAttribute(1, "value", last);
                        builder.AddAttribute(2, "onchange", EventCallback.Factory.CreateBinder(this, __value => last = __value, last));
                        builder.SetUpdatesAttributeName("value");
                        builder.CloseElement();

                        builder.OpenElement(3, "form");
                        builder.AddMultipleAttributes(4, new Dictionary<string, object>
                        {
                            ["class"] = "checkout",
                            ["onsubmit"] = (Action)Submit
                        });
                        builder.SetKey("checkout-key");
                        builder.AddNamedEvent("onsubmit", "checkout");
                        builder.AddEventPreventDefaultAttribute(5, "onsubmit", true);
                        builder.AddEventStopPropagationAttribute(6, "onsubmit", true);
                        builder.OpenRegion(7);
                        builder.AddContent(8, "Region: ");
                        builder.AddContent(9, submitted);
                        builder.CloseRegion();
                        builder.CloseElement();

                        builder.OpenElement(10, "p");
                        builder.AddElementReferenceCapture(11, value => captured = "element");
                        builder.AddContent(12, "Parent: ");
                        builder.AddContent(13, last);
                        builder.AddContent(14, " Ref: ");
                        builder.AddContent(15, captured);
                        builder.CloseElement();

                        builder.OpenComponent<Child>(16);
                        builder.AddComponentParameter(17, "Title", last);
                        builder.AddComponentParameter(18, "OnValueChanged", EventCallback.Factory.Create<string>(this, HandleValueChanged));
                        builder.AddComponentParameter(19, "Detail", detail);
                        builder.AddComponentReferenceCapture(20, value => captured = value is null ? "component:null" : "component:ready");
                        builder.CloseComponent();
                    }

                    private void Submit()
                    {
                        submitted = "submitted";
                    }

                    private void HandleValueChanged(string value)
                    {
                        last = value;
                    }

                    public void Dispose()
                    {
                        last = "disposed";
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
        var parent = fixture.Binding.Components.Single(component => component.ComponentSymbol.Name == "Counter");
        var parentClosure = BuildClosure(fixture, parent.ComponentSymbol.Name);
        var parentArtifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            parent,
            parentClosure);

        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            "Jazor.RazorVue.Sg.Test",
            Guid.NewGuid().ToString("N"));
        try
        {
            WriteFile(Path.Combine(tempRoot, childArtifact.RelativePath), childArtifact.ModuleText);
            WriteFile(Path.Combine(tempRoot, parentArtifact.RelativePath), parentArtifact.ModuleText);
            WriteFile(
                Path.Combine(tempRoot, "index.html"),
                """
                <!doctype html>
                <html>
                <head>
                  <meta charset="utf-8">
                  <title>RazorVue browser render-context smoke</title>
                  <script type="importmap">
                  {
                    "imports": {
                      "vue": "/node_modules/vue/index.mjs",
                      "@jazor/vue-runtime/render-context.mjs": "/node_modules/@jazor/vue-runtime/render-context.mjs"
                    }
                  }
                  </script>
                </head>
                <body>
                  <main id="app"></main>
                  <script type="module" src="/browser-smoke.mjs"></script>
                </body>
                </html>
                """);
            WriteFile(
                Path.Combine(tempRoot, "browser-smoke.mjs"),
                """
                import { __runUnmounted } from "vue";
                import component from "/components/counter.mjs";

                const app = document.querySelector("#app");
                const render = component.setup({}, { slots: {} });

                function mount() {
                  app.replaceChildren(render());
                }

                mount();
                const before = app.textContent;
                const inputBefore = app.querySelector("input").value;
                const input = app.querySelector("input");
                input.value = "typed";
                input.dispatchEvent(new Event("change", { bubbles: true }));
                mount();
                const afterInput = app.textContent;
                const inputAfter = app.querySelector("input").value;
                let bubbledSubmit = false;
                app.addEventListener("submit", () => bubbledSubmit = true);
                const submitEvent = new Event("submit", { bubbles: true, cancelable: true });
                app.querySelector("form").dispatchEvent(submitEvent);
                mount();
                const afterSubmit = app.textContent;
                app.querySelector("button").click();
                mount();
                const after = app.textContent;
                __runUnmounted();
                mount();
                const afterUnmount = app.textContent;

                globalThis.__razorVueBrowserSmoke = {
                  before,
                  inputBefore,
                  afterInput,
                  inputAfter,
                  afterSubmit,
                  after,
                  afterUnmount,
                  formClass: app.querySelector("form").getAttribute("class"),
                  submitDefaultPrevented: submitEvent.defaultPrevented,
                  bubbledSubmit,
                  inputCount: app.querySelectorAll("input").length,
                  formCount: app.querySelectorAll("form.checkout").length,
                  buttonCount: app.querySelectorAll("button").length
                };
                """);
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "vue", "package.json"),
                """{"type":"module","exports":"./index.mjs"}""");
            WriteFile(
                Path.Combine(tempRoot, "node_modules", "vue", "index.mjs"),
                """
                export const Fragment = Symbol("Fragment");
                export function createStaticVNode(html) {
                  const template = document.createElement("template");
                  template.innerHTML = html ?? "";
                  return template.content.cloneNode(true);
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
                const unmounted = [];
                export function onUnmounted(callback) {
                  unmounted.push(callback);
                }
                export function __runUnmounted() {
                  for (const callback of unmounted) {
                    callback();
                  }
                }
                export function h(type, props, children) {
                  if (type === Fragment) {
                    const fragment = document.createDocumentFragment();
                    appendChildren(fragment, children);
                    return fragment;
                  }

                  if (type !== null && typeof type === "object" && typeof type.setup === "function") {
                    const render = type.setup(props ?? {}, { slots: children ?? {} });
                    if (props !== null && typeof props.ref === "function") {
                      props.ref({ type });
                    }
                    return render();
                  }

                  const element = document.createElement(type);
                  for (const [name, value] of Object.entries(props ?? {})) {
                    if (name === "ref" && typeof value === "function") {
                      value(element);
                    } else if (/^on[A-Z]/.test(name) && typeof value === "function") {
                      element.addEventListener(name.slice(2).toLowerCase(), value);
                    } else if (name !== "key" && value !== null && value !== undefined) {
                      element.setAttribute(name, String(value));
                    }
                  }

                  appendChildren(element, children);
                  return element;
                }
                function appendChildren(parent, children) {
                  const values = Array.isArray(children) ? children : [children];
                  for (const child of values) {
                    if (child === null || child === undefined) {
                      continue;
                    }
                    parent.append(child instanceof Node ? child : document.createTextNode(String(child)));
                  }
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

            var testFile = Path.Combine(tempRoot, "browser-render-context-smoke.test.mjs");
            WriteFile(
                testFile,
                $$"""
                import assert from "node:assert/strict";
                import { createServer } from "node:http";
                import { readFile } from "node:fs/promises";
                import { extname, join, normalize, resolve, sep } from "node:path";
                import { spawn } from "node:child_process";
                import test from "node:test";

                const root = {{System.Text.Json.JsonSerializer.Serialize(tempRoot)}};
                const browserPath = {{System.Text.Json.JsonSerializer.Serialize(browserPath)}};

                test("generated render-context module mounts and updates in a browser", async () => {
                  const server = await startServer(root);
                  const browser = await startBrowser(browserPath);
                  try {
                    const page = await connectToPage(browser.port);
                    await page.send("Page.enable");
                    await page.send("Runtime.enable");
                    await page.navigate(`http://127.0.0.1:${server.port}/index.html`);
                    const result = await page.waitForSmoke();
                    assert.deepEqual(result, {
                      before: "Region: idleParent: initial Ref: noneChild: initialSlot: initial",
                      inputBefore: "initial",
                      afterInput: "Region: idleParent: typed Ref: component:readyChild: typedSlot: typed",
                      inputAfter: "typed",
                      afterSubmit: "Region: submittedParent: typed Ref: component:readyChild: typedSlot: typed",
                      after: "Region: submittedParent: updated Ref: component:readyChild: updatedSlot: updated",
                      afterUnmount: "Region: submittedParent: disposed Ref: component:readyChild: disposedSlot: disposed",
                      formClass: "checkout",
                      submitDefaultPrevented: true,
                      bubbledSubmit: false,
                      inputCount: 1,
                      formCount: 1,
                      buttonCount: 1
                    });
                  } finally {
                    await browser.dispose();
                    await server.dispose();
                  }
                });

                async function startServer(root) {
                  const server = createServer(async (request, response) => {
                    const url = new URL(request.url ?? "/", "http://127.0.0.1");
                    const relativePath = url.pathname === "/" ? "index.html" : decodeURIComponent(url.pathname.slice(1));
                    const filePath = normalize(resolve(root, relativePath));
                    const normalizedRoot = normalize(resolve(root));
                    const rootPrefix = normalizedRoot.endsWith(sep) ? normalizedRoot : `${normalizedRoot}${sep}`;
                    if (filePath !== normalizedRoot && !filePath.startsWith(rootPrefix)) {
                      response.writeHead(403);
                      response.end("Forbidden");
                      return;
                    }

                    try {
                      const contents = await readFile(filePath);
                      response.writeHead(200, { "content-type": contentType(filePath), "cache-control": "no-store" });
                      response.end(contents);
                    } catch {
                      response.writeHead(404);
                      response.end("Not Found");
                    }
                  });

                  await new Promise((resolvePromise) => server.listen(0, "127.0.0.1", resolvePromise));
                  return {
                    port: server.address().port,
                    dispose: () => new Promise((resolvePromise) => server.close(resolvePromise))
                  };
                }

                function contentType(path) {
                  switch (extname(path)) {
                    case ".html": return "text/html; charset=utf-8";
                    case ".mjs":
                    case ".js": return "text/javascript; charset=utf-8";
                    case ".json": return "application/json; charset=utf-8";
                    default: return "application/octet-stream";
                  }
                }

                async function startBrowser(browserPath) {
                  const port = await reservePort();
                  const userDataDir = join(root, ".browser-profile");
                  const process = spawn(browserPath, [
                    "--headless=new",
                    "--disable-gpu",
                    "--disable-dev-shm-usage",
                    "--no-first-run",
                    "--no-default-browser-check",
                    "--no-sandbox",
                    `--remote-debugging-port=${port}`,
                    `--user-data-dir=${userDataDir}`,
                    "about:blank"
                  ], { stdio: "ignore" });

                  let exited = false;
                  const exitPromise = new Promise((resolvePromise) => process.once("exit", resolvePromise));
                  process.once("exit", () => exited = true);
                  const deadline = Date.now() + 15000;
                  while (Date.now() < deadline) {
                    if (exited) {
                      throw new Error("Browser exited before CDP was ready.");
                    }
                    try {
                      const response = await fetch(`http://127.0.0.1:${port}/json/list`, { cache: "no-store" });
                      if (response.ok) {
                        return {
                          port,
                          dispose: async () => {
                            if (!exited) {
                              process.kill("SIGKILL");
                            }
                            await exitPromise;
                          }
                        };
                      }
                    } catch {
                    }
                    await delay(100);
                  }

                  if (!exited) {
                    process.kill("SIGKILL");
                    await exitPromise;
                  }
                  throw new Error("Timed out waiting for browser CDP.");
                }

                async function reservePort() {
                  const server = createServer();
                  await new Promise((resolvePromise) => server.listen(0, "127.0.0.1", resolvePromise));
                  const port = server.address().port;
                  await new Promise((resolvePromise) => server.close(resolvePromise));
                  return port;
                }

                async function connectToPage(port) {
                  const targets = await fetch(`http://127.0.0.1:${port}/json/list`, { cache: "no-store" })
                    .then((response) => response.json());
                  const target = targets.find((candidate) => candidate.type === "page" && candidate.webSocketDebuggerUrl);
                  if (!target) {
                    throw new Error("Browser CDP did not expose a page target.");
                  }

                  const socket = new WebSocket(target.webSocketDebuggerUrl);
                  await new Promise((resolvePromise, reject) => {
                    socket.addEventListener("open", resolvePromise, { once: true });
                    socket.addEventListener("error", () => reject(new Error("CDP websocket failed to open.")), { once: true });
                  });
                  return new Page(socket);
                }

                class Page {
                  nextId = 1;
                  pending = new Map();
                  loadResolvers = [];

                  constructor(socket) {
                    this.socket = socket;
                    socket.addEventListener("message", (event) => this.handle(JSON.parse(String(event.data))));
                  }

                  send(method, params = {}) {
                    const id = this.nextId++;
                    const promise = new Promise((resolvePromise, reject) => this.pending.set(id, { resolve: resolvePromise, reject }));
                    this.socket.send(JSON.stringify({ id, method, params }));
                    return promise;
                  }

                  async navigate(url) {
                    const loaded = new Promise((resolvePromise) => this.loadResolvers.push(resolvePromise));
                    await this.send("Page.navigate", { url });
                    await loaded;
                  }

                  async evaluate(expression) {
                    const response = await this.send("Runtime.evaluate", { expression, returnByValue: true, awaitPromise: true });
                    if (response.exceptionDetails) {
                      throw new Error(response.exceptionDetails.exception?.description ?? response.exceptionDetails.text ?? "Runtime.evaluate failed.");
                    }
                    return response.result?.value;
                  }

                  async waitForSmoke() {
                    const deadline = Date.now() + 10000;
                    while (Date.now() < deadline) {
                      const result = await this.evaluate("globalThis.__razorVueBrowserSmoke ?? null");
                      if (result !== null) {
                        return result;
                      }
                      await delay(100);
                    }
                    const body = await this.evaluate("document.body ? document.body.textContent : ''");
                    throw new Error(`Timed out waiting for browser smoke result. Body: ${body}`);
                  }

                  handle(message) {
                    if (message.id !== undefined) {
                      const pending = this.pending.get(message.id);
                      if (pending === undefined) {
                        return;
                      }
                      this.pending.delete(message.id);
                      if (message.error) {
                        pending.reject(new Error(message.error.message ?? JSON.stringify(message.error)));
                      } else {
                        pending.resolve(message.result);
                      }
                      return;
                    }
                    if (message.method === "Page.loadEventFired") {
                      for (const resolvePromise of this.loadResolvers.splice(0)) {
                        resolvePromise();
                      }
                    }
                  }
                }

                function delay(ms) {
                  return new Promise((resolvePromise) => setTimeout(resolvePromise, ms));
                }
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            DeleteDirectoryWithRetry(tempRoot);
        }
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
        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "function createCounterSetupScope(props) {", StringComparison.Ordinal);
        StringAssert.Contains(script, "const state = reactive({", StringComparison.Ordinal);
        StringAssert.Contains(script, "count: seed()", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("function buildRenderTree(builder)", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.addAttribute(\"onclick\", increment);", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.addContent(props.title);", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.addContent(state.count);", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "function $renderDirect() {", StringComparison.Ordinal);
        StringAssert.Contains(script, "return h(\"button\", { onClick: increment }, [props.title, state.count]);", StringComparison.Ordinal);
        StringAssert.Contains(script, "return { $renderDirect };", StringComparison.Ordinal);
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
        Assert.IsFalse(script.Contains("const builder = createRenderContext(h);", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("return builder.finish();", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "return scope.$renderDirect();", StringComparison.Ordinal);
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
            using System.Threading.Tasks;
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
    public async Task BuildVueComponentModule_RuntimeRunsLifecycleHooksAcrossMountUpdateAndUnmount()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent, IDisposable
                {
                    [Parameter]
                    public string Title { get; set; } = "";

                    private string log = "";

                    protected override void OnInitialized()
                    {
                        log += "init|";
                    }

                    protected override void OnParametersSet()
                    {
                        log += "params:" + Title + "|";
                    }

                    protected override void OnAfterRender(bool firstRender)
                    {
                        log += firstRender ? "after:first|" : "after:update|";
                    }

                    public void Dispose()
                    {
                        log += "dispose|";
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "p");
                        builder.AddContent(1, log);
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

        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.finish();", StringComparison.Ordinal), script);

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
                const mounted = [];
                const updated = [];
                const unmounted = [];
                const watchers = [];

                export function createStaticVNode(html, count) {
                    return { html, count };
                }
                export function defineComponent(options) {
                    return options;
                }
                export function reactive(value) {
                    return value;
                }
                export function watch(source, callback) {
                    watchers.push(callback);
                    return () => {};
                }
                export function onMounted(callback) {
                    mounted.push(callback);
                }
                export function onUpdated(callback) {
                    updated.push(callback);
                }
                export function onUnmounted(callback) {
                    unmounted.push(callback);
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                export function __runMounted() {
                    for (const callback of mounted) {
                        callback();
                    }
                }
                export function __runUpdated() {
                    for (const callback of updated) {
                        callback();
                    }
                }
                export function __runUnmounted() {
                    for (const callback of unmounted) {
                        callback();
                    }
                }
                export function __runWatchers() {
                    for (const callback of watchers) {
                        callback();
                    }
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
            var testFile = Path.Combine(tempRoot, "module-lifecycle-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";
                import { __runMounted, __runUpdated, __runUnmounted, __runWatchers } from "vue";

                import component from "./components/counter.mjs";

                test("lifecycle hooks run across mount, prop update, and unmount", () => {
                    const props = { title: "one" };
                    const render = component.setup(props, { slots: {} });

                    assert.deepEqual(render().children, ["init|params:one|"]);

                    __runMounted();
                    assert.deepEqual(render().children, ["init|params:one|after:first|"]);

                    props.title = "two";
                    __runWatchers();
                    __runUpdated();
                    assert.deepEqual(render().children, ["init|params:one|after:first|params:two|after:update|"]);

                    __runUnmounted();
                    assert.deepEqual(render().children, ["init|params:one|after:first|params:two|after:update|dispose|"]);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
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
        StringAssert.Contains(script, "function $renderDirect() {", StringComparison.Ordinal);
        StringAssert.Contains(script, "return h(\"div\", null, [state.count]);", StringComparison.Ordinal);
        StringAssert.Contains(script, "cachedVNode = scope.$renderDirect();", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("cachedVNode = builder.finish();", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeUsesShouldRenderCachedVNodeGate()
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
                        builder.OpenElement(0, "button");
                        builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, Increment));
                        builder.AddContent(2, count);
                        builder.CloseElement();
                    }

                    private void Increment()
                    {
                        count++;
                    }
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
            var testFile = Path.Combine(tempRoot, "module-should-render-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/counter.mjs";

                test("ShouldRender false reuses the previous VNode until the gate allows rendering", () => {
                    const render = component.setup({}, { slots: {} });
                    const first = render();
                    assert.equal(first.name, "button");
                    assert.deepEqual(first.children, [0]);
                    assert.equal(typeof first.props.onClick, "function");

                    first.props.onClick();
                    const blocked = render();
                    assert.equal(blocked, first);
                    assert.deepEqual(blocked.children, [0]);

                    blocked.props.onClick();
                    const resumed = render();
                    assert.notEqual(resumed, first);
                    assert.deepEqual(resumed.children, [2]);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeAppliesElementAndComponentReferenceCaptures()
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
                public sealed class Child : ComponentBase, IVueComponent
                {
                }

                [ECMAScriptModule("./components/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    private string log = "none";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.OpenElement(1, "input");
                        builder.AddElementReferenceCapture(2, value => log = "element");
                        builder.CloseElement();
                        builder.OpenComponent<Child>(3);
                        builder.AddComponentReferenceCapture(4, value => log = value is null ? "component:null" : "component:ready");
                        builder.CloseComponent();
                        builder.OpenElement(5, "p");
                        builder.AddContent(6, log);
                        builder.CloseElement();
                        builder.CloseElement();
                    }
                }
            }
            """);
        var closure = BuildClosure(fixture, "Counter");
        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            fixture.Component,
            closure);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.finish();", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains(".addElementReferenceCapture(", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains(".addComponentReferenceCapture(", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "ref: value =>", StringComparison.Ordinal);

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
            var testFile = Path.Combine(tempRoot, "module-reference-capture-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/parent.mjs";

                test("element and component reference captures update component state through VNode ref callbacks", () => {
                    const render = component.setup({}, { slots: {} });
                    const first = render();

                    assert.equal(first.name, "section");
                    assert.equal(first.children[0].name, "input");
                    assert.equal(typeof first.children[0].props.ref, "function");
                    assert.equal(first.children[1].name.name, "Child");
                    assert.equal(typeof first.children[1].props.ref, "function");
                    assert.deepEqual(first.children[2].children, ["none"]);

                    first.children[0].props.ref({ tagName: "INPUT" });
                    assert.deepEqual(render().children[2].children, ["element"]);

                    first.children[1].props.ref({ id: "child" });
                    assert.deepEqual(render().children[2].children, ["component:ready"]);

                    first.children[1].props.ref(null);
                    assert.deepEqual(render().children[2].children, ["component:null"]);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
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
        StringAssert.Contains(script, "const invokeAsync = workItem => {", StringComparison.Ordinal);
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

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeRejectsStateHasChangedAfterUnmount()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent, IDisposable
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "button");
                        builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, Refresh));
                        builder.AddAttribute(3, "ondblclick", EventCallback.Factory.Create(this, RefreshAsync));
                        builder.AddContent(2, "refresh");
                        builder.CloseElement();
                    }

                    private void Refresh()
                    {
                        StateHasChanged();
                    }

                    private Task RefreshAsync()
                    {
                        return InvokeAsync(() => { });
                    }

                    public void Dispose()
                    {
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

        StringAssert.Contains(script, "let disposed = false;", StringComparison.Ordinal);
        StringAssert.Contains(script, "RazorVue component is disposed; StateHasChanged cannot run after unmount.", StringComparison.Ordinal);
        StringAssert.Contains(script, "RazorVue component is disposed; InvokeAsync cannot run after unmount.", StringComparison.Ordinal);
        StringAssert.Contains(script, "disposed = true;", StringComparison.Ordinal);

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
                const unmounted = [];

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
                export function onUnmounted(callback) {
                    unmounted.push(callback);
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                export function __runUnmounted() {
                    for (const callback of unmounted) {
                        callback();
                    }
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
            var testFile = Path.Combine(tempRoot, "module-disposed-event-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";
                import { __runUnmounted } from "vue";

                import component from "./components/counter.mjs";

                test("StateHasChanged and InvokeAsync reject event calls after component unmount", async () => {
                    const render = component.setup({}, { slots: {} });
                    const first = render();

                    assert.equal(typeof first.props.onClick, "function");
                    assert.equal(typeof first.props.onDblclick, "function");
                    assert.doesNotThrow(() => first.props.onClick());
                    await assert.doesNotReject(() => first.props.onDblclick());

                    __runUnmounted();

                    assert.throws(
                        () => first.props.onClick(),
                        /RazorVue component is disposed; StateHasChanged cannot run after unmount\./
                    );
                    await assert.rejects(
                        () => first.props.onDblclick(),
                        /RazorVue component is disposed; InvokeAsync cannot run after unmount\./
                    );
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
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
    public async Task BuildVueComponentModule_RuntimeUsesLatestOnParametersSetAsyncGeneration()
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

                    private string applied = "";
                    private int count;

                    protected override Task OnParametersSetAsync()
                    {
                        applied = Title;
                        count++;
                        return Task.CompletedTask;
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "p");
                        builder.AddContent(1, applied);
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
                const watchers = [];

                export function createStaticVNode(html, count) {
                    return { html, count };
                }
                export function defineComponent(options) {
                    return options;
                }
                export function reactive(value) {
                    return value;
                }
                export function watch(source, callback) {
                    watchers.push(callback);
                    return () => {};
                }
                export function onMounted() {}
                export function onUpdated() {}
                export function onUnmounted() {}
                export function h(name, props, children) {
                    return { name, props, children };
                }
                export function __runWatchers() {
                    for (const callback of watchers) {
                        callback();
                    }
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
            var testFile = Path.Combine(tempRoot, "module-parameters-set-async-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";
                import { __runWatchers } from "vue";

                import component from "./components/counter.mjs";

                const flush = () => new Promise((resolve) => setTimeout(resolve, 0));

                test("OnParametersSetAsync applies only the latest queued parameter generation", async () => {
                    const props = { title: "one" };
                    const render = component.setup(props, { slots: {} });

                    assert.deepEqual(render().children, ["", 0]);

                    props.title = "two";
                    __runWatchers();
                    props.title = "three";
                    __runWatchers();

                    await flush();
                    await flush();

                    assert.deepEqual(render().children, ["three", 1]);

                    props.title = "four";
                    __runWatchers();

                    await flush();
                    await flush();

                    assert.deepEqual(render().children, ["four", 2]);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
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
    public async Task BuildVueComponentModule_RuntimeRunsAsyncAfterRenderAndDisposeHooks()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/counter")]
                public partial class Counter : ComponentBase, IVueComponent, IAsyncDisposable
                {
                    private string log = "";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "p");
                        builder.AddContent(1, log);
                        builder.CloseElement();
                    }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        log += firstRender ? "afterAsync:first|" : "afterAsync:update|";
                        return Task.CompletedTask;
                    }

                    public async ValueTask DisposeAsync()
                    {
                        log += "disposeAsync|";
                        await Task.CompletedTask;
                    }
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
                const mounted = [];
                const updated = [];
                const unmounted = [];

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
                export function onMounted(callback) {
                    mounted.push(callback);
                }
                export function onUpdated(callback) {
                    updated.push(callback);
                }
                export function onUnmounted(callback) {
                    unmounted.push(callback);
                }
                export function h(name, props, children) {
                    return { name, props, children };
                }
                export async function __runMounted() {
                    for (const callback of mounted) {
                        await callback();
                    }
                    await Promise.resolve();
                }
                export async function __runUpdated() {
                    for (const callback of updated) {
                        await callback();
                    }
                    await Promise.resolve();
                }
                export async function __runUnmounted() {
                    for (const callback of unmounted) {
                        await callback();
                    }
                    await Promise.resolve();
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
            var testFile = Path.Combine(tempRoot, "module-async-lifecycle-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";
                import { __runMounted, __runUpdated, __runUnmounted } from "vue";

                import component from "./components/counter.mjs";

                test("async after-render and dispose hooks run through Vue lifecycle hooks", async () => {
                    const render = component.setup({}, { slots: {} });

                    assert.deepEqual(render().children, [""]);

                    await __runMounted();
                    assert.deepEqual(render().children, ["afterAsync:first|"]);

                    await __runUpdated();
                    assert.deepEqual(render().children, ["afterAsync:first|afterAsync:update|"]);

                    await __runUnmounted();
                    assert.deepEqual(render().children, ["afterAsync:first|afterAsync:update|disposeAsync|"]);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_EmitsChildContentAsDirectVueSlot()
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
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
        StringAssert.Contains(script, "slots.default", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("componentProps", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("syncSlotParameters", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "createPanelSetupScope(props, slots)", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeRendersNamedRenderFragmentSlotDirectly()
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
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(childScript);
        StringAssert.Contains(childScript, "slots.header", StringComparison.Ordinal);
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
        Assert.IsFalse(parentScript.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), parentScript);
        Assert.IsFalse(parentScript.Contains("builder.addComponentSlot(\"Header\", header);", StringComparison.Ordinal), parentScript);
        Assert.IsFalse(parentScript.Contains("builder.addComponentParameter(\"Header\"", StringComparison.Ordinal), parentScript);
        Assert.IsFalse(parentScript.Contains("const header =", StringComparison.Ordinal), parentScript);
        StringAssert.Contains(parentScript, "{ header: () => [].concat(h(\"h1\", null, [\"Named header\"]) ?? []) }", StringComparison.Ordinal);

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
                import { h } from "vue";

                import child from "./components/child.mjs";
                import parent from "./components/parent.mjs";

                test("RenderFragment parameter bridge follows dynamic Vue slot presence", () => {
                    const slots = {};
                    const render = child.setup({}, { slots });

                    const withoutHeader = render();
                    assert.equal(withoutHeader.children[1], null);

                    slots.header = () => [h("h1", null, ["Dynamic header"])];
                    const withHeader = render();
                    assert.equal(withHeader.children[1][0].name, "h1");
                    assert.deepEqual(withHeader.children[1][0].children, ["Dynamic header"]);

                    delete slots.header;
                    const removedHeader = render();
                    assert.equal(removedHeader.children[1], null);
                });

                test("named RenderFragment parameter is transported as a Vue named slot", () => {
                    const parentRender = parent.setup({}, { slots: {} });
                    const childVNode = parentRender();

                    assert.equal(typeof childVNode.children.header, "function");
                    assert.equal(childVNode.props?.header, undefined);

                    const childRender = childVNode.name.setup(childVNode.props ?? {}, { slots: childVNode.children });
                    const rendered = childRender();

                    assert.equal(rendered.name, "section");
                    assert.deepEqual(rendered.children[0], "before");
                    assert.equal(rendered.children[1][0].name, "h1");
                    assert.deepEqual(rendered.children[1][0].children, ["Named header"]);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RendersTypedRenderFragmentParameterAsScopedVueSlot()
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
                public partial class Child : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<string>? Header { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "before");
                        builder.AddContent(2, Header, "Scoped header");
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
        StringAssert.Contains(childScript, "setup(props, { slots }) {", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(childScript);
        StringAssert.Contains(childScript, "slots.header(\"Scoped header\")", StringComparison.Ordinal);
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
                    public RenderFragment<string>? Header { get; set; }
                }

                [ECMAScriptModule("./components/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment<string> header = value => child =>
                        {
                            child.OpenElement(0, "h1");
                            child.AddContent(1, value);
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
        Assert.IsFalse(parentScript.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), parentScript);
        Assert.IsFalse(parentScript.Contains("builder.addComponentScopedSlot(\"Header\", header);", StringComparison.Ordinal), parentScript);
        Assert.IsFalse(parentScript.Contains("builder.addComponentParameter(\"Header\"", StringComparison.Ordinal), parentScript);
        Assert.IsFalse(parentScript.Contains("const header =", StringComparison.Ordinal), parentScript);
        StringAssert.Contains(parentScript, "{ header: value => [].concat(h(\"h1\", null, [value]) ?? []) }", StringComparison.Ordinal);

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
            var testFile = Path.Combine(tempRoot, "module-scoped-slot-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import parent from "./components/parent.mjs";

                test("typed RenderFragment parameter is transported as a Vue scoped slot", () => {
                    const parentRender = parent.setup({}, { slots: {} });
                    const childVNode = parentRender();

                    assert.equal(typeof childVNode.children.header, "function");
                    assert.equal(childVNode.props?.header, undefined);

                    const childRender = childVNode.name.setup(childVNode.props ?? {}, { slots: childVNode.children });
                    const rendered = childRender();

                    assert.equal(rendered.name, "section");
                    assert.deepEqual(rendered.children[0], "before");
                    assert.equal(rendered.children[1].name, "h1");
                    assert.deepEqual(rendered.children[1].children, ["Scoped header"]);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_UsesVueSlotDescriptorForNamedRenderFragmentSlot()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using ECMAScript.VueContract;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/child")]
                [VueSlot(nameof(Header), Name = "title")]
                public partial class Child : ComponentBase, IVueComponent
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

                [ECMAScriptModule("./components/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment title = child =>
                        {
                            child.OpenElement(0, "h1");
                            child.AddContent(1, "Descriptor title");
                            child.CloseElement();
                        };

                        builder.OpenComponent<Child>(2);
                        builder.AddComponentParameter(3, "Header", title);
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

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(childScript);
        StringAssert.Contains(childScript, "slots.title", StringComparison.Ordinal);
        Assert.IsFalse(childScript.Contains("slots.header", StringComparison.Ordinal), childScript);
        Assert.IsFalse(childScript.Contains("\"header\"", StringComparison.Ordinal), childScript);

        var parent = fixture.Binding.Components.Single(component => component.ComponentSymbol.Name == "Counter");
        var parentClosure = BuildClosure(fixture, parent.ComponentSymbol.Name);
        var parentArtifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            parent,
            parentClosure);
        var parentScript = parentArtifact.ModuleText.ReplaceLineEndings("\n");

        Assert.IsFalse(parentScript.Contains("\"Header\": \"title\"", StringComparison.Ordinal), parentScript);
        Assert.IsFalse(parentScript.Contains("builder.addComponentSlot(\"Header\", title);", StringComparison.Ordinal), parentScript);
        Assert.IsFalse(parentScript.Contains("builder.addComponentParameter(\"Header\"", StringComparison.Ordinal), parentScript);
        StringAssert.Contains(parentScript, "{ title: () => [].concat(h(\"h1\", null, [\"Descriptor title\"]) ?? []) }", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task BuildVueComponentModule_UsesVueSlotDescriptorForTypedRenderFragmentSlot()
    {
        var fixture = CreateManualGeneratedFixture(
            """
            using ECMAScript;
            using ECMAScript.VueContract;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/child")]
                [VueSlot(nameof(TitleContent), Name = "title")]
                public partial class Child : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<string>? TitleContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, TitleContent, "Descriptor title");
                    }
                }

                [ECMAScriptModule("./components/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment<string> title = value => child =>
                        {
                            child.OpenElement(0, "h1");
                            child.AddContent(1, value);
                            child.CloseElement();
                        };

                        builder.OpenComponent<Child>(2);
                        builder.AddComponentParameter(3, "TitleContent", title);
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

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(childScript);
        StringAssert.Contains(childScript, "slots.title(\"Descriptor title\")", StringComparison.Ordinal);
        Assert.IsFalse(childScript.Contains("slots.titleContent", StringComparison.Ordinal), childScript);

        var parent = fixture.Binding.Components.Single(component => component.ComponentSymbol.Name == "Counter");
        var parentClosure = BuildClosure(fixture, parent.ComponentSymbol.Name);
        var parentArtifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            parent,
            parentClosure);
        var parentScript = parentArtifact.ModuleText.ReplaceLineEndings("\n");

        Assert.IsFalse(parentScript.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), parentScript);
        Assert.IsFalse(parentScript.Contains("builder.addComponentScopedSlot(\"TitleContent\", title);", StringComparison.Ordinal), parentScript);
        StringAssert.Contains(parentScript, "{ title: value => [].concat(h(\"h1\", null, [value]) ?? []) }", StringComparison.Ordinal);
        Assert.IsFalse(parentScript.Contains("slots.titleContent", StringComparison.Ordinal), parentScript);
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

            await RunDenoTestAsync(testFile, tempRoot);
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
                        builder.AddAttribute(1, "Title", Title);
                        builder.AddAttribute(2, "OnValueChanged", EventCallback.Factory.Create<string>(this, HandleValueChanged));
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
        var script = artifact.ModuleText.ReplaceLineEndings("\n");
        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("scope.buildRenderTree(builder);", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.finish();", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "{ title: props.title, onValueChanged: handleValueChanged }", StringComparison.Ordinal);

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

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeChildEventCallbackUpdatesParentState()
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
                    private string last = "initial";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "p");
                        builder.AddContent(1, last);
                        builder.CloseElement();

                        builder.OpenComponent<Child>(2);
                        builder.AddComponentParameter(3, "Title", last);
                        builder.AddComponentParameter(4, "OnValueChanged", EventCallback.Factory.Create<string>(this, HandleValueChanged));
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
            var testFile = Path.Combine(tempRoot, "module-child-event-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import component from "./components/parent.mjs";

                test("child EventCallback updates parent state on next render", () => {
                    const render = component.setup({}, { slots: {} });
                    const first = render();
                    const firstChildren = first.children;

                    assert.equal(firstChildren[0].name, "p");
                    assert.deepEqual(firstChildren[0].children, ["initial"]);
                    assert.equal(firstChildren[1].props.title, "initial");
                    assert.equal(typeof firstChildren[1].props.onValueChanged, "function");

                    firstChildren[1].props.onValueChanged("updated");

                    const second = render();
                    const secondChildren = second.children;
                    assert.deepEqual(secondChildren[0].children, ["updated"]);
                    assert.equal(secondChildren[1].props.title, "updated");
                    assert.equal(typeof secondChildren[1].props.onValueChanged, "function");
                    assert.equal(secondChildren[1].props.OnValueChanged, undefined);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
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

        Assert.IsFalse(script.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.addComponentParameter(\"Value\", state.text);", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.addComponentParameter(\"ValueChanged\", __value => state.text = __value);", StringComparison.Ordinal), script);
        StringAssert.Contains(script, "{ value: state.text, valueChanged: __value => state.text = __value }", StringComparison.Ordinal);
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

            await RunDenoTestAsync(testFile, tempRoot);
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

        Assert.IsFalse(parentScript.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), parentScript);
        Assert.IsFalse(parentScript.Contains("builder.openComponent(", StringComparison.Ordinal), parentScript);
        StringAssert.Contains(parentScript, "{ modelValue: state.text, \"onUpdate:modelValue\": __value => state.text = __value }", StringComparison.Ordinal);

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

            await RunDenoTestAsync(testFile, tempRoot);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildVueComponentModule_RuntimeCombinesDescriptorBindSlotAndEventCallback()
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
                [VueLibraryEmit(nameof(OnAction), VueEmitKind.Normal, Name = "action")]
                [VueSlot(nameof(TitleContent), Name = "title")]
                public partial class Child : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string Value { get; set; } = "";

                    [Parameter]
                    public EventCallback<string> ValueChanged { get; set; }

                    [Parameter]
                    public EventCallback<string> OnAction { get; set; }

                    [Parameter]
                    public RenderFragment<string>? TitleContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Value);
                        builder.AddContent(2, TitleContent, Value);
                        builder.OpenElement(3, "button");
                        builder.AddAttribute(4, "onclick", EventCallback.Factory.Create(this, Notify));
                        builder.AddContent(5, "notify");
                        builder.CloseElement();
                        builder.CloseElement();
                    }

                    private void Notify()
                    {
                        _ = OnAction.InvokeAsync(Value);
                    }
                }

                [ECMAScriptModule("./components/parent")]
                public partial class Counter : ComponentBase, IVueComponent
                {
                    private string text = "initial";
                    private string lastAction = "none";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment<string> title = value => child =>
                        {
                            child.OpenElement(0, "h1");
                            child.AddContent(1, "slot:");
                            child.AddContent(2, value);
                            child.CloseElement();
                        };

                        builder.OpenElement(0, "p");
                        builder.AddContent(1, lastAction);
                        builder.CloseElement();

                        builder.OpenComponent<Child>(2);
                        builder.AddComponentParameter(3, "Value", text);
                        builder.AddComponentParameter(4, "ValueChanged", EventCallback.Factory.CreateBinder(this, __value => text = __value, text));
                        builder.AddComponentParameter(5, "OnAction", EventCallback.Factory.Create<string>(this, HandleAction));
                        builder.AddComponentParameter(6, "TitleContent", title);
                        builder.CloseComponent();
                    }

                    private void HandleAction(string value)
                    {
                        lastAction = value;
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

        StringAssert.Contains(childScript, "props.modelValue", StringComparison.Ordinal);
        StringAssert.Contains(childScript, "props.onAction?.(props.modelValue);", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(childScript);
        StringAssert.Contains(childScript, "slots.title(props.modelValue)", StringComparison.Ordinal);

        var parent = fixture.Binding.Components.Single(component => component.ComponentSymbol.Name == "Counter");
        var parentClosure = BuildClosure(fixture, parent.ComponentSymbol.Name);
        var parentArtifact = await RazorSgVueComponentModuleBuilder.BuildAsync(
            fixture.Binding,
            parent,
            parentClosure);
        var parentScript = parentArtifact.ModuleText.ReplaceLineEndings("\n");

        Assert.IsFalse(parentScript.Contains("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal), parentScript);
        Assert.IsFalse(parentScript.Contains("builder.addComponentScopedSlot(\"TitleContent\", title);", StringComparison.Ordinal), parentScript);
        StringAssert.Contains(parentScript, "modelValue: state.text", StringComparison.Ordinal);
        StringAssert.Contains(parentScript, "\"onUpdate:modelValue\": __value => state.text = __value", StringComparison.Ordinal);
        StringAssert.Contains(parentScript, "onAction: handleAction", StringComparison.Ordinal);
        StringAssert.Contains(parentScript, "{ title: value => [].concat(h(\"h1\", null, [\"slot:\", value]) ?? []) }", StringComparison.Ordinal);

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
            var testFile = Path.Combine(tempRoot, "module-component-combined-runtime.test.mjs");
            WriteFile(
                testFile,
                """
                import assert from "node:assert/strict";
                import test from "node:test";

                import parent from "./components/parent.mjs";

                test("descriptor bind, typed slot, and EventCallback compose across parent and child modules", () => {
                    const renderParent = parent.setup({}, { slots: {} });
                    const firstParent = renderParent();
                    const firstChildren = firstParent.children;
                    const firstChildVNode = firstChildren[1];

                    assert.deepEqual(firstChildren[0].children, ["none"]);
                    assert.equal(firstChildVNode.props.modelValue, "initial");
                    assert.equal(typeof firstChildVNode.props["onUpdate:modelValue"], "function");
                    assert.equal(typeof firstChildVNode.props.onAction, "function");
                    assert.equal(typeof firstChildVNode.children.title, "function");
                    assert.equal(firstChildVNode.props.value, undefined);
                    assert.equal(firstChildVNode.props.valueChanged, undefined);
                    assert.equal(firstChildVNode.children.titleContent, undefined);

                    const renderFirstChild = firstChildVNode.name.setup(firstChildVNode.props, { slots: firstChildVNode.children });
                    const firstChild = renderFirstChild();
                    assert.equal(firstChild.name, "section");
                    assert.deepEqual(firstChild.children[0], "initial");
                    assert.equal(firstChild.children[1].name, "h1");
                    assert.deepEqual(firstChild.children[1].children, ["slot:", "initial"]);
                    assert.equal(firstChild.children[2].name, "button");
                    assert.equal(typeof firstChild.children[2].props.onClick, "function");

                    firstChild.children[2].props.onClick();
                    assert.deepEqual(renderParent().children[0].children, ["initial"]);

                    firstChildVNode.props["onUpdate:modelValue"]("updated");
                    const secondParent = renderParent();
                    const secondChildVNode = secondParent.children[1];
                    assert.equal(secondChildVNode.props.modelValue, "updated");
                    assert.equal(secondChildVNode.props.value, undefined);

                    const renderSecondChild = secondChildVNode.name.setup(secondChildVNode.props, { slots: secondChildVNode.children });
                    const secondChild = renderSecondChild();
                    assert.deepEqual(secondChild.children[0], "updated");
                    assert.deepEqual(secondChild.children[1].children, ["slot:", "updated"]);
                });
                """);

            await RunDenoTestAsync(testFile, tempRoot);
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

    private static async Task RunDenoTestAsync(string testFile, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = DenoExecutable.Value,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory
        };
        startInfo.ArgumentList.Add("test");
        startInfo.ArgumentList.Add("--quiet");
        startInfo.ArgumentList.Add("--allow-all");
        startInfo.ArgumentList.Add(testFile);

        Process process;
        try
        {
            process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Failed to start Deno runtime test process.");
        }
        catch (Win32Exception ex)
        {
            Assert.Fail("Bundled DenoHost runtime was not available to run the RazorVue generated module runtime test: " + ex.Message);
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

                Assert.Fail("Deno runtime test timed out after 30 seconds.");
            }

            var output = await standardOutput;
            var error = await standardError;
            if (process.ExitCode != 0)
            {
                Assert.Fail(
                    "Deno runtime test failed with exit code " +
                    process.ExitCode +
                    Environment.NewLine +
                    output +
                    Environment.NewLine +
                    error);
            }
        }
    }

    private static string ResolveDenoExecutable()
    {
        var repositoryRoot = Path.GetDirectoryName(FindRepositoryFile("Jazor.slnx"))
            ?? throw new InvalidOperationException("Could not resolve the repository root for DenoHost.");
        var executableName = OperatingSystem.IsWindows() ? "deno.exe" : "deno";
        var candidates = new List<string>();

        AddDenoRuntimeCandidates(candidates, Path.Combine(repositoryRoot, "src", "Jazor.Emit", "bin"), executableName);
        var packageRoot = Path.Combine(repositoryRoot, ".dotnet", ".nuget", "packages");
        if (Directory.Exists(packageRoot))
        {
            foreach (var runtimePackage in Directory.EnumerateDirectories(packageRoot, "denohost.runtime.*"))
                AddDenoRuntimeCandidates(candidates, runtimePackage, executableName);
        }

        // Runtime semantics must execute on the DenoHost asset shipped by Jazor, never Node/PATH.
        return candidates.FirstOrDefault(System.IO.File.Exists)
            ?? throw new FileNotFoundException(
                "Bundled DenoHost runtime was not found. Restore or build Jazor.Emit before running RazorVue runtime tests.");
    }

    private static void AddDenoRuntimeCandidates(ICollection<string> candidates, string root, string executableName)
    {
        if (!Directory.Exists(root))
            return;

        foreach (var candidate in Directory
            .EnumerateFiles(root, executableName, SearchOption.AllDirectories)
            .OrderByDescending(static path => path, StringComparer.OrdinalIgnoreCase))
        {
            candidates.Add(candidate);
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

    private static string? ResolveBrowserExecutable()
    {
        var explicitPath = Environment.GetEnvironmentVariable("RAZORVUE_BROWSER_EXE");
        if (!string.IsNullOrWhiteSpace(explicitPath) && System.IO.File.Exists(explicitPath))
            return explicitPath;

        string[] candidates = OperatingSystem.IsWindows()
            ?
            [
                @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                "msedge.exe",
                "chrome.exe"
            ]
            : OperatingSystem.IsMacOS()
                ?
                [
                    "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge",
                    "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
                    "microsoft-edge",
                    "google-chrome",
                    "chromium"
                ]
                :
                [
                    "microsoft-edge",
                    "microsoft-edge-stable",
                    "google-chrome",
                    "google-chrome-stable",
                    "chromium",
                    "chromium-browser"
                ];

        foreach (var candidate in candidates)
        {
            if (System.IO.File.Exists(candidate))
                return candidate;

            if (Path.IsPathFullyQualified(candidate))
                continue;

            var pathValue = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            var pathExtensions = OperatingSystem.IsWindows()
                ? (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT").Split(';', StringSplitOptions.RemoveEmptyEntries)
                : [string.Empty];
            foreach (var directory in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                foreach (var extension in pathExtensions)
                {
                    var fileName = candidate.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                        ? candidate
                        : candidate + extension;
                    var resolved = Path.Combine(directory, fileName);
                    if (System.IO.File.Exists(resolved))
                        return resolved;
                }
            }
        }

        return null;
    }

    private static void WriteFile(string path, string contents)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        System.IO.File.WriteAllText(path, contents.ReplaceLineEndings("\n"));
    }

    private static void DeleteDirectoryWithRetry(string path)
    {
        if (!Directory.Exists(path))
            return;

        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                Directory.Delete(path, recursive: true);
                return;
            }
            catch (IOException) when (attempt < 9)
            {
                System.Threading.Thread.Sleep(100);
            }
            catch (UnauthorizedAccessException) when (attempt < 9)
            {
                System.Threading.Thread.Sleep(100);
            }
        }
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
            closure.StateFields.Any(field => Util.GetConfigOrSymbolName(field) == name),
            $"Expected field '{name}' in closure: {Describe(closure)}");

    private static void AssertHasProperty(RazorSgComponentMemberClosure closure, string name)
        => Assert.IsTrue(
            closure.StateProperties
                .Concat(closure.ParameterProperties)
                .Concat(closure.ComputedProperties)
                .Any(property => Util.GetConfigOrSymbolName(property) == name),
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

        var bound = RazorSgGeneratedCSharpBinder.TryBindFinalCompilation(
            compilation,
            RazorSgComponentCandidateSelector.DiscoverTailRequiredComponents(compilation),
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
