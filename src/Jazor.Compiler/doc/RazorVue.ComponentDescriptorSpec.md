# RazorVue Component Descriptor Spec

## 1. Purpose

This document defines the component contract model used by RazorVue.

Its purpose is to fix:

1. how a RazorVue component describes itself to the compiler
2. how component call sites are validated
3. how slots, emits, and bindable channels are discovered
4. how intrinsic and library components fit into the same contract system

This document is intentionally narrower than `RazorVue.Design.md`.
It focuses only on component contract shape and resolution behavior.

Related documents:

- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.HardRules.md](./RazorVue.HardRules.md)
- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
- [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)

## 2. Design Goal

RazorVue must not infer component contracts ad hoc while lowering templates.

Every component used by the RazorVue pipeline must have an explicit descriptor shape that can answer at least:

- what this component is called
- where it is imported from
- what props it accepts
- what emits it exposes
- what slots it supports
- whether it supports model-style binding
- whether it carries style/runtime hints

## 3. Descriptor Scope

The descriptor model applies to three kinds of components:

1. user components
2. Vue intrinsic components
3. library components

Examples:

- user component: `Counter`
- intrinsic: `Teleport`
- library component: `VBtn`

The compiler should consume all three through one descriptor contract instead of separate ad hoc paths.

## 4. Top-level Descriptor Model

Recommended structure:

```csharp
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
```

### 4.1 `Name`

Short component name used for template resolution.

Examples:

- `Counter`
- `MyDialog`
- `VBtn`
- `Teleport`

### 4.2 `FullName`

Stable full identity for diagnostics and cross-assembly lookup.

Examples:

- `Demo.Components.Counter`
- `ECMAScript.Vue.Components.Teleport`
- `ECMAScript.Vue.Vuetify.VBtn`

### 4.3 `SourceKind`

Recommended enum:

```csharp
public enum VueComponentSourceKind
{
    UserComponent,
    Intrinsic,
    LibraryComponent
}
```

### 4.4 `ImportSpecifier`

The ESM import source used when lowering this component into Vue output.

Examples:

- `./Counter.mjs`
- `vue`
- `vuetify/components`

### 4.5 `ResolutionNamespace`

The namespace through which the component becomes visible to RazorVue component resolution.

Examples:

- user component: `Demo.Components`
- intrinsic: `ECMAScript.UI.Vue`
- library component: `ECMAScript.UI.Vue.Vuetify`

This field exists to support `using`-driven component visibility without introducing extra target attributes on each Razor component.

### 4.6 `ExportName`

The exported symbol name used from the module.

Phase-one recommendation:

- user components default to `default`
- intrinsic/library components use their runtime export name

## 5. Prop Descriptor

Recommended structure:

```csharp
public sealed record VuePropDescriptor(
    string Name,
    string PublicName,
    string TypeName,
    bool Required,
    bool AcceptsBinding,
    string? DefaultExpression,
    VuePropKind Kind);
```

Recommended enum:

```csharp
public enum VuePropKind
{
    Normal,
    Model,
    HtmlLike,
    LibrarySpecific
}
```

### 5.1 `Name`

The runtime Vue prop name emitted into the final component call.

Examples:

- `title`
- `visible`
- `modelValue`

### 5.2 `PublicName`

The Razor/C# surface name exposed to component authors.

Examples:

- `Title`
- `Visible`
- `Value`

### 5.3 `AcceptsBinding`

Indicates that the prop participates in `@bind-*` lowering.

### 5.4 `DefaultExpression`

Stores component-side default metadata when available.

The descriptor should preserve this as contract metadata.
Actual runtime realization may still happen in setup or prop options.

## 6. Emit Descriptor

Recommended structure:

```csharp
public sealed record VueEmitDescriptor(
    string Name,
    string PayloadTypeName,
    string? RazorAlias,
    VueEmitKind Kind);
```

Recommended enum:

```csharp
public enum VueEmitKind
{
    Normal,
    ModelUpdate,
    LifecycleLike,
    LibrarySpecific
}
```

### 6.1 `Name`

The runtime Vue emit name.

Examples:

- `save`
- `close`
- `update:modelValue`
- `update:visible`

### 6.2 `RazorAlias`

Optional Razor/C#-facing sugar alias.

Examples:

- `OnSave`
- `OnClose`
- `ValueChanged`

### 6.3 `PayloadTypeName`

The best-known payload type name at extraction time.

Phase one does not require perfect inference for every explicit `Emit("...")` path,
but the field must exist.

## 7. Slot Descriptor

Recommended structure:

```csharp
public sealed record VueSlotDescriptor(
    string Name,
    bool IsDefault,
    ImmutableArray<VueSlotParameterDescriptor> Parameters,
    bool Required);
```

```csharp
public sealed record VueSlotParameterDescriptor(
    string Name,
    string TypeName);
```

### 7.1 Default slot

`ChildContent` lowers to:

- `Name = "default"`
- `IsDefault = true`

### 7.2 Named slot

Named `RenderFragment` lowers to:

- `Name = lowerCamelCase(parameterName)`

Example:

- `Header` -> `header`

### 7.3 Scoped slot

`RenderFragment<T>` lowers to:

- named slot
- one or more slot parameter descriptors

Phase one only requires the minimal scoped-slot parameter model needed by the supported template subset.

## 8. Component Flags

Recommended enum:

```csharp
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
```

Phase one should keep flags intentionally small.
Only include flags that materially affect:

- template validation
- `@bind` lowering
- runtime import/layout hints

## 9. Descriptor Generation Rules for User Components

### 9.1 Normal `[Parameter]`

Maps to:

- one `VuePropDescriptor`

### 9.2 `EventCallback` and `EventCallback<T>`

Maps to:

- one `VueEmitDescriptor`

Recommended alias rule:

- `OnSave` -> `save`
- `OnClose` -> `close`

### 9.3 `RenderFragment`

Maps to:

- default slot if parameter is `ChildContent`
- named slot otherwise

### 9.4 `RenderFragment<T>`

Maps to:

- scoped slot descriptor

### 9.5 `Foo + FooChanged`

Maps to:

- prop marked bindable
- corresponding `update:*`-style emit metadata
- relevant model-support flags

### 9.6 Explicit `Emit(...)`

May augment emit metadata when not already covered by `EventCallback`.

## 10. Descriptor Rules for Intrinsic Components

Intrinsic components use the same descriptor model.

Examples:

- `Teleport`
- `Transition`
- `KeepAlive`
- `Suspense`

They differ only in:

- `SourceKind = Intrinsic`
- import source
- prop/slot definitions

They must not be handled through compiler-only name guessing without a descriptor entry.

## 11. Descriptor Rules for Library Components

Library components also use the same descriptor model.

Examples:

- `VBtn`
- `VDialog`
- `VTextField`

They differ only in:

- `SourceKind = LibraryComponent`
- import source
- style/runtime dependencies

Phase one recommendation:

- library packages provide descriptors through a registry/provider mechanism
- compiler core does not hardcode every third-party component

## 12. Component Registry Model

Recommended aggregated registry:

```csharp
public sealed class VueComponentRegistry
{
    public ImmutableDictionary<string, ImmutableArray<VueComponentDescriptor>> ComponentsByName { get; }
    public ImmutableDictionary<string, VueComponentDescriptor> ComponentsByFullName { get; }
    public ImmutableDictionary<string, ImmutableArray<VueComponentDescriptor>> ComponentsByResolutionNamespace { get; }
}
```

Recommended descriptor sources:

1. built-in intrinsic descriptors
2. current-project user component descriptors
3. referenced-project user component descriptors
4. library registry providers

## 12.1 Resolution context

Component resolution must not be global-name-only.

Each RazorVue file needs a resolution context built from:

- current component namespace
- in-scope `using` directives
- referenced user component descriptors
- intrinsic descriptor registry
- library descriptor registries

If the compiler cannot explain why a component name is visible in one file but not another,
the resolution model is incomplete.

## 13. Resolution Rules

When the compiler sees an upper-case component-like tag:

1. resolve an explicit alias or fully-qualified match first
2. reserve intrinsic component names and resolve intrinsic matches
3. resolve visible user components from current namespace and imported namespaces
4. resolve visible library components from imported namespaces
5. if more than one visible candidate remains, report an ambiguity diagnostic
6. otherwise report an error

When the compiler sees a child node under a component body:

1. resolve slot-name match against the parent descriptor first
2. if matched, treat as slot content
3. otherwise resolve as normal component

## 13.1 Intrinsic names are reserved

Phase one should treat intrinsic component names as reserved.

Examples:

- `Teleport`
- `Transition`
- `KeepAlive`
- `Suspense`

User components or library components must not silently shadow these names.

Recommended behavior:

- exact intrinsic-name collision -> diagnostic
- no silent shadowing
- if later escape syntax is added, it must be explicit

## 13.2 `using`-driven visibility

Library components are visible only when their `ResolutionNamespace` is imported into the current Razor file or otherwise brought into scope by the surrounding compilation model.

Examples:

- `using ECMAScript.UI.Vue;`
- `using ECMAScript.UI.Vue.Vuetify;`

This is the mechanism that keeps UI library adoption lightweight.
Do not add per-component target attributes as the default visibility model.

## 13.3 Ambiguity behavior

If two or more non-intrinsic components with the same short name are simultaneously visible,
phase one must report a diagnostic instead of picking one heuristically.

Examples:

- two libraries both export `VBtn`
- project component `Dialog` conflicts with imported library `Dialog`

Recommended behavior:

- fully-qualified or aliased usage resolves the ambiguity
- simple-name fallback does not

## 14. Strictness Rules

For component call sites:

- prop matching is strict
- unknown props are diagnostics
- unknown event aliases are diagnostics
- unresolved slot names are not silently accepted as normal props
- ambiguous component names are diagnostics

For HTML elements:

- phase one may remain more permissive where appropriate

## 15. Descriptor Identity and HMR/Sourcemap

Descriptors must participate in artifact identity and future HMR decisions.

At minimum:

- descriptor content must be hashable
- descriptor changes must be distinguishable from template-only and logic-only changes

Descriptors should also preserve enough source-linkable identity for diagnostics and later tooling,
though phase one does not require full descriptor-level sourcemap behavior.

## 16. Phase-one Scope

Phase one requires:

- the descriptor structures
- user component descriptor extraction
- intrinsic descriptor registration
- `using`-aware visibility rules
- ambiguity diagnostics
- strict descriptor-based component validation

Phase one does not require:

- exhaustive third-party ecosystem coverage
- automatic npm/jsr package introspection
- advanced directive metadata systems

## 17. Conclusion

RazorVue must treat component contracts as explicit compiler-owned metadata.

`VueComponentDescriptor` is not optional convenience structure.
It is the boundary that keeps:

- template validation
- slot resolution
- `@bind` lowering
- ecosystem integration
- host-facing metadata

stable and reviewable.
