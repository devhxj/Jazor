# Jazor.Razor

> Status: active reference
> Positioning: Module-local operational entry for the thin Razor substrate used by Jazor and RazorVue.

`Jazor.Razor` is intentionally small.

It provides the base authoring substrate that lets Razor-based entry types participate in the Jazor stack without mixing compiler semantics or RazorVue-specific lowering into the substrate layer.

## Responsibilities

- Define the thin base component type used by Razor-authored Jazor components.
- Mark the boundary between general Razor authoring surface and higher-level product lanes such as RazorVue.

## Boundaries

- `Jazor.Razor` does not own RazorVue descriptors, lowering, pipeline, or generated artifacts.
- `Jazor.RazorVue` owns RazorVue core semantics.
- `Jazor.RazorVue.Analysis` owns Roslyn generator and diagnostic wiring.

## Key File

- `JazorComponent.cs`: intentionally thin base type over `ComponentBase`.

## Read Next

- [../Jazor.RazorVue/README.md](../Jazor.RazorVue/README.md)
- [../../src/Jazor.Compiler/doc/RazorVue.Overview.md](../../src/Jazor.Compiler/doc/RazorVue.Overview.md)
- [../../docs/plans/razorvue-execution-bridge.md](../../docs/plans/razorvue-execution-bridge.md)
