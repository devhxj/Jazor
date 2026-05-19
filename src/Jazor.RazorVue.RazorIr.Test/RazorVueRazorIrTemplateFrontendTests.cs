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

        var artifact = RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "const localTitle = props.title;");
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
    public void RazorVueSfcArtifactFactory_WithRazorIrTemplateFrontend_LowersSwitchStatementCodeBlock_ToRenderFunctionVueSfc()
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

        Assert.AreEqual(VueSfcArtifactRenderMode.RenderFunction, artifact.RenderMode);
        Assert.IsFalse(artifact.HasTemplateBlock, artifact.SfcText);
        StringAssert.Contains(artifact.SfcText, "<script lang=\"ts\">");
        StringAssert.Contains(artifact.SfcText, "switch (props.count)");
        StringAssert.Contains(artifact.SfcText, "__jazorBuilder.AddContent(h(\"p\", null, \"empty\"));");
        StringAssert.Contains(artifact.SfcText, "__jazorBuilder.AddContent(h(\"section\", null, props.count));");
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
        StringAssert.Contains(artifact.SfcText, "__jazorBuilder.AddContent(h(\"section\", null, \"ready\"));");
        StringAssert.Contains(artifact.SfcText, "__jazorBuilder.AddContent(h(\"p\", null, \"fallback\"));");
        StringAssert.Contains(artifact.SfcText, "_count++;");
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
        StringAssert.Contains(artifact.SfcText, "__jazorBuilder.AddContent(h(\"section\", null, \"ready\"));");
        StringAssert.Contains(artifact.SfcText, "if (disposable !== null)");
        StringAssert.Contains(artifact.SfcText, "disposable.dispose();");
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

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
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
        Assert.AreEqual("Hide", imperative.Operation.Descendants().OfType<IPropertyReferenceOperation>().First().Property.Name);
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

        Assert.AreEqual(1, renderTree.Children.Length, RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot));
        var imperative = renderTree.Children[0] as RazorVueImperativeBlockNode;
        Assert.IsNotNull(imperative);
        Assert.AreEqual(RazorVueImperativeBlockKind.LocalBlock, imperative.Kind);
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

        StringAssert.Contains(artifact.ModuleCode, "const __jazorBuilder = __jazorCreateRenderTreeBuilder(h);");
        StringAssert.Contains(artifact.ModuleCode, "if (props.hide) {");
        StringAssert.Contains(artifact.ModuleCode, "return __jazorBuilder.complete();");
        StringAssert.Contains(artifact.ModuleCode, "__jazorBuilder.AddContent(h(\"section\", null, \"ready\"));");
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
        StringAssert.Contains(artifact.ModuleCode, "__jazorBuilder.OpenElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorBuilder.AddContent(index);");
        StringAssert.Contains(artifact.ModuleCode, "index++;");
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
        StringAssert.Contains(artifact.ModuleCode, "__jazorBuilder.AddContent(h(\"p\", null, \"empty\"));");
        StringAssert.Contains(artifact.ModuleCode, "__jazorBuilder.OpenElement(\"section\");");
        StringAssert.Contains(artifact.ModuleCode, "__jazorBuilder.AddContent(props.count);");
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
        StringAssert.Contains(artifact.ModuleCode, "__jazorBuilder.AddContent(h(\"section\", null, \"ready\"));");
        StringAssert.Contains(artifact.ModuleCode, "__jazorBuilder.AddContent(h(\"p\", null, \"fallback\"));");
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
        StringAssert.Contains(artifact.ModuleCode, "__jazorBuilder.AddContent(h(\"section\", null, \"ready\"));");
        StringAssert.Contains(artifact.ModuleCode, "if (disposable !== null)");
        StringAssert.Contains(artifact.ModuleCode, "disposable.dispose();");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersLockStatementTemplateCodeBlock_UsingImperativeRenderBridge()
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

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "if (_gate == null)");
        StringAssert.Contains(artifact.ModuleCode, "throw new TypeError(\"obj\");");
        StringAssert.Contains(artifact.ModuleCode, "try {");
        StringAssert.Contains(artifact.ModuleCode, "__jazorBuilder.AddContent(h(\"section\", null, \"ready\"));");
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

        StringAssert.Contains(artifact.ModuleCode, "__jazorBuilder.OpenComponent(NavShellComponent, __jazorImperativeComponentMetadata_NavShell);");
        StringAssert.Contains(artifact.ModuleCode, "__jazorBuilder.AddComponentParameter(\"Header\", __jazorCreateSlotReference(slots.header ?? null, true));");
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
