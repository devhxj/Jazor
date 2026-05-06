# ECMAScript.VueRoute.Test

`ECMAScript.VueRoute.Test` is the dedicated regression project for the `src/ECMAScript.VueRoute` host binding surface.

## Scope

- Module layout and project wiring guards for the standalone `ECMAScript.VueRoute` library.
- Reflection-based proxy surface checks for the exported Vue Router runtime bindings.
- Compiler-boundary coverage proving the binding types are consumable by `Jazor.Compiler`.
- Packaging and shared-test-entry wiring guards so `ECMAScript.VueRoute` remains part of the normal repo build/test flow.

## Current regression coverage

- `EcmaScriptVueRouteLayoutGuardTests`
- `EcmaScriptVueRouteProxyTests`
- `EcmaScriptVueRouteCompilerBoundaryTests`

These tests intentionally live outside `Jazor.CompilerTest`. The compiler project keeps compiler semantics, while `ECMAScript.VueRoute.Test` owns the external library contract for the Vue Router binding surface.

## Run

```powershell
dotnet test src/ECMAScript.VueRoute.Test/ECMAScript.VueRoute.Test.csproj
```

Run with coverage settings:

```powershell
dotnet test src/ECMAScript.VueRoute.Test/ECMAScript.VueRoute.Test.csproj --settings src/ECMAScript.VueRoute.Test/coverlet.runsettings
```

Or use the shared repo entry point:

```powershell
pwsh ./scripts/test-dotnet.ps1 -Project vueroute
```

## Notes

- The tests read repository files directly to guard solution, package, and script wiring.
- Compiler-boundary tests validate current supported behavior instead of forcing `Jazor.CompilerTest` to carry Vue Router-specific fixtures.
