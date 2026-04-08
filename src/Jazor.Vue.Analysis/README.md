# Jazor.Vue.Analysis

> Status: experimental reference
> Positioning: Thin Roslyn-facing shallow analysis layer for `.jazor`.

`Jazor.Vue.Analysis` is intentionally narrow.

Target scheme name:

- `Jazor.VueAnalysis`

Current implementation path during transition:

- [src/Jazor.Vue.Analysis/Jazor.Vue.Analysis.csproj](D:/repository/own/jazor/Jazor/src/Jazor.Vue.Analysis/Jazor.Vue.Analysis.csproj)

It hosts shallow semantic glue that:

- scans `.jazor` additional files
- parses and compiles them through `Jazor.Vue`
- projects generated bridge artifacts into source output
- projects Roslyn-consumable semantic information for `.jazor`
- exposes deterministic Roslyn-backed diagnostics that do not need IDE session state
- consumes `.vue/.js/.ts` semantic context from `Jazor.VueHost` for advanced cross-file analysis
- now also includes a transport-neutral runtime analysis service / processor for `AnalyzeJazor`

It should not become:

- the project/workspace host
- the Bun/Vite process manager
- the IDE/session orchestrator
- the long-lived cache coordinator
- the final executable RPC host process

Those responsibilities belong to the future `Jazor.VueHost`.

`Jazor.VueHost` should treat this project as the Roslyn semantic engine for `.jazor`, not as optional helper code.
`Jazor.VueAnalysis` and `Jazor.VueHost` may communicate at runtime, but should only share DTO/protocol contracts at compile time.

Current runtime-facing library surface:

- `Runtime/JazorVueAnalysisService.cs`
- `Runtime/IVueAnalysisRpcService.cs`
- `Runtime/VueAnalysisRpcProcessor.cs`
- `Runtime/IVueAnalysisRpcProcessor.cs`
- `Runtime/StdioVueAnalysisRpcServer.cs`

That surface is library-only for now. A future executable host can wrap it without moving analysis logic into `Jazor.VueHost`.

Reference architecture:

- [docs/architecture/jazor-ls-bun-vite.md](D:/repository/own/jazor/Jazor/docs/architecture/jazor-ls-bun-vite.md)
