# jazor.wiki

`jazor.wiki` is the real Wiki/docs-site MVP for this repository.

It currently uses:

- Vue 3 `h()` authoring in C# through `ECMAScript.Vue3`
- Jazor static module authoring in C# (`ECMAScriptModule`) for app bootstrap and page shell
- compile-time emit into project-local `jazor/` through `JazorEmit`
- explicit `/jazor` static mounting in development, with publish-time materialization into `wwwroot/jazor`
- ASP.NET Core static hosting with route fallback for real docs URLs

Current product boundary:

- `H(...)` owns the docs shell, navigation, article layout, TOC, and pager
- content is still code-first, but the shell, route contract, page bodies, and leaf render helpers are split across partial module files
- registered pages now flow through one central page catalog for route metadata, hero copy, body dispatch, navigation grouping, TOC wiring, related-page suggestions, and pager continuity
- the ASP.NET Core host now validates that the route catalog stays internally aligned before serving requests, so array drift fails fast instead of leaking into runtime behavior
- in-app docs navigation now upgrades to `history.pushState` / `popstate` shell routing while preserving real URL fallback
- right-rail TOC now upgrades same-page hash navigation into active-state, scroll-synced section routing while preserving shareable `#anchor` URLs
- each section now exposes a direct permalink action that updates the hash, copies the full section URL through the browser clipboard, and falls back to a visible "Link ready" state when clipboard write is unavailable
- left-rail page discovery includes client-side filtering over routes, group labels, titles, statuses, and summaries
- unknown docs routes now recover into a real not-found document that shows the requested path plus suggested registered pages instead of a dead-end shell
- this is a real docs-site MVP, not a CMS and not an editable wiki backend

## Project Layout

- `Wiki.csproj`: single web host project for the docs site.
- `WikiHomeModule.cs`, `WikiHomeModule.RouteContract.cs`, `WikiHomeModule.Elements.cs`, and the per-page files `WikiHomeModule.Overview.cs`, `WikiHomeModule.GettingStarted.cs`, `WikiHomeModule.ContentModel.cs`, `WikiHomeModule.NavigationDiscovery.cs`, `WikiHomeModule.InformationArchitecture.cs`, `WikiHomeModule.HFunctionAuthoring.cs`, `WikiHomeModule.CompilerBoundary.cs`, `WikiHomeModule.RouteCatalogContract.cs`, `WikiHomeModule.HostSemanticSeams.cs`, `WikiHomeModule.ImportEmitContract.cs`, `WikiHomeModule.RuntimeCatalog.cs`, `WikiHomeModule.ContentGovernance.cs`, `WikiHomeModule.Deployment.cs`, `WikiHomeModule.TestingVerification.cs`: the route shell, centralized route contract, page bodies, and reusable leaf render helpers for the named-export Vue component.
- `AppModule.cs`: Jazor C# module source for runtime bootstrap.
- `Program.cs`: ASP.NET Core host with `/health`, route fallback, and an explicit `/jazor` mount for the local emit directory when present.
- `build-local.ps1`: local build entry that verifies emitted Wiki artifacts exist after build.
- `serve.ps1`: local preview entry that can build first and refuses to run when emitted modules are missing.
- `verify-smoke.ps1`: focused smoke verification for build output, `/health`, all registered docs routes, and unknown-route fallback.
- `jazor/`: local emitted Jazor browser artifacts used for development and smoke verification.
- `wwwroot/`: static entry (`index.html`, `site.css`, `favicon.svg`) plus the publish-time destination for `/jazor` assets.

## Build from This Repository

From repository root:

```powershell
dotnet build .\src\Wiki\Wiki.csproj
```

Generated artifacts:

- `.\src\Wiki\jazor\jazor-manifest.json`
- `.\src\Wiki\jazor\components\wiki-home.mjs`
- `.\src\Wiki\jazor\main.mjs`

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
- `http://localhost:4173/guides/navigation-discovery`
- `http://localhost:4173/guides/information-architecture`
- `http://localhost:4173/engineering/h-function-authoring`
- `http://localhost:4173/engineering/compiler-support-boundary`
- `http://localhost:4173/engineering/route-catalog-contract`
- `http://localhost:4173/engineering/host-semantic-seams`
- `http://localhost:4173/engineering/import-emit-contract`
- `http://localhost:4173/engineering/runtime-catalog`
- `http://localhost:4173/operations/content-governance`
- `http://localhost:4173/operations/deployment`
- `http://localhost:4173/operations/testing-verification`

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
- `jazor/main.mjs`, `components/wiki-home.mjs`, and `jazor-manifest.json` exist
- `/health` returns HTTP 200 with `ok`
- every registered docs route returns HTTP 200 and still contains `#app` plus `/jazor/main.mjs`
- every registered docs route still carries the CLR runtime import-map prefix `"System/": "/jazor/System/"`
- an unknown docs route still returns the frontend shell through fallback
- browser-served assets such as `/jazor/main.mjs`, `/jazor/System/StringModule.js`, `/site.css`, and `/favicon.svg` resolve successfully
- emitted `wiki-home.mjs` still contains the client-side navigation contract (`replaceState`, `pushState`, `popstate`, and click interception)
- emitted `wiki-home.mjs` still contains the section-routing contract (`hashchange`, active TOC markers, and hash-driven section scrolling)
- emitted `wiki-home.mjs` still contains the section permalink contract (`window.navigator.clipboard`, `clipboard.writeText`, permalink button labels, and copied/fallback-state styling markers)
- emitted `wiki-home.mjs` still contains the page-discovery filter contract (`Search docs pages`, filter-empty state, and left-rail search styling markers)
- emitted `wiki-home.mjs` still contains the registered docs-route markers, page-title labels, and section-anchor contract markers
- host startup now rejects route-catalog drift such as mismatched page-array lengths, duplicate page paths, duplicate section ids, empty metadata entries, or related links that point at unknown pages

## Runtime Dependency Notes

`wwwroot/index.html` now keeps the browser import map deliberately explicit:

- `System/`
- `vue`
- `npm:vue@3`

The Wiki shell is authored with plain Vue `h()` calls and site-local CSS. That keeps the browser entry compatible with direct ESM loading instead of relying on library-distribution modules that recursively import component CSS as JavaScript modules.

The browser entry and static assets are rooted with absolute paths (`/jazor/main.mjs`, `/site.css`, `/favicon.svg`) so direct refreshes on nested docs URLs keep resolving against the site root instead of the current route segment.

In development, the host serves `/jazor/*` from the project-local `src/Wiki/jazor/` emit directory. During publish, the same artifacts are copied into `wwwroot/jazor/` so production still uses standard web-root static hosting.

When the generated shell uses CLR-backed helper members such as `string.Contains(..., StringComparison.OrdinalIgnoreCase)`, `Jazor.CLR` support modules are emitted locally under `src/Wiki/jazor/System/...`, published into `wwwroot/jazor/System/...`, and resolved in the browser through the import-map prefix `"System/": "/jazor/System/"`.

## Positioning

This project now targets a **real docs-site MVP**.
The site shell is authored with typed `H()` calls instead of `RenderTreeBuilder`.
Current routes are real product-facing entry points, not sample-only demo pages.
If the docs corpus grows, the next step is a more structured content source, while keeping `H()` as the production shell authoring standard.
