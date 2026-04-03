# RazorVue Hard Rules

This document fixes the implementation rules that cannot remain ambiguous during phase one.

It does not repeat all design discussion.
It exists to lock the boundaries that later implementation and review must not keep renegotiating.

Related documents:

- [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)
- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
- [RazorVue.Pitfalls.md](./RazorVue.Pitfalls.md)

## 1. Scope

These rules apply to RazorVue phase one:

- Razor component entry into ECMAScript frontend compilation
- Vue-first lowering
- Vue ESM artifact emission
- `DenoHost` handoff

## 2. Rule 1. RazorVue is Vue-first

Phase one RazorVue is:

- Razor as template syntax
- Vue as the real runtime semantic model
- `DenoHost` as the unified build host

It is not:

- a generic UI abstraction layer
- a cross-framework compilation target
- a Blazor runtime clone

## 3. Rule 2. `[ECMAScriptModule]` unifies entry, not lowering

`[ECMAScriptModule]` remains the single entry marker.

After entry, the compiler must split inputs into:

1. plain static module classes
2. Razor components

Any implementation that tries to use one lowering path for both is out of bounds.

## 4. Rule 3. RazorVue components must inherit `JazorComponent`

Any Razor component entering the RazorVue path must inherit `JazorComponent`.

Recommended author base:

- `VueComponent : JazorComponent`

Invalid cases must be diagnosed:

- `[ECMAScriptModule]` Razor component inheriting only `ComponentBase`
- `[ECMAScriptModule]` Razor component inheriting neither `JazorComponent` nor its descendants

## 5. Rule 4. `JazorComponent` must inherit `ComponentBase`

The phase-one hierarchy is fixed:

`ComponentBase -> JazorComponent -> VueComponent`

This is not optional.

Rationale:

- Razor components are technically grounded in `ComponentBase`
- C# does not support multiple inheritance
- trying to bypass `ComponentBase` is not a stable design path

## 6. Rule 5. `JazorComponent` must stay thin

`JazorComponent` is a component identity base, not a second runtime framework host.

It must not absorb:

- Vue composable API
- runtime scheduling
- bundling/build concerns
- generalized state runtime

Vue-first author APIs belong in `VueComponent`.

## 7. Rule 6. `VueComponent` is the required host for Vue-first author APIs

Vue-first APIs must have a stable, compiler-recognizable home.

That home is `VueComponent`.

Phase-one API surface belongs there, including:

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

## 8. Rule 7. Do not parse `.razor` in phase one

Phase one must not introduce a custom `.razor` parser as the main input path.

The compiler is expected to consume:

- component symbols
- generated `BuildRenderTree` operations
- code-behind symbols/operations

Any implementation that rebuilds a Razor parser as a prerequisite for phase one is outside scope.

## 9. Rule 8. Do not rely on source generator ordering

Phase one must not assume:

- Razor SG runs first
- another SG can reliably consume its output

Source-generator ordering must not become the architectural foundation for RazorVue.

## 10. Rule 9. Generated code analysis is mandatory

The RazorVue analyzer must enable generated code analysis.

This is not an optional enhancement.
It is a prerequisite for using generated Razor component code as the semantic extraction source.

## 11. Rule 10. Analyzer mode split is mandatory

The existing plain ECMAScript analyzer rules must not run unchanged against RazorVue components.

Phase one requires an explicit mode split between:

- plain static-module analysis
- RazorVue component analysis

Otherwise valid RazorVue symbols such as:

- `ComponentBase`
- `RenderFragment`
- `EventCallback`

will be rejected by the wrong rule set before RazorVue lowering begins.

## 12. Rule 11. Razor components do not reuse static-module lowering

RazorVue must not send generated Razor component bodies into the plain static-module lowering pipeline.

Reasons:

- generated Razor code is builder-pattern based
- it is structurally different from plain user-authored module methods

Therefore phase one requires a separate path for:

- render-tree extraction
- Razor-to-Vue lowering

## 13. Rule 12. Semantic carrier must be explicit

Phase one must define an explicit compiler-owned carrier between semantic extraction and host-facing emission.

That carrier may be implemented through:

- `RazorVueSemanticSnapshot`
- `VueCompiledArtifact`
- `RazorVueCatalog`

or equivalent structures.

But it must not be replaced with:

- repeated re-analysis in later stages
- analyzer-only hidden state
- raw string concatenation as the only handoff

## 14. Rule 13. Vue defines runtime lifecycle semantics

Phase one runtime lifecycle semantics are Vue-first.

Blazor lifecycle members are compile-time sugar only.

Therefore:

- `OnInitialized*`
- `OnParametersSet*`
- `OnAfterRender*`
- `Dispose*`

must lower to Vue concepts such as:

- `setup`
- `watch(props, ...)`
- `onMounted`
- `onUpdated`
- `onUnmounted`

Phase one does not promise full Blazor runtime equivalence.

## 15. Rule 14. `StateHasChanged`, `ShouldRender`, and `SetParametersAsync` stay out of the main model

These members may be technically inherited through `ComponentBase`,
but phase one must not accept them as part of the RazorVue semantic model.

Implementation requirement:

- using them must produce diagnostics
- they must not silently influence Vue lowering behavior

## 16. Rule 15. Component contracts must be extracted before render lowering

Phase one must extract an explicit component contract model before render lowering.

That contract includes at least:

- props
- emits
- slots
- bind/model metadata
- import/export identity

Do not guess component contract ad hoc while traversing render-tree output.

## 17. Rule 16. Component resolution must be `using`-driven and explicit

Phase one component visibility must be determined from:

- current namespace
- in-scope `using` directives
- intrinsic component registry
- referenced user-component descriptors
- library descriptor registries

The compiler must not fall back to a global short-name search.

If multiple visible components share the same short name,
phase one must report an ambiguity diagnostic.

## 18. Rule 17. Intrinsic component names are reserved

Intrinsic Vue component names are reserved in phase one.

Examples include:

- `Teleport`
- `Transition`
- `KeepAlive`
- `Suspense`

User components and library components must not silently shadow them.

## 19. Rule 18. Component property matching is strict

Phase one must be strict for component calls.

Allowed:

- reasonable attribute flexibility for HTML elements

Not allowed:

- silent passthrough of unknown component props
- silent fallback for unknown component event aliases
- unresolved slot names accepted as normal component props

Unknown component-side attributes must be diagnosed.

## 20. Rule 19. Output must be standard Vue ESM

Phase one output is fixed to standard Vue ESM with:

- `defineComponent`
- `setup`
- render function

Phase one must not pivot to:

- `.vue` SFC as the main format
- custom runtime module shapes
- bundler-owned private module formats

## 21. Rule 20. Compiler and `DenoHost` responsibilities must stay separate

Compiler owns:

- semantic extraction
- contract generation
- Vue module generation
- artifact/manifest generation

`DenoHost` owns:

- dependency resolution
- unified compilation
- bundling
- runtime integration

The compiler must not grow its own bundler.
`DenoHost` must not re-interpret Razor component semantics.

## 22. Rule 21. Current host integration needs an explicit migration path

The repository already has a working static-module catalog and emit flow.

Phase one RazorVue must define how new Vue artifacts coexist with:

- the current `ModuleCatalog`
- current host manifest handling
- current downstream bundling flow

The project must not enter implementation with this transition left implicit.

## 23. Rule 22. Phase one must stay minimal

Phase one must only close the smallest viable loop:

- RazorVue component discovery
- contract extraction
- minimal render-tree recovery
- Vue lowering
- artifact emission
- `DenoHost` handoff

It must not expand into:

- full ecosystem support
- full HMR runtime
- full sourcemap emit
- broad Razor compatibility
- generic multi-framework abstractions

## 24. Rule 23. Source-origin metadata must be preserved from phase one

Phase one does not need complete sourcemap output,
but it must preserve source-origin metadata across the pipeline.

At minimum that applies to:

- render-tree nodes
- component logic bindings
- lifecycle bindings
- Vue lowering nodes
- artifact output anchors

Origin categories should at least distinguish:

- `razor-template`
- `component-logic`
- `generated-render`

Do not rely on reverse-inference from final JS text.

And do not preserve only categories.

Phase one source-origin metadata must also preserve:

- original source file path when known
- stable span or stable segment identity
- generated fallback span when exact source is unavailable
- explicit mapping quality

## 25. Rule 24. Artifact identity must be stable and split

Phase one artifacts must preserve stable identity and split hash information.

At minimum reserve:

- `ComponentId`
- `ModuleId`
- `DescriptorHash`
- `TemplateHash`
- `LogicHash`
- `HmrBoundaryKind`

Do not collapse all change categories into one undifferentiated content hash.

## 26. Rule 25. HMR and sourcemap are architecturally included, not silently deferred

Phase one does not fully implement HMR or sourcemap,
but the architecture must already reserve the data needed for both.

That means:

- source-origin metadata is required now
- stable artifact identity is required now
- later HMR/sourcemap must not require redesigning the main pipeline

## 27. Conclusion

RazorVue phase one succeeds only if it keeps these boundaries stable:

- Vue-first semantics
- unified entry plus split lowering
- `JazorComponent` / `VueComponent` hierarchy
- analyzer-based generated-code semantic extraction
- dedicated Razor render-tree lowering
- standard Vue ESM output
- compiler/`DenoHost` separation
- HMR/sourcemap-ready metadata preservation
