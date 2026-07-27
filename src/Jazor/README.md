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

Keep `JazorEmit` disabled (default) in library projects.

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

The final executable or web host project enables emit and optionally bundling:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.27" />
</ItemGroup>

<PropertyGroup>
  <JazorEmit>true</JazorEmit>
  <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\generated\</JazorOutDir>
  <JazorToolchain>Deno</JazorToolchain>
  <JazorBundle>false</JazorBundle>
</PropertyGroup>
```

- `JazorEmit` scans the host output and referenced assemblies, emitting all declared modules together.
- `JazorBundle=true` bundles emitted modules through the selected `JazorToolchain`; `Deno` uses the bundled runtime, and `Netpack` uses the packaged NetPack lane.

### Razor integration status

The transformation branch currently exposes the official Razor Source Generator final-document input boundary. A Web SDK project can opt into that boundary with the compiler-visible property already shipped by `buildTransitive/Jazor.props`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
    <RazorLangVersion>11.0</RazorLangVersion>
    <UseRazorSourceGenerator>true</UseRazorSourceGenerator>
    <JazorRazorVueEnableRazorSgIntegration>true</JazorRazorVueEnableRazorSgIntegration>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Jazor" Version="0.1.27" />
  </ItemGroup>
</Project>
```

This opt-in enables the controlled SG tail hook that adapts final generated C# and binds `BuildRenderTree` against the callback compilation. Razor component modules lower to Vue render-function `.mjs` artifacts through the shared compiler/render-context path.

### Current MSBuild emit behavior

The existing emit targets continue to handle declared ECMAScript modules independently of the future Razor component lowering:

- `JazorEmit` runs after a successful build when enabled, scans the target and copy-local assemblies, and writes modules plus `jazor-manifest.json`.
- `JazorOutDir` defaults to `JazorDevOutDir`, which defaults to `$(MSBuildProjectDirectory)\jazor\`.
- During publish, the default non-materialized path switches to `JazorPublishOutDir`, which defaults to `$(MSBuildProjectDirectory)\wwwroot\jazor\`.
- `JazorToolchain` defaults to `Deno`; bundle builds pass the explicit manifest, artifact root, source root, and output root to the selected `Deno` or `Netpack` lane.
- `JazorBundle=true` runs the bundled emit tool after `JazorEmit` and writes production bundle assets under the `JazorBundleOut` output root.
- `JazorCleanEmit` and `JazorFailOnPathConflict` both default to `true`.
- `JazorPublishMaterializeEnabled=true` copies `JazorOutDir` into the published `wwwroot/jazor` directory and removes a publish-root shadow `jazor` directory.
