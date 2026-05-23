using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorVueRazorIrCompilerExpressionBridgeTests
{
    public TestContext TestContext { get; set; } = default!;

    [TestMethod]
    public void RazorVueRazorIrOperationResolver_ForRawMarkupElementDomEventLambda_ResolvesCompilationOwnedProbeOperation()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """<button @onclick="() => Count++">Go</button>""";

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.OperationResolver.ElementDomEventLambda.Tests",
            documentPath,
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    private int Count { get; set; }
                }
            }
            """);

        var resolver = new RazorVueRazorIrOperationResolver(
            context,
            snapshot,
            snapshot.RazorSourceGeneratorDocument!);

        Assert.IsFalse(
            resolver.TryResolveBuilderAttributeValue("AddAttribute", "onclick", 0, out _),
            "Razor SDK raw markup fallback currently keeps @onclick inside AddMarkupContent rather than generating AddAttribute.");
        Assert.IsTrue(
            resolver.TryResolveComponentExpression(
                "Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => Count++)",
                out var operation),
            snapshot.RazorSourceGeneratorDocument!.CSharpText.ToString());
        Assert.IsInstanceOfType<IInvocationOperation>(operation);
        Assert.IsTrue(
            operation.SemanticModel?.Compilation.ContainsSyntaxTree(operation.Syntax.SyntaxTree) == true,
            "Resolved event handler operation must carry the compilation that owns its probe syntax tree.");
    }

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

    [TestMethod]
    public void RazorVueRazorIrOperationResolver_ForRenderFragmentPropertyInitializer_RemainsUnmappedAtGeneratedBuilderLambdaBoundary()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @Template

            @code {
                private RenderFragment Template => @<section><span>safe</span><p>ok</p></section>;
            }
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.OperationResolver.RenderFragmentProperty.Expression.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var property = snapshot.ComponentSymbol.GetMembers("Template").OfType<IPropertySymbol>().Single();
        var declaration = (PropertyDeclarationSyntax)property.DeclaringSyntaxReferences.Single().GetSyntax();
        var semanticModel = context.Compilation.GetSemanticModel(declaration.SyntaxTree);

        Assert.IsTrue(
            RazorVuePropertyInitializerHelper.TryGetPropertyValueOperation(semanticModel, declaration, out var initializer) &&
            initializer is not null,
            "Property initializer operation could not be resolved.");

        var resolver = new RazorVueRazorIrOperationResolver(
            context,
            snapshot,
            snapshot.RazorSourceGeneratorDocument!);

        Assert.IsFalse(
            resolver.TryMapGeneratedOperationToOriginalSourceSpan(initializer, out _),
            "Generated builder-lambda property carrier should not be assumed to carry direct Razor source mapping.");
    }

    [TestMethod]
    public void RazorVueImperativeRenderFragmentCarrierHelper_ForImmediatelyAssignedUntypedRenderFragment_FindsAnonymousFunctionInitializer()
    {
        const string documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        const string documentText = """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment template;
                template = @<section><span>safe</span><p>ok</p></section>;
            }

            @template
            """;

        var (context, snapshot) = RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.OperationResolver.RenderFragmentLocalCarrier.Expression.ImmediateAssignment.Tests",
            documentPath,
            documentText,
            RazorVueRazorIrTestContextFactory.CreateParentComponentSource());

        var buildRenderTree = snapshot.BuildRenderTreeMethod!;
        var methodDeclaration = (MethodDeclarationSyntax)buildRenderTree.DeclaringSyntaxReferences.Single().GetSyntax();
        var semanticModel = context.Compilation.GetSemanticModel(methodDeclaration.SyntaxTree);
        var rootOperation = (IBlockOperation)semanticModel.GetOperation(methodDeclaration.Body!)!;

        static IEnumerable<IOperation> EnumerateOperations(IOperation root)
        {
            yield return root;
            foreach (var child in root.ChildOperations)
            {
                if (child is null)
                    continue;

                foreach (var nested in EnumerateOperations(child))
                    yield return nested;
            }
        }

        var local = EnumerateOperations(rootOperation)
            .OfType<IVariableDeclaratorOperation>()
            .Single(operation => string.Equals(operation.Symbol.Name, "template", StringComparison.Ordinal))
            .Symbol;

        Assert.IsTrue(
            RazorVueImperativeRenderFragmentCarrierHelper.TryGetSourceStableLocalRenderFragmentInitializer(
                context.Compilation,
                local,
                out var initializer) &&
            initializer is not null,
            "Source-stable RenderFragment initializer could not be resolved.");

        Assert.IsTrue(
            RazorVueImperativeRenderFragmentCarrierHelper.TryGetAnonymousFunction(initializer, out _),
            $"Expected immediate-assignment initializer to normalize to an anonymous function, but got '{initializer.Kind}' with syntax '{initializer.Syntax}'.");
    }

}
