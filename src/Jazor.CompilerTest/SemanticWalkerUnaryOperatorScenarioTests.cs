using Acornima;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerUnaryOperatorScenarioTests
{
    public static IEnumerable<TestDataRow<UnaryOperatorScenario>> Cases
        => UnaryOperatorScenarioCatalog.All.Select(static scenario =>
            new TestDataRow<UnaryOperatorScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndSources()
    {
        var scenarios = UnaryOperatorScenarioCatalog.All;

        Assert.IsNotEmpty(scenarios);
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Source).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(scenarios.All(static scenario =>
            scenario.Id.StartsWith("unary-operator.", StringComparison.Ordinal)));
        Assert.IsTrue(scenarios.All(static scenario => !string.IsNullOrWhiteSpace(scenario.Dimension)));
        Assert.IsTrue(scenarios.All(static scenario => !string.IsNullOrWhiteSpace(scenario.ExpectedJavaScriptFragment)));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Visit_UnaryOperatorScenario_ProducesDeterministicParsableJavaScript(UnaryOperatorScenario scenario)
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

            public sealed class UnaryOperatorScenarios
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
            assemblyName: "UnaryOperatorScenarios",
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

public sealed record UnaryOperatorScenario(
    string Id,
    string Dimension,
    string Source,
    string ExpectedJavaScriptFragment);

internal static class UnaryOperatorScenarioCatalog
{
    private static readonly UnaryType[] IntegralTypes =
    [
        new("sbyte", "sbyte", "(sbyte)5", "int"),
        new("byte", "byte", "(byte)5", "int"),
        new("short", "short", "(short)5", "int"),
        new("ushort", "ushort", "(ushort)5", "int"),
        new("int", "int", "5", "int"),
        new("uint", "uint", "5u", "uint"),
        new("long", "long", "5L", "long"),
        new("ulong", "ulong", "5UL", "ulong")
    ];

    private static readonly UnaryType[] NumericTypes =
    [
        .. IntegralTypes,
        new("float", "float", "5f", "float"),
        new("double", "double", "5d", "double"),
        new("decimal", "decimal", "5m", "decimal")
    ];

    private static readonly UnaryExpressionContext[] ExpressionContexts =
    [
        new("local", static (expression, _) => $"var result = {expression};"),
        new("expression-lambda", static (expression, resultType) => $"Func<{resultType}> calculate = () => {expression};"),
        new("block-lambda", static (expression, resultType) => $"Func<{resultType}> calculate = () => {{ return {expression}; }};"),
        new("argument", static (expression, _) => $"Consume({expression});"),
        new("array-element", static (expression, _) => $"var results = new[] {{ {expression} }};"),
        new("anonymous-property", static (expression, _) => $"var result = new {{ Value = {expression} }};"),
        new("conditional-arm", static (expression, _) => $"var result = true ? {expression} : {expression};"),
        new("switch-arm", static (expression, _) => $"var result = 0 switch {{ 0 => {expression}, _ => {expression} }};")
    ];

    private static readonly MutationContext[] MutationContexts =
    [
        new("local-result", static (expression, _) => ($"var result = {expression};", expression)),
        new("local-statement", static (expression, _) => ($"{expression};", expression)),
        new("lambda", static (expression, resultType) => ($"Func<{resultType}> update = () => {expression};", expression)),
        new("argument", static (expression, _) => ($"Consume({expression});", expression)),
        new("conditional-arm", static (expression, _) => ($"var result = true ? {expression} : {expression};", expression)),
        new("anonymous-property", static (expression, _) => ($"var result = new {{ Value = {expression} }};", expression)),
        new("array-result", static (expression, _) =>
        {
            var arrayExpression = expression.Replace("value", "values[0]", StringComparison.Ordinal);
            return ($"var values = new[] {{ value }}; var result = {arrayExpression};", arrayExpression);
        }),
        new("array-statement", static (expression, _) =>
        {
            var arrayExpression = expression.Replace("value", "values[0]", StringComparison.Ordinal);
            return ($"var values = new[] {{ value }}; {arrayExpression};", arrayExpression);
        })
    ];

    public static IReadOnlyList<UnaryOperatorScenario> All { get; } = Build();

    private static IReadOnlyList<UnaryOperatorScenario> Build()
    {
        var scenarios = new List<UnaryOperatorScenario>();

        foreach (var type in NumericTypes)
            AddExpressionContexts(scenarios, "numeric", type, new UnaryOperation("plus", "+", type.PromotedResultType));

        foreach (var type in NumericTypes.Where(static type => type.Id != "ulong"))
        {
            var resultType = type.Id == "uint" ? "long" : type.PromotedResultType;
            AddExpressionContexts(
                scenarios,
                "numeric",
                type,
                new UnaryOperation(
                    "minus",
                    "-",
                    resultType,
                    type.Id == "uint" ? "-BigInt(value)" : null));
        }

        foreach (var type in IntegralTypes)
            AddExpressionContexts(scenarios, "integral", type, new UnaryOperation("complement", "~", type.PromotedResultType));

        AddExpressionContexts(
            scenarios,
            "boolean",
            new UnaryType("bool", "bool", "true", "bool"),
            new UnaryOperation("not", "!", "bool"));

        foreach (var type in NumericTypes)
        {
            AddMutationContexts(scenarios, type, "prefix-increment", "++", prefix: true);
            AddMutationContexts(scenarios, type, "prefix-decrement", "--", prefix: true);
            AddMutationContexts(scenarios, type, "postfix-increment", "++", prefix: false);
            AddMutationContexts(scenarios, type, "postfix-decrement", "--", prefix: false);
        }

        return scenarios;
    }

    private static void AddExpressionContexts(
        List<UnaryOperatorScenario> scenarios,
        string family,
        UnaryType type,
        UnaryOperation operation)
    {
        var expression = $"{operation.Token}value";
        foreach (var context in ExpressionContexts)
        {
            var id = $"unary-operator.{family}.{type.Id}.{operation.Id}.{context.Id}";
            var body = $$"""
                            {{type.DeclarationType}} value = {{type.Value}};
                            {{context.Build(expression, operation.ResultType)}}
                """;
            scenarios.Add(new UnaryOperatorScenario(
                id,
                $"family={family};type={type.Id};operator={operation.Id};context={context.Id}",
                body,
                operation.ExpectedJavaScriptExpression ?? expression));
        }
    }

    private static void AddMutationContexts(
        List<UnaryOperatorScenario> scenarios,
        UnaryType type,
        string operationId,
        string token,
        bool prefix)
    {
        var expression = prefix ? $"{token}value" : $"value{token}";
        foreach (var context in MutationContexts)
        {
            var (contextBody, expected) = context.Build(expression, type.DeclarationType);
            var id = $"unary-operator.mutation.{type.Id}.{operationId}.{context.Id}";
            var body = $$"""
                            {{type.DeclarationType}} value = {{type.Value}};
                            {{contextBody}}
                """;
            scenarios.Add(new UnaryOperatorScenario(
                id,
                $"family=mutation;type={type.Id};operator={operationId};context={context.Id}",
                body,
                expected));
        }
    }

    private sealed record UnaryType(
        string Id,
        string DeclarationType,
        string Value,
        string PromotedResultType);

    private sealed record UnaryOperation(
        string Id,
        string Token,
        string ResultType,
        string? ExpectedJavaScriptExpression = null);

    private sealed record UnaryExpressionContext(
        string Id,
        Func<string, string, string> Build);

    private sealed record MutationContext(
        string Id,
        Func<string, string, (string Body, string Expected)> Build);
}
