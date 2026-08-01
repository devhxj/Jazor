using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerEcmascriptParamsProtocolTests
{
    private static readonly Lazy<IReadOnlyDictionary<string, IBlockOperation>> Operations = new(CreateOperations);

    public static IEnumerable<TestDataRow<EcmascriptParamsProtocolCase>> Cases
        => EcmascriptParamsProtocolCatalog.Cases.Select(static testCase =>
            new TestDataRow<EcmascriptParamsProtocolCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndBodies()
    {
        var cases = EcmascriptParamsProtocolCatalog.Cases;

        Assert.IsNotEmpty(cases);
        Assert.HasCount(cases.Count, cases.Select(static item => item.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(cases.Count, cases.Select(static item => item.Body).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(cases.All(static item => item.Id.StartsWith("ecmascript-params.", StringComparison.Ordinal)));
        Assert.IsTrue(cases.All(static item => !string.IsNullOrWhiteSpace(item.Dimension)));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Visit_EcmascriptParams_ExpandsNormalFormArguments(EcmascriptParamsProtocolCase testCase)
    {
        var block = Operations.Value[testCase.Id];
        var first = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).Visit(block, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first, testCase.Id);
        Assert.AreEqual(first, second, testCase.Id);
        foreach (var fragment in testCase.ExpectedJavaScriptFragments)
            StringAssert.Contains(first, fragment, testCase.Id);

        var previousIndex = -1;
        foreach (var fragment in testCase.OrderedJavaScriptFragments)
        {
            var index = first.IndexOf(fragment, previousIndex + 1, StringComparison.Ordinal);
            Assert.IsGreaterThan(previousIndex, index, $"{testCase.Id}: {fragment}");
            previousIndex = index;
        }

        _ = new Parser().ParseScript(first);
    }

    private static IReadOnlyDictionary<string, IBlockOperation> CreateOperations()
    {
        var cases = EcmascriptParamsProtocolCatalog.Cases;
        var methods = string.Join(
            Environment.NewLine,
            cases.Select(static (testCase, index) => $$"""
                    public void Scenario{{index:D2}}()
                    {
                {{testCase.Body}}
                    }
                """));
        var source = $$"""
            public sealed class EcmascriptParamsProtocolScenarios
            {
                private static int First() => 1;
                private static int Second() => 2;

            {{methods}}
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "EcmascriptParamsProtocolScenarios",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var model = compilation.GetSemanticModel(syntaxTree);
        var blocks = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(static method => method.Identifier.ValueText.StartsWith("Scenario", StringComparison.Ordinal))
            .OrderBy(static method => method.Identifier.ValueText, StringComparer.Ordinal)
            .Select(method => Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(method.Body!)))
            .ToArray();

        return cases.Select(static item => item.Id)
            .Zip(blocks, static (id, block) => (id, block))
            .ToDictionary(static item => item.id, static item => item.block, StringComparer.Ordinal);
    }
}

public sealed record EcmascriptParamsProtocolCase(
    string Id,
    string Dimension,
    string Body,
    IReadOnlyList<string> ExpectedJavaScriptFragments,
    IReadOnlyList<string> OrderedJavaScriptFragments);

internal static class EcmascriptParamsProtocolCatalog
{
    public static IReadOnlyList<EcmascriptParamsProtocolCase> Cases { get; } =
    [
        Case(
            "collection-expression",
            "call-form=normal;argument=collection-expression;lowering=expanded-elements;evaluation=left-to-right",
                """
                        var values = ECMAScript.Array<int>.Of([First(), Second()]);
                """,
            ["let values = Array.of(EcmascriptParamsProtocolScenarios.first(), EcmascriptParamsProtocolScenarios.second());"],
            ["EcmascriptParamsProtocolScenarios.first()", "EcmascriptParamsProtocolScenarios.second()"]),
        Case(
            "array-creation",
            "call-form=normal;argument=array-creation;lowering=expanded-elements;evaluation=left-to-right",
                """
                        var values = ECMAScript.Array<int>.Of(new[] { First(), Second() });
                """,
            ["let values = Array.of(EcmascriptParamsProtocolScenarios.first(), EcmascriptParamsProtocolScenarios.second());"],
            ["EcmascriptParamsProtocolScenarios.first()", "EcmascriptParamsProtocolScenarios.second()"]),
        Case(
            "collection-expression-spread",
            "call-form=normal;argument=collection-expression-spread;lowering=expanded-elements;evaluation=left-to-right",
                """
                        int[] source = [First(), Second()];
                        var values = ECMAScript.Array<int>.Of([0, ..source, 3]);
                """,
            [
                "let source = [EcmascriptParamsProtocolScenarios.first(), EcmascriptParamsProtocolScenarios.second()];",
                "let values = Array.of(0, ...source, 3);"
            ],
            ["let source = [", "Array.of(0, ...source, 3)"]),
        Case(
            "array-variable-spread",
            "call-form=normal;argument=array-local;lowering=spread;source=evaluated-once",
            """
                        int[] source = [First(), Second()];
                        var values = ECMAScript.Array<int>.Of(source);
                """,
            [
                "let source = [EcmascriptParamsProtocolScenarios.first(), EcmascriptParamsProtocolScenarios.second()];",
                "let values = Array.of(...source);"
            ],
            ["let source = [", "Array.of(...source)"])
    ];

    private static EcmascriptParamsProtocolCase Case(
        string id,
        string dimension,
        string body,
        IReadOnlyList<string> expectedJavaScriptFragments,
        IReadOnlyList<string> orderedJavaScriptFragments)
        => new(
            $"ecmascript-params.{id}",
            dimension,
            body,
            expectedJavaScriptFragments,
            orderedJavaScriptFragments);
}
