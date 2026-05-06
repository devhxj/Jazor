using System.IO;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
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

        var titleAttribute = element.Attributes.Single(static attribute => attribute.Name == "title");
        Assert.IsNotNull(titleAttribute.Value);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleAttribute.Value);
        Assert.AreEqual("Title", ((IPropertyReferenceOperation)titleAttribute.Value).Property.Name);

        var classAttribute = element.Attributes.Single(static attribute => attribute.Name == "class");
        Assert.IsNotNull(classAttribute.Value);
        Assert.IsTrue(classAttribute.Value.ConstantValue.HasValue);
        Assert.AreEqual("hero", classAttribute.Value.ConstantValue.Value);
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
        var titleAttribute = component.Attributes[0];
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
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerSimpleMarkup()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<section><h1>@Title</h1></section>""";

        var (context, _) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = new Jazor.RazorVue.RazorVuePipeline(RazorVueRazorDocumentSemanticFrontend.Instance, new RazorVueRazorIrTemplateFrontend())
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

        var (context, _) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
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

        var artifact = new Jazor.RazorVue.RazorVuePipeline(RazorVueRazorDocumentSemanticFrontend.Instance, new RazorVueRazorIrTemplateFrontend())
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "props.items.length > 0");
        StringAssert.Contains(artifact.ModuleCode, ".map((item) => h(\"li\", item))");
        StringAssert.Contains(artifact.ModuleCode, "h(\"ul\"");
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

        var (context, _) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
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

        var artifact = new Jazor.RazorVue.RazorVuePipeline(RazorVueRazorDocumentSemanticFrontend.Instance, new RazorVueRazorIrTemplateFrontend())
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "props.visible ? h(\"p\", \"Visible\") : h(\"p\", \"Hidden\")");
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

        var (context, _) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
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

        var artifact = new Jazor.RazorVue.RazorVuePipeline(RazorVueRazorDocumentSemanticFrontend.Instance, new RazorVueRazorIrTemplateFrontend())
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "props.visible ? h(\"p\", \"Visible\") : h(\"ul\"");
        StringAssert.Contains(artifact.ModuleCode, ".map((item) => h(\"li\", item))");
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

        var (context, _) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
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

        var artifact = new Jazor.RazorVue.RazorVuePipeline(RazorVueRazorDocumentSemanticFrontend.Instance, new RazorVueRazorIrTemplateFrontend())
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "props.primary ? h(\"p\", \"Primary\") : (props.secondary ? h(\"p\", \"Secondary\") : h(\"p\", \"Fallback\"))");
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
        StringAssert.Contains(exception.Message, "single for-loop iterator expression");
    }

}
