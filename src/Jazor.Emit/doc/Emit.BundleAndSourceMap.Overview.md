# Emit Bundle And SourceMap Overview

> Status: active reference
> Positioning: Module-local overview of bundling and SourceMap continuation inside `Jazor.Emit`.

## 1. Purpose

This document focuses on the bundling and SourceMap side of the emit lane.

Use it when you need to understand:

1. how emit assembles a final bundle
2. how import rewriting is kept inside emit
3. where module-level SourceMap generation currently lives
4. how the narrow active SourceMap lane continues through emit

## 2. Bundle Path

The current bundle path is:

1. load manifest state from emitted output
2. assemble a temporary bundle workspace
3. copy emitted modules into that workspace
4. rewrite intra-graph import paths when needed
5. generate a temporary bundle entry file
6. invoke `DenoHost` for final bundle output

This keeps bundling in the host-facing lane instead of leaking it back into compiler semantics.

## 3. Core Components

### 3.1 Bundle assembly

- `BundleOptions.cs`
- `ModuleBundler.cs`

Current role:

- prepare a temporary workspace
- normalize manifest-driven entry selection
- rewrite imports for the workspace view
- invoke `DenoHost` for final bundle creation

Key rule:

- bundling stays an emit concern and must not force compiler redesign

### 3.2 SourceMap generation

- `SourceMaps/SourceMapBuilder.cs`
- `SourceMaps/SourceMapDocument.cs`
- `SourceMaps/SourceMapWriter.cs`
- `RazorVueModuleWriter.cs`

Current role:

- build module-level `.map` files for RazorVue-emitted modules
- persist `sourcesContent`
- append `sourceMappingURL` to emitted module files

## 4. Current Program Position

The current SourceMap situation in emit is intentionally narrow:

- module-level map generation already lives here
- RazorVue-related SourceMap continuation is active
- broader SourceMap rollout remains more conservative than this slice

That means emit is already the operational home of SourceMap writing,
but not the owner of the entire broad SourceMap program.

## 5. Boundaries

This path owns:

- bundle workspace assembly
- host-facing bundle invocation
- module-level SourceMap writing
- emit-side continuation for narrow SourceMap rollout

This path does not own:

- broad SourceMap program policy
- compiler-side origin semantics
- runtime HMR policy

## 6. Read Next

- [Emit.Pipeline.Overview.md](./Emit.Pipeline.Overview.md)
- [Emit.Materialization.Overview.md](./Emit.Materialization.Overview.md)
- [../../Jazor.Compiler/doc/SourceMap.Overview.md](../../Jazor.Compiler/doc/SourceMap.Overview.md)
- [../../../docs/plans/sourcemap-execution-bridge.md](../../../docs/plans/sourcemap-execution-bridge.md)

