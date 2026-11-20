using Jolt.Compiler;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class KeyDirectiveTests
{
    [TestMethod]
    public async Task Compile_KeyOnElementAndComponent_EmitsBoundVNodeKey()
    {
        var result = await JoltProjectCompiler.CompileAsync([
            new("Parent.jazor", """
                @foreach (var id in new[] { 1, 2 }) {
                    <div @key="id"><Child @key="@Caption" /></div>
                }
                @code { private string Caption = "row"; }
                """),
            new("Child.jazor", "<b>child</b>"),
        ]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single(module => module.Name == "Parent").Content;
        StringAssert.Contains(js, "\"key\": id");
        Assert.AreEqual(2, js.Split("\"key\":").Length - 1);
        Assert.DoesNotContain("@key", js);
        Assert.DoesNotContain("PropertyKeyRef", js);
    }

    [TestMethod]
    [DataRow("<li key='one' @key='1' />", "JOLT1003")]
    [DataRow("<li @key='1' key='one' />", "JOLT1003")]
    [DataRow("<li @key />", "JOLT1025")]
    [DataRow("<li @key='new object()' />", "CS1503")]
    [DataRow("<slot @key='1' />", "JOLT1022")]
    public void Analyze_InvalidKey_ReportsAtTheAuthorLocation(string markup, string expected)
    {
        var source = new JoltSource("Item.jazor", markup);
        var result = JoltProjectCompiler.Analyze([source]);
        Assert.IsTrue(result.Diagnostics.Any(diagnostic => diagnostic.Id == expected &&
            diagnostic.Location.GetLineSpan().Path == source.Path && diagnostic.Location.SourceSpan.Length > 0),
            string.Join(Environment.NewLine, result.Diagnostics));
    }
}
