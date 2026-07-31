using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class NativeMapSetEqualityScenarioTests
{
    public static IEnumerable<TestDataRow<NativeMapSetEqualitySuccessScenario>> SuccessCases
        => NativeMapSetEqualityScenarioCatalog.Successes.Select(static testCase =>
            new TestDataRow<NativeMapSetEqualitySuccessScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<NativeMapSetEqualityFailureScenario>> FailureCases
        => NativeMapSetEqualityScenarioCatalog.Failures.Select(static testCase =>
            new TestDataRow<NativeMapSetEqualityFailureScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalog_HasUniqueIdsDimensionsKindsAndSources()
    {
        var allIds = NativeMapSetEqualityScenarioCatalog.Successes.Select(static testCase => testCase.Id)
            .Concat(NativeMapSetEqualityScenarioCatalog.Failures.Select(static testCase => testCase.Id))
            .ToArray();
        var allDimensions = NativeMapSetEqualityScenarioCatalog.Successes.Select(static testCase => testCase.Dimension)
            .Concat(NativeMapSetEqualityScenarioCatalog.Failures.Select(static testCase => testCase.Dimension))
            .ToArray();
        var allSources = NativeMapSetEqualityScenarioCatalog.Successes.Select(static testCase => testCase.Source)
            .Concat(NativeMapSetEqualityScenarioCatalog.Failures.Select(static testCase => testCase.Source))
            .ToArray();

        Assert.IsNotEmpty(allIds);
        Assert.HasCount(allIds.Length, allIds.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(allIds.All(static id => id.StartsWith("native-map-set-equality.", StringComparison.Ordinal)));
        Assert.IsTrue(allDimensions.All(static dimension => !string.IsNullOrWhiteSpace(dimension)));
        Assert.HasCount(allDimensions.Length, allDimensions.Distinct(StringComparer.Ordinal));
        Assert.HasCount(allSources.Length, allSources.Distinct(StringComparer.Ordinal));
        Assert.HasCount(
            Enum.GetValues<NativeMapSetEqualitySuccessKind>().Length,
            NativeMapSetEqualityScenarioCatalog.Successes.Select(static testCase => testCase.Kind).Distinct());
        Assert.HasCount(
            Enum.GetValues<NativeMapSetEqualityFailureKind>().Length,
            NativeMapSetEqualityScenarioCatalog.Failures.Select(static testCase => testCase.Kind).Distinct());
    }

    [TestMethod]
    [DynamicData(nameof(SuccessCases))]
    public void VisitObjectCreation_AllowsClrEqualityCompatibleWithNativeMapSet(
        NativeMapSetEqualitySuccessScenario testCase)
    {
        var operation = CompileObjectCreation(testCase.Source, testCase.Id);
        var walker = new SemanticWalker(true);

        var script = walker.VisitObjectCreation(operation, new SenseArgument())?.ToKnRECMAScript();

        Assert.AreEqual(testCase.ExpectedScript, script, testCase.Id);
    }

    [TestMethod]
    [DynamicData(nameof(FailureCases))]
    public void VisitObjectCreation_RejectsClrEqualityNotPreservedByNativeMapSet(
        NativeMapSetEqualityFailureScenario testCase)
    {
        var operation = CompileObjectCreation(testCase.Source, testCase.Id);
        var walker = new SemanticWalker(true);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            walker.VisitObjectCreation(operation, new SenseArgument()));

        StringAssert.Contains(exception.Message, "does not have JS-stable default equality", StringComparison.Ordinal, testCase.Id);
        StringAssert.Contains(exception.Message, testCase.ExpectedTypeFragment, StringComparison.Ordinal, testCase.Id);
        StringAssert.Contains(exception.Message, testCase.ExpectedReasonFragment, StringComparison.Ordinal, testCase.Id);
        Assert.AreEqual(testCase.Id + ".cs", Path.GetFileName(exception.Data["location.path"] as string), testCase.Id);
        Assert.IsGreaterThan(0, ReadLocationInt(exception, "location.startLine", testCase.Id), testCase.Id);
        Assert.IsGreaterThan(0, ReadLocationInt(exception, "location.startColumn", testCase.Id), testCase.Id);
    }

    private static int ReadLocationInt(Exception exception, string key, string scenarioId)
    {
        var value = exception.Data[key];
        Assert.IsInstanceOfType<int>(value, scenarioId);
        return (int)value;
    }

    private static IObjectCreationOperation CompileObjectCreation(string source, string scenarioId)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            "using System;\nusing System.Collections.Generic;\n" + source,
            TestMetadataReferences.PreviewParseOptions,
            scenarioId + ".cs");
        var compilation = CSharpCompilation.Create(
            "NativeMapSetEquality_" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsEmpty(errors, scenarioId + ": " + string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var model = compilation.GetSemanticModel(syntaxTree);
        var creationSyntax = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ObjectCreationExpressionSyntax>()
            .Single();
        var operation = model.GetOperation(creationSyntax);
        Assert.IsInstanceOfType<IObjectCreationOperation>(operation, scenarioId);
        return (IObjectCreationOperation)operation;
    }
}

public enum NativeMapSetEqualitySuccessKind
{
    EnumScalar,
    ArrayIdentity,
    ReferenceIdentity,
    NonSelfEquatableReferenceIdentity
}

public sealed record NativeMapSetEqualitySuccessScenario(
    string Id,
    string Dimension,
    NativeMapSetEqualitySuccessKind Kind,
    string Source,
    string ExpectedScript);

public enum NativeMapSetEqualityFailureKind
{
    OpenTypeParameter,
    InterfaceContract,
    StructValueEquality,
    DelegateSemanticEquality,
    RecordValueEquality,
    SelfEquatableValueEquality,
    ObjectEqualsOverride,
    GetHashCodeOverride,
    InheritedValueEquality,
    DynamicUnknown
}

public sealed record NativeMapSetEqualityFailureScenario(
    string Id,
    string Dimension,
    NativeMapSetEqualityFailureKind Kind,
    string Source,
    string ExpectedTypeFragment,
    string ExpectedReasonFragment);

internal static class NativeMapSetEqualityScenarioCatalog
{
    public static IReadOnlyList<NativeMapSetEqualitySuccessScenario> Successes { get; } =
    [
        new(
            "native-map-set-equality.enum-dictionary-key",
            "enum-erases-to-stable-scalar-key",
            NativeMapSetEqualitySuccessKind.EnumScalar,
            """
            enum Status
            {
                Pending,
                Ready
            }

            sealed class Scenario
            {
                void Run()
                {
                    var values = new Dictionary<Status, string>();
                }
            }
            """,
            "new Map"),
        new(
            "native-map-set-equality.array-set-element",
            "array-preserves-reference-identity-element",
            NativeMapSetEqualitySuccessKind.ArrayIdentity,
            """
            sealed class Scenario
            {
                void Run()
                {
                    var values = new HashSet<byte[]>();
                }
            }
            """,
            "new Set"),
        new(
            "native-map-set-equality.reference-identity-dictionary-key",
            "ordinary-reference-key-preserves-identity-equality",
            NativeMapSetEqualitySuccessKind.ReferenceIdentity,
            """
            sealed class IdentityKey
            {
                public IdentityKey(int value) => Value = value;
                public int Value { get; }
            }

            sealed class Scenario
            {
                void Run()
                {
                    var values = new Dictionary<IdentityKey, string>();
                }
            }
            """,
            "new Map"),
        new(
            "native-map-set-equality.non-self-equatable-set-element",
            "non-self-equatable-interface-does-not-change-default-reference-equality",
            NativeMapSetEqualitySuccessKind.NonSelfEquatableReferenceIdentity,
            """
            sealed class IdentityKey : IEquatable<string>
            {
                public bool Equals(string? other) => false;
            }

            sealed class Scenario
            {
                void Run()
                {
                    var values = new HashSet<IdentityKey>();
                }
            }
            """,
            "new Set")
    ];

    public static IReadOnlyList<NativeMapSetEqualityFailureScenario> Failures { get; } =
    [
        new(
            "native-map-set-equality.open-type-parameter-key",
            "open-generic-key-has-no-proven-equality-carrier",
            NativeMapSetEqualityFailureKind.OpenTypeParameter,
            """
            sealed class Scenario
            {
                void Run<T>()
                {
                    var values = new Dictionary<T, string>();
                }
            }
            """,
            "T",
            "Type-parameter keys/elements"),
        new(
            "native-map-set-equality.interface-set-element",
            "interface-element-may-hide-value-equality",
            NativeMapSetEqualityFailureKind.InterfaceContract,
            """
            interface IKey
            {
                int Id { get; }
            }

            sealed class Scenario
            {
                void Run()
                {
                    var values = new HashSet<IKey>();
                }
            }
            """,
            "IKey",
            "Interface-typed keys/elements"),
        new(
            "native-map-set-equality.struct-dictionary-key",
            "struct-key-requires-clr-value-equality",
            NativeMapSetEqualityFailureKind.StructValueEquality,
            """
            readonly struct Key
            {
                public Key(int value)
                {
                    Value = value;
                }

                public int Value { get; }
            }

            sealed class Scenario
            {
                void Run()
                {
                    var values = new Dictionary<Key, string>();
                }
            }
            """,
            "Key",
            "Struct keys/elements use CLR value equality"),
        new(
            "native-map-set-equality.delegate-set-element",
            "delegate-element-has-clr-invocation-list-equality",
            NativeMapSetEqualityFailureKind.DelegateSemanticEquality,
            """
            delegate void Handler(int value);

            sealed class Scenario
            {
                void Run()
                {
                    var values = new HashSet<Handler>();
                }
            }
            """,
            "Handler",
            "Delegate equality is not modeled"),
        new(
            "native-map-set-equality.record-dictionary-key",
            "record-key-uses-synthesized-value-equality",
            NativeMapSetEqualityFailureKind.RecordValueEquality,
            """
            sealed record Key(int Value);

            sealed class Scenario
            {
                void Run()
                {
                    var values = new Dictionary<Key, string>();
                }
            }
            """,
            "Key",
            "record/custom equality semantics"),
        new(
            "native-map-set-equality.self-equatable-set-element",
            "self-equatable-element-overrides-default-reference-equality",
            NativeMapSetEqualityFailureKind.SelfEquatableValueEquality,
            """
            sealed class Key : IEquatable<Key>
            {
                public Key(int value) => Value = value;
                public int Value { get; }
                public bool Equals(Key? other) => other is not null && Value == other.Value;
            }

            sealed class Scenario
            {
                void Run()
                {
                    var values = new HashSet<Key>();
                }
            }
            """,
            "Key",
            "record/custom equality semantics"),
        new(
            "native-map-set-equality.equals-override-dictionary-key",
            "object-equals-override-changes-default-reference-equality",
            NativeMapSetEqualityFailureKind.ObjectEqualsOverride,
            """
            sealed class Key
            {
                public Key(int value) => Value = value;
                public int Value { get; }
                public override bool Equals(object? other) => other is Key key && Value == key.Value;
            }

            sealed class Scenario
            {
                void Run()
                {
                    var values = new Dictionary<Key, string>();
                }
            }
            """,
            "Key",
            "record/custom equality semantics"),
        new(
            "native-map-set-equality.hash-code-override-set-element",
            "hash-code-override-signals-custom-default-equality-contract",
            NativeMapSetEqualityFailureKind.GetHashCodeOverride,
            """
            sealed class Key
            {
                public Key(int value) => Value = value;
                public int Value { get; }
                public override int GetHashCode() => Value;
            }

            sealed class Scenario
            {
                void Run()
                {
                    var values = new HashSet<Key>();
                }
            }
            """,
            "Key",
            "record/custom equality semantics"),
        new(
            "native-map-set-equality.inherited-equals-override-dictionary-key",
            "derived-key-inherits-custom-default-equality-contract",
            NativeMapSetEqualityFailureKind.InheritedValueEquality,
            """
            abstract class KeyBase
            {
                public override bool Equals(object? other) => other is KeyBase;
            }

            sealed class Key : KeyBase
            {
            }

            sealed class Scenario
            {
                void Run()
                {
                    var values = new Dictionary<Key, string>();
                }
            }
            """,
            "Key",
            "record/custom equality semantics"),
        new(
            "native-map-set-equality.dynamic-dictionary-key",
            "dynamic-key-has-no-static-equality-contract",
            NativeMapSetEqualityFailureKind.DynamicUnknown,
            """
            sealed class Scenario
            {
                void Run()
                {
                    var values = new Dictionary<dynamic, string>();
                }
            }
            """,
            "dynamic",
            "does not map to a JS-stable default equality contract")
    ];
}
