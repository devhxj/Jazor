using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using System.Numerics;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class OptimizerPureExpressionScenarioTests
{
    public static IEnumerable<TestDataRow<OptimizerPureExpressionScenario>> Cases
        => OptimizerPureExpressionScenarioCatalog.All.Select(static testCase =>
            new TestDataRow<OptimizerPureExpressionScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndKinds()
    {
        var cases = OptimizerPureExpressionScenarioCatalog.All;

        Assert.IsNotEmpty(cases);
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Id).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(cases.All(static testCase =>
            testCase.Id.StartsWith("optimizer.pure-expression.", StringComparison.Ordinal)));
        Assert.IsTrue(cases.All(static testCase => !string.IsNullOrWhiteSpace(testCase.Dimension)));
        Assert.HasCount(
            Enum.GetValues<OptimizerPureExpressionScenarioKind>().Length,
            cases.Select(static testCase => testCase.Kind).Distinct());
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void OptimizeLogical_MatchesStructuralPurityContract(OptimizerPureExpressionScenario testCase)
    {
        var (left, right) = CreateOperands(testCase.Kind);
        Assert.AreNotSame(left, right, $"{testCase.Id}: scenarios must use distinct AST instances.");
        var input = new LogicalExpression(Operator.LogicalAnd, left, right);

        var actual = Optimizer.OptimizeLogical(input);

        Assert.AreEqual(testCase.ExpectedJavaScript, actual.ToKnRECMAScript(), testCase.Id);
        if (testCase.ShouldDeduplicate)
        {
            if (testCase.ShouldReuseLeftInstance)
                Assert.AreSame(left, actual, testCase.Id);
            else
                Assert.AreNotSame(left, actual, testCase.Id);
        }
        else
        {
            Assert.IsInstanceOfType<LogicalExpression>(actual, testCase.Id);
            Assert.AreEqual(Operator.LogicalAnd, ((LogicalExpression)actual).Operator, testCase.Id);
        }
    }

    private static (Expression Left, Expression Right) CreateOperands(
        OptimizerPureExpressionScenarioKind kind)
        => kind switch
        {
            OptimizerPureExpressionScenarioKind.EqualIdentifiers =>
                (Identifier("value"), Identifier("value")),
            OptimizerPureExpressionScenarioKind.DifferentIdentifiers =>
                (Identifier("left"), Identifier("right")),
            OptimizerPureExpressionScenarioKind.EqualNullLiterals =>
                (new NullLiteral("null"), new NullLiteral("null")),
            OptimizerPureExpressionScenarioKind.EqualBooleanLiterals =>
                (Boolean(true), Boolean(true)),
            OptimizerPureExpressionScenarioKind.DifferentBooleanLiterals =>
                (Boolean(true), Boolean(false)),
            OptimizerPureExpressionScenarioKind.EqualNumericLiterals =>
                (Number(42), Number(42)),
            OptimizerPureExpressionScenarioKind.DifferentNumericLiterals =>
                (Number(1), Number(2)),
            OptimizerPureExpressionScenarioKind.EqualBigIntLiterals =>
                (BigInt(42), BigInt(42)),
            OptimizerPureExpressionScenarioKind.DifferentBigIntLiterals =>
                (BigInt(1), BigInt(2)),
            OptimizerPureExpressionScenarioKind.EqualStringLiterals =>
                (String("ready"), String("ready")),
            OptimizerPureExpressionScenarioKind.DifferentStringLiterals =>
                (String("left"), String("right")),
            OptimizerPureExpressionScenarioKind.EqualThisExpressions =>
                (new ThisExpression(), new ThisExpression()),
            OptimizerPureExpressionScenarioKind.EqualSuperExpressions =>
                (new Super(), new Super()),
            OptimizerPureExpressionScenarioKind.EqualNestedLogicalExpressions =>
                (Logical(Operator.LogicalOr, Identifier("a"), Identifier("b")),
                    Logical(Operator.LogicalOr, Identifier("a"), Identifier("b"))),
            OptimizerPureExpressionScenarioKind.DifferentNestedLogicalOperators =>
                (Logical(Operator.LogicalAnd, Identifier("a"), Identifier("b")),
                    Logical(Operator.LogicalOr, Identifier("a"), Identifier("b"))),
            OptimizerPureExpressionScenarioKind.EqualStrictBinaryExpressions =>
                (Binary(Operator.StrictEquality, Identifier("left"), Identifier("right")),
                    Binary(Operator.StrictEquality, Identifier("left"), Identifier("right"))),
            OptimizerPureExpressionScenarioKind.DifferentStrictBinaryOperators =>
                (Binary(Operator.StrictEquality, Identifier("left"), Identifier("right")),
                    Binary(Operator.StrictInequality, Identifier("left"), Identifier("right"))),
            OptimizerPureExpressionScenarioKind.DifferentStrictBinaryLeftOperands =>
                (Binary(Operator.StrictEquality, Identifier("left"), Identifier("right")),
                    Binary(Operator.StrictEquality, Identifier("other"), Identifier("right"))),
            OptimizerPureExpressionScenarioKind.EqualLooseNullChecks =>
                (Binary(Operator.Equality, Identifier("value"), new NullLiteral("null")),
                    Binary(Operator.Equality, Identifier("value"), new NullLiteral("null"))),
            OptimizerPureExpressionScenarioKind.EqualLooseNonNullChecksPreserved =>
                (Binary(Operator.Equality, Identifier("left"), Identifier("right")),
                    Binary(Operator.Equality, Identifier("left"), Identifier("right"))),
            OptimizerPureExpressionScenarioKind.EqualUnaryExpressions =>
                (Unary(Operator.LogicalNot, Identifier("ready")),
                    Unary(Operator.LogicalNot, Identifier("ready"))),
            OptimizerPureExpressionScenarioKind.DifferentUnaryOperators =>
                (Unary(Operator.LogicalNot, Identifier("ready")),
                    Unary(Operator.TypeOf, Identifier("ready"))),
            OptimizerPureExpressionScenarioKind.EqualConditionalExpressions =>
                (Conditional("ready", "yes", "no"), Conditional("ready", "yes", "no")),
            OptimizerPureExpressionScenarioKind.DifferentConditionalTests =>
                (Conditional("ready", "yes", "no"), Conditional("other", "yes", "no")),
            OptimizerPureExpressionScenarioKind.DifferentConditionalConsequents =>
                (Conditional("ready", "yes", "no"), Conditional("ready", "other", "no")),
            OptimizerPureExpressionScenarioKind.DifferentConditionalAlternates =>
                (Conditional("ready", "yes", "no"), Conditional("ready", "yes", "other")),
            OptimizerPureExpressionScenarioKind.EqualSequenceExpressions =>
                (Sequence("first", "second"), Sequence("first", "second")),
            OptimizerPureExpressionScenarioKind.DifferentSequenceLengths =>
                (Sequence("first", "second"), Sequence("first", "second", "third")),
            OptimizerPureExpressionScenarioKind.DifferentSequenceElements =>
                (Sequence("first", "second"), Sequence("first", "other")),
            OptimizerPureExpressionScenarioKind.EqualArithmeticExpressionsPreserved =>
                (Binary(Operator.Addition, Identifier("left"), Identifier("right")),
                    Binary(Operator.Addition, Identifier("left"), Identifier("right"))),
            OptimizerPureExpressionScenarioKind.ConditionalCallEffectsPreserved =>
                (ConditionalWithCall(), ConditionalWithCall()),
            OptimizerPureExpressionScenarioKind.SequenceCallEffectsPreserved =>
                (SequenceWithCall(), SequenceWithCall()),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

    private static Identifier Identifier(string name)
        => new(name);

    private static BooleanLiteral Boolean(bool value)
        => new(value, value ? "true" : "false");

    private static NumericLiteral Number(int value)
        => new(value, value.ToString(System.Globalization.CultureInfo.InvariantCulture));

    private static BigIntLiteral BigInt(int value)
        => new(new BigInteger(value), value.ToString(System.Globalization.CultureInfo.InvariantCulture) + "n");

    private static StringLiteral String(string value)
        => new(value, System.Text.Json.JsonSerializer.Serialize(value));

    private static LogicalExpression Logical(Operator op, Expression left, Expression right)
        => new(op, left, right);

    private static NonLogicalBinaryExpression Binary(Operator op, Expression left, Expression right)
        => new(op, left, right);

    private static NonUpdateUnaryExpression Unary(Operator op, Expression argument)
        => new(op, argument);

    private static ConditionalExpression Conditional(string test, string consequent, string alternate)
        => new(Identifier(test), Identifier(consequent), Identifier(alternate));

    private static SequenceExpression Sequence(params string[] names)
        => new(NodeList.From(names.Select(Identifier).Cast<Expression>().ToArray()));

    private static ConditionalExpression ConditionalWithCall()
        => new(
            Call("check"),
            Identifier("yes"),
            Identifier("no"));

    private static SequenceExpression SequenceWithCall()
        => new(NodeList.From<Expression>(Identifier("first"), Call("next")));

    private static CallExpression Call(string name)
        => new(Identifier(name), NodeList.From<Expression>(), optional: false);
}

public enum OptimizerPureExpressionScenarioKind
{
    EqualIdentifiers,
    DifferentIdentifiers,
    EqualNullLiterals,
    EqualBooleanLiterals,
    DifferentBooleanLiterals,
    EqualNumericLiterals,
    DifferentNumericLiterals,
    EqualBigIntLiterals,
    DifferentBigIntLiterals,
    EqualStringLiterals,
    DifferentStringLiterals,
    EqualThisExpressions,
    EqualSuperExpressions,
    EqualNestedLogicalExpressions,
    DifferentNestedLogicalOperators,
    EqualStrictBinaryExpressions,
    DifferentStrictBinaryOperators,
    DifferentStrictBinaryLeftOperands,
    EqualLooseNullChecks,
    EqualLooseNonNullChecksPreserved,
    EqualUnaryExpressions,
    DifferentUnaryOperators,
    EqualConditionalExpressions,
    DifferentConditionalTests,
    DifferentConditionalConsequents,
    DifferentConditionalAlternates,
    EqualSequenceExpressions,
    DifferentSequenceLengths,
    DifferentSequenceElements,
    EqualArithmeticExpressionsPreserved,
    ConditionalCallEffectsPreserved,
    SequenceCallEffectsPreserved
}

public sealed record OptimizerPureExpressionScenario(
    string Id,
    string Dimension,
    OptimizerPureExpressionScenarioKind Kind,
    bool ShouldDeduplicate,
    bool ShouldReuseLeftInstance,
    string ExpectedJavaScript);

internal static class OptimizerPureExpressionScenarioCatalog
{
    public static IReadOnlyList<OptimizerPureExpressionScenario> All { get; } =
    [
        Case("identifier.equal", "identifier-name-structural-equality", OptimizerPureExpressionScenarioKind.EqualIdentifiers, true, "value"),
        Case("identifier.different", "identifier-name-inequality", OptimizerPureExpressionScenarioKind.DifferentIdentifiers, false, "left && right"),
        Case("null.equal", "null-literal-structural-equality", OptimizerPureExpressionScenarioKind.EqualNullLiterals, true, "null"),
        Case("boolean.equal", "boolean-value-structural-equality", OptimizerPureExpressionScenarioKind.EqualBooleanLiterals, true, "true"),
        Case("boolean.different", "boolean-value-inequality", OptimizerPureExpressionScenarioKind.DifferentBooleanLiterals, false, "true && false"),
        Case("number.equal", "numeric-value-structural-equality", OptimizerPureExpressionScenarioKind.EqualNumericLiterals, true, "42"),
        Case("number.different", "numeric-value-inequality", OptimizerPureExpressionScenarioKind.DifferentNumericLiterals, false, "1 && 2"),
        Case("bigint.equal", "bigint-value-structural-equality", OptimizerPureExpressionScenarioKind.EqualBigIntLiterals, true, "42n"),
        Case("bigint.different", "bigint-value-inequality", OptimizerPureExpressionScenarioKind.DifferentBigIntLiterals, false, "1n && 2n"),
        Case("string.equal", "string-value-ordinal-equality", OptimizerPureExpressionScenarioKind.EqualStringLiterals, true, "\"ready\""),
        Case("string.different", "string-value-ordinal-inequality", OptimizerPureExpressionScenarioKind.DifferentStringLiterals, false, "\"left\" && \"right\""),
        Case("this.equal", "this-expression-structural-equality", OptimizerPureExpressionScenarioKind.EqualThisExpressions, true, "this"),
        Case("super.equal", "super-expression-structural-equality", OptimizerPureExpressionScenarioKind.EqualSuperExpressions, true, "super"),
        Case("logical.equal", "nested-logical-tree-structural-equality", OptimizerPureExpressionScenarioKind.EqualNestedLogicalExpressions, true, "a || b", shouldReuseLeftInstance: false),
        Case("logical.operator-different", "nested-logical-operator-inequality", OptimizerPureExpressionScenarioKind.DifferentNestedLogicalOperators, false, "a && b && (a || b)"),
        Case("binary.equal", "strict-binary-tree-structural-equality", OptimizerPureExpressionScenarioKind.EqualStrictBinaryExpressions, true, "left === right"),
        Case("binary.operator-different", "strict-binary-operator-inequality", OptimizerPureExpressionScenarioKind.DifferentStrictBinaryOperators, false, "left === right && left !== right"),
        Case("binary.left-different", "strict-binary-left-operand-inequality", OptimizerPureExpressionScenarioKind.DifferentStrictBinaryLeftOperands, false, "left === right && other === right"),
        Case("binary.loose-null-equal", "loose-null-check-structural-equality", OptimizerPureExpressionScenarioKind.EqualLooseNullChecks, true, "value == null"),
        Case("binary.loose-non-null-preserved", "loose-value-coercion-purity-boundary", OptimizerPureExpressionScenarioKind.EqualLooseNonNullChecksPreserved, false, "left == right && left == right"),
        Case("unary.equal", "pure-unary-tree-structural-equality", OptimizerPureExpressionScenarioKind.EqualUnaryExpressions, true, "!ready"),
        Case("unary.operator-different", "pure-unary-operator-inequality", OptimizerPureExpressionScenarioKind.DifferentUnaryOperators, false, "!ready && typeof ready"),
        Case("conditional.equal", "conditional-tree-structural-equality", OptimizerPureExpressionScenarioKind.EqualConditionalExpressions, true, "ready ? yes : no"),
        Case("conditional.test-different", "conditional-test-inequality", OptimizerPureExpressionScenarioKind.DifferentConditionalTests, false, "(ready ? yes : no) && (other ? yes : no)"),
        Case("conditional.consequent-different", "conditional-consequent-inequality", OptimizerPureExpressionScenarioKind.DifferentConditionalConsequents, false, "(ready ? yes : no) && (ready ? other : no)"),
        Case("conditional.alternate-different", "conditional-alternate-inequality", OptimizerPureExpressionScenarioKind.DifferentConditionalAlternates, false, "(ready ? yes : no) && (ready ? yes : other)"),
        Case("sequence.equal", "sequence-element-structural-equality", OptimizerPureExpressionScenarioKind.EqualSequenceExpressions, true, "first, second"),
        Case("sequence.length-different", "sequence-length-inequality", OptimizerPureExpressionScenarioKind.DifferentSequenceLengths, false, "(first, second) && (first, second, third)"),
        Case("sequence.element-different", "sequence-element-inequality", OptimizerPureExpressionScenarioKind.DifferentSequenceElements, false, "(first, second) && (first, other)"),
        Case("arithmetic.equal-preserved", "arithmetic-purity-conservative-boundary", OptimizerPureExpressionScenarioKind.EqualArithmeticExpressionsPreserved, false, "left + right && left + right"),
        Case("conditional.call-preserved", "conditional-side-effect-boundary", OptimizerPureExpressionScenarioKind.ConditionalCallEffectsPreserved, false, "(check() ? yes : no) && (check() ? yes : no)"),
        Case("sequence.call-preserved", "sequence-side-effect-boundary", OptimizerPureExpressionScenarioKind.SequenceCallEffectsPreserved, false, "(first, next()) && (first, next())")
    ];

    private static OptimizerPureExpressionScenario Case(
        string id,
        string dimension,
        OptimizerPureExpressionScenarioKind kind,
        bool shouldDeduplicate,
        string expectedJavaScript,
        bool? shouldReuseLeftInstance = null)
        => new(
            $"optimizer.pure-expression.{id}",
            dimension,
            kind,
            shouldDeduplicate,
            shouldReuseLeftInstance ?? shouldDeduplicate,
            expectedJavaScript);
}
