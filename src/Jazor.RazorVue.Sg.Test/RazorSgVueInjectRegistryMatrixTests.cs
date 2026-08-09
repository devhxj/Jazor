using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class VueInjectRegistryMatrixTests
{
    public static IEnumerable<TestDataRow<VueInjectCase>> Cases
        => VueInjectCase.All.Select(static testCase => new TestDataRow<VueInjectCase>(testCase)
        {
            DisplayName = "VueInject_" + testCase.Id
        });

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void ForCompilation_ValidatesStronglyTypedContainerContract(VueInjectCase testCase)
    {
        var compilation = CreateCompilation(testCase);
        if (testCase.ExpectedFailureFragment is not null)
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => VueInjectRegistry.ForCompilation(compilation));
            StringAssert.Contains(exception.Message, testCase.ExpectedFailureFragment, StringComparison.Ordinal);
            StringAssert.Contains(exception.Message, "Demo.ContractShell", StringComparison.Ordinal);
            return;
        }

        var first = VueInjectRegistry.ForCompilation(compilation);
        var second = VueInjectRegistry.ForCompilation(compilation);
        Assert.AreSame(first, second);

        var contract = compilation.GetTypeByMetadataName("Demo.ContractShell");
        var implementation = compilation.GetTypeByMetadataName("Demo.InjectedShell");
        var unregistered = compilation.GetTypeByMetadataName("Demo.OtherContract");
        Assert.IsNotNull(contract);
        Assert.IsNotNull(implementation);
        Assert.IsNotNull(unregistered);
        Assert.IsTrue(SymbolEqualityComparer.Default.Equals(
            implementation,
            first.ResolveImplementation(contract!)));
        Assert.IsTrue(SymbolEqualityComparer.Default.Equals(
            unregistered,
            first.ResolveImplementation(unregistered!)));
    }

    [TestMethod]
    public void ForCompilation_RejectsNullInputsAndAllowsCompilationsWithoutVueInjectMetadata()
    {
        var nullCompilation = Assert.Throws<ArgumentNullException>(() => VueInjectRegistry.ForCompilation(null!));
        Assert.AreEqual("compilation", nullCompilation.ParamName);

        var sourceTree = CSharpSyntaxTree.ParseText(
            "namespace Demo; public sealed class PlainComponent { }",
            new CSharpParseOptions(LanguageVersion.Preview));
        var compilation = CSharpCompilation.Create(
            "RazorVue.VueInject.NoMetadata",
            [sourceTree],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var component = compilation.GetTypeByMetadataName("Demo.PlainComponent");
        Assert.IsNotNull(component);

        var registry = VueInjectRegistry.ForCompilation(compilation);
        Assert.IsTrue(SymbolEqualityComparer.Default.Equals(component, registry.ResolveImplementation(component!)));

        var nullComponent = Assert.Throws<ArgumentNullException>(() => registry.ResolveImplementation(null!));
        Assert.AreEqual("authoredComponent", nullComponent.ParamName);
    }

    [TestMethod]
    public void ForCompilation_IgnoresUnrelatedAttributesAndRejectsNonNamedVueInjectTypeArguments()
    {
        var ignoredAttributeCompilation = CreateContractCompilation(
            "RazorVue.VueInject.UnrelatedAttribute",
            """
            using System;
            using ECMAScript.VueContract;

            [assembly: CLSCompliant(true)]

            namespace Demo;

            public sealed class PlainComponent
            {
            }
            """);
        var plainComponent = ignoredAttributeCompilation.GetTypeByMetadataName("Demo.PlainComponent");
        Assert.IsNotNull(plainComponent);
        Assert.IsTrue(SymbolEqualityComparer.Default.Equals(
            plainComponent,
            VueInjectRegistry.ForCompilation(ignoredAttributeCompilation).ResolveImplementation(plainComponent!)));

        var invalidContractCompilation = CreateContractCompilation(
            "RazorVue.VueInject.InvalidContractArgument",
            """
            using ECMAScript.VueContract;

            [assembly: VueInject(typeof(string[]), typeof(string))]
            """);
        var invalidContract = Assert.Throws<InvalidOperationException>(
            () => VueInjectRegistry.ForCompilation(invalidContractCompilation));
        StringAssert.Contains(invalidContract.Message, "contract argument must be a named component type", StringComparison.Ordinal);

        var invalidImplementationCompilation = CreateContractCompilation(
            "RazorVue.VueInject.InvalidImplementationArgument",
            """
            using ECMAScript.VueContract;

            [assembly: VueInject(typeof(string), typeof(string[]))]
            """);
        var invalidImplementation = Assert.Throws<InvalidOperationException>(
            () => VueInjectRegistry.ForCompilation(invalidImplementationCompilation));
        StringAssert.Contains(invalidImplementation.Message, "implementation argument must be a named component type", StringComparison.Ordinal);
    }

    [TestMethod]
    public void ForCompilation_UsesDerivedParametersAndSkipsShadowedOrNonParameterProperties()
    {
        var compilation = CreateContractCompilation(
            "RazorVue.VueInject.InheritedParameters",
            """
            #nullable enable
            using ECMAScript;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue3;

            [assembly: VueInject(typeof(Demo.ContractShell), typeof(Demo.InjectedShell))]

            namespace Demo;

            public abstract class ContractBase : ComponentBase
            {
                [Parameter] public string? Shared { get; set; }
                [Parameter(CaptureUnmatchedValues = false)] public string? BaseOnly { get; set; }
                public string? Ignored { get; set; }
            }

            [ECMAScriptModule("./contracts/shell")]
            public sealed class ContractShell : ContractBase, IVueComponent, IVueContainerComponent
            {
                [Parameter] public new string? Shared { get; set; }
            }

            public abstract class ImplementationBase : ComponentBase
            {
                [Parameter] public string? Shared { get; set; }
                [Parameter(CaptureUnmatchedValues = false)] public string? BaseOnly { get; set; }
                public string? Ignored { get; set; }
            }

            [ECMAScriptModule("./implementations/shell")]
            public sealed class InjectedShell : ImplementationBase, IVueComponent, IVueContainerImplementation<ContractShell>
            {
                [Parameter] public new string? Shared { get; set; }
            }
            """);
        var contract = compilation.GetTypeByMetadataName("Demo.ContractShell");
        var implementation = compilation.GetTypeByMetadataName("Demo.InjectedShell");
        Assert.IsNotNull(contract);
        Assert.IsNotNull(implementation);

        var resolved = VueInjectRegistry.ForCompilation(compilation).ResolveImplementation(contract!);
        Assert.IsTrue(SymbolEqualityComparer.Default.Equals(implementation, resolved));
    }

    private static CSharpCompilation CreateCompilation(VueInjectCase testCase)
    {
        var source = BuildSource(testCase);
        var compilation = CSharpCompilation.Create(
            "RazorVue.VueInject.Matrix." + testCase.Id,
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview))],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        return compilation;
    }

    private static CSharpCompilation CreateContractCompilation(string assemblyName, string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview))],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        return compilation;
    }

    private static string BuildSource(VueInjectCase testCase)
    {
        var duplicate = testCase.DuplicateRegistration
            ? "[assembly: VueInject(typeof(Demo.ContractShell), typeof(Demo.InjectedShell))]"
            : string.Empty;
        var contractInterfaces = testCase.ContractImplementsContainer
            ? ", IVueComponent, IVueContainerComponent"
            : ", IVueComponent";
        var implementationBases = new List<string>();
        if (testCase.ImplementationIsComponent)
        {
            implementationBases.Add("ComponentBase");
            implementationBases.Add("IVueComponent");
        }
        if (testCase.ImplementationContractName is not null)
            implementationBases.Add("IVueContainerImplementation<" + testCase.ImplementationContractName + ">");
        var implementationBaseList = implementationBases.Count == 0
            ? string.Empty
            : " : " + string.Join(", ", implementationBases);

        return $$"""
            #nullable enable
            using System;
            using System.Collections.Generic;
            using ECMAScript;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue3;

            [assembly: VueInject(typeof(Demo.ContractShell), typeof(Demo.InjectedShell))]
            {{duplicate}}

            namespace Demo
            {
                [ECMAScriptModule("./contracts/shell")]
                public sealed class ContractShell : ComponentBase{{contractInterfaces}}
                {
                    {{testCase.ContractParameter}}
                }

                [ECMAScriptModule("./contracts/other")]
                public sealed class OtherContract : ComponentBase, IVueComponent, IVueContainerComponent
                {
                }

                [ECMAScriptModule("./implementations/shell")]
                public sealed class InjectedShell{{implementationBaseList}}
                {
                    {{testCase.ImplementationParameter}}
                }
            }
            """;
    }
}

public sealed record VueInjectCase(
    string Id,
    string ContractParameter,
    string ImplementationParameter,
    string? ExpectedFailureFragment,
    bool ContractImplementsContainer = true,
    bool ImplementationIsComponent = true,
    string? ImplementationContractName = "ContractShell",
    bool DuplicateRegistration = false)
{
    public static IReadOnlyList<VueInjectCase> All { get; } =
    [
        Valid("nullable_string", "[Parameter] public string? Value { get; set; }"),
        Valid("required_string", "[Parameter] public string Value { get; set; } = string.Empty;"),
        Valid("int32", "[Parameter] public int Value { get; set; }"),
        Valid("boolean", "[Parameter] public bool Value { get; set; }"),
        Valid("int64", "[Parameter] public long Value { get; set; }"),
        Valid("double", "[Parameter] public double Value { get; set; }"),
        Valid("decimal", "[Parameter] public decimal Value { get; set; }"),
        Valid("nullable_datetime", "[Parameter] public DateTime? Value { get; set; }"),
        Valid("event_callback", "[Parameter] public EventCallback Value { get; set; }"),
        Valid("generic_event_callback", "[Parameter] public EventCallback<string> Value { get; set; }"),
        Valid("render_fragment", "[Parameter] public RenderFragment? Value { get; set; }"),
        Valid("generic_render_fragment", "[Parameter] public RenderFragment<string>? Value { get; set; }"),
        Valid("string_array", "[Parameter] public string[] Value { get; set; } = [];"),
        Valid("int_array", "[Parameter] public int[] Value { get; set; } = [];"),
        Valid("readonly_list", "[Parameter] public IReadOnlyList<string>? Value { get; set; }"),
        Valid("readonly_dictionary", "[Parameter] public IReadOnlyDictionary<string, int>? Value { get; set; }"),
        Valid("callback_delegate", "[Parameter] public Action<string>? Value { get; set; }"),
        Valid("editor_required", "[Parameter, EditorRequired] public string? Value { get; set; }"),
        Valid("capture_unmatched", "[Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? Value { get; set; }"),
        new(
            "missing_parameter",
            "[Parameter] public string? Value { get; set; }",
            string.Empty,
            "is missing parameter 'Value'"),
        new(
            "type_mismatch",
            "[Parameter] public string? Value { get; set; }",
            "[Parameter] public int Value { get; set; }",
            "has type 'int'"),
        new(
            "nullability_mismatch",
            "[Parameter] public string? Value { get; set; }",
            "[Parameter] public string Value { get; set; } = string.Empty;",
            "contract requires 'string?'"),
        new(
            "editor_required_missing",
            "[Parameter, EditorRequired] public string? Value { get; set; }",
            "[Parameter] public string? Value { get; set; }",
            "EditorRequired"),
        new(
            "editor_required_extra",
            "[Parameter] public string? Value { get; set; }",
            "[Parameter, EditorRequired] public string? Value { get; set; }",
            "EditorRequired"),
        new(
            "capture_missing",
            "[Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? Value { get; set; }",
            "[Parameter] public IReadOnlyDictionary<string, object>? Value { get; set; }",
            "CaptureUnmatchedValues"),
        new(
            "capture_extra",
            "[Parameter] public IReadOnlyDictionary<string, object>? Value { get; set; }",
            "[Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? Value { get; set; }",
            "CaptureUnmatchedValues"),
        new(
            "wrong_implementation_contract",
            "[Parameter] public string? Value { get; set; }",
            "[Parameter] public string? Value { get; set; }",
            "must implement IVueContainerImplementation<Demo.ContractShell>",
            ImplementationContractName: "OtherContract"),
        new(
            "duplicate_registration",
            "[Parameter] public string? Value { get; set; }",
            "[Parameter] public string? Value { get; set; }",
            "duplicate implementations",
            DuplicateRegistration: true),
        new(
            "contract_not_container",
            "[Parameter] public string? Value { get; set; }",
            "[Parameter] public string? Value { get; set; }",
            "contract must implement IVueContainerComponent",
            ContractImplementsContainer: false,
            ImplementationContractName: null),
        new(
            "implementation_not_component",
            "[Parameter] public string? Value { get; set; }",
            "[Parameter] public string? Value { get; set; }",
            "must implement IComponent",
            ImplementationIsComponent: false)
    ];

    private static VueInjectCase Valid(string id, string parameter)
        => new(id, parameter, parameter, ExpectedFailureFragment: null);
}
