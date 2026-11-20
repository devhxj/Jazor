using Jolt.Compiler;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class SlotOutletTests
{
    [TestMethod]
    [DataRow("value=\"@Count\"", "@Count")]
    [DataRow("", "slot")]
    [DataRow("value=\"text\" extra=\"yes\"", "extra")]
    public async Task AnalyzeAndCompile_RejectInconsistentConditionalOutlets(string attributes, string authored)
    {
        var source = $$"""
            @if (Count > 0) {
                <slot name="row" value="text" />
            } else {
                <slot name="row" {{attributes}} />
            }
            @code { private int Count = 1; }
            """;
        var sources = new JoltSource[] { new("Table.jazor", source) };
        var diagnostics = JoltProjectCompiler.Analyze(sources).Diagnostics;
        var result = await JoltProjectCompiler.CompileAsync(sources);
        var diagnostic = diagnostics.Single(item => item.Id == "JOLT1037");
        Assert.AreEqual("Table.jazor", diagnostic.Location.GetLineSpan().Path);
        Assert.AreEqual(source.LastIndexOf(authored, StringComparison.Ordinal), diagnostic.Location.SourceSpan.Start);
        Assert.AreEqual(authored.Length, diagnostic.Location.SourceSpan.Length);
        Assert.AreEqual(diagnostic.Location.SourceSpan, result.Diagnostics.Single(item => item.Id == "JOLT1037").Location.SourceSpan);
        Assert.IsEmpty(result.Modules);
    }

    [TestMethod]
    public async Task Compile_AcceptsConditionalOutletsWithTheSameTypedFieldsInDifferentOrder()
    {
        var result = await JoltProjectCompiler.CompileAsync([
            new("Table.jazor", """
                @if (Count > 0) {
                    <slot name="row" value="item @Count" selected />
                } else {
                    <slot name="row" selected="@Selected" value="@Label" />
                }
                @code { private int Count = 1; private bool Selected; private string Label = "text"; }
                """),
            new("App.jazor", """
                <Table><Row Context="row"><p>@row.value.Length @row.selected</p></Row></Table>
                """),
        ]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules.Single(module => module.Name == "App").Content, "row[\"value\"].length");
    }
}
