# RazorVue.Authoring

This is the smallest standalone Blazor-first RazorVue sample in the repository. It uses
ordinary Razor authoring with typed TDesign controls for a task-board CRUD flow:

- `TForm<TaskDraft>` and typed `TInput<string>` with `@bind-Value`
- `TPrimaryTable<TaskRow>` with typed columns and `CellEmptyContent` context
- async form/dialog callbacks, writable `[Inject] NavigationManager`, and a named
  `[CascadingParameter]`
- `@page "/"` and `@page "/tasks"` through the existing RazorVue route catalog

## Source build

From the repository root, build against the source projects with isolated outputs:

```text
dotnet run --file samples/RazorVue.Authoring/build-local.cs -- --source-only --configuration Debug --work-root .tmp/authoring-local-build-debug
```

The generated Debug graph is written to `.tmp/authoring-local-build-debug/source-jazor`.
The build record includes elapsed time and the authoring-source internal-symbol count.

## Local package and Release proof

The reproducible package lane packs only `Jazor`, `Jazor.Vue`, `ECMAScript.TDesign`, and
`ECMAScript.Style` with `--skip-push`, builds an isolated package consumer, and materializes
the Release browser closure:

```text
dotnet run --file samples/RazorVue.Authoring/build-local.cs -- --configuration Release --work-root .tmp/authoring-local-build
dotnet run --file samples/RazorVue.Authoring/verify-smoke.cs -- --skip-build --work-root .tmp/authoring-local-build --package-output .tmp/nupkg-sample/RazorVue.Authoring
```

The first command produces local packages under `.tmp/nupkg-sample/RazorVue.Authoring`, a
package-consumer build under `.tmp/authoring-local-build/consumer-build-out`, Debug artifacts
under `.tmp/authoring-local-build/package-jazor`, and Release artifacts under
`.tmp/authoring-local-build/release-jazor`. The second command checks manifests, modules,
source maps, package nuspec IDs, vendor closure, and the absence of `node_modules`.

To include the browser mount in the same gate, omit `--skip-browser` (the default). Edge,
Chrome, or Chromium is discovered automatically; set `RAZORVUE_BROWSER_EXE` or
`RAZORVUE_BROWSER_PATH` to choose a specific executable.

## Authoring boundary

The sample intentionally contains no `BuildRenderTree`, `RenderTreeBuilder`, historical bridge
components, application-side casts, `object` escape hatches, handwritten JavaScript, or
`IJSRuntime`. It also does not use Microsoft/Blazor built-in UI components or recreate
`Router`, `RouteView`, `LayoutView`, or `NavLink`. `Bootstrap` owns only the Vue mount framing;
RazorVue and `Jazor.Emit` own expression lowering, route-catalog generation, module/source-map
materialization, and Release bundling.
