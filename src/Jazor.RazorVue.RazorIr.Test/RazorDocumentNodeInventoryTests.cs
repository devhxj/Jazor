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

}
