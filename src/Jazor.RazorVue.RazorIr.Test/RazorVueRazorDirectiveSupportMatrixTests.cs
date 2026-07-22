using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RazorSdk;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorVueRazorDirectiveSupportMatrixTests
{
    private const string DocumentPath = @"D:\repo\Demo\Pages\TodoApp.razor";

    [TestMethod]
    public void RazorDirectiveSupportMatrix_SupportedComponentAuthoringFamilies_LowerThroughRazorIrPipeline()
    {
        const string documentText = """
            <section @key="Title" @attributes="AdditionalAttributes">
                @if (Visible)
                {
                    <button @onclick="OnClick">@Title</button>
                }
                <ul>
                @foreach (var item in Items)
                {
                    <li>@item</li>
                }
                </ul>
                <input @bind="Title" />
                <EditorCard @bind-Value="Title" />
            </section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateContext(
            "RazorVue.RazorIr.DirectiveSupportMatrix.Supported.Tests",
            DocumentPath,
            documentText,
            CreateSupportedComponentSource());

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);
        var section = Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children.Single());

        Assert.IsNotNull(section.Key);
        Assert.IsTrue(
            section.Attributes.OfType<RazorVueAttributeSpreadNode>().Any(),
            "The supported matrix fixture must cover @attributes splat lowering.");

        var conditional = Assert.IsInstanceOfType<RazorVueConditionalNode>(
            section.Children.Children.OfType<RazorVueConditionalNode>().Single());
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(conditional.Condition);

        var button = Descendants(section).OfType<RazorVueElementNode>().Single(static node => node.TagName == "button");
        Assert.IsTrue(
            button.Attributes.OfType<RazorVueAttributeNode>().Any(static attribute => attribute.Name == "onclick" && attribute.Value is not null),
            "The supported matrix fixture must cover DOM event directive lowering.");

        var listLoop = Descendants(section).OfType<RazorVueForEachNode>().Single();
        Assert.AreEqual("item", listLoop.ItemName);

        var input = Descendants(section).OfType<RazorVueElementNode>().Single(static node => node.TagName == "input");
        Assert.IsTrue(input.Attributes.OfType<RazorVueAttributeNode>().Any(static attribute => attribute.Name == "value"));
        Assert.IsTrue(input.Attributes.OfType<RazorVueAttributeNode>().Any(static attribute => attribute.Name == "onchange"));

        var editor = Descendants(section).OfType<RazorVueComponentNode>().Single(static node => node.ComponentName == "EditorCard");
        Assert.IsTrue(editor.Attributes.OfType<RazorVueAttributeNode>().Any(static attribute => attribute.Name == "Value"));
        Assert.IsTrue(editor.Attributes.OfType<RazorVueAttributeNode>().Any(static attribute => attribute.Name == "ValueChanged"));

        var artifact = RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "\"onChange\": (__event) =>");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:value\": (__value) => emit(\"update:title\", __value)");
        Assert.IsFalse(artifact.ModuleCode.Contains("@bind", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("@onclick", StringComparison.Ordinal), artifact.ModuleCode);
    }

    [TestMethod]
    public void RazorDirectiveSupportMatrix_ControlledOrPartialFamilies_ReportCurrentBoundaries()
    {
        AssertRazorVueIssue(
            "RazorVue.RazorIr.DirectiveSupportMatrix.BindFormat.Tests",
            """<input @bind="PublishedAt" @bind:format="yyyy-MM-dd" />""",
            """
            using System;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    public DateTime PublishedAt { get; set; }
                }
            }
            """,
            "@bind:format",
            "HTML element @bind");

        AssertRazorVueIssue(
            "RazorVue.RazorIr.DirectiveSupportMatrix.CheckboxBind.Tests",
            """<input type="checkbox" @bind="Done" />""",
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    public bool Done { get; set; }
                }
            }
            """,
            "input type 'checkbox'",
            "HTML element @bind");

        AssertRazorVueIssue(
            "RazorVue.RazorIr.DirectiveSupportMatrix.ComponentRef.Tests",
            """<ChildCard @ref="_child" />""",
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/child-card")]
                public partial class ChildCard : ComponentBase, IVueComponent
                {
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private ChildCard? _child;
                }
            }
            """,
            "component reference capture",
            "Blazor component instance semantics");
    }

    [TestMethod]
    public void RazorDirectiveSupportMatrix_NonTargetHostRuntimeDirectives_DoNotBecomeSilentVueSemantics()
    {
        const string documentText = """
            @layout MainLayout
            @inject Microsoft.AspNetCore.Components.NavigationManager Navigation
            @rendermode InteractiveServer
            <section>ready</section>
            """;

        var artifact = LowerSfc(
            "RazorVue.RazorIr.DirectiveSupportMatrix.HostRuntime.Transparent.Tests",
            documentText,
            """
            namespace Demo.Pages
            {
                public sealed class MainLayout : LayoutComponentBase
                {
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                }
            }
            """,
            """
            @using Demo.Pages
            @using static Microsoft.AspNetCore.Components.Web.RenderMode
            """);

        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
        Assert.IsFalse(artifact.SfcText.Contains("Navigation", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("MainLayout", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("InteractiveServer", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("RenderMode", StringComparison.Ordinal), artifact.SfcText);

        AssertRazorVueIssue(
            "RazorVue.RazorIr.DirectiveSupportMatrix.FormName.Tests",
            """<form @formname="todo"><input /></form>""",
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource(),
            "@formname",
            ".vue artifact");
    }

    [TestMethod]
    public void RazorDirectiveSupportMatrix_UnsupportedRenderShapes_StopAtOfficialOrRazorVueBoundary()
    {
        var exception = Assert.ThrowsExactly<AssertFailedException>(() =>
            RazorVueRazorIrTestContextFactory.CreateAlignedContext(
                "RazorVue.RazorIr.DirectiveSupportMatrix.AsyncRender.Tests",
                DocumentPath,
                """
                @{
                    await System.Threading.Tasks.Task.CompletedTask;
                    <section>ready</section>
                }
                """,
                RazorVueRazorIrTestContextFactory.CreateParentComponentSource()));

        StringAssert.Contains(exception.Message, "CS4033");
    }

    private static VueSfcArtifact LowerSfc(
        string assemblyName,
        string documentText,
        string componentSource,
        string? importsText = "@using Demo.Pages")
    {
        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateContext(
            assemblyName,
            DocumentPath,
            documentText,
            componentSource,
            importsText);

        return new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);
    }

    private static void AssertRazorVueIssue(
        string assemblyName,
        string documentText,
        string componentSource,
        params string[] expectedMessageFragments)
    {
        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateContext(
            assemblyName,
            DocumentPath,
            documentText,
            componentSource);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        foreach (var fragment in expectedMessageFragments)
            StringAssert.Contains(exception.Issue.Message, fragment);
    }

    private static IEnumerable<RazorVueRenderNode> Descendants(RazorVueRenderNode node)
    {
        foreach (var child in Children(node))
        {
            yield return child;
            foreach (var descendant in Descendants(child))
                yield return descendant;
        }
    }

    private static ImmutableArray<RazorVueRenderNode> Children(RazorVueRenderNode node)
        => node switch
        {
            RazorVueElementNode element => element.Children.Children,
            RazorVueComponentNode component => component.Children.Children
                                             .AddRange(component.AmbientDefaultSlotChildren.Children)
                                             .AddRange(component.SlotTemplates.SelectMany(static slot => slot.Children.Children))
                                             .AddRange(component.ImplicitDefaultSlotAssignments.SelectMany(static assignment => assignment.Children.Children)),
            RazorVueConditionalNode conditional => conditional.WhenTrue.Children.AddRange(conditional.WhenFalse.Children),
            RazorVueForEachNode loop => loop.Body.Children,
            RazorVueForNode loop => loop.Body.Children,
            RazorVueTemplateScopeNode scope => scope.Children.Children,
            _ => ImmutableArray<RazorVueRenderNode>.Empty
        };

    private static string CreateSupportedComponentSource()
        => """
        using System.Collections.Generic;

        namespace Demo.Pages
        {
            [ECMAScript.ECMAScriptModule("./components/editor-card")]
            public partial class EditorCard : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Value { get; set; }

                [Parameter]
                public EventCallback<string?> ValueChanged { get; set; }
            }

            [ECMAScript.ECMAScriptModule("./components/todo-app")]
            public partial class TodoApp : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Title { get; set; }

                [Parameter]
                public EventCallback<string?> TitleChanged { get; set; }

                [Parameter]
                public bool Visible { get; set; }

                [Parameter]
                public IReadOnlyList<string> Items { get; set; } = new List<string>();

                [Parameter(CaptureUnmatchedValues = true)]
                public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

                private void OnClick()
                {
                }
            }
        }
        """;
}
