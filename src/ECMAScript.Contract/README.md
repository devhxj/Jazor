# ECMAScript.Contract

> Status: active reference
> Positioning: Dependency-free contract assembly for the smallest shared declaration surface.

`ECMAScript.Contract` holds the minimal primitives that producer-side declaration code and consumer-side compiler/analyzer/generator code need to agree on without pulling Roslyn, JSON, or other higher-level dependencies into the `ECMAScript` namespace.

## Responsibilities

- Define `JazorAttribute` as the producer-side whitelist declaration primitive.
- Define `Op` as the declaration-side operation vocabulary.
- Keep the contract assembly dependency-free.
- Expose only the minimal marker/base contract surface needed by internal producer/consumer layers.

## Boundaries

- `ECMAScript.Contract` does not own RazorVue authoring implementation, source maps, emit helpers, or Vue/Jolt protocol DTOs; those live in `Jazor.Common`.
- `ECMAScript.Contract` does not implement compiler lowering or analyzer policy.
- `JazorAttribute` and `Op` are internal to the repo and are shared with the required assemblies through `InternalsVisibleTo`.

## Key Files

- `JazorAttribute.cs`: producer-side whitelist declaration attribute.
- `Op.cs`: shared declaration-side operation enum.
- `IComponent.cs`: minimal Razor component marker contract.

## Read Next

- [../Jazor.Common/README.md](../Jazor.Common/README.md)
- [../Jazor.Compiler.Generator/README.md](../Jazor.Compiler.Generator/README.md)
- [../Jazor.CLR/readme.md](../Jazor.CLR/readme.md)
