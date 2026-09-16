# Jazor Usage Patterns

Use these patterns as starting points and adjust them to the project inspected by the caller. Keep every Jazor and ECMAScript package on the same version.

## Core module library

```bash
dotnet new classlib -n Sample.Modules
dotnet add Sample.Modules package Jazor --version 1.0.0-preview.3
```

```csharp
using ECMAScript;

namespace Sample.Modules;

[ECMAScriptModule("features/greetings.mjs")]
public static class Greetings
{
    public static string Create(string name) => $"Hello, {name}";
}
```

The generated assembly carries `Jazor.Generated.ModuleCatalog`. The final host owns materialization:

```xml
<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)\jazor\</JazorDir>
</PropertyGroup>
```

## RazorVue project

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="1.0.0-preview.3" />
  <PackageReference Include="Jazor.Vue" Version="1.0.0-preview.3" PrivateAssets="all" />
</ItemGroup>
```

Keep `.razor` parameters strongly typed and valid for official Razor SG. Use Vue and component binding contracts instead of JavaScript strings. Read `docs/03-guides/razorvue-authoring.md` for current component and slot shapes.

## Ecosystem bindings

```xml
<ItemGroup>
  <PackageReference Include="ECMAScript.VueRoute" Version="1.0.0-preview.3" />
  <PackageReference Include="ECMAScript.Pinia" Version="1.0.0-preview.3" />
  <PackageReference Include="ECMAScript.ElementPlus" Version="1.0.0-preview.3" />
  <PackageReference Include="ECMAScript.Vuetify" Version="1.0.0-preview.3" />
  <PackageReference Include="ECMAScript.TDesign" Version="1.0.0-preview.3" />
</ItemGroup>
```

Add only packages whose public APIs are authored. Prefer typed props, events, slots, enums, unions, callbacks, and collection builders. Package XML provides IDE documentation; upstream comments are preferred for framework API descriptions.

## SSR host

```xml
<PropertyGroup>
  <JazorMode>release</JazorMode>
  <JazorSSR>true</JazorSSR>
</PropertyGroup>
```

Register Jazor SSR services and middleware from `docs/03-guides/installation-and-configuration.md`. Keep business bootstrap data in typed DTOs passed through `JazorSsrRequest.Props`; keep authentication state in the documented authentication envelope.

## Validation

```bash
dotnet restore Jazor.slnx
dotnet build Jazor.slnx
dotnet run --file scripts/csharp/test-dotnet.cs
```

For a consumer project, start with `dotnet build <host-project>` and verify the configured `jazor/` directory contains the expected `.mjs`, source map, manifest, and import map. Use focused tests after the first build succeeds.
