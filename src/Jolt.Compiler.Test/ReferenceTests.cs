namespace Jolt.Compiler.Test;

[TestClass]
public sealed class ReferenceTests
{
    [TestMethod]
    public async Task Compile_ReferenceInsideLoop_ReportsDirectiveEvenInsideComponentSlot()
    {
        const string source = """
            @for (int i = 0; i < 1; i++) {
                <Child><input @ref="target" /></Child>
            }
            @foreach (var item in new[] { 1 }) {
                <input @key="item" @ref="target" />
            }
            @code { private ECMAScript.HTMLInputElement? target; }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source), new("Child.jazor", "<slot />")]);
        Assert.IsEmpty(result.Modules);
        var diagnostics = result.Diagnostics.Where(diagnostic => diagnostic.Id == "JOLT1028").ToArray();
        Assert.HasCount(2, diagnostics);
        Assert.IsTrue(diagnostics.All(diagnostic => source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length) == "@ref"));
    }

    [TestMethod]
    public async Task Compile_DomReference_LowersStableVueReferenceAndBindsConcreteElement()
    {
        const string source = """
            @using ECMAScript
            <input @ref="searchBox" />
            <button @onclick="Focus">Focus</button>
            @code {
                private HTMLInputElement? searchBox;
                private void Focus() {
                    searchBox?.Focus();
                    if (searchBox is not null) searchBox.Value = "query";
                }
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var module = result.Modules.Single().Content;
        StringAssert.Contains(module, "\"ref\": $joltToRaw(this)[\"$joltRef0\"] ??= $joltCustomRef");
        StringAssert.Contains(module, "= __joltRefValue");
        StringAssert.Contains(module, ".focus(");
    }

    [TestMethod]
    public async Task Compile_GenericLocalReference_UsesConstructedExposedContract()
    {
        const string child = """
            @typeparam T where T : class
            <div>@Value</div>
            @code {
                [Parameter] public T? Value { get; set; }
                [Expose] public T? Read() => Value;
            }
            """;
        const string parent = """
            @using JoltGenerated.Controls
            <Child T="string" Value="example" @ref="child" />
            <p>@Read()</p>
            @code {
                private ChildRef<string>? child;
                private string Read() => child?.Read() ?? "";
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", parent), new("Controls/Child.jazor", child)],
            new(RootNamespace: "JoltGenerated"));
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules[0].Content, ".Read(");
    }

    [TestMethod]
    public async Task Compile_LocalReference_UsesExposedPublicInstanceMethods()
    {
        const string child = """
            <input @ref="input" />
            @code {
                private ECMAScript.HTMLInputElement? input;
                [Expose] public void Focus() => input?.Focus();
                public void Hidden() { }
            }
            """;
        const string parent = """
            <Child @ref="child" />
            <button @onclick="Focus">Focus</button>
            @code {
                private ChildRef? child;
                private void Focus() => child?.Focus();
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", parent), new("Child.jazor", child)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var modules = result.Modules.Select(module => module.Content).ToArray();
        StringAssert.Contains(modules[0], "\"ref\"");
        StringAssert.Contains(modules[1], "\"expose\"");
        StringAssert.Contains(modules[1], "\"Focus\": $joltState[\"Focus\"][\"bind\"]($joltState)");
        Assert.IsFalse(modules[1].Contains("export function Focus", StringComparison.Ordinal));

        var invalid = await JoltProjectCompiler.CompileAsync(
            [new("App.jazor", parent.Replace("child?.Focus()", "child?.Hidden()")), new("Child.jazor", child)]);
        Assert.IsTrue(invalid.Diagnostics.Any(diagnostic => diagnostic.Id == "CS1061"));
    }

    [DataRow("ECMAScript.HTMLTextAreaElement?", "CS0029")]
    [DataRow("ECMAScript.HTMLInputElement", "JOLT1028")]
    [TestMethod]
    public async Task Compile_DomReference_ReportsIncompatibleOrNonNullableTarget(string type, string id)
    {
        var source = $$"""
            <input @ref="target" />
            @code { private {{type}} target; }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)]);
        Assert.IsEmpty(result.Modules);
        Assert.IsTrue(result.Diagnostics.Any(diagnostic => diagnostic.Id == id),
            string.Join(Environment.NewLine, result.Diagnostics));
    }
}
