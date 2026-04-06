# RazorVue Project Responsibilities

This document fixes the final project-level ownership boundary for the RazorVue route.

It exists because the repository has reached the point where "the code works" is no longer enough.
Without a stable project boundary, the next implementation steps will slowly blur:

- compiler core orchestration
- Razor frontend extraction
- Vue target lowering
- author-facing component APIs

This document therefore answers two questions:

1. which project should own each responsibility
2. which narrow interfaces should connect those projects

Related documents:

- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
- [RazorVue.Review.md](./RazorVue.Review.md)

## 1. Final Position

The repository should converge on this public project split:

- `Jazor.Compiler`
  owns compiler core orchestration, shared contracts, artifact shaping, static-module generation, and extension points
- `Jazor.Razor`
  owns the Razor-facing base component substrate (`JazorComponent`)
- `Jazor.RazorVue`
  owns the Vue-facing authoring substrate (`VueComponent`) plus the RazorVue core semantic lane: frontend discovery, generated Razor analysis, descriptor extraction, source provenance, render-tree extraction, and Vue artifact lowering
- `Jazor.RazorVue.Analysis`
  owns the thin RazorVue generator/analyzer host entry: Roslyn wiring and diagnostic projection only
- `Jazor.Emit`
  owns catalog/materialized artifact reading and manifest persistence

The important clarification is:

- Razor base types are not compiler analysis
- Vue author APIs are not Roslyn/frontend extraction
- compiler orchestration is not the same thing as Razor parsing

## 2. Responsibility Matrix

| Capability | Jazor.Compiler | Jazor.Razor | Jazor.RazorVue | Jazor.RazorVue.Analysis | Jazor.Emit |
|---|---|---|---|---|---|
| Incremental generator orchestration | Owns | No | No | Owns thin host entry | No |
| Extension-point interfaces | Owns | No | No | Consumes | No |
| Shared artifact/catalog contracts | Owns | Uses | Uses | Uses | Consumes |
| HMR/source-origin core contracts | Owns | Uses | Produces/uses | Projects to diagnostics | Persists/consumes |
| Razor entry detection | No | No | Owns | No | No |
| Generated Razor code analysis | No | No | Owns | No | No |
| `BuildRenderTree` extraction | Owns shared primitives | No | Owns | No | No |
| Razor source provenance | Owns contracts | No | Owns extraction/mapping | Projects to diagnostics | No |
| Vue descriptor shaping | Shared boundary | No | Owns | No | No |
| Vue render-function lowering | Owns target pipeline coordination | No | Owns | No | No |
| `JazorComponent` | No | Owns | No | No | No |
| `VueComponent` and Vue author sugar | No | No | Owns | No | No |
| Catalog manifest reading/writing | No | No | No | No | Owns |

## 3. Hard Rules

The following rules should stay fixed during the next stages:

1. `Jazor.Compiler` must not accumulate more Razor-specific extraction logic than necessary to preserve the current build.
2. `Jazor.Razor` must stay a thin runtime/base library, not a Roslyn analysis home.
3. `Jazor.RazorVue` owns the RazorVue core semantic lane, including generated Razor analysis and `BuildRenderTree` parsing/lowering.
4. `Jazor.RazorVue.Analysis` must stay a thin host entry and must not absorb generic compiler-core logic or duplicate RazorVue core semantics.
5. Vue target semantics may depend on Razor frontend outputs, but Razor frontend outputs must not depend on Vue author APIs.
6. Any extension seam added now must be narrow enough that a later physical move does not require another public redesign.

## 4. Minimal Interface Scheme

The current route does not need a broad generic UI framework abstraction.

It only needs a narrow compiler seam between:

1. Razor semantic frontend extraction
2. Vue target lowering
3. catalog materialization

The recommended minimum interfaces are:

```csharp
public interface IRazorSemanticFrontend
{
    string Name { get; }
    bool CanHandle(Compilation compilation);
    RazorVueEntryKind ClassifyEntry(Compilation compilation, INamedTypeSymbol symbol);
    ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots(Compilation compilation);
}
```

```csharp
public interface IRazorVueArtifactLowerer
{
    VueCompiledArtifact Lower(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot);
    VueCompiledArtifact Lower(RazorVueSemanticSnapshot snapshot);
}
```

Why this stays intentionally narrow:

- it does not invent a fake multi-framework compiler model
- it allows `Jazor.Compiler` to own orchestration without owning all Razor extraction forever
- it keeps `Jazor.RazorVue` as the single RazorVue core semantic home
- it keeps `Jazor.RazorVue.Analysis` thin and replaceable as a host entry
- it keeps the Vue target lane explicit

## 5. Staged Code Move Plan

Because the repository currently has a target-framework split:

- `Jazor.Compiler` is `netstandard2.0`
- `Jazor.Razor`, `Jazor.RazorVue`, and the current `Jazor.RazorVue.Analysis` entry are `net10.0`

the project should not force unsafe reverse references just to satisfy folder purity.

The practical staged move is:

### Stage 1

- add the extension interfaces in `Jazor.Compiler`
- make `RazorVuePipeline` consume the interfaces
- keep the RazorVue core semantic implementation in `Jazor.RazorVue`
- expose the thin Roslyn host entry through `Jazor.RazorVue.Analysis`
- keep runtime/base libraries free of Roslyn analysis code

### Stage 2

- migrate any remaining RazorVue semantic logic out of `Jazor.Compiler` into `Jazor.RazorVue`
- keep `Jazor.RazorVue.Analysis` limited to Roslyn wiring and diagnostic projection
- stop growing Razor extraction directly inside `Jazor.Compiler`

### Stage 3

- retire transitional default implementations once `Jazor.Compiler` is orchestration-only for the RazorVue route

## 6. Review Round One

### 6.1 Developer Review

Findings:

- the architecture direction is better if authoring/runtime libraries stay separate from analysis
- a hard physical move right now would still fight the current target-framework layering
- introducing a narrow seam now is cheaper than waiting until more extraction logic lands

Decision:

- accept the interface-first staged move
- reject a big-bang project move and keep `Jazor.RazorVue` as the single RazorVue core semantic home in the current iteration

### 6.2 Project Manager Review

Findings:

- a clear public project split lowers future coordination cost between compiler, runtime, and analysis work
- a staged seam lets delivery continue while architecture debt stops growing
- moving author-facing types out of compiler-flavored project names improves product clarity immediately

Decision:

- approve a staged implementation
- require that the documentation explicitly explains why the physical move is phased

## 7. Review Round Two

### 7.1 Developer Review

Challenge:

- the first interface draft could still drift into over-abstraction

Resolution:

- keep the seam Razor/Vue-route-specific
- do not add generic framework-neutral type systems yet
- keep only frontend extraction and lowering seams

Decision:

- the narrow interface shape is acceptable
- adding more generic compiler abstractions at this stage would be premature

### 7.2 Project Manager Review

Challenge:

- staged refactors often fail when the first stage changes naming but not ownership

Resolution:

- require real code movement in this stage:
  - pipeline consumes interfaces
  - new public projects exist for runtime/base and thin analysis host entry
  - tests prove the new runtime names and the core-owned RazorVue path

Decision:

- approved, with the condition that this stage produces executable code movement signals, not only documents

## 8. Final Outcome

The repository should move toward:

- core orchestration in `Jazor.Compiler`
- Razor base types in `Jazor.Razor`
- Vue author surface in `Jazor.RazorVue`
- RazorVue generator/analysis entry in `Jazor.RazorVue.Analysis`

The correct implementation strategy is:

- stage the move behind narrow interfaces first
- avoid unsafe reverse references
- keep comments near the seam so future contributors understand why the boundary exists
