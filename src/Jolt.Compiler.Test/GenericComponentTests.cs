namespace Jolt.Compiler.Test;

[TestClass]
public sealed class GenericComponentTests
{
    private const string Table = """
        @typeparam TItem where TItem : class
        @typeparam TKey where TKey : struct
        <div>
            @foreach (var item in Items) {
                <slot name="row" item="@item" rowKey="@KeyFor(item)" />
            }
        </div>
        @code {
            [Parameter, EditorRequired] public TItem[] Items { get; set; } = [];
            [Parameter, EditorRequired] public Func<TItem, TKey> KeyFor { get; set; } = default!;
        }
        """;

    private const string App = """
        <TypedTable Items="@Items" KeyFor="@KeyFor">
            <template #row context="row"><p>@(row.item.Length + row.rowKey)</p></template>
        </TypedTable>
        @code { private string[] Items = ["one"]; private int KeyFor(string item) => item.Length; }
        """;

    [TestMethod]
    public async Task Compile_InfersNestedGenericComponentsAndForwardedSlotMembers()
    {
        var result = await JoltProjectCompiler.CompileAsync([
            new("ListView.jazor", """
                @typeparam T
                @foreach (var item in Items) {
                    <slot name="row" value="@item" />
                }
                @code { [Parameter] public T[] Items { get; set; } = []; }
                """),
            new("Forwarder.jazor", """
                <ListView Items="@(new[] { "text" })">
                    <Row Context="row"><slot name="entry" value="@row.value" /></Row>
                </ListView>
                """),
            new("App.jazor", """
                <Forwarder>
                    <Entry Context="entry">
                        <ListView Items="@(new[] { entry.value })">
                            <Row Context="row"><b>@row.value.Length</b></Row>
                        </ListView>
                    </Entry>
                </Forwarder>
                """),
        ]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single(module => module.Name == "App").Content;
        StringAssert.Contains(js, "row[\"value\"].length");
        Assert.DoesNotContain("__JoltBindType", js);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("TItem=\"string\" TKey=\"int\" ")]
    public async Task Compile_MultipleTypeArgumentsSpecializePropsCallbacksAndSlots(string attributes)
    {
        var source = App.Replace("<TypedTable ", "<TypedTable " + attributes, StringComparison.Ordinal);
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source), new("TypedTable.jazor", Table)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var module = result.Modules.Single(module => module.Name == "App").Content;
        StringAssert.Contains(module, "row[\"item\"].length + row[\"rowKey\"]");
        Assert.IsFalse(module.Contains("__JoltBindType", StringComparison.Ordinal));
        Assert.IsFalse(module.Contains("\"TItem\"", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("TItem=\"int\" TKey=\"int\" ", "int")]
    [DataRow("TItem=\"string\" TKey=\"string?\" ", "string?")]
    [DataRow("TItem=\"string\" ", "string")]
    public async Task Analyze_BadConstraintOrMissingTypeArgumentReportsAuthorSpan(string attributes, string authored)
    {
        var source = App.Replace("<TypedTable ", "<TypedTable " + attributes, StringComparison.Ordinal);
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source), new("TypedTable.jazor", Table)]);
        Assert.IsEmpty(result.Modules);
        Assert.IsTrue(result.Diagnostics.Any(diagnostic => diagnostic.Id == "JOLT1021" &&
            diagnostic.Location.GetLineSpan().Path == "App.jazor" &&
            source.AsSpan(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length).SequenceEqual(authored)),
            string.Join(Environment.NewLine, result.Diagnostics));
    }
}
