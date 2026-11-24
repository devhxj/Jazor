using Jazor.Npm;
using Jolt.Npm;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class BindModifierTests
{
    [TestMethod]
    public async Task Compile_BindModifiers_AcceptsTaskAndSynchronousCallbacks()
    {
        const string source = """
            <input @bind:set="async value => await SetName(value)" title="@Before()" @bind:get="Read()" />
            <input @bind="Name" @bind:after="After" />
            <input @bind="Name" @bind:after="SyncAfter" />
            @code {
                private string Name = "";
                private string Before() => Name;
                private string Read() => Name.ToUpper();
                private async System.Threading.Tasks.Task SetName(string value) => Name = value;
                private async System.Threading.Tasks.Task After() => Name = Name.Trim();
                private void SyncAfter() => Name = Name.Trim();
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        string module = result.Modules.Single().Content;
        StringAssert.Contains(module, "$joltAssign($joltModelValue);");
        StringAssert.Contains(module, "return $joltAfter();");
        StringAssert.Contains(module, "await this.SetName(value)");
        Assert.IsTrue(module.IndexOf("this.Before()", StringComparison.Ordinal) < module.IndexOf("this.Read()", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task Compile_BindModifiers_UsesNpmModelContract()
    {
        var definition = new NpmDefinition("index.d.ts", 0, 1);
        var component = new NpmVueComponent("reka-ui", "CollapsibleRoot", "reka-ui", "1.0.0", "index.mjs", "index.d.ts", definition,
            new Dictionary<string, NpmVueProp> { ["open"] = new("boolean", true, [], definition) },
            new Dictionary<string, NpmVueEvent> { ["update:open"] = new([[new("value", "boolean")]], definition) },
            new Dictionary<string, NpmVueSlot>());
        const string source = """
            @using Npm.RekaUi
            <CollapsibleRoot @bind-Open:get="!State" @bind-Open:set="SetState" />
            <CollapsibleRoot @bind-Open:after="After" @bind-Open="State" />
            @code {
                private bool State;
                private void SetState(bool next) => State = !next;
                private void After() { }
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], new(NpmComponents: [component]));
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var module = result.Modules.Single().Content;
        StringAssert.Contains(module, "\"open\": !this.");
        StringAssert.Contains(module, "\"onUpdate:open\":");
        StringAssert.Contains(module, "return $joltAfter();");
    }

    [DataRow("<input @bind:get=\"Name\" />", "@bind:get")]
    [DataRow("<input @bind=\"Name\" @bind:set=\"Set\" />", "@bind:set")]
    [DataRow("<input @bind:get=\"Name\" @bind:set=\"Set\" @bind:after=\"After\" />", "@bind:after")]
    [DataRow("<input @bind=\"Name\" @bind:after />", "@bind:after")]
    [TestMethod]
    public async Task Compile_BindModifiers_ReportsInvalidPairsAtAuthoredDirective(string markup, string token)
    {
        string source = markup + "\n@code { private string Name = \"\"; private void Set(string value) => Name = value; private void After() { } }";
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)]);
        Assert.IsEmpty(result.Modules);
        var diagnostic = result.Diagnostics.FirstOrDefault(item => item.Id == "JOLT1023");
        Assert.IsNotNull(diagnostic, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.AreEqual(token, source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
    }
}
