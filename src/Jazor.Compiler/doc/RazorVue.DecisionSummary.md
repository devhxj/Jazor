# RazorVue Decision Summary

## 1. What This Document Solves

This is a short document that keeps only the final decisions for the RazorVue direction so future work can restart quickly without reopening settled questions.

Full design lives in:

- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.HardRules.md](./RazorVue.HardRules.md)
- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)

## 2. Final Decisions

### 2.1 Vue-first, not a cross-framework UI abstraction

RazorVue is not a React/Vue/Svelte unification effort.

The target is:

- Razor as the template syntax
- Vue as the real component/runtime model
- `DenoHost` as the unified build host

### 2.2 `[ECMAScriptModule]` stays the unified entry

`[ECMAScriptModule]` remains the single entry marker for frontend compilation,
but it no longer means "always compile as a plain static ECMAScript module".

After entry, inputs must split into:

1. plain static module classes
2. Razor components

### 2.3 Razor components must inherit `JazorComponent`

All Razor components entering the RazorVue pipeline must inherit:

- `JazorComponent`

Recommended authoring base type is:

- `VueComponent : JazorComponent`

### 2.4 Base class hierarchy is fixed

The base hierarchy is:

`ComponentBase -> JazorComponent -> VueComponent`

Meaning:

- `ComponentBase` is the technical Razor base
- `JazorComponent` defines component identity for Jazor frontend compilation
- `VueComponent` carries Vue-first authoring APIs

### 2.5 Do not build the main pipeline on source generator ordering

RazorVue must not assume:

- Razor source generator runs first
- RazorVue source generator can then consume its output

Main semantic extraction is based on:

- analyzer
- generated code analysis

### 2.6 Do not parse `.razor` in phase one

Phase one does not introduce a custom `.razor` parser.

Instead it uses:

- component symbols
- generated `BuildRenderTree(RenderTreeBuilder)` operations
- code-behind symbols and operations

### 2.7 Razor components do not reuse static-module lowering

Even though analyzer can see generated code, Razor components are not plain user-authored method bodies.

Therefore:

- do not send Razor components into the existing static-module lowering path
- build a dedicated Razor render-tree extraction and Vue lowering path

### 2.8 Runtime semantics are Vue-first

Vue is the real runtime semantic model.

Blazor lifecycle members are preserved only as compile-time sugar:

- `OnInitialized*`
- `OnParametersSet*`
- `OnAfterRender*`
- `Dispose*`

They lower to Vue concepts such as:

- `setup`
- `watch(props, ...)`
- `onMounted`
- `onUpdated`
- `onUnmounted`

### 2.9 Vue-first authoring API lives on `VueComponent`

`VueComponent` is the host for Vue-first APIs such as:

- `Ref`
- `Reactive`
- `Computed`
- `Watch`
- `WatchEffect`
- `NextTick`
- `OnMounted`
- `OnUpdated`
- `OnUnmounted`
- `Emit`
- `Provide`
- `Inject`
- `Expose`

Razor-side sugar remains available:

- `[Parameter]`
- `EventCallback`
- `RenderFragment`
- `@bind`
- `@ref`
- `@key`

### 2.10 Output target is standard Vue ESM

Phase one output is fixed to:

- standard ESM
- `defineComponent`
- `setup`
- render function

Not phase-one targets:

- `.vue` SFC output
- generic UI runtime
- bundler-owned custom module formats

### 2.11 `DenoHost` is the unified build owner

Compiler responsibilities:

- component semantics
- Vue module generation
- manifest generation

`DenoHost` responsibilities:

- dependency resolution
- unified compilation
- bundling
- runtime integration

### 2.12 HMR and sourcemap are architected now, implemented later

Phase one does not need full HMR or sourcemap support,
but it must preserve:

- source-origin metadata
- stable artifact identity
- separated template/logic/descriptor hashes

These are architecture requirements, not optional future polish.

## 3. Phase-One Scope

Phase one only needs to close this loop:

1. discover `[ECMAScriptModule]` Razor components
2. enforce `JazorComponent` / `VueComponent` contracts
3. extract props / emits / slots / bind metadata
4. recover a minimal render-tree model from `BuildRenderTree`
5. lower to Vue `defineComponent + render`
6. emit manifest for `DenoHost`

Phase one does not require:

- full ecosystem integration
- deep SSR/hydration strategy
- sourcemap output
- HMR runtime
- generic multi-framework abstractions

## 4. Acceptance Summary

RazorVue phase one is complete only when all of the following are true:

1. Razor component entry detection is stable.
2. Component contracts are extractable.
3. Minimal render-tree recovery works.
4. Vue ESM artifacts are emitted deterministically.
5. `DenoHost` can consume the manifest.
6. Source-origin and HMR identity metadata are already reserved in the pipeline.

## 5. One-line Conclusion

RazorVue is a Vue-first pipeline where Razor is the template syntax, analyzer extracts semantics from generated Razor component code, the compiler emits Vue ESM artifacts, and `DenoHost` owns the final unified build.
