using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerAttributeProtocolTests
{
    private static readonly Lazy<IReadOnlyDictionary<string, IAttributeOperation>> Operations = new(CreateOperations);

    public static IEnumerable<TestDataRow<AttributeProtocolCase>> EmittedCases
        => AttributeProtocolCatalog.EmittedCases.Select(static testCase =>
            new TestDataRow<AttributeProtocolCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<AttributeProtocolCase>> IgnoredCases
        => AttributeProtocolCatalog.IgnoredCases.Select(static testCase =>
            new TestDataRow<AttributeProtocolCase>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsAndSources()
    {
        var all = AttributeProtocolCatalog.EmittedCases
            .Concat(AttributeProtocolCatalog.IgnoredCases)
            .ToArray();

        Assert.IsNotEmpty(all);
        Assert.HasCount(all.Length, all.Select(static item => item.Id).Distinct(StringComparer.Ordinal));
        Assert.HasCount(all.Length, all.Select(static item => item.Dimension).Distinct(StringComparer.Ordinal));
        Assert.HasCount(all.Length, all.Select(static item => item.Source).Distinct(StringComparer.Ordinal));
        Assert.IsTrue(all.All(static item => item.Id.StartsWith("attribute-protocol.", StringComparison.Ordinal)));
    }

    [TestMethod]
    [DynamicData(nameof(EmittedCases))]
    public void Visit_AttributeProtocol_EmitsDeterministicDecoratorAst(AttributeProtocolCase testCase)
    {
        var operation = Operations.Value[testCase.Id];
        var first = new SemanticWalker(true).Visit(operation, new SenseArgument());
        var second = new SemanticWalker(true).Visit(operation, new SenseArgument());

        Assert.IsInstanceOfType<Decorator>(first, testCase.Id);
        Assert.IsInstanceOfType<Decorator>(second, testCase.Id);
        var firstJavaScript = first.ToKnRECMAScript();
        var secondJavaScript = second.ToKnRECMAScript();
        Assert.AreEqual(firstJavaScript, secondJavaScript, testCase.Id);
        Assert.AreEqual(testCase.ExpectedJavaScript, firstJavaScript, testCase.Id);
    }

    [TestMethod]
    [DynamicData(nameof(IgnoredCases))]
    public void Visit_AttributeProtocol_IgnoresTypesOutsideExactMarkerContract(AttributeProtocolCase testCase)
    {
        var operation = Operations.Value[testCase.Id];

        var node = new SemanticWalker(true).Visit(operation, new SenseArgument());

        Assert.IsNull(node, testCase.Id);
    }

    private static IReadOnlyDictionary<string, IAttributeOperation> CreateOperations()
        => AttributeProtocolCatalog.EmittedCases
            .Concat(AttributeProtocolCatalog.IgnoredCases)
            .ToDictionary(
                static testCase => testCase.Id,
                static testCase => CreateOperation(testCase),
                StringComparer.Ordinal);

    private static IAttributeOperation CreateOperation(AttributeProtocolCase testCase)
    {
        var source = $$"""
            using System;
            using System.ComponentModel;
            using ECMAScript;

            {{testCase.Source}}
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            $"AttributeProtocol_{testCase.Id.Replace('.', '_')}",
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, $"{testCase.Id}{Environment.NewLine}{string.Join(Environment.NewLine, errors.Select(static error => error.ToString()))}");

        var model = compilation.GetSemanticModel(syntaxTree);
        var targetMethod = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static method => method.Identifier.ValueText == "Target");
        var attribute = targetMethod.AttributeLists
            .SelectMany(static list => list.Attributes)
            .Single();

        Assert.IsInstanceOfType<IAttributeOperation>(model.GetOperation(attribute), testCase.Id);
        return (IAttributeOperation)model.GetOperation(attribute)!;
    }
}

public sealed record AttributeProtocolCase(
    string Id,
    string Dimension,
    string Source,
    string? ExpectedJavaScript);

internal static class AttributeProtocolCatalog
{
    public static IReadOnlyList<AttributeProtocolCase> EmittedCases { get; } =
    [
        Emit(
            "marker.no-arguments",
            "marker=exact;constructor=parameterless;named=none;name=short",
            """
            sealed class FlagAttribute : Attribute, IECMAScript
            {
            }

            sealed class Scenario
            {
                [Flag]
                public void Target()
                {
                }
            }
            """,
            "@Flag"),
        Emit(
            "constructor.multiple-positional",
            "marker=exact;constructor=string+int;named=none;name=suffix-elided",
            """
            sealed class RouteAttribute : Attribute, IECMAScript
            {
                public RouteAttribute(string path, int order)
                {
                }
            }

            sealed class Scenario
            {
                [Route("users", 2)]
                public void Target()
                {
                }
            }
            """,
            "@Route(\"users\", 2)"),
        Emit(
            "initializer.property-and-field",
            "marker=exact;constructor=parameterless;named=property+field;keys=symbol-mapped",
            """
            sealed class MetadataAttribute : Attribute, IECMAScript
            {
                public string Label { get; set; } = "";
                public bool Enabled;
            }

            sealed class Scenario
            {
                [Metadata(Label = "entry", Enabled = true)]
                public void Target()
                {
                }
            }
            """,
            "@Metadata({ label: \"entry\", enabled: true })"),
        Emit(
            "constructor-and-initializer.mixed",
            "marker=exact;constructor=int;named=property+field;shape=args-then-options",
            """
            sealed class AuditAttribute : Attribute, IECMAScript
            {
                public AuditAttribute(int order)
                {
                }

                public string Category { get; set; } = "";
                public bool Critical;
            }

            sealed class Scenario
            {
                [Audit(3, Category = "ops", Critical = true)]
                public void Target()
                {
                }
            }
            """,
            "@Audit(3, { category: \"ops\", critical: true })"),
        Emit(
            "configured.type-and-member-aliases",
            "marker=exact;constructor=parameterless;named=configured-property;name=configured",
            """
            [Description("@#trace")]
            sealed class TraceAttribute : Attribute, IECMAScript
            {
                [Description("@#eventName")]
                public string Event { get; set; } = "";
            }

            sealed class Scenario
            {
                [Trace(Event = "save")]
                public void Target()
                {
                }
            }
            """,
            "@trace({ eventName: \"save\" })"),
        Emit(
            "qualified.global-alias",
            "marker=exact;constructor=parameterless;named=none;syntax=global-qualified",
            """
            namespace Contracts
            {
                sealed class QualifiedAttribute : Attribute, IECMAScript
                {
                }
            }

            sealed class Scenario
            {
                [global::Contracts.QualifiedAttribute]
                public void Target()
                {
                }
            }
            """,
            "@Qualified")
    ];

    public static IReadOnlyList<AttributeProtocolCase> IgnoredCases { get; } =
    [
        Ignore(
            "lookalike-interface.different-namespace",
            "marker=short-name-collision;namespace=Other;result=ignored",
            """
            namespace Other
            {
                interface IECMAScript
                {
                }

                sealed class FakeAttribute : Attribute, IECMAScript
                {
                }
            }

            sealed class Scenario
            {
                [Other.Fake]
                public void Target()
                {
                }
            }
            """)
    ];

    private static AttributeProtocolCase Emit(string id, string dimension, string source, string expectedJavaScript)
        => new($"attribute-protocol.{id}", dimension, source, expectedJavaScript);

    private static AttributeProtocolCase Ignore(string id, string dimension, string source)
        => new($"attribute-protocol.{id}", dimension, source, null);
}
