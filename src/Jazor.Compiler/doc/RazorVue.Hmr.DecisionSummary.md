# RazorVue HMR Decision Summary

## 1. What This Document Solves

This is a short document that keeps only the final decisions for the RazorVue HMR direction.

Full design lives in:

- [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)
- [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)
- [RazorVue.Hmr.ImplementationChecklist.md](./RazorVue.Hmr.ImplementationChecklist.md)

## 2. Final Decisions

### 2.1 HMR is included architecturally from phase one

Phase one does not need runtime HMR,
but it must reserve the data required for later HMR without redesign.

### 2.2 Compiler owns HMR identity and change classification

Compiler-owned artifacts must already carry:

- `ComponentId`
- `ModuleId`
- `DescriptorHash`
- `TemplateHash`
- `LogicHash`
- `HmrBoundaryKind`

### 2.3 `DenoHost` owns runtime HMR orchestration

The compiler classifies changes.
`DenoHost` decides how to apply updates at runtime.

This keeps build/runtime ownership stable.

### 2.4 HMR is not based on final JS text diffing

RazorVue HMR must be based on compiler-owned semantic categories,
not only on line-level differences in emitted JavaScript.

### 2.5 Conservative fallback is required

If the compiler cannot prove a hot update is safe,
it must classify the change as full reload required.

Unsafe optimistic patching is out of scope.

### 2.6 Descriptor, template, and logic changes are different categories

At minimum, RazorVue must distinguish:

- public contract changes
- template-only changes
- logic changes

These categories must not collapse into one undifferentiated content hash.

### 2.7 HMR must stay compatible with sourcemap/source-origin work

HMR and sourcemap are separate concerns,
but both depend on:

- stable artifact identity
- stable segment ownership
- preserved source-origin metadata

### 2.8 UI library integrations may extend HMR metadata, not redefine it

Libraries such as Vuetify or MUI-style integrations may provide extra HMR hints,
but they do not redefine the core HMR contract.

## 3. Phase-one Scope

Phase one HMR only requires:

1. stable identity fields in artifacts and manifest
2. split change hashes
3. explicit boundary classification
4. host-consumable metadata shape

Phase one does not require:

- live hot swap runtime
- component instance state preservation
- library-specific hot patch engines
- template-level DOM patch debugging UI

## 4. Acceptance Summary

RazorVue HMR phase-one reservation is complete only when all of the following are true:

1. artifacts have stable component/module identity
2. descriptor/template/logic hashes are separate
3. `HmrBoundaryKind` is carried to host-facing outputs
4. `DenoHost` does not need to rediscover change categories itself
5. HMR metadata does not require redesign of the main lowering path later

## 5. One-line Conclusion

RazorVue HMR should begin as compiler-owned change identity and conservative safety classification, with `DenoHost` reserved as the future runtime update owner.
