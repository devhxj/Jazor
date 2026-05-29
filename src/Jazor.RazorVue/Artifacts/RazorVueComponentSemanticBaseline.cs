using System.Collections.Immutable;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Artifacts;

/// <summary>
/// Roslyn-backed semantic baseline for one RazorVue component after all partial
/// declarations have been merged into a single component symbol.
/// </summary>
internal sealed record RazorVueComponentSemanticBaseline(
    Compilation Compilation,
    INamedTypeSymbol ComponentSymbol,
    RazorVueComponentDescriptorSemantics Descriptor,
    RazorVueComponentRuntimeSemantics Runtime,
    RazorVueComponentRenderSemantics Render,
    ImmutableArray<RazorVueSourceOrigin> Origins,
    ImmutableArray<string> ImportedNamespaces)
{
    public static RazorVueComponentSemanticBaseline FromSnapshot(RazorVueSemanticSnapshot snapshot)
    {
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        return new RazorVueComponentSemanticBaseline(
            snapshot.Compilation,
            snapshot.ComponentSymbol,
            new RazorVueComponentDescriptorSemantics(snapshot.Descriptor),
            new RazorVueComponentRuntimeSemantics(
                snapshot.Lifecycle,
                snapshot.Logic,
                snapshot.OnInitializedMethod,
                snapshot.OnInitializedAsyncMethod,
                snapshot.OnParametersSetMethod,
                snapshot.OnParametersSetAsyncMethod,
                snapshot.ShouldRenderMethod,
                snapshot.SetParametersAsyncMethod,
                snapshot.OnAfterRenderMethod,
                snapshot.OnAfterRenderAsyncMethod,
                snapshot.DisposeMethod,
                snapshot.DisposeAsyncMethod),
            new RazorVueComponentRenderSemantics(
                snapshot.BuildRenderTreeMethod,
                snapshot.RazorIrCarrier,
                snapshot.RazorSourceGeneratorDocument),
            Normalize(snapshot.Origins),
            Normalize(snapshot.ImportedNamespaces));
    }

    private static ImmutableArray<T> Normalize<T>(ImmutableArray<T> values)
        => values.IsDefault ? ImmutableArray<T>.Empty : values;
}

internal sealed record RazorVueComponentDescriptorSemantics(
    VueComponentDescriptor Descriptor);

internal sealed record RazorVueComponentRuntimeSemantics(
    VueLifecycleDescriptor Lifecycle,
    VueLogicDescriptor Logic,
    IMethodSymbol? OnInitializedMethod,
    IMethodSymbol? OnInitializedAsyncMethod,
    IMethodSymbol? OnParametersSetMethod,
    IMethodSymbol? OnParametersSetAsyncMethod,
    IMethodSymbol? ShouldRenderMethod,
    IMethodSymbol? SetParametersAsyncMethod,
    IMethodSymbol? OnAfterRenderMethod,
    IMethodSymbol? OnAfterRenderAsyncMethod,
    IMethodSymbol? DisposeMethod,
    IMethodSymbol? DisposeAsyncMethod);

internal sealed record RazorVueComponentRenderSemantics(
    IMethodSymbol? BuildRenderTreeMethod,
    RazorVueRazorIrCarrier? RazorIrCarrier,
    RazorVueRazorSourceGeneratorDocument? RazorSourceGeneratorDocument);
