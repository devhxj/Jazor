using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerCustomOperatorBoundaryTests
{
    private static readonly Lazy<IReadOnlyDictionary<string, IBlockOperation>> Operations = new(CreateOperations);

    public static IEnumerable<TestDataRow<CustomOperatorBoundaryScenario>> Cases
        => CustomOperatorBoundaryCatalog.All.Select(static scenario =>
            new TestDataRow<CustomOperatorBoundaryScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndMethods()
    {
        var scenarios = CustomOperatorBoundaryCatalog.All;

        Assert.IsNotEmpty(scenarios);
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.MethodName).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(scenarios.All(static scenario =>
            scenario.Id.StartsWith("custom-operator-boundary.", StringComparison.Ordinal)));
        Assert.IsTrue(scenarios.All(static scenario => !string.IsNullOrWhiteSpace(scenario.Dimension)));
        Assert.IsTrue(scenarios.All(static scenario => scenario.ExpectedDiagnosticFragments.Count > 1));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Visit_UnmappedCustomOperator_ReportsControlledFailure(CustomOperatorBoundaryScenario scenario)
    {
        var block = Operations.Value[scenario.MethodName];
        Assert.IsFalse(
            block.DescendantsAndSelf().Any(static operation => operation.Kind == OperationKind.Invalid),
            scenario.Id);

        var exception = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()), scenario.Id);

        foreach (var fragment in scenario.ExpectedDiagnosticFragments)
            StringAssert.Contains(exception.Message, fragment, scenario.Id);
    }

    private static IReadOnlyDictionary<string, IBlockOperation> CreateOperations()
    {
        const string source = """
            public readonly struct SourceValue
            {
                public SourceValue(int value) => Value = value;
                public int Value { get; }

                public static explicit operator TargetValue(SourceValue value) => new(value.Value);
                public static explicit operator SourceValue(int value) => new(value);
                public static explicit operator int(SourceValue value) => value.Value;
            }

            public readonly struct TargetValue
            {
                public TargetValue(int value) => Value = value;
                public int Value { get; }
            }

            public readonly struct SequenceNumber
            {
                public SequenceNumber(int value) => Value = value;
                public int Value { get; }

                public static SequenceNumber operator ++(SequenceNumber value) => new(value.Value + 1);
            }

            public sealed class CustomOperatorBoundaryFixture
            {
                public void Convert(SourceValue value)
                {
                    TargetValue converted = (TargetValue)value;
                }

                public void ConvertFromNumber(int value)
                {
                    SourceValue converted = (SourceValue)value;
                }

                public void ConvertToNumber(SourceValue value)
                {
                    int converted = (int)value;
                }

                public void Increment(SequenceNumber value)
                {
                    SequenceNumber incremented = ++value;
                }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            assemblyName: "CustomOperatorBoundaryScenarios",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var model = compilation.GetSemanticModel(syntaxTree);
        return syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(static method => method.Identifier.ValueText is
                "Convert" or "ConvertFromNumber" or "ConvertToNumber" or "Increment")
            .ToDictionary(
                static method => method.Identifier.ValueText,
                method => Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(method.Body!)),
                StringComparer.Ordinal);
    }
}

public sealed record CustomOperatorBoundaryScenario(
    string Id,
    string Dimension,
    string MethodName,
    IReadOnlyList<string> ExpectedDiagnosticFragments);

internal static class CustomOperatorBoundaryCatalog
{
    public static IReadOnlyList<CustomOperatorBoundaryScenario> All { get; } =
    [
        new(
            "custom-operator-boundary.conversion.unmapped-source-operator",
            "operation=conversion;mapping=absent;fallback=forbidden",
            "Convert",
            [
                "Conversion operator 'static SourceValue.explicit operator TargetValue(SourceValue)'",
                "requires an explicit whitelist/ECMAScript mapping",
                "cannot fall back to raw JavaScript conversion"
            ]),
        new(
            "custom-operator-boundary.conversion.number-to-unmapped-source",
            "operation=conversion;source=number;target=class;fallback=forbidden",
            "ConvertFromNumber",
            [
                "Conversion operator 'static SourceValue.explicit operator SourceValue(int)'",
                "requires an explicit whitelist/ECMAScript mapping",
                "cannot fall back to raw JavaScript conversion"
            ]),
        new(
            "custom-operator-boundary.conversion.unmapped-source-to-number",
            "operation=conversion;source=class;target=number;fallback=forbidden",
            "ConvertToNumber",
            [
                "Conversion operator 'static SourceValue.explicit operator int(SourceValue)'",
                "requires an explicit whitelist/ECMAScript mapping",
                "cannot fall back to raw JavaScript conversion"
            ]),
        new(
            "custom-operator-boundary.increment.unmapped-value-operator",
            "operation=prefix-increment;mapping=absent;fallback=forbidden",
            "Increment",
            [
                "Increment/decrement operator 'static SequenceNumber.operator ++(SequenceNumber)'",
                "requires an explicit whitelist mapping",
                "cannot fall back to raw JavaScript update semantics"
            ])
    ];
}
