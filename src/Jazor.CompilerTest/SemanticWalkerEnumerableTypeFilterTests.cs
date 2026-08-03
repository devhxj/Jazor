using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerEnumerableTypeFilterTests
{
    [TestMethod]
    public void Visit_EnumerableCastAndOfType_UseCarrierDiscriminatorsForInvocationAndMethodReference()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Collections;
            using System.Collections.Generic;
            using System.Linq;

            public static class EnumerableTypeFilterScenarios
            {
                public static void Evaluate(IEnumerable source)
                {
                    var booleans = source.Cast<bool>();
                    var integers = source.OfType<long>();
                    var dates = source.OfType<DateTime>();
                    var dictionaries = source.OfType<Dictionary<string, int>>();
                    var sets = source.OfType<HashSet<int>>();
                    var arrays = source.OfType<int[]>();
                    var exceptions = source.Cast<Exception>();
                    var objects = source.OfType<object>();
                    Func<IEnumerable, IEnumerable<bool>> filter = Enumerable.OfType<bool>;
                    Func<IEnumerable, IEnumerable<object>> caster = Enumerable.Cast<object>;
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        StringAssert.Contains(body, "typeof __enumerableTypeFilterItem === \"boolean\"", StringComparison.Ordinal);
        StringAssert.Contains(body, "typeof __enumerableTypeFilterItem === \"bigint\"", StringComparison.Ordinal);
        StringAssert.Contains(body, "__enumerableTypeFilterItem instanceof JDateTime", StringComparison.Ordinal);
        StringAssert.Contains(body, "__enumerableTypeFilterItem instanceof Map", StringComparison.Ordinal);
        StringAssert.Contains(body, "__enumerableTypeFilterItem instanceof Set", StringComparison.Ordinal);
        StringAssert.Contains(body, "Array.isArray(__enumerableTypeFilterItem)", StringComparison.Ordinal);
        StringAssert.Contains(body, "__enumerableTypeFilterItem instanceof Error", StringComparison.Ordinal);
		StringAssert.Contains(body, "InvalidCastException: element cannot be cast to bool.", StringComparison.Ordinal);
		StringAssert.Contains(body, "filter = v$0$0 =>", StringComparison.Ordinal);
		StringAssert.Contains(body, "caster = v$1$0 =>", StringComparison.Ordinal);
        Assert.IsNotEmpty(argument.FlushImportSpecifiers(), body);

        _ = new Parser().ParseScript("function verify(source) " + body);
    }

    [TestMethod]
    public void Visit_EnumerableArrayLike_MethodGroupsUseDeclaredEnumerableSourceContract()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            public static class EnumerableArrayLikeMethodGroupScenarios
            {
                public static void Evaluate()
                {
                    Func<IEnumerable<int>, int[]> toArray = Enumerable.ToArray;
                    Func<IEnumerable<int>, List<int>> toList = Enumerable.ToList;
                    Func<IEnumerable<int>, Func<int, bool>, IEnumerable<int>> where = Enumerable.Where;
                }
            }
            """);

        var body = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(body);
        StringAssert.Contains(body, "toArray =", StringComparison.Ordinal);
        StringAssert.Contains(body, "toList =", StringComparison.Ordinal);
        StringAssert.Contains(body, "where =", StringComparison.Ordinal);
        Assert.IsFalse(body.Contains("Enumerable", StringComparison.Ordinal), body);
        _ = new Parser().ParseScript("function verify() " + body);
    }

    [TestMethod]
    [DataRow("System.IDisposable", "", "erased target")]
    [DataRow("T", "<T>", "erased target")]
    [DataRow("char", "", "shared JavaScript string carrier")]
    [DataRow("string", "", "shared JavaScript string carrier")]
    [DataRow("int", "", "numeric or enum CLR values")]
    [DataRow("Status", "", "numeric or enum CLR values")]
    [DataRow("System.Guid", "", "stable runtime discriminator")]
    public void Visit_EnumerableOfType_RejectsErasedOrAmbiguousCarrierTargets(
        string target,
        string typeParameters,
        string expectedMessage)
    {
        var block = GetBlockOperation(
            $$"""
            using System;
            using System.Collections;
            using System.Linq;

            public enum Status
            {
                Ready
            }

            public static class EnumerableTypeFilterScenarios
            {
                public static void Evaluate{{typeParameters}}(IEnumerable source)
                {
                    var result = source.OfType<{{target}}>();
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument(UseImportAliases: true)));

        StringAssert.Contains(exception.Message, expectedMessage, StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, target, StringComparison.Ordinal);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "EnumerableTypeFilterScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "Evaluate");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
