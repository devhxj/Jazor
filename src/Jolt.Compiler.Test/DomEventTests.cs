namespace Jolt.Compiler.Test;

[TestClass]
public sealed class DomEventTests
{
    [TestMethod]
    public async Task Compile_DomEvent_ProjectsSynchronousAndTaskHandlersWithWebIdlTypes()
    {
        const string source = """
            <form @onsubmit="Submit" @onsubmit:preventDefault>
                <input @onkeydown="e => Key = e.Key" />
                <button @onclick="Click" @onclick:stopPropagation="@ShouldStop">Save</button>
                <button @onclick="Reset">Reset</button>
                <button @onclick="async () => await Save()">Save asynchronously</button>
                <button @onclick="async e => await Save(e.ClientX)">Save pointer position</button>
            </form>
            @code {
                private string Key = "";
                private double X;
                private bool ShouldStop = true;
                private void Click(ECMAScript.MouseEvent e) => X = e.ClientX;
                private void Reset() => X = 0;
                private async System.Threading.Tasks.Task Save(double x = 0) => X = x;
                private async System.Threading.Tasks.Task Submit(ECMAScript.SubmitEvent e)
                {
                    Key = e.Type;
                }
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        string module = result.Modules.Single().Content;
        StringAssert.Contains(module, "e.key");
        StringAssert.Contains(module, "e.clientX");
        StringAssert.Contains(module, "e.type");
        StringAssert.Contains(module, ".preventDefault()");
        StringAssert.Contains(module, ".stopPropagation()");
        StringAssert.Contains(module, "return $joltHandler($joltEvent)");
    }

    [TestMethod]
    public async Task Compile_DomEvent_ModifierWithoutHandlerEmitsListener()
    {
        const string source = "<form @onsubmit:preventDefault></form>";
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        string module = result.Modules.Single().Content;
        StringAssert.Contains(module, "\"onSubmit\"");
        StringAssert.Contains(module, ".preventDefault()");
    }

    [DataRow("<input @onkeydown=\"e => e.ClientX\" />", "CS1061", "ClientX")]
    [DataRow("<form @onsubmit:preventDefault=\"1\"></form>", "CS0029", "1")]
    [DataRow("<form @onsubmit:preventDefault @onsubmit:preventDefault></form>", "JOLT1024", "@onsubmit:preventDefault")]
    [DataRow("<form @onsubmit=\"\"></form>", "JOLT1024", "@onsubmit")]
    [DataRow("<form @onsubmit:capture=\"Submit\"></form>", "JOLT1024", "@onsubmit:capture")]
    [TestMethod]
    public async Task Compile_DomEvent_ReportsInvalidEventsInAuthoringSource(string source, string id, string text)
    {
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)]);
        Assert.IsEmpty(result.Modules);
        var diagnostic = result.Diagnostics.FirstOrDefault(diagnostic => diagnostic.Id == id);
        Assert.IsNotNull(diagnostic, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.AreEqual(text, source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
    }
}
