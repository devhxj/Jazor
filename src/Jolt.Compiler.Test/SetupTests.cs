namespace Jolt.Compiler.Test;

[TestClass]
public sealed class SetupTests
{
    [TestMethod]
    public async Task Compile_SetupRunsAfterPropsAndSlotsAndKeepsTypedRefs()
    {
        const string source = """
            @using ECMAScript
            <p>@Count.Value</p>
            <p>@field.Value</p>
            <slot />
            @code {
                [Parameter] public int Start { get; set; }
                private Vue.IVueRef<int> Count { get; set; } = null!;
                private Vue.IVueRef<int> field = null!;
                [Setup]
                private void Configure() {
                    Count = Vue.Ref(Start);
                    field = Count;
                    Vue.OnScopeDispose(() => Count.Value = 0);
                }
            }
            """;

        var result = await JoltProjectCompiler.CompileAsync([new("Counter.jazor", source)]);

        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var module = result.Modules.Single().Content;
        var props = module.IndexOf("""$joltState["$joltProps"] =""", StringComparison.Ordinal);
        var slots = module.IndexOf("""$joltState["$joltSlots"] =""", StringComparison.Ordinal);
        var setup = module.IndexOf("""$joltState["Configure"]()""", StringComparison.Ordinal);
        var render = module.IndexOf("""["__JoltRender"]["call"]($setup["joltState"])""", StringComparison.Ordinal);
        Assert.IsTrue(props >= 0 && props < slots && slots < setup && setup < render, module);
        StringAssert.Contains(module, "shallowReactive as $joltShallowReactive");
        StringAssert.Contains(module, """$joltShallowReactive($joltToRaw(this))["$jazor$private$field$""");
        StringAssert.Contains(module, """] = this.Count""");
        StringAssert.Contains(module, "onScopeDispose");
        StringAssert.Contains(module, ".value");
    }

    [TestMethod]
    [DataRow("[Setup] public void Configure() {}", "Setup")]
    [DataRow("[Setup] private static void Configure() {}", "Setup")]
    [DataRow("[Setup] private async void Configure() {}", "Setup")]
    [DataRow("[Setup] private int Configure() => 0;", "Setup")]
    [DataRow("[Setup] private void Configure(int arg = 0) {}", "Setup")]
    [DataRow("[Setup] private void Configure<T>() {}", "Setup")]
    [DataRow("[Setup] private void Configure() {} [Setup] private void Other() {}", "Setup")]
    public void Analyze_InvalidSetupReportsAuthoredSpan(string declaration, string expected)
    {
        var source = "<p />\n@code { " + declaration + " }";
        var result = JoltProjectCompiler.Analyze([new("Counter.jazor", source)]);

        var diagnostic = result.Diagnostics.Single(diagnostic => diagnostic.Id == "JOLT1038");
        Assert.AreEqual("Counter.jazor", diagnostic.Location.GetLineSpan().Path);
        Assert.AreEqual(expected, source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
    }
}
