using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerCompoundAssignmentScenarioTests
{
    public static IEnumerable<TestDataRow<CompoundAssignmentScenario>> Cases
        => CompoundAssignmentScenarioCatalog.All.Select(static scenario =>
            new TestDataRow<CompoundAssignmentScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndSources()
    {
        var scenarios = CompoundAssignmentScenarioCatalog.All;

        Assert.IsNotEmpty(scenarios);
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Source).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(scenarios.All(static scenario =>
            scenario.Id.StartsWith("compound-assignment.", StringComparison.Ordinal)));
        Assert.IsTrue(scenarios.All(static scenario => !string.IsNullOrWhiteSpace(scenario.Dimension)));
        Assert.IsTrue(scenarios.All(static scenario => !string.IsNullOrWhiteSpace(scenario.ExpectedJavaScriptFragment)));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Visit_CompoundAssignmentScenario_ProducesDeterministicParsableJavaScript(CompoundAssignmentScenario scenario)
    {
        var block = GetBlockOperation(scenario.Source);
        var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first, scenario.Id);
        Assert.AreEqual(first, second, scenario.Id);
        StringAssert.Contains(first, scenario.ExpectedJavaScriptFragment, scenario.Id);
        _ = new Parser().ParseScript(first);
    }

    private static IBlockOperation GetBlockOperation(string body)
    {
        var source = $$"""
            using System;

            public sealed class CompoundAssignmentScenarios
            {
                private static void Consume<T>(T value) { }

                public void TestMethod()
                {
            {{body}}
                }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            assemblyName: "CompoundAssignmentScenarios",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "TestMethod");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}

public sealed record CompoundAssignmentScenario(
    string Id,
    string Dimension,
    string Source,
    string ExpectedJavaScriptFragment);

internal static class CompoundAssignmentScenarioCatalog
{
    private static readonly AssignmentType[] IntegralTypes =
    [
        new("sbyte", "sbyte", "(sbyte)5", "(sbyte)2"),
        new("byte", "byte", "(byte)5", "(byte)2"),
        new("short", "short", "(short)5", "(short)2"),
        new("ushort", "ushort", "(ushort)5", "(ushort)2"),
        new("int", "int", "5", "2"),
        new("uint", "uint", "5u", "2u"),
        new("long", "long", "5L", "2L"),
        new("ulong", "ulong", "5UL", "2UL")
    ];

    private static readonly AssignmentType[] NumericTypes =
    [
        .. IntegralTypes,
        new("float", "float", "5f", "2f"),
        new("double", "double", "5d", "2d"),
        new("decimal", "decimal", "5m", "2m")
    ];

    private static readonly AssignmentContext[] Contexts =
    [
        new("local", static statement => (statement, statement)),
        new("lambda", static statement => ($"Action apply = () => {statement};", statement)),
        new("array-element", static statement =>
        {
            var arrayStatement = statement.Replace("left", "values[0]", StringComparison.Ordinal);
            return ($"var values = new[] {{ left }}; {arrayStatement}", arrayStatement);
        }),
        new("if-body", static statement => ($"if (true) {{ {statement} }}", statement)),
        new("for-body", static statement => ($"for (var index = 0; index < 1; index++) {{ {statement} }}", statement)),
        new("while-body", static statement => ($"bool active = false; while (active) {{ {statement} }}", statement)),
        new("try-finally", static statement => ($"try {{ {statement} }} finally {{ Consume(left); }}", statement)),
        new("checked-block", static statement => ($"checked {{ {statement} }}", statement)),
        new("unchecked-block", static statement => ($"unchecked {{ {statement} }}", statement))
    ];

    public static IReadOnlyList<CompoundAssignmentScenario> All { get; } = Build();

    private static IReadOnlyList<CompoundAssignmentScenario> Build()
    {
        var scenarios = new List<CompoundAssignmentScenario>();

        AddOperators(scenarios, NumericTypes, "arithmetic",
        [
            new("add", "+="),
            new("subtract", "-="),
            new("multiply", "*="),
            new("divide", "/="),
            new("remainder", "%=")
        ]);
        AddOperators(scenarios, IntegralTypes, "bitwise",
        [
            new("and", "&="),
            new("or", "|="),
            new("xor", "^=")
        ]);
        AddShiftOperators(scenarios);
        AddStringConcatenation(scenarios);

        return scenarios;
    }

    private static void AddOperators(
        List<CompoundAssignmentScenario> scenarios,
        IReadOnlyList<AssignmentType> types,
        string family,
        IReadOnlyList<AssignmentOperator> operators)
    {
        foreach (var type in types)
        {
            foreach (var assignmentOperator in operators)
            {
                AddContexts(
                    scenarios,
                    family,
                    type.Id,
                    assignmentOperator.Id,
                    type.DeclarationType,
                    type.LeftValue,
                    type.DeclarationType,
                    type.RightValue,
                    assignmentOperator.Token);
            }
        }
    }

    private static void AddShiftOperators(List<CompoundAssignmentScenario> scenarios)
    {
        var operators = new[]
        {
            new AssignmentOperator("left", "<<="),
            new AssignmentOperator("right", ">>="),
            new AssignmentOperator("unsigned-right", ">>>=")
        };
        foreach (var type in IntegralTypes)
        {
            foreach (var assignmentOperator in operators)
            {
                AddContexts(
                    scenarios,
                    "shift",
                    type.Id,
                    assignmentOperator.Id,
                    type.DeclarationType,
                    type.LeftValue,
                    "int",
                    "2",
                    assignmentOperator.Token);
            }
        }
    }

    private static void AddStringConcatenation(List<CompoundAssignmentScenario> scenarios)
        => AddContexts(
            scenarios,
            "string",
            "string",
            "concatenate",
            "string",
            "\"release\"",
            "string",
            "\" candidate\"",
            "+=");

    private static void AddContexts(
        List<CompoundAssignmentScenario> scenarios,
        string family,
        string typeId,
        string operatorId,
        string leftType,
        string leftValue,
        string rightType,
        string rightValue,
        string token)
    {
        var statement = $"left {token} right;";
        foreach (var context in Contexts)
        {
            var (contextBody, expected) = context.Build(statement);
            var id = $"compound-assignment.{family}.{typeId}.{operatorId}.{context.Id}";
            var body = $$"""
                            {{leftType}} left = {{leftValue}};
                            {{rightType}} right = {{rightValue}};
                            {{contextBody}};
                """;
            scenarios.Add(new CompoundAssignmentScenario(
                id,
                $"family={family};type={typeId};operator={operatorId};context={context.Id}",
                body,
                expected));
        }
    }

    private sealed record AssignmentType(
        string Id,
        string DeclarationType,
        string LeftValue,
        string RightValue);

    private sealed record AssignmentOperator(string Id, string Token);

    private sealed record AssignmentContext(
        string Id,
        Func<string, (string Body, string Expected)> Build);
}
