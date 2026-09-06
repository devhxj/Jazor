using System.Collections.Immutable;
using System.Reflection;
using Jazor.RazorVue.Generation;
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
    public void TryBuild_AcceptsParameterlessConstructorsOnSourceComponentHierarchy()
    {
        var fixture = CreateExplicitConstructorFixture();

        foreach (var scenario in fixture.Scenarios)
        {
            Assert.IsTrue(MemberClosureBuilder.TryBuild(
                fixture.Binding,
                scenario.Component,
                out var closure,
                out var failure), failure);
            Assert.IsNotNull(closure);
            Assert.IsTrue(closure.InitializationPlan.HasExplicitConstructors);
            Assert.IsTrue(closure.InitializationPlan.Constructors.Any(constructor =>
                SymbolEqualityComparer.Default.Equals(
                    constructor.OriginalDefinition,
                    scenario.Constructor.OriginalDefinition)));

            Assert.IsTrue(MemberClosureBuilder.TryBuildWithDiagnostic(
                fixture.Binding,
                scenario.Component,
                out var diagnosticClosure,
                out var diagnostic), diagnostic?.Message);
            Assert.IsNotNull(diagnosticClosure);
            Assert.IsNull(diagnostic);
        }
    }

    [TestMethod]
    public void TryBuild_RejectsUnsupportedSourceConstructorActivationProtocols()
    {
        var fixture = CreateUnsupportedConstructorFixture();

        foreach (var scenario in fixture.Scenarios)
        {
            Assert.IsFalse(MemberClosureBuilder.TryBuild(
                fixture.Binding,
                scenario.Component,
                out var closure,
                out var failure));
            Assert.IsNull(closure);
            StringAssert.Contains(failure, scenario.ExpectedFailure, StringComparison.Ordinal);

            Assert.IsFalse(MemberClosureBuilder.TryBuildWithDiagnostic(
                fixture.Binding,
                scenario.Component,
                out var diagnosticClosure,
                out var diagnostic));
            Assert.IsNull(diagnosticClosure);
            Assert.IsNotNull(diagnostic);
            Assert.AreEqual(RazorVueDiagnosticCategory.MemberClosure, diagnostic.Category);
            StringAssert.Contains(diagnostic.Message, scenario.ExpectedFailure, StringComparison.Ordinal);
            Assert.AreEqual(
                scenario.FailureSubject.Locations.Single(static location => location.IsInSource).SourceSpan,
                diagnostic.PrimaryLocation.SourceSpan);
        }
    }

    [TestMethod]
    public void TryBuild_AcceptsParameterViewRuntimeEntryPoint()
    {
        var fixture = CreateUnsupportedRuntimeEntryFixture();
        var scenario = fixture.Scenarios.Single(static scenario =>
            string.Equals(scenario.EntryPoint.Name, "SetParametersAsync", StringComparison.Ordinal));

        Assert.IsTrue(MemberClosureBuilder.TryBuild(
            fixture.Binding,
            scenario.Component,
            out var closure,
            out var failure), failure);
        Assert.IsNotNull(closure);
        Assert.IsTrue(closure.UsesParameterViewState);
        Assert.IsTrue(closure.LifecycleRoots.Any(root =>
            SymbolEqualityComparer.Default.Equals(root.OriginalDefinition, scenario.EntryPoint.OriginalDefinition)));

        Assert.IsTrue(MemberClosureBuilder.TryBuildWithDiagnostic(
            fixture.Binding,
            scenario.Component,
            out var diagnosticClosure,
            out var diagnostic));
        Assert.IsNotNull(diagnosticClosure);
        Assert.IsNull(diagnostic);
    }

    [TestMethod]
    public void TryBuild_ResolvesSourceBaseLifecycleAndDisposeEntryPoints()
    {
        var fixture = CreateUnsupportedRuntimeEntryFixture();

        foreach (var scenario in fixture.Scenarios.Where(static scenario => scenario.IsSupported))
        {
            Assert.IsTrue(MemberClosureBuilder.TryBuild(
                fixture.Binding,
                scenario.Component,
                out var closure,
                out var failure), failure);
            Assert.IsNotNull(closure);
            Assert.IsTrue(closure.LifecycleRoots.Any(root =>
                SymbolEqualityComparer.Default.Equals(
                    root.OriginalDefinition,
                    scenario.EntryPoint.OriginalDefinition)));

            Assert.IsTrue(MemberClosureBuilder.TryBuildWithDiagnostic(
                fixture.Binding,
                scenario.Component,
                out var diagnosticClosure,
                out var diagnostic), diagnostic?.Message);
            Assert.IsNotNull(diagnosticClosure);
            Assert.IsNull(diagnostic);
        }
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

    [TestMethod]
    public void PrivateStorageAndHierarchyHelpers_KeepOnlyReplayableInstanceSlots()
    {
        var fixture = CreateFixture();
        var compilation = fixture.Binding.Compilation;
        var component = fixture.Component.ComponentSymbol;
        var componentBase = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase");
        Assert.IsNotNull(componentBase);

        var autoStorage = component.GetMembers("AutoStorage").OfType<IPropertySymbol>().Single();
        var explicitStorage = component.GetMembers("ExplicitStorage").OfType<IPropertySymbol>().Single();
        var parameterStorage = component.GetMembers("ParameterStorage").OfType<IPropertySymbol>().Single();
        var storageMembers = InvokeStatic<IEnumerable<ISymbol>>(
                "GetInstanceStorageMembers",
                component)
            .Select(static member => member.Name)
            .ToArray();

        CollectionAssert.Contains(storageMembers, "AutoStorage");
        CollectionAssert.DoesNotContain(storageMembers, "ExplicitStorage");
        CollectionAssert.DoesNotContain(storageMembers, "ParameterStorage");
        CollectionAssert.DoesNotContain(storageMembers, "StaticStorage");
        CollectionAssert.DoesNotContain(storageMembers, "ConstantStorage");
        Assert.IsTrue(InvokeStatic<bool>("IsAutoProperty", autoStorage));
        Assert.IsFalse(InvokeStatic<bool>("IsAutoProperty", explicitStorage));
        Assert.IsTrue(InvokeStatic<bool>("IsParameterProperty", parameterStorage));

        CollectionAssert.AreEqual(
            new[] { "ContractComponent" },
            InvokeStatic<IEnumerable<INamedTypeSymbol>>(
                    "GetSourceComponentHierarchy",
                    component)
                .Select(static type => type.Name)
                .ToArray());
        Assert.IsEmpty(InvokeStatic<IEnumerable<INamedTypeSymbol>>(
            "GetSourceComponentHierarchy",
            componentBase!));
        Assert.IsNull(InvokeStatic<IMethodSymbol?>(
            "FindEffectiveLifecycleOverride",
            component,
            null,
            "OnInitialized"));
        Assert.IsNull(InvokeStatic<IMethodSymbol?>(
            "FindEffectiveLifecycleOverride",
            fixture.ForeignComponent,
            componentBase,
            "OnInitialized"));

        var disposable = compilation.GetTypeByMetadataName("System.IDisposable");
        Assert.IsNotNull(disposable);
        var disposeMethod = disposable!.GetMembers("Dispose").OfType<IMethodSymbol>().Single();
        Assert.IsNull(InvokeStatic<IMethodSymbol?>(
            "FindEffectiveInterfaceImplementation",
            fixture.ForeignComponent,
            disposeMethod));
    }

    [TestMethod]
    public void PrivateLifecycleHelpers_ExcludeMetadataStaticAndHiddenRuntimeShapes()
    {
        var fixture = CreateFixture();
        var compilation = fixture.Binding.Compilation;
        var component = fixture.Component.ComponentSymbol;
        var hiddenLifecycleComponent = compilation.GetTypeByMetadataName("ClosureContract.HiddenLifecycleComponent");
        var componentBase = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase");
        var disposable = compilation.GetTypeByMetadataName("System.IDisposable");
        var asyncDisposable = compilation.GetTypeByMetadataName("System.IAsyncDisposable");
        Assert.IsNotNull(hiddenLifecycleComponent);
        Assert.IsNotNull(componentBase);
        Assert.IsNotNull(disposable);
        Assert.IsNotNull(asyncDisposable);

        var hiddenInitialized = hiddenLifecycleComponent!.GetMembers("OnInitialized")
            .OfType<IMethodSymbol>()
            .Single();
        var staticInitialized = component.GetMembers("OnInitialized")
            .OfType<IMethodSymbol>()
            .Single(static method => method.IsStatic);
        var dispose = component.GetMembers("Dispose")
            .OfType<IMethodSymbol>()
            .Single(static method => method.Parameters.Length == 0);
        var disposeOverload = component.GetMembers("Dispose")
            .OfType<IMethodSymbol>()
            .Single(static method => method.Parameters.Length == 1);
        var interfaceDispose = disposable!.GetMembers("Dispose").OfType<IMethodSymbol>().Single();
        var metadataLength = compilation.GetSpecialType(SpecialType.System_String)
            .GetMembers("Length")
            .OfType<IPropertySymbol>()
            .Single();

        Assert.IsFalse(InvokeStatic<bool>("IsAutoProperty", metadataLength));
        Assert.IsFalse(InvokeStatic<bool>("OverridesComponentBase", staticInitialized, null));
        Assert.IsFalse(InvokeStatic<bool>("IsEffectiveLifecycleOverride", hiddenInitialized, componentBase));
        Assert.IsFalse(InvokeStatic<bool>(
            "IsDisposeRoot",
            component,
            staticInitialized,
            disposable,
            asyncDisposable));
        Assert.IsFalse(InvokeStatic<bool>(
            "IsDisposeEntryPoint",
            staticInitialized,
            disposable,
            asyncDisposable));
        Assert.IsFalse(InvokeStatic<bool>(
            "IsDisposeEntryPoint",
            disposeOverload,
            disposable,
            asyncDisposable));
        Assert.IsTrue(InvokeStatic<bool>(
            "IsEffectiveInterfaceImplementation",
            dispose,
            interfaceDispose,
            dispose));
        Assert.IsFalse(InvokeStatic<bool>(
            "IsEffectiveInterfaceImplementation",
            staticInitialized,
            interfaceDispose,
            dispose));
        Assert.IsFalse(InvokeStatic<bool>(
            "IsEffectiveInterfaceImplementation",
            disposeOverload,
            interfaceDispose,
            dispose));
        Assert.IsFalse(InvokeStatic<bool>(
            "IsDeclaredOnSourceComponentHierarchy",
            component,
            null));
    }

    [TestMethod]
    public void LifecycleRoots_UseTheMostDerivedSourceLifecycleAndDisposeImplementation()
    {
        var fixture = CreateFixture();
        var compilation = fixture.Binding.Compilation;
        var component = compilation.GetTypeByMetadataName("ClosureContract.DerivedVirtualDisposeComponent");
        Assert.IsNotNull(component);

        var roots = InvokeStatic<ImmutableArray<IMethodSymbol>>(
            "GetSupportedLifecycleRoots",
            compilation,
            component!);
        var sourceRoots = roots
            .Select(static method => method.ContainingType!.Name + "." + method.Name)
            .ToArray();

        CollectionAssert.Contains(sourceRoots, "DerivedVirtualDisposeComponent.OnInitialized");
        CollectionAssert.Contains(sourceRoots, "DerivedVirtualDisposeComponent.Dispose");
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

                public int AutoStorage { get; set; }

                public int ExplicitStorage
                {
                    get
                    {
                        return 1;
                    }
                    set
                    {
                    }
                }

                [Parameter]
                public int ParameterStorage { get; set; }

                public static int StaticStorage;
                public const int ConstantStorage = 1;
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

            public abstract class VirtualDisposeComponentBase : ComponentBase, IDisposable
            {
                protected override void OnInitialized()
                {
                }

                public virtual void Dispose()
                {
                }
            }

            public sealed class DerivedVirtualDisposeComponent : VirtualDisposeComponentBase
            {
                protected override void OnInitialized()
                {
                }

                public override void Dispose()
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

    private static ExplicitConstructorFixture CreateExplicitConstructorFixture()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using static ECMAScript.Vue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace ConstructorContract;

            [ECMAScriptModule("./components/direct-constructor")]
            public sealed class DirectConstructorComponent : ComponentBase, IVueComponent
            {
                public DirectConstructorComponent()
                {
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "direct");
                }
            }

            public abstract class ConstructorComponentBase : ComponentBase
            {
                protected ConstructorComponentBase()
                {
                }
            }

            [ECMAScriptModule("./components/inherited-constructor")]
            public sealed class InheritedConstructorComponent : ConstructorComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "inherited");
                }
            }

            [ECMAScriptModule("./components/explicit-base-constructor")]
            public sealed class ExplicitBaseConstructorComponent : ConstructorComponentBase, IVueComponent
            {
                public ExplicitBaseConstructorComponent()
                    : base()
                {
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "explicit-base");
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "ConstructorContract.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.MemberClosure.Constructor.Tests",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var directType = compilation.GetTypeByMetadataName("ConstructorContract.DirectConstructorComponent");
        var inheritedType = compilation.GetTypeByMetadataName("ConstructorContract.InheritedConstructorComponent");
        var explicitBaseType = compilation.GetTypeByMetadataName("ConstructorContract.ExplicitBaseConstructorComponent");
        var baseType = compilation.GetTypeByMetadataName("ConstructorContract.ConstructorComponentBase");
        Assert.IsNotNull(directType);
        Assert.IsNotNull(inheritedType);
        Assert.IsNotNull(explicitBaseType);
        Assert.IsNotNull(baseType);
        Assert.IsTrue(
            GeneratedCSharpBinder.TryBindFinalCompilation(
                compilation,
                ImmutableArray.Create(directType!, inheritedType!, explicitBaseType!),
                out var binding,
                out var bindingFailure),
            bindingFailure);
        Assert.IsNotNull(binding);

        var components = binding!.Components.ToDictionary(
            static component => component.ComponentSymbol.Name,
            StringComparer.Ordinal);
        var directConstructor = directType!.InstanceConstructors.Single(static constructor => !constructor.IsImplicitlyDeclared);
        var explicitBaseConstructor = explicitBaseType!.InstanceConstructors.Single(static constructor => !constructor.IsImplicitlyDeclared);
        var baseConstructor = baseType!.InstanceConstructors.Single(static constructor => !constructor.IsImplicitlyDeclared);
        return new ExplicitConstructorFixture(
            binding,
            [
                new ExplicitConstructorScenario(components[directType.Name], directConstructor),
                new ExplicitConstructorScenario(components[inheritedType!.Name], baseConstructor),
                new ExplicitConstructorScenario(components[explicitBaseType.Name], explicitBaseConstructor)
            ]);
    }

    private static UnsupportedConstructorFixture CreateUnsupportedConstructorFixture()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using static ECMAScript.Vue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace UnsupportedConstructorContract;

            [ECMAScriptModule("./components/primary-constructor")]
            public sealed class PrimaryConstructorComponent(int initialValue) : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, initialValue);
                }
            }

            [ECMAScriptModule("./components/parameterized-constructor")]
            public sealed class ParameterizedConstructorComponent : ComponentBase, IVueComponent
            {
                public ParameterizedConstructorComponent(int initialValue)
                {
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "parameterized");
                }
            }

            [ECMAScriptModule("./components/this-constructor")]
            public sealed class ThisConstructorComponent : ComponentBase, IVueComponent
            {
                public ThisConstructorComponent() : this(1)
                {
                }

                private ThisConstructorComponent(int initialValue)
                {
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "this");
                }
            }

            public abstract class ArgumentConstructorBase : ComponentBase
            {
                protected ArgumentConstructorBase()
                {
                }

                protected ArgumentConstructorBase(int initialValue)
                {
                }
            }

            [ECMAScriptModule("./components/base-constructor")]
            public sealed class BaseConstructorComponent : ArgumentConstructorBase, IVueComponent
            {
                public BaseConstructorComponent() : base(1)
                {
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "base");
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "UnsupportedConstructorContract.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "JazorVue.MemberClosure.UnsupportedConstructor.Tests",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var primary = compilation.GetTypeByMetadataName("UnsupportedConstructorContract.PrimaryConstructorComponent");
        var parameterized = compilation.GetTypeByMetadataName("UnsupportedConstructorContract.ParameterizedConstructorComponent");
        var thisChained = compilation.GetTypeByMetadataName("UnsupportedConstructorContract.ThisConstructorComponent");
        var baseArguments = compilation.GetTypeByMetadataName("UnsupportedConstructorContract.BaseConstructorComponent");
        Assert.IsNotNull(primary);
        Assert.IsNotNull(parameterized);
        Assert.IsNotNull(thisChained);
        Assert.IsNotNull(baseArguments);
        Assert.IsTrue(
            GeneratedCSharpBinder.TryBindFinalCompilation(
                compilation,
                ImmutableArray.Create(primary!, parameterized!, thisChained!, baseArguments!),
                out var binding,
                out var bindingFailure),
            bindingFailure);
        Assert.IsNotNull(binding);

        var components = binding!.Components.ToDictionary(
            static component => component.ComponentSymbol.Name,
            StringComparer.Ordinal);
        return new UnsupportedConstructorFixture(
            binding,
            [
                new UnsupportedConstructorScenario(
                    components[primary!.Name],
                    primary,
                    "cannot supply source component primary-constructor parameters"),
                new UnsupportedConstructorScenario(
                    components[parameterized!.Name],
                    parameterized.InstanceConstructors.Single(static constructor => !constructor.IsImplicitlyDeclared),
                    "only reference-type service parameters resolved from Vue providers are supported"),
                new UnsupportedConstructorScenario(
                    components[thisChained!.Name],
                    thisChained.InstanceConstructors.Single(static constructor =>
                        !constructor.IsImplicitlyDeclared && constructor.Parameters.Length == 0),
                    "does not yet simulate this(...) component constructor chaining"),
                new UnsupportedConstructorScenario(
                    components[baseArguments!.Name],
                    baseArguments.InstanceConstructors.Single(static constructor =>
                        !constructor.IsImplicitlyDeclared && constructor.Parameters.Length == 0),
                    "base(...) arguments require a component activation protocol")
            ]);
    }

    private static UnsupportedRuntimeEntryFixture CreateUnsupportedRuntimeEntryFixture()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript;
            using static ECMAScript.Vue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace RuntimeEntryContract;

            [ECMAScriptModule("./components/set-parameters")]
            public sealed class SetParametersComponent : ComponentBase, IVueComponent
            {
                public override Task SetParametersAsync(ParameterView parameters) => Task.CompletedTask;

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "parameters");
                }
            }

            public abstract class SourceLifecycleBase : ComponentBase
            {
                protected override void OnInitialized()
                {
                }
            }

            [ECMAScriptModule("./components/inherited-lifecycle")]
            public sealed class InheritedLifecycleComponent : SourceLifecycleBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "lifecycle");
                }
            }

            [ECMAScriptModule("./components/indirect-lifecycle")]
            public sealed class IndirectLifecycleComponent : SourceLifecycleBase, IVueComponent
            {
                protected override void OnInitialized()
                {
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "indirect lifecycle");
                }
            }

            public abstract class SourceDisposeBase : ComponentBase, IDisposable
            {
                public void Dispose()
                {
                }
            }

            [ECMAScriptModule("./components/inherited-dispose")]
            public sealed class InheritedDisposeComponent : SourceDisposeBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "dispose");
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "RuntimeEntryContract.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.MemberClosure.RuntimeEntry.Tests",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var setParametersType = compilation.GetTypeByMetadataName("RuntimeEntryContract.SetParametersComponent");
        var inheritedLifecycleType = compilation.GetTypeByMetadataName("RuntimeEntryContract.InheritedLifecycleComponent");
        var indirectLifecycleType = compilation.GetTypeByMetadataName("RuntimeEntryContract.IndirectLifecycleComponent");
        var lifecycleBaseType = compilation.GetTypeByMetadataName("RuntimeEntryContract.SourceLifecycleBase");
        var inheritedDisposeType = compilation.GetTypeByMetadataName("RuntimeEntryContract.InheritedDisposeComponent");
        var disposeBaseType = compilation.GetTypeByMetadataName("RuntimeEntryContract.SourceDisposeBase");
        Assert.IsNotNull(setParametersType);
        Assert.IsNotNull(inheritedLifecycleType);
        Assert.IsNotNull(indirectLifecycleType);
        Assert.IsNotNull(lifecycleBaseType);
        Assert.IsNotNull(inheritedDisposeType);
        Assert.IsNotNull(disposeBaseType);
        Assert.IsTrue(
            GeneratedCSharpBinder.TryBindFinalCompilation(
                compilation,
                ImmutableArray.Create(
                    setParametersType!,
                    inheritedLifecycleType!,
                    indirectLifecycleType!,
                    inheritedDisposeType!),
                out var binding,
                out var bindingFailure),
            bindingFailure);
        Assert.IsNotNull(binding);

        var components = binding!.Components.ToDictionary(
            static component => component.ComponentSymbol.Name,
            StringComparer.Ordinal);
        var setParameters = setParametersType!.GetMembers("SetParametersAsync").OfType<IMethodSymbol>().Single();
        var inheritedLifecycle = lifecycleBaseType!.GetMembers("OnInitialized").OfType<IMethodSymbol>().Single();
        var indirectLifecycle = indirectLifecycleType!.GetMembers("OnInitialized").OfType<IMethodSymbol>().Single();
        var inheritedDispose = disposeBaseType!.GetMembers("Dispose").OfType<IMethodSymbol>().Single();
        return new UnsupportedRuntimeEntryFixture(
            binding,
            [
                new UnsupportedRuntimeEntryScenario(
                    components[setParametersType.Name],
                    setParameters,
                    IsSupported: true,
                    ExpectedFailure: null),
                new UnsupportedRuntimeEntryScenario(
                    components[inheritedLifecycleType!.Name],
                    inheritedLifecycle,
                    IsSupported: true,
                    ExpectedFailure: null),
                new UnsupportedRuntimeEntryScenario(
                    components[indirectLifecycleType.Name],
                    indirectLifecycle,
                    IsSupported: true,
                    ExpectedFailure: null),
                new UnsupportedRuntimeEntryScenario(
                    components[inheritedDisposeType!.Name],
                    inheritedDispose,
                    IsSupported: true,
                    ExpectedFailure: null)
            ]);
    }

    private sealed record Fixture(
        GeneratedCSharpBinding Binding,
        BoundComponent Component,
        INamedTypeSymbol ExplicitDisposeComponent,
        INamedTypeSymbol ForeignComponent,
        IMethodSymbol NonRenderTreeMethod);

    private sealed record ExplicitConstructorFixture(
        GeneratedCSharpBinding Binding,
        ImmutableArray<ExplicitConstructorScenario> Scenarios);

    private sealed record ExplicitConstructorScenario(
        BoundComponent Component,
        IMethodSymbol Constructor);

    private sealed record UnsupportedConstructorFixture(
        GeneratedCSharpBinding Binding,
        ImmutableArray<UnsupportedConstructorScenario> Scenarios);

    private sealed record UnsupportedConstructorScenario(
        BoundComponent Component,
        ISymbol FailureSubject,
        string ExpectedFailure);

    private sealed record UnsupportedRuntimeEntryFixture(
        GeneratedCSharpBinding Binding,
        ImmutableArray<UnsupportedRuntimeEntryScenario> Scenarios);

    private sealed record UnsupportedRuntimeEntryScenario(
        BoundComponent Component,
        IMethodSymbol EntryPoint,
        bool IsSupported,
        string? ExpectedFailure);
}
