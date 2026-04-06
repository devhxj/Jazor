# Jazor.Common

> Status: active reference
> Positioning: Module-local operational entry for shared producer-side contracts used across generator, analyzer, compiler, and CLR mapping.

`Jazor.Common` is the shared contract surface for Jazor whitelist declaration.

It defines the attribute and operation enum that producer-side code uses to declare how a type or member should enter the Jazor compilation domain.

## Responsibilities

- Define `JazorAttribute` as the producer-side fact source for whitelist mapping.
- Define `Op` as the shared declaration vocabulary for allowed, aliased, inline, imported, discarded, and compiler-owned cases.
- Provide a stable shared contract used by generator, analyzer, compiler, and CLR mapping code.

## Boundaries

- `Jazor.Common` declares producer-side intent; it does not implement compiler lowering.
- `Jazor.Compiler.Generator` scans these declarations and generates whitelist data.
- `Jazor.Analyzer` and `Jazor.Compiler` consume the generated mapping and runtime rules.

## Key Files

- `JazorAttribute.cs`: producer-side whitelist declaration attribute.
- `Op.cs`: shared enum describing declaration-side handling mode.

## Read Next

- [../Jazor.Compiler.Generator/README.md](../Jazor.Compiler.Generator/README.md)
- [../Jazor.CLR/readme.md](../Jazor.CLR/readme.md)
- [../../src/Jazor.Compiler/doc/OpCompileSpec.md](../../src/Jazor.Compiler/doc/OpCompileSpec.md)
- [../../src/Jazor.Compiler/doc/WhiteList.md](../../src/Jazor.Compiler/doc/WhiteList.md)
