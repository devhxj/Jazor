# RazorVue HMR Overview

## 1. Document Position

This document is the entry point for the RazorVue HMR document set.

It does not repeat the whole RazorVue architecture.
It answers four questions:

1. what HMR state RazorVue is currently in
2. what each HMR document is for
3. what order to read them in
4. what must still be reconfirmed before implementation starts

## 2. Current State

The current HMR state is:

- HMR is architecturally reserved
- runtime HMR is not implemented
- compiler-side identity and change classification are designed first
- `DenoHost` remains the eventual HMR host owner

Current consensus is:

1. HMR is a compiler-plus-host contract, not a bundler afterthought
2. compiler owns stable identity and change-category metadata
3. `DenoHost` owns actual hot-update orchestration
4. phase one may over-classify to full reload if safety is unclear

## 3. Core Conclusions

The main fixed conclusions are:

1. HMR is part of the RazorVue architecture, even if runtime support lands later.
2. HMR must be based on stable `ComponentId` and `ModuleId`.
3. HMR must preserve split hashes for descriptor, template, and logic.
4. HMR classification belongs to compiler-owned artifacts, not only final JS diffing.
5. `DenoHost` owns runtime application of updates.
6. Conservative fallback to full reload is allowed; silent unsafe hot patching is not.

## 4. Document Roles

### 4.1 Quick conclusions

- [RazorVue.Hmr.DecisionSummary.md](./RazorVue.Hmr.DecisionSummary.md)

Use it when:

- you want the settled HMR decisions only
- you need to restart work quickly

### 4.2 Full design

- [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)

Use it when:

- you need the HMR responsibility split
- you need the identity and change-classification model
- you need the compiler/host boundary

### 4.3 Hard constraints

- [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)

Use it when:

- you are reviewing an implementation
- you need to know what cannot be decided ad hoc

### 4.4 Implementation sequencing

- [RazorVue.Hmr.ImplementationChecklist.md](./RazorVue.Hmr.ImplementationChecklist.md)

Use it when:

- implementation is about to start
- you need phased execution and acceptance gates

### 4.5 Common failure modes

- [RazorVue.Hmr.Pitfalls.md](./RazorVue.Hmr.Pitfalls.md)

Use it when:

- you want to avoid unsafe HMR design drift
- you are checking whether an implementation is becoming too optimistic

## 5. Recommended Reading Order

### 5.1 If you only want the final direction

Read in this order:

1. [RazorVue.Hmr.DecisionSummary.md](./RazorVue.Hmr.DecisionSummary.md)
2. [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)

### 5.2 If you are about to implement

Read in this order:

1. [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)
2. [RazorVue.Hmr.DecisionSummary.md](./RazorVue.Hmr.DecisionSummary.md)
3. [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)
4. [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)
5. [RazorVue.ComponentDescriptorSpec.md](./RazorVue.ComponentDescriptorSpec.md)
6. [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)
7. [RazorVue.Hmr.Pitfalls.md](./RazorVue.Hmr.Pitfalls.md)
8. [RazorVue.Hmr.ImplementationChecklist.md](./RazorVue.Hmr.ImplementationChecklist.md)

### 5.3 If you are reviewing code/design

Read in this order:

1. [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)
2. [RazorVue.Hmr.Pitfalls.md](./RazorVue.Hmr.Pitfalls.md)
3. [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)
4. [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)

## 6. What Must Be Reconfirmed Before Implementation

Before real HMR work starts, re-check at least:

1. `ComponentId` and `ModuleId` identity rules are already stable enough
2. descriptor/template/logic hashes are produced before host handoff
3. `DenoHost` still owns runtime update orchestration
4. sourcemap/source-origin data still lines up with HMR diagnostics needs
5. Vue library integrations expose enough metadata to classify reload safety conservatively

## 7. One-line Conclusion

RazorVue HMR starts as compiler-owned identity plus change metadata and grows into `DenoHost`-owned runtime hot update behavior without redesigning the main RazorVue pipeline.
