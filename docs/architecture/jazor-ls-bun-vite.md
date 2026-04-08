# Jazor: Shallow Analysis + LS + Bun/Vite

## Status

- Status: proposed execution baseline
- Scope: `.jazor` authoring, diagnostics, dev-server orchestration, and IDE integration
- Non-goal: re-create a second standalone frontend language ecosystem inside C#

## Decision

Jazor vNext should converge on a three-layer design:

1. `Jazor.Vue` and `Jazor.Vue.Analysis` remain shallow semantic libraries.
2. `Jazor.LanguageServer` becomes the single development-time orchestrator.
3. `Bun + Vite` remain the frontend execution layer and are controlled by the language server, not by IDE plugins directly.

This means:

- `.jazor` stays the single authoring source.
- `.jazor.vue` remains a logical bridge artifact, but defaults to a virtual artifact rather than a required on-disk file.
- IDEs talk to one C# server.
- Vite/Bun obtain compiled artifacts and metadata through RPC rather than re-implementing `.jazor` semantics.

## Goals

- Preserve C# as the only authoritative component logic language in `.jazor`.
- Deliver near-native Vue SFC authoring for template, imports, diagnostics, and navigation.
- Remove Node as a required runtime dependency for Jazor development flow.
- Keep semantic rules single-sourced and testable outside IDE sessions.
- Keep Vite integration thin and replaceable.

## Non-Goals

- No heavy standalone `analysis` subsystem with its own lifecycle, cache graph, and host logic.
- No Rust- or JS-owned source of truth for `.jazor` semantics.
- No direct IDE integration against Vite internals.
- No mandatory physical emission of `.jazor.vue` files during normal development.

## Final Layering

### 1. Shallow Analysis

Target projects:

- [src/Jazor.Vue/Jazor.Vue.csproj](D:/repository/own/jazor/Jazor/src/Jazor.Vue/Jazor.Vue.csproj)
- [src/Jazor.Vue.Analysis/Jazor.Vue.Analysis.csproj](D:/repository/own/jazor/Jazor/src/Jazor.Vue.Analysis/Jazor.Vue.Analysis.csproj)

Responsibilities:

- parse `.jazor`
- split imports, template, and `@code`
- build the virtual external symbol model
- emit virtual `.vue` bridge text
- emit virtual `g.cs` analysis text
- emit import metadata, source maps, and symbol maps
- run deterministic Roslyn-backed rules that do not need IDE session state

Non-responsibilities:

- process hosting
- Bun/Vite lifecycle management
- project-wide file watching
- HMR invalidation policy
- editor session state
- UX composition for completions or code actions

Design rule:

- `Jazor.Vue.Analysis` must stay library-like.
- If a feature needs long-lived process state, background workers, project coordination, or frontend process management, it belongs in the language server, not here.

## Language Server As The Real Host

Target project:

- `src/Jazor.LanguageServer/Jazor.LanguageServer.csproj` in the next implementation slice

Responsibilities:

- own workspace and document lifecycle
- own project graph state for `.jazor`
- produce and cache virtual documents
- route C# authoring to Roslyn
- route Vue/template authoring to the frontend semantic lane
- aggregate diagnostics from shallow analysis, Roslyn, and frontend semantic services
- manage Bun and Vite child processes
- expose a local RPC surface for IDEs and toolchain clients

This server is not only an LSP adapter. It is the development-time control plane.

## Bun/Vite Execution Layer

Target project:

- `src/Jazor.Vite/` or `tooling/Jazor.Vite/` in a Bun-first TS package

Responsibilities:

- run Vite dev/build lifecycle on Bun
- resolve/load/transform `.jazor` through RPC
- participate in module graph invalidation
- apply HMR boundaries and module reload policy
- return standard frontend artifacts to the browser toolchain

Non-responsibilities:

- defining `.jazor` semantics
- re-implementing import classification
- diagnosing `.jazor` logic misuse
- deciding C#-side authoring behavior

Design rule:

- the Vite plugin should remain thin glue.
- if a feature needs understanding of `.jazor` semantics, it should call the language server RPC instead of duplicating logic in TS.

## Artifact Contract

For each `.jazor` file, the shallow analysis layer should be able to produce:

- virtual Vue SFC text
- virtual C# analysis text
- import classification metadata
- external symbol metadata
- source map / span map
- dependency list
- HMR boundary hints
- stable content hash

Recommended logical artifact names:

- `A.jazor -> virtual:A.jazor.vue`
- `A.jazor -> virtual:A.jazor.g.cs`
- `A.jazor -> import-info`
- `A.jazor -> source-map`

Physical emission policy:

- default: virtual only
- optional: emit to cache folder for debugging, inspection, or offline build scenarios

Recommended cache location:

- `.jazor-cache/`

## RPC Contract

The language server should expose one local RPC surface used by:

- IDE clients
- Bun/Vite plugin
- future CLI/build adapters

Recommended RPC groups:

### Document RPC

- `openDocument`
- `updateDocument`
- `closeDocument`
- `getVirtualDocuments`

### Compilation RPC

- `compileJazor`
- `getVueArtifact`
- `getAnalysisArtifact`
- `getImportInfos`
- `getSourceMap`

### Diagnostics RPC

- `getDiagnostics`
- `getSemanticSummary`

### Dev-Server RPC

- `ensureFrontendRuntime`
- `notifyFileChanged`
- `getHmrPlan`

Contract rule:

- RPC returns structured DTOs only.
- Never leak Roslyn objects, TS objects, or in-process symbol instances over RPC.

## IDE Integration Model

The IDE should connect only to `Jazor.LanguageServer`.

The language server then fans out internally:

- Roslyn lane for `@code`
- frontend semantic lane for template/SFC behavior
- shallow analysis for `.jazor`-specific projection and rules

Virtual document model:

- one user document: `A.jazor`
- one virtual C# document
- one virtual Vue document
- optional future virtual TS helper document

Required IDE behaviors:

- completion
- hover
- go to definition
- find references
- rename
- diagnostics
- code actions
- source mapping from generated artifacts back to `.jazor`

## Project Boundary Recommendation

Recommended medium-term project split:

- `Jazor.Vue`
  - parser
  - bridge compiler
  - source map model
  - import symbol model
- `Jazor.Vue.Analysis`
  - Roslyn generator/analyzer glue
  - deterministic `.jazor` diagnostics
  - no hosting
- `Jazor.LanguageServer`
  - RPC server
  - LSP surface
  - workspace/session/cache orchestration
  - Bun/Vite process management
- `Jazor.Vite`
  - thin Bun-first TS plugin shell
  - Vite hooks only

Optional later split:

- `Jazor.DevHost`
  - only if CLI/dev-server orchestration needs to run without an IDE-attached language server

Current recommendation:

- do not create `Jazor.DevHost` yet
- let `Jazor.LanguageServer` own the host role first

## Migration Plan

### Phase 1. Lock The Shallow Boundary

- Keep extending [src/Jazor.Vue/Jazor.Vue.csproj](D:/repository/own/jazor/Jazor/src/Jazor.Vue/Jazor.Vue.csproj) only for syntax, projection, and bridge outputs.
- Keep extending [src/Jazor.Vue.Analysis/Jazor.Vue.Analysis.csproj](D:/repository/own/jazor/Jazor/src/Jazor.Vue.Analysis/Jazor.Vue.Analysis.csproj) only for deterministic Roslyn-facing diagnostics and generated artifacts.
- Reject host/session/process logic in `Jazor.Vue.Analysis`.

### Phase 2. Introduce `Jazor.LanguageServer`

- add workspace model
- add virtual document manager
- add local RPC transport
- add DTO contracts for compile/diagnostic/source-map requests

### Phase 3. Move Frontend Process Ownership To LS

- LS starts Bun
- LS starts or supervises Vite
- LS exposes compile/load/HMR RPC for the Vite plugin

### Phase 4. Introduce Thin `Jazor.Vite`

- implement `resolveId`
- implement `load`
- implement `handleHotUpdate`
- fetch artifacts from LS RPC only

### Phase 5. Deepen IDE Experience

- template-aware navigation
- cross `.jazor` / `.vue` references
- source-mapped rename and diagnostics

## Acceptance Criteria

This architecture is considered landed when all of the following are true:

- `.jazor` remains the only source file users author
- no Node runtime is required for dev-server execution
- Bun runs the frontend toolchain
- Vite plugin does not duplicate `.jazor` semantics
- IDE talks only to one C# language server
- shallow analysis remains free of host/session logic
- `.jazor` diagnostics match between tests, CLI, and IDE

## Risks

- If `Jazor.Vue.Analysis` keeps absorbing host concerns, the layering will collapse again.
- If the Vite plugin starts owning semantic fallbacks, rules will drift from IDE/CI behavior.
- If the LS becomes a thin pass-through instead of the real host, Bun/Vite orchestration will fragment.
- If physical `.jazor.vue` emission becomes mandatory too early, incremental workflow and mapping complexity will increase.

## Immediate Next Work

1. Add `Jazor.LanguageServer` with RPC skeleton and virtual document store.
2. Define the first transport DTOs for `compileJazor`, `getDiagnostics`, and `getSourceMap`.
3. Add a Bun-first `Jazor.Vite` thin plugin that calls those RPC endpoints.
4. Keep new semantic rules in `Jazor.Vue.Analysis` shallow and deterministic.
