# jazor.wiki (Library Mode)

`jazor.wiki` is a real Jazor sample focused on **library-mode authoring**.

It demonstrates:

- Vue `h()` component authoring in C# through `ECMAScript.Vue`
- Jazor static module authoring in C# (`ECMAScriptModule`) for page bootstrap logic
- compile-time emit into `wwwroot/jazor` through `JazorEmit`
- a browser playground with live C# -> JS preview for fast authoring feedback

## Project Layout

- `Wiki.csproj`: single web host project for this sample.
- `WikiHomeModule.cs`: named-export Vue component built with `Vue.DefineComponent(...)` and `H(...)`.
- `AppModule.cs`: Jazor C# module source for runtime bootstrap.
- `wwwroot/`: static entry (`index.html`, `site.css`) and emitted modules.

## Build from This Repository

From repository root:

```powershell
dotnet build .\src\Wiki\Wiki.csproj
```

Generated artifacts:

- `.\src\Wiki\wwwroot\jazor\jazor-manifest.json`
- `.\src\Wiki\wwwroot\jazor\components\wiki-home.mjs`
- `.\src\Wiki\wwwroot\jazor\main.mjs`

## Local Build Script

```powershell
.\src\Wiki\build-local.ps1
```

The script builds the host against repository project references and runs local `Jazor.Emit` through MSBuild.

## Runtime Preview

One-command preview:

```powershell
.\src\Wiki\serve.ps1 -Build
```

Then open:

- `http://localhost:4173/index.html`

If you want to call the local build script first:

```powershell
.\src\Wiki\serve.ps1 -BuildLocal
```

The page mounts the emitted Vue component via Vue runtime and provides a live C# -> JS preview panel.

## Runtime Dependency Notes

`wwwroot/index.html` configures browser-side module resolution for:

- `vue`
- `npm:vue@3`
- `vuetify`
- `npm:vuetify`
- `vuetify/components`
- `vuetify/directives`

The emitted Jazor modules keep the original ECMAScript package specifiers, so the browser import map must include those exact `npm:` keys. At the same time, the Vuetify CDN ESM entry still imports bare `vue`, and the component/directive entry points live under `.js` paths rather than the non-existent `.mjs` paths.

`AppModule.cs` uses typed `ECMAScript.Vue.Vuetify` proxies and bootstraps Vuetify via:

- `Vuetify.CreateVuetify(VuetifyOptions)`
- `VuetifyComponentRegistry`
- `VuetifyDirectiveRegistry`

## Positioning

This sample is intentionally **library-mode only**.
The current page is authored with typed `H()` calls instead of `RenderTreeBuilder`.
The live preview state now lives inside the emitted Vue component module instead of `AppModule` DOM event wiring.
