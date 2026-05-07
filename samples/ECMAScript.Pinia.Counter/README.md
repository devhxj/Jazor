# ECMAScript.Pinia.Counter

This sample demonstrates the current `ECMAScript.Pinia` consumption path with a normal Vue/Vite frontend:

- author a Pinia option store in C#
- emit raw `.mjs` modules from a Jazor host project
- consume those generated modules from Vite with Vue + Pinia installed from npm

The sample is split into:

- `Pinia.Counter.Host`: Jazor host that emits the generated modules to `wwwroot/jazor/`
- `pinia-consumer`: minimal Vite frontend that imports the generated host module and now also includes a Vitest smoke test against the generated Pinia/testing modules

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
- `stores/activity-store.mjs`
- `components/counter-app.mjs`
- `components/counter-cookbook.mjs`
- `components/counter-multi-store.mjs`
- `components/counter-subscription.mjs`
- `components/counter-hydration.mjs`
- `components/counter-hmr.mjs`
- `tests/counter-testing.mjs`
- `host/app.mjs`
- `jazor-manifest.json`

## Run the frontend consumer

After `.\build-local.ps1` succeeds:

```powershell
cd .\pinia-consumer
npm install
npm run dev
```

Run the generated-module smoke tests:

```powershell
npm test
```

The consumer imports:

- the generated host bootstrap from `..\Pinia.Counter.Host\wwwroot\jazor\host\app.mjs`
- the generated internal `components/*`, `stores/*`, and `tests/*` modules through Vite aliases

It also aliases `npm:vue@3` to the local `vue` package so the generated Jazor modules can run inside a standard Vite toolchain.
The Vitest setup also aliases `@pinia/testing` so the generated `tests/counter-testing.mjs` artifact can execute as a normal frontend-side testing seam.
The generated root app now also installs the sample Pinia audit plugin through `createConfiguredPinia()`, so projected custom properties/state are exercised through the same runtime path the sample UI uses.
The generated root app also disposes its Pinia root on `app.unmount()`, so repeated mount/unmount flows do not retain store state or plugin side effects.
The consumer also includes a small JS-side HMR bridge module so `acceptHMRUpdate(...)` stays generated in C# while `import.meta.hot.accept(...)` remains an explicit host concern.

## What the sample covers

- `createPinia()` root installation on a Vue app
- `createTestingPinia()` testing root authoring through the standalone `ECMAScript.Pinia.Testing` line
- option-store authoring with `defineStore(...)`
- explicit `StoreDefinition<TStore>.Use()` store resolution
- typed `storeToRefs()` projections
- projected plugin store / store-definition authoring via `ProjectStoreDefinition(...)`
- projected store flowing through `storeToRefs()`, object-form `mapState()`, and `mapActions()`
- object-form `mapState()` explicit union factory authoring through `PiniaStateMapValue<TStore>.From("key")` and `PiniaStateMapValue<TStore>.From(selector)`
- multi-store Options API helper authoring via `mapStores()` + `setMapStoreSuffix("")`
- `$subscribe()` cookbook coverage across direct mutation, object patch, and function patch flows
- `skipHydrate()` / `shouldHydrate()` plus option-store `hydrate(storeState, initialState)` cookbook coverage
- explicit multi-root `StoreDefinition.Use(pinia)` isolation coverage
- `acceptHMRUpdate(...)` plus `StoreDefinition<TStore>.Use(pinia, hot)` cookbook coverage
- consumer-side `import.meta.hot.accept(...)` bridge over the generated HMR handlers
- typed `this`-bound Pinia actions via `Vue3.BindThis(...)`
- plugin-added custom properties / custom state cookbook contracts
- direct store runtime calls such as `$patch({ ... })` and `$reset()`
- root lifecycle coverage for `setActivePinia()` / `setActivePinia(undefined)` / `getActivePinia()` / `disposePinia()`
- generated host helper coverage for clearing the active root via `ClearActivePinia()` instead of consumer-side raw `setActivePinia(undefined)` calls
- generated root-app teardown coverage for `app.unmount()` -> `disposePinia(...)`
- testing-only state seeding, selective `stubActions`, and plugin install ordering through `TestingOptions`
- combined typed testing-root authoring through `TestingOptions<TDelegate, TStore>` without changing the emitted `@pinia/testing` runtime shape
- testing-root `fakeApp` / `TestingPinia.app` runtime seam
- testing-only named-action `stubActions` contract through the standalone `ECMAScript.Pinia.Testing` line
- testing-root typed `stubActions` predicate projection through `ProjectStubActionPredicate<TStore>(...)`
- testing-root combined typed `createSpy` + typed `stubActions` projection through `ProjectStubActions<TStore>(...)`
- testing-root combined typed `createSpy` + typed `stubActions` explicit union factory path through `TestingStubActions<TStore>.From(...)`
- testing-root typed/projected plugin reuse through `ProjectPlugin(...)`, including projected custom-state writes on the generated testing root
- strict testing-root coverage for named action stubs plus `stubPatch` / `stubReset`
- frontend-side Vitest smoke coverage against generated `createTestingPinia()` + store modules
- frontend-side Vitest DOM coverage against the generated root app, including plugin projection, multi-store rendering, subscription notifications, hydration state, and HMR cookbook state
- frontend-side Vitest runtime coverage for store `$dispose()`, root recreation after `disposePinia()`, and repeated mount/unmount cleanup
- frontend-side Vitest runtime/DOM coverage for explicit multi-root isolation and non-leaking plugin custom state

## Notes

- The host emits raw modules instead of relying on `JazorBundle`. This keeps the sample aligned with the current `ECMAScript.Pinia` contract where Pinia itself stays a normal external library import.
- The testing root module is emitted as a normal generated artifact so consumers can inspect `@pinia/testing` lowering without mixing testing-only APIs back into `ECMAScript.Pinia` main package code.
- `pinia-consumer` intentionally stays small and explicit so the module-resolution boundary is visible: Vue comes from npm, Pinia comes from npm, `@pinia/testing` comes from npm, and the generated C# modules are imported from the host output.
