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

## Pack

```bash
pwsh ./scripts/publish-nuget.ps1 -SkipPush
```

- `publish-nuget.ps1` is the local entry point for `pack -> push`.
- `Jazor.csproj` builds the required sibling projects and publishes `Jazor.Emit` automatically before generating the `.nupkg`.
- The current package content is assembled from Windows publish output for `Jazor.Emit`, so use a Windows machine/runner when producing the release package.
- If the current worktree has unrelated compile breakage but the required outputs already exist, use `pwsh ./scripts/publish-nuget.ps1 -SkipPush -NoBuild`.

## Verify

```bash
pwsh ./scripts/verify-nuget-package.ps1
```

- `verify-nuget-package.ps1` runs a local dry-run package build and checks the generated `.nupkg` for required metadata and key entries such as `buildTransitive`, analyzer payload, and `tools/net10.0/Jazor.Emit.dll`.

## Publish

- Local publish:

```powershell
$env:NUGET_API_KEY = "<nuget-api-key>"
pwsh ./scripts/publish-nuget.ps1
```

- GitHub Actions workflow: `.github/workflows/nuget-publish.yml`
- Trigger by pushing a `v*` tag that matches `src/Jazor/Jazor.csproj` `Version`, or run the workflow manually.
- Configure `NUGET_API_KEY` for direct push to nuget.org.
- If you prefer NuGet Trusted Publishing, leave `NUGET_API_KEY` empty and configure `NUGET_USER` instead.
