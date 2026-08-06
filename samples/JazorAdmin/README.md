# JazorAdmin

JazorAdmin is the reference administration product for the current RazorVue render-function `.mjs` pipeline.

This sample has three jobs:

- exercise the `Jazor.Admin` admin shell as an application foundation
- provide an integration smoke path for Jazor package consumption, Razor SG component emission, generated `.mjs` artifacts, and manifest output
- prove assembly-level `[VueInject]` replacement through an independent companion application without hiding the native TDesign implementation path

`Jazor.Admin` supplies application framing and strongly typed admin models. JazorAdmin owns an independent IconBar for primary work areas, a scoped TDesign secondary menu, its header and page-container implementation, login/lock flow, and localized 404/500 error pages. Both navigation tiers are derived from the same route catalog, so their targets, labels, and selected state cannot drift. The only application CSS namespace is `ja-*`.

The application runs in one ASP.NET Core host: RazorVue frontend, Web API, Identity, independent OpenIddict SSO center, organization structure and memberships, resource-operation grants, and platform account administration. The SSO center has applications, scopes, authorizations, and tokens. Applications support interactive, machine, and API profiles; public or confidential clients; secret rotation; consent and PKCE; endpoint, grant, response, redirect URI, and scope permissions. Scopes carry display metadata and API resources; authorization and token records can be inspected and revoked.

The configuration center stores typed `text`, `boolean`, `number`, and `json` values. The task center uses Quartz.NET for Cron interpretation, trigger execution, misfire behavior, and single-task concurrency. It only schedules catalogued application tasks: administrators can change a task's Cron expression, enable or pause it, run it manually, and inspect recent execution history, but cannot submit arbitrary executable code. The first managed task prunes expired OpenIddict tokens and detached authorizations. The frontend is RazorVue, not Blazor; application source contains no handwritten JavaScript, CSS, or static `index.html`. `Jazor.Admin` registers its native shell rules through `ECMAScript.Style`; JazorAdmin registers its TDesign and application rules through the same runtime rather than loading a sample-owned static stylesheet.

Run from the repository root:

```powershell
dotnet run --no-launch-profile --file .\samples\JazorAdmin\verify-smoke.cs -- --configuration Release
```

The smoke packs local `Jazor`, `Jazor.Vue`, `ECMAScript.Style`, `ECMAScript.VueRoute`, and `Jazor.Admin` packages, then rebuilds both the native application and `InjectSmoke` with a timestamp-isolated NuGet package cache. Through the `0.8` milestone, JazorAdmin development, integration tests, and smoke consume these current-source local packages; public NuGet consumption becomes a formal acceptance path from `0.9`. `Jazor.Vue` is a private build-time dependency that installs the merged Razor-to-Vue generator. Native artifacts are written under `.tmp/sample-smoke/JazorAdmin/<configuration>/jazor/`; companion artifacts are written under `.tmp/sample-smoke/JazorAdmin/InjectSmoke/<configuration>/jazor/`.

The real-browser lane verifies stylesheet delivery, the desktop column IconBar and 64px collapsed shell, the mobile row IconBar and stacked secondary menu, navigation order and viewport containment, nested-route navigation, table overflow containment, localized 404/500 recovery, and the injected page-container runtime contract. It also creates machine and API applications, verifies the one-time machine secret, updates a scope, revokes an authorization and token, creates a typed setting, manually runs a Quartz task, reads its execution history, and checks the application editor on a mobile viewport. The injection assertions cover the implementation marker, runtime prop name, breadcrumb content, named/default slots, and an event callback that updates visible state from `0` to `1`.
