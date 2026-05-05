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
- route changes now keep session-local scroll memory, so browser back or forward returns to the previous reading position when the URL has no explicit section hash
- the shell now keeps page title, description, canonical URL, and social metadata in sync with the active route
- page heroes now expose catalog-backed metadata cards, tag-driven search entry points, source links, and issue-report actions
- each section now exposes a direct permalink action that updates the hash, copies the full section URL through the browser clipboard, and falls back to a visible "Link ready" state when clipboard write is unavailable
- each page now exposes a direct page permalink action with the same clipboard / fallback behavior as section links
- each code block now exposes a direct copy action with visible copied / unavailable feedback instead of forcing manual selection
- docs search now supports keyboard focus shortcuts: `/` and `Ctrl/Cmd+K` jump to search, `Escape` clears or exits the field
- left-rail page discovery includes client-side filtering over routes, group labels, titles, statuses, and summaries
- `/search` is now a real route with `?q=` query support, result highlighting, section-level matches, and shareable search URLs
- theme preference and page feedback are now persisted in browser `localStorage`, and mobile nav / TOC drawers are first-class shell behaviors
- unknown docs routes now recover into a real not-found document that shows the requested path plus suggested registered pages instead of a dead-end shell
- this is a real docs-site MVP, not a CMS and not an editable wiki backend

## Project Layout

- `Wiki.csproj`: single web host project for the docs site.
- `WikiHomeModule.cs`, `WikiHomeModule.RouteContract.cs`, `WikiHomeModule.Elements.cs`, `WikiCatalogGuard.cs`, and the per-page files `WikiHomeModule.Overview.cs`, `WikiHomeModule.Search.cs`, `WikiHomeModule.GettingStarted.cs`, `WikiHomeModule.ProjectLines.cs`, `WikiHomeModule.ContentModel.cs`, `WikiHomeModule.NavigationDiscovery.cs`, `WikiHomeModule.InformationArchitecture.cs`, `WikiHomeModule.TopicIndex.cs`, `WikiHomeModule.Glossary.cs`, `WikiHomeModule.Faq.cs`, `WikiHomeModule.Troubleshooting.cs`, `WikiHomeModule.HFunctionAuthoring.cs`, `WikiHomeModule.CompilerOverview.cs`, `WikiHomeModule.CompilerBoundary.cs`, `WikiHomeModule.RouteCatalogContract.cs`, `WikiHomeModule.HostSemanticSeams.cs`, `WikiHomeModule.ImportEmitContract.cs`, `WikiHomeModule.RuntimeCatalog.cs`, `WikiHomeModule.JoltHost.cs`, `WikiHomeModule.RazorVueLibraryMode.cs`, `WikiHomeModule.ContentGovernance.cs`, `WikiHomeModule.Deployment.cs`, `WikiHomeModule.TestingVerification.cs`: the route shell, centralized route contract, startup guard, page bodies, and reusable leaf render helpers for the named-export Vue component.
- `AppModule.cs`: Jazor C# module source for runtime bootstrap.
- `Program.cs`: ASP.NET Core host with `/health`, route fallback, and an explicit `/jazor` mount for the local emit directory when present.
- `build-local.ps1`: local build entry that verifies emitted Wiki artifacts exist after build.
- `serve.ps1`: preview entry that can run either the local development host or a production-shape published host, and refuses to run when the required artifacts are missing.
- `verify-smoke.ps1`: focused smoke verification for build output, `/health`, all registered docs routes, and unknown-route fallback.
- `verify-browser.ps1` + `verify-browser.mjs`: real browser verification for runtime mount, SPA routing, search/not-found recovery, persisted shell state, copy affordances, hash routing, and mobile drawers.
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

Production-shape preview:

```powershell
.\src\Wiki\serve.ps1 -Publish
```

Then open:

- `http://localhost:4173/`
- `http://localhost:4173/search`
- `http://localhost:4173/search?q=compiler`
- `http://localhost:4173/guides/getting-started`
- `http://localhost:4173/guides/project-lines`
- `http://localhost:4173/guides/content-model`
- `http://localhost:4173/guides/navigation-discovery`
- `http://localhost:4173/guides/information-architecture`
- `http://localhost:4173/guides/topic-index`
- `http://localhost:4173/guides/glossary`
- `http://localhost:4173/guides/faq`
- `http://localhost:4173/guides/troubleshooting`
- `http://localhost:4173/engineering/h-function-authoring`
- `http://localhost:4173/engineering/compiler-overview`
- `http://localhost:4173/engineering/compiler-support-boundary`
- `http://localhost:4173/engineering/route-catalog-contract`
- `http://localhost:4173/engineering/host-semantic-seams`
- `http://localhost:4173/engineering/import-emit-contract`
- `http://localhost:4173/engineering/runtime-catalog`
- `http://localhost:4173/engineering/jolt-host`
- `http://localhost:4173/engineering/razorvue-library-mode`
- `http://localhost:4173/operations/content-governance`
- `http://localhost:4173/operations/deployment`
- `http://localhost:4173/operations/testing-verification`

If you want to call the local build script first:

```powershell
.\src\Wiki\serve.ps1 -BuildLocal
```

The page mounts the emitted Vue component via Vue runtime and serves the docs shell from real route paths.

`-Publish` defaults to `Release`, publishes into a repo-local `.tmp/wiki-publish-preview/<Configuration>/` directory, and starts the published host from that output so `/jazor/*` is exercised through `wwwroot/jazor` instead of the local emit mount.

Dry-run only:

```powershell
.\src\Wiki\serve.ps1 -Build -DryRun
.\src\Wiki\serve.ps1 -Publish -DryRun
```

This verifies the selected preview artifacts and prints the preview URL without starting the host.

## Smoke Verification

From repository root:

```powershell
.\src\Wiki\verify-smoke.ps1 -Build
```

Production-shape publish verification:

```powershell
.\src\Wiki\verify-smoke.ps1 -Publish
```

`-Publish` defaults to `Release` unless you pass `-Configuration` explicitly.

Repository-root shortcuts:

```powershell
pwsh .\scripts\test-dotnet.ps1 -Project wiki
pwsh .\scripts\test-dotnet.ps1 -Project wiki-publish
```

`pwsh .\scripts\test-dotnet.ps1 -Project wiki-publish` also defaults to `Release` unless you pass `-Configuration` explicitly.

The smoke check verifies:

- `dotnet build` succeeds when `-Build` is used
- `jazor/main.mjs`, `components/wiki-home.mjs`, and `jazor-manifest.json` exist
- `/health` returns HTTP 200 with `ok`
- every registered docs route returns HTTP 200 and still contains `#app` plus `/jazor/main.mjs`
- every registered docs route still carries the CLR runtime import-map prefix `"System/": "/jazor/System/"`
- an unknown docs route still returns the frontend shell through fallback
- browser-served assets such as `/jazor/main.mjs`, `/jazor/System/StringModule.js`, `/site.css`, `/favicon.svg`, and `/vendor/vue@3.5.16.mjs` resolve successfully
- `wwwroot/index.html` references the vendored Vue module and contains no external CDN URLs
- emitted `wiki-home.mjs` still contains the client-side navigation contract (`replaceState`, `pushState`, `popstate`, click interception, and session-local scroll restoration)
- emitted `wiki-home.mjs` still contains the section-routing contract (`hashchange`, scroll listeners, active TOC markers, and section-driven reading-state sync)
- emitted `wiki-home.mjs` still contains the section permalink contract (`window.navigator.clipboard`, `clipboard.writeText`, permalink button labels, and copied/fallback-state styling markers)
- emitted `wiki-home.mjs` still contains the code-block copy contract (`Copy code`, copied/unavailable state labels, and code-copy button styling markers)
- emitted `wiki-home.mjs` still contains the page-discovery filter contract (`Search docs pages`, keyboard focus shortcuts, filter-empty state, and left-rail search styling markers)
- emitted `wiki-home.mjs` still contains the product-shell contract for theme toggle, page metadata cards, source/report actions, page feedback, mobile drawers, and search-route results
- `wwwroot/index.html` still carries the base description, canonical, and social metadata placeholders that the shell updates at runtime
- `wwwroot/site.css` still carries the shell selectors for skip link, breadcrumbs, metadata cards, feedback actions, search results, drawers, and light-theme overrides
- emitted `wiki-home.mjs` still contains the registered docs-route markers, page-title labels, and section-anchor contract markers
- host startup now rejects route-catalog drift such as mismatched page-array lengths, duplicate page paths, duplicate section ids, empty metadata entries, or related links that point at unknown pages
- `-Publish` verifies production-shape output under `wwwroot/jazor`, confirms that no shadow root `jazor/` directory survives publish, and proves the published host still serves `/jazor/main.mjs` plus `System/*` runtime assets

## Browser Verification

From repository root:

```powershell
.\src\Wiki\verify-browser.ps1 -BuildLocal
```

Production-shape browser verification:

```powershell
.\src\Wiki\verify-browser.ps1 -Publish
```

Repository-root shortcuts:

```powershell
pwsh .\scripts\test-dotnet.ps1 -Project wiki-browser
pwsh .\scripts\test-dotnet.ps1 -Project wiki-browser-publish
```

`verify-browser.ps1` starts the Wiki host, launches headless Microsoft Edge with CDP enabled, and runs the project-owned `verify-browser.mjs` assertions through Node.js. The local path exercises `src/Wiki/jazor`, while `-Publish` exercises published assets from `wwwroot/jazor`.

The browser check verifies:

- the home route mounts the real shell instead of an empty app root
- SPA navigation from home reaches `Getting Started`, focuses `#wiki-main-content`, and announces the route change through the live region
- search query hydration, highlighted results, and clear-button query reset work on `/search?q=compiler`
- unknown routes render the not-found document and recover through suggested route cards
- theme preference and page feedback persist through `localStorage` and rehydrate after a full reload
- page permalink, section permalink, and code-copy actions expose copied/fallback feedback instead of silent failure
- back navigation restores scroll position on hash-free reading routes
- direct `#anchor` loads activate the expected document section and TOC state
- mobile navigation and TOC drawers open, close, and update the hash as expected
- the browser session finishes without actionable network failures, console errors, or runtime exceptions

Requirements:

- Node.js available on `PATH`
- Microsoft Edge installed at `C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe`

## Runtime Dependency Notes

`wwwroot/index.html` keeps the browser import map deliberately explicit:

- `System/` — CLR runtime support modules emitted by `Jazor.CLR`
- `vue` — Vue 3.5.16 ESM browser production build, vendored locally at `/vendor/vue@3.5.16.mjs`
- `npm:vue@3` — alias to the same vendored Vue build

The Wiki shell has **no runtime external CDN dependencies**. Vue 3 is served from `wwwroot/vendor/` alongside site-local CSS and emitted Jazor modules, making the site fully deployable offline.

The browser entry and static assets are rooted with absolute paths (`/jazor/main.mjs`, `/site.css`, `/favicon.svg`) so direct refreshes on nested docs URLs keep resolving against the site root instead of the current route segment.

In development, the host serves `/jazor/*` from the project-local `src/Wiki/jazor/` emit directory. During publish, the same artifacts are copied into `wwwroot/jazor/` so production still uses standard web-root static hosting.

When the generated shell uses CLR-backed helper members such as `string.Contains(..., StringComparison.OrdinalIgnoreCase)`, `Jazor.CLR` support modules are emitted locally under `src/Wiki/jazor/System/...`, published into `wwwroot/jazor/System/...`, and resolved in the browser through the import-map prefix `"System/": "/jazor/System/"`.

## Deployment Contract

See [DEPLOY.md](DEPLOY.md) for the full deployment guide, directory structure contract, key invariants, and rollback procedure.

Summary of invariants enforced by `verify-smoke.ps1 -Publish`:

- Published output serves `/jazor/*` only from `wwwroot/jazor/`, with no shadow root directory
- `main.mjs`, `components/wiki-home.mjs`, and `jazor-manifest.json` exist under `wwwroot/jazor/`
- `/vendor/vue@3.5.16.mjs` is servable (Vue 3 vendored locally, no CDN dependency)
- All 23 registered docs routes return HTTP 200 with the SPA shell
- `index.html` contains no external CDN URLs

## Positioning

This project now targets a **real docs-site MVP**.
The site shell is authored with typed `H()` calls instead of `RenderTreeBuilder`.
Current routes are real product-facing entry points, not sample-only demo pages.
If the docs corpus grows, the next step is a more structured content source, while keeping `H()` as the production shell authoring standard.
