# RazorVue HMR Hard Rules

> Status: active reference
> Positioning: Reserved-lane constraint reference for future RazorVue HMR work; not a signal that runtime HMR is already active.

This document fixes the HMR implementation rules that cannot remain ambiguous.

It does not repeat all HMR design discussion.
It exists to lock the boundaries that later implementation and review must not keep renegotiating.

Related documents:

- [RazorVue.Hmr.DecisionSummary.md](./RazorVue.Hmr.DecisionSummary.md)
- [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)
- [RazorVue.Hmr.ImplementationChecklist.md](./RazorVue.Hmr.ImplementationChecklist.md)
- [RazorVue.Hmr.Pitfalls.md](./RazorVue.Hmr.Pitfalls.md)

## 1. Scope

These rules apply to RazorVue HMR planning and later implementation:

- compiler-owned artifact identity
- change classification
- host-facing HMR metadata
- `DenoHost` runtime boundary

## 2. Rule 1. HMR is a first-class architecture concern

Phase one does not need runtime HMR,
but it must preserve the data required for it.

HMR must not be treated as optional post-processing.

## 3. Rule 2. Compiler owns HMR identity

The compiler must own:

- `ComponentId`
- `ModuleId`
- `DescriptorHash`
- `TemplateHash`
- `LogicHash`
- `HmrBoundaryKind`

`DenoHost` must consume this metadata, not reconstruct it.

## 4. Rule 3. HMR must not be inferred only from emitted JS diff

Final JS text diffing may help diagnostics,
but it must not be the primary HMR safety model.

Primary classification must remain compiler-owned and semantic.

## 5. Rule 4. Split hashes are mandatory

Phase one must not collapse HMR change categories into one content hash.

At minimum, descriptor, template, and logic must remain separate.

## 6. Rule 5. Conservative fallback is mandatory

If HMR safety cannot be proven,
the compiler or host must escalate to full reload.

Unsafe optimistic hot patching is out of bounds.

## 7. Rule 6. Public contract drift is not template-only

Changes to props, emits, slots, or bind/model metadata must not be classified as template-only.

They are contract-level changes and should usually force full reload.

## 8. Rule 7. `DenoHost` owns runtime application

The compiler does not own:

- browser update transport
- module invalidation runtime
- component instance replacement runtime

Those belong to `DenoHost`.

## 9. Rule 8. HMR must not distort main lowering

HMR metadata is an extension of the main RazorVue pipeline.

It must not become the reason to:

- redesign template lowering around runtime patch tricks
- couple render emission to host runtime details
- leak bundler/runtime state into compiler semantic extraction

## 10. Rule 9. Descriptor identity must participate in HMR

`VueComponentDescriptor` is part of the HMR boundary.

Descriptor changes must affect:

- `DescriptorHash`
- boundary classification
- host invalidation decisions

## 11. Rule 10. Source-origin metadata must remain HMR-compatible

HMR does not require full sourcemap first,
but it does require compatible source-origin metadata.

At minimum, HMR-related diagnostics must be able to trace to:

- original source when known
- generated mapping when source is indirect
- generated fallback when that is all that exists

## 12. Rule 11. Library integrations may only extend the base HMR contract

Libraries may add:

- extra invalidation hints
- style dependency hints
- always-reload markers

Libraries must not redefine:

- `ComponentId`
- `ModuleId`
- core boundary kinds
- compiler/host ownership split

## 13. Rule 12. HMR classification must remain explainable

For every classification,
the system should be able to explain in compiler/host terms why the change became:

- template-only
- logic-safe
- full reload required

Hidden heuristics without explainable categories are out of bounds.

## 14. Rule 13. Initial runtime scope stays minimal

The first runtime-capable HMR milestone should prove only:

- host can see identity changes
- host can attempt a conservative update path
- host can fall back cleanly to full reload

It must not try to solve every state-preservation case immediately.

## 15. Rule 14. HMR verification must start with identity stability

Before any runtime HMR demo is considered valid,
the repository must already prove:

1. stable component identity
2. stable module identity
3. stable split hashes
4. deterministic boundary classification

## 16. Conclusion

RazorVue HMR is valid only if it stays conservative, compiler-owned at the metadata layer, `DenoHost`-owned at the runtime layer, and compatible with the broader RazorVue artifact model.
