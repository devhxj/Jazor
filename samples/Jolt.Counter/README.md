# Jolt counter

The sample uses two `.jazor` components. The button renders a default slot and sends the next count to its parent. The parent directly imports Reka UI's published `CollapsibleRoot` component and passes its reactive state as the `Open` prop.

Start the dev server from the repository root after installing the CLI. [Jolt CLI setup](../../docs/03-guides/jolt-authoring.md#安装并运行)

```sh
jolt dev samples/Jolt.Counter --port 15658
```

To run with the CLI built from source, use `dotnet run --project src/Jolt.Cli -- dev samples/Jolt.Counter`. Open the printed URL or run the browser check from a second terminal. Pass the installed Chrome or Chromium executable. The final argument makes the check edit and restore the source file to verify hot updates.

```sh
dotnet run --file samples/Jolt.Counter/verify-browser.cs -- chromium http://127.0.0.1:15658/ samples/Jolt.Counter/CounterButton.jazor
```

Build the production assets in `samples/Jolt.Counter/dist`.

```sh
jolt bundle samples/Jolt.Counter
```

Serve `dist` with a static HTTP server and run the same browser check against that URL without the source-file argument. The dev server and production bundle both use Vite in the embedded LibNode host. The generated maps include the original `.jazor` source.

## Scoped styles and generic components

Prepare an isolated copy of the authoring fixture and start it with the candidate CLI.

```sh
dotnet run --file samples/Jolt.Counter/verify-browser.cs -- --prepare .tmp/jolt-feature-web
jolt dev .tmp/jolt-feature-web --port 15658
dotnet run --file samples/Jolt.Counter/verify-browser.cs -- chromium http://127.0.0.1:15658/ .tmp/jolt-feature-web/CounterButton.jazor --features
jolt bundle .tmp/jolt-feature-web
```

Run the browser check against the served production directory with `--features`. It checks scoped CSS and HMR, Reka UI bindings, and a local table whose rows and status values come from two inferred type parameters. Reversing the table must reuse the same DOM rows and evaluate each row key once. The editor fixture verifies model writes, focus through an exposed method, and the lifetime of the component reference when the editor is removed and mounted again. The form fixture checks setter normalization and veto, post-assignment callbacks, radio buttons, selects and nullable numbers. [Table consumer](.fixtures/TableDemo.jazor) · [Table declaration](.fixtures/TypedTable.jazor) · [Native controls](.fixtures/NativeForm.jazor)

The same fixture imports `camelCase` from `es-toolkit/string` and calls VueUse's `useCounter` in `[Setup]`. The browser check exercises the returned callbacks and reads the published readonly ref in two independent instances. It also verifies that Vue disposes the watcher created during setup when the component unmounts and that remounting starts with a fresh counter. [npm counter](.fixtures/NpmCounter.jazor)
