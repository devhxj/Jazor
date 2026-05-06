namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorCodeDocumentAcquisitionTests
{
    [TestMethod]
    public void ProcessDesignTime_ForSimpleComponent_CreatesCodeDocumentWithIntermediateRoot()
    {
        var codeDocument = RazorIrTestHost.CreateCodeDocument(
            @"D:\temp\Counter.jazor",
            """
            <h1>@message</h1>

            @code {
                private string message = "hello";
            }
            """);

        var intermediateRoot = RazorIrTestHost.GetDocumentNode(codeDocument);
        var nodeTypeNames = RazorIrTestHost.EnumerateIntermediateNodeTypeNames(intermediateRoot).ToArray();

        Assert.IsNotNull(intermediateRoot);
        CollectionAssert.Contains(nodeTypeNames, "DocumentIntermediateNode");
        CollectionAssert.Contains(nodeTypeNames, "MarkupElementIntermediateNode");
        CollectionAssert.Contains(nodeTypeNames, "CSharpExpressionIntermediateNode");
    }

    [TestMethod]
    public void ProcessDesignTime_ForControlFlowComponent_ExposesStructuredIntermediateNodes()
    {
        var codeDocument = RazorIrTestHost.CreateCodeDocument(
            @"D:\temp\LoopingCounter.jazor",
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

        var intermediateRoot = RazorIrTestHost.GetDocumentNode(codeDocument);
        var nodeTypeNames = RazorIrTestHost.EnumerateIntermediateNodeTypeNames(intermediateRoot).ToArray();

        Assert.IsNotNull(intermediateRoot);
        CollectionAssert.Contains(nodeTypeNames, "DocumentIntermediateNode");
        CollectionAssert.Contains(nodeTypeNames, "MarkupElementIntermediateNode");
        CollectionAssert.Contains(nodeTypeNames, "CSharpCodeIntermediateNode");
    }
}
