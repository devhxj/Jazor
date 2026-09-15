# Jazor Troubleshooting

Classify the failure before changing code.

## Package and project setup

- `NU1605`, missing types, or incompatible assets: inspect every Jazor and `ECMAScript.*` package reference and align versions to the same preview.
- No generated output: inspect the final executable/web host for `JazorMode`. A class library alone does not own Emit output. Confirm `JazorDir` points to the directory the host actually builds.
- Razor component types do not bind: check the Razor SDK, direct `Jazor.Vue` reference, parameter type shape, and official Razor SG diagnostics before investigating Vue lowering.

## Compiler and analyzer diagnostics

- An unsupported external type is rejected when it is materialized or used in a runtime-sensitive member/type operation. Erased generic arguments alone are not necessarily a failure. Move the fix to the reported usage site.
- An unmapped member must be expressed through an existing ECMAScript/CLR binding or added through the repository's whitelist and generator workflow. Do not add a raw JavaScript escape hatch to silence the diagnostic.
- If a union or component value is too broad, use the binding's closed union or explicit overload. Do not replace it with `object` just because JavaScript is dynamically typed.

## Emit output failures

Emit stages into a sibling temporary directory and atomically replaces `JazorDir`. If the error says that `jazor` is in use or exits with MSBuild code 5:

1. Close terminals, IDE shells, file explorers, dev servers, or other processes whose current working directory is the output directory.
2. Locate a hidden directory handle with Resource Monitor's associated handles or Sysinternals `handle64.exe jazor`.
3. After releasing the handle, remove the stale output directory and rebuild.

The directory may be empty and still locked by a process current working directory; file ACLs are not the first suspect. Keep the original Emit exception when reporting the failure because a cleanup error can otherwise obscure the lock owner clue.

## Generated artifacts and runtime behavior

- Missing imports or resource files: inspect the manifest dependency closure and package-local `dist/**`; do not hand-copy files into `JazorDir`.
- Wrong output semantics: reproduce with the smallest C# source and inspect the compiler test or binding contract. Jazor's semantic walker owns C# expression and member lowering; manual emitted-JS edits are not a durable fix.
- SSR works in one mode but not another: compare `JazorMode`, `JazorSSR`, the materialized runner, and the final host's resource closure. Debug, release, SSR, and HMR are projections of the same selected modules.

## Repository verification

When changing Jazor itself, use the narrowest relevant suite first, then the full mainline script. Documentation or binding changes should also run the repository's XML documentation and binding-contract gates when those scripts exist.
