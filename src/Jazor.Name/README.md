# Jazor.Name

> Status: active reference
> Positioning: Module-local operational entry for shared symbol-formatting and hash naming utilities.

`Jazor.Name` is the shared naming utility module for Jazor.

It provides the canonical Roslyn symbol display format and the hash-based member naming helper used by whitelist generation and compiler/runtime mapping code.

## Responsibilities

- Provide the canonical `SymbolDisplayFormat` used to turn Roslyn symbols into stable Jazor names.
- Provide hash-based member naming through `Format.HashName`.
- Keep shared naming logic in one place so generator, analyzer, compiler, and CLR mapping stay consistent.

## Boundaries

- `Jazor.Name` does not own whitelist declarations or compiler lowering behavior.
- It is a utility dependency used by `Jazor.Compiler`, `Jazor.Compiler.Generator`, and `Jazor.Analyzer`.

## Key File

- `Format.cs`: shared symbol display format plus stable hash-name generation.

## Read Next

- [../Jazor.Compiler.Generator/README.md](../Jazor.Compiler.Generator/README.md)
- [../../src/Jazor.Compiler/doc/WhiteList.md](../../src/Jazor.Compiler/doc/WhiteList.md)
- [../../src/Jazor.Compiler/doc/OpCompileSpec.md](../../src/Jazor.Compiler/doc/OpCompileSpec.md)
