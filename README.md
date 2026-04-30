<div align="center">

![Today's Verse](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)
</div>

# Jazor - C# to JavaScript Compiler and `.jazor` Tooling

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.txt)

> Experimental. Public APIs, generated output shape, and adjacent tooling are still being tightened.

Jazor is a Roslyn-based C# → JavaScript toolchain centered on `IOperation → ECMAScript AST` lowering. The repository has two active technical lines: compile-time library mode via `RazorVue`, and the `.jazor` development host `Jolt`.

## Two Technical Lines

| Line | Mode | Description |
|------|------|-------------|
| **RazorVue** | Library mode | Source Generator–driven, no `.vue` SFC, compiles Razor directly to JS/TS modules at build time |
| **Jolt** | Full-featured host | Vite-like dev host for `.jazor` + `.vue` SFC, with LSP + DevServer/HMR + Debug + Build |

Both lines share the same compiler infrastructure (SemanticWalker, WhiteList, AstConverter).

## Documentation

| Audience | Entry |
|----------|-------|
| **New visitors** | [Docs Hub](docs/README.md) — project overview and navigation |
| **Maintainers** | [Workstream Dashboard](docs/02-计划/workstream-dashboard.md) — resume work entry point |
| **Architecture** | [Compiler Architecture](docs/01-目标/compiler/ArchitectureOverview.Simplified.md) · [Jolt Design](docs/01-目标/jolt/README.md) · [RazorVue Design](docs/01-目标/razorvue/README.md) |

Docs are organized into five categories: [Goals](docs/01-目标/README.md) · [Plans](docs/02-计划/README.md) · [Completed](docs/03-完成/README.md) · [Supplements](docs/04-补充/README.md) · [Retired](docs/05-遗弃/README.md)

## Project Status

See [Workstream Dashboard](docs/02-计划/workstream-dashboard.md) for details.

- ✅ **Compiler core** — nearing stable, the most mature part of the repo
- 🔄 **Jolt** — Phase 1–6 wrapping up, Phase 7 extension system in planning
- 🔄 **Emit / Materialisation** — ongoing, output and bundling pipeline
- 🔄 **SourceMap** — narrow lane, supporting Jolt / Deno materialisation

## Project Structure

```
Jazor/
├── src/
│   ├── ECMAScript/                         # ECMAScript AST core types and attributes
│   ├── ECMAScript.Contract/                # Zero-dependency minimal contract layer (JazorAttribute, Op, IUIComponent)
│   ├── ECMAScript.Vue/                     # Vue 3 runtime projection types and bindings
│   ├── ECMAScript.Vuetify/                 # Vuetify bindings and RazorVue component stubs
│   ├── ECMAScript.WebIDL.Generator/        # WebIDL binding generator (.NET)
│   ├── ECMAScript.WebIDL.GeneratorTest/    # WebIDL generator tests (MSTest)
│   ├── Jazor.Compiler/                     # C# → JS compiler core (SemanticWalker partial files)
│   ├── Jazor.Compiler.Generator/           # Whitelist Source Generator
│   ├── Jazor.Compiler.Razor/               # Compiler Razor integration layer
│   ├── Jazor.CompilerTest/                 # Compiler + Jolt + LSP tests (MSTest)
│   ├── Jazor.CLR/                          # CLR runtime support (whitelist declarations + JS implementations)
│   ├── Jazor.CLR.Generator/                # CLR type mapping and binding code generator
│   ├── Jazor.CLR.Test/                     # CLR tests (MSTest)
│   ├── Jazor.Common/                       # Shared contracts, Format, SourceMap, RazorVue shared semantics
│   ├── Jazor.Emit/                         # Emit pipeline, bundle materialisation, SourceMap output
│   ├── Jazor.EmitTest/                     # Emit tests (MSTest)
│   ├── Jazor.Analyzer/                     # Static analyzer (whitelist compile-time validation)
│   ├── Jazor.RazorVue.Test/                # RazorVue tests (MSTest)
│   ├── Jolt/                               # [Jolt] LSP + DevServer + HMR + Debug + Build
│   ├── Jolt.Test/                          # Jolt tests (MSTest)
│   ├── Jolt.VSCodeExtension/               # VS Code Language Client extension
│   ├── Wiki/                               # Wiki sample application
│   └── Jazor/                              # NuGet package (runtime + analyzer + generators + MSBuild)
├── samples/                                # Sample projects
├── docs/                                   # Documentation hub
├── scripts/                                # Build and tooling scripts
└── README.md
```

## Core Components

- **Jazor.Compiler** — C# to JavaScript compiler core [→ Docs](src/Jazor.Compiler/README.md)
- **Jazor.Analyzer** — Static analysis and whitelist validation for compile-time type safety
- **Jazor.CLR** — .NET type runtime module support, providing JavaScript runtime implementations
- **Jazor.Emit** — Output and bundling pipeline for host-facing artifacts [→ Docs](src/Jazor.Emit/README.md)
- **RazorVue** — Vue-oriented Razor compilation path, Blazor-style component authoring [→ Design](docs/01-目标/razorvue/README.md)
- **Jolt** — `.jazor` full-featured development boundary: LSP + DevServer + HMR + Debug + Build [→ Design](docs/01-目标/jolt/README.md) [→ Status](docs/03-完成/jolt/status.md)

`.jazor` uses Razor as its source syntax; Vue-related artifacts serve only as internal projections or bridge artifacts.

## Capabilities

Jazor converts C# code to JavaScript, supporting:

- Variable declarations and basic type conversions
- Pattern matching and conditional expressions
- Nullable type handling
- Async programming (async/await)
- String interpolation (template literals)
- Object and collection initialization
- Tuples and deconstruction
- Switch statements and expressions
- Loops (for / foreach / while / do-while)

See [Compiler Docs](src/Jazor.Compiler/README.md) for the full feature set.

### Conversion Example

```csharp
// C# code
int x = 42;
string message = $"Value is {x}";
bool isPositive = x > 0;
```

```javascript
// Converted JavaScript code
let x = 42;
let message = `Value is ${x}`;
let isPositive = x > 0;
```

## ECMAScript Attribute Conventions

- `[ECMAScript("jsr:@scope/pkg")]`, `[ECMAScript("npm:vue@3")]`, `[ECMAScript("https://...")]` declare **Deno-resolvable import addresses**.
- `[ECMAScriptModule("features/todo/index.mjs")]` declares the **module path after emission** — it is not a package resolution address.
- CLR and host mapping producer-side facts are declared via `[Jazor(...)]`.

Example:

```csharp
using ECMAScript;

[ECMAScript("npm:vue@3")]
public static partial class VueRuntime
{
}

[ECMAScriptModule("features/todo/index.mjs")]
public partial class TodoPage
{
}
```

## Usage

### Using ECMAScriptModule Attribute

```csharp
using ECMAScript;

[ECMAScriptModule]
public static class MyMathModule
{
    public static int Add(int a, int b) => a + b;
    public static string Greet(string name) => $"Hello, {name}!";
}
```

### Basic Compilation Flow

```csharp
using Jazor.Compiler;
using Microsoft.CodeAnalysis;

// Get semantic model
var semanticModel = compilation.GetSemanticModel(syntaxTree);

// Convert to JavaScript AST - class level
var converter = new AstConverter(classSymbol, semanticModel);
var module = converter.Convert();

// Convert to JavaScript AST - operation level
var walker = new SemanticWalker();
var jsAst = walker.Visit(operation, new());
```

## Development

### Prerequisites
- .NET 10 SDK
- PowerShell 7+ (for test scripts)
- Windows, Linux, or macOS

### Build Steps

```bash
# Clone the repository
git clone https://github.com/devhxj/Jazor.git
cd Jazor

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run all tests
pwsh ./scripts/test-dotnet.ps1

# Run compiler tests
pwsh ./scripts/test-dotnet.ps1 -Project compiler

# Run a single test class
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest"

# Run a single test method
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest.Visit_IsPattern_Constant"
```

## Contributing

Community contributions are welcome. Please review the repository documentation and follow the conventions described in the codebase before submitting a Pull Request.

### Development Workflow
1. Fork the repository
2. Create a feature branch
3. Implement the feature and add tests
4. Ensure all tests pass
5. Submit a Pull Request

### Code Conventions
- Follow C# coding conventions
- Add appropriate comments and documentation where clarification is needed
- Ensure new features have corresponding unit tests
- Follow semantics-preserving design principles

## License

This project is licensed under the MIT License. See [LICENSE.txt](LICENSE.txt) for details.

## Contact

- Project homepage: https://github.com/devhxj/Jazor
- Issue tracker: https://github.com/devhxj/Jazor/issues
- Email: developerhan@msn.cn

## Acknowledgements

Thanks to all developers and community members who have contributed to the Jazor project.

Special thanks to these open-source projects:
- [Roslyn](https://github.com/dotnet/roslyn) - C# compiler platform
- [Acornima](https://github.com/adams85/acornima) - JavaScript parser and AST library
- [WebRef](https://github.com/w3c/webref) - Web specification references
- [WootzJs](https://github.com/kswoll/WootzJs) - C# to JavaScript compiler
- [h5](https://github.com/curiosity-ai/h5) - C# to JavaScript compiler
- [SharpKit](https://github.com/SharpKit/SharpKit) - C# to JavaScript converter
- [SharpPromise](https://github.com/legacybass/SharpPromise) - Promise implementation for C#
- [DenoHost](https://github.com/thomas3577/DenoHost) - Deno runtime host for .NET
- [CSharpToJavaScript](https://github.com/TiLied/CSharpToJavaScript) - C# to JavaScript transpiler
