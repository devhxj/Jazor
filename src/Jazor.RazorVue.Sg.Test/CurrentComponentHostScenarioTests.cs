using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Jazor.RazorVue.RazorSdk;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class CurrentComponentHostScenarioTests
{
    public static IEnumerable<TestDataRow<CurrentComponentHostSuccessScenario>> SuccessCases
        => CurrentComponentHostScenarioCatalog.Successes.Select(static testCase =>
            new TestDataRow<CurrentComponentHostSuccessScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<CurrentComponentHostFailureScenario>> FailureCases
        => CurrentComponentHostScenarioCatalog.Failures.Select(static testCase =>
            new TestDataRow<CurrentComponentHostFailureScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    public static IEnumerable<TestDataRow<CurrentComponentHostValidationScenario>> ValidationCases
        => CurrentComponentHostScenarioCatalog.Validations.Select(static testCase =>
            new TestDataRow<CurrentComponentHostValidationScenario>(testCase)
            {
                DisplayName = testCase.Id
            });

    [TestMethod]
    public void ScenarioCatalogs_HaveUniqueIdsDimensionsKindsAndInputs()
    {
        var allIds = CurrentComponentHostScenarioCatalog.Successes.Select(static testCase => testCase.Id)
            .Concat(CurrentComponentHostScenarioCatalog.Failures.Select(static testCase => testCase.Id))
            .Concat(CurrentComponentHostScenarioCatalog.Validations.Select(static testCase => testCase.Id))
            .ToArray();

        Assert.IsNotEmpty(allIds);
        Assert.HasCount(allIds.Length, allIds.Distinct(StringComparer.Ordinal));
        Assert.IsTrue(allIds.All(static id => id.StartsWith("component-host.", StringComparison.Ordinal)));
        Assert.IsTrue(CurrentComponentHostScenarioCatalog.Successes.All(static testCase =>
            !string.IsNullOrWhiteSpace(testCase.Dimension)));
        Assert.IsTrue(CurrentComponentHostScenarioCatalog.Failures.All(static testCase =>
            !string.IsNullOrWhiteSpace(testCase.Dimension)));
        Assert.IsTrue(CurrentComponentHostScenarioCatalog.Validations.All(static testCase =>
            !string.IsNullOrWhiteSpace(testCase.Dimension)));
        Assert.HasCount(
            Enum.GetValues<CurrentComponentHostConfigurationKind>().Length,
            CurrentComponentHostScenarioCatalog.Successes.Select(static testCase => testCase.Configuration).Distinct());
        Assert.HasCount(
            Enum.GetValues<CurrentComponentHostFailureKind>().Length,
            CurrentComponentHostScenarioCatalog.Failures.Select(static testCase => testCase.Kind).Distinct());
        Assert.HasCount(
            Enum.GetValues<CurrentComponentHostValidationKind>().Length,
            CurrentComponentHostScenarioCatalog.Validations.Select(static testCase => testCase.Kind).Distinct());
        Assert.HasCount(
            CurrentComponentHostScenarioCatalog.Successes.Count,
            CurrentComponentHostScenarioCatalog.Successes
                .Select(static testCase => testCase.InputIdentity)
                .Distinct(StringComparer.Ordinal));
    }

    [TestMethod]
    [DynamicData(nameof(SuccessCases))]
    public void Rewrite_MatchesConfiguredCurrentComponentSurface(CurrentComponentHostSuccessScenario testCase)
    {
        var fixture = Compile(testCase.Source, testCase.Id);
        var walker = new SemanticWalker(true)
        {
            Host = CreateHost(fixture, testCase.Configuration)
        };

        var script = walker.Visit(fixture.RenderBody, new SenseArgument())
            ?.ToKnRECMAScript()
            ?.ReplaceLineEndings("\n");

        Assert.IsNotNull(script, testCase.Id);
        foreach (var expected in testCase.ExpectedFragments)
            StringAssert.Contains(script, expected, StringComparison.Ordinal, testCase.Id);
        foreach (var forbidden in testCase.ForbiddenFragments)
            Assert.IsFalse(script.Contains(forbidden, StringComparison.Ordinal), $"{testCase.Id}: {script}");
    }

    [TestMethod]
    [DynamicData(nameof(FailureCases))]
    public void Rewrite_RejectsUnsupportedCurrentComponentProtocol(CurrentComponentHostFailureScenario testCase)
    {
        var fixture = Compile(testCase.Source, testCase.Id);
        var walker = new SemanticWalker(true)
        {
            Host = new VueSemanticWalkerHost(fixture.Component)
        };

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            walker.Visit(fixture.RenderBody, new SenseArgument()));

        foreach (var expected in testCase.ExpectedMessageFragments)
            StringAssert.Contains(exception.Message, expected, StringComparison.Ordinal, testCase.Id);
        Assert.AreEqual(
            "Counter.razor.g.cs",
            Path.GetFileName(exception.Data["location.path"] as string),
            testCase.Id);
        Assert.IsGreaterThan(0, ReadLocationInt(exception, "location.startLine", testCase.Id), testCase.Id);
        Assert.IsGreaterThan(0, ReadLocationInt(exception, "location.startColumn", testCase.Id), testCase.Id);
    }

    [TestMethod]
    [DynamicData(nameof(ValidationCases))]
    public void Constructor_RejectsInvalidConfiguration(CurrentComponentHostValidationScenario testCase)
    {
        var fixture = Compile(ValidationSource, testCase.Id);

        var exception = testCase.Kind switch
        {
            CurrentComponentHostValidationKind.NullComponent => Assert.ThrowsExactly<ArgumentNullException>(() =>
                new VueSemanticWalkerHost(null!)),
            CurrentComponentHostValidationKind.BlankStateIdentifier => Assert.ThrowsExactly<ArgumentException>(() =>
                new VueSemanticWalkerHost(fixture.Component, stateIdentifier: " ")),
            CurrentComponentHostValidationKind.BlankPropsIdentifier => Assert.ThrowsExactly<ArgumentException>(() =>
                new VueSemanticWalkerHost(fixture.Component, propsIdentifier: "\t")),
            _ => throw new InvalidOperationException(
                $"{testCase.Id}: unsupported validation kind '{testCase.Kind}'.")
        };

        Assert.AreEqual(testCase.ExpectedParameterName, exception.ParamName, testCase.Id);
        StringAssert.Contains(exception.Message, testCase.ExpectedMessageFragment, StringComparison.Ordinal, testCase.Id);
    }

    private static VueSemanticWalkerHost CreateHost(
        ComponentFixture fixture,
        CurrentComponentHostConfigurationKind configuration)
    {
        var parameterNames = new Dictionary<string, string>(StringComparer.Ordinal);
        var memberNames = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default);
        var stateIdentifier = "state";
        var propsIdentifier = "props";

        switch (configuration)
        {
            case CurrentComponentHostConfigurationKind.CustomSurfaceIdentifiers:
                stateIdentifier = "componentState";
                propsIdentifier = "componentProps";
                break;
            case CurrentComponentHostConfigurationKind.HyphenatedParameterName:
                parameterNames.Add("Label", "aria-label");
                break;
            case CurrentComponentHostConfigurationKind.LeadingDigitParameterName:
                parameterNames.Add("Label", "1st-label");
                break;
            case CurrentComponentHostConfigurationKind.BlankParameterNameFallback:
                parameterNames.Add("Label", " ");
                break;
            case CurrentComponentHostConfigurationKind.MemberRuntimeNames:
                memberNames.Add(GetMember<IFieldSymbol>(fixture.Component, "count"), "currentCount");
                memberNames.Add(GetMember<IMethodSymbol>(fixture.Component, "Increment"), "runIncrement");
                break;
            case CurrentComponentHostConfigurationKind.AutoPropertyState:
            case CurrentComponentHostConfigurationKind.ExternalInstanceReceiver:
                break;
            case CurrentComponentHostConfigurationKind.ComponentInvokeAsyncName:
                memberNames.Add(GetMember<IMethodSymbol>(fixture.Component, "InvokeAsync"), "componentInvokeAsync");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(configuration), configuration, null);
        }

        return new VueSemanticWalkerHost(
            fixture.Component,
            stateIdentifier,
            propsIdentifier,
            parameterNames,
            memberNames);
    }

    private static TSymbol GetMember<TSymbol>(INamedTypeSymbol type, string name)
        where TSymbol : class, ISymbol
        => type.GetMembers(name).OfType<TSymbol>().Single();

    private static int ReadLocationInt(Exception exception, string key, string scenarioId)
    {
        var value = exception.Data[key];
        Assert.IsInstanceOfType<int>(value, $"{scenarioId}: metadata '{key}'.");
        return (int)value;
    }

    private static ComponentFixture Compile(string source, string scenarioId)
    {
        const string usings = """
            global using System;
            global using ECMAScript;
            global using static ECMAScript.Global;
            global using Microsoft.AspNetCore.Components;
            global using Microsoft.AspNetCore.Components.Web;
            global using Microsoft.AspNetCore.Components.Rendering;
            """;
        var references = TestMetadataReferences.Net11
            .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(ComponentBase).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(MouseEventArgs).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(RenderTreeBuilder).Assembly.Location));
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "Counter.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName: "CurrentComponentHostScenarios_" + Guid.NewGuid().ToString("N"),
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(usings, TestMetadataReferences.PreviewParseOptions, path: "GlobalUsings.g.cs"),
                syntaxTree
            ],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(
            0,
            errors,
            $"{scenarioId}:{Environment.NewLine}" +
            string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var componentDeclaration = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "Counter");
        var component = semanticModel.GetDeclaredSymbol(componentDeclaration)
            ?? throw new InvalidOperationException("Counter component symbol was not available.");
        var render = componentDeclaration.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static method => method.Identifier.ValueText == "Render");
        var body = semanticModel.GetOperation(render.Body!) as IBlockOperation
            ?? throw new InvalidOperationException("Render body operation was not available.");
        return new ComponentFixture(component, body);
    }

    private sealed record ComponentFixture(
        INamedTypeSymbol Component,
        IBlockOperation RenderBody);

    private const string ValidationSource =
        "sealed class Counter : ComponentBase { void Render() { } }";
}

public enum CurrentComponentHostConfigurationKind
{
    CustomSurfaceIdentifiers,
    HyphenatedParameterName,
    LeadingDigitParameterName,
    BlankParameterNameFallback,
    MemberRuntimeNames,
    AutoPropertyState,
    ComponentInvokeAsyncName,
    ExternalInstanceReceiver
}

public sealed record CurrentComponentHostSuccessScenario(
    string Id,
    string Dimension,
    CurrentComponentHostConfigurationKind Configuration,
    string Source,
    IReadOnlyList<string> ExpectedFragments,
    IReadOnlyList<string> ForbiddenFragments)
{
    public string InputIdentity
        => $"{Configuration}|{Source}";
}

public enum CurrentComponentHostFailureKind
{
    EventCallbackExternalReceiver,
    EventCallbackLocalHandler,
    BinderMultipleStatements,
    BinderConstantAssignment,
    BinderLocalAssignment,
    IndexedProperty,
    InterfaceMethodReference
}

public sealed record CurrentComponentHostFailureScenario(
    string Id,
    string Dimension,
    CurrentComponentHostFailureKind Kind,
    string Source,
    IReadOnlyList<string> ExpectedMessageFragments);

public enum CurrentComponentHostValidationKind
{
    NullComponent,
    BlankStateIdentifier,
    BlankPropsIdentifier
}

public sealed record CurrentComponentHostValidationScenario(
    string Id,
    string Dimension,
    CurrentComponentHostValidationKind Kind,
    string ExpectedParameterName,
    string ExpectedMessageFragment);

internal static class CurrentComponentHostScenarioCatalog
{
    public static IReadOnlyList<CurrentComponentHostSuccessScenario> Successes { get; } =
    [
        Success(
            "custom-surface-identifiers",
            "custom-state-and-props-identifiers",
            CurrentComponentHostConfigurationKind.CustomSurfaceIdentifiers,
            ParameterAndFieldSource,
            ["componentProps.label", "componentState.count"],
            ["props.label", "state.count"]),
        Success(
            "hyphenated-parameter-name",
            "computed-props-access-for-hyphenated-runtime-name",
            CurrentComponentHostConfigurationKind.HyphenatedParameterName,
            ParameterSource,
            ["props[\"aria-label\"]"],
            ["props.label"]),
        Success(
            "leading-digit-parameter-name",
            "computed-props-access-for-leading-digit-runtime-name",
            CurrentComponentHostConfigurationKind.LeadingDigitParameterName,
            ParameterSource,
            ["props[\"1st-label\"]"],
            ["props.label"]),
        Success(
            "blank-parameter-name-fallback",
            "blank-parameter-runtime-name-fallback",
            CurrentComponentHostConfigurationKind.BlankParameterNameFallback,
            ParameterSource,
            ["props.label"],
            ["props[\" \"]"]),
        Success(
            "member-runtime-names",
            "configured-field-and-method-runtime-names",
            CurrentComponentHostConfigurationKind.MemberRuntimeNames,
            MemberSource,
            ["state.currentCount", "runIncrement()"],
            ["state.count", "increment()"]),
        Success(
            "auto-property-state",
            "non-parameter-auto-property-state-access",
            CurrentComponentHostConfigurationKind.AutoPropertyState,
            AutoPropertySource,
            ["state.value"],
            ["value()", "props.value"]),
        Success(
            "component-invoke-async",
            "component-declared-invoke-async-member-path",
            CurrentComponentHostConfigurationKind.ComponentInvokeAsyncName,
            ComponentInvokeAsyncSource,
            ["componentInvokeAsync(() => {", "state.count++;", "return;"],
            ["invokeAsync(() => {"]),
        Success(
            "external-instance-receiver",
            "other-component-instance-normal-dispatch",
            CurrentComponentHostConfigurationKind.ExternalInstanceReceiver,
            ExternalReceiverSource,
            ["other.count", "other.increment()"],
            ["state.count"])
    ];

    public static IReadOnlyList<CurrentComponentHostFailureScenario> Failures { get; } =
    [
        Failure(
            "event-callback-external-receiver",
            "event-callback-factory-current-receiver-requirement",
            CurrentComponentHostFailureKind.EventCallbackExternalReceiver,
            """
            sealed class Counter : ComponentBase
            {
                void Handle() { }
                void Render(Counter other)
                {
                    _ = EventCallback.Factory.Create(other, Handle);
                }
            }
            """,
            ["EventCallbackFactory.Create", "current-component receivers"]),
        Failure(
            "event-callback-local-handler",
            "event-callback-handler-shape-requirement",
            CurrentComponentHostFailureKind.EventCallbackLocalHandler,
            """
            sealed class Counter : ComponentBase
            {
                void Handle() { }
                void Render()
                {
                    Action handler = Handle;
                    _ = EventCallback.Factory.Create(this, handler);
                }
            }
            """,
            ["EventCallbackFactory.Create", "method-group or simple state-assignment lambda"]),
        Failure(
            "binder-multiple-statements",
            "binder-single-assignment-requirement",
            CurrentComponentHostFailureKind.BinderMultipleStatements,
            """
            sealed class Counter : ComponentBase
            {
                string text = "";
                bool changed;
                void Render()
                {
                    _ = EventCallback.Factory.CreateBinder(this, value => { text = value; changed = true; }, text);
                }
            }
            """,
            ["EventCallbackFactory.CreateBinder", "Anonymous body operation kinds"]),
        Failure(
            "binder-constant-assignment",
            "binder-parameter-flow-requirement",
            CurrentComponentHostFailureKind.BinderConstantAssignment,
            """
            sealed class Counter : ComponentBase
            {
                string text = "";
                void Render()
                {
                    _ = EventCallback.Factory.CreateBinder(this, value => text = "fixed", text);
                }
            }
            """,
            ["EventCallbackFactory.CreateBinder", "simple current-component state assignment"]),
        Failure(
            "binder-local-assignment",
            "binder-current-component-target-requirement",
            CurrentComponentHostFailureKind.BinderLocalAssignment,
            """
            sealed class Counter : ComponentBase
            {
                string text = "";
                void Render()
                {
                    var local = text;
                    _ = EventCallback.Factory.CreateBinder(this, value => local = value, text);
                }
            }
            """,
            ["EventCallbackFactory.CreateBinder", "simple current-component state assignment"]),
        Failure(
            "indexed-property",
            "current-component-indexer-rejection",
            CurrentComponentHostFailureKind.IndexedProperty,
            """
            sealed class Counter : ComponentBase
            {
                int this[int index] => index;
                void Render() { _ = this[1]; }
            }
            """,
            ["indexed property", "Counter.this[int]"]),
        Failure(
            "interface-method-reference",
            "indirect-interface-method-group-rejection",
            CurrentComponentHostFailureKind.InterfaceMethodReference,
            """
            interface ICounter { void Increment(); }
            sealed class Counter : ComponentBase, ICounter
            {
                void ICounter.Increment() { }
                void Render() { Action handler = ((ICounter)this).Increment; }
            }
            """,
            ["Indirect current-component dispatch", "ICounter.Increment()"])
    ];

    public static IReadOnlyList<CurrentComponentHostValidationScenario> Validations { get; } =
    [
        new(
            "component-host.validation.null-component",
            "null-component-symbol-validation",
            CurrentComponentHostValidationKind.NullComponent,
            "componentType",
            "Value cannot be null"),
        new(
            "component-host.validation.blank-state",
            "blank-state-identifier-validation",
            CurrentComponentHostValidationKind.BlankStateIdentifier,
            "stateIdentifier",
            "State identifier cannot be empty"),
        new(
            "component-host.validation.blank-props",
            "blank-props-identifier-validation",
            CurrentComponentHostValidationKind.BlankPropsIdentifier,
            "propsIdentifier",
            "Props identifier cannot be empty")
    ];

    private static CurrentComponentHostSuccessScenario Success(
        string id,
        string dimension,
        CurrentComponentHostConfigurationKind configuration,
        string source,
        IReadOnlyList<string> expectedFragments,
        IReadOnlyList<string> forbiddenFragments)
        => new(
            $"component-host.success.{id}",
            dimension,
            configuration,
            source,
            expectedFragments,
            forbiddenFragments);

    private static CurrentComponentHostFailureScenario Failure(
        string id,
        string dimension,
        CurrentComponentHostFailureKind kind,
        string source,
        IReadOnlyList<string> expectedMessageFragments)
        => new(
            $"component-host.failure.{id}",
            dimension,
            kind,
            source,
            expectedMessageFragments);

    private const string ParameterAndFieldSource = """
        sealed class Counter : ComponentBase
        {
            [Parameter] public string Label { get; set; } = "";
            int count;
            void Render() { _ = Label; _ = count; }
        }
        """;

    private const string ParameterSource = """
        sealed class Counter : ComponentBase
        {
            [Parameter] public string Label { get; set; } = "";
            void Render() { _ = Label; }
        }
        """;

    private const string MemberSource = """
        sealed class Counter : ComponentBase
        {
            int count;
            void Increment() => count++;
            void Render() { _ = count; Increment(); }
        }
        """;

    private const string AutoPropertySource = """
        sealed class Counter : ComponentBase
        {
            int Value { get; set; }
            void Render() { _ = Value; }
        }
        """;

    private const string ComponentInvokeAsyncSource = """
        sealed class Counter : ComponentBase
        {
            int count;
            void InvokeAsync(Action work) => work();
            void Render() { InvokeAsync(() => count++); }
        }
        """;

    private const string ExternalReceiverSource = """
        sealed class Counter : ComponentBase
        {
            int count;
            void Increment() => count++;
            void Render(Counter other) { _ = other.count; other.Increment(); }
        }
        """;
}
