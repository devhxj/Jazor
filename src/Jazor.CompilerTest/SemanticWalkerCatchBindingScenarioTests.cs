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
public sealed class SemanticWalkerCatchBindingScenarioTests
{
    public static IEnumerable<TestDataRow<CatchBindingScenario>> Cases
        => CatchBindingScenarioCatalog.All.Select(static testCase =>
            new TestDataRow<CatchBindingScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndSources()
    {
        var cases = CatchBindingScenarioCatalog.All;

        Assert.IsNotEmpty(cases);
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Dimension).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Count, cases.Select(static testCase => testCase.Source).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(cases.All(static testCase =>
            testCase.Id.StartsWith("semantic.catch-binding.", StringComparison.Ordinal)));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void VisitTry_PreservesSyntheticAndBranchLocalCatchBindings(CatchBindingScenario testCase)
    {
        var operation = GetTryOperation(testCase);
        var node = new SemanticWalker(true).VisitTry(operation, new SenseArgument());
        Assert.IsInstanceOfType<TryStatement>(node, testCase.Id);

        var statement = (TryStatement)node!;
        Assert.IsNotNull(statement.Handler, testCase.Id);
        Assert.IsInstanceOfType<Identifier>(statement.Handler.Param, testCase.Id);
        var catchIdentifier = (Identifier)statement.Handler.Param!;

        var catchNodes = DescendantsAndSelf(statement.Handler.Body).ToArray();
        var throws = catchNodes.OfType<ThrowStatement>().ToArray();
        Assert.HasCount(testCase.ExpectedThrowCount, throws, testCase.Id);
        foreach (var throwStatement in throws)
        {
            Assert.IsInstanceOfType<Identifier>(throwStatement.Argument, testCase.Id);
            Assert.AreEqual(catchIdentifier.Name, ((Identifier)throwStatement.Argument).Name, testCase.Id);
        }

        var declaredBindings = catchNodes
            .OfType<VariableDeclaration>()
            .SelectMany(static declaration => declaration.Declarations)
            .Select(static declarator => declarator.Id)
            .OfType<Identifier>()
            .Select(static identifier => identifier.Name)
            .ToArray();
        CollectionAssert.AreEqual(
            testCase.ExpectedBranchBindings.ToArray(),
            declaredBindings,
            $"{testCase.Id}: actual=[{string.Join(", ", declaredBindings)}]");

        _ = new Parser().ParseScript(node!.ToKnRECMAScript());
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

    private static ITryOperation GetTryOperation(CatchBindingScenario testCase)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            testCase.Source,
            TestMetadataReferences.PreviewParseOptions,
            path: $"{testCase.Id}.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "SemanticWalker.CatchBinding.Tests",
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

        var syntax = syntaxTree.GetRoot().DescendantNodes().OfType<TryStatementSyntax>().First();
        var operation = compilation.GetSemanticModel(syntaxTree).GetOperation(syntax);
        Assert.IsInstanceOfType<ITryOperation>(operation, testCase.Id);
        return (ITryOperation)operation!;
    }
}

public sealed record CatchBindingScenario(
    string Id,
    string Dimension,
    string Source,
    int ExpectedThrowCount,
    IReadOnlyList<string> ExpectedBranchBindings);

internal static class CatchBindingScenarioCatalog
{
    public static IReadOnlyList<CatchBindingScenario> All { get; } =
    [
        Case(
            "direct-bare-rethrow",
            "catch-all-direct-rethrow-requires-synthetic-binding",
            """
            class Demo
            {
                void Risky() { }
                void Run()
                {
                    try { Risky(); }
                    catch { throw; }
                }
            }
            """,
            expectedThrowCount: 1),
        Case(
            "conditional-bare-rethrow",
            "catch-all-nested-if-rethrow-is-detected-recursively",
            """
            class Demo
            {
                void Risky() { }
                void Run(bool shouldRethrow)
                {
                    try { Risky(); }
                    catch
                    {
                        if (shouldRethrow)
                            throw;
                    }
                }
            }
            """,
            expectedThrowCount: 1),
        Case(
            "nested-try-bare-rethrow",
            "catch-all-nested-try-rethrow-is-detected-recursively",
            """
            class Demo
            {
                void Risky() { }
                void Cleanup() { }
                void Run()
                {
                    try { Risky(); }
                    catch
                    {
                        try { throw; }
                        finally { Cleanup(); }
                    }
                }
            }
            """,
            expectedThrowCount: 1),
        Case(
            "filter-without-declaration",
            "catch-filter-and-handler-share-synthetic-binding",
            """
            class Demo
            {
                void Risky() { }
                void Run(bool accept)
                {
                    try { Risky(); }
                    catch when (accept) { throw; }
                }
            }
            """,
            expectedThrowCount: 2),
        Case(
            "typed-filter-without-variable",
            "typed-filter-and-handler-share-synthetic-binding",
            """
            class Demo
            {
                void Risky() { }
                void Run(bool accept)
                {
                    try { Risky(); }
                    catch (System.Exception) when (accept) { throw; }
                }
            }
            """,
            expectedThrowCount: 2),
        Case(
            "same-type-filter-different-variables",
            "multi-catch-filter-chain-declares-branch-local-bindings",
            """
            class Demo
            {
                sealed class Failure : System.Exception { }

                void Risky() { }
                void Handle(int branch) { }

                void Run()
                {
                    try { Risky(); }
                    catch (Failure first) when (first.Message.Length > 0) { Handle(1); }
                    catch (Failure second) when (second.Message.Length == 0) { Handle(2); }
                    catch (System.Exception final) { Handle(3); }
                }
            }
            """,
            expectedThrowCount: 2,
            expectedBranchBindings: ["first", "second", "final", "final"])
    ];

    private static CatchBindingScenario Case(
        string id,
        string dimension,
        string source,
        int expectedThrowCount,
        IReadOnlyList<string>? expectedBranchBindings = null)
        => new(
            $"semantic.catch-binding.{id}",
            dimension,
            source,
            expectedThrowCount,
            expectedBranchBindings ?? []);
}
