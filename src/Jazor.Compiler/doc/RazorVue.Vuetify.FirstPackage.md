# RazorVue Vuetify First Package

> Status: active reference
> Positioning: Scoped package-design reference for the first RazorVue library-authoring package.

## 1. Document Position

This document defines the first RazorVue ecosystem package shape for Vuetify.

It is intentionally narrow.
It describes the first authoring package, not the full Vuetify surface area.

Related documents:

- [RazorVue.Overview.md](./RazorVue.Overview.md)
- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.ComponentDescriptorSpec.md](./RazorVue.ComponentDescriptorSpec.md)
- [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)
- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)

## 2. Purpose

`Jazor.RazorVue.Vuetify` is the first concrete test of the RazorVue library authoring model.

Its purpose is to prove that a Vue ecosystem library can be exposed to C# authors as a normal Razor component library while still lowering to standard Vue runtime imports.

The package must support:

- C#-friendly component authoring
- descriptor-driven lowering
- explicit style dependencies
- host-facing plugin requirements

## 3. Positioning

The package is not a runtime wrapper library.

It is an authoring package.

That means:

- the source of truth is the C# stub type
- the compiler derives descriptor data from the stub metadata
- the host owns plugin installation and bundling
- business authors do not write JavaScript or TypeScript wrappers

## 4. Package Contract

The package should use the namespace:

`ECMAScript.UI.Vue.Vuetify`

The package should define its components as thin stubs that inherit `VueLibraryComponent`.

The stub surface should stay close to Blazor conventions:

- `[Parameter]` for props
- `EventCallback` for events
- `RenderFragment` for default and named slots
- `RenderFragment<TContext>` for scoped slots when needed

## 5. First-Wave Components

The first-wave component set should be small and high-value:

- `VBtn`
- `VTextField`
- `VCard`
- `VIcon`
- `VDialog`

`VDialog` should be treated as the first slot-context example and may land after the simpler components.

## 6. Component Modeling Rules

Each component stub must declare enough metadata for the compiler to derive:

- `SourceKind = LibraryComponent`
- runtime import specifier
- runtime export name
- style dependencies

Recommended metadata shape:

- `VueLibraryComponentAttribute`
- `VueLibraryStyleAttribute`

Recommended authoring rule:

- the stub should remain thin
- no runtime behavior should live in the stub
- no component-specific lowering branch should be added in the core pipeline

## 7. Binding Rules

Binding should remain C#-shaped.

Recommended model:

- `ModelValue`
- `ModelValueChanged`

This keeps the authoring experience familiar for Blazor users while still lowering cleanly into Vue model update semantics.

## 8. Slot Rules

Slots should remain strongly typed where possible.

Recommended mapping:

- `RenderFragment` -> default slot or simple named slot
- `RenderFragment<TContext>` -> scoped slot with a C# context type

`VDialog.Activator` should be the first strong typed scoped-slot example.

## 9. Style Rules

Vuetify components should declare style dependencies explicitly.

The first package should at minimum declare:

- `vuetify/styles`

The compiler should preserve this as metadata for host-side consumption.

## 10. Plugin Rules

Vuetify also requires a host-facing plugin requirement.

The package should not install the plugin itself.
It should only make the requirement explicit so the host can consume it later.

## 11. Deferred Scope

The first package should not try to cover:

- full Vuetify component coverage
- all Vuetify props and emits
- all slot-context variants
- icon-set and theme configuration
- Router or Pinia integration
- custom Vuetify-specific lowering paths

## 12. Acceptance Criteria

The package is useful when all of the following are true:

- a C# author can import `ECMAScript.UI.Vue.Vuetify`
- Vuetify components are recognized as library components
- descriptor extraction stays unified with the existing RazorVue model
- generated artifacts declare the correct imports, styles, and plugin requirements
- the authoring model still feels Blazor-like
