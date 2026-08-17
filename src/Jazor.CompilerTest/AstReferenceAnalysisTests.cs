using System.Numerics;
using Acornima;
using Acornima.Ast;
using Jazor.Compiler;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class AstReferenceAnalysisTests
{
    public static IEnumerable<TestDataRow<ReferenceEquivalenceCase>> EquivalenceCases
        => AstReferenceScenarioCatalog.EquivalenceCases.Select(static testCase =>
            new TestDataRow<ReferenceEquivalenceCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<IdentifierCollectionCase>> CollectionCases
        => AstReferenceScenarioCatalog.CollectionCases.Select(static testCase =>
            new TestDataRow<IdentifierCollectionCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndInputs()
    {
        var equivalenceCases = AstReferenceScenarioCatalog.EquivalenceCases;
        var collectionCases = AstReferenceScenarioCatalog.CollectionCases;
        var allIds = equivalenceCases.Select(static testCase => testCase.Id)
            .Concat(collectionCases.Select(static testCase => testCase.Id))
            .ToArray();

        Assert.IsNotEmpty(equivalenceCases);
        Assert.IsNotEmpty(collectionCases);
        Assert.HasCount(allIds.Length, allIds.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(allIds.All(static id => id.StartsWith("ast-reference.", StringComparison.Ordinal)));
        Assert.IsTrue(equivalenceCases.All(static testCase => !string.IsNullOrWhiteSpace(testCase.Dimension)));
        Assert.IsTrue(collectionCases.All(static testCase => !string.IsNullOrWhiteSpace(testCase.Dimension)));
        Assert.HasCount(
            equivalenceCases.Count,
            equivalenceCases
                .Select(static testCase => testCase.InputIdentity)
                .Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            collectionCases.Count,
            collectionCases
                .Select(static testCase => testCase.Source)
                .Distinct(StringComparer.Ordinal));
    }

    [TestMethod]
    [DynamicData(nameof(EquivalenceCases))]
    public void AreEquivalentReference_MatchesScenarioContract(ReferenceEquivalenceCase testCase)
    {
        var left = testCase.Left.Create();
        var right = testCase.ReuseLeft ? left : testCase.Right.Create();

        var actual = AstReferenceAnalysis.AreEquivalentReference(left, right);

        Assert.AreEqual(testCase.Expected, actual, testCase.Id);
    }

    [TestMethod]
    public void AreEquivalentReference_NullOperands_ThrowNamedArgumentExceptions()
    {
        var expression = new Identifier("value");

        var leftException = Assert.Throws<ArgumentNullException>(
            () => AstReferenceAnalysis.AreEquivalentReference(null!, expression));
        var rightException = Assert.Throws<ArgumentNullException>(
            () => AstReferenceAnalysis.AreEquivalentReference(expression, null!));
        var bothException = Assert.Throws<ArgumentNullException>(
            () => AstReferenceAnalysis.AreEquivalentReference(null!, null!));

        Assert.AreEqual("left", leftException.ParamName);
        Assert.AreEqual("right", rightException.ParamName);
        Assert.AreEqual("left", bothException.ParamName);
    }

    [TestMethod]
    [DynamicData(nameof(CollectionCases))]
    public void IdentifierCollection_MatchesReferencePositionContract(IdentifierCollectionCase testCase)
    {
        var program = new Parser().ParseScript(testCase.Source);

        var names = AstReferenceAnalysis.CollectIdentifiers([program]);

        CollectionAssert.AreEquivalent(
            testCase.ExpectedNames.ToArray(),
            names.ToArray(),
            $"{testCase.Id}: actual references [{string.Join(", ", names.OrderBy(static name => name, StringComparer.Ordinal))}]");
        foreach (var expectedName in testCase.ExpectedNames)
        {
            Assert.IsTrue(
                AstReferenceAnalysis.ReferencesIdentifier(program, expectedName),
                $"{testCase.Id}: expected reference '{expectedName}'.");
        }
        Assert.IsFalse(
            AstReferenceAnalysis.ReferencesIdentifier(program, "__absent_reference__"),
            testCase.Id);
    }

    [TestMethod]
    public void IdentifierCollection_NullArguments_ThrowNamedArgumentExceptions()
    {
        var nodeException = Assert.Throws<ArgumentNullException>(
            () => AstReferenceAnalysis.ReferencesIdentifier(null!, "value"));
        var nameException = Assert.Throws<ArgumentNullException>(
            () => AstReferenceAnalysis.ReferencesIdentifier(new Identifier("value"), null!));
        var nodesException = Assert.Throws<ArgumentNullException>(
            () => AstReferenceAnalysis.CollectIdentifiers(null!));

        Assert.AreEqual("node", nodeException.ParamName);
        Assert.AreEqual("name", nameException.ParamName);
        Assert.AreEqual("nodes", nodesException.ParamName);
    }

    [TestMethod]
    public void IdentifierCollection_ObjectPatternProperties_OnlyReadComputedKeysAndBindingDefaults()
    {
        var staticProperty = new ObjectProperty(
            PropertyKind.Init,
            new Identifier("label"),
            new AssignmentPattern(new Identifier("value"), new Identifier("fallback")),
            computed: false,
            shorthand: false,
            method: false);
        var computedProperty = new ObjectProperty(
            PropertyKind.Init,
            new Identifier("key"),
            new AssignmentPattern(new Identifier("other"), new Identifier("otherFallback")),
            computed: true,
            shorthand: false,
            method: false);
        var pattern = new ObjectPattern(NodeList.From<Node>(staticProperty, computedProperty));
        var declaration = new VariableDeclaration(
            VariableDeclarationKind.Const,
            NodeList.From(new VariableDeclarator(pattern, new Identifier("source"))));

        var names = AstReferenceAnalysis.CollectIdentifiers([declaration]);

        CollectionAssert.AreEquivalent(
            new[] { "fallback", "key", "otherFallback", "source" },
            names.ToArray());
    }

    [TestMethod]
    public void IdentifierCollection_ClassMembersWithoutSuperclassOrInitializers_DoNotCreateReferences()
    {
        var program = new Parser().ParseScript(
            "class Standalone { method() {} field; } const Expression = class { method() {} field; };");

        var names = AstReferenceAnalysis.CollectIdentifiers([program]);

        Assert.IsEmpty(names);
        Assert.IsFalse(AstReferenceAnalysis.ReferencesIdentifier(program, "Standalone"));
        Assert.IsFalse(AstReferenceAnalysis.ReferencesIdentifier(program, "Expression"));
    }
}

public sealed record ReferenceEquivalenceCase(
    string Id,
    string Dimension,
    ReferenceExpressionSpec Left,
    ReferenceExpressionSpec Right,
    bool ReuseLeft,
    bool Expected)
{
    public string InputIdentity
        => $"{Left}|{Right}|reuse:{ReuseLeft}";
}

public sealed record IdentifierCollectionCase(
    string Id,
    string Dimension,
    string Source,
    IReadOnlyList<string> ExpectedNames);

public abstract record ReferenceExpressionSpec
{
    public sealed record IdentifierReference(string Name) : ReferenceExpressionSpec;

    public sealed record ThisReference : ReferenceExpressionSpec;

    public sealed record SuperReference : ReferenceExpressionSpec;

    public sealed record NullReference : ReferenceExpressionSpec;

    public sealed record BooleanReference(bool Value, string Raw) : ReferenceExpressionSpec;

    public sealed record NumericReference(double Value, string Raw) : ReferenceExpressionSpec;

    public sealed record BigIntReference(BigInteger Value, string Raw) : ReferenceExpressionSpec;

    public sealed record StringReference(string Value, string Raw) : ReferenceExpressionSpec;

    public sealed record MemberReference(
        ReferenceExpressionSpec Receiver,
        ReferenceExpressionSpec Property,
        bool Computed,
        bool Optional) : ReferenceExpressionSpec;

    public sealed record CallReference(string Callee) : ReferenceExpressionSpec;

    public Expression Create()
        => this switch
        {
            IdentifierReference identifier => new Identifier(identifier.Name),
            ThisReference => new ThisExpression(),
            SuperReference => new Super(),
            NullReference => new NullLiteral("null"),
            BooleanReference boolean => new BooleanLiteral(boolean.Value, boolean.Raw),
            NumericReference number => new NumericLiteral(number.Value, number.Raw),
            BigIntReference bigint => new BigIntLiteral(bigint.Value, bigint.Raw),
            StringReference text => new StringLiteral(text.Value, text.Raw),
            MemberReference member => new MemberExpression(
                member.Receiver.Create(),
                member.Property.Create(),
                member.Computed,
                member.Optional),
            CallReference call => new CallExpression(
                new Identifier(call.Callee),
                NodeList.From<Expression>(),
                optional: false),
            _ => throw new InvalidOperationException($"Unknown reference expression spec '{GetType().Name}'.")
        };
}

internal static class AstReferenceScenarioCatalog
{
    private static readonly ReferenceExpressionSpec.IdentifierReference State = new("state");
    private static readonly ReferenceExpressionSpec.IdentifierReference Value = new("value");
    private static readonly ReferenceExpressionSpec.IdentifierReference Other = new("other");

    public static IReadOnlyList<ReferenceEquivalenceCase> EquivalenceCases { get; } =
    [
        Case("ast-reference.same-instance.call", "identity-fast-path", Call("read"), Call("ignored"), true, true),
        Case("ast-reference.identifier.equal", "identifier-name", State, Identifier("state"), false, true),
        Case("ast-reference.identifier.ordinal-case", "identifier-name", State, Identifier("State"), false, false),
        Case("ast-reference.this.equal", "receiver-keyword", new ReferenceExpressionSpec.ThisReference(), new ReferenceExpressionSpec.ThisReference(), false, true),
        Case("ast-reference.super.equal", "receiver-keyword", new ReferenceExpressionSpec.SuperReference(), new ReferenceExpressionSpec.SuperReference(), false, true),
        Case("ast-reference.null.equal", "literal-value", new ReferenceExpressionSpec.NullReference(), new ReferenceExpressionSpec.NullReference(), false, true),
        Case("ast-reference.boolean.equal-raw-independent", "literal-value", Boolean(true, "true"), Boolean(true, "TRUE"), false, true),
        Case("ast-reference.boolean.different", "literal-value", Boolean(true, "true"), Boolean(false, "false"), false, false),
        Case("ast-reference.numeric.equal-raw-independent", "literal-value", Number(1, "1"), Number(1, "1.0"), false, true),
        Case("ast-reference.numeric.different", "literal-value", Number(1, "1"), Number(2, "2"), false, false),
        Case("ast-reference.bigint.equal-raw-independent", "literal-value", BigInt(42, "42n"), BigInt(42, "0x2an"), false, true),
        Case("ast-reference.bigint.different", "literal-value", BigInt(42, "42n"), BigInt(43, "43n"), false, false),
        Case("ast-reference.string.equal-raw-independent", "literal-value", Text("name", "\"name\""), Text("name", "'name'"), false, true),
        Case("ast-reference.string.ordinal-case", "literal-value", Text("name", "\"name\""), Text("Name", "\"Name\""), false, false),
        Case("ast-reference.type-mismatch", "node-kind", State, Text("state", "\"state\""), false, false),
        Case("ast-reference.unsupported-call.same-shape", "unsupported-reference-shape", Call("read"), Call("read"), false, false),
        Case("ast-reference.member.equal", "member-recursion", Member(State, Value), Member(Identifier("state"), Identifier("value")), false, true),
        Case("ast-reference.member.receiver-different", "member-recursion", Member(State, Value), Member(Other, Value), false, false),
        Case("ast-reference.member.property-different", "member-recursion", Member(State, Value), Member(State, Other), false, false),
        Case("ast-reference.member.computed-different", "member-flags", Member(State, Value), Member(State, Value, computed: true), false, false),
        Case("ast-reference.member.optional-different", "member-flags", Member(State, Value), Member(State, Value, optional: true), false, false),
        Case(
            "ast-reference.member.nested-equal",
            "nested-member-recursion",
            Member(Member(State, new ReferenceExpressionSpec.IdentifierReference("child")), Value, computed: true),
            Member(Member(Identifier("state"), Identifier("child")), Identifier("value"), computed: true),
            false,
            true)
    ];

    public static IReadOnlyList<IdentifierCollectionCase> CollectionCases { get; } =
    [
        Collect("ast-reference.collect.identifier", "direct-reference", "source;", "source"),
        Collect("ast-reference.collect.member-static", "noncomputed-member-name", "source.header;", "source"),
        Collect("ast-reference.collect.member-computed", "computed-member-key", "source[key];", "source", "key"),
        Collect("ast-reference.collect.object-static", "noncomputed-object-key", "({ header: value });", "value"),
        Collect("ast-reference.collect.object-computed", "computed-object-key", "({ [key]: value });", "key", "value"),
        Collect("ast-reference.collect.object-shorthand", "shorthand-object-value", "({ value });", "value"),
        Collect("ast-reference.collect.variable-initializer", "variable-binding", "const local = source;", "source"),
        Collect("ast-reference.collect.variable-without-initializer", "variable-binding", "let local;"),
        Collect(
            "ast-reference.collect.variable-destructuring-default",
            "binding-side-effects",
            "const { [key]: value = fallback, ...rest } = source;",
            "key",
            "fallback",
            "source"),
        Collect(
            "ast-reference.collect.function-declaration",
            "function-bindings",
            "function render(unused) { return source; }",
            "source"),
        Collect(
            "ast-reference.collect.function-default",
            "parameter-default-reference",
            "function render(input = fallback) { return source(input); }",
            "fallback",
            "source",
            "input"),
        Collect(
            "ast-reference.collect.function-destructuring",
            "destructured-parameter-side-effects",
            "function render({ [key]: value = fallback, ...rest }) { return source; }",
            "key",
            "fallback",
            "source"),
        Collect(
            "ast-reference.collect.function-expression",
            "function-expression-bindings",
            "const render = function named(unused = fallback) { return source; };",
            "fallback",
            "source"),
        Collect(
            "ast-reference.collect.arrow-expression",
            "arrow-function-bindings",
            "const render = ([unused = fallback, ...rest]) => source;",
            "fallback",
            "source"),
        Collect(
            "ast-reference.collect.catch-binding",
            "catch-binding",
            "try { work(); } catch ({ message, ...rest }) { report(message); }",
            "work",
            "report",
            "message"),
        Collect(
            "ast-reference.collect.catch-unused-binding",
            "catch-binding",
            "try { work(); } catch (unused) { report(); }",
            "work",
            "report"),
        Collect(
            "ast-reference.collect.class-declaration",
            "class-and-member-bindings",
            "class Widget extends Base { render(unused) { return source; } [methodName](unused = fallback) { return other; } field = initial; [fieldName] = computedInitial; }",
            "Base",
            "source",
            "methodName",
            "fallback",
            "other",
            "initial",
            "fieldName",
            "computedInitial"),
        Collect(
            "ast-reference.collect.class-expression",
            "class-expression-binding",
            "const ctor = class Internal extends Base { method() { return source; } };",
            "Base",
            "source"),
        Collect(
            "ast-reference.collect.labels",
            "control-flow-labels",
            "outer: while (condition) { if (stop) break outer; continue outer; }",
            "condition",
            "stop")
    ];

    private static ReferenceEquivalenceCase Case(
        string id,
        string dimension,
        ReferenceExpressionSpec left,
        ReferenceExpressionSpec right,
        bool reuseLeft,
        bool expected)
        => new(id, dimension, left, right, reuseLeft, expected);

    private static IdentifierCollectionCase Collect(
        string id,
        string dimension,
        string source,
        params string[] expectedNames)
        => new(id, dimension, source, expectedNames);

    private static ReferenceExpressionSpec.BooleanReference Boolean(bool value, string raw)
        => new(value, raw);

    private static ReferenceExpressionSpec.IdentifierReference Identifier(string name)
        => new(name);

    private static ReferenceExpressionSpec.NumericReference Number(double value, string raw)
        => new(value, raw);

    private static ReferenceExpressionSpec.BigIntReference BigInt(long value, string raw)
        => new(new BigInteger(value), raw);

    private static ReferenceExpressionSpec.StringReference Text(string value, string raw)
        => new(value, raw);

    private static ReferenceExpressionSpec.CallReference Call(string callee)
        => new(callee);

    private static ReferenceExpressionSpec.MemberReference Member(
        ReferenceExpressionSpec receiver,
        ReferenceExpressionSpec property,
        bool computed = false,
        bool optional = false)
        => new(receiver, property, computed, optional);
}
