# Jolt

Jolt builds Vue components authored in C# with `.jazor`. Install the published tool and run it from a Jolt project folder. [Authoring guide](../../docs/03-guides/jolt-authoring.md)

```sh
dotnet tool install --global Jazor.Jolt
jolt dev . --port 15658
```

The tool requires .NET 11 and publishes packages for Windows and glibc Linux on x64 and ARM64. Component builds restore the project's locked npm dependencies and use the public TypeScript declarations and ESM exports from those dependencies.

A host that starts its own development server can run `jolt dev . --port 15658 --stop-on-stdin-close` with redirected standard input. Keep the write end open for the lifetime of the session. Closing it asks Jolt to close Vite and exit. When connecting to an existing server, the process that started it retains ownership. [ASP.NET Core example](../../samples/Jolt.AspNetCore/)

With the Jazor SDK built from this checkout, ordinary C# projects can use `using Npm…` and `dotnet build` to resolve npm bindings. To test the matching Jolt tool locally, run the package verifier below. [Authoring guide](../../docs/03-guides/jolt-authoring.md#普通-c-类库的公开投影)

`jolt build` writes component modules and declarations under the project's `.jazor` directory by default. In a .NET project, the SDK and standalone tool both honor the evaluated `JazorDir`. For example, import `/cart/checkout.mjs` for a module declared as `cart/checkout.mjs`. `jolt bundle` writes the Vite application under the generated root's `dist/`. `jolt library` publishes a package under the project's `dist/` by default.

`jolt bundle . --ssr entry-server.ts` compiles the components once, then builds both production entries with Vite. The client starts at `index.html` and emits `.jazor/dist/client/index.html`, hashed assets, and `.jazor/dist/client/.vite/manifest.json`. The server entry exports a named `render` function and bundles to `.jazor/dist/server/entry-server.mjs`. Failed SSR builds preserve the previous output.

To pack and verify the tool from the Jazor repository, run:

```sh
dotnet run --file scripts/csharp/verify-jolt-package.cs
```
