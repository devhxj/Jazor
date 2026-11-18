# Jolt counter

The sample uses two `.jazor` components. The button renders a default slot and sends the next count to its parent. The parent directly imports Reka UI's published `CollapsibleRoot` component and passes its reactive state as the `Open` prop.

Start the dev server from the repository root.

```sh
dotnet run --project src/Jolt.Cli -- dev samples/Jolt.Counter
```

Open the printed URL or run the browser check from a second terminal. Pass the installed Chrome or Chromium executable. The final argument makes the check edit and restore the source file to verify hot updates.

```sh
dotnet run --file samples/Jolt.Counter/verify-browser.cs -- chromium http://127.0.0.1:5173/ samples/Jolt.Counter/CounterButton.jazor
```

Build the production assets in `samples/Jolt.Counter/dist`.

```sh
dotnet run --project src/Jolt.Cli -- bundle samples/Jolt.Counter
```

Serve `dist` with a static HTTP server and run the same browser check against that URL without the source-file argument. The dev server and production bundle both use Vite in the embedded LibNode host. The generated maps include the original `.jazor` source.

## Scoped styles and generic components

Prepare an isolated copy of the authoring fixture and start it with the candidate CLI.

```sh
dotnet run --file samples/Jolt.Counter/verify-browser.cs -- --prepare .tmp/jolt-feature-web
jolt dev .tmp/jolt-feature-web
dotnet run --file samples/Jolt.Counter/verify-browser.cs -- chromium http://127.0.0.1:5173/ .tmp/jolt-feature-web/CounterButton.jazor --features
jolt bundle .tmp/jolt-feature-web
```

Run the browser check against the served production directory with `--features`. The check covers matching CSS class names in two components, the parent's selector on the child's root, slotted content and the rendered slots of both generic lists.
