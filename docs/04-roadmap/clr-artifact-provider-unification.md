# ECMAScript JS Resource Package Note

> This is a narrow implementation note for the CLR runtime JavaScript package. The authoritative
> carrier, dependency, Emit, test, and release plan is [Artifact Graph Stabilization Plan](./artifact-graph-stabilization-plan.md).

## Stable Ownership

`Jazor.CLR` owns the C# mapping surface, `[Jazor]` declarations, whitelist source, and runtime
semantics. Its lowered JavaScript belongs to `src/ECMAScript`, which is a JS resource library with
the package contract:

```text
src/ECMAScript/
  manifest.json
  dist/
    System/**
```

`src/ECMAScript` is not a pure Jazor library. Its C# sources may participate in analysis and the
build-time generation of `dist`, but its shipped JavaScript carrier is always `manifest.json +
dist/**`. A developer-authored Jazor class library is the other fixed form: it carries generated
modules in `Jazor.Generated.ModuleCatalog` inside its assembly.

## Delivery Flow

```text
Jazor.CLR mappings and modules
          -> compiler lowering
          -> ECMAScript manifest.json + dist/**
          -> final host Emit materialization
```

The final `Exe` or `WinExe` host supplies the resource manifest locator through MSBuild. `Jazor.Emit`
validates the manifest, resolves only explicit module and package dependencies, and copies the
selected closure to `JazorDir`. Referencing `ECMAScript.dll` does not itself select or copy all
`System/**` modules.

## Required Invariants

- Every runtime module and auxiliary file is declared with its typed manifest record, path, hash,
  and explicit dependency edge.
- No directory scan, JavaScript-text dependency inference, or CLR-path-prefix rule expands the
  runtime closure.
- A pure Jazor assembly never rewrites or repackages `ECMAScript` resources; it records only its
  own generated modules in `ModuleCatalog`.
- The same manifest/dist bytes and dependency closure are used for project references and NuGet
  consumers.
- Debug, Release, SSR, and HMR are output projections over the same two library inputs. They do
  not create another class-library carrier.

The repository-wide manifest contract test and the Emit integration tests are the executable
verification for this note.
