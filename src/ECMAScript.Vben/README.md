# ECMAScript.Vben

Admin-shell contracts and native RazorVue admin-shell components for `Jazor`.

Scope of this package:

- Core admin-shell models: navigation, breadcrumb, page actions, layout modes
- Shared authoring base types for admin-shell components
- Native admin-shell components built on shared Vue3 authoring contracts
- Container-contract shell components that can be compile-time injected to concrete implementations
- Stable public contracts that do not leak specific UI-library props

Authoring notes:

- Native Vben shell components stay on the RazorVue user-component path: `ComponentBase + IVueComponent + [ECMAScriptModule(...)]`
- Public shell components also act as container contracts through `IVueContainerComponent`
- Concrete replacements implement `IVueContainerImplementation<TContainer>` and are selected through assembly-level `[VueInject]`
- Shared metadata such as prop/slot overrides belongs to the general Vue authoring layer via `VueProp` / `VueSlot`

Verified baseline:

- Default registry resolution keeps `VbenAdminLayout`, `VbenSidebarMenu`, `VbenHeaderBar`, and `VbenPageContainer` on the native `ECMAScript.Vben` implementation path
- `[VueInject]` replacement is regression-covered for all four public shell components
- Injected runtime shape lowering is regression-covered at both Vue SFC and pipeline artifact levels for all four public Vben shell components
- Multi-shell composition is regression-covered at both Vue SFC and pipeline artifact levels, including cross-container import aggregation, style/plugin dependency aggregation, and nested slot/prop/model mapping stability
- Container compatibility failures are regression-covered for missing props, prop type mismatch, emit payload mismatch, default-slot mismatch, `CaptureUnmatchedValues` mismatch, duplicate registrations, and mismatched `IVueContainerImplementation<TContainer>` contracts
- A real sample now exists under `samples/ECMAScript.Vben.ElementPlusInject/`, proving Razor-authored Vben shell composition, sample-local `Element Plus` container injection, official `razorvue-consumer-entry` bridge generation, and Deno-only SSR/browser/bundle smoke verification

This package is the semantic and implementation core. Third-party UI-library cooperation belongs in samples or app-level composition, not in `ECMAScript.Vben.*` adapter packages.
