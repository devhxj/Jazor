# RazorVue Design

This document defines the RazorVue design across the Jazor compiler, Razor substrate, Vue authoring library, and RazorVue analysis layer.

It is primarily an architecture document.
It does not attempt to mirror every current implementation detail line by line.

The current repository now contains a partial RazorVue pipeline, including:

- entry discovery and analyzer split
- Roslyn entry/misuse diagnostics for `JAZORVUE001`, `JAZORVUE002`, `JAZORVUE004`, `JAZORVUE005`, and `JAZORVUE006`
- descriptor extraction
- `RazorVueCatalog` generation
- emit/manifest materialization
- a minimal real Vue render-function emission lane for a limited `BuildRenderTree` subset
- a proven component happy path covering component nodes, props, event/listener wiring, and default/named/scoped slot flow

The repository still does not contain a complete phase-one RazorVue pipeline.
Unsupported extraction/lowering shapes still fall back to the general `JAZORVGA001` diagnostic surface. On the current thin `Jazor.RazorVue.Analysis` host path, known component-resolution `NotFound` failures now project to `JAZORVGA002`, and unsupported lifecycle-lowering shapes now project to `JAZORVGA005`. `JAZORVGA003` / `JAZORVGA004` are defined but are not yet reachable because short-name / intrinsic-name resolution is not wired into this path.

This document exists to:

1. define the problem RazorVue is solving
2. define the layers where RazorVue should and should not live
3. fix responsibilities before implementation begins
4. provide a stable contract for later `DenoHost` integration

Related documents:

- [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)
- [RazorVue.HardRules.md](./RazorVue.HardRules.md)
- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
- [RazorVue.Pitfalls.md](./RazorVue.Pitfalls.md)

## 0. Project Split

The public project split is:

- `Jazor.Compiler`
  owns compiler core orchestration, shared contracts, and static-module generation
- `Jazor.Razor`
  owns the Razor-facing base component substrate (`JazorComponent`)
- `Jazor.RazorVue`
  owns the Vue-facing authoring substrate (`VueComponent`) plus the RazorVue core semantic lane: descriptor extraction, `BuildRenderTree` extraction, component resolution, render-function lowering, and artifact shaping
- `Jazor.RazorVue.Analysis`
  owns the thin RazorVue generator/analyzer-facing host entry: Roslyn wiring and diagnostic projection into the compiler pipeline

This split is intentional:

- author-facing runtime types do not belong in the compiler core
- Vue analysis entry does not belong in the Vue authoring runtime
- the existing static-module path stays isolated from RazorVue analysis growth

## 1. Goals

RazorVue exists to let Razor components enter the ECMAScript frontend compilation domain and become Vue-first component modules.

The target chain is:

`[ECMAScriptModule] Razor Component -> Vue component artifact -> DenoHost unified compile`

The design goals are:

- keep Razor as the author-facing template syntax
- use Vue as the real runtime component model
- keep build ownership with `DenoHost`
- emit stable Vue ESM artifacts
- preserve enough metadata for later HMR and sourcemap support

## 2. Non-goals

Phase one is explicitly not trying to do the following:

1. build a cross-framework UI abstraction for React/Vue/Svelte
2. fully emulate Blazor runtime semantics
3. build a new bundler into the compiler
4. output `.vue` SFC as the primary artifact
5. fully solve SSR/hydration strategy
6. fully support all Razor syntax
7. fully implement sourcemap or HMR runtime behavior

## 3. Positioning

RazorVue is Vue-first.

That means:

- Razor is the template frontend
- Vue is the semantic/runtime target
- Blazor familiarity is preserved only where it improves author adoption

This is not:

- a generic UI compiler
- a Blazor runtime clone on top of Vue
- a hidden multi-framework abstraction layer

## 4. Entry Model

### 4.1 Unified entry

`[ECMAScriptModule]` remains the unified marker for entering frontend compilation.

Its meaning expands from:

- "plain ECMAScript static module"

to:

- "this type participates in ECMAScript frontend output"

### 4.2 Required split after entry

After `[ECMAScriptModule]` is detected, the compiler must split into two paths.

#### Plain static module path

Input:

- `static class`
- current ECMAScript module contract

Output:

- current plain ECMAScript module artifact flow

#### Razor component path

Input:

- Razor component
- inherits `JazorComponent`
- marked with `[ECMAScriptModule]`

Output:

- Vue-first component artifact flow

## 5. Base Class Hierarchy

The hierarchy is fixed to:

`ComponentBase -> JazorComponent -> VueComponent`

### 5.1 `JazorComponent`

`JazorComponent` is the Razor component identity base for Jazor frontend compilation.

Responsibilities:

- define "this component belongs to the Jazor frontend pipeline"
- host shared Razor-component contract boundaries
- host Blazor lifecycle sugar entry points

Non-responsibilities:

- Vue-specific APIs
- runtime scheduling
- build host logic
- state update orchestration

### 5.2 `VueComponent`

`VueComponent` is the Vue-first authoring base class.

Responsibilities:

- host Vue-first author APIs
- provide a stable symbol surface for analyzer/lowering
- separate Vue semantics from generic Razor component identity

Expected API surface includes:

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

## 6. Why RazorVue Does Not Parse `.razor`

Phase one does not introduce a custom `.razor` parser.

Reasons:

1. Razor already has a toolchain and generated-code model.
2. The main challenge is Vue lowering, not rebuilding Razor parsing.
3. A parallel `.razor` parser would increase maintenance scope and semantic drift.
4. The current practical integration point is generated Razor component code.

Therefore the main inputs are:

- component symbols
- generated `BuildRenderTree(RenderTreeBuilder)` operations
- code-behind symbol/operation data

## 7. Compile-time Entry Strategy

### 7.1 Not source-generator-order-dependent

RazorVue must not depend on source generator ordering.

Specifically, phase one does not rely on:

- Razor source generator producing C# first
- another source generator then consuming those outputs

### 7.2 Analyzer as the semantic extraction entry point

RazorVue uses analyzer-based semantic extraction with generated code analysis enabled.

Analyzer is responsible for:

- discovering valid RazorVue components
- extracting symbols and operations
- reporting invalid usages early

### 7.2.1 Analyzer is not the semantic transport

Analyzer is the semantic entry point for validation and extraction,
but it is not the cross-phase transport mechanism.

That distinction must stay explicit because:

- analyzers report diagnostics, they do not define the build-facing artifact carrier
- the current repository already has a downstream host path that consumes compiler-owned module metadata
- letting analyzer semantics leak into ad hoc host handoff would create hidden coupling immediately

### 7.2.2 Required semantic carrier

Phase one needs an explicit compiler-owned carrier between:

1. semantic extraction
2. Vue lowering
3. build-facing materialization

Recommended internal stages are:

- `RazorVueSemanticSnapshot`
- `VueCompiledArtifact`
- `RazorVueCatalog` or equivalent host-facing carrier

Recommended responsibility split:

- analyzer validates and extracts semantic inputs
- compiler-owned extraction/lowering builds `RazorVueSemanticSnapshot`
- lowering converts snapshot into `VueCompiledArtifact`
- a later build-facing stage materializes `RazorVueCatalog` and manifest outputs

The important rule is not the exact type names.
It is that the pipeline must have an explicit semantic carrier instead of:

- repeated re-analysis in later stages
- direct analyzer-to-host coupling
- temporary string-only handoff

### 7.2.3 Production and consumption surfaces

Phase one must also define where each carrier is produced and consumed.

Recommended implementation boundary:

1. analyzer runs as diagnostics/discovery only
2. a compiler-owned extraction driver builds `RazorVueSemanticSnapshot` from the final compilation view
3. lowering consumes `RazorVueSemanticSnapshot` and produces `VueCompiledArtifact`
4. a catalog/materialization stage consumes `VueCompiledArtifact` and emits `RazorVueCatalog` plus manifest/sidecars
5. `DenoHost` consumes only the materialized compiler-owned outputs

Important constraint:

- `RazorVueSemanticSnapshot` must not depend on hidden analyzer state
- `VueCompiledArtifact` must not be reconstructed by `DenoHost`
- compiler-owned extraction must run on a compilation view where generated Razor component code is already available

Phase one does not need to freeze the final class name of the driver,
but it does need to freeze this production/consumption split.

### 7.3 Build ownership remains with `DenoHost`

The compiler extracts and emits metadata/artifacts.
`DenoHost` performs the later unified build.

This also implies a concrete implementation constraint:

- analyzer is the semantic extraction entry
- analyzer is not the physical artifact writer
- final module/manifest materialization must happen in a later compiler-owned build step or equivalent host-facing emission stage

### 7.4 Migration boundary with the current module path

The repository already has a working plain ECMAScript module path based on generated catalog metadata.

Phase one RazorVue must therefore define a migration boundary instead of assuming a greenfield host path.

Recommended rule:

- keep the current plain `ModuleCatalog` path working for static modules
- add a parallel RazorVue host-facing catalog or a versioned superset carrier
- let downstream host/emission code consume both during transition

Do not require phase one RazorVue to replace the entire existing module catalog flow before the first Vue path is proven.

## 8. Why Razor Components Need Their Own Lowering

Although analyzer can see generated Razor component symbols and operations,
Razor templates are represented through `BuildRenderTree(RenderTreeBuilder)` builder calls, not direct user-authored method bodies.

That means RazorVue cannot safely reuse the current plain static-module lowering path.

RazorVue needs its own stages:

1. component discovery
2. contract extraction
3. render-tree extraction from `BuildRenderTree`
4. Vue lowering
5. artifact emission

## 9. High-level Pipeline

The recommended RazorVue pipeline is:

1. `ComponentDiscovery`
2. `ContractExtraction`
3. `LogicExtraction`
4. `RenderTreeExtraction`
5. `VueLowering`
6. `ArtifactEmission`
7. `DenoHost`

Each stage must have stable inputs/outputs and must not collapse directly into string generation.

## 10. Component Contract Extraction

Every RazorVue component needs an explicit Vue-facing contract model.

That contract must describe:

- component identity
- import/export identity
- props
- emits
- slots
- model/binding metadata
- style dependencies
- a small set of flags

Recommended structure is a `VueComponentDescriptor`-style model.

### 10.1 Mapping rules

- `[Parameter]` normal property -> prop
- `EventCallback*` -> emit
- `RenderFragment` -> default or named slot
- `RenderFragment<T>` -> scoped slot
- `Foo + FooChanged` -> bind/model metadata
- explicit `Emit("...")` usage -> additional emit contract information

This contract must be extracted before render-tree lowering.

## 11. Logic Extraction

Logic extraction is separate from render extraction.

It covers:

- fields
- methods
- `Ref/Reactive/Computed`
- lifecycle sugar
- `Emit`
- `Provide/Inject`
- `Expose`
- watchers/effects

This stage feeds Vue `setup` lowering.

It should not try to reconstruct template structure.

## 12. Render-tree Extraction

`BuildRenderTree` is not emitted Vue code.
It is only the generated Razor representation of template structure.

RazorVue therefore needs a minimal intermediate render-tree model.

That model should capture at least:

- element nodes
- component nodes
- text nodes
- expression nodes
- conditional nodes
- loop nodes
- attribute nodes
- slot-content nodes

This extraction stage must recover stable structure from builder call patterns such as:

- `OpenElement`
- `CloseElement`
- `OpenComponent`
- `CloseComponent`
- `AddAttribute`
- `AddContent`

## 13. Template Semantics

RazorVue template semantics are Vue-first.

Key rules:

- lower-case tag -> HTML element
- upper-case tag -> Vue component
- `Teleport` / `Transition` / `KeepAlive` / `Suspense` -> intrinsic
- `@bind` -> Vue `v-model`
- `@ref` -> template ref
- `@key` -> vnode key
- `RenderFragment*` -> Vue slots

This is not a Blazor render-tree compatibility target.

## 14. Lifecycle Semantics

Vue is the real lifecycle model.

Blazor lifecycle members are preserved as compile-time sugar only.

### 14.1 Supported sugar

- `OnInitialized*`
- `OnParametersSet*`
- `OnAfterRender*`
- `Dispose*`

### 14.2 Lowering targets

Those members lower to Vue concepts such as:

- `setup`
- `watch(props, ...)`
- `onMounted`
- `onUpdated`
- `onUnmounted`

The goal is stable, explainable behavior, not full Blazor runtime equivalence.

## 15. Vue Output Model

Phase one output is fixed to standard Vue ESM with `defineComponent + setup + render`.

The canonical shape is:

```js
export default defineComponent({
  name: "...",
  props: { ... },
  emits: [ ... ],
  setup(props, { emit, slots, expose, attrs }) {
    return () => h(...);
  }
})
```

Phase one does not target `.vue` SFC.

## 16. Ecosystem Extensions

Later Vue ecosystem packages can extend the compiler through descriptor/registry-style integration.

Examples:

- `ECMAScript.Vue.Vuetify`
- `ECMAScript.Vue.Router`
- `ECMAScript.Vue.Pinia`

Their role is:

- component descriptor registration
- additional import/style declarations
- Vue ecosystem-specific authoring helpers

They do not redefine the core RazorVue pipeline.

## 17. Artifact and Manifest Model

The compiler should not treat the result as a loose JS string.

A structured artifact must exist and should include at least:

- component name
- relative module path
- module content
- import dependencies
- style dependencies
- content hash
- runtime hints

The manifest consumed by `DenoHost` should be derived from those artifacts, not rebuilt independently.

In phase one, "artifact emission" means two distinct responsibilities:

1. semantic/lowering stages produce a structured artifact model
2. a later build-facing emission stage materializes those artifacts for `DenoHost`

This distinction matters because analyzer is part of semantic extraction, not the final file-writing host.

## 18. HMR and SourceMap Reservation

Phase one does not require full HMR or sourcemap behavior,
but the pipeline must preserve the metadata required for them.

### 18.1 Source-origin reservation

The pipeline should preserve source-origin metadata for:

- render-tree nodes
- component logic bindings
- lifecycle bindings
- Vue lowering nodes
- artifact output anchors

At minimum, origin categories should distinguish:

- `razor-template`
- `component-logic`
- `generated-render`

But origin category alone is not enough.

Phase one should preserve source-origin data as structured spans or stable references, not only labels.

Recommended minimum source-origin entry shape:

```csharp
public sealed record RazorVueSourceOrigin(
    RazorVueOriginKind OriginKind,
    string SourceFilePath,
    int SourceSpanStart,
    int SourceSpanLength,
    int StartLine,
    int StartColumn,
    string? GeneratedFilePath,
    int? GeneratedSpanStart,
    int? GeneratedSpanLength,
    RazorVueMappingQuality MappingQuality);
```

Recommended mapping quality values:

- `ExactSource`
- `MappedFromGenerated`
- `GeneratedOnly`

Phase one does not require perfect source recovery for every node,
but it must preserve whether a node has:

- exact `.razor` source identity
- generated-code-derived mapping
- only generated fallback identity

Without this distinction, later sourcemap and HMR work will still require redesign.

### 18.1.1 Source-origin provenance strategy

Phase one source-origin data should be produced through a layered provenance strategy.

Recommended order:

1. use Razor toolchain/source-mapping data when available
2. otherwise use generated C# syntax/operation locations tied to Razor-generated files
3. otherwise fall back to generated-only origin records

This means phase one does not promise exact `.razor` mapping for every node.
It does promise that every node records which provenance tier produced its origin data.

### 18.2 HMR reservation

Artifacts should also reserve stable identity information such as:

- `ComponentId`
- `ModuleId`
- `DescriptorHash`
- `TemplateHash`
- `LogicHash`
- `HmrBoundaryKind`

These are not phase-one runtime features, but they are phase-one structure requirements.

### 18.3 HMR/sourcemap must not redefine the main lowering

HMR and sourcemap must remain metadata extensions.

They should not become the main driver of:

- component semantic design
- render-tree extraction shape
- Vue lowering structure

## 19. `DenoHost` Boundary

Compiler responsibilities:

- semantic extraction
- Vue component generation
- contract generation
- artifact generation
- manifest generation

`DenoHost` responsibilities:

- dependency resolution
- unified compilation
- bundling
- runtime integration
- later HMR/sourcemap host behavior

Compiler and host responsibilities must remain distinct.

### 19.1 Source-origin sidecar is part of the boundary

The compiler/host boundary should allow source-origin data to cross phases without forcing `DenoHost` to reverse-engineer final JS text.

Phase-one recommendation:

- artifacts carry source-origin entries directly, or
- artifacts reference a compiler-owned sidecar such as `*.jzrmap.json`

The exact file format may evolve later.
The architectural requirement is that source-origin data remains compiler-owned and host-consumable.

## 20. Phase-one Scope

Phase one closes only the minimal loop:

- RazorVue component discovery
- `JazorComponent` / `VueComponent` constraints
- props/emits/slots extraction
- minimal render-tree recovery
- `@bind`, `@ref`, `@key`
- `if`, `foreach`
- lifecycle sugar lowering
- Vue render-function ESM emission
- artifact + manifest generation
- `DenoHost` consumption path

### 20.1 Current implementation checkpoint

The current implementation has already closed these parts of the loop:

- RazorVue component discovery
- `JazorComponent` / `VueComponent` constraints
- props / emits / slots extraction
- real Vue ESM artifact emission
- artifact + manifest generation
- emit-side host handoff shape for `DenoHost`

The current implementation has only a minimal render extraction/lowering subset proven in tests:

- `OpenElement`
- `CloseElement`
- `AddAttribute`
- `AddContent`
- `AddMarkupContent`
- simple parameter-backed template expressions

The current implementation does not yet claim complete support for:

- component-node lowering
- broad control-flow coverage
- lifecycle sugar lowering
- general component-member logic inside template expressions
- final sourcemap output
- runtime HMR behavior

Deferred work includes:

- full ecosystem integrations
- deep SSR/hydration strategy
- full HMR runtime
- full sourcemap emit
- `.vue` SFC output
- generic multi-framework abstractions

## 21. Design Conclusion

RazorVue is not "Razor plus a little Vue support".

It is a dedicated Vue-first compilation path that:

- reuses Razor as the authoring syntax
- uses analyzer over generated Razor component code
- extracts stable component contracts and render structure
- emits standard Vue ESM artifacts
- hands unified build ownership to `DenoHost`
