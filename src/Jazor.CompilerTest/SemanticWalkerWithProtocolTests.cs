using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerWithProtocolTests
{
    private static readonly Lazy<IReadOnlyDictionary<string, IWithOperation>> Operations = new(CreateOperations);

    public static IEnumerable<TestDataRow<WithProtocolCase>> Cases
        => WithProtocolCatalog.Cases.Select(static testCase =>
            new TestDataRow<WithProtocolCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndSources()
    {
        var cases = WithProtocolCatalog.Cases;

        Assert.IsNotEmpty(cases);
        Assert.HasCount(cases.Count, cases.Select(static item => item.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Count, cases.Select(static item => item.Dimension).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Count, cases.Select(static item => item.Source).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(cases.All(static item => item.Id.StartsWith("with-protocol.", StringComparison.Ordinal)));
        Assert.IsTrue(cases.All(static item => item.ExpectedFragments.Count > 0));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Visit_WithProtocol_UsesBoundMembersAndPreservesEvaluationContract(WithProtocolCase testCase)
    {
        var operation = Operations.Value[testCase.Id];
        var first = new SemanticWalker(true).VisitWith(operation, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).VisitWith(operation, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first, testCase.Id);
        Assert.AreEqual(first, second, testCase.Id);

        var previousIndex = -1;
        foreach (var fragment in testCase.ExpectedFragments)
        {
            var index = first.IndexOf(fragment, StringComparison.Ordinal);
            Assert.IsGreaterThan(previousIndex, index, $"{testCase.Id}: {fragment}");
            previousIndex = index;
        }

        if (testCase.SingleOccurrenceFragment is not null)
        {
            Assert.AreEqual(
                1,
                CountOccurrences(first, testCase.SingleOccurrenceFragment),
                $"{testCase.Id}: {testCase.SingleOccurrenceFragment}");
        }

        _ = new Parser().ParseExpression($"({first})");
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

    private static IReadOnlyDictionary<string, IWithOperation> CreateOperations()
        => WithProtocolCatalog.Cases.ToDictionary(
            static testCase => testCase.Id,
            static testCase => CreateOperation(testCase),
            StringComparer.Ordinal);

    private static IWithOperation CreateOperation(WithProtocolCase testCase)
    {
        var source = $$"""
            using System;
            using System.ComponentModel;

            {{testCase.Source}}
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            $"WithProtocol_{testCase.Id.Replace('.', '_')}",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, $"{testCase.Id}{Environment.NewLine}{string.Join(Environment.NewLine, errors.Select(static error => error.ToString()))}");

        var model = compilation.GetSemanticModel(syntaxTree);
        var withExpression = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<WithExpressionSyntax>()
            .Single();
        Assert.IsInstanceOfType<IWithOperation>(model.GetOperation(withExpression), testCase.Id);
        return (IWithOperation)model.GetOperation(withExpression)!;
    }
}

public sealed record WithProtocolCase(
    string Id,
    string Dimension,
    string Source,
    IReadOnlyList<string> ExpectedFragments,
    string? SingleOccurrenceFragment = null);

internal static class WithProtocolCatalog
{
    public static IReadOnlyList<WithProtocolCase> Cases { get; } =
    [
        Case(
            "record.multiple-properties",
            "carrier=record-class;targets=properties;count=2;order=source",
            """
            public sealed record Person(string Name, int Age);

            public sealed class Scenario
            {
                public Person Update(Person person)
                    => person with { Name = "Jane", Age = 31 };
            }
            """,
            ["...person", "name: \"Jane\"", "age: 31"]),
        Case(
            "record.configured-property-name",
            "carrier=record-class;target=property;name=description-alias;key=non-identifier",
            """
            public sealed record Person
            {
                [Description("@#display-name")]
                public string Name { get; init; } = "";
            }

            public sealed class Scenario
            {
                public Person Update(Person person)
                    => person with { Name = "Jane" };
            }
            """,
            ["...person", "\"display-name\": \"Jane\""]),
        Case(
            "record-struct.field",
            "carrier=record-struct;target=field;name=default-mapping;value=literal",
            """
            public record struct Counter
            {
                public int Value;
            }

            public sealed class Scenario
            {
                public Counter Update(Counter counter)
                    => counter with { Value = 2 };
            }
            """,
            ["...counter", "value: 2"]),
        Case(
            "anonymous.property",
            "carrier=anonymous;target=property;name=default-mapping;scope=local",
            """
            public sealed class Scenario
            {
                public string Update()
                {
                    var person = new { DisplayName = "John" };
                    var updated = person with { DisplayName = "Jane" };
                    return updated.DisplayName;
                }
            }
            """,
            ["...person", "displayName: \"Jane\""]),
        Case(
            "record.tuple-target-view",
            "carrier=record-class;target=tuple-property;source-labels=different;mapping=target-view",
            """
            public sealed record Person((string first, int years) Info);

            public sealed class Scenario
            {
                public Person Update(Person person)
                    => person with { Info = (name: "Jane", age: 40) };
            }
            """,
            ["...person", "info: { first: \"Jane\", years: 40 }"]),
        Case(
            "record.side-effecting-operand",
            "carrier=record-class;operand=invocation;side-effect-count=once;target=property",
            """
            public sealed record Person(string Name, int Age);

            public sealed class Scenario
            {
                private static Person Next() => new("John", 30);

                public static Person Update()
                    => Next() with { Age = 31 };
            }
            """,
            ["next()", "age: 31"],
            "next()")
    ];

    private static WithProtocolCase Case(
        string id,
        string dimension,
        string source,
        IReadOnlyList<string> expectedFragments,
        string? singleOccurrenceFragment = null)
        => new($"with-protocol.{id}", dimension, source, expectedFragments, singleOccurrenceFragment);
}
