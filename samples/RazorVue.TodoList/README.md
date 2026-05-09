# RazorVue.TodoList

This sample demonstrates the current RazorVue library-mode path:

- author components in `.razor + .razor.cs`
- generate Vue `.vue` SFC artifacts at design time
- materialize those artifacts during host build
- consume the generated SFCs from a pure Deno frontend pipeline

The sample is split into:

- `Todo.Library`: RazorVue component library authored with Razor and C#
- `Todo.Host`: build host that turns on `JazorEmit` and writes generated artifacts to `wwwroot/jazor/`
- `todo-consumer`: a minimal Deno + Vue + Vuetify consumer that compiles generated `.vue` files and bundles them without Vite

`Todo.Library` follows the explicit authoring contract. Component marker types are brought in with:

```csharp
using static ECMAScript.Vue3;
```

The sample does not rely on package-level global aliases for `IVueComponent` / `IVueLibraryComponent`.

## Build from this repository

Use the helper script to build the local package inputs, pack `Jazor` and `ECMAScript.Vuetify`, and rebuild the host:

```powershell
.\build-local.ps1
```

Generated RazorVue artifacts are written to:

```text
.\Todo.Host\wwwroot\jazor\
```

You should see:

- `components/todo-app.vue`
- `components/todo-summary-card.vue`
- `jazor-manifest-razorvue.json`
- `__jazor/razorvue-host.mjs`

If you also want the regular JS bundle sidecars from `Jazor.Emit`, build with:

```powershell
.\build-local.ps1 -Bundle
```

## Run the frontend consumer

After `.\build-local.ps1` succeeds:

1. open `todo-consumer/`
2. run the pure Deno pipeline

```powershell
cd .\todo-consumer
npm run test
```

The consumer imports:

- the generated root component from `..\Todo.Host\wwwroot\jazor\components\todo-app.vue`
- host metadata from `..\Todo.Host\wwwroot\jazor\__jazor\razorvue-host.mjs`

and then:

- precompiles each generated `.vue` into `.mjs + .css`
- runs SSR smoke through `vue/server-renderer` + Vuetify
- runs a `Deno.bundle()` smoke over the prepared browser entry
- runs `deno bundle` to emit the browser build under `todo-consumer/dist/`

Useful focused commands:

```powershell
npm run smoke:ssr
npm run smoke:bundle-api
npm run build
```

## What the sample covers

- design-time SFC generation in library mode
- Razor authoring with `.razor + .razor.cs`
- user component composition
- Vuetify library component integration
- `v-if` / `v-for`
- local state, methods, and computed-style lifted bindings
- `Xxx + XxxChanged` model binding surfaces

## Notes

- The `.NET` host does not run the Vue app itself. Its responsibility is artifact generation and materialization.
- The Deno consumer is intentionally small and explicit so the generated SFCs are consumed through a production-style Deno pipeline instead of a Vite-specific loader contract.
- `Todo.Library` currently sets `UseRazorSourceGenerator=false`. The current library-mode design-time path still depends on generated `*.razor.g.cs` being present in compilation.
- The generated SFCs do not emit `<style src="vuetify/styles">` blocks. Style and plugin requirements stay in `__jazor/razorvue-host.mjs`, and the Deno consumer imports `vuetify/styles` explicitly.
- `todo-consumer/scripts/lib/pipeline.ts` owns the Deno-side SFC compilation contract. It compiles RazorVue-generated `.vue` files into local `.mjs` modules before bundling. Deno is not expected to consume `.vue` files directly.
- `deno bundle` is the formal browser build entry for this sample. `Deno.bundle()` is kept as an additional API-level smoke because its option surface is still unstable.
- `build-local.ps1` is fail-fast. If any framework build, pack, publish, or host rebuild step fails, the script stops instead of silently continuing with stale outputs.
