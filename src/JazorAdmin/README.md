# JazorAdmin

JazorAdmin is the concrete admin dogfood application for the current RazorVue render-function `.mjs` pipeline.

This sample has three jobs:

- exercise the `Jazor.Admin` admin shell as an application foundation
- provide an integration smoke path for Jazor package consumption, Razor SG component emission, generated `.mjs` artifacts, and manifest output
- prove assembly-level `[VueInject]` replacement through an independent companion application without hiding the native TDesign implementation path

`Jazor.Admin` supplies application framing and strongly typed admin models. JazorAdmin owns its TDesign layout, navigation, header and page-container implementation together with its release table, settings form, action feedback, login page, lock screen, and localized 404/500 error pages.

The current slice also exercises controlled sidebar collapse, live sidebar/top layout switching, global theme and language switching, grayscale memorial mode, login/lock navigation, unknown-route handling, internal-error recovery, asynchronous loading, typed page models, and direct render-function artifacts without using `object` as a page-data catch-all. The sample-owned `wwwroot/app.css` supplies the concrete responsive application skin; `Jazor.Admin` exposes the framework contracts while the application owns its TDesign components and presentation policy.

Run from the repository root:

```powershell
dotnet run --no-launch-profile --file .\src\JazorAdmin\verify-smoke.cs -- --configuration Release
```

The smoke packs local `Jazor`, `Jazor.Vue`, `ECMAScript.VueRoute`, and `Jazor.Admin` packages, then rebuilds both the native application and `InjectSmoke` with a timestamp-isolated NuGet package cache. `Jazor.Vue` is a private build-time dependency that installs the merged Razor-to-Vue generator. Native artifacts are written under `.tmp/sample-smoke/JazorAdmin/<configuration>/jazor/`; companion artifacts are written under `.tmp/sample-smoke/JazorAdmin/InjectSmoke/<configuration>/jazor/`.

The real-browser lane verifies stylesheet delivery, desktop shell geometry, mobile navigation reflow, nested-route navigation, table overflow containment, localized 404/500 recovery, and the injected page-container runtime contract. The injection assertions cover the implementation marker, runtime prop name, breadcrumb content, named/default slots, and an event callback that updates visible state from `0` to `1`.
