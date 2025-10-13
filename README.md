# Jazor - C# to JavaScript Compiler

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Jazor is a high-performance C# to JavaScript compiler that aims to achieve semantically equivalent conversion from C# code to JavaScript code. Based on the Roslyn compiler platform, this project utilizes AST (Abstract Syntax Tree) transformation technology to precisely convert C# code into JavaScript code that can run in browsers or Node.js environments.

## Key Features

- **Semantic Equivalence**: Ensures complete semantic equivalence between C# and JavaScript, avoiding any form of simplification
- **Complete Syntax Support**: Supports modern C# syntax including variable declarations, control flow, functions, classes, pattern matching, and more
- **Advanced Pattern Matching**: Full support for C# 8.0+ pattern matching features, including recursive patterns, relational patterns, etc.
- **Async Programming Support**: Complete support for async/await asynchronous programming model
- **Web API Bindings**: Automatically generates C# bindings for Web APIs, supporting DOM, CSS, WebGL, and more
- **Compile-time Optimization**: Leverages compile-time information from C#'s strong type system to generate optimized JavaScript code
- **Extensible Architecture**: Modular design supporting custom transformation rules and extensions

## Project Structure

```
Jazor/
├── src/
│   ├── ECMAScript/                 # Core ECMAScript implementation
│   │   ├── attribute/             # ECMAScript attribute definitions
│   │   ├── generate/              # Auto-generated type bindings
│   │   ├── internal/              # Internal type implementations
│   │   └── Global.cs              # Global methods and properties
│   ├── ECMAScript.CLR/            # CLR runtime support
│   ├── ECMAScript.Analyzer/       # Static code analyzer
│   ├── ECMAScript.Compiler/       # C# to JavaScript compiler
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

### 2. ECMAScript.WebIDL
Web API binding generator that automatically generates C# type bindings from Web IDL specifications. Supports:
- DOM API bindings
- CSS API bindings
- WebGL API bindings
- Modern Web standard API bindings

### 3. ECMAScript.CLRGenerate
CLR type generator that creates JavaScript bindings for .NET base type libraries. Including:
- Basic types (Object, String, Number, etc.)
- Collection types (Array, Dictionary, Set, etc.)
- Date and time type handling

### 4. ECMAScript.Server
Compilation server providing named pipe-based compilation services:
- Supports continuous compilation
- Provides remote compilation interface
- Integrates into development workflows

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

# Run tests
dotnet test

# Generate Web API bindings
cd src/ECMAScript.WebIDL
npm install
npm run build
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

- Project homepage: https://github.com/your-repo/Jazor
- Issue tracker: https://github.com/your-repo/Jazor/issues
- Email: your-email@example.com

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