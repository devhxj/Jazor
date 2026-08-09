using System.Collections.Immutable;
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

    private static Fixture CreateFixture()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript;
            using static ECMAScript.Vue3;
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
        var foreignComponent = compilation.GetTypeByMetadataName("ClosureContract.ForeignComponent");
        Assert.IsNotNull(componentSymbol);
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
        return new Fixture(binding!, binding.Components.Single(), foreignComponent!, nonRenderTreeMethod);
    }

    private sealed record Fixture(
        GeneratedCSharpBinding Binding,
        BoundComponent Component,
        INamedTypeSymbol ForeignComponent,
        IMethodSymbol NonRenderTreeMethod);
}
