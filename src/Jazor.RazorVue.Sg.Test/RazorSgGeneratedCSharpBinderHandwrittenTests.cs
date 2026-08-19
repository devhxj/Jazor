using System.Collections.Immutable;
using Jazor.RazorVue.Generation;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class GeneratedCSharpBinderHandwrittenTests
{
    [TestMethod]
    public void TryBindHandwritten_ReusesCurrentCompilationBuildRenderTreeBody()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/counter")]
            public partial class Counter : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, "handwritten");
                    builder.CloseElement();
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/Counter.razor.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.HandwrittenCompilation.Binder",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var components = ComponentSelector.DiscoverHandwrittenComponents(compilation);

        var result = GeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            components,
            out var binding,
            out var failure);

        Assert.IsTrue(result, failure);
        Assert.IsNotNull(binding);
        Assert.AreSame(compilation, binding!.Compilation);
        Assert.AreEqual(1, binding.Documents.Length);
        Assert.AreSame(sourceTree, binding.Components.Single().BuildRenderTreeMethod.DeclaringSyntaxReferences.Single().SyntaxTree);
        Assert.AreSame(binding.Documents.Single(), binding.Components.Single().Document);
        Assert.AreEqual(3, binding.Components.Single().BuildRenderTreeBody.Operations.Length);
    }

    [TestMethod]
    public void TryBindHandwritten_OrdersComponentsAndSharesTheirSourceDocument()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/zebra")]
            public sealed class Zebra : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "zebra");
                }
            }

            [ECMAScriptModule("./components/alpha")]
            public sealed class Alpha : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "alpha");
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/Components.razor.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.HandwrittenCompilation.StableOrder",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var alpha = compilation.GetTypeByMetadataName("Demo.Pages.Alpha");
        var zebra = compilation.GetTypeByMetadataName("Demo.Pages.Zebra");

        Assert.IsNotNull(alpha);
        Assert.IsNotNull(zebra);
        Assert.IsTrue(GeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            ImmutableArray.Create(zebra!, alpha!),
            out var binding,
            out var failure), failure);
        Assert.IsNotNull(binding);

        CollectionAssert.AreEqual(
            new[] { "Demo.Pages.Alpha", "Demo.Pages.Zebra" },
            binding!.Components.Select(static component => component.ComponentSymbol.ToDisplayString()).ToArray());
        Assert.AreEqual(1, binding.Documents.Length);
        Assert.AreSame(binding.Documents[0], binding.Components[0].Document);
        Assert.AreSame(binding.Documents[0], binding.Components[1].Document);
        Assert.AreEqual("Pages/Components.razor.cs", binding.Documents[0].SourcePath);
    }

    [TestMethod]
    public void TryBuildHandwrittenClosure_BlockBodiedComputedPropertyRemainsExecutableMember()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/counter")]
            public partial class Counter : ComponentBase, IVueComponent
            {
                private int count = 2;

                private string Label
                {
                    get
                    {
                        return count.ToString();
                    }
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, Label);
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/Counter.razor.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.HandwrittenClosure.ComputedProperty",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var components = ComponentSelector.DiscoverHandwrittenComponents(compilation);

        Assert.IsTrue(GeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            components,
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        var component = binding!.Components.Single();

        Assert.IsTrue(MemberClosureBuilder.TryBuild(
            binding,
            component,
            out var closure,
            out var closureFailure), closureFailure);
        Assert.IsNotNull(closure);
        CollectionAssert.AreEqual(new[] { "Label" }, closure!.ComputedProperties.Select(static property => property.Name).ToArray());
        Assert.IsFalse(closure.StateProperties.Any(static property => property.Name == "Label"));
        Assert.IsTrue(closure.CreateMemberFilter()(component.ComponentSymbol.GetMembers("get_Label").OfType<IMethodSymbol>().Single()));
    }

    [TestMethod]
    public async Task BuildHandwrittenArtifact_BlockBodiedComputedPropertyLowersThroughComponentModule()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/counter")]
            public partial class Counter : ComponentBase, IVueComponent
            {
                private int count = 2;

                private string Label
                {
                    get
                    {
                        return count.ToString();
                    }
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "span");
                    builder.AddContent(1, Label);
                    builder.CloseElement();
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/Counter.razor.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.HandwrittenArtifact.ComputedProperty",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var components = ComponentSelector.DiscoverHandwrittenComponents(compilation);

        Assert.IsTrue(GeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            components,
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        var component = binding!.Components.Single();
        Assert.IsTrue(MemberClosureBuilder.TryBuild(
            binding,
            component,
            out var closure,
            out var closureFailure), closureFailure);
        Assert.IsNotNull(closure);

        var artifact = await VueModuleBuilder.BuildAsync(binding, component, closure!);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "function Label()", StringComparison.Ordinal);
        StringAssert.Contains(script, "return state.count.toString();", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("this.count", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.finish()", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public async Task BuildHandwrittenArtifact_InheritedGenericBuildRenderTreeLowersOpenComponent()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

            namespace Demo.Pages;

            public abstract class TableBridge<T> : ComponentBase
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<GenericTable<T>>(0);
                    builder.CloseComponent();
                }
            }

            [ECMAScriptModule("./components/setting-table")]
            public sealed class SettingTable : TableBridge<string>, IVueComponent { }

            [ECMAScriptModule("./components/generic-table")]
            public sealed class GenericTable<TValue> : ComponentBase, IVueComponent { }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/SettingTable.razor.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.HandwrittenArtifact.InheritedGeneric",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var components = ComponentSelector.DiscoverHandwrittenComponents(compilation);

        Assert.IsTrue(GeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            components,
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        var component = binding!.Components.Single(candidate => candidate.ComponentSymbol.Name == "SettingTable");
        Assert.IsFalse(SymbolEqualityComparer.Default.Equals(
            component.BuildRenderTreeMethod,
            component.BuildRenderTreeMethod.OriginalDefinition));

        Assert.IsTrue(MemberClosureBuilder.TryBuild(
            binding,
            component,
            out var closure,
            out var closureFailure), closureFailure);
        Assert.IsNotNull(closure);

        var artifact = await VueModuleBuilder.BuildAsync(binding, component, closure!);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "from \"./generic-table.mjs\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "return h(", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("builder.", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public async Task BuildHandwrittenArtifact_ConditionalDifferentNamedSlotsReportsUnsupportedLowering()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

            namespace Demo.Components;

            [ECMAScriptModule("./components/dual-slot-panel")]
            public sealed class DualSlotPanel : ComponentBase, IVueComponent
            {
                [ECMAScriptName("header")]
                [Parameter] public RenderFragment? Header { get; set; }

                [ECMAScriptName("footer")]
                [Parameter] public RenderFragment? Footer { get; set; }
            }

            [ECMAScriptModule("./components/dual-slot-page")]
            public sealed class DualSlotPage : ComponentBase, IVueComponent
            {
                private bool UseHeader { get; set; } = true;

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    RenderFragment header = child =>
                    {
                        child.OpenElement(0, "strong");
                        child.AddContent(1, "Header content");
                        child.CloseElement();
                    };
                    RenderFragment footer = child =>
                    {
                        child.OpenElement(0, "strong");
                        child.AddContent(1, "Footer content");
                        child.CloseElement();
                    };
                    builder.OpenComponent<DualSlotPanel>(0);
                    if (UseHeader)
                    {
                        builder.AddComponentParameter(1, "Header", header);
                    }
                    else
                    {
                        builder.AddComponentParameter(2, "Footer", footer);
                    }
                    builder.CloseComponent();
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/DualSlotPage.razor.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.HandwrittenArtifact.ConditionalDifferentSlots",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var components = ComponentSelector.DiscoverHandwrittenComponents(compilation);

        Assert.IsTrue(GeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            components,
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        var component = binding!.Components.Single(candidate => candidate.ComponentSymbol.Name == "DualSlotPage");
        Assert.IsTrue(MemberClosureBuilder.TryBuild(
            binding,
            component,
            out var closure,
            out var closureFailure), closureFailure);
        Assert.IsNotNull(closure);

        RazorVueDiagnosticException? exception = null;
        try
        {
            await VueModuleBuilder.BuildAsync(binding, component, closure!);
            Assert.Fail("Conditional RenderFragment component parameters should be rejected explicitly.");
        }
        catch (RazorVueDiagnosticException caught)
        {
            exception = caught;
        }

        Assert.IsNotNull(exception);
        StringAssert.Contains(exception!.Message, "Conditional RenderFragment component parameters", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task BuildHandwrittenArtifact_ConditionalSameNamedSlotAcrossAttributeFormsPreservesBothBranchesOnDenoHost()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/mixed-slot-panel")]
                public sealed class MixedSlotPanel : ComponentBase, IVueComponent
                {
                    [ECMAScriptName("header")]
                    [Parameter] public RenderFragment? Header { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/mixed-slot-page")]
                public sealed class MixedSlotPage : ComponentBase, IVueComponent
                {
                    [Parameter] public bool UseAttributeSlot { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        RenderFragment attributeHeader = child => child.AddContent(0, "attribute header");
                        RenderFragment componentParameterHeader = child => child.AddContent(0, "component parameter header");

                        builder.OpenComponent<Demo.Components.MixedSlotPanel>(0);
                        if (UseAttributeSlot)
                        {
                            builder.AddAttribute(1, "Header", attributeHeader);
                        }
                        else
                        {
                            builder.AddComponentParameter(2, "Header", componentParameterHeader);
                        }
                        builder.CloseComponent();
                    }
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/MixedSlotPage.razor.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.HandwrittenArtifact.MixedConditionalSlotForms",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var components = ComponentSelector.DiscoverHandwrittenComponents(compilation);

        Assert.IsTrue(GeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            components,
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        var component = binding!.Components.Single(candidate => candidate.ComponentSymbol.Name == "MixedSlotPage");
        Assert.IsTrue(MemberClosureBuilder.TryBuild(
            binding,
            component,
            out var closure,
            out var closureFailure), closureFailure);
        Assert.IsNotNull(closure);

        var artifact = await VueModuleBuilder.BuildAsync(binding, component, closure!);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
        StringAssert.Contains(script, "createSlots", StringComparison.Ordinal);
        StringAssert.Contains(script, "props.UseAttributeSlot", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("builder.", StringComparison.Ordinal), script);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/mixed-slot-page.mjs",
            script,
            "handwritten-mixed-slot-page.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/mixed-slot-page.mjs";
            import panel from "./components/mixed-slot-panel.mjs";

            test("same named slot remains dynamic when direct RenderTreeBuilder mixes attribute forms", () => {
                const attributeResult = component.setup({ UseAttributeSlot: true }, { slots: {} })();
                const parameterResult = component.setup({ UseAttributeSlot: false }, { slots: {} })();

                assert.equal(attributeResult.name, panel);
                assert.equal(parameterResult.name, panel);
                assert.deepEqual(attributeResult.children.header(), ["attribute header"]);
                assert.deepEqual(parameterResult.children.header(), ["component parameter header"]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/mixed-slot-panel.mjs"] = "export default { name: \"mixed-slot-panel\" };"
            });
    }

    [TestMethod]
    public async Task BuildHandwrittenArtifact_ExpressionBodiedRenderTreeHelpersLowerLikeBlockHelpersOnDenoHost()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/expression-helper-page")]
            public sealed class ExpressionHelperPage : ComponentBase, IVueComponent
            {
                [Parameter] public string Title { get; set; } = string.Empty;

                private string Suffix { get; } = "!";

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "p");
                    RenderStaticPrefix(builder, Title);
                    RenderInstanceSuffix(builder, Title);
                    builder.CloseElement();
                }

                private static void RenderStaticPrefix(RenderTreeBuilder target, string value)
                    => target.AddContent(0, "prefix:" + value);

                private void RenderInstanceSuffix(RenderTreeBuilder target, string value)
                    => target.AddContent(1, value + Suffix);
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/ExpressionHelperPage.razor.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.HandwrittenArtifact.ExpressionBodiedHelpers",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var components = ComponentSelector.DiscoverHandwrittenComponents(compilation);

        Assert.IsTrue(GeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            components,
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        var component = binding!.Components.Single();
        Assert.IsTrue(MemberClosureBuilder.TryBuild(
            binding,
            component,
            out var closure,
            out var closureFailure), closureFailure);
        Assert.IsNotNull(closure);

        var artifact = await VueModuleBuilder.BuildAsync(binding, component, closure!);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
        StringAssert.Contains(script, "createTextVNode(\"prefix:\" + props.Title, 1)", StringComparison.Ordinal);
        StringAssert.Contains(script, "createTextVNode(props.Title + state.Suffix, 1)", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("builder.", StringComparison.Ordinal), script);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/expression-helper-page.mjs",
            script,
            "handwritten-expression-helper-page.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/expression-helper-page.mjs";

            test("expression-bodied direct RenderTreeBuilder helpers retain static and instance substitutions", () => {
                const paragraph = component.setup({ Title: "Deploy API" }, { slots: {} })();
                assert.equal(paragraph.name, "p");
                assert.deepEqual(paragraph.children, [
                    { name: "__text", children: "prefix:Deploy API", patchFlag: 1 },
                    { name: "__text", children: "Deploy API!", patchFlag: 1 }
                ]);
            });
            """);
    }

    [TestMethod]
    public async Task BuildHandwrittenArtifact_StaticScalarContentPreservesLiteralVNodeChildrenOnDenoHost()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/static-scalar-page")]
            public sealed class StaticScalarPage : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "div");
                    builder.AddContent(1, (object?)null);
                    builder.AddContent(2, "text");
                    builder.AddContent(3, true);
                    builder.AddContent(4, 'x');
                    builder.AddContent(5, (sbyte)-1);
                    builder.AddContent(6, (byte)2);
                    builder.AddContent(7, (short)-3);
                    builder.AddContent(8, (ushort)4);
                    builder.AddContent(9, -5);
                    builder.AddContent(10, (uint)6);
                    builder.AddContent(11, (long)-7);
                    builder.AddContent(12, (ulong)8);
                    builder.AddContent(13, 1.5f);
                    builder.AddContent(14, 2.5d);
                    builder.AddContent(15, 3.5m);
                    builder.CloseElement();
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/StaticScalarPage.razor.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.HandwrittenArtifact.StaticScalarContent",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var components = ComponentSelector.DiscoverHandwrittenComponents(compilation);

        Assert.IsTrue(GeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            components,
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        var component = binding!.Components.Single();
        Assert.IsTrue(MemberClosureBuilder.TryBuild(
            binding,
            component,
            out var closure,
            out var closureFailure), closureFailure);
        Assert.IsNotNull(closure);

        var artifact = await VueModuleBuilder.BuildAsync(binding, component, closure!);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
        Assert.IsFalse(script.Contains("builder.", StringComparison.Ordinal), script);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/static-scalar-page.mjs",
            script,
            "handwritten-static-scalar-page.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/static-scalar-page.mjs";

            test("direct RenderTreeBuilder scalar literals remain immutable Vue children", () => {
                const result = component.setup({}, { slots: {} })();
                assert.equal(result.name, "div");
                assert.deepEqual(result.children, [
                    null,
                    "text",
                    true,
                    "x",
                    -1,
                    2,
                    -3,
                    4,
                    -5,
                    6,
                    -7,
                    8,
                    1.5,
                    2.5,
                    3.5
                ]);
            });
            """);
    }

    [TestMethod]
    public async Task BuildHandwrittenArtifact_ConditionalOptionalSlotOmitsTheSlotWhenTheAuthorBranchIsAbsentOnDenoHost()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/optional-slot-panel")]
                public sealed class OptionalSlotPanel : ComponentBase, IVueComponent
                {
                    [ECMAScriptName("header")]
                    [Parameter] public RenderFragment? Header { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/optional-slot-page")]
                public sealed class OptionalSlotPage : ComponentBase, IVueComponent
                {
                    [Parameter] public bool ShowHeader { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenComponent<Demo.Components.OptionalSlotPanel>(0);
                        if (ShowHeader)
                        {
                            builder.AddComponentParameter(
                                1,
                                "Header",
                                (RenderFragment)(child => child.AddContent(0, "conditional header")));
                        }
                        builder.CloseComponent();
                    }
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/OptionalSlotPage.razor.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.HandwrittenArtifact.OptionalConditionalSlot",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var components = ComponentSelector.DiscoverHandwrittenComponents(compilation);

        Assert.IsTrue(GeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            components,
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        var component = binding!.Components.Single(candidate => candidate.ComponentSymbol.Name == "OptionalSlotPage");
        Assert.IsTrue(MemberClosureBuilder.TryBuild(
            binding,
            component,
            out var closure,
            out var closureFailure), closureFailure);
        Assert.IsNotNull(closure);

        var artifact = await VueModuleBuilder.BuildAsync(binding, component, closure!);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
        StringAssert.Contains(script, "createSlots", StringComparison.Ordinal);
        StringAssert.Contains(script, "props.ShowHeader", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/optional-slot-page.mjs",
            script,
            "handwritten-optional-slot-page.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/optional-slot-page.mjs";
            import panel from "./components/optional-slot-panel.mjs";

            test("a conditional direct slot is omitted rather than invoked as an empty callback", () => {
                const visible = component.setup({ ShowHeader: true }, { slots: {} })();
                const hidden = component.setup({ ShowHeader: false }, { slots: {} })();

                assert.equal(visible.name, panel);
                assert.equal(hidden.name, panel);
                assert.equal(typeof visible.children.header, "function");
                assert.deepEqual(visible.children.header(), ["conditional header"]);
                assert.equal("header" in hidden.children, false);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/optional-slot-panel.mjs"] = "export default { name: \"optional-slot-panel\" };"
            });
    }
}
