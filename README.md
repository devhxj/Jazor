<div align="center">

![Today's Verse](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)
</div>

# Jazor - C# to JavaScript Compiler with Module-Oriented Tooling

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.txt)

> ⚠️ **EXPERIMENTAL DEMO** ⚠️\
> Jazor is still evolving. Public APIs, generated output shapes, and adjacent toolchains may change as the repository continues to stabilize.

Jazor is an experimental Roslyn-based C# to JavaScript compiler project. It focuses on semantic-preserving lowering into JavaScript AST and currently treats the compiler mainline as the repository's most stable reference area, while RazorVue, emit/materialization, and source-map-related work continue as active execution lanes.

## Documentation map

### Start here

- [Repository documentation hub](docs/README.md)
- [Current workstream dashboard](docs/status/2026-04-06-project-workstream-dashboard.md)
- [Current project stage assessment](docs/status/2026-04-04-project-stage-assessment.md)

### Current status and execution

- [Compiler mainline status](docs/status/2026-04-06-compiler-mainline-status.md)
- [Emit and host materialization status](docs/status/2026-04-06-emit-host-materialization-status.md)
- [Project execution index](docs/plans/project-execution-index.md)
- [Project program roadmap](docs/plans/project-program-roadmap.md)

### Architecture

- [Repository architecture bridge](docs/architecture/README.md)
- [Compiler architecture bridge](docs/architecture/compiler/README.md)
- [Module-level bridge](docs/architecture/modules/README.md)

### Subsystem deep dives

- [Compiler deep-dive index](src/Jazor.Compiler/doc/README.md)
- [Jazor.Compiler module README](src/Jazor.Compiler/README.md)
- [Emit local docs](src/Jazor.Emit/doc/README.md)

### Planning and documentation governance

- [Documentation governance rules](docs/guides/documentation-governance.md)
- [Repository plans index](docs/plans/README.md)

If you are new to the repository, read in this order:

1. `docs/README.md`
2. `docs/status/2026-04-06-project-workstream-dashboard.md`
3. `docs/status/2026-04-06-compiler-mainline-status.md`
4. `docs/plans/project-execution-index.md`
5. `docs/architecture/README.md`

If you are resuming a specific workstream, start from the current status page for that lane and then drill into the linked subsystem documentation.

## What Jazor focuses on today

- Translating supported C# constructs into JavaScript through AST-based lowering instead of string templating.
- Preserving semantic intent across the compiler pipeline, analyzer checks, and runtime/module surfaces.
- Keeping the compiler mainline usable as a stable reference while adjacent workstreams keep evolving.
- Documenting active execution status explicitly so repository-level docs stay aligned with current work.

## Project status

### Stable reference areas

- **Compiler mainline**: the most mature part of the repository and the primary long-term reference surface.
- **Compiler architecture and deep-dive docs**: the best entry point when you need to understand the existing lowering pipeline.

### Active workstreams

- **RazorVue**: active implementation lane for Vue-oriented Razor lowering and authoring flow.
- **Emit / host materialization**: active dependency lane for shaping emitted assets and host-facing outputs.
- **SourceMap / bundle chaining**: active partial rollout, especially where it already intersects with current RazorVue execution work.

### Evolving / future-facing areas

- Broader authoring ergonomics beyond the currently closed safe subsets.
- Additional host integrations and packaging flows that are still being refined.
- Deeper capability expansion that should follow, not destabilize, the compiler mainline.

## Project Structure

```
Jazor/
├── src/
│   ├── ECMAScript/                  # Core ECMAScript runtime surface
│   ├── Jazor.Compiler/              # C# to JavaScript compiler
│   ├── Jazor.Compiler.Generator/    # Source generation pipeline
│   ├── Jazor.Analyzer/              # Static analyzer and whitelist validation
│   ├── Jazor.CLR/                   # CLR runtime support modules
│   ├── Jazor.Emit/                  # Emit and packaging pipeline
│   ├── Jazor.Razor/                 # Razor syntax support
│   ├── Jazor.RazorVue/              # RazorVue integration surface
│   ├── Jazor.RazorVue.Analysis/     # RazorVue analysis and lowering
│   ├── Jazor.CompilerTest/          # Compiler tests (MSTest)
│   ├── Jazor.EmitTest/              # Emit and bundle tests (MSTest)
│   ├── ECMAScript.WebIDL/           # WebIDL collection worker (TypeScript/Deno)
│   └── ECMAScript.WebIDL.Generator/ # C# host for the WebIDL pipeline
├── docs/                            # Repository-level documentation hub
├── README.md                        # This file
└── README_CN.md                     # Chinese version
```

## Core Components

### 1. Jazor.Compiler

The compiler core lowers Roslyn symbols and operations into JavaScript AST. It is currently the repository's most mature reference surface and the best starting point if you want to understand the project's long-lived architecture.

See [Jazor.Compiler README](src/Jazor.Compiler/README.md) for module-level details and [compiler deep-dive docs](src/Jazor.Compiler/doc/README.md) for the broader pipeline.

### 2. Jazor.Analyzer

The analyzer validates ECMAScript-tagged code against the project's supported surface and whitelist rules. Its role is to keep unsupported shapes visible at compile time instead of leaving them as silent runtime mismatches.

### 3. Jazor.CLR

Jazor.CLR provides runtime-oriented module surfaces for supported .NET types and bridges compiler output to JavaScript-facing behavior. The root README should describe its responsibility, not freeze module completion statistics that change over time.

### 4. ECMAScript.WebIDL

The WebIDL pipeline collects and materializes Web API metadata for future binding generation. It remains an important supporting lane rather than the primary repository entry point.

### 5. Jazor.Emit

Jazor.Emit shapes generated modules into host-facing outputs and bundle-oriented assets. It sits in an active dependency lane shared by multiple current workstreams.

## Current capability snapshot

Jazor currently emphasizes the compiler mainline and the repository infrastructure around it rather than claiming a frozen end-user feature surface.

The repository already contains substantial work in these areas:

- Roslyn-driven AST lowering through `AstConverter` and `SemanticWalker`
- Analyzer-backed validation for supported ECMAScript-tagged code
- Runtime/module surfaces for supported .NET types
- Ongoing work around RazorVue, emit/materialization, and source-map-adjacent output flows

The exact supported shape is still evolving. For detailed capability boundaries, prefer the subsystem documentation and current status pages over this top-level README.

## Conversion Examples

### Basic Code Conversion
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

### Pattern Matching Conversion
```csharp
// C# code
string DescribeValue(int value) => value switch
{
    < 0 => "Negative",
    > 0 and < 100 => "Small Positive",
    >= 100 => "Large Positive",
    _ => "Zero"
};
```

```javascript
// Converted JavaScript code
function describeValue(value) {
    return (() => {
        if (value < 0) return "Negative";
        if (value > 0 && value < 100) return "Small Positive";
        if (value >= 100) return "Large Positive";
        return "Zero";
    })();
}
```

### Nullable Type Handling
```csharp
// C# code
void Process(string? input)
{
    if (input is string actual)
    {
        Console.WriteLine(actual.Length);
    }
}
```

```javascript
// Converted JavaScript code
function process(input) {
    if (typeof input === "string" || input === null) {
        if (input !== null) {
            console.log(input.length);
        }
    }
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

### Basic Compilation

```csharp
using ECMAScript.Compiler;
using Microsoft.CodeAnalysis;

// Get semantic model
var semanticModel = compilation.GetSemanticModel(syntaxTree);

// Convert to JavaScript AST - Class level
var converter = new AstConverter(classSymbol, semanticModel);
var module = converter.Convert();

// Convert to JavaScript AST - Operation level
var walker = new SemanticWalker();
var jsAst = walker.Visit(operation, new());
```

## Development and Build

### Prerequisites
- .NET 10 SDK
- PowerShell 7+ for the repository test helper scripts
- Windows, Linux, or macOS

### Build Steps

```bash
# Clone repository
git clone https://github.com/devhxj/Jazor.git
cd Jazor

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run core .NET tests
pwsh ./scripts/test-dotnet.ps1

# Run compiler tests only
pwsh ./scripts/test-dotnet.ps1 -Project compiler

# Run emit/bundle tests only
pwsh ./scripts/test-dotnet.ps1 -Project emit

# Run specific test project directly
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj

# Run single test class
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest"

# Run single test method
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest.Visit_IsPattern_Constant"
```

## Contributing

We welcome community contributions. Please review the repository documentation and follow the conventions described in the codebase before opening a Pull Request.

### Development Workflow
1. Fork the project repository
2. Create a feature branch
3. Implement functionality and add tests
4. Ensure all tests pass
5. Submit a Pull Request

### Code Standards
- Follow C# coding conventions
- Add appropriate comments and documentation where clarification is needed
- Ensure new features have corresponding unit tests
- Adhere to semantic-preserving design principles

## License

This project is licensed under the MIT License. See the [LICENSE.txt](LICENSE.txt) file for details.

## Contact

- Project homepage: https://github.com/devhxj/Jazor
- Issue tracker: https://github.com/devhxj/Jazor/issues
- Email: developerhan@msn.cn

## Acknowledgments

Thanks to all developers and community members who have contributed to the Jazor project.

Special thanks to the following open-source projects:
- [Roslyn](https://github.com/dotnet/roslyn) - C# compiler platform
- [Acornima](https://github.com/adams85/acornima) - JavaScript parser and AST library
- [WebRef](https://github.com/w3c/webref) - Web specification references
- [WootzJs](https://github.com/kswoll/WootzJs) - C# to JavaScript compiler
- [h5](https://github.com/curiosity-ai/h5) - C# to JavaScript compiler
- [SharpKit](https://github.com/SharpKit/SharpKit) - C# to JavaScript converter
- [SharpPromise](https://github.com/legacybass/SharpPromise) - Promise implementation for C#
- [DenoHost](https://github.com/thomas3577/DenoHost) - Deno runtime host for .NET
- [CSharpToJavaScript](https://github.com/TiLied/CSharpToJavaScript) - C# to JavaScript transpiler
