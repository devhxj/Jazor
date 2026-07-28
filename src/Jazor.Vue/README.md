# Jazor.Vue

`Jazor.Vue` is the explicit Razor-to-Vue integration package for Jazor.

It installs the generator-driver hook that consumes the final Roslyn `Compilation` produced by the official Razor source generator. Razor component `BuildRenderTree` operations are lowered to Vue render-function modules and registered in `Jazor.Generated.VueRenderCatalog` for `Jazor.Emit`.

## Install

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.31" />
  <PackageReference Include="Jazor.Vue" Version="0.1.31" PrivateAssets="all" />
</ItemGroup>
```

The package is opt-in and must be used together with `Jazor`, which supplies the shared analyzer dependencies. Referencing `Jazor` alone does not install the Razor hook, scan Razor components, or generate a Vue render catalog. `Jazor.Vue` packages only the merged `Jazor.RazorVue` analyzer assembly so shared generators are loaded exactly once.

No Razor host-output property is required. The integration does not use `EnableRazorHostOutputs`, `RazorCodeDocument`, `RazorCSharpDocument`, or a second parse of generated C#.

## Output

`JazorMode` only selects output materialization:

| Value | Result |
|---|---|
| `none` | Default. No output. |
| `debug` | Modules, source maps, and manifest. |
| `release` | Production bundle and source map. |

`JazorDir` defaults to `$(MSBuildProjectDirectory)\wwwroot\jazor\`. `JazorTool` is used only when `JazorMode=release`.
