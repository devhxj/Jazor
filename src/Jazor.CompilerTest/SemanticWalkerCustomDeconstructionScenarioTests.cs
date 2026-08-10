using Acornima;
using Acornima.Ast;
using Jazor.ComplierTest;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class SemanticWalkerCustomDeconstructionScenarioTests
{
    public static IEnumerable<TestDataRow<CustomDeconstructionScenario>> Cases
        => CustomDeconstructionScenarioCatalog.All.Select(static testCase =>
            new TestDataRow<CustomDeconstructionScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndSources()
    {
        var cases = CustomDeconstructionScenarioCatalog.All;

        Assert.IsNotEmpty(cases);
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Dimension).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Source).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(cases.All(static testCase =>
            testCase.Id.StartsWith("semantic.custom-deconstruction.", StringComparison.Ordinal)));
        Assert.IsTrue(cases.All(static testCase =>
            (testCase.ExpectedErrorFragment is null) == (testCase.ExpectedReceiverKinds is not null)));
        Assert.IsTrue(cases.All(static testCase =>
            testCase.ExpectedReceiverKinds is null ||
            testCase.ExpectedReceiverKinds.Count == testCase.ExpectedArgumentCounts!.Count));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void VisitDeconstructionAssignment_UsesRoslynBindingAndPreservesProtocol(
        CustomDeconstructionScenario testCase)
    {
        var (operation, deconstructionInfo) = GetDeconstructionOperation(testCase);
        var boundMethods = EnumerateBoundMethods(deconstructionInfo)
            .Select(static method => $"{method.ContainingType.Name}.{method.Name}")
            .ToArray();
        CollectionAssert.AreEqual(testCase.ExpectedBoundMethods.ToArray(), boundMethods, testCase.Id);

        var walker = new SemanticWalker(true);
        if (testCase.ExpectedErrorFragment is not null)
        {
            var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
                walker.VisitDeconstructionAssignment(operation, new SenseArgument()));
            StringAssert.Contains(exception.Message, testCase.ExpectedErrorFragment, testCase.Id);
            return;
        }

        var node = walker.VisitDeconstructionAssignment(operation, new SenseArgument());
        Assert.IsInstanceOfType<SequenceExpression>(node, testCase.Id);

        var deconstructCalls = DescendantsAndSelf(node!)
            .OfType<CallExpression>()
            .Where(static call => string.Equals(
                GetCallName(call),
                "Deconstruct",
                StringComparison.OrdinalIgnoreCase))
            .ToArray();
        Assert.HasCount(testCase.ExpectedReceiverKinds!.Count, deconstructCalls, testCase.Id);

        var receiverKinds = deconstructCalls
            .Select(static call => GetReceiverKind(call))
            .ToArray();
        CollectionAssert.AreEqual(testCase.ExpectedReceiverKinds.ToArray(), receiverKinds, testCase.Id);

        var argumentCounts = deconstructCalls
            .Select(static call => call.Arguments.Count)
            .ToArray();
        CollectionAssert.AreEqual(testCase.ExpectedArgumentCounts!.ToArray(), argumentCounts, testCase.Id);

        var nonDeconstructCallNames = DescendantsAndSelf(node!)
            .OfType<CallExpression>()
            .Select(static call => GetCallName(call))
            .Where(static name => name is not null && !string.Equals(name, "Deconstruct", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        CollectionAssert.AreEqual(testCase.ExpectedNonDeconstructCallNames.ToArray(), nonDeconstructCallNames, testCase.Id);
        if (testCase.ExpectedNonDeconstructCallNames.Count > 0)
        {
            var sequence = (SequenceExpression)node;
            var firstExpressionCallNames = DescendantsAndSelf(sequence.Expressions[0])
                .OfType<CallExpression>()
                .Select(static call => GetCallName(call))
                .Where(static name => name is not null && !string.Equals(name, "Deconstruct", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            CollectionAssert.AreEqual(testCase.ExpectedNonDeconstructCallNames.ToArray(), firstExpressionCallNames, testCase.Id);
            Assert.IsFalse(sequence.Expressions
                .Skip(1)
                .SelectMany(static expression => DescendantsAndSelf(expression))
                .OfType<CallExpression>()
                .Select(static call => GetCallName(call))
                .Any(name => testCase.ExpectedNonDeconstructCallNames.Contains(name!, StringComparer.Ordinal)), testCase.Id);
        }

        var assignments = DescendantsAndSelf(node!)
            .OfType<AssignmentExpression>()
            .ToArray();
        var localWrites = assignments
            .Select(static assignment => assignment.Left)
            .OfType<Identifier>()
            .Where(static identifier => !identifier.Name.StartsWith("v$", StringComparison.Ordinal))
            .Select(static identifier => identifier.Name)
            .ToArray();
        CollectionAssert.AreEqual(testCase.ExpectedLocalWrites.ToArray(), localWrites, testCase.Id);

        var memberWriteCount = assignments.Count(static assignment => assignment.Left is MemberExpression);
        Assert.AreEqual(testCase.ExpectedMemberWriteCount, memberWriteCount, testCase.Id);

        _ = new Parser().ParseScript(node!.ToKnRECMAScript());
    }

    private static CustomDeconstructReceiverKind GetReceiverKind(CallExpression call)
    {
        Assert.IsInstanceOfType<MemberExpression>(call.Callee);
        return ((MemberExpression)call.Callee).Object switch
        {
            Identifier => CustomDeconstructReceiverKind.Identifier,
            CallExpression => CustomDeconstructReceiverKind.Invocation,
            MemberExpression => CustomDeconstructReceiverKind.Member,
            Node node => throw new AssertFailedException($"Unexpected Deconstruct receiver node '{node.Type}'.")
        };
    }

    private static string? GetCallName(CallExpression call)
        => call.Callee is MemberExpression { Property: Identifier identifier }
            ? identifier.Name
            : null;

    private static IEnumerable<IMethodSymbol> EnumerateBoundMethods(DeconstructionInfo info)
    {
        if (info.Method is not null)
            yield return info.Method;

        foreach (var nested in info.Nested)
        {
            foreach (var method in EnumerateBoundMethods(nested))
                yield return method;
        }
    }

    private static IEnumerable<Node> DescendantsAndSelf(Node node)
    {
        yield return node;
        foreach (var child in node.ChildNodes)
        {
            foreach (var descendant in DescendantsAndSelf(child))
                yield return descendant;
        }
    }

    private static (IDeconstructionAssignmentOperation Operation, DeconstructionInfo Info)
        GetDeconstructionOperation(CustomDeconstructionScenario testCase)
    {
        var parsedTree = CSharpSyntaxTree.ParseText(
            testCase.Source,
            TestMetadataReferences.PreviewParseOptions,
            path: $"{testCase.Id}.cs");
        var syntaxTree = testCase.WrapInSourceBoundary
            ? WrapInSourceBoundary(parsedTree, testCase.Id)
            : parsedTree;
        var compilation = CSharpCompilation.Create(
            assemblyName: "SemanticWalker.CustomDeconstruction.Tests",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(
            0,
            errors,
            $"{testCase.Id}{Environment.NewLine}{string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString()))}");

        var model = compilation.GetSemanticModel(syntaxTree);
        var candidates = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Select(syntax => (Syntax: syntax, Operation: model.GetOperation(syntax)))
            .Where(static candidate => candidate.Operation is IDeconstructionAssignmentOperation)
            .ToArray();
        Assert.HasCount(1, candidates, testCase.Id);

        return (
            (IDeconstructionAssignmentOperation)candidates[0].Operation!,
            model.GetDeconstructionInfo(candidates[0].Syntax));
    }

    private static SyntaxTree WrapInSourceBoundary(SyntaxTree syntaxTree, string scenarioId)
    {
        var root = (CompilationUnitSyntax)syntaxTree.GetRoot();
        var boundary = SyntaxFactory.ClassDeclaration("ScenarioModule")
            .WithMembers(root.Members);
        var wrappedRoot = root.WithMembers(
            SyntaxFactory.SingletonList<MemberDeclarationSyntax>(boundary));
        return CSharpSyntaxTree.Create(
            wrappedRoot,
            TestMetadataReferences.PreviewParseOptions,
            path: $"{scenarioId}.wrapped.cs");
    }
}

public enum CustomDeconstructReceiverKind
{
    Identifier,
    Invocation,
    Member
}

public sealed record CustomDeconstructionScenario(
    string Id,
    string Dimension,
    string Source,
    IReadOnlyList<string> ExpectedBoundMethods,
    IReadOnlyList<CustomDeconstructReceiverKind>? ExpectedReceiverKinds,
    IReadOnlyList<int>? ExpectedArgumentCounts,
    IReadOnlyList<string> ExpectedNonDeconstructCallNames,
    IReadOnlyList<string> ExpectedLocalWrites,
    int ExpectedMemberWriteCount,
    string? ExpectedErrorFragment,
    bool WrapInSourceBoundary);

internal static class CustomDeconstructionScenarioCatalog
{
    private static readonly CustomDeconstructReceiverKind Identifier = CustomDeconstructReceiverKind.Identifier;
    private static readonly CustomDeconstructReceiverKind Invocation = CustomDeconstructReceiverKind.Invocation;
    private static readonly CustomDeconstructReceiverKind Member = CustomDeconstructReceiverKind.Member;

    public static IReadOnlyList<CustomDeconstructionScenario> All { get; } =
    [
        Success(
            "class-parameter-existing-locals",
            "instance-method-binding-with-existing-targets",
            """
            sealed class Point
            {
                public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
            }
            class Demo
            {
                void Run(Point point) { int x, y; (x, y) = point; }
            }
            """,
            ["Point.Deconstruct"], [Identifier], [2], ["x", "y"]),
        Success(
            "class-declaration-targets",
            "instance-method-binding-with-declared-targets",
            """
            sealed class Point
            {
                public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
            }
            class Demo
            {
                void Run(Point point) { var (left, right) = point; }
            }
            """,
            ["Point.Deconstruct"], [Identifier], [2], ["left", "right"]),
        Success(
            "invocation-source",
            "deconstruct-method-is-not-confused-with-source-factory",
            """
            sealed class Point
            {
                public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
            }
            class Demo
            {
                Point Create() => new Point();
                void Run() { int left, right; (left, right) = Create(); }
            }
            """,
            ["Point.Deconstruct"], [Invocation], [2], ["left", "right"], expectedNonDeconstructCallNames: ["Create"]),
        Success(
            "property-source",
            "property-source-is-evaluated-as-custom-receiver",
            """
            sealed class Point
            {
                public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
            }
            class Demo
            {
                Point Current => new Point();
                void Run() { int left, right; (left, right) = Current; }
            }
            """,
            ["Point.Deconstruct"], [Member], [2], ["left", "right"]),
        Success(
            "inherited-instance-method",
            "roslyn-selected-base-method-binding",
            """
            class PointBase
            {
                public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
            }
            sealed class Point : PointBase { }
            class Demo
            {
                void Run(Point point) { int left, right; (left, right) = point; }
            }
            """,
            ["PointBase.Deconstruct"], [Identifier], [2], ["left", "right"]),
        Success(
            "interface-source",
            "interface-contract-method-binding",
            """
            interface IPoint
            {
                void Deconstruct(out int x, out int y);
            }
            class Demo
            {
                void Run(IPoint point) { int left, right; (left, right) = point; }
            }
            """,
            ["IPoint.Deconstruct"], [Identifier], [2], ["left", "right"]),
        Success(
            "generic-interface-constraint",
            "constrained-type-parameter-method-binding",
            """
            interface IPoint
            {
                void Deconstruct(out int x, out int y);
            }
            class Demo
            {
                void Run<T>(T point) where T : IPoint
                {
                    int left, right;
                    (left, right) = point;
                }
            }
            """,
            ["IPoint.Deconstruct"], [Identifier], [2], ["left", "right"]),
        Success(
            "discard-first",
            "first-output-is-evaluated-but-not-written",
            """
            sealed class Point
            {
                public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
            }
            class Demo
            {
                void Run(Point point) { int right; (_, right) = point; }
            }
            """,
            ["Point.Deconstruct"], [Identifier], [2], ["right"]),
        Success(
            "discard-second",
            "second-output-is-evaluated-but-not-written",
            """
            sealed class Point
            {
                public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
            }
            class Demo
            {
                void Run(Point point) { int left; (left, _) = point; }
            }
            """,
            ["Point.Deconstruct"], [Identifier], [2], ["left"]),
        Success(
            "discard-both",
            "all-outputs-are-evaluated-without-target-writes",
            """
            sealed class Point
            {
                public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
            }
            class Demo
            {
                void Run(Point point) { (_, _) = point; }
            }
            """,
            ["Point.Deconstruct"], [Identifier], [2], []),
        Success(
            "instance-field-targets",
            "custom-outputs-write-instance-fields",
            """
            sealed class Point
            {
                public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
            }
            class Demo
            {
                int _left;
                int _right;
                void Run(Point point) { (this._left, this._right) = point; }
            }
            """,
            ["Point.Deconstruct"], [Identifier], [2], [], expectedMemberWriteCount: 2),
        Success(
            "property-targets",
            "custom-outputs-write-property-setters",
            """
            sealed class Point
            {
                public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
            }
            class Demo
            {
                int Left { get; set; }
                int Right { get; set; }
                void Run(Point point) { (Left, Right) = point; }
            }
            """,
            ["Point.Deconstruct"], [Identifier], [2], [], expectedMemberWriteCount: 2),
        Success(
            "indexer-targets",
            "custom-outputs-write-indexer-setters",
            """
            sealed class Point
            {
                public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
            }
            class Demo
            {
                int this[int index] { get => 0; set { } }
                void Run(Point point) { (this[0], this[1]) = point; }
            }
            """,
            ["Point.Deconstruct"], [Identifier], [2], [], expectedMemberWriteCount: 2),
        Success(
            "static-field-targets",
            "custom-outputs-write-same-module-static-fields",
            """
            sealed class Point
            {
                public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
            }
            static class Targets
            {
                public static int Left;
                public static int Right;
            }
            class Demo
            {
                void Run(Point point) { (Targets.Left, Targets.Right) = point; }
            }
            """,
            ["Point.Deconstruct"], [Identifier], [2], [], expectedMemberWriteCount: 2),
        Success(
            "tuple-with-nested-custom",
            "tuple-slot-recurses-through-roslyn-nested-binding",
            """
            sealed class Point
            {
                public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
            }
            class Demo
            {
                void Run((Point Point, int Id) pair)
                {
                    int x, y, id;
                    ((x, y), id) = pair;
                }
            }
            """,
            ["Point.Deconstruct"], [Member], [2], ["x", "y", "id"]),
        Success(
            "record-with-nested-custom",
            "record-member-type-drives-nested-custom-binding",
            """
            sealed class Point
            {
                public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
            }
            sealed record Envelope(Point Point, int Id);
            class Demo
            {
                void Run(Envelope envelope)
                {
                    int x, y, id;
                    ((x, y), id) = envelope;
                }
            }
            """,
            ["Envelope.Deconstruct", "Point.Deconstruct"], [Member], [2], ["x", "y", "id"]),
        Success(
            "record-invocation-source-with-discard",
            "structural-record-source-is-evaluated-once-and-discard-is-not-written",
            """
            sealed record Envelope(int Code, int Id);
            class Demo
            {
                Envelope Create() => new Envelope(1, 2);
                void Run()
                {
                    var (_, id) = Create();
                }
            }
            """,
            ["Envelope.Deconstruct"], [], [], ["id"], expectedNonDeconstructCallNames: ["Create"]),
        Success(
            "custom-with-nested-custom",
            "nested-protocol-calls-precede-source-order-writes",
            """
            sealed class Point
            {
                public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
            }
            sealed class Envelope
            {
                public void Deconstruct(out Point point, out int id)
                {
                    point = new Point();
                    id = 3;
                }
            }
            class Demo
            {
                void Run(Envelope envelope)
                {
                    int x, y, id;
                    ((x, y), id) = envelope;
                }
            }
            """,
            ["Envelope.Deconstruct", "Point.Deconstruct"],
            [Identifier, Identifier], [2, 2], ["x", "y", "id"]),
        Success(
            "custom-with-nested-tuple-output",
            "custom-output-tuple-is-expanded-after-protocol-call",
            """
            sealed class Packet
            {
                public void Deconstruct(out (int Left, int Right) pair, out int id)
                {
                    pair = (1, 2);
                    id = 3;
                }
            }
            class Demo
            {
                void Run(Packet packet)
                {
                    int left, right, id;
                    ((left, right), id) = packet;
                }
            }
            """,
            ["Packet.Deconstruct"], [Identifier], [2], ["left", "right", "id"]),
        Success(
            "mixed-four-output-targets",
            "local-member-and-discard-target-order",
            """
            sealed class Values
            {
                public void Deconstruct(out int a, out int b, out int c, out int d)
                {
                    a = 1; b = 2; c = 3; d = 4;
                }
            }
            class Demo
            {
                int _field;
                int Property { get; set; }
                void Run(Values values)
                {
                    int local;
                    (local, this._field, _, Property) = values;
                }
            }
            """,
            ["Values.Deconstruct"], [Identifier], [4], ["local"], expectedMemberWriteCount: 2),
        Failure(
            "struct-instance",
            "member-struct-runtime-protocol-is-explicitly-rejected",
            """
            readonly struct Point
            {
                public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
            }
            class Demo
            {
                void Run(Point point) { int x, y; (x, y) = point; }
            }
            """,
            ["Point.Deconstruct"],
            "member struct runtime declarations are not emitted"),
        Failure(
            "class-extension",
            "source-extension-runtime-slot-is-explicitly-rejected",
            """
            sealed class Point { }
            static class PointExtensions
            {
                public static void Deconstruct(this Point point, out int x, out int y)
                {
                    x = 1; y = 2;
                }
            }
            class Demo
            {
                void Run(Point point) { int x, y; (x, y) = point; }
            }
            """,
            ["PointExtensions.Deconstruct"],
            "source extension methods do not have a receiver-member runtime slot",
            wrapInSourceBoundary: false),
        Failure(
            "struct-extension",
            "struct-extension-runtime-slot-is-explicitly-rejected",
            """
            readonly struct Point { }
            static class PointExtensions
            {
                public static void Deconstruct(this Point point, out int x, out int y)
                {
                    x = 1; y = 2;
                }
            }
            class Demo
            {
                void Run(Point point) { int x, y; (x, y) = point; }
            }
            """,
            ["PointExtensions.Deconstruct"],
            "source extension methods do not have a receiver-member runtime slot",
            wrapInSourceBoundary: false),
        Failure(
            "tuple-with-nested-struct",
            "nested-member-struct-protocol-is-explicitly-rejected",
            """
            readonly struct Point
            {
                public void Deconstruct(out int x, out int y) { x = 1; y = 2; }
            }
            class Demo
            {
                void Run((Point Point, int Id) pair)
                {
                    int x, y, id;
                    ((x, y), id) = pair;
                }
            }
            """,
            ["Point.Deconstruct"],
            "member struct runtime declarations are not emitted"),
        Failure(
            "record-with-nested-extension",
            "nested-source-extension-runtime-slot-is-explicitly-rejected",
            """
            sealed class Point { }
            static class PointExtensions
            {
                public static void Deconstruct(this Point point, out int x, out int y)
                {
                    x = 1; y = 2;
                }
            }
            sealed record Envelope(Point Point, int Id);
            class Demo
            {
                void Run(Envelope envelope)
                {
                    int x, y, id;
                    ((x, y), id) = envelope;
                }
            }
            """,
            ["Envelope.Deconstruct", "PointExtensions.Deconstruct"],
            "source extension methods do not have a receiver-member runtime slot",
            wrapInSourceBoundary: false)
    ];

    private static CustomDeconstructionScenario Success(
        string id,
        string dimension,
        string source,
        IReadOnlyList<string> expectedBoundMethods,
        IReadOnlyList<CustomDeconstructReceiverKind> expectedReceiverKinds,
        IReadOnlyList<int> expectedArgumentCounts,
        IReadOnlyList<string> expectedLocalWrites,
        int expectedMemberWriteCount = 0,
        IReadOnlyList<string>? expectedNonDeconstructCallNames = null)
        => new(
            $"semantic.custom-deconstruction.{id}",
            dimension,
            source,
            expectedBoundMethods,
            expectedReceiverKinds,
            expectedArgumentCounts,
            expectedNonDeconstructCallNames ?? [],
            expectedLocalWrites,
            expectedMemberWriteCount,
            null,
            true);

    private static CustomDeconstructionScenario Failure(
        string id,
        string dimension,
        string source,
        IReadOnlyList<string> expectedBoundMethods,
        string expectedErrorFragment,
        bool wrapInSourceBoundary = true)
        => new(
            $"semantic.custom-deconstruction.{id}",
            dimension,
            source,
            expectedBoundMethods,
            null,
            null,
            [],
            [],
            0,
            expectedErrorFragment,
            wrapInSourceBoundary);
}
