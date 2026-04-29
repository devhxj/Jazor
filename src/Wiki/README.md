# jazor.wiki (RazorVue Library Mode)

`jazor.wiki` is a real RazorVue sample focused on **library-mode authoring**.

It demonstrates:

- RazorVue component authoring in C# (`IVueComponent` + `BuildRenderTree`)
- Jazor static module authoring in C# (`ECMAScriptModule`) for page bootstrap logic
- compile-time emit into `wwwroot/jazor` through `JazorEmit`
- a browser playground with live C# -> JS preview for fast authoring feedback

## Project Layout

- `Jazor.Wiki.csproj`: single web host project for this sample.
- `WikiHome.cs`: RazorVue component source.
- `AppModule.cs`: Jazor C# module source for runtime bootstrap.
- `wwwroot/`: static entry (`index.html`, `site.css`) and emitted modules.

## Build from This Repository

From repository root:

```powershell
dotnet build .\src\Jazor.Wiki\Jazor.Wiki.csproj
```

Generated artifacts:

- `.\src\Jazor.Wiki\wwwroot\jazor\jazor-manifest.json`
- `.\src\Jazor.Wiki\wwwroot\jazor\jazor-manifest-razorvue.json`
- `.\src\Jazor.Wiki\wwwroot\jazor\components\wiki-home.mjs`
- `.\src\Jazor.Wiki\wwwroot\jazor\app\main.mjs`

## Local Build Script

```powershell
.\src\Jazor.Wiki\build-local.ps1
```

The script builds the host against repository project references and runs local `Jazor.Emit` through MSBuild.

## Runtime Preview

One-command preview:

```powershell
.\src\Jazor.Wiki\serve.ps1 -Build
```

Then open:

- `http://localhost:4173/index.html`

If you want to call the local build script first:

```powershell
.\src\Jazor.Wiki\serve.ps1 -BuildLocal
```

The page mounts RazorVue output via Vue runtime and provides a live C# -> JS preview panel.

## Runtime Dependency Notes

`wwwroot/index.html` configures browser-side module resolution for:

- `vue`
- `vuetify`
- `vuetify/components`
- `vuetify/directives`

`AppModule.cs` uses typed `ECMAScript.Vue.Vuetify` proxies and bootstraps Vuetify via:

- `Vuetify.CreateVuetify(VuetifyOptions)`
- `VuetifyComponentRegistry`
- `VuetifyDirectiveRegistry`

## Positioning

This sample is intentionally **RazorVue library-mode only**.
Jolt host-mode verification can be developed in a separate project.
