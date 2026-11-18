using System.Text.Json;
using Jolt.Compiler;
using Jolt.Npm;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class ProjectCompilerTests
{
    [TestMethod]
    public async Task Compile_LongProp_PreservesExactIntegerBeyondDoublePrecision()
    {
        var result = await JoltProjectCompiler.CompileAsync([
            new("App.jazor", """<Counter Count="9007199254740993" />"""),
            new("Counter.jazor", "@code { [Parameter] public long Count { get; set; } }"),
        ]);
        Assert.AreEqual(0, result.Diagnostics.Count, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules.Single(m => m.Name == "App").Content, "9007199254740993n");
    }

    [TestMethod]
    public async Task Compile_RendersReactivityAndLocalComponentBindings()
    {
        var result = await JoltProjectCompiler.CompileAsync([
            new("App.jazor", """
                <main>
                    <p>@Count</p>
                    @if (Count > 0) { <i>positive</i> }
                    @foreach (var number in new int[] { 2, 3 }) { <span>@number</span> }
                    <Counter Count="@Count" OnChange="@Update">
                        <template #header><b>Header</b></template>
                        Increase
                    </Counter>
                </main>
                @code {
                    private int Count;
                    private void Update(int value) => Count = value;
                }
                """),
            new("Counter.jazor", """
                <button @onclick="@Click"><slot name="header" /><slot /></button>
                @code {
                    [Parameter, EditorRequired] public int Count { get; set; }
                    [Parameter] public Action<int>? OnChange { get; set; }
                    private void Click() => OnChange?.Invoke(Count + 1);
                }
                """),
        ]);

        Assert.AreEqual(0, result.Diagnostics.Count, string.Join(Environment.NewLine, result.Diagnostics));
        var parent = result.Modules.Single(module => module.Name == "App");
        var child = result.Modules.Single(module => module.Name == "Counter");
        StringAssert.Contains(parent.Content, "from \"./Counter.mjs\"");
        StringAssert.Contains(parent.Content, "if (");
        StringAssert.Contains(parent.Content, "for (");
        StringAssert.Contains(parent.Content, "\"header\"");
        StringAssert.Contains(child.Content, "$joltReactive(new __StateCounter");
        StringAssert.Contains(child.Content, "\"onClick\"");
        StringAssert.Contains(child.Content, "\"$joltProps\"");
        using var sourceMap = JsonDocument.Parse(parent.SourceMap);
        Assert.AreEqual("App.jazor", sourceMap.RootElement.GetProperty("sources")[0].GetString());
        Assert.IsFalse(string.IsNullOrEmpty(sourceMap.RootElement.GetProperty("mappings").GetString()));
    }

    [TestMethod]
    public async Task Compile_UsesRoslynInferredLocalSlotPayloadAndEventSignatures()
    {
        var result = await JoltProjectCompiler.CompileAsync([
            new("App.jazor", """
                <Child OnChange="@Update">
                    <template #header context="scope"><span>@(scope.value + 1)</span></template>
                </Child>
                @code { private void Update(int count) { } }
                """),
            new("Child.jazor", """
                @foreach (var number in new[] { 2, 3 }) { <slot name="header" value="@number" /> }
                @code { [Parameter] public Action<int>? OnChange { get; set; } }
                """),
        ]);

        Assert.AreEqual(0, result.Diagnostics.Count, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules[0].Content, "scope[\"value\"] + 1");
        StringAssert.Contains(result.Modules[1].Content, "\"value\": number");
    }

    [TestMethod]
    public async Task Compile_UsesNpmPublicExportAndValidatesSourcePropTypes()
    {
        var definition = new NpmDefinition("index.d.ts", 0, 1);
        var npm = new NpmVueComponent("reka-ui", "CollapsibleRoot", "reka-ui", "1.0.0", "index.mjs", "index.d.ts", definition,
            new Dictionary<string, NpmVueProp> { ["open"] = new("boolean", true, [], definition) },
            new Dictionary<string, NpmVueEvent>(), new Dictionary<string, NpmVueSlot>());
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", """
            @using Npm.RekaUi
            <CollapsibleRoot Open="@(Count > 0)" />
            @code { private int Count = 1; }
            """)], new(NpmComponents: [npm]));

        Assert.AreEqual(0, result.Diagnostics.Count, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules.Single().Content, "CollapsibleRoot as");
        StringAssert.Contains(result.Modules.Single().Content, "from \"reka-ui\"");
        StringAssert.Contains(result.Modules.Single().Content, "\"open\"");

        var invalid = await JoltProjectCompiler.CompileAsync([new("App.jazor", """
            @using Npm.RekaUi
            <CollapsibleRoot Open="@(42)" />
            """)], new(NpmComponents: [npm]));
        Assert.AreEqual(0, invalid.Modules.Count);
        var diagnostic = invalid.Diagnostics.Single();
        Assert.AreEqual("CS1503", diagnostic.Id);
        Assert.AreEqual("App.jazor", diagnostic.Location.GetLineSpan().Path);
        Assert.AreEqual(1, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
    }

    [TestMethod]
    public async Task Compile_ValidatesNpmEventAndScopedSlot()
    {
        var definition = new NpmDefinition("index.d.ts", 0, 1);
        var npm = new NpmVueComponent("menu", "Menu", "menu", "1.0.0", "index.mjs", "index.d.ts", definition,
            new Dictionary<string, NpmVueProp>
            {
                ["mode"] = new("\"light\" | \"dark\"", false, ["light", "dark"], definition),
                ["item"] = new("SomeExternalObject", false, [], definition),
            },
            new Dictionary<string, NpmVueEvent> { ["update:open"] = new([[new("value", "boolean")]], definition) },
            new Dictionary<string, NpmVueSlot> { ["default"] = new(false, [[new("scope", "MenuScope",
                new Dictionary<string, string> { ["open"] = "boolean" })]], definition) });
        var options = new JoltCompileOptions(NpmComponents: [npm]);
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", """
            @using Npm.Menu
            <Menu Mode="light" @onupdate:open="@Toggle">
                <template #default context="scope">@(!scope.open)</template>
            </Menu>
            @code { private void Toggle(bool value) { } }
            """)], options);
        Assert.AreEqual(0, result.Diagnostics.Count, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules.Single().Content, "\"onUpdate:open\": this.Toggle.bind(this)");
        StringAssert.Contains(result.Modules.Single().Content, "!scope[\"open\"]");

        var invalid = await JoltProjectCompiler.CompileAsync([new("App.jazor", """
            @using Npm.Menu
            <Menu Mode="wrong" Item="@(42)" @onupdate:open="@Toggle" />
            @code { private void Toggle(string value) { } }
            """)], options);
        CollectionAssert.AreEquivalent(new[] { "JOLT1006", "JOLT1014", "CS1503" }, invalid.Diagnostics.Select(diagnostic => diagnostic.Id).ToArray());
    }
}
