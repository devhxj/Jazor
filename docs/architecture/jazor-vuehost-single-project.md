# Jazor.VueHost Single-Project Host Design

## Status

- status: active target architecture
- scope: `.jazor` authoring host, LSP, virtual documents, frontend language services, dev-time module loading
- runtime choice: Deno
- non-scope: RazorVue library route and its historical host/bundling assumptions

Related architecture docs:

- [vuehost-capabilities.md](./vuehost-capabilities.md) describes the long-range VueHost capability blueprint.
- [vuehost-document-map.md](./vuehost-document-map.md) explains how this document differs from the capability blueprint and how to use both during implementation.

## Decision

`Jazor.VueHost` becomes the only project boundary and the only public entry point.

The old split-host / analysis-host project boundaries are migration leftovers and should stay deleted.

Remaining Vue-facing capability belongs inside `Jazor.VueHost` rather than reappearing as sibling projects.

`Jazor.Vite` and Bun are obsolete and are not part of the target architecture.

This design explicitly does **not** inherit RazorVue's host/bundling conclusions. RazorVue is a separate library technology path and is out of scope for this host design.

## Core Principles

- `.jazor` is the primary authoring document.
- `Jazor.VueHost` is the only IDE/dev-host boundary.
- Roslyn and frontend semantics are internal lanes, not separate products.
- Deno is the only frontend runtime for this host.
- `Jazor.Vite`, Bun, and the old split-host route are migration leftovers, not design inputs.
- LSP is projection-aware and lane-aware.
- Virtual document mapping is a first-class subsystem, not a helper detail.
- bridge/build projection should infer nearby `.vue` dependencies from Razor markup and derive co-located `.css/.js/.ts` sidecars from the `.jazor` path before consulting legacy import directives.
- Capability migration happens before project deletion.

## Implementation Contract

### 1. `.jazor` is Razor

- `.jazor` is a Razor-authored document, not a `.vue`-style SFC dialect.
- do not introduce a new authoring model based on `template/script/style`.
- the source-of-truth document is the Razor-first `.jazor` file.

### 2. `Jazor.VueHost` is the only active host

- all active dev-time Vue-facing capability belongs inside `Jazor.VueHost`.
- do not reintroduce `Jazor.Vue.Analysis`, `Jazor.Vue.Analysis.Host`, or `Jazor.Vue.Analysis.Runtime`.
- do not revive `Jazor.Vite`, Bun, or the old split-host route.
- Deno is the only frontend/runtime host path.

### 3. IntelliSense and build are separate stages

- design-time intelligence must work from the `.jazor` source document directly.
- IntelliSense must not depend on first materializing final `.vue` or `.cs` artifacts.
- build/materialization may still project `.jazor` into internal `.vue`, bridge artifacts, and runtime outputs.
- those projected artifacts are build-time implementation details, not authoring semantics.

### 4. VueHost is a lane-based host

- Razor/Roslyn lane owns C#, `@code`, navigation, rename, references, and source diagnostics.
- frontend lane owns Vue/TS/CSS/HTML understanding for nearby `.vue`, `.css`, `.js`, and `.ts`.
- VueHost owns the shared workspace graph and routes requests to the correct lane before aggregating the result.
- `.vue` and `.jazor` navigation should meet in that shared workspace graph, so definition/references/rename do not stop at the current file boundary.
- workspace-open `.vue` documents should also participate in live `.jazor` diagnostics, so opening/closing a component can immediately suppress or reintroduce unresolved-component diagnostics in related `.jazor`.
- component rename/reference aggregation should stay markup-only for component tags and should not rewrite Roslyn-owned `@code` identifiers with the same text.
- until a full workspace index exists, a shared workspace resolver should be the single place that owns path normalization, nearby lookup, bounded workspace scans, and cache invalidation for `.jazor <-> .vue` design-time relations.
- before a true workspace index exists, VueHost may use bounded ancestor-root disk scans to widen `.vue <-> .jazor` navigation beyond nearby directories; this is an IntelliSense heuristic, not a build contract.
- virtual documents and projection maps exist to bridge lanes, not to redefine the authoring model.

### 5. Virtual artifacts are internal

- virtual `.vue` and other bridge outputs are internal projections.
- virtual `.cs` generation is **optional**; whether RoslynLane needs a projected C# document depends on the Roslyn integration approach (direct fragment analysis vs. minimal context projection). This decision is deferred to implementation.
- they are allowed for LSP routing, worker interop, materialization, and tooling.
- they must remain implementation details rather than becoming user-facing language boundaries.

### 6. Non-goals

- do not make `.jazor` into another `.vue`.
- do not make `@vueimport` / `@jsimport` the long-term authoring model.
- do not make IntelliSense wait for generated `g.cs` or final materialized output.
- do not split VueHost responsibilities back into sibling analysis/runtime products.

## Top-Level Structure

```text
src/Jazor.VueHost
├─ Program.cs
├─ Jazor.VueHost.csproj
├─ Protocol
│  ├─ Documents
│  ├─ Lsp
│  ├─ Rpc
│  └─ Serialization
├─ Workspace
│  ├─ Documents
│  ├─ ProjectModel
│  ├─ Dependencies
│  └─ FileWatching
├─ Jazor
│  ├─ Parsing
│  ├─ Syntax
│  ├─ Directives
│  ├─ Projection
│  └─ Diagnostics
├─ VirtualDocuments
│  ├─ Models
│  ├─ Builders
│  ├─ Mapping
│  └─ Registry
├─ Roslyn
│  ├─ Workspace
│  ├─ Projections
│  ├─ Diagnostics
│  ├─ Completion
│  ├─ Hover
│  ├─ Navigation
│  └─ Refactoring
├─ Frontend
│  ├─ Deno
│  │  ├─ Worker
│  │  ├─ Protocol
│  │  └─ Hosting
│  ├─ VolarTs          ← Volar + TSServer unified lane service
│  └─ Mapping
├─ Lsp
│  ├─ Hosting
│  ├─ Routing
│  ├─ Aggregation
│  └─ Handlers
├─ DevServer
│  ├─ ModuleGraph
│  ├─ Loading
│  ├─ Hmr
│  ├─ SourceMaps
│  └─ StaticAssets
└─ Infrastructure
   ├─ Configuration
   ├─ Logging
   ├─ Processes
   ├─ Caching
   └─ Utilities
```

## Folder Responsibilities

### `Protocol`

Stable DTOs and wire contracts only.

- document snapshots
- LSP payloads
- internal RPC envelopes
- serialization helpers

### `Workspace`

Unified workspace state for:

- `.jazor`
- `.vue`
- `.ts`
- `.js`
- `.css`
- `.html`

Responsibilities:

- tracked documents
- versions
- dependency graph
- file watching
- project-level context

### `Jazor`

Owns `.jazor` host-native logic:

- parsing
- syntax tree
- Razor directives and markup classification
- top-level structure checks
- projection inputs
- host-native diagnostics

### `VirtualDocuments`

Owns generated internal documents and mappings:

- virtual `.vue`
- virtual `.cs`
- reverse/forward span maps
- virtual document registry

This is the foundational layer for Roslyn and frontend integration.

### `Roslyn`

Owns C# semantics over projected virtual C# documents:

- completion
- hover
- signature help
- diagnostics
- definition
- references
- rename
- code actions

### `Frontend`

Owns Vue/TS/JS/CSS/HTML semantics through Deno-hosted workers.

- Deno worker hosting
- Volar integration
- TypeScript integration
- Vue component, CSS/HTML, and script semantics projected from Razor markup
- CSS/HTML semantics
- projected frontend edits and diagnostics

### `Lsp`

Owns protocol serving, not language rules.

- request routing
- lane selection
- result aggregation
- final response/notification emission

### `DevServer`

Owns development-time runtime behavior:

- virtual module loading
- module graph
- HMR planning
- source map production/merging
- static asset behavior

### `Infrastructure`

Shared operational support:

- config
- logging
- process lifecycle
- caches
- common utilities

## Initial Core Types

### Protocol and Workspace

- `DocumentSnapshot`
- `DocumentKind`
- `DocumentVersion`
- `TextSpan`
- `TextChange`
- `IWorkspaceStore`
- `WorkspaceStore`
- `DocumentTracker`
- `WorkspaceProjectGraph`

### Jazor

- `JazorDocumentParser`
- `JazorSyntaxTree`
- `JazorImportDirectiveSyntax`
- `JazorDirectiveClassifier`
- `JazorDirectiveCompletionService`
- `JazorProjectionService`
- `JazorTemplateProjection`
- `JazorCodeProjection`

### Virtual Documents

- `VirtualDocument`
- `VirtualDocumentKind`
- `VirtualDocumentIdentity`
- `VueVirtualDocumentBuilder`
- `CSharpVirtualDocumentBuilder`
- `ProjectionMap`
- `ProjectionMapEntry`
- `ProjectionSegment`
- `ProjectionMapComposer`
- `IVirtualDocumentRegistry`
- `VirtualDocumentRegistry`

### Roslyn

- `IRoslynWorkspaceHost`
- `RoslynWorkspaceHost`
- `ProjectedDocumentWorkspaceUpdater`
- `RoslynDiagnosticService`
- `RoslynCompletionService`
- `RoslynHoverService`
- `RoslynDefinitionService`
- `RoslynReferenceService`
- `RoslynRenameService`
- `RoslynCodeActionService`

### Frontend

- `IDenoFrontendHost`
- `DenoFrontendHost`
- `DenoWorkerProcess`
- `FrontendRpcRequest`
- `FrontendRpcResponse`
- `FrontendDiagnostic`
- `FrontendEdit`
- `VolarTsLaneService`   ← unified Volar + TSServer lane service

### LSP

- `ILspLaneRouter`
- `LspLaneRouter`
- `DocumentProjectionResolver`
- `DocumentRegionClassifier`
- `LaneKind`
- `ProjectionTarget`
- `DiagnosticAggregator`
- `WorkspaceEditAggregator`
- `LocationAggregator`
- `HoverHandler`
- `CompletionHandler`
- `DefinitionHandler`
- `ReferencesHandler`
- `RenameHandler`
- `CodeActionHandler`

### Dev Server

- `ModuleLoadService`
- `VirtualModuleResolver`
- `HotUpdatePlanner`
- `HotUpdateClassifier`
- `SourceMapBuilder`
- `SourceMapMerger`

## Lane Model

`Jazor.VueHost` should become a projection-aware broker across three internal lanes.

### `JazorLane`

Responsibilities:

- `.jazor` custom syntax and directives
- document structure
- `.jazor -> virtual .vue` projection (virtual `.cs` is optional)
- ProjectionMap generation (段级位置映射, distinct from Source Map — see Mapping Requirements)
- symbol identity coordination across lanes
- result aggregation: all Lane outputs mapped back to `.jazor` before publishing
- host-native quick fixes
- host-native structural diagnostics

Does not own:

- C# semantics
- Vue/TS/CSS/HTML semantics
- final LSP aggregation

### `RoslynLane`

Responsibilities:

- `@code` completion
- `@code` hover
- `@code` signature help
- C# diagnostics
- C# definition/references/rename/code actions

Input:

- projected virtual C# documents or direct `@code` fragments (approach TBD)
- `.jazor <-> virtual .cs` mapping (if virtual documents are used)

Does not own:

- `.jazor` directives
- Razor markup semantics projected to the frontend lane
- frontend file semantics

### `FrontendLane`

Language services provided by **Volar + TSServer** running in Deno Worker. No independent CSS/HTML/JSON language services.

Responsibilities:

- Razor markup semantics projected to the frontend lane
- Vue component and attribute resolution
- `.vue/.ts/.js/.css/.html` diagnostics and intelligence
- frontend completion/hover/definition/references/rename
- silent degradation when Deno Worker is unavailable (RoslynLane continues independently)

Input:

- projected virtual Vue documents
- tracked frontend workspace documents
- `.jazor <-> virtual .vue` mapping

Does not own:

- `.jazor` rule definition
- C# semantics

## LSP Routing Model

### Core Components

- `DocumentProjectionResolver`
- `LspLaneRouter`
- `LspResultAggregator`

### Projection Target

For a `.jazor` document position/range, the resolver returns:

- `LaneKind`
- `ProjectedDocumentUri`
- `ProjectedRange`
- `MappingId`

Current progress note:

- projection metadata may already be computed during routing, but the current lane implementations still execute against source-document coordinates until a lane consumes projected virtual text end-to-end
- this keeps hover/completion/navigation stable while ProjectionMap and virtual-document consumption are still being tightened

### Region Classification

Suggested defaults:

- directive/import/top-level structure -> `JazorLane`
- `@code` block -> `RoslynLane`
- `<template>` region -> `FrontendLane`
- ambiguous region -> `JazorLane` first, optional fallback to other lanes

## LSP Request Routing Table

### Lifecycle

- `textDocument/didOpen`
  - update workspace
  - parse `.jazor`
  - generate virtual `.cs` and `.vue`
  - sync projected documents to Roslyn and Frontend lanes

- `textDocument/didChange`
  - same as `didOpen`, but incrementally when possible
  - if projection invalidates, rebuild full projection

- `textDocument/didClose`
  - close tracked document
  - close projected documents
  - clear mapping cache when needed

### Primary Language Features

- `textDocument/completion`
  - resolve primary lane
  - execute primary lane
  - allow targeted fallback only when region is ambiguous

- `textDocument/hover`
  - resolve primary lane
  - allow fallback at cross-lane symbol boundaries

- `textDocument/signatureHelp`
  - `RoslynLane` primary
  - `FrontendLane` only for valid frontend expression regions

- `textDocument/definition`
  - primary lane resolves
  - cross-lane symbols are remapped through `JazorLane`
  - final location mapped back to user-visible URI

- `textDocument/references`
  - primary lane resolves
  - if symbol identity is cross-lane, query additional lanes

- `textDocument/rename`
  - never return direct lane output
  - always go through host-level rename coordination

- `textDocument/codeAction`
  - split diagnostics by origin lane
  - collect per-lane actions
  - reproject and aggregate edits

- `textDocument/documentSymbol`
  - `JazorLane` primary
  - optional child symbol expansion later

- `textDocument/semanticTokens`
  - aggregate from all lanes
  - not phase-one critical

## Diagnostics Aggregation

No lane should publish diagnostics directly to the client.

Pipeline:

1. `JazorLane` emits directive/structure/projection diagnostics
2. `RoslynLane` emits projected C# diagnostics
3. `FrontendLane` emits projected Vue/TS/CSS/HTML diagnostics
4. `LspResultAggregator`:
   - maps projected spans back to `.jazor`
   - normalizes source labels
   - de-duplicates
   - sorts
   - publishes diagnostics only on the user document

Suggested diagnostic model:

- `OriginLane`
- `ProjectedDocumentUri`
- `ProjectedRange`
- `OriginalDocumentUri`
- `OriginalRange`
- `DiagnosticCode`
- `DiagnosticSource`
- `Severity`
- `Message`
- `Tags`

Suggested de-dup key:

- `OriginalUri + OriginalStart + OriginalEnd + Code + Message + OriginLane`

## Rename Coordination

`rename` is host-coordinated and projection-aware.

Suggested flow:

1. user issues rename in `.jazor`
2. projection resolver finds primary lane
3. primary lane returns:
   - `SymbolIdentity`
   - projected edits
4. `JazorLane` determines cross-lane scope
5. additional lanes are queried if needed
6. all projected edits are merged
7. edits are mapped back to original user-visible documents
8. edits are sorted by descending offset per document
9. single final `WorkspaceEdit` is returned

Lane rename responses should return projected results, not final LSP edits.

Suggested rename result model:

- `SymbolIdentity`
- `ProjectedEdits[]`
- `AffectedProjectedDocuments[]`
- `CanRename`
- `FailureReason`

## Code Action Coordination

Suggested action groups:

- `Jazor-native`
- `Roslyn-derived`
- `Frontend-derived`

Flow:

1. collect current diagnostics
2. bucket by `OriginLane` or normalized source
3. dispatch to the relevant lane
4. receive projected actions
5. reproject edits to original documents
6. emit final LSP code actions

Constraint:

- do not expose any action that cannot be reliably reprojected back to `.jazor`

## Mapping Requirements

`VirtualDocuments/Mapping` must support:

- original -> projected position/range
- projected -> original position/range
- rename edit span projection
- diagnostic projection
- symbol identity anchoring

Whole-file maps are not sufficient.

Without segment-level mapping:

- diagnostics drift
- rename corrupts source
- definition/references jump incorrectly
- code actions become unsafe

This is the highest-priority technical prerequisite.

## Deno Frontend Runtime

The host uses Deno only.

Recommended process model:

- main process: `Jazor.VueHost` (.NET)
- child process: long-lived Deno frontend worker
- communication: host-controlled RPC over stdio or length-prefixed messages

The Deno worker runs **Volar + TSServer** as the unified frontend language service:

- Volar: `.vue` SFC semantics, template/script/style blocks, CSS/HTML within Vue context
- TSServer (embedded in Volar): TypeScript/JavaScript type checking, completion, navigation
- no independent CSS, HTML, or JSON language services are needed

The Deno worker is **not** allowed to define `.jazor` semantics.

### Degradation

If the Deno worker crashes or fails to start:

- FrontendLane is marked unavailable
- RoslynLane continues independently (C# intelligence for `@code` blocks still works)
- automatic restart with exponential backoff (max 3 retries)
- all failures are silent to the user (no error popups for non-startup failures)

## Project-to-Folder Migration Map

### `Jazor.Vue` (legacy migration source)

Current responsibilities:

- `.jazor` parsing
- Razor document parsing and bridge projection inputs
- virtual `.vue` generation
- bridge models

Target folders:

- `Jazor/Parsing`
- `Jazor/Syntax`
- `Jazor/Projection`
- `VirtualDocuments/Builders`

Legacy note:

- `Jazor.Vue.Analysis`
- `Jazor.Vue.Analysis.Host`
- `Jazor.Vue.Analysis.Runtime`

Those projects belonged to the old split route and should stay removed.
If similar capability is needed again, it must be implemented inside `Jazor.VueHost` rather than revived as separate projects.

## Phase Plan

### Phase 1. Fix the Boundary

- declare `Jazor.VueHost` as the only long-term project boundary
- stop adding new capabilities to `Jazor.Vue*`
- update architecture docs to the single-project model

### Phase 2. Absorb Jazor Core

- move parser/document/projection code from `Jazor.Vue`
- keep old project callable until migrated code is verified

### Phase 3. Build Virtual Document Core

- implement `VirtualDocuments`
- implement precise segment/span mapping
- make this the new foundation for all lanes

### Phase 4. Absorb Roslyn

- move `.jazor` Roslyn projection and diagnostics into `Roslyn/*`
- collapse the separate analysis runtime boundary

### Phase 5. Absorb Frontend Runtime

- move document sync, loading, HMR, and dev maps into `Jazor.VueHost`
- host frontend semantics through Deno workers

### Phase 6. Rewrite LSP and Dev Server

- replace the current hand-written shallow LSP service with lane routing and aggregation
- replace the current external plugin-style dev flow with host-owned loading and HMR services

### Phase 7. Remove Old Projects

- delete project references
- update solution layout
- archive or remove obsolete project folders

## Risks

- virtual document mapping may be too coarse today
- Roslyn behavior may drift during absorption
- frontend worker lifecycle adds host complexity
- deleting projects too early can break existing `getVirtualArtifact`, LSP, or HMR paths
- docs can drift again if code and architecture updates are not done together
- ProjectionMap precision is the single most critical technical prerequisite — without segment-level mapping, diagnostics drift, rename corrupts source, and definition jumps incorrectly
- production bundling (esbuild/Rollup) introduces an external dependency that must be managed separately from the core compilation pipeline

### Degradation Principle

Each Lane must be independently available. A failure in one Lane must not cascade to another. See `vuehost-capabilities.md` Section 14 for the full degradation matrix.

## Safe Rollback Points

- keep old projects in solution until replacement layers are proven
- preserve compatibility entry points before deleting old RPC shapes
- keep old parser/compiler path callable until new projection layer is stable
- treat project deletion as the last cleanup step, not the first migration step

## Phase-One Delivery Scope

Implement first:

- `didOpen`
- `didChange`
- `didClose`
- `completion`
- `hover`
- `definition`
- diagnostics aggregation

Current progress note:

- the host now advertises and serves `references`, `rename`, and `codeAction`, but these remain source-snapshot-first implementations rather than the fully projected Roslyn/Razor end state
- `@code` regions now project into a segment-aware virtual C# document inside `Jazor.VueHost`, and the in-proc Roslyn path serves diagnostics/completion/hover/definition/references/rename before any external Roslyn fallback is considered
- the in-proc Razor/Roslyn projection pipeline is now explicitly shared from `Program` into both `JazorProjectionService` and `RoslynLaneService`, so virtual-document projection and lane queries use one consistent Razor->C# mapping implementation
- in-proc Razor projection now uses `UnsafeAccessor` only (`GetRequiredCSharpDocument`, then `GetCSharpDocument`) with no reflection fallback path
- `textDocument/signatureHelp` is now advertised and served for `@code` regions through the in-proc Roslyn lane, and focused tests now lock active-parameter tracking for multi-argument invocations at both the Roslyn service layer and the end-to-end LSP layer
- `textDocument/documentSymbol` is now advertised and served for `.jazor`, with Jazor structure symbols (`Template` / `Code` plus template component children) aggregated alongside in-proc Roslyn top-level `@code` members
- Roslyn in-proc definition/references/rename now compile over all open `.jazor` projections from workspace state, so code-lane symbol queries can resolve and edit across documents when symbols are shared
- code-region LSP routing no longer blocks on virtual C# document registration; VueHost still routes `@code` requests into Roslyn from the source snapshot when projection materialization is temporarily unavailable
- the bottom bootstrap has started shifting to real language-service entrypoints: Roslyn is now wired as an external stdio host candidate, Razor is treated as a Roslyn-side extension/component rather than a fake standalone lane, and VueHost has dedicated catalog/probe hooks for Volar and tsserver discovery
- the Deno frontend worker is being moved to the same self-contained runtime model used by the repo's `DenoHost`-backed tooling, so frontend intelligence does not depend on a globally installed `deno`
- the Deno frontend host bootstrap is moving to the same self-contained runtime model already used elsewhere in the repository: VueHost now resolves its worker entrypoint and bundled `DenoHost` runtime from its own output layout by default, while still allowing explicit `--deno-command` / `--deno-arg` overrides for diagnostics and local experiments

Original phase-two tail:

- `references`
- `rename`
- `codeAction`
- `semanticTokens`

## Summary

`Jazor.VueHost` should become a single-project, projection-aware host:

- `JazorLane` defines `.jazor` rules and projections
- `RoslynLane` handles C#
- `FrontendLane` handles Vue/TS ecosystem through Deno
- `Jazor.VueHost` itself owns routing, mapping, aggregation, and final protocol serving

The architecture only works if virtual document mapping is treated as core infrastructure from the beginning.
