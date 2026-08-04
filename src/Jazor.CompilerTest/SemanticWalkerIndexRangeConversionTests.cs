using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerIndexRangeConversionTests
{
    [TestMethod]
    public void Visit_UserDefinedIndexAndRangeConversions_PreservesErasedCarriers()
    {
        var block = GetBlockOperation(
            """
            using System;

            struct IndexAdapter
            {
                public static implicit operator Index(IndexAdapter value) => Index.FromStart(0);
                public static implicit operator IndexAdapter(Index value) => default;
            }

            struct RangeAdapter
            {
                public static implicit operator Range(RangeAdapter value) => ..;
                public static implicit operator RangeAdapter(Range value) => default;
            }

            class TestClass
            {
                void TestMethod(Index index, Range range, IndexAdapter indexAdapter, RangeAdapter rangeAdapter)
                {
                    Index fromAdapter = indexAdapter;
                    IndexAdapter fromIndex = index;
                    Range fromRangeAdapter = rangeAdapter;
                    RangeAdapter fromRange = range;
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "let fromAdapter = indexAdapter;", StringComparison.Ordinal);
        StringAssert.Contains(script, "let fromIndex = index;", StringComparison.Ordinal);
        StringAssert.Contains(script, "let fromRangeAdapter = rangeAdapter;", StringComparison.Ordinal);
        StringAssert.Contains(script, "let fromRange = range;", StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_ArrayRangeWithStoredIndexBoundaries_ConvertsBothCarriersToOffsets()
    {
        var block = GetBlockOperation(
            """
            using System;

            class TestClass
            {
                void TestMethod(int[] values, Index start, Index end)
                {
                    int[] slice = values[start..end];
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "start, values.length", StringComparison.Ordinal);
        StringAssert.Contains(script, "end, values.length", StringComparison.Ordinal);
        Assert.DoesNotContain(".slice(start, end)", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    [TestMethod]
    public void Visit_ListRangeWithStoredIndexBoundaries_ConvertsBothCarriersToOffsets()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Collections.Generic;

            class TestClass
            {
                void TestMethod(List<int> values, Index start, Index end)
                {
                    List<int> slice = values[start..end];
                }
            }
            """);

        var script = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "start, values.length", StringComparison.Ordinal);
        StringAssert.Contains(script, "end, values.length", StringComparison.Ordinal);
        Assert.AreEqual(1, script.Split(["start, values.length"], StringSplitOptions.None).Length - 1);
        Assert.DoesNotContain("start, end", script, StringComparison.Ordinal);
        _ = new Parser().ParseScript(script);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerIndexRangeConversionTests",
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
        return Assert.IsInstanceOfType<IBlockOperation>(
            compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
