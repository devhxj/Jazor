# RazorVue TODOList

`RazorVue.TodoList` exercises the generated standard JavaScript project through a Deno web service and an ASP.NET Core proxy:

- `Todo.Library` contains a native RazorVue component with Razor bindings, events, loops, and conditional state.
- `TodoStyleSheet` owns every visual rule through `ECMAScript.Style`; there is no authored CSS under `wwwroot`.
- `Todo.Host` emits to its project-root `jazor/` directory. Deno runs Vite to serve browser modules; `UseJazorViteProxy` forwards `/jazor` HTTP and WebSocket requests. The host does not mount or watch that directory.
- SSR executes the project's `ssr` task in production and `ssr:dev` in Development. The latter uses Deno watch so transitive module edits become visible without rewriting the entry.
- `Todo:PathBase` supports a mounted application such as `/docs`.
- `dotnet run --file scripts/csharp/verify-windows-ssr-release.cs -- --path-base /todo` publishes this sample as an isolated NuGet consumer with `JazorSSR=true`; the SSR gate exercises the standard `[Parameter]`/`SetParametersAsync(ParameterView)` entry before Chrome hydration.

Build the host from the repository root (include `-p:JazorSSR=true` when using SSR):

```bash
dotnet build samples/RazorVue.TodoList/Todo.Host/Todo.Host.csproj
```

Start the project web service in another terminal:

```bash
cd samples/RazorVue.TodoList/Todo.Host/jazor
deno task dev
```

Run the .NET development watcher from the repository root:

```bash
dotnet watch --project samples/RazorVue.TodoList/Todo.Host/Todo.Host.csproj --no-hot-reload --no-launch-profile -- --urls http://127.0.0.1:4308
```

Set `Todo:Ssr=true` to serve rendered HTML and hydrate it through the same proxy. `Todo:JavaScriptServer` selects the project server origin (default `http://127.0.0.1:5173`). When using `Todo:PathBase=/docs`, set the project's static Vite `base` to `/docs/jazor/` so generated URLs and WebSocket connections use the same public prefix.

The Release gate starts a Deno/Vite preview service for `dist/`, starts the published ASP.NET Core host, and checks real browser hydration and interactions through `/todo`. Vite preview is used to verify the build; production hosting can use the project's chosen web server with the same public paths. The CSR shell loads `entry.js` in Development and the production build output `dist/bundle.js` (served by the project web service under the same public `/jazor/` prefix) in production.

Generated Vue components still need migration from the former Jazor HMR bridge to standard tool HMR. Ordinary Vite module updates and SSR Deno watch do not yet prove preservation of Vue component state.
