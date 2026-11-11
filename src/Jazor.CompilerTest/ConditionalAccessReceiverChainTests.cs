using Jazor.NodeHost;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class ConditionalAccessReceiverChainTests
{
    [TestMethod]
    public async Task Convert_ConditionalReceiverChains_SkipIndicesAndPreserveNullishSemantics()
    {
        var tree = CSharpSyntaxTree.ParseText("""
            using ECMAScript;
            [ECMAScript] public class Payload
            {
                public int[] Items;
                public Payload Child;
                public int Number;
                public int[] Values { get; set; }
            }
            [ECMAScriptModule("./chains.mjs")]
            public static class Chains
            {
                private static int Calls = 0;
                private static int NextIndex() { Calls++; return 0; }
                public static int Count() => Calls;
                public static int? Array(int[]? value) => value?[NextIndex()];
                public static int? IndexFromEnd(int[]? value) => value?[^1];
                public static int? FieldArray(Payload? value) => value?.Items[NextIndex()];
                public static int? NestedField(Payload? value) => value?.Child.Number;
                public static int? PropertyIndex(Payload? value) => value?.Values[^1];
                public static int[]? PropertyRange(Payload? value) => value?.Values[NextIndex()..^1];
                public static double? ConvertedField(Payload? value) => value?.Child.Number;
            }
            """, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create("ConditionalReceiverChains", [tree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(
                typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(d => d.ToString())));
        var converter = new AstConverter(compilation.GetTypeByMetadataName("Chains")!, compilation.GetSemanticModel(tree));
        var module = await converter.Convert();
        Assert.IsNotNull(module);
        var root = Path.Combine(RepositoryTemp.Root, "conditional-chains-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "chains.mjs"), module.ToKnRECMAScript());
            var testPath = Path.Combine(root, "chains.test.mjs");
            await File.WriteAllTextAsync(testPath, """
                import assert from "node:assert/strict";
                import * as p from "./chains.mjs";
                globalThis.__jazorTest("nullish chains skip indices and retain field/property values", () => {
                    for (const absent of [null, undefined]) {
                        for (const read of [p.Array, p.IndexFromEnd, p.FieldArray, p.NestedField, p.PropertyIndex, p.PropertyRange, p.ConvertedField])
                            assert.equal(read(absent) ?? null, null);
                    }
                    assert.equal(p.Count(), 0, "short circuit must skip every effectful index");
                    assert.equal(p.Array([0]), 0);
                    assert.equal(p.IndexFromEnd([2, 3]), 3);
                    const value = { Items: [7], Child: { Number: 0 }, Values: [2, 3] };
                    assert.equal(p.FieldArray(value), 7);
                    assert.equal(p.Count(), 2, "each non-null index executes once");
                    assert.equal(p.NestedField(value), 0);
                    assert.equal(p.PropertyIndex(value), 3);
                    assert.deepEqual(p.PropertyRange(value), [2]);
                    assert.equal(p.Count(), 3, "range boundaries execute once after the null guard");
                    assert.equal(p.ConvertedField(value), 0);
                });
                """);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await NodeModuleRunner.RunAsync(root, testPath, cancellationToken: timeout.Token);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
