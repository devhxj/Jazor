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
  <PackageReference Include="Jazor" Version="0.1.22" />
</ItemGroup>
```

## Usage

### Class libraries

Every project that declares `[ECMAScriptModule]` must reference `Jazor`:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.22" />
</ItemGroup>
```

Keep `JazorEmit` disabled (default) in library projects.

### Optional frontend ecosystem packages

`Jazor` no longer bundles higher-level Vue ecosystem libraries by default. Add them explicitly when your authoring surface needs them:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.22" />
  <PackageReference Include="ECMAScript.VueRoute" Version="0.1.22" />
  <PackageReference Include="ECMAScript.Pinia" Version="0.1.22" />
  <PackageReference Include="ECMAScript.Vuetify" Version="0.1.22" />
  <PackageReference Include="ECMAScript.TDesign" Version="0.1.22" />
</ItemGroup>
```

- `ECMAScript.Vue3` remains part of the default `Jazor` package.
- `ECMAScript.Pinia.Testing` is a separate opt-in testing package layered on top of `ECMAScript.Pinia`.

### Host / executable projects

The final executable or web host project enables emit and optionally bundling:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.22" />
</ItemGroup>

<PropertyGroup>
  <JazorEmit>true</JazorEmit>
  <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\generated\</JazorOutDir>
  <JazorBundle>false</JazorBundle>
</PropertyGroup>
```

- `JazorEmit` scans the host output and referenced assemblies, emitting all declared modules together.
- `JazorBundle=true` bundles emitted modules through the bundled Deno runtime — no global Deno install needed on the consumer machine.
