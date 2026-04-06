# Jazor Project Program Roadmap

> Status: active plan
> Positioning: Repository-level program bridge across current Jazor workstreams.

## 1. Scope

This document exists to make the cross-workstream execution order explicit.

It is not:

- a replacement for subsystem-local deep docs
- a detailed implementation checklist
- a promise that all workstreams move in one strict serial order

Use it to answer:

1. which workstreams are upstream foundations
2. which expansions are allowed now
3. which gates must hold before the next lane broadens

## 2. Current Program State

Current repo-level state should be read through these snapshots first:

- [2026-04-04-project-stage-assessment.md](../status/2026-04-04-project-stage-assessment.md)
- [2026-04-06-project-workstream-dashboard.md](../status/2026-04-06-project-workstream-dashboard.md)
- [2026-04-06-compiler-mainline-status.md](../status/2026-04-06-compiler-mainline-status.md)
- [2026-04-06-emit-host-materialization-status.md](../status/2026-04-06-emit-host-materialization-status.md)
- [2026-04-06-razorvue-stage-assessment.md](../status/2026-04-06-razorvue-stage-assessment.md)

Current high-level picture:

- compiler mainline is the strongest foundation
- emit/materialization is an active dependency lane
- RazorVue is in active execution
- RazorVue authoring is a controlled expansion lane
- broad SourceMap remains conservative, while a narrower active lane exists
- documentation governance runs continuously across all of the above

## 3. Dependency Order

The practical dependency order for the current program is:

1. compiler mainline stabilization
2. emit / host materialization consolidation
3. RazorVue phase-one closure
4. RazorVue authoring lane execution
5. SourceMap partial rollout for active consumers
6. broader SourceMap program
7. ongoing documentation governance and bridge maintenance

This is not a pure serial model.

It means:

- upstream lanes define the safe boundary for downstream expansion
- some downstream lanes can move in parallel when their scope stays narrow

## 4. Workstream Map

### 4.1 Compiler mainline

- Status: [2026-04-06-compiler-mainline-status.md](../status/2026-04-06-compiler-mainline-status.md)
- Execution bridge: [compiler-mainline-execution-bridge.md](./compiler-mainline-execution-bridge.md)
- Deep docs: [Compiler Architecture Bridge](../architecture/compiler/README.md)

### 4.2 Emit / materialization

- Status: [2026-04-06-emit-host-materialization-status.md](../status/2026-04-06-emit-host-materialization-status.md)
- Execution bridge: [emit-materialization-execution-bridge.md](./emit-materialization-execution-bridge.md)
- Module bridge: [Modules Bridge](../architecture/modules/README.md)
- Local docs: [Jazor.Emit Docs](../../src/Jazor.Emit/doc/README.md)

### 4.3 RazorVue phase-one

- Status: [2026-04-06-razorvue-stage-assessment.md](../status/2026-04-06-razorvue-stage-assessment.md)
- Active plans:
  - [2026-04-05-razorvue-layering-implementation.md](../superpowers/plans/2026-04-05-razorvue-layering-implementation.md)
  - [2026-04-05-razorvue-lifecycle-safe-subset-implementation.md](../superpowers/plans/2026-04-05-razorvue-lifecycle-safe-subset-implementation.md)
- Deep docs: [RazorVue.Overview.md](../../src/Jazor.Compiler/doc/RazorVue.Overview.md)

### 4.4 RazorVue authoring lane

- Active plans:
  - [2026-04-06-razorvue-v1-authoring-roadmap.md](../superpowers/plans/2026-04-06-razorvue-v1-authoring-roadmap.md)
  - [2026-04-06-razorvue-v1-authoring-pr-breakdown.md](../superpowers/plans/2026-04-06-razorvue-v1-authoring-pr-breakdown.md)

### 4.5 SourceMap

- Deep docs: [SourceMap.Overview.md](../../src/Jazor.Compiler/doc/SourceMap.Overview.md)
- Narrow active plan:
  - [2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md](../superpowers/plans/2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md)

### 4.6 Documentation governance

- Governance rules: [documentation-governance.md](../guides/documentation-governance.md)
- Repo hub: [docs/README.md](../README.md)

## 5. Execution Gates

### Gate A. Compiler stability before downstream expansion

Downstream lanes should not force a redesign of the compiler main path.

What must remain true:

- compiler local docs remain authoritative
- core conversion boundaries remain stable enough for downstream assumptions

### Gate B. Emit/materialization bridge before downstream closure claims

Do not describe downstream lanes as closed if host-facing handoff is still unclear.

This gate matters for:

- RazorVue artifact/manifest flow
- SourceMap writer and bundle chaining flow

### Gate C. RazorVue minimal path before authoring breadth

Authoring expansion should build on a closed minimal RazorVue path, not bypass it.

### Gate D. RazorVue authoring must not fork core semantics

Before moving beyond the mid-authoring review gate, confirm:

- stub-as-truth-source still holds
- lowering remains generic
- package-specific branches have not leaked into core

### Gate E. Partial SourceMap rollout only on stable upstream carriers

A narrow SourceMap slice can advance earlier only if:

- artifact/source-origin shape is already available
- emit-side evolution is explicit
- the slice stays narrower than the broad SourceMap program

### Gate F. Broad SourceMap program remains conservative

The broad SourceMap program should not be treated as fully active just because one narrower lane is active.

### Gate G. Documentation updates are mandatory on phase change

When any workstream changes phase, update at least:

1. the relevant repo-level status snapshot
2. the relevant repo-level execution bridge
3. this roadmap if the dependency order or gate changed

## 6. Allowed Parallelism

The current program explicitly allows some parallel movement.

Examples:

- documentation governance can run continuously
- narrow SourceMap work can advance with active RazorVue/emit integration
- emit can continue as an active dependency lane while compiler remains the upstream foundation

What is not allowed:

- broad SourceMap expansion that outruns compiler/emit stability
- authoring breadth that outruns RazorVue phase-one closure

## 7. Stop Conditions

Pause expansion when any of the following becomes true:

- downstream work starts forcing upstream redesign
- repo-level status and plan docs drift from actual execution
- local active lanes contradict broad-program docs
- a repo-level bridge starts duplicating subsystem-local authority

## 8. Canonical Entry Points

If you are resuming project work, use this path:

1. [2026-04-04-project-stage-assessment.md](../status/2026-04-04-project-stage-assessment.md)
2. [2026-04-06-project-workstream-dashboard.md](../status/2026-04-06-project-workstream-dashboard.md)
3. [project-execution-index.md](./project-execution-index.md)
4. this document
5. then enter the relevant workstream bridge or subsystem deep docs

## 9. Maintenance Rules

This roadmap should stay short.

Do not turn it into:

- a second docs hub
- a subsystem checklist
- a more detailed replacement for local design docs

If a change only affects one workstream's internal design, update that workstream's docs instead of expanding this file.
