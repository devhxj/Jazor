using System.IO;
using Jazor.RazorVue.Artifacts;
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
}
