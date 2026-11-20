# Jolt mixed Vue SSR

This sample runs a Vue workspace with Vue Router, Pinia, Vue SFCs and typed Jolt components. The editor switches between a local Vue input and a Jolt preview using the same named model. A Teleport places its notice in the document shell.

From the repository root, use the repository's .NET NodeApi restore and Jolt apphost.

```powershell
dotnet run --file scripts/csharp/restore-node-project.cs -- samples/Jolt.Mixed.Ssr/frontend
dotnet build src/Jolt.Cli/Jolt.Cli.csproj
src/Jolt.Cli/bin/Debug/net11.0/Jolt.Cli.exe bundle samples/Jolt.Mixed.Ssr/frontend --ssr entry-server.ts
dotnet run --project samples/Jolt.Mixed.Ssr --urls http://127.0.0.1:5168
```

Open [`/editor?name=Alice`](http://127.0.0.1:5168/editor?name=Alice) directly or use the Editor link from [`/?name=Alice`](http://127.0.0.1:5168/?name=Alice). The hydrated app retains the initial request name and prefetch count through client-side navigation. The editor disposal count increases when leaving the editor and the teleported notice leaves the overlay. [`/?fail=1`](http://127.0.0.1:5168/?fail=1) returns an HTTP error and logs the failed prefetch.

`DraftDetails.jazor` adds the [`/drafts/42`](http://127.0.0.1:5168/drafts/42) route via `@page` and wraps its content in `DraftLayout` via `@layout`. The page adds one to the typed route parameter so both direct requests and client navigation display `43`.

`Program.cs` supplies the request name as a Vue provider. The server entry creates a fresh app, memory router and Pinia instance for each request. It awaits the route and server prefetch, snapshots the resolved Pinia state, then disposes the server-side Pinia instance. The browser creates the same app with web history, applies the snapshot and waits for its routes before mounting onto the existing HTML.

After bundling, `dotnet publish samples/Jolt.Mixed.Ssr -o .tmp/jolt-mixed-ssr-publish` copies the client and server artifacts beside the host. Launch the published apphost to serve the application.

## SSR benchmark

```powershell
dotnet build src/Jolt.Cli/Jolt.Cli.csproj -c Release
dotnet run --file scripts/csharp/verify-jolt-ssr.cs -- src/Jolt.Cli/bin/Release/net11.0/Jolt.Cli.exe 'C:/Program Files/Google/Chrome/Application/chrome.exe' --benchmark
```

The benchmark builds a temporary copy with Vite's production settings and publishes the ASP.NET Core host in Release. It measures `/editor?name=Alice` over loopback with a reused HTTP client and then with successive full navigations in one headless Chrome profile. Each pass discards five warmups and prints twenty observations in milliseconds.

`SSR_HTTP` measures elapsed time from issuing the request to receiving the response headers, followed by elapsed time to finishing the response body. Both values include loopback HTTP and server processing. `SSR_BROWSER` measures from immediately before to immediately after the synchronous Vue hydration mount. It then dispatches the first click on the original hydrated button and measures until a mutation observer sees the updated counter. The browser measurements exclude navigation, asset fetch, router readiness before mount, and painting after the update. The benchmark adds hydration marks to the temporary copy for this run.

Measured on 2026-11-20 with an Intel Core i5-12500H, Windows build 26300, .NET SDK 11.0.100, Vite 8.2.0, and headless Chrome 157.0.8103.79. The Release CLI and host were built from the in-progress Jolt worktree at `3465c236e793867bc7c33117f864be4fcfe434e1` (tag `v2.2.0` plus local changes). The medians and nearest-rank p95 values summarize one run of this SSR sample on loopback.

| Measurement | p50 (ms) | p95 (ms) |
| --- | ---: | ---: |
| Request to response headers | 1.443 | 3.116 |
| Request to complete response | 1.479 | 3.133 |
| Vue hydration mount | 1.800 | 2.000 |
| First click to counter DOM update | 0.300 | 0.400 |
