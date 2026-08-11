<div align="center">

![Today's Verse](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)

<h1>Jazor</h1>

<p><strong>A typed .NET toolchain for compiling C# and Razor into deterministic ECMAScript modules and Vue render functions.</strong></p>

<p>
  <a href="https://dotnet.microsoft.com/"><img alt=".NET 11 Preview 6" src="https://img.shields.io/badge/.NET-11%20Preview%206-512BD4?logo=dotnet&amp;logoColor=white" /></a>
  <a href="https://www.nuget.org/packages/Jazor"><img alt="NuGet" src="https://img.shields.io/nuget/v/Jazor?logo=nuget&amp;label=NuGet" /></a>
  <a href="https://github.com/devhxj/Jazor/releases/latest"><img alt="GitHub release" src="https://img.shields.io/github/v/release/devhxj/Jazor?display_name=tag&amp;label=release" /></a>
  <a href="https://github.com/devhxj/Jazor/actions/workflows/razorvue-ci.yml"><img alt="RazorVue CI" src="https://github.com/devhxj/Jazor/actions/workflows/razorvue-ci.yml/badge.svg?branch=main" /></a>
  <a href="LICENSE.txt"><img alt="MIT License" src="https://img.shields.io/badge/license-MIT-2ea44f" /></a>
</p>

<p>
  <a href="src/Jazor.CompilerTest/README.md"><img alt="10297 compiler tests passed" src="https://img.shields.io/badge/compiler%20tests-10297%20passed-2ea44f" /></a>
  <a href="docs/03-%E5%AE%8C%E6%88%90/compiler/status.md"><img alt="98.94 percent compiler line coverage" src="https://img.shields.io/badge/compiler%20line%20coverage-98.94%25-2ea44f" /></a>
  <a href="docs/03-%E5%AE%8C%E6%88%90/compiler/status.md"><img alt="96.01 percent compiler branch coverage" src="https://img.shields.io/badge/compiler%20branch%20coverage-96.01%25-2ea44f" /></a>
</p>

<p><strong>English</strong> · <a href="README_CN.md">简体中文</a></p>

</div>

> [!IMPORTANT]
> Jazor remains experimental. Public APIs, generated artifact shape, and tooling may evolve; the compiler core, emit pipeline, and Razor SG binding boundary are the most stable foundations.

Jazor is a .NET toolchain for building JavaScript and Vue applications with C# and Razor.

The core package provides the compiler, runtime contracts, analyzer, emit tool, and MSBuild integration. Razor-to-Vue transformation is an explicit `Jazor.Vue` opt-in: official Razor source-generator output is bound as Roslyn `IOperation` and lowered to Vue render-function `.mjs` artifacts.

The implementation is composed from `Jazor.Compiler`, `Jazor.CLR`, `Jazor.Analyzer`, `Jazor.Emit`, `Jazor.Common`, and the ECMAScript/Vue binding assemblies.

## Acknowledgements

Jazor draws on these projects and the work of their maintainers:

- [Roslyn](https://github.com/dotnet/roslyn) - C# compiler platform
- [Esprima .NET](https://github.com/sebastienros/esprima-dotnet) - ECMAScript parser for .NET
- [Acornima](https://github.com/adams85/acornima) - ECMAScript parser and ESTree library for .NET
- [Netpack](https://github.com/FlorianRappl/netpack) - JavaScript module bundling support
- [DenoHost](https://github.com/thomas3577/DenoHost) - Deno runtime host for .NET
- [WebRef](https://github.com/w3c/webref) - Web specification references
- [WootzJs](https://github.com/kswoll/WootzJs), [h5](https://github.com/curiosity-ai/h5), and [SharpKit](https://github.com/SharpKit/SharpKit) - prior C# to JavaScript compilers

## Verified Compiler Baseline

The compiler and RazorVue paths are covered by repeatable regression, coverage, and runtime gates. Current results, thresholds, and reproduction commands live in the [compiler status](docs/03-%E5%AE%8C%E6%88%90/compiler/status.md), [compiler test guide](src/Jazor.CompilerTest/README.md), and [RazorVue integration guide](src/Jazor.RazorVue/README.md).

## Architecture

The ownership and data flow are:

```mermaid
flowchart LR
    subgraph Authoring["Authoring"]
        CSharp["C# modules"]
        Razor["Razor components"]
    end

    subgraph Roslyn["Roslyn semantic boundary"]
        SG["Official Razor SG"]
        Compilation["Final Compilation<br/>SemanticModel + IOperation"]
        Analyzer["Jazor.Analyzer<br/>compile-time diagnostics"]
    end

    subgraph Lowering["Jazor lowering"]
        Compiler["Jazor.Compiler<br/>AstConverter + SemanticWalker"]
        RazorVue["Jazor.RazorVue<br/>BuildRenderTree binding + Vue framing"]
        Bindings["Jazor.CLR + ECMAScript/Vue bindings<br/>whitelist mappings"]
        ESTree["Acornima ESTree"]
    end

    subgraph Artifacts["Artifact delivery"]
        Emit["Jazor.Emit"]
        Modules[".mjs modules + source maps"]
        Catalog["manifest + runtime assets"]
        Bundle["production bundle"]
    end

    CSharp --> Compilation
    Razor --> SG --> Compilation
    Compilation -. validation .-> Analyzer
    Compilation --> Compiler
    Compilation -. BuildRenderTree binding .-> RazorVue
    RazorVue -. compiler translation hooks .-> Compiler
    Bindings --> Compiler
    Compiler --> ESTree --> Emit
    RazorVue --> Emit
    Emit --> Modules
    Emit --> Catalog
    Emit --> Bundle

    classDef authoring fill:#EFF6FF,stroke:#3B82F6,color:#1D4ED8,stroke-width:1.5px
    classDef semantic fill:#ECFEFF,stroke:#0F766E,color:#134E4A,stroke-width:1.5px
    classDef lowering fill:#F0FDF4,stroke:#16A34A,color:#14532D,stroke-width:1.5px
    classDef artifact fill:#FFF7ED,stroke:#EA580C,color:#9A3412,stroke-width:1.5px
    class CSharp,Razor authoring
    class SG,Compilation,Analyzer semantic
    class Compiler,RazorVue,Bindings,ESTree lowering
    class Emit,Modules,Catalog,Bundle artifact
```

- **One semantic boundary**: C# modules and official Razor SG output are both bound from the final Roslyn `Compilation`.
- **Compiler-owned lowering**: `Jazor.Compiler` translates C# member and expression semantics to Acornima ESTree; host bindings are supplied through generated whitelist mappings.
- **RazorVue framing**: `Jazor.RazorVue` binds generated `BuildRenderTree` operations and uses compiler translation hooks to form Vue render-function modules.
- **Artifact delivery**: `Jazor.Emit` materializes modules, source maps, manifests, runtime assets, and production bundles.

## Capabilities

- **Semantic C# lowering**: C# is lowered through Roslyn `IOperation`, not syntax-string rewriting.
- **Fail-fast host boundary**: unsupported external runtime semantics are rejected at the actual lowering site instead of silently emitting raw JavaScript approximations.
- **Whitelist-gated CLR surface**: common CLR APIs are mapped through `Jazor.CLR` and generated whitelist metadata; analyzer diagnostics catch many unsupported usages early.
- **ECMAScript module output**: `[ECMAScriptModule]` classes emit named-export `.mjs` modules with stable import collection, source-origin tracking, and source-map carriers.
- **RazorVue artifact generation**: Razor component semantics flow from official Razor SG generated C# through Roslyn binding and compiler-owned `IOperation` lowering.
- **Typed Vue authoring**: `ECMAScript.Vue3` provides typed bindings for Vue 3 `defineComponent`, `h`, refs/reactivity, lifecycle hooks, props, slots, and component contracts.
- **Host-facing build support**: MSBuild selects one output mode for ECMAScript/RazorVue artifacts: no output, debug modules and manifest, or a Netpack production bundle.

## Latest Updates

### 2026-08-11

- Generated RazorVue components now register with Vue's development HMR runtime. Compiler-proven template-only changes reload the component in place while preserving parent state; unsafe or unavailable update paths continue to fall back to a full-page reload.
- RazorVue debug output now links each generated `.mjs` module to an external source map containing the authored Razor text, so browser DevTools can trace render-function code back to `.razor` source without a separate source-file route.
- ASP.NET Core applications can now opt into Vue SSR with local server rendering and browser hydration. SSR artifacts carry their Vue runtime resources without application `node_modules`, a CDN, or remote imports; DenoHost executes SSR while Netpack owns browser bundling.
- Pinia sample applications now preserve authored C# member names and explicitly declare Pinia's lowercase protocol keys where required, keeping generated browser modules and their test workflow aligned with the naming contract.
- Current Pinia and Vue Router samples now verify Netpack release bundles separately from DenoHost-backed runtime smoke coverage. Retired generated-SFC/Deno-bundle sample fixtures are no longer part of the active tree.

See [release notes](docs/releases/release-notes.md) for the full history.

## Install

```bash
dotnet add package Jazor --version 0.8.3
```

The `Jazor` package includes the core runtime contracts, `ECMAScript`, `ECMAScript.Vue3`, `ECMAScript.VueContract`, `Jazor.Compiler`, `Jazor.Analyzer`, ASP.NET Core integration assemblies, the emit tool, and MSBuild props/targets. Razor-to-Vue generation is supplied by the separate `Jazor.Vue` package.

Razor SDK projects opt in explicitly:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.8.3" />
  <PackageReference Include="Jazor.Vue" Version="0.8.3" PrivateAssets="all" />
</ItemGroup>
```

Add ecosystem packages explicitly when needed:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.8.3" />
  <PackageReference Include="ECMAScript.Style" Version="0.8.3" />
  <PackageReference Include="ECMAScript.Pinia" Version="0.8.3" />
  <PackageReference Include="ECMAScript.VueRoute" Version="0.8.3" />
  <PackageReference Include="ECMAScript.Vuetify" Version="0.8.3" />
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

The resulting `shared/greetings.mjs` is a named-export ECMAScript module:

```js
export function prefix() {
  return "Hello";
}
export function compose(name) {
  return `${prefix()}, ${name}`;
}
//# sourceMappingURL=greetings.mjs.map
```

When another module calls `GreetingModule.Compose(...)`, the compiler resolves the corresponding cross-module import automatically.

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

### Razor-to-Vue Components

Razor components use the final Roslyn compilation as their only production input:

- Reference `Jazor.Vue` from the project that declares `.razor` or `.razor.cs` components.
- The integration binds generated `BuildRenderTree` operations from the completed Razor source-generator compilation.
- `Jazor.Compiler` lowers the bound semantics and `Jazor.Emit` materializes Vue render-function artifacts.
- No `EnableRazorHostOutputs`, Razor host-output setting, Razor IR/document model, or generated-C# reparse is required.

See the [Razor-to-Vue design](docs/01-%E7%9B%AE%E6%A0%87/razorvue/README.md) for implementation details.

### Deterministic CSS-in-JS

Reference `ECMAScript.Style` when an application needs structured runtime styles:

```csharp
using ECMAScript.Style;
using static ECMAScript.Style.css;

var actionClass = style(new CssRule
{
    Display = inlineFlex,
    Gap = rem(0.5),
    Width = percent(100) - rem(2),
    Color = varOr("--action-color", color("white")),
    BackgroundColor = hex("1769aa"),
    Border = px(1) | solid | var("--action-border"),
    BackdropFilter = filters(blur(px(12)), saturate(1.15)),
    Children =
    [
        new(CssChildKind.Selector, "&:hover", new CssRule
        {
            BackgroundColor = hex("125486")
        })
    ]
});
```

The package generates 705 standard properties from a locked Webref grammar snapshot. Native C# unions distinguish lengths, percentages, colors, times, display values, border shorthand, filters, and other domains; for example, `px(1) | solid | var("--border")` yields a value restricted to border properties. `raw(...)` explicitly admits only the remaining future or unmodeled syntax. Stable content names, nonce-aware `document`/`ShadowRoot` ownership, detached extraction, and hydration share one runtime contract. `style(...)` returns a plain string for ordinary modules and RazorVue `class` attributes. The package adds no CSS-specific MSBuild property; debug materialization writes `style.mjs` under `JazorDir`.

See the [ECMAScript.Style package guide](src/ECMAScript.Style/README.md) and [design boundary](docs/01-%E7%9B%AE%E6%A0%87/ecmascript.style/README.md).

## MSBuild Properties

No output configuration is required for class libraries because `JazorMode` defaults to `none`.

For development builds, configure debug artifacts as follows:

```xml
<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorDir>
</PropertyGroup>
```

For production delivery, configure a release bundle as follows:

```xml
<PropertyGroup>
  <JazorMode>release</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorDir>
</PropertyGroup>
```

`debug` and `release` are mutually exclusive. A release build performs intermediate materialization internally, clears `JazorDir`, and writes only `bundle.js` and `bundle.js.map` to that directory.

| Property | Default | Description |
|----------|---------|-------------|
| `JazorMode` | `none` | `none` writes nothing; `debug` writes modules and a manifest; `release` writes a production bundle. |
| `JazorDir` | `$(MSBuildProjectDirectory)\wwwroot\jazor\` | Output root for debug modules or the release bundle. |

See [src/Jazor/README.md](src/Jazor/README.md) and [src/Jazor.Emit/README.md](src/Jazor.Emit/README.md) for package and emit details.

## Repository Layout

```text
Jazor/
├── src/
│   ├── Jazor.Compiler/              # C# -> JavaScript compiler core
│   ├── Jazor.CLR/                   # CLR runtime mappings and JavaScript helpers
│   ├── Jazor.Analyzer/              # Static analyzer diagnostics
│   ├── Jazor.RazorVue/              # Generator integration, SG binding, and Vue render framing
│   ├── Jazor.Emit/                  # Materialization, manifests, source maps, and bundling
│   ├── Jazor.Admin/                 # UI-neutral admin shell contracts and Razor components
│   ├── ECMAScript.Style/            # Strongly typed, deterministic CSS-in-JS runtime
│   ├── Jazor.Common/                # Shared formatting/source-map utilities and contracts
│   ├── Jazor.AspNetCore*/           # ASP.NET Core runtime and dev integration
│   ├── Jazor/                       # NuGet package bundling core SDK assets
│   ├── Jazor.Vue/                   # Opt-in Razor-to-Vue NuGet package
│   ├── ECMAScript*/                 # ECMAScript AST/contracts plus Vue ecosystem bindings
│   └── *Test/                       # MSTest regression projects
├── samples/
│   ├── Jazor.MultiProject/          # Baseline multi-project module emission
│   ├── JazorAdmin/                  # Formal administration product and package-consumer smoke
│   ├── ECMAScript.Pinia.Counter/    # Vue 3 + Pinia sample
│   └── ECMAScript.VueRoute.MemorySmoke/ # Vue Router module and browser-runtime sample
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
dotnet run --file scripts/csharp/verify-compiler-coverage.cs
dotnet run --file scripts/csharp/test-dotnet.cs -- --project style
dotnet run --file scripts/csharp/test-dotnet.cs -- --project style-browser
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj

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
| ECMAScript.Style design and status | [docs/01-目标/ecmascript.style/README.md](docs/01-%E7%9B%AE%E6%A0%87/ecmascript.style/README.md), [docs/03-完成/ecmascript.style/status.md](docs/03-%E5%AE%8C%E6%88%90/ecmascript.style/status.md) |
| Transformation plan | [docs/02-计划/Jazor 架构转型开发计划.md](docs/02-%E8%AE%A1%E5%88%92/Jazor%20%E6%9E%B6%E6%9E%84%E8%BD%AC%E5%9E%8B%E5%BC%80%E5%8F%91%E8%AE%A1%E5%88%92.md) |
| G0 decision record | [docs/02-计划/RazorSgFinalDocument.G0.DecisionRecord.md](docs/02-%E8%AE%A1%E5%88%92/RazorSgFinalDocument.G0.DecisionRecord.md) |
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

## Security Policy

If you discover a security vulnerability, please report it privately via [GitHub Security Advisories](https://github.com/devhxj/Jazor/security/advisories/new). Do not file public issues for security concerns.

## Feedback

- [Report a bug](https://github.com/devhxj/Jazor/issues/new?template=bug_report.md)
- [Request a feature](https://github.com/devhxj/Jazor/issues/new?template=feature_request.md)
- [Discussions](https://github.com/devhxj/Jazor/discussions)
