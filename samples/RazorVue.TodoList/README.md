# RazorVue TODOList

`RazorVue.TodoList` is the real application used by the Windows development-host/HMR gate. It is intentionally small, but it exercises the production layout rather than a synthetic module fixture:

- `Todo.Library` contains a native RazorVue component with Razor bindings, events, loops, and conditional state.
- `TodoStyleSheet` owns every visual rule through `ECMAScript.Style`; there is no authored CSS under `wwwroot`.
- `Todo.Host` emits to its project-root `jazor/` directory, serves that directory with `UseJazorHost`, and enables development reload through `AddJazorReload` / `UseJazorReload`.
- `Todo:PathBase` supports a mounted application such as `/docs`.

Build the host from the repository root:

```bash
dotnet build samples/RazorVue.TodoList/Todo.Host/Todo.Host.csproj
```

Run it under the development watcher:

```bash
dotnet watch --project samples/RazorVue.TodoList/Todo.Host/Todo.Host.csproj --no-hot-reload --no-launch-profile -- --urls http://127.0.0.1:4308
```

The Windows browser gate edits a temporary copy through `dotnet watch`; it verifies template-only state-preserving HMR, logic full reload, transient replacement, compile-error recovery, PathBase transport, and reconnect behavior.
