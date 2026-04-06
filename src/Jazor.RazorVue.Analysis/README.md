# Jazor.RazorVue.Analysis

> Status: active reference
> Positioning: Module-local operational entry for the thin Roslyn host around RazorVue generation and diagnostics.

`Jazor.RazorVue.Analysis` is the Roslyn-facing host layer for RazorVue.

It wires the incremental generator into compilation, projects RazorVue compilation issues into diagnostics, and emits the generated `Jazor.Generated.RazorVueCatalog.g.cs` source. It is intentionally not the home of RazorVue core semantics.

## Responsibilities

- Discover `[ECMAScriptModule]` candidates through Roslyn incremental generation.
- Create and emit generated RazorVue catalog source for assemblies that contain RazorVue artifacts.
- Project RazorVue compilation issues into compiler diagnostics.
- Keep the generator host thin while delegating semantic work to `Jazor.RazorVue`.

## Boundaries

- This module should not accumulate descriptor, lowering, artifact, or pipeline logic.
- `Jazor.RazorVue` owns semantic extraction and catalog production.
- `Jazor.RazorVue.Analysis` owns Roslyn wiring and diagnostic projection only.

## Key Files

- `RazorVueGenerator.cs`: incremental generator entry and diagnostic projection.
- `AnalyzerReleases.Unshipped.md`: current analyzer/release tracking for the generator diagnostics.
- `Jazor.RazorVue.Analysis.csproj`: thin host dependency boundary.

## Current Diagnostics

- `JAZORVGA001`: catalog generation failed
- `JAZORVGA002`: component not found
- `JAZORVGA003`: ambiguous short component name
- `JAZORVGA004`: reserved intrinsic-name collision
- `JAZORVGA005`: unsupported lifecycle lowering

## Verification

- `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "FullyQualifiedName~ESGeneratorTests|FullyQualifiedName~RazorVueAnalyzerTests"`

## Read Next

- [../Jazor.RazorVue/README.md](../Jazor.RazorVue/README.md)
- [../../src/Jazor.Compiler/doc/RazorVue.Overview.md](../../src/Jazor.Compiler/doc/RazorVue.Overview.md)
- [../../docs/plans/razorvue-execution-bridge.md](../../docs/plans/razorvue-execution-bridge.md)
