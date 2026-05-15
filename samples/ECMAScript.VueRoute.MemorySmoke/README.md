# ECMAScript.VueRoute.MemorySmoke

This sample demonstrates the current `ECMAScript.VueRoute` consumer path with a Deno-based frontend:

- author Vue Router 4 route tables, guards, links, and router-view composition in C#
- emit raw `.mjs` modules from a Jazor host project
- consume those generated modules from Deno with Vue + Vue Router resolved through an import map

The sample is split into:

- `VueRoute.MemorySmoke.Host`: Jazor host that emits the generated modules
- `vueroute-consumer`: minimal Deno consumer that imports the generated host module and runs build/runtime/DOM smoke coverage against the generated Vue Router modules

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
- run the Deno bundle build
- run the Deno runtime/DOM suites

## Run the frontend consumer

After `build-local.cs` succeeds:

```powershell
cd .\vueroute-consumer
deno task build
deno task test
```

The consumer imports:

- the generated host bootstrap from `host/app.mjs`
- the generated internal `components/*`, `router/*`, `tests/*`, and `System/*` modules through import-map aliases

`verify-smoke.cs` sets `JAZOR_GENERATED_ROOT` so the consumer resolves the isolated generated output instead of relying on a fixed `wwwroot/jazor` path.
