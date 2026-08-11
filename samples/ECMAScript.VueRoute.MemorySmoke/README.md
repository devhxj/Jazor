# ECMAScript.VueRoute.MemorySmoke

> Purpose: external Vue Router binding, generated modules, Netpack browser bundle, and DenoHost runtime smoke sample.

This sample demonstrates the current `ECMAScript.VueRoute` consumer path with a fixed build/runtime boundary:

- author Vue Router 4 route tables, guards, links, and router-view composition in C#
- emit raw debug `.mjs` modules from a Jazor host project
- build the production browser artifact through `JazorMode=release` and Netpack
- execute generated-module runtime and DOM coverage through the DenoHost-provided Deno runtime

The sample is split into:

- `VueRoute.MemorySmoke.Host`: Jazor host that emits the generated modules
- `vueroute-consumer`: minimal Deno runtime consumer that imports debug modules for coverage and materializes the already-built Netpack browser artifact

## Build from this repository

Use the helper script to build the local package inputs, pack `Jazor`, and rebuild the host:

```powershell
dotnet run --file .\samples\ECMAScript.VueRoute.MemorySmoke\build-local.cs
```

By default, generated output is written to an isolated smoke directory:

```text
.\..\..\.tmp\sample-smoke\ECMAScript.VueRoute.MemorySmoke\Debug\jazor\
```

Run the end-to-end smoke verification from the repository root or sample directory:

```powershell
dotnet run --file .\samples\ECMAScript.VueRoute.MemorySmoke\verify-smoke.cs -- -Configuration Release
```

This validates the production-oriented consumer path:

- pack `Jazor` from the current repository state
- rebuild `VueRoute.MemorySmoke.Host` against the freshly packed local NuGet
- emit isolated generated Vue Router artifacts and assert the expected lowering shape
- build and assert the Netpack release bundle
- run the DenoHost runtime/DOM suites

## Run the frontend consumer

Build the production artifact and then materialize it through the Deno runtime consumer:

```powershell
dotnet run --file .\samples\ECMAScript.VueRoute.MemorySmoke\build-local.cs -- --bundle-out-dir .\.tmp\vueroute-bundle
$env:JAZOR_BUNDLE_ROOT = (Resolve-Path .\.tmp\vueroute-bundle)
cd .\vueroute-consumer
deno task build
deno task test
```

The consumer imports:

- the generated host bootstrap from `host/app.mjs`
- the generated internal `components/*`, `router/*`, `tests/*`, and `System/*` modules through import-map aliases

`verify-smoke.cs` sets `JAZOR_GENERATED_ROOT` so the consumer resolves the isolated generated output instead of relying on a fixed `wwwroot/jazor` path.
