using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerMutationProtocolTests
{
    [TestMethod]
    public void Visit_MemberArrayAndIndexerMutations_EvaluateEverySourceOperandOnce()
    {
        var block = GetBlockOperation("""
            class MutationProtocolScenarios
            {
                private sealed class Counter
                {
                    public int Value { get; set; }

                    public int? this[int index]
                    {
                        get => null;
                        set { }
                    }
                }

                void TestMethod(int[] values, Counter counter)
                {
                    counter.Value += NextPropertyDelta();
                    counter.Value++;
                    values[NextArrayIndex()] += NextArrayDelta();
                    counter[NextNullableIndex()] ??= NextFallback();
                }

                private static int NextPropertyDelta() => 1;
                private static int NextArrayIndex() => 0;
                private static int NextArrayDelta() => 2;
                private static int NextNullableIndex() => 1;
                private static int NextFallback() => 3;
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        AssertSingleCall(script, "MutationProtocolScenarios.NextPropertyDelta()");
        AssertSingleCall(script, "MutationProtocolScenarios.NextArrayIndex()");
        AssertSingleCall(script, "MutationProtocolScenarios.NextArrayDelta()");
        AssertSingleCall(script, "MutationProtocolScenarios.NextNullableIndex()");
        AssertSingleCall(script, "MutationProtocolScenarios.NextFallback()");
        StringAssert.Contains(script, "counter", StringComparison.Ordinal);
        StringAssert.Contains(script, "values", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    private static void AssertSingleCall(string script, string call)
        => Assert.AreEqual(1, script.Split(call, StringSplitOptions.None).Length - 1, script);

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerMutationProtocolTests",
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
