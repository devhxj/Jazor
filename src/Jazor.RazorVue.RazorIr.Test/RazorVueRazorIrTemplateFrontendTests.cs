using System.Collections.Immutable;
using System.IO;
using System.Text;
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
    public void CreateRenderTree_ForElementDomEventWithModifiers_PreservesModifierMetadata()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<button @onclick="OnClick" @onclick:preventDefault="true" @onclick:stopPropagation="StopClick">Go</button>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.EventModifiers.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool StopClick { get; set; }

                    private void OnClick()
                    {
                    }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var button = Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children.Single());
        var clickAttribute = button.Attributes.OfType<RazorVueAttributeNode>().Single(static attribute => attribute.Name == "onclick");

        Assert.IsNotNull(clickAttribute.Value);
        Assert.IsNotNull(clickAttribute.EventModifiers.PreventDefault);
        Assert.IsTrue(clickAttribute.EventModifiers.PreventDefault.Value.ConstantValue.HasValue);
        Assert.AreEqual(true, clickAttribute.EventModifiers.PreventDefault.Value.ConstantValue.Value);
        Assert.IsNotNull(clickAttribute.EventModifiers.StopPropagation);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(clickAttribute.EventModifiers.StopPropagation.Value);
        Assert.AreEqual("StopClick", ((IPropertyReferenceOperation)clickAttribute.EventModifiers.StopPropagation.Value).Property.Name);
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersElementDomEventWithModifiers()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<button @onclick="OnClick" @onclick:preventDefault="true" @onclick:stopPropagation="StopClick">Go</button>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.EventModifiers.Sfc.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool StopClick { get; set; }

                    private void OnClick()
                    {
                    }
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<button @click=\"(__event) =&gt; { __event?.preventDefault?.(); if (props.stopClick) __event?.stopPropagation?.(); return (__jazor$0)(__event); }\">");
        StringAssert.Contains(artifact.ScriptSetupText, "function onClick()");
        Assert.IsFalse(artifact.TemplateText.Contains("@onclick", StringComparison.Ordinal), artifact.TemplateText);
        Assert.IsFalse(artifact.TemplateText.Contains(":onclick", StringComparison.Ordinal), artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersElementDomEventLambdaHandler()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<button @onclick="() => Count++" @onclick:preventDefault="PreventClick">Go</button>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.EventLambda.Sfc.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private int Count { get; set; }

                    [Parameter]
                    public bool PreventClick { get; set; }
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "@click=\"(__event) =&gt; { if (props.preventClick) __event?.preventDefault?.(); return (__jazor$0)(__event); }\"");
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazor$0 = computed(() => () =>");
        StringAssert.Contains(artifact.ScriptSetupText, "count++;");
        Assert.IsFalse(artifact.TemplateText.Contains("@onclick", StringComparison.Ordinal), artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_ForStringOnAttribute_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<button onclick="return false">Go</button>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.StringOnAttribute.Sfc.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "raw markup execution");
        StringAssert.Contains(exception.Issue.Message, "onclick");
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
    public void CreateRenderTree_ForMixedStaticAndExpressionAttributeContent_ProducesRuntimeExpression()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<div class="todo-card @Title">Hello</div>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.MixedAttribute.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);
        var element = renderTree.Children[0] as RazorVueElementNode;

        Assert.IsNotNull(element);
        var classAttribute = element.Attributes.OfType<RazorVueAttributeNode>().Single(static attribute => attribute.Name == "class");
        Assert.IsNotNull(classAttribute.Value);
        Assert.IsInstanceOfType<IBinaryOperation>(classAttribute.Value);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerMixedStaticAndExpressionAttributeContent()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<div class="todo-card @Title">Hello</div>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.MixedAttribute.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "\"class\": (\"todo-card \" + props.title)");
        StringAssert.Contains(artifact.ModuleCode, "h(\"div\", { \"class\": (\"todo-card \" + props.title) }, \"Hello\")");
    }

    [TestMethod]
    public void CreateRenderTree_ForMixedStaticAndCodeBlockAttributeContent_ProducesRuntimeExpression()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<div class="todo-card @(Title?.Trim() ?? "untitled")">Hello</div>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.MixedCodeAttribute.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);
        var element = renderTree.Children[0] as RazorVueElementNode;

        Assert.IsNotNull(element);
        var classAttribute = element.Attributes.OfType<RazorVueAttributeNode>().Single(static attribute => attribute.Name == "class");
        Assert.IsNotNull(classAttribute.Value);
        Assert.IsInstanceOfType<IBinaryOperation>(classAttribute.Value);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerMixedStaticAndCodeBlockAttributeContent()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<div class="todo-card @(Title?.Trim() ?? "untitled")">Hello</div>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.MixedCodeAttribute.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "\"class\": ((() => {");
        StringAssert.Contains(artifact.ModuleCode, "return \"todo-card \" + ((");
        StringAssert.Contains(artifact.ModuleCode, "props.title");
        StringAssert.Contains(artifact.ModuleCode, ".trim()) ?? \"untitled\"");
        StringAssert.Contains(artifact.ModuleCode, "h(\"div\", { \"class\": ((() => {");
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
    public void CreateRenderTree_ForTemplateLocalCodeBlock_ProducesTemplateScopedLocalNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var localTitle = Title;
            }

            <section>@localTitle</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(2, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var local = renderTree.Children[0] as RazorVueLocalDeclarationNode;
        Assert.IsNotNull(local);
        Assert.AreEqual("localTitle", local.LocalSymbol.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(local.Initializer);

        var section = renderTree.Children[1] as RazorVueElementNode;
        Assert.IsNotNull(section);
        var expression = section.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<ILocalReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_ForTemplateLocalCodeBlockWithoutInitializerThenImmediateAssignment_ProducesTemplateScopedLocalNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                string? localTitle;
                localTitle = Title;
            }

            <section>@localTitle</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.ImmediateAssignment.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(2, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var local = renderTree.Children[0] as RazorVueLocalDeclarationNode;
        Assert.IsNotNull(local);
        Assert.AreEqual("localTitle", local.LocalSymbol.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(local.Initializer);

        var section = renderTree.Children[1] as RazorVueElementNode;
        Assert.IsNotNull(section);
        var expression = section.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<ILocalReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_ForTemplateLocalCodeBlockWithoutInitializerThenAssignmentAfterSiblingLocalDeclaration_ProducesOrderedTemplateScopedLocalNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                string? localTitle;
                var revision = 0;
                localTitle = Title;
            }

            <section>@localTitle</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.SiblingDeclarationImmediateAssignment.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(3, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var localTitle = Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(renderTree.Children[0]);
        Assert.AreEqual("localTitle", localTitle.LocalSymbol.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(localTitle.Initializer);

        var revision = Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(renderTree.Children[1]);
        Assert.AreEqual("revision", revision.LocalSymbol.Name);
        Assert.IsInstanceOfType<ILiteralOperation>(revision.Initializer);

        var section = Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children[2]);
        var expression = Assert.IsInstanceOfType<RazorVueExpressionNode>(section.Children.Children.Single());
        Assert.IsInstanceOfType<ILocalReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTemplateLocalCodeBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var localTitle = Title;
            }

            <section>@localTitle</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const localTitle = props.title;");
        StringAssert.Contains(artifact.ModuleCode, "h(\"section\", null, localTitle)");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTemplateLocalCodeBlockWithoutInitializerThenImmediateAssignment()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                string? localTitle;
                localTitle = Title;
            }

            <section>@localTitle</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.ImmediateAssignment.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const localTitle = props.title;");
        StringAssert.Contains(artifact.ModuleCode, "h(\"section\", null, localTitle)");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTemplateLocalCodeBlockWithoutInitializerThenAssignmentAfterSiblingLocalDeclaration()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                string? localTitle;
                var revision = 0;
                localTitle = Title;
            }

            <section>@localTitle</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.SiblingDeclarationImmediateAssignment.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const localTitle = props.title;");
        StringAssert.Contains(artifact.ModuleCode, "const revision = 0;");
        StringAssert.Contains(artifact.ModuleCode, "h(\"section\", null, localTitle)");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTemplateLocalCodeBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var localTitle = Title;
            }

            <section>@localTitle</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.Sfc.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(localTitle) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ localTitle }}");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTemplateLocalCodeBlockWithoutInitializerThenImmediateAssignment()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                string? localTitle;
                localTitle = Title;
            }

            <section>@localTitle</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.ImmediateAssignment.Sfc.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(localTitle) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ localTitle }}");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTemplateLocalCodeBlockWithoutInitializerThenAssignmentAfterSiblingLocalDeclaration()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                string? localTitle;
                var revision = 0;
                localTitle = Title;
            }

            <section>@localTitle</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.SiblingDeclarationImmediateAssignment.Sfc.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(localTitle) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(revision) in [0]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ localTitle }}");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTemplateCodeBlockWithLocalReassignment_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var localTitle = Title;
                localTitle = localTitle + "!";
            }

            <section>@localTitle</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TemplateCodeBlock.LocalReassignment.Sfc.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "let localTitle = props.title;");
        StringAssert.Contains(artifact.SfcText, "localTitle = localTitle + \"!\";");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(localTitle);");
        Assert.IsFalse(artifact.SfcText.Contains("<template v-for=\"(localTitle)", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTemplateCodeBlockWithLocalIncrementAndDecrement_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var index = Start;
                index++;
                index--;
            }

            <section>@index</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TemplateCodeBlock.LocalIncrementDecrement.Sfc.Tests",
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
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "let index = props.start;");
        StringAssert.Contains(artifact.SfcText, "index++;");
        StringAssert.Contains(artifact.SfcText, "index--;");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(index);");
        Assert.IsFalse(artifact.SfcText.Contains("<template v-for=\"(index)", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void CreateRenderTree_ForTemplateLocalCodeBlockWithCallableLocalInitializer_ThrowsActionableUnsupportedIssue()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                Func<string?, string?> decorate = value => value + "!";
            }

            <section>@decorate(Title)</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TemplateCodeBlock.CallableLocal.Tests",
            documentPath,
            documentText,
            """
            using System;

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
            importsText: "@using System");

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot));

        StringAssert.Contains(exception.Message, "template-scoped local 'decorate'");
        StringAssert.Contains(exception.Message, "immutable value/cache initializer without nested write or callable template state");
    }

    [TestMethod]
    public void CreateRenderTree_ForTemplateCodeBlockWithLocalFunctionCall_ProducesImperativeLocalBlockNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                string? Decorate(string? value) => value + "!";
                var decorated = Decorate(Title);
            }

            <section>@decorated</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TemplateCodeBlock.LocalFunctionCall.Tree.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(renderTree.Children[0]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LocalBlock, imperative.Kind);
        Assert.IsTrue(imperative.Operations.Any(static operation => operation is ILocalFunctionOperation));
        Assert.IsTrue(imperative.Operations.Any(static operation => operation is IVariableDeclarationGroupOperation));
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersTemplateCodeBlockWithLocalFunctionCall_UsingImperativeRenderBridge()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                string? Decorate(string? value) => value + "!";
                var decorated = Decorate(Title);
            }

            <section>@decorated</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TemplateCodeBlock.LocalFunctionCall.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "function Decorate(value)");
        StringAssert.Contains(artifact.ModuleCode, "let decorated = Decorate(props.title);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(decorated);");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTemplateCodeBlockWithLocalFunctionCall_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                string? Decorate(string? value) => value + "!";
                var decorated = Decorate(Title);
            }

            <section>@decorated</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TemplateCodeBlock.LocalFunctionCall.Sfc.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "function Decorate(value)");
        StringAssert.Contains(artifact.SfcText, "let decorated = Decorate(props.title);");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(decorated);");
        Assert.IsFalse(artifact.SfcText.Contains("<template v-for=\"(decorated)", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersSimpleConstantSwitchStatementCodeBlock_ToTemplateVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                switch (Count)
                {
                    case 0:
                        <p>empty</p>
                        break;
                    default:
                        <section>@Count</section>
                        break;
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Switch.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(__jazorSwitchValue) in [props.count]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"__jazorSwitchValue === 0\">");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "empty");
        StringAssert.Contains(artifact.TemplateText, "<template v-else>");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ props.count }}");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersSimpleSwitchWithNestedHelperType_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                switch (Count)
                {
                    case 0:
                        <p>@Helper.Text</p>
                        break;
                    default:
                        <section>@Count</section>
                        break;
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Switch.NestedHelper.Sfc.Tests",
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

                    private sealed class Helper
                    {
                        public static string Text => "ready";
                    }
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);
        var classIndex = artifact.SfcText.IndexOf("class Helper", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsTrue(classIndex >= 0, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "switch (props.count)");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(Helper.text);");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTryCatchFinallyCodeBlock_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                try
                {
                    <section>ready</section>
                }
                catch
                {
                    <p>fallback</p>
                }
                finally
                {
                    _count++;
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TryCatchFinally.Sfc.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private int _count;
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} catch {");
        StringAssert.Contains(artifact.SfcText, "} finally {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(h(\"section\", null, \"ready\"));");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(h(\"p\", null, \"fallback\"));");
        StringAssert.Contains(artifact.SfcText, "_count++;");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTryFinallyWithNestedHelperType_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                try
                {
                    <section>@Helper.Text</section>
                }
                finally
                {
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TryFinally.NestedHelper.Sfc.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private sealed class Helper
                    {
                        public static string Text => "ready";
                    }
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);
        var classIndex = artifact.SfcText.IndexOf("class Helper", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsTrue(classIndex >= 0, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} finally {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(Helper.text);");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTypedEmptyCatchCodeBlock_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                try
                {
                    <section>ready</section>
                }
                catch (Exception)
                {
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedEmptyCatch.Sfc.Tests",
            documentPath,
            documentText,
            """
            using System;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                }
            }
            """,
            importsText: "@using System");

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} catch {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(h(\"section\", null, \"ready\"));");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersUsingDeclarationCodeBlock_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                using var disposable = CreateDisposable();
                <section>ready</section>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.UsingDeclaration.Sfc.Tests",
            documentPath,
            documentText,
            """
            using System;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private sealed class TestDisposable : IDisposable
                    {
                        public void Dispose() { }
                    }

                    private TestDisposable CreateDisposable() => new TestDisposable();
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "let disposable = ");
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} finally {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(h(\"section\", null, \"ready\"));");
        StringAssert.Contains(artifact.SfcText, "if (disposable !== null)");
        StringAssert.Contains(artifact.SfcText, "disposable.dispose();");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersDefaultUsingDeclarationCodeBlock_ToTemplateSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                using var disposable = default(global::System.IDisposable);
                <section>ready</section>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DefaultUsingDeclaration.Sfc.Tests",
            documentPath,
            documentText,
            """
            using System;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
        Assert.IsFalse(artifact.SfcText.Contains("__jazorCreateRenderContext", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("try {", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("dispose", StringComparison.OrdinalIgnoreCase), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersUsingStatementCodeBlock_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                using (CreateDisposable())
                {
                    <section>ready</section>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.UsingStatement.Sfc.Tests",
            documentPath,
            documentText,
            """
            using System;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private sealed class TestDisposable : IDisposable
                    {
                        public void Dispose() { }
                    }

                    private IDisposable CreateDisposable() => new TestDisposable();
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "let ");
        StringAssert.Contains(artifact.SfcText, " = createDisposable();");
        StringAssert.Contains(artifact.SfcText, "try {");
        StringAssert.Contains(artifact.SfcText, "} finally {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(h(\"section\", null, \"ready\"));");
        StringAssert.Contains(artifact.SfcText, "!== null)");
        StringAssert.Contains(artifact.SfcText, "_6f97d94b6f2e4bc1(");
    }

    [TestMethod]
    public void CreateRenderTree_ForAtKeyAttributes_ResolvesLiteralAndExpressionKeys()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <section @key="'root'">
                <SharedBadge @key="Id" Text="@Title" />
            </section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.AtKey.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Shared
            {
                [ECMAScript.ECMAScriptModule("./components/shared-badge")]
                public partial class SharedBadge : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Text { get; set; }
                }
            }

            namespace Demo.Pages
            {
                using Demo.Shared;

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Id { get; set; }

                    [Parameter]
                    public string? Title { get; set; }
                }
            }
            """,
            importsText: "@using Demo.Shared");

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var section = renderTree.Children[0] as RazorVueElementNode;
        Assert.IsNotNull(section);
        Assert.IsNotNull(section.Key);
        Assert.IsTrue(section.Key.Expression.ConstantValue.HasValue);
        Assert.AreEqual("root", section.Key.Expression.ConstantValue.Value);

        var badge = section.Children.Children[0] as RazorVueComponentNode;
        Assert.IsNotNull(badge);
        Assert.IsNotNull(badge.Key);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(badge.Key.Expression);
        Assert.AreEqual("Id", ((IPropertyReferenceOperation)badge.Key.Expression).Property.Name);
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
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersLibraryDefaultChildContentWithoutDuplicateSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<VContainer Fluid="true"><p>Body</p></VContainer>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.LibraryDefaultChildContent.Tests",
            documentPath,
            documentText,
            """
            using ECMAScript.Vuetify;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                }
            }
            """,
            """
            @using Demo.Pages
            @using ECMAScript.Vuetify
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<VContainer");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "Body");
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
    public void CreateRenderTree_ForRenderFragmentLocalCarrierAssignedToTypedComponentSlot_ProducesStructuredSlotTemplate()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template = item => @<p>@item</p>;
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));
        var component = renderTree.Children[0] as RazorVueComponentNode;
        Assert.IsNotNull(component);
        Assert.AreEqual(0, component.Attributes.Length);
        Assert.AreEqual(1, component.SlotTemplates.Length);

        var slotTemplate = component.SlotTemplates[0];
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("itemTemplate", slotTemplate.SlotName);
        Assert.AreEqual("item", slotTemplate.ParameterName);

        var paragraph = slotTemplate.Children.Children.Single() as RazorVueElementNode;
        Assert.IsNotNull(paragraph);
        Assert.AreEqual("p", paragraph.TagName);
        var expression = paragraph.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_ForImmediatelyAssignedRenderFragmentLocalCarrierAssignedToTypedComponentSlot_ProducesStructuredSlotTemplate()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template;
                template = item => @<p>@item</p>;
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.ImmediateAssignment.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));
        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children[0]);
        Assert.AreEqual(1, component.SlotTemplates.Length);

        var slotTemplate = component.SlotTemplates[0];
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("itemTemplate", slotTemplate.SlotName);
        Assert.AreEqual("item", slotTemplate.ParameterName);

        var paragraph = Assert.IsInstanceOfType<RazorVueElementNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("p", paragraph.TagName);
        var expression = Assert.IsInstanceOfType<RazorVueExpressionNode>(paragraph.Children.Children.Single());
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRenderFragmentLocalCarrierAssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template = item => @<p>@item</p>;
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => h(\"p\", null, item)");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerImmediatelyAssignedRenderFragmentLocalCarrierAssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template;
                template = item => @<p>@item</p>;
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.ImmediateAssignment.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => h(\"p\", null, item)");
    }

    [TestMethod]
    public void CreateRenderTree_ForImmediatelyAssignedRenderFragmentLocalCarrierWithTrailingIf_AssignedToTypedComponentSlot_ProducesSlotTemplateThenConditional()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template;
                template = item => @<p>@item</p>;
                if (Show)
                {
                    <section>tail</section>
                }
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.ImmediateAssignment.TrailingIf.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(2, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));

        var conditional = Assert.IsInstanceOfType<RazorVueConditionalNode>(renderTree.Children[0]);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(conditional.Condition);
        var section = Assert.IsInstanceOfType<RazorVueElementNode>(conditional.WhenTrue.Children.Single());
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual("tail", Assert.IsInstanceOfType<RazorVueTextNode>(section.Children.Children.Single()).Text);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children[1]);
        Assert.AreEqual(1, component.SlotTemplates.Length);
        Assert.AreEqual(0, component.Attributes.Length);
        var slotTemplate = component.SlotTemplates[0];
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("item", slotTemplate.ParameterName);
        var paragraph = Assert.IsInstanceOfType<RazorVueElementNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("p", paragraph.TagName);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerImmediatelyAssignedRenderFragmentLocalCarrierWithTrailingIf_AssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template;
                template = item => @<p>@item</p>;
                if (Show)
                {
                    <section>tail</section>
                }
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.ImmediateAssignment.TrailingIf.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => h(\"p\", null, item)");
        StringAssert.Contains(artifact.ModuleCode, "(props.show ? h(\"section\", null, \"tail\") : null)");
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentLocalCarrierWithTrailingIf_AssignedToTypedComponentSlot_ProducesSlotTemplateThenConditional()
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.TrailingIf.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(2, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));

        var conditional = Assert.IsInstanceOfType<RazorVueConditionalNode>(renderTree.Children[0]);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(conditional.Condition);
        var section = Assert.IsInstanceOfType<RazorVueElementNode>(conditional.WhenTrue.Children.Single());
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual("tail", Assert.IsInstanceOfType<RazorVueTextNode>(section.Children.Children.Single()).Text);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children[1]);
        Assert.AreEqual(1, component.SlotTemplates.Length);
        Assert.AreEqual(0, component.Attributes.Length);
        var slotTemplate = component.SlotTemplates[0];
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("item", slotTemplate.ParameterName);
        var paragraph = Assert.IsInstanceOfType<RazorVueElementNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("p", paragraph.TagName);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRenderFragmentLocalCarrierWithTrailingIf_AssignedToTypedComponentSlot()
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.TrailingIf.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => h(\"p\", null, item)");
        StringAssert.Contains(artifact.ModuleCode, "(props.show ? h(\"section\", null, \"tail\") : null)");
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentLocalCarrierWithTrailingForeach_AssignedToTypedComponentSlot_ProducesSlotTemplateThenLoop()
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.TrailingForEach.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(2, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var loop = Assert.IsInstanceOfType<RazorVueForEachNode>(renderTree.Children[0]);
        Assert.AreEqual("tag", loop.ItemName);
        var section = Assert.IsInstanceOfType<RazorVueElementNode>(loop.Body.Children.Single());
        Assert.AreEqual("section", section.TagName);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(section.Children.Children.Single());
        Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children[1]);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRenderFragmentLocalCarrierWithTrailingForeach_AssignedToTypedComponentSlot()
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.TrailingForEach.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => h(\"p\", null, item)");
        StringAssert.Contains(artifact.ModuleCode, "props.tags.map((tag) => h(\"section\", null, tag))");
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentLocalCarrierWithTrailingFor_AssignedToTypedComponentSlot_ProducesSlotTemplateThenCountLoop()
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.TrailingFor.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(2, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var loop = Assert.IsInstanceOfType<RazorVueForNode>(renderTree.Children[0]);
        Assert.AreEqual("i", loop.VariableName);
        var section = Assert.IsInstanceOfType<RazorVueElementNode>(loop.Body.Children.Single());
        Assert.AreEqual("section", section.TagName);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(section.Children.Children.Single());
        Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children[1]);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRenderFragmentLocalCarrierWithTrailingFor_AssignedToTypedComponentSlot()
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.TrailingFor.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => h(\"p\", null, item)");
        StringAssert.Contains(artifact.ModuleCode, "__jazorVueForRange(0, props.count, \"<\", \"++\", null).map((i) => h(\"section\", null, i))");
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentLocalCarrierWithTrailingWhile_AssignedToTypedComponentSlot_ProducesImperativeTailThatKeepsCarrierVisible()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template = item => @<p>@item</p>;
                var index = 0;
                while (index < Count)
                {
                    <section>@index</section>
                    index++;
                }
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.TrailingWhile.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(renderTree.Children.Single());
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static local => local.Name).ToArray(), "template");
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static local => local.Name).ToArray(), "index");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRenderFragmentLocalCarrierWithTrailingWhile_AssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template = item => @<p>@item</p>;
                var index = 0;
                while (index < Count)
                {
                    <section>@index</section>
                    index++;
                }
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.TrailingWhile.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const __jazorRenderContext = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.ModuleCode, "let index = 0;");
        Assert.AreEqual(1, CountOccurrences(artifact.ModuleCode, "let index = 0;"), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("const index = 0;", StringComparison.Ordinal), artifact.ModuleCode);
        StringAssert.Contains(artifact.ModuleCode, "let template = item => {");
        StringAssert.Contains(artifact.ModuleCode, "while (index < props.count)");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setComponentParameter(\"ItemTemplate\", template);");
    }

    [TestMethod]
    public void CreateRenderTree_ForImmediatelyAssignedRenderFragmentLocalCarrierWithTrailingWhile_AssignedToTypedComponentSlot_ProducesImperativeTailThatKeepsCarrierVisible()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template;
                template = item => @<p>@item</p>;
                var index = 0;
                while (index < Count)
                {
                    <section>@index</section>
                    index++;
                }
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.ImmediateAssignment.TrailingWhile.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            2,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(renderTree.Children[0]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static local => local.Name).ToArray(), "index");
        CollectionAssert.DoesNotContain(imperative.VisibleLocals.Select(static local => local.Name).ToArray(), "template");

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children[1]);
        Assert.AreEqual(1, component.SlotTemplates.Length);
        Assert.AreEqual("ItemTemplate", component.SlotTemplates[0].PublicName);
        Assert.AreEqual("item", component.SlotTemplates[0].ParameterName);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerImmediatelyAssignedRenderFragmentLocalCarrierWithTrailingWhile_AssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template;
                template = item => @<p>@item</p>;
                var index = 0;
                while (index < Count)
                {
                    <section>@index</section>
                    index++;
                }
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.ImmediateAssignment.TrailingWhile.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const __jazorRenderContext = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.ModuleCode, "let index = 0;");
        Assert.AreEqual(1, CountOccurrences(artifact.ModuleCode, "let index = 0;"), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("const index = 0;", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("let template =", StringComparison.Ordinal), artifact.ModuleCode);
        StringAssert.Contains(artifact.ModuleCode, "while (index < props.count)");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(LayoutCardComponent, null, { itemTemplate: (item) => h(\"p\", null, item) }));");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerImmediatelyAssignedRenderFragmentLocalCarrierWithTrailingWhileAndPostLoopLocalUse()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template;
                template = item => @<p>@item</p>;
                var index = 0;
                while (index < Count)
                {
                    <section>@index</section>
                    index++;
                }
            }

            <span>@index</span>
            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.ImmediateAssignment.TrailingWhile.PostLoopLocalUse.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "let index = 0;");
        Assert.AreEqual(1, CountOccurrences(artifact.ModuleCode, "let index = 0;"), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("const index = 0;", StringComparison.Ordinal), artifact.ModuleCode);
        StringAssert.Contains(artifact.ModuleCode, "let template;");
        StringAssert.Contains(artifact.ModuleCode, "template = item => {");
        StringAssert.Contains(artifact.ModuleCode, "while (index < props.count)");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"span\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(index);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.leaveElement();");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterComponent(LayoutCardComponent, __jazorImperativeComponentMetadata_LayoutCard);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setComponentParameter(\"ItemTemplate\", template);");
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentLocalCarrierWithTrailingConditionalReturn_AssignedToTypedComponentSlot_ProducesImperativeTailThatKeepsCarrierVisible()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template = item => @<p>@item</p>;
                if (Hide)
                {
                    return;
                }
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.TrailingConditionalReturn.Tests",
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
                    public bool Hide { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(renderTree.Children.Single());
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static local => local.Name).ToArray(), "template");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRenderFragmentLocalCarrierWithTrailingConditionalReturn_AssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template = item => @<p>@item</p>;
                if (Hide)
                {
                    return;
                }
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.TrailingConditionalReturn.Pipeline.Tests",
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
                    public bool Hide { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const __jazorRenderContext = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.ModuleCode, "if (props.hide) {");
        StringAssert.Contains(artifact.ModuleCode, "return __jazorRenderContext.finish();");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterComponent(LayoutCardComponent, __jazorImperativeComponentMetadata_LayoutCard);");
        StringAssert.Contains(artifact.ModuleCode, "let template = item => {");
        StringAssert.Contains(artifact.ModuleCode, "const __jazorImperativeRenderContext0 = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeRenderContext0.enterElement(\"p\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeRenderContext0.append(item);");
        StringAssert.Contains(artifact.ModuleCode, "return __jazorImperativeRenderContext0.finish();");
        Assert.IsFalse(artifact.ModuleCode.Contains("__jazorCreateContextualRenderSlot", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("return __builder", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("__jazorRenderContext.enterElement(\"p\")", StringComparison.Ordinal), artifact.ModuleCode);
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setComponentParameter(\"ItemTemplate\", template);");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForRenderFragmentLocalCarrierAssignedNonImmediately_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template;
                <section>prefix</section>
                template = item => @<p>@item</p>;
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.NonImmediateAssignment.Pipeline.Tests",
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "RenderFragment local 'template'");
        StringAssert.Contains(exception.Issue.Message, "later writes");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForReassignedImmediatelyAssignedRenderFragmentLocalCarrier_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template;
                template = item => @<p>@item</p>;
                template = item => @<span>@item</span>;
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.Reassigned.Pipeline.Tests",
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
                .Execute(context));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "RenderFragment local 'template'");
        StringAssert.Contains(exception.Issue.Message, "later writes");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForBranchAssignedRenderFragmentLocalCarrier_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template;
                if (UseStrong)
                {
                    template = item => @<strong>@item</strong>;
                }
                else
                {
                    template = item => @<span>@item</span>;
                }
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.TypedSlot.BranchAssigned.Pipeline.Tests",
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
                    public bool UseStrong { get; set; }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
                .Execute(context));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "RenderFragment local 'template'");
        StringAssert.Contains(exception.Issue.Message, "later writes");
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentLocalCarrierInitializedFromFactoryMethod_AssignedToTypedComponentSlot_ProducesCapturedScopeAndStructuredSlotTemplate()
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.Factory.TypedSlot.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var slotTemplate = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("item", slotTemplate.ParameterName);

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        Assert.AreEqual(3, span.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[0]);
        var whitespace = Assert.IsInstanceOfType<RazorVueTextNode>(span.Children.Children[1]);
        Assert.AreEqual(" ", whitespace.Text);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[2]);
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentLocalCarrierInitializedFromFactoryMethodWithInParameter_AssignedToTypedComponentSlot_ProducesCapturedScopeAndStructuredSlotTemplate()
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

                private RenderFragment<int> CreateTemplate(in string? title)
                    => CreateTemplateCore(title);

                private RenderFragment<int> CreateTemplateCore(string? capturedTitle)
                    => item => @<span>@capturedTitle @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.Factory.InParameter.TypedSlot.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var slotTemplate = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("item", slotTemplate.ParameterName);

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var capturedTitleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("capturedTitle", capturedTitleScope.ScopeName);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(capturedTitleScope.Initializer);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(capturedTitleScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        Assert.AreEqual(3, span.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[0]);
        var whitespace = Assert.IsInstanceOfType<RazorVueTextNode>(span.Children.Children[1]);
        Assert.AreEqual(" ", whitespace.Text);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[2]);
    }

    [TestMethod]
    public void CreateRenderTree_ForImmediatelyAssignedRenderFragmentLocalCarrierInitializedFromFactoryMethod_AssignedToTypedComponentSlot_ProducesCapturedScopeAndStructuredSlotTemplate()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template;
                template = CreateTemplate(Title);
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.Factory.TypedSlot.ImmediateAssignment.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        Assert.AreEqual(1, component.SlotTemplates.Length, DescribeStructure(renderTree));
        var slotTemplate = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("item", slotTemplate.ParameterName);

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        Assert.AreEqual(3, span.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[0]);
        var whitespace = Assert.IsInstanceOfType<RazorVueTextNode>(span.Children.Children[1]);
        Assert.AreEqual(" ", whitespace.Text);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[2]);
    }

    [TestMethod]
    public void CreateRenderTree_ForImmediatelyAssignedRenderFragmentLocalCarrierInitializedFromFactoryMethodAfterSiblingLocalDeclaration_AssignedToTypedComponentSlot_ProducesCapturedScopeAndStructuredSlotTemplate()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template;
                var revision = 0;
                template = CreateTemplate(Title);
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.Factory.TypedSlot.SiblingDeclarationImmediateAssignment.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var revision = Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(renderTree.Children[0]);
        Assert.AreEqual("revision", revision.LocalSymbol.Name);
        Assert.IsInstanceOfType<ILiteralOperation>(revision.Initializer);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children[1]);
        Assert.AreEqual(1, component.SlotTemplates.Length, DescribeStructure(renderTree));
        var slotTemplate = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("item", slotTemplate.ParameterName);

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        Assert.AreEqual(3, span.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentLocalCarrierInitializedFromGenericFactoryMethod_AssignedToTypedComponentSlot_ProducesCapturedScopeAndStructuredSlotTemplate()
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

                private RenderFragment<int> CreateTemplate<TTitle>(TTitle title)
                    => item => @<span>@title @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.FactoryGeneric.TypedSlot.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var slotTemplate = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("item", slotTemplate.ParameterName);

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        Assert.AreEqual(3, span.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[0]);
        var whitespace = Assert.IsInstanceOfType<RazorVueTextNode>(span.Children.Children[1]);
        Assert.AreEqual(" ", whitespace.Text);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[2]);
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentLocalCarrierInitializedFromFactoryMethodWithOmittedOptionalParameter_AssignedToTypedComponentSlot_ProducesCapturedScopeAndStructuredSlotTemplate()
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.FactoryOptional.TypedSlot.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var slotTemplate = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("item", slotTemplate.ParameterName);

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<ILiteralOperation>(titleScope.Initializer);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        Assert.AreEqual(3, span.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[0]);
        var whitespace = Assert.IsInstanceOfType<RazorVueTextNode>(span.Children.Children[1]);
        Assert.AreEqual(" ", whitespace.Text);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[2]);
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentLocalCarrierInitializedFromFactoryMethodWithParamsParameter_AssignedToTypedComponentSlot_ProducesCapturedScopeAndStructuredSlotTemplate()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = CreateTemplate(Title, "suffix");
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment<int> CreateTemplate(params string?[] values)
                    => item => @<span>@values.Length @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.FactoryParams.TypedSlot.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var slotTemplate = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("item", slotTemplate.ParameterName);

        var valuesScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("values", valuesScope.ScopeName);
        Assert.IsInstanceOfType<IArrayCreationOperation>(valuesScope.Initializer);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(valuesScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        Assert.AreEqual(3, span.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[0]);
        var whitespace = Assert.IsInstanceOfType<RazorVueTextNode>(span.Children.Children[1]);
        Assert.AreEqual(" ", whitespace.Text);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[2]);
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentLocalCarrierInitializedFromLocalFunctionFactoryMethod_AssignedToTypedComponentSlot_ProducesCapturedScopeAndStructuredSlotTemplate()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = CreateTemplate(Title);

                RenderFragment<int> CreateTemplate(string? title)
                    => item => @<span>@title @item</span>;
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.LocalFunctionFactory.TypedSlot.Tests",
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
                    [Parameter]
                    public string? Title { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var slotTemplate = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("item", slotTemplate.ParameterName);

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        Assert.AreEqual(3, span.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[0]);
        var whitespace = Assert.IsInstanceOfType<RazorVueTextNode>(span.Children.Children[1]);
        Assert.AreEqual(" ", whitespace.Text);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[2]);
    }

    [TestMethod]
    public void CreateRenderTree_ForMultipleRenderFragmentLocalCarriersInitializedFromSameFactoryMethodWithDifferentArguments_AssignedToDifferentTypedComponentSlots_PreservesPerInvocationCapturedScopes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> primaryTemplate = CreateTemplate(Title);
                RenderFragment<int> secondaryTemplate = CreateTemplate(Subtitle);
            }

            <LayoutCard PrimaryTemplate="primaryTemplate" SecondaryTemplate="secondaryTemplate" />

            @code {
                [Parameter]
                public string? Title { get; set; }

                [Parameter]
                public string? Subtitle { get; set; }

                private RenderFragment<int> CreateTemplate(string? title)
                    => item => @<span>@title @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.FactoryMultiInvocation.TypedSlot.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/layout-card")]
                public partial class LayoutCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? PrimaryTemplate { get; set; }

                    [Parameter]
                    public RenderFragment<int>? SecondaryTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var primarySlot = component.SlotTemplates.Single(static slot => slot.PublicName == "PrimaryTemplate");
        var secondarySlot = component.SlotTemplates.Single(static slot => slot.PublicName == "SecondaryTemplate");

        var primaryTitleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(primarySlot.Children.Children.Single());
        Assert.AreEqual("title", primaryTitleScope.ScopeName);
        var primaryInitializer = Assert.IsInstanceOfType<IPropertyReferenceOperation>(primaryTitleScope.Initializer);
        Assert.AreEqual("Title", primaryInitializer.Property.Name);

        var secondaryTitleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(secondarySlot.Children.Children.Single());
        Assert.AreEqual("title", secondaryTitleScope.ScopeName);
        var secondaryInitializer = Assert.IsInstanceOfType<IPropertyReferenceOperation>(secondaryTitleScope.Initializer);
        Assert.AreEqual("Subtitle", secondaryInitializer.Property.Name);
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentLocalCarrierInitializedFromCurrentComponentPropertyCarrier_AssignedToTypedComponentSlot_ProducesCapturedScopeAndStructuredSlotTemplate()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = Template;
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment<int> Template => CreateTemplate(Title);

                private RenderFragment<int> CreateTemplate(string? title)
                    => item => @<span>@title @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.Member.TypedSlot.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var slotTemplate = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("item", slotTemplate.ParameterName);

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_ForImmediatelyAssignedRenderFragmentLocalCarrierInitializedFromCurrentComponentPropertyCarrier_AssignedToTypedComponentSlot_ProducesCapturedScopeAndStructuredSlotTemplate()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template;
                template = Template;
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment<int> Template => CreateTemplate(Title);

                private RenderFragment<int> CreateTemplate(string? title)
                    => item => @<span>@title @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.Member.TypedSlot.ImmediateAssignment.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var slotTemplate = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("item", slotTemplate.ParameterName);

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentLocalCarrierInitializedFromCurrentComponentAutoPropertyCarrier_AssignedToTypedComponentSlot_ProducesStructuredSlotTemplate()
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.AutoProperty.TypedSlot.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var slotTemplate = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("itemTemplate", slotTemplate.SlotName);
        Assert.AreEqual("item", slotTemplate.ParameterName);
        Assert.IsNotNull(slotTemplate.ParameterSymbol);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        var expression = Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children.Single());
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentLocalCarrierInitializedFromUnwrittenCurrentComponentSettablePropertyCarrier_AssignedToTypedComponentSlot_ProducesStructuredSlotTemplate()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = Template;
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                private RenderFragment<int> Template { get; set; } = item => @<span>@item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.SettableAutoProperty.TypedSlot.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var slotTemplate = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("itemTemplate", slotTemplate.SlotName);
        Assert.AreEqual("item", slotTemplate.ParameterName);
        Assert.IsNotNull(slotTemplate.ParameterSymbol);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        var expression = Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children.Single());
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForRenderFragmentLocalCarrierInitializedFromNonPrivateCurrentComponentSettableProperty_AssignedToTypedComponentSlot_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = Template;
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                internal RenderFragment<int> Template { get; set; } = item => @<span>@item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.NonPrivateSettableAutoProperty.TypedSlot.Pipeline.Tests",
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "RenderFragment");
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentLocalCarrierInitializedFromCurrentComponentFieldCarrier_AssignedToTypedComponentSlot_ProducesCapturedScopeAndStructuredSlotTemplate()
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.Field.TypedSlot.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var slotTemplate = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("itemTemplate", slotTemplate.SlotName);
        Assert.AreEqual("item", slotTemplate.ParameterName);
        Assert.IsNotNull(slotTemplate.ParameterSymbol);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        var expression = Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children.Single());
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_ForImmediatelyAssignedRenderFragmentLocalCarrierInitializedFromCurrentComponentFieldCarrier_AssignedToTypedComponentSlot_ProducesCapturedScopeAndStructuredSlotTemplate()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template;
                template = _template;
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                private readonly RenderFragment<int> _template
                    = item => @<span>@item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.Field.TypedSlot.ImmediateAssignment.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var slotTemplate = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("itemTemplate", slotTemplate.SlotName);
        Assert.AreEqual("item", slotTemplate.ParameterName);
        Assert.IsNotNull(slotTemplate.ParameterSymbol);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        var expression = Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children.Single());
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentLocalCarrierInitializedFromUnwrittenCurrentComponentNonReadonlyFieldCarrier_AssignedToTypedComponentSlot_ProducesCapturedScopeAndStructuredSlotTemplate()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = _template;
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                private RenderFragment<int> _template
                    = item => @<span>@item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.NonReadonlyField.TypedSlot.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var slotTemplate = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("itemTemplate", slotTemplate.SlotName);
        Assert.AreEqual("item", slotTemplate.ParameterName);
        Assert.IsNotNull(slotTemplate.ParameterSymbol);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        var expression = Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children.Single());
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForRenderFragmentLocalCarrierInitializedFromNonPrivateCurrentComponentNonReadonlyField_AssignedToTypedComponentSlot_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = _template;
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                internal RenderFragment<int> _template
                    = item => @<span>@item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.NonPrivateNonReadonlyField.TypedSlot.Pipeline.Tests",
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "RenderFragment");
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentLocalCarrierInitializedFromChainedCurrentComponentPropertyCarrier_AssignedToTypedComponentSlot_ProducesCapturedScopeAndStructuredSlotTemplate()
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.MemberChain.TypedSlot.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var slotTemplate = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slotTemplate.PublicName);
        Assert.AreEqual("item", slotTemplate.ParameterName);

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(slotTemplate.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRenderFragmentLocalCarrierInitializedFromCurrentComponentFieldCarrier_AssignedToTypedComponentSlot()
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.Field.TypedSlot.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => h(\"span\", null, item)");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerImmediatelyAssignedRenderFragmentLocalCarrierInitializedFromCurrentComponentFieldCarrier_AssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template;
                template = _template;
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                private readonly RenderFragment<int> _template
                    = item => @<span>@item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.Field.TypedSlot.ImmediateAssignment.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => h(\"span\", null, item)");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRenderFragmentLocalCarrierInitializedFromUnwrittenCurrentComponentNonReadonlyFieldCarrier_AssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = _template;
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                private RenderFragment<int> _template
                    = item => @<span>@item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.NonReadonlyField.TypedSlot.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => h(\"span\", null, item)");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRenderFragmentLocalCarrierInitializedFromCurrentComponentAutoPropertyCarrier_AssignedToTypedComponentSlot()
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.AutoProperty.TypedSlot.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => h(\"span\", null, item)");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerImmediatelyAssignedRenderFragmentLocalCarrierInitializedFromCurrentComponentPropertyCarrier_AssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template;
                template = Template;
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment<int> Template => CreateTemplate(Title);

                private RenderFragment<int> CreateTemplate(string? title)
                    => item => @<span>@title @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.Member.TypedSlot.ImmediateAssignment.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => ((title) => h(\"span\", null, [title, \" \", item]))(props.title)");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerImmediatelyAssignedRenderFragmentLocalCarrierInitializedFromFactoryMethodAfterSiblingLocalDeclaration_AssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template;
                var revision = 0;
                template = CreateTemplate(Title);
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.Factory.TypedSlot.SiblingDeclarationImmediateAssignment.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const revision = 0;");
        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => ((title) => h(\"span\", null, [title, \" \", item]))(props.title)");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRenderFragmentLocalCarrierInitializedFromUnwrittenCurrentComponentSettablePropertyCarrier_AssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = Template;
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                private RenderFragment<int> Template { get; set; } = item => @<span>@item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.SettableAutoProperty.TypedSlot.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => h(\"span\", null, item)");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRenderFragmentLocalCarrierInitializedFromChainedCurrentComponentPropertyCarrier_AssignedToTypedComponentSlot()
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.MemberChain.TypedSlot.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => ((title) => h(\"span\", null, [title, \" \", item]))(props.title)");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForRenderFragmentLocalCarrierInitializedFromSelfReferentialCurrentComponentPropertyCarrier_AssignedToTypedComponentSlot_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = Template;
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                private RenderFragment<int> Template => Template;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.MemberSelf.TypedSlot.Tests",
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "recursively");
        StringAssert.Contains(exception.Issue.Message, "Template");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForRenderFragmentLocalCarrierInitializedFromCyclicCurrentComponentPropertyCarriers_AssignedToTypedComponentSlot_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = TemplateA;
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                private RenderFragment<int> TemplateA => TemplateB;
                private RenderFragment<int> TemplateB => TemplateA;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.MemberCycle.TypedSlot.Tests",
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "recursively");
        StringAssert.Contains(exception.Issue.Message, "TemplateA");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRenderFragmentLocalCarrierInitializedFromFactoryMethod_AssignedToTypedComponentSlot()
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.Factory.TypedSlot.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => ((title) => h(\"span\", null, [title, \" \", item]))(props.title)");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRenderFragmentLocalCarrierInitializedFromFactoryMethodWithInParameter_AssignedToTypedComponentSlot()
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

                private RenderFragment<int> CreateTemplate(in string? title)
                    => CreateTemplateCore(title);

                private RenderFragment<int> CreateTemplateCore(string? capturedTitle)
                    => item => @<span>@capturedTitle @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.Factory.InParameter.TypedSlot.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => ((title) => ((capturedTitle) => h(\"span\", null, [capturedTitle, \" \", item]))(title))(props.title)");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerImmediatelyAssignedRenderFragmentLocalCarrierInitializedFromFactoryMethod_AssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template;
                template = CreateTemplate(Title);
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.Factory.TypedSlot.ImmediateAssignment.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => ((title) => h(\"span\", null, [title, \" \", item]))(props.title)");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRenderFragmentLocalCarrierInitializedFromFactoryMethodWithOmittedOptionalParameter_AssignedToTypedComponentSlot()
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.FactoryOptional.TypedSlot.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => ((title) => h(\"span\", null, [title, \" \", item]))(\"fallback-title\")");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRenderFragmentLocalCarrierInitializedFromFactoryMethodWithParamsParameter_AssignedToTypedComponentSlot()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = CreateTemplate(Title, "suffix");
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment<int> CreateTemplate(params string?[] values)
                    => item => @<span>@values.Length @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.FactoryParams.TypedSlot.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => ((values) => h(\"span\", null, [values.length, \" \", item]))([props.title, \"suffix\"])");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerMultipleRenderFragmentLocalCarriersInitializedFromSameFactoryMethodWithDifferentArguments_AssignedToDifferentTypedComponentSlots()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> primaryTemplate = CreateTemplate(Title);
                RenderFragment<int> secondaryTemplate = CreateTemplate(Subtitle);
            }

            <LayoutCard PrimaryTemplate="primaryTemplate" SecondaryTemplate="secondaryTemplate" />

            @code {
                [Parameter]
                public string? Title { get; set; }

                [Parameter]
                public string? Subtitle { get; set; }

                private RenderFragment<int> CreateTemplate(string? title)
                    => item => @<span>@title @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.FactoryMultiInvocation.TypedSlot.Pipeline.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/layout-card")]
                public partial class LayoutCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? PrimaryTemplate { get; set; }

                    [Parameter]
                    public RenderFragment<int>? SecondaryTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "primaryTemplate: (item) => ((title) => h(\"span\", null, [title, \" \", item]))(props.title)");
        StringAssert.Contains(artifact.ModuleCode, "secondaryTemplate: (item) => ((title) => h(\"span\", null, [title, \" \", item]))(props.subtitle)");
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
    public void CreateRenderTree_ForTypedChildContentTemplateLocalCodeBlock_ProducesTemplateScopedLocalNode()
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
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = renderTree.Children[0] as RazorVueComponentNode;
        Assert.IsNotNull(component);
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual("item", itemTemplateSlot.ParameterName);

        Assert.AreEqual(2, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var local = itemTemplateSlot.Children.Children[0] as RazorVueLocalDeclarationNode;
        Assert.IsNotNull(local);
        Assert.AreEqual("decorated", local.LocalSymbol.Name);
        Assert.IsInstanceOfType<IBinaryOperation>(local.Initializer);

        var paragraph = itemTemplateSlot.Children.Children[1] as RazorVueElementNode;
        Assert.IsNotNull(paragraph);
        var expression = paragraph.Children.Children.Single() as RazorVueExpressionNode;
        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType<ILocalReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateLocalCodeBlockWithoutInitializerThenImmediateAssignment_ProducesTemplateScopedLocalNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        string? decorated;
                        decorated = item;
                    }
                    <p>@decorated</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.ImmediateAssignment.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual("item", itemTemplateSlot.ParameterName);

        Assert.AreEqual(2, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var local = Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(itemTemplateSlot.Children.Children[0]);
        Assert.AreEqual("decorated", local.LocalSymbol.Name);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(local.Initializer);

        var paragraph = Assert.IsInstanceOfType<RazorVueElementNode>(itemTemplateSlot.Children.Children[1]);
        var expression = Assert.IsInstanceOfType<RazorVueExpressionNode>(paragraph.Children.Children.Single());
        Assert.IsInstanceOfType<ILocalReferenceOperation>(expression.Expression);
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateCodeBlockWithStandaloneIf_ProducesConditionalNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        if (Show)
                        {
                            <p>@item</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneIf.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(1, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));

        var conditional = Assert.IsInstanceOfType<RazorVueConditionalNode>(itemTemplateSlot.Children.Children.Single());
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(conditional.Condition);
        var paragraph = Assert.IsInstanceOfType<RazorVueElementNode>(conditional.WhenTrue.Children.Single());
        Assert.AreEqual("p", paragraph.TagName);
        var expression = Assert.IsInstanceOfType<RazorVueExpressionNode>(paragraph.Children.Children.Single());
        Assert.IsInstanceOfType<IParameterReferenceOperation>(expression.Expression);
        Assert.AreEqual(0, conditional.WhenFalse.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateCodeBlockWithStandaloneForeachBreak_ProducesImperativeLoopBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        foreach (var value in Items!)
                        {
                            if (value < 0)
                            {
                                break;
                            }

                            <p>@item @value</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneForeachBreak.Tests",
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
                    public IReadOnlyList<int>? Items { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(1, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));

        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children.Single());
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(), "item");
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "value");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateCodeBlockWithStandaloneForeachBreak()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        foreach (var value in Items!)
                        {
                            if (value < 0)
                            {
                                break;
                            }

                            <p>@item @value</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneForeachBreak.Pipeline.Tests",
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
                    public IReadOnlyList<int>? Items { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "for (let value of props.items)");
        StringAssert.Contains(artifact.ModuleCode, "break;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(item);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(value);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateCodeBlockWithStandaloneForeachContinue_ProducesImperativeLoopBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        foreach (var value in Items!)
                        {
                            if (value == SkipValue)
                            {
                                continue;
                            }

                            <p>@item @value</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneForeachContinue.Tests",
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
                    public IReadOnlyList<int>? Items { get; set; }

                    [Parameter]
                    public int SkipValue { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(1, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));

        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children.Single());
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(), "item");
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "value");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateCodeBlockWithStandaloneForeachContinue()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        foreach (var value in Items!)
                        {
                            if (value == SkipValue)
                            {
                                continue;
                            }

                            <p>@item @value</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneForeachContinue.Pipeline.Tests",
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
                    public IReadOnlyList<int>? Items { get; set; }

                    [Parameter]
                    public int SkipValue { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "for (let value of props.items)");
        StringAssert.Contains(artifact.ModuleCode, "if (value === props.skipValue) {");
        StringAssert.Contains(artifact.ModuleCode, "continue;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(item);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(value);");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTypedChildContentTemplateCodeBlockWithStandaloneForeachContinue_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        foreach (var value in Items!)
                        {
                            if (value == SkipValue)
                            {
                                continue;
                            }

                            <p>@item @value</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneForeachContinue.Sfc.Tests",
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
                    public IReadOnlyList<int>? Items { get; set; }

                    [Parameter]
                    public int SkipValue { get; set; }
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.SfcText, "for (let value of props.items)");
        StringAssert.Contains(artifact.SfcText, "continue;");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeContext0.append(item);");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeContext0.append(value);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateCodeBlockWithStandaloneForContinue_ProducesImperativeLoopBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        for (var index = 0; index < Count; index++)
                        {
                            if (index == SkipIndex)
                            {
                                continue;
                            }

                            <p>@item @index</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneForContinue.Tests",
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

                    [Parameter]
                    public int SkipIndex { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(1, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));

        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children.Single());
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(), "item");
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "index");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateCodeBlockWithStandaloneForContinue()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        for (var index = 0; index < Count; index++)
                        {
                            if (index == SkipIndex)
                            {
                                continue;
                            }

                            <p>@item @index</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneForContinue.Pipeline.Tests",
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

                    [Parameter]
                    public int SkipIndex { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "for (let index = 0; index < props.count; index++)");
        StringAssert.Contains(artifact.ModuleCode, "if (index === props.skipIndex) {");
        StringAssert.Contains(artifact.ModuleCode, "continue;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(item);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(index);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateCodeBlockWithStandaloneForBreak_ProducesImperativeLoopBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        for (var index = 0; index < Count; index++)
                        {
                            if (index >= StopIndex)
                            {
                                break;
                            }

                            <p>@item @index</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneForBreak.Tests",
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

                    [Parameter]
                    public int StopIndex { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(1, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));

        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children.Single());
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(), "item");
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "index");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateCodeBlockWithStandaloneForBreak()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        for (var index = 0; index < Count; index++)
                        {
                            if (index >= StopIndex)
                            {
                                break;
                            }

                            <p>@item @index</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneForBreak.Pipeline.Tests",
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

                    [Parameter]
                    public int StopIndex { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "for (let index = 0; index < props.count; index++)");
        StringAssert.Contains(artifact.ModuleCode, "if (index >= props.stopIndex) {");
        StringAssert.Contains(artifact.ModuleCode, "break;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(item);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(index);");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTypedChildContentTemplateCodeBlockWithStandaloneForBreak_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        for (var index = 0; index < Count; index++)
                        {
                            if (index >= StopIndex)
                            {
                                break;
                            }

                            <p>@item @index</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneForBreak.Sfc.Tests",
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

                    [Parameter]
                    public int StopIndex { get; set; }
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.SfcText, "for (let index = 0; index < props.count; index++)");
        StringAssert.Contains(artifact.SfcText, "break;");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeContext0.append(item);");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeContext0.append(index);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateCodeBlockWithStandaloneDoWhileLoop_ProducesImperativeLoopBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        do
                        {
                            <p>@item</p>
                            break;
                        }
                        while (Show);
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneDoWhile.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(1, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));

        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children.Single());
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(), "item");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateCodeBlockWithStandaloneDoWhileLoop()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        do
                        {
                            <p>@item</p>
                            break;
                        }
                        while (Show);
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneDoWhile.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "do {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(item);");
        StringAssert.Contains(artifact.ModuleCode, "break;");
        StringAssert.Contains(artifact.ModuleCode, "while (props.show);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateCodeBlockWithStandaloneUsingStatement_ProducesImperativeTryBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        using (CreateDisposable())
                        {
                            <section>@item</section>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneUsingStatement.Tests",
            documentPath,
            documentText,
            """
            using System;

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
                    private sealed class TestDisposable : IDisposable
                    {
                        public void Dispose() { }
                    }

                    private IDisposable CreateDisposable() => new TestDisposable();
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(1, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));

        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children.Single());
        Assert.AreEqual(RazorVueImperativeBlockKind.TryBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(), "item");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateCodeBlockWithStandaloneUsingStatement()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        using (CreateDisposable())
                        {
                            <section>@item</section>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneUsingStatement.Pipeline.Tests",
            documentPath,
            documentText,
            """
            using System;

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
                    private sealed class TestDisposable : IDisposable
                    {
                        public void Dispose() { }
                    }

                    private IDisposable CreateDisposable() => new TestDisposable();
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, " = createDisposable();");
        StringAssert.Contains(artifact.ModuleCode, "try {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(item);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateCodeBlockWithStandaloneLock_ProducesImperativeLockBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        lock (_gate)
                        {
                            <section>@item</section>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneLock.Tests",
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
                    private readonly object _gate = new();
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(1, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));

        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children.Single());
        Assert.AreEqual(RazorVueImperativeBlockKind.LockBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(), "item");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateCodeBlockWithStandaloneLock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        lock (_gate)
                        {
                            <section>@item</section>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneLock.Pipeline.Tests",
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
                    private readonly object _gate = new();
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "if (_gate == null)");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(item);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateCodeBlockWithStandaloneThrowAndTailMarkup_ProducesImperativeMethodBodyBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        if (Fail)
                        {
                            throw new InvalidOperationException("boom");
                        }
                    }
                    <section>@item</section>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneThrowTail.Tests",
            documentPath,
            documentText,
            """
            using System;

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
                    public bool Fail { get; set; }
                }
            }
            """,
            importsText: "@using System");

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(1, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));

        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children.Single());
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(), "item");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateCodeBlockWithStandaloneThrowAndTailMarkup()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        if (Fail)
                        {
                            throw new InvalidOperationException("boom");
                        }
                    }
                    <section>@item</section>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneThrowTail.Pipeline.Tests",
            documentPath,
            documentText,
            """
            using System;

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
                    public bool Fail { get; set; }
                }
            }
            """,
            importsText: "@using System");

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "if (props.fail) {");
        StringAssert.Contains(artifact.ModuleCode, "throw new Error(\"boom\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(item);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTemplateLocalCodeBlockWithNestedIf_ProducesLocalThenConditional()
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
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.NestedIf.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(2, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var local = Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(renderTree.Children[0]);
        Assert.AreEqual("localTitle", local.LocalSymbol.Name);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(local.Initializer);

        var conditional = Assert.IsInstanceOfType<RazorVueConditionalNode>(renderTree.Children[1]);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(conditional.Condition);
        var section = Assert.IsInstanceOfType<RazorVueElementNode>(conditional.WhenTrue.Children.Single());
        Assert.AreEqual("section", section.TagName);
        var expression = Assert.IsInstanceOfType<RazorVueExpressionNode>(section.Children.Children.Single());
        Assert.IsInstanceOfType<ILocalReferenceOperation>(expression.Expression);
        Assert.AreEqual(0, conditional.WhenFalse.Children.Length);
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
    public void CreateRenderTree_ForConstantMarkupStringExpression_ProducesStaticMarkupNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@((MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>")""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.MarkupStringExpression.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));
        var section = Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children[0]);
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual("hero", ((RazorVueAttributeNode)section.Attributes.Single()).Value?.ConstantValue.Value);
        Assert.AreEqual(2, section.Children.Children.Length);
        Assert.AreEqual("span", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[0]).TagName);
        Assert.AreEqual("p", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[1]).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_ForNewMarkupStringExpression_ProducesStaticMarkupNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@(new MarkupString("<section class='hero'><span>safe</span><p>ok</p></section>"))""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.NewMarkupStringExpression.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));
        var section = Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children[0]);
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    private static string DescribeStructure(RazorVueRenderFragment fragment)
    {
        var builder = new StringBuilder();
        AppendFragment(builder, fragment, 0);
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
                builder.Append("Element(").Append(element.TagName).Append(')').AppendLine();
                AppendFragment(builder, element.Children, depth + 1);
                break;
            case RazorVueComponentNode component:
                builder.Append("Component(").Append(component.ComponentName).Append(')').AppendLine();
                AppendFragment(builder, component.Children, depth + 1);
                break;
            case RazorVueExpressionNode expression:
                builder.Append("Expression(").Append(expression.Expression.Syntax.ToString()).Append(')').AppendLine();
                break;
            case RazorVueTextNode text:
                builder.Append("Text(").Append(text.Text).Append(')').AppendLine();
                break;
            case RazorVueSlotOutletNode slotOutlet:
                builder.Append("Slot(").Append(slotOutlet.SlotName).Append(')').AppendLine();
                break;
            case RazorVueTemplateScopeNode scope:
                builder.Append("TemplateScope(").Append(scope.ScopeName).Append(')').AppendLine();
                AppendFragment(builder, scope.Children, depth + 1);
                break;
            case RazorVueLocalDeclarationNode local:
                builder.Append("Local(").Append(local.LocalSymbol.Name).Append(')').AppendLine();
                break;
            case RazorVueImperativeBlockNode imperative:
                builder
                    .Append("Imperative(")
                    .Append(imperative.Kind)
                    .Append("; locals=")
                    .Append(string.Join(",", imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name)))
                    .Append("; operations=")
                    .Append(string.Join(",", imperative.Operations.Select(static operation => operation.Kind.ToString())))
                    .Append(')')
                    .AppendLine();
                break;
            default:
                builder.Append(node.GetType().Name).AppendLine();
                break;
        }
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    [TestMethod]
    public void CreateRenderTree_ForLocalMarkupStringCarrierExpression_ProducesStaticMarkupNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                MarkupString markup = (MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>";
            }

            @markup
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.LocalMarkupStringCarrier.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.AreEqual("section", Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children[0]).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_ForImmediatelyAssignedLocalMarkupStringCarrierExpression_ProducesStaticMarkupNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                MarkupString markup;
                markup = (MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>";
            }

            @markup
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ImmediatelyAssignedLocalMarkupStringCarrier.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.AreEqual("section", Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children[0]).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentPropertyExpression_ProducesStructuredRenderNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @Template

            @code {
                private RenderFragment Template => @<section><span>safe</span><p>ok</p></section>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.PropertyRenderFragmentCarrier.Expression.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));
        var section = Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children[0]);
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
        Assert.AreEqual("span", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[0]).TagName);
        Assert.AreEqual("p", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[1]).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_ForImmediatelyAssignedLocalRenderFragmentCarrierExpression_ProducesStructuredRenderNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment template;
                template = @<section><span>safe</span><p>ok</p></section>;
            }

            @template
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.LocalRenderFragmentCarrier.Expression.ImmediateAssignment.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));
        var section = Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children[0]);
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_ForRenderFragmentPropertyBackedByFactoryExpression_ProducesCapturedScopeAndStructuredRenderNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @Template

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment Template => CreateTemplate(Title);

                private RenderFragment CreateTemplate(string? title)
                    => @<section><span>@title</span><p>ok</p></section>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.PropertyRenderFragmentCarrier.Expression.FactoryBacked.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(renderTree.Children[0]);
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var section = Assert.IsInstanceOfType<RazorVueElementNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
        Assert.AreEqual("span", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[0]).TagName);
        Assert.AreEqual("p", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[1]).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_ForImmediatelyAssignedLocalRenderFragmentCarrierInitializedFromFactoryMethodExpression_ProducesCapturedScopeAndStructuredRenderNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment template;
                template = CreateTemplate(Title);
            }

            @template

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment CreateTemplate(string? title)
                    => @<section><span>@title</span><p>ok</p></section>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.LocalRenderFragmentCarrier.Expression.Factory.ImmediateAssignment.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(renderTree.Children[0]);
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var section = Assert.IsInstanceOfType<RazorVueElementNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
        Assert.AreEqual("span", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[0]).TagName);
        Assert.AreEqual("p", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[1]).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedRenderFragmentPropertyInvocation_ProducesCapturedAndInvocationScopes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @Template(42)

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment<int> Template => CreateTemplate(Title);

                private RenderFragment<int> CreateTemplate(string? title)
                    => item => @<span>@title @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.PropertyRenderFragmentCarrier.Invocation.Typed.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(renderTree.Children[0]);
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var itemScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("item", itemScope.ScopeName);
        var literal = Assert.IsInstanceOfType<ILiteralOperation>(itemScope.Initializer);
        Assert.AreEqual(42, literal.ConstantValue.Value);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(itemScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        Assert.AreEqual(3, span.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[0]);
        var whitespace = Assert.IsInstanceOfType<RazorVueTextNode>(span.Children.Children[1]);
        Assert.AreEqual(" ", whitespace.Text);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[2]);
    }

    [TestMethod]
    public void CreateRenderTree_ForDirectRenderFragmentFactoryExpression_ProducesStructuredRenderNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @CreateTemplate(Title)

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment CreateTemplate(string? title)
                    => @<section><span>@title</span><p>ok</p></section>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactory.Expression.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(renderTree.Children[0]);
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var section = Assert.IsInstanceOfType<RazorVueElementNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[0]);
        Assert.AreEqual("span", span.TagName);
        Assert.AreEqual(1, span.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[0]);

        var paragraph = Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[1]);
        Assert.AreEqual("p", paragraph.TagName);
        Assert.AreEqual("ok", Assert.IsInstanceOfType<RazorVueTextNode>(paragraph.Children.Children.Single()).Text);
    }

    [TestMethod]
    public void CreateRenderTree_ForDirectRenderFragmentFactoryExpressionWithInParameter_ProducesStructuredRenderNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @CreateTemplate(Title)

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment CreateTemplate(in string? title)
                    => CreateTemplateCore(title);

                private RenderFragment CreateTemplateCore(string? capturedTitle)
                    => @<section><span>@capturedTitle</span><p>ok</p></section>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactory.InParameter.Expression.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(renderTree.Children[0]);
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var capturedTitleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("capturedTitle", capturedTitleScope.ScopeName);
        Assert.IsInstanceOfType<IParameterReferenceOperation>(capturedTitleScope.Initializer);

        var section = Assert.IsInstanceOfType<RazorVueElementNode>(capturedTitleScope.Children.Children.Single());
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
    }

    [TestMethod]
    public void CreateRenderTree_ForDirectRenderFragmentFactoryExpressionForwardingInParameterByReference_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @CreateTemplate(Title)

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment CreateTemplate(in string? title)
                {
                    ConsumeByRef(in title);
                    return @<section><span>safe</span></section>;
                }

                private static void ConsumeByRef(in string? value)
                {
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactory.InParameter.ByRefEscape.Tests",
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

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "by-reference");
        StringAssert.Contains(exception.Issue.Message, "fragment factory");
    }

    [TestMethod]
    public void CreateRenderTree_ForDirectRenderFragmentFactoryExpressionWithInParameter_IgnoresNestedLocalFunctionInParameterByRefForwarding()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @CreateTemplate(Title)

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment CreateTemplate(in string? title)
                    => builder =>
                    {
                        void Nested(in string? value)
                        {
                            ConsumeByRef(in value);
                        }

                        builder.OpenElement(1, "section");
                        builder.AddContent(2, "ok");
                        builder.CloseElement();
                    };

                private static void ConsumeByRef(in string? value)
                {
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactory.InParameter.NestedByRefLocal.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(renderTree.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        var section = Assert.IsInstanceOfType<RazorVueElementNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual("ok", Assert.IsInstanceOfType<RazorVueTextNode>(section.Children.Children.Single()).Text);
    }

    [TestMethod]
    public void CreateRenderTree_ForDirectRenderFragmentZeroArgumentFactoryExpression_ProducesCapturedScopeAndStructuredRenderNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @CreateTemplate()

            @code {
                private RenderFragment CreateTemplate(string? title = "fallback-title")
                    => @<section><span>@title</span><p>ok</p></section>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactoryZeroArg.Expression.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(renderTree.Children[0]);
        Assert.AreEqual("title", titleScope.ScopeName);
        var titleLiteral = Assert.IsInstanceOfType<ILiteralOperation>(titleScope.Initializer);
        Assert.AreEqual("fallback-title", titleLiteral.ConstantValue.Value);

        var section = Assert.IsInstanceOfType<RazorVueElementNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
        Assert.AreEqual("span", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[0]).TagName);
        Assert.AreEqual("p", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[1]).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_ForDirectRenderFragmentFactoryExpressionUsingNamedArgumentsOutOfDeclarationOrder_PreservesCallSiteEvaluationOrder()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @CreateTemplate(subtitle: Subtitle, title: Title)

            @code {
                [Parameter]
                public string? Title { get; set; }

                [Parameter]
                public string? Subtitle { get; set; }

                private RenderFragment CreateTemplate(string? title, string? subtitle)
                    => @<section><span>@title</span><p>@subtitle</p></section>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactoryNamed.Expression.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));

        var subtitleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(renderTree.Children[0]);
        Assert.AreEqual("subtitle", subtitleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(subtitleScope.Initializer);

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(subtitleScope.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var section = Assert.IsInstanceOfType<RazorVueElementNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);
        Assert.AreEqual("span", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[0]).TagName);
        Assert.AreEqual("p", Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[1]).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_ForDirectTypedRenderFragmentFactoryInvocation_ProducesCapturedAndInvocationScopes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @CreateTemplate(Title)(42)

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment<int> CreateTemplate(string? title)
                    => item => @<span>@title @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactory.Invocation.Typed.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(renderTree.Children[0]);
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var itemScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("item", itemScope.ScopeName);
        var literal = Assert.IsInstanceOfType<ILiteralOperation>(itemScope.Initializer);
        Assert.AreEqual(42, literal.ConstantValue.Value);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(itemScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        Assert.AreEqual(3, span.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[0]);
        var whitespace = Assert.IsInstanceOfType<RazorVueTextNode>(span.Children.Children[1]);
        Assert.AreEqual(" ", whitespace.Text);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[2]);
    }

    [TestMethod]
    public void CreateRenderTree_ForDirectTypedRenderFragmentZeroArgumentFactoryInvocation_ProducesCapturedAndInvocationScopes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @CreateTemplate()(42)

            @code {
                private RenderFragment<int> CreateTemplate(string? title = "fallback-title")
                    => item => @<span>@title @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactoryZeroArg.Invocation.Typed.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(renderTree.Children[0]);
        Assert.AreEqual("title", titleScope.ScopeName);
        var titleLiteral = Assert.IsInstanceOfType<ILiteralOperation>(titleScope.Initializer);
        Assert.AreEqual("fallback-title", titleLiteral.ConstantValue.Value);

        var itemScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("item", itemScope.ScopeName);
        var itemLiteral = Assert.IsInstanceOfType<ILiteralOperation>(itemScope.Initializer);
        Assert.AreEqual(42, itemLiteral.ConstantValue.Value);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(itemScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        Assert.AreEqual(3, span.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[0]);
        var whitespace = Assert.IsInstanceOfType<RazorVueTextNode>(span.Children.Children[1]);
        Assert.AreEqual(" ", whitespace.Text);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[2]);
    }

    [TestMethod]
    public void CreateRenderTree_ForDirectTypedRenderFragmentFactoryInvocationUsingNamedArgumentsOutOfDeclarationOrder_PreservesCallSiteEvaluationOrder()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @CreateTemplate(subtitle: Subtitle, title: Title)(42)

            @code {
                [Parameter]
                public string? Title { get; set; }

                [Parameter]
                public string? Subtitle { get; set; }

                private RenderFragment<int> CreateTemplate(string? title, string? subtitle)
                    => item => @<span>@title @subtitle @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactoryNamed.Invocation.Typed.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));

        var subtitleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(renderTree.Children[0]);
        Assert.AreEqual("subtitle", subtitleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(subtitleScope.Initializer);

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(subtitleScope.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var itemScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("item", itemScope.ScopeName);
        var itemLiteral = Assert.IsInstanceOfType<ILiteralOperation>(itemScope.Initializer);
        Assert.AreEqual(42, itemLiteral.ConstantValue.Value);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(itemScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        Assert.AreEqual(5, span.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[0]);
        Assert.AreEqual(" ", Assert.IsInstanceOfType<RazorVueTextNode>(span.Children.Children[1]).Text);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[2]);
        Assert.AreEqual(" ", Assert.IsInstanceOfType<RazorVueTextNode>(span.Children.Children[3]).Text);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[4]);
    }

    [TestMethod]
    public void CreateRenderTree_ForDirectTypedRenderFragmentLocalFunctionFactoryInvocation_ProducesCapturedAndInvocationScopes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> CreateTemplate(string? title)
                    => item => @<span>@title @item</span>;
            }

            @CreateTemplate(Title)(42)

            @code {
                [Parameter]
                public string? Title { get; set; }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactory.LocalFunction.Invocation.Typed.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(renderTree.Children[0]);
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var itemScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("item", itemScope.ScopeName);
        var itemLiteral = Assert.IsInstanceOfType<ILiteralOperation>(itemScope.Initializer);
        Assert.AreEqual(42, itemLiteral.ConstantValue.Value);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(itemScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        Assert.AreEqual(3, span.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[0]);
        Assert.AreEqual(" ", Assert.IsInstanceOfType<RazorVueTextNode>(span.Children.Children[1]).Text);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[2]);
    }

    [TestMethod]
    public void CreateRenderTree_ForDirectRenderFragmentLocalFunctionFactoryExpression_ProducesStructuredRenderNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment CreateTemplate(string? title)
                    => @<section><span>@title</span><p>ok</p></section>;
            }

            @CreateTemplate(Title)

            @code {
                [Parameter]
                public string? Title { get; set; }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactory.LocalFunction.Expression.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(renderTree.Children[0]);
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var section = Assert.IsInstanceOfType<RazorVueElementNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("section", section.TagName);
        Assert.AreEqual(2, section.Children.Children.Length);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[0]);
        Assert.AreEqual("span", span.TagName);
        Assert.AreEqual(1, span.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[0]);

        var paragraph = Assert.IsInstanceOfType<RazorVueElementNode>(section.Children.Children[1]);
        Assert.AreEqual("p", paragraph.TagName);
        Assert.AreEqual("ok", Assert.IsInstanceOfType<RazorVueTextNode>(paragraph.Children.Children.Single()).Text);
    }

    [TestMethod]
    public void CreateRenderTree_ForImmediatelyAssignedLocalTypedRenderFragmentCarrierInvocation_ProducesCapturedAndInvocationScopes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template;
                template = CreateTemplate(Title);
            }

            @template(42)

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment<int> CreateTemplate(string? title)
                    => item => @<span>@title @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.LocalRenderFragmentCarrier.Invocation.Typed.ImmediateAssignment.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(renderTree.Children[0]);
        Assert.AreEqual("title", titleScope.ScopeName);
        Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleScope.Initializer);

        var itemScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("item", itemScope.ScopeName);
        var literal = Assert.IsInstanceOfType<ILiteralOperation>(itemScope.Initializer);
        Assert.AreEqual(42, literal.ConstantValue.Value);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(itemScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
        Assert.AreEqual(3, span.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[0]);
        var whitespace = Assert.IsInstanceOfType<RazorVueTextNode>(span.Children.Children[1]);
        Assert.AreEqual(" ", whitespace.Text);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children[2]);
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedSlotOutletInvocation_ProducesStructuredSlotOutletNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @Header(Count + 1)
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.SlotOutlet.Invocation.Typed.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? Header { get; set; }

                    [Parameter]
                    public int Count { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(
            1,
            renderTree.Children.Length,
            RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot) + Environment.NewLine + DescribeStructure(renderTree));

        var slotOutlet = Assert.IsInstanceOfType<RazorVueSlotOutletNode>(renderTree.Children[0]);
        Assert.AreEqual("header", slotOutlet.SlotName);
        Assert.IsNotNull(slotOutlet.Argument);
        var argument = slotOutlet.Argument!;
        Assert.IsInstanceOfType<IBinaryOperation>(argument);
        Assert.AreEqual("Count + 1", argument.Syntax.ToString());
    }

    [TestMethod]
    public void CreateRenderTree_ForReadonlyMarkupStringPropertyExpression_ProducesStaticMarkupNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@HeroMarkup""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.PropertyMarkupStringCarrier.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private MarkupString HeroMarkup => (MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.AreEqual("section", Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children[0]).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_ForUnwrittenSettableMarkupStringPropertyExpression_ProducesStaticMarkupNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@HeroMarkup""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.SettablePropertyMarkupStringCarrier.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private MarkupString HeroMarkup { get; set; } = (MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.AreEqual("section", Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children[0]).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_ForReadonlyMarkupStringFieldExpression_ProducesStaticMarkupNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@_heroMarkup""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.FieldMarkupStringCarrier.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private readonly MarkupString _heroMarkup = (MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.AreEqual("section", Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children[0]).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_ForUnwrittenNonReadonlyMarkupStringFieldExpression_ProducesStaticMarkupNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@_heroMarkup""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.NonReadonlyFieldMarkupStringCarrier.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private MarkupString _heroMarkup = (MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.AreEqual("section", Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children[0]).TagName);
    }

    [TestMethod]
    public void CreateRenderTree_ForMarkupStringCastFromUnwrittenNonReadonlyStringFieldExpression_ProducesStaticMarkupNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@((MarkupString)_heroMarkup)""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.StringFieldMarkupStringCastCarrier.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private string _heroMarkup = "<section class='hero'><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.AreEqual("section", Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children[0]).TagName);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerConstantMarkupStringExpression()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@((MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>")""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.MarkupStringExpression.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "h(\"section\", { \"class\": \"hero\" }, [h(\"span\", null, \"safe\"), h(\"p\", null, \"ok\")])");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerLocalMarkupStringCarrierExpression()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                MarkupString markup = (MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>";
            }

            @markup
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.LocalMarkupStringCarrier.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "h(\"section\", { \"class\": \"hero\" }, [h(\"span\", null, \"safe\"), h(\"p\", null, \"ok\")])");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerImmediatelyAssignedLocalMarkupStringCarrierExpression()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                MarkupString markup;
                markup = (MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>";
            }

            @markup
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ImmediatelyAssignedLocalMarkupStringCarrier.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "h(\"section\", { \"class\": \"hero\" }, [h(\"span\", null, \"safe\"), h(\"p\", null, \"ok\")])");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerImmediatelyAssignedLocalMarkupStringCarrierExpressionAfterSiblingLocalDeclaration()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                MarkupString markup;
                var revision = 0;
                markup = (MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>";
            }

            @markup
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ImmediatelyAssignedLocalMarkupStringCarrier.SiblingDeclarationImmediateAssignment.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "const revision = 0;");
        StringAssert.Contains(artifact.ModuleCode, "h(\"section\", { \"class\": \"hero\" }, [h(\"span\", null, \"safe\"), h(\"p\", null, \"ok\")])");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerUnwrittenSettableMarkupStringPropertyExpression()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@HeroMarkup""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.SettablePropertyMarkupStringCarrier.Pipeline.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private MarkupString HeroMarkup { get; set; } = (MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "h(\"section\", { \"class\": \"hero\" }, [h(\"span\", null, \"safe\"), h(\"p\", null, \"ok\")])");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerUnwrittenNonReadonlyMarkupStringFieldExpression()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@_heroMarkup""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.NonReadonlyFieldMarkupStringCarrier.Pipeline.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private MarkupString _heroMarkup = (MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "h(\"section\", { \"class\": \"hero\" }, [h(\"span\", null, \"safe\"), h(\"p\", null, \"ok\")])");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerMarkupStringCastFromUnwrittenNonReadonlyStringFieldExpression()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@((MarkupString)_heroMarkup)""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.StringFieldMarkupStringCastCarrier.Pipeline.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private string _heroMarkup = "<section class='hero'><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "h(\"section\", { \"class\": \"hero\" }, [h(\"span\", null, \"safe\"), h(\"p\", null, \"ok\")])");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersConstantMarkupStringExpression()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@((MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>")""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.MarkupStringExpression.Sfc.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "safe");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersImmediatelyAssignedLocalMarkupStringCarrierExpression()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                MarkupString markup;
                markup = (MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>";
            }

            @markup
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ImmediatelyAssignedLocalMarkupStringCarrier.Sfc.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "<span>");
        StringAssert.Contains(artifact.TemplateText, "safe");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersImmediatelyAssignedLocalMarkupStringCarrierExpressionAfterSiblingLocalDeclaration()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                MarkupString markup;
                var revision = 0;
                markup = (MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>";
            }

            @markup
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ImmediatelyAssignedLocalMarkupStringCarrier.SiblingDeclarationImmediateAssignment.Sfc.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(revision) in [0]\">");
        StringAssert.Contains(artifact.TemplateText, "<section class=\"hero\">");
        StringAssert.Contains(artifact.TemplateText, "safe");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForDynamicMarkupStringExpression_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@((MarkupString)(Title ?? "<section class='hero'>safe</section>"))""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DynamicMarkupStringExpression.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "MarkupString");
        StringAssert.Contains(exception.Issue.Message, "compile-time provable static");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForDynamicNewMarkupStringExpression_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@(new MarkupString("<section>" + Title + "</section>"))""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DynamicNewMarkupStringExpression.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "MarkupString");
        StringAssert.Contains(exception.Issue.Message, "compile-time provable static");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForMutatedStringFieldMarkupStringCastCarrier_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@((MarkupString)_heroMarkup)""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.MutatedStringFieldMarkupStringCastCarrier.Pipeline.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private string _heroMarkup = "<section class='hero'><span>safe</span><p>ok</p></section>";

                    protected override void OnParametersSet()
                    {
                        _heroMarkup = "<section class='hero'><span>changed</span></section>";
                    }
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "_heroMarkup");
        StringAssert.Contains(exception.Issue.Message, "later writes");
    }

    [TestMethod]
    public void CreateRenderTree_ForScriptMarkupStringExpression_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@((MarkupString)"<script>alert('x')</script>")""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ScriptMarkupStringExpression.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "raw markup execution");
        StringAssert.Contains(exception.Issue.Message, "script");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_ForInlineEventMarkupStringExpression_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@((MarkupString)"<section onclick='alert(1)'>safe</section>")""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.InlineEventMarkupStringExpression.Sfc.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "raw markup execution");
        StringAssert.Contains(exception.Issue.Message, "onclick");
    }

    [TestMethod]
    public void CreateRenderTree_ForRazorEventDirectiveMarkupBlock_AllowsRazorEventFallback()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<button @onclick="OnClick">Go</button>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RazorEventDirectiveMarkupBlock.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private void OnClick()
                    {
                    }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var button = Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children.Single());
        Assert.AreEqual("button", button.TagName);
        Assert.IsTrue(button.Attributes.OfType<RazorVueAttributeNode>().Any(static attribute => attribute.Name == "onclick"));
    }

    [TestMethod]
    public void CreateRenderTree_ForDirectiveMarkupStringExpression_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@((MarkupString)"<section v-html='payload'>safe</section>")""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectiveMarkupStringExpression.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "raw markup execution");
        StringAssert.Contains(exception.Issue.Message, "v-html");
    }

    [TestMethod]
    public void CreateRenderTree_ForMalformedStaticMarkupNameMarkupStringExpression_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@((MarkupString)"<section><x<script>safe</x<script></section>")""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.MalformedStaticMarkupName.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "static markup element name");
        StringAssert.Contains(exception.Issue.Message, "x<script");
    }

    [TestMethod]
    public void CreateRenderTree_ForSrcdocMarkupStringExpression_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@((MarkupString)"<section srcdoc='<p>unsafe</p>'>safe</section>")""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.SrcdocMarkupStringExpression.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "raw markup execution attribute");
        StringAssert.Contains(exception.Issue.Message, "srcdoc");
    }

    [TestMethod]
    public void CreateRenderTree_ForExecutableDataUriMarkupStringExpression_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@((MarkupString)"<a href='data:text/html,%3Cscript%3Ealert(1)%3C/script%3E'>safe</a>")""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ExecutableDataUriMarkupStringExpression.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "raw markup execution URL");
        StringAssert.Contains(exception.Issue.Message, "href");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForReassignedLocalMarkupStringCarrier_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                MarkupString markup;
                markup = (MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>";
                markup = (MarkupString)"<section class='hero'><span>changed</span></section>";
            }

            @markup
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ReassignedLocalMarkupStringCarrier.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
                .Execute(context));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "MarkupString local 'markup'");
        StringAssert.Contains(exception.Issue.Message, "later writes");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForLocalFunctionMutatedMarkupStringCarrier_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                MarkupString markup = (MarkupString)"<section class='hero'><span>safe</span></section>";

                void Mutate()
                {
                    markup = (MarkupString)"<section class='hero'><span>changed</span></section>";
                }
            }

            @markup
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.LocalFunctionMutatedMarkupStringCarrier.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
                .Execute(context));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "MarkupString local 'markup'");
        StringAssert.Contains(exception.Issue.Message, "later writes");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForRefEscapedMarkupStringCarrier_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                MarkupString markup = (MarkupString)"<section class='hero'><span>safe</span></section>";
                Replace(ref markup);
            }

            @markup

            @code {
                private static void Replace(ref MarkupString markup)
                {
                    markup = (MarkupString)"<section class='hero'><span>changed</span></section>";
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RefEscapedMarkupStringCarrier.Pipeline.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
                .Execute(context));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "MarkupString local 'markup'");
        StringAssert.Contains(exception.Issue.Message, "later writes");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForNonPrivateSettableMarkupStringPropertyExpression_ThrowsCanonicalizationFailed()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """@HeroMarkup""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.NonPrivateSettablePropertyMarkupStringCarrier.Pipeline.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    internal MarkupString HeroMarkup { get; set; } = (MarkupString)"<section class='hero'><span>safe</span><p>ok</p></section>";
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(
            () => new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "MarkupString");
        StringAssert.Contains(exception.Issue.Message, "compile-time provable static");
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
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateLocalCodeBlock()
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
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorNodes.push(h(\"p\", null, decorated));");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateLocalCodeBlockWithoutInitializerThenImmediateAssignment()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        string? decorated;
                        decorated = item;
                    }
                    <p>@decorated</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.ImmediateAssignment.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "const decorated = item;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorNodes.push(h(\"p\", null, decorated));");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateCodeBlockWithStandaloneIf()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        if (Show)
                        {
                            <p>@item</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneIf.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (props.show ? h(\"p\", null, item) : null)");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTypedChildContentTemplateLocalCodeBlockWithoutInitializerThenImmediateAssignment()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        string? decorated;
                        decorated = item;
                    }
                    <p>@decorated</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.ImmediateAssignment.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(decorated) in [item]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ decorated }}");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersImmediatelyAssignedRenderFragmentLocalCarrierInitializedFromFactoryMethod()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template;
                template = CreateTemplate(Title);
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
            "RazorVue.RazorIr.TemplateFrontend.RenderFragmentLocalCarrier.Factory.TypedSlot.ImmediateAssignment.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template #itemTemplate=\"item\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTypedRenderFragmentPropertyInvocation()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @Template(42)

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment<int> Template => CreateTemplate(Title);

                private RenderFragment<int> CreateTemplate(string? title)
                    => item => @<span>@title @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.PropertyRenderFragmentCarrier.Invocation.Typed.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersDirectRenderFragmentFactoryExpression()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @CreateTemplate(Title)

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment CreateTemplate(string? title)
                    => @<section><span>@title</span><p>ok</p></section>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactory.Expression.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
        StringAssert.Contains(artifact.TemplateText, "</p>");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersDirectRenderFragmentFactoryExpressionWithInParameter()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @CreateTemplate(Title)

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment CreateTemplate(in string? title)
                    => CreateTemplateCore(title);

                private RenderFragment CreateTemplateCore(string? capturedTitle)
                    => @<section><span>@capturedTitle</span><p>ok</p></section>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactory.InParameter.Expression.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(capturedTitle) in [title]\">");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ capturedTitle }}");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersRenderFragmentPropertyBackedByFactoryExpression()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @Template

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment Template => CreateTemplate(Title);

                private RenderFragment CreateTemplate(string? title)
                    => @<section><span>@title</span><p>ok</p></section>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.PropertyRenderFragmentCarrier.Expression.FactoryBacked.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersImmediatelyAssignedLocalRenderFragmentCarrierInitializedFromFactoryMethodExpression()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment template;
                template = CreateTemplate(Title);
            }

            @template

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment CreateTemplate(string? title)
                    => @<section><span>@title</span><p>ok</p></section>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.LocalRenderFragmentCarrier.Expression.Factory.ImmediateAssignment.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersDirectRenderFragmentZeroArgumentFactoryExpression()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @CreateTemplate()

            @code {
                private RenderFragment CreateTemplate(string? title = "fallback-title")
                    => @<section><span>@title</span><p>ok</p></section>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactoryZeroArg.Expression.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [\"fallback-title\"]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        StringAssert.Contains(artifact.TemplateText, "ok");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersDirectRenderFragmentFactoryExpressionUsingNamedArgumentsOutOfDeclarationOrder_PreservingCallSiteEvaluationOrder()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @CreateTemplate(subtitle: Subtitle, title: Title)

            @code {
                [Parameter]
                public string? Title { get; set; }

                [Parameter]
                public string? Subtitle { get; set; }

                private RenderFragment CreateTemplate(string? title, string? subtitle)
                    => @<section><span>@title</span><p>@subtitle</p></section>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactoryNamed.Expression.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        var subtitleIndex = artifact.TemplateText.IndexOf("<template v-for=\"(subtitle) in [props.subtitle]\">", StringComparison.Ordinal);
        var titleIndex = artifact.TemplateText.IndexOf("<template v-for=\"(title) in [props.title]\">", StringComparison.Ordinal);
        Assert.IsTrue(subtitleIndex >= 0, artifact.TemplateText);
        Assert.IsTrue(titleIndex >= 0, artifact.TemplateText);
        Assert.IsTrue(subtitleIndex < titleIndex, artifact.TemplateText);
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        StringAssert.Contains(artifact.TemplateText, "{{ subtitle }}");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersDirectTypedRenderFragmentFactoryInvocation()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @CreateTemplate(Title)(42)

            @code {
                [Parameter]
                public string? Title { get; set; }

                private RenderFragment<int> CreateTemplate(string? title)
                    => item => @<span>@title @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactory.Invocation.Typed.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersDirectRenderFragmentLocalFunctionFactoryExpression()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment CreateTemplate(string? title)
                    => @<section><span>@title</span><p>ok</p></section>;
            }

            @CreateTemplate(Title)

            @code {
                [Parameter]
                public string? Title { get; set; }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactory.LocalFunction.Expression.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        StringAssert.Contains(artifact.TemplateText, "<p>");
        StringAssert.Contains(artifact.TemplateText, "ok");
        StringAssert.Contains(artifact.TemplateText, "</p>");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersDirectTypedRenderFragmentZeroArgumentFactoryInvocation()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @CreateTemplate()(42)

            @code {
                private RenderFragment<int> CreateTemplate(string? title = "fallback-title")
                    => item => @<span>@title @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactoryZeroArg.Invocation.Typed.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(title) in [\"fallback-title\"]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(item) in [42]\">");
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersDirectTypedRenderFragmentFactoryInvocationUsingNamedArgumentsOutOfDeclarationOrder_PreservingCallSiteEvaluationOrder()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @CreateTemplate(subtitle: Subtitle, title: Title)(42)

            @code {
                [Parameter]
                public string? Title { get; set; }

                [Parameter]
                public string? Subtitle { get; set; }

                private RenderFragment<int> CreateTemplate(string? title, string? subtitle)
                    => item => @<span>@title @subtitle @item</span>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactoryNamed.Invocation.Typed.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        var subtitleIndex = artifact.TemplateText.IndexOf("<template v-for=\"(subtitle) in [props.subtitle]\">", StringComparison.Ordinal);
        var titleIndex = artifact.TemplateText.IndexOf("<template v-for=\"(title) in [props.title]\">", StringComparison.Ordinal);
        var itemIndex = artifact.TemplateText.IndexOf("<template v-for=\"(item) in [42]\">", StringComparison.Ordinal);
        Assert.IsTrue(subtitleIndex >= 0, artifact.TemplateText);
        Assert.IsTrue(titleIndex >= 0, artifact.TemplateText);
        Assert.IsTrue(itemIndex >= 0, artifact.TemplateText);
        Assert.IsTrue(subtitleIndex < titleIndex, artifact.TemplateText);
        Assert.IsTrue(subtitleIndex < itemIndex, artifact.TemplateText);
        Assert.IsTrue(titleIndex < itemIndex, artifact.TemplateText);
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersDirectTypedRenderFragmentLocalFunctionFactoryInvocation()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> CreateTemplate(string? title)
                    => item => @<span>@title @item</span>;
            }

            @CreateTemplate(Title)(42)

            @code {
                [Parameter]
                public string? Title { get; set; }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.DirectFactory.LocalFunction.Invocation.Typed.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        var titleIndex = artifact.TemplateText.IndexOf("<template v-for=\"(title) in [props.title]\">", StringComparison.Ordinal);
        var itemIndex = artifact.TemplateText.IndexOf("<template v-for=\"(item) in [42]\">", StringComparison.Ordinal);
        Assert.IsTrue(titleIndex >= 0, artifact.TemplateText);
        Assert.IsTrue(itemIndex >= 0, artifact.TemplateText);
        Assert.IsTrue(titleIndex < itemIndex, artifact.TemplateText);
        StringAssert.Contains(artifact.TemplateText, "{{ title }}");
        StringAssert.Contains(artifact.TemplateText, "{{ item }}");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTypedSlotOutletInvocation()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @Header(Count + 1)
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.SlotOutlet.Invocation.Typed.Sfc.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? Header { get; set; }

                    [Parameter]
                    public int Count { get; set; }
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.TemplateText, "<slot name=\"header\" :value=\"(props.count + 1)\" />");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTemplateLocalCodeBlockWithNestedIf()
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
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.NestedIf.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const localTitle = props.title;");
        StringAssert.Contains(artifact.ModuleCode, "(props.show ? h(\"section\", null, localTitle) : null)");
    }

    [TestMethod]
    public void CreateRenderTree_ForTemplateLocalCodeBlockWithNestedIfElse_ProducesLocalThenConditionalBranches()
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
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.NestedIfElse.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(2, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var local = Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(renderTree.Children[0]);
        Assert.AreEqual("localTitle", local.LocalSymbol.Name);

        var conditional = Assert.IsInstanceOfType<RazorVueConditionalNode>(renderTree.Children[1]);
        var section = Assert.IsInstanceOfType<RazorVueElementNode>(conditional.WhenTrue.Children.Single());
        Assert.AreEqual("section", section.TagName);
        var paragraph = Assert.IsInstanceOfType<RazorVueElementNode>(conditional.WhenFalse.Children.Single());
        Assert.AreEqual("p", paragraph.TagName);
        Assert.AreEqual("hidden", Assert.IsInstanceOfType<RazorVueTextNode>(paragraph.Children.Children.Single()).Text);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTemplateLocalCodeBlockWithNestedIfElse()
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
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.NestedIfElse.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const localTitle = props.title;");
        StringAssert.Contains(artifact.ModuleCode, "(props.show ? h(\"section\", null, localTitle) : h(\"p\", null, \"hidden\"))");
    }

    [TestMethod]
    public void CreateRenderTree_ForTemplateLocalCodeBlockWithNestedForeach_ProducesLocalThenLoop()
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
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.NestedForEach.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(2, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var local = Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(renderTree.Children[0]);
        Assert.AreEqual("prefix", local.LocalSymbol.Name);

        var loop = Assert.IsInstanceOfType<RazorVueForEachNode>(renderTree.Children[1]);
        Assert.AreEqual("item", loop.ItemName);
        var paragraph = Assert.IsInstanceOfType<RazorVueElementNode>(loop.Body.Children.Single());
        Assert.AreEqual("p", paragraph.TagName);
        Assert.AreEqual(3, paragraph.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(paragraph.Children.Children[0]);
        Assert.AreEqual(" ", Assert.IsInstanceOfType<RazorVueTextNode>(paragraph.Children.Children[1]).Text);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(paragraph.Children.Children[2]);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTemplateLocalCodeBlockWithNestedForeach()
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
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.NestedForEach.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const prefix = props.title;");
        StringAssert.Contains(artifact.ModuleCode, "props.items.map((item) => h(\"p\", null, [prefix, \" \", item]))");
    }

    [TestMethod]
    public void CreateRenderTree_ForTemplateLocalCodeBlockWithNestedFor_ProducesLocalThenCountLoop()
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
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.NestedFor.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(2, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var local = Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(renderTree.Children[0]);
        Assert.AreEqual("prefix", local.LocalSymbol.Name);

        var loop = Assert.IsInstanceOfType<RazorVueForNode>(renderTree.Children[1]);
        Assert.AreEqual("i", loop.VariableName);
        var paragraph = Assert.IsInstanceOfType<RazorVueElementNode>(loop.Body.Children.Single());
        Assert.AreEqual("p", paragraph.TagName);
        Assert.AreEqual(3, paragraph.Children.Children.Length);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(paragraph.Children.Children[0]);
        Assert.AreEqual(" ", Assert.IsInstanceOfType<RazorVueTextNode>(paragraph.Children.Children[1]).Text);
        Assert.IsInstanceOfType<RazorVueExpressionNode>(paragraph.Children.Children[2]);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTemplateLocalCodeBlockWithNestedFor()
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
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.NestedFor.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const prefix = props.title;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorVueForRange(0, props.count, \"<\", \"++\", null).map((i) => h(\"p\", null, [prefix, \" \", i]))");
        StringAssert.Contains(artifact.ModuleCode, "h(\"p\", null, [prefix, \" \", i])");
    }

    [TestMethod]
    public void CreateRenderTree_ForTemplateLocalCodeBlockWithSequentialIfs_ProducesLocalThenTwoConditionals()
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
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.SequentialIfs.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(3, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(renderTree.Children[0]);
        Assert.IsInstanceOfType<RazorVueConditionalNode>(renderTree.Children[1]);
        Assert.IsInstanceOfType<RazorVueConditionalNode>(renderTree.Children[2]);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTemplateLocalCodeBlockWithSequentialIfs()
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
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.SequentialIfs.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const localTitle = props.title;");
        StringAssert.Contains(artifact.ModuleCode, "(props.showPrimary ? h(\"section\", null, localTitle) : null)");
        StringAssert.Contains(artifact.ModuleCode, "(props.showSecondary ? h(\"p\", null, \"secondary\") : null)");
    }

    [TestMethod]
    public void CreateRenderTree_ForTemplateLocalCodeBlockWithIfThenForeach_ProducesSequentialControlNodes()
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
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.IfThenForEach.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(3, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(renderTree.Children[0]);
        Assert.IsInstanceOfType<RazorVueConditionalNode>(renderTree.Children[1]);
        Assert.IsInstanceOfType<RazorVueForEachNode>(renderTree.Children[2]);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTemplateLocalCodeBlockWithIfThenForeach()
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
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.IfThenForEach.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const prefix = props.title;");
        StringAssert.Contains(artifact.ModuleCode, "(props.showPrimary ? h(\"section\", null, prefix) : null)");
        StringAssert.Contains(artifact.ModuleCode, "props.items.map((item) => h(\"p\", null, [prefix, \" \", item]))");
    }

    [TestMethod]
    public void CreateRenderTree_ForTemplateLocalCodeBlockWithForeachThenIf_ProducesSequentialControlNodes()
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
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.ForEachThenIf.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(3, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(renderTree.Children[0]);
        Assert.IsInstanceOfType<RazorVueForEachNode>(renderTree.Children[1]);
        Assert.IsInstanceOfType<RazorVueConditionalNode>(renderTree.Children[2]);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTemplateLocalCodeBlockWithForeachThenIf()
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
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.ForEachThenIf.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const prefix = props.title;");
        StringAssert.Contains(artifact.ModuleCode, "props.items.map((item) => h(\"p\", null, [prefix, \" \", item]))");
        StringAssert.Contains(artifact.ModuleCode, "(props.showTail ? h(\"section\", null, prefix) : null)");
    }

    [TestMethod]
    public void CreateRenderTree_ForTemplateLocalCodeBlockWithForThenIf_ProducesSequentialControlNodes()
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
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.ForThenIf.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(3, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(renderTree.Children[0]);
        Assert.IsInstanceOfType<RazorVueForNode>(renderTree.Children[1]);
        Assert.IsInstanceOfType<RazorVueConditionalNode>(renderTree.Children[2]);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTemplateLocalCodeBlockWithForThenIf()
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
            "RazorVue.RazorIr.TemplateFrontend.TemplateLocalCodeBlock.ForThenIf.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const prefix = props.title;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorVueForRange(0, props.count, \"<\", \"++\", null).map((i) => h(\"p\", null, [prefix, \" \", i]))");
        StringAssert.Contains(artifact.ModuleCode, "(props.showTail ? h(\"section\", null, prefix) : null)");
    }

    [TestMethod]
    public void CreateRenderTree_ForTemplateLocalCodeBlockWithWhileLoop_ProducesLocalThenImperativeLoopBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        while (Show)
                        {
                            <p>@decorated</p>
                            break;
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.While.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(2, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var local = Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(itemTemplateSlot.Children.Children[0]);
        Assert.AreEqual("decorated", local.LocalSymbol.Name);

        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children[1]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "decorated");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateCodeBlockWithStandaloneWhileLoop_ProducesImperativeLoopBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        while (Show)
                        {
                            <p>@item</p>
                            break;
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneWhile.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(1, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));

        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children.Single());
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.DoesNotContain(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "item");
        CollectionAssert.Contains(imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(), "item");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTemplateLocalCodeBlockWithWhileLoop()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        while (Show)
                        {
                            <p>@decorated</p>
                            break;
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.While.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.ModuleCode, "while (props.show)");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(decorated);");
        StringAssert.Contains(artifact.ModuleCode, "break;");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateCodeBlockWithStandaloneWhileLoop()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        while (Show)
                        {
                            <p>@item</p>
                            break;
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneWhile.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "while (props.show)");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.enterElement(\"p\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(item);");
        StringAssert.Contains(artifact.ModuleCode, "break;");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentDirectWhileLoop_ProducesImperativeLoopBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @while (Show)
                    {
                        <p>@item</p>
                        break;
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.DirectWhile.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(1, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));

        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children.Single());
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.DoesNotContain(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "item");
        CollectionAssert.Contains(imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(), "item");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentDirectWhileLoop()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @while (Show)
                    {
                        <p>@item</p>
                        break;
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.DirectWhile.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "while (props.show)");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.enterElement(\"p\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(item);");
        StringAssert.Contains(artifact.ModuleCode, "break;");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTypedChildContentDirectWhileLoop_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @while (Show)
                    {
                        <p>@item</p>
                        break;
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.DirectWhile.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.SfcText, "while (props.show)");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeContext0.append(item);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateCodeBlockWithStandaloneSwitch_ProducesImperativeSwitchBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        switch (Mode)
                        {
                            case 0:
                                <p>@item</p>
                                break;
                            default:
                                <section>fallback</section>
                                break;
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneSwitch.Tests",
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
                    public int Mode { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(1, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children.Single());
        Assert.AreEqual(RazorVueImperativeBlockKind.SwitchBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(), "item");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateCodeBlockWithStandaloneSwitch()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        switch (Mode)
                        {
                            case 0:
                                <p>@item</p>
                                break;
                            default:
                                <section>fallback</section>
                                break;
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneSwitch.Pipeline.Tests",
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
                    public int Mode { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "switch (props.mode)");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(item);");
        StringAssert.Contains(artifact.ModuleCode, "\"fallback\"");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateCodeBlockWithStandaloneTryCatchFinally_ProducesImperativeTryBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        try
                        {
                            <section>@item</section>
                        }
                        catch
                        {
                            <p>fallback</p>
                        }
                        finally
                        {
                            _count++;
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneTryCatchFinally.Tests",
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
                    private int _count;
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(1, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children.Single());
        Assert.AreEqual(RazorVueImperativeBlockKind.TryBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(), "item");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateCodeBlockWithStandaloneTryCatchFinally()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        try
                        {
                            <section>@item</section>
                        }
                        catch
                        {
                            <p>fallback</p>
                        }
                        finally
                        {
                            _count++;
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneTryCatchFinally.Pipeline.Tests",
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
                    private int _count;
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "try {");
        StringAssert.Contains(artifact.ModuleCode, "} catch {");
        StringAssert.Contains(artifact.ModuleCode, "} finally {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(item);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateCodeBlockWithStandaloneFieldMutation_ProducesImperativeLocalBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        _count++;
                    }
                    <p>@item @_count</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneFieldMutation.Tests",
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
                    private int _count = 1;
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(1, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children.Single());
        Assert.AreEqual(RazorVueImperativeBlockKind.LocalBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(), "item");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateCodeBlockWithStandaloneFieldMutation()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        _count++;
                    }
                    <p>@item @_count</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneFieldMutation.Pipeline.Tests",
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
                    private int _count = 1;
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "_count++;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(item);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(_count);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateCodeBlockWithStandaloneConditionalReturnAndTailMarkup_ProducesSingleImperativeMethodBodyNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        if (Hide)
                        {
                            return;
                        }
                    }
                    <p>@item</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneConditionalReturnTail.Tests",
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
                    public bool Hide { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(1, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children.Single());
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(), "item");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateCodeBlockWithStandaloneConditionalReturnAndTailMarkup()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        if (Hide)
                        {
                            return;
                        }
                    }
                    <p>@item</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneConditionalReturnTail.Pipeline.Tests",
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
                    public bool Hide { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "if (props.hide) {");
        StringAssert.Contains(artifact.ModuleCode, "return __jazorImperativeContext0.finish();");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.enterElement(\"p\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(item);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentDirectIfWithReturnAndTailMarkup_ProducesMixedImperativeAndDeclarativeNodes()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @if (Hide)
                    {
                        return;
                    }
                    <p>@item</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.DirectIfReturnTail.Tests",
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
                    public bool Hide { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(2, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children[0]);
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, imperative.Kind);
        var tail = Assert.IsInstanceOfType<RazorVueElementNode>(itemTemplateSlot.Children.Children[1]);
        Assert.AreEqual("p", tail.TagName);
        var tailExpression = Assert.IsInstanceOfType<RazorVueExpressionNode>(tail.Children.Children.Single());
        var parameterReference = Assert.IsInstanceOfType<IParameterReferenceOperation>(tailExpression.Expression);
        Assert.AreEqual("item", parameterReference.Parameter.Name);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentDirectIfWithReturnAndTailMarkup()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @if (Hide)
                    {
                        return;
                    }
                    <p>@item</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.DirectIfReturnTail.Pipeline.Tests",
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
                    public bool Hide { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "if (props.hide) {");
        StringAssert.Contains(artifact.ModuleCode, "return __jazorImperativeContext0.finish();");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(h(\"p\", null, item));");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTypedChildContentDirectIfWithReturnAndTailMarkup_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @if (Hide)
                    {
                        return;
                    }
                    <p>@item</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.DirectIfReturnTail.Sfc.Tests",
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
                    public bool Hide { get; set; }
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.SfcText, "if (props.hide) {");
        StringAssert.Contains(artifact.SfcText, "return __jazorImperativeContext0.finish();");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeContext0.append(h(\"p\", null, item));");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateCodeBlockWithStandaloneUsingDeclarationAndTailMarkup_ProducesSingleImperativeTryBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        using var disposable = CreateDisposable();
                    }
                    <section>@item</section>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneUsingDeclarationTail.Tests",
            documentPath,
            documentText,
            """
            using System;

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
                    private sealed class TestDisposable : IDisposable
                    {
                        public void Dispose() { }
                    }

                    private TestDisposable CreateDisposable() => new();
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(1, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children.Single());
        Assert.AreEqual(RazorVueImperativeBlockKind.TryBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(), "item");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateCodeBlockWithStandaloneUsingDeclarationAndTailMarkup()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        using var disposable = CreateDisposable();
                    }
                    <section>@item</section>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateCodeBlock.StandaloneUsingDeclarationTail.Pipeline.Tests",
            documentPath,
            documentText,
            """
            using System;

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
                    private sealed class TestDisposable : IDisposable
                    {
                        public void Dispose() { }
                    }

                    private TestDisposable CreateDisposable() => new();
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "let disposable = createDisposable();");
        StringAssert.Contains(artifact.ModuleCode, "try {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(item);");
        StringAssert.Contains(artifact.ModuleCode, "disposable.dispose();");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTemplateLocalCodeBlockWithWhileLoop_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        while (Show)
                        {
                            <p>@decorated</p>
                            break;
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.While.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.SfcText, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.SfcText, "while (props.show)");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeContext0.append(decorated);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateLocalCodeBlockWithConditionalReturn_ProducesLocalThenImperativeMethodBodyTail()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        if (Hide)
                        {
                            return;
                        }
                    }
                    <p>@decorated</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.ConditionalReturn.Tests",
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
                    public bool Hide { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(2, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(itemTemplateSlot.Children.Children[0]);
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children[1]);
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "decorated");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateLocalCodeBlockWithConditionalReturn()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        if (Hide)
                        {
                            return;
                        }
                    }
                    <p>@decorated</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.ConditionalReturn.Pipeline.Tests",
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
                    public bool Hide { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.ModuleCode, "if (props.hide) {");
        StringAssert.Contains(artifact.ModuleCode, "return __jazorImperativeContext0.finish();");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.enterElement(\"p\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(decorated);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.leaveElement();");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateLocalCodeBlockWithThrowStatement_ProducesLocalThenImperativeMethodBodyTail()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        if (Fail)
                        {
                            throw new InvalidOperationException("boom");
                        }
                    }
                    <p>@decorated</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.Throw.Tests",
            documentPath,
            documentText,
            """
            using System;

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
                    public bool Fail { get; set; }
                }
            }
            """,
            importsText: "@using System");

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(2, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(itemTemplateSlot.Children.Children[0]);
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children[1]);
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "decorated");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateLocalCodeBlockWithThrowStatement()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        if (Fail)
                        {
                            throw new InvalidOperationException("boom");
                        }
                    }
                    <p>@decorated</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.Throw.Pipeline.Tests",
            documentPath,
            documentText,
            """
            using System;

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
                    public bool Fail { get; set; }
                }
            }
            """,
            importsText: "@using System");

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.ModuleCode, "if (props.fail) {");
        StringAssert.Contains(artifact.ModuleCode, "throw new Error(\"boom\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.enterElement(\"p\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(decorated);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.leaveElement();");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateLocalCodeBlockWithFieldMutation_ProducesLocalThenImperativeLocalBlockTail()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        _count++;
                    }
                    <p>@decorated @_count</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.FieldMutation.Tests",
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
                    private int _count = 1;
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(2, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(itemTemplateSlot.Children.Children[0]);
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children[1]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LocalBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "decorated");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateLocalCodeBlockWithFieldMutation()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        _count++;
                    }
                    <p>@decorated @_count</p>
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.FieldMutation.Pipeline.Tests",
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
                    private int _count = 1;
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.ModuleCode, "_count++;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.enterElement(\"p\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(decorated);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(\" \");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(_count);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.leaveElement();");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateLocalCodeBlockWithDoWhileLoop_ProducesLocalThenImperativeLoopBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        do
                        {
                            <p>@decorated</p>
                            break;
                        }
                        while (Show);
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.DoWhile.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(2, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(itemTemplateSlot.Children.Children[0]);
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children[1]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "decorated");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateLocalCodeBlockWithDoWhileLoop()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        do
                        {
                            <p>@decorated</p>
                            break;
                        }
                        while (Show);
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.DoWhile.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.ModuleCode, "do {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(decorated);");
        StringAssert.Contains(artifact.ModuleCode, "while (props.show);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateLocalCodeBlockWithForeachContinue_ProducesLocalThenImperativeLoopBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        foreach (var tag in Tags!)
                        {
                            if (tag == SkipTag)
                            {
                                continue;
                            }

                            <p>@decorated @tag</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.ForeachContinue.Tests",
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
                    public IReadOnlyList<string>? Tags { get; set; }

                    [Parameter]
                    public string? SkipTag { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(2, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(itemTemplateSlot.Children.Children[0]);
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children[1]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "decorated");
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "tag");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateLocalCodeBlockWithForeachContinue()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        foreach (var tag in Tags!)
                        {
                            if (tag == SkipTag)
                            {
                                continue;
                            }

                            <p>@decorated @tag</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.ForeachContinue.Pipeline.Tests",
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
                    public IReadOnlyList<string>? Tags { get; set; }

                    [Parameter]
                    public string? SkipTag { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.ModuleCode, "for (let tag of props.tags)");
        StringAssert.Contains(artifact.ModuleCode, "if (tag === props.skipTag) {");
        StringAssert.Contains(artifact.ModuleCode, "continue;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(decorated);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(tag);");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTypedChildContentTemplateLocalCodeBlockWithForeachContinue_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        foreach (var tag in Tags!)
                        {
                            if (tag == SkipTag)
                            {
                                continue;
                            }

                            <p>@decorated @tag</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.ForeachContinue.Sfc.Tests",
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
                    public IReadOnlyList<string>? Tags { get; set; }

                    [Parameter]
                    public string? SkipTag { get; set; }
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.SfcText, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.SfcText, "for (let tag of props.tags)");
        StringAssert.Contains(artifact.SfcText, "continue;");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeContext0.append(decorated);");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeContext0.append(tag);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateLocalCodeBlockWithForeachBreak_ProducesLocalThenImperativeLoopBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        foreach (var tag in Tags!)
                        {
                            if (tag == StopTag)
                            {
                                break;
                            }

                            <p>@decorated @tag</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.ForeachBreak.Tests",
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
                    public IReadOnlyList<string>? Tags { get; set; }

                    [Parameter]
                    public string? StopTag { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(2, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(itemTemplateSlot.Children.Children[0]);
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children[1]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "decorated");
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "tag");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateLocalCodeBlockWithForeachBreak()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        foreach (var tag in Tags!)
                        {
                            if (tag == StopTag)
                            {
                                break;
                            }

                            <p>@decorated @tag</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.ForeachBreak.Pipeline.Tests",
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
                    public IReadOnlyList<string>? Tags { get; set; }

                    [Parameter]
                    public string? StopTag { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.ModuleCode, "for (let tag of props.tags)");
        StringAssert.Contains(artifact.ModuleCode, "if (tag === props.stopTag) {");
        StringAssert.Contains(artifact.ModuleCode, "break;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(decorated);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(tag);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateLocalCodeBlockWithForBreak_ProducesLocalThenImperativeLoopBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        for (var index = 0; index < Count; index++)
                        {
                            if (index >= StopIndex)
                            {
                                break;
                            }

                            <p>@decorated @index</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.ForBreak.Tests",
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

                    [Parameter]
                    public int StopIndex { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(2, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(itemTemplateSlot.Children.Children[0]);
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children[1]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "decorated");
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "index");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateLocalCodeBlockWithForBreak()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        for (var index = 0; index < Count; index++)
                        {
                            if (index >= StopIndex)
                            {
                                break;
                            }

                            <p>@decorated @index</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.ForBreak.Pipeline.Tests",
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

                    [Parameter]
                    public int StopIndex { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.ModuleCode, "for (let index = 0; index < props.count; index++)");
        StringAssert.Contains(artifact.ModuleCode, "if (index >= props.stopIndex) {");
        StringAssert.Contains(artifact.ModuleCode, "break;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(decorated);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(index);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateLocalCodeBlockWithForContinue_ProducesLocalThenImperativeLoopBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        for (var index = 0; index < Count; index++)
                        {
                            if (index == SkipIndex)
                            {
                                continue;
                            }

                            <p>@decorated @index</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.ForContinue.Tests",
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

                    [Parameter]
                    public int SkipIndex { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(2, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(itemTemplateSlot.Children.Children[0]);
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children[1]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "decorated");
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "index");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateLocalCodeBlockWithForContinue()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        for (var index = 0; index < Count; index++)
                        {
                            if (index == SkipIndex)
                            {
                                continue;
                            }

                            <p>@decorated @index</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.ForContinue.Pipeline.Tests",
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

                    [Parameter]
                    public int SkipIndex { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.ModuleCode, "for (let index = 0; index < props.count; index++)");
        StringAssert.Contains(artifact.ModuleCode, "if (index === props.skipIndex) {");
        StringAssert.Contains(artifact.ModuleCode, "continue;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(decorated);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(index);");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersTypedChildContentTemplateLocalCodeBlockWithForContinue_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        for (var index = 0; index < Count; index++)
                        {
                            if (index == SkipIndex)
                            {
                                continue;
                            }

                            <p>@decorated @index</p>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.ForContinue.Sfc.Tests",
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

                    [Parameter]
                    public int SkipIndex { get; set; }
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.SfcText, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.SfcText, "for (let index = 0; index < props.count; index++)");
        StringAssert.Contains(artifact.SfcText, "continue;");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeContext0.append(decorated);");
        StringAssert.Contains(artifact.SfcText, "__jazorImperativeContext0.append(index);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateLocalCodeBlockWithSwitch_ProducesLocalThenImperativeSwitchBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        switch (Mode)
                        {
                            case 0:
                                <p>@decorated</p>
                                break;
                            default:
                                <section>@item</section>
                                break;
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.Switch.Tests",
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
                    public int Mode { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(2, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(itemTemplateSlot.Children.Children[0]);
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children[1]);
        Assert.AreEqual(RazorVueImperativeBlockKind.SwitchBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "decorated");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateLocalCodeBlockWithSwitch()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        switch (Mode)
                        {
                            case 0:
                                <p>@decorated</p>
                                break;
                            default:
                                <section>@item</section>
                                break;
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.Switch.Pipeline.Tests",
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
                    public int Mode { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.ModuleCode, "switch (props.mode)");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(decorated);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateLocalCodeBlockWithTryCatchFinally_ProducesLocalThenImperativeTryBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        try
                        {
                            <section>@decorated</section>
                        }
                        catch
                        {
                            <p>fallback</p>
                        }
                        finally
                        {
                            _count++;
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.TryCatchFinally.Tests",
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
                    private int _count;
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(2, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(itemTemplateSlot.Children.Children[0]);
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children[1]);
        Assert.AreEqual(RazorVueImperativeBlockKind.TryBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "decorated");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateLocalCodeBlockWithTryCatchFinally()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        try
                        {
                            <section>@decorated</section>
                        }
                        catch
                        {
                            <p>fallback</p>
                        }
                        finally
                        {
                            _count++;
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.TryCatchFinally.Pipeline.Tests",
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
                    private int _count;
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.ModuleCode, "try {");
        StringAssert.Contains(artifact.ModuleCode, "} catch {");
        StringAssert.Contains(artifact.ModuleCode, "} finally {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(decorated);");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateLocalCodeBlockWithUsingDeclaration_ProducesLocalThenImperativeTryBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        using var disposable = CreateDisposable();
                        <section>@decorated</section>
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.UsingDeclaration.Tests",
            documentPath,
            documentText,
            """
            using System;

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
                    private sealed class TestDisposable : IDisposable
                    {
                        public void Dispose() { }
                    }

                    private TestDisposable CreateDisposable() => new();
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(2, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(itemTemplateSlot.Children.Children[0]);
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children[1]);
        Assert.AreEqual(RazorVueImperativeBlockKind.TryBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "decorated");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateLocalCodeBlockWithUsingDeclaration()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        using var disposable = CreateDisposable();
                        <section>@decorated</section>
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.UsingDeclaration.Pipeline.Tests",
            documentPath,
            documentText,
            """
            using System;

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
                    private sealed class TestDisposable : IDisposable
                    {
                        public void Dispose() { }
                    }

                    private TestDisposable CreateDisposable() => new();
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.ModuleCode, "let disposable = createDisposable();");
        StringAssert.Contains(artifact.ModuleCode, "try {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(decorated);");
        StringAssert.Contains(artifact.ModuleCode, "disposable.dispose();");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateLocalCodeBlockWithUsingStatement_ProducesLocalThenImperativeTryBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        using (CreateDisposable())
                        {
                            <section>@decorated</section>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.UsingStatement.Tests",
            documentPath,
            documentText,
            """
            using System;

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
                    private sealed class TestDisposable : IDisposable
                    {
                        public void Dispose() { }
                    }

                    private IDisposable CreateDisposable() => new TestDisposable();
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(2, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(itemTemplateSlot.Children.Children[0]);
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children[1]);
        Assert.AreEqual(RazorVueImperativeBlockKind.TryBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "decorated");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateLocalCodeBlockWithUsingStatement()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        using (CreateDisposable())
                        {
                            <section>@decorated</section>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.UsingStatement.Pipeline.Tests",
            documentPath,
            documentText,
            """
            using System;

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
                    private sealed class TestDisposable : IDisposable
                    {
                        public void Dispose() { }
                    }

                    private IDisposable CreateDisposable() => new TestDisposable();
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.ModuleCode, "try {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorImperativeContext0.append(decorated);");
        StringAssert.Contains(artifact.ModuleCode, "createDisposable();");
    }

    [TestMethod]
    public void CreateRenderTree_ForTypedChildContentTemplateLocalCodeBlockWithLock_ProducesLocalThenImperativeLockBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        lock (_gate)
                        {
                            <section>@decorated</section>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.Lock.Tests",
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
                    private readonly object _gate = new();
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var itemTemplateSlot = component.SlotTemplates.Single(static slot => slot.PublicName == "ItemTemplate");
        Assert.AreEqual(2, itemTemplateSlot.Children.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(itemTemplateSlot.Children.Children[0]);
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(itemTemplateSlot.Children.Children[1]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LockBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "decorated");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerTypedChildContentTemplateLocalCodeBlockWithLock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <LayoutCard>
                <ItemTemplate Context="item">
                    @{
                        var decorated = item + "!";
                        lock (_gate)
                        {
                            <section>@decorated</section>
                        }
                    }
                </ItemTemplate>
            </LayoutCard>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.TypedChildContent.TemplateLocalCodeBlock.Lock.Pipeline.Tests",
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
                    private readonly object _gate = new();
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => (() => {");
        StringAssert.Contains(artifact.ModuleCode, "const decorated = (item + \"!\");");
        StringAssert.Contains(artifact.ModuleCode, "if (_gate == null)");
        StringAssert.Contains(artifact.ModuleCode, "throw new TypeError(\"obj\");");
        StringAssert.Contains(artifact.ModuleCode, "try {");
    }

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
        StringAssert.Contains(artifact.ModuleCode, "!item.isDone");
        StringAssert.Contains(artifact.ModuleCode, "h(\"li\", null, item.title)");
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
    public void CreateRenderTree_ForLoop_WithSimpleAssignmentAddStep_CreatesCountStyleForNodeWithStepValue()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @for (var i = Start; i <= Count; i = i + Step)
            {
                <p>@i</p>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ForLoop.SimpleAssignmentAdd.Tests",
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
    public void CreateRenderTree_ForLoop_WithStaticLocalStepCarrier_CreatesCountStyleForNodeWithLocalStepValue()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                const int step = 2;
            }
            @for (var i = 0; i < Count; i += step)
            {
                <p>@i</p>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ForLoop.StaticLocalStep.Tests",
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
        Assert.HasCount(2, renderTree.Children);
        Assert.IsInstanceOfType<RazorVueLocalDeclarationNode>(renderTree.Children[0]);
        var loop = Assert.IsInstanceOfType<RazorVueForNode>(renderTree.Children[1]);
        Assert.AreEqual("i", loop.VariableName);
        Assert.AreEqual(RazorVueForConditionKind.LessThan, loop.ConditionKind);
        Assert.AreEqual(RazorVueForStepKind.AddAssign, loop.StepKind);
        Assert.AreEqual("0", loop.InitialValue.Syntax.ToString());
        Assert.AreEqual("Count", loop.LimitValue.Syntax.ToString());
        Assert.IsNotNull(loop.StepValue);
        Assert.AreEqual("step", loop.StepValue.Syntax.ToString());
    }

    [TestMethod]
    public void CreateRenderTree_ForLoop_WithDynamicAddAssignStepExpression_CreatesImperativeLoopBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @for (var i = Start; i <= Count; i += GetStep())
            {
                <p>@i</p>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ForLoop.DynamicAddAssignStep.Tests",
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

                    private int GetStep()
                        => 1;
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);
        var loop = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(renderTree.Children[0]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, loop.Kind);
        Assert.HasCount(1, loop.Operations);
        StringAssert.Contains(loop.Operations[0].Syntax.ToString(), "GetStep()");
    }

    [TestMethod]
    public void CreateRenderTree_ForLoop_WithDynamicSimpleAssignmentAddStep_CreatesImperativeLoopBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @for (var i = Start; i <= Count; i = i + GetStep())
            {
                <p>@i</p>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ForLoop.DynamicSimpleAssignmentStep.Tests",
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

                    private int GetStep()
                        => 1;
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);
        var loop = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(renderTree.Children[0]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, loop.Kind);
        Assert.HasCount(1, loop.Operations);
        StringAssert.Contains(loop.Operations[0].Syntax.ToString(), "GetStep()");
    }

    [TestMethod]
    public void CreateRenderTree_ForLoop_WithCommutativeSimpleAssignmentAddStep_CreatesCountStyleForNodeWithStepValue()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @for (var i = Start; i <= Count; i = Step + i)
            {
                <p>@i</p>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ForLoop.SimpleAssignmentCommutativeAdd.Tests",
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
        var loop = Assert.IsInstanceOfType<RazorVueForNode>(renderTree.Children[0]);
        Assert.AreEqual("i", loop.VariableName);
        Assert.AreEqual(RazorVueForConditionKind.LessThanOrEqual, loop.ConditionKind);
        Assert.AreEqual(RazorVueForStepKind.AddAssign, loop.StepKind);
        Assert.AreEqual("Start", loop.InitialValue.Syntax.ToString());
        Assert.AreEqual("Count", loop.LimitValue.Syntax.ToString());
        Assert.IsNotNull(loop.StepValue);
        Assert.AreEqual("Step", loop.StepValue.Syntax.ToString());
    }

    [TestMethod]
    public void CreateRenderTree_ForLoop_WithSimpleAssignmentSubtractStep_CreatesCountStyleForNodeWithStepValue()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @for (var i = Start; i >= Count; i = i - Step)
            {
                <p>@i</p>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.ForLoop.SimpleAssignmentSubtract.Tests",
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
        Assert.AreEqual(RazorVueForConditionKind.GreaterThanOrEqual, loop.ConditionKind);
        Assert.AreEqual(RazorVueForStepKind.SubtractAssign, loop.StepKind);
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
    public void CreateRenderTree_ForLoop_WithMultipleIteratorExpressions_ProducesImperativeLoopBlock()
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
        var renderTree = frontend.CreateRenderTree(context, snapshot);
        Assert.AreEqual(1, renderTree.Children.Length);
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(renderTree.Children[0]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static local => local.Name).ToArray(), "i");
    }

    [TestMethod]
    public void CreateRenderTree_ForRootTemplateCodeBlockWithConditionalReturn_ProducesImperativeMethodBodyNode()
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
            "RazorVue.RazorIr.TemplateFrontend.Imperative.ConditionalReturn.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, imperative.Kind);
        Assert.AreEqual(
            "Hide",
            imperative.Operations
                .SelectMany(static operation => operation.DescendantsAndSelf())
                .OfType<IPropertyReferenceOperation>()
                .First()
                .Property.Name);
    }

    [TestMethod]
    public void CreateRenderTree_ForDirectIfWithReturn_ProducesImperativeMethodBodyNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @if (Hide)
            {
                return;
            }

            <section>ready</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.DirectIfReturn.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(renderTree.Children[0]);
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, imperative.Kind);
    }

    [TestMethod]
    public void CreateRenderTree_ForRootTemplateCodeBlockWithWhileLoop_ProducesImperativeLoopBlockNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var index = 0;
                while (index < Count)
                {
                    <section>@index</section>
                    index++;
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.WhileLoop.Tests",
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

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static local => local.Name).ToArray(), "index");
    }

    [TestMethod]
    public void CreateRenderTree_ForRootTemplateCodeBlockWithDoWhileLoop_ProducesImperativeLoopBlockNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var index = 0;
                do
                {
                    <section>@index</section>
                    index++;
                }
                while (index < Count);
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.DoWhileLoop.Tests",
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

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static local => local.Name).ToArray(), "index");
    }

    [TestMethod]
    public void CreateRenderTree_ForTemplateWithDeclarativeSiblingsAroundWhileLoop_PromotesOnlyLoopBlock()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <header>start</header>

            @{
                var index = 0;
                while (index < Count)
                {
                    <section>@index</section>
                    index++;
                }
            }

            <footer>end</footer>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.LocalWhilePromotion.Tests",
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

        Assert.AreEqual(3, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children[0]);
        Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(renderTree.Children[1]);
        Assert.IsInstanceOfType<RazorVueElementNode>(renderTree.Children[2]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, ((RazorVueImperativeBlockNode)renderTree.Children[1]).Kind);
    }

    [TestMethod]
    public void CreateRenderTree_ForRootTemplateCodeBlockWithForLoopAndContinue_ProducesImperativeLoopBlockNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                for (var index = 0; index < Count; index++)
                {
                    if ((index % 2) == 0)
                    {
                        continue;
                    }

                    <section>@index</section>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.ForContinue.Tests",
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

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static local => local.Name).ToArray(), "index");
    }

    [TestMethod]
    public void CreateRenderTree_ForRootTemplateCodeBlockWithForEachLoopAndBreak_ProducesImperativeLoopBlockNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                foreach (var item in Items!)
                {
                    if (item < 0)
                    {
                        break;
                    }

                    <section>@item</section>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.ForEachBreak.Tests",
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
                    public IReadOnlyList<int>? Items { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static local => local.Name).ToArray(), "item");
    }

    [TestMethod]
    public void CreateRenderTree_ForDirectForeachWithBreak_ProducesImperativeLoopBlockNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @foreach (var item in Items!)
            {
                @if (item < 0)
                {
                    break;
                }

                <section>@item</section>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.DirectForeachBreak.Tests",
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
                    public IReadOnlyList<int>? Items { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(renderTree.Children[0]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static local => local.Name).ToArray(), "item");
    }

    [TestMethod]
    public void CreateRenderTree_ForRootTemplateCodeBlockWithSwitchStatement_ProducesImperativeSwitchBlockNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                switch (Count)
                {
                    case 0:
                        <p>empty</p>
                        break;
                    default:
                        <section>@Count</section>
                        break;
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.Switch.Tests",
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

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.SwitchBlock, imperative.Kind);
    }

    [TestMethod]
    public void CreateRenderTree_ForRootTemplateCodeBlockWithTryCatchFinally_ProducesImperativeTryBlockNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                try
                {
                    <section>ready</section>
                }
                catch
                {
                    <p>fallback</p>
                }
                finally
                {
                    _count++;
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.TryCatchFinally.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private int _count;
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.TryBlock, imperative.Kind);
    }

    [TestMethod]
    public void CreateRenderTree_ForRootTemplateCodeBlockWithUsingStatement_ProducesImperativeTryBlockNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                using (CreateDisposable())
                {
                    <section>ready</section>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.UsingStatement.Tests",
            documentPath,
            documentText,
            """
            using System;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private sealed class TestDisposable : IDisposable
                    {
                        public void Dispose() { }
                    }

                    private IDisposable CreateDisposable() => new TestDisposable();
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.TryBlock, imperative.Kind);
    }

    [TestMethod]
    public void CreateRenderTree_ForRootTemplateCodeBlockWithLockStatement_ProducesImperativeLockBlockNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                lock (_gate)
                {
                    <section>ready</section>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.Lock.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private readonly object _gate = new();
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.LockBlock, imperative.Kind);
    }

    [TestMethod]
    public void CreateRenderTree_ForRootTemplateCodeBlockWithStandaloneFieldMutation_ProducesImperativeLocalBlockNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                _count++;
            }

            <section>@_count</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.FieldMutation.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private int _count = 1;
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(2, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.LocalBlock, imperative.Kind);
        var section = renderTree.Children[1] as RazorVueElementNode;
        Assert.IsNotNull(section, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        Assert.AreEqual("section", section.TagName);
    }

    [TestMethod]
    public void CreateRenderTree_ForRootTemplateCodeBlockWithTemplateLocalThenFieldMutation_ProducesSingleImperativeLocalBlockThatKeepsLocalVisible()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var localTitle = Title;
                _count++;
            }

            <section>@localTitle @_count</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RootTemplateCodeBlock.TemplateLocal.FieldMutation.Tests",
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

                    private int _count = 1;
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(renderTree.Children[0]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LocalBlock, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "localTitle");
    }

    [TestMethod]
    public void CreateRenderTree_ForRootTemplateCodeBlockWithTemplateLocalThenConditionalReturn_ProducesSingleImperativeMethodBodyBlockThatKeepsLocalVisible()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var localTitle = Title;
                if (Hide)
                {
                    return;
                }
            }

            <section>@localTitle</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RootTemplateCodeBlock.TemplateLocal.ConditionalReturn.Tests",
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
                    public bool Hide { get; set; }
                }
            }
            """);

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(renderTree.Children[0]);
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, imperative.Kind);
        CollectionAssert.Contains(imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray(), "localTitle");
    }

    [TestMethod]
    public void CreateRenderTree_ForRootTemplateCodeBlockWithTemplateLocalThenWhileLoopAndTrailingSibling_ProducesSingleImperativeLoopBlockThatKeepsLocalsVisible()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var localTitle = Title;
                var index = 0;
                while (index < Count)
                {
                    <section>@localTitle @index</section>
                    index++;
                }
            }

            <footer>@localTitle @index</footer>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RootTemplateCodeBlock.TemplateLocal.WhileTail.Tests",
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

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(renderTree.Children[0]);
        Assert.AreEqual(RazorVueImperativeBlockKind.LoopBlock, imperative.Kind);
        var visibleLocals = imperative.VisibleLocals.Select(static localSymbol => localSymbol.Name).ToArray();
        CollectionAssert.Contains(visibleLocals, "localTitle");
        CollectionAssert.Contains(visibleLocals, "index");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRootTemplateCodeBlockWithTemplateLocalThenFieldMutation()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var localTitle = Title;
                _count++;
            }

            <section>@localTitle @_count</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RootTemplateCodeBlock.TemplateLocal.FieldMutation.Pipeline.Tests",
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

                    private int _count = 1;
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "let localTitle = props.title;");
        StringAssert.Contains(artifact.ModuleCode, "_count++;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(localTitle);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(\" \");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(_count);");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRootTemplateCodeBlockWithTemplateLocalThenConditionalReturn()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var localTitle = Title;
                if (Hide)
                {
                    return;
                }
            }

            <section>@localTitle</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RootTemplateCodeBlock.TemplateLocal.ConditionalReturn.Pipeline.Tests",
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
                    public bool Hide { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "let localTitle = props.title;");
        StringAssert.Contains(artifact.ModuleCode, "if (props.hide) {");
        StringAssert.Contains(artifact.ModuleCode, "return __jazorRenderContext.finish();");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(localTitle);");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_CanLowerRootTemplateCodeBlockWithTemplateLocalThenWhileLoopAndTrailingSibling()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var localTitle = Title;
                var index = 0;
                while (index < Count)
                {
                    <section>@localTitle @index</section>
                    index++;
                }
            }

            <footer>@localTitle @index</footer>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RootTemplateCodeBlock.TemplateLocal.WhileTail.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "let localTitle = props.title;");
        StringAssert.Contains(artifact.ModuleCode, "let index = 0;");
        StringAssert.Contains(artifact.ModuleCode, "while (index < props.count)");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"footer\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(localTitle);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(index);");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersRootTemplateCodeBlockWithTemplateLocalThenFieldMutation_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var localTitle = Title;
                _count++;
            }

            <section>@localTitle @_count</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RootTemplateCodeBlock.TemplateLocal.FieldMutation.Sfc.Tests",
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

                    private int _count = 1;
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "let localTitle = props.title;");
        StringAssert.Contains(artifact.SfcText, "_count++;");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(localTitle);");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(_count);");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersRootTemplateCodeBlockWithTemplateLocalThenConditionalReturn_ToTemplateSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var localTitle = Title;
                if (Hide)
                {
                    return;
                }
            }

            <section>@localTitle</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RootTemplateCodeBlock.TemplateLocal.ConditionalReturn.Sfc.Tests",
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
                    public bool Hide { get; set; }
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<template v-for=\"(localTitle) in [props.title]\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"props.hide\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-else>");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "{{ localTitle }}");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersRootTemplateCodeBlockWithTemplateLocalThenWhileLoopAndTrailingSibling_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var localTitle = Title;
                var index = 0;
                while (index < Count)
                {
                    <section>@localTitle @index</section>
                    index++;
                }
            }

            <footer>@localTitle @index</footer>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.RootTemplateCodeBlock.TemplateLocal.WhileTail.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "let localTitle = props.title;");
        StringAssert.Contains(artifact.SfcText, "let index = 0;");
        StringAssert.Contains(artifact.SfcText, "while (index < props.count)");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"footer\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(localTitle);");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(index);");
    }

    [TestMethod]
    public void CreateRenderTree_ForRootTemplateCodeBlockWithThrowStatement_ProducesImperativeMethodBodyNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                if (Fail)
                {
                    throw new InvalidOperationException("boom");
                }
            }

            <section>ready</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.Throw.Tests",
            documentPath,
            documentText,
            """
            using System;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Fail { get; set; }
                }
            }
            """,
            importsText: "@using System");

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, imperative.Kind);
    }

    [TestMethod]
    public void CreateRenderTree_ForDirectIfWithThrow_ProducesImperativeMethodBodyNode()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @if (Fail)
            {
                throw new InvalidOperationException("boom");
            }

            <section>ready</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.DirectIfThrow.Tests",
            documentPath,
            documentText,
            """
            using System;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Fail { get; set; }
                }
            }
            """,
            importsText: "@using System");

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = Assert.IsInstanceOfType<RazorVueImperativeBlockNode>(renderTree.Children[0]);
        Assert.AreEqual(RazorVueImperativeBlockKind.MethodBody, imperative.Kind);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersConditionalReturnTemplateCodeBlock_UsingImperativeRenderBridge()
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
            "RazorVue.RazorIr.TemplateFrontend.Imperative.ConditionalReturn.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const __jazorRenderContext = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.ModuleCode, "if (props.hide) {");
        StringAssert.Contains(artifact.ModuleCode, "return __jazorRenderContext.finish();");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(\"section\", null, \"ready\"));");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersDirectIfWithReturn_UsingImperativeRenderBridge()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @if (Hide)
            {
                return;
            }

            <section>ready</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.DirectIfReturn.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const __jazorRenderContext = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.ModuleCode, "if (props.hide) {");
        StringAssert.Contains(artifact.ModuleCode, "return __jazorRenderContext.finish();");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(\"section\", null, \"ready\"));");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersConditionalReturnWithNestedHelperCondition_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                if (Helper.Hide)
                {
                    return;
                }
            }

            <section>ready</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.ConditionalReturn.NestedHelperCondition.Sfc.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private sealed class Helper
                    {
                        public static bool Hide => false;
                    }
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);
        var classIndex = artifact.SfcText.IndexOf("class Helper", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsTrue(classIndex >= 0, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "if (Helper.hide)");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(h(\"section\", null, \"ready\"));");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersThrowStatementTemplateCodeBlock_UsingImperativeRenderBridge()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                if (Fail)
                {
                    throw new InvalidOperationException("boom");
                }
            }

            <section>ready</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.Throw.Pipeline.Tests",
            documentPath,
            documentText,
            """
            using System;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Fail { get; set; }
                }
            }
            """,
            importsText: "@using System");

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const __jazorRenderContext = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.ModuleCode, "if (props.fail) {");
        StringAssert.Contains(artifact.ModuleCode, "throw new Error(\"boom\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(\"section\", null, \"ready\"));");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersDirectIfWithThrow_UsingImperativeRenderBridge()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @if (Fail)
            {
                throw new InvalidOperationException("boom");
            }

            <section>ready</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.DirectIfThrow.Pipeline.Tests",
            documentPath,
            documentText,
            """
            using System;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Fail { get; set; }
                }
            }
            """,
            importsText: "@using System");

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const __jazorRenderContext = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.ModuleCode, "if (props.fail) {");
        StringAssert.Contains(artifact.ModuleCode, "throw new Error(\"boom\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(\"section\", null, \"ready\"));");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersDirectIfWithReturn_ToTemplateSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @if (Hide)
            {
                return;
            }

            <section>ready</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.DirectIfReturn.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("<script lang=\"ts\">", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("__jazorCreateRenderContext", StringComparison.Ordinal), artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<template v-if=\"props.hide\">");
        StringAssert.Contains(artifact.TemplateText, "<template v-else>");
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersDirectIfWithThrow_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @if (Fail)
            {
                throw new InvalidOperationException("boom");
            }

            <section>ready</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.DirectIfThrow.Sfc.Tests",
            documentPath,
            documentText,
            """
            using System;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Fail { get; set; }
                }
            }
            """,
            importsText: "@using System");

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "if (props.fail) {");
        StringAssert.Contains(artifact.SfcText, "throw new Error(\"boom\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(h(\"section\", null, \"ready\"));");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersWhileLoopTemplateCodeBlock_UsingImperativeRenderBridge()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var index = 0;
                while (index < Count)
                {
                    <section>@index</section>
                    index++;
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.WhileLoop.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "let index = 0;");
        StringAssert.Contains(artifact.ModuleCode, "while (index < props.count)");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(index);");
        StringAssert.Contains(artifact.ModuleCode, "index++;");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersDoWhileLoopTemplateCodeBlock_UsingImperativeRenderBridge()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var index = 0;
                do
                {
                    <section>@index</section>
                    index++;
                }
                while (index < Count);
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.DoWhileLoop.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "let index = 0;");
        StringAssert.Contains(artifact.ModuleCode, "do {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(index);");
        StringAssert.Contains(artifact.ModuleCode, "index++;");
        StringAssert.Contains(artifact.ModuleCode, "while (index < props.count);");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersRootRuntimeWhileCondition_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                while (ShouldContinue())
                {
                    <section>runtime</section>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.RuntimeWhileCondition.Sfc.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private bool ShouldContinue() => false;
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "function shouldContinue()");
        StringAssert.Contains(artifact.SfcText, "while (shouldContinue())");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(h(\"section\", null, \"runtime\"));");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersRootDoWhileFalseWithContinue_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                do
                {
                    if (ShouldSkip())
                    {
                        continue;
                    }

                    <section>ready</section>
                }
                while (false);
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.DoWhileFalseContinue.Sfc.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private bool ShouldSkip() => false;
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "do {");
        StringAssert.Contains(artifact.SfcText, "if (shouldSkip()) {");
        StringAssert.Contains(artifact.SfcText, "continue;");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(h(\"section\", null, \"ready\"));");
        StringAssert.Contains(artifact.SfcText, "while (false);");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersRootDoWhileFalseWithNestedHelperType_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                do
                {
                    <section>@Helper.Text</section>
                }
                while (false);
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.DoWhileFalseNestedHelper.Sfc.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private sealed class Helper
                    {
                        public static string Text => "ready";
                    }
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);
        var classIndex = artifact.SfcText.IndexOf("class Helper", StringComparison.Ordinal);
        var exportIndex = artifact.SfcText.IndexOf("export default defineComponent", StringComparison.Ordinal);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsFalse(artifact.UsesScriptSetup, artifact.SfcText);
        Assert.IsTrue(classIndex >= 0, artifact.SfcText);
        Assert.IsTrue(exportIndex > classIndex, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "do {");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(Helper.text);");
        StringAssert.Contains(artifact.SfcText, "while (false);");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersDeclarativeSiblingsAroundWhileLoop_UsingMixedImperativeRenderBridge()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <header>start</header>

            @{
                var index = 0;
                while (index < Count)
                {
                    <section>@index</section>
                    index++;
                }
            }

            <footer>end</footer>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.LocalWhilePromotion.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "const __jazorRenderContext = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(\"header\", null, \"start\"));");
        StringAssert.Contains(artifact.ModuleCode, "let index = 0;");
        StringAssert.Contains(artifact.ModuleCode, "while (index < props.count)");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(index);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(\"footer\", null, \"end\"));");
        StringAssert.Contains(artifact.ModuleCode, "return __jazorRenderContext.finish();");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersDeclarativeSiblingsAroundWhileLoop_ToMixedRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <header>start</header>

            @{
                var index = 0;
                while (index < Count)
                {
                    <section>@index</section>
                    index++;
                }
            }

            <footer>end</footer>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.LocalWhilePromotion.Sfc.Tests",
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

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "const __jazorRenderContext = __jazorCreateRenderContext(h);");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(h(\"header\", null, \"start\"));");
        StringAssert.Contains(artifact.SfcText, "let index = 0;");
        StringAssert.Contains(artifact.SfcText, "while (index < props.count)");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(index);");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(h(\"footer\", null, \"end\"));");
        StringAssert.Contains(artifact.SfcText, "return __jazorRenderContext.finish();");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersForLoopWithContinueTemplateCodeBlock_UsingImperativeRenderBridge()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                for (var index = 0; index < Count; index++)
                {
                    if ((index % 2) == 0)
                    {
                        continue;
                    }

                    <section>@index</section>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.ForContinue.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "for (let index = 0; index < props.count; index++)");
        StringAssert.Contains(artifact.ModuleCode, "continue;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(index);");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersForEachLoopWithBreakTemplateCodeBlock_UsingImperativeRenderBridge()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                foreach (var item in Items!)
                {
                    if (item < 0)
                    {
                        break;
                    }

                    <section>@item</section>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.ForEachBreak.Pipeline.Tests",
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
                    public IReadOnlyList<int>? Items { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "for (let item of props.items)");
        StringAssert.Contains(artifact.ModuleCode, "break;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(item);");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersDirectForeachWithBreak_UsingImperativeRenderBridge()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @foreach (var item in Items!)
            {
                @if (item < 0)
                {
                    break;
                }

                <section>@item</section>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.DirectForeachBreak.Pipeline.Tests",
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
                    public IReadOnlyList<int>? Items { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "for (let item of props.items)");
        StringAssert.Contains(artifact.ModuleCode, "break;");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(item);");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersDirectForeachWithBreak_ToRenderFunctionVueSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @foreach (var item in Items!)
            {
                @if (item < 0)
                {
                    break;
                }

                <section>@item</section>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.DirectForeachBreak.Sfc.Tests",
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
                    public IReadOnlyList<int>? Items { get; set; }
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "for (let item of props.items)");
        StringAssert.Contains(artifact.SfcText, "break;");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.SfcText, "__jazorRenderContext.append(item);");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersSwitchStatementTemplateCodeBlock_UsingImperativeRenderBridge()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                switch (Count)
                {
                    case 0:
                        <p>empty</p>
                        break;
                    default:
                        <section>@Count</section>
                        break;
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.Switch.Pipeline.Tests",
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "switch (props.count)");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(\"p\", null, \"empty\"));");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(props.count);");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersTryCatchFinallyTemplateCodeBlock_UsingImperativeRenderBridge()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                try
                {
                    <section>ready</section>
                }
                catch
                {
                    <p>fallback</p>
                }
                finally
                {
                    _count++;
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.TryCatchFinally.Pipeline.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private int _count;
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "try {");
        StringAssert.Contains(artifact.ModuleCode, "} catch {");
        StringAssert.Contains(artifact.ModuleCode, "} finally {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(\"section\", null, \"ready\"));");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(\"p\", null, \"fallback\"));");
        StringAssert.Contains(artifact.ModuleCode, "_count++;");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersUsingDeclarationTemplateCodeBlock_UsingImperativeRenderBridge()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                using var disposable = CreateDisposable();
                <section>ready</section>
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.UsingDeclaration.Pipeline.Tests",
            documentPath,
            documentText,
            """
            using System;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private sealed class TestDisposable : IDisposable
                    {
                        public void Dispose() { }
                    }

                    private TestDisposable CreateDisposable() => new TestDisposable();
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "let disposable = ");
        StringAssert.Contains(artifact.ModuleCode, "try {");
        StringAssert.Contains(artifact.ModuleCode, "} finally {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(\"section\", null, \"ready\"));");
        StringAssert.Contains(artifact.ModuleCode, "if (disposable !== null)");
        StringAssert.Contains(artifact.ModuleCode, "disposable.dispose();");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersUsingStatementTemplateCodeBlock_UsingImperativeRenderBridge()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                using (CreateDisposable())
                {
                    <section>ready</section>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.UsingStatement.Pipeline.Tests",
            documentPath,
            documentText,
            """
            using System;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private sealed class TestDisposable : IDisposable
                    {
                        public void Dispose() { }
                    }

                    private IDisposable CreateDisposable() => new TestDisposable();
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "let ");
        StringAssert.Contains(artifact.ModuleCode, " = createDisposable();");
        StringAssert.Contains(artifact.ModuleCode, "try {");
        StringAssert.Contains(artifact.ModuleCode, "} finally {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.append(h(\"section\", null, \"ready\"));");
        StringAssert.Contains(artifact.ModuleCode, "!== null)");
        StringAssert.Contains(artifact.ModuleCode, "_6f97d94b6f2e4bc1(");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersReadonlyObjectGateLockStatementTemplateCodeBlock_ToTemplateSfc()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                lock (_gate)
                {
                    <section>ready</section>
                }
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Lock.Pipeline.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private readonly object _gate = new();
                }
            }
            """);

        var artifact = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        Assert.AreEqual(VueSfcArtifactRenderMode.Template, artifact.RenderMode);
        Assert.IsTrue(artifact.HasTemplateBlock, artifact.SfcText);
        Assert.IsTrue(artifact.UsesScriptSetup, artifact.SfcText);
        StringAssert.Contains(artifact.TemplateText, "<section>");
        StringAssert.Contains(artifact.TemplateText, "ready");
        Assert.IsFalse(artifact.SfcText.Contains("__jazorCreateRenderContext", StringComparison.Ordinal), artifact.SfcText);
        Assert.IsFalse(artifact.SfcText.Contains("if (_gate == null)", StringComparison.Ordinal), artifact.SfcText);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersImperativeInjectedNamedSlotForwarding_UsingRuntimeSlotMetadata()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Demo.Containers
            @{
                if (!ShowShell)
                {
                    return;
                }

                <NavShell Header="Header" />
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.TemplateFrontend.Imperative.InjectedNamedSlot.Pipeline.Tests",
            documentPath,
            documentText,
            """
            using ECMAScript.VueContract.Descriptor;

            [assembly: VueInject(
                typeof(Demo.Containers.NavShell),
                typeof(Demo.Implementations.ElementPlusNavShell))]

            namespace Demo.Contracts
            {
                public sealed record HeaderContext(string Title);
            }

            namespace Demo.Containers
            {
                [ECMAScript.ECMAScriptModule("./containers/nav-shell")]
                public sealed class NavShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                    [Parameter]
                    public RenderFragment<Demo.Contracts.HeaderContext>? Header { get; set; }
                }
            }

            namespace Demo.Implementations
            {
                [VueLibraryComponent("element-plus", "ElMenu")]
                [VueSlot(nameof(Header), Name = "top", ContextTypeName = "Demo.Contracts.HeaderContext", ContextParameterName = "headerContext")]
                public sealed class ElementPlusNavShell : ComponentBase, IVueLibraryComponent, IVueContainerImplementation<Demo.Containers.NavShell>
                {
                    [Parameter]
                    public RenderFragment<Demo.Contracts.HeaderContext>? Header { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool ShowShell { get; set; }

                    [Parameter]
                    public RenderFragment<Demo.Contracts.HeaderContext>? Header { get; set; }
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.enterComponent(NavShellComponent, __jazorImperativeComponentMetadata_NavShell);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorRenderContext.setComponentParameter(\"Header\", __jazorCreateSlotReference(slots.header ?? null, true));");
        StringAssert.Contains(artifact.ModuleCode, "\"top\"");
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
