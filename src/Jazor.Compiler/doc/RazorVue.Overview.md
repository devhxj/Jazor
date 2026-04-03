# RazorVue Overview

## 1. Document Position

This document is the entry point for the RazorVue document set.

It does not repeat the full design.
It answers three questions:

1. what state RazorVue is currently in
2. what each RazorVue document is for
3. what order to read them in when work resumes

## 2. Current State

The current state of RazorVue is:

- main direction is designed
- implementation is not yet complete
- phase one scope is intentionally limited
- HMR and sourcemap are architecturally reserved but not fully implemented

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
5. RazorVue does not build its main path on source-generator ordering.
6. Razor components do not reuse plain static-module lowering.
7. The compiler emits Vue ESM artifacts and `DenoHost` owns the unified build.
8. HMR and sourcemap are reserved in architecture through metadata, not fully implemented in phase one.

## 4. Document Roles

### 4.1 Quick conclusions

- [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)

Use it when:

- you want the final decisions only
- you do not want to reread the full design

### 4.2 Full design

- [RazorVue.Design.md](./RazorVue.Design.md)

Use it when:

- you need architecture, boundaries, and responsibilities
- you need to understand why the design is shaped this way

### 4.3 Focused specs

- [RazorVue.ComponentDescriptorSpec.md](./RazorVue.ComponentDescriptorSpec.md)
- [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)

Use them when:

- you are implementing contract extraction
- you are implementing host-facing artifact/manifest flow
- you need narrower, implementation-facing specs than the main design doc

### 4.4 Hard constraints

- [RazorVue.HardRules.md](./RazorVue.HardRules.md)

Use it when:

- you need review rules
- you need to know what cannot be decided ad hoc during implementation

### 4.5 Implementation sequencing

- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)

Use it when:

- implementation is actually starting
- you need phased execution and acceptance gates

### 4.6 Common failure modes

- [RazorVue.Pitfalls.md](./RazorVue.Pitfalls.md)

Use it when:

- you want to avoid architecture drift
- you are reviewing changes and want to catch familiar wrong turns early

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
5. [RazorVue.HardRules.md](./RazorVue.HardRules.md)
6. [RazorVue.Pitfalls.md](./RazorVue.Pitfalls.md)
7. [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)

### 5.3 If you are reviewing code/design

Read in this order:

1. [RazorVue.HardRules.md](./RazorVue.HardRules.md)
2. [RazorVue.Pitfalls.md](./RazorVue.Pitfalls.md)
3. [RazorVue.Design.md](./RazorVue.Design.md)
4. [RazorVue.ComponentDescriptorSpec.md](./RazorVue.ComponentDescriptorSpec.md)
5. [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)

## 6. What Must Be Reconfirmed Before Implementation

Before real implementation starts, re-check at least:

1. `JazorComponent` / `VueComponent` API surface is still aligned with current goals
2. analyzer remains the accepted main semantic extraction point
3. the plain static-module path does not need major entry refactoring first
4. `DenoHost` manifest expectations are still compatible with the planned artifact model
5. HMR/sourcemap reservation requirements are still present in the implementation plan

## 7. One-line Conclusion

If you need to resume RazorVue later, start here, then read:

1. [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)
2. [RazorVue.HardRules.md](./RazorVue.HardRules.md)
3. [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
