# RazorVue Pitfalls

This document records the most likely implementation mistakes for RazorVue.

Its purpose is to stop future work from drifting back into paths that feel familiar but are structurally wrong for the design already chosen.

Related documents:

- [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)
- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.HardRules.md](./RazorVue.HardRules.md)
- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)

## 1. Re-expanding into a generic UI abstraction

This is the biggest strategic trap.

Wrong direction:

- treat Razor as a neutral UI DSL
- try to unify React/Vue/Svelte too early
- introduce a framework-agnostic UI core as the first-class goal

Why it fails:

- Vue semantics get flattened too early
- scope explodes before the first loop is closed
- author experience becomes conceptually split

Correct direction:

- keep phase one Vue-first
- only extract shared ideas later, if the Vue path proves stable

## 2. Assuming `[ECMAScriptModule]` means one lowering path

Another common trap is treating the shared entry marker as shared lowering.

Wrong direction:

- detect `[ECMAScriptModule]`
- send everything into one compiler path

Why it fails:

- plain static modules and Razor components do not have the same input shape
- generated Razor component structure is fundamentally builder-driven

Correct direction:

- unify entry
- split lowering immediately

## 3. Fighting `ComponentBase`

Trying to avoid `ComponentBase` entirely can look cleaner on paper, but it is not the practical path here.

Wrong direction:

- make `JazorComponent` independent from `ComponentBase`
- try to detach Razor components from their actual technical substrate

Why it fails:

- Razor tooling is built around `ComponentBase`
- C# has no multiple inheritance
- the resulting author experience becomes unstable

Correct direction:

- accept `ComponentBase` as the technical base
- redefine semantics above it with `JazorComponent` and `VueComponent`

## 4. Turning `JazorComponent` into a second framework core

The inverse trap is also dangerous.

Wrong direction:

- keep adding behavior to `JazorComponent`
- let it grow into a scheduling/state/runtime host

Why it fails:

- responsibilities blur immediately
- `VueComponent` loses meaning
- compiler and runtime concepts collapse into one vague base class

Correct direction:

- keep `JazorComponent` thin
- put Vue-first author APIs on `VueComponent`

## 5. Making `VueComponent` APIs "generic enough for later"

This is an abstraction trap disguised as future-proofing.

Wrong direction:

- make `Ref/Reactive/Emit/Provide` generic and framework-neutral now

Why it fails:

- they stop being real Vue semantics
- implementation complexity grows
- phase one stops being about a usable Vue-first loop

Correct direction:

- let `VueComponent` APIs be explicitly Vue-shaped

## 6. Building the main path on source-generator ordering

This is one of the most likely implementation mistakes.

Wrong direction:

- assume Razor SG runs first
- assume another SG can safely consume its outputs

Why it fails:

- source-generator ordering is not the stable foundation for this pipeline
- the architecture becomes timing-sensitive instead of contract-driven

Correct direction:

- analyzer plus generated-code analysis for semantic extraction
- later build handoff to `DenoHost`

## 7. Treating generated Razor code like a normal user method body

Analyzer can see generated code, but that does not make generated Razor methods ordinary input.

Wrong direction:

- treat Razor generated methods like plain methods
- reuse the static-module lowering path

Why it fails:

- template semantics have already been lowered into builder patterns
- direct reuse of the static path loses too much structure

Correct direction:

- extract render-tree structure first
- lower that render tree into Vue semantics

## 8. Generating Vue code directly from `BuildRenderTree` operations

This looks fast, but it is structurally brittle.

Wrong direction:

- walk builder operations
- emit Vue code immediately

Why it fails:

- hard to test
- hard to debug
- hard to attach source origins
- fragile against changes in generated Razor patterns

Correct direction:

- build a minimal render-tree model first
- lower from that model into Vue

## 9. Guessing component contracts on demand

Another easy mistake:

- infer props/events/slots only while visiting a call site

Why it fails:

- slot resolution becomes unstable
- bind semantics drift
- ecosystem components are hard to integrate

Correct direction:

- extract a `VueComponentDescriptor`-style contract first
- use it consistently during lowering and call-site validation

## 10. Letting unknown component attributes silently pass through

This often feels convenient, especially early on.

Wrong direction:

- allow unknown component-side attrs to pass through to runtime

Why it fails:

- spelling mistakes become silent bugs
- contract extraction loses value
- diagnostics degrade sharply

Correct direction:

- be flexible for HTML where appropriate
- stay strict for component contracts

## 11. Letting inherited `ComponentBase` members redefine the model

Because `JazorComponent` inherits `ComponentBase`, certain members are visible.

Wrong direction:

- quietly support `StateHasChanged`
- quietly support `ShouldRender`
- quietly support `SetParametersAsync`

Why it fails:

- Vue-first semantics get pulled back toward Blazor scheduling
- author expectations split across two runtime models

Correct direction:

- diagnose these usages
- keep them out of the main RazorVue model

## 12. Deferring HMR and sourcemap thinking completely

This is one of the most expensive future traps.

Wrong direction:

- ignore HMR and sourcemap until phase two
- emit only final JS text plus one generic content hash
- preserve no origin chain

Why it fails:

- later sourcemap becomes reconstruction instead of build-time composition
- HMR degenerates into coarse reload behavior
- artifact structures must be redesigned

Correct direction:

- do not fully implement HMR/sourcemap now
- but reserve source-origin metadata and split artifact identity now

## 13. Letting HMR or sourcemap drive the entire phase-one shape

The opposite trap also exists.

Wrong direction:

- redesign core lowering around hypothetical HMR or sourcemap needs

Why it fails:

- the main loop is not even closed yet
- implementation cost spikes before phase one proves useful

Correct direction:

- HMR/sourcemap are metadata-first concerns in phase one
- they reserve structure, not define the main lowering

## 14. Splitting dependency ownership between compiler and `DenoHost`

This is a classic responsibility-boundary failure.

Wrong direction:

- compiler resolves part of dependency topology
- `DenoHost` resolves another part

Why it fails:

- behavior becomes hard to explain
- import/bundling rules drift
- later host features become harder to stabilize

Correct direction:

- compiler declares dependencies
- `DenoHost` resolves/builds them

## 15. Deep-integrating ecosystem packages too early

Wrong direction:

- start phase one by deeply supporting Vuetify/Router/Pinia

Why it fails:

- ecosystem details start driving core compiler architecture
- the main RazorVue loop remains unproven

Correct direction:

- finish the Vue-first main path first
- add ecosystem support as descriptor/registry-based extensions later

## 16. Over-supporting Razor syntax in phase one

Another frequent scope trap:

- attempt broad Razor compatibility immediately

Why it fails:

- extractor complexity rises before the main path stabilizes
- many unsupported corners distract from render-tree recovery and Vue lowering

Correct direction:

- support a small, explicit template subset first

## 17. Using only end-to-end tests

This slows debugging and hides where the real failures are.

Wrong direction:

- rely only on final emitted Vue modules or host-level tests

Why it fails:

- impossible to tell whether failures came from:
  - discovery
  - contract extraction
  - logic extraction
  - render-tree extraction
  - lowering
  - emission

Correct direction:

- keep layered tests across each compiler stage

## 18. Letting phase-one scope expand while implementing

The biggest delivery risk is not technical impossibility.
It is uncontrolled scope growth.

Wrong direction:

- add more syntax
- add more runtime behaviors
- add HMR
- add sourcemap
- add ecosystem packs
- add SSR
- add `.vue` output
before the minimal loop is closed

Why it fails:

- every stage remains half-finished
- architecture drifts before it proves itself

Correct direction:

- enforce the minimal loop
- keep delayed items explicitly delayed

## 19. Final Reminder

RazorVue will fail primarily if implementation keeps drifting back into:

- generic abstraction
- timing-dependent generator assumptions
- static-module reuse
- blurred host/compiler ownership
- metadata loss for future HMR/sourcemap

The safest path is not the one with the most features.
It is the one that preserves the boundaries already decided.
