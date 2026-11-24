using Jazor.Npm;
using System.Text.RegularExpressions;
using Jolt.Compiler;
using Jolt.Npm;
using Microsoft.CodeAnalysis;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class NpmValueTests
{
    [TestMethod]
    public void GenerateSource_GenericSparseDictionaryDoesNotUseVue()
    {
        var element = new NpmValueType("typeParameter", Name: "T");
        var read = new NpmValueType("readonlyDictionary", Name: "Lookup", ElementType: element);
        var write = new NpmValueType("writableDictionary", Name: "Table", ElementType: element);
        var source = NpmBindings.GenerateSource([], [
            Export("counter", "read", [new([], read, TypeParameters: ["T"])]),
            Export("counter", "write", [new([], write, TypeParameters: ["T"])])
        ]);

        Assert.DoesNotContain("ECMAScript.Vue", source);
        StringAssert.Contains(source, "class Lookup<@T>");
        StringAssert.Contains(source, "class Table<@T>");
        StringAssert.Contains(source, "extern global::ECMAScript.NullableValue<@T>? this[string key]");
        StringAssert.Contains(source, "Set(string key, @T value)");
    }

    [TestMethod]
    public async Task Compile_StringDictionaryUsesStructuralLiteralAndKeepsVueRefProjection()
    {
        const string source = """
            @using Tool = Npm.Counter.Exports
            <p>@(Tool.Save(new() { ["key"] = "value" }) + Tool.Current().Value)</p>
            """;
        var dictionary = new NpmValueType("inputDictionary", ElementType: Text);
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], Options(
            Export("counter", "save", [new([new("values", dictionary, false)], Text)]),
            Export("counter", "current", [new([], new("ref", ElementType: Text, ReadOnly: true))])));

        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var script = result.Modules.Single().Content;
        StringAssert.Contains(script, "key:");
        StringAssert.Contains(script, ".value");
        Assert.DoesNotContain("new Map(", script);
    }

    [TestMethod]
    public void GenerateSource_PreservesRealExportOverVariantFromAnotherSpecifier()
    {
        var factory = Export("sample/feature-api", "createPlugin", [
            new([], Text),
            new([], Text, ManagedNameSuffix: "WithModeTrue"),
            new([], Text, ManagedNameSuffix: "WithModeFalse"),
        ]);
        var real = Export("sample/feature_api", "createPluginWithModeTrue", [new([], Number)]);
        var source = NpmBindings.GenerateSource([], [factory, real]);
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator).Select(path => MetadataReference.CreateFromFile(path));
        var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("PublicFacade",
            [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source)], references,
            new(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        var facade = compilation.GetTypeByMetadataName("Npm.Sample.FeatureApi.Exports")!;
        var winner = (IMethodSymbol)facade.GetMembers("CreatePluginWithModeTrue").Single();
        Assert.AreEqual(SpecialType.System_Double, winner.ReturnType.SpecialType);
        Assert.AreEqual("sample/feature_api", winner.GetAttributes()
            .Single(attribute => attribute.AttributeClass?.Name == "ECMAScriptAttribute").ConstructorArguments[0].Value);
        Assert.HasCount(1, facade.GetMembers("CreatePlugin"));
        Assert.HasCount(1, facade.GetMembers("CreatePluginWithModeFalse"));
    }

    private static readonly NpmValueType Text = new("string");
    private static readonly NpmValueType Number = new("number");

    [TestMethod]
    public void Analyze_PreservesStringEnumLiteralsWithReadableMemberNames()
    {
        var greeting = new NpmValueType("stringEnum", Name: "Greeting",
            StringLiterals: ["Salut {guest}!", "SalutGuest", "Salut? guest", "你好，访客", "!!!"]);
        var analysis = JoltProjectCompiler.Analyze([new("App.jazor", "<p />")],
            Options(Export("messages", "greet", [new([], greeting)])));
        Assert.IsEmpty(analysis.Diagnostics, string.Join(Environment.NewLine, analysis.Diagnostics));
        var facade = analysis.Documents.Single().SemanticModel.Compilation.GetTypeByMetadataName("Npm.Messages.Exports")!;
        var method = (IMethodSymbol)facade.GetMembers("Greet").Single();
        void AssertLiteral(string name, string literal)
        {
            var field = (IFieldSymbol)method.ReturnType.GetMembers(name).Single();
            Assert.AreEqual(literal, field.GetAttributes()
                .Single(attribute => attribute.AttributeClass?.Name == "ECMAScriptNameAttribute").ConstructorArguments[0].Value);
        }
        AssertLiteral("SalutGuest_2", "Salut {guest}!");
        AssertLiteral("SalutGuest", "SalutGuest");
        AssertLiteral("SalutGuest_3", "Salut? guest");
        AssertLiteral("你好访客", "你好，访客");
        AssertLiteral("Value", "!!!");
    }

    [TestMethod]
    public void Analyze_ReusesOnlyMatchingNumericEnumDomains()
    {
        var first = new NpmValueType("enum", Name: "Level", EnumMembers: [new("low", 1)]);
        var same = first with { EnumMembers = [new("low", 1)] };
        var otherValue = first with { EnumMembers = [new("low", 2)] };
        var otherMember = first with { EnumMembers = [new("high", 1)] };
        var analysis = JoltProjectCompiler.Analyze([new("App.jazor", "<p />")], Options(
            Export("counter", "first", [new([], first)]),
            Export("counter", "same", [new([], same)]),
            Export("counter", "otherValue", [new([], otherValue)]),
            Export("counter", "otherMember", [new([], otherMember)])));
        Assert.IsEmpty(analysis.Diagnostics, string.Join(Environment.NewLine, analysis.Diagnostics));
        var facade = analysis.Documents.Single().SemanticModel.Compilation
            .GetTypeByMetadataName("Npm.Counter.Exports")!;
        IMethodSymbol Method(string name) => (IMethodSymbol)facade.GetMembers(name).Single();
        Assert.IsTrue(SymbolEqualityComparer.Default.Equals(
            Method("First").ReturnType, Method("Same").ReturnType));
        var differentValue = Method("OtherValue").ReturnType;
        var differentMember = Method("OtherMember").ReturnType;
        Assert.IsFalse(SymbolEqualityComparer.Default.Equals(Method("First").ReturnType, differentValue));
        Assert.AreEqual(2, ((IFieldSymbol)differentValue.GetMembers("Low").Single()).ConstantValue);
        Assert.AreEqual(1, ((IFieldSymbol)differentMember.GetMembers("High").Single()).ConstantValue);
    }

    [TestMethod]
    public void Analyze_PreservesPartialRecordElementsAndRefKinds()
    {
        var definition = new NpmDefinition("counter.d.ts", 0, 6);
        NpmValueType Shape(NpmValueType element, string? refKind) => new("object",
            Name: "Result", TypeDefinition: definition, Members:
            [
                new("values", new("partialRecord", ElementType: element), true),
                new("current", new("ref", ElementType: Text, Name: refKind, ReadOnly: true), true),
            ]);
        var plainRecord = new NpmValueType("plainRecord");
        var typedRecord = new NpmValueType("recordTypeParameter", Name: "T");
        var analysis = JoltProjectCompiler.Analyze([new("App.jazor", "<p />")], Options(
            Export("counter", "first", [new([], Shape(plainRecord, null))]),
            Export("counter", "same", [new([], Shape(plainRecord, null))]),
            Export("counter", "otherElement", [new([], Shape(typedRecord, null), TypeParameters: ["T"])]),
            Export("counter", "otherRef", [new([], Shape(plainRecord, "VueComputedRef"))])));
        Assert.IsEmpty(analysis.Diagnostics, string.Join(Environment.NewLine, analysis.Diagnostics));
        var facade = analysis.Documents.Single().SemanticModel.Compilation.GetTypeByMetadataName("Npm.Counter.Exports")!;
        ITypeSymbol Result(string name) => ((IMethodSymbol)facade.GetMembers(name).Single()).ReturnType;
        Assert.IsTrue(SymbolEqualityComparer.Default.Equals(Result("First"), Result("Same")));
        var values = (INamedTypeSymbol)((IPropertySymbol)Result("OtherElement").GetMembers("Values").Single()).Type;
        var current = (IPropertySymbol)Result("OtherRef").GetMembers("Current").Single();
        Assert.AreEqual(TypeKind.TypeParameter, values.TypeArguments.Single().TypeKind);
        Assert.AreEqual("VueComputedRef", current.Type.Name);
    }

    [TestMethod]
    public async Task Compile_PreservesPromisePayloadInThenCallback()
    {
        const string source = """
            @using Tool = Npm.Counter.Exports
            <p>@Current</p>
            @code {
                private double Current;
                [Setup]
                private void Configure() {
                    Tool.Fetch().Then(result => { Current = result.Count; });
                }
            }
            """;
        var payload = new NpmValueType("object", Members: [new("count", Number, true)]);
        var promise = new NpmValueType("promise", ElementType: payload);
        var options = Options(Export("counter", "fetch", [new([], promise)]));
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules.Single().Content, ".then(");
        StringAssert.Contains(result.Modules.Single().Content, "result.count");
    }

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
    public async Task Compile_ProjectsCallbackArgumentsAndResultsByValueDirection()
    {
        const string source = """
            @using Tool = Npm.Counter.Exports
            <p>@Current</p>
            @code {
                private double Current {
                    get {
                        var callback = Tool.GetCallback();
                        var result = callback(new() { X = 3 });
                        return Tool.ApplyCallback(input => new() { Y = input.X }) + result.Y;
                    }
                }
            }
            """;
        var input = new NpmValueType("object", Members: [new("x", Number, true)]);
        var output = new NpmValueType("object", Members: [new("y", Number, true)]);
        var callback = new NpmValueType("function", Signatures: [new([new("event", input, false)], output)]);
        var options = Options(
            Export("counter", "getCallback", [new([], callback)]),
            Export("counter", "applyCallback", [new([new("callback", callback, false)], Number)]));
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "callback({ x: 3 })");
        StringAssert.Contains(js, "y: input.x");
        StringAssert.Contains(js, "result.y");
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

    [TestMethod]
    public async Task Compile_UsesTypedDateAndArrayOverloadsWithOneImport()
    {
        const string source = """
            @using Dates = Npm.Dates.Exports
            <p>@Current</p>
            @code {
                private double Current {
                    get {
                        ECMAScript.Date date = Dates.Normalize("2025-01-01");
                        ECMAScript.Array<ECMAScript.Date> dates = Dates.Normalize(date);
                        ECMAScript.IArray<ECMAScript.Date> snapshot = Dates.Snapshot(dates);
                        date = snapshot[0];
                        return Dates.Read(new ECMAScript.Date[] { date, dates[0] });
                    }
                }
            }
            """;
        var date = new NpmValueType("date");
        var dates = new NpmValueType("array", ElementType: date);
        var options = Options(
            Export("dates", "normalize", [new([new("value", Text, false)], date), new([new("value", date, false)], dates)]),
            Export("dates", "read", [new([new("values", dates, false)], Number)]),
            Export("dates", "snapshot", [new([new("values", dates with { ReadOnly = true }, false)], dates with { ReadOnly = true })]));
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        Assert.HasCount(1, Regex.Matches(js, "normalize as "));
        Assert.HasCount(1, Regex.Matches(js, "read as "));
        StringAssert.Contains(js, "dates[0]");

        var invalid = JoltProjectCompiler.Analyze([new("App.jazor", source.Replace("Dates.Normalize(date)", "Dates.Normalize(true)"))], options);
        Assert.AreEqual("CS1503", invalid.Diagnostics.Single().Id);
        var invalidWrite = JoltProjectCompiler.Analyze([new("App.jazor", source.Replace("date = snapshot[0]", "snapshot[0] = date"))], options);
        Assert.AreEqual("CS0200", invalidWrite.Diagnostics.Single().Id);
    }

    [TestMethod]
    public async Task Compile_PassesNominalDomValuesDirectlyToNpm()
    {
        const string source = """
            @using Browser = Npm.Browser.Exports
            <p>@Current</p>
            @code {
                private double Current {
                    get {
                        ECMAScript.WindowRef window = Browser.CurrentWindow();
                        return Browser.Measure(window, Browser.CurrentElement());
                    }
                }
            }
            """;
        var window = new NpmValueType("web", Name: "WindowRef");
        var element = new NpmValueType("web", Name: "Element");
        var options = Options(
            Export("browser", "currentWindow", [new([], window)]),
            Export("browser", "currentElement", [new([], element)]),
            Export("browser", "measure", [new([new("window", window, false), new("element", element, false)], Number)]));
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        var measure = Regex.Match(js, @"measure as (\w+\$\w+)").Groups[1].Value;
        var currentElement = Regex.Match(js, @"currentElement as (\w+\$\w+)").Groups[1].Value;
        StringAssert.Contains(js, $"{measure}(window, {currentElement}())");

        var invalid = JoltProjectCompiler.Analyze([new("App.jazor", source.Replace("Browser.Measure(window", "Browser.Measure(Browser.CurrentElement()"))], options);
        Assert.AreEqual("CS1503", invalid.Diagnostics.Single().Id);
    }

    [TestMethod]
    public async Task Compile_DistinguishesExplicitNullFromAnOmittedParameter()
    {
        const string source = """
            @using Tool = Npm.Counter.Exports
            <p>@Current</p>
            @code {
                private double Current {
                    get {
                        Tool.Measure();
                        Tool.Measure(null);
                        return Tool.Measure(label: "sample");
                    }
                }
            }
            """;
        var nullable = new NpmValueType("nullable", ElementType: Number);
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)],
            Options(Export("counter", "measure", [new([new("value", nullable, true), new("label", Text, true)], Number)])));
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        var binding = Regex.Match(js, @"measure as (\w+\$\w+)").Groups[1].Value;
        StringAssert.Contains(js, $"{binding}()");
        StringAssert.Contains(js, $"{binding}(null)");
        // Named argument canonicalization captures the supplied label before the gap.
        StringAssert.Matches(js, new Regex(Regex.Escape(binding) +
            @"\(\((__arg\$\w+) = ""sample"", (__arg\$\w+) = void 0, \2\), \1\)"));
    }

    private static NpmValueExport Export(string specifier, string name, NpmValueSignature[] signatures, NpmValueType? constant = null) =>
        new(specifier, name, specifier.Split('/')[0], "1.0.0", "index.js", "index.d.ts", null, signatures, constant);

    private static JoltCompileOptions Options(params NpmValueExport[] exports) => new() { NpmValues = exports };
}
