using System.Collections.Immutable;
using System.Reflection;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class MemberClosureBuilderContractTests
{
    [TestMethod]
    public void TryBuild_RejectsNullForeignAndNonRenderTreeRoots()
    {
        var fixture = CreateFixture();

        var nullBinding = Assert.Throws<ArgumentNullException>(() =>
            MemberClosureBuilder.TryBuild(null!, fixture.Component, out _, out _));
        Assert.AreEqual("binding", nullBinding.ParamName);

        var nullComponent = Assert.Throws<ArgumentNullException>(() =>
            MemberClosureBuilder.TryBuild(fixture.Binding, null!, out _, out _));
        Assert.AreEqual("component", nullComponent.ParamName);

        var foreignComponent = fixture.Component with { ComponentSymbol = fixture.ForeignComponent };
        Assert.IsFalse(MemberClosureBuilder.TryBuild(
            fixture.Binding,
            foreignComponent,
            out var foreignClosure,
            out var foreignFailure));
        Assert.IsNull(foreignClosure);
        StringAssert.Contains(foreignFailure, "not present", StringComparison.Ordinal);

        var invalidRoot = fixture.Component with { BuildRenderTreeMethod = fixture.NonRenderTreeMethod };
        Assert.IsFalse(MemberClosureBuilder.TryBuild(
            fixture.Binding,
            invalidRoot,
            out var invalidClosure,
            out var invalidFailure));
        Assert.IsNull(invalidClosure);
        StringAssert.Contains(invalidFailure, "not BuildRenderTree(RenderTreeBuilder)", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryBuildWithDiagnostic_PreservesSuccessfulAndRejectedClosureContracts()
    {
        var fixture = CreateFixture();

        Assert.IsTrue(MemberClosureBuilder.TryBuildWithDiagnostic(
            fixture.Binding,
            fixture.Component,
            out var validClosure,
            out var validDiagnostic));
        Assert.IsNotNull(validClosure);
        Assert.IsNull(validDiagnostic);

        var invalidRoot = fixture.Component with { BuildRenderTreeMethod = fixture.NonRenderTreeMethod };
        Assert.IsFalse(MemberClosureBuilder.TryBuildWithDiagnostic(
            fixture.Binding,
            invalidRoot,
            out var invalidClosure,
            out var invalidDiagnostic));
        Assert.IsNull(invalidClosure);
        Assert.IsNotNull(invalidDiagnostic);
        StringAssert.Contains(
            invalidDiagnostic.Message,
            "not BuildRenderTree(RenderTreeBuilder)",
            StringComparison.Ordinal);
        Assert.AreEqual(
            fixture.NonRenderTreeMethod.Locations.Single(static location => location.IsInSource).SourceSpan,
            invalidDiagnostic.PrimaryLocation.SourceSpan);
    }

    [TestMethod]
    public void TryBuild_CollectsSupportedLifecycleAndDisposeRootsWithoutUnrelatedOverloads()
    {
        var fixture = CreateFixture();

        Assert.IsTrue(MemberClosureBuilder.TryBuild(
            fixture.Binding,
            fixture.Component,
            out var closure,
            out var failure), failure);
        Assert.IsNotNull(closure);

        var roots = closure!.LifecycleRoots;
        CollectionAssert.AreEquivalent(
            new[]
            {
                "OnInitialized",
                "OnInitializedAsync",
                "OnParametersSet",
                "OnParametersSetAsync",
                "OnAfterRender",
                "OnAfterRenderAsync",
                "ShouldRender",
                "Dispose",
                "DisposeAsync"
            },
            roots.Select(static method => method.Name).ToArray());
        Assert.IsFalse(roots.Any(static method => method.IsStatic));
        Assert.IsFalse(roots.Any(static method => method.Parameters.Length != 0 && method.Name is "Dispose" or "OnInitialized"));
        Assert.IsTrue(roots.Any(static method => method.Name == "Dispose" && method.MethodKind == MethodKind.Ordinary));
        Assert.IsTrue(roots.Any(static method => method.Name == "DisposeAsync" && method.MethodKind == MethodKind.Ordinary));
    }

    [TestMethod]
    public void PrivateLifecycleHelpers_ClassifySourceAndRuntimeSymbols()
    {
        var fixture = CreateFixture();
        var compilation = fixture.Binding.Compilation;
        var component = fixture.Component.ComponentSymbol;
        var hiddenLifecycleComponent = compilation.GetTypeByMetadataName("ClosureContract.HiddenLifecycleComponent");
        var indirectLifecycleComponent = compilation.GetTypeByMetadataName("ClosureContract.IndirectLifecycleComponent");
        var componentBase = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase");
        var disposable = compilation.GetTypeByMetadataName("System.IDisposable");
        var asyncDisposable = compilation.GetTypeByMetadataName("System.IAsyncDisposable");
        var task = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var taskOfT = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        var valueTask = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
        var valueTaskOfT = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");
        Assert.IsNotNull(componentBase);
        Assert.IsNotNull(hiddenLifecycleComponent);
        Assert.IsNotNull(indirectLifecycleComponent);
        Assert.IsNotNull(disposable);
        Assert.IsNotNull(asyncDisposable);
        Assert.IsNotNull(task);
        Assert.IsNotNull(taskOfT);
        Assert.IsNotNull(valueTask);
        Assert.IsNotNull(valueTaskOfT);

        var initialized = component.GetMembers("OnInitialized")
            .OfType<IMethodSymbol>()
            .Single(method => method.IsOverride);
        var hiddenInitialized = hiddenLifecycleComponent!.GetMembers("OnInitialized")
            .OfType<IMethodSymbol>()
            .Single(method => !method.IsStatic && !method.IsOverride && method.Parameters.Length == 0);
        var indirectInitialized = indirectLifecycleComponent!.GetMembers("OnInitialized")
            .OfType<IMethodSymbol>()
            .Single(method => method.IsOverride);
        var staticInitialized = component.GetMembers("OnInitialized")
            .OfType<IMethodSymbol>()
            .Single(method => method.IsStatic);
        var dispose = component.GetMembers("Dispose")
            .OfType<IMethodSymbol>()
            .Single(method => method.Parameters.Length == 0);
        var disposeAsync = component.GetMembers("DisposeAsync")
            .OfType<IMethodSymbol>()
            .Single(method => method.Parameters.Length == 0);
        var disposeOverload = component.GetMembers("Dispose")
            .OfType<IMethodSymbol>()
            .Single(method => method.Parameters.Length == 1);
        var statusGetter = component.GetMembers("Status")
            .OfType<IPropertySymbol>()
            .Single()
            .GetMethod;
        Assert.IsNotNull(statusGetter);

        Assert.IsTrue(InvokeStatic<bool>("IsSupportedLifecycleMethod", component, initialized, componentBase));
        Assert.IsTrue(InvokeStatic<bool>("IsSupportedLifecycleMethod", component, initialized, null));
        Assert.IsFalse(InvokeStatic<bool>("IsSupportedLifecycleMethod", hiddenLifecycleComponent, hiddenInitialized, componentBase));
        Assert.IsFalse(InvokeStatic<bool>("IsSupportedLifecycleMethod", indirectLifecycleComponent, indirectInitialized, componentBase));
        Assert.IsFalse(InvokeStatic<bool>("IsSupportedLifecycleMethod", component, staticInitialized, componentBase));
        Assert.IsFalse(InvokeStatic<bool>("IsSupportedLifecycleMethod", component, fixture.NonRenderTreeMethod, componentBase));
        var inheritedInitialized = componentBase!.GetMembers("OnInitialized").OfType<IMethodSymbol>().Single();
        Assert.IsFalse(InvokeStatic<bool>("IsSupportedLifecycleMethod", component, inheritedInitialized, componentBase));

        Assert.IsTrue(InvokeStatic<bool>("IsDisposeRoot", component, dispose, disposable, asyncDisposable));
        Assert.IsTrue(InvokeStatic<bool>("IsDisposeRoot", component, disposeAsync, disposable, asyncDisposable));
        Assert.IsFalse(InvokeStatic<bool>("IsDisposeRoot", component, fixture.NonRenderTreeMethod, disposable, asyncDisposable));
        Assert.IsFalse(InvokeStatic<bool>("IsDisposeRoot", component, disposeOverload, disposable, asyncDisposable));
        Assert.IsFalse(InvokeStatic<bool>("IsDisposeRoot", component, statusGetter!, disposable, asyncDisposable));
        var interfaceDispose = disposable!.GetMembers("Dispose").OfType<IMethodSymbol>().Single();
        Assert.IsFalse(InvokeStatic<bool>("IsDisposeRoot", component, interfaceDispose, disposable, asyncDisposable));
        Assert.IsTrue(InvokeStatic<bool>("ImplementsInterface", component, disposable));
        Assert.IsFalse(InvokeStatic<bool>("ImplementsInterface", component, componentBase));
        Assert.IsFalse(InvokeStatic<bool>("ImplementsInterface", component, null));

        var intType = compilation.GetSpecialType(SpecialType.System_Int32);
        Assert.IsTrue(InvokeStatic<bool>("IsAsyncDisposeReturnType", task));
        Assert.IsTrue(InvokeStatic<bool>("IsAsyncDisposeReturnType", taskOfT!.Construct(intType)));
        Assert.IsTrue(InvokeStatic<bool>("IsAsyncDisposeReturnType", valueTask));
        Assert.IsTrue(InvokeStatic<bool>("IsAsyncDisposeReturnType", valueTaskOfT!.Construct(intType)));
        Assert.IsFalse(InvokeStatic<bool>("IsAsyncDisposeReturnType", compilation.GetSpecialType(SpecialType.System_String)));

        var explicitDispose = fixture.ExplicitDisposeComponent.GetMembers()
            .OfType<IMethodSymbol>()
            .Single(static method =>
                method.MethodKind == MethodKind.ExplicitInterfaceImplementation &&
                method.ExplicitInterfaceImplementations.Any(static implementation => implementation.Name == "Dispose"));
        var explicitDisposeAsync = fixture.ExplicitDisposeComponent.GetMembers()
            .OfType<IMethodSymbol>()
            .Single(static method =>
                method.MethodKind == MethodKind.ExplicitInterfaceImplementation &&
                method.ExplicitInterfaceImplementations.Any(static implementation => implementation.Name == "DisposeAsync"));
        Assert.IsTrue(InvokeStatic<bool>(
            "IsDisposeRoot",
            fixture.ExplicitDisposeComponent,
            explicitDispose,
            disposable,
            asyncDisposable));
        Assert.IsTrue(InvokeStatic<bool>(
            "IsDisposeRoot",
            fixture.ExplicitDisposeComponent,
            explicitDisposeAsync,
            disposable,
            asyncDisposable));
        Assert.IsTrue(InvokeStatic<bool>(
            "IsDisposeRoot",
            fixture.ExplicitDisposeComponent,
            explicitDisposeAsync,
            null,
            asyncDisposable));

        StringAssert.Contains(
            InvokeStatic<string>("GetStableMemberKey", initialized),
            "ClosureContract.razor.g.cs",
            StringComparison.Ordinal);
        Assert.IsTrue(InvokeStatic<string>("GetStableMemberKey", compilation.GetSpecialType(SpecialType.System_String))
            .StartsWith("~|", StringComparison.Ordinal));
    }

    private static T InvokeStatic<T>(string methodName, params object?[] arguments)
    {
        var method = typeof(MemberClosureBuilder)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        return (T)method.Invoke(null, arguments)!;
    }

    private static Fixture CreateFixture()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript;
            using static ECMAScript.Vue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ClosureContract;

            [ECMAScriptModule("./components/contract")]
            public sealed class ContractComponent : ComponentBase, IVueComponent, IDisposable, IAsyncDisposable
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "contract");
                }

                public void BuildOther()
                {
                }

                protected override void OnInitialized()
                {
                }

                protected override Task OnInitializedAsync() => Task.CompletedTask;

                protected override void OnParametersSet()
                {
                }

                protected override Task OnParametersSetAsync() => Task.CompletedTask;

                protected override void OnAfterRender(bool firstRender)
                {
                }

                protected override Task OnAfterRenderAsync(bool firstRender) => Task.CompletedTask;

                protected override bool ShouldRender() => true;

                public void Dispose()
                {
                }

                public ValueTask DisposeAsync() => ValueTask.CompletedTask;

                private static void OnInitialized(string marker)
                {
                }

                private void Dispose(int marker)
                {
                }

                public string Status => "ready";
            }

            public sealed class ExplicitDisposeComponent : ComponentBase, IDisposable, IAsyncDisposable
            {
                void IDisposable.Dispose()
                {
                }

                ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;
            }

            public sealed class HiddenLifecycleComponent : ComponentBase
            {
                public new void OnInitialized()
                {
                }
            }

            public abstract class IntermediateLifecycleComponentBase : ComponentBase
            {
                protected override void OnInitialized()
                {
                }
            }

            public sealed class IndirectLifecycleComponent : IntermediateLifecycleComponentBase
            {
                protected override void OnInitialized()
                {
                }
            }

            internal sealed class ForeignComponent
            {
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "ClosureContract.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.MemberClosure.Contract.Tests",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var componentSymbol = compilation.GetTypeByMetadataName("ClosureContract.ContractComponent");
        var explicitDisposeComponent = compilation.GetTypeByMetadataName("ClosureContract.ExplicitDisposeComponent");
        var foreignComponent = compilation.GetTypeByMetadataName("ClosureContract.ForeignComponent");
        Assert.IsNotNull(componentSymbol);
        Assert.IsNotNull(explicitDisposeComponent);
        Assert.IsNotNull(foreignComponent);
        Assert.IsTrue(
            GeneratedCSharpBinder.TryBindFinalCompilation(
                compilation,
                ImmutableArray.Create(componentSymbol!),
                out var binding,
                out var bindingFailure),
            bindingFailure);
        Assert.IsNotNull(binding);

        var nonRenderTreeMethod = componentSymbol!.GetMembers("BuildOther").OfType<IMethodSymbol>().Single();
        return new Fixture(binding!, binding.Components.Single(), explicitDisposeComponent!, foreignComponent!, nonRenderTreeMethod);
    }

    private sealed record Fixture(
        GeneratedCSharpBinding Binding,
        BoundComponent Component,
        INamedTypeSymbol ExplicitDisposeComponent,
        INamedTypeSymbol ForeignComponent,
        IMethodSymbol NonRenderTreeMethod);
}
