using DenoHost.Core;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class ErasedUnionProjectionTests
{
    [TestMethod]
    public async Task Convert_AnyNamedUnion_UsesBranchTypesForNullableFlatAndNestedProjections()
    {
        var tree = CSharpSyntaxTree.ParseText("""
            using ECMAScript;
            using static ECMAScript.Vue;
            [ECMAScript] public readonly union Scalar(bool, double, string)
            {
                public bool? AsBool => Value is bool value ? value : default(bool?);
                public double? AsNumber => Value is double value ? value : default(double?);
                public string? AsString => Value as string;
            }
            [ECMAScript] public readonly union Nested(Scalar, Scalar[])
            {
                public Scalar? AsSingle => Value is Scalar value ? value : default(Scalar?);
                public Scalar[]? AsMultiple => Value as Scalar[];
                public double? AsNumber => AsSingle?.AsNumber;
                public string? AsString => AsSingle?.AsString;
            }
            [ECMAScript, String] public enum Status
            {
                [ECMAScriptName("")] Empty,
                [ECMAScriptName("ok")] Ready
            }
            [ECMAScript] public readonly union State(Status, double)
            {
                public Status? AsStatus => Value is Status value ? value : default(Status?);
            }
            [ECMAScript, String] public enum EmptyStatus { }
            [ECMAScript] public readonly union EmptyState(EmptyStatus, double)
            {
                public EmptyStatus? AsStatus => Value is EmptyStatus value ? value : default(EmptyStatus?);
            }
            [ECMAScript] public readonly union ObjectLike(VueProps, Date, Map<string, string>, Set<string>, Error)
            {
                public VueProps? AsProps => Value as VueProps;
            }
            [ECMAScript] public readonly union Envelope<T>(T, bool)
            {
                public T? AsPayload => Value is T value ? value : default;
            }
            [ECMAScriptModule("./projections.mjs")]
            public static class Projections
            {
                private static int Calls = 0;
                private static Nested Next() { Calls++; return new((Scalar)32); }
                public static double? Once() => Next().AsNumber;
                public static int CallCount() => Calls;
                public static double? Number(Nested? value) => value?.AsNumber;
                public static string? Text(Nested? value) => value?.AsString;
                public static bool? Boolean(Scalar value) => value.AsBool;
                public static double? NestedNumber(Nested value) => value.AsSingle?.AsNumber;
                public static Scalar[]? ReadArray(Nested value) => value.AsMultiple;
                public static double? OtherVueUnion(VueBooleanNumberValue value) => value.AsNumber;
                public static string? OtherVueText(VueStringNumberDateValue value) => value.AsString;
                public static Date? ReadDate(VueStringNumberDateValue value) => value.AsDate;
                public static VueProps? ReadObject(VueStringNumberObjectValue value) => value.AsProps;
                public static Status? ReadStatus(State value) => value.AsStatus;
                public static EmptyStatus? ReadEmptyStatus(EmptyState value) => value.AsStatus;
                public static VueProps? ReadObjectLike(ObjectLike value) => value.AsProps;
                public static T? ReadAnnotation<T>(Envelope<T> value) => value.AsPayload;
                public static ECMAScript.Vuetify.VuetifyBorderValue NumericBridge(int value) => value;
            }
            """, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create("GenericUnionProjection", [tree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Vuetify.VAvatar).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(d => d.ToString())));
        var converter = new AstConverter(compilation.GetTypeByMetadataName("Projections")!, compilation.GetSemanticModel(tree));
        var module = await converter.Convert();
        var script = module!.ToKnRECMAScript();
        var root = Path.Combine(RepositoryTemp.Root, "generic-union-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "projections.mjs"), script);
            await File.WriteAllTextAsync(Path.Combine(root, "projections.test.mjs"), """
                import assert from "node:assert/strict";
                import * as p from "./projections.mjs";
                Deno.test("all named host unions keep numeric/string/bool/array branches", () => {
                    assert.equal(p.Number(0), 0);
                    assert.equal(p.Number("32"), null);
                    assert.equal(p.Text(32), null);
                    assert.equal(p.Text("32"), "32");
                    assert.equal(p.Boolean(false), false);
                    assert.equal(p.Boolean(0), null);
                    assert.equal(p.NestedNumber(32), 32);
                    assert.equal(p.NestedNumber([]), null);
                    const values = [0, "32"];
                    assert.equal(p.ReadArray(values), values);
                    assert.equal(p.ReadArray("32"), null);
                    assert.equal(p.OtherVueUnion(false), null);
                    assert.equal(p.OtherVueUnion(0), 0);
                    assert.equal(p.OtherVueText(32), null);
                    const date = new Date(0);
                    assert.equal(p.ReadDate(date), date);
                    assert.equal(p.ReadDate(0), null);
                    const object = { id: "9007199254740993" };
                    assert.equal(p.ReadObject(object), object);
                    assert.equal(p.ReadObject([]), null);
                    assert.equal(p.ReadObject(false), null);
                    assert.equal(p.ReadStatus("ok"), "ok");
                    assert.equal(p.ReadStatus(""), "");
                    assert.equal(p.ReadStatus("unknown"), null);
                    assert.equal(p.ReadStatus(0), null);
                    assert.equal(p.ReadEmptyStatus(0), null);
                    assert.equal(p.ReadEmptyStatus(""), null);
                    assert.equal(p.ReadObjectLike(object), object);
                    for (const other of [date, new Map(), new Set(), new Error("other branch"), []])
                        assert.equal(p.ReadObjectLike(other), null);
                    assert.equal(p.ReadAnnotation(object), object, "open generic projection remains an erased annotation");
                    assert.equal(p.NumericBridge(32), 32);
                    assert.equal(p.Once(), 32);
                    assert.equal(p.CallCount(), 1, "union projection must evaluate its receiver once");
                    for (const absent of [null, undefined]) assert.equal(p.Number(absent) ?? null, null);
                });
                """);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", "projections.test.mjs"], timeout.Token);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [TestMethod]
    [DataRow("Second")]
    [DataRow("IObject")]
    public async Task Convert_ObjectLiteralUnionWithoutRuntimeDiscriminator_RejectsAmbiguousProjection(string otherType)
    {
        var tree = CSharpSyntaxTree.ParseText($$"""
            using ECMAScript;
            using System.ComponentModel;
            [ECMAScript, Description("@#")] public sealed class First { public string Name { get; set; } = ""; }
            [ECMAScript, Description("@#")] public sealed class Second { public string Name { get; set; } = ""; }
            [ECMAScript] public readonly union Ambiguous(First, {{otherType}})
            {
                public First? AsFirst => Value as First;
            }
            [ECMAScriptModule("./ambiguous.mjs")] public static class Projection
            {
                public static First? Read(Ambiguous value) => value.AsFirst;
            }
            """, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create("AmbiguousUnionProjection", [tree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.IsEmpty(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        var converter = new AstConverter(compilation.GetTypeByMetadataName("Projection")!, compilation.GetSemanticModel(tree));
        var error = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(error.Message, "cannot distinguish");
        StringAssert.Contains(error.Message, "First");
        StringAssert.Contains(error.Message, otherType);
    }
}
