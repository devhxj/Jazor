# Jazor - C# to JavaScript Compiler with Module System

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Jazor is a high-performance C# to JavaScript compiler that aims to achieve semantically equivalent conversion from C# code to JavaScript code. Based on the Roslyn compiler platform, this project utilizes AST (Abstract Syntax Tree) transformation technology to precisely convert C# code into JavaScript code that can run in browsers or Node.js environments.

## Key Features

- **ECMAScript Module System**: Support for `[ECMAScriptModule]` and `[ECMAScript]` attributes to mark classes for JavaScript conversion
- **Static Analysis**: Roslyn analyzer automatically performs syntax validation for tagged classes
- **Source Generator**: Automatically generates `ECMAScript.g.cs` files containing converted ES6+ module JavaScript content
- **Web Project Integration**: Configure output targets to extract JavaScript code from `ECMAScript.g.cs` and generate JS files
- **Bun Host Integration**: Bundle and compile JS files with other npm packages through bunhost
- **CLI Proxy Generation**: Generate proxy classes for TypeScript-written npm packages (with `[ECMAScript]` attribute, no conversion but callable)
- **Razor JSX Support**: Implement JSX-like capabilities based on `.razor` files
- **Complete Type Mapping**: Comprehensive support for C# types with automatic JavaScript type conversion

- **Semantic Equivalence**: Ensures complete semantic equivalence between C# and JavaScript, avoiding any form of simplification
- **Complete Syntax Support**: Supports modern C# syntax including variable declarations, control flow, functions, classes, pattern matching, and more
- **Advanced Pattern Matching**: Full support for C# 8.0+ pattern matching features, including recursive patterns, relational patterns, etc.
- **Async Programming Support**: Complete support for async/await asynchronous programming model
- **Web IDL Bindings**: Automatically generates C# bindings for Web APIs, supporting DOM, CSS, WebGL, and more
- **Compile-time Optimization**: Leverages compile-time information from C#'s strong type system to generate optimized JavaScript code
- **Extensible Architecture**: Modular design supporting custom transformation rules and extensions
- **CLR Runtime**: ES6+ module implementation for all supported native types with tree shaking support

## Project Structure

```
Jazor/
├── src/
│   ├── ECMAScript/                 # Core ECMAScript implementation
│   │   ├── attribute/             # ECMAScript attribute definitions
│   │   │   ├── ECMAScriptAttribute.cs
│   │   │   └── ECMAScriptModuleAttribute.cs
│   │   ├── generate/              # Auto-generated type bindings
│   │   ├── internal/              # Internal type implementations
│   │   └── Global.cs              # Global methods and properties
│   ├── ECMAScript.CLR/            # CLR runtime support
│   │   ├── StringModule.cs        # String type implementation
│   │   ├── DateTimeModule.cs      # DateTime type implementation
│   │   └── ...                    # Other CLR type modules
│   ├── ECMAScript.Analyzer/       # Static code analyzer
│   ├── ECMAScript.Compiler/       # C# to JavaScript compiler
│   │   ├── ESGenerator.cs         # Source generator for ECMAScript.g.cs
│   │   └── AstOperationWalker.cs # Core AST transformer
│   ├── ECMAScript.Server/         # Compilation server
│   ├── ECMAScript.Test/           # Unit tests
│   ├── ECMAScript.ComplierTest/  # Compiler tests
│   ├── ECMAScript.WebIDL/         # Web IDL binding generation
│   └── ECMAScript.CLRGenerate/    # CLR type generator
├── PROJECT_RULES.md               # Project development rules
└── README.md                      # Project documentation
```

## Core Components

### 1. ECMAScript.Compiler
The core compiler component responsible for converting C# Roslyn operation trees to JavaScript Acornima AST. Key features:
- Direct AST construction, avoiding string parsing overhead
- Semantic equivalence guarantee, ensuring consistent behavior before and after conversion
- Complete error handling and exception reporting mechanisms
- **ESGenerator**: Source generator that automatically creates `ECMAScript.g.cs` files with converted JavaScript content

### 2. ECMAScript.Analyzer
Static code analyzer that provides syntax validation for classes marked with `[ECMAScriptModule]` or `[ECMAScript]` attributes:
- Validates type usage according to supported type mappings
- Ensures only compatible members are used in ECMAScript-tagged classes
- Provides compile-time error reporting for unsupported operations

### 3. ECMAScript.WebIDL
Web API binding generator that automatically generates C# type bindings from Web IDL specifications. Supports:
- DOM API bindings
- CSS API bindings
- WebGL API bindings
- Modern Web standard API bindings

### 4. ECMAScript.CLR
CLR runtime support providing ES6+ module implementations for all supported native C# types:
- Type-safe conversion between C# and JavaScript
- Complete method and property implementations
- Tree shaking support for optimized bundles

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
| `long`, `int128`, `uint128`, `TimeSpan` | `BigInt` |
| `DateOnly`, `TimeOnly`, `DateTime`, `DateTimeOffset` | `Date` |
| `bool` | `boolean` |
| `char` | `string` |

### Collection Types
| C# Type | JavaScript Type |
|---------|-----------------|
| `Array<>`, `List<>`, `IList<>`, `ICollection<>` | `Array` |
| `Dictionary<,>` | `Map` |
| `HashSet<>`, `IEnumerable` (non-IDictionary) | `Set` |
| `ReadOnlyCollection<>`, `ReadOnlyDictionary<,>`, `ReadOnlySet<>` | `Map` |

### Special Types
| C# Type | JavaScript Type |
|---------|-----------------|
| `Exception` | `Error` |
| `StringBuilder` | `StringBuilder` implementation |
| `Nullable<T>` | `Nullable type handling` |
| `ValueTuple` | `Array or Object` |
| `WeakReference<T>` | `WeakRef` |
| `ConditionalWeakTable<,>` | `WeakMap` |
| `GregorianCalendar`, `CultureInfo` | `Internationalization API` |

### Custom Types
- Classes marked with `[ECMAScript]` or `[ECMAScriptModule]` attributes
- Classes converted to JavaScript classes with preserved semantics

## Supported C# Syntax

### Basic Syntax
- Variable declarations and initialization
- Operators (arithmetic, logical, bitwise)
- Control flow (if/else, switch, for, foreach, while)
- Exception handling (try/catch/finally)

### Advanced Syntax
- Lambda expressions and local functions
- Asynchronous programming (async/await)
- Pattern matching (is, switch expressions)
- Tuples and deconstruction
- Interpolated strings
- Null-coalescing operators
- Conditional access operators

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
let message = "Value is " + x;
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

### Async Programming Conversion
```csharp
// C# code
async Task<string> FetchDataAsync(string url)
{
    var client = new HttpClient();
    var response = await client.GetAsync(url);
    return await response.Content.ReadAsStringAsync();
}
```

```javascript
// Converted JavaScript code
async function fetchDataAsync(url) {
    const response = await fetch(url);
    return await response.text();
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

// Create compiler instance
var walker = new AstOperationWalker();

// Compile C# code
var compilation = CSharpCompilation.Create("TestAssembly",
    syntaxTrees,
    references,
    options);

// Get semantic model
var semanticModel = compilation.GetSemanticModel(syntaxTree);

// Convert to JavaScript AST
var operation = semanticModel.GetOperation(syntaxNode);
var jsAst = walker.Visit(operation, operation);

// Generate JavaScript code
var jsCode = jsAst.ToJavaScript();
```

### Using Compilation Server
```csharp
using ECMAScript.Server;
using System.IO.Pipes;

// Connect to compilation server
using var client = new NamedPipeClient("ECMAScript");

// Prepare compilation request
var request = new PipeMessage(
    CommandType: 1,
    Body: Encoding.UTF8.GetBytes(csharpCode)
);

// Send request and get response
var response = await client.RequestAsync(request);
var jsCode = Encoding.UTF8.GetString(response.Body);
```

### Web Project Configuration

For web projects, configure the output target to extract JavaScript code from `ECMAScript.g.cs`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <JazorOutputDirectory>wwwroot\js</JazorOutputDirectory>
    <JazorGenerateSourceFiles>true</JazorGenerateSourceFiles>
  </PropertyGroup>
  
  <ItemGroup>
    <JazorModule Include="MyModule" OutputFile="mymodule.js" />
  </ItemGroup>
</Project>
```

### Using Bun Host for Bundling

```bash
# Install bun
curl -fsSL https://bun.sh/install | bash

# Bundle your JavaScript modules
bun build wwwroot/js/mymodule.js --outdir wwwroot/dist --target browser

# Run with bun
bun run wwwroot/dist/mymodule.js
```

### CLI Tool Usage

```bash
# Install Jazor CLI
dotnet tool install --global Jazor.CLI

# Generate proxy classes for npm packages
jazor generate-proxy --package lodash --output ./Proxies/LodashProxy.cs

# Convert C# project to JavaScript
jazor convert --project ./MyProject.csproj --output ./dist

# Bundle with npm packages
jazor bundle --input ./dist --packages lodash,axios --output ./bundle.js
```

## Advanced Features

### Razor JSX Support

Jazor supports JSX-like syntax using Razor components:

```razor
@* MyComponent.razor *@
@attribute [ECMAScriptModule]

<div class="my-component">
    <h3>@Title</h3>
    @if (Items != null)
    {
        <ul>
            @foreach (var item in Items)
            {
                <li>@item.Name</li>
            }
        </ul>
    }
</div>

@code {
    [Parameter]
    public string Title { get; set; } = string.Empty;
    
    [Parameter]
    public List<Item>? Items { get; set; }
}

// Converts to JavaScript:
export class MyComponent {
    render() {
        return `<div class="my-component">
            <h3>${this.title}</h3>
            ${this.items ? `<ul>${
                this.items.map(item => `<li>${item.name}</li>`).join('')
            }</ul>` : ''}
        </div>`;
    }
}
```

### Tree Shaking Support

The CLR modules are designed with tree shaking in mind:

```javascript
// Only used methods are included in the final bundle
import { StringCompare, StringIndexOf } from './clr/string.mjs';

// Unused methods like StringToUpper are eliminated during bundling
```

### Proxy Generation for NPM Packages

Generate C# proxy classes for existing npm packages:

```bash
# Generate proxy for lodash
jazor generate-proxy --package lodash --output ./Proxies/

# Generated proxy:
[ECMAScript]
public static class LodashProxy
{
    public static T Get<T>(object obj, string path) => default!;
    public static T[] Filter<T>(T[] collection, Func<T, bool> predicate) => default!;
    // ... other lodash methods
}
```

### Integration with Existing JavaScript Libraries

```csharp
[ECMAScriptModule]
public static class ChartInterop
{
    public static void CreateChart(string elementId, object chartData)
    {
        // Direct JavaScript interop
        // Generated as: import { Chart } from 'chart.js';
    }
}
```

## Development and Build

### Prerequisites
- .NET 10.0 SDK or higher
- Visual Studio 2022 or Visual Studio Code
- Node.js and npm (for web development)
- Bun (optional, for bundling)
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

# Run tests
dotnet test

# Generate Web API bindings
cd src/ECMAScript.WebIDL
npm install
npm run build

# Install CLI tool
dotnet pack ./src/Jazor.CLI
dotnet tool install --global --add-source ./nupkg Jazor.CLI
```

## Contributing

We welcome community contributions! Please read [PROJECT_RULES.md](PROJECT_RULES.md) for detailed development rules and contribution guidelines.

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

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact

- Project homepage: https://github.com/devhxj/Jazor
- Issue tracker: https://github.com/devhxj/Jazor/issues
- Email: developerhan@msn.cn

## Acknowledgments

Thanks to all developers and community members who have contributed to the Jazor project!

Special thanks to the following open-source projects:
- [Roslyn](https://github.com/dotnet/roslyn) - C# compiler platform
- [Acornima](https://github.com/terrytyrius/Acornima) - JavaScript parser
- [WebRef](https://github.com/w3c/webref) - Web specification references
- [WootzJs](https://github.com/kswoll/WootzJs) - C# to JavaScript compiler
- [h5](https://github.com/curiosity-ai/h5) - The next generation C# to JavaScript compiler
- [SharpKit](https://github.com/SharpKit/SharpKit) - C# to JavaScript converter
- [SharpPromise](https://github.com/legacybass/SharpPromise) - Promise implementation for C#
- [DenoHost](https://github.com/thomas3577/DenoHost) - Deno runtime host for .NET
- [CSharpToJavaScript](https://github.com/TiLied/CSharpToJavaScript) - C# to JavaScript transpiler