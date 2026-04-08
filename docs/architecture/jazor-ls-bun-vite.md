# Jazor: VueAnalysis + VueHost + Jazor.Vite

## Status

- Status: working baseline with focused tests green
- Scope: `.jazor` authoring, diagnostics, dev-server orchestration, and IDE integration
- Non-goal: re-create a second standalone frontend language ecosystem inside C#

## Decision

`Jazor.VueAnalysis` is the Roslyn-backed semantic analysis service for `.jazor`. It resolves C#-side semantics and consumes `.vue/.js/.ts/.css` context from `Jazor.VueHost` to support cross-file references, diagnostics, and other advanced composition features.

`Jazor.Vue.Analysis.Runtime` is the transport-neutral executable runtime for `AnalyzeJazor`. It stays separate from the analyzer/generator assembly so runtime host concerns do not leak back into Roslyn packaging.

`Jazor.VueHost` is the independent RPC service. It owns workspace state, frontend coordination, and communication with `Jazor.Vite`.

`Jazor.Vite` is a thin C# orchestration shell. It probes `Jazor.VueHost`, launches Bun/Vite, and passes host bootstrap settings into the frontend runtime without owning `.jazor` semantics.

This means:

- `.jazor` stays the single authoring source.
- `.jazor.vue` remains a logical bridge artifact, but defaults to a virtual artifact rather than a required on-disk file.
- IDEs and frontend tooling talk to one C# RPC server.
- `Jazor.VueAnalysis` stays the semantic center for `.jazor`.
- `Jazor.VueHost`, `Jazor.Vite`, and `Jazor.VueAnalysis.Runtime` share only DTO/protocol definitions at compile time.

Current landed baseline:

- `Jazor.VueContracts` holds shared DTOs and RPC method names
- `Jazor.Vue.Analysis.Runtime` serves runtime `AnalyzeJazor`
- `Jazor.Vue.Analysis.Host` wraps runtime analysis over stdio
- `Jazor.VueHost` exposes stdio RPC, workspace document tracking, derived frontend context, and `getVirtualArtifact`
- `Jazor.VueHost` now also exposes a minimal stdio LSP surface for `.jazor` diagnostics, hover, completion, and definition
- `Jazor.VueHost` falls back to local runtime analysis when no external analysis transport is configured
- `Jazor.Vite` keeps a persistent stdio session to `Jazor.VueHost` for load/HMR loops and returns a minimal consumable sourcemap from host descriptors
- the C# `ProcessVueHostRpcClient` now reuses one host process per client instance

## Goals

- Preserve Razor syntax as the `.jazor` authoring surface and keep C# as the authoritative component logic language.
- Deliver near-native frontend intelligence for Razor markup, diagnostics, and navigation while still targeting Vue at runtime.
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

- parse `.jazor` as a Razor-authored document
- identify markup, directives, and `@code`
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

### 3. Jazor.Vue.Analysis.Runtime

Target project:

- [src/Jazor.Vue.Analysis.Runtime/Jazor.Vue.Analysis.Runtime.csproj](D:/repository/own/jazor/Jazor/src/Jazor.Vue.Analysis.Runtime/Jazor.Vue.Analysis.Runtime.csproj)

Responsibilities:

- host runtime `AnalyzeJazor` service logic
- own transport-neutral RPC processing for analysis calls
- expose stdio-friendly server primitives for thin executable wrappers

Non-responsibilities:

- Roslyn analyzer packaging
- workspace/session ownership
- Bun/Vite orchestration
- IDE-facing control-plane logic

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
- expose a local RPC surface for IDEs and toolchain clients
- coordinate with `Jazor.Vite` rather than embedding frontend process ownership in the host baseline

`Jazor.VueHost` is not only an LSP adapter. It is the development-time control plane and standalone RPC server.

## Bun/Jazor.Vite Execution Layer

Target project:

- [src/Jazor.Vite/Jazor.Vite.csproj](D:/repository/own/jazor/Jazor/src/Jazor.Vite/Jazor.Vite.csproj)
- [src/Jazor.Vite/src/index.ts](D:/repository/own/jazor/Jazor/src/Jazor.Vite/src/index.ts)

Responsibilities:

- probe `Jazor.VueHost` over RPC
- launch Bun/Vite dev or build processes
- pass `Jazor.VueHost` bootstrap settings into the frontend runtime
- keep the TS plugin layer limited to `resolveId` / `load` / HMR glue
- remain thin enough that a future Vite plugin layer can stay dumb

Current baseline behavior:

- `src/index.ts` owns the Vite plugin entry
- `.jazor` is resolved as a virtual module prefix and loaded through `vuehost/getVirtualArtifact`
- `src/vue-host-session.ts` maintains a persistent `process-stdio` host session
- `.jazor` documents are tracked across `buildStart`, `load`, HMR updates, and teardown
- `load` returns `code + map` using host `SourceMapDescriptor[]`
- watched `.vue/.ts/.js` documents are synchronized into `Jazor.VueHost` workspace state

Non-responsibilities:

- defining `.jazor` semantics
- re-implementing import classification
- diagnosing `.jazor` logic misuse
- deciding C#-side authoring behavior

Design rule:

- `Jazor.Vite` should remain thin glue.
- if a feature needs understanding of `.jazor` semantics, it should call `Jazor.VueHost` RPC instead of duplicating logic in JS/TS.

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
- `getOpenDocuments`

### Compilation RPC

- `analyzeJazor`
- `getVirtualArtifact`

### Diagnostics RPC

- `getFrontendContext`

### Dev-Server RPC

- future: `ensureFrontendRuntime`
- future: `notifyFileChanged`
- future: `getHmrPlan`

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
- frontend semantic lane for Razor-markup-backed component and asset behavior
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

Current landed IDE baseline:

- stdio LSP entry in `Jazor.VueHost` via `--lsp`
- `textDocument/didOpen`, `didChange`, and `didClose` keep `.jazor` workspace state synchronized
- `textDocument/publishDiagnostics` is emitted from the same `.jazor` analysis lane used by host/runtime tests
- `hover`, `completion`, and `definition` currently focus on import directives and imported component tags inside `.jazor`

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
- `Jazor.Vue.Analysis.Runtime`
  - runtime `AnalyzeJazor` service
  - RPC processor/server primitives
  - no Roslyn analyzer packaging
- `Jazor.VueHost`
  - RPC server
  - LSP surface
  - workspace/session/cache orchestration
  - communication boundary to `Jazor.VueAnalysis`
  - frontend coordination without owning Bun/Vite processes in the current baseline
- `Jazor.Vite`
  - thin .NET Bun/Vite launcher
  - VueHost bootstrap client

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
- status: landed baseline includes stdio RPC, workspace document tracking, bootstrap methods, `getVirtualArtifact`, local runtime fallback, shallow analysis delegation, and process-level verification

### Phase 3. Move Frontend Process Ownership To `Jazor.VueHost`

- optional future direction only if a single-process C# control plane becomes necessary
- current baseline keeps frontend process ownership in `Jazor.Vite`
- if revisited later, `Jazor.VueHost` would expose compile/load/HMR RPC for `Jazor.Vite`

### Phase 4. Deepen `Jazor.Vite`

- extend the C# launcher into a Bun/Vite supervisor
- add frontend-side plugin hooks that fetch artifacts from `Jazor.VueHost`
- keep `.jazor` semantics remote instead of re-implemented in JS/TS
- status: initial C# launcher/bootstrap client and TS plugin baseline landed, including persistent VueHost sessions, real `.jazor -> vue-sfc` loading, and HMR document refresh through `vuehost/getVirtualArtifact`

### Phase 5. Deepen IDE Experience

- status: initial LSP baseline landed in `Jazor.VueHost --lsp`
- current: shallow Razor-markup/component diagnostics, hover, completion, definition, references, rename, and code actions are available for `.jazor`
- next: deepen source-mapped edits and cross `.jazor` / `.vue` symbol navigation beyond the current shallow nearby-component model

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

1. Deepen the `Jazor.VueHost --lsp` surface with richer source-mapped edits and deeper cross `.jazor` / `.vue` navigation.
2. Expand `Jazor.VueHost` RPC beyond bootstrap and `getVirtualArtifact` toward richer source-map and HMR-oriented endpoints.
3. Deepen frontend semantic ingestion beyond shallow tracked-document context and feed richer results back into `GetFrontendContext`.
4. Keep extending the persistent C# helper/client path where broader launcher/runtime reuse is needed, while the Vite-side session model stays the default path.
