namespace Jolt.Compiler.Test;

[TestClass]
public sealed class NativeFormBindingTests
{
    [TestMethod]
    public async Task Compile_SplitTextBindingRestoresTheGetterAfterCallingTheSetter()
    {
        const string source = """
            <input @bind:get="Name" @bind:set="Reject" @bind:event="oninput" />
            @code {
                private string Name = "original";
                private void Reject(string value) { }
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var module = result.Modules.Single().Content;
        StringAssert.Contains(module, "$joltAssign($joltValue)");
        StringAssert.Contains(module, "String($joltGetter() ?? \"\")");
        StringAssert.Contains(module, "$joltInput[\"value\"] = $joltCurrent");
    }

    [TestMethod]
    public async Task Compile_RadioSelectAndNumberUseTheirTypedVueUpdates()
    {
        const string source = """
            <div>
                <input type="radio" value="one" @bind="Selection" />
                <select @bind="Selection"><option value="one">One</option></select>
                <select multiple="false" @bind="Selections"><option value="@Selection">Current</option></select>
                <input type="number" @bind="Amount" @bind:event="oninput" />
            </div>
            @code {
                private string Selection = "one";
                private string[] Selections = ["one"];
                private double? Amount = 1.5;
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var module = result.Modules.Single().Content;
        StringAssert.Contains(module, "vModelRadio as $joltModelRadio");
        StringAssert.Contains(module, "vModelSelect as $joltModelSelect");
        StringAssert.Contains(module, "vModelText as $joltModelText");
        StringAssert.Contains(module, "\"multiple\", true");
        StringAssert.Contains(module, "Number[\"isFinite\"]($joltNumber) ? $joltNumber : null");
    }

    [TestMethod]
    [DataRow("<input type=\"radio\" value=\"@(42)\" @bind=\"Selection\" />")]
    [DataRow("<select multiple @bind=\"Selection\"><option>One</option></select>")]
    [DataRow("<select @bind=\"Selection\"><option value=\"@(42)\">One</option></select>")]
    [DataRow("<select multiple=\"@(true)\" @bind=\"Selection\"><option>One</option></select>")]
    [DataRow("<input type=\"number\" @bind=\"Selection\" />")]
    public async Task Compile_InvalidFormTypesReportAuthoredMarkup(string source)
    {
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source + "\n@code { private string Selection = \"one\"; }")]);
        Assert.IsEmpty(result.Modules);
        Assert.IsTrue(result.Diagnostics.Any(diagnostic => diagnostic.Location.SourceSpan is { Start: > 0, Length: > 0 } span && span.End < source.Length),
            string.Join(Environment.NewLine, result.Diagnostics));
    }
}
