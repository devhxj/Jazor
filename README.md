<div align="center">

![Today's Verse](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)
</div>

# Jazor - C# to JavaScript Compiler

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.txt)
[![NuGet](https://img.shields.io/nuget/v/Jazor.svg)](https://www.nuget.org/packages/Jazor)

> Experimental. Public APIs, generated output shape, and tooling are still evolving. The compiler core and ECMAScript module emission are the most stable parts of the project.

Jazor is a Roslyn-based C# to JavaScript compiler centered on `IOperation` to ECMAScript AST lowering. Annotate C# classes with `[ECMAScriptModule]`, get `.mjs` files at build time. Includes typed Vue 3 `h()` render function bindings via `ECMAScript.Vue3`.

## Status

| Tier | Component | Status |
|------|-----------|--------|
| **Working** | Compiler core (SemanticWalker, AstConverter) | Stable — the most mature part |
| **Working** | ECMAScript module emission (`[ECMAScriptModule]` → `.mjs`) | Stable |
| **Working** | ECMAScript.Vue3 bindings (h, ref, reactive, lifecycle, createApp) | Stable |
| **Working** | MSBuild integration (JazorEmit, JazorBundle, JazorOutDir) | Stable |
| **Working** | Jazor.Analyzer (whitelist compile-time validation) | Stable |
| **In progress** | SourceMap | Narrow lane — module-level `.mjs.map`, not full coverage yet |
| **In progress** | Deno bundling | `JazorBundle` target works for basic cases |
| **In progress** | Debugging | Design and milestone code exist, not user-facing yet |
| **Long-term** | RazorVue | Full Razor component framework with Source Generator — [Design](docs/01-目标/razorvue/README.md) |
| **Long-term** | Jolt | Dev toolchain: LSP, HMR, DevServer, debug, build — [Design](docs/01-目标/jolt/README.md) |

Users today should target the **Working** tier. The Long-term items have extensive design documents and milestone code but are not ready for external consumption.

---

## Getting Started

### Install

```
dotnet add package Jazor
```

The package includes runtime (`ECMAScript`, `ECMAScript.Vue3`, `ECMAScript.Vuetify`), compiler (`Jazor.Compiler`), static analyzer (`Jazor.Analyzer`), emit tool (`Jazor.Emit`), and MSBuild props/targets.

### Multi-project Setup

Jazor works best with a multi-project layout where library projects declare modules and a host project emits them.

**Library project** — declares modules, no emit:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <JazorEmit>false</JazorEmit>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Jazor" Version="*" />
  </ItemGroup>
</Project>
```

**Host project** — triggers emit and writes `.mjs` files:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <JazorEmit>true</JazorEmit>
    <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorOutDir>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Jazor" Version="*" />
    <ProjectReference Include="..\MyApp.Features\MyApp.Features.csproj" />
  </ItemGroup>
</Project>
```

The host project scans its own assembly and all referenced assemblies for `[ECMAScriptModule]`-annotated types and emits `.mjs` files to `JazorOutDir`. See the [multi-project sample](samples/Jazor.MultiProject/) for a working example.

### MSBuild Properties

| Property | Default | Description |
|----------|---------|-------------|
| `JazorCompile` | `true` | Enables compilation of `[ECMAScriptModule]` types. |
| `JazorEmit` | `true` for `Exe`, `false` for `Library` | Emits `.mjs` files after build. |
| `JazorOutDir` | `$(IntermediateOutputPath)jazor\$(TargetFramework)\modules\` | Output directory for emitted `.mjs` files. |
| `JazorBundle` | `false` | Bundles all emitted modules into a single JS file (uses bundled Deno runtime). |
| `JazorBundleOut` | `$(OutDir)jazor\app.js` | Output path for the bundled JS file. |
| `JazorCleanEmit` | `true` | Removes stale `.mjs` files from the output directory. |
| `JazorFailOnPathConflict` | `true` | Fails the build if two modules claim the same output path. |

---

## Authoring Modules

### Basic Module

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

Generates `shared/greetings.mjs`:

```javascript
export function prefix() {
  return "Hello";
}
export function compose(name) {
  return `${prefix()}, ${name}`;
}
```

Cross-module imports are resolved automatically — when another module calls `GreetingModule.Compose(name)`, the compiler generates the corresponding `import` statement.

### Vue 3 h() Function Authoring

`ECMAScript.Vue3` provides typed C# bindings for Vue 3's Composition API and `h()` render function:

```csharp
using ECMAScript;
using static ECMAScript.Vue3;

namespace MyApp;

[ECMAScript("npm:vue@3")]
[ECMAScriptModule("app/counter.mjs")]
public static class CounterModule
{
    public static IVueComponent Counter
        => DefineComponent(new VueComponentOptions
        {
            Setup = () =>
            {
                var count = Ref(0);
                return () => H("div", new VueObject { Class = "counter" },
                [
                    H("p", $"Count: {count.Value}"),
                    H("button", new VueObject
                    {
                        Events = new VueDictionary
                        {
                            ["click"] = (Action)(() => count.Value++)
                        }
                    }, "Increment")
                ]);
            }
        });
}
```

### Compilation Capabilities

The compiler supports variable declarations, basic types, pattern matching, nullable types, async/await, string interpolation, object and collection initialization, tuples, deconstruction, switch statements/expressions, and loops (for/foreach/while/do-while). See [Compiler Docs](src/Jazor.Compiler/README.md) for the full feature set.

---

## ECMAScript Attribute Conventions

- `[ECMAScript("npm:vue@3")]` — declares a **runtime import dependency**. The compiler generates `import { ... } from "npm:vue@3"`.
- `[ECMAScript("jsr:@scope/pkg")]` or `[ECMAScript("https://...")]` — Deno-resolvable import addresses.
- `[ECMAScriptModule("features/todo/index.mjs")]` — declares the **output module path** after emission. Not a package resolution address.
- `[Jazor(...)]` — CLR and host mapping producer-side declarations.

---

## Project Structure

```
Jazor/
├── src/
│   ├── ECMAScript/                  # ECMAScript AST core types and attributes
│   ├── ECMAScript.Contract/         # Minimal contract layer (JazorAttribute, Op)
│   ├── ECMAScript.Vue3/             # Vue 3 runtime binding surface
│   ├── ECMAScript.Vuetify/          # Vuetify bindings and component stubs
│   ├── Jazor.Compiler/              # C# → JS compiler core
│   ├── Jazor.Analyzer/              # Static analyzer (whitelist validation)
│   ├── Jazor.CLR/                   # CLR runtime module support
│   ├── Jazor.Emit/                  # Emit pipeline and bundle materialisation
│   ├── Jazor.Common/                # Shared contracts and utilities
│   ├── Jazor/                       # NuGet package (bundles everything above)
│   ├── Jolt/                        # [Long-term] Dev toolchain
│   ├── Wiki/                        # Docs site built with Jazor
│   └── samples/                     # Multi-project usage sample
├── docs/                            # Documentation hub
└── scripts/                         # Build and tooling scripts
```

## Documentation

| Audience | Entry |
|----------|-------|
| **New visitors** | [Docs Hub](docs/README.md) — project overview and navigation |
| **Maintainers** | [Workstream Dashboard](docs/02-计划/workstream-dashboard.md) — resume work entry point |
| **Architecture** | [Compiler Architecture](docs/01-目标/compiler/ArchitectureOverview.Simplified.md) · [ECMAScript.Vue3 Design](docs/01-目标/ecmascript.vue3/README.md) |

Docs are organized into five categories: [Goals](docs/01-目标/README.md) · [Plans](docs/02-计划/README.md) · [Completed](docs/03-完成/README.md) · [Supplements](docs/04-补充/README.md) · [Retired](docs/05-遗弃/README.md)

---

## Development

### Prerequisites
- .NET 10 SDK
- PowerShell 7+ (for test scripts)
- Windows, Linux, or macOS

### Build Steps

```bash
git clone https://github.com/devhxj/Jazor.git
cd Jazor
dotnet restore
dotnet build

# Run all tests
pwsh ./scripts/test-dotnet.ps1

# Run compiler tests only
pwsh ./scripts/test-dotnet.ps1 -Project compiler

# Run a single test class
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest"
```

---

## Contributing

Community contributions are welcome. Please review the repository documentation and follow the conventions described in the codebase before submitting a Pull Request.

## License

This project is licensed under the MIT License. See [LICENSE.txt](LICENSE.txt) for details.

## Acknowledgements

- [Roslyn](https://github.com/dotnet/roslyn) — C# compiler platform
- [Acornima](https://github.com/adams85/acornima) — JavaScript parser and AST library
- [WebRef](https://github.com/w3c/webref) — Web specification references
- [WootzJs](https://github.com/kswoll/WootzJs) · [h5](https://github.com/curiosity-ai/h5) · [SharpKit](https://github.com/SharpKit/SharpKit) — C# to JavaScript compilers
- [DenoHost](https://github.com/thomas3577/DenoHost) — Deno runtime host for .NET
- [CSharpToJavaScript](https://github.com/TiLied/CSharpToJavaScript) — C# to JavaScript transpiler

---

## Security Policy

If you discover a security vulnerability, please report it privately via [GitHub Security Advisories](https://github.com/devhxj/Jazor/security/advisories/new). Do not file public issues for security concerns.

## Feedback

- [Report a bug](https://github.com/devhxj/Jazor/issues/new?template=bug_report.md)
- [Request a feature](https://github.com/devhxj/Jazor/issues/new?template=feature_request.md)
- [Discussions](https://github.com/devhxj/Jazor/discussions)
