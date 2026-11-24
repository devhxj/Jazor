using Jazor.Npm;
using Jolt.Compiler;
using Jolt.Npm;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class NpmOptionalMemberTests
{
    [TestMethod]
    public async Task Compile_SparseErrorMapReadsMissingKeyAsNull()
    {
        var errors = new NpmValueType("readonlyDictionary", ElementType: new("string"), Name: "Errors");
        var signature = new NpmValueSignature([], errors);
        var export = new NpmValueExport("counter", "errors", "counter", "1.0.0", "index.js", "index.d.ts", null, [signature], null);
        const string source = """
            @using Tool = Npm.Counter.Exports
            @code {
                public static bool Missing(string key) => Tool.Errors()[key] == null;
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], new() { NpmValues = [export] });
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "Object.hasOwn(");
        StringAssert.Contains(js, "?? null : null");
        StringAssert.Contains(js, ", key) === null");
    }

    [TestMethod]
    public async Task Compile_PreservesExplicitNullAndNormalizesMissingResultMembers()
    {
        const string source = """
            @using Tool = Npm.Counter.Exports
            <p>@Current</p>
            @code {
                private bool Current {
                    get {
                        Tool.Inspect(new() { Nullable = null });
                        var result = Tool.Inspect(new());
                        result.Nullable = null;
                        return result.Value == null;
                    }
                }
            }
            """;
        var number = new NpmValueType("number");
        var shape = new NpmValueType("object", Members:
            [new("nullable", new("nullable", ElementType: number), false, Optional: true),
             new("value", number, false, Optional: true)]);
        var signature = new NpmValueSignature([new("options", shape, false)], shape);
        var export = new NpmValueExport("counter", "inspect", "counter", "1.0.0", "index.js", "index.d.ts", null, [signature], null);
        var options = new JoltCompileOptions { NpmValues = [export] };
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "nullable: null");
        StringAssert.Contains(js, "({})");
        StringAssert.Contains(js, "result.nullable = null");
        StringAssert.Contains(js, """(result["value"] ?? null) === null""");

        var invalid = source.Replace("Nullable = null", "Value = null", StringComparison.Ordinal);
        var diagnostics = JoltProjectCompiler.Analyze([new("App.jazor", invalid)], options).Diagnostics;
        CollectionAssert.AreEquivalent(new[] { "CS0037", "CS0200" }, diagnostics.Select(item => item.Id).ToArray());
    }
}
