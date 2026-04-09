# RazorVue HMR Design

> Status: active reference
> Positioning: Reserved-lane design reference for RazorVue HMR; not an active implementation plan.

This document defines the RazorVue HMR design.

It is a pre-implementation design document.
The current repository does not yet contain a complete RazorVue HMR runtime.

This document exists to:

1. define what HMR must solve for RazorVue
2. fix the compiler/host responsibility split
3. define stable identity and change classification
4. prevent HMR from warping the main RazorVue pipeline

Related documents:

- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)
- [RazorVue.ComponentDescriptorSpec.md](./RazorVue.ComponentDescriptorSpec.md)
- [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)

## 1. Goals

RazorVue HMR exists to support fast frontend iteration without weakening compiler and host boundaries.

The design goals are:

- keep HMR aligned with Vue-first component semantics
- make change classification compiler-owned
- keep runtime application ownership with `DenoHost`
- preserve deterministic artifact identity across builds
- allow conservative fallback when safety is unclear

## 2. Non-goals

Phase one HMR is explicitly not trying to do the following:

1. fully implement runtime hot-update behavior
2. preserve all component local state across every update
3. rebuild host-owned frontend/runtime internals inside Jazor.Compiler
4. infer HMR behavior from final JS strings alone
5. make every Vue ecosystem library hot-reload-aware on day one

## 3. Positioning

HMR is a cross-boundary capability.

Compiler owns:

- identity
- change classification
- update safety metadata

`DenoHost` owns:

- module invalidation
- runtime update transport
- browser/runtime reload strategy
- fallback escalation to full page reload

This separation must remain stable.

## 4. Why HMR Must Be Designed Early

If HMR is deferred as a runtime-only concern,
the pipeline will quickly lose the data it needs:

- stable component identity
- change-category boundaries
- segment ownership
- source-origin links for update diagnostics

That would force later redesign in:

- artifacts
- manifest shape
- descriptor identity
- lowering output ownership

Phase one therefore reserves HMR structurally even without runtime implementation.

## 5. Identity Model

RazorVue HMR needs stable identity across equivalent builds.

Recommended minimum identity shape:

```csharp
public sealed record VueArtifactIdentity(
    string ComponentId,
    string ModuleId,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    HmrBoundaryKind HmrBoundaryKind);
```

### 5.1 `ComponentId`

`ComponentId` identifies the semantic component.

It should be stable across equivalent rebuilds and should not depend on:

- temporary output file names
- bundle chunk renaming
- runtime session identifiers

Recommended inputs:

- assembly identity
- namespace-qualified component identity
- compiler path normalization

### 5.2 `ModuleId`

`ModuleId` identifies the emitted ESM module unit.

It should be stable for host/runtime module invalidation.

It may differ from `ComponentId` when one component can later materialize into multiple modules,
but phase one may keep them closely aligned.

### 5.3 Split hashes

The compiler must preserve at least three independent change hashes:

- `DescriptorHash`
- `TemplateHash`
- `LogicHash`

Reason:

- public contract changes affect consumers
- template-only changes affect render output
- logic changes affect setup/runtime behavior

Collapsing these into one hash removes the ability to classify updates safely.

## 6. Change Classification

RazorVue HMR is driven by compiler-owned change categories.

Recommended first boundary enum:

```csharp
public enum HmrBoundaryKind
{
    Unknown,
    TemplateOnly,
    LogicSafe,
    FullReloadRequired
}
```

### 6.1 `TemplateOnly`

Use when:

- render structure changed
- public contract stayed the same
- logic surface stayed the same

This is the most promising HMR-safe category.

### 6.2 `LogicSafe`

Use only when the compiler can conservatively prove:

- descriptor/public contract stayed stable
- logic changed within supported hot-update-safe bounds
- runtime can re-run supported update hooks safely

This category should be introduced conservatively.
If proof is weak, classify as full reload.

### 6.3 `FullReloadRequired`

Use when:

- props/emits/slots/model contract changed
- update safety is unclear
- library/runtime integration declares incompatibility
- compiler cannot preserve a stable hot boundary

This is not failure.
It is the safe fallback.

## 7. Relationship with Component Descriptor

The descriptor is part of the HMR contract, not only template compilation metadata.

Descriptor changes usually imply:

- public contract drift
- caller/callee compatibility risk
- wider invalidation scope

Therefore `VueComponentDescriptor` must participate in identity through `DescriptorHash`.

Descriptor examples that should generally force full reload:

- prop added/removed/renamed
- emit name contract changed
- slot contract changed
- bind/model pair changed

## 8. Relationship with Template Lowering

Template lowering must expose stable ownership for the template segment of a component.

That does not mean:

- diffing final JS strings
- encoding HMR logic inside render emission

It means the lowering pipeline must preserve enough structure to say:

- this template changed
- this template belongs to this component/module
- this template still targets the same descriptor boundary

## 9. Relationship with Logic Extraction

Logic extraction should also expose a stable identity surface.

Important categories include:

- fields participating in setup state
- lifecycle sugar lowering
- explicit `Emit`, `Provide`, `Inject`, `Expose`
- Vue composable-style author APIs on `VueComponent`

Not every logic change will be HMR-safe.

Phase-one design rule:

- preserve `LogicHash`
- allow future finer logic segmentation
- do not promise safe live patching for all logic changes

## 10. Source-origin and HMR

HMR and sourcemap are different outputs,
but they share the same source-origin prerequisites.

HMR needs source-origin data for:

- update diagnostics
- developer-facing reload explanations
- future overlay/debug tooling

Therefore source-origin metadata should survive at least to artifact or sidecar level.

HMR must not require exact `.razor` mapping for every node in phase one,
but it must be able to say whether a hot decision came from:

- exact Razor-backed source
- generated-code-derived mapping
- generated-only fallback

## 11. `DenoHost` Contract

`DenoHost` should consume HMR-relevant metadata instead of rediscovering it.

Recommended host-facing fields include:

- `ComponentId`
- `ModuleId`
- `DescriptorHash`
- `TemplateHash`
- `LogicHash`
- `HmrBoundaryKind`
- optional source-origin sidecar reference

Recommended host responsibilities:

1. receive artifact/manifest updates
2. compare old and new identity records
3. decide template patch, logic patch, or full reload path
4. surface developer diagnostics for conservative fallback

`DenoHost` should not need to reinterpret Razor semantics to do this.

## 12. UI Library Extensions

Vue UI libraries may need extra HMR metadata.

Examples:

- a component library may require style dependency invalidation
- a plugin may mark certain wrappers as always full reload
- a descriptor registry may declare whether a component is transparent to template-only updates

But these are extensions.

Core HMR must still be expressed in base compiler-owned metadata.

## 13. First Implementation Shape

The first HMR implementation lane should stay narrow.

### 13.1 Phase one reservation

Required:

- identity fields exist
- split hashes exist
- `HmrBoundaryKind` exists
- host manifest can carry them

Not required:

- actual live module patching
- browser runtime protocol
- state-preserving component replacement

### 13.2 Phase two runtime proof

Only after artifact identity is stable should the project attempt:

- runtime invalidation wiring in `DenoHost`
- conservative template-only update path
- explicit full reload fallback path

### 13.3 Phase three ecosystem refinement

Only after the core path is stable should the project attempt:

- library-specific HMR hints
- more granular logic-safe updates
- better developer diagnostics and tooling

## 14. Validation Strategy

HMR design should be validated in layers:

1. identity stability tests
2. hash split tests
3. boundary classification tests
4. host manifest compatibility tests
5. later runtime behavior tests

Do not jump directly to runtime demos before the identity model is proven.

## 15. Design Conclusion

RazorVue HMR should be built as a conservative compiler-and-host contract.

The compiler classifies change and preserves stable identity.
`DenoHost` applies updates.
When safety is unclear, the system falls back to full reload instead of pretending every change is hot-safe.
