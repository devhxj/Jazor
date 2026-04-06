# Emit Pipeline Overview

> Status: active reference
> Positioning: Module-local overview of the current Jazor.Emit pipeline and boundaries.

## 1. Purpose

This document explains the current role of `Jazor.Emit` inside the Jazor program.

It is the top overview for the local emit doc set.

It answers four questions:

1. what `Jazor.Emit` consumes
2. what it writes
3. where RazorVue and SourceMap continue through emit
4. which responsibilities still belong somewhere else

## 2. Module Position

`Jazor.Emit` is the host-facing materialization layer that sits after compiler-side catalog generation.

The current split is:

- compiler-side modules generate catalog and artifact data
- `Jazor.Emit` reads those compiled carriers from built assemblies
- `Jazor.Emit` writes files and manifests into an output tree
- `Jazor.Emit` can then assemble a final bundle through `DenoHost`

`Jazor.Emit` is not the owner of compile-time semantics.

It is the owner of:

- load/read/materialize/write flow
- manifest persistence
- bundle workspace assembly
- emit-side SourceMap writing

## 3. Main Flow

The current pipeline is:

1. parse CLI options in `Program.cs`
2. load root and referenced assemblies through `EmitLoadContext`
3. collect regular module catalogs and RazorVue catalogs through `ModuleCollector`
4. write regular modules through `ModuleWriter`
5. write RazorVue modules, manifests, and module-level `.map` files through `RazorVueModuleWriter`
6. optionally bundle emitted modules through `ModuleBundler`

This means the emit lane is already a combined path for:

- regular ECMAScript module output
- RazorVue artifact output
- SourceMap sidecar output
- final bundle assembly

## 4. Core Components

Use the narrower follow-up documents when you do not need the whole pipeline at once:

- [Emit.Materialization.Overview.md](./Emit.Materialization.Overview.md)
- [Emit.BundleAndSourceMap.Overview.md](./Emit.BundleAndSourceMap.Overview.md)

### 4.1 Load and collect

- `EmitLoadContext.cs`
- `ModuleCollector.cs`
- `CatalogReader.cs`
- `RazorVueCatalogReader.cs`

This layer loads built assemblies and extracts compiler-owned generated carriers.

Key current rule:

- path conflicts are detected at collection time, before writing output

### 4.2 Manifest and module writing

- `ManifestModel.cs`
- `ModuleWriter.cs`
- `RazorVueManifestModel.cs`
- `RazorVueModuleWriter.cs`

This layer turns collected records into concrete output files.

Current behavior includes:

- skip unchanged files by comparing manifest hash state
- clean removed outputs when `clean` is enabled
- keep regular module and RazorVue manifest evolution parallel rather than collapsing them into one ambiguous carrier

### 4.3 Bundle assembly

- `BundleOptions.cs`
- `ModuleBundler.cs`

This layer prepares a temporary bundle workspace, rewrites intra-graph imports as needed, and invokes `DenoHost` for final bundling.

Current boundary:

- bundling stays in emit and must not leak back into compiler semantic ownership

### 4.4 SourceMap continuation

- `SourceMaps/SourceMapBuilder.cs`
- `SourceMaps/SourceMapWriter.cs`

Current SourceMap position:

- module-level map writing already lives in emit
- SourceMap continuation for RazorVue is an active narrow lane
- broader SourceMap program still remains more conservative than this active slice

## 5. Boundaries

### 5.1 What emit owns

- filesystem materialization
- manifest persistence
- emitted file layout
- RazorVue artifact continuation into files
- emit-side map generation
- bundle orchestration

### 5.2 What emit does not own

- compiler lowering rules
- RazorVue descriptor semantics
- Roslyn generator entry logic
- repo-level sequencing and workstream policy

Those remain in:

- `Jazor.Compiler`
- `Jazor.RazorVue`
- `Jazor.RazorVue.Analysis`
- repo-level `docs/status/` and `docs/plans/`

## 6. Recommended Reading

If you are working on emit itself, read in this order:

1. `src/Jazor.Emit/README.md`
2. this document
3. `Emit.Materialization.Overview.md` or `Emit.BundleAndSourceMap.Overview.md`
4. `src/Jazor.EmitTest/README.md`
5. `docs/status/2026-04-06-emit-host-materialization-status.md`
6. `docs/plans/emit-materialization-execution-bridge.md`

Then enter adjacent lanes only when needed:

- `src/Jazor.Compiler/doc/RazorVue.Overview.md`
- `src/Jazor.Compiler/doc/SourceMap.Overview.md`
