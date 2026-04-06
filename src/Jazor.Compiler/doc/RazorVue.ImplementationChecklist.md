# RazorVue Implementation Checklist

> Status: Active phase-one implementation artifact.
> Positioning: Primary execution checklist for the RazorVue phase-one lane.
> Note: Use this as staged implementation guidance; checklist items may mix completed, partial, and still-open slices.

This document breaks the RazorVue design into execution phases.

It is not meant to repeat design reasoning.
Its purpose is to turn the RazorVue design into a sequence of implementable steps with clear acceptance gates.

Related documents:

- [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)
- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.HardRules.md](./RazorVue.HardRules.md)
- [RazorVue.Pitfalls.md](./RazorVue.Pitfalls.md)

## 1. Preconditions

Do not start implementation before these are true:

1. plain `[ECMAScriptModule] static class` flow is still working
2. `JazorComponent` / `VueComponent` hierarchy is accepted
3. analyzer-based extraction is accepted as the main semantic-entry path
4. `DenoHost` is accepted as the unified build host
5. phase-one scope is explicitly limited

If these prerequisites are unstable, implementation will drift.

## 2. Phase-one Success Criteria

Phase one is considered complete only when the project can:

1. discover `[ECMAScriptModule]` Razor components
2. enforce `JazorComponent` / `VueComponent` constraints
3. extract props / emits / slots / bind metadata
4. recover a minimal render-tree model from `BuildRenderTree`
5. lower that model into Vue `defineComponent + render`
6. emit a `DenoHost`-consumable manifest
7. preserve minimal source-origin metadata
8. preserve minimal HMR identity metadata

## 2.1 Current Progress Snapshot

The repository has already crossed the following milestone boundary:

- P0 foundation and migration boundary
- P1 discovery and first Roslyn diagnostics (`JAZORVUE001`, `JAZORVUE002`, `JAZORVUE004`, `JAZORVUE005`, `JAZORVUE006`)
- P2 component contract extraction
- the layering refactor that moved RazorVue core semantic ownership into `Jazor.RazorVue` and kept `Jazor.RazorVue.Analysis` as a thin Roslyn host
- the main semantic carrier/orchestration path: `RazorVueCompilationContext` -> `RazorVueSemanticSnapshot` -> `RazorVuePipeline` -> `RazorVueArtifactFactory` -> `RazorVueCatalog`
- P6 artifact emission
- emit-side materialization and manifest transition for `RazorVueCatalog`

The repository has partially completed:

- P4 minimal `BuildRenderTree` extraction
- P5 Razor -> Vue lowering
- structured generator diagnostics beyond the fallback surface

The currently proven lowering subset is:

- HTML elements
- component happy path with component-node lowering
- props
- emit/listener wiring
- text nodes
- simple expressions backed by parameter properties
- default slot fallback
- named slot wiring
- scoped slot wiring
- minimal `if` / `foreach` structural lowering
- lifecycle safe-subset lowering for `OnInitialized*`, `OnParametersSet*`, and `OnAfterRender*`
- `OnParametersSet*` immediate watch bridging
- `OnAfterRender*` explicit `firstRender` bridging
- minimal setup-side logic lowering for simple instance fields and zero-arg helper methods

The following checklist items remain effectively open even if some scaffolding exists:

- broader logic extraction outside the current lifecycle/EventCallback/setup-field/helper safe subset
- full component-instance semantics
- `Dispose*`, `ShouldRender`, and `SetParametersAsync` runtime-equivalent lowering
- broader control-flow coverage validation
- comprehensive Razor syntax coverage validation
- final `DenoHost` end-to-end integration
- final HMR/sourcemap outputs

The current fallback for unsupported analysis/lowering shapes is still `JAZORVGA001` from `RazorVueGenerator` in the general case. The current thin `Jazor.RazorVue.Analysis` host path also projects structured issue diagnostics for `JAZORVGA002` (component not found), `JAZORVGA003` (ambiguous short component name), `JAZORVGA004` (reserved intrinsic-name collision), `JAZORVGA005` (unsupported lifecycle lowering), and `JAZORVGA006` (unsupported setup-side logic lowering).

## 3. P0. Foundation and Constraints

### 3.1 Define semantic carrier and host migration boundary

Tasks:

- define the compiler-owned semantic carrier:
  - `RazorVueSemanticSnapshot`
  - `VueCompiledArtifact`
  - `RazorVueCatalog` or equivalent
- define how RazorVue output coexists with the current `ModuleCatalog` / `Jazor.Emit` path
- define which layer produces and consumes each carrier

Acceptance:

- semantic extraction does not depend on hidden analyzer state
- build-facing emission has a concrete carrier
- current static-module flow remains intact during transition
- stage ownership is explicit before implementation starts

### 3.2 Add base classes

Tasks:

- add `JazorComponent : ComponentBase`
- add `VueComponent : JazorComponent`

Acceptance:

- hierarchy compiles
- `JazorComponent` remains thin
- `VueComponent` becomes the stable host for Vue-first APIs

### 3.3 Add analyzer shell

Tasks:

- add RazorVue analyzer entry
- enable generated code analysis
- split plain static-module rules from RazorVue rules
- cache compilation-level symbols for:
  - `ECMAScriptModuleAttribute`
  - `JazorComponent`
  - `VueComponent`

Acceptance:

- analyzer can recognize core entry symbols
- valid RazorVue component symbols are not rejected by the plain static-module rule set

### 3.4 Define source-origin contract

Tasks:

- define source-origin entry shape
- preserve:
  - original `.razor` file path when known
  - source span or stable segment identity
  - generated fallback span
  - mapping quality
- define provenance tiers:
  - Razor source mapping
  - generated syntax location
  - generated fallback

Acceptance:

- source-origin metadata is more than category labels
- later sourcemap/HMR work does not require redesigning origin storage
- provenance quality is explicit

### 3.5 Define component resolution rules

Tasks:

- define `using`-driven visibility rules
- define intrinsic-name reservation
- define ambiguity diagnostics
- define phase-one disambiguation syntax

Acceptance:

- library adoption stays `using`-based
- component name conflicts are deterministic
- fully-qualified component names are the required phase-one ambiguity escape

### 3.6 Add doc index entry

Tasks:

- add RazorVue docs to `doc/README.md`

Acceptance:

- RazorVue docs become discoverable from the compiler doc index

## 4. P1. Discovery and Diagnostics

### 4.1 Implement component discovery

Tasks:

- detect `[ECMAScriptModule] static class`
- detect `[ECMAScriptModule] JazorComponent` descendants
- detect invalid inputs

Acceptance:

- entry splitting is stable

### 4.2 Add entry diagnostics

Tasks:

- diagnose `[ECMAScriptModule]` Razor component not inheriting `JazorComponent`
- diagnose direct `ComponentBase` usage in RazorVue entry
- diagnose clearly invalid entry shapes

Acceptance:

- invalid entry cases are caught before lowering begins

### 4.3 Add misuse diagnostics

Tasks:

- diagnose usage of:
  - `StateHasChanged`
  - `ShouldRender`
  - `SetParametersAsync`
- diagnose obvious bind/property conflicts

Acceptance:

- top-priority invalid patterns are covered

### 4.4 Tests

Tasks:

- add analyzer tests for:
  - valid `VueComponent`
  - valid static module
  - invalid plain class
  - invalid `ComponentBase` inheritance
  - valid RazorVue symbols not accepted by plain static-module rules
  - generated-code RazorVue discovery

Acceptance:

- discovery/diagnostic path has regression protection

## 5. P2. Component Contract Extraction

### 5.1 Define descriptor structures

Tasks:

- define `VueComponentDescriptor`
- define prop / emit / slot / flag descriptor models

Acceptance:

- contract model is stable enough for later phases

### 5.2 Extract props

Tasks:

- extract normal `[Parameter]` properties
- support required/default/basic type metadata

Acceptance:

- prop extraction is deterministic

### 5.3 Extract emits

Tasks:

- extract `EventCallback`
- extract `EventCallback<T>`
- define `OnXxx -> xxx` mapping

Acceptance:

- emit contract is available without template lowering

### 5.4 Extract slots

Tasks:

- map `RenderFragment` to default/named slot
- map `RenderFragment<T>` to scoped slot

Acceptance:

- slot contract is available without template lowering

### 5.5 Extract model/bind metadata

Tasks:

- detect `Foo + FooChanged`
- reserve Vue model/update metadata

Acceptance:

- component-side `@bind` contract is known

### 5.6 Extract explicit emits from logic

Tasks:

- inspect `Emit("...")` usage where practical
- augment emit contract

Acceptance:

- explicit emit-only channels are not lost

### 5.7 Tests

Tasks:

- add contract extraction tests
- add component ambiguity tests
- add `using`-based visibility tests

Acceptance:

- props/emits/slots/model metadata have regression protection

## 6. P3. Logic Extraction

### 6.1 Extract fields and methods

Tasks:

- extract normal fields
- extract normal methods
- identify Vue-first helper usage in members

Acceptance:

- setup-side logic input exists independently from render extraction

### 6.2 Extract state-like constructs

Tasks:

- identify:
  - `Ref`
  - `Reactive`
  - `Computed`
  - `TemplateRef`

Acceptance:

- minimal state setup information is available

### 6.3 Extract lifecycle sugar

Tasks:

- identify:
  - `OnInitialized*`
  - `OnParametersSet*`
  - `OnAfterRender*`
  - `Dispose*`

Acceptance:

- lifecycle lowering inputs exist

### 6.4 Extract Vue-first APIs

Tasks:

- identify:
  - `Emit`
  - `Provide`
  - `Inject`
  - `Expose`
  - `Watch`
  - `WatchEffect`
  - `NextTick`

Acceptance:

- Vue-first authoring APIs are visible to lowering

### 6.5 Preserve source origin for logic

Tasks:

- attach `component-logic` source-origin metadata to extracted logical bindings

Acceptance:

- logic extraction no longer produces anonymous opaque nodes

### 6.6 Tests

Tasks:

- add logic extraction tests

Acceptance:

- lifecycle/state/api extraction is covered

## 7. P4. Minimal `BuildRenderTree` Extraction

### 7.1 Define minimal render-tree model

Tasks:

- define nodes for:
  - element
  - component
  - text
  - expression
  - conditional
  - loop
  - attribute
  - slot content

Acceptance:

- render lowering does not depend on raw operation walking

### 7.2 Recognize minimal builder patterns

Tasks:

- recognize:
  - `OpenElement`
  - `CloseElement`
  - `OpenComponent`
  - `CloseComponent`
  - `AddAttribute`
  - `AddContent`

Acceptance:

- smallest supported Razor template subset can be reconstructed

### 7.3 Support minimal template structures

Tasks:

- handle:
  - plain HTML nodes
  - component nodes
  - basic child content
  - `if`
  - `foreach`

Acceptance:

- minimal templates no longer require direct operation-to-string emission

### 7.4 Preserve source origin for render tree

Tasks:

- attach at least:
  - `razor-template`
  - `generated-render`
  metadata to render-tree nodes
- preserve exact source span or mapping-quality fallback

Acceptance:

- later sourcemap support has a source-origin chain to build on

### 7.5 Tests

Tasks:

- add minimal render-tree extraction tests

Acceptance:

- render-tree extraction is regression-protected

## 8. P5. Razor -> Vue Lowering

### 8.1 Define Vue component model

Tasks:

- define a Vue lowering model that includes:
  - descriptor
  - setup bindings
  - lifecycle bindings
  - render nodes
  - import/style requirements

Acceptance:

- lowering targets a stable model, not direct string output

### 8.2 Lower HTML elements

Tasks:

- support:
  - native attrs
  - DOM events
  - DOM `@bind`
  - `@ref`
  - `@key`

Acceptance:

- HTML nodes lower to Vue `h("tag", ...)`

### 8.3 Lower component nodes

Tasks:

- support:
  - prop matching
  - emit listeners
  - component `@bind-*`
  - slots
  - scoped slots
  - `@ref`
  - `@key`

Acceptance:

- component nodes lower to `h(Component, props, slots)`

### 8.4 Lower structure nodes

Tasks:

- support:
  - `if`
  - `foreach`

Acceptance:

- minimal control-flow structures lower correctly

### 8.5 Lower lifecycle sugar

Tasks:

- lower:
  - `OnInitialized*`
  - `OnParametersSet*`
  - `OnAfterRender*`
  - `Dispose*`

Acceptance:

- Vue lifecycle/watch equivalents are generated

### 8.6 Preserve source origin through lowering

Tasks:

- ensure source-origin metadata survives through Vue lowering nodes

Acceptance:

- origin chain does not stop at render-tree extraction

### 8.7 Tests

Tasks:

- add Vue lowering tests

Acceptance:

- lowering shape is protected by regression tests or snapshots

## 9. P6. Artifact Emission

### 9.1 Define artifact structure

Tasks:

- define output fields for:
  - component name
  - relative module path
  - module code
  - imports
  - styles
  - content hash
  - hints

Acceptance:

- artifact model is stable enough for host consumption
- artifact model is explicitly separated from the final file-writing step

### 9.2 Emit standard Vue ESM

Tasks:

- emit:
  - `defineComponent`
  - `setup`
  - render function

Acceptance:

- output is readable and deterministic

### 9.3 Emit descriptor provider or equivalent metadata

Tasks:

- generate descriptor provider source or an equivalent discoverable carrier

Acceptance:

- other components can consume component contracts

### 9.4 Reserve HMR identity data

Tasks:

- add:
  - `ComponentId`
  - `ModuleId`
  - `DescriptorHash`
  - `TemplateHash`
  - `LogicHash`
  - `HmrBoundaryKind`

Acceptance:

- artifact identity is split, not collapsed into one generic hash

### 9.5 Preserve source-origin handoff in artifacts

Tasks:

- ensure artifacts expose the minimum hooks/structures later sourcemap build will need
- choose direct embedded source origins or sidecar origin map output

Acceptance:

- source-origin metadata is not discarded before host handoff

### 9.6 Tests

Tasks:

- add artifact emission tests

Acceptance:

- module output, imports, hashes, and metadata are regression-protected

## 10. P7. `DenoHost` Integration

### 10.1 Define host manifest contract

Tasks:

- define manifest fields for:
  - component name
  - relative path
  - imports
  - styles
  - hashes
  - runtime hints

Acceptance:

- host-side consumption contract is explicit

### 10.2 Integrate build handoff

Tasks:

- materialize emitted artifacts/manifests in a build-facing stage
- hand those materialized outputs to `DenoHost`
- update host/emission flow to consume the new RazorVue carrier alongside the current static-module carrier during transition

Acceptance:

- `DenoHost` can consume RazorVue outputs
- current static-module bundling still works

### 10.3 Minimal end-to-end validation

Tasks:

- run one minimal RazorVue component through the whole path

Acceptance:

- compiler plus host close the first usable loop

## 11. P8. Deferred Work

The following stay out of the phase-one milestone:

- deep Vuetify integration
- router/pinia integration
- full intrinsic support matrix
- SSR/hydration strategy refinement
- full sourcemap emit
- HMR runtime behavior
- `.vue` SFC output
- generic multi-framework abstractions

Management rule for this phase:

- do not begin ecosystem-deep implementation work before the phase-one completion gate is met

## 12. Test Strategy

Phase one testing should be layered:

1. analyzer discovery and diagnostics
2. contract extraction
3. logic extraction
4. render-tree extraction
5. Vue lowering
6. artifact emission
7. minimal host integration

Do not rely on end-to-end tests alone.

## 13. Phase-one Completion Gate

Phase one is complete only when all of the following are true:

1. `[ECMAScriptModule]` RazorVue component discovery is stable.
2. `JazorComponent` / `VueComponent` constraints are enforced.
3. props/emits/slots/model metadata are extracted.
4. minimal render-tree recovery works.
5. Vue `defineComponent + render` ESM is emitted.
6. `DenoHost` can consume the manifest.
7. source-origin metadata survives the main pipeline.
8. artifact identity is split enough for later HMR.
9. the main path is covered by regression tests.

## 14. Conclusion

This checklist is not about implementing everything quickly.
It is about ensuring RazorVue can be built in a sequence that preserves architecture, diagnostics, metadata, and future extensibility without reopening settled design decisions.
