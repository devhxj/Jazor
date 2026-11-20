using Jolt.Npm;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class NpmOverloadTests
{
    [TestMethod]
    public async Task Compile_OverloadsKeepDistinctOptionsAndResultShapes()
    {
        const string source = """
            @using Tool = Npm.Tool.Exports
            @code {
                [Setup]
                private void Configure() {
                    var first = Tool.Format(new Tool.FormatOptionsInput { Prefix = "demo" });
                    var second = Tool.Format(new Tool.FormatOptionsInput_2 { Digits = 2 });
                    var detailed = Tool.Format(new Tool.FormatOptionsInput { Prefix = "test" }, true);
                    string text = first.Text + second.Output + detailed.Text;
                }
            }
            """;
        var text = new NpmValueType("string");
        var number = new NpmValueType("number");
        var firstOptions = new NpmValueType("object", Members: [new("prefix", text, false)]);
        var secondOptions = new NpmValueType("object", Members: [new("digits", number, false)]);
        var firstResult = new NpmValueType("object", Members: [new("text", text, true)]);
        var secondResult = new NpmValueType("object", Members: [new("output", text, true)]);
        var export = new NpmValueExport("tool", "format", "tool", "1.0.0", "index.js", "index.d.ts", null,
            [new([new("options", firstOptions, false)], firstResult),
             new([new("options", secondOptions, false)], secondResult),
             new([new("options", firstOptions with { Members = [.. firstOptions.Members!] }, false),
                  new("detailed", new("boolean"), true)], firstResult)], null);

        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], new() { NpmValues = [export] });

        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "format as ");
        StringAssert.Contains(js, ".text");
        StringAssert.Contains(js, ".output");
    }
}
