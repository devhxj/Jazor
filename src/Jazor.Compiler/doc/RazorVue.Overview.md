# RazorVue Overview

> Status: active reference
> Positioning: Main deep-doc entry point for the active RazorVue document set.

## 1. Document Position

This document is the entry point for the RazorVue document set.

It does not repeat the full design.
It answers three questions:

1. what state RazorVue is currently in
2. what each RazorVue document is for
3. what order to read them in when work resumes

## 2. Current State

The current state of RazorVue is:

- the core main-path milestone is already landed
- the current stage is phase-one closure work around the minimal path
- phase-one scope remains intentionally limited
- HMR and sourcemap remain metadata-first, reserved for later milestones
- the current logic lane is still a conservative subset, but it now includes lifecycle safe-subset lowering plus a minimal setup-side logic closure for simple fields and zero-arg helpers

As of the current implementation lane, the repository already has:

- `[ECMAScriptModule]` entry split between static modules and RazorVue components
- `Jazor.Razor` / `Jazor.RazorVue` / `Jazor.RazorVue.Analysis` split, with `Jazor.RazorVue` now owning the RazorVue core semantic lane and `Jazor.RazorVue.Analysis` remaining a thin Roslyn host
- Roslyn analyzers for the current RazorVue entry/misuse set:
  - `JAZORVUE001` invalid entry inheritance
  - `JAZORVUE002` direct `ComponentBase` entry
  - `JAZORVUE004` `StateHasChanged`
  - `JAZORVUE005` `ShouldRender`
  - `JAZORVUE006` `SetParametersAsync`
- component descriptor extraction for props / emits / slots
- `RazorVueCompilationContext` -> `RazorVueSemanticSnapshot` -> `RazorVuePipeline` -> `RazorVueArtifactFactory` -> `RazorVueCatalog` main path
- a real `BuildRenderTree` extraction/lowering lane that can emit Vue `defineComponent + setup + render`
- proven component-node lowering for props, emit/listener wiring, and default / named / scoped slot flow
- minimal structural lowering for `if` and `foreach`
- lifecycle safe-subset lowering for `OnInitialized*`, `OnParametersSet*`, and `OnAfterRender*`, including `watch(..., { immediate: true })` and `firstRender` bridging
- minimal setup-side logic lowering for simple instance fields and zero-arg helper methods that can be projected safely into `setup()`
- artifact identity/hash shaping and basic HMR boundary classification

The following are still not complete phase-one coverage:

- broader logic extraction beyond the current lifecycle/event-callback/setup-field/helper safe subset
- full component-instance semantics
- comprehensive Razor syntax coverage
- `Dispose*`, `ShouldRender`, and `SetParametersAsync` runtime-equivalent handling
- final `DenoHost` end-to-end integration
- final HMR runtime and sourcemap emission

When analysis/lowering hits unsupported shapes, `JAZORVGA001` (`RazorVue catalog generation failed`) remains the general fallback surface. The current thin `Jazor.RazorVue.Analysis` host path also has structured compiler-facing issue projection for:

- `JAZORVGA002` component not found
- `JAZORVGA003` ambiguous short component name
- `JAZORVGA004` reserved intrinsic-name collision
- `JAZORVGA005` unsupported lifecycle lowering
- `JAZORVGA006` unsupported setup-side logic lowering

Current stage memo:

- [RazorVue 阶段评估（2026-04-06）](../../../docs/status/2026-04-06-razorvue-stage-assessment.md)

Current consensus is:

1. keep RazorVue Vue-first
2. use analyzer plus generated code analysis for semantic extraction
3. keep final build ownership with `DenoHost`
4. close the minimal main path before ecosystem expansion

## 3. Core Conclusions

The main fixed conclusions are:

1. RazorVue is Vue-first, not a generic multi-framework UI abstraction.
2. `[ECMAScriptModule]` remains the unified entry marker.
3. Razor components must inherit `JazorComponent`.
4. The base hierarchy is `ComponentBase -> JazorComponent -> VueComponent`.
5. Final public project split is `Jazor.Compiler` + `Jazor.Razor` + `Jazor.RazorVue` + `Jazor.RazorVue.Analysis`.
6. RazorVue does not build its main path on source-generator ordering.
7. Razor components do not reuse plain static-module lowering.
8. The compiler emits Vue ESM artifacts and `DenoHost` owns the unified build.
9. HMR and sourcemap are reserved in architecture through metadata, not fully implemented as phase-one runtime features.

## 4. Document Roles

### 4.1 Quick conclusions

- [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)

Use it when:

- you want the final decisions only
- you do not want to reread the full design

### 4.2 Full design

- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.ProjectResponsibilities.md](./RazorVue.ProjectResponsibilities.md)

Use it when:

- you need architecture, boundaries, and responsibilities
- you need to understand why the design is shaped this way
- you need the current project split and extension seam

### 4.3 Focused specs

- [RazorVue.ComponentDescriptorSpec.md](./RazorVue.ComponentDescriptorSpec.md)
- [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)
- [RazorVue.ImplementationSkeleton.md](./RazorVue.ImplementationSkeleton.md)
- [RazorVue.LibraryAuthoring.Design.md](./RazorVue.LibraryAuthoring.Design.md)
- [RazorVue.Vuetify.FirstPackage.md](./RazorVue.Vuetify.FirstPackage.md)

Use them when:

- you are implementing contract extraction
- you are implementing host-facing artifact/manifest flow
- you need narrower, implementation-facing specs than the main design doc
- you are defining the first library-authoring package shape

### 4.4 Stage assessment

- [RazorVue 阶段评估（2026-04-06）](../../../docs/status/2026-04-06-razorvue-stage-assessment.md)

Use it when:

- you need a dated checkpoint of the current design/implementation state
- you want completed / partial / open work in one place
- you need a resume memo before the next implementation slice

### 4.5 Review memo

- [RazorVue.Review.md](./RazorVue.Review.md)

Use it when:

- you want the double-pass review result
- you want the remaining risks and immediate next step in one place

### 4.6 HMR package

- [RazorVue.Hmr.Overview.md](./RazorVue.Hmr.Overview.md)
- [RazorVue.Hmr.DecisionSummary.md](./RazorVue.Hmr.DecisionSummary.md)
- [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)
- [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)
- [RazorVue.Hmr.ImplementationChecklist.md](./RazorVue.Hmr.ImplementationChecklist.md)
- [RazorVue.Hmr.Pitfalls.md](./RazorVue.Hmr.Pitfalls.md)

Use them when:

- you are preparing future HMR support
- you need the reserved identity/change model
- you need the compiler/`DenoHost` HMR boundary

### 4.7 Hard constraints

- [RazorVue.HardRules.md](./RazorVue.HardRules.md)

Use it when:

- you need review rules
- you need to know what cannot be decided ad hoc during implementation

### 4.8 Implementation sequencing

- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
- [RazorVue.ImplementationSkeleton.md](./RazorVue.ImplementationSkeleton.md)
- [RazorVue.FirstPrPlan.md](./RazorVue.FirstPrPlan.md) (historical context)

Use it when:

- implementation is actually starting
- you need phased execution and acceptance gates

### 4.9 Common failure modes

- [RazorVue.Pitfalls.md](./RazorVue.Pitfalls.md)

Use it when:

- you want to avoid architecture drift
- you are reviewing changes and want to catch familiar wrong turns early

### 4.10 Authoring product direction

- [RazorVue.Authoring.ProductDefinition.md](./RazorVue.Authoring.ProductDefinition.md)
- [2026-04-06-razorvue-v1-authoring-roadmap.md](../../../docs/superpowers/plans/2026-04-06-razorvue-v1-authoring-roadmap.md)
- [2026-04-06-razorvue-v1-authoring-pr-breakdown.md](../../../docs/superpowers/plans/2026-04-06-razorvue-v1-authoring-pr-breakdown.md)

Use them when:

- you are defining the C# authoring experience for RazorVue
- you are planning library-wrapper work such as Vuetify
- you need the staged execution plan for the authoring lane
- you need execution-sized PR scopes for the authoring lane

## 5. Recommended Reading Order

### 5.1 If you only want the final direction

Read in this order:

1. [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)
2. [RazorVue.HardRules.md](./RazorVue.HardRules.md)

### 5.2 If you are about to implement

Read in this order:

1. [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)
2. [RazorVue.Design.md](./RazorVue.Design.md)
3. [RazorVue.ComponentDescriptorSpec.md](./RazorVue.ComponentDescriptorSpec.md)
4. [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)
5. [RazorVue.Hmr.DecisionSummary.md](./RazorVue.Hmr.DecisionSummary.md)
6. [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)
7. [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)
8. [RazorVue.ImplementationSkeleton.md](./RazorVue.ImplementationSkeleton.md)
9. [RazorVue.FirstPrPlan.md](./RazorVue.FirstPrPlan.md) (historical context)
10. [RazorVue.HardRules.md](./RazorVue.HardRules.md)
11. [RazorVue.Pitfalls.md](./RazorVue.Pitfalls.md)
12. [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
13. [2026-04-06-razorvue-v1-authoring-roadmap.md](../../../docs/superpowers/plans/2026-04-06-razorvue-v1-authoring-roadmap.md)
14. [2026-04-06-razorvue-v1-authoring-pr-breakdown.md](../../../docs/superpowers/plans/2026-04-06-razorvue-v1-authoring-pr-breakdown.md)

### 5.3 If you are reviewing code/design

Read in this order:

1. [RazorVue.HardRules.md](./RazorVue.HardRules.md)
2. [RazorVue.Pitfalls.md](./RazorVue.Pitfalls.md)
3. [RazorVue.Design.md](./RazorVue.Design.md)
4. [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)
5. [RazorVue.Hmr.Pitfalls.md](./RazorVue.Hmr.Pitfalls.md)
6. [RazorVue.ComponentDescriptorSpec.md](./RazorVue.ComponentDescriptorSpec.md)
7. [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)

## 6. What Must Be Reconfirmed Before The Next Implementation Stage

Before the next implementation stage starts, re-check at least:

1. `JazorComponent` / `VueComponent` API surface is still aligned with current goals
2. analyzer remains the accepted main semantic extraction point
3. the plain static-module path does not need major entry refactoring first
4. `DenoHost` manifest expectations are still compatible with the planned artifact model
5. HMR/sourcemap reservation requirements are still present in the implementation plan
6. HMR identity and boundary classification are still aligned with the reserved artifact model

## 7. One-line Conclusion

If you need to resume RazorVue later, start here, then read:

1. [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)
2. [RazorVue.HardRules.md](./RazorVue.HardRules.md)
3. [RazorVue.Hmr.DecisionSummary.md](./RazorVue.Hmr.DecisionSummary.md)
4. [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
