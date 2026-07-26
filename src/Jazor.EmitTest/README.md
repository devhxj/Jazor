# Jazor.EmitTest

`Jazor.EmitTest` covers the file emission and bundle pipeline in `src/Jazor.Emit`.

## Scope

- Manifest-driven bundle generation.
- CLR runtime and generated module catalog collection.
- Cross-module import rewriting inside the temporary bundle workspace.
- Root-assembly export preservation, so the final bundle re-exports the host module members instead of collapsing to an empty file.
- Static module, chained bundle, and writer source-map behavior.
- Retirement guards for the removed RazorVue emit contracts.

## Current regression coverage

- `BundleAsync_SingleRootModule_PreservesExports`
- `BundleAsync_MultiProjectHostBundle_ReExportsRootAssemblyMembers`
- `ModuleCollector_Collect_ReadsClrRuntimeModules_FromEcmascriptAssemblyCatalog`
- `EmitAssembly_DoesNotExposeLegacyRazorVueCatalogOrConsumerContracts`

These tests exercise `ModuleBundler` directly and validate the bundled JavaScript output, not just the exit code.

## Run

```powershell
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj
```

Or run the shared repo test entry point:

```powershell
dotnet run --file ./scripts/csharp/test-dotnet.cs -- --project emit
```

## Notes

- The tests use temporary workspaces and clean them up after execution.
- `Jazor.Emit` depends on `DenoHost`; if restore prints `NU1900` warnings for vulnerability feeds, that does not block the tests as long as package restore succeeds.
