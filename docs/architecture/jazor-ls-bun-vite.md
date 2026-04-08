# Jazor: VueAnalysis + VueHost + Jazor.Vite

## Status

- Status: proposed execution baseline
- Scope: `.jazor` authoring, diagnostics, dev-server orchestration, and IDE integration
- Non-goal: re-create a second standalone frontend language ecosystem inside C#

## Decision

`Jazor.VueAnalysis` is the Roslyn-backed semantic analysis service for `.jazor`. It resolves C#-side semantics and consumes `.vue/.js/.ts` context from `Jazor.VueHost` to support cross-file references, diagnostics, and other advanced composition features.

`Jazor.VueHost` is the independent RPC service. It owns workspace state, frontend coordination, and communication with `Jazor.Vite`.

`Jazor.Vite` is a thin frontend client. It talks only to `Jazor.VueHost`.

This means:

- `.jazor` stays the single authoring source.
- `.jazor.vue` remains a logical bridge artifact, but defaults to a virtual artifact rather than a required on-disk file.
- IDEs and frontend tooling talk to one C# RPC server.
- `Jazor.VueAnalysis` stays the semantic center for `.jazor`.
- `Jazor.VueHost` and `Jazor.VueAnalysis` communicate at runtime but should share only DTO/protocol definitions at compile time.

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

### 1. Jazor.Vue

Target projects:

- [src/Jazor.Vue/Jazor.Vue.csproj](D:/repository/own/jazor/Jazor/src/Jazor.Vue/Jazor.Vue.csproj)

Responsibilities:

- parse `.jazor`
- split imports, template, and `@code`
- build the virtual external symbol model
- emit virtual `.vue` bridge text
- emit virtual `g.cs` analysis text
- emit import metadata, source maps, and symbol maps

Non-responsibilities:

- Roslyn semantic policy
- workspace state
- Bun/Vite lifecycle management
- RPC hosting

### 2. Jazor.VueAnalysis

Target project:

- `src/Jazor.VueAnalysis/Jazor.VueAnalysis.csproj` as the target scheme name
- Current implementation path during transition: [src/Jazor.Vue.Analysis/Jazor.Vue.Analysis.csproj](D:/repository/own/jazor/Jazor/src/Jazor.Vue.Analysis/Jazor.Vue.Analysis.csproj)

Responsibilities:

- parse `.jazor` through `Jazor.Vue`
- build Roslyn semantic projections for `.jazor`
- classify imports and external symbols
- emit diagnostics, symbols, source maps, and semantic summaries
- consume `.vue/.js/.ts` semantic context from `Jazor.VueHost`
- support `.jazor` and `.vue` cross-file references and other advanced composition features

Non-responsibilities:

- workspace ownership
- Bun/Vite process management
- frontend dev-server policy
- direct IDE transport ownership

Design rule:

- `Jazor.VueAnalysis` is a semantic analysis service, not a host.
- `Jazor.VueAnalysis` may communicate with `Jazor.VueHost`, but the two projects must not directly reference each other.
- Shared DTOs and service contracts belong in a separate contracts/protocol layer.

## VueHost As The Real Host

Target project:

- `src/Jazor.VueHost/Jazor.VueHost.csproj` in the next implementation slice

Responsibilities:

- own workspace and document lifecycle
- own project graph state for `.jazor`
- own project graph state for `.vue/.js/.ts`
- produce and cache virtual documents
- provide `.vue/.js/.ts` semantic context to `Jazor.VueAnalysis`
- aggregate diagnostics and semantic results from `Jazor.VueAnalysis` and frontend semantic services
- manage Bun and Vite child processes
- expose a local RPC surface for IDEs and toolchain clients

`Jazor.VueHost` is not only an LSP adapter. It is the development-time control plane and standalone RPC server.

## Bun/Jazor.Vite Execution Layer

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

- `Jazor.Vite` should remain thin glue.
- if a feature needs understanding of `.jazor` semantics, it should call `Jazor.VueHost` RPC instead of duplicating logic in TS.

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

`Jazor.VueHost` should expose one local RPC surface used by:

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

## VueAnalysis <-> VueHost Contract

`Jazor.VueAnalysis` needs frontend semantic context that it does not own. `Jazor.VueHost` needs Roslyn-backed `.jazor` semantic results that it does not own.

That cooperation should happen through protocol contracts, not direct project references.

Minimal RPC groups:

### VueHost -> VueAnalysis

- `AnalyzeJazor`
- `GetDiagnostics`
- `GetSymbols`
- `GetSourceMap`
- `GetImportInfo`

### VueAnalysis -> VueHost

- `GetFrontendContext`
- `GetVueDocument`
- `GetScriptContext`
- `GetComponentMetadata`

### Shared DTOs

- `DocumentSnapshot`
- `SemanticContext`
- `ImportDescriptor`
- `SourceMapDescriptor`
- `DiagnosticRecord`
- `ArtifactRecord`

## IDE Integration Model

The IDE should connect only to `Jazor.VueHost`.

`Jazor.VueHost` then fans out internally:

- Roslyn lane for `@code`
- frontend semantic lane for template/SFC behavior
- `Jazor.VueAnalysis` for `.jazor`-specific projection, Roslyn semantics, and deterministic rules

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
- `Jazor.VueAnalysis`
  - Roslyn semantic engine
  - Roslyn generator/analyzer glue
  - deterministic `.jazor` diagnostics
  - no hosting
- `Jazor.VueHost`
  - RPC server
  - LSP surface
  - workspace/session/cache orchestration
  - communication boundary to `Jazor.VueAnalysis`
  - Bun/Vite process management
- `Jazor.Vite`
  - thin Bun-first TS plugin shell
  - Vite hooks only

Optional later split:

- `Jazor.DevHost`
  - only if CLI/dev-server orchestration needs to run without an IDE-attached `Jazor.VueHost`

Current recommendation:

- do not create `Jazor.DevHost` yet
- let `Jazor.VueHost` own the host role first

## Migration Plan

### Phase 1. Lock The Shallow Boundary

- Keep extending [src/Jazor.Vue/Jazor.Vue.csproj](D:/repository/own/jazor/Jazor/src/Jazor.Vue/Jazor.Vue.csproj) only for syntax, projection, and bridge outputs.
- Keep extending the `Jazor.VueAnalysis` lane only for deterministic Roslyn-facing diagnostics and generated artifacts.
- Reject host/session/process logic in `Jazor.VueAnalysis`.

### Phase 2. Introduce `Jazor.VueHost`

- add workspace model
- add virtual document manager
- add local RPC transport
- add DTO contracts for compile/diagnostic/source-map requests

### Phase 3. Move Frontend Process Ownership To `Jazor.VueHost`

- `Jazor.VueHost` starts Bun
- `Jazor.VueHost` starts or supervises `Jazor.Vite`
- `Jazor.VueHost` exposes compile/load/HMR RPC for `Jazor.Vite`

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
- IDE and frontend tooling talk only to one C# RPC host
- shallow analysis remains free of host/session logic
- `.jazor` diagnostics match between tests, CLI, and IDE

## Risks

- If `Jazor.VueAnalysis` keeps absorbing host concerns, the layering will collapse again.
- If the Vite plugin starts owning semantic fallbacks, rules will drift from IDE/CI behavior.
- If `Jazor.VueHost` becomes a thin pass-through instead of the real host, Bun/Vite orchestration will fragment.
- If physical `.jazor.vue` emission becomes mandatory too early, incremental workflow and mapping complexity will increase.

## Immediate Next Work

1. Add `Jazor.VueHost` as a standalone RPC service with RPC skeleton and virtual document store.
2. Define the first transport DTOs for `compileJazor`, `getDiagnostics`, and `getSourceMap`.
3. Add a Bun-first `Jazor.Vite` thin plugin that calls those RPC endpoints.
4. Keep new semantic rules in `Jazor.VueAnalysis` shallow and deterministic.
