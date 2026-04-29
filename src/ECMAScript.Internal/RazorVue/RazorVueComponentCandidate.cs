using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue;

public sealed record RazorVueComponentCandidate
{
    public RazorVueComponentCandidate(
        INamedTypeSymbol componentSymbol,
        IMethodSymbol? buildRenderTreeMethod,
        IMethodSymbol? onInitializedMethod,
        IMethodSymbol? onInitializedAsyncMethod,
        IMethodSymbol? onParametersSetMethod,
        IMethodSymbol? onParametersSetAsyncMethod,
        IMethodSymbol? onAfterRenderMethod,
        IMethodSymbol? onAfterRenderAsyncMethod,
        IMethodSymbol? shouldRenderMethod,
        IMethodSymbol? setParametersAsyncMethod,
        IMethodSymbol? disposeMethod,
        IMethodSymbol? disposeAsyncMethod,
        ImmutableArray<IMethodSymbol> logicMethods,
        ImmutableArray<IFieldSymbol> logicFields,
        RazorVueEntryKind entryKind)
    {
        ComponentSymbol = componentSymbol ?? throw new ArgumentNullException(nameof(componentSymbol));
        BuildRenderTreeMethod = buildRenderTreeMethod;
        OnInitializedMethod = onInitializedMethod;
        OnInitializedAsyncMethod = onInitializedAsyncMethod;
        OnParametersSetMethod = onParametersSetMethod;
        OnParametersSetAsyncMethod = onParametersSetAsyncMethod;
        OnAfterRenderMethod = onAfterRenderMethod;
        OnAfterRenderAsyncMethod = onAfterRenderAsyncMethod;
        ShouldRenderMethod = shouldRenderMethod;
        SetParametersAsyncMethod = setParametersAsyncMethod;
        DisposeMethod = disposeMethod;
        DisposeAsyncMethod = disposeAsyncMethod;
        LogicMethods = logicMethods.IsDefault ? ImmutableArray<IMethodSymbol>.Empty : logicMethods;
        LogicFields = logicFields.IsDefault ? ImmutableArray<IFieldSymbol>.Empty : logicFields;
        EntryKind = entryKind;
    }

    public INamedTypeSymbol ComponentSymbol { get; }

    public IMethodSymbol? BuildRenderTreeMethod { get; }

    public IMethodSymbol? OnInitializedMethod { get; }

    public IMethodSymbol? OnInitializedAsyncMethod { get; }

    public IMethodSymbol? OnParametersSetMethod { get; }

    public IMethodSymbol? OnParametersSetAsyncMethod { get; }

    public IMethodSymbol? OnAfterRenderMethod { get; }

    public IMethodSymbol? OnAfterRenderAsyncMethod { get; }

    public IMethodSymbol? ShouldRenderMethod { get; }

    public IMethodSymbol? SetParametersAsyncMethod { get; }

    public IMethodSymbol? DisposeMethod { get; }

    public IMethodSymbol? DisposeAsyncMethod { get; }

    public ImmutableArray<IMethodSymbol> LogicMethods { get; }

    public ImmutableArray<IFieldSymbol> LogicFields { get; }

    public RazorVueEntryKind EntryKind { get; }
}

