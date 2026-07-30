using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerInlineEvaluationProtocolTests
{
    private static readonly Lazy<IReadOnlyDictionary<string, IBlockOperation>> Operations = new(CreateOperations);

    public static IEnumerable<TestDataRow<InlineEvaluationProtocolCase>> Cases
        => InlineEvaluationProtocolCatalog.Cases.Select(static testCase =>
            new TestDataRow<InlineEvaluationProtocolCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<InlineEvaluationProtocolFailureCase>> FailureCases
        => InlineEvaluationProtocolCatalog.FailureCases.Select(static testCase =>
            new TestDataRow<InlineEvaluationProtocolFailureCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndSources()
    {
        var cases = InlineEvaluationProtocolCatalog.Cases
            .Select(static testCase => (testCase.Id, testCase.Dimension, testCase.Source))
            .Concat(InlineEvaluationProtocolCatalog.FailureCases.Select(static testCase =>
                (testCase.Id, testCase.Dimension, testCase.Source)))
            .ToArray();

        Assert.IsNotEmpty(cases);
        Assert.HasCount(cases.Length, cases.Select(static item => item.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Length, cases.Select(static item => item.Dimension).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Length, cases.Select(static item => item.Source).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(cases.All(static item => item.Id.StartsWith("inline-evaluation.", StringComparison.Ordinal)));
        Assert.IsTrue(InlineEvaluationProtocolCatalog.Cases.All(static item => item.ExpectedFragments.Count > 0));
        Assert.IsTrue(InlineEvaluationProtocolCatalog.FailureCases.All(static item => item.ExpectedDiagnosticFragments.Count > 0));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Visit_InlineEvaluationProtocol_PreservesSingleEvaluationAndOrder(InlineEvaluationProtocolCase testCase)
    {
        var block = Operations.Value[testCase.Id];
        var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first, testCase.Id);
        Assert.AreEqual(first, second, testCase.Id);
        foreach (var fragment in testCase.ExpectedFragments)
            StringAssert.Contains(first, fragment, testCase.Id);

        foreach (var fragment in testCase.SingleOccurrenceFragments)
            Assert.AreEqual(1, CountOccurrences(first, fragment), $"{testCase.Id}: {fragment}");

        var previousIndex = -1;
        foreach (var fragment in testCase.OrderedFragments)
        {
            var index = first.IndexOf(fragment, StringComparison.Ordinal);
            Assert.IsGreaterThan(previousIndex, index, $"{testCase.Id}: {fragment}");
            previousIndex = index;
        }

        _ = new Parser().ParseScript(first);
    }

    [TestMethod]
    [DynamicData(nameof(FailureCases))]
    public void Visit_InlineEvaluationProtocol_RejectsInvalidPlaceholderContract(InlineEvaluationProtocolFailureCase testCase)
    {
        var block = Operations.Value[testCase.Id];

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        foreach (var fragment in testCase.ExpectedDiagnosticFragments)
            StringAssert.Contains(exception.Message, fragment, testCase.Id);
    }

    [TestMethod]
    public void Visit_IndependentCompilationsWithSameSignature_UseTheirOwnInlineTemplates()
    {
        var validCase = InlineEvaluationProtocolCatalog.Cases.Single(static testCase =>
            testCase.Id == "inline-evaluation.custom.placeholder-like-identifiers");
        var validBlock = Operations.Value[validCase.Id];
        var script = new SemanticWalker(true).Visit(validBlock, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(script, validCase.Id);
        StringAssert.Contains(script, "__argValue + __arg0value + value", validCase.Id);

        var invalidCase = InlineEvaluationProtocolCatalog.FailureCases.Single(static testCase =>
            testCase.Id == "inline-evaluation.custom.placeholder-index-overflow");
        var invalidBlock = Operations.Value[invalidCase.Id];
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new SemanticWalker(true).Visit(invalidBlock, new SenseArgument()));

        StringAssert.Contains(exception.Message, "exceeds the supported index range", invalidCase.Id);
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

    private static IReadOnlyDictionary<string, IBlockOperation> CreateOperations()
        => InlineEvaluationProtocolCatalog.Cases
            .Select(static testCase => (testCase.Id, testCase.Source))
            .Concat(InlineEvaluationProtocolCatalog.FailureCases.Select(static testCase =>
                (testCase.Id, testCase.Source)))
            .ToDictionary(
                static testCase => testCase.Id,
                static testCase => CreateOperation(testCase.Id, testCase.Source),
                StringComparer.Ordinal);

    private static IBlockOperation CreateOperation(string id, string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            $"InlineEvaluation_{id.Replace('.', '_')}",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, $"{id}{Environment.NewLine}{string.Join(Environment.NewLine, errors.Select(static error => error.ToString()))}");

        var model = compilation.GetSemanticModel(syntaxTree);
        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(method.Body!), id);
    }
}

public sealed record InlineEvaluationProtocolCase(
    string Id,
    string Dimension,
    string Source,
    IReadOnlyList<string> ExpectedFragments,
    IReadOnlyList<string> SingleOccurrenceFragments,
    IReadOnlyList<string> OrderedFragments);

public sealed record InlineEvaluationProtocolFailureCase(
    string Id,
    string Dimension,
    string Source,
    IReadOnlyList<string> ExpectedDiagnosticFragments);

internal static class InlineEvaluationProtocolCatalog
{
    public static IReadOnlyList<InlineEvaluationProtocolCase> Cases { get; } =
    [
        Case(
            "custom.repeated-argument",
            "source=ECMAScriptInline;placeholder-use=2;argument=invocation;evaluation=single",
            """
            using ECMAScript;

            public static class InlineHost
            {
                [ECMAScriptInline("__arg1 + __arg1")]
                public static extern int Twice(int value);
            }

            public sealed class Scenario
            {
                private static int NextNumber() => 21;

                public void TestMethod()
                {
                    var result = InlineHost.Twice(NextNumber());
                }
            }
            """,
            ["__jz_arg0 + __jz_arg0", "nextNumber()"],
            ["nextNumber()"],
            []),
        Case(
            "custom.unused-argument",
            "source=ECMAScriptInline;placeholder-use=0;argument=invocation;evaluation=preserved",
            """
            using ECMAScript;

            public static class InlineHost
            {
                [ECMAScriptInline("42")]
                public static extern int Ignore(int value);
            }

            public sealed class Scenario
            {
                private static int NextNumber() => 7;

                public void TestMethod()
                {
                    var result = InlineHost.Ignore(NextNumber());
                }
            }
            """,
            ["=> 42", "nextNumber()"],
            ["nextNumber()"],
            []),
        Case(
            "custom.placeholder-text-literal",
            "source=ECMAScriptInline;placeholder-shape=string-literal;argument=invocation;text=preserved",
            """
            using ECMAScript;

            public static class InlineHost
            {
                [ECMAScriptInline("\"__arg1\"")]
                public static extern string Literal(int value);
            }

            public sealed class Scenario
            {
                private static int NextNumber() => 7;

                public void TestMethod()
                {
                    var result = InlineHost.Literal(NextNumber());
                }
            }
            """,
            ["=> \"__arg1\"", "nextNumber()"],
            ["nextNumber()"],
            []),
        Case(
            "custom.placeholder-like-identifiers",
            "source=ECMAScriptInline;placeholder-shape=identifier-prefix;argument=parameter;names=preserved",
            """
            using ECMAScript;

            public static class InlineHost
            {
                [ECMAScriptInline("__argValue + __arg0value + __arg1")]
                public static extern int Read(int value);
            }

            public sealed class Scenario
            {
                public void TestMethod(int value)
                {
                    var result = InlineHost.Read(value);
                }
            }
            """,
            ["__argValue + __arg0value + value"],
            [],
            []),
        Case(
            "custom.reordered-arguments",
            "source=ECMAScriptInline;placeholder-order=second-first;arguments=invocations;order=first-second",
            """
            using ECMAScript;

            public static class InlineHost
            {
                [ECMAScriptInline("__arg2 - __arg1")]
                public static extern int Difference(int first, int second);
            }

            public sealed class Scenario
            {
                private static int NextFirst() => 3;
                private static int NextSecond() => 8;

                public void TestMethod()
                {
                    var result = InlineHost.Difference(NextFirst(), NextSecond());
                }
            }
            """,
            ["__jz_arg1 - __jz_arg0"],
            ["nextFirst()", "nextSecond()"],
            ["nextFirst()", "nextSecond()"]),
        Case(
            "custom.conditional-argument",
            "source=ECMAScriptInline;placeholder-path=conditional;arguments=invocations;evaluation=eager",
            """
            using ECMAScript;

            public static class InlineHost
            {
                [ECMAScriptInline("__arg1 ? __arg2 : 0")]
                public static extern int Select(bool condition, int value);
            }

            public sealed class Scenario
            {
                private static bool NextCondition() => false;
                private static int NextValue() => 5;

                public void TestMethod()
                {
                    var result = InlineHost.Select(NextCondition(), NextValue());
                }
            }
            """,
            ["__jz_arg0 ? __jz_arg1 : 0"],
            ["nextCondition()", "nextValue()"],
            ["nextCondition()", "nextValue()"]),
        Case(
            "custom.optional-call-argument",
            "source=ECMAScriptInline;placeholder-path=optional-call;argument=invocation;evaluation=eager",
            """
            using System;
            using ECMAScript;

            public static class InlineHost
            {
                [ECMAScriptInline("__arg1?.(__arg2)")]
                public static extern int? Invoke(Func<int, int>? callback, int value);
            }

            public sealed class Scenario
            {
                private static int NextValue() => 5;

                public void TestMethod(Func<int, int>? callback)
                {
                    var result = InlineHost.Invoke(callback, NextValue());
                }
            }
            """,
            ["__jz_arg0?.(__jz_arg1)"],
            ["nextValue()"],
            []),
        Case(
            "custom.optional-computed-key",
            "source=ECMAScriptInline;placeholder-path=optional-computed-member;argument=invocation;evaluation=eager",
            """
            using ECMAScript;

            public static class InlineHost
            {
                [ECMAScriptInline("__arg1?.[__arg2]")]
                public static extern int? Read(int[]? values, int index);
            }

            public sealed class Scenario
            {
                private static int NextIndex() => 1;

                public void TestMethod(int[]? values)
                {
                    var result = InlineHost.Read(values, NextIndex());
                }
            }
            """,
            ["__jz_arg0?.[__jz_arg1]"],
            ["nextIndex()"],
            []),
        Case(
            "string.to-char-array-range.variables",
            "source=CLR-inline;member=ToCharArray(start,length);arguments=locals;shape=direct",
            """
            public sealed class Scenario
            {
                public void TestMethod(string text, int start, int length)
                {
                    var chars = text.ToCharArray(start, length);
                }
            }
            """,
            ["text.substring(start, start + length).split(\"\")"],
            [],
            []),
        Case(
            "string.to-char-array-range.side-effects",
            "source=CLR-inline;member=ToCharArray(start,length);arguments=invocations;order=receiver-start-length",
            """
            public sealed class Scenario
            {
                private static string NextText() => "abcdef";
                private static int NextStart() => 1;
                private static int NextLength() => 3;

                public void TestMethod()
                {
                    var chars = NextText().ToCharArray(NextStart(), NextLength());
                }
            }
            """,
            ["__jz_arg0.substring(__jz_arg1, __jz_arg1 + __jz_arg2).split(\"\")"],
            ["nextText()", "nextStart()", "nextLength()"],
            ["nextText()", "nextStart()", "nextLength()"]),
        Case(
            "string.remove-range.variables",
            "source=CLR-inline;member=Remove(start,count);arguments=locals;shape=direct",
            """
            public sealed class Scenario
            {
                public void TestMethod(string text, int start, int count)
                {
                    var result = text.Remove(start, count);
                }
            }
            """,
            ["text.slice(0, start) + text.slice(start + count)"],
            [],
            []),
        Case(
            "string.remove-range.side-effects",
            "source=CLR-inline;member=Remove(start,count);arguments=invocations;order=receiver-start-count",
            """
            public sealed class Scenario
            {
                private static string NextText() => "abcdef";
                private static int NextStart() => 1;
                private static int NextCount() => 2;

                public void TestMethod()
                {
                    var result = NextText().Remove(NextStart(), NextCount());
                }
            }
            """,
            ["__jz_arg0.slice(0, __jz_arg1) + __jz_arg0.slice(__jz_arg1 + __jz_arg2)"],
            ["nextText()", "nextStart()", "nextCount()"],
            ["nextText()", "nextStart()", "nextCount()"]),
        Case(
            "string.insert.side-effects",
            "source=CLR-inline;member=Insert(index,value);arguments=invocations;order=receiver-index-value",
            """
            public sealed class Scenario
            {
                private static string NextText() => "abcdef";
                private static int NextIndex() => 2;
                private static string NextValue() => "XY";

                public void TestMethod()
                {
                    var result = NextText().Insert(NextIndex(), NextValue());
                }
            }
            """,
            ["__jz_arg0.slice(0, __jz_arg1) + __jz_arg2 + __jz_arg0.slice(__jz_arg1)"],
            ["nextText()", "nextIndex()", "nextValue()"],
            ["nextText()", "nextIndex()", "nextValue()"])
    ];

    public static IReadOnlyList<InlineEvaluationProtocolFailureCase> FailureCases { get; } =
    [
        Failure(
            "custom.placeholder-index-overflow",
            "source=ECMAScriptInline;placeholder-shape=numeric-overflow;result=rejected",
            """
            using ECMAScript;

            public static class InlineHost
            {
                [ECMAScriptInline("__arg999999999999999999999")]
                public static extern int Read(int value);
            }

            public sealed class Scenario
            {
                public void TestMethod(int value)
                {
                    var result = InlineHost.Read(value);
                }
            }
            """,
            "Inline placeholder",
            "exceeds the supported index range")
    ];

    private static InlineEvaluationProtocolCase Case(
        string id,
        string dimension,
        string source,
        IReadOnlyList<string> expectedFragments,
        IReadOnlyList<string> singleOccurrenceFragments,
        IReadOnlyList<string> orderedFragments)
        => new(
            $"inline-evaluation.{id}",
            dimension,
            source,
            expectedFragments,
            singleOccurrenceFragments,
            orderedFragments);

    private static InlineEvaluationProtocolFailureCase Failure(
        string id,
        string dimension,
        string source,
        params string[] expectedDiagnosticFragments)
        => new(
            $"inline-evaluation.{id}",
            dimension,
            source,
            expectedDiagnosticFragments);
}
