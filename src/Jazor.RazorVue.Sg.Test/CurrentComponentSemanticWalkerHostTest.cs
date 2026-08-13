using ECMAScript;
using Jazor.Compiler;
using Jazor.RazorVue.RazorSdk;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class VueSemanticWalkerHostTest
{
    [TestMethod]
    public void CollectReachableMembers_IncludesRenderStateParameterAndEventHandlerWithoutUnusedMembers()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                [Parameter]
                public string Title { get; set; } = "";

                private int count = 1;

                private int unused;

                private void Increment()
                {
                    count++;
                }

                private void Unused()
                {
                    unused++;
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "button");
                    builder.AddAttribute(1, "onclick", (Action)Increment);
                    builder.AddContent(2, count);
                    builder.AddContent(3, Title);
                    builder.CloseElement();
                }
            }
            """);

        var first = CurrentComponentMemberClosure.Build(
            fixture.Component,
            fixture.SemanticModel,
            new[] { fixture.BuildRenderTreeMethod });
        var second = CurrentComponentMemberClosure.Build(
            fixture.Component,
            fixture.SemanticModel,
            new[] { fixture.BuildRenderTreeMethod });

        var firstMembers = first.Members.Select(static member => member.Name).ToArray();
        var secondMembers = second.Members.Select(static member => member.Name).ToArray();
        CollectionAssert.AreEqual(firstMembers, secondMembers);
        CollectionAssert.Contains(firstMembers, "BuildRenderTree");
        CollectionAssert.Contains(firstMembers, "Increment");
        CollectionAssert.Contains(firstMembers, "count");
        CollectionAssert.Contains(firstMembers, "Title");
        CollectionAssert.DoesNotContain(firstMembers, "Unused");
        CollectionAssert.DoesNotContain(firstMembers, "unused");
    }

    [TestMethod]
    public void CollectReachableMembers_IncludesInheritedCurrentComponentHelper()
    {
        var fixture = CompileComponent(
            """
            public abstract class CounterBase : ComponentBase
            {
                protected string FormatTitle(string value) => "Count: " + value;
            }

            public sealed class Counter : CounterBase
            {
                private string title = "1";

                private string DisplayTitle => FormatTitle(title);

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, DisplayTitle);
                }
            }
            """);

        var closure = CurrentComponentMemberClosure.Build(
            fixture.Component,
            fixture.SemanticModel,
            new[] { fixture.BuildRenderTreeMethod });

        var members = closure.Members.Select(static member => member.Name).ToArray();
        CollectionAssert.Contains(members, "BuildRenderTree");
        CollectionAssert.Contains(members, "DisplayTitle");
        CollectionAssert.Contains(members, "FormatTitle");
        CollectionAssert.Contains(members, "title");
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_RejectsParameterWrites()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                [Parameter]
                public string Title { get; set; } = "";

                private void Update()
                {
                    Title = "changed";
                }
            }
            """,
            methodName: "Update");

        var walker = new SemanticWalker(true)
        {
            Host = new VueSemanticWalkerHost(fixture.Component)
        };

        var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(fixture.BuildRenderTreeBody, new()));
        StringAssert.Contains(exception.Message, "Microsoft.AspNetCore.Components.ParameterAttribute", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "Counter.Title", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "read-only props", StringComparison.Ordinal);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_RejectsInterfaceDispatchWithSourceLocation()
    {
        var fixture = CompileComponent(
            """
            public interface ICounterActions
            {
                void Increment();
            }

            public sealed class Counter : ComponentBase, ICounterActions
            {
                private int count;

                public void Increment()
                {
                    count++;
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    ((ICounterActions)this).Increment();
                    builder.AddContent(0, count);
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new VueSemanticWalkerHost(fixture.Component)
        };

        var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(fixture.BuildRenderTreeBody, new()));
        StringAssert.Contains(exception.Message, "Indirect current-component dispatch", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "ICounterActions.Increment()", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "static receiver type 'ICounterActions'", StringComparison.Ordinal);
        Assert.AreEqual("Counter.razor.g.cs", Path.GetFileName((string)exception.Data["location.path"]!));
        Assert.AreEqual(17, exception.Data["location.startLine"]);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_LowersStateHasChangedToSetupInvalidator()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                private int count;

                private void Increment()
                {
                    count++;
                    StateHasChanged();
                }
            }
            """,
            methodName: "Increment");

        var walker = new SemanticWalker(true)
        {
            Host = new VueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "state.count++;", StringComparison.Ordinal);
        StringAssert.Contains(script!, "stateHasChanged();", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("StateHasChanged", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("this.", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_LowersComponentBaseInvokeAsyncToSetupDispatcher()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                private int count;

                private void Refresh()
                {
                    _ = InvokeAsync(Increment);
                    _ = InvokeAsync(() => { count++; StateHasChanged(); });
                }

                private void Increment()
                {
                    count++;
                }
            }
            """,
            methodName: "Refresh");

        var walker = new SemanticWalker(true)
        {
            Host = new VueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "invokeAsync(Increment);", StringComparison.Ordinal);
        StringAssert.Contains(script!, "invokeAsync(() => {", StringComparison.Ordinal);
        StringAssert.Contains(script!, "state.count++;", StringComparison.Ordinal);
        StringAssert.Contains(script!, "stateHasChanged();", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("InvokeAsync", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("this.", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_LowersExplicitComponentBaseReceiversToSetupHelpers()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                private int count;

                private void Refresh()
                {
                    count++;
                    this.StateHasChanged();
                    _ = this.InvokeAsync(Increment);
                }

                private void Increment()
                {
                    count++;
                }
            }
            """,
            methodName: "Refresh");

        var walker = new SemanticWalker(true)
        {
            Host = new VueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "state.count++;", StringComparison.Ordinal);
        StringAssert.Contains(script!, "stateHasChanged();", StringComparison.Ordinal);
        StringAssert.Contains(script!, "invokeAsync(Increment);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("StateHasChanged", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("InvokeAsync", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("this.", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_LowersEventCallbackParameterInvokeToOptionalPropsCall()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                [Parameter]
                public EventCallback OnClick { get; set; }

                [Parameter]
                public EventCallback<int> OnValue { get; set; }

                private void Raise()
                {
                    _ = OnClick.InvokeAsync();
                    _ = OnValue.InvokeAsync(3);
                }
            }
            """,
            methodName: "Raise");

        var walker = new SemanticWalker(true)
        {
            Host = new VueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "props.OnClick?.();", StringComparison.Ordinal);
        StringAssert.Contains(script!, "props.OnValue?.(3);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("InvokeAsync", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("this.", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_LowersNestedEventCallbackInvokeToOptionalFunctionCall()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                public sealed class ActionEntry
                {
                    public EventCallback Click { get; set; }

                    public EventCallback<int> Select { get; set; }
                }

                private void Raise(ActionEntry action)
                {
                    _ = action.Click.InvokeAsync();
                    _ = action.Select.InvokeAsync(3);
                }
            }
            """,
            methodName: "Raise");

        var walker = new SemanticWalker(true)
        {
            Host = new VueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "action.Click?.();", StringComparison.Ordinal);
        StringAssert.Contains(script!, "action.Select?.(3);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("InvokeAsync", StringComparison.Ordinal), script);
    }

    private static ComponentFixture CompileComponent(string source, string methodName = "BuildRenderTree")
    {
        var usings =
            """
            global using System;
            global using ECMAScript;
            global using static ECMAScript.Global;
            global using Microsoft.AspNetCore.Components;
            global using Microsoft.AspNetCore.Components.Web;
            global using Microsoft.AspNetCore.Components.Rendering;
            """;
        var references = TestMetadataReferences.Net11
            .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(ComponentBase).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(MouseEventArgs).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(RenderTreeBuilder).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            assemblyName: "CurrentComponent.SemanticWalkerHost.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(usings, TestMetadataReferences.PreviewParseOptions, path: "GlobalUsings.g.cs"),
                CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions, path: "Counter.razor.g.cs")
            ],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        if (errors.Length > 0)
            throw new InvalidOperationException(string.Join("\n", errors.Select(static error => $"{error.Id}: {error.GetMessage()}")));

        var syntaxTree = compilation.SyntaxTrees.Last();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var componentDeclaration = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "Counter");
        var component = semanticModel.GetDeclaredSymbol(componentDeclaration)
            ?? throw new InvalidOperationException("Counter component symbol was not available.");
        var methodDeclaration = componentDeclaration
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(method => method.Identifier.ValueText == methodName);
        var method = semanticModel.GetDeclaredSymbol(methodDeclaration)
            ?? throw new InvalidOperationException("Target method symbol was not available.");
        var body = semanticModel.GetOperation(methodDeclaration.Body!) as IBlockOperation
            ?? throw new InvalidOperationException("Target method body operation was not available.");

        return new ComponentFixture(compilation, semanticModel, component, method, body);
    }

    private sealed record ComponentFixture(
        Compilation Compilation,
        SemanticModel SemanticModel,
        INamedTypeSymbol Component,
        IMethodSymbol BuildRenderTreeMethod,
        IBlockOperation BuildRenderTreeBody);
}
