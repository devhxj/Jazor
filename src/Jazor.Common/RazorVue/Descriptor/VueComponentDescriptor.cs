using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Descriptor;

internal sealed record VueComponentDescriptor(
    string Name,
    string FullName,
    VueComponentSourceKind SourceKind,
    string ResolutionNamespace,
    string ImportSpecifier,
    string ExportName,
    ImmutableArray<VuePropDescriptor> Props,
    ImmutableArray<VueEmitDescriptor> Emits,
    ImmutableArray<VueSlotDescriptor> Slots,
    ImmutableArray<string> StyleDependencies,
    ImmutableArray<string> PluginRequirements,
    VueComponentFlags Flags);

internal sealed record VueLifecycleDescriptor(
    bool HasOnInitialized,
    bool HasOnInitializedAsync,
    bool HasOnParametersSet,
    bool HasOnParametersSetAsync,
    bool HasOnAfterRender,
    bool HasOnAfterRenderAsync,
    bool HasShouldRender,
    bool HasSetParametersAsync,
    bool HasDispose,
    bool HasDisposeAsync)
{
    public bool HasAnyHook
        => HasOnInitialized || HasOnInitializedAsync ||
           HasOnParametersSet || HasOnParametersSetAsync ||
           HasOnAfterRender || HasOnAfterRenderAsync ||
           HasShouldRender || HasSetParametersAsync ||
           HasDispose || HasDisposeAsync;
}

internal sealed record VueLogicMethodDescriptor(
    string Name,
    int Arity,
    bool IsAsync,
    IMethodSymbol MethodSymbol);

internal sealed record VueLogicFieldDescriptor(
    string Name,
    bool IsReadOnly,
    IFieldSymbol FieldSymbol);

internal sealed record VueLogicDescriptor(
    ImmutableArray<VueLogicFieldDescriptor> Fields,
    ImmutableArray<VueLogicMethodDescriptor> Methods)
{
    public static VueLogicDescriptor Empty { get; } = new(
        ImmutableArray<VueLogicFieldDescriptor>.Empty,
        ImmutableArray<VueLogicMethodDescriptor>.Empty);
}

internal enum VueComponentSourceKind
{
    UserComponent,
    Intrinsic,
    LibraryComponent
}

[Flags]
public enum VueComponentFlags
{
    None = 0,
    SupportsModelValue = 1,
    SupportsMultipleModels = 2,
    RequiresExplicitChildren = 4,
    IsDynamicSafe = 8,
    IsFormControl = 16
}
