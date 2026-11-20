namespace Jolt.Compiler.Test;

[TestClass]
public sealed class InlineFragmentTests
{
    [TestMethod]
    public async Task Compile_ResolvesComponentsInsideInlineFragments()
    {
        const string app = """
            <p>@Label("one")</p>
            @code { private RenderFragment<string> Label = value => @<Caption Text="@value" />; }
            """;
        const string caption = """
            <strong>@Text</strong>
            @code { [Parameter] public string Text { get; set; } = ""; }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", app), new("Caption.jazor", caption)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules[0].Content, "./Caption.mjs");
    }

    [TestMethod]
    public async Task Compile_PassesTypedInlineFragmentsAsPropsAndRendersInvocations()
    {
        const string app = """
            <Display Label="@Label" />
            @{ var prefix = "!"; RenderFragment<string> local = value => @<b>@(prefix + value)</b>; }
            <p>@local("two")</p>
            @code { private RenderFragment<string> Label = name => @<strong>@name</strong>; }
            """;
        const string display = """
            <div>@Label("one")</div>
            @code { [Parameter] public RenderFragment<string> Label { get; set; } = default!; }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", app), new("Display.jazor", display)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules[0].Content, "\"strong\"");
        StringAssert.Contains(result.Modules[0].Content, "\"b\"");
        Assert.IsFalse(result.Modules[1].Content.Contains("$joltDisplay(", StringComparison.Ordinal));
        var invalidApp = app.Replace("local(\"two\")", "local(42)");
        var invalid = await JoltProjectCompiler.CompileAsync([new("App.jazor", invalidApp), new("Display.jazor", display)]);
        var diagnostic = invalid.Diagnostics.Single(item => item.Id == "CS1503");
        var span = diagnostic.Location.SourceSpan;
        Assert.AreEqual("42", invalidApp.Substring(span.Start, span.Length));
    }

    [TestMethod]
    public async Task Compile_ReusesPayloadlessInlineFragmentInLoop()
    {
        const string source = """
            @{ var message = "message"; RenderFragment label = () => @<strong>@message</strong>; }
            @for (var i = 0; i < 2; i++) { <p>@label()</p> }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var module = result.Modules.Single().Content;
        StringAssert.Contains(module, "label()");
        StringAssert.Contains(module, "\"strong\"");
        Assert.IsFalse(module.Contains("<strong>", StringComparison.Ordinal));
    }
}
