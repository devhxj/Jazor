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
public sealed class SemanticWalkerUsingLifetimeScenarioTests
{
    public static IEnumerable<TestDataRow<UsingLifetimeScenario>> Cases
        => UsingLifetimeScenarioCatalog.All.Select(static testCase =>
            new TestDataRow<UsingLifetimeScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndSources()
    {
        var cases = UsingLifetimeScenarioCatalog.All;

        Assert.IsNotEmpty(cases);
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Source).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(cases.All(static testCase =>
            testCase.Id.StartsWith("semantic.using-lifetime.", StringComparison.Ordinal)));
        Assert.IsTrue(cases.All(static testCase => !string.IsNullOrWhiteSpace(testCase.Dimension)));
        Assert.IsTrue(cases.All(static testCase =>
            (testCase.ExpectedFinalizers is null) != (testCase.ExpectedErrorFragment is null)));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void VisitUsing_PreservesResourceLifetimeContract(UsingLifetimeScenario testCase)
    {
        var operation = GetTranslationOperation(testCase);
        var walker = new SemanticWalker(true);

        if (testCase.ExpectedErrorFragment is not null)
        {
            var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
                walker.Visit(operation, new SenseArgument()));
            StringAssert.Contains(exception.Message, testCase.ExpectedErrorFragment, testCase.Id);
            return;
        }

        var node = walker.Visit(operation, new SenseArgument());
        Assert.IsNotNull(node, testCase.Id);

        var allNodes = DescendantsAndSelf(node).ToArray();
        var tryStatements = allNodes.OfType<TryStatement>().ToArray();
        Assert.HasCount(testCase.ExpectedFinalizers!.Count, tryStatements, testCase.Id);

        for (var index = 0; index < tryStatements.Length; index++)
            AssertFinalizer(tryStatements[index], testCase.ExpectedFinalizers[index], testCase.Id);

        foreach (var callName in testCase.ExpectedSingleEvaluationCalls)
        {
            Assert.HasCount(
                1,
                allNodes.OfType<CallExpression>().Where(call => GetCallName(call) == callName),
                $"{testCase.Id}: '{callName}' must be evaluated exactly once.");
        }

        Assert.AreEqual(
            testCase.ExpectedReturn,
            allNodes.OfType<ReturnStatement>().Any(),
            testCase.Id);
        Assert.AreEqual(
            testCase.ExpectedThrow,
            allNodes.OfType<ThrowStatement>().Any(),
            testCase.Id);

        AssertCallPlacement(node, tryStatements, testCase);

        if (!testCase.ExpectedReturn)
            _ = new Parser().ParseModule(node.ToKnRECMAScript());
    }

    private static void AssertFinalizer(
        TryStatement tryStatement,
        UsingFinalizerSpec expected,
        string scenarioId)
    {
        Assert.IsNull(tryStatement.Handler, scenarioId);
        Assert.IsNotNull(tryStatement.Finalizer, scenarioId);
        Assert.HasCount(1, tryStatement.Finalizer.Body, scenarioId);
        Assert.IsInstanceOfType<IfStatement>(tryStatement.Finalizer.Body[0], scenarioId);
        var conditional = (IfStatement)tryStatement.Finalizer.Body[0];

        Assert.IsNull(conditional.Alternate, scenarioId);
        Assert.IsInstanceOfType<NonLogicalBinaryExpression>(conditional.Test, scenarioId);
        var nullCheck = (NonLogicalBinaryExpression)conditional.Test;
        Assert.AreEqual(Operator.StrictInequality, nullCheck.Operator, scenarioId);
        Assert.IsInstanceOfType<NullLiteral>(nullCheck.Right, scenarioId);
        var checkedReceiver = AssertReceiver(nullCheck.Left, expected, scenarioId);

        Assert.IsInstanceOfType<NonSpecialExpressionStatement>(conditional.Consequent, scenarioId);
        Expression finalizerExpression = ((NonSpecialExpressionStatement)conditional.Consequent).Expression;
        if (expected.Awaited)
        {
            Assert.IsInstanceOfType<AwaitExpression>(finalizerExpression, scenarioId);
            finalizerExpression = ((AwaitExpression)finalizerExpression).Argument;
        }
        else
        {
            Assert.IsNotInstanceOfType<AwaitExpression>(finalizerExpression, scenarioId);
        }

        Assert.IsInstanceOfType<CallExpression>(finalizerExpression, scenarioId);
        var call = (CallExpression)finalizerExpression;
        Expression calledReceiver;
        switch (expected.Dispatch)
        {
            case UsingDisposeDispatch.Member:
                Assert.IsInstanceOfType<MemberExpression>(call.Callee, scenarioId);
                var member = (MemberExpression)call.Callee;
                Assert.IsFalse(member.Computed, scenarioId);
                Assert.IsInstanceOfType<Identifier>(member.Property, scenarioId);
                Assert.AreEqual(expected.CalleeName, ((Identifier)member.Property).Name, scenarioId);
                Assert.HasCount(0, call.Arguments, scenarioId);
                calledReceiver = member.Object;
                break;
            case UsingDisposeDispatch.Import:
                Assert.IsInstanceOfType<Identifier>(call.Callee, scenarioId);
                Assert.AreEqual(expected.CalleeName, ((Identifier)call.Callee).Name, scenarioId);
                Assert.HasCount(1, call.Arguments, scenarioId);
                Assert.IsInstanceOfType<Expression>(call.Arguments[0], scenarioId);
                calledReceiver = (Expression)call.Arguments[0];
                break;
            default:
                Assert.Fail($"{scenarioId}: unsupported dispatch '{expected.Dispatch}'.");
                return;
        }

        var calledReceiverIdentity = AssertReceiver(calledReceiver, expected, scenarioId);
        Assert.AreEqual(checkedReceiver, calledReceiverIdentity, scenarioId);
    }

    private static string AssertReceiver(Expression receiver, UsingFinalizerSpec expected, string scenarioId)
    {
        switch (expected.ReceiverKind)
        {
            case UsingReceiverKind.Named:
                Assert.IsInstanceOfType<Identifier>(receiver, scenarioId);
                Assert.AreEqual(expected.ReceiverName, ((Identifier)receiver).Name, scenarioId);
                return ((Identifier)receiver).Name;
            case UsingReceiverKind.This:
                Assert.IsInstanceOfType<ThisExpression>(receiver, scenarioId);
                return "this";
            case UsingReceiverKind.Temporary:
                Assert.IsInstanceOfType<Identifier>(receiver, scenarioId);
                var name = ((Identifier)receiver).Name;
                StringAssert.StartsWith(name, "v$", scenarioId);
                return name;
            default:
                throw new AssertFailedException(
                    $"{scenarioId}: unsupported receiver kind '{expected.ReceiverKind}'.");
        }
    }

    private static void AssertCallPlacement(
        Node node,
        IReadOnlyList<TryStatement> tryStatements,
        UsingLifetimeScenario testCase)
    {
        if (testCase.ExpectedLeadingCall is not null)
        {
            Assert.IsInstanceOfType<NestedBlockStatement>(node, testCase.Id);
            var block = (NestedBlockStatement)node;
            Assert.IsNotEmpty(block.Body, testCase.Id);
            Assert.AreEqual(testCase.ExpectedLeadingCall, GetStatementCallName(block.Body[0]), testCase.Id);
        }

        if (testCase.ExpectedProtectedCall is not null)
        {
            Assert.IsNotEmpty(tryStatements, testCase.Id);
            var protectedCalls = DescendantsAndSelf(tryStatements[0].Block)
                .OfType<CallExpression>()
                .Select(GetCallName);
            CollectionAssert.Contains(protectedCalls.ToArray(), testCase.ExpectedProtectedCall, testCase.Id);
        }
    }

    private static string? GetStatementCallName(Statement statement)
    {
        if (statement is not NonSpecialExpressionStatement expressionStatement ||
            expressionStatement.Expression is not CallExpression call)
        {
            return null;
        }

        return GetCallName(call);
    }

    private static string? GetCallName(CallExpression call)
        => call.Callee switch
        {
            Identifier identifier => identifier.Name,
            MemberExpression { Property: Identifier identifier } => identifier.Name,
            _ => null
        };

    private static IEnumerable<Node> DescendantsAndSelf(Node node)
    {
        yield return node;
        foreach (var child in node.ChildNodes)
        {
            foreach (var descendant in DescendantsAndSelf(child))
                yield return descendant;
        }
    }

    private static IOperation GetTranslationOperation(UsingLifetimeScenario testCase)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            testCase.Source,
            TestMetadataReferences.PreviewParseOptions,
            path: $"{testCase.Id}.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "SemanticWalker.UsingLifetime.Tests",
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

        var method = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "TestMethod");
        var block = compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!);
        Assert.IsInstanceOfType<IBlockOperation>(block, testCase.Id);

        if (testCase.Target == UsingTranslationTarget.DeclarationVisitor)
        {
            var usingDeclaration = ((IBlockOperation)block!).Operations
                .OfType<IUsingDeclarationOperation>()
                .Single();
            return usingDeclaration;
        }

        return block!;
    }
}

public enum UsingTranslationTarget
{
    MethodBody,
    DeclarationVisitor
}

public enum UsingDisposeDispatch
{
    Member,
    Import
}

public enum UsingReceiverKind
{
    Named,
    This,
    Temporary
}

public sealed record UsingFinalizerSpec(
    UsingReceiverKind ReceiverKind,
    string? ReceiverName,
    UsingDisposeDispatch Dispatch,
    string CalleeName,
    bool Awaited);

public sealed record UsingLifetimeScenario(
    string Id,
    string Dimension,
    string Source,
    UsingTranslationTarget Target,
    IReadOnlyList<UsingFinalizerSpec>? ExpectedFinalizers,
    IReadOnlyList<string> ExpectedSingleEvaluationCalls,
    bool ExpectedReturn,
    bool ExpectedThrow,
    string? ExpectedLeadingCall,
    string? ExpectedProtectedCall,
    string? ExpectedErrorFragment);

internal static class UsingLifetimeScenarioCatalog
{
    private const string DisposeImport = "_6f97d94b6f2e4bc1";
    private const string DisposeAsyncImport = "_d17f7fbf9eb14eef";

    public static IReadOnlyList<UsingLifetimeScenario> All { get; } =
    [
        Success(
            "statement-multiple-sync",
            "statement-declarators-nest-for-reverse-disposal",
            """
            class TestClass
            {
                class Resource : System.IDisposable { public void Dispose() { } }

                void TestMethod()
                {
                    using (Resource first = new Resource(), second = new Resource()) { }
                }
            }
            """,
            [Member("first", "dispose"), Member("second", "dispose")]),
        Success(
            "statement-multiple-async",
            "async-statement-declarators-nest-for-reverse-disposal",
            """
            class TestClass
            {
                class Resource : System.IAsyncDisposable
                {
                    public System.Threading.Tasks.ValueTask DisposeAsync() => default;
                }

                async System.Threading.Tasks.Task TestMethod()
                {
                    await using (Resource first = new Resource(), second = new Resource()) { }
                }
            }
            """,
            [Member("first", "disposeAsync", awaited: true), Member("second", "disposeAsync", awaited: true)]),
        Success(
            "ref-struct-pattern-sync",
            "ref-struct-pattern-dispose",
            """
            class TestClass
            {
                ref struct Resource { public void Dispose() { } }

                void TestMethod()
                {
                    using (var resource = new Resource()) { }
                }
            }
            """,
            [Member("resource", "dispose")]),
        Success(
            "ref-struct-pattern-async",
            "ref-struct-pattern-dispose-async",
            """
            class TestClass
            {
                ref struct Resource
                {
                    public System.Threading.Tasks.ValueTask DisposeAsync() => default;
                }

                async System.Threading.Tasks.Task TestMethod()
                {
                    await using (var resource = new Resource()) { }
                }
            }
            """,
            [Member("resource", "disposeAsync", awaited: true)]),
        Success(
            "inherited-interface-sync",
            "inherited-idisposable-implementation",
            """
            class TestClass
            {
                class ResourceBase : System.IDisposable { public void Dispose() { } }
                class Resource : ResourceBase { }

                void TestMethod()
                {
                    using (var resource = new Resource()) { }
                }
            }
            """,
            [Member("resource", "dispose")]),
        Success(
            "inherited-interface-async",
            "inherited-iasyncdisposable-implementation",
            """
            class TestClass
            {
                class ResourceBase : System.IAsyncDisposable
                {
                    public System.Threading.Tasks.ValueTask DisposeAsync() => default;
                }
                class Resource : ResourceBase { }

                async System.Threading.Tasks.Task TestMethod()
                {
                    await using (var resource = new Resource()) { }
                }
            }
            """,
            [Member("resource", "disposeAsync", awaited: true)]),
        Success(
            "this-expression-sync",
            "this-resource-reused-without-temp",
            """
            class TestClass : System.IDisposable
            {
                public void Dispose() { }

                void TestMethod()
                {
                    using (this) { }
                }
            }
            """,
            [ThisMember("dispose")]),
        Success(
            "this-expression-async",
            "this-async-resource-reused-without-temp",
            """
            class TestClass : System.IAsyncDisposable
            {
                public System.Threading.Tasks.ValueTask DisposeAsync() => default;

                async System.Threading.Tasks.Task TestMethod()
                {
                    await using (this) { }
                }
            }
            """,
            [ThisMember("disposeAsync", awaited: true)]),
        Success(
            "non-block-body",
            "embedded-statement-body-is-protected",
            """
            class TestClass
            {
                class Resource : System.IDisposable { public void Dispose() { } }
                void Touch() { }

                void TestMethod(Resource resource)
                {
                    using (resource)
                        Touch();
                }
            }
            """,
            [Member("resource", "dispose")],
            expectedProtectedCall: "touch"),
        Success(
            "declaration-prefix-and-tail",
            "using-declaration-wraps-only-following-statements",
            """
            class TestClass
            {
                class Resource : System.IDisposable { public void Dispose() { } }
                void Before() { }
                void After() { }

                void TestMethod()
                {
                    Before();
                    using var resource = new Resource();
                    After();
                }
            }
            """,
            [Member("resource", "dispose")],
            expectedLeadingCall: "before",
            expectedProtectedCall: "after"),
        Success(
            "consecutive-declarations",
            "consecutive-declarations-nest-lifetimes",
            """
            class TestClass
            {
                class Resource : System.IDisposable { public void Dispose() { } }

                void TestMethod()
                {
                    using var first = new Resource();
                    using var second = new Resource();
                }
            }
            """,
            [Member("first", "dispose"), Member("second", "dispose")]),
        Success(
            "declaration-return",
            "return-remains-inside-protected-body",
            """
            class TestClass
            {
                class Resource : System.IDisposable { public void Dispose() { } }

                void TestMethod()
                {
                    using var resource = new Resource();
                    return;
                }
            }
            """,
            [Member("resource", "dispose")],
            expectedReturn: true),
        Success(
            "declaration-throw",
            "throw-remains-inside-protected-body",
            """
            class TestClass
            {
                class Resource : System.IDisposable { public void Dispose() { } }

                void TestMethod()
                {
                    using var resource = new Resource();
                    throw null;
                }
            }
            """,
            [Member("resource", "dispose")],
            expectedThrow: true),
        Success(
            "conditional-expression-once",
            "conditional-resource-is-materialized-once",
            """
            class TestClass
            {
                class Resource : System.IDisposable { public void Dispose() { } }
                Resource CreateFirst() => new Resource();
                Resource CreateSecond() => new Resource();

                void TestMethod(bool chooseFirst)
                {
                    using (chooseFirst ? CreateFirst() : CreateSecond()) { }
                }
            }
            """,
            [TemporaryMember("dispose")],
            expectedSingleEvaluationCalls: ["createFirst", "createSecond"]),
        Success(
            "coalesce-expression-once",
            "coalesced-resource-is-materialized-once",
            """
            class TestClass
            {
                class Resource : System.IDisposable { public void Dispose() { } }
                Resource CreateFallback() => new Resource();

                void TestMethod(Resource candidate)
                {
                    using (candidate ?? CreateFallback()) { }
                }
            }
            """,
            [TemporaryMember("dispose")],
            expectedSingleEvaluationCalls: ["createFallback"]),
        Success(
            "interface-declarator-sync",
            "interface-typed-declarator-uses-runtime-helper",
            """
            class TestClass
            {
                void TestMethod(System.IDisposable candidate)
                {
                    using (System.IDisposable resource = candidate) { }
                }
            }
            """,
            [Import("resource", DisposeImport)]),
        Success(
            "interface-declarator-async",
            "async-interface-typed-declarator-uses-runtime-helper",
            """
            class TestClass
            {
                async System.Threading.Tasks.Task TestMethod(System.IAsyncDisposable candidate)
                {
                    await using (System.IAsyncDisposable resource = candidate) { }
                }
            }
            """,
            [Import("resource", DisposeAsyncImport, awaited: true)]),
        Success(
            "generic-interface-statement-sync",
            "generic-idisposable-constraint-statement",
            """
            class TestClass
            {
                void TestMethod<T>(T resource) where T : System.IDisposable
                {
                    using (resource) { }
                }
            }
            """,
            [Import("resource", DisposeImport)]),
        Success(
            "generic-transitive-constraint-sync",
            "generic-transitive-idisposable-constraint",
            """
            class TestClass
            {
                void TestMethod<T, U>(T resource)
                    where T : U
                    where U : System.IDisposable
                {
                    using (resource) { }
                }
            }
            """,
            [Import("resource", DisposeImport)]),
        Success(
            "generic-struct-declaration-sync",
            "generic-struct-idisposable-constraint-declaration",
            """
            class TestClass
            {
                void TestMethod<T>(T candidate) where T : struct, System.IDisposable
                {
                    using T resource = candidate;
                }
            }
            """,
            [Import("resource", DisposeImport)]),
        Success(
            "generic-derived-interface-sync",
            "generic-derived-idisposable-interface-constraint",
            """
            class TestClass
            {
                interface IResource : System.IDisposable { }

                void TestMethod<T>(T resource) where T : IResource
                {
                    using (resource) { }
                }
            }
            """,
            [Import("resource", DisposeImport)]),
        Success(
            "generic-class-constraint-sync",
            "generic-class-with-idisposable-contract-constraint",
            """
            class TestClass
            {
                class ResourceBase : System.IDisposable
                {
                    public void Dispose() { }
                }

                void TestMethod<T>(T resource) where T : ResourceBase
                {
                    using (resource) { }
                }
            }
            """,
            [Member("resource", "dispose")]),
        Success(
            "generic-interface-statement-async",
            "generic-iasyncdisposable-constraint-statement",
            """
            class TestClass
            {
                async System.Threading.Tasks.Task TestMethod<T>(T resource)
                    where T : System.IAsyncDisposable
                {
                    await using (resource) { }
                }
            }
            """,
            [Import("resource", DisposeAsyncImport, awaited: true)]),
        Success(
            "generic-derived-interface-declaration-async",
            "generic-derived-iasyncdisposable-interface-declaration",
            """
            class TestClass
            {
                interface IAsyncResource : System.IAsyncDisposable { }

                async System.Threading.Tasks.Task TestMethod<T>(T candidate)
                    where T : IAsyncResource
                {
                    await using T resource = candidate;
                }
            }
            """,
            [Import("resource", DisposeAsyncImport, awaited: true)]),
        Success(
            "dispose-overload-filter",
            "zero-parameter-dispose-overload-is-selected",
            """
            class TestClass
            {
                class Resource : System.IDisposable
                {
                    public void Dispose(int mode) { }
                    public void Dispose() { }
                }

                void TestMethod()
                {
                    using (var resource = new Resource()) { }
                }
            }
            """,
            [Member("resource", "dispose_ab2ea3c0f2bf66a0")]),
        Success(
            "nested-sync-and-async",
            "nested-mixed-disposal-kinds",
            """
            class TestClass
            {
                class SyncResource : System.IDisposable { public void Dispose() { } }
                class AsyncResource : System.IAsyncDisposable
                {
                    public System.Threading.Tasks.ValueTask DisposeAsync() => default;
                }

                async System.Threading.Tasks.Task TestMethod(SyncResource sync, AsyncResource asyncResource)
                {
                    using (sync)
                    await using (asyncResource) { }
                }
            }
            """,
            [Member("sync", "dispose"), Member("asyncResource", "disposeAsync", awaited: true)]),
        Failure(
            "nullable-struct",
            "nullable-struct-runtime-shape-is-rejected",
            """
            class TestClass
            {
                struct Resource : System.IDisposable { public void Dispose() { } }

                void TestMethod(Resource? candidate)
                {
                    using (candidate) { }
                }
            }
            """,
            "System.Nullable<T>"),
        Failure(
            "explicit-interface-sync",
            "explicit-dispose-without-runtime-slot-is-rejected",
            """
            class TestClass
            {
                class Resource : System.IDisposable
                {
                    void System.IDisposable.Dispose() { }
                }

                void TestMethod()
                {
                    using var resource = new Resource();
                }
            }
            """,
            "explicit interface implementation"),
        Failure(
            "explicit-interface-async",
            "explicit-dispose-async-without-runtime-slot-is-rejected",
            """
            class TestClass
            {
                class Resource : System.IAsyncDisposable
                {
                    System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync() => default;
                }

                async System.Threading.Tasks.Task TestMethod()
                {
                    await using var resource = new Resource();
                }
            }
            """,
            "explicit interface implementation"),
        Failure(
            "generic-class-explicit-interface-sync",
            "generic-class-constraint-preserves-explicit-dispose-rejection",
            """
            class TestClass
            {
                class ResourceBase : System.IDisposable
                {
                    void System.IDisposable.Dispose() { }
                }

                void TestMethod<T>(T resource) where T : ResourceBase
                {
                    using (resource) { }
                }
            }
            """,
            "explicit interface implementation"),
        Failure(
            "direct-declaration-visitor",
            "declaration-requires-sequential-statement-lowering",
            """
            class TestClass
            {
                class Resource : System.IDisposable { public void Dispose() { } }

                void TestMethod()
                {
                    using var resource = new Resource();
                }
            }
            """,
            "must be lowered by the enclosing sequential statement translator",
            target: UsingTranslationTarget.DeclarationVisitor)
    ];

    private static UsingLifetimeScenario Success(
        string id,
        string dimension,
        string source,
        IReadOnlyList<UsingFinalizerSpec> expectedFinalizers,
        IReadOnlyList<string>? expectedSingleEvaluationCalls = null,
        bool expectedReturn = false,
        bool expectedThrow = false,
        string? expectedLeadingCall = null,
        string? expectedProtectedCall = null)
        => new(
            $"semantic.using-lifetime.{id}",
            dimension,
            source,
            UsingTranslationTarget.MethodBody,
            expectedFinalizers,
            expectedSingleEvaluationCalls ?? [],
            expectedReturn,
            expectedThrow,
            expectedLeadingCall,
            expectedProtectedCall,
            null);

    private static UsingLifetimeScenario Failure(
        string id,
        string dimension,
        string source,
        string expectedErrorFragment,
        UsingTranslationTarget target = UsingTranslationTarget.MethodBody)
        => new(
            $"semantic.using-lifetime.{id}",
            dimension,
            source,
            target,
            null,
            [],
            false,
            false,
            null,
            null,
            expectedErrorFragment);

    private static UsingFinalizerSpec Member(string receiverName, string calleeName, bool awaited = false)
        => new(UsingReceiverKind.Named, receiverName, UsingDisposeDispatch.Member, calleeName, awaited);

    private static UsingFinalizerSpec ThisMember(string calleeName, bool awaited = false)
        => new(UsingReceiverKind.This, null, UsingDisposeDispatch.Member, calleeName, awaited);

    private static UsingFinalizerSpec TemporaryMember(string calleeName, bool awaited = false)
        => new(UsingReceiverKind.Temporary, null, UsingDisposeDispatch.Member, calleeName, awaited);

    private static UsingFinalizerSpec Import(string receiverName, string calleeName, bool awaited = false)
        => new(UsingReceiverKind.Named, receiverName, UsingDisposeDispatch.Import, calleeName, awaited);
}
