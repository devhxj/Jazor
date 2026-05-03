# jazor.wiki

`jazor.wiki` is the real Wiki/docs-site MVP for this repository.

It currently uses:

- Vue 3 `h()` authoring in C# through `ECMAScript.Vue3`
- typed Vuetify bootstrap through `ECMAScript.Vuetify`
- Jazor static module authoring in C# (`ECMAScriptModule`) for app bootstrap and page shell
- compile-time emit into `wwwroot/jazor` through `JazorEmit`
- ASP.NET Core static hosting with route fallback for real docs URLs

Current product boundary:

- `H(...)` owns the docs shell, navigation, article layout, TOC, and pager
- content is still code-first, but the shell, route contract, page bodies, and leaf render helpers are split across partial module files
- in-app docs navigation now upgrades to `history.pushState` / `popstate` shell routing while preserving real URL fallback
- right-rail TOC now upgrades same-page hash navigation into active-state, scroll-synced section routing while preserving shareable `#anchor` URLs
- each section now exposes a direct permalink action that updates the hash, copies the full section URL through the browser clipboard, and falls back to a visible "Link ready" state when clipboard write is unavailable
- left-rail page discovery includes client-side filtering over routes, group labels, titles, statuses, and summaries
- this is a real docs-site MVP, not a CMS and not an editable wiki backend

## Project Layout

- `Wiki.csproj`: single web host project for the docs site.
- `WikiHomeModule.cs`, `WikiHomeModule.RouteContract.cs`, `WikiHomeModule.Elements.cs`, and the per-page files `WikiHomeModule.Overview.cs`, `WikiHomeModule.GettingStarted.cs`, `WikiHomeModule.ContentModel.cs`, `WikiHomeModule.HFunctionAuthoring.cs`, `WikiHomeModule.Deployment.cs`: the route shell, centralized route contract, page bodies, and reusable leaf render helpers for the named-export Vue component.
- `AppModule.cs`: Jazor C# module source for runtime bootstrap.
- `Program.cs`: ASP.NET Core host with static files, `/health`, and fallback to `index.html`.
- `build-local.ps1`: local build entry that verifies emitted Wiki artifacts exist after build.
- `serve.ps1`: local preview entry that can build first and refuses to run when emitted modules are missing.
- `verify-smoke.ps1`: focused smoke verification for build output, `/health`, all registered docs routes, and unknown-route fallback.
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

The script builds the host against repository project references, runs local `Jazor.Emit` through MSBuild, and verifies that the emitted entry modules exist.

## Runtime Preview

One-command preview:

```powershell
.\src\Wiki\serve.ps1 -Build
```

Then open:

- `http://localhost:4173/`
- `http://localhost:4173/guides/getting-started`
- `http://localhost:4173/guides/content-model`
- `http://localhost:4173/engineering/h-function-authoring`
- `http://localhost:4173/operations/deployment`

If you want to call the local build script first:

```powershell
.\src\Wiki\serve.ps1 -BuildLocal
```

The page mounts the emitted Vue component via Vue runtime and serves the docs shell from real route paths.

Dry-run only:

```powershell
.\src\Wiki\serve.ps1 -Build -DryRun
```

This verifies the emitted modules and prints the preview URL without starting the host.

## Smoke Verification

From repository root:

```powershell
.\src\Wiki\verify-smoke.ps1 -Build
```

The smoke check verifies:

- `dotnet build` succeeds when `-Build` is used
- `wwwroot/jazor/main.mjs`, `components/wiki-home.mjs`, and `jazor-manifest.json` exist
- `/health` returns HTTP 200 with `ok`
- every registered docs route returns HTTP 200 and still contains `#app` plus `./jazor/main.mjs`
- an unknown docs route still returns the frontend shell through fallback
- emitted `wiki-home.mjs` still contains the client-side navigation contract (`replaceState`, `pushState`, `popstate`, and click interception)
- emitted `wiki-home.mjs` still contains the section-routing contract (`hashchange`, active TOC markers, and hash-driven section scrolling)
- emitted `wiki-home.mjs` still contains the section permalink contract (`window.navigator.clipboard`, `clipboard.writeText`, permalink button labels, and copied/fallback-state styling markers)
- emitted `wiki-home.mjs` still contains the page-discovery filter contract (`Search docs pages`, filter-empty state, and left-rail search styling markers)
- emitted `wiki-home.mjs` still contains the registered docs-route markers

## Runtime Dependency Notes

`wwwroot/index.html` configures browser-side module resolution for:

- `vue`
- `npm:vue@3`
- `vuetify`
- `npm:vuetify`
- `vuetify/components`
- `vuetify/directives`

The emitted Jazor modules keep the original ECMAScript package specifiers, so the browser import map must include those exact `npm:` keys. At the same time, the Vuetify CDN ESM entry still imports bare `vue`, and the component/directive entry points live under `.js` paths rather than the non-existent `.mjs` paths.

`AppModule.cs` uses typed `ECMAScript.Vuetify` proxies and bootstraps Vuetify via:

- `Vuetify.CreateVuetify(VuetifyOptions)`
- `VuetifyComponentRegistry`
- `VuetifyDirectiveRegistry`

## Positioning

This project now targets a **real docs-site MVP**.
The site shell is authored with typed `H()` calls instead of `RenderTreeBuilder`.
Current routes are real product-facing entry points, not sample-only demo pages.
If the docs corpus grows, the next step is a more structured content source, while keeping `H()` as the production shell authoring standard.
