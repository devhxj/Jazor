# JazorAdmin

JazorAdmin is the concrete admin dogfood application for the current RazorVue render-function `.mjs` pipeline.

This sample has three jobs:

- exercise the `Jazor.Admin` admin shell as an application foundation
- provide an integration smoke path for Jazor package consumption, Razor SG component emission, generated `.mjs` artifacts, and manifest output
- prove assembly-level `[VueInject]` replacement through an independent companion application without hiding the native TDesign implementation path

`Jazor.Admin` supplies application framing and strongly typed admin models. JazorAdmin owns its TDesign-inspired icon rail, scoped secondary navigation, header, page-container implementation, login/lock flow, and localized 404/500 error pages.

The current slice validates a single ASP.NET Core host for the RazorVue frontend, Web API, Identity, OpenIddict SSO, organization structure and memberships, resource-operation grants, platform account administration, and OpenIddict client/scope configuration. The frontend is RazorVue, not Blazor; application source contains no handwritten JavaScript, CSS, or static `index.html`. `Jazor.Admin` registers its native shell rules through `ECMAScript.Style`; JazorAdmin registers its TDesign and application rules through the same runtime rather than loading a sample-owned static stylesheet.

Run from the repository root:

```powershell
dotnet run --no-launch-profile --file .\src\JazorAdmin\verify-smoke.cs -- --configuration Release
```

The smoke packs local `Jazor`, `Jazor.Vue`, `ECMAScript.Style`, `ECMAScript.VueRoute`, and `Jazor.Admin` packages, then rebuilds both the native application and `InjectSmoke` with a timestamp-isolated NuGet package cache. Through the `0.8` milestone, JazorAdmin development, integration tests, and smoke consume these current-source local packages; public NuGet consumption becomes a formal acceptance path from `0.9`. `Jazor.Vue` is a private build-time dependency that installs the merged Razor-to-Vue generator. Native artifacts are written under `.tmp/sample-smoke/JazorAdmin/<configuration>/jazor/`; companion artifacts are written under `.tmp/sample-smoke/JazorAdmin/InjectSmoke/<configuration>/jazor/`.

The real-browser lane verifies stylesheet delivery, desktop shell geometry, mobile navigation reflow, nested-route navigation, table overflow containment, localized 404/500 recovery, and the injected page-container runtime contract. The injection assertions cover the implementation marker, runtime prop name, breadcrumb content, named/default slots, and an event callback that updates visible state from `0` to `1`.
