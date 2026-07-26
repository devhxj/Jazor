# RazorVue.TodoList

> Status: legacy pre-G0 sample, pending migration to the current RazorVue render-function `.mjs` artifact path.
>
> This README documents the old generated-SFC consumer workflow so the migration target is explicit. It is not a current architecture guide. Current RazorVue production input is official Razor SG generated C# and the output contract is Vue render-function `.mjs`.

This sample demonstrates the retired RazorVue library-mode path:

- author components in `.razor + .razor.cs`
- generate Vue `.vue` SFC artifacts at design time in the legacy flow
- keep a single ASP.NET Core host as the runtime boundary
- consume the generated SFCs from a colocated Deno frontend pipeline in the legacy flow
- materialize publish output into `wwwroot/jazor`

The sample is split into:

- `Todo.Library`: RazorVue component library authored with Razor and C#
- `Todo.Host`: the single ASP.NET Core host project; development emit goes to `Todo.Host/jazor`, browser assets go to `Todo.Host/wwwroot/jazor`
- `Todo.Host/consumer`: a colocated DenoHost consumer that uses the retired `razorvue-consumer-entry` contract and bundles browser assets without Vite or npm wrapper scripts

`Todo.Library` follows the explicit authoring contract. Component marker types are brought in with:

```csharp
using static ECMAScript.Vue3;
```

The sample does not rely on package-level global aliases for `IVueComponent` / `IVueLibraryComponent`.

## Build from this repository

Use the helper script to build the local package inputs, pack `Jazor` and `ECMAScript.Vuetify`, and rebuild the host:

```powershell
dotnet run --file .\samples\RazorVue.TodoList\build-local.cs
```

Generated RazorVue artifacts are written to:

```text
.\Todo.Host\jazor\
```

You should see:

- `components/todo-app.vue`
- `components/todo-summary-card.vue`
- `jazor-manifest.json`
- `__jazor/razorvue-host.mjs`

The colocated browser bundle is emitted by the consumer into:

```text
.\Todo.Host\wwwroot\jazor\
```

The host shell served by ASP.NET Core lives at:

```text
.\Todo.Host\wwwroot\index.html
```

## Run the frontend consumer

After `build-local.cs` succeeds:

1. open `Todo.Host/consumer/`
2. run the pure Deno pipeline through the bundled runtime entry

```powershell
cd .\Todo.Host\consumer
dotnet run --file .\scripts\run-deno.cs -- task test
```

The consumer imports:

- the generated root component through the retired `razorvue-consumer-entry` bridge modules
- host metadata from `..\jazor\__jazor\razorvue-host.mjs`

and then:

- uses the retired `Jazor.Emit razorvue-consumer-entry` command to generate browser/SSR bridge modules and stable entrypoints
- runs SSR smoke through `vue/server-renderer` + Vuetify
- runs a `Deno.bundle()` smoke over the prepared browser entry
- runs `deno bundle` to emit the browser build under `Todo.Host/consumer/dist/jazor/`
- copies the generated browser assets into `Todo.Host/wwwroot/jazor/`

Useful focused commands:

```powershell
dotnet run --file .\scripts\run-deno.cs -- task smoke:ssr
dotnet run --file .\scripts\run-deno.cs -- task smoke:bundle-api
dotnet run --file .\scripts\run-deno.cs -- task build
```

## What the sample covers

- legacy design-time SFC generation in library mode
- Razor authoring with `.razor + .razor.cs`
- user component composition
- Vuetify library component integration
- `v-if` / `v-for`
- local state, methods, and computed-style lifted bindings
- `Xxx + XxxChanged` model binding surfaces

## Notes

- `Todo.Host` is the only runtime host. The colocated `consumer` directory is a build-time frontend consumption layer, not a second application host.
- The Deno consumer is intentionally small and explicit so the legacy generated SFCs are consumed through the retired `razorvue-consumer-entry` contract instead of a project-private SFC compiler.
- `Todo.Host/consumer` no longer relies on `package.json` / `npm run ...` wrapper scripts. The repository-level contract is `deno.json` tasks plus `scripts/run-deno.cs`, which executes the bundled Deno runtime through `DenoHost`.
- `Todo.Library` currently sets `UseRazorSourceGenerator=false`. The current library-mode design-time path still depends on generated `*.razor.g.cs` being present in compilation.
- The generated SFCs do not emit `<style src="vuetify/styles">` blocks. Style and plugin requirements stay in `__jazor/razorvue-host.mjs`, and the Deno consumer imports `vuetify/styles` explicitly.
- `Todo.Host/consumer/scripts/lib/pipeline.ts` owns only consumer orchestration. Legacy SFC bridge generation is delegated to the retired `Jazor.Emit razorvue-consumer-entry` command.
- `deno bundle` is the formal browser build entry for this sample. `Deno.bundle()` is kept as an additional API-level smoke because its option surface is still unstable.
- `build-local.cs` is fail-fast. If any framework build, pack, restore, or host rebuild step fails, the script stops instead of silently continuing with stale outputs.
