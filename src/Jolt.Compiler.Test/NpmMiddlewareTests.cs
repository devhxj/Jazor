using Jolt.Npm;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class NpmMiddlewareTests
{
    [TestMethod]
    public async Task Compile_ReadsNestedOptionalMiddlewareDataAsNullable()
    {
        const string source = """
            @using Floating = Npm.FloatingUi.Dom.Exports
            @code {
                private double Current;
                private void Read(ECMAScript.Element anchor, ECMAScript.HTMLElement popup) {
                    Floating.ComputePosition(anchor, popup).Then(position => {
                        Current = position.MiddlewareData.Arrow?.X ?? position.X;
                    });
                }
            }
            """;
        var number = new NpmValueType("number");
        var arrow = new NpmValueType("object", Members: [new("x", number, true, Optional: true)]);
        var middleware = new NpmValueType("object", Members: [new("arrow", arrow, true, Optional: true)]);
        var position = new NpmValueType("object", Members: [new("x", number, false), new("middlewareData", middleware, true)]);
        var compute = new NpmValueExport("@floating-ui/dom", "computePosition", "@floating-ui/dom", "1.8.0", "index.mjs", "index.d.mts", null,
            [new([new("anchor", new("web", Name: "Element"), false), new("popup", new("web", Name: "HTMLElement"), false)],
                new("promise", ElementType: position))], null);
        var options = new JoltCompileOptions { NpmValues = [compute] };
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, """middlewareData["arrow"] ?? null""");
        StringAssert.Contains(js, """["x"] ?? null""");
        var invalid = JoltProjectCompiler.Analyze([new("App.jazor", source.Replace(".Arrow?.X", """["unknown"]"""))], options);
        Assert.AreEqual("CS0021", invalid.Diagnostics.Single().Id);
    }
}
