# Emit Materialization Overview

> Status: active reference
> Positioning: Module-local overview of manifest persistence and file materialization inside `Jazor.Emit`.

## 1. Purpose

This document focuses on the non-bundling half of the emit lane.

Use it when you need to understand:

1. how emit reads compiler-owned carriers
2. how output files and manifests are written
3. how RazorVue artifacts continue into concrete emitted files

## 2. Main Materialization Path

The current materialization path is:

1. load root and referenced assemblies through `EmitLoadContext`
2. collect ECMAScript and RazorVue carriers through `ModuleCollector`
3. write regular modules through `ModuleWriter`
4. write RazorVue modules and sidecar manifest data through `RazorVueModuleWriter`
5. persist manifest state for incremental skip/clean behavior

This is the part of emit that turns compiler output into a stable on-disk tree.

## 3. Core Components

### 3.1 Load and collect

- `EmitLoadContext.cs`
- `ModuleCollector.cs`
- `CatalogReader.cs`
- `RazorVueCatalogReader.cs`

Current role:

- load built assemblies safely
- extract compiler-generated catalog payloads
- detect content and relative-path conflicts before write time

### 3.2 Regular module materialization

- `ManifestModel.cs`
- `ModuleWriter.cs`

Current role:

- write regular ECMAScript module files
- track hash-based manifest state
- skip unchanged files
- clean removed files when requested

### 3.3 RazorVue materialization

- `RazorVueManifestModel.cs`
- `RazorVueModuleWriter.cs`

Current role:

- materialize RazorVue artifact modules
- persist RazorVue-specific manifest data
- append module-level SourceMap sidecars
- keep RazorVue output evolution parallel to regular module output rather than hiding it inside one merged manifest

## 4. Key Rules

- collection-time conflict detection happens before any file write
- output writes must stay inside the configured output directory
- regular modules and RazorVue artifacts evolve in parallel, not as one ambiguous carrier
- manifest state is part of materialization behavior, not a secondary afterthought

## 5. Boundaries

This path owns:

- filesystem writes
- manifest persistence
- output-tree shape
- RazorVue artifact continuation into files

This path does not own:

- compiler lowering semantics
- RazorVue descriptor meaning
- bundle orchestration policy beyond handing off emitted files

## 6. Read Next

- [Emit.Pipeline.Overview.md](./Emit.Pipeline.Overview.md)
- [Emit.BundleAndSourceMap.Overview.md](./Emit.BundleAndSourceMap.Overview.md)
- [../README.md](../README.md)

