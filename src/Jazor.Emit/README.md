# Jazor.Emit

> Status: active reference
> Positioning: Module-local operational entry for emit, manifest materialization, bundling, and SourceMap output handoff.

`Jazor.Emit` is the host-facing materialization tool in the Jazor pipeline.

It takes compiler-generated module catalogs from built assemblies, writes concrete output files and manifests, and can bundle the emitted graph into a final artifact through `DenoHost`.

## Responsibilities

- Load the root assembly and referenced assemblies for emit.
- Collect generated ECMAScript and RazorVue catalogs from compiled assemblies.
- Materialize module files and manifests into an output directory.
- Materialize RazorVue output, including sidecar manifest and module-level SourceMap files.
- Bundle emitted modules into a final output artifact.

## Boundaries

- `Jazor.Compiler` and `Jazor.RazorVue` own compile-time semantics and generated catalog shape.
- `Jazor.Emit` owns filesystem materialization, manifest persistence, and bundle orchestration.
- `DenoHost` is the runtime carrier used by the bundling path.

## Key Files

- `Program.cs`: CLI entry for `emit` and `bundle` flows.
- `ModuleCollector.cs`: collects module catalogs from assemblies.
- `ModuleWriter.cs`: writes ECMAScript module output and manifest data.
- `RazorVueCatalogReader.cs`: reads generated RazorVue catalog payloads via reflection.
- `RazorVueModuleWriter.cs`: writes RazorVue modules, manifests, and module-level `.map` files.
- `ModuleBundler.cs`: orchestrates final bundle output.
- `SourceMaps/`: emit-side SourceMap builder and writer types.

## CLI Surface

Emit flow:

```powershell
dotnet run --project src/Jazor.Emit -- --root <root.dll> --assembly <ref.dll> --out <dir> --write-manifest <manifest.json>
```

Bundle flow:

```powershell
dotnet run --project src/Jazor.Emit -- bundle --in <dir> --manifest <manifest.json> --out <bundle.mjs>
```

## Verification

- [Jazor.EmitTest README](../Jazor.EmitTest/README.md)
- `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj`

## Read Next

- [doc/README.md](./doc/README.md)
- [doc/Emit.Pipeline.Overview.md](./doc/Emit.Pipeline.Overview.md)
- [doc/Emit.Materialization.Overview.md](./doc/Emit.Materialization.Overview.md)
- [doc/Emit.BundleAndSourceMap.Overview.md](./doc/Emit.BundleAndSourceMap.Overview.md)
- [docs/plans/emit-materialization-execution-bridge.md](../../docs/plans/emit-materialization-execution-bridge.md)
- [docs/status/2026-04-06-emit-host-materialization-status.md](../../docs/status/2026-04-06-emit-host-materialization-status.md)
- [docs/plans/sourcemap-execution-bridge.md](../../docs/plans/sourcemap-execution-bridge.md)
