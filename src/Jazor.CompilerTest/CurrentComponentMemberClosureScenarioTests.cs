using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class CurrentComponentMemberClosureScenarioTests
{
    public static IEnumerable<TestDataRow<CurrentComponentClosureScenario>> Cases
        => CurrentComponentClosureScenarioCatalog.All.Select(static testCase =>
            new TestDataRow<CurrentComponentClosureScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<CurrentComponentClosureValidationScenario>> ValidationCases
        => CurrentComponentClosureScenarioCatalog.Validations.Select(static testCase =>
            new TestDataRow<CurrentComponentClosureValidationScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalogs_HaveUniqueIdsDimensionsKindsAndInputs()
    {
        var allIds = CurrentComponentClosureScenarioCatalog.All.Select(static testCase => testCase.Id)
            .Concat(CurrentComponentClosureScenarioCatalog.Validations.Select(static testCase => testCase.Id))
            .ToArray();

        Assert.IsNotEmpty(allIds);
        Assert.HasCount(allIds.Length, allIds.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(allIds.All(static id => id.StartsWith("component-closure.", StringComparison.Ordinal)));
        Assert.IsTrue(CurrentComponentClosureScenarioCatalog.All.All(static testCase =>
            !string.IsNullOrWhiteSpace(testCase.Dimension)));
        Assert.IsTrue(CurrentComponentClosureScenarioCatalog.Validations.All(static testCase =>
            !string.IsNullOrWhiteSpace(testCase.Dimension)));
        Assert.HasCount(
            Enum.GetValues<CurrentComponentClosureRootKind>().Length,
            CurrentComponentClosureScenarioCatalog.All.Select(static testCase => testCase.RootKind).Distinct());
        Assert.HasCount(
            Enum.GetValues<CurrentComponentClosureValidationKind>().Length,
            CurrentComponentClosureScenarioCatalog.Validations.Select(static testCase => testCase.Kind).Distinct());
        Assert.HasCount(
            CurrentComponentClosureScenarioCatalog.All.Count,
            CurrentComponentClosureScenarioCatalog.All
                .Select(static testCase => testCase.InputIdentity)
                .Distinct(StringComparer.Ordinal));
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void Build_MatchesExactCurrentComponentDependencyClosure(CurrentComponentClosureScenario testCase)
    {
        var fixture = Compile(testCase);

        var closure = BuildClosure(fixture, testCase);
        var actualMembers = closure.Members.Select(MemberIdentity).ToArray();

        CollectionAssert.AreEqual(
            testCase.ExpectedMembers.Order(StringComparer.Ordinal).ToArray(),
            actualMembers.Order(StringComparer.Ordinal).ToArray(),
            testCase.Id);
        Assert.IsTrue(closure.Members.All(closure.Contains), testCase.Id);
        Assert.IsTrue(closure.Members.All(closure.ShouldInclude), testCase.Id);
        AssertAssociatedPropertyProjection(closure, fixture.Component, testCase);
    }

    [TestMethod]
    [DynamicData(nameof(ValidationCases))]
    public void Build_ValidatesInputsAndCancellation(CurrentComponentClosureValidationScenario testCase)
    {
        var fixture = Compile(ValidationSource, "Component", testCase.Id + ".cs");
        var root = GetOrdinaryMethod(fixture.Component, "Render");

        switch (testCase.Kind)
        {
            case CurrentComponentClosureValidationKind.NullSemanticModel:
                {
                    var exception = Assert.ThrowsExactly<ArgumentNullException>(() =>
                        CurrentComponentMemberClosure.Build(fixture.Component, null!, [root]));
                    Assert.AreEqual("semanticModel", exception.ParamName, testCase.Id);
                    break;
                }
            case CurrentComponentClosureValidationKind.NullComponent:
                {
                    var exception = Assert.ThrowsExactly<ArgumentNullException>(() =>
                        CurrentComponentMemberClosure.Create(
                            null!,
                            fixture.Compilation,
                            [root],
                            Array.Empty<IOperation>()));
                    Assert.AreEqual("componentType", exception.ParamName, testCase.Id);
                    break;
                }
            case CurrentComponentClosureValidationKind.NullCompilation:
                {
                    var exception = Assert.ThrowsExactly<ArgumentNullException>(() =>
                        CurrentComponentMemberClosure.Create(
                            fixture.Component,
                            null!,
                            [root],
                            Array.Empty<IOperation>()));
                    Assert.AreEqual("compilation", exception.ParamName, testCase.Id);
                    break;
                }
            case CurrentComponentClosureValidationKind.NullRoots:
                {
                    var exception = Assert.ThrowsExactly<ArgumentNullException>(() =>
                        CurrentComponentMemberClosure.Create(
                            fixture.Component,
                            fixture.Compilation,
                            null!,
                            Array.Empty<IOperation>()));
                    Assert.AreEqual("roots", exception.ParamName, testCase.Id);
                    break;
                }
            case CurrentComponentClosureValidationKind.NullRootOperations:
                {
                    var exception = Assert.ThrowsExactly<ArgumentNullException>(() =>
                        CurrentComponentMemberClosure.Create(
                            fixture.Component,
                            fixture.Compilation,
                            [root],
                            null!));
                    Assert.AreEqual("rootOperations", exception.ParamName, testCase.Id);
                    break;
                }
            case CurrentComponentClosureValidationKind.NullContainsSymbol:
                {
                    var closure = CurrentComponentMemberClosure.Build(
                        fixture.Component,
                        fixture.SemanticModel,
                        [root]);
                    var exception = Assert.ThrowsExactly<ArgumentNullException>(() => closure.Contains(null!));
                    Assert.AreEqual("symbol", exception.ParamName, testCase.Id);
                    break;
                }
            case CurrentComponentClosureValidationKind.NullShouldIncludeSymbol:
                {
                    var closure = CurrentComponentMemberClosure.Build(
                        fixture.Component,
                        fixture.SemanticModel,
                        [root]);
                    Assert.IsFalse(closure.ShouldInclude(null!), testCase.Id);
                    break;
                }
            case CurrentComponentClosureValidationKind.PreCanceledBuild:
                {
                    using var cancellationSource = new CancellationTokenSource();
                    cancellationSource.Cancel();
                    Assert.ThrowsExactly<OperationCanceledException>(() =>
                        CurrentComponentMemberClosure.Build(
                            fixture.Component,
                            fixture.SemanticModel,
                            [root],
                            cancellationSource.Token));
                    break;
                }
            default:
                Assert.Fail($"{testCase.Id}: unsupported validation kind '{testCase.Kind}'.");
                break;
        }
    }

    private static CurrentComponentMemberClosure BuildClosure(
        ClosureFixture fixture,
        CurrentComponentClosureScenario testCase)
        => testCase.RootKind switch
        {
            CurrentComponentClosureRootKind.MethodSymbols => CurrentComponentMemberClosure.Build(
                fixture.Component,
                fixture.SemanticModel,
                testCase.RootNames.Select(name => GetOrdinaryMethod(fixture.Component, name))),
            CurrentComponentClosureRootKind.InstanceConstructor => CurrentComponentMemberClosure.Build(
                fixture.Component,
                fixture.SemanticModel,
                [GetSourceConstructor(fixture.Component)]),
            CurrentComponentClosureRootKind.OperationOnly => CurrentComponentMemberClosure.Create(
                fixture.Component,
                fixture.Compilation,
                Array.Empty<IMethodSymbol>(),
                [GetMethodBodyOperation(fixture, testCase.RootNames.Single())]),
            CurrentComponentClosureRootKind.ExternalMetadataMethod => CurrentComponentMemberClosure.Build(
                fixture.Component,
                fixture.SemanticModel,
                [fixture.Compilation.GetSpecialType(SpecialType.System_Object)
                    .GetMembers("GetHashCode")
                    .OfType<IMethodSymbol>()
                    .Single(static method => method.Parameters.Length == 0)]),
            _ => throw new InvalidOperationException(
                $"{testCase.Id}: unsupported root kind '{testCase.RootKind}'.")
        };

    private static void AssertAssociatedPropertyProjection(
        CurrentComponentMemberClosure closure,
        INamedTypeSymbol component,
        CurrentComponentClosureScenario testCase)
    {
        if (testCase.AssociatedPropertyName is null)
            return;

        var property = component.GetMembers(testCase.AssociatedPropertyName)
            .OfType<IPropertySymbol>()
            .Single();
        var backingField = component.GetMembers()
            .OfType<IFieldSymbol>()
            .Single(field => SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol, property));
        var getter = property.GetMethod;

        Assert.IsNotNull(getter, testCase.Id);
        Assert.IsFalse(closure.Members.Contains(getter, SymbolEqualityComparer.Default), testCase.Id);
        Assert.IsFalse(closure.Members.Contains(backingField, SymbolEqualityComparer.Default), testCase.Id);
        Assert.IsTrue(closure.Contains(getter), testCase.Id);
        Assert.IsTrue(closure.ShouldInclude(getter), testCase.Id);
        Assert.IsFalse(closure.Contains(backingField), testCase.Id);
        Assert.IsTrue(closure.ShouldInclude(backingField), testCase.Id);
    }

    private static string MemberIdentity(ISymbol symbol)
        => $"{symbol.Kind}:{symbol.ContainingType?.Name}.{symbol.Name}";

    private static IMethodSymbol GetOrdinaryMethod(INamedTypeSymbol type, string name)
        => type.GetMembers(name)
            .OfType<IMethodSymbol>()
            .Single(static method => method.MethodKind == MethodKind.Ordinary);

    private static IMethodSymbol GetSourceConstructor(INamedTypeSymbol type)
        => type.InstanceConstructors.Single(static constructor =>
            constructor.DeclaringSyntaxReferences.Length > 0);

    private static IOperation GetMethodBodyOperation(ClosureFixture fixture, string methodName)
    {
        var method = fixture.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(method => method.Identifier.ValueText == methodName);
        var syntax = (SyntaxNode?)method.Body ?? method.ExpressionBody?.Expression
            ?? throw new InvalidOperationException($"Method '{methodName}' does not have a body.");
        return fixture.SemanticModel.GetOperation(syntax)
            ?? throw new InvalidOperationException($"Method '{methodName}' did not bind to an operation.");
    }

    private static ClosureFixture Compile(CurrentComponentClosureScenario testCase)
        => Compile(testCase.Source, testCase.ComponentTypeName, testCase.Id + ".cs");

    private static ClosureFixture Compile(string source, string componentTypeName, string sourcePath)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: sourcePath);
        var compilation = CSharpCompilation.Create(
            assemblyName: "CurrentComponentClosureScenarios_" + Guid.NewGuid().ToString("N"),
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(
            0,
            errors,
            string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var component = compilation.GetTypeByMetadataName(componentTypeName)
            ?? throw new InvalidOperationException($"Component type '{componentTypeName}' was not found.");
        return new ClosureFixture(
            compilation,
            syntaxTree,
            compilation.GetSemanticModel(syntaxTree),
            component);
    }

    private sealed record ClosureFixture(
        CSharpCompilation Compilation,
        SyntaxTree SyntaxTree,
        SemanticModel SemanticModel,
        INamedTypeSymbol Component);

    private const string ValidationSource =
        "class Component { int value; void Render() { _ = value; } }";
}

public enum CurrentComponentClosureRootKind
{
    MethodSymbols,
    InstanceConstructor,
    OperationOnly,
    ExternalMetadataMethod
}

public sealed record CurrentComponentClosureScenario(
    string Id,
    string Dimension,
    string Source,
    string ComponentTypeName,
    CurrentComponentClosureRootKind RootKind,
    IReadOnlyList<string> RootNames,
    IReadOnlyList<string> ExpectedMembers,
    string? AssociatedPropertyName = null)
{
    public string InputIdentity
        => $"{ComponentTypeName}|{RootKind}|{string.Join(",", RootNames)}|{Source}";
}

public enum CurrentComponentClosureValidationKind
{
    NullSemanticModel,
    NullComponent,
    NullCompilation,
    NullRoots,
    NullRootOperations,
    NullContainsSymbol,
    NullShouldIncludeSymbol,
    PreCanceledBuild
}

public sealed record CurrentComponentClosureValidationScenario(
    string Id,
    string Dimension,
    CurrentComponentClosureValidationKind Kind);

internal static class CurrentComponentClosureScenarioCatalog
{
    public static IReadOnlyList<CurrentComponentClosureScenario> All { get; } =
    [
        Case(
            "direct-instance-dependencies",
            "instance-method-and-field-transitive-closure",
            """
            class Component
            {
                int count;
                void Increment() => count++;
                void Render() => Increment();
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            ["Field:Component.count", "Method:Component.Increment", "Method:Component.Render"]),
        Case(
            "duplicate-roots",
            "duplicate-root-idempotence",
            """
            class Component
            {
                int count;
                void Render() => count++;
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render", "Render"],
            ["Field:Component.count", "Method:Component.Render"]),
        Case(
            "static-field-initializer",
            "static-member-and-initializer-closure",
            """
            class Component
            {
                static int shared = Initialize();
                static int Initialize() => 42;
                void Render() => _ = shared;
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            ["Field:Component.shared", "Method:Component.Initialize", "Method:Component.Render"]),
        Case(
            "field-initializer",
            "instance-field-initializer-dependencies",
            """
            class Component
            {
                int value = CreateValue();
                static int CreateValue() => 7;
                void Render() => _ = value;
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            ["Field:Component.value", "Method:Component.CreateValue", "Method:Component.Render"]),
        Case(
            "auto-property-initializer",
            "auto-property-initializer-and-associated-symbol-projection",
            """
            class Component
            {
                int Value { get; } = CreateValue();
                static int CreateValue() => 7;
                void Render() => _ = Value;
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            ["Method:Component.CreateValue", "Method:Component.Render", "Property:Component.Value"],
            associatedPropertyName: "Value"),
        Case(
            "expression-property",
            "expression-bodied-property-getter",
            """
            class Component
            {
                int Compute() => 7;
                int Value => Compute();
                void Render() => _ = Value;
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            ["Method:Component.Compute", "Method:Component.Render", "Property:Component.Value"]),
        Case(
            "block-property-getter",
            "block-bodied-property-getter",
            """
            class Component
            {
                int Compute() => 7;
                int Value { get { return Compute(); } }
                void Render() => _ = Value;
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            ["Method:Component.Compute", "Method:Component.Render", "Property:Component.Value"]),
        Case(
            "expression-accessor-getter",
            "expression-bodied-get-accessor",
            """
            class Component
            {
                int Compute() => 7;
                int Value { get => Compute(); }
                void Render() => _ = Value;
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            ["Method:Component.Compute", "Method:Component.Render", "Property:Component.Value"]),
        Case(
            "source-base-members",
            "source-declared-base-hierarchy",
            """
            class ComponentBase
            {
                protected int seed = 3;
                protected int Format() => seed + 1;
            }

            class Component : ComponentBase
            {
                void Render() => _ = Format();
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            ["Field:ComponentBase.seed", "Method:Component.Render", "Method:ComponentBase.Format"]),
        Case(
            "external-instance-receiver",
            "other-component-instance-exclusion",
            """
            class Component
            {
                int count;
                void Increment() => count++;
                void Render(Component other) => other.Increment();
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            ["Method:Component.Render"]),
        Case(
            "external-base-member",
            "metadata-base-member-exclusion",
            """
            class Component
            {
                void Render() => _ = ToString();
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            ["Method:Component.Render"]),
        Case(
            "nested-runtime-class",
            "nested-class-construction-and-member-closure",
            """
            class Component
            {
                int seed = 2;

                class Helper
                {
                    int offset = 1;
                    public int Compute(int value) => value + offset;
                }

                void Render() => _ = new Helper().Compute(seed);
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            [
                "Field:Component.seed",
                "Field:Helper.offset",
                "Method:Component.Render",
                "Method:Helper..ctor",
                "Method:Helper.Compute",
                "NamedType:Component.Helper"
            ]),
        Case(
            "nested-record",
            "nested-record-runtime-exclusion",
            """
            class Component
            {
                record Helper(int Value);
                void Render() => _ = new Helper(1).Value;
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            ["Method:Component.Render"]),
        Case(
            "nested-static-class",
            "nested-static-class-runtime-exclusion",
            """
            class Component
            {
                static class Helper { public static int Compute() => 1; }
                void Render() => _ = Helper.Compute();
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            ["Method:Component.Render"]),
        Case(
            "method-group",
            "current-component-method-reference",
            """
            using System;

            class Component
            {
                Action? handler;
                void Handle() { }
                void Render() => handler = Handle;
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            ["Field:Component.handler", "Method:Component.Handle", "Method:Component.Render"]),
        Case(
            "constructor-block-root",
            "block-bodied-constructor-root",
            """
            class Component
            {
                int value;
                public Component() { value = 1; }
            }
            """,
            CurrentComponentClosureRootKind.InstanceConstructor,
            [],
            ["Field:Component.value", "Method:Component..ctor"]),
        Case(
            "constructor-expression-root",
            "expression-bodied-constructor-root",
            """
            class Component
            {
                int value;
                public Component() => value = 1;
            }
            """,
            CurrentComponentClosureRootKind.InstanceConstructor,
            [],
            ["Field:Component.value", "Method:Component..ctor"]),
        Case(
            "operation-only-root",
            "operation-root-without-root-symbol-inventory",
            """
            class Component
            {
                int count;
                void Increment() => count++;
                void Render() { Increment(); }
            }
            """,
            CurrentComponentClosureRootKind.OperationOnly,
            ["Render"],
            ["Field:Component.count", "Method:Component.Increment"]),
        Case(
            "multiple-roots",
            "multiple-independent-root-union",
            """
            class Component
            {
                int first;
                int second;
                void RenderFirst() => _ = first;
                void RenderSecond() => _ = second;
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["RenderSecond", "RenderFirst"],
            [
                "Field:Component.first",
                "Field:Component.second",
                "Method:Component.RenderFirst",
                "Method:Component.RenderSecond"
            ]),
        Case(
            "set-only-property",
            "set-only-property-accessor-dependencies",
            """
            class Component
            {
                int count;
                int Value { set { count = value; } }
                void Render() { Value = 1; }
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            ["Field:Component.count", "Method:Component.Render", "Property:Component.Value"]),
        Case(
            "converted-current-receiver",
            "explicit-conversion-current-component-receiver",
            """
            class Component
            {
                int count;
                void Increment() => count++;
                void Render() => ((Component)this).Increment();
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            ["Field:Component.count", "Method:Component.Increment", "Method:Component.Render"]),
        Case(
            "nested-runtime-property",
            "nested-class-property-reference",
            """
            class Component
            {
                class Helper { public int Value { get; } = 1; }
                void Render() => _ = new Helper().Value;
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            [
                "Method:Component.Render",
                "Method:Helper..ctor",
                "NamedType:Component.Helper",
                "Property:Helper.Value"
            ]),
        Case(
            "nested-runtime-method-group",
            "nested-class-method-reference",
            """
            using System;

            class Component
            {
                Func<int>? handler;
                class Helper { public int Compute() => 1; }
                void Render() => handler = new Helper().Compute;
            }
            """,
            CurrentComponentClosureRootKind.MethodSymbols,
            ["Render"],
            [
                "Field:Component.handler",
                "Method:Component.Render",
                "Method:Helper..ctor",
                "Method:Helper.Compute",
                "NamedType:Component.Helper"
            ]),
        Case(
            "external-metadata-root",
            "external-root-filtering",
            "class Component { }",
            CurrentComponentClosureRootKind.ExternalMetadataMethod,
            [],
            [])
    ];

    public static IReadOnlyList<CurrentComponentClosureValidationScenario> Validations { get; } =
    [
        Validation("null-semantic-model", "build-semantic-model-validation", CurrentComponentClosureValidationKind.NullSemanticModel),
        Validation("null-component", "create-component-validation", CurrentComponentClosureValidationKind.NullComponent),
        Validation("null-compilation", "create-compilation-validation", CurrentComponentClosureValidationKind.NullCompilation),
        Validation("null-roots", "create-roots-validation", CurrentComponentClosureValidationKind.NullRoots),
        Validation("null-root-operations", "create-root-operations-validation", CurrentComponentClosureValidationKind.NullRootOperations),
        Validation("contains-null", "contains-symbol-validation", CurrentComponentClosureValidationKind.NullContainsSymbol),
        Validation("should-include-null", "null-symbol-projection", CurrentComponentClosureValidationKind.NullShouldIncludeSymbol),
        Validation("pre-canceled", "pre-canceled-build-propagation", CurrentComponentClosureValidationKind.PreCanceledBuild)
    ];

    private static CurrentComponentClosureScenario Case(
        string id,
        string dimension,
        string source,
        CurrentComponentClosureRootKind rootKind,
        IReadOnlyList<string> rootNames,
        IReadOnlyList<string> expectedMembers,
        string? associatedPropertyName = null)
        => new(
            $"component-closure.{id}",
            dimension,
            source,
            "Component",
            rootKind,
            rootNames,
            expectedMembers,
            associatedPropertyName);

    private static CurrentComponentClosureValidationScenario Validation(
        string id,
        string dimension,
        CurrentComponentClosureValidationKind kind)
        => new($"component-closure.validation.{id}", dimension, kind);
}
