# Jazor

Write JavaScript in C#.

Jazor is a C#-to-JavaScript compiler that translates Roslyn `IOperation` semantics into standard ECMAScript AST. The `Jazor` package carries the core runtime, analyzer, source generator, emit tool, MSBuild integration, and the baseline Vue 3 authoring surface.

## Features

- **C# → JS compilation** — lowers Roslyn semantic model (`IOperation`) to ESTree-compliant JavaScript via Acornima, preserving evaluation order and side-effect semantics.
- **Whitelist-gated runtime surface** — only explicitly mapped CLR APIs are emitted as JavaScript. The bundled analyzer enforces these boundaries at compile time.
- **Vue 3 authoring surface** — write `defineComponent()`, `h()`, typed props/slots, and reactive setups in C#; emitted JS is standard Vue 3 component shapes with no private runtime wrapper.
- **Record structural lowering** — C# records lower to plain JS objects with `[Spread]` flattening and static `null` omission, no runtime overhead.
- **Source Generator + MSBuild** — whitelist generation, emit, and optional bundling run automatically during build. No extra toolchain required.
- **Source Maps** — every emitted `.mjs` ships with an accompanying `.mjs.map`.

## Install

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.27" />
</ItemGroup>
```

## Usage

### Class libraries

Every project that declares `[ECMAScriptModule]` must reference `Jazor`:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.27" />
</ItemGroup>
```

Library projects keep the default `JazorMode=none`.

### Optional frontend ecosystem packages

`Jazor` no longer bundles higher-level Vue ecosystem libraries by default. Add them explicitly when your authoring surface needs them:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.27" />
  <PackageReference Include="ECMAScript.VueRoute" Version="0.1.27" />
  <PackageReference Include="ECMAScript.Pinia" Version="0.1.27" />
  <PackageReference Include="ECMAScript.Vuetify" Version="0.1.27" />
  <PackageReference Include="ECMAScript.TDesign" Version="0.1.27" />
</ItemGroup>
```

- `ECMAScript.Vue3` remains part of the default `Jazor` package.
- `ECMAScript.Pinia.Testing` is a separate opt-in testing package layered on top of `ECMAScript.Pinia`.

### Host / executable projects

The final executable or web host project selects one output mode:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.27" />
</ItemGroup>

<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorDir>
  <JazorTool>Deno</JazorTool> <!-- Used only by JazorMode=release -->
</PropertyGroup>
```

- `JazorMode=none` is the default and writes no artifacts.
- `JazorMode=debug` scans the host output and referenced assemblies, then writes debug modules and `jazor-manifest.json`.
- `JazorMode=release` performs its internal materialization in an intermediate directory, then writes only the production bundle and source map through `JazorTool`; `Deno` uses the bundled runtime, and `Netpack` uses the packaged NetPack lane.

### Razor integration status

The transformation branch automatically exposes the official Razor Source Generator final-document input boundary for Razor SDK projects:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Jazor" Version="0.1.27" />
  </ItemGroup>
</Project>
```

The package hooks the completed Roslyn generator driver result, then binds final generated C# and `BuildRenderTree` against that final compilation. Razor component modules lower to Vue render-function `.mjs` artifacts through the shared compiler/render-context path.

### Current MSBuild emit behavior

The existing emit targets continue to handle declared ECMAScript modules independently of the future Razor component lowering:

- `JazorMode` defaults to `none`; `debug` and `release` are mutually exclusive build outputs.
- `JazorDir` defaults to `$(MSBuildProjectDirectory)\wwwroot\jazor\`.
- `debug` writes modules plus `jazor-manifest.json`; `release` clears `JazorDir`, materializes internally, and writes only production bundle assets.
- `JazorTool` defaults to `Deno` and is used only by `release`; bundle builds pass the intermediate manifest, artifact root, source root, and output root to the selected `Deno` or `Netpack` lane.
