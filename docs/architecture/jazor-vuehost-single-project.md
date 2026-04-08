# Jazor.VueHost Single-Project Host Design

## Status

- status: proposed replacement architecture
- scope: `.jazor` authoring host, LSP, virtual documents, frontend language services, dev-time module loading
- runtime choice: Deno
- non-scope: RazorVue library route and its historical host/bundling assumptions

## Decision

`Jazor.VueHost` becomes the only project boundary and the only public entry point.

The following existing project boundaries are treated as temporary migration sources, not long-term architecture:

- `Jazor.Vue`
- `Jazor.Vue.Analysis`
- `Jazor.Vue.Analysis.Runtime`
- `Jazor.Vite`

After migration, their capabilities are absorbed into `Jazor.VueHost` and organized by folders instead of separate projects.

This design explicitly does **not** inherit RazorVue's host/bundling conclusions. RazorVue is a separate library technology path and is out of scope for this host design.

## Core Principles

- `.jazor` is the primary authoring document.
- `Jazor.VueHost` is the only IDE/dev-host boundary.
- Roslyn and frontend semantics are internal lanes, not separate products.
- Deno is the only frontend runtime for this host.
- LSP is projection-aware and lane-aware.
- Virtual document mapping is a first-class subsystem, not a helper detail.
- Capability migration happens before project deletion.

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
│  ├─ Volar
│  ├─ TypeScript
│  ├─ Vue
│  ├─ CssHtml
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
- `VolarLaneService`
- `TypeScriptLaneService`
- `CssHtmlLaneService`

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
- `.jazor -> virtual .vue / virtual .cs` projection
- symbol identity coordination across lanes
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

- projected virtual C# documents
- `.jazor <-> virtual .cs` mapping

Does not own:

- `.jazor` directives
- template semantics
- frontend file semantics

### `FrontendLane`

Responsibilities:

- template semantics
- Vue component and attribute resolution
- `.vue/.ts/.js/.css/.html` diagnostics
- frontend completion/hover/definition/references/rename

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

- main process: `Jazor.VueHost`
- child process: long-lived Deno frontend worker
- communication: host-controlled RPC over stdio or length-prefixed messages

The Deno worker is responsible for:

- Volar integration
- TypeScript language services
- Vue/TS/JS/CSS/HTML semantic responses

The Deno worker is **not** allowed to define `.jazor` semantics.

## Project-to-Folder Migration Map

### `Jazor.Vue`

Current responsibilities:

- `.jazor` parsing
- import/template/code split
- virtual `.vue` generation
- bridge models

Target folders:

- `Jazor/Parsing`
- `Jazor/Syntax`
- `Jazor/Projection`
- `VirtualDocuments/Builders`

### `Jazor.Vue.Analysis`

Current responsibilities:

- Roslyn-facing shallow analysis
- `.jazor` semantic projection
- deterministic diagnostics

Target folders:

- `Roslyn/*`
- `Jazor/Diagnostics`

### `Jazor.Vue.Analysis.Runtime`

Current responsibilities:

- `AnalyzeJazor` runtime entry
- RPC processor/server primitives

Target folders:

- `Roslyn/*`
- `Protocol/Rpc`

### `Jazor.Vite`

Current responsibilities:

- plugin-style virtual module loading
- document sync
- HMR adaptation
- dev-time source map shaping

Target folders:

- `Frontend/*`
- `DevServer/*`
- `Workspace/*`
- `Infrastructure/Processes`

## Phase Plan

### Phase 1. Fix the Boundary

- declare `Jazor.VueHost` as the only long-term project boundary
- stop adding new capabilities to `Jazor.Vue*` and `Jazor.Vite`
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

Delay to phase two:

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
