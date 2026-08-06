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

## Verified Compiler Baseline

`Jazor.Compiler` is validated against genuine Roslyn `IOperation` graphs and emits JavaScript through Acornima ESTree. The current reproducible baseline, recorded on 2026-08-05, is:

| Metric | Verified result | Active enforced minimum |
|--------|-----------------|-------------------------|
| Compiler regression tests | 10297 / 10297 passed | 10000 passed |
| Line coverage | 16369 / 16545 (98.94%) | 98% |
| Branch coverage | 6324 / 6587 (96.01%) | 96% |

Run the authoritative coverage gate from the repository root:

```bash
dotnet run --file scripts/csharp/verify-compiler-coverage.cs
```

The gate runs the complete compiler suite, reads the resulting TRX and Cobertura reports, and exits with a nonzero status when the test-count or coverage thresholds are not met. The 96.01% result is the verified `v0.1.45` release baseline and satisfies the active 96% branch gate. See the [compiler status](docs/03-%E5%AE%8C%E6%88%90/compiler/status.md) and [compiler test guide](src/Jazor.CompilerTest/README.md) for the current scope and methodology.

## RazorVue Verification Baseline

Official Razor SG output is tested through Roslyn binding, direct Vue render-function generation, source maps, catalog output, and Deno.host runtime scenarios. The current reproducible baseline is:

| Metric | Verified result | Enforced minimum |
|--------|-----------------|------------------|
| RazorVue SG scenarios | 4484 / 4484 passed | 4000 passed |
| Line coverage | 8147 / 8719 (93.44%) | 90% |
| Branch coverage | 3568 / 4265 (83.66%) | 80% |

Run the RazorVue coverage gate from the repository root:

```bash
dotnet run --file scripts/csharp/verify-razorvue-coverage.cs
```

## Architecture

- **Semantic lowering**: Roslyn `IOperation` is translated to Acornima ESTree with explicit support boundaries and deterministic output.
- **Razor integration**: `Jazor.Vue` receives the final `Compilation` from `GeneratorDriver.RunGeneratorsAndUpdateCompilation` and binds generated `BuildRenderTree` operations. Razor DR/IR, host-output documents, and generated-C# reparsing are not part of the production boundary.
- **Artifact contract**: Razor components produce Vue render-function `.mjs` modules. `Jazor.Emit` materializes modules, source maps, manifests, runtime assets, and production bundles.
- **Typed bindings**: Vue 3 bindings are included with `Jazor`; Pinia, Vue Router, Vuetify, and other ecosystem bindings are independently referenced packages.

## Capabilities

- **Semantic C# lowering**: C# is lowered through Roslyn `IOperation`, not syntax-string rewriting.
- **Fail-fast host boundary**: unsupported external runtime semantics are rejected at the actual lowering site instead of silently emitting raw JavaScript approximations.
- **Whitelist-gated CLR surface**: common CLR APIs are mapped through `Jazor.CLR` and generated whitelist metadata; analyzer diagnostics catch many unsupported usages early.
- **ECMAScript module output**: `[ECMAScriptModule]` classes emit named-export `.mjs` modules with stable import collection, source-origin tracking, and source-map carriers.
- **RazorVue artifact generation**: Razor component semantics flow from official Razor SG generated C# through Roslyn binding and compiler-owned `IOperation` lowering.
- **Typed Vue authoring**: `ECMAScript.Vue3` provides typed bindings for Vue 3 `defineComponent`, `h`, refs/reactivity, lifecycle hooks, props, slots, and component contracts.
- **Host-facing build support**: MSBuild selects one output mode for ECMAScript/RazorVue artifacts: no output, debug modules and manifest, or a production bundle through the Deno or Netpack lane.

## Latest Updates

### 2026-08-07

- JazorAdmin now provides OpenIddict application, scope, authorization, and token management with public/confidential and interactive/machine/API profiles, one-time secret rotation, Consent/PKCE, endpoint/grant/response permissions, API resource audiences, and authorization/token revocation. Its local-package browser smoke covers those management flows and mobile editor containment.
- JazorAdmin now separates its SSO center from a typed configuration center and a Quartz.NET task center. Administrators can manage `text`, `boolean`, `number`, and `json` settings, schedule catalogued operational tasks with Cron expressions, pause or manually run them, and inspect execution history without exposing arbitrary script execution.

See [release notes](docs/releases/release-notes.md) for the full history.

## Install

```bash
dotnet add package Jazor --version 0.3.2
```

The `Jazor` package includes the core runtime contracts, `ECMAScript`, `ECMAScript.Vue3`, `ECMAScript.VueContract`, `Jazor.Compiler`, `Jazor.Analyzer`, ASP.NET Core integration assemblies, the emit tool, and MSBuild props/targets. Razor-to-Vue generation is supplied by the separate `Jazor.Vue` package.

Razor SDK projects opt in explicitly:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.3.2" />
  <PackageReference Include="Jazor.Vue" Version="0.3.2" PrivateAssets="all" />
</ItemGroup>
```

Add ecosystem packages explicitly when needed:

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.3.2" />
  <PackageReference Include="ECMAScript.Style" Version="0.3.2" />
  <PackageReference Include="ECMAScript.Pinia" Version="0.3.2" />
  <PackageReference Include="ECMAScript.VueRoute" Version="0.3.2" />
  <PackageReference Include="ECMAScript.Vuetify" Version="0.3.2" />
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
    Children =
    [
        new(CssChildKind.Selector, "&:hover", new CssRule
        {
            BackgroundColor = hex("125486")
        })
    ]
});
```

The package generates 705 standard properties from a locked Webref grammar snapshot. Native C# unions distinguish lengths, percentages, colors, times, display values, and other domains; `raw(...)` explicitly admits future or unmodeled syntax. Stable content names, nonce-aware `document`/`ShadowRoot` ownership, detached extraction, and hydration share one runtime contract. `style(...)` returns a plain string for ordinary modules and RazorVue `class` attributes. The package adds no CSS-specific MSBuild property; debug materialization writes `style.mjs` under `JazorDir`.

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
  <JazorTool>Deno</JazorTool>
</PropertyGroup>
```

`debug` and `release` are mutually exclusive. A release build performs intermediate materialization internally, clears `JazorDir`, and writes only `bundle.js` and `bundle.js.map` to that directory.

| Property | Default | Description |
|----------|---------|-------------|
| `JazorMode` | `none` | `none` writes nothing; `debug` writes modules and a manifest; `release` writes a production bundle. |
| `JazorDir` | `$(MSBuildProjectDirectory)\wwwroot\jazor\` | Output root for debug modules or the release bundle. |
| `JazorTool` | `Deno` | Selects the release tool lane, currently `Deno` or `Netpack`. |

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
│   └── RazorVue.TodoList/           # Legacy sample pending transformation
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
