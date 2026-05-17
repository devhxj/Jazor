namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorDocumentNodeInventoryTests
{
    public TestContext TestContext { get; set; } = default!;

    [TestMethod]
    public void ProcessDesignTime_ForMarkupAndInterpolation_ProducesInspectableTree()
    {
        var codeDocument = RazorIrTestHost.CreateCodeDocument(
            @"D:\temp\InventoryMarkup.jazor",
            """
            <section>
                <h1>@message</h1>
                <p>Hello</p>
            </section>

            @code {
                private string message = "hello";
            }
            """);

        var documentNode = RazorIrTestHost.GetDocumentNode(codeDocument);
        var tree = RazorIrTestHost.DumpIntermediateNodeTree(documentNode);

        TestContext.WriteLine(tree);

        StringAssert.Contains(tree, "DocumentIntermediateNode");
        StringAssert.Contains(tree, "MarkupElementIntermediateNode TagName=\"section\"");
        StringAssert.Contains(tree, "MarkupElementIntermediateNode TagName=\"h1\"");
        StringAssert.Contains(tree, "MarkupElementIntermediateNode TagName=\"p\"");
        StringAssert.Contains(tree, "CSharpExpressionIntermediateNode");
    }

    [TestMethod]
    public void ProcessDesignTime_ForIfAndForeach_ProducesMarkupAndCodeNodesInOneTree()
    {
        var codeDocument = RazorIrTestHost.CreateCodeDocument(
            @"D:\temp\InventoryControlFlow.jazor",
            """
            @if (items.Count > 0)
            {
                <ul>
                @foreach (var item in items)
                {
                    <li>@item</li>
                }
                </ul>
            }

            @code {
                private List<string> items = new() { "a", "b" };
            }
            """);

        var documentNode = RazorIrTestHost.GetDocumentNode(codeDocument);
        var tree = RazorIrTestHost.DumpIntermediateNodeTree(documentNode);

        TestContext.WriteLine(tree);

        StringAssert.Contains(tree, "DocumentIntermediateNode");
        StringAssert.Contains(tree, "CSharpCodeIntermediateNode");
        StringAssert.Contains(tree, "MarkupElementIntermediateNode TagName=\"ul\"");
        StringAssert.Contains(tree, "MarkupElementIntermediateNode TagName=\"li\"");
        StringAssert.Contains(tree, "CSharpExpressionIntermediateNode");
    }

    [TestMethod]
    public void ProcessDesignTime_ForElseIf_ProducesChainedCodeAndMarkupNodes()
    {
        var codeDocument = RazorIrTestHost.CreateCodeDocument(
            @"D:\temp\InventoryElseIf.jazor",
            """
            @if (primary)
            {
                <p>Primary</p>
            }
            else if (secondary)
            {
                <p>Secondary</p>
            }
            else
            {
                <p>Fallback</p>
            }

            @code {
                private bool primary;
                private bool secondary;
            }
            """);

        var documentNode = RazorIrTestHost.GetDocumentNode(codeDocument);
        var tree = RazorIrTestHost.DumpIntermediateNodeTree(documentNode);

        TestContext.WriteLine(tree);

        StringAssert.Contains(tree, "DocumentIntermediateNode");
        StringAssert.Contains(tree, "CSharpCodeIntermediateNode");
        StringAssert.Contains(tree, "MarkupElementIntermediateNode TagName=\"p\"");
        StringAssert.Contains(tree, "Content=\"if (primary)");
        StringAssert.Contains(tree, "Content=\"\\n}\\nelse if (secondary)");
        StringAssert.Contains(tree, "Content=\"\\n}\\nelse\\n{");
    }

    [TestMethod]
    public void ProcessDesignTime_ForCountStyleFor_ProducesCodeAndMarkupNodes()
    {
        var codeDocument = RazorIrTestHost.CreateCodeDocument(
            @"D:\temp\InventoryFor.jazor",
            """
            @for (var i = 0; i < count; i++)
            {
                <p>@i</p>
            }

            @code {
                private int count;
            }
            """);

        var documentNode = RazorIrTestHost.GetDocumentNode(codeDocument);
        var tree = RazorIrTestHost.DumpIntermediateNodeTree(documentNode);

        TestContext.WriteLine(tree);

        StringAssert.Contains(tree, "DocumentIntermediateNode");
        StringAssert.Contains(tree, "CSharpCodeIntermediateNode");
        StringAssert.Contains(tree, "MarkupElementIntermediateNode TagName=\"p\"");
        StringAssert.Contains(tree, "Content=\"for (var i = 0; i < count; i++)");
    }

    [TestMethod]
    public void ProcessDesignTime_ForUppercaseTagInMinimalHost_StillProducesMarkupElementInventory()
    {
        var codeDocument = RazorIrTestHost.CreateCodeDocument(
            @"D:\temp\InventoryUppercaseTag.jazor",
            """
            <CounterCard Title="@message">
                <p>Body</p>
            </CounterCard>

            @code {
                private string message = "hello";
            }
            """);

        var documentNode = RazorIrTestHost.GetDocumentNode(codeDocument);
        var tree = RazorIrTestHost.DumpIntermediateNodeTree(documentNode);

        TestContext.WriteLine(tree);

        StringAssert.Contains(tree, "MarkupElementIntermediateNode TagName=\"CounterCard\"");
        StringAssert.Contains(tree, "MarkupElementIntermediateNode TagName=\"p\"");
        StringAssert.Contains(tree, "HtmlAttributeIntermediateNode AttributeName=\"Title\"");
        StringAssert.Contains(tree, "CSharpExpressionAttributeValueIntermediateNode");
        Assert.IsFalse(
            tree.Contains("TagHelper", StringComparison.Ordinal),
            "The minimal standalone host unexpectedly emitted TagHelper-oriented nodes. Re-check component discovery assumptions before relying on this host for component IR.");
    }

    [TestMethod]
    public void ProcessDesignTime_ForNestedUppercaseTagsInMinimalHost_ProducesNestedMarkupInventory()
    {
        var codeDocument = RazorIrTestHost.CreateCodeDocument(
            @"D:\temp\InventoryNestedUppercaseTags.jazor",
            """
            <LayoutCard>
                <Header>
                    <h1>@title</h1>
                </Header>
                <Body>
                    <p>@message</p>
                </Body>
            </LayoutCard>

            @code {
                private string title = "Title";
                private string message = "Body";
            }
            """);

        var documentNode = RazorIrTestHost.GetDocumentNode(codeDocument);
        var tree = RazorIrTestHost.DumpIntermediateNodeTree(documentNode);

        TestContext.WriteLine(tree);

        StringAssert.Contains(tree, "MarkupElementIntermediateNode TagName=\"LayoutCard\"");
        StringAssert.Contains(tree, "MarkupElementIntermediateNode TagName=\"Header\"");
        StringAssert.Contains(tree, "MarkupElementIntermediateNode TagName=\"Body\"");
        StringAssert.Contains(tree, "MarkupElementIntermediateNode TagName=\"h1\"");
        StringAssert.Contains(tree, "MarkupElementIntermediateNode TagName=\"p\"");
    }

    [TestMethod]
    public void ProcessDesignTime_ForAttributeAndBodyExpressions_UsesDifferentExpressionNodeShapes()
    {
        var codeDocument = RazorIrTestHost.CreateCodeDocument(
            @"D:\temp\InventoryAttributeAndBodyExpressions.jazor",
            """
            <div title="@message">@message</div>

            @code {
                private string message = "hello";
            }
            """);

        var documentNode = RazorIrTestHost.GetDocumentNode(codeDocument);
        var tree = RazorIrTestHost.DumpIntermediateNodeTree(documentNode);

        TestContext.WriteLine(tree);

        StringAssert.Contains(tree, "MarkupElementIntermediateNode TagName=\"div\"");
        StringAssert.Contains(tree, "HtmlAttributeIntermediateNode AttributeName=\"title\"");
        StringAssert.Contains(tree, "CSharpExpressionAttributeValueIntermediateNode");
        StringAssert.Contains(tree, "CSharpExpressionIntermediateNode");
    }

    [TestMethod]
    public void AlignedContext_ForElementSplat_ProducesSplatInventory()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<div title="@Title" @attributes="AdditionalAttributes">Hello</div>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Inventory.ElementSplat",
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

        var tree = RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot);
        TestContext.WriteLine(tree);
        StringAssert.Contains(tree, "HtmlAttributeIntermediateNode AttributeName=\"@attributes\"");
    }

    [TestMethod]
    public void AlignedContext_ForAtKey_ProducesKeyInventory()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            <section @key="'root'">
                <SharedBadge @key="Id" Text="@Title" />
            </section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.Inventory.AtKey",
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

        var tree = RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot);
        TestContext.WriteLine(tree);

        StringAssert.Contains(tree, "MarkupElementIntermediateNode TagName=\"section\"");
        StringAssert.Contains(tree, "ComponentIntermediateNode");
        StringAssert.Contains(tree, "HtmlAttributeIntermediateNode AttributeName=\"@key\"");
        StringAssert.Contains(tree, "ComponentAttributeIntermediateNode AttributeName=\"@key\"");
        StringAssert.Contains(tree, "HtmlAttributeValueIntermediateNode");
        StringAssert.Contains(tree, "HtmlContentIntermediateNode");
    }

    [TestMethod]
    public void ProcessDesignTime_ForComponentNamedAndTypedChildContent_ProducesComponentChildContentInventory()
    {
        var codeDocument = RazorIrTestHost.CreateCodeDocument(
            @"D:\temp\InventoryNamedTypedChildContent.razor",
            """
            <LayoutCard Title="@title">
                <Header>
                    <h1>@title</h1>
                </Header>
                <ItemTemplate Context="item">
                    <p>@item</p>
                </ItemTemplate>
            </LayoutCard>

            @code {
                private string title = "Title";
            }
            """,
            importSources: [],
            tagHelpers: null);

        var documentNode = RazorIrTestHost.GetDocumentNode(codeDocument);
        var tree = RazorIrTestHost.DumpIntermediateNodeTree(documentNode);

        TestContext.WriteLine(tree);

        StringAssert.Contains(tree, "LayoutCard");
    }

    [TestMethod]
    public void AlignedContext_ForComponentNamedAndTypedChildContent_ProducesComponentChildContentNodes()
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
            "RazorVue.RazorIr.Inventory.NamedTypedChildContent",
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

        var tree = RazorVueRazorIrTestContextFactory.GetDocumentTreeDump(context, snapshot);

        TestContext.WriteLine(tree);

        StringAssert.Contains(tree, "ComponentIntermediateNode");
        StringAssert.Contains(tree, "ComponentChildContentIntermediateNode AttributeName=\"Header\"");
        StringAssert.Contains(tree, "ComponentChildContentIntermediateNode AttributeName=\"ItemTemplate\"");
    }

}
