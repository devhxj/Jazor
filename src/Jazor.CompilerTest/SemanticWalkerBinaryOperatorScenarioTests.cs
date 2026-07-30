using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerBinaryOperatorScenarioTests
{
    public static IEnumerable<TestDataRow<BinaryOperatorScenario>> Cases
        => BinaryOperatorScenarioCatalog.All.Select(static scenario =>
            new TestDataRow<BinaryOperatorScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndSources()
    {
        var scenarios = BinaryOperatorScenarioCatalog.All;

        Assert.IsNotEmpty(scenarios);
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Source).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(scenarios.All(static scenario =>
            scenario.Id.StartsWith("binary-operator.", StringComparison.Ordinal)));
        Assert.IsTrue(scenarios.All(static scenario => !string.IsNullOrWhiteSpace(scenario.Dimension)));
        Assert.IsTrue(scenarios.All(static scenario => !string.IsNullOrWhiteSpace(scenario.ExpectedJavaScriptFragment)));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Visit_BinaryOperatorScenario_ProducesDeterministicParsableJavaScript(BinaryOperatorScenario scenario)
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

            public sealed class BinaryOperatorScenarios
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
            assemblyName: "BinaryOperatorScenarios",
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

public sealed record BinaryOperatorScenario(
    string Id,
    string Dimension,
    string Source,
    string ExpectedJavaScriptFragment);

internal static class BinaryOperatorScenarioCatalog
{
    private static readonly BinaryType[] IntegralTypes =
    [
        new("sbyte", "sbyte", "(sbyte)5", "(sbyte)2", "int"),
        new("byte", "byte", "(byte)5", "(byte)2", "int"),
        new("short", "short", "(short)5", "(short)2", "int"),
        new("ushort", "ushort", "(ushort)5", "(ushort)2", "int"),
        new("int", "int", "5", "2", "int"),
        new("uint", "uint", "5u", "2u", "uint"),
        new("long", "long", "5L", "2L", "long"),
        new("ulong", "ulong", "5UL", "2UL", "ulong")
    ];

    private static readonly BinaryType[] NumericTypes =
    [
        .. IntegralTypes,
        new("float", "float", "5f", "2f", "float"),
        new("double", "double", "5d", "2d", "double"),
        new("decimal", "decimal", "5m", "2m", "decimal")
    ];

    private static readonly BinaryContext[] Contexts =
    [
        new("local", static (expression, _) => $"var result = {expression};"),
        new("expression-lambda", static (expression, resultType) => $"Func<{resultType}> calculate = () => {expression};"),
        new("block-lambda", static (expression, resultType) => $"Func<{resultType}> calculate = () => {{ return {expression}; }};"),
        new("argument", static (expression, _) => $"Consume({expression});"),
        new("array-element", static (expression, _) => $"var results = new[] {{ {expression} }};"),
        new("anonymous-property", static (expression, _) => $"var result = new {{ Value = {expression} }};"),
        new("tuple-element", static (expression, _) => $"var result = (Value: {expression}, Marker: 1);"),
        new("conditional-arm", static (expression, _) => $"var result = true ? {expression} : {expression};"),
        new("switch-arm", static (expression, _) => $"var result = 0 switch {{ 0 => {expression}, _ => {expression} }};")
    ];

    public static IReadOnlyList<BinaryOperatorScenario> All { get; } = Build();

    private static IReadOnlyList<BinaryOperatorScenario> Build()
    {
        var scenarios = new List<BinaryOperatorScenario>();

        AddOperators(scenarios, NumericTypes, "arithmetic",
        [
            new("add", "+", "+"),
            new("subtract", "-", "-"),
            new("multiply", "*", "*"),
            new("divide", "/", "/"),
            new("remainder", "%", "%")
        ]);
        AddOperators(scenarios, NumericTypes, "comparison",
        [
            new("equal", "==", "===" , ResultType: "bool"),
            new("not-equal", "!=", "!==", ResultType: "bool"),
            new("less-than", "<", "<", ResultType: "bool"),
            new("less-than-or-equal", "<=", "<=", ResultType: "bool"),
            new("greater-than", ">", ">", ResultType: "bool"),
            new("greater-than-or-equal", ">=", ">=", ResultType: "bool")
        ]);
        AddOperators(scenarios, IntegralTypes, "bitwise",
        [
            new("and", "&", "&"),
            new("or", "|", "|"),
            new("xor", "^", "^")
        ]);
        AddShiftOperators(scenarios);
        AddBooleanOperators(scenarios);
        AddStringOperators(scenarios);

        return scenarios;
    }

    private static void AddOperators(
        List<BinaryOperatorScenario> scenarios,
        IReadOnlyList<BinaryType> types,
        string family,
        IReadOnlyList<BinaryOperator> operators)
    {
        foreach (var type in types)
        {
            foreach (var binaryOperator in operators)
            {
                var resultType = binaryOperator.ResultType ?? type.PromotedResultType;
                AddContexts(
                    scenarios,
                    family,
                    type.Id,
                    binaryOperator.Id,
                    type.DeclarationType,
                    type.LeftValue,
                    type.DeclarationType,
                    type.RightValue,
                    binaryOperator.CSharpToken,
                    binaryOperator.JavaScriptToken,
                    resultType);
            }
        }
    }

    private static void AddShiftOperators(List<BinaryOperatorScenario> scenarios)
    {
        var operators = new[]
        {
            new BinaryOperator("left", "<<", "<<"),
            new BinaryOperator("right", ">>", ">>"),
            new BinaryOperator("unsigned-right", ">>>", ">>>")
        };
        foreach (var type in IntegralTypes)
        {
            foreach (var binaryOperator in operators)
            {
                AddContexts(
                    scenarios,
                    "shift",
                    type.Id,
                    binaryOperator.Id,
                    type.DeclarationType,
                    type.LeftValue,
                    "int",
                    "2",
                    binaryOperator.CSharpToken,
                    binaryOperator.JavaScriptToken,
                    type.PromotedResultType);
            }
        }
    }

    private static void AddBooleanOperators(List<BinaryOperatorScenario> scenarios)
    {
        var operators = new[]
        {
            new BinaryOperator("conditional-and", "&&", "&&", "bool"),
            new BinaryOperator("conditional-or", "||", "||", "bool"),
            new BinaryOperator("and", "&", "&", "bool"),
            new BinaryOperator("or", "|", "|", "bool"),
            new BinaryOperator("xor", "^", "^", "bool"),
            new BinaryOperator("equal", "==", "===", "bool"),
            new BinaryOperator("not-equal", "!=", "!==", "bool")
        };
        foreach (var binaryOperator in operators)
        {
            AddContexts(
                scenarios,
                "boolean",
                "bool",
                binaryOperator.Id,
                "bool",
                "true",
                "bool",
                "false",
                binaryOperator.CSharpToken,
                binaryOperator.JavaScriptToken,
                "bool");
        }
    }

    private static void AddStringOperators(List<BinaryOperatorScenario> scenarios)
    {
        var operators = new[]
        {
            new BinaryOperator("concatenate", "+", "+", "string"),
            new BinaryOperator("equal", "==", "===", "bool"),
            new BinaryOperator("not-equal", "!=", "!==", "bool")
        };
        foreach (var binaryOperator in operators)
        {
            AddContexts(
                scenarios,
                "string",
                "string",
                binaryOperator.Id,
                "string",
                "\"release\"",
                "string",
                "\"candidate\"",
                binaryOperator.CSharpToken,
                binaryOperator.JavaScriptToken,
                binaryOperator.ResultType!);
        }
    }

    private static void AddContexts(
        List<BinaryOperatorScenario> scenarios,
        string family,
        string typeId,
        string operatorId,
        string leftType,
        string leftValue,
        string rightType,
        string rightValue,
        string csharpToken,
        string javaScriptToken,
        string resultType)
    {
        const string leftName = "left";
        const string rightName = "right";
        var expression = $"{leftName} {csharpToken} {rightName}";
        var expected = $"{leftName} {javaScriptToken} {rightName}";

        foreach (var context in Contexts)
        {
            var id = $"binary-operator.{family}.{typeId}.{operatorId}.{context.Id}";
            var body = $$"""
                            {{leftType}} {{leftName}} = {{leftValue}};
                            {{rightType}} {{rightName}} = {{rightValue}};
                            {{context.BuildBody(expression, resultType)}}
                """;
            scenarios.Add(new BinaryOperatorScenario(
                id,
                $"family={family};type={typeId};operator={operatorId};context={context.Id}",
                body,
                expected));
        }
    }

    private sealed record BinaryType(
        string Id,
        string DeclarationType,
        string LeftValue,
        string RightValue,
        string PromotedResultType);

    private sealed record BinaryOperator(
        string Id,
        string CSharpToken,
        string JavaScriptToken,
        string? ResultType = null);

    private sealed record BinaryContext(
        string Id,
        Func<string, string, string> BuildBody);
}
