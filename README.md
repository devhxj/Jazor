**English** | [中文](README_CN.md)

<div align="center">

![Today's Verse](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)
</div>

# Jazor - C# to JavaScript Compiler

[![.NET](https://img.shields.io/badge/.NET-11.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.txt)
[![NuGet](https://img.shields.io/nuget/v/Jazor.svg)](https://www.nuget.org/packages/Jazor)

> Experimental. Public APIs, generated output shape, and tooling are still evolving. The compiler core and ECMAScript module emission are the most stable parts of the project.

Jazor is a Roslyn-based C# to JavaScript compiler centered on `IOperation` to ECMAScript AST lowering. Annotate C# classes with `[ECMAScriptModule]`, get `.mjs` files at build time. Includes typed Vue 3 `h()` render function bindings via `ECMAScript.Vue3`, Pinia store bindings via `ECMAScript.Pinia`, and Vue Router bindings via `ECMAScript.VueRoute`.

## Features

- **C# syntax coverage** — supports virtually all C# 15 language features: variable declarations, basic types, pattern matching, nullable types, async/await, string interpolation, object and collection initialization, tuples, deconstruction, switch statements/expressions, and loops (for/foreach/while/do-while)
- **Roslyn `IOperation` lowering** — semantic-driven compilation via `IOperation` → ECMAScript AST, not syntax-level translation
- **Static analysis safety** — `Jazor.Analyzer` enforces whitelist boundaries at compile time, diagnosing unsupported types and members before emission
- **CLR runtime modules** — common BCL types (`string`, `int`, `double`, `List<T>`, `Dictionary<TKey, TValue>`, `Task<T>`, etc.) are mapped to JavaScript runtime implementations through `Jazor.CLR`
- **ECMAScript module emission** — `[ECMAScriptModule]` annotated classes compile to `.mjs` files with automatic cross-module import resolution and source maps
- **Vue 3 integration** — `ECMAScript.Vue3` provides typed C# bindings for Vue 3's Composition API and `h()` render function, enabling component authoring in pure C#
- **Pinia integration** — `ECMAScript.Pinia` provides typed C# bindings for Pinia root/store authoring, including `createPinia()`, `defineStore()`, `storeToRefs()`, and common Options API helpers such as `mapState()` / `mapActions()`
- **Vue Router integration** — `ECMAScript.VueRoute` provides typed C# bindings for Vue Router 4 authoring, including `createRouter()`, history creators, `useRouter()`, `useRoute()`, `RouterLink`, and `RouterView`
- **Vuetify integration** — `ECMAScript.Vuetify` provides production-grade typed C# bindings for Vuetify 3 component authoring, with strongly typed props, named slots, event bindings, and full runtime export coverage
- **MSBuild integration** — emit, bundle, and output path configuration through standard MSBuild properties

## Status

<table>
<tr><th nowrap>Tier</th><th>Component</th><th>Status</th></tr>
<tr><td nowrap><strong>Working</strong></td><td>Compiler core (SemanticWalker, AstConverter) — 1,891 tests</td><td>Stable — the most mature part</td></tr>
<tr><td nowrap><strong>Working</strong></td><td>ECMAScript module emission (<code>[ECMAScriptModule]</code> → <code>.mjs</code>) — 111 tests</td><td>Stable</td></tr>
<tr><td nowrap><strong>Working</strong></td><td>ECMAScript.Vue3 bindings (h, ref, reactive, lifecycle, createApp)</td><td>Stable</td></tr>
<tr><td nowrap><strong>Working</strong></td><td>MSBuild integration (JazorEmit, JazorBundle, JazorOutDir)</td><td>Stable</td></tr>
<tr><td nowrap><strong>Working</strong></td><td>Jazor.Analyzer (whitelist compile-time validation)</td><td>Stable</td></tr>
<tr><td nowrap><strong>Working</strong></td><td>ECMAScript.Vuetify bindings (117 components, typed props/slots/events) — 117 components</td><td>Stable — production-grade Vuetify 3 authoring</td></tr>
<tr><td nowrap><strong>Working</strong></td><td>CLR runtime modules — 70 tests</td><td>Stable</td></tr>
<tr><td nowrap><strong>In progress</strong></td><td>RazorVue (SFC pipeline, canonical h() model, Vuetify production coverage) — 772 tests</td><td>Production-grade Vuetify authoring, pipeline validation, slot/fallthrough support</td></tr>
<tr><td nowrap><strong>In progress</strong></td><td>ECMAScript.Pinia + ECMAScript.VueRoute — 205 tests</td><td>Expanding API coverage, Pinia Testing utilities</td></tr>
<tr><td nowrap><strong>In progress</strong></td><td>SourceMap</td><td>Narrow lane — module-level <code>.mjs.map</code>, not full coverage yet</td></tr>
<tr><td nowrap><strong>In progress</strong></td><td>Deno bundling</td><td><code>JazorBundle</code> target works for basic cases</td></tr>
<tr><td nowrap><strong>In progress</strong></td><td>Debugging</td><td>Design and milestone code exist, not user-facing yet</td></tr>
<tr><td nowrap><strong>Long-term</strong></td><td>Jolt (LSP, HMR, DevServer, debug, build) — 778 tests</td><td>Phase 1–6 closing, <a href="docs/01-目标/jolt/README.md">Design</a></td></tr>
</table>

Users today should target the **Working** tier. 32 projects · ~290K production LoC · ~190K test LoC · ~3,850+ total tests.

---

## Getting Started

### Install

```
dotnet add package Jazor
```

The package includes runtime (`ECMAScript`, `ECMAScript.Vue3`, `ECMAScript.Pinia`, `ECMAScript.VueRoute`, `ECMAScript.Vuetify`), compiler (`Jazor.Compiler`), static analyzer (`Jazor.Analyzer`), emit tool (`Jazor.Emit`), and MSBuild props/targets.

### Multi-project Setup

Jazor works best with a multi-project layout where library projects declare modules and a host project emits them.

**Library project** — declares modules, no emit:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
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
    <TargetFramework>net11.0</TargetFramework>
    <JazorEmit>true</JazorEmit>
    <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorOutDir>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Jazor" Version="*" />
    <ProjectReference Include="..\MyApp.Features\MyApp.Features.csproj" />
  </ItemGroup>
</Project>
```

The host project scans its own assembly and all referenced assemblies for `[ECMAScriptModule]`-annotated types and emits `.mjs` files to `JazorOutDir`. See the [multi-project sample](samples/Jazor.MultiProject/) for the baseline emit layout, and [ECMAScript.Pinia.Counter](samples/ECMAScript.Pinia.Counter/) for a Vue 3 + Pinia consumption sample.

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
│   ├── ECMAScript.Pinia/            # Pinia runtime binding surface
│   ├── ECMAScript.Pinia.Test/       # Pinia binding tests
│   ├── ECMAScript.Vue3/             # Vue 3 runtime binding surface
│   ├── ECMAScript.VueRoute/         # Vue Router runtime binding surface
│   ├── ECMAScript.VueRoute.Test/    # Vue Router binding tests
│   ├── ECMAScript.Vuetify/          # Vuetify 3 production-grade component bindings
│   ├── Jazor.Compiler/              # C# → JS compiler core
│   ├── Jazor.Analyzer/              # Static analyzer (whitelist validation)
│   ├── Jazor.CLR/                   # CLR runtime module support
│   ├── Jazor.Emit/                  # Emit pipeline and bundle materialisation
│   ├── Jazor.Common/                # Shared contracts and utilities
│   ├── Jazor/                       # NuGet package (bundles everything above)
│   ├── Jolt/                        # [Long-term] Dev toolchain
│   ├── Wiki/                        # Docs site built with Jazor
│   └── samples/                     # Working host/consumer samples
├── docs/                            # Documentation hub
└── scripts/                         # Build and tooling scripts
```

## Documentation

| Audience | Entry |
|----------|-------|
| **New visitors** | [Docs Hub](docs/README.md) — project overview and navigation |
| **Maintainers** | [Workstream Dashboard](docs/02-计划/workstream-dashboard.md) — resume work entry point |
| **Architecture** | [Compiler Architecture](docs/01-目标/compiler/ArchitectureOverview.Simplified.md) · [ECMAScript.Vue3 Design](docs/01-目标/ecmascript.vue3/README.md) · [ECMAScript.Pinia Design](docs/01-目标/ecmascript.pinia/README.md) · [ECMAScript.VueRoute Design](docs/01-目标/ecmascript.vueroute/README.md) |

Docs are organized into five categories: [Goals](docs/01-目标/README.md) · [Plans](docs/02-计划/README.md) · [Completed](docs/03-完成/README.md) · [Supplements](docs/04-补充/README.md) · [Retired](docs/05-遗弃/README.md)

---

## Development

### Prerequisites
- .NET 11 SDK (preview)
- PowerShell 7+ (for test scripts)
- Windows, Linux, or macOS

### Build Steps

```bash
git clone https://github.com/devhxj/Jazor.git
cd Jazor
dotnet restore
dotnet build

# Run all tests
dotnet run --file ./scripts/csharp/test-dotnet.cs

# Run compiler tests only
dotnet run --file ./scripts/csharp/test-dotnet.cs -- --project compiler

# Run Pinia binding tests only
dotnet run --file ./scripts/csharp/test-dotnet.cs -- --project pinia

# Run Vue Router binding tests only
dotnet run --file ./scripts/csharp/test-dotnet.cs -- --project vueroute

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
