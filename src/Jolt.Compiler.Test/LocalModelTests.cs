namespace Jolt.Compiler.Test;

[TestClass]
public sealed class LocalModelTests
{
    [TestMethod]
    public async Task Compile_LocalModel_OffersVueEventAndPreservesLegacyCallback()
    {
        const string child = """
            @typeparam T where T : class
            <button @onclick="Change">@Value</button>
            @code {
                [Parameter, EditorRequired] public T Value { get; set; }
                [Parameter, EditorRequired] public Action<T>? ValueChanged { get; set; }
                private void Change() => ValueChanged?.Invoke(Value);
            }
            """;
        const string app = """
            <Child @bind-Value="Name" />
            <Child @bind-Value:get="Name.Trim()" @bind-Value:set="SetName" />
            <Child Value="@Name" ValueChanged="@SetName" />
            @code {
                private string Name = "a";
                private void SetName(string value) => Name = value;
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", app), new("Child.jazor", child)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        string parentModule = result.Modules.Single(module => module.Name == "App").Content;
        StringAssert.Contains(parentModule, "\"onUpdate:value\"");
        StringAssert.Contains(parentModule, "\"ValueChanged\"");
        string childModule = result.Modules.Single(module => module.Name == "Child").Content;
        StringAssert.Contains(childModule, "\"emits\": [\"update:value\"]");
        StringAssert.Contains(childModule, "\"value\"");
        StringAssert.Contains(childModule, "\"$joltEmit\"");
    }

    [TestMethod]
    public async Task Compile_LocalModel_ReportsMismatchedCallbackAtBinding()
    {
        const string child = """
            @code {
                [Parameter] public string Value { get; set; } = "";
                [Parameter] public Action<int>? ValueChanged { get; set; }
            }
            """;
        const string app = """
            <Child @bind-Value="Name" />
            @code { private string Name = ""; }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", app), new("Child.jazor", child)]);
        Assert.IsEmpty(result.Modules);
        var diagnostic = result.Diagnostics.Single(item => item.Id == "JOLT1023");
        Assert.AreEqual("@bind-Value", app.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
    }
}
