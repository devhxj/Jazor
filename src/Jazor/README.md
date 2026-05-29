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
  <PackageReference Include="Jazor" Version="0.1.26" />
</ItemGroup>
```

## Usage

### Class libraries

Every project that declares `[ECMAScriptModule]` must reference `Jazor`:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.26" />
</ItemGroup>
```

Keep `JazorEmit` disabled (default) in library projects.

### Optional frontend ecosystem packages

`Jazor` no longer bundles higher-level Vue ecosystem libraries by default. Add them explicitly when your authoring surface needs them:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.26" />
  <PackageReference Include="ECMAScript.VueRoute" Version="0.1.26" />
  <PackageReference Include="ECMAScript.Pinia" Version="0.1.26" />
  <PackageReference Include="ECMAScript.Vuetify" Version="0.1.26" />
  <PackageReference Include="ECMAScript.TDesign" Version="0.1.26" />
</ItemGroup>
```

- `ECMAScript.Vue3` remains part of the default `Jazor` package.
- `ECMAScript.Pinia.Testing` is a separate opt-in testing package layered on top of `ECMAScript.Pinia`.

### Host / executable projects

The final executable or web host project enables emit and optionally bundling:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.26" />
</ItemGroup>

<PropertyGroup>
  <JazorEmit>true</JazorEmit>
  <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\generated\</JazorOutDir>
  <JazorBundle>false</JazorBundle>
</PropertyGroup>
```

- `JazorEmit` scans the host output and referenced assemblies, emitting all declared modules together.
- `JazorBundle=true` bundles emitted modules through the bundled Deno runtime — no global Deno install needed on the consumer machine.

### RazorVue Web SDK host with colocated consumer

For RazorVue library mode, the supported Web SDK shape is one ASP.NET Core runtime host plus a colocated frontend consumer build layer. The `consumer` directory is build-time tooling; it is not a second application host.

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
    <RazorLangVersion>11.0</RazorLangVersion>
    <UseRazorSourceGenerator>true</UseRazorSourceGenerator>

    <JazorEmit>true</JazorEmit>
    <JazorBundle>false</JazorBundle>
    <JazorRazorVueOutputMode>sfc</JazorRazorVueOutputMode>
    <JazorRazorVueEnableRazorSgIntegration>true</JazorRazorVueEnableRazorSgIntegration>

    <JazorOutDir>$(MSBuildProjectDirectory)\jazor\</JazorOutDir>
    <JazorPublishMaterializeEnabled>true</JazorPublishMaterializeEnabled>
    <JazorConsumerRoot>$(MSBuildProjectDirectory)\consumer</JazorConsumerRoot>
  </PropertyGroup>

  <ItemGroup>
    <Compile Remove="consumer\**" />
    <Content Remove="consumer\**" />
    <EmbeddedResource Remove="consumer\**" />
    <None Remove="consumer\**" />
    <Compile Remove="jazor\**" />
    <Content Remove="jazor\**" />
    <EmbeddedResource Remove="jazor\**" />
    <None Remove="jazor\**" />
    <Compile Remove="wwwroot\jazor\**" />
    <Content Remove="wwwroot\jazor\**" />
    <EmbeddedResource Remove="wwwroot\jazor\**" />
    <None Remove="wwwroot\jazor\**" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Jazor" Version="0.1.26" />
  </ItemGroup>
</Project>
```

The standard layout is:

```text
HostProject/
  jazor/                    # compiler-owned development emit root
  wwwroot/jazor/            # browser assets produced by the consumer
  consumer/
    deno.json
    index.html
    scripts/run-deno.cs     # default JazorConsumerBuild runner
    scripts/build.ts
```

`dotnet build` runs `JazorEmit` first, writing compiler-owned RazorVue artifacts such as `.vue`, `jazor-manifest.json`, source maps, origins files, and `__jazor/razorvue-host.mjs` under `jazor/`. After that, `JazorConsumerBuild` runs:

```powershell
dotnet run --file "<JazorConsumerRoot>\scripts\run-deno.cs" -- task build
```

The SDK passes these environment variables to the consumer runner:

- `RAZORVUE_HOST_JAZOR_ROOT`: the compiler-owned host `jazor/` root.
- `RAZORVUE_HOST_WWWROOT_ROOT`: the host `wwwroot/` root.
- `JAZOR_EMIT_TOOL_PATH`: the `Jazor.Emit` tool used for `razorvue-consumer-entry` and SFC bridge generation.

The consumer build should use `Jazor.Emit razorvue-consumer-entry` to convert generated `.vue` default components into named exports before bundling. Build-time browser assets belong in `wwwroot/jazor/`, typically `client-entry.js`, `client-entry.css`, and maps. Do not copy compiler-owned `.vue` files or `jazor-manifest.json` into `wwwroot/jazor/` during normal build.

If `JazorConsumerRoot` is set but the default runner does not exist, the SDK fails the build with `Jazor consumer runner was not found: ...` after `JazorEmit` completes. It does not run a private Razor Source Generator fallback and does not continue with stale browser assets.

During `dotnet publish`, `JazorPublishMaterializeEnabled=true` materializes compiler-owned output from `jazor/` into the published `wwwroot/jazor/`, then overlays the consumer browser assets from the host `wwwroot/jazor/`. The published output must serve `/jazor/*` from `wwwroot/jazor/`; it should not contain a shadow publish-root `jazor/` directory or legacy `wwwroot/assets/` browser output.
