**English** | [中文](README_CN.md)

<div align="center">

![Today's Verse](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)
</div>

# Jazor

[![.NET](https://img.shields.io/badge/.NET-11.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.txt)
[![NuGet](https://img.shields.io/nuget/v/Jazor.svg)](https://www.nuget.org/packages/Jazor)

> Experimental. Public APIs, generated artifact shape, and tooling are still evolving. The compiler core, emit pipeline, and shared RazorVue/Jolt contracts are the most stable foundations.

Jazor is a .NET toolchain for authoring JavaScript and Vue applications from C# and Razor.

The repository currently has two active product lines:

| Line | Mode | Main projects | What it does |
|------|------|---------------|--------------|
| **RazorVue** | Library mode | `Jazor.RazorVue`, `Jazor.Analyzer`, `Jazor.Emit` | Build-time Razor component analysis and generation of final `.vue` / render-function artifacts. |
| **Jolt** | Full development host | `Jolt` | `.jazor` authoring host with LSP, workspace coordination, DevServer/HMR, debug, build, and Deno-backed frontend tooling. |

Both lines share the same compiler and runtime foundations: `Jazor.Compiler`, `Jazor.CLR`, `Jazor.Analyzer`, `Jazor.Emit`, `Jazor.Common`, and the ECMAScript/Vue binding assemblies.

## Current Focus

- **Compiler core**: Roslyn `IOperation` to Acornima ESTree lowering, with explicit support boundaries and deterministic emission.
- **RazorVue library mode**: Razor components compile to final Vue artifacts. The `.vue` / render-function artifact boundary is the public output contract, not an intermediate wrapper-JS protocol.
- **Jolt development mode**: `.jazor` remains the authoring document. Jolt coordinates Razor/Roslyn/Volar lanes, local preview, HMR, source maps, debug, and production build.
- **Emit and materialization**: `Jazor.Emit` writes `.mjs`, `.vue`, manifests, source maps, bundle output, and RazorVue consumer bridge modules.
- **Vue ecosystem bindings**: Vue 3 core bindings are part of the `Jazor` package; Pinia, Vue Router, Vuetify, and other UI/library bindings are maintained as explicit ecosystem projects.

For current status, prefer the status pages under `docs/03-完成/` and local test output over hard-coded counts in README files.

## Feature Overview

- **Semantic C# lowering**: C# is lowered through Roslyn `IOperation`, not syntax-string rewriting.
- **Fail-fast host boundary**: unsupported external runtime semantics are rejected at the actual lowering site instead of silently emitting raw JavaScript approximations.
- **Whitelist-gated CLR surface**: common CLR APIs are mapped through `Jazor.CLR` and generated whitelist metadata; analyzer diagnostics catch many unsupported usages early.
- **ECMAScript module output**: `[ECMAScriptModule]` classes emit named-export `.mjs` modules with stable import collection, source-origin tracking, and source-map carriers.
- **RazorVue artifact generation**: Razor component semantics flow through Razor SG / Roslyn / Razor IR where appropriate, then lower to Vue SFC or render-function artifacts.
- **Typed Vue authoring**: `ECMAScript.Vue3` provides typed bindings for Vue 3 `defineComponent`, `h`, refs/reactivity, lifecycle hooks, props, slots, and component contracts.
- **Host-facing build support**: MSBuild targets can emit modules, materialize RazorVue artifacts, run consumer builds, publish assets, and bundle through the bundled Deno runtime.
- **Jolt tooling**: Jolt provides the active `.jazor` development-time boundary for editor services, preview/HMR, source-map-aware debug, and production build.

## Install

```bash
dotnet add package Jazor
```

The `Jazor` package includes the core runtime contracts, `ECMAScript`, `ECMAScript.Vue3`, `ECMAScript.VueContract`, `Jazor.Compiler`, `Jazor.RazorVue`, `Jazor.Analyzer`, ASP.NET Core integration assemblies, the emit tool, and MSBuild props/targets.

Add ecosystem packages explicitly when needed:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.26" />
  <PackageReference Include="ECMAScript.Pinia" Version="0.1.26" />
  <PackageReference Include="ECMAScript.VueRoute" Version="0.1.26" />
  <PackageReference Include="ECMAScript.Vuetify" Version="0.1.26" />
</ItemGroup>
```

## Authoring Paths

### ECMAScript Module Authoring

Use `[ECMAScriptModule]` for plain C# to JavaScript module emission:

```csharp
using ECMAScript;

namespace MyApp;

[ECMAScriptModule("shared/greetings.mjs")]
public static class GreetingModule
{
    public static string Prefix() => "Hello";
    public static string Compose(string name) => $"{Prefix()}, {name}";
}
```

The compiler emits a named-export ECMAScript module and resolves cross-module imports automatically when another module calls `GreetingModule.Compose(...)`.

### Vue 3 h() Authoring

Use `ECMAScript.Vue3` when authoring Vue components directly in C#:

```csharp
using ECMAScript;
using static ECMAScript.Vue3;

namespace MyApp;

[ECMAScriptModule("app/counter.mjs")]
public static class CounterModule
{
    public static IVueComponent Counter
        => DefineComponent(new VueComponentOptions
        {
            Setup = () =>
            {
                var count = Ref(0);
                return () => H("button", new VueObject
                {
                    Events = new VueDictionary
                    {
                        ["click"] = (Action)(() => count.Value++)
                    }
                }, $"Count: {count.Value}");
            }
        });
}
```

### RazorVue Library Mode

Use RazorVue when you want Razor component authoring and build-time Vue artifacts:

- component libraries author `.razor` / `.razor.cs` components;
- RazorVue analyzes the merged component class and Razor SG output;
- `Jazor.Emit` materializes `.vue` / render-function artifacts, manifests, source maps, and consumer bridge modules;
- a single ASP.NET Core host can run a colocated Deno consumer build layer.

Start from [samples/RazorVue.TodoList](samples/RazorVue.TodoList/) for the current library-mode shape.

### Jolt Development Mode

Use Jolt when you need the full `.jazor` development host:

```bash
dotnet run --project src/Jolt/Jolt.csproj -- --lsp
dotnet run --project src/Jolt/Jolt.csproj -- --dev
dotnet run --project src/Jolt/Jolt.csproj -- --build
```

Jolt owns editor/runtime coordination only. Compiler lowering rules stay in `Jazor.Compiler`, and shared RazorVue/Jolt protocol DTOs stay in `Jazor.RazorVue`.

## MSBuild Properties

| Property | Default | Description |
|----------|---------|-------------|
| `JazorCompile` | `true` | Enables compilation of `[ECMAScriptModule]` types. |
| `JazorEmit` | `true` for executable hosts, `false` for libraries | Emits generated modules/artifacts after build. |
| `JazorRazorVueOutputMode` | `sfc` | Selects the RazorVue artifact mode. |
| `JazorRazorVueEnableRazorSgIntegration` | `false` | Enables Razor Source Generator integration for RazorVue paths that opt in. |
| `JazorDevOutDir` | `$(MSBuildProjectDirectory)\jazor\` | Default development output root for compiler-owned artifacts. |
| `JazorPublishOutDir` | `$(MSBuildProjectDirectory)\wwwroot\jazor\` | Default publish-time browser asset root when publish materialization is not enabled. |
| `JazorOutDir` | `$(JazorDevOutDir)` | Selected output directory for compiler-owned artifacts. |
| `JazorBundle` | `false` | Bundles emitted modules through the bundled Deno runtime. |
| `JazorBundleOut` | `$(OutDir)jazor\app.js` | Output path for bundled JavaScript. |
| `JazorCleanEmit` | `true` | Removes stale emitted files from the output directory. |
| `JazorFailOnPathConflict` | `true` | Fails the build when two modules claim the same output path. |
| `JazorConsumerRoot` | empty | RazorVue colocated consumer root for host projects. |
| `JazorPublishMaterializeEnabled` | `false` | Materializes compiler-owned RazorVue output into publish assets. |

See [src/Jazor/README.md](src/Jazor/README.md) and [src/Jazor.Emit/README.md](src/Jazor.Emit/README.md) for package and emit details.

## Repository Layout

```text
Jazor/
├── src/
│   ├── Jazor.Compiler/              # C# -> JavaScript compiler core
│   ├── Jazor.CLR/                   # CLR runtime mappings and JavaScript helpers
│   ├── Jazor.Analyzer/              # Analyzer and RazorVue source-generator host
│   ├── Jazor.RazorVue/              # Shared RazorVue semantics, artifacts, Razor SDK bridge, protocol DTOs
│   ├── Jazor.Emit/                  # Materialization, manifests, bundling, RazorVue consumer bridge
│   ├── Jazor.Common/                # Shared formatting/source-map utilities and contracts
│   ├── Jazor.AspNetCore*/           # ASP.NET Core runtime and dev integration
│   ├── Jazor/                       # NuGet package bundling core SDK assets
│   ├── Jolt/                        # .jazor development host
│   ├── ECMAScript*/                 # ECMAScript AST/contracts plus Vue ecosystem bindings
│   └── *Test/                       # MSTest regression projects
├── samples/
│   ├── Jazor.MultiProject/          # Baseline multi-project module emission
│   ├── ECMAScript.Pinia.Counter/    # Vue 3 + Pinia sample
│   └── RazorVue.TodoList/           # RazorVue SFC library-mode sample
├── docs/                            # Goals, plans, status snapshots, supplements, retired material
└── scripts/csharp/                  # Repository automation scripts
```

## Development

Prerequisites:

- .NET 11 SDK preview matching [global.json](global.json)
- Windows, Linux, or macOS
- Node/npm only for the archived WebIDL TypeScript generator under `src/ECMAScript.WebIDL`

Common commands from the repository root:

```bash
dotnet restore Jazor.slnx
dotnet build Jazor.slnx

# Main repository test runner
dotnet run --file scripts/csharp/test-dotnet.cs

# Focused suites
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj
dotnet test src/Jolt.Test/Jolt.Test.csproj

# Focused class example
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest"
```

Repository automation scripts should be single-file C# entrypoints under `scripts/csharp/`; avoid adding new PowerShell build/test wrappers.

## Documentation

| Need | Entry |
|------|-------|
| Repository docs hub | [docs/README.md](docs/README.md) |
| Current workstream dashboard | [docs/02-计划/workstream-dashboard.md](docs/02-%E8%AE%A1%E5%88%92/workstream-dashboard.md) |
| Compiler implementation principles | [src/Jazor.Compiler/ImplementationPrinciples.md](src/Jazor.Compiler/ImplementationPrinciples.md) |
| Compiler status | [docs/03-完成/compiler/status.md](docs/03-%E5%AE%8C%E6%88%90/compiler/status.md) |
| RazorVue design | [docs/01-目标/razorvue/README.md](docs/01-%E7%9B%AE%E6%A0%87/razorvue/README.md) |
| Jolt design | [docs/01-目标/jolt/README.md](docs/01-%E7%9B%AE%E6%A0%87/jolt/README.md) |
| Jolt status | [docs/03-完成/jolt/status.md](docs/03-%E5%AE%8C%E6%88%90/jolt/status.md) |
| Emit status | [docs/03-完成/emit/status.md](docs/03-%E5%AE%8C%E6%88%90/emit/status.md) |

Docs are organized as:

- `docs/01-目标/`: goals and design rationale
- `docs/02-计划/`: plans, milestones, and work breakdowns
- `docs/03-完成/`: status snapshots and review results
- `docs/04-补充/`: governance and supplemental rules
- `docs/05-遗弃/`: retired historical material

Treat `docs/03-完成/compiler/testing/` as historical audit material. For current compiler truth, prefer `src/Jazor.Compiler/ImplementationPrinciples.md`, `docs/03-完成/compiler/status.md`, and the current compiler/test READMEs.

## Contributing

Contributions are welcome. Keep changes scoped, follow the repository conventions, and update the relevant docs/status pages when a workstream boundary or public contract changes.

## License

This project is licensed under the MIT License. See [LICENSE.txt](LICENSE.txt) for details.

## Acknowledgements

- [Roslyn](https://github.com/dotnet/roslyn) — C# compiler platform
- [Acornima](https://github.com/adams85/acornima) — JavaScript parser and AST library
- [WebRef](https://github.com/w3c/webref) — Web specification references
- [DenoHost](https://github.com/thomas3577/DenoHost) — Deno runtime host for .NET
- [WootzJs](https://github.com/kswoll/WootzJs), [h5](https://github.com/curiosity-ai/h5), and [SharpKit](https://github.com/SharpKit/SharpKit) — prior C# to JavaScript compilers

## Security Policy

If you discover a security vulnerability, please report it privately via [GitHub Security Advisories](https://github.com/devhxj/Jazor/security/advisories/new). Do not file public issues for security concerns.

## Feedback

- [Report a bug](https://github.com/devhxj/Jazor/issues/new?template=bug_report.md)
- [Request a feature](https://github.com/devhxj/Jazor/issues/new?template=feature_request.md)
- [Discussions](https://github.com/devhxj/Jazor/discussions)
