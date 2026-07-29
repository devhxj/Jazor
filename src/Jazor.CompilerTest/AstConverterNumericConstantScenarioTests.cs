using System.Numerics;
using Acornima;
using Acornima.Ast;
using Jazor.ComplierTest;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class AstConverterNumericConstantScenarioTests
{
    public static IEnumerable<TestDataRow<AstConverterNumericConstantScenario>> Cases
        => AstConverterNumericConstantScenarioCatalog.All.Select(static scenario =>
            new TestDataRow<AstConverterNumericConstantScenario>(scenario)
            {
                DisplayName = scenario.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsInputsAndKinds()
    {
        var scenarios = AstConverterNumericConstantScenarioCatalog.All;

        Assert.IsNotEmpty(scenarios);
        Assert.HasCount(scenarios.Count, scenarios.Select(static scenario => scenario.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            scenarios.Count,
            scenarios.Select(static scenario => scenario.InputIdentity).Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            Enum.GetValues<AstNumericConstantExpressionKind>().Length,
            scenarios.Select(static scenario => scenario.ExpectedKind).Distinct());
        Assert.IsTrue(scenarios.All(static scenario =>
            scenario.Id.StartsWith("ast-converter.numeric-constant.", StringComparison.Ordinal)));
        Assert.IsTrue(scenarios.All(static scenario => !string.IsNullOrWhiteSpace(scenario.Dimension)));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public async Task Convert_MemberConst_UsesCanonicalEstreeNumericExpression(
        AstConverterNumericConstantScenario scenario)
    {
        var fixture = CompileModule(scenario);
        var converter = new AstConverter(fixture.Module, fixture.SemanticModel);

        var module = await converter.Convert();

        Assert.IsNotNull(module, scenario.Id);
        Assert.HasCount(1, module.Body, scenario.Id);
        Assert.IsInstanceOfType<ExportNamedDeclaration>(module.Body[0], scenario.Id);
        var export = (ExportNamedDeclaration)module.Body[0];
        Assert.IsInstanceOfType<ClassDeclaration>(export.Declaration, scenario.Id);
        var declaration = (ClassDeclaration)export.Declaration!;
        Assert.AreEqual("Values", declaration.Id?.Name, scenario.Id);

        var property = declaration.Body.Body.OfType<PropertyDefinition>().Single();
        Assert.IsTrue(property.Static, scenario.Id);
        Assert.IsFalse(property.Computed, scenario.Id);
        Assert.IsInstanceOfType<Identifier>(property.Key, scenario.Id);
        Assert.AreEqual("value", ((Identifier)property.Key).Name, scenario.Id);
        Assert.IsNotNull(property.Value, scenario.Id);
        AssertNumericExpression(property.Value, scenario);

        _ = new Parser().ParseModule(module.ToKnRECMAScript());
    }

    private static void AssertNumericExpression(
        Expression expression,
        AstConverterNumericConstantScenario scenario)
    {
        switch (scenario.ExpectedKind)
        {
            case AstNumericConstantExpressionKind.Number:
                AssertNumericLiteral(expression, scenario.ExpectedNumber!.Value, scenario.ExpectedOperandRaw!, scenario.Id);
                break;
            case AstNumericConstantExpressionKind.NegativeNumber:
                AssertUnaryOperand<NumericLiteral>(expression, scenario, static (operand, expected) =>
                {
                    Assert.AreEqual(expected.ExpectedNumber!.Value, operand.Value, expected.Id);
                    Assert.AreEqual(expected.ExpectedOperandRaw, operand.Raw, expected.Id);
                });
                break;
            case AstNumericConstantExpressionKind.BigInt:
                AssertBigIntLiteral(expression, scenario.ExpectedBigInt!, scenario.ExpectedOperandRaw!, scenario.Id);
                break;
            case AstNumericConstantExpressionKind.NegativeBigInt:
                AssertUnaryOperand<BigIntLiteral>(expression, scenario, static (operand, expected) =>
                {
                    Assert.AreEqual(BigInteger.Parse(expected.ExpectedBigInt!), operand.Value, expected.Id);
                    Assert.AreEqual(expected.ExpectedOperandRaw, operand.Raw, expected.Id);
                });
                break;
            case AstNumericConstantExpressionKind.NaN:
                AssertIdentifier(expression, "NaN", scenario.Id);
                break;
            case AstNumericConstantExpressionKind.PositiveInfinity:
                AssertIdentifier(expression, "Infinity", scenario.Id);
                break;
            case AstNumericConstantExpressionKind.NegativeInfinity:
                AssertUnaryOperand<Identifier>(expression, scenario, static (operand, expected) =>
                    Assert.AreEqual("Infinity", operand.Name, expected.Id));
                break;
            default:
                Assert.Fail($"{scenario.Id}: unsupported expected kind '{scenario.ExpectedKind}'.");
                break;
        }
    }

    private static void AssertNumericLiteral(Expression expression, double value, string raw, string scenarioId)
    {
        Assert.IsInstanceOfType<NumericLiteral>(expression, scenarioId);
        var literal = (NumericLiteral)expression;
        Assert.AreEqual(value, literal.Value, scenarioId);
        Assert.AreEqual(raw, literal.Raw, scenarioId);
    }

    private static void AssertBigIntLiteral(Expression expression, string value, string raw, string scenarioId)
    {
        Assert.IsInstanceOfType<BigIntLiteral>(expression, scenarioId);
        var literal = (BigIntLiteral)expression;
        Assert.AreEqual(BigInteger.Parse(value), literal.Value, scenarioId);
        Assert.AreEqual(raw, literal.Raw, scenarioId);
    }

    private static void AssertIdentifier(Expression expression, string name, string scenarioId)
    {
        Assert.IsInstanceOfType<Identifier>(expression, scenarioId);
        Assert.AreEqual(name, ((Identifier)expression).Name, scenarioId);
    }

    private static void AssertUnaryOperand<TExpression>(
        Expression expression,
        AstConverterNumericConstantScenario scenario,
        Action<TExpression, AstConverterNumericConstantScenario> assertOperand)
        where TExpression : Expression
    {
        Assert.IsInstanceOfType<NonUpdateUnaryExpression>(expression, scenario.Id);
        var unary = (NonUpdateUnaryExpression)expression;
        Assert.AreEqual(Operator.UnaryNegation, unary.Operator, scenario.Id);
        Assert.IsInstanceOfType<TExpression>(unary.Argument, scenario.Id);
        assertOperand((TExpression)unary.Argument, scenario);
    }

    private static AstConverterNumericConstantFixture CompileModule(
        AstConverterNumericConstantScenario scenario)
    {
        var source = $$"""
            public static class TestModule
            {
                public sealed class Values
                {
                    public const {{scenario.TypeName}} Value = {{scenario.ConstantExpression}};
                }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: $"{scenario.Id}.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "AstConverterNumericConstants_" + scenario.Id.Replace('.', '_'),
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(
            0,
            errors,
            $"{scenario.Id}{Environment.NewLine}{string.Join(Environment.NewLine, errors.Select(static error => error.ToString()))}");

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var module = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.ValueText == "TestModule")
            .Select(declaration => semanticModel.GetDeclaredSymbol(declaration))
            .OfType<INamedTypeSymbol>()
            .Single();
        return new AstConverterNumericConstantFixture(module, semanticModel);
    }
}

public enum AstNumericConstantExpressionKind
{
    Number,
    NegativeNumber,
    BigInt,
    NegativeBigInt,
    NaN,
    PositiveInfinity,
    NegativeInfinity
}

public sealed record AstConverterNumericConstantScenario(
    string Id,
    string Dimension,
    string TypeName,
    string ConstantExpression,
    AstNumericConstantExpressionKind ExpectedKind,
    double? ExpectedNumber,
    string? ExpectedBigInt,
    string? ExpectedOperandRaw)
{
    public string InputIdentity => $"{TypeName}:{ConstantExpression}";
}

public sealed record AstConverterNumericConstantFixture(
    INamedTypeSymbol Module,
    SemanticModel SemanticModel);

internal static class AstConverterNumericConstantScenarioCatalog
{
    public static IReadOnlyList<AstConverterNumericConstantScenario> All { get; } =
    [
        NegativeNumber("sbyte-min", "signed-8-bit-minimum", "sbyte", "-128", 128, "128"),
        Number("byte-max", "unsigned-8-bit-maximum", "byte", "255", 255, "255"),
        NegativeNumber("short-min", "signed-16-bit-minimum", "short", "-32768", 32768, "32768"),
        Number("ushort-max", "unsigned-16-bit-maximum", "ushort", "65535", 65535, "65535"),
        NegativeNumber("int-min", "signed-32-bit-minimum", "int", "-2147483648", 2147483648, "2147483648"),
        Number("uint-max", "unsigned-32-bit-maximum", "uint", "4294967295U", 4294967295, "4294967295"),
        NegativeBigInt(
            "long-min",
            "signed-64-bit-minimum",
            "long",
            "-9223372036854775808L",
            "9223372036854775808",
            "9223372036854775808n"),
        BigInt(
            "ulong-max",
            "unsigned-64-bit-maximum",
            "ulong",
            "18446744073709551615UL",
            "18446744073709551615",
            "18446744073709551615n"),
        NegativeNumber("float-finite", "negative-single-precision", "float", "-1.25F", 1.25, "1.25"),
        NegativeNumber("double-finite", "negative-double-precision", "double", "-2.5D", 2.5, "2.5"),
        NegativeNumber("decimal-finite", "negative-decimal", "decimal", "-123.456M", 123.456, "123.456"),
        NegativeNumber("float-negative-zero", "single-precision-negative-zero", "float", "-0.0F", 0, "0"),
        NegativeNumber("double-negative-zero", "double-precision-negative-zero", "double", "-0.0D", 0, "0"),
        Special("float-nan", "single-precision-not-a-number", "float", "float.NaN", AstNumericConstantExpressionKind.NaN),
        Special(
            "float-positive-infinity",
            "single-precision-positive-infinity",
            "float",
            "float.PositiveInfinity",
            AstNumericConstantExpressionKind.PositiveInfinity),
        Special(
            "float-negative-infinity",
            "single-precision-negative-infinity",
            "float",
            "float.NegativeInfinity",
            AstNumericConstantExpressionKind.NegativeInfinity),
        Special("double-nan", "double-precision-not-a-number", "double", "double.NaN", AstNumericConstantExpressionKind.NaN),
        Special(
            "double-positive-infinity",
            "double-precision-positive-infinity",
            "double",
            "double.PositiveInfinity",
            AstNumericConstantExpressionKind.PositiveInfinity),
        Special(
            "double-negative-infinity",
            "double-precision-negative-infinity",
            "double",
            "double.NegativeInfinity",
            AstNumericConstantExpressionKind.NegativeInfinity)
    ];

    private static AstConverterNumericConstantScenario Number(
        string id,
        string dimension,
        string typeName,
        string expression,
        double expectedNumber,
        string expectedRaw)
        => Create(
            id,
            dimension,
            typeName,
            expression,
            AstNumericConstantExpressionKind.Number,
            expectedNumber,
            null,
            expectedRaw);

    private static AstConverterNumericConstantScenario NegativeNumber(
        string id,
        string dimension,
        string typeName,
        string expression,
        double expectedMagnitude,
        string expectedRaw)
        => Create(
            id,
            dimension,
            typeName,
            expression,
            AstNumericConstantExpressionKind.NegativeNumber,
            expectedMagnitude,
            null,
            expectedRaw);

    private static AstConverterNumericConstantScenario BigInt(
        string id,
        string dimension,
        string typeName,
        string expression,
        string expectedValue,
        string expectedRaw)
        => Create(
            id,
            dimension,
            typeName,
            expression,
            AstNumericConstantExpressionKind.BigInt,
            null,
            expectedValue,
            expectedRaw);

    private static AstConverterNumericConstantScenario NegativeBigInt(
        string id,
        string dimension,
        string typeName,
        string expression,
        string expectedMagnitude,
        string expectedRaw)
        => Create(
            id,
            dimension,
            typeName,
            expression,
            AstNumericConstantExpressionKind.NegativeBigInt,
            null,
            expectedMagnitude,
            expectedRaw);

    private static AstConverterNumericConstantScenario Special(
        string id,
        string dimension,
        string typeName,
        string expression,
        AstNumericConstantExpressionKind kind)
        => Create(id, dimension, typeName, expression, kind, null, null, null);

    private static AstConverterNumericConstantScenario Create(
        string id,
        string dimension,
        string typeName,
        string expression,
        AstNumericConstantExpressionKind kind,
        double? expectedNumber,
        string? expectedBigInt,
        string? expectedRaw)
        => new(
            $"ast-converter.numeric-constant.{id}",
            dimension,
            typeName,
            expression,
            kind,
            expectedNumber,
            expectedBigInt,
            expectedRaw);
}
