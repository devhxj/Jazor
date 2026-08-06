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
- `test-render-context.cs` runs the RazorVue render-context runtime checks directly with Node.
- `benchmark-razorvue-g2.cs` records the RazorVue G2 benchmark protocol for the plain-text, Counter, and 100-item keyed-list fixtures. It supports protocol dry-run/materialization, `--measure-runtime` for a partial runtime-protocol JSON/Markdown report against handwritten `h()` calls, and `--measure-generated-artifacts` for a partial official Razor SG three-fixture consumer report with per-fixture component/source-map rows, handwritten Vue `h()` `.mjs` baselines, gzip ratios, plus the full artifact table (component modules, source maps, Vue runtime, CLR runtime, manifest); these modes do not claim full G2 performance evidence.
- `wiki-verify-smoke.cs` verifies the current Wiki host smoke path.
- `generate-tdesign.cs` snapshots the locked `tdesign-vue-next` declaration inputs without Node.js or npm; `--check` verifies the frozen snapshot and its external declaration inputs.
- `generate-tdesign-bindings.cs` parses frozen TypeScript declarations with Tree-sitter and generates/verifies the TDesign binding and Props-contract catalogs; `--check` verifies the generated catalogs.
- `generate-tdesign-components.cs` consumes those catalogs and generates the complete strongly typed TDesign surface. `--report` must show every current runtime component, and `--check` verifies that `TBasic.g.cs`, `TComponents.cs`, and `TRegistry.cs` are reproducible. It never substitutes `object`, `VueValue`, or placeholder contracts for props.

The retired `Playground` host and `playground-verify-smoke.cs` lane are no longer part of the active transformation branch. Use Git history for the old Playground smoke workflow.
