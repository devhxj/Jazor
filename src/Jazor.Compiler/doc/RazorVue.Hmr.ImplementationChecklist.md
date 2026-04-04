# RazorVue HMR Implementation Checklist

> Status: Architecture reserved, not fully implemented.
> Positioning: Phased checklist for future RazorVue HMR work.
> Note: HMR structure and metadata are intentionally reserved early, but runtime behavior and full implementation remain deferred beyond the current phase-one lane.

This document turns the RazorVue HMR design into an execution checklist.

It is intentionally phased.
It does not assume runtime HMR should be implemented immediately.

Related documents:

- [RazorVue.Hmr.DecisionSummary.md](./RazorVue.Hmr.DecisionSummary.md)
- [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)
- [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)
- [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)

## 1. Preconditions

Do not start real HMR work until all of the following are true:

1. RazorVue entry split is stable
2. component descriptor extraction is stable
3. render-tree extraction is stable enough to produce deterministic template output
4. artifact emission already exists
5. host-facing manifest materialization already exists

If these are not true, HMR work will become architecture churn instead of feature delivery.

## 2. Phase 1. Metadata Reservation

Goal:

- make HMR structurally possible without runtime implementation

Required outputs:

- `ComponentId`
- `ModuleId`
- `DescriptorHash`
- `TemplateHash`
- `LogicHash`
- `HmrBoundaryKind`

Checklist:

- add identity fields to compiler artifact model
- add HMR fields to host-facing manifest model
- make paths deterministic enough for stable `ModuleId`
- make descriptor/template/logic hashing deterministic
- reserve optional origin-sidecar association for diagnostics

Acceptance:

- equivalent builds produce stable identity
- split hashes are preserved through materialization
- `DenoHost` can read the metadata

## 3. Phase 2. Classification Shell

Goal:

- let the compiler classify change categories conservatively

Checklist:

- define initial `HmrBoundaryKind` enum
- map descriptor changes to reload classification
- map template-only changes to template boundary classification
- keep ambiguous logic changes on full reload
- add explainable reasons for each classification path

Acceptance:

- compiler can compare old/new identity records
- classification is deterministic
- unclear cases default to full reload

## 4. Phase 3. Host Runtime Skeleton

Goal:

- let `DenoHost` consume HMR metadata and choose update paths

Checklist:

- add manifest read path for HMR metadata
- add minimal update coordinator in `DenoHost`
- add explicit template-only update attempt path
- add explicit full reload fallback path
- add developer-visible reason for reload fallback

Acceptance:

- host can receive compiler change metadata
- host can attempt minimal hot update when allowed
- host can fall back cleanly when not allowed

## 5. Phase 4. Conservative End-to-End Proof

Goal:

- prove one safe end-to-end HMR lane

Recommended first proof:

- a template-only Razor change on a stable component

Checklist:

- create characterization fixture with stable descriptor
- verify template hash changes while descriptor hash stays stable
- verify host chooses template-capable update path
- verify unsupported changes still trigger full reload

Acceptance:

- one template-only case works
- one descriptor-change case forces full reload
- no unsafe hot patch case is silently accepted

## 6. Phase 5. Later Refinement

Only after the conservative loop is green should later work consider:

- more granular logic-safe classification
- state-preserving strategies
- library-specific HMR hints
- developer overlay/debug tooling

These are refinement items, not entry requirements.

## 7. Test Strategy

Recommended test layers:

1. identity stability tests
2. split hash tests
3. boundary classification tests
4. manifest round-trip tests
5. later host runtime behavior tests

Recommended early test names:

- `RazorVue_HmrIdentity_EquivalentBuilds_AreStable`
- `RazorVue_HmrBoundary_TemplateOnlyChange_IsClassified`
- `RazorVue_HmrBoundary_DescriptorChange_ForcesFullReload`
- `RazorVue_HmrManifest_ContainsSplitHashes`

## 8. Non-goals for Early HMR Work

Do not expand early HMR work into:

- generic runtime patch infrastructure
- every Vue library integration
- SSR/hydration compatibility matrix
- component instance state persistence guarantees
- perfect source overlay UX

## 9. Completion Gate

RazorVue HMR is ready for real runtime expansion only when all of the following are true:

1. artifact identity is deterministic
2. manifest carries HMR metadata end to end
3. conservative classification is implemented
4. full reload fallback is explicit and tested
5. at least one template-only flow is proven

## 10. Conclusion

The right way to start RazorVue HMR is not with a live demo.
It is with stable identity, split hashes, deterministic classification, and a clean `DenoHost` fallback path.
