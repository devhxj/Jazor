using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerConfiguredContractProtocolTests
{
    private static readonly Lazy<IReadOnlyDictionary<string, IObjectCreationOperation>> Operations = new(CreateOperations);

    public static IEnumerable<TestDataRow<ConfiguredContractSuccessCase>> SuccessCases
        => ConfiguredContractProtocolCatalog.SuccessCases.Select(static testCase =>
            new TestDataRow<ConfiguredContractSuccessCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<ConfiguredContractFailureCase>> FailureCases
        => ConfiguredContractProtocolCatalog.FailureCases.Select(static testCase =>
            new TestDataRow<ConfiguredContractFailureCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndBodies()
    {
        var all = ConfiguredContractProtocolCatalog.SuccessCases
            .Select(static testCase => (testCase.Id, testCase.Dimension, testCase.Body))
            .Concat(ConfiguredContractProtocolCatalog.FailureCases.Select(static testCase =>
                (testCase.Id, testCase.Dimension, testCase.Body)))
            .ToArray();

        Assert.IsNotEmpty(all);
        Assert.HasCount(all.Length, all.Select(static item => item.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(all.Length, all.Select(static item => item.Body).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(all.All(static item => item.Id.StartsWith("configured-contract.", StringComparison.Ordinal)));
        Assert.IsTrue(all.All(static item => !string.IsNullOrWhiteSpace(item.Dimension)));
    }

    [TestMethod]
    [DynamicData(nameof(SuccessCases))]
    public void Visit_ConfiguredContract_InfersDeterministicParsableObjectLiteral(ConfiguredContractSuccessCase testCase)
    {
        var operation = Operations.Value[testCase.Id];
        var first = new SemanticWalker(true).VisitObjectCreation(operation, new SenseArgument())?.ToKnRECMAScript();
        var second = new SemanticWalker(true).VisitObjectCreation(operation, new SenseArgument())?.ToKnRECMAScript();

        Assert.IsNotNull(first, testCase.Id);
        Assert.AreEqual(first, second, testCase.Id);
        foreach (var fragment in testCase.ExpectedJavaScriptFragments)
            StringAssert.Contains(first, fragment, testCase.Id);

        _ = new Parser().ParseExpression($"({first})");
    }

    [TestMethod]
    [DynamicData(nameof(FailureCases))]
    public void Visit_ConfiguredContract_RejectsInvalidInferenceContract(ConfiguredContractFailureCase testCase)
    {
        var operation = Operations.Value[testCase.Id];

        var exception = Assert.Throws<OperationTransformationException>(() =>
            new SemanticWalker(true).VisitObjectCreation(operation, new SenseArgument()));

        foreach (var fragment in testCase.ExpectedDiagnosticFragments)
            StringAssert.Contains(exception.Message, fragment, testCase.Id);
    }

    private static IReadOnlyDictionary<string, IObjectCreationOperation> CreateOperations()
    {
        var all = ConfiguredContractProtocolCatalog.SuccessCases
            .Select(static testCase => (testCase.Id, testCase.Body))
            .Concat(ConfiguredContractProtocolCatalog.FailureCases.Select(static testCase => (testCase.Id, testCase.Body)))
            .ToArray();
        var methods = string.Join(
            Environment.NewLine,
            all.Select(static (testCase, index) => $$"""
                    public void Scenario{{index:D2}}()
                    {
                {{testCase.Body}}
                    }
                """));
        var source = $$"""
            using System;
            using System.ComponentModel;
            using ECMAScript;
            using Jazor.ComplierTest;
            using static ECMAScript.Vue3;

            public record BaseCounterProps : VueProps
            {
                [Description("@#baseValue")]
                public int BaseValue { get; init; }

                public static string StaticValue { get; } = "ignored";
                internal string InternalValue { get; init; } = "ignored";
            }

            public sealed record CounterProps : BaseCounterProps
            {
                [Description("@#message")]
                public string? Message { get; init; }

                public string this[int index] => index.ToString();
            }

            public sealed class ConfiguredContractScenarios
            {
                private static VueRenderCallback Setup(CounterProps props, VueSetupContext context)
                {
                    context.Emit("ready", props.Message);
                    context.Emit("ready", props.BaseValue);
                    context.Emit("changed");
                    return () => H("div", props.Message);
                }

            {{methods}}
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "ConfiguredContractScenarios",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.PropsAttribute).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.Vue3).Assembly.Location))
                .Add(MetadataReference.CreateFromFile(typeof(TestShiftedContractComponentOptions<,>).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var model = compilation.GetSemanticModel(syntaxTree);
        var creations = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(static method => method.Identifier.ValueText.StartsWith("Scenario", StringComparison.Ordinal))
            .OrderBy(static method => method.Identifier.ValueText, StringComparer.Ordinal)
            .Select(method => model.GetOperation(method.Body!)!
                .DescendantsAndSelf()
                .OfType<IObjectCreationOperation>()
                .Single(static operation => operation.Type?.Name.StartsWith("Test", StringComparison.Ordinal) == true))
            .ToArray();

        return all.Select(static item => item.Id)
            .Zip(creations, static (id, operation) => (id, operation))
            .ToDictionary(static item => item.id, static item => item.operation, StringComparer.Ordinal);
    }
}

public sealed record ConfiguredContractSuccessCase(
    string Id,
    string Dimension,
    string Body,
    IReadOnlyList<string> ExpectedJavaScriptFragments);

public sealed record ConfiguredContractFailureCase(
    string Id,
    string Dimension,
    string Body,
    IReadOnlyList<string> ExpectedDiagnosticFragments);

internal static class ConfiguredContractProtocolCatalog
{
    public static IReadOnlyList<ConfiguredContractSuccessCase> SuccessCases { get; } =
    [
        Success(
            "method-group.inherited-props-deduplicated-emits",
            "props=generic-index-1;inheritance=base-first;setup=method-group;emits=deduplicated",
            """
                        var options = new TestShiftedContractComponentOptions<int, CounterProps>
                        {
                            Name = "Counter",
                            Bootstrap = Setup
                        };
                """,
            "name: \"Counter\"",
            "props: [\"baseValue\", \"message\"]",
            "emits: [\"ready\", \"changed\"]",
            "setup: ConfiguredContractScenarios.setup"),
        Success(
            "inline-lambda.literal-emits",
            "props=generic-index-1;setup=anonymous-function;emits=literal-order",
            """
                        var options = new TestShiftedContractComponentOptions<int, CounterProps>
                        {
                            Bootstrap = (props, context) =>
                            {
                                context.Emit("opened");
                                context.Emit("closed");
                                return () => H("div", props.Message);
                            }
                        };
                """,
            "props: [\"baseValue\", \"message\"]",
            "emits: [\"opened\", \"closed\"]",
            "setup: (props, context) =>"),
        Success(
            "local-function.no-emits",
            "props=generic-index-1;setup=local-function-expression-body;emits=empty",
            """
                        VueRenderCallback LocalSetup(CounterProps props, VueSetupContext context)
                            => () => H("div", props.Message);

                        var options = new TestShiftedContractComponentOptions<int, CounterProps>
                        {
                            Bootstrap = LocalSetup
                        };
            """,
            "props: [\"baseValue\", \"message\"]",
            "emits: []",
            "setup: LocalSetup.bind(this)"),
        Success(
            "static-local-function.no-emits",
            "props=generic-index-1;setup=static-local-function-expression-body;emits=empty",
            """
                        static VueRenderCallback StaticSetup(CounterProps props, VueSetupContext context)
                            => () => H("div", props.BaseValue);

                        var options = new TestShiftedContractComponentOptions<int, CounterProps>
                        {
                            Bootstrap = StaticSetup
                        };
                """,
            "props: [\"baseValue\", \"message\"]",
            "emits: []",
            "setup: StaticSetup"),
        Success(
            "setup-omitted.empty-emits",
            "props=generic-index-1;setup=omitted;emits=empty",
            """
                        var options = new TestShiftedContractComponentOptions<int, CounterProps>
                        {
                            Name = "Static"
                        };
                """,
            "name: \"Static\"",
            "props: [\"baseValue\", \"message\"]",
            "emits: []")
    ];

    public static IReadOnlyList<ConfiguredContractFailureCase> FailureCases { get; } =
    [
        Failure(
            "props.invalid-target-type",
            "attribute=Props;target=string;result=rejected",
            """
                        var options = new TestInvalidPropsTypeComponentOptions<CounterProps>();
                """,
            "[Props] can only be applied to string[] members"),
        Failure(
            "props.negative-type-index",
            "attribute=Props;type-argument-index=-1;result=rejected",
            """
                        var options = new TestNegativePropsIndexComponentOptions<CounterProps>();
                """,
            "must declare a non-negative TypeArgumentIndex"),
        Failure(
            "props.missing-type-argument",
            "attribute=Props;type-argument-index=1;generic-arity=1;result=rejected",
            """
                        var options = new TestMissingPropsTypeArgumentComponentOptions<CounterProps>();
                """,
            "provide a named generic type argument at index 1"),
        Failure(
            "emits.invalid-target-type",
            "attribute=Emits;target=string;result=rejected",
            """
                        var options = new TestInvalidEmitsTypeComponentOptions<CounterProps>
                        {
                            Setup = Setup
                        };
                """,
            "[Emits] can only be applied to string[] members"),
        Failure(
            "emits.missing-source-member",
            "attribute=Emits;source=missing;result=rejected",
            """
                        var options = new TestMissingEmitsSourceComponentOptions<CounterProps>();
                """,
            "source member 'Missing'",
            "was not found"),
        Failure(
            "emits.whitespace-source-member",
            "attribute=Emits;source=whitespace;result=rejected",
            """
                        var options = new TestWhitespaceEmitsSourceComponentOptions<CounterProps>();
                """,
            "must declare a non-empty SourceMemberName"),
        Failure(
            "emits.context-alias",
            "attribute=Emits;setup=anonymous-function;context=aliased;result=rejected",
            """
                        var options = new TestShiftedContractComponentOptions<int, CounterProps>
                        {
                            Bootstrap = (props, context) =>
                            {
                                var alias = context;
                                alias.Emit("ready");
                                return () => H("div", props.Message);
                            }
                        };
                """,
            "only supports direct setup-context member usage",
            "passed around or aliased"),
        Failure(
            "emits.empty-event-name",
            "attribute=Emits;setup=anonymous-function;event=empty;result=rejected",
            """
                        var options = new TestShiftedContractComponentOptions<int, CounterProps>
                        {
                            Bootstrap = (props, context) =>
                            {
                                context.Emit("");
                                return () => H("div", props.Message);
                            }
                        };
                """,
            "requires literal non-empty event names")
    ];

    private static ConfiguredContractSuccessCase Success(
        string id,
        string dimension,
        string body,
        params string[] expectedJavaScriptFragments)
        => new($"configured-contract.{id}", dimension, body, expectedJavaScriptFragments);

    private static ConfiguredContractFailureCase Failure(
        string id,
        string dimension,
        string body,
        params string[] expectedDiagnosticFragments)
        => new($"configured-contract.{id}", dimension, body, expectedDiagnosticFragments);
}
