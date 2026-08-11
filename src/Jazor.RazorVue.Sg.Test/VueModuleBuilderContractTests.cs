using System.Collections.Immutable;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class VueModuleBuilderContractTests
{
    [TestMethod]
    public async Task BuildAsync_RejectsInvalidContractsBeforeLowering()
    {
        var fixture = CreateFixture();

        var nullBinding = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await VueModuleBuilder.BuildAsync(null!, fixture.Component, fixture.Closure));
        Assert.AreEqual("binding", nullBinding.ParamName);

        var nullComponent = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await VueModuleBuilder.BuildAsync(fixture.Binding, null!, fixture.Closure));
        Assert.AreEqual("component", nullComponent.ParamName);

        var nullClosure = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await VueModuleBuilder.BuildAsync(fixture.Binding, fixture.Component, null!));
        Assert.AreEqual("closure", nullClosure.ParamName);

        var foreignClosure = fixture.Closure with { ComponentSymbol = fixture.ForeignComponent };
        var mismatch = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await VueModuleBuilder.BuildAsync(fixture.Binding, fixture.Component, foreignClosure));
        Assert.AreEqual("closure", mismatch.ParamName);
        StringAssert.Contains(mismatch.Message, "does not belong", StringComparison.Ordinal);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await VueModuleBuilder.BuildAsync(
                fixture.Binding,
                fixture.Component,
                fixture.Closure,
                cancellation.Token));
    }

    private static Fixture CreateFixture()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using static ECMAScript.Vue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace BuilderContract;

            [ECMAScriptModule("./components/contract")]
            public sealed class ContractComponent : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "contract");
                }
            }

            internal sealed class ForeignComponent
            {
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "BuilderContract.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.ModuleBuilder.Contract.Tests",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));

        var componentSymbol = compilation.GetTypeByMetadataName("BuilderContract.ContractComponent");
        var foreignComponent = compilation.GetTypeByMetadataName("BuilderContract.ForeignComponent");
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

        var component = binding!.Components.Single();
        Assert.IsTrue(
            MemberClosureBuilder.TryBuild(binding, component, out var closure, out var closureFailure),
            closureFailure);
        Assert.IsNotNull(closure);

        return new Fixture(binding, component, closure!, foreignComponent!);
    }

    private sealed record Fixture(
        GeneratedCSharpBinding Binding,
        BoundComponent Component,
        MemberClosure Closure,
        INamedTypeSymbol ForeignComponent);
}
