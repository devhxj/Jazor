using Jazor.Npm;
using Jolt.Compiler;
using Jolt.Npm;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class NpmFormPathMapTests
{
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task Compile_PathMapWritesFlatOwnKeysAndRestrictsValues(bool veeValidate)
    {
        var stringType = new NpmValueType("string");
        var filter = new NpmValueType("object", Members: [
            new("profile", new("object", Members: [new("displayName", stringType, false)]), false),
            new("rows", new("recordArray", ElementType: new("object", Members: [
                new("displayName", stringType, false),
            ])), false),
        ]);
        var values = new NpmValueType("union", Variants: [stringType, new("array", ElementType: stringType)]);
        var errors = new NpmValueType("formPathMap", ElementType: new("undefinedable", ElementType: values),
            TypeArguments: [filter], VeeValidatePath: veeValidate, ArrayIndices: true);
        var signature = new NpmValueSignature([new("fields", errors, false)], new("void"));
        var export = new NpmValueExport("forms", "setErrors", "forms", "1.0.0", "index.js", "index.d.ts", null, [signature], null);
        var options = new JoltCompileOptions { NpmValues = [export] };
        const string source = """
            @using Tool = Npm.Forms.Exports
            @code {
                public static void Update() {
                    Tool.SetErrors(new() {
                        { form => form.Profile.DisplayName, "Required" },
                        { form => form.Rows[0].DisplayName, new string[] { "First", "Second" } },
                        { form => form.Rows[1].DisplayName, null },
                    });
                    Tool.SetErrors(new());
                }
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "[\"profile.displayName\"]:");
        StringAssert.Contains(js, "[\"rows[0].displayName\"]:");
        StringAssert.Contains(js, "[\"rows[1].displayName\"]:");
        StringAssert.Contains(js, "null ?? undefined");
        StringAssert.Contains(js, "\"First\", \"Second\"");
        StringAssert.Matches(js, new(@"i\$[a-f0-9]+\(\{\}\)"));

        var invalid = source.Replace("form.Profile.DisplayName, \"Required\"", "form.Profile.DisplayName, 42", StringComparison.Ordinal)
            .Replace("form.Rows[0].DisplayName, new string[]", "form.Rows[0].Missing, new string[]", StringComparison.Ordinal);
        var diagnostics = JoltProjectCompiler.Analyze([new("App.jazor", invalid)], options).Diagnostics;
        Assert.IsTrue(diagnostics.Any(item => item.Id == "CS1503"));
        Assert.IsTrue(diagnostics.Any(item => item.Id == "CS1061"));
    }

    [TestMethod]
    public async Task Compile_NullAndUndefinedArgumentsKeepTheirDeclaredWireValues()
    {
        var text = new NpmValueType("string");
        NpmValueExport Function(string name, string kind) => new("forms", name, "forms", "1.0.0",
            "index.js", "index.d.ts", null, [new([new("message", new(kind, ElementType: text), false)], new("void"))], null);
        const string source = """
            @using Tool = Npm.Forms.Exports
            @code {
                public static void Update(string? message) {
                    Tool.Clear(message);
                    Tool.Store(null);
                    System.Action<string?> clear = Tool.Clear;
                    clear(message);
                }
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)],
            new JoltCompileOptions { NpmValues = [Function("clear", "undefinedable"), Function("store", "nullable")] });
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "message ?? undefined");
        StringAssert.Contains(js, "__jz_fixedArg0 ?? undefined");
        StringAssert.Contains(js, "(null)");
    }
}
