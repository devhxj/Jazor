# RazorVue TODOList

`RazorVue.TodoList` exercises the generated standard JavaScript project through the unified ASP.NET Core frontend host:

- `Todo.Library` contains a native RazorVue component with Razor bindings, events, loops, and conditional state.
- `TodoStyleSheet` owns every visual rule through `ECMAScript.Style`; there is no authored CSS under `wwwroot`.
- `Todo.Host` emits to its project-root `jazor/` directory. In Development, DenoHost starts Vite and the ASP.NET Core host forwards `/jazor` HTTP and WebSocket requests. In other environments the same middleware serves the Release artifact graph directly.
- SSR executes the project's `ssr` task in production and `ssr:dev` in Development. The latter uses Deno watch so transitive module edits become visible without rewriting the entry.
- `Todo:PathBase` supports a mounted application such as `/docs`.
- `dotnet run --file scripts/csharp/verify-windows-ssr-release.cs -- --path-base /todo` publishes this sample as an isolated NuGet consumer with `JazorSSR=true`; the SSR gate exercises the standard `[Parameter]`/`SetParametersAsync(ParameterView)` entry before Chrome hydration.

Build the host from the repository root (include `-p:JazorSSR=true` when using SSR):

```bash
dotnet build samples/RazorVue.TodoList/Todo.Host/Todo.Host.csproj
```

Run the host from Visual Studio, or start the .NET development watcher from the repository root:

```bash
dotnet watch --project samples/RazorVue.TodoList/Todo.Host/Todo.Host.csproj --no-hot-reload --no-launch-profile -- --urls http://127.0.0.1:4308
```

Set `Todo:Ssr=true` to serve rendered HTML and hydrate it through the same frontend host. `Todo:JavaScriptServer` selects the Vite origin (default `http://127.0.0.1:5173`); `Todo:PathBase=/docs` configures PathBase, Vite base, proxy targets and browser URLs together.

The Release gate starts the published ASP.NET Core host and checks real browser hydration and interactions through `/todo`. The CSR shell loads `entry.js` through Vite in Development and `dist/bundle.js` from the published artifact graph in production; publishing never starts Vite.

Generated Vue components still need migration from the former Jazor HMR bridge to standard tool HMR. Ordinary Vite module updates and SSR Deno watch do not yet prove preservation of Vue component state.
