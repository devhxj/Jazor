# ECMAScript.Vben.ElementPlusInject

This sample demonstrates the current production-oriented `ECMAScript.Vben` composition path:

- author the app against `ECMAScript.Vben` shell contracts in Razor
- compile-time inject concrete container implementations through `[VueInject]`
- keep the injected implementations sample-local and user-component based
- compose `ECMAScript.ElementPlus` library components inside those injected implementations
- consume the generated Vue SFCs from a colocated Deno pipeline
- keep the entire consumer path on the supported Deno-only consumer flow

The sample is split into:

- `Vben.ElementPlusInject.Library`: RazorVue library authored with Razor and C#
- `Vben.ElementPlusInject.Host`: ASP.NET Core host that emits generated SFCs to `jazor/` and browser assets to `wwwroot/jazor/`
- `Vben.ElementPlusInject.Host/consumer`: colocated Deno consumer using the official `razorvue-consumer-entry` contract

## Build from this repository

Use the helper script to build the local package inputs, pack `Jazor`, `ECMAScript.Vben`, and `ECMAScript.ElementPlus`, then rebuild the host:

```powershell
dotnet run --file .\samples\ECMAScript.Vben.ElementPlusInject\build-local.cs
```

Generated RazorVue artifacts are written to:

```text
.\Vben.ElementPlusInject.Host\jazor\
```

You should see:

- `components/vben-dashboard-app.vue`
- `components/element-admin-layout.vue`
- `components/element-header-bar.vue`
- `components/element-sidebar-menu.vue`
- `components/element-page-container.vue`
- `jazor-manifest.json`
- `__jazor/razorvue-host.mjs`

The colocated browser bundle is emitted by the consumer into:

```text
.\Vben.ElementPlusInject.Host\wwwroot\jazor\
```

## Run the frontend consumer

After `build-local.cs` succeeds:

```powershell
cd .\Vben.ElementPlusInject.Host\consumer
dotnet run --file .\scripts\run-deno.cs -- task test
```

Useful focused commands:

```powershell
dotnet run --file .\scripts\run-deno.cs -- task smoke:ssr
dotnet run --file .\scripts\run-deno.cs -- task smoke:bundle-api
dotnet run --file .\scripts\run-deno.cs -- task build
dotnet run --file .\scripts\run-deno.cs -- task smoke:browser
```

## Run the end-to-end smoke verification

From the repository root:

```powershell
dotnet run --file .\samples\ECMAScript.Vben.ElementPlusInject\verify-smoke.cs -- -Configuration Release
```

This verification:

- packs `Jazor`, `ECMAScript.Vben`, and `ECMAScript.ElementPlus` from the current repository state
- rebuilds the host against the freshly packed local NuGet inputs
- emits generated SFCs into an isolated `.tmp/sample-smoke/.../jazor/` directory by default
- asserts the injected Vben shell artifacts and host requirements module exist
- asserts host requirements carry `element-plus` plugin/style dependencies
- runs Deno SSR smoke
- runs `Deno.bundle()` smoke
- runs browser build
- runs browser smoke

## What the sample covers

- abstract `ECMAScript.Vben` shell authoring
- compile-time container substitution through `[VueInject]`
- sample-local user-component implementations of:
  - `VbenAdminLayout`
  - `VbenHeaderBar`
  - `VbenSidebarMenu`
  - `VbenPageContainer`
- internal composition of `ECMAScript.ElementPlus` wrappers without coupling the public Vben contract to Element Plus
- official `razorvue-consumer-entry` bridge generation
- Deno-only browser/SSR/bundle verification

## Notes

- There is no `ECMAScript.Vben.ElementPlus` product package in this design. Element Plus cooperation stays in the sample/app layer.
- The injected implementations are user components (`IVueComponent + IVueContainerImplementation<TContainer>`), not library components masquerading as the public Vben contract.
- The supported consumer path here is `deno.json` + `scripts/run-deno.cs` + `razorvue-consumer-entry`.
