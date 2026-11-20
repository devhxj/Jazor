using System.Text.Json;
using System.Text.RegularExpressions;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class TemplateStatementTests
{
    [TestMethod]
    public async Task Compile_TemplateIf_StabilizesFollowingSiblingsAndPreservesLoopKeysAndTransfers()
    {
        const string source = """
            <div>
                @if (Show) { <b>One</b><b>Two</b> }
                <em>After</em>
                @foreach (var row in new[] { "a", "b" }) {
                    if (Show) {
                        <span @key="row">@row</span>
                        continue;
                    }
                }
            </div>
            @code { private bool Show = true; }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        string module = result.Modules.Single().Content;
        Assert.AreEqual(2, Regex.Matches(module, @"\$joltH\(\$joltFragment").Count, module);
        StringAssert.Contains(module, "\"key\": row");
        Assert.IsTrue(module.IndexOf("$joltH(\"span\"", StringComparison.Ordinal) <
            module.IndexOf("continue;", StringComparison.Ordinal), module);
    }

    [TestMethod]
    public async Task Compile_TemplateStatements_KeepsControlFlowAndLocalEvaluationInRender()
    {
        const string source = """
            @{ var count = CountRows(); }
            <ul>
            @for (var index = 0; index < count; index++) {
                var label = Label(index);
                switch (index) {
                    case 0:
                        continue;
                    case 1:
                    case 2:
                    {
                        var text = label;
                        <li>@text</li>
                        break;
                    }
                    default:
                        <li>@label</li>
                        break;
                }
            }
            </ul>
            @code {
                private int CountRows() => 4;
                private string Label(int index) => "row " + index;
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var module = result.Modules.Single();
        Assert.AreEqual(1, Regex.Matches(module.Content, @"this\.CountRows\(").Count);
        Assert.AreEqual(1, Regex.Matches(module.Content, @"this\.Label\(").Count);
        StringAssert.Contains(module.Content, "for (");
        StringAssert.Contains(module.Content, "switch (");
        StringAssert.Contains(module.Content, "continue;");
        StringAssert.Contains(module.Content, "break;");
        StringAssert.Contains(module.Content, "$joltDisplay(text)");
        using var sourceMap = JsonDocument.Parse(module.SourceMap);
        Assert.AreEqual("App.jazor", sourceMap.RootElement.GetProperty("sources")[0].GetString());
    }

    [TestMethod]
    public async Task Compile_TemplateStatements_DiscoversSlotInsideSwitchBlock()
    {
        const string host = """
            @switch (0) {
                default:
                {
                    <slot name="main" />
                    break;
                }
            }
            """;
        const string app = """
            <Host><template #main><strong>Content</strong></template></Host>
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", app), new("Host.jazor", host)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules.Single(module => module.Name == "Host").Content, "\"main\"");
    }

    [TestMethod]
    public async Task Compile_TemplateStatements_BindsSwitchPatternVariableInMarkup()
    {
        const string source = """
            @switch (Value()) {
                case string label when label.Length > 0:
                    <strong>@label</strong>
                    break;
                default:
                    <span>Empty</span>
                    break;
            }
            @code {
                private object Value() => "ready";
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        string module = result.Modules.Single().Content;
        Assert.AreEqual(1, Regex.Matches(module, @"this\.Value\(").Count);
        StringAssert.Contains(module, "$joltDisplay(label)");
    }

    [DataRow("@for (var index = 0; index < 1; index++) { <b>@index</b> } <b>@index</b>", "CS0103", "index")]
    [DataRow("@{ var title = Missing; } <b>@title</b>", "CS0103", "Missing")]
    [TestMethod]
    public async Task Compile_TemplateStatements_MapsRoslynDiagnosticsToAuthoringSource(string source, string id, string text)
    {
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)]);
        Assert.IsEmpty(result.Modules);
        var diagnostic = result.Diagnostics.FirstOrDefault(diagnostic => diagnostic.Id == id);
        Assert.IsNotNull(diagnostic, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.AreEqual(source.LastIndexOf(text, StringComparison.Ordinal), diagnostic.Location.SourceSpan.Start);
        Assert.AreEqual(text.Length, diagnostic.Location.SourceSpan.Length);
    }
}
