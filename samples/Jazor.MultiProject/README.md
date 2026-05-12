# Jazor.MultiProject

This sample shows the recommended SDK layout for a multi-project solution:

- `Sample.Contracts`: shared module library.
- `Sample.Features`: class library that declares `[ECMAScriptModule]` and references `Sample.Contracts`.
- `Sample.Host`: final host that turns on `JazorEmit` and emits modules from itself and referenced libraries.

## Build with a published package

If `Jazor` is already published to your feed, build the host project directly:

```powershell
dotnet build .\Sample.Host\Sample.Host.csproj
```

Generated modules are written to:

```text
.\Sample.Host\wwwroot\jazor\
```

## Build from this repository

Use the helper script to build the pack inputs, pack the local `Jazor` package, and rebuild the host against that package:

```powershell
dotnet run --file .\samples\Jazor.MultiProject\build-local.cs
```

The script:

1. builds the runtime, analyzer/compiler, and emit tool,
2. packs `src/Jazor`,
3. restores the sample from the local package folder,
4. rebuilds `Sample.Host`,
5. emits JavaScript into `Sample.Host\wwwroot\jazor\`.

The script uses `Rebuild` on the host so local package/output caching does not leave stale generated modules behind.

Generated output includes modules from all referenced projects that declare `[ECMAScriptModule]`.

## Bundle with DenoHost

`JazorBundle` now uses `DenoHost`, so the sample does not depend on a globally installed `deno`.

Use the same script with `-Bundle`:

```powershell
dotnet run --file .\samples\Jazor.MultiProject\build-local.cs -- --bundle
```

This writes:

```text
.\Sample.Host\wwwroot\app.bundle.js
```

The bundled file re-exports the host module members, so in this sample the final bundle exports `boot`.
