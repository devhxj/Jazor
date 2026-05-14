# ECMAScript.Vben

Admin-shell contracts and native RazorVue admin-shell components for `Jazor`.

Scope of this package:

- Core admin-shell models: navigation, breadcrumb, page actions, layout modes
- Shared authoring base types for admin-shell components
- Native admin-shell components built on shared Vue3 authoring contracts
- Stable public contracts that do not leak specific UI-library props

Authoring notes:

- Native Vben shell components stay on the RazorVue user-component path: `ComponentBase + IVueComponent + [ECMAScriptModule(...)]`
- Shared metadata such as prop/slot overrides belongs to the general Vue authoring layer via `VueProp` / `VueSlot`

This package is the semantic and implementation core. Third-party UI-library cooperation belongs in samples or app-level composition, not in `ECMAScript.Vben.*` adapter packages.
