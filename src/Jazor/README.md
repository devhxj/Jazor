# Jazor

`Jazor` packages the Jazor runtime, analyzer, source generator, emit tool, and MSBuild integration into a single installable NuGet package.

## Rules

- Every project that declares `[ECMAScriptModule]` must reference `Jazor` directly.
- Class libraries usually keep `JazorEmit=false`.
- The final executable or web host project is expected to enable `JazorEmit` and optionally `JazorBundle`.

## Example

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.13" />
</ItemGroup>

<PropertyGroup>
  <JazorEmit>true</JazorEmit>
  <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\generated\</JazorOutDir>
  <JazorBundle>false</JazorBundle>
</PropertyGroup>
```

## Multi-project solutions

- Every class library that declares `[ECMAScriptModule]` should reference `Jazor` so the analyzer and source generator run in that project.
- The final executable or web host project should also reference `Jazor` and set `JazorEmit=true`.
- `JazorEmit` scans the host output plus copied referenced assemblies, so modules generated in referenced class libraries are emitted together.
- `JazorBundle=true` bundles emitted modules through `DenoHost`, so the consumer machine does not need a globally installed `deno`.
- The generated bundle re-exports the root assembly modules, while referenced library modules are inlined as dependencies inside the bundle.

Repository sample:

- `samples/Jazor.MultiProject` shows `contracts -> features -> host` layout and local pack/build flow.
