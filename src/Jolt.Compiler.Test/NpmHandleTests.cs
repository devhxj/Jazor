using Jazor.Npm;
using Jolt.Npm;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class NpmHandleTests
{
    [TestMethod]
    public async Task Compile_PreservesEnrichedRefAndItsClosedWriteDomain()
    {
        var domain = new NpmValueType("stringEnum", Name: "Color", StringLiterals: ["dark", "light"]);
        var enriched = new NpmValueType("enrichedRef", ElementType: domain, Members: [
            new("system", domain, true),
        ]);
        var scalar = new NpmValueExport("tool", "mode", "tool", "1.0.0", "index.js", "index.d.ts", null,
            [new([], enriched)], null);
        var options = new JoltCompileOptions { NpmValues = [scalar] };
        const string source = """
            @using ECMAScript
            @using Tool = Npm.Tool.Exports
            @code {
                private Tool.Color Update() {
                    var mode = Tool.Mode();
                    Vue.IVueRef<Tool.Color> writable = mode;
                    writable.Value = Tool.Color.Dark;
                    return mode.System;
                }
            }
            """;
        var compiled = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(compiled.Diagnostics, string.Join(Environment.NewLine, compiled.Diagnostics));
        var js = compiled.Modules.Single().Content;
        StringAssert.Contains(js, "let writable = mode");
        StringAssert.Contains(js, "writable.value = \"dark\"");
        StringAssert.Contains(js, "return mode.system");
        var invalid = source.Replace("Tool.Color.Dark", "\"sepia\"", StringComparison.Ordinal);
        Assert.IsTrue(JoltProjectCompiler.Analyze([new("App.jazor", invalid)], options).Diagnostics.Any(diagnostic => diagnostic.Id == "CS0029"));
    }

    [TestMethod]
    public async Task Compile_SharesNamedRefDomainAcrossModules()
    {
        var domain = new NpmValueType("stringEnum", Name: "SwitchState", StringLiterals: ["enabled", "disabled"],
            TypeDefinition: new("C:/repo/node_modules/source/types.d.ts", 1, 5), TypeIdentity: "source@1.0.0/types.d.ts/SwitchState");
        var reference = new NpmValueType("ref", ElementType: domain);
        var producer = new NpmValueExport("source", "read", "source", "1.0.0", "index.js", "index.d.ts", null,
            [new([], reference)], null);
        var consumer = new NpmValueExport("target", "save", "target", "1.0.0", "index.js", "index.d.ts", null,
            [new([new("value", reference, false)], reference)], null);

        static string[] EnumNames(params NpmValueExport[] exports) =>
            Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(NpmBindings.GenerateSource([], exports))
                .GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.EnumDeclarationSyntax>()
                .Select(declaration => declaration.Identifier.ValueText).ToArray();

        var enumName = EnumNames(producer, consumer).Single();
        Assert.AreEqual(enumName, EnumNames(consumer).Single());
        var otherDomain = domain with { TypeIdentity = "source@2.0.0/types.d.ts/SwitchState" };
        var otherReference = reference with { ElementType = otherDomain };
        var otherProducer = producer with { ModuleSpecifier = "other", Signatures = [new([], otherReference)] };
        Assert.AreEqual(2, EnumNames(producer, consumer, otherProducer).Distinct(StringComparer.Ordinal).Count());

        var source = $$"""
            @using Source = Npm.Source.Exports
            @using Target = Npm.Target.Exports
            @using State = Jolt.Generated.Npm.Exports.{{enumName}}
            @code {
                private bool Update() {
                    var value = Source.Read();
                    value.Value = State.Disabled;
                    return Target.Save(value).Value == State.Disabled;
                }
            }
            """;
        var compiled = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], new() { NpmValues = [producer, consumer] });
        Assert.IsEmpty(compiled.Diagnostics, string.Join(Environment.NewLine, compiled.Diagnostics));
        StringAssert.Contains(compiled.Modules.Single().Content, "value.value = \"disabled\"");

        var consumerOnly = source.Replace("var value = Source.Read();", "", StringComparison.Ordinal)
            .Replace("@using Source = Npm.Source.Exports", "", StringComparison.Ordinal)
            .Replace("Update()", "Update(global::ECMAScript.Vue.IVueRef<State> value)", StringComparison.Ordinal);
        var standalone = await JoltProjectCompiler.CompileAsync([new("App.jazor", consumerOnly)], new() { NpmValues = [consumer] });
        Assert.IsEmpty(standalone.Diagnostics, string.Join(Environment.NewLine, standalone.Diagnostics));
    }

    [TestMethod]
    public async Task Compile_SharesNamedAugmentedRefAndBindsItsMethods()
    {
        var read = new NpmValueType("typeParameter", Name: "TRead");
        var auxiliary = new NpmValueType("typeParameter", Name: "TAuxiliary");
        var text = new NpmValueType("string");
        var flag = new NpmValueType("boolean");
        var reference = new NpmValueType("enrichedRef", Name: "Signal", ElementType: read,
            TypeDefinition: new("C:/repo/node_modules/source/types.d.ts", 1, 5),
            TypeIdentity: "source@1.0.0/types.d.ts/Signal",
            TypeParameters: ["TRead", "TAuxiliary"], TypeArguments: [text, flag], Members: [
                new("updateFrom", new("method", Signatures: [new([new("value", auxiliary, false)], read)]), true),
            ]);
        var producer = new NpmValueExport("source", "read", "source", "1.0.0", "index.js", "index.d.ts", null,
            [new([], reference)], null);
        var consumer = new NpmValueExport("target", "save", "target", "1.0.0", "index.js", "index.d.ts", null,
            [new([new("value", reference, false)], reference)], null);

        const string source = """
            @using Source = Npm.Source.Exports
            @using Target = Npm.Target.Exports
            @code {
                private string Update() {
                    var value = Source.Read();
                    value.Value = "pending";
                    return Target.Save(value).UpdateFrom(true);
                }
            }
            """;
        var options = new JoltCompileOptions { NpmValues = [producer, consumer] };
        var compiled = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(compiled.Diagnostics, string.Join(Environment.NewLine, compiled.Diagnostics));
        StringAssert.Contains(compiled.Modules.Single().Content, ".updateFrom(true)");
        var wrongArguments = reference with { TypeArguments = [flag, text] };
        var incompatible = consumer with { Signatures = [new([new("value", wrongArguments, false)], reference)] };
        var invalid = JoltProjectCompiler.Analyze([new("App.jazor", source)], options with { NpmValues = [producer, incompatible] });
        Assert.IsTrue(invalid.Diagnostics.Any(diagnostic => diagnostic.Id == "CS1503"));

        var standaloneSource = NpmBindings.GenerateSource([], [consumer]);
        var declaration = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(standaloneSource)
            .GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .Single(type => type.Identifier.ValueText.StartsWith("Signal_", StringComparison.Ordinal));
        var standaloneConsumer = source.Replace("@using Source = Npm.Source.Exports",
                $"@using Signal = Jolt.Generated.Npm.Exports.{declaration.Identifier.ValueText}<string, bool>", StringComparison.Ordinal)
            .Replace("Update()", "Update(Signal value)", StringComparison.Ordinal)
            .Replace("var value = Source.Read();", "", StringComparison.Ordinal);
        var standalone = await JoltProjectCompiler.CompileAsync([new("App.jazor", standaloneConsumer)],
            options with { NpmValues = [consumer] });
        Assert.IsEmpty(standalone.Diagnostics, string.Join(Environment.NewLine, standalone.Diagnostics));
    }

    [TestMethod]
    public async Task Compile_PreservesKeysWhenProjectedMemberNamesCollide()
    {
        var boolean = new NpmValueType("boolean");
        var shape = new NpmValueType("object", Members: [
            new("clone", boolean, false),
            new("cloneValue", boolean, false),
        ]);
        var export = new NpmValueExport("tool", "save", "tool", "1.0.0", "index.js", "index.d.ts", null,
            [new([new("options", shape, false)], new("void"))], null);
        const string source = """
            @using Tool = Npm.Tool.Exports
            @code {
                private void Save() => Tool.Save(new() { CloneValue_2 = true, CloneValue = false });
            }
            """;
        var facade = NpmBindings.GenerateSource([], [export]);
        StringAssert.Contains(facade, "@CloneValue_2");
        StringAssert.Contains(facade, "@CloneValue {");
        var compiled = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], new() { NpmValues = [export] });
        Assert.IsEmpty(compiled.Diagnostics, string.Join(Environment.NewLine, compiled.Diagnostics));
        StringAssert.Contains(compiled.Modules.Single().Content, "{ clone: true, cloneValue: false }");
    }

    [TestMethod]
    public async Task Compile_CorrelatesKeyedOptionsWithSelectedValue()
    {
        var record = new NpmValueType("recordTypeParameter", Name: "TRecord");
        var slot = new NpmValueType("typeParameter", Name: "TValue");
        NpmValueSignature Signature(NpmValueType value, bool passive)
        {
            var options = new NpmValueType("object", Members: [
                new("passive", new("boolean"), false, Optional: !passive, FixedBoolean: passive),
                new("defaultValue", value, false, Optional: true),
                new("shouldEmit", new("function", Signatures: [new([new("value", value, false)], new("boolean"))]), false, Optional: true),
            ]);
            return new([
                new("props", record, false),
                new("key", new("propertyKey", ElementType: record, Variants: [value]), false),
                new("emit", new("void"), false, FixedUndefined: true),
                new("options", options, false),
            ], new("ref", ElementType: value),
                TypeParameters: passive ? ["TRecord"] : ["TRecord", "TValue"],
                ManagedNameSuffix: passive ? "WithPassiveTrue" : null);
        }
        var export = new NpmValueExport("tool", "model", "tool", "1.0.0", "index.js", "index.d.ts", null,
            [Signature(slot, false), Signature(new("number"), true), Signature(new("boolean"), true)], null);
        var facade = NpmBindings.GenerateSource([], [export]);
        StringAssert.Contains(facade, """ECMAScriptInline("\"model\"(__arg1, __arg2, void 0, __arg3)")""");
        var options = new JoltCompileOptions { NpmValues = [export] };
        const string source = """
            @using ECMAScript
            @using Tool = Npm.Tool.Exports
            @code {
                private record Props : Vue.VueProps {
                    public double Count { get; init; }
                    public bool Flag { get; init; }
                }
                private void Update(Props props) {
                    Tool.Model(props, p => p.Count, options: new() { DefaultValue = 0, ShouldEmit = value => value > 1 });
                    Tool.ModelWithPassiveTrue(props, p => p.Flag, options: new() { DefaultValue = false });
                }
            }
            """;
        var compiled = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(compiled.Diagnostics, string.Join(Environment.NewLine, compiled.Diagnostics));
        var js = compiled.Modules.Single().Content;
        StringAssert.Contains(js, "\"Count\", void 0, {");
        StringAssert.Contains(js, "passive: false,");
        StringAssert.Contains(js, "defaultValue: 0,");
        StringAssert.Contains(js, "shouldEmit: value =>");
        StringAssert.Contains(js, "\"Flag\", void 0, { passive: true, defaultValue: false }");
        var invalid = source.Replace("DefaultValue = 0", "DefaultValue = false", StringComparison.Ordinal);
        Assert.IsNotEmpty(JoltProjectCompiler.Analyze([new("App.jazor", invalid)], options).Diagnostics);
        var invalidCallback = source.Replace("value => value > 1", "(bool value) => value", StringComparison.Ordinal);
        Assert.IsNotEmpty(JoltProjectCompiler.Analyze([new("App.jazor", invalidCallback)], options).Diagnostics);
    }

    [TestMethod]
    public async Task Compile_CallableOverloadLowersReservedImportAndMethodGroupWithFixedSlots()
    {
        var text = new NpmValueType("string");
        var signature = new NpmValueSignature(
            [new("first", text, false), new("skip", text, false, FixedUndefined: true),
             new("enabled", new("boolean"), false, FixedBoolean: true), new("last", text, false)], text);
        var export = new NpmValueExport("tool", "default", "tool", "1.0.0", "index.js", "index.d.ts", null,
            [signature], null);
        const string source = """
            @using Tool = Npm.Tool.Exports
            @code {
                private string Call() => Tool.Default("one", "two");
                private string Forward(string first, string last) {
                    System.Func<string, string, string> method = Tool.Default;
                    return method(first, last);
                }
            }
            """;

        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], new() { NpmValues = [export] });

        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var script = result.Modules.Single().Content;
        StringAssert.Contains(script, "from \"tool\";");
        StringAssert.Contains(script, "(\"one\", void 0, true, \"two\")");
        Assert.AreEqual(2, System.Text.RegularExpressions.Regex.Matches(script, @", void 0, true, ").Count, script);
    }

    [TestMethod]
    public async Task Compile_PreservesWritableComputedRefIdentity()
    {
        var read = new NpmValueType("typeParameter", Name: "TRead");
        var write = new NpmValueType("typeParameter", Name: "TWrite");
        var computed = new NpmValueType("ref", Name: "VueWritableComputedRef", ElementType: read, WriteType: write, ReadOnly: true);
        NpmValueExport Export(string name, NpmValueSignature signature) =>
            new("tool", name, "tool", "1.0.0", "index.js", "index.d.ts", null, [signature], null);
        var split = computed with { ElementType = new("string"), WriteType = new("number") };
        var writable = split with { ReadOnly = false, WriteType = new("nullable", ElementType: new("string")) };
        var plain = split with { Name = null };
        var options = new JoltCompileOptions { NpmValues = [
            Export("forward", new([new("value", computed, false)], computed, TypeParameters: ["TRead", "TWrite"])),
            Export("split", new([], split)),
            Export("writable", new([], writable)),
            Export("plain", new([], plain)),
        ] };
        const string source = """
            @using ECMAScript
            @using Tool = Npm.Tool.Exports
            @code {
                private string Update(Vue.VueWritableComputedRef<string> symmetric) {
                    symmetric.Value = "first";
                    Tool.Forward(symmetric).WriteValue = "second";
                    Tool.Forward(Tool.Split()).WriteValue = 1;
                    var writable = Tool.Writable();
                    writable.Value = "value";
                    writable.WriteValue = null;
                    return Tool.Forward(writable).Value;
                }
            }
            """;
        var compiled = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(compiled.Diagnostics, string.Join(Environment.NewLine, compiled.Diagnostics));
        var js = compiled.Modules.Single().Content;
        StringAssert.Contains(js, "symmetric.value =");
        StringAssert.Contains(js, "writable.value = null");
        StringAssert.Contains(js, ").value = 1");
        var invalid = source.Replace("Tool.Forward(Tool.Split())", "Tool.Forward(Tool.Plain())", StringComparison.Ordinal);
        var diagnostics = JoltProjectCompiler.Analyze([new("App.jazor", invalid)], options).Diagnostics;
        Assert.IsTrue(diagnostics.Any(diagnostic => diagnostic.Id == "CS0411"), string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task Compile_ReadsAndWritesOptionalVueRefFromMetadata()
    {
        var number = new NpmValueType("number");
        var optional = new NpmValueType("ref", Name: "VueOptionalRef", ElementType: new("nullable", ElementType: number));
        var export = new NpmValueExport("tool", "optional", "tool", "1.0.0", "index.js", "index.d.ts", null, [new([], optional)], null);
        var options = new JoltCompileOptions { NpmValues = [export] };
        const string source = """
            @using Tool = Npm.Tool.Exports
            @code {
                private double? Update() {
                    var value = Tool.Optional();
                    value.Value = 0;
                    value.Value = null;
                    return value.Value;
                }
            }
            """;
        var compiled = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(compiled.Diagnostics, string.Join(Environment.NewLine, compiled.Diagnostics));
        var js = compiled.Modules.Single().Content;
        StringAssert.Contains(js, "value.value = 0");
        StringAssert.Contains(js, "value.value = null");
        StringAssert.Contains(js, """return value["value"] ?? null""");
    }

    [TestMethod]
    public async Task Compile_UsesSameVuePropertyForAsymmetricReadsAndWrites()
    {
        var number = new NpmValueType("number");
        var writable = new NpmValueType("ref", ElementType: number, WriteType: new("nullable", ElementType: number));
        var split = new NpmValueType("ref", ElementType: new("string"), ReadOnly: true, WriteType: number);
        NpmValueExport Export(string name, NpmValueType result) =>
            new("tool", name, "tool", "1.0.0", "index.js", "index.d.ts", null, [new([], result)], null);
        var options = new JoltCompileOptions { NpmValues = [Export("writable", writable), Export("split", split)] };
        const string source = """
            @using Tool = Npm.Tool.Exports
            @code {
                private string Update() {
                    var counter = Tool.Writable();
                    counter.Value = 0;
                    counter.WriteValue = null;
                    var separate = Tool.Split();
                    separate.WriteValue = 2;
                    return separate.Value;
                }
            }
            """;
        var compiled = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(compiled.Diagnostics, string.Join(Environment.NewLine, compiled.Diagnostics));
        var js = compiled.Modules.Single().Content;
        StringAssert.Contains(js, "counter.value = 0");
        StringAssert.Contains(js, "counter.value = null");
        StringAssert.Contains(js, "separate.value = 2");
        StringAssert.Contains(js, "return separate.value");
        var invalid = source.Replace("separate.WriteValue = 2;", """separate.Value = "invalid";""", StringComparison.Ordinal);
        var diagnostics = JoltProjectCompiler.Analyze([new("App.jazor", invalid)], options).Diagnostics;
        Assert.IsTrue(diagnostics.Any(diagnostic => diagnostic.Id == "CS0200"), string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task Compile_ErasesNullableValueCarrierFromMetadata()
    {
        const string source = """
            @using ECMAScript
            @code {
                private NullableValue<double>? Zero() => 0d;
                private NullableValue<bool>? False() => false;
                private NullableValue<string>? Missing() => null;
                private string ReadReference(NullableValue<string>? value) =>
                    value is null ? "missing" : value.Value;
                private bool ReadBoolean(NullableValue<bool>? value) =>
                    value is not null && value.Value;
            }
            """;
        var compiled = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], new());
        Assert.IsEmpty(compiled.Diagnostics, string.Join(Environment.NewLine, compiled.Diagnostics));
        var js = compiled.Modules.Single().Content;
        StringAssert.Contains(js, "return 0;");
        StringAssert.Contains(js, "return false;");
        StringAssert.Contains(js, "return null;");
        StringAssert.Contains(js, """value == null ? "missing" : value""");
        StringAssert.Contains(js, "!(value == null) && value");
        Assert.IsFalse(js.Contains(".value", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task Compile_KeepsDistinctGenericIdentifiersAndEscapesKeywordParameters()
    {
        var input = new NpmValueType("typeParameter", Name: "_T");
        var output = new NpmValueType("typeParameter", Name: "$T");
        var signature = new NpmValueSignature([new("class", input, false), new("record", output, false)],
            output, TypeParameters: ["_T", "$T"], ValueTypeParameters: ["$T"]);
        var export = new NpmValueExport("tool", "pick", "tool", "1.0.0", "index.js", "index.d.ts", null, [signature], null);
        const string source = """
            @using Tool = Npm.Tool.Exports
            <p>@Count</p>
            @code {
                private double Count => Tool.Pick(@class: "label", @record: 2.0);
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], new() { NpmValues = [export] });
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules.Single().Content, "\"label\", 2");
    }

    [TestMethod]
    public async Task Compile_InsertsFixedBooleanForCallsAndMethodGroups()
    {
        var type = new NpmValueType("typeParameter", Name: "T");
        var signature = new NpmValueSignature([
            new("value", type, false),
            new("enabled", new("boolean"), false, FixedBoolean: true),
            new("label", new("string"), true),
        ], type, TypeParameters: ["T"], ManagedNameSuffix: "WithEnabledTrue");
        var export = new NpmValueExport("tool", "read", "tool", "1.0.0", "index.js", "index.d.ts", null, [signature], null);
        var trailing = export with
        {
            ExportName = "write",
            Signatures = [signature with { Parameters = [signature.Parameters[0], signature.Parameters[2], signature.Parameters[1]] }],
        };
        const string source = """
            @using Tool = Npm.Tool.Exports
            @code {
                private double Direct() => Tool.ReadWithEnabledTrue(1.0);
                private double Trailing() => Tool.WriteWithEnabledTrue(1.0);
                private System.Func<double, string, double> Bind() => Tool.ReadWithEnabledTrue<double>;
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], new() { NpmValues = [export, trailing] });
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "(1, true)");
        StringAssert.Contains(js, "(1, void 0, true)");
        StringAssert.Contains(js, "(__jz_fixedArg0, true, __jz_fixedArg1)");
        StringAssert.Contains(NpmBindings.GenerateSource([], [export]), "ECMAScriptFixedBooleanArgument(1, true)");
    }

    [TestMethod]
    public async Task Compile_ComposesMatchingMappedRecordResultsOnly()
    {
        var definition = new NpmDefinition("C:/repo/node_modules/sample/model.d.ts", 1, 5);
        var count = new NpmValueType("object", Name: "Patch", TypeDefinition: definition,
            TypeArguments: [new("string")], Members: [new("count", new("number"), false, Optional: true)],
            TypeIdentity: "sample/Patch/count");
        var otherKey = count with { TypeIdentity = "sample/Patch/label", Members = [new("label", new("string"), false, Optional: true)] };
        var otherArgument = count with { TypeArguments = [new("number")] };
        NpmValueExport Export(string name, NpmValueSignature signature) =>
            new("sample", name, "sample", "1.0.0", "index.js", "index.d.ts", null, [signature], null);
        var exports = new[] {
            Export("pickCount", new([], count)),
            Export("pickLabel", new([], otherKey)),
            Export("pickNumericCount", new([], otherArgument)),
            Export("acceptCount", new([new("patch", count, false)], new("number"))),
        };
        const string source = """
            @using Tool = Npm.Sample.Exports
            @code {
                private double Compose() => Tool.AcceptCount(Tool.PickCount());
                private double Construct() => Tool.AcceptCount(new() { Count = 2 });
                private double? Read() => Tool.PickCount().Count;
            }
            """;
        var compiled = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], new() { NpmValues = exports });
        Assert.IsEmpty(compiled.Diagnostics, string.Join(Environment.NewLine, compiled.Diagnostics));
        var generated = NpmBindings.GenerateSource([], exports);
        Assert.AreEqual(2, generated.Split("@AcceptCount(", StringSplitOptions.None).Length - 1);
    }

    [TestMethod]
    public async Task Compile_ComposesMappedRecordWithRenamedAndPermutedTypeParameters()
    {
        NpmValueType Slot(string name) => new("typeParameter", Name: name);
        NpmValueType Patch(NpmValueType first, NpmValueType second) => new("object", Name: "Patch",
            TypeDefinition: new("C:/repo/node_modules/sample/model.d.ts", 1, 5), TypeIdentity: "sample/Patch/count",
            TypeArguments: [first, second], Members: [new("count", new("number"), false, Optional: true)]);
        NpmValueExport Export(string name, NpmValueSignature signature) =>
            new("sample", name, "sample", "1.0.0", "index.js", "index.d.ts", null, [signature], null);
        var exports = new[] {
            Export("pick", new([], Patch(Slot("TProducer"), Slot("UProducer")), TypeParameters: ["TProducer", "UProducer"])),
            Export("pickRepeated", new([], Patch(Slot("TProducer"), Slot("TProducer")), TypeParameters: ["TProducer"])),
            Export("accept", new([new("patch", Patch(Slot("UConsumer"), Slot("TConsumer")), false)], new("number"),
                TypeParameters: ["TConsumer", "UConsumer"])),
        };
        const string source = """
            @using Tool = Npm.Sample.Exports
            @code {
                private double Compose() => Tool.Accept<double, string>(Tool.Pick<string, double>());
            }
            """;
        var compiled = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], new() { NpmValues = exports });
        Assert.IsEmpty(compiled.Diagnostics, string.Join(Environment.NewLine, compiled.Diagnostics));
        var generated = NpmBindings.GenerateSource([], exports);
        Assert.AreEqual(2, generated.Split("@Accept<", StringSplitOptions.None).Length - 1);
        StringAssert.Contains(generated, "PatchResult<@UConsumer, @TConsumer> @patch");
    }

    [TestMethod]
    public async Task Compile_KeepsVueRefReachedThroughRecursiveTypeArgumentSwap()
    {
        var first = new NpmValueType("typeParameter", Name: "TFirst");
        var second = new NpmValueType("typeParameter", Name: "TSecond");
        var number = new NpmValueType("number");
        var vueRef = new NpmValueType("ref", ElementType: number);
        var recursive = new NpmValueType("namedRecord", Name: "Node", TypeDefinition: new("C:/repo/node_modules/sample/model.d.ts", 1, 5),
            TypeParameters: ["TFirst", "TSecond"], TypeArguments: [second, first], TypeIdentity: "sample/Node");
        var root = recursive with
        {
            TypeArguments = [number, vueRef],
            Members = [
                new("value", first, true),
                new("children", new("array", ElementType: recursive, ReadOnly: true), true),
            ],
        };
        var make = new NpmValueExport("sample", "make", "sample", "1.0.0", "index.js", "index.d.ts", null, [new([], root)], null);
        var rootType = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(NpmBindings.GenerateSource([], [make]))
            .GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .Single().ReturnType.ToString();
        const string source = """
            @using Tool = Npm.Sample.Exports
            <p>@Root.Children[0].Value.Value</p>
            @code {
                private ROOT_TYPE Root = null!;
                [Setup]
                private void Configure() => Root = Tool.Make();
            }
            """;

        var result = await JoltProjectCompiler.CompileAsync(
            [new("App.jazor", source.Replace("ROOT_TYPE", rootType, StringComparison.Ordinal))], new() { NpmValues = [make] });
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var module = result.Modules.Single().Content;
        StringAssert.Contains(module, "shallowReactive as $joltShallowReactive");
        StringAssert.Contains(module, """$joltShallowReactive($joltToRaw(this))["$jazor$private$Root$""");
        StringAssert.Contains(module, ".value");
    }

    [TestMethod]
    public void Projection_SharesOpenRecordAcrossExportsAndRefResults()
    {
        var formal = new NpmValueType("typeParameter", Name: "TItem");
        var argument = new NpmValueType("typeParameter", Name: "TValue");
        var shape = new NpmValueType("namedRecord", Name: "Envelope",
            TypeDefinition: new("C:/repo/node_modules/sample/model.d.ts", 1, 5),
            TypeParameters: ["TItem"], TypeArguments: [argument], TypeIdentity: "sample/Envelope", Members: [
                new("item", formal, true),
                new("other", formal, false),
            ]);
        var wrap = new NpmValueExport("sample", "wrap", "sample", "1.0.0", "index.js", "index.d.ts", null,
            [new([new("value", argument, false)], new("ref", ElementType: shape, ReadOnly: true), TypeParameters: ["TValue"])], null);
        var read = wrap with
        {
            ModuleSpecifier = "sample/read",
            ExportName = "read",
            Signatures = [new([new("input", shape, false)], argument, TypeParameters: ["TValue"])],
        };
        var source = NpmBindings.GenerateSource([], [wrap, read]);
        Assert.AreEqual(source, NpmBindings.GenerateSource([], [read, wrap]));
        Assert.AreEqual(2, source.Split("public sealed record Envelope_", StringSplitOptions.None).Length);
        StringAssert.Contains(source, "VueReadonlyRef<global::Jolt.Generated.Npm.Exports.Envelope_");
        StringAssert.Contains(source, "required @TItem @Item { get; init; }");
        StringAssert.Contains(source, "required @TItem @Other { get; set; }");
        StringAssert.Contains(source, "<@TValue> @input");
    }

    [TestMethod]
    public void Projection_CompilesIndependentFilesTogether()
    {
        NpmValueExport Export(string package, string type) =>
            new(package, "read", package, "1.0.0", "index.js", "index.d.ts", null, [new([], new("handle",
                Name: type, TypeDefinition: new($"C:/repo/node_modules/{package}/index.d.ts", 1, 5)))], null);
        var first = NpmBindings.GenerateSource([], [Export("first", "First")]);
        var second = NpmBindings.GenerateSource([], [Export("second", "Second")]);
        var scalar = Export("scalar", "Unused") with { Signatures = [new([], new("string"))] };
        var third = NpmBindings.GenerateSource([], [scalar]);
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
            Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(NpmBindings.GenerateSource([], exports))
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
            Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(NpmBindings.GenerateSource([], exports))
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
    public async Task Compile_ProjectsDependentPathWithExactFieldTypeAndOneReceiverCall()
    {
        var field = new NpmValueType("typeParameter", Name: "TField");
        var root = new NpmValueType("recordTypeParameter", Name: "TForm");
        var setter = new NpmValueSignature([
            new("field", new("propertyPath", ElementType: root, Variants: [field], VeeValidatePath: true), false),
            new("value", field, false),
            new("validate", new("boolean"), true),
        ], new("void"), TypeParameters: ["TField"]);
        var export = new NpmValueExport("tool", "openForm", "tool", "1.0.0", "index.js", "index.d.ts", null,
            [new([], new("object", Members: [new("update", new("method", Signatures: [setter]), true)]), TypeParameters: ["TForm"])], null);
        const string source = """
            @using Tool = Npm.Tool.Exports
            @code {
                public record Details(
                    [property: ECMAScript.ECMAScriptName("address")] string Address,
                    string? Optional,
                    double Count,
                    [property: ECMAScript.ECMAScriptName("real.key")] string Split);
                public record Input([property: ECMAScript.ECMAScriptName("contact")] Details? Contact,
                    [property: ECMAScript.ECMAScriptName("flat.key")] double Dotted,
                    [property: ECMAScript.ECMAScriptName("flat[label]")] string Bracketed,
                    [property: ECMAScript.ECMAScriptName("flat[0]")] string Indexed) : ECMAScript.Vue.VueProps;
                private void Write(Input other, string? maybe) {
                    var form = Tool.OpenForm<Input>();
                    form.Update(field => field.Contact!.Address, "written", false);
                    Tool.OpenForm<Input>().Update(field => field.Contact!.Count, 0);
                    form.Update(field => field.Contact!.Optional, null);
                    form.Update(field => field.Dotted, 0);
                    form.Update(field => field.Bracketed, "literal");
                    form.Update<string>(field => field.Contact!.Address, maybe ?? "fallback");
                }
            }
            """;
        var options = new JoltCompileOptions { NpmValues = [export] };
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, """form.update("contact.address", "written", false)""");
        StringAssert.Contains(js, """().update("contact.Count", 0)""");
        StringAssert.Contains(js, """form.update("contact.Optional", null)""");
        StringAssert.Contains(js, """form.update("[flat.key]", 0)""");
        StringAssert.Contains(js, """form.update("flat[label]", "literal")""");
        Assert.AreEqual(2, js.Split("().update(", StringSplitOptions.None).Length);

        var plainSetter = setter with { Parameters = [setter.Parameters[0] with
            { Type = setter.Parameters[0].Type with { VeeValidatePath = false } }, .. setter.Parameters.Skip(1)] };
        var plainSignature = export.Signatures[0] with { ReturnType = export.Signatures[0].ReturnType with
            { Members = [new("update", new("method", Signatures: [plainSetter]), true)] } };
        var plainExport = export with { Signatures = [plainSignature] };
        var plainOptions = options with { NpmValues = [plainExport] };
        var plain = JoltProjectCompiler.Analyze([new("App.jazor", source)], plainOptions);
        Assert.HasCount(2, plain.Diagnostics, string.Join(Environment.NewLine, plain.Diagnostics));
        Assert.IsTrue(plain.Diagnostics.All(diagnostic => diagnostic.Id == "JOLT2001" &&
            diagnostic.GetMessage().Contains("path segments", StringComparison.Ordinal)));

        const string call = "form.Update(field => field.Contact!.Address, \"written\", false);";
        foreach (var invalidCall in new[] {
            "form.Update<object>(field => field.Contact!.Address, true);",
            "form.Update<string?>(field => field.Contact!.Address, null);",
            "form.Update<string>(field => field.Contact!.Address, maybe);",
            "form.Update<string>(field => field.Contact!.Address, null);",
            "form.Update(field => other.Contact!.Address, \"captured\");",
            "form.Update(field => field.Contact!.Count + 1, 0);",
            "form.Update(field => field.Contact!.Split, \"wrong key\");",
            "form.Update(field => field.Indexed, \"wrong key\");",
        })
        {
            var invalid = JoltProjectCompiler.Analyze([new("App.jazor", source.Replace(call, invalidCall, StringComparison.Ordinal))], options);
            Assert.HasCount(1, invalid.Diagnostics, string.Join(Environment.NewLine, invalid.Diagnostics));
            Assert.AreEqual("JOLT2001", invalid.Diagnostics.Single().Id, invalid.Diagnostics.Single().GetMessage());
        }
    }

    [TestMethod]
    public async Task Compile_ProjectsLiteralPropertyKeyAndRejectsNestedSelectorOnCacheHit()
    {
        var root = new NpmValueType("typeParameter", Name: "TRecord");
        var value = new NpmValueType("typeParameter", Name: "TValue");
        var lookup = new NpmValueSignature([
            new("record", root, false),
            new("key", new("propertyKey", ElementType: root, Variants: [value]), false),
        ], value, TypeParameters: ["TRecord", "TValue"]);
        var export = new NpmValueExport("tool", "lookup", "tool", "1.0.0", "index.js", "index.d.ts", null, [lookup], null);
        var lens = export with
        {
            ExportName = "lens",
            Signatures = [lookup with { ReturnType = new("ref", Name: "VueComputedRef", ElementType: value, ReadOnly: true) }],
        };
        var facade = NpmBindings.GenerateSource([], [export, lens]);
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator).Select(path => Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(path));
        var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("KeySelectors",
            [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(facade)], references,
            new(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));
        using var dll = new MemoryStream();
        var emit = compilation.Emit(dll);
        Assert.IsTrue(emit.Success, string.Join(Environment.NewLine, emit.Diagnostics));
        var exports = System.Reflection.Assembly.Load(dll.ToArray()).GetType("Npm.Tool.Exports")!;
        bool IndirectRead(string name) => exports.GetMethod(name)!.GetParameters()[1].GetCustomAttributesData()
            .Single(attribute => attribute.AttributeType.Name == "ECMAScriptPropertyPathAttribute")
            .NamedArguments.Any(argument => argument.MemberName == "IndirectRead" && argument.TypedValue.Value is true);
        Assert.IsTrue(IndirectRead("Lens"));
        Assert.IsFalse(IndirectRead("Lookup"));
        const string source = """
            @using Tool = Npm.Tool.Exports
            @code {
                public record Detail(double Count);
                public record Input([property: ECMAScript.ECMAScriptName("field[0].name")] double Count, Detail Nested,
                    [property: ECMAScript.ECMAScriptOptional] double? Optional,
                    [property: ECMAScript.ECMAScriptRequiredRead] double Required);
                private double Read(Input input) => Tool.Lookup(input, field => field.Count);
                private double? ReadOptional(Input input) => Tool.Lookup(input, field => field.Optional);
            }
            """;
        var options = new JoltCompileOptions { NpmValues = [export, lens] };
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        StringAssert.Contains(result.Modules.Single().Content, """(input, "field[0].name")""");
        StringAssert.Contains(result.Modules.Single().Content, """(input, "Optional") ?? null""");
        var required = source.Replace("field.Count", "field.Required", StringComparison.Ordinal);
        var invalidRequired = JoltProjectCompiler.Analyze([new("App.jazor", required)], options);
        Assert.AreEqual("JOLT2001", invalidRequired.Diagnostics.Single().Id);
        StringAssert.Contains(invalidRequired.Diagnostics.Single().GetMessage(), "Read 'Required' directly");

        var nested = source.Replace("field.Count", "field.Nested.Count", StringComparison.Ordinal);
        var invalid = JoltProjectCompiler.Analyze([new("App.jazor", nested)], options);
        Assert.HasCount(1, invalid.Diagnostics, string.Join(Environment.NewLine, invalid.Diagnostics));
        Assert.AreEqual("JOLT2001", invalid.Diagnostics.Single().Id);
        StringAssert.Contains(invalid.Diagnostics.Single().GetMessage(), "exactly one structural record data property");
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
                        double? number = Tool.Invoke(new Tool.InvokeNumberValueOptionsInput<bool> { Load = variable => variable ? 0 : 1 }).Data.Value;
                        bool? flag = Tool.Invoke(new Tool.InvokeBooleanValueOptionsInput<double> { Load = variable => variable > 0 }).Data.Value;
                        double? supplied = Tool.Invoke(new Tool.InvokeBooleanValueOptionsInput<double> { Load = variable => variable > 0 }).Supplied.Value;
                        string? reference = Tool.Invoke(new Tool.InvokeNumberOptionsInput<string> { Load = variable => 0 }).Supplied.Value;
                        double? withReferenceResult = Tool.Invoke(new Tool.InvokeValueOptionsInput<string, double> { Load = variable => "saved" }).Supplied.Value;
                        return number == null && flag == null && supplied == null && reference == null && withReferenceResult == null;
                    }
                }
            }
            """;
        var data = new NpmValueType("typeParameter", Name: "TData");
        var variable = new NpmValueType("typeParameter", Name: "TVariable");
        var callback = new NpmValueType("function", Signatures: [new([new("variable", variable, false)], data)]);
        var input = new NpmValueType("object", Members: [new("load", callback, true)]);
        var optional = new NpmValueType("ref", ReadOnly: true, ElementType: new("undefinedable", ElementType: data));
        var supplied = optional with { ElementType = new("undefinedable", ElementType: variable) };
        var output = new NpmValueType("object", Members: [new("data", optional, true), new("supplied", supplied, true)]);
        var signature = new NpmValueSignature([new("options", input, false)], output,
            TypeParameters: ["TData", "TVariable"], ReferenceTypeParameters: ["TData", "TVariable"],
            ScalarResultTypeParameter: "TData");
        var options = new JoltCompileOptions
        {
            NpmValues = [new("tool", "invoke", "tool", "1.0.0", "index.js", "index.d.ts", null, [signature], null)],
        };
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        Assert.AreEqual(5, js.Split("""["value"] ?? null""").Length - 1);
        StringAssert.Contains(js, "variable ? 0 : 1");
        StringAssert.Contains(js, "variable > 0");
        StringAssert.Contains(js, "number === null && flag === null");

        var invalid = JoltProjectCompiler.Analyze([new("App.jazor", source.Replace("variable > 0", "variable + 1"))], options);
        Assert.IsTrue(invalid.Diagnostics.Any(diagnostic => diagnostic.Id == "CS0029"));
        var wrongVariable = JoltProjectCompiler.Analyze(
            [new("App.jazor", source.Replace("double? supplied", "double supplied", StringComparison.Ordinal))], options);
        Assert.IsTrue(wrongVariable.Diagnostics.Any(diagnostic => diagnostic.Id == "CS0266"));
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
        const string storagePrefix = """$joltShallowReactive($joltToRaw(this))["$jazor$private$Data$""";
        var storageStart = js.IndexOf(storagePrefix, StringComparison.Ordinal);
        Assert.IsTrue(storageStart >= 0, js);
        var storageEnd = js.IndexOf("\"]", storageStart + storagePrefix.Length, StringComparison.Ordinal) + 2;
        var storage = js[storageStart..storageEnd];
        StringAssert.Contains(js, storage[(storage.IndexOf('"') + 1)..^2] + " = i$");
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
