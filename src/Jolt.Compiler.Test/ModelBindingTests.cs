using Jolt.Npm;

namespace Jolt.Compiler.Test;

[TestClass]
public class ModelBindingTests
{
    [TestMethod]
    public async Task Compile_NativeModelsUseVueDirectivesForChangeInputAndCheckbox()
    {
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", """
            <div>
                <input @bind="Name" />
                <textarea @bind:event="oninput" @bind="Name"></textarea>
                <input type="checkbox" @bind="Open" />
            </div>
            @code { private string Name = ""; private bool Open; }
            """)]);

        Assert.AreEqual(0, result.Diagnostics.Count, string.Join(Environment.NewLine, result.Diagnostics));
        var module = result.Modules.Single().Content;
        StringAssert.Contains(module, "vModelText as $joltModelText");
        StringAssert.Contains(module, "vModelCheckbox as $joltModelCheckbox");
        StringAssert.Contains(module, "withDirectives as $joltWithDirectives");
        StringAssert.Contains(module, "\"lazy\": true");
        StringAssert.Contains(module, "\"onUpdate:modelValue\"");
        Assert.IsFalse(module.Contains("@bind", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task Compile_NpmModelUsesPublishedPropAndEventAndKeepsSetterDeferred()
    {
        var definition = new NpmDefinition("index.d.ts", 0, 1);
        var npm = new NpmVueComponent("reka-ui", "CollapsibleRoot", "reka-ui", "1.0.0", "index.mjs", "index.d.ts", definition,
            new Dictionary<string, NpmVueProp> { ["open"] = new("boolean", true, [], definition) },
            new Dictionary<string, NpmVueEvent> { ["update:open"] = new([[new("value", "boolean")]], definition) },
            new Dictionary<string, NpmVueSlot>());
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", """
            @using Npm.RekaUi
            <CollapsibleRoot @bind-Open="GetState()[0]" />
            @code {
                private bool[] state = [false];
                private bool[] GetState() => state;
            }
            """)], new(NpmComponents: [npm]));

        Assert.AreEqual(0, result.Diagnostics.Count, string.Join(Environment.NewLine, result.Diagnostics));
        var module = result.Modules.Single().Content;
        StringAssert.Contains(module, "\"open\": this.GetState()[0]");
        StringAssert.Contains(module, "\"onUpdate:open\": ");
        StringAssert.Contains(module, "this.GetState()[0] = ");
        Assert.IsFalse(module.Contains("@bind", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("<input @bind=\"Name\" />", "private string Name => \"\";", "Name")]
    [DataRow("<input @bind=\"Name\" />", "private object Name = \"\";", "Name")]
    [DataRow("<input type=\"checkbox\" @bind=\"Name\" />", "private string Name = \"\";", "Name")]
    [DataRow("<textarea @bind=\"Name\" @bind:event=\"onblur\"></textarea>", "private string Name = \"\";", "onblur")]
    [DataRow("<input @bind:event=\"oninput\" />", "private string Name = \"\";", "@bind:event")]
    [DataRow("<input @bind=\"Name\" Value=\"other\" />", "private string Name = \"\";", "Value")]
    [DataRow("<input @bind=\"Name\" />", "[Parameter] public string Name { get; set; } = \"\";", "Name")]
    public async Task Compile_InvalidModelsReportAuthoredSpan(string markup, string members, string expectedSpan)
    {
        var source = markup + "\n@code { " + members + " }";
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)]);

        Assert.AreEqual(0, result.Modules.Count);
        Assert.IsTrue(result.Diagnostics.Any(diagnostic => diagnostic.Location.GetLineSpan().Path == "App.jazor" &&
            source.AsSpan(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length).SequenceEqual(expectedSpan)),
            string.Join(Environment.NewLine, result.Diagnostics));
    }
}
