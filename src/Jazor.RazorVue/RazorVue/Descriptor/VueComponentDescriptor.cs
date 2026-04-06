using System;
using System.Collections.Immutable;

namespace Jazor.RazorVue.Descriptor;

public sealed record VueComponentDescriptor(
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
    VueComponentFlags Flags);

public sealed record VueLifecycleDescriptor(
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

public sealed record VueLogicMethodDescriptor(
    string Name,
    int Arity,
    bool IsAsync);

public sealed record VueLogicDescriptor(
    ImmutableArray<VueLogicMethodDescriptor> Methods)
{
    public static VueLogicDescriptor Empty { get; } = new(ImmutableArray<VueLogicMethodDescriptor>.Empty);
}

public enum VueComponentSourceKind
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

