using ECMAScript;
using System.Reflection;
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

    [TestMethod]
    public void PrivateRegistryHelpers_RejectMissingHostContractsAndScanDecoratedParameters()
    {
        var decoratedCompilation = CreateCompilation(RegistrationSource(
            contractParameter:
            "[global::System.Obsolete(\"contract metadata\"), Parameter(CaptureUnmatchedValues = false)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }",
            implementationParameter:
            "[global::System.Obsolete(\"implementation metadata\"), Parameter(CaptureUnmatchedValues = false)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }"));
        var registration = decoratedCompilation.Assembly.GetAttributes().Single(attribute =>
            string.Equals(
                attribute.AttributeClass?.ToDisplayString(),
                "ECMAScript.VueContract.VueInjectAttribute",
                StringComparison.Ordinal));
        var contract = GetNamedType(decoratedCompilation, "Demo.ContractShell");
        var decoratedParameter = contract.GetMembers("AdditionalAttributes").OfType<IPropertySymbol>().Single();

        Assert.IsFalse((bool)InvokePrivate("CapturesUnmatchedValues", decoratedParameter)!);
        var readFailure = Assert.Throws<TargetInvocationException>(() =>
            InvokePrivate("ReadComponentType", registration, 2, "extra"));
        Assert.IsInstanceOfType<InvalidOperationException>(readFailure.InnerException);
        StringAssert.Contains(readFailure.InnerException.Message, "extra argument", StringComparison.Ordinal);

        AssertPrivateValidationFailure(
            CreateMinimalCompilation(
                "RazorVue.VueInject.MissingContainer",
                "namespace Demo { public sealed class Contract { } public sealed class Implementation { } }"),
            "Demo.Contract",
            "Demo.Implementation",
            "contract must implement IVueContainerComponent");
        AssertPrivateValidationFailure(
            CreateMinimalCompilation(
                "RazorVue.VueInject.MissingComponent",
                """
                namespace ECMAScript.VueContract
                {
                    public interface IVueContainerComponent { }
                    public interface IVueContainerImplementation<T> { }
                }

                namespace Demo
                {
                    public sealed class Contract : ECMAScript.VueContract.IVueContainerComponent { }
                    public sealed class Implementation : ECMAScript.VueContract.IVueContainerImplementation<Contract> { }
                }
                """),
            "Demo.Contract",
            "Demo.Implementation",
            "must implement IComponent");
        AssertPrivateValidationFailure(
            CreateMinimalCompilation(
                "RazorVue.VueInject.MissingImplementationContract",
                """
                namespace ECMAScript.VueContract
                {
                    public interface IVueContainerComponent { }
                }

                namespace Microsoft.AspNetCore.Components
                {
                    public interface IComponent { }
                }

                namespace Demo
                {
                    public sealed class Contract : ECMAScript.VueContract.IVueContainerComponent { }
                    public sealed class Implementation : Microsoft.AspNetCore.Components.IComponent { }
                }
                """),
            "Demo.Contract",
            "Demo.Implementation",
            "must implement IVueContainerImplementation<Demo.Contract>");
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

    private static CSharpCompilation CreateMinimalCompilation(string assemblyName, string source)
        => CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview))],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static INamedTypeSymbol GetNamedType(Compilation compilation, string metadataName)
    {
        var symbol = compilation.GetTypeByMetadataName(metadataName);
        Assert.IsNotNull(symbol, metadataName);
        return symbol!;
    }

    private static void AssertPrivateValidationFailure(
        Compilation compilation,
        string contractMetadataName,
        string implementationMetadataName,
        string expectedMessage)
    {
        var contract = GetNamedType(compilation, contractMetadataName);
        var implementation = GetNamedType(compilation, implementationMetadataName);
        var failure = Assert.Throws<TargetInvocationException>(() =>
            InvokePrivate("ValidateRegistration", compilation, contract, implementation));

        Assert.IsInstanceOfType<InvalidOperationException>(failure.InnerException);
        StringAssert.Contains(failure.InnerException.Message, expectedMessage, StringComparison.Ordinal);
    }

    private static object? InvokePrivate(string methodName, params object?[] arguments)
    {
        var method = typeof(VueInjectRegistry)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        return method.Invoke(null, arguments);
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
