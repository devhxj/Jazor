# RazorVue HMR Pitfalls

This document collects the mistakes that are most likely to derail RazorVue HMR.

It exists so later implementation can avoid repeating predictable bad turns.

Related documents:

- [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)
- [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)
- [RazorVue.Hmr.ImplementationChecklist.md](./RazorVue.Hmr.ImplementationChecklist.md)

## 1. Treating HMR as a runtime-only problem

If HMR starts only from runtime module replacement,
the compiler artifact model will already be missing:

- stable component identity
- split change hashes
- descriptor participation

That guarantees later redesign.

## 2. Using one content hash for everything

One undifferentiated hash removes the ability to distinguish:

- public contract change
- template change
- logic change

That makes safe classification impossible.

## 3. Assuming final JS diff is enough

Final JS diff is too low-level and too unstable to be the primary HMR model.

It tells you that output changed.
It does not reliably tell you whether hot update is safe.

## 4. Making logic-safe updates too optimistic

Logic changes are the easiest place to over-promise.

If the compiler cannot prove a logic change is safe,
the system should reload instead of guessing.

## 5. Ignoring descriptor drift

Props, emits, slots, and bind/model changes are not cosmetic.

If HMR ignores descriptor drift,
component consumers may keep running against an old public contract.

## 6. Letting libraries redefine the base contract

A library may need extra HMR hints.

That does not mean each library should invent its own:

- identity model
- boundary enum
- runtime ownership split

That would fragment the system immediately.

## 7. Designing HMR around temporary output paths

If identity depends on unstable output naming,
equivalent builds will look like unrelated components.

HMR will become noisy and unreliable.

## 8. Losing source-origin compatibility

Even if HMR does not ship full sourcemap first,
it still needs enough source-origin data for diagnostics and tooling.

If that metadata is discarded early,
later developer tooling will have no stable explanation surface.

## 9. Binding compiler too tightly to host runtime details

If the compiler starts depending on runtime module cache behavior or browser transport details,
the design boundary with `DenoHost` collapses.

That will make both sides harder to change.

## 10. Trying to solve every HMR scenario in the first milestone

The first milestone should prove:

- stable identity
- safe classification
- explicit fallback

It should not try to solve every Vue ecosystem edge case.

## 11. Treating full reload as failure

Full reload is the safety valve.

The wrong failure is silent unsafe hot patching,
not conservative fallback.

## 12. Conclusion

RazorVue HMR goes wrong when it becomes too optimistic, too runtime-driven, or too loosely defined.
The safe path is stable identity first, conservative classification second, runtime behavior third.
