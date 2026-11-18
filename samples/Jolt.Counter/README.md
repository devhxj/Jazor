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
