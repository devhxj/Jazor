# Jazor.Emit

> Status: active reference
> Positioning: host-facing ECMAScript module materialization and bundle layer.

`Jazor.Emit` materializes compiler-produced ECMAScript catalogs and source-map carriers. It owns assembly loading, deterministic file output, manifest maintenance, cleanup, and Deno-backed browser bundling; it does not own compiler lowering semantics.

## Responsibilities

- Load the root assembly and explicitly supplied reference assemblies.
- Collect generated ECMAScript module catalogs, `Jazor.Generated.VueRenderCatalog`, the repository-owned CLR runtime catalog, and embedded RazorVue render-context runtime assets.
- Write `.mjs`, optional `.map`, and the shared schema-v1 `jazor-manifest.json`.
- Remove stale module and source-map files when clean output is requested.
- Bundle manifest modules through `DenoHost` while preserving root-assembly exports and chained source maps.

RazorVue catalog, SFC, bridge, consumer-entry, host-sidecar, and update-plan contracts were retired after the Razor SG G0 gate. They are not part of this tool's CLI or manifest model.

## Key Files

- `Program.cs`: CLI entry point.
- `CatalogReader.cs`: reads generated, CLR runtime, VueRenderCatalog, source-map, and RazorVue runtime resource catalogs.
- `ModuleCollector.cs`: merges modules across assemblies with deterministic conflict handling.
- `ModuleWriter.cs`: writes modules, source maps, and the manifest.
- `ModuleBundler.cs`: orchestrates browser bundling and source-map chaining.
- `ManifestModel.cs`: defines the canonical schema-v1 module manifest contract; legacy `rootAssemblyPath` / `generatedAtUtc` manifests remain readable, but new manifest writes use `rootAssemblyName` and omit wall-clock or machine-absolute state.

## CLI

Emit modules:

```powershell
dotnet run --project src/Jazor.Emit -- --root <root.dll> --assembly <ref.dll> --out <dir> --write-manifest <manifest.json>
```

Bundle modules:

```powershell
dotnet run --project src/Jazor.Emit -- bundle --in <dir> --manifest <manifest.json> --out <bundle.mjs>
```

## Verification

```powershell
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj
```

## Read Next

- [../Jazor.EmitTest/README.md](../Jazor.EmitTest/README.md)
- [../../docs/01-目标/compiler/emit/Emit.Pipeline.Overview.md](../../docs/01-目标/compiler/emit/Emit.Pipeline.Overview.md)
- [../../docs/01-目标/compiler/emit/Emit.Materialization.Overview.md](../../docs/01-目标/compiler/emit/Emit.Materialization.Overview.md)
- [../../docs/01-目标/compiler/emit/Emit.BundleAndSourceMap.Overview.md](../../docs/01-目标/compiler/emit/Emit.BundleAndSourceMap.Overview.md)
