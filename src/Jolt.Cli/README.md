# Jolt

Jolt builds Vue components authored in C# with `.jazor`. Install the published tool and run it from a Jolt project folder. [Authoring guide](../../docs/03-guides/jolt-authoring.md)

```sh
dotnet tool install --global Jazor.Jolt --version 2.4.0
jolt dev . --port 15658
```

The tool requires .NET 11 and publishes packages for Windows and glibc Linux on x64 and ARM64. Component builds restore the project's locked npm dependencies and use the public TypeScript declarations and ESM exports from those dependencies.

Use `jolt bindings` to generate public C# declarations for selected exports from a locked npm dependency. Point the command at the directory containing `package.json` and `pnpm-lock.yaml`. Place the generated file in a .NET 11 library referencing the Jazor host contracts and enable preview C# syntax for packages that use projected unions.

```sh
jolt bindings . --module date-fns --export addDays --export formatDuration --output Library/Dates.g.cs
```

Repeat `--module` with its own `--export` flags to generate one file with shared types across module specifiers.

```sh
jolt bindings . --module date-fns --export formatDistance --module date-fns/locale --export fr --output Library/LocalizedDates.g.cs
```

Other projects can reference the compiled library and use the generated API through `Npm.DateFns.Exports`. The generated declarations preserve npm module and export names for Jazor's JavaScript emitter.

`jolt bundle . --ssr entry-server.ts` compiles the components once, then builds both production entries with Vite. The client starts at `index.html` and emits `dist/client/index.html`, hashed assets, and `dist/client/.vite/manifest.json`. The server entry exports a named `render` function and bundles to `dist/server/entry-server.mjs`. A successful SSR build replaces the project's `dist` directory. Failed builds preserve the previous output.

To pack and verify the tool from the Jazor repository, run:

```sh
dotnet run --file scripts/csharp/verify-jolt-package.cs
```
