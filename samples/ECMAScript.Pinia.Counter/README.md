# ECMAScript.Pinia.Counter

This sample demonstrates the current `ECMAScript.Pinia` consumption path with a normal Vue/Vite frontend:

- author a Pinia option store in C#
- emit raw `.mjs` modules from a Jazor host project
- consume those generated modules from Vite with Vue + Pinia installed from npm

The sample is split into:

- `Pinia.Counter.Host`: Jazor host that emits the generated modules to `wwwroot/jazor/`
- `pinia-consumer`: minimal Vite frontend that imports the generated host module

## Build from this repository

Use the helper script to build the local package inputs, pack `Jazor`, and rebuild the host:

```powershell
.\build-local.ps1
```

Generated output is written to:

```text
.\Pinia.Counter.Host\wwwroot\jazor\
```

You should see:

- `stores/counter-store.mjs`
- `components/counter-app.mjs`
- `host/app.mjs`
- `jazor-manifest.json`

## Run the frontend consumer

After `.\build-local.ps1` succeeds:

```powershell
cd .\pinia-consumer
npm install
npm run dev
```

The consumer imports:

- the generated host bootstrap from `..\Pinia.Counter.Host\wwwroot\jazor\host\app.mjs`
- the generated internal `components/*` and `stores/*` modules through Vite aliases

It also aliases `npm:vue@3` to the local `vue` package so the generated Jazor modules can run inside a standard Vite toolchain.

## What the sample covers

- `createPinia()` root installation on a Vue app
- option-store authoring with `defineStore(...)`
- explicit `StoreDefinition<TStore>.Use()` store resolution
- typed `storeToRefs()` projections
- typed `this`-bound Pinia actions via `Vue3.BindThis(...)`
- direct store runtime calls such as `$patch({ ... })` and `$reset()`

## Notes

- The host emits raw modules instead of relying on `JazorBundle`. This keeps the sample aligned with the current `ECMAScript.Pinia` contract where Pinia itself stays a normal external library import.
- `pinia-consumer` intentionally stays small and explicit so the module-resolution boundary is visible: Vue comes from npm, Pinia comes from npm, and the generated C# modules are imported from the host output.
