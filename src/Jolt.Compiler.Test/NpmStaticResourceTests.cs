using Jazor.Npm;
using Jolt.Compiler;
using Jolt.Npm;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class NpmStaticResourceTests
{
    [TestMethod]
    public async Task Compile_StaticResourceNumericLiterals_ShareClosedWriteDomains()
    {
        var set = NumericWriteMethod("setLocaleMessage", [-1.25, 2, 1e-100]);
        var merge = NumericWriteMethod("mergeLocaleMessage", [1e-100, 2, -1.25]);
        var composer = new NpmValueType("object", Members: [set, merge]);
        var export = new NpmValueExport("resources", "composer", "resources", "1", "index.js", "index.d.ts", null,
            [new([], composer)], null);
        const string source = """
            @using Tool = Npm.Resources.Exports
            @code {
                public static void Write() {
                    var choice = Tool.Choice.ValueMinus1_25;
                    Tool.Composer().SetLocaleMessage(choice);
                    Tool.Composer().MergeLocaleMessage(choice);
                    Tool.Composer().SetLocaleMessage(Tool.Choice.Value2);
                    Tool.Composer().MergeLocaleMessage(Tool.Choice.Value1EMinus100);
                }
            }
            """;
        var options = new JoltCompileOptions { NpmValues = [export] };
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "choice = -1.25");
        StringAssert.Contains(js, "setLocaleMessage(choice)");
        StringAssert.Contains(js, "mergeLocaleMessage(choice)");
        StringAssert.Contains(js, "setLocaleMessage(2)");
        StringAssert.Matches(js, new Regex(@"mergeLocaleMessage\(1[eE]-100\)"));
        Assert.DoesNotContain("ValueMinus", js, StringComparison.Ordinal);

        foreach (var argument in new[] { "0", "2", "new Tool.Choice()" })
        {
            var invalid = source.Replace("SetLocaleMessage(choice)", $"SetLocaleMessage({argument})", StringComparison.Ordinal);
            var rejected = await JoltProjectCompiler.CompileAsync([new("App.jazor", invalid)], options);
            Assert.IsTrue(rejected.Diagnostics.Any(diagnostic => diagnostic.Id is "CS1503" or "CS0122"),
                string.Join(Environment.NewLine, rejected.Diagnostics));
        }

        static NpmValueMember NumericWriteMethod(string name, IReadOnlyList<double> values) =>
            new(name, new("method", Signatures:
                [new([new("choice", new("numberLiterals", Name: "Choice", NumberLiterals: values), false)], new("void"))]), true);
    }

    [TestMethod]
    public async Task Compile_StaticResourceMethods_PreserveReceiverAndExactKeys()
    {
        var str = new NpmValueType("string");
        var number = new NpmValueType("number");
        NpmValueParameter[] list = [new("first", str, false), new("second", str, false)];
        var named = new NpmValueType("object", Members: [new("name", str, false)]);
        var composer = new NpmValueType("object", Members: [
            new("tWelcome", new("method", Signatures: [new([new("named", named, false)], str)]), true,
                JsName: "t", StaticKey: "say\"hello\\world"),
            new("tWelcome", str, true),
            new("tHome", new("method", Signatures: [new([], str)]), true, JsName: "t", StaticKey: "home"),
            new("tList", new("method", Signatures: [new(list, str), new([.. list, new("plural", number, false)], str)]),
                true, JsName: "t", StaticKey: "list", StaticListArity: 2),
        ]);
        var export = new NpmValueExport("resources", "composer", "resources", "1", "index.js", "index.d.ts", null,
            [new([], composer)], null);
        const string source = """
            @using Tool = Npm.Resources.Exports
            @code {
                public static string Read() => Tool.Composer().TWelcomeValue(new() { Name = "Name" });
                public static string Home() => Tool.Composer().THome();
                public static string Original() => Tool.Composer().TWelcome;
                public static string List() => Tool.Composer().TList(First(), Second());
                public static string PluralList() => Tool.Composer().TList(First(), Second(), Count());
                private static string First() => "first";
                private static string Second() => "second";
                private static double Count() => 2;
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], new() { NpmValues = [export] });
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        var call = Regex.Match(js, """return i\$[a-f0-9]+\(\)\["t"\]\(("(?:\\.|[^"\\])*"),""");
        Assert.IsTrue(call.Success, js);
        Assert.AreEqual("say\"hello\\world", JsonSerializer.Deserialize<string>(call.Groups[1].Value));
        StringAssert.Contains(js, """["t"]("home")""");
        StringAssert.Matches(js, new("""return i\$[a-f0-9]+\(\)\["t"\]\("list", \[First\(\), Second\(\)\]\)"""));
        StringAssert.Matches(js, new("""return i\$[a-f0-9]+\(\)\["t"\]\("list", \[First\(\), Second\(\)\], Count\(\)\)"""));
        StringAssert.Contains(js, "tWelcome");
        StringAssert.Contains(js, "name: \"Name\"");
    }
}
