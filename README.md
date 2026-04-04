<div align="center">

![Today's Verse](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)
</div>

# Jazor - C# to JavaScript Compiler with Module System

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.txt)

> ⚠️ **EXPERIMENTAL DEMO** ⚠️\
> This library is a pilot project, and its API and features are subject to change during development, and may ultimately not be completed.
---

Jazor is a high-performance C# to JavaScript compiler that aims to achieve semantically equivalent conversion from C# code to JavaScript code. Based on the Roslyn compiler platform, this project utilizes AST (Abstract Syntax Tree) transformation technology to precisely convert C# code into JavaScript code that can run in browsers or Node.js environments.

## Documentation Map

- [Repository documentation hub](docs/README.md)
- [Current project stage assessment](docs/status/2026-04-04-project-stage-assessment.md)
- [Documentation governance rules](docs/guides/documentation-governance.md)
- [Compiler deep-dive index](src/Jazor.Compiler/doc/README.md)

## Key Features

- **Semantic Equivalence**: Ensures complete semantic equivalence between C# and JavaScript, avoiding any form of simplification
- **Complete Syntax Support**: Supports modern C# syntax including variable declarations, control flow, functions, classes, pattern matching, and more
- **Advanced Pattern Matching**: Full support for C# 8.0+ pattern matching features, including recursive patterns, relational patterns, list patterns, etc.
- **Async Programming Support**: Complete support for async/await asynchronous programming model

## Planned Features

- **ECMAScript Module System**: Support for `[ECMAScriptModule]` and `[ECMAScript]` attributes to mark classes for JavaScript conversion
- **Static Analysis**: Roslyn analyzer automatically performs syntax validation for tagged classes
- **Source Generator**: Automatically generates `ECMAScript.g.cs` files containing converted ES6+ module JavaScript content
- **Web Project Integration**: Configure output targets to extract JavaScript code from `ECMAScript.g.cs` and generate JS files
- **Bun/Deno Host Integration**: Bundle and compile JS files with other npm packages through bun/denohost
- **CLI Proxy Generation**: Generate proxy classes for TypeScript-written npm packages (with `[ECMAScript]` attribute, no conversion but callable)
- **Razor JSX Support**: Implement JSX-like capabilities based on `.razor` files
- **Complete Type Mapping**: Comprehensive support for C# types with automatic JavaScript type conversion
- **Source Map & Debugging**: Source map generation and debugging support

## Project Structure

```
Jazor/
├── src/
│   ├── ECMAScript/                 # Core ECMAScript implementation
│   │   ├── attribute/             # ECMAScript attribute definitions
│   │   ├── generate/              # Auto-generated type bindings
│   │   └── Global.cs              # Global methods and properties
│   ├── ECMAScript.CLR/            # CLR runtime support
│   │   ├── BooleanModule.cs       # Boolean type implementation
│   │   ├── StringModule.cs        # String type implementation
│   │   ├── DateTimeModule.cs      # DateTime type implementation
│   │   ├── BigIntegerModule.cs    # BigInteger type implementation
│   │   └── ...                    # Other CLR type modules
│   ├── ECMAScript.Analyzer/       # Static code analyzer
│   │   └── WhiteList.cs            # White list for type and member validation
│   ├── ECMAScript.Compiler/       # C# to JavaScript compiler
│   │   ├── AstConverter.cs        # Class-level converter (C# class → ES6 module)
│   │   ├── SemanticWalker.cs      # Operation-level converter (IOperation → JS AST)
│   │   │   ├── SemanticWalker.cs.Declaration.cs    # Variable declarations
│   │   │   ├── SemanticWalker.cs.Ordinary.cs       # Operators, expressions
│   │   │   ├── SemanticWalker.cs.Reference.cs      # References, array indexing
│   │   │   ├── SemanticWalker.cs.Loop.cs           # Loops
│   │   │   ├── SemanticWalker.cs.Switch.cs         # Switch statements/expressions
│   │   │   ├── SemanticWalker.cs.Pattern.cs        # Pattern matching
│   │   │   ├── SemanticWalker.cs.String.cs         # String interpolation
│   │   │   ├── SemanticWalker.cs.TryCatch.cs       # Exception handling
│   │   │   ├── SemanticWalker.cs.Creation.cs       # Object/array creation
│   │   │   ├── SemanticWalker.cs.Tuple.cs          # Tuples and deconstruction
│   │   │   ├── SemanticWalker.cs.Invalid.cs        # SyntaxNode fallback
│   │   │   └── SemanticWalker.cs.NotSupport.cs    # Unsupported operations
│   │   ├── WalkerArgument.cs       # Conversion context parameter
│   │   ├── StatementGroup.cs        # Statement grouping utilities
│   │   ├── AstTransformationException.cs  # Exception definitions
│   │   └── ESGenerator.cs         # Source generator for ECMAScript.g.cs
│   ├── ECMAScript.Server/         # Compilation server
│   ├── ECMAScript.Test/           # Manual test console
│   ├── Jazor.CompilerTest/        # Compiler tests (MSTest)
│   ├── Jazor.EmitTest/            # Emit and bundle tests (MSTest)
│   ├── ECMAScript.WebIDL/         # WebIDL collection worker (TypeScript/Deno)
│   ├── ECMAScript.WebIDL.Generator/ # C# host for the WebIDL pipeline
│   ├── ECMAScript.Common/         # Common types and utilities
│   └── ECMASCript.MSBuild/        # MSBuild integration
├── README.md                      # This file
└── README_CN.md                   # Chinese version
```

## Core Components

### 1. ECMAScript.Compiler

The core compiler component with a two-layer conversion architecture:

**AstConverter (Class-Level Conversion)**:
- Converts entire C# classes to ES6 modules
- Handles static fields, properties, methods, nested classes, and enums
- Manages export declarations based on accessibility

**SemanticWalker (Operation-Level Conversion)**:
- Converts C# Roslyn operation trees to JavaScript Acornima AST
- Direct AST construction, avoiding string parsing overhead
- Semantic equivalence guarantee, ensuring consistent behavior before and after conversion
- Supports fallback to SyntaxNode conversion for optimized code via `IInvalidOperation`
- **ESGenerator**: Source generator that automatically creates `ECMAScript.g.cs` files

**Status**: ✅ Core complete | 533 tests passing (100%) | Build successful
See [Jazor.Compiler readme](src/Jazor.Compiler/readme.md) for detailed documentation.

### 2. ECMAScript.Analyzer

Static code analyzer that provides syntax validation for classes marked with `[ECMAScriptModule]` or `[ECMAScript]` attributes:
- Validates type usage according to supported type mappings
- Ensures only compatible members are used in ECMAScript-tagged classes via white list
- Provides compile-time error reporting for unsupported operations

### 3. ECMAScript.CLR

CLR runtime support providing ES6+ module implementations for all supported native C# types:
- Written in C# (syntax-compatible with JavaScript) but compiled to ES6 modules
- Type-safe conversion between C# and JavaScript
- Complete method and property implementations
- Tree shaking support for optimized bundles via `[WhiteList]` attribute mapping

**Module Status** (39 modules total):
- ✅ Complete (9/10): 27 modules (69%)
- ⚠️ Partial (7-8/10): 12 modules (31%)
- 🔴 Needs work (< 7/10): 0 modules

See [Jazor.CLR readme](src/Jazor.CLR/readme.md) for detailed module documentation.

### 4. ECMAScript.WebIDL

Web API binding generator that automatically generates C# type bindings from Web IDL specifications. Supports:
- DOM API bindings
- CSS API bindings
- WebGL API bindings
- Modern Web standard API bindings

The pipeline is being migrated to a split architecture:
- `src/ECMAScript.WebIDL` keeps the `webref` / `webidl2` collection layer
- `src/ECMAScript.WebIDL.Generator` hosts Deno through `DenoHost` and persists a stable JSON inventory for the future C# emitter

### 5. ECMAScript.Server

Compilation server providing named pipe-based compilation services:
- Supports continuous compilation
- Provides remote compilation interface
- Integrates into development workflows

## Supported C# Types and Type Mapping

### Primitive Types
| C# Type | JavaScript Type |
|---------|-----------------|
| `object` | `object` |
| `string` | `string` |
| `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `decimal`, `double`, `float` | `Number` |
| `long`, `ulong`, `Int128`, `UInt128`, `TimeSpan`, `BigInteger` | `BigInt` |
| `DateOnly`, `TimeOnly`, `DateTime`, `DateTimeOffset` | `Date` |
| `bool` | `boolean` |
| `char` | `string` |

### Collection Types
| C# Type | JavaScript Type |
|---------|-----------------|
| `Array<>`, `List<>`, `IList<>`, `IEnumerable<>` | `Array` |
| `Dictionary<,>`, `IDictionary<,>` | `Map` |
| `HashSet<>`, `ISet<>` | `Set` |

### Special Types
| C# Type | JavaScript Type |
|---------|-----------------|
| `Exception` | `Error` |
| `StringBuilder` | StringBuilder implementation |
| `Nullable<T>` | Nullable type handling |
| `ValueTuple` | Array or Object |
| `WeakReference<T>` | `WeakRef` |
| `ConditionalWeakTable<,>` | `WeakMap` |
| `GregorianCalendar`, `CultureInfo` | Internationalization API |

### Custom Types
- Classes marked with `[ECMAScript]` or `[ECMAScriptModule]` attributes
- Classes converted to JavaScript classes with preserved semantics

## Supported C# Syntax

### Basic Syntax
- Variable declarations and initialization
- Operators (arithmetic, logical, bitwise, compound assignment)
- Control flow (if/else, switch, for, foreach, while, do-while)
- Exception handling (try/catch/finally)

### Advanced Syntax
- Lambda expressions and local functions
- Asynchronous programming (async/await)
- Pattern matching (is expressions, switch expressions, recursive patterns, list patterns, etc.)
- Tuples and deconstruction
- Interpolated strings (template strings)
- Null-coalescing operators (`??`, `??=`)
- Conditional access operators (`?.`, `?[]`, `?..`)
- Index ranges (`array[1..^4]`, `array[..]`)

### Object-Oriented Programming
- Classes and structs
- Properties and fields
- Methods and constructors
- Inheritance and polymorphism
- Interface implementations

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
- .NET 10.0 SDK or higher
- Visual Studio 2022 or Visual Studio Code
- Windows, Linux, or macOS

### Build Steps

```bash
# Clone repository
git clone https://github.com/your-repo/Jazor.git
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

We welcome community contributions! Please review the repository documentation and follow the conventions described in the codebase before opening a Pull Request.

### Development Workflow
1. Fork the project repository
2. Create a feature branch
3. Implement functionality and add tests
4. Ensure all tests pass
5. Submit a Pull Request

### Code Standards
- Follow C# coding conventions
- Add appropriate comments and documentation
- Ensure new features have corresponding unit tests
- Adhere to semantic equivalence principles

## License

This project is licensed under the MIT License. See the [LICENSE.txt](LICENSE.txt) file for details.

## Contact

- Project homepage: https://github.com/devhxj/Jazor
- Issue tracker: https://github.com/devhxj/Jazor/issues
- Email: developerhan@msn.cn

## Acknowledgments

Thanks to all developers and community members who have contributed to the Jazor project!

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
