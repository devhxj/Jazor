# C# Diagnostic Scripts

This directory contains single-file C# apps for local repository diagnostics.

Run scripts from the repository root with:

```powershell
dotnet run --file scripts/csharp/<script-name>.cs
```

Use these scripts for reflection, Roslyn, metadata inspection, and other probes where inline PowerShell quoting would be fragile. Keep scripts deterministic and read-only unless their name and header clearly say otherwise.

## Current script lanes

- `test-dotnet.cs` builds once and runs the active compiler, CLR, Pinia, VueRoute, Razor SG, emit, and render-context suites.
- `verify-compiler-coverage.cs` runs the complete compiler suite and fails unless at least 10,000 tests pass with 98% line and 96% branch coverage for `Jazor.Compiler`.
- `verify-vue-binding-coverage.cs` runs the Vue binding test lanes and independently audits every public binding contract unit in Vue3, Vuetify, Element Plus, TDesign, Pinia, Pinia Testing, and Vue Router. It requires every target to reach 80%; this is a metadata-contract audit, not misleading Coverlet IL line coverage for `extern` wrappers.
- `test-render-context.cs` runs the RazorVue render-context runtime checks directly with Node.
- `benchmark-razorvue-g2.cs` records the RazorVue G2 benchmark protocol for the plain-text, Counter, and 100-item keyed-list fixtures. `--write-release-report` runs the protocol, runtime, external official Razor SG package-consumer, and browser lanes, including generated module/source-map/manifest checks. The report is a reproducible baseline and keeps warnings or unavailable retired-line comparisons explicit rather than claiming performance completion.
- `wiki-verify-smoke.cs` verifies the current Wiki host smoke path.
- `generate-jazoradmin-brand-assets.cs` deterministically regenerates the JazorAdmin 16/32/48/64px ICO fallback from the compact local mark; pass `--check` in verification lanes.
- Element Plus, Vuetify, and TDesign binding maintenance is owned by `src/ECMAScript.Vue.Generator/`; it is a project because it owns versioned snapshots and multiple reproducible generator lanes, rather than a local diagnostic script.

The retired `Playground` host and `playground-verify-smoke.cs` lane are no longer part of the active transformation branch. Use Git history for the old Playground smoke workflow.
