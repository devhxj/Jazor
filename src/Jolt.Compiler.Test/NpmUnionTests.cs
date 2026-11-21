using Jolt.Npm;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class NpmUnionTests
{
    [TestMethod]
    public async Task Compile_SharesAndErasesClosedMutableArrayElementTypes()
    {
        const string source = """
            @using DateFns = Npm.DateFns.Exports
            <p>@Current</p>
            @code {
                private double Current {
                    get {
                        ECMAScript.Number stamp = 1700000000000d;
                        var choices = new DateFns.DateOrNumberOrString[] {
                            new ECMAScript.Date(1710000000000d), stamp, "2025-01-01"
                        };
                        return DateFns.Min(choices).GetDate() + DateFns.Max(choices).GetDate();
                    }
                }
            }
            """;
        var date = new NpmValueType("date");
        var number = new NpmValueType("number");
        var text = new NpmValueType("string");
        NpmValueExport Export(string name, params NpmValueType[] cases) =>
            new("date-fns", name, "date-fns", "1.0.0", "index.js", "index.d.ts", null,
                [new([new("choices", new("array", ElementType: new("union", Variants: cases)), false)], date)], null);
        var options = new JoltCompileOptions { NpmValues = [Export("min", date, text, number), Export("max", number, text, date)] };
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);

        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "min as ");
        StringAssert.Contains(js, "max as ");
        StringAssert.Contains(js, """[new Date(1710000000000), stamp, "2025-01-01"]""");
        Assert.DoesNotContain("DateOrNumberOrString", js);

        var wrongElement = source.Replace(", stamp,", ", true,");
        var dateArray = source.Replace("DateFns.DateOrNumberOrString[]", "ECMAScript.Date[]")
            .Replace(", stamp, \"2025-01-01\"", "");
        var invalidElement = JoltProjectCompiler.Analyze([new("App.jazor", wrongElement)], options);
        Assert.AreEqual("CS0029", invalidElement.Diagnostics.Single().Id);
        var withoutNumber = new JoltCompileOptions { NpmValues = [Export("min", date, text), Export("max", text, date)] };
        var invalidHostNumber = JoltProjectCompiler.Analyze(
            [new("App.jazor", source.Replace("DateOrNumberOrString", "DateOrString", StringComparison.Ordinal))],
            withoutNumber);
        Assert.AreEqual("CS0029", invalidHostNumber.Diagnostics.Single().Id);
        var invalidArray = JoltProjectCompiler.Analyze([new("App.jazor", dateArray)], options);
        Assert.HasCount(2, invalidArray.Diagnostics);
        Assert.IsTrue(invalidArray.Diagnostics.All(diagnostic => diagnostic.Id == "CS1503"));
    }
}
