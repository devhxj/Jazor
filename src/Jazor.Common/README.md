# Jazor.Common

> Status: active reference
> Positioning: Shared dependency-bearing support module for naming, RazorVue authoring, source maps, emit-side shared models, and Vue analysis/Jolt protocol DTOs.

`Jazor.Common` is the place for shared code that is used across generator, analyzer, compiler, emit, Jolt, and RazorVue flows but should not live in the dependency-free `ECMAScript.Contract` assembly.

## Responsibilities

- Provide the canonical `SymbolDisplayFormat` and hash-based naming helper through `Format`.
- Host shared RazorVue authoring/analysis/lowering support types.
- Host shared SourceMap models and serialization helpers.
- Host shared emit-side manifest models/serialization helpers.
- Host shared Vue/Jolt RPC/document protocol DTOs.

## Boundaries

- `Jazor.Common` is allowed to carry package dependencies such as Roslyn and `System.Text.Json`.
- Producer-side whitelist declaration primitives such as `JazorAttribute` and `Op` are owned by `ECMAScript.Contract`, not by this module.
- `Jazor.Common` does not own ECMAScript AST definitions or compiler lowering entry points.

## Key Areas

- `Format.cs`: canonical symbol formatting and stable hash naming.
- `RazorVue/`: shared RazorVue discovery, lowering, and authoring support.
- `SourceMaps/`: SourceMap model/writer helpers.
- `Emit/`: shared manifest and serialization helpers.
- `VueContracts/`: shared document/RPC protocol contracts.

## Read Next

- [../ECMAScript.Contract/README.md](../ECMAScript.Contract/README.md)
- [../Jazor.Compiler.Generator/README.md](../Jazor.Compiler.Generator/README.md)
- [../Jolt/README.md](../Jolt/README.md)
