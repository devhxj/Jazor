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
public sealed class RazorVueSemanticWalkerHostTest
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
    public void RewriteCurrentComponentMembers_UsesStatePropsAndStableMethodReferenceInRenderTree()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                [Parameter]
                public string Title { get; set; } = "";

                private int count;

                private void Increment()
                {
                    count++;
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

        var walker = new SemanticWalker(true)
        {
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "builder.openElement(\"button\");", StringComparison.Ordinal);
        StringAssert.Contains(script!, "builder.addAttribute(\"onclick\", increment);", StringComparison.Ordinal);
        StringAssert.Contains(script!, "builder.addContent(state.count);", StringComparison.Ordinal);
        StringAssert.Contains(script!, "builder.addContent(props.title);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains(".bind(", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("this.increment", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("this.count", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("this.title", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_LowersInheritedCurrentComponentHelper()
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

        var walker = new SemanticWalker(true)
        {
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "builder.addContent(displayTitle());", StringComparison.Ordinal);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_DelegatesRenderTreeBuilderObjectAndArgumentHooks()
    {
        var fixture = CompileComponent(
            """
            [ECMAScriptModule("./components/child")]
            public sealed class Child : ComponentBase
            {
            }

            public sealed class Counter : ComponentBase
            {
                private string ReadMarkup() => "<em>raw</em>";

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, new MarkupString(ReadMarkup()));
                    builder.OpenComponent(1, typeof(Child));
                    builder.CloseComponent();
                    var childType = typeof(Child);
                    builder.OpenComponent(2, childType);
                    builder.CloseComponent();
                    var nested = new RenderTreeBuilder();
                    nested.AddContent(0, new MarkupString(ReadMarkup()));
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };
        var argument = new SenseArgument(UseImportAliases: true);
        var body = walker.Visit(fixture.BuildRenderTreeBody, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");
        Assert.IsNotNull(body);

        var imports = argument.FlushImportSpecifiers()
            .Select(static pair =>
            {
                var names = string.Join(
                    ", ",
                    pair.Value.Select(static specifier => specifier.ToECMAScript()));
                return "import " + names + " from \"" + pair.Key + "\";";
            });
        var script = string.Join("\n", imports.Concat([body!])).ReplaceLineEndings("\n");

        StringAssert.Contains(script, "builder.addMarkupContent(readMarkup());", StringComparison.Ordinal);
        StringAssert.Contains(script, "from \"./components/child.mjs\";", StringComparison.Ordinal);
        StringAssert.Contains(script, "@jazor/vue-runtime/render-context.mjs", StringComparison.Ordinal);
        StringAssert.Contains(script, "let nested = createRenderContext(h);", StringComparison.Ordinal);
        StringAssert.Contains(script, "nested.addMarkupContent(readMarkup());", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("childType", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("typeof", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("MarkupString", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("from \"./components/child\";", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("new RenderTreeBuilder", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_UnwrapsEventCallbackFactoryCreateHandler()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                private int count;

                private void Increment(MouseEventArgs args)
                {
                    count++;
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "button");
                    builder.AddAttribute(1, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, Increment));
                    builder.CloseElement();
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "builder.addAttribute(\"onclick\", increment);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("EventCallback", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains(".bind(", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_UnwrapsAsyncEventCallbackFactoryCreateHandler()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                private async System.Threading.Tasks.Task Refresh()
                {
                    await System.Threading.Tasks.Task.Delay(1);
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "button");
                    builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, Refresh));
                    builder.CloseElement();
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "builder.addAttribute(\"onclick\", refresh);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("EventCallback", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_ForwardsEventCallbackParameterThroughFactoryCreate()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                [Parameter]
                public EventCallback<string> Changed { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddAttribute(0, "changed", EventCallback.Factory.Create<string>(this, Changed));
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "builder.addAttribute(\"changed\", props.changed);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("EventCallback", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_LowersEventCallbackFactoryCreateCurrentMethodLambda()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                private string selected = "";

                private void Select(string item)
                {
                    selected = item;
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    var item = "home";
                    builder.AddAttribute(0, "onclick", EventCallback.Factory.Create(this, () => Select(item)));
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "builder.addAttribute(\"onclick\", () => {", StringComparison.Ordinal);
        StringAssert.Contains(script, "select(item);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("EventCallback", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_UnwrapsEventCallbackFactoryCreateLambdaHandler()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                private bool collapsed;

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddComponentParameter(1, "CollapsedChanged", EventCallback.Factory.Create<bool>(this, value => collapsed = value));
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "builder.addComponentParameter(\"CollapsedChanged\", value => state.collapsed = value);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("EventCallback", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("this.", StringComparison.Ordinal), script);
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
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };

        var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(fixture.BuildRenderTreeBody, new()));
        StringAssert.Contains(exception.Message, "Microsoft.AspNetCore.Components.ParameterAttribute", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "Counter.Title", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "read-only props", StringComparison.Ordinal);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_LowersComputedPropertyGetter()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                private int count;

                private int Count => count + 1;

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, Count);
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };

        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "builder.addContent(count());", StringComparison.Ordinal);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_ErasesRazorRuntimeHelpersTypeCheck()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                [Parameter]
                public string Title { get; set; } = "";

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<string>(Title));
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "builder.addContent(props.title);", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("TypeCheck", StringComparison.Ordinal), script);
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
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
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
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
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
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "invokeAsync(increment);", StringComparison.Ordinal);
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
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "state.count++;", StringComparison.Ordinal);
        StringAssert.Contains(script!, "stateHasChanged();", StringComparison.Ordinal);
        StringAssert.Contains(script!, "invokeAsync(increment);", StringComparison.Ordinal);
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
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "props.onClick?.();", StringComparison.Ordinal);
        StringAssert.Contains(script!, "props.onValue?.(3);", StringComparison.Ordinal);
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
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "action.click?.();", StringComparison.Ordinal);
        StringAssert.Contains(script!, "action.select?.(3);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("InvokeAsync", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_LowersConditionalEventCallbackMethodGroups()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                private bool usePrimary;

                private void Primary() { }

                private void Secondary() { }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "form");
                    builder.AddAttribute(1, "onsubmit", EventCallback.Factory.Create(this, usePrimary ? Primary : Secondary));
                    builder.CloseElement();
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "builder.addAttribute(\"onsubmit\", state.usePrimary ? primary : secondary);", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("EventCallback", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("this.", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_LowersSingleArgumentBindConverterFormatValueToValue()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                private string text = "";
                private bool enabled;

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "input");
                    builder.AddAttribute(1, "value", BindConverter.FormatValue(text));
                    builder.AddAttribute(2, "checked", BindConverter.FormatValue(enabled));
                    builder.CloseElement();
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "builder.addAttribute(\"value\", state.text);", StringComparison.Ordinal);
        StringAssert.Contains(script!, "builder.addAttribute(\"checked\", state.enabled);", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("BindConverter", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("FormatValue", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_LowersDomBindCreateBinderToStateAssignmentHandler()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                private string text = "";

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "input");
                    builder.AddAttribute(1, "value", text);
                    builder.AddAttribute(2, "onchange", EventCallback.Factory.CreateBinder(this, __value => text = __value, text));
                    builder.SetUpdatesAttributeName("value");
                    builder.CloseElement();
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "builder.addAttribute(\"value\", state.text);", StringComparison.Ordinal);
        StringAssert.Contains(script!, "builder.addAttribute(\"onchange\", __value => state.text = __value);", StringComparison.Ordinal);
        StringAssert.Contains(script!, "builder.setUpdatesAttributeName(\"value\");", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("CreateBinder", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("this.", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_LowersComponentBindCreateBinderToStateAssignmentHandler()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                private string text = "";

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddComponentParameter(1, "Value", text);
                    builder.AddComponentParameter(2, "ValueChanged", EventCallback.Factory.CreateBinder(this, __value => text = __value, text));
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };
        var script = walker.Visit(fixture.BuildRenderTreeBody, new())?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script);
        StringAssert.Contains(script!, "builder.addComponentParameter(\"Value\", state.text);", StringComparison.Ordinal);
        StringAssert.Contains(script!, "builder.addComponentParameter(\"ValueChanged\", __value => state.text = __value);", StringComparison.Ordinal);
        Assert.IsFalse(script!.Contains("CreateBinder", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("modelValue", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("this.", StringComparison.Ordinal), script);
    }

    [TestMethod]
    public void RewriteCurrentComponentMembers_RejectsDomBindToParameter()
    {
        var fixture = CompileComponent(
            """
            public sealed class Counter : ComponentBase
            {
                [Parameter]
                public string Text { get; set; } = "";

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "input");
                    builder.AddAttribute(1, "onchange", EventCallback.Factory.CreateBinder(this, __value => Text = __value, Text));
                    builder.CloseElement();
                }
            }
            """);

        var walker = new SemanticWalker(true)
        {
            Host = new RazorVueSemanticWalkerHost(fixture.Component)
        };

        var exception = Assert.Throws<OperationTransformationException>(() => walker.Visit(fixture.BuildRenderTreeBody, new()));
        StringAssert.Contains(exception.Message, "EventCallbackFactory.CreateBinder", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "Counter.Text", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "read-only props", StringComparison.Ordinal);
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
