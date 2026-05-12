# ECMAScript.VueRoute.MemorySmoke

This sample demonstrates the current `ECMAScript.VueRoute` consumer path with a normal Vue/Vite frontend:

- author Vue Router 4 route tables, guards, links, and router-view composition in C#
- emit raw `.mjs` modules from a Jazor host project
- consume those generated modules from Vite with Vue + Vue Router installed from npm

The sample is split into:

- `VueRoute.MemorySmoke.Host`: Jazor host that emits the generated modules
- `vueroute-consumer`: minimal Vite frontend that imports the generated host module and runs Vitest smoke coverage against the generated Vue Router modules

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
- run the Vite build
- run the frontend Vitest runtime/DOM suites

## Run the frontend consumer

After `build-local.cs` succeeds:

```powershell
cd .\vueroute-consumer
npm ci
npm run dev
```

Run the generated-module smoke tests:

```powershell
npm test
```

The consumer imports:

- the generated host bootstrap from `host/app.mjs`
- the generated internal `components/*`, `router/*`, `tests/*`, and `System/*` modules through Vite aliases

`verify-smoke.cs` sets `JAZOR_GENERATED_ROOT` so the consumer resolves the isolated generated output instead of relying on a fixed `wwwroot/jazor` path. The Vite config also aliases `npm:vue@3` and `npm:vue-router@4` to the local npm packages so the generated Jazor modules can run inside a standard Vite toolchain.
