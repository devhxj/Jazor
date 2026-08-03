using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerMutationMatrixTests
{
    private static readonly Lazy<IReadOnlyDictionary<string, IBlockOperation>> Operations = new(CreateOperations);

    public static IEnumerable<TestDataRow<MutationLoweringCase>> Cases
        => MutationLoweringCaseCatalog.All.Select(static testCase =>
            new TestDataRow<MutationLoweringCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsMethodsAndOperationCoverage()
    {
        var cases = MutationLoweringCaseCatalog.All;

        Assert.IsNotEmpty(cases);
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.MethodName).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(cases.All(static testCase => testCase.Id.StartsWith("mutation-lowering.", StringComparison.Ordinal)));
        Assert.IsTrue(cases.All(static testCase => !string.IsNullOrWhiteSpace(testCase.Dimension)));
        CollectionAssert.AreEquivalent(
            Operations.Value.Keys.ToArray(),
            cases.Select(static testCase => testCase.MethodName).ToArray());
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Visit_MatchesMutationScenarioContract(MutationLoweringCase testCase)
    {
        var operation = Operations.Value[testCase.MethodName];

        var node = new SemanticWalker(true).Visit(operation, new SenseArgument());
        var script = node?.ToKnRECMAScript();

        Assert.AreEqual(
            testCase.ExpectedJavaScript.ReplaceLineEndings("\n"),
            script?.ReplaceLineEndings("\n"),
            testCase.Id);
    }

    private static IReadOnlyDictionary<string, IBlockOperation> CreateOperations()
    {
        const string source = """
            using System;
            using System.Collections.Generic;
            using System.Numerics;

            public sealed class MutationScenarios
            {
                public void ListPrefixIncrement()
                {
                    var list = new List<int> { 1, 2, 3 };
                    int after = ++list[0];
                }

                public void ListPrefixDecrement()
                {
                    var list = new List<int> { 1, 2, 3 };
                    int after = --list[0];
                }

                public void ListPostfixDecrement()
                {
                    var list = new List<int> { 1, 2, 3 };
                    int before = list[0]--;
                }

                public void ListFromEndPrefixIncrement()
                {
                    var list = new List<int> { 1, 2, 3 };
                    Func<List<int>> next = () => list;
                    int after = ++next()[^1];
                }

                public void ListFromEndPrefixDecrement()
                {
                    var list = new List<int> { 1, 2, 3 };
                    Func<List<int>> next = () => list;
                    int after = --next()[^1];
                }

                public void ArrayPostfixDecrement()
                {
                    int[] values = new[] { 1, 2, 3 };
                    int before = values[0]--;
                }

                public void ArrayFromEndPostfixIncrement()
                {
                    int[] values = new[] { 1, 2, 3 };
                    Func<int[]> next = () => values;
                    int before = next()[^1]++;
                }

                public void ArrayFromEndPrefixDecrement()
                {
                    int[] values = new[] { 1, 2, 3 };
                    Func<int[]> next = () => values;
                    int after = --next()[^1];
                }

                public void LongListPostfixIncrement()
                {
                    var list = new List<long> { 10L };
                    long before = list[0]++;
                }

                public void LongListPrefixDecrement()
                {
                    var list = new List<long> { 10L };
                    long after = --list[0];
                }

                public void BigIntegerListPostfixIncrement()
                {
                    var list = new List<BigInteger> { BigInteger.Zero };
                    BigInteger before = list[0]++;
                }

                public void BigIntegerListPrefixDecrement()
                {
                    var list = new List<BigInteger> { BigInteger.Zero };
                    BigInteger after = --list[0];
                }

                public void ULongListPrefixIncrement()
                {
                    var list = new List<ulong> { 10UL };
                    ulong after = ++list[0];
                }

                public void Int128ListPostfixDecrement()
                {
                    var list = new List<Int128> { default(Int128) };
                    Int128 before = list[0]--;
                }

                public void UInt128ListPrefixIncrement()
                {
                    var list = new List<UInt128> { default(UInt128) };
                    UInt128 after = ++list[0];
                }

                public void DoubleListPostfixIncrement()
                {
                    var list = new List<double> { 10.5 };
                    double before = list[0]++;
                }

                public void Int128LocalPostfixIncrement()
                {
                    Int128 value = default;
                    Int128 before = value++;
                }

                public void UInt128LocalPrefixDecrement()
                {
                    UInt128 value = default;
                    UInt128 after = --value;
                }
            }
            """;

        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "SemanticWalkerMutationMatrixTests",
            [syntaxTree],
            TestMetadataReferences.Net11.Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        if (errors.Length > 0)
        {
            throw new InvalidOperationException(
                string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));
        }

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        return syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .ToDictionary(
                static method => method.Identifier.ValueText,
                method => semanticModel.GetOperation(method.Body!) as IBlockOperation
                    ?? throw new InvalidOperationException($"Method '{method.Identifier.ValueText}' has no block operation."),
                StringComparer.Ordinal);
    }
}

public sealed record MutationLoweringCase(
    string Id,
    string Dimension,
    string MethodName,
    string ExpectedJavaScript);

internal static class MutationLoweringCaseCatalog
{
    public static IReadOnlyList<MutationLoweringCase> All { get; } =
    [
        Case(
            "mutation-lowering.list.prefix-increment",
            "mapped-indexer-prefix-new-value",
            "ListPrefixIncrement",
            """
            {
              let v$1;
              let list = (() => {
                let v$0 = createDefault();
                add(v$0, 1);
                add(v$0, 2);
                add(v$0, 3);
                return v$0;
              })();
              let after = (v$1 = _d389c31d59037b42(list, 0) + 1, _c16a7960302ea054(list, 0, v$1), v$1);
            }
            """),
        Case(
            "mutation-lowering.list.prefix-decrement",
            "mapped-indexer-prefix-new-value",
            "ListPrefixDecrement",
            """
            {
              let v$1;
              let list = (() => {
                let v$0 = createDefault();
                add(v$0, 1);
                add(v$0, 2);
                add(v$0, 3);
                return v$0;
              })();
              let after = (v$1 = _d389c31d59037b42(list, 0) - 1, _c16a7960302ea054(list, 0, v$1), v$1);
            }
            """),
        Case(
            "mutation-lowering.list.postfix-decrement",
            "mapped-indexer-postfix-old-value",
            "ListPostfixDecrement",
            """
            {
              let v$1;
              let list = (() => {
                let v$0 = createDefault();
                add(v$0, 1);
                add(v$0, 2);
                add(v$0, 3);
                return v$0;
              })();
              let before = (v$1 = _d389c31d59037b42(list, 0), _c16a7960302ea054(list, 0, v$1 - 1), v$1);
            }
            """),
        Case(
            "mutation-lowering.list-from-end.prefix-increment",
            "implicit-indexer-prefix-single-evaluation",
            "ListFromEndPrefixIncrement",
            """
            {
              let v$1, v$2, v$3;
              let list = (() => {
                let v$0 = createDefault();
                add(v$0, 1);
                add(v$0, 2);
                add(v$0, 3);
                return v$0;
              })();
              let next = () => {
                return list;
              };
              let after = (v$1 = next(), v$2 = v$1.length - 1, v$3 = _d389c31d59037b42(v$1, v$2) + 1, _c16a7960302ea054(v$1, v$2, v$3), v$3);
            }
            """),
        Case(
            "mutation-lowering.list-from-end.prefix-decrement",
            "implicit-indexer-prefix-single-evaluation",
            "ListFromEndPrefixDecrement",
            """
            {
              let v$1, v$2, v$3;
              let list = (() => {
                let v$0 = createDefault();
                add(v$0, 1);
                add(v$0, 2);
                add(v$0, 3);
                return v$0;
              })();
              let next = () => {
                return list;
              };
              let after = (v$1 = next(), v$2 = v$1.length - 1, v$3 = _d389c31d59037b42(v$1, v$2) - 1, _c16a7960302ea054(v$1, v$2, v$3), v$3);
            }
            """),
        Case(
            "mutation-lowering.array.postfix-decrement",
            "native-array-update",
            "ArrayPostfixDecrement",
            """
            {
              let values = [1, 2, 3];
              let before = values[0]--;
            }
            """),
        Case(
            "mutation-lowering.array-from-end.postfix-increment",
            "native-array-from-end-single-evaluation",
            "ArrayFromEndPostfixIncrement",
            """
            {
              let v$0;
              let values = [1, 2, 3];
              let next = () => {
                return values;
              };
              let before = (v$0 = next(), v$0[v$0.length - 1]++);
            }
            """),
        Case(
            "mutation-lowering.array-from-end.prefix-decrement",
            "native-array-from-end-single-evaluation",
            "ArrayFromEndPrefixDecrement",
            """
            {
              let v$0;
              let values = [1, 2, 3];
              let next = () => {
                return values;
              };
              let after = (v$0 = next(), --v$0[v$0.length - 1]);
            }
            """),
        Case(
            "mutation-lowering.long-list.postfix-increment",
            "bigint-indexer-postfix-old-value",
            "LongListPostfixIncrement",
            """
            {
              let v$1;
              let list = (() => {
                let v$0 = createDefault();
                add(v$0, 10n);
                return v$0;
              })();
              let before = (v$1 = _d389c31d59037b42(list, 0), _c16a7960302ea054(list, 0, v$1 + 1n), v$1);
            }
            """),
        Case(
            "mutation-lowering.long-list.prefix-decrement",
            "bigint-indexer-prefix-new-value",
            "LongListPrefixDecrement",
            """
            {
              let v$1;
              let list = (() => {
                let v$0 = createDefault();
                add(v$0, 10n);
                return v$0;
              })();
              let after = (v$1 = _d389c31d59037b42(list, 0) - 1n, _c16a7960302ea054(list, 0, v$1), v$1);
            }
            """),
        Case(
            "mutation-lowering.biginteger-list.postfix-increment",
            "mapped-bigint-indexer-postfix-old-value",
            "BigIntegerListPostfixIncrement",
            """
            {
              let v$1;
              let list = (() => {
                let v$0 = createDefault();
                add(v$0, 0n);
                return v$0;
              })();
              let before = (v$1 = _d389c31d59037b42(list, 0), _c16a7960302ea054(list, 0, v$1 + 1n), v$1);
            }
            """),
        Case(
            "mutation-lowering.biginteger-list.prefix-decrement",
            "mapped-bigint-indexer-prefix-new-value",
            "BigIntegerListPrefixDecrement",
            """
            {
              let v$1;
              let list = (() => {
                let v$0 = createDefault();
                add(v$0, 0n);
                return v$0;
              })();
              let after = (v$1 = _d389c31d59037b42(list, 0) - 1n, _c16a7960302ea054(list, 0, v$1), v$1);
            }
            """),
        Case(
            "mutation-lowering.ulong-list.prefix-increment",
            "bigint-indexer-prefix-new-value",
            "ULongListPrefixIncrement",
            """
            {
              let v$1;
              let list = (() => {
                let v$0 = createDefault();
                add(v$0, 10n);
                return v$0;
              })();
              let after = (v$1 = _d389c31d59037b42(list, 0) + 1n, _c16a7960302ea054(list, 0, v$1), v$1);
            }
            """),
        Case(
            "mutation-lowering.int128-list.postfix-decrement",
            "bigint-indexer-postfix-old-value",
            "Int128ListPostfixDecrement",
            """
            {
              let v$1;
              let list = (() => {
                let v$0 = createDefault();
                add(v$0, 0n);
                return v$0;
              })();
              let before = (v$1 = _d389c31d59037b42(list, 0), _c16a7960302ea054(list, 0, BigInt.asIntN(128, v$1 - 1n)), v$1);
            }
            """),
        Case(
            "mutation-lowering.uint128-list.prefix-increment",
            "bigint-indexer-prefix-new-value",
            "UInt128ListPrefixIncrement",
            """
            {
              let v$1;
              let list = (() => {
                let v$0 = createDefault();
                add(v$0, 0n);
                return v$0;
              })();
              let after = (v$1 = BigInt.asUintN(128, _d389c31d59037b42(list, 0) + 1n), _c16a7960302ea054(list, 0, v$1), v$1);
            }
            """),
        Case(
            "mutation-lowering.double-list.postfix-increment",
            "number-indexer-postfix-old-value",
            "DoubleListPostfixIncrement",
            """
            {
              let v$1;
              let list = (() => {
                let v$0 = createDefault();
                add(v$0, 10.5);
                return v$0;
              })();
              let before = (v$1 = _d389c31d59037b42(list, 0), _c16a7960302ea054(list, 0, v$1 + 1), v$1);
            }
            """),
        Case(
            "mutation-lowering.int128-local.postfix-increment",
            "intrinsic-bigint-local-update",
            "Int128LocalPostfixIncrement",
            """
            {
              let v$0;
              let value = 0n;
              let before = (v$0 = value, value = BigInt.asIntN(128, v$0 + 1n), v$0);
            }
            """),
        Case(
            "mutation-lowering.uint128-local.prefix-decrement",
            "intrinsic-bigint-local-update",
            "UInt128LocalPrefixDecrement",
            """
            {
              let v$0;
              let value = 0n;
              let after = (v$0 = BigInt.asUintN(128, value - 1n), value = v$0, v$0);
            }
            """)
    ];

    private static MutationLoweringCase Case(
        string id,
        string dimension,
        string methodName,
        string expectedJavaScript)
        => new(id, dimension, methodName, expectedJavaScript);
}
