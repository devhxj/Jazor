using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RazorSdk;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorVueRazorIrGenericComponentDirectiveTests
{
    private const string DocumentPath = @"D:\repo\Demo\Pages\TodoApp.razor";

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForTypeParameterComponent_LowersGenericParameterAsCompileTimeAnnotation()
    {
        const string documentText = """
            @typeparam TValue

            <section>@Value</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.GenericComponent.TypeParameterAnnotation.Tests",
            DocumentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp<TValue> : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public TValue? Value { get; set; }
                }
            }
            """);

        Assert.AreEqual("TodoApp", snapshot.Descriptor.Name);
        Assert.AreEqual("Demo.Pages.TodoApp<TValue>", snapshot.Descriptor.FullName);
        Assert.AreEqual("TValue?", snapshot.Descriptor.Props.Single().TypeName);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "props.value");
        Assert.IsFalse(artifact.ModuleCode.Contains("typeof(TValue)", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("default(TValue)", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("new TValue", StringComparison.Ordinal), artifact.ModuleCode);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForGenericChildComponentTypedSlot_LowersOpenGenericDescriptorAndTypedSlotContext()
    {
        const string documentText = """
            <GenericList Items="Items">
                <ItemTemplate Context="item">
                    <span>@item.Title</span>
                </ItemTemplate>
            </GenericList>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.GenericComponent.TypedSlot.Tests",
            DocumentPath,
            documentText,
            CreateGenericChildComponentSource());

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);
        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());

        Assert.AreEqual("GenericList", component.ComponentName);
        Assert.AreEqual("Demo.Pages.GenericList<TItem>", component.ComponentFullName);
        Assert.AreEqual("GenericList", component.ResolutionName);
        Assert.AreEqual(1, component.Attributes.Length);
        Assert.AreEqual("Items", Assert.IsInstanceOfType<RazorVueAttributeNode>(component.Attributes[0]).Name);

        var slot = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slot.PublicName);
        Assert.AreEqual("itemTemplate", slot.SlotName);
        Assert.AreEqual("item", slot.ParameterName);
        Assert.AreEqual("item", slot.ParameterSymbol?.Name);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(slot.Children.Children.Single());
        var titleExpression = Assert.IsInstanceOfType<RazorVueExpressionNode>(span.Children.Children.Single());
        var property = Assert.IsInstanceOfType<IPropertyReferenceOperation>(titleExpression.Expression);
        Assert.AreEqual("Title", property.Property.Name);
        Assert.AreEqual("Demo.Pages.TodoItem", property.Property.ContainingType.ToDisplayString());

        var artifact = RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "import GenericListComponent from \"./components/generic-list.mjs\";");
        StringAssert.Contains(artifact.ModuleCode, "\"items\": props.items");
        StringAssert.Contains(artifact.ModuleCode, "itemTemplate: (item) => h(\"span\", null, item.title)");
        Assert.IsFalse(artifact.ModuleCode.Contains("TItem", StringComparison.Ordinal), artifact.ModuleCode);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForGenericChildDescriptor_UsesOpenGenericParameterAndSlotShapes()
    {
        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.GenericComponent.Descriptor.Tests",
            DocumentPath,
            """
            <GenericList Items="Items">
                <ItemTemplate Context="item">
                    <span>@item.Title</span>
                </ItemTemplate>
            </GenericList>
            """,
            CreateGenericChildComponentSource());

        var renderTree = new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);
        var resolvedComponents = RazorVueArtifactFactory.ResolveComponentsForCanonicalization(context, snapshot, renderTree);
        var descriptor = resolvedComponents["GenericList"];

        Assert.AreEqual("GenericList", descriptor.Name);
        Assert.AreEqual("Demo.Pages.GenericList<TItem>", descriptor.FullName);
        Assert.AreEqual("./components/generic-list.mjs", descriptor.ImportSpecifier);
        Assert.AreEqual("System.Collections.Generic.IReadOnlyList<TItem>?", descriptor.Props.Single().TypeName);

        var slotParameter = descriptor.Slots.Single().Parameters.Single();
        Assert.AreEqual("context", slotParameter.Name);
        Assert.AreEqual("TItem", slotParameter.TypeName);
    }

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_ForRuntimeTypeParameterSemantics_ReportsUnsupportedBoundary()
    {
        const string documentText = """
            @typeparam TValue

            <section>@typeof(TValue).Name</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.GenericComponent.RuntimeTypeParameter.Tests",
            DocumentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp<TValue> : ComponentBase, IVueComponent
                {
                }
            }
            """);

        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "runtime generic type-parameter semantics");
        StringAssert.Contains(exception.Issue.Message, "TValue");
        StringAssert.Contains(exception.Issue.Message, "TodoApp");
    }

    private static string CreateGenericChildComponentSource()
        => """
        using System.Collections.Generic;

        namespace Demo.Pages
        {
            public sealed record TodoItem(string Title);

            [ECMAScript.ECMAScriptModule("./components/generic-list")]
            public partial class GenericList<TItem> : ComponentBase, IVueComponent
            {
                [Parameter]
                public IReadOnlyList<TItem>? Items { get; set; }

                [Parameter]
                public RenderFragment<TItem>? ItemTemplate { get; set; }
            }

            [ECMAScript.ECMAScriptModule("./components/todo-app")]
            public partial class TodoApp : ComponentBase, IVueComponent
            {
                [Parameter]
                public IReadOnlyList<TodoItem>? Items { get; set; }
            }
        }
        """;
}
