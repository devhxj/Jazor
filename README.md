<div align="center">

[![Today's Verse](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)

<h1>Jazor</h1>

<p><strong>Build Vue applications in C# with Jolt and compile C# modules to ECMAScript with Jazor.</strong></p>

<p>
  <a href="https://dotnet.microsoft.com/"><img alt=".NET 11" src="https://img.shields.io/badge/.NET-11-512BD4?logo=dotnet&amp;logoColor=white" /></a>
  <a href="https://www.nuget.org/packages/Jazor"><img alt="NuGet" src="https://img.shields.io/nuget/v/Jazor?logo=nuget&amp;label=NuGet" /></a>
  <a href="https://github.com/devhxj/Jazor/releases/tag/v2.7.2"><img alt="GitHub release" src="https://img.shields.io/github/v/tag/devhxj/Jazor?tag=v2.7.2&amp;display_name=tag&amp;label=release" /></a>
  <a href="LICENSE.txt"><img alt="MIT License" src="https://img.shields.io/badge/license-MIT-2ea44f" /></a>
</p>

<p>
  <a href="docs/04-roadmap/current-status.md"><img alt="At least 10,000 compiler tests pass" src="https://img.shields.io/badge/compiler%20tests-10%2C000%2B%20passing-2ea44f" /></a>
  <a href="docs/04-roadmap/current-status.md"><img alt="At least 98 percent compiler line coverage" src="https://img.shields.io/badge/compiler%20line%20coverage-98%25%2B-2ea44f" /></a>
  <a href="docs/04-roadmap/current-status.md"><img alt="At least 97 percent compiler branch coverage" src="https://img.shields.io/badge/compiler%20branch%20coverage-97%25%2B-2ea44f" /></a>
</p>

<p><a href="docs/03-guides/jolt-authoring.md">Get started with Jolt</a> · <a href="samples/Jolt.Counter/README.md">Counter example</a> · <a href="docs/README.md">Documentation</a> · <a href="CHANGELOG.md">Changelog</a></p>

<p><strong>English</strong> · <a href="README_CN.md">简体中文</a></p>

</div>

## Run a Jolt app

Install the .NET 11 SDK on Windows or glibc Linux (x64 or ARM64). Clone the repository and run these commands from its root.

```sh
dotnet tool install --global Jazor.Jolt --version 2.7.2
jolt dev samples/Jolt.Counter --port 15658
```

If the tool is already installed, use `dotnet tool update --global Jazor.Jolt --version 2.7.2`. Open the URL printed in the terminal and click **Increment**. A typed callback updates the parent’s count and the state of a Reka UI component. [Counter project](samples/Jolt.Counter/README.md)

A `.jazor` file combines markup and C#.

```razor
<button @onclick="@Increment">Count: @Count</button>

@code {
    private int Count;
    private void Increment() => Count++;
}
```

Each `.jazor` file becomes one ES module. The Vue component is the default export. Public static methods and fields and public constants become named exports. Instance state belongs to each mounted component. Jolt checks npm components against their public TypeScript declarations and imports their published ESM exports. [Component authoring](docs/03-guides/jolt-authoring.md)

### Edit and build

| Editor | Extension |
| --- | --- |
| VS Code | Install `jolt-2.7.2.vsix` with **Extensions: Install from VSIX**. [VS Code setup](editors/jolt/README.md) |
| Visual Studio 2026 on Windows x64 | Open `Jolt.VisualStudio.vsix` with the VSIX Installer. [Visual Studio setup](editors/jolt-visual-studio/README.md) |
| JetBrains Rider 2026.3 | Install `jolt-rider-2.7.2.zip` from disk in **Settings → Plugins**. [Rider setup](editors/jolt-rider/README.md) |

Get the CLI and your editor’s extension from the same release. Open `samples/Jolt.Counter` as a folder, then open `App.jazor`. [2.7.2 release](https://github.com/devhxj/Jazor/releases/tag/v2.7.2)

`jolt build samples/Jolt.Counter` writes modules and source maps to `samples/Jolt.Counter/.jolt/`. `jolt bundle samples/Jolt.Counter` writes production assets to `samples/Jolt.Counter/dist/`.

## Compile C# modules

Add the `Jazor` SDK to a .NET project to compile `[ECMAScriptModule]` classes. The SDK includes the compiler, analyzer, Emit tooling and ASP.NET Core integration. [C# module quickstart](docs/03-guides/quick-start.md)

```sh
dotnet add package Jazor --version 2.7.2
```

| Topic | Documentation |
| --- | --- |
| SDK configuration, output modes and ASP.NET Core SSR | [Installation and configuration](docs/03-guides/installation-and-configuration.md) |
| Reusing C# libraries and NuGet packages | [Managed library imports](docs/04-roadmap/ordinary-nuget-compilation-research.md) |
| Compiler and artifact pipeline | [System architecture](docs/01-overview/system-architecture.md) |
| Supported features and test requirements | [Current status](docs/04-roadmap/current-status.md) |

## Develop Jazor

Run from the repository root with the .NET SDK selected by [global.json](global.json).

```sh
dotnet build Jazor.slnx
dotnet run --file scripts/csharp/test-dotnet.cs -- --project jolt
dotnet run --file scripts/csharp/test-dotnet.cs
```

The Jolt release workflow verifies an isolated CLI install, the language server and browser interaction, and builds both VSIX packages. [Development and testing](docs/03-guides/development-and-testing.md)

## Latest release

Jazor 2.7.2 improves Jolt completion in blank markup and HTML start tags, adds npm import suggestions, and enables F12 navigation from `.jazor` to shared C# declarations. It also fixes missing npm inputs in ordinary C# design-time builds. [Release notes](CHANGELOG.md#jazor-272)

## AI-assisted development

The initial implementations of Jazor's core compiler and CLR support, together with the first 500 tests, were written by hand over nearly two years. This work established the project's technical foundation.

AI assistance has primarily helped advance documentation, test coverage, external library bindings, iterative improvements, CI, and version management. The Zhipu GLM-5 and GPT-5 series have been the main AI collaborators in this subsequent work; maintainers continue to guide the project, review changes, and make release decisions.

## Acknowledgements

Jazor builds on [Roslyn](https://github.com/dotnet/roslyn), [Acornima](https://github.com/adams85/acornima), [node-api-dotnet](https://github.com/microsoft/node-api-dotnet), [pnpm](https://github.com/pnpm/pnpm), [Vite](https://github.com/vitejs/vite), and [WebRef](https://github.com/w3c/webref). Earlier work also benefited from Netpack, DenoHost, and C#-to-JavaScript projects including [WootzJs](https://github.com/kswoll/WootzJs), [h5](https://github.com/curiosity-ai/h5), and [SharpKit](https://github.com/SharpKit/SharpKit).

## License and Feedback

Jazor is licensed under the [MIT License](LICENSE.txt). Report security issues privately through [GitHub Security Advisories](https://github.com/devhxj/Jazor/security/advisories/new); use [issues](https://github.com/devhxj/Jazor/issues) and [discussions](https://github.com/devhxj/Jazor/discussions) for other feedback.
