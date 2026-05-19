using System.Collections.Immutable;
using System.Text;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Extensibility;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RenderTree;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorVueTemplateFrontendParityTests
{
    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForMarkupAndInterpolation()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<section><h1>@Title</h1><p>Hello</p></section>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.Markup.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForAttributes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<div title="@Title" class="hero">Hello</div>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.Attributes.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForElementSplat()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<div title="@Title" @attributes="AdditionalAttributes">Hello</div>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.ElementSplat.Tests",
            documentPath,
            documentText,
            """
            using System.Collections.Generic;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter(CaptureUnmatchedValues = true)]
                    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForComponentAndDefaultChildContent()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<ChildCard Title="@Title"><p>Body</p></ChildCard>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.Component.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentAndChildComponentSource());

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForNamedAndTypedChildContent()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard Title="@Title">
                <Header>
                    <h1>@Title</h1>
                </Header>
                <ItemTemplate Context="item">
                    <p>@item</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.NamedTypedChildContent.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/layout-card")]
                public partial class LayoutCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public RenderFragment? Header { get; set; }

                    [Parameter]
                    public RenderFragment<string>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForTypedChildContentTemplateLocalCodeBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                    }
                    <p>@decorated</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.TypedChildContent.TemplateLocalCodeBlock.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/layout-card")]
                public partial class LayoutCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<string>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForRenderFragmentLocalCarrierAssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template = item => @<p>@item</p>;
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.RenderFragmentLocalCarrier.TypedSlot.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/layout-card")]
                public partial class LayoutCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<string>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForRenderFragmentLocalCarrierWithTrailingIfAssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template = item => @<p>@item</p>;
                if (Show)
                {
                    <section>tail</section>
                }
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.RenderFragmentLocalCarrier.TypedSlot.TrailingIf.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/layout-card")]
                public partial class LayoutCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<string>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Show { get; set; }
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForRenderFragmentLocalCarrierWithTrailingForeachAssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template = item => @<p>@item</p>;
                foreach (var tag in Tags!)
                {
                    <section>@tag</section>
                }
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.RenderFragmentLocalCarrier.TypedSlot.TrailingForEach.Tests",
            documentPath,
            documentText,
            """
            using System.Collections.Generic;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/layout-card")]
                public partial class LayoutCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<string>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public List<string>? Tags { get; set; }
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontend_ForComplexRootTemplateCodeBlock_UsesRazorIrImperativePromotion()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                if (Hide)
                {
                    return;
                }
            }

            <section>ready</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.Preferred.Imperative.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Hide { get; set; }
                }
            }
            """);

        var renderTree = RazorVuePreferredTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length);
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, imperative.Kind);

        var artifact = new RazorVueArtifactFactory(RazorVuePreferredTemplateFrontend.Instance).Lower(context, snapshot);
        StringAssert.Contains(artifact.ModuleCode, "const __jazorBuilder = __jazorCreateRenderTreeBuilder(h);");
        StringAssert.Contains(artifact.ModuleCode, "if (props.hide) {");
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForRenderFragmentLocalCarrierWithTrailingForAssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template = item => @<p>@item</p>;
                for (var i = 0; i < Count; i++)
                {
                    <section>@i</section>
                }
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.RenderFragmentLocalCarrier.TypedSlot.TrailingFor.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/layout-card")]
                public partial class LayoutCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<string>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Count { get; set; }
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForRenderFragmentLocalCarrierInitializedFromFactoryMethodAssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = CreateTemplate(Title);
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment<int> CreateTemplate(string? title)
                    => item => @<span>@title @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.RenderFragmentLocalCarrier.Factory.TypedSlot.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/layout-card")]
                public partial class LayoutCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForRenderFragmentLocalCarrierInitializedFromFactoryMethodWithOmittedOptionalParameterAssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = CreateTemplate();
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                private RenderFragment<int> CreateTemplate(string? title = "fallback-title")
                    => item => @<span>@title @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.RenderFragmentLocalCarrier.FactoryOptional.TypedSlot.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/layout-card")]
                public partial class LayoutCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForRenderFragmentLocalCarrierInitializedFromCurrentComponentFieldCarrierAssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = _template;
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                private readonly RenderFragment<int> _template
                    = item => @<span>@item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.RenderFragmentLocalCarrier.Field.TypedSlot.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/layout-card")]
                public partial class LayoutCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForRenderFragmentLocalCarrierInitializedFromCurrentComponentAutoPropertyCarrierAssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = Template;
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                private RenderFragment<int> Template { get; } = item => @<span>@item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.RenderFragmentLocalCarrier.AutoProperty.TypedSlot.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/layout-card")]
                public partial class LayoutCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForRenderFragmentLocalCarrierInitializedFromChainedCurrentComponentPropertyCarrierAssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = PrimaryTemplate;
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment<int> PrimaryTemplate => ForwardedTemplate;

                private RenderFragment<int> ForwardedTemplate => CreateTemplate(Title);

                private RenderFragment<int> CreateTemplate(string? title)
                    => item => @<span>@title @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.RenderFragmentLocalCarrier.MemberChain.TypedSlot.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/layout-card")]
                public partial class LayoutCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForTemplateLocalCodeBlockWithNestedIf()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var localTitle = Title;
                if (Show)
                {
                    <section>@localTitle</section>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.TemplateLocalCodeBlock.NestedIf.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public bool Show { get; set; }
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForTemplateLocalCodeBlockWithNestedIfElse()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var localTitle = Title;
                if (Show)
                {
                    <section>@localTitle</section>
                }
                else
                {
                    <p>hidden</p>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.TemplateLocalCodeBlock.NestedIfElse.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public bool Show { get; set; }
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForTemplateLocalCodeBlockWithNestedForeach()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var prefix = Title;
                foreach (var item in Items!)
                {
                    <p>@prefix @item</p>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.TemplateLocalCodeBlock.NestedForEach.Tests",
            documentPath,
            documentText,
            """
            using System.Collections.Generic;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public List<string>? Items { get; set; }
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForTemplateLocalCodeBlockWithNestedFor()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var prefix = Title;
                for (var i = 0; i < Count; i++)
                {
                    <p>@prefix @i</p>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.TemplateLocalCodeBlock.NestedFor.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public int Count { get; set; }
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForTemplateLocalCodeBlockWithSequentialIfs()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var localTitle = Title;
                if (ShowPrimary)
                {
                    <section>@localTitle</section>
                }

                if (ShowSecondary)
                {
                    <p>secondary</p>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.TemplateLocalCodeBlock.SequentialIfs.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public bool ShowPrimary { get; set; }

                    [Parameter]
                    public bool ShowSecondary { get; set; }
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForTemplateLocalCodeBlockWithIfThenForeach()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var prefix = Title;
                if (ShowPrimary)
                {
                    <section>@prefix</section>
                }

                foreach (var item in Items!)
                {
                    <p>@prefix @item</p>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.TemplateLocalCodeBlock.IfThenForEach.Tests",
            documentPath,
            documentText,
            """
            using System.Collections.Generic;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public bool ShowPrimary { get; set; }

                    [Parameter]
                    public List<string>? Items { get; set; }
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForTemplateLocalCodeBlockWithForeachThenIf()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var prefix = Title;
                foreach (var item in Items!)
                {
                    <p>@prefix @item</p>
                }

                if (ShowTail)
                {
                    <section>@prefix</section>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.TemplateLocalCodeBlock.ForEachThenIf.Tests",
            documentPath,
            documentText,
            """
            using System.Collections.Generic;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public List<string>? Items { get; set; }

                    [Parameter]
                    public bool ShowTail { get; set; }
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForTemplateLocalCodeBlockWithForThenIf()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var prefix = Title;
                for (var i = 0; i < Count; i++)
                {
                    <p>@prefix @i</p>
                }

                if (ShowTail)
                {
                    <section>@prefix</section>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.TemplateLocalCodeBlock.ForThenIf.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public int Count { get; set; }

                    [Parameter]
                    public bool ShowTail { get; set; }
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForElseIfChain()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @if (Primary)
            {
                <p>Primary</p>
            }
            else if (Secondary)
            {
                <p>Secondary</p>
            }
            else
            {
                <p>Fallback</p>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.ElseIf.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Primary { get; set; }

                    [Parameter]
                    public bool Secondary { get; set; }
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontendAndRazorIr_AgreeOnSupportedSubset_ForCountStyleForLoop()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @for (var i = 0; i < Count; i++)
            {
                <p>@i</p>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Parity.ForLoop.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Count { get; set; }
                }
            }
            """);

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, new RazorVueRazorIrTemplateFrontend(), context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontend_FallsBackToBuildRenderTree_OnlyForHandwrittenBuildRenderTreeComponents()
    {
        var context = CreateBuildRenderTreeOnlyContext();
        var snapshot = context.CreateSemanticSnapshots().Single(static item => item.Descriptor.Name == "CounterCard");

        AssertParity(RazorVuePreferredTemplateFrontend.Instance, BuildRenderTreeTemplateFrontend.Instance, context, snapshot);
    }

    [TestMethod]
    public void PreferredTemplateFrontend_WithRazorGeneratedBuildRenderTreeButNoBoundRazorDocument_Throws()
    {
        var context = CreateGeneratedRazorContextWithoutBoundDocuments();
        var snapshot = RazorVueRazorDocumentSemanticFrontend.Instance.CreateSemanticSnapshots(context).Single();

        Assert.IsNull(snapshot.RazorIrCarrier);

        var exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => RazorVuePreferredTemplateFrontend.Instance.CreateRenderTree(context, snapshot));

        StringAssert.Contains(exception.Message, "only falls back to BuildRenderTree for source-authored components");
    }

    private static void AssertParity(
        IRazorVueTemplateFrontend expectedFrontend,
        IRazorVueTemplateFrontend actualFrontend,
        Jazor.RazorVue.RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot)
    {
        var expectedTree = expectedFrontend.CreateRenderTree(context, snapshot);
        var actualTree = actualFrontend.CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            DescribeStructure(expectedTree),
            DescribeStructure(actualTree),
            "Template frontend render tree diverged.");

        var primaryRazorPath = snapshot.RazorIrCarrier?.DocumentPath;

        Assert.AreEqual(CountOrigins(expectedTree), CountOrigins(actualTree), "Template origin entry count diverged.");
        Assert.IsTrue(
            EnumerateOrigins(actualTree).All(origin => string.Equals(origin.SourceFilePath, primaryRazorPath, StringComparison.OrdinalIgnoreCase) ||
                                                     string.IsNullOrWhiteSpace(primaryRazorPath)),
            "Template frontend emitted a non-primary Razor document path in template origins.");
        if (!string.IsNullOrWhiteSpace(primaryRazorPath))
        {
            Assert.IsTrue(
                EnumerateOrigins(actualTree).All(origin => origin.MappingQuality == RazorVueMappingQuality.ExactSource),
                "Preferred Razor IR template path should preserve exact Razor source origins.");
        }

        var expectedArtifact = new RazorVueArtifactFactory(expectedFrontend).Lower(context, snapshot);
        var actualArtifact = new RazorVueArtifactFactory(actualFrontend).Lower(context, snapshot);

        Assert.AreEqual(expectedArtifact.ModuleCode, actualArtifact.ModuleCode, "Generated module code diverged.");
        CollectionAssert.AreEqual(expectedArtifact.Imports.ToArray(), actualArtifact.Imports.ToArray(), "Generated imports diverged.");
        CollectionAssert.AreEqual(expectedArtifact.Styles.ToArray(), actualArtifact.Styles.ToArray(), "Generated styles diverged.");
        CollectionAssert.AreEqual(expectedArtifact.PluginRequirements.ToArray(), actualArtifact.PluginRequirements.ToArray(), "Generated plugin requirements diverged.");
        Assert.AreEqual(expectedArtifact.Identity.TemplateHash, actualArtifact.Identity.TemplateHash, "TemplateHash diverged.");
        Assert.AreEqual(expectedArtifact.Identity.LogicHash, actualArtifact.Identity.LogicHash, "LogicHash diverged.");
        Assert.AreEqual(expectedArtifact.Identity.HmrBoundaryKind, actualArtifact.Identity.HmrBoundaryKind, "HMR boundary diverged.");

        if (!string.IsNullOrWhiteSpace(primaryRazorPath))
        {
            Assert.IsTrue(
                actualArtifact.SourceOrigins.Any(origin => origin.MappingQuality == RazorVueMappingQuality.ExactSource),
                "Preferred Razor IR artifact did not preserve exact Razor source origins.");
        }
    }

    private static Jazor.RazorVue.RazorVueCompilationContext CreateBuildRenderTreeOnlyContext()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.RazorIr.PreferredFrontend.Fallback.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    global using static ECMAScript.Vue3;
                    global using ECMAScript.VueContract;
                    global using Microsoft.AspNetCore.Components;
                    """,
                    options: parseOptions,
                    path: "RazorVueTestGlobalUsings.g.cs"),
                CSharpSyntaxTree.ParseText(
                    """
                    using System;

                    namespace ECMAScript
                    {
                        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                        public sealed class ECMAScriptModuleAttribute : Attribute
                        {
                            public ECMAScriptModuleAttribute() { }
                            public ECMAScriptModuleAttribute(string import) { }
                        }
                    }

                    namespace Demo.Components
                    {
                        [ECMAScript.ECMAScriptModule("./components/counter-card")]
                        public class CounterCard : ComponentBase, IVueComponent
                        {
                            [Parameter]
                            public string? Title { get; set; }

                            protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
                            {
                                builder.OpenElement(0, "section");
                                builder.AddContent(1, Title);
                                builder.CloseElement();
                            }
                        }
                    }
                    """,
                    options: parseOptions,
                    path: "CounterCard.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var context = Jazor.RazorVue.RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        return context;
    }

    private static Jazor.RazorVue.RazorVueCompilationContext CreateGeneratedRazorContextWithoutBoundDocuments()
    {
        const string importsPath = @"D:\repo\Demo\_Imports.razor";
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";

        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.RazorIr.PreferredFrontend.GeneratedWithoutDocs.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    global using static ECMAScript.Vue3;
                    global using ECMAScript.VueContract;
                    global using Microsoft.AspNetCore.Components;
                    """,
                    options: parseOptions,
                    path: "RazorVueTestGlobalUsings.g.cs"),
                CSharpSyntaxTree.ParseText(
                    """
                    using System;

                    namespace ECMAScript
                    {
                        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                        public sealed class ECMAScriptModuleAttribute : Attribute
                        {
                            public ECMAScriptModuleAttribute() { }
                            public ECMAScriptModuleAttribute(string import) { }
                        }
                    }

                    namespace Demo.Pages
                    {
                        [ECMAScript.ECMAScriptModule("./components/todo-app")]
                        public partial class TodoApp : ComponentBase, IVueComponent
                        {
                            [Parameter]
                            public string? Title { get; set; }
                        }
                    }
                    """,
                    options: parseOptions,
                    path: "TodoApp.razor.cs"),
                CSharpSyntaxTree.ParseText(
                    $$"""
                    #line 1 "{{importsPath}}"
                    using Demo.Pages;
                    #line default
                    #line hidden
                    using Microsoft.AspNetCore.Components.Rendering;

                    namespace Demo.Pages
                    {
                        public partial class TodoApp
                        {
                            protected override void BuildRenderTree(RenderTreeBuilder __builder)
                            {
                    #line 1 "{{documentPath}}"
                                __builder.OpenElement(0, "section");
                                __builder.AddContent(1, Title);
                                __builder.CloseElement();
                    #line default
                    #line hidden
                            }
                        }
                    }
                    """,
                    options: parseOptions,
                    path: "TodoApp.razor.g.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var context = Jazor.RazorVue.RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        return context;
    }

    private static string DescribeStructure(RazorVueRenderFragment fragment)
    {
        var builder = new StringBuilder();
        AppendFragment(builder, fragment, depth: 0);
        return builder.ToString();
    }

    private static void AppendFragment(StringBuilder builder, RazorVueRenderFragment fragment, int depth)
    {
        foreach (var node in fragment.Children)
            AppendNode(builder, node, depth);
    }

    private static void AppendNode(StringBuilder builder, RazorVueRenderNode node, int depth)
    {
        builder.Append(' ', depth * 2);

        switch (node)
        {
            case RazorVueElementNode element:
                builder.Append("Element(").Append(element.TagName).Append(')');
                AppendAttributes(builder, element.Attributes, includeOrigins: false);
                builder.AppendLine();
                AppendFragment(builder, element.Children, depth + 1);
                break;
            case RazorVueComponentNode component:
                builder.Append("Component(").Append(component.ComponentName).Append('|').Append(component.ComponentFullName).Append(')');
                AppendAttributes(builder, component.Attributes, includeOrigins: false);
                builder.AppendLine();
                AppendSlotTemplates(builder, component.SlotTemplates, depth + 1);
                AppendFragment(builder, component.Children, depth + 1);
                break;
            case RazorVueTextNode text:
                builder.Append("Text(").Append(text.Text).Append(')');
                builder.AppendLine();
                break;
            case RazorVueExpressionNode expression:
                builder.Append("Expression(").Append(expression.Expression.Syntax.ToString()).Append(')');
                builder.AppendLine();
                break;
            case RazorVueSlotOutletNode slot:
                builder.Append("Slot(").Append(slot.SlotName).Append(')');
                if (slot.Argument is not null)
                    builder.Append(" arg=").Append(slot.Argument.Syntax.ToString());
                builder.AppendLine();
                break;
            case RazorVueConditionalNode conditional:
                builder.Append("Conditional(").Append(conditional.Condition.Syntax.ToString()).Append(')');
                builder.AppendLine();
                AppendFragment(builder, conditional.WhenTrue, depth + 1);
                if (!conditional.WhenFalse.Children.IsDefaultOrEmpty)
                {
                    builder.Append(' ', (depth + 1) * 2).AppendLine("Else");
                    AppendFragment(builder, conditional.WhenFalse, depth + 2);
                }
                break;
            case RazorVueForEachNode loop:
                builder.Append("ForEach(").Append(loop.ItemName).Append(':').Append(loop.Source.Syntax.ToString()).Append(')');
                builder.AppendLine();
                AppendFragment(builder, loop.Body, depth + 1);
                break;
            case RazorVueForNode loop:
                builder.Append("For(")
                    .Append(loop.VariableName)
                    .Append('=').Append(loop.InitialValue.Syntax.ToString())
                    .Append(';').Append(loop.ConditionKind)
                    .Append(':').Append(loop.LimitValue.Syntax.ToString())
                    .Append(';').Append(loop.StepKind);
                if (loop.StepValue is not null)
                    builder.Append(':').Append(loop.StepValue.Syntax.ToString());
                builder.Append(')');
                builder.AppendLine();
                AppendFragment(builder, loop.Body, depth + 1);
                break;
            default:
                builder.Append(node.GetType().Name).AppendLine();
                break;
        }
    }

    private static void AppendAttributes(StringBuilder builder, ImmutableArray<RazorVueAttributeEntry> attributes, bool includeOrigins)
    {
        if (attributes.IsDefaultOrEmpty)
            return;

        builder.Append(" attrs=[");
        for (var index = 0; index < attributes.Length; index++)
        {
            if (index > 0)
                builder.Append(", ");

            switch (attributes[index])
            {
                case RazorVueAttributeNode attribute:
                    builder.Append(attribute.Name).Append('=').Append(attribute.Value?.Syntax.ToString() ?? "true");
                    if (includeOrigins)
                        builder.Append('@').Append(DescribeOrigins(attribute.Origins));
                    break;
                case RazorVueAttributeSpreadNode spread:
                    builder.Append("...").Append(spread.Expression.Syntax.ToString());
                    if (includeOrigins)
                        builder.Append('@').Append(DescribeOrigins(spread.Origins));
                    break;
            }
        }

        builder.Append(']');
    }

    private static void AppendSlotTemplates(StringBuilder builder, ImmutableArray<RazorVueComponentSlotTemplateNode> slotTemplates, int depth)
    {
        foreach (var slotTemplate in slotTemplates)
        {
            builder.Append(' ', depth * 2)
                .Append("SlotTemplate(")
                .Append(slotTemplate.PublicName);
            if (!string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
                builder.Append('|').Append(slotTemplate.ParameterName);
            builder.Append(')').AppendLine();
            AppendFragment(builder, slotTemplate.Children, depth + 1);
        }
    }

    private static int CountOrigins(RazorVueRenderFragment fragment)
        => EnumerateOrigins(fragment).Count();

    private static IEnumerable<RazorVueSourceOrigin> EnumerateOrigins(RazorVueRenderFragment fragment)
    {
        foreach (var node in fragment.Children)
        {
            foreach (var origin in EnumerateOrigins(node))
                yield return origin;
        }
    }

    private static IEnumerable<RazorVueSourceOrigin> EnumerateOrigins(RazorVueRenderNode node)
    {
        foreach (var origin in node.Origins)
            yield return origin;

        switch (node)
        {
            case RazorVueElementNode element:
                foreach (var attribute in element.Attributes)
                {
                    foreach (var origin in attribute.Origins)
                        yield return origin;
                }

                foreach (var child in element.Children.Children)
                {
                    foreach (var origin in EnumerateOrigins(child))
                        yield return origin;
                }
                break;
            case RazorVueComponentNode component:
                foreach (var attribute in component.Attributes)
                {
                    foreach (var origin in attribute.Origins)
                        yield return origin;
                }

                foreach (var slotTemplate in component.SlotTemplates)
                {
                    foreach (var origin in slotTemplate.Origins)
                        yield return origin;
                    foreach (var origin in EnumerateOrigins(slotTemplate.Children))
                        yield return origin;
                }

                foreach (var implicitDefaultSlotAssignment in component.ImplicitDefaultSlotAssignments)
                {
                    foreach (var origin in implicitDefaultSlotAssignment.Origins)
                        yield return origin;
                    foreach (var origin in EnumerateOrigins(implicitDefaultSlotAssignment.Children))
                        yield return origin;
                }

                foreach (var child in component.Children.Children)
                {
                    foreach (var origin in EnumerateOrigins(child))
                        yield return origin;
                }
                break;
            case RazorVueConditionalNode conditional:
                foreach (var origin in EnumerateOrigins(conditional.WhenTrue))
                    yield return origin;
                foreach (var origin in EnumerateOrigins(conditional.WhenFalse))
                    yield return origin;
                break;
            case RazorVueForEachNode loop:
                foreach (var origin in EnumerateOrigins(loop.Body))
                    yield return origin;
                break;
            case RazorVueForNode loop:
                foreach (var origin in EnumerateOrigins(loop.Body))
                    yield return origin;
                break;
        }
    }

    private static string DescribeOrigins(ImmutableArray<RazorVueSourceOrigin> origins)
        => string.Join(
            ";",
            origins.Select(static origin =>
                $"{origin.OriginKind}|{origin.SourceFilePath}|{origin.SourceSpanStart}|{origin.SourceSpanLength}|{origin.MappingQuality}"));
}
