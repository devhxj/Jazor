# JazorAdmin

JazorAdmin is the reference administration product for the current RazorVue render-function `.mjs` pipeline.

This sample has three jobs:

- exercise the `Jazor.Admin` admin shell as an application foundation
- provide an integration smoke path for Jazor package consumption, Razor SG component emission, generated `.mjs` artifacts, and manifest output
- prove assembly-level `[VueInject]` replacement through an independent companion application without hiding the native TDesign implementation path

`Jazor.Admin` supplies application framing and strongly typed admin models. JazorAdmin owns an independent IconBar for primary work areas, a scoped TDesign secondary menu, its header and page-container implementation, login/lock flow, and localized 404/500 error pages. Both navigation tiers are derived from the same route catalog, so their targets, labels, and selected state cannot drift. The only application CSS namespace is `ja-*`.

The application runs in one ASP.NET Core host: RazorVue frontend, Web API, Identity, independent OpenIddict SSO center, organization structure and memberships, resource-operation grants, and platform account administration. The SSO center has applications, scopes, authorizations, and tokens. Applications support interactive, machine, and API profiles; public or confidential clients; secret rotation; consent and PKCE; endpoint, grant, response, redirect URI, and scope permissions. Scopes carry display metadata and API resources; authorization and token records can be inspected and revoked.

The original Jinsha sunbird-inspired `Jazor` mark is used by the login, shell, consent page, and browser tab. It is supplied as a scalable SVG plus a 16/32/48/64px ICO fallback. Regenerate the ICO from the compact mark with `dotnet run --file scripts/csharp/generate-jazoradmin-brand-assets.cs`; `--check` verifies that the checked-in fallback is current. The login scene uses the application-owned cyan-green ink/mineral landscape at `wwwroot/brand/login-art.webp` behind an Aero-style glass form. Neither surface uses an external font, image, or CDN resource.

## First login

Run the project with its development profile:

```powershell
dotnet run --project .\samples\JazorAdmin\JazorAdmin.csproj
```

Open the `/login` URL for the address printed at startup and sign in with `admin@jazor.local` / `JazorAdmin123!`. The form presents a four-character image captcha; it is server-issued, one-time, and valid for three minutes. The default development profile listens on `https://localhost:49732` and `http://localhost:49733`; those addresses are development configuration only. The bootstrap configuration creates this platform administrator only when no platform administrator exists. It never resets an existing user's password; use **Accounts** after sign-in to create users or reset passwords.

For a custom local account, set the bootstrap values in user secrets before the first launch:

```powershell
dotnet user-secrets set --project .\samples\JazorAdmin\JazorAdmin.csproj "JazorAdmin:Bootstrap:Email" "admin@example.test"
dotnet user-secrets set --project .\samples\JazorAdmin\JazorAdmin.csproj "JazorAdmin:Bootstrap:Password" "ChangeThisAdmin123!"
dotnet user-secrets set --project .\samples\JazorAdmin\JazorAdmin.csproj "JazorAdmin:Bootstrap:DisplayName" "Platform Administrator"
```

For every non-test deployment, configure `JazorAdmin:Bootstrap:Email`, `JazorAdmin:Bootstrap:Password`, `JazorAdmin:OpenIddict:RedirectUris`, and `JazorAdmin:OpenIddict:PostLogoutRedirectUris` through the deployment's secret store before the first start. Startup stops with an actionable error if the first administrator or callback URLs are missing. The bootstrap values are consumed only to create the first account or promote the configured existing account; later starts do not reset its password. OpenIddict requires exact callback URLs, so these must use the actual public application origin.

The configuration center stores typed `text`, `boolean`, `number`, and `json` values. The task center uses Quartz.NET for Cron interpretation, trigger execution, misfire behavior, and single-task concurrency. It only schedules catalogued application tasks: administrators can change a task's Cron expression, enable or pause it, run it manually, and inspect recent execution history, but cannot submit arbitrary executable code. The first managed task prunes expired OpenIddict tokens and detached authorizations. The frontend is RazorVue, not Blazor; application source contains no handwritten JavaScript, CSS, or static `index.html`. `Jazor.Admin` registers its native shell rules through `ECMAScript.Style`; JazorAdmin registers its TDesign and application rules through the same runtime rather than loading a sample-owned static stylesheet.

Run from the repository root:

```powershell
dotnet run --no-launch-profile --file .\samples\JazorAdmin\verify-smoke.cs -- --configuration Release
```

The smoke packs local `Jazor`, `Jazor.Vue`, `ECMAScript.Style`, `ECMAScript.VueRoute`, and `Jazor.Admin` packages, then rebuilds both the native application and `InjectSmoke` with a timestamp-isolated NuGet package cache. Through the `0.8` milestone, JazorAdmin development, integration tests, and smoke consume these current-source local packages; public NuGet consumption becomes a formal acceptance path from `0.9`. `Jazor.Vue` is a private build-time dependency that installs the merged Razor-to-Vue generator. Native artifacts are written under `.tmp/sample-smoke/JazorAdmin/<configuration>/jazor/`; companion artifacts are written under `.tmp/sample-smoke/JazorAdmin/InjectSmoke/<configuration>/jazor/`.

The real-browser lane verifies stylesheet delivery, the desktop column IconBar and 64px collapsed shell, the mobile row IconBar and stacked secondary menu, navigation order and viewport containment, nested-route navigation, table overflow containment, localized 404/500 recovery, and the injected page-container runtime contract. It also creates machine and API applications, verifies the one-time machine secret, updates a scope, revokes an authorization and token, creates a typed setting, manually runs a Quartz task, reads its execution history, and checks the application editor on a mobile viewport. The injection assertions cover the implementation marker, runtime prop name, breadcrumb content, named/default slots, and an event callback that updates visible state from `0` to `1`.
