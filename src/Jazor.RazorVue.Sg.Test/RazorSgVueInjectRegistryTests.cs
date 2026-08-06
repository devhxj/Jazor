using ECMAScript;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class VueInjectRegistryTests
{
    [TestMethod]
    public void ForCompilation_ValidRegistration_ResolvesImplementation()
    {
        var compilation = CreateCompilation(RegistrationSource());
        var registry = VueInjectRegistry.ForCompilation(compilation);
        var contract = compilation.GetTypeByMetadataName("Demo.ContractShell");
        var implementation = compilation.GetTypeByMetadataName("Demo.InjectedShell");

        Assert.IsNotNull(contract);
        Assert.IsNotNull(implementation);
        Assert.IsTrue(SymbolEqualityComparer.Default.Equals(
            implementation,
            registry.ResolveImplementation(contract!)));
    }

    [TestMethod]
    public void ForCompilation_DuplicateRegistration_RejectsAmbiguousImplementation()
    {
        var compilation = CreateCompilation(RegistrationSource(
            additionalAssemblyAttribute:
            "[assembly: VueInject(typeof(Demo.ContractShell), typeof(Demo.InjectedShell))]"));

        var exception = Assert.Throws<InvalidOperationException>(
            () => VueInjectRegistry.ForCompilation(compilation));

        StringAssert.Contains(exception.Message, "duplicate implementations", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "Demo.ContractShell", StringComparison.Ordinal);
    }

    [TestMethod]
    public void ForCompilation_MismatchedImplementationContract_IsRejected()
    {
        var compilation = CreateCompilation(RegistrationSource(
            implementationInterface: "IVueContainerImplementation<OtherContract>"));

        var exception = Assert.Throws<InvalidOperationException>(
            () => VueInjectRegistry.ForCompilation(compilation));

        StringAssert.Contains(exception.Message, "must implement IVueContainerImplementation<Demo.ContractShell>", StringComparison.Ordinal);
    }

    [TestMethod]
    public void ForCompilation_MissingImplementationParameter_IsRejected()
    {
        var compilation = CreateCompilation(RegistrationSource(
            contractParameter: "[Parameter] public string? Title { get; set; }",
            implementationParameter: string.Empty));

        var exception = Assert.Throws<InvalidOperationException>(
            () => VueInjectRegistry.ForCompilation(compilation));

        StringAssert.Contains(exception.Message, "is missing parameter 'Title'", StringComparison.Ordinal);
    }

    [TestMethod]
    public void ForCompilation_ImplementationParameterTypeMismatch_IsRejected()
    {
        var compilation = CreateCompilation(RegistrationSource(
            contractParameter: "[Parameter] public string? Title { get; set; }",
            implementationParameter: "[Parameter] public int Title { get; set; }"));

        var exception = Assert.Throws<InvalidOperationException>(
            () => VueInjectRegistry.ForCompilation(compilation));

        StringAssert.Contains(exception.Message, "has type 'int'", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "requires 'string?'", StringComparison.Ordinal);
    }

    [TestMethod]
    public void ForCompilation_CaptureUnmatchedValuesMismatch_IsRejected()
    {
        var compilation = CreateCompilation(RegistrationSource(
            contractParameter:
            "[Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }",
            implementationParameter:
            "[Parameter] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }"));

        var exception = Assert.Throws<InvalidOperationException>(
            () => VueInjectRegistry.ForCompilation(compilation));

        StringAssert.Contains(exception.Message, "CaptureUnmatchedValues", StringComparison.Ordinal);
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var compilation = CSharpCompilation.Create(
            "RazorSg.VueInject.Tests",
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview))],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors));
        return compilation;
    }

    private static string RegistrationSource(
        string contractParameter = "[Parameter] public string? Title { get; set; }",
        string implementationParameter = "[Parameter] public string? Title { get; set; }",
        string implementationInterface = "IVueContainerImplementation<ContractShell>",
        string additionalAssemblyAttribute = "")
        => $$"""
            #nullable enable
            using System.Collections.Generic;
            using ECMAScript;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue3;

            [assembly: VueInject(typeof(Demo.ContractShell), typeof(Demo.InjectedShell))]
            {{additionalAssemblyAttribute}}

            namespace Demo
            {
                [ECMAScriptModule("./components/contract-shell")]
                public sealed class ContractShell : ComponentBase, IVueComponent, IVueContainerComponent
                {
                    {{contractParameter}}
                }

                [ECMAScriptModule("./components/other-contract")]
                public sealed class OtherContract : ComponentBase, IVueComponent, IVueContainerComponent
                {
                    {{contractParameter}}
                }

                [ECMAScriptModule("./components/injected-shell")]
                public sealed class InjectedShell : ComponentBase, IVueComponent, {{implementationInterface}}
                {
                    {{implementationParameter}}
                }
            }
            """;
}
