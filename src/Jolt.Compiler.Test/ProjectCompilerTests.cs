using System.Text.Json;
using Jolt.Compiler;
using Jolt.Npm;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class ProjectCompilerTests
{
    [TestMethod]
    public async Task Compile_BindsGenericArrayCallbackAndSlotToTheSameType()
    {
        const string list = """
            @typeparam T
            <div>
                @foreach (var item in Items) {
                    <slot name="item" value="@item" />
                    <button @onclick="() => Choose(item)">Select</button>
                }
            </div>
            @code {
                [Parameter, EditorRequired] public T[] Items { get; set; } = [];
                [Parameter, EditorRequired] public Action<T> Choose { get; set; } = default!;
            }
            """;
        const string app = """
            <TypedList Items="@Items" Choose="@Pick">
                <template #item context="row"><p>@row.value.Length</p></template>
            </TypedList>
            @code { private string[] Items = ["one"]; private void Pick(string item) { } }
            """;
        var namedChild = app.Replace("<template #item context=", "<Item Context=").Replace("</template>", "</Item>");
        foreach (var source in new[] { app, app.Replace("<TypedList ", "<TypedList T=\"string\" "), namedChild })
        {
            var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source), new("TypedList.jazor", list)]);
            Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
            StringAssert.Contains(result.Modules[0].Content, "row[\"value\"].length");
            StringAssert.Contains(result.Modules[1].Content, "new __StateTypedList");
            Assert.IsFalse(result.Modules[0].Content.Contains("__JoltBindType", StringComparison.Ordinal));
        }
        foreach (var (source, expected) in new[]
        {
            (app.Replace("Pick(string", "Pick(bool"), "CS1503"),
            (app.Replace("value.Length", "value.Missing"), "CS1061"),
            (app.Replace("Items=\"@Items\"", "Items=\"42\""), "JOLT1021"),
            (app.Replace("<TypedList ", "<TypedList T=\"bool\" "), "CS1503"),
        })
        {
            var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source), new("TypedList.jazor", list)]);
            Assert.IsEmpty(result.Modules);
            Assert.IsTrue(result.Diagnostics.Any(diagnostic => diagnostic.Id == expected && diagnostic.Location.SourceSpan.Length > 0),
                string.Join(Environment.NewLine, result.Diagnostics));
        }
    }

    [TestMethod]
    public async Task Compile_ResolvesDefaultAndExplicitNamedChildrenAndReportsAmbiguity()
    {
        const string panel = """<section><slot name="header" /><slot /></section>""";
        const string source = """<Panel><Header><h1>Header</h1></Header><ChildContent><p>Body</p></ChildContent></Panel>""";
        var inputs = new[] { new JoltSource("App.jazor", source), new JoltSource("Panel.jazor", panel) };
        var result = await JoltProjectCompiler.CompileAsync(inputs);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules[0].Content, "h1");
        StringAssert.Contains(result.Modules[0].Content, "Body");

        result = await JoltProjectCompiler.CompileAsync([.. inputs, new("Header.jazor", "<strong>Component</strong>")]);
        var diagnostic = result.Diagnostics.Single();
        StringAssert.Contains(diagnostic.GetMessage(), "both a child component and a slot");
        Assert.AreEqual("Header", source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
    }

    [TestMethod]
    public async Task Compile_ScopedCssUsesAStablePerModuleIdAndSideEffectImport()
    {
        const string source = "<p class=\"label\">Text</p>\n<style scoped>\n@media (width > 4px) { .label { color: red; } }\n</style>";
        var result = await JoltProjectCompiler.CompileAsync([new("Panel.jazor", source), new("Other/Panel.jazor", source)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        foreach (var module in result.Modules)
        {
            var css = module.ScopedCss!;
            var scope = css.Split(' ')[1];
            StringAssert.Contains(module.Content, $"\"__scopeId\": \"{scope}\"");
            StringAssert.Contains(module.Content, "import \"./Panel.jolt.css\";");
            StringAssert.Contains(css, "@media (width > 4px)");
            Assert.IsFalse(module.Content.Contains("$joltH(\"style\"", StringComparison.Ordinal));
        }
        Assert.AreNotEqual(result.Modules[0].ScopedCss, result.Modules[1].ScopedCss);
    }

    [TestMethod]
    public async Task Compile_ExportsStaticMembersAndKeepsComponentStatePrivate()
    {
        var result = await JoltProjectCompiler.CompileAsync([new("Counter.jazor", """
            <button @onclick="@Increment">@Count</button>
            @code {
                public static int Step = 2;
                private int Count;
                private static int Double(int value) => value * 2;
                public const int Initial = 3;
                private void Increment() => Count = Advance(Count);
                public static int Advance(int value) => Double(value) + Step + Initial;
            }
            """)]);
        Assert.AreEqual(0, result.Diagnostics.Count, string.Join(Environment.NewLine, result.Diagnostics));
        var module = result.Modules.Single().Content;
        StringAssert.Contains(module, "export let Step = 2");
        StringAssert.Contains(module, "export const Initial = 3");
        StringAssert.Contains(module, "export function Advance(");
        StringAssert.Contains(module, "function Double(");
        Assert.IsFalse(module.Contains("export function Double", StringComparison.Ordinal));
        Assert.IsFalse(module.Contains("export class __StateCounter", StringComparison.Ordinal));
        StringAssert.Contains(module, "export default");
        StringAssert.Contains(module, "$joltReactive(new __StateCounter");
        StringAssert.Contains(module, "Advance(");
    }

    [TestMethod]
    public async Task Compile_ErasesHostInterfaceSharedByStaticExportAndInstance()
    {
        var result = await JoltProjectCompiler.CompileAsync([new("Worker.jazor", """
            @code {
                [ECMAScript.ECMAScript]
                public interface IWorker {
                    [ECMAScript.ECMAScriptName("read")]
                    string Read();
                }
                public static string Read(IWorker worker) => worker.Read();
                private string ReadAgain(IWorker worker) => Read(worker);
            }
            """)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "export function Read(");
        StringAssert.Contains(js, "worker.read()");
        Assert.DoesNotContain("class IWorker", js);
    }

    [TestMethod]
    public async Task Compile_UnnamedElement_ReportsDiagnosticDuringEditing()
    {
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", "<\n@code { private int Count = 1; }")]);
        Assert.IsTrue(result.Diagnostics.Any(diagnostic => diagnostic.Id == "JOLT1017"));
        Assert.IsEmpty(result.Modules);
    }

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
