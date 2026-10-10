using DenoHost.Core;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class ErasedUnionInlineProjectionTests
{
    [TestMethod]
    public async Task Convert_AuthoredInlineUnionProjections_PreserveBranchesAndSingleEvaluation()
    {
        var tree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using static ECMAScript.Vue;

            [ECMAScript] public readonly union MappedValue(string, double)
            {
                public string? AsString
                {
                    [ECMAScriptInline("typeof __arg1 === 'string' ? 'mapped:' + __arg1 : null")]
                    get => Value is string value ? "mapped:" + value : null;
                }
            }

            [ECMAScriptModule("./projections.mjs")]
            public static class Projections
            {
                private static int Calls = 0;
                public static double? ReadNumber(VueBooleanStringNumberObjectArrayableValue? value)
                    => value?.AsSingle?.AsNumber;
                public static string? ReadString(VueBooleanStringNumberObjectArrayableValue? value)
                    => value?.AsSingle?.AsString;
                public static VueBooleanStringNumberObjectValue[]? ReadMultiple(VueBooleanStringNumberObjectArrayableValue? value)
                    => value?.AsMultiple;
                public static string? ReadEffectfulString() => NextValue().AsSingle?.AsString;
                private static VueBooleanStringNumberObjectArrayableValue NextValue()
                {
                    Calls++;
                    return "exact";
                }
                public static int ReadCalls() => Calls;
                public static string? ReadMapped(MappedValue value) => value.AsString;
            }
            """, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create("UnionProjectionTest", [tree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        var symbol = compilation.GetTypeByMetadataName("Projections")!;
        var converter = new AstConverter(symbol, compilation.GetSemanticModel(tree));
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();
        Assert.IsNotNull(script);
        StringAssert.Contains(script, "typeof", StringComparison.Ordinal);
        StringAssert.Contains(script, "Array.isArray", StringComparison.Ordinal);

        var root = Path.Combine(RepositoryTemp.Root, "union-projections-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "projections.mjs"), script);
            var testPath = Path.Combine(root, "projections.test.mjs");
            await File.WriteAllTextAsync(testPath,
                """
                import assert from "node:assert/strict";
                import { ReadNumber, ReadString, ReadMultiple, ReadEffectfulString, ReadCalls, ReadMapped } from "./projections.mjs";
                Deno.test("authored host projections keep branches and evaluate receivers once", () => {
                    assert.equal(ReadNumber(0), 0);
                    assert.equal(ReadNumber(32), 32);
                    assert.equal(ReadString("32"), "32");
                    assert.equal(ReadNumber("32"), null);
                    assert.equal(ReadString(32), null);
                    for (const absent of [null, undefined]) {
                        assert.equal(ReadNumber(absent) ?? null, null);
                        assert.equal(ReadString(absent) ?? null, null);
                        assert.equal(ReadMultiple(absent) ?? null, null);
                    }
                    const values = [0, "32"];
                    assert.equal(ReadMultiple(values), values);
                    assert.equal(ReadMultiple(32), null);
                    assert.equal(ReadNumber(values) ?? null, null);
                    assert.equal(ReadString(values) ?? null, null);
                    assert.equal(ReadEffectfulString(), "exact");
                    assert.equal(ReadCalls(), 1);
                    assert.equal(ReadMapped("text"), "mapped:text", "explicit Inline mapping must precede a type-derived projection");
                    assert.equal(ReadMapped(32), null);
                });
                """);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-read", testPath], timeout.Token);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
