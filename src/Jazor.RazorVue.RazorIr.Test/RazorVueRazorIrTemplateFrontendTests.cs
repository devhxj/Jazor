using System.Collections.Immutable;
using System.IO;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RazorSdk;
using Jazor.RazorVue.RenderTree;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorVueRazorIrTemplateFrontendTests
{
    [TestMethod]
    public void CreateRenderTree_ForMarkupAndInterpolation_UsesIrStructureAndRoslynExpression()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<section><h1>@Title</h1><p>Hello</p></section>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Markup.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var frontend = new RazorVueRazorIrTemplateFrontend();
        var renderTree = frontend.CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length);

        var section = renderTree.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(RazorVueMappingQuality.ExactSource, section.Origins[0].MappingQuality);
        Assert.AreEqual(documentPath, section.Origins[0].SourceFilePath);

        Assert.AreEqual(2, section.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));

        var h1 = section.Children.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(h1);
        var expression = h1.Children.Children[0] as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(expression.Expression);
        Assert.AreEqual("Title", ((IPropertyReferenceOperation)expression.Expression).Property.Name);

        var paragraph = section.Children.Children[1] as RazorVueElementNode;
        Assert.IsNotNull(paragraph);
        var text = paragraph.Children.Children[0] as RazorVueTextNode;
        Assert.IsNotNull(text);
        Assert.AreEqual("Hello", text.Text);
    }

    [TestMethod]
    public void CreateRenderTree_ForLiteralAndExpressionAttributes_ResolvesAttributeValues()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<div title="@Title" class="hero">Hello</div>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Attribute.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var frontend = new RazorVueRazorIrTemplateFrontend();
        var renderTree = frontend.CreateRenderTree(context, snapshot);

        var element = renderTree.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(element);
        Assert.AreEqual(2, element.Attributes.Length);

        var titleAttribute = element.Attributes.OfType<RazorVueAttributeNode>().Single(static attribute => attribute.Name == "title");
        Assert.IsNotNull(titleAttribute.Value);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleAttribute.Value);
        Assert.AreEqual("Title", ((IPropertyReferenceOperation)titleAttribute.Value).Property.Name);

        var classAttribute = element.Attributes.OfType<RazorVueAttributeNode>().Single(static attribute => attribute.Name == "class");
        Assert.IsNotNull(classAttribute.Value);
        Assert.IsTrue(classAttribute.Value.ConstantValue.HasValue);
        Assert.AreEqual("hero", classAttribute.Value.ConstantValue.Value);
    }

    [TestMethod]
    public void CreateRenderTree_ForStaticAttributeSplitAcrossLiteralIrNodes_ConcatenatesValue()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<div title="@Title" class="todo-card todo-card--active">Hello</div>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.StaticSplitAttribute.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        Assert.IsNotNull(snapshot.RazorSourceGeneratorDocument);
        var sourceGeneratorDocument = snapshot.RazorSourceGeneratorDocument!;
        var splitDocument = sourceGeneratorDocument with
        {
            DocumentNode = SplitFirstClassAttributeIntoLiteralTokens(sourceGeneratorDocument.DocumentNode)
        };
        var splitSnapshot = snapshot with
        {
            RazorSourceGeneratorDocument = splitDocument
        };

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, splitSnapshot);

        var element = renderTree.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(element);
        var classAttribute = element.Attributes.OfType<RazorVueAttributeNode>().Single(static attribute => attribute.Name == "class");
        Assert.IsNotNull(classAttribute.Value);
        Assert.IsTrue(classAttribute.Value.ConstantValue.HasValue);
        Assert.AreEqual("todo-card todo-card--active", classAttribute.Value.ConstantValue.Value);
    }

    [TestMethod]
    public void CreateRenderTree_ForMixedStaticAndExpressionAttributeContent_StillThrowsExplicitFailure()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<div class="todo-card @Title">Hello</div>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.MixedAttribute.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "mixed attribute content");
        StringAssert.Contains(exception.Message, "class");
    }

    [TestMethod]
    public void CreateRenderTree_ForElementSplat_ProducesAttributeSpread()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<div title="@Title" @attributes="AdditionalAttributes">Hello</div>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ElementSplat.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);
        var element = renderTree.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(element);
        Assert.AreEqual(2, element.Attributes.Length);
        Assert.IsInstanceOfType<RazorVueAttributeNode>(element.Attributes[0]);
        Assert.IsInstanceOfType<RazorVueAttributeSpreadNode>(element.Attributes[1]);
    }

    [TestMethod]
    public void CreateRenderTree_ForElementBind_CurrentHostStillExposesRawBindAttribute()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<input @bind="Title" />""";

        var codeDocument = RazorIrTestHost.CreateCodeDocument(
            documentPath,
            documentText,
            importSources: [],
            tagHelpers: null);
        var tree = RazorIrTestHost.DumpIntermediateNodeTree(RazorIrTestHost.GetDocumentNode(codeDocument));

        StringAssert.Contains(tree, "HtmlAttributeIntermediateNode AttributeName=\"@bind\"");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForElementBind_PreservesRawBindAttribute()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<input @bind="Title" />""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateContext(
            "RazorVue.RazorIr.TemplateFrontend.ElementBind.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);
        var input = renderTree.Children[0] as RazorVueElementNode;

        Assert.IsNotNull(input);
        Assert.AreEqual("input", input!.TagName);
        Assert.IsTrue(input.Attributes.OfType<RazorVueAttributeNode>().Any(static item => item.Name == "@bind"));
    }

    [TestMethod]
    public void CreateRenderTree_ForComponentBindAttribute_ProducesValueAndValueChangedAttributes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<EditorCard @bind-Value="Title" />""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ComponentBind.Tests",
            documentPath,
            documentText,
            """
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
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);
        var component = renderTree.Children[0] as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual("EditorCard", component.ComponentName);
        Assert.AreEqual(2, component.Attributes.Length);

        var valueAttribute = component.Attributes.OfType<RazorVueAttributeNode>().Single(static attribute => attribute.Name == "Value");
        Assert.IsNotNull(valueAttribute.Value);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(valueAttribute.Value);

        var valueChangedAttribute = component.Attributes.OfType<RazorVueAttributeNode>().Single(static attribute => attribute.Name == "ValueChanged");
        Assert.IsNotNull(valueChangedAttribute.Value);
        Assert.IsInstanceOfType<IInvocationOperation>(valueChangedAttribute.Value);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerComponentBindAttribute()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<EditorCard @bind-Value="Title" />""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ComponentBind.Pipeline.Tests",
            documentPath,
            documentText,
            """
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
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "\"value\": props.title");
        StringAssert.Contains(artifact.ModuleCode, "\"onUpdate:value\": (__value) => emit(\"update:title\", __value)");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForComponentBindWithoutBindableHostTarget_ReportsInvalidBindTarget()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<EditorCard @bind-Value="Title" />""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ComponentBind.InvalidTarget.Tests",
            documentPath,
            documentText,
            """
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
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.InvalidBindTarget, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "Title");
    }

    [TestMethod]
    public void CreateRenderTree_ForComponentAndDefaultChildContent_ProducesComponentNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<ChildCard Title="@Title"><p>Body</p></ChildCard>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Component.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentAndChildComponentSource());

        var frontend = new RazorVueRazorIrTemplateFrontend();
        var renderTree = frontend.CreateRenderTree(context, snapshot);

        var component = renderTree.Children[0] as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual("ChildCard", component.ComponentName);
        Assert.AreEqual("ChildCard", component.ResolutionName);
        StringAssert.Contains(component.ComponentFullName, "Demo.Pages.ChildCard");

        Assert.AreEqual(1, component.Attributes.Length);
        var titleAttribute = Assert.IsInstanceOfType<RazorVueAttributeNode>(component.Attributes[0]);
        Assert.AreEqual("Title", titleAttribute.Name);
        Assert.IsNotNull(titleAttribute.Value);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleAttribute.Value);

        var childElement = component.Children.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(childElement);
        Assert.AreEqual("p", childElement.TagName);
        var bodyText = childElement.Children.Children[0] as RazorVueTextNode;
        Assert.IsNotNull(bodyText);
        Assert.AreEqual("Body", bodyText.Text);
    }

    [TestMethod]
    public void CreateRenderTree_ForComponentRawClassFallthrough_ProducesLiteralClassAttribute()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<VChip class="playground-category-chip" Text="@Title" />""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Component.ClassFallthrough.Tests",
            documentPath,
            documentText,
            """
            using ECMAScript.Vuetify;

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
            """
            @using Demo.Pages
            @using ECMAScript.Vuetify
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);
        var component = renderTree.Children[0] as RazorVueComponentNode;

        Assert.IsNotNull(component, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.AreEqual("VChip", component.ComponentName);

        var classAttribute = component.Attributes
            .OfType<RazorVueAttributeNode>()
            .Single(static attribute => attribute.Name == "class");
        Assert.IsNotNull(classAttribute.Value);
        Assert.IsTrue(classAttribute.Value.ConstantValue.HasValue);
        Assert.AreEqual("playground-category-chip", classAttribute.Value.ConstantValue.Value);

        var textAttribute = component.Attributes
            .OfType<RazorVueAttributeNode>()
            .Single(static attribute => attribute.Name == "Text");
        Assert.IsNotNull(textAttribute.Value);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(textAttribute.Value);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerComponentRawClassFallthrough()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<VChip class="playground-category-chip" Text="@Title" />""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Component.ClassFallthrough.Pipeline.Tests",
            documentPath,
            documentText,
            """
            using ECMAScript.Vuetify;

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
            """
            @using Demo.Pages
            @using ECMAScript.Vuetify
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "\"class\": \"playground-category-chip\"");
        StringAssert.Contains(artifact.ModuleCode, "\"text\": props.title");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerComponentCssClassPropToRuntimeClass()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<VChip CssClass='@("typed-category-chip")' CssStyle='@("margin-inline: 1rem")' Text="@Title" />""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Component.CssClassProp.Pipeline.Tests",
            documentPath,
            documentText,
            """
            using ECMAScript.Vuetify;

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
            """
            @using Demo.Pages
            @using ECMAScript.Vuetify
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "\"class\": \"typed-category-chip\"");
        StringAssert.Contains(artifact.ModuleCode, "\"style\": \"margin-inline: 1rem\"");
        StringAssert.Contains(artifact.ModuleCode, "\"text\": props.title");
    }

    [TestMethod]
    public void CreateRenderTree_ForComponentNamedAndTypedChildContent_ProducesStructuredSlots()
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
            "RazorVue.RazorIr.TemplateFrontend.NamedTypedChildContent.Tests",
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

        var frontend = new RazorVueRazorIrTemplateFrontend();
        var renderTree = frontend.CreateRenderTree(context, snapshot);

        var component = renderTree.Children[0] as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual("LayoutCard", component.ComponentName);
        Assert.AreEqual("LayoutCard", component.ResolutionName);
        Assert.AreEqual(1, component.Attributes.Length);
        Assert.AreEqual("Title", Assert.IsInstanceOfType<RazorVueAttributeNode>(component.Attributes[0]).Name);
        Assert.AreEqual(2, component.SlotTemplates.Length);
        Assert.AreEqual(0, component.Children.Children.Length);

        var headerSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "Header");
        Assert.AreEqual("header", headerSlot.SlotName);
        Assert.IsNull(headerSlot.ParameterName);
        var headerElement = headerSlot.Children.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(headerElement);
        Assert.AreEqual("h1", headerElement.TagName);

        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual("itemTemplate", itemTemplateSlot.SlotName);
        Assert.AreEqual("item", itemTemplateSlot.ParameterName);
        var paragraph = itemTemplateSlot.Children.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual("p", paragraph.TagName);
        var itemExpression = paragraph.Children.Children[0] as RazorVueExpressionNode;
        Assert.IsNotNull(itemExpression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(itemExpression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_ForOfficialSgComponentAttributesAndNestedControlFlow_LowersTokenOnlyIr()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <PanelCard Fluid="true" Density="compact">
                @foreach (var item in Items!)
                {
                    @if (ShowCompleted || !item.IsDone)
                    {
                        <ItemCard Title="@item.Title"
                                  Subtitle="@(item.Category + " | " + (item.IsDone ? "Completed" : "Active"))">
                            @if (item.IsPinned)
                            {
                                <ChipCard Text="Pinned" Color="primary" />
                            }
                        </ItemCard>
                    }
                }
            </PanelCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.OfficialSg.TokenOnly.Tests",
            documentPath,
            documentText,
            """
            using System.Collections.Generic;

            namespace Demo.Pages
            {
                public sealed class TodoItem
                {
                    public string? Title { get; set; }
                    public string? Category { get; set; }
                    public bool IsDone { get; set; }
                    public bool IsPinned { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/panel-card")]
                public partial class PanelCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Fluid { get; set; }

                    [Parameter]
                    public string? Density { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/item-card")]
                public partial class ItemCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public string? Subtitle { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/chip-card")]
                public partial class ChipCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Text { get; set; }

                    [Parameter]
                    public string? Color { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public List<TodoItem>? Items { get; set; }

                    [Parameter]
                    public bool ShowCompleted { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var panel = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var fluid = panel.Attributes.OfType<RazorVueAttributeNode>().Single(static attribute => attribute.Name == "Fluid");
        Assert.IsNotNull(fluid.Value);
        Assert.AreEqual("true", fluid.Value.Syntax.ToString());

        var density = panel.Attributes.OfType<RazorVueAttributeNode>().Single(static attribute => attribute.Name == "Density");
        Assert.IsNotNull(density.Value);
        Assert.AreEqual("\"compact\"", density.Value.Syntax.ToString());

        var loop = Assert.IsInstanceOfType<RazorVueForEachNode>(panel.Children.Children.Single());
        Assert.AreEqual("item", loop.ItemName);

        var conditional = Assert.IsInstanceOfType<RazorVueConditionalNode>(loop.Body.Children.Single());
        Assert.IsInstanceOfType<IBinaryOperation>(conditional.Condition);

        var itemCard = Assert.IsInstanceOfType<RazorVueComponentNode>(conditional.WhenTrue.Children.Single());
        Assert.AreEqual("ItemCard", itemCard.ComponentName);
        Assert.AreEqual(2, itemCard.Attributes.Length);

        var pinnedConditional = Assert.IsInstanceOfType<RazorVueConditionalNode>(itemCard.Children.Children.Single());
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(pinnedConditional.Condition);

        var chip = Assert.IsInstanceOfType<RazorVueComponentNode>(pinnedConditional.WhenTrue.Children.Single());
        Assert.AreEqual("ChipCard", chip.ComponentName);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerSimpleMarkup()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<section><h1>@Title</h1></section>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "h(\"section\"");
        StringAssert.Contains(artifact.ModuleCode, "props.title");
        Assert.IsTrue(
            artifact.SourceOrigins.Any(origin =>
                origin.MappingQuality == RazorVueMappingQuality.ExactSource &&
                string.Equals(origin.SourceFilePath, documentPath, StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerNamedAndTypedChildContent()
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
            "RazorVue.RazorIr.TemplateFrontend.NamedTypedChildContent.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "\"title\": props.title");
        StringAssert.Contains(artifact.ModuleCode, "header: () => h(\"h1\", null, props.title)");
        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => h(\"p\", null, item)");
    }

    [TestMethod]
    public void CreateRenderTree_ForIfAndForeach_LowersMinimalStructuredControlFlow()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @if (Items.Count > 0)
            {
                <ul>
                @foreach (var item in Items)
                {
                    <li>@item</li>
                }
                </ul>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ControlFlow.Tests",
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
                    public List<string> Items { get; set; } = new();
                }
            }
            """);

        var frontend = new RazorVueRazorIrTemplateFrontend();
        var renderTree = frontend.CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));

        var conditional = renderTree.Children[0] as RazorVueConditionalNode;
        Assert.IsNotNull(conditional);
        Assert.IsInstanceOfType<IBinaryOperation>(conditional.Condition);

        var ul = conditional.WhenTrue.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(ul);
        Assert.AreEqual("ul", ul.TagName);

        var loop = ul.Children.Children[0] as RazorVueForEachNode;
        Assert.IsNotNull(loop);
        Assert.AreEqual("item", loop.ItemName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(loop.Source);

        var li = loop.Body.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(li);
        Assert.AreEqual("li", li.TagName);
        var itemExpression = li.Children.Children[0] as RazorVueExpressionNode;
        Assert.IsNotNull(itemExpression);
        Assert.IsInstanceOfType<ILocalReferenceOperation>(itemExpression.Expression);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerMinimalIfAndForeach()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @if (Items.Count > 0)
            {
                <ul>
                @foreach (var item in Items)
                {
                    <li>@item</li>
                }
                </ul>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ControlFlow.Pipeline.Tests",
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
                    public List<string> Items { get; set; } = new();
                }
            }
            """);

        var artifact = RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "props.items.length > 0");
        StringAssert.Contains(artifact.ModuleCode, ".map((item) => h(\"li\", null, item))");
        StringAssert.Contains(artifact.ModuleCode, "h(\"ul\"");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanProjectUserDtoPropertiesInsideForeach()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <ul>
            @foreach (var item in Items!)
            {
                @if (!item.IsDone)
                {
                    <li>@item.Title</li>
                }
            }
            </ul>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DtoProjection.Pipeline.Tests",
            documentPath,
            documentText,
            """
            using System.Collections.Generic;

            namespace Demo.Pages
            {
                public sealed record TodoItem(
                    int Id,
                    string Title,
                    bool IsDone);

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public List<TodoItem>? Items { get; set; }
                }
            }
            """);

        var artifact = RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "props.items.map((item)");
        StringAssert.Contains(artifact.ModuleCode, "!item.IsDone");
        StringAssert.Contains(artifact.ModuleCode, "h(\"li\", null, item.Title)");
    }

    [TestMethod]
    public void CreateRenderTree_ForIfElse_LowersStructuredElseBranch()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @if (Visible)
            {
                <p>Visible</p>
            }
            else
            {
                <p>Hidden</p>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.IfElse.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Visible { get; set; }
                }
            }
            """);

        var frontend = new RazorVueRazorIrTemplateFrontend();
        var renderTree = frontend.CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));

        var conditional = renderTree.Children[0] as RazorVueConditionalNode;
        Assert.IsNotNull(conditional);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(conditional.Condition);

        var visibleParagraph = conditional.WhenTrue.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(visibleParagraph);
        Assert.AreEqual("p", visibleParagraph.TagName);
        Assert.AreEqual("Visible", ((RazorVueTextNode)visibleParagraph.Children.Children[0]).Text);

        var hiddenParagraph = conditional.WhenFalse.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(hiddenParagraph);
        Assert.AreEqual("p", hiddenParagraph.TagName);
        Assert.AreEqual("Hidden", ((RazorVueTextNode)hiddenParagraph.Children.Children[0]).Text);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerIfElse()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @if (Visible)
            {
                <p>Visible</p>
            }
            else
            {
                <p>Hidden</p>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.IfElse.Pipeline.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Visible { get; set; }
                }
            }
            """);

        var artifact = RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "props.visible ? h(\"p\", null, \"Visible\") : h(\"p\", null, \"Hidden\")");
    }

    [TestMethod]
    public void CreateRenderTree_ForIfElseWithForeachInElse_LowersNestedStructuredControlFlow()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @if (Visible)
            {
                <p>Visible</p>
            }
            else
            {
                <ul>
                @foreach (var item in Items)
                {
                    <li>@item</li>
                }
                </ul>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.IfElseForeach.Tests",
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
                    public bool Visible { get; set; }

                    [Parameter]
                    public List<string> Items { get; set; } = new();
                }
            }
            """);

        var frontend = new RazorVueRazorIrTemplateFrontend();
        var renderTree = frontend.CreateRenderTree(context, snapshot);

        var conditional = renderTree.Children[0] as RazorVueConditionalNode;
        Assert.IsNotNull(conditional);

        var elseList = conditional.WhenFalse.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(elseList);
        Assert.AreEqual("ul", elseList.TagName);

        var loop = elseList.Children.Children[0] as RazorVueForEachNode;
        Assert.IsNotNull(loop);
        Assert.AreEqual("item", loop.ItemName);

        var itemElement = loop.Body.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(itemElement);
        Assert.AreEqual("li", itemElement.TagName);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerIfElseWithForeachInElse()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @if (Visible)
            {
                <p>Visible</p>
            }
            else
            {
                <ul>
                @foreach (var item in Items)
                {
                    <li>@item</li>
                }
                </ul>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.IfElseForeach.Pipeline.Tests",
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
                    public bool Visible { get; set; }

                    [Parameter]
                    public List<string> Items { get; set; } = new();
                }
            }
            """);

        var artifact = RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "props.visible ? h(\"p\", null, \"Visible\") : h(\"ul\"");
        StringAssert.Contains(artifact.ModuleCode, ".map((item) => h(\"li\", null, item))");
    }

    [TestMethod]
    public void CreateRenderTree_ForElseIf_LowersConditionalChain()
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
            "RazorVue.RazorIr.TemplateFrontend.ElseIf.Unsupported.Tests",
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

        var frontend = new RazorVueRazorIrTemplateFrontend();
        var renderTree = frontend.CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));

        var primaryConditional = renderTree.Children[0] as RazorVueConditionalNode;
        Assert.IsNotNull(primaryConditional);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(primaryConditional.Condition);

        var primaryParagraph = primaryConditional.WhenTrue.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(primaryParagraph);
        Assert.AreEqual("Primary", ((RazorVueTextNode)primaryParagraph.Children.Children[0]).Text);

        var secondaryConditional = primaryConditional.WhenFalse.Children[0] as RazorVueConditionalNode;
        Assert.IsNotNull(secondaryConditional);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(secondaryConditional.Condition);

        var secondaryParagraph = secondaryConditional.WhenTrue.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(secondaryParagraph);
        Assert.AreEqual("Secondary", ((RazorVueTextNode)secondaryParagraph.Children.Children[0]).Text);

        var fallbackParagraph = secondaryConditional.WhenFalse.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(fallbackParagraph);
        Assert.AreEqual("Fallback", ((RazorVueTextNode)fallbackParagraph.Children.Children[0]).Text);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerElseIf()
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
            "RazorVue.RazorIr.TemplateFrontend.ElseIf.Pipeline.Tests",
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

        var artifact = RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "props.primary ? h(\"p\", null, \"Primary\") : (props.secondary ? h(\"p\", null, \"Secondary\") : h(\"p\", null, \"Fallback\"))");
    }

    [TestMethod]
    public void CreateRenderTree_ForElseIfChainWithoutFinalElse_LowersNestedConditionalAndEmptyTail()
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
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ElseIf.NoFinalElse.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);
        var primaryConditional = renderTree.Children[0] as RazorVueConditionalNode;
        Assert.IsNotNull(primaryConditional);

        var secondaryConditional = primaryConditional.WhenFalse.Children[0] as RazorVueConditionalNode;
        Assert.IsNotNull(secondaryConditional);
        Assert.AreEqual(0, secondaryConditional.WhenFalse.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_ForMultiStageElseIfChain_LowersAllConditionalLevels()
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
            else if (Tertiary)
            {
                <p>Tertiary</p>
            }
            else
            {
                <p>Fallback</p>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ElseIf.MultiStage.Tests",
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

                    [Parameter]
                    public bool Tertiary { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);
        var primaryConditional = renderTree.Children[0] as RazorVueConditionalNode;
        Assert.IsNotNull(primaryConditional);

        var secondaryConditional = primaryConditional.WhenFalse.Children[0] as RazorVueConditionalNode;
        Assert.IsNotNull(secondaryConditional);

        var tertiaryConditional = secondaryConditional.WhenFalse.Children[0] as RazorVueConditionalNode;
        Assert.IsNotNull(tertiaryConditional);

        var tertiaryParagraph = tertiaryConditional.WhenTrue.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(tertiaryParagraph);
        Assert.AreEqual("Tertiary", ((RazorVueTextNode)tertiaryParagraph.Children.Children[0]).Text);

        var fallbackParagraph = tertiaryConditional.WhenFalse.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(fallbackParagraph);
        Assert.AreEqual("Fallback", ((RazorVueTextNode)fallbackParagraph.Children.Children[0]).Text);
    }

    [TestMethod]
    public void CreateRenderTree_ForLoop_CreatesCountStyleForNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @for (var i = 0; i < Count; i++)
            {
                <p>@i</p>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ForLoop.Unsupported.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);
        var loop = renderTree.Children[0] as RazorVueForNode;
        Assert.IsNotNull(loop);
        Assert.AreEqual("i", loop.VariableName);
        Assert.AreEqual(RazorVueForConditionKind.LessThan, loop.ConditionKind);
        Assert.AreEqual(RazorVueForStepKind.Increment, loop.StepKind);
        Assert.AreEqual("0", loop.InitialValue.Syntax.ToString());
        Assert.AreEqual("Count", loop.LimitValue.Syntax.ToString());
        var paragraph = loop.Body.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(paragraph);
        var expression = paragraph.Children.Children[0] as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.AreEqual("i", expression.Expression.Syntax.ToString());
    }

    [TestMethod]
    public void CreateRenderTree_ForLoop_WithAddAssignStep_CreatesCountStyleForNodeWithStepValue()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @for (var i = Start; i <= Count; i += Step)
            {
                <p>@i</p>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ForLoop.AddAssign.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Start { get; set; }

                    [Parameter]
                    public int Count { get; set; }

                    [Parameter]
                    public int Step { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);
        var loop = renderTree.Children[0] as RazorVueForNode;
        Assert.IsNotNull(loop);
        Assert.AreEqual("i", loop.VariableName);
        Assert.AreEqual(RazorVueForConditionKind.LessThanOrEqual, loop.ConditionKind);
        Assert.AreEqual(RazorVueForStepKind.AddAssign, loop.StepKind);
        Assert.AreEqual("Start", loop.InitialValue.Syntax.ToString());
        Assert.AreEqual("Count", loop.LimitValue.Syntax.ToString());
        Assert.IsNotNull(loop.StepValue);
        Assert.AreEqual("Step", loop.StepValue.Syntax.ToString());
    }

    [TestMethod]
    public void CreateRenderTree_ForLoop_WithStaticallyZeroStep_ThrowsExplicitFailure()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @for (var i = 0; i < 3; i += 0)
            {
                <p>@i</p>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ForLoop.ZeroStep.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                }
            }
            """);

        var frontend = new RazorVueRazorIrTemplateFrontend();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => frontend.CreateRenderTree(context, snapshot));
        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Message, "step becomes zero");
    }

    [TestMethod]
    public void CreateRenderTree_ForLoop_WithMultipleIteratorExpressions_ThrowsExplicitFailure()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @for (var i = 0; i < Count; i++, Total++)
            {
                <p>@i</p>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ForLoop.UnsupportedShape.Tests",
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

                    [Parameter]
                    public int Total { get; set; }
                }
            }
            """);

        var frontend = new RazorVueRazorIrTemplateFrontend();
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() => frontend.CreateRenderTree(context, snapshot));
        StringAssert.Contains(exception.Message, "only supports count-style for-loops");
    }

    private static RazorVueRazorIrNode SplitFirstClassAttributeIntoLiteralTokens(RazorVueRazorIrNode root)
    {
        var replaced = false;
        var rewritten = Rewrite(root, ref replaced);
        Assert.IsTrue(replaced, "The test fixture did not contain a class attribute to rewrite.");
        return rewritten;

        static RazorVueRazorIrNode Rewrite(RazorVueRazorIrNode node, ref bool replaced)
        {
            if (!replaced &&
                node.Kind == RazorVueRazorIrNodeKind.HtmlAttribute &&
                string.Equals(node.AttributeName, "class", StringComparison.Ordinal))
            {
                replaced = true;
                return node with
                {
                    Children = ImmutableArray.Create(
                        CreateStaticAttributeValue(prefix: string.Empty, content: "todo-card"),
                        CreateStaticAttributeValue(prefix: " ", content: "todo-card--active"))
                };
            }

            return node with
            {
                Children = RewriteNodes(node.Children, ref replaced),
                Attributes = RewriteNodes(node.Attributes, ref replaced),
                Body = RewriteNodes(node.Body, ref replaced),
                Splats = RewriteNodes(node.Splats, ref replaced),
                ChildContents = RewriteNodes(node.ChildContents, ref replaced),
                Captures = RewriteNodes(node.Captures, ref replaced),
                SetKeys = RewriteNodes(node.SetKeys, ref replaced)
            };
        }

        static ImmutableArray<RazorVueRazorIrNode> RewriteNodes(
            ImmutableArray<RazorVueRazorIrNode> nodes,
            ref bool replaced)
        {
            if (nodes.IsDefaultOrEmpty)
                return nodes;

            var builder = ImmutableArray.CreateBuilder<RazorVueRazorIrNode>(nodes.Length);
            foreach (var child in nodes)
                builder.Add(Rewrite(child, ref replaced));

            return builder.ToImmutable();
        }

        static RazorVueRazorIrNode CreateStaticAttributeValue(string prefix, string content)
            => new(
                RazorVueRazorIrNodeKind.HtmlAttributeValue,
                "Microsoft.AspNetCore.Razor.Language.Intermediate.HtmlAttributeValueIntermediateNode",
                ImmutableArray<RazorVueRazorIrNode>.Empty,
                ImmutableArray.Create(new RazorVueRazorIrToken(content, Source: null)),
                Source: null,
                Prefix: prefix);
    }
}
