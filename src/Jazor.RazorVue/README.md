# Jazor.RazorVue

> Status: active reference
> Positioning: Module-local operational entry for RazorVue core semantics.

`Jazor.RazorVue` is the core semantic layer for RazorVue.

It owns descriptor extraction, component discovery, semantic snapshot shaping, lowering, artifact construction, and host-facing catalog output for the RazorVue lane.

## Responsibilities

- Define the Vue-facing authoring marker interfaces through `IVueComponent` and `IVueLibraryComponent`.
- Build the RazorVue compilation context and component candidate model.
- Extract component descriptors, props, emits, slots, and related semantic inputs.
- Lower Razor-driven component semantics into Vue-oriented artifacts.
- Produce compiler-owned RazorVue catalog output for downstream emit and host materialization.

## Boundaries

- `Jazor.Razor` stays a thin authoring substrate only.
- `Jazor.RazorVue` owns RazorVue semantic logic and artifact shaping.
- `Jazor.RazorVue.Analysis` stays a thin Roslyn host that calls into this module and projects diagnostics.
- `Jazor.Emit` consumes the resulting catalogs and materializes host-facing output.

## Key Files

- `IVueComponent.cs`: user component authoring marker used together with `ComponentBase`.
- `IVueLibraryComponent.cs`: external Vue library component marker used together with `ComponentBase`.
- `RazorVueCompilationContext.cs`: shared compilation-time context.
- `RazorVuePipeline.cs`: main execution path for catalog generation.
- `Artifacts/`: semantic snapshot, catalog, source-origin, and compiled-artifact carriers.
- `Descriptor/`: descriptor extraction, registry, and resolution.
- `Lowering/`: Vue artifact shaping and lowering.

## Verification

- `pwsh ./scripts/test-dotnet.ps1 -Project razorvue`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~RazorVuePipelineTests"`

## Read Next

- [../../src/Jazor.Compiler/doc/RazorVue.Overview.md](../../src/Jazor.Compiler/doc/RazorVue.Overview.md)
- [../../docs/status/2026-04-06-razorvue-stage-assessment.md](../../docs/status/2026-04-06-razorvue-stage-assessment.md)
- [../../docs/plans/razorvue-execution-bridge.md](../../docs/plans/razorvue-execution-bridge.md)
