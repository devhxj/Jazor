# Jazor.RazorVue

> Status: active reference
> Positioning: Module-local operational entry for RazorVue core semantics.

`Jazor.RazorVue` is the core semantic layer for RazorVue.

It owns descriptor extraction, component discovery, semantic snapshot shaping, lowering, artifact construction, and host-facing catalog output for the RazorVue lane.

## Responsibilities

- Define the Vue-facing authoring entry type through `VueComponent`.
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

- `VueComponent.cs`: authoring entry base type for RazorVue components.
- `RazorVue/RazorVueCompilationContext.cs`: shared compilation-time context.
- `RazorVue/RazorVuePipeline.cs`: main execution path for catalog generation.
- `RazorVue/Artifacts/`: semantic snapshot, catalog, source-origin, and compiled-artifact carriers.
- `RazorVue/Descriptor/`: descriptor extraction, registry, and resolution.
- `RazorVue/Lowering/`: Vue artifact shaping and lowering.

## Verification

- `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "FullyQualifiedName~RazorVuePipelineTests"`
- `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "FullyQualifiedName~ESGeneratorTests"`

## Read Next

- [../../src/Jazor.Compiler/doc/RazorVue.Overview.md](../../src/Jazor.Compiler/doc/RazorVue.Overview.md)
- [../../docs/status/2026-04-06-razorvue-stage-assessment.md](../../docs/status/2026-04-06-razorvue-stage-assessment.md)
- [../../docs/plans/razorvue-execution-bridge.md](../../docs/plans/razorvue-execution-bridge.md)
