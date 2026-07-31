using System.Text.RegularExpressions;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class SemanticNameKeyScenarioTests
{
    private static readonly Regex GeneratedNameRegex = new(
        @"__[a-z0-9]+\$[0-9a-f]+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static IEnumerable<TestDataRow<SemanticNameKeyScenario>> Cases
        => SemanticNameKeyScenarioCatalog.All.Select(static testCase =>
            new TestDataRow<SemanticNameKeyScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsKindsAndInputs()
    {
        var cases = SemanticNameKeyScenarioCatalog.All;

        Assert.IsNotEmpty(cases);
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Dimension).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.BaselineSource).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.EquivalentSource).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.DistinctSource).Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            Enum.GetValues<SemanticNameKeyScenarioKind>().Length,
            cases.Select(static testCase => testCase.Kind).Distinct());
        Assert.IsTrue(cases.All(static testCase =>
            testCase.Id.StartsWith("semantic-name-key.", StringComparison.Ordinal)));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public async Task Convert_SemanticNameKey_IsTriviaStableAndSeparatesMeaning(
        SemanticNameKeyScenario testCase)
    {
        var baseline = await ConvertModuleAsync(testCase.BaselineSource, testCase.Id);
        var repeated = await ConvertModuleAsync(testCase.BaselineSource, testCase.Id);
        var equivalent = await ConvertModuleAsync(testCase.EquivalentSource, testCase.Id);
        var distinct = await ConvertModuleAsync(testCase.DistinctSource, testCase.Id);

        var baselineNames = ExtractGeneratedNames(baseline, testCase.Id);
        var repeatedNames = ExtractGeneratedNames(repeated, testCase.Id);
        var equivalentNames = ExtractGeneratedNames(equivalent, testCase.Id);
        var distinctNames = ExtractGeneratedNames(distinct, testCase.Id);

        CollectionAssert.AreEqual(baselineNames, repeatedNames, testCase.Id);
        CollectionAssert.AreEqual(baselineNames, equivalentNames, testCase.Id);
        Assert.IsFalse(
            baselineNames.SequenceEqual(distinctNames, StringComparer.Ordinal),
            $"{testCase.Id}: semantic variants unexpectedly reused [{string.Join(", ", baselineNames)}].");
    }

    private static string[] ExtractGeneratedNames(string script, string scenarioId)
    {
        var names = GeneratedNameRegex.Matches(script)
            .Select(static match => match.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        Assert.IsNotEmpty(names, scenarioId);
        return names;
    }

    private static async Task<string> ConvertModuleAsync(string source, string scenarioId)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            scenarioId + ".cs");
        var compilation = CSharpCompilation.Create(
            "SemanticNameKey_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, scenarioId + ": " + string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var classDeclaration = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .First();
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        Assert.IsNotNull(classSymbol, scenarioId);

        var module = await new AstConverter(classSymbol, semanticModel).Convert();
        var script = module?.ToKnRECMAScript();
        Assert.IsNotNull(script, scenarioId);
        return script;
    }
}

public enum SemanticNameKeyScenarioKind
{
    ObjectCreationArgument,
    ObjectCreationInitializer,
    BinaryOperator,
    TupleBinaryOperator,
    StaticPropertyReference,
    StaticMethodReference
}

public sealed record SemanticNameKeyScenario(
    string Id,
    string Dimension,
    SemanticNameKeyScenarioKind Kind,
    string BaselineSource,
    string EquivalentSource,
    string DistinctSource);

internal static class SemanticNameKeyScenarioCatalog
{
    public static IReadOnlyList<SemanticNameKeyScenario> All { get; } =
    [
        new(
            "semantic-name-key.object-creation-argument",
            "constructor-argument-participates-in-switch-input-name",
            SemanticNameKeyScenarioKind.ObjectCreationArgument,
            """
            public static class TestClass
            {
                public sealed class Box
                {
                    public Box(int value)
                    {
                        Value = value;
                    }

                    public int Value { get; }
                }

                public static int Read()
                    => new Box(1) switch
                    {
                        { Value: > 0 } => 1,
                        _ => 0
                    };
            }
            """,
            """
            public static class TestClass
            {
                public sealed class Box
                {
                    public Box(int value) { Value = value; }
                    public int Value { get; }
                }

                // Trivia and layout must not perturb the generated owner key.
                public static int Read() =>
                    new Box(
                        1
                    ) switch
                    {
                        { Value: > 0 } => 1,
                        _ => 0
                    };
            }
            """,
            """
            public static class TestClass
            {
                public sealed class Box
                {
                    public Box(int value)
                    {
                        Value = value;
                    }

                    public int Value { get; }
                }

                public static int Read()
                    => new Box(2) switch
                    {
                        { Value: > 0 } => 1,
                        _ => 0
                    };
            }
            """),
        new(
            "semantic-name-key.binary-switch-input",
            "binary-operator-participates-in-switch-input-name",
            SemanticNameKeyScenarioKind.BinaryOperator,
            """
            public static class TestClass
            {
                public static int Classify(int left, int right)
                    => (left + right) switch
                    {
                        > 0 => 1,
                        _ => 0
                    };
            }
            """,
            """
            public static class TestClass
            {
                public static int Classify(
                    int left,
                    int right)
                    => (
                        left + // trivia only
                        right
                    ) switch
                    {
                        > 0 => 1,
                        _ => 0
                    };
            }
            """,
            """
            public static class TestClass
            {
                public static int Classify(int left, int right)
                    => (left - right) switch
                    {
                        > 0 => 1,
                        _ => 0
                    };
            }
            """),
        new(
            "semantic-name-key.tuple-binary-switch-input",
            "tuple-comparison-operator-participates-in-switch-input-name",
            SemanticNameKeyScenarioKind.TupleBinaryOperator,
            """
            public static class TestClass
            {
                public static int Compare(int leftId, string leftName, int rightId, string rightName)
                    => ((leftId, leftName) == (rightId, rightName)) switch
                    {
                        true => 1,
                        _ => 0
                    };
            }
            """,
            """
            public static class TestClass
            {
                public static int Compare(
                    int leftId,
                    string leftName,
                    int rightId,
                    string rightName)
                    => (
                        (leftId, leftName)
                        ==
                        (rightId, rightName)
                    ) switch
                    {
                        true => 1,
                        _ => 0
                    };
            }
            """,
            """
            public static class TestClass
            {
                public static int Compare(int leftId, string leftName, int rightId, string rightName)
                    => ((leftId, leftName) != (rightId, rightName)) switch
                    {
                        true => 1,
                        _ => 0
                    };
            }
            """),
        new(
            "semantic-name-key.object-creation-initializer",
            "object-initializer-participates-in-switch-input-name",
            SemanticNameKeyScenarioKind.ObjectCreationInitializer,
            """
            public static class TestClass
            {
                public sealed class Box
                {
                    public int Value { get; set; }
                }

                public static int Read()
                    => new Box { Value = 1 } switch
                    {
                        { Value: > 0 } => 1,
                        _ => 0
                    };
            }
            """,
            """
            public static class TestClass
            {
                public sealed class Box
                {
                    public int Value { get; set; }
                }

                public static int Read() =>
                    new Box
                    {
                        // Trivia must not change the initializer identity.
                        Value = 1
                    } switch
                    {
                        { Value: > 0 } => 1,
                        _ => 0
                    };
            }
            """,
            """
            public static class TestClass
            {
                public sealed class Box
                {
                    public int Value { get; set; }
                }

                public static int Read()
                    => new Box { Value = 2 } switch
                    {
                        { Value: > 0 } => 1,
                        _ => 0
                    };
            }
            """),
        new(
            "semantic-name-key.static-property-switch-input",
            "static-property-symbol-participates-in-switch-input-name",
            SemanticNameKeyScenarioKind.StaticPropertyReference,
            """
            public static class TestClass
            {
                public static int Primary => 1;
                public static int Secondary => 2;

                public static int Read()
                    => Primary switch
                    {
                        > 0 => 1,
                        _ => 0
                    };
            }
            """,
            """
            public static class TestClass
            {
                public static int Primary => 1;
                public static int Secondary => 2;

                public static int Read() =>
                    Primary // trivia only
                    switch
                    {
                        > 0 => 1,
                        _ => 0
                    };
            }
            """,
            """
            public static class TestClass
            {
                public static int Primary => 1;
                public static int Secondary => 2;

                public static int Read()
                    => Secondary switch
                    {
                        > 0 => 1,
                        _ => 0
                    };
            }
            """),
        new(
            "semantic-name-key.static-method-reference-switch-input",
            "static-method-group-symbol-participates-in-switch-input-name",
            SemanticNameKeyScenarioKind.StaticMethodReference,
            """
            using System;

            public static class TestClass
            {
                public static int Primary() => 1;
                public static int Secondary() => 2;

                public static int Read()
                    => ((Func<int>)Primary) switch
                    {
                        null => 0,
                        _ => 1
                    };
            }
            """,
            """
            using System;

            public static class TestClass
            {
                public static int Primary() => 1;
                public static int Secondary() => 2;

                public static int Read() =>
                    ((Func<int>)
                        Primary) // trivia only
                    switch
                    {
                        null => 0,
                        _ => 1
                    };
            }
            """,
            """
            using System;

            public static class TestClass
            {
                public static int Primary() => 1;
                public static int Secondary() => 2;

                public static int Read()
                    => ((Func<int>)Secondary) switch
                    {
                        null => 0,
                        _ => 1
                    };
            }
            """)
    ];
}
