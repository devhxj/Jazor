using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RazorSdk;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorVueRazorIrMetadataDirectiveTests
{
    private const string DocumentPath = @"D:\repo\Demo\Pages\TodoApp.razor";

    [TestMethod]
    public void RazorVuePipeline_WithAttributeDirective_ProjectsRouteAndKeepsTransparentMetadataOutOfArtifact()
    {
        const string documentText = """
            @attribute [Route("/from-attribute")]
            @attribute [Demo.Pages.ClientOnlyMarker("transparent")]

            <section>@Title</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.MetadataDirective.Attribute.Tests",
            DocumentPath,
            documentText,
            """
            using System;

            namespace Demo.Pages
            {
                [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
                public sealed class ClientOnlyMarkerAttribute : Attribute
                {
                    public ClientOnlyMarkerAttribute(string value)
                        => Value = value;

                    public string Value { get; }
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }
                }
            }
            """);

        CollectionAssert.AreEqual(
            new[] { "/from-attribute" },
            snapshot.Descriptor.RouteTemplates.ToArray());

        Assert.IsTrue(
            snapshot.ComponentSymbol.GetAttributes().Any(static attribute =>
                string.Equals(
                    attribute.AttributeClass?.ToDisplayString(),
                    "Demo.Pages.ClientOnlyMarkerAttribute",
                    StringComparison.Ordinal)),
            "The transparent C# attribute must remain visible to Roslyn metadata.");

        var artifact = RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
            .Execute(context)
            .Artifacts
            .Single();

        CollectionAssert.AreEqual(new[] { "/from-attribute" }, artifact.RouteTemplates.ToArray());
        StringAssert.Contains(artifact.ModuleCode, "props.title");
        Assert.IsFalse(artifact.ModuleCode.Contains("ClientOnlyMarker", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("@attribute", StringComparison.Ordinal), artifact.ModuleCode);
    }

    [TestMethod]
    public void RazorVuePipeline_WithInheritsDirective_UsesSourceAnalyzableBaseParametersAndShouldRender()
    {
        const string documentText = """
            @inherits TodoAppBase

            <section>@Title</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.MetadataDirective.Inherits.Tests",
            DocumentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                public abstract class TodoAppBase : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    protected override bool ShouldRender()
                        => Title is not null;
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : TodoAppBase
                {
                }
            }
            """);

        Assert.AreEqual("TodoAppBase", snapshot.ComponentSymbol.BaseType?.Name);
        Assert.AreEqual("Demo.Pages.TodoAppBase", snapshot.ShouldRenderMethod?.ContainingType.ToDisplayString());

        var prop = snapshot.Descriptor.Props.Single();
        Assert.AreEqual("Title", prop.PublicName);
        Assert.AreEqual("title", prop.Name);

        var artifact = RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
            .Execute(context)
            .Artifacts
            .Single();

        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
        StringAssert.Contains(artifact.ModuleCode, "props.title");
        StringAssert.Contains(artifact.ModuleCode, "__jazorShouldRenderHasRendered");
        StringAssert.Contains(artifact.ModuleCode, "__jazorShouldRenderCachedVNode");
        StringAssert.Contains(artifact.ModuleCode, "return __jazorShouldRenderCachedVNode;");
    }

    [TestMethod]
    public void RazorVuePipeline_WithImplementsDirective_TreatsInterfaceAsCompileTimeContractOnly()
    {
        const string documentText = """
            @implements Demo.Pages.ITodoComponentContract

            <section>ready</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.MetadataDirective.Implements.Tests",
            DocumentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                public interface ITodoComponentContract
                {
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                }
            }
            """);

        Assert.IsTrue(
            snapshot.ComponentSymbol.AllInterfaces.Any(static item =>
                string.Equals(item.ToDisplayString(), "Demo.Pages.ITodoComponentContract", StringComparison.Ordinal)),
            "@implements must remain a Roslyn-visible compile-time contract.");

        var artifact = RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "ready");
        Assert.IsFalse(artifact.ModuleCode.Contains("ITodoComponentContract", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("@implements", StringComparison.Ordinal), artifact.ModuleCode);
    }
}
