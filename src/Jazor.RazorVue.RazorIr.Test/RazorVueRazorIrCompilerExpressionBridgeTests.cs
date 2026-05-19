using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorVueRazorIrCompilerExpressionBridgeTests
{
    public TestContext TestContext { get; set; } = default!;

    [TestMethod]
    public void RazorVuePipeline_WithRazorIrTemplateFrontend_LowersWhitelistedTemplateExpressions_UsingCompilerSemantics()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using System
            <section>@Math.Abs(Value) @DateOnly.Parse(RawDate).ToString()</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.CompilerExpressionBridge.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public string RawDate { get; set; } = "2024-01-02";
                }
            }
            """);

        var artifact = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);

        StringAssert.Contains(artifact.ModuleCode, "Math.abs(props.value)");
        StringAssert.Contains(artifact.ModuleCode, "from \"System/DateOnlyModule.js\";");
        CollectionAssert.Contains(artifact.Imports.ToArray(), "System/DateOnlyModule.js");
    }

    [TestMethod]
    public void RazorVueRazorIrOperationResolver_ForTemplateLocalCodeBlock_MapsToVariableDeclarationOperation()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                var localTitle = Title;
            }

            <section>@localTitle</section>
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.OperationResolver.TemplateLocalCodeBlock.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        static string ResolveNodeText(RazorVueRazorIrNode node)
        {
            var text = string.Concat(node.Tokens.Select(static token => token.Content));
            return text.Length == 0 ? node.Content ?? string.Empty : text;
        }

        static IEnumerable<RazorVueRazorIrNode> EnumerateNodes(RazorVueRazorIrNode node)
        {
            yield return node;
            foreach (var child in node.Children)
            {
                foreach (var nested in EnumerateNodes(child))
                    yield return nested;
            }
        }

        var codeNode = EnumerateNodes(snapshot.RazorSourceGeneratorDocument!.DocumentNode)
            .First(static node => node.Kind == RazorVueRazorIrNodeKind.CSharpCode &&
                                  ResolveNodeText(node).Contains("var localTitle = Title;", StringComparison.Ordinal));

        var resolver = new RazorVueRazorIrOperationResolver(
            context,
            snapshot,
            snapshot.RazorSourceGeneratorDocument!);
        var operation = resolver.ResolveRequiredOperation(codeNode.Source, "template local code block");

        Assert.IsTrue(
            operation is IVariableDeclarationGroupOperation or IVariableDeclarationOperation,
            $"Unexpected operation type: {operation.GetType().FullName}, kind: {operation.Kind}, syntax: {operation.Syntax}");
    }

    [TestMethod]
    public void RazorVueRazorIrOperationResolver_ForComplexTemplateLocalCodeBlock_MapsToBlockOperation()
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
            "RazorVue.RazorIr.OperationResolver.ComplexTemplateLocalCodeBlock.Tests",
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

        static string ResolveNodeText(RazorVueRazorIrNode node)
        {
            var text = string.Concat(node.Tokens.Select(static token => token.Content));
            return text.Length == 0 ? node.Content ?? string.Empty : text;
        }

        static IEnumerable<RazorVueRazorIrNode> EnumerateNodes(RazorVueRazorIrNode node)
        {
            yield return node;
            foreach (var child in node.Children)
            {
                foreach (var nested in EnumerateNodes(child))
                    yield return nested;
            }
        }

        var codeNode = EnumerateNodes(snapshot.RazorSourceGeneratorDocument!.DocumentNode)
            .First(static node => node.Kind == RazorVueRazorIrNodeKind.CSharpCode &&
                                  ResolveNodeText(node).Contains("var localTitle = Title;", StringComparison.Ordinal) &&
                                  ResolveNodeText(node).Contains("if (Show)", StringComparison.Ordinal));

        var resolver = new RazorVueRazorIrOperationResolver(
            context,
            snapshot,
            snapshot.RazorSourceGeneratorDocument!);
        var operation = resolver.ResolveRequiredOperation(codeNode.Source, "complex template local code block");

        Assert.IsInstanceOfType<IBlockOperation>(operation);
    }

    [TestMethod]
    public void RazorVueRazorIrOperationResolver_ForSequentialIfBoundaryCodeNode_MapsToSecondConditional()
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
            "RazorVue.RazorIr.OperationResolver.SequentialIfBoundary.Tests",
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

        static string ResolveNodeText(RazorVueRazorIrNode node)
        {
            var text = string.Concat(node.Tokens.Select(static token => token.Content));
            return text.Length == 0 ? node.Content ?? string.Empty : text;
        }

        static IEnumerable<RazorVueRazorIrNode> EnumerateNodes(RazorVueRazorIrNode node)
        {
            yield return node;
            foreach (var child in node.Children)
            {
                foreach (var nested in EnumerateNodes(child))
                    yield return nested;
            }
        }

        var codeNode = EnumerateNodes(snapshot.RazorSourceGeneratorDocument!.DocumentNode)
            .First(static node => node.Kind == RazorVueRazorIrNodeKind.CSharpCode &&
                                  ResolveNodeText(node).Contains("ShowSecondary", StringComparison.Ordinal));

        var rawText = ResolveNodeText(codeNode);
        var keywordIndex = rawText.IndexOf("if (ShowSecondary)", StringComparison.Ordinal);
        Assert.IsTrue(keywordIndex >= 0, rawText);

        var narrowedSourceSpan = codeNode.Source!.Value with
        {
            AbsoluteIndex = codeNode.Source.Value.AbsoluteIndex + keywordIndex,
            Length = Math.Max(1, codeNode.Source.Value.Length - keywordIndex)
        };

        var resolver = new RazorVueRazorIrOperationResolver(
            context,
            snapshot,
            snapshot.RazorSourceGeneratorDocument!);

        var resolved = resolver.TryResolveConditional(narrowedSourceSpan, out var conditional);

        Assert.IsTrue(resolved);
        Assert.AreEqual("ShowSecondary", conditional.Operation.Condition.Syntax.ToString());
    }

    [TestMethod]
    public void RazorVueRazorIrOperationResolver_ForSequentialForeachThenIfBoundaryCodeNode_MapsToTrailingConditional()
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
            "RazorVue.RazorIr.OperationResolver.SequentialForeachThenIfBoundary.Tests",
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

        static string ResolveNodeText(RazorVueRazorIrNode node)
        {
            var text = string.Concat(node.Tokens.Select(static token => token.Content));
            return text.Length == 0 ? node.Content ?? string.Empty : text;
        }

        static IEnumerable<RazorVueRazorIrNode> EnumerateNodes(RazorVueRazorIrNode node)
        {
            yield return node;
            foreach (var child in node.Children)
            {
                foreach (var nested in EnumerateNodes(child))
                    yield return nested;
            }
        }

        var codeNode = EnumerateNodes(snapshot.RazorSourceGeneratorDocument!.DocumentNode)
            .First(static node => node.Kind == RazorVueRazorIrNodeKind.CSharpCode &&
                                  ResolveNodeText(node).Contains("ShowTail", StringComparison.Ordinal));

        var rawText = ResolveNodeText(codeNode);
        var keywordIndex = rawText.IndexOf("if (ShowTail)", StringComparison.Ordinal);
        Assert.IsTrue(keywordIndex >= 0, rawText);

        var narrowedSourceSpan = codeNode.Source!.Value with
        {
            AbsoluteIndex = codeNode.Source.Value.AbsoluteIndex + keywordIndex,
            Length = Math.Max(1, codeNode.Source.Value.Length - keywordIndex)
        };

        var resolver = new RazorVueRazorIrOperationResolver(
            context,
            snapshot,
            snapshot.RazorSourceGeneratorDocument!);

        var resolved = resolver.TryResolveConditional(narrowedSourceSpan, out var conditional);

        Assert.IsTrue(resolved);
        Assert.AreEqual("ShowTail", conditional.Operation.Condition.Syntax.ToString());
    }

    [TestMethod]
    public void RazorVueRazorIrOperationResolver_ForRenderFragmentLocalCarrierTemplateCodeBlock_MapsToVariableDeclarationGroup()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @{
                RenderFragment<string> template = item => @<p>@item</p>;
            }

            <LayoutCard ItemTemplate="template" />
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.OperationResolver.RenderFragmentLocalCarrier.Tests",
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

        static string ResolveNodeText(RazorVueRazorIrNode node)
        {
            var text = string.Concat(node.Tokens.Select(static token => token.Content));
            return text.Length == 0 ? node.Content ?? string.Empty : text;
        }

        static IEnumerable<RazorVueRazorIrNode> EnumerateNodes(RazorVueRazorIrNode node)
        {
            yield return node;
            foreach (var child in node.Children)
            {
                foreach (var nested in EnumerateNodes(child))
                    yield return nested;
            }
        }

        var codeNode = EnumerateNodes(snapshot.RazorSourceGeneratorDocument!.DocumentNode)
            .First(static node => node.Kind == RazorVueRazorIrNodeKind.CSharpCode &&
                                  ResolveNodeText(node).Contains("RenderFragment<string> template", StringComparison.Ordinal));

        var resolver = new RazorVueRazorIrOperationResolver(
            context,
            snapshot,
            snapshot.RazorSourceGeneratorDocument!);
        var operation = resolver.ResolveRequiredOperation(codeNode.Source, "RenderFragment local carrier code block");

        Assert.IsTrue(
            operation is IVariableDeclarationGroupOperation or IVariableDeclarationOperation,
            $"Unexpected operation type: {operation.GetType().FullName}, kind: {operation.Kind}, syntax: {operation.Syntax}");
    }

    [TestMethod]
    public void RazorVueRazorIrOperationResolver_ForRenderFragmentLocalCarrierTrailingIfBoundaryCodeNode_MapsToConditional()
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
            "RazorVue.RazorIr.OperationResolver.RenderFragmentLocalCarrier.TrailingIf.Tests",
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

        static string ResolveNodeText(RazorVueRazorIrNode node)
        {
            var text = string.Concat(node.Tokens.Select(static token => token.Content));
            return text.Length == 0 ? node.Content ?? string.Empty : text;
        }

        static IEnumerable<RazorVueRazorIrNode> EnumerateNodes(RazorVueRazorIrNode node)
        {
            yield return node;
            foreach (var child in node.Children)
            {
                foreach (var nested in EnumerateNodes(child))
                    yield return nested;
            }
        }

        var codeNode = EnumerateNodes(snapshot.RazorSourceGeneratorDocument!.DocumentNode)
            .First(static node => node.Kind == RazorVueRazorIrNodeKind.CSharpCode &&
                                  ResolveNodeText(node).Contains("if (Show)", StringComparison.Ordinal));

        var rawText = ResolveNodeText(codeNode);
        var keywordIndex = rawText.IndexOf("if (Show)", StringComparison.Ordinal);
        Assert.IsTrue(keywordIndex >= 0, rawText);

        var narrowedSourceSpan = codeNode.Source!.Value with
        {
            AbsoluteIndex = codeNode.Source.Value.AbsoluteIndex + keywordIndex,
            Length = Math.Max(1, codeNode.Source.Value.Length - keywordIndex)
        };

        var resolver = new RazorVueRazorIrOperationResolver(
            context,
            snapshot,
            snapshot.RazorSourceGeneratorDocument!);
        var resolved = resolver.TryResolveConditional(narrowedSourceSpan, out var conditional);

        Assert.IsTrue(resolved);
        Assert.AreEqual("Show", conditional.Operation.Condition.Syntax.ToString());
    }

}
