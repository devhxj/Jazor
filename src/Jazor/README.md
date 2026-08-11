# Jazor

Write JavaScript in C#.

Jazor is a C#-to-JavaScript compiler that translates Roslyn `IOperation` semantics into standard ECMAScript AST. The `Jazor` package carries the core runtime, analyzer, source generator, emit tool, MSBuild integration, and the baseline Vue 3 authoring surface. Razor-to-Vue integration is provided separately by `Jazor.Vue`.

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
  <PackageReference Include="Jazor" Version="0.8.3" />
</ItemGroup>
```

## Usage

### Class libraries

Every project that declares `[ECMAScriptModule]` must reference `Jazor`:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.8.3" />
</ItemGroup>
```

Library projects keep the default `JazorMode=none`.

### Optional frontend ecosystem packages

`Jazor` no longer bundles higher-level Vue ecosystem libraries by default. Add them explicitly when your authoring surface needs them:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.8.3" />
  <PackageReference Include="ECMAScript.VueRoute" Version="0.8.3" />
  <PackageReference Include="ECMAScript.Pinia" Version="0.8.3" />
  <PackageReference Include="ECMAScript.Vuetify" Version="0.8.3" />
  <PackageReference Include="ECMAScript.TDesign" Version="0.8.3" />
  <PackageReference Include="ECMAScript.Style" Version="0.8.3" />
</ItemGroup>
```

- `ECMAScript.Vue3` remains part of the default `Jazor` package.
- `ECMAScript.Pinia.Testing` is a separate opt-in testing package layered on top of `ECMAScript.Pinia`.
- `ECMAScript.Style` is a framework-neutral module in the ECMAScript ecosystem. It depends on the exact same `Jazor` version and reuses this package's compiler and MSBuild integration.

### Host / executable projects

The final executable or web host project selects one output mode:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.8.3" />
</ItemGroup>

<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorDir>
</PropertyGroup>
```

- `JazorMode=none` is the default and writes no artifacts.
- `JazorMode=debug` scans the host output and referenced assemblies, then writes debug modules and `jazor-manifest.json`.
- `JazorMode=release` performs its internal materialization in an intermediate directory, then writes the production browser bundle and source map through the packaged Netpack lane.
- `JazorSsrEnabled=true` additionally preserves a materialized raw module graph at `wwwroot/jazor/ssr/` for server rendering. This graph is separate from the optimized browser bundle.

### SSR

ASP.NET Core owns the SSR request pipeline, routing, response document, and static assets. DenoHost executes the generated Vue module through its packaged local runtime, so the application does not need a globally installed Deno executable.

```xml
<PropertyGroup>
  <JazorMode>release</JazorMode>
  <JazorSsrEnabled>true</JazorSsrEnabled>
</PropertyGroup>
```

```csharp
builder.Services.AddJazorSsr();

var app = builder.Build();
app.UseStaticFiles();
app.UseJazorSsr("components/app.mjs", new { Title = "Jazor" });
```

`UseJazorSsr` uses the existing SPA fallback rules, so static files and mapped endpoints continue to win. It renders with `@vue/server-renderer`, emits the same JSON props for client hydration, and retains the browser import map and styles in the response.

`IJazorSsrRenderer` is the stable SSR boundary. DenoHost is the runtime executor, while Netpack remains the browser build-time bundler. Neither role requires application `node_modules`, a CDN, or remote imports. Vue server-prefetch state is not automatically transferred to the browser; applications must explicitly include any shared state in their props or another application-owned payload.

### Razor-to-Vue integration

Add `Jazor.Vue` to a Razor SDK project to opt into the official Razor Source Generator final-compilation boundary:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Jazor" Version="0.8.3" />
    <PackageReference Include="Jazor.Vue" Version="0.8.3" PrivateAssets="all" />
  </ItemGroup>
</Project>
```

`Jazor.Vue` hooks the completed Roslyn generator-driver result, then binds final generated C# and `BuildRenderTree` against that final compilation. Razor component modules lower to Vue render-function `.mjs` artifacts through the shared compiler/render-context path. `Jazor` alone neither installs the hook nor scans Razor components.

The integration directly consumes the final `Compilation`; it does not require `EnableRazorHostOutputs`, `RazorCodeDocument`, `RazorCSharpDocument`, or reparsing generated C#.

### Current MSBuild emit behavior

The existing emit targets continue to handle declared ECMAScript modules independently of the future Razor component lowering:

- `JazorMode` defaults to `none`; `debug` and `release` are mutually exclusive build outputs.
- `JazorDir` defaults to `$(MSBuildProjectDirectory)\wwwroot\jazor\`.
- `debug` writes modules plus `jazor-manifest.json`; `release` clears `JazorDir`, materializes internally, and writes browser bundle assets. With `JazorSsrEnabled=true`, it also writes the raw SSR graph under `JazorDir\ssr\`.
- `release` passes the intermediate manifest, artifact root, source root, and output root to the fixed Netpack bundle lane.
