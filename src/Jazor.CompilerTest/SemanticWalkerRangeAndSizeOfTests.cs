using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerRangeAndSizeOfTests
{
    private static IBlockOperation GetBlockOperation(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "RangeAndSizeOfScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }

    [TestMethod]
    public void Visit_RangeValues_UseMappedConstructorAndOpenBoundaryContracts()
    {
        var block = GetBlockOperation("""
            using System;

            public sealed class RangeScenarios
            {
                public Range TestMethod(int start)
                {
                    Index end = ^1;
                    Range bounded = start..end;
                    Range openStart = ..end;
                    Range openEnd = start..;
                    Range all = ..;
                    return bounded;
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "_ce8b9229a41c8545(1)");
        StringAssert.Contains(script, "_fc3dfc5dbaa397eb(_1e1b56e4e760a5d5(start), end)");
        StringAssert.Contains(script, "_fc3dfc5dbaa397eb(_c6ec2b575aff2e24(), end)");
        StringAssert.Contains(script, "_fc3dfc5dbaa397eb(_1e1b56e4e760a5d5(start), _0ba7c760bb17a58f())");
        StringAssert.Contains(script, "_fc3dfc5dbaa397eb(_c6ec2b575aff2e24(), _0ba7c760bb17a58f())");
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_MaterializedIndexAndRange_ArrayConsumptionUsesCarrierOffsetsOnce()
    {
        var block = GetBlockOperation("""
            using System;

            public sealed class RangeScenarios
            {
                public void TestMethod()
                {
                    int[] values = [1, 2, 3, 4];
                    Index index = ^1;
                    Range range = 1..^1;
                    int last = values[index];
                    int[] middle = values[range];
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "values[_9b817e75f3f8f58f(index, values.length)]");
        StringAssert.Contains(script, "_1c7a1e658ed790ff(range, values.length)");
        Assert.AreEqual(1, CountOccurrences(script, "_1c7a1e658ed790ff(range, values.length)"));
        StringAssert.Contains(script, ".slice(");
        StringAssert.Contains(script, ".Offset");
        StringAssert.Contains(script, ".Length");
        Assert.IsTrue(
            System.Text.RegularExpressions.Regex.IsMatch(script, @"\.Offset \+ [A-Za-z_$][A-Za-z0-9_$]*\.Length"),
            script);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_IndexAndRangeValueMembers_UseMappedRuntimeImports()
    {
        var block = GetBlockOperation("""
            using System;

            public sealed class RangeScenarios
            {
                public bool TestMethod(Index left, Index right, Range first, Range second)
                {
                    string indexText = left.ToString();
                    int indexHash = left.GetHashCode();
                    bool indexesEqual = left.Equals(right);
                    string rangeText = first.ToString();
                    int rangeHash = first.GetHashCode();
                    bool rangesEqual = first.Equals(second);
                    return indexesEqual && rangesEqual && indexText.Length + rangeText.Length + indexHash + rangeHash > 0;
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var script = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        CollectionAssert.AreEquivalent(
            new[] { "_83db7aa629254762", "_1c7f7405a620c971", "_0fb768c390456f95" },
            imports["System/IndexModule.js"].Select(static specifier => specifier.ToECMAScript()).ToArray());
        CollectionAssert.AreEquivalent(
            new[] { "_f858c453f3829489", "_7fc0f3cc7ec542d3", "_1c286146a6526629" },
            imports["System/RangeModule.js"].Select(static specifier => specifier.ToECMAScript()).ToArray());
        StringAssert.Contains(script, "_0fb768c390456f95(left)");
        StringAssert.Contains(script, "_1c7f7405a620c971(left)");
        StringAssert.Contains(script, "_83db7aa629254762(left, right)");
        StringAssert.Contains(script, "_1c286146a6526629(first)");
        StringAssert.Contains(script, "_7fc0f3cc7ec542d3(first)");
        StringAssert.Contains(script, "_f858c453f3829489(first, second)");
        _ = new Parser().ParseScript("function verify() " + script);
    }

    [TestMethod]
    public void Visit_SizeOfPrimitiveAndEnum_EmitsCompileTimeNumericConstants()
    {
        var block = GetBlockOperation("""
            public enum SmallCode : short
            {
                None
            }

            public sealed class SizeOfScenarios
            {
                public unsafe void TestMethod()
                {
                    int byteSize = sizeof(byte);
                    int doubleSize = sizeof(double);
                    int decimalSize = sizeof(decimal);
                    int enumSize = sizeof(SmallCode);
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.AreEqual("""
            {
              let byteSize = 1;
              let doubleSize = 8;
              let decimalSize = 16;
              let enumSize = 2;
            }
            """, script);
    }

    [TestMethod]
    public void Visit_SizeOfCarrierBackedType_RejectsInventedJavaScriptLayout()
    {
        var block = GetBlockOperation("""
            using System;

            public sealed class SizeOfScenarios
            {
                public unsafe void TestMethod()
                {
                    int dateTimeSize = sizeof(DateTime);
                }
            }
            """);

        var exception = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        Assert.AreEqual(OperationKind.SizeOf, exception.Kind);
        StringAssert.Contains(exception.Message, "compile-time primitive scalar or enum-underlying sizes");
    }

    private static int CountOccurrences(string value, string fragment)
    {
        var count = 0;
        var offset = 0;
        while ((offset = value.IndexOf(fragment, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += fragment.Length;
        }

        return count;
    }
}
