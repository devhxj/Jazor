using Jolt.Npm;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class NpmHandleTests
{
    [TestMethod]
    public void Projection_CompilesIndependentFilesTogether()
    {
        NpmValueExport Export(string package, string type) =>
            new(package, "read", package, "1.0.0", "index.js", "index.d.ts", null, [new([], new("handle",
                Name: type, TypeDefinition: new($"C:/repo/node_modules/{package}/index.d.ts", 1, 5)))], null);
        var first = JoltNpmBindings.GenerateSource([Export("first", "First")]);
        var second = JoltNpmBindings.GenerateSource([Export("second", "Second")]);
        var scalar = Export("scalar", "Unused") with { Signatures = [new([], new("string"))] };
        var third = JoltNpmBindings.GenerateSource([scalar]);
        Assert.IsFalse(third.Contains("namespace Jolt.Generated.Npm", StringComparison.Ordinal));

        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator).Select(path => Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(path));
        var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("Combined",
            new[] { first, second, third }.Select(source => Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source)),
            references, new(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(diagnostic =>
            diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
    }

    [TestMethod]
    public async Task Projection_KeepsPackageDeclarationNamesWhenModulesAndCollisionsAreAdded()
    {
        NpmValueExport Export(string module, string export, string file, int line) =>
            new(module, export, "sample", "1.0.0", "index.js", "index.d.ts", null,
                [new([], new("handle", Name: "Locale", TypeDefinition: new(file, line, 5)))], null);

        var baseline = Export("sample", "locale", "C:/first/node_modules/sample/lib/locale.d.ts", 10);
        var reexport = Export("sample/extra", "sameLocale", "D:/second/node_modules/sample/lib/locale.d.ts", 90);
        var distinct = Export("sample", "alternateLocale", "D:/second/node_modules/sample/other/locale.d.ts", 90);
        var generated = Export("generated", "readLocale", "C:/first/node_modules/sample/lib/locale.d.ts", 10) with
        {
            PackageName = "generated",
        };
        string ReturnType(string method, params NpmValueExport[] exports) =>
            Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(JoltNpmBindings.GenerateSource(exports))
                .GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
                .Single(declaration => declaration.Identifier.ValueText == method).ReturnType.ToString();

        var original = ReturnType("Locale", baseline);
        StringAssert.StartsWith(original, "global::Jolt.Generated.Npm.Exports.Locale_");
        Assert.AreEqual(original, ReturnType("Locale", distinct, reexport, baseline));
        Assert.AreEqual(original, ReturnType("SameLocale", distinct, reexport, baseline));
        Assert.AreNotEqual(original, ReturnType("AlternateLocale", distinct, reexport, baseline));
        Assert.AreEqual(original, ReturnType("ReadLocale", generated, baseline));
        const string source = """
            @using Package = Npm.Generated.Exports
            @code {
                private void Read() {
                    Jolt.Generated.Npm.Exports.LOCALE_TYPE locale = Package.ReadLocale();
                }
            }
            """;
        var compiled = await JoltProjectCompiler.CompileAsync(
            [new("App.jazor", source.Replace("Jolt.Generated.Npm.Exports.LOCALE_TYPE", original, StringComparison.Ordinal))],
            new() { NpmValues = [generated, baseline] });
        Assert.IsEmpty(compiled.Diagnostics, string.Join(Environment.NewLine, compiled.Diagnostics));
        Assert.HasCount(1, compiled.Modules);
    }

    [TestMethod]
    public void Projection_KeepsNamesForCollidingNamedAndAnonymousInputs()
    {
        NpmValueExport Export(string name, string member, NpmDefinition? definition) =>
            new("sample", name, "sample", "1.0.0", "index.js", "index.d.ts", null,
                [new([new("options", new("object", Name: definition is null ? null : "Settings",
                    TypeDefinition: definition, Members: [new(member, new("string"), false)]), false)], new("void"))], null);
        string[] ParameterTypes(string method, params NpmValueExport[] exports) =>
            Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(JoltNpmBindings.GenerateSource(exports))
                .GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
                .Where(declaration => declaration.Identifier.ValueText == method)
                .Select(declaration => declaration.ParameterList.Parameters.Single().Type!.ToString()).ToArray();

        var original = Export("primary", "label", new("C:/repo/node_modules/sample/lib/primary.d.ts", 1, 5));
        var second = Export("secondary", "label", new("C:/repo/node_modules/sample/lib/secondary.d.ts", 1, 5));
        var extra = Export("reserve", "label", new("C:/repo/node_modules/sample/lib/reserve.d.ts", 1, 5));
        var named = ParameterTypes("Secondary", original, second).Single();
        Assert.AreNotEqual("Settings", named);
        Assert.AreEqual(named, ParameterTypes("Secondary", extra, second, original).Single());

        var anonymous = Export("update", "label", null);
        var alternate = Export("update", "value", null);
        var additional = Export("update", "title", null);
        var initialTypes = ParameterTypes("Update", anonymous, alternate);
        var expandedTypes = ParameterTypes("Update", anonymous, additional, alternate);
        Assert.AreEqual(2, initialTypes.Distinct().Count());
        CollectionAssert.IsSubsetOf(initialTypes, expandedTypes);
    }

    [TestMethod]
    public async Task Compile_LowersNestedPartialRecordPathsWithBoundNamesAndOneReceiverRead()
    {
        const string source = """
            @code {
                public record Leaf([property: ECMAScript.ECMAScriptName("count-value")] double Count) {
                    public double Calculated => Count + 1;
                }
                public record Detail([property: ECMAScript.ECMAScriptName("inner leaf")] Leaf? Leaf);
                public record Shape([property: ECMAScript.ECMAScriptName("detail")] Detail? Child) : ECMAScript.Vue.VueProps;

                private double? Read(System.Func<ECMAScript.Vue.PartialRecord<Shape>> snapshot, Shape other) =>
                    snapshot().GetValue(value => value.Child!.Leaf!.Count);
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules.Single().Content, """snapshot()["detail"]?.["inner leaf"]?.["count-value"] ?? null""");

        foreach (var invalidSource in new[] {
            source.Replace("value.Child!", "other.Child!", StringComparison.Ordinal),
            source.Replace("Leaf!.Count);", "Leaf!.Count + 1);", StringComparison.Ordinal),
            source.Replace("Leaf!.Count);", "Leaf!.Calculated);", StringComparison.Ordinal),
        })
        {
            var invalid = await JoltProjectCompiler.CompileAsync([new("App.jazor", invalidSource)]);
            Assert.IsTrue(invalid.Diagnostics.Any(diagnostic => diagnostic.Id == "JOLT2001" &&
                diagnostic.GetMessage().Contains("selecting a record data property path", StringComparison.Ordinal)),
                string.Join(Environment.NewLine, invalid.Diagnostics));
        }
    }

    [TestMethod]
    public async Task Compile_ProjectsOptionalContextValuesForReferenceAndValueTypes()
    {
        const string source = """
            @using Tool = Npm.Tool.Exports
            @code {
                public static bool Read() {
                    string key = "count";
                    double? number = Tool.Read<double>(in key).Value;
                    bool? flag = Tool.Read<bool>().Value;
                    var input = ECMAScript.Vue.Ref("ref-key");
                    bool? byRef = Tool.Read<bool>(input).Value;
                    double? byGetter = Tool.Read<double>(() => "getter-key").Value;
                    string? label = Tool.Read<string>("label").Value;
                    int? assertedInteger = Tool.Read<int>("integer").Value;
                    return number == 0 && flag == false && byRef == null && byGetter == null && label == null && assertedInteger == null;
                }
            }
            """;
        var value = new NpmValueType("typeParameter", Name: "TValue");
        var output = new NpmValueType("ref", ReadOnly: true, ElementType: new("undefinedable", ElementType: value));
        var path = new NpmValueParameter("key", new("string"), false);
        var reference = new NpmValueSignature([path], output, TypeParameters: ["TValue"]);
        var scalar = reference with { ValueTypeParameters = ["TValue"],
            Parameters = [path with { Optional = true, ReadOnlyReference = true }] };
        var refPath = path with { Type = new("ref", ElementType: path.Type), ReadOnlyReference = true };
        var getterPath = path with { Type = new("function", Signatures: [new([], path.Type)]), ReadOnlyReference = true };
        var export = new NpmValueExport("tool", "read", "tool", "1.0.0", "index.js", "index.d.ts", null,
            [reference with { Parameters = [] }, reference, scalar,
                scalar with { Parameters = [refPath] }, scalar with { Parameters = [getterPath] }], null);
        var options = new JoltCompileOptions { NpmValues = [export] };
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, """(key)["value"] ?? null""");
        StringAssert.Contains(js, """()["value"] ?? null""");
        StringAssert.Contains(js, """(input)["value"] ?? null""");
        StringAssert.Contains(js, """return "getter-key";""");
        StringAssert.Contains(js, """("label")["value"] ?? null""");
        StringAssert.Contains(js, """("integer")["value"] ?? null""");

        var invalid = JoltProjectCompiler.Analyze([new("App.jazor", source.Replace("bool? flag", "double? flag"))], options);
        Assert.IsTrue(invalid.Diagnostics.Any(diagnostic => diagnostic.Id == "CS0029"));
        var nullable = JoltProjectCompiler.Analyze([new("App.jazor", source.Replace("Tool.Read<double>(in key)", "Tool.Read<double?>(in key)"))], options);
        Assert.IsTrue(nullable.Diagnostics.Any(diagnostic => diagnostic.Id == "CS0453"));
    }

    [TestMethod]
    public async Task Compile_SpecializesOptionalScalarResultsWithIndependentVariables()
    {
        const string source = """
            @using Tool = Npm.Tool.Exports
            <p>@Current</p>
            @code {
                private bool Current {
                    get {
                        double? number = Tool.Invoke(new Tool.InvokeNumberOptionsInput<bool> { Load = variable => variable ? 0 : 1 }).Data.Value;
                        bool? flag = Tool.Invoke(new Tool.InvokeBooleanOptionsInput<double> { Load = variable => variable > 0 }).Data.Value;
                        return number == null && flag == null;
                    }
                }
            }
            """;
        var data = new NpmValueType("typeParameter", Name: "TData");
        var variable = new NpmValueType("typeParameter", Name: "TVariable");
        var callback = new NpmValueType("function", Signatures: [new([new("variable", variable, false)], data)]);
        var input = new NpmValueType("object", Members: [new("load", callback, true)]);
        var optional = new NpmValueType("ref", ReadOnly: true, ElementType: new("undefinedable", ElementType: data));
        var output = new NpmValueType("object", Members: [new("data", optional, true)]);
        var signature = new NpmValueSignature([new("options", input, false)], output,
            TypeParameters: ["TData", "TVariable"], ScalarResultTypeParameter: "TData");
        var options = new JoltCompileOptions
        {
            NpmValues = [new("tool", "invoke", "tool", "1.0.0", "index.js", "index.d.ts", null, [signature], null)],
        };
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        Assert.AreEqual(2, js.Split("""["value"] ?? null""").Length - 1);
        StringAssert.Contains(js, "variable ? 0 : 1");
        StringAssert.Contains(js, "variable > 0");
        StringAssert.Contains(js, "number === null && flag === null");

        var invalid = JoltProjectCompiler.Analyze([new("App.jazor", source.Replace("variable > 0", "variable + 1"))], options);
        Assert.IsTrue(invalid.Diagnostics.Any(diagnostic => diagnostic.Id == "CS0029"));
    }

    [TestMethod]
    public async Task Compile_KeepsOptionalRefsInComponentFieldsAcrossReplacement()
    {
        const string source = """
            @using Tool = Npm.Tool.Exports
            <p>@(Data.Value ?? "missing")</p>
            @code {
                private ECMAScript.Vue.VueReadonlyOptionalRef<string?> Data = Tool.GetData();
                private Tool.Recursive Loop = Tool.GetLoop();
                private string ReadAndReplace() {
                    var before = Data.Value;
                    Data = Tool.GetData();
                    return (before ?? "") + (Data.Value ?? "") + Loop.Next.Label;
                }
            }
            """;
        var text = new NpmValueType("string");
        var optional = new NpmValueType("ref", ReadOnly: true, ElementType: new("undefinedable", ElementType: text));
        var recursive = new NpmValueType("handle", Name: "Recursive", TypeDefinition: new("tool/index.d.ts", 0, 9));
        var loop = recursive with { Members = [new("next", recursive, true), new("label", text, true)] };
        NpmValueExport Export(string name, NpmValueType result) =>
            new("tool", name, "tool", "1.0.0", "index.js", "index.d.ts", null, [new([], result)], null);
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)],
            new() { NpmValues = [Export("getData", optional), Export("getLoop", loop)] });
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        const string storage = """$joltShallowReactive($joltToRaw(this))["$jazor$private$Data"]""";
        StringAssert.Contains(js, """$jazor$private$Data = i$""");
        StringAssert.Contains(js, storage + " = i$");
        Assert.AreEqual(3, js.Split(storage + """["value"] ?? null""", StringSplitOptions.None).Length - 1);
        StringAssert.Contains(js, ".next.label");
    }

    [TestMethod]
    public async Task Compile_PreservesGenericCallbackAndOptionalRefValue()
    {
        const string source = """
            @using Tool = Npm.Tool.Exports
            <p>@Current</p>
            @code {
                private string Current => Tool.Read<string>(new() { Load = () => "data" }).Data.Value ?? "missing";
            }
            """;
        var parameter = new NpmValueType("typeParameter", Name: "TData");
        var callback = new NpmValueType("function", Signatures: [new([], parameter)]);
        var input = new NpmValueType("object", Members: [new("load", callback, true)]);
        var optionalRef = new NpmValueType("ref", ReadOnly: true, ElementType: new("undefinedable", ElementType: parameter));
        var resultType = new NpmValueType("object", Members: [new("data", optionalRef, true)]);
        var options = new JoltCompileOptions
        {
            NpmValues = [new("tool", "read", "tool", "1.0.0", "index.js", "index.d.ts", null,
                [new([new("options", input, false)], resultType, TypeParameters: ["TData"])], null)],
        };
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules.Single().Content, "load: () => {");
        StringAssert.Contains(result.Modules.Single().Content, "return \"data\";");
        StringAssert.Contains(result.Modules.Single().Content, """["value"] ?? null""");
        var invalid = JoltProjectCompiler.Analyze([new("App.jazor", source.Replace("() => \"data\"", "() => 42"))], options);
        Assert.IsTrue(invalid.Diagnostics.Any(diagnostic => diagnostic.Id == "CS0029"));

        const string numericSource = """
            @using Tool = Npm.Tool.Exports
            <p>@Current</p>
            @code {
                private double Current => Tool.Read<double>(new() { Load = () => 42 }).Data.Value;
            }
            """;
        var regularRef = new NpmValueType("ref", ReadOnly: true, ElementType: parameter);
        var numericResult = resultType with { Members = [new("data", regularRef, true)] };
        var export = options.NpmValues.Single();
        var numeric = await JoltProjectCompiler.CompileAsync([new("App.jazor", numericSource)], options with
        {
            NpmValues = [export with { Signatures = [export.Signatures.Single() with { ReturnType = numericResult }] }],
        });
        Assert.IsEmpty(numeric.Diagnostics, string.Join(Environment.NewLine, numeric.Diagnostics));
        StringAssert.Contains(numeric.Modules.Single().Content, "return 42;");

        var optionalNumeric = JoltProjectCompiler.Analyze([new("App.jazor", numericSource)], options);
        Assert.IsTrue(optionalNumeric.Diagnostics.Any(diagnostic => diagnostic.Id == "CS0452"));
    }

    [TestMethod]
    public async Task Compile_PreservesNamedHandleIdentityAndMethodReceiver()
    {
        const string source = """
            @using Tool = Npm.Tool.Exports
            <p>@Current</p>
            @code {
                private string Current {
                    get {
                        Tool.ITextModel model = Tool.CreateModel("content");
                        Tool.UseModel(model);
                        return model.GetValue();
                    }
                }
            }
            """;
        var text = new NpmValueType("string");
        var handle = new NpmValueType("handle", Name: "ITextModel", TypeDefinition: new("tool/index.d.ts", 10, 10));
        var model = handle with { Members = [new("getValue", new("method", Signatures: [new([], text)]), true)] };
        NpmValueExport Export(string name, NpmValueSignature signature) =>
            new("tool", name, "tool", "1.0.0", "index.js", "index.d.ts", null, [signature], null);
        var options = new JoltCompileOptions
        {
            NpmValues = [
                Export("createModel", new([new("value", text, false)], model)),
                Export("useModel", new([new("value", handle, false)], new("void"))),
            ],
        };
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules.Single().Content, "model.getValue()");
        var invalid = JoltProjectCompiler.Analyze([new("App.jazor", source.Replace("Tool.UseModel(model)", """Tool.UseModel("content")"""))], options);
        Assert.AreEqual("CS1503", invalid.Diagnostics.Single().Id);
    }
}
