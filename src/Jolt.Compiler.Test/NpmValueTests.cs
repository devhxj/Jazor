using System.Text.RegularExpressions;
using Jolt.Compiler;
using Jolt.Npm;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class NpmValueTests
{
    private static readonly NpmValueType Text = new("string");
    private static readonly NpmValueType Number = new("number");

    [TestMethod]
    public async Task Compile_ImportsOnlyUsedValuesWithStableAliases()
    {
        const string aliases = """
            @using Text = Npm.EsToolkit.String.Exports
            @using Other = Npm.Other.Exports
            @using Again = Npm.EsToolkit.String.Exports

            """;
        const string first = """<p>@(Text.CamelCase("order total")) @(Again.CamelCase("cart"))</p>""";
        const string second = """<p>@(Other.CamelCase("second call")) @(Text.Default("text")) @Other.Initial</p>""";
        var signature = new NpmValueSignature([new("value", Text, false)], Text);
        var options = Options(Export("es-toolkit/string", "camelCase", [signature]),
            Export("es-toolkit/string", "default", [signature]), Export("es-toolkit/string", "unused", [signature]),
            Export("other", "camelCase", [signature]), Export("other", "initial", [], Number));
        var firstOrder = await JoltProjectCompiler.CompileAsync([new("App.jazor", aliases + first + second), new("Unused.jazor", aliases + "<p />")], options);
        var oppositeOrder = await JoltProjectCompiler.CompileAsync([new("App.jazor", aliases + second + first)], options);
        Assert.IsEmpty(firstOrder.Diagnostics, string.Join(Environment.NewLine, firstOrder.Diagnostics));
        Assert.IsEmpty(oppositeOrder.Diagnostics, string.Join(Environment.NewLine, oppositeOrder.Diagnostics));
        var content = firstOrder.Modules[0].Content;
        var imports = content.Split('\n').Where(line => line.StartsWith("import ") && (line.Contains("es-toolkit/string") || line.Contains("other"))).ToArray();
        CollectionAssert.AreEqual(imports,
            oppositeOrder.Modules[0].Content.Split('\n').Where(line => line.StartsWith("import ") && (line.Contains("es-toolkit/string") || line.Contains("other"))).ToArray());
        var bindings = Regex.Matches(string.Join(' ', imports), @"camelCase as (\w+\$\w+)").Select(match => match.Groups[1].Value).ToArray();
        Assert.HasCount(2, bindings.Distinct());
        Assert.AreEqual(3, bindings.Sum(binding => Regex.Matches(content, Regex.Escape(binding) + @"\(").Count));
        StringAssert.Contains(content, "initial as ");
        Assert.IsTrue(imports.Any(line => Regex.IsMatch(line, @"^import i\$\w+.*es-toolkit/string")));
        Assert.DoesNotContain("unused", content);
        Assert.DoesNotContain("es-toolkit/string", firstOrder.Modules[1].Content);
    }

    [TestMethod]
    public void Analyze_MapsTypeErrorsAndNpmDefinitionsToTheirSource()
    {
        const string source = """
            @using Text = Npm.EsToolkit.String.Exports
            <p>@(Text.CamelCase(42))</p>
            """;
        var definition = new NpmDefinition("package/index.d.ts", 3, 5);
        var export = Export("es-toolkit/string", "camelCase", [new([new("value", Text, false)], Text)]) with { Definition = definition };
        var analysis = JoltProjectCompiler.Analyze([new("App.jazor", source)], Options(export));
        var diagnostic = analysis.Diagnostics.Single();
        Assert.AreEqual("CS1503", diagnostic.Id);
        Assert.AreEqual("App.jazor", diagnostic.Location.GetLineSpan().Path);
        Assert.AreEqual("42", source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
        Assert.AreEqual(definition, analysis.NpmValueDefinitions.Single().Value);
    }

    [TestMethod]
    public async Task Compile_LowersRefReadsAndWritesAndNestedCallbackCalls()
    {
        const string source = """
            @using Tool = Npm.Counter.Exports
            <p>@Current</p>
            @code {
                private double Current {
                    get {
                        var counter = Tool.UseCounter();
                        ECMAScript.Vue.Watch(counter.Count, (value, previous) => Seen = value);
                        counter.Count.Value = 3;
                        counter.Count.Value++;
                        return counter.Inc();
                    }
                }
                private double Seen;
            }
            """;
        var counter = new NpmValueType("object", Members:
            [new("count", new("ref", ElementType: Number), true),
             new("inc", new("function", Signatures: [new([new("delta", Number, true)], Number)]), true)]);
        var export = Export("counter", "useCounter", [new([new("initial", Number, true)], counter)]);
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], Options(export));
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "useCounter as ");
        StringAssert.Contains(js, ".count.value = 3");
        StringAssert.Contains(js, ".inc()");
        StringAssert.Contains(js, "watch");

        var readonlyCounter = counter with { Members = [counter.Members![0] with { Type = counter.Members[0].Type with { ReadOnly = true } }, counter.Members[1]] };
        var invalid = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)],
            Options(export with { Signatures = [export.Signatures[0] with { ReturnType = readonlyCounter }] }));
        Assert.IsEmpty(invalid.Modules);
        Assert.IsTrue(invalid.Diagnostics.All(diagnostic => diagnostic.Id == "CS0200"));
        Assert.HasCount(2, invalid.Diagnostics);
        var readOnlySource = source.Replace("counter.Count.Value = 3;", "").Replace("counter.Count.Value++;", "");
        var validReadOnly = await JoltProjectCompiler.CompileAsync([new("App.jazor", readOnlySource)],
            Options(export with { Signatures = [export.Signatures[0] with { ReturnType = readonlyCounter }] }));
        Assert.IsEmpty(validReadOnly.Diagnostics, string.Join(Environment.NewLine, validReadOnly.Diagnostics));
        StringAssert.Contains(validReadOnly.Modules.Single().Content, "watch");
    }

    [TestMethod]
    public async Task Compile_PreservesRefsInAnAutoPropertyInitializedByNpmSetup()
    {
        const string source = """
            @using Tool = Npm.Counter.Exports
            <p>@Counter.Count.Value</p>
            @code {
                private Tool.UseCounterResult Counter { get; set; } = null!;
                [Setup]
                private void Configure() {
                    Counter = Tool.UseCounter();
                    ECMAScript.Vue.Watch(Counter.Count, (double value, double previous) => { });
                }
            }
            """;
        var counter = new NpmValueType("object", Members: [new("count", new("ref", ElementType: Number, ReadOnly: true), true)]);
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)],
            Options(Export("counter", "useCounter", [new([], counter)])));
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "useCounter as ");
        StringAssert.Contains(js, "get Counter()");
        StringAssert.Contains(js, "$joltShallowReactive($joltToRaw(this))");
        StringAssert.Contains(js, "this.Counter.count.value");
    }

    [TestMethod]
    public async Task Compile_PassesUndefinedForOmittedParameterBeforeNamedOptions()
    {
        const string source = """
            @using Tool = Npm.Counter.Exports
            <p>@Current</p>
            @code {
                private double Current => Tool.UseCounter(options: new() { Min = 3 }).Count.Value;
            }
            """;
        var counter = new NpmValueType("object", Members: [new("count", new("ref", ElementType: Number), true)]);
        var options = new NpmValueType("object", Members: [new("min", Number, true, Optional: true)]);
        var export = Export("counter", "useCounter", [new([new("initial", Number, true), new("options", options, true)], counter)]);
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], Options(export));
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "void 0");
        StringAssert.Contains(js, "min: 3");
    }

    [TestMethod]
    public async Task Compile_BindsNpmCallbackArgumentsAndFunctionValues()
    {
        const string source = """
            @using Tool = Npm.Counter.Exports
            <p>@Current</p>
            @code {
                private double Current {
                    get {
                        Func<double, double> callback = Tool.Double;
                        return Tool.Apply(3, value => callback(value));
                    }
                }
            }
            """;
        var callback = new NpmValueType("function", Signatures: [new([new("value", Number, false)], Number)]);
        var exports = Options(
            Export("counter", "double", [new([new("value", Number, false)], Number)]),
            Export("counter", "apply", [new([new("value", Number, false), new("callback", callback, false)], Number)]));
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], exports);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "apply as ");
        StringAssert.Contains(js, "double as ");
        StringAssert.Contains(js, "callback(value)");
    }

    [TestMethod]
    public async Task Compile_PreservesPunctuationInExportAndMemberNames()
    {
        const string source = """
            @using Tool = Npm.Counter.Exports
            <p>@Current</p>
            @code {
                private string Current => Tool.ContentInfo().ContentType;
            }
            """;
        var resultType = new NpmValueType("object", Members: [new("content-type", Text, true)]);
        var export = Export("counter", "content-info", [new([], resultType)]);
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], Options(export));
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "\"content-info\" as ");
        StringAssert.Contains(js, "[\"content-type\"]");
    }

    private static NpmValueExport Export(string specifier, string name, NpmValueSignature[] signatures, NpmValueType? constant = null) =>
        new(specifier, name, specifier.Split('/')[0], "1.0.0", "index.js", "index.d.ts", null, signatures, constant);

    private static JoltCompileOptions Options(params NpmValueExport[] exports) => new() { NpmValues = exports };
}
