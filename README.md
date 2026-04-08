<div align="center">

![Today's Verse](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)
</div>

# Jazor - C# to JavaScript Compiler with Module-Oriented Tooling

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.txt)

> ⚠️ **EXPERIMENTAL DEMO** ⚠️\
> Jazor is still evolving. Public APIs, generated output shapes, and adjacent toolchains may change as the repository continues to stabilize.

Jazor is an experimental Roslyn-based C# to JavaScript compiler project. It focuses on semantic-preserving lowering into JavaScript AST and currently treats the compiler mainline as the repository's most stable reference area, while RazorVue, emit/materialization, and source-map-related work continue as active execution lanes.

## 文档入口

- **新访客**：查看 [文档中心](docs/README.md) 了解项目全貌
- **维护者**：查看 [工作流总览](docs/workstream-dashboard.md) 快速恢复工作
- **架构设计**：查看 [架构文档](docs/architecture/README.md) 深入了解设计

## 项目状态

Jazor 当前以编译器主线为核心，RazorVue、Emit、SourceMap 等工作流持续演进中。详见 [工作流总览](docs/workstream-dashboard.md)。

- ✅ **Compiler 主线**：接近稳定，是仓库最成熟的部分
- 🔄 **RazorVue**：活跃执行中，Vue 导向的 Razor 编译路径
- 🔄 **Emit / Materialisation**：持续承接，产物输出和打包管道
- 🔄 **SourceMap**：局部活跃，支持当前的 RazorVue 需求

## 项目结构

```
Jazor/
├── src/
│   ├── Jazor.Compiler/              # C# to JavaScript compiler core
│   ├── Jazor.Analyzer/              # Static analyzer and whitelist validation
│   ├── Jazor.CLR/                   # CLR runtime support modules
│   ├── Jazor.Emit/                  # Emit and packaging pipeline
│   ├── Jazor.RazorVue/              # RazorVue integration surface
│   ├── Jazor.RazorVue.Analysis/     # RazorVue analysis and lowering
│   ├── Jazor.RazorVue.Vuetify/      # Vuetify component stubs
│   ├── Jazor.Vue/                   # Vue bridge compiler (.jazor)
│   ├── Jazor.Vue.Analysis/          # Vue bridge analysis and generation
│   ├── Jazor.Razor/                 # Razor template support
│   ├── Jazor.Common/                # Shared utilities and types
│   ├── ECMAScript/                  # ECMAScript core types and attributes
│   ├── ECMAScript.WebIDL/           # WebIDL binding generator
│   ├── Jazor.CompilerTest/          # Compiler tests (MSTest)
│   └── Jazor.EmitTest/              # Emit pipeline tests (MSTest)
├── docs/                            # Repository-level documentation hub
└── README.md                        # This file
```

## 核心组件

- **Jazor.Compiler** - C# 到 JavaScript 的编译器核心 [→ 详细文档](src/Jazor.Compiler/README.md)
- **Jazor.Analyzer** - 静态分析和白名单验证，确保编译时类型安全
- **Jazor.CLR** - .NET 类型的运行时模块支持，提供 JavaScript 运行时实现
- **Jazor.Emit** - 产物输出和打包管道，处理 host-facing 输出
- **Jazor.RazorVue** - Vue 导向的 Razor 编译路径，支持 Blazor 风格的组件编写
- **Jazor.Vue** - `.jazor` 文档模型和 Vue SFC 桥接编译器

详见 [模块索引](docs/architecture/modules/README.md) 了解所有模块。

## 核心能力

Jazor 支持将 C# 代码转换为 JavaScript，包括：

- 变量声明和基础类型转换
- 模式匹配和条件表达式
- 可空类型处理
- 异步编程（async/await）
- 字符串插值
- 对象和集合初始化

详见 [编译器文档](src/Jazor.Compiler/doc/README.md) 了解支持的完整特性。

### 转换示例

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

## 使用方法

### 使用 ECMAScriptModule 特性

```csharp
using ECMAScript;

[ECMAScriptModule]
public static class MyMathModule
{
    public static int Add(int a, int b) => a + b;
    public static string Greet(string name) => $"Hello, {name}!";
}
```

### 基本编译流程

```csharp
using Jazor.Compiler;
using Microsoft.CodeAnalysis;

// 获取语义模型
var semanticModel = compilation.GetSemanticModel(syntaxTree);

// 转换为 JavaScript AST - 类级别
var converter = new AstConverter(classSymbol, semanticModel);
var module = converter.Convert();

// 转换为 JavaScript AST - 操作级别
var walker = new SemanticWalker();
var jsAst = walker.Visit(operation, new());
```

## 开发和构建

### 环境要求
- .NET 10 SDK
- PowerShell 7+（用于测试脚本）
- Windows、Linux 或 macOS

### 构建步骤

```bash
# 克隆仓库
git clone https://github.com/devhxj/Jazor.git
cd Jazor

# 恢复依赖
dotnet restore

# 构建解决方案
dotnet build

# 运行所有测试
pwsh ./scripts/test-dotnet.ps1

# 运行编译器测试
pwsh ./scripts/test-dotnet.ps1 -Project compiler

# 运行单个测试类
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest"

# 运行单个测试方法
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest.Visit_IsPattern_Constant"
```

## 贡献

欢迎社区贡献。请在提交 Pull Request 前查阅仓库文档并遵循代码库中描述的约定。

### 开发流程
1. Fork 项目仓库
2. 创建功能分支
3. 实现功能并添加测试
4. 确保所有测试通过
5. 提交 Pull Request

### 代码规范
- 遵循 C# 编码约定
- 在需要澄清的地方添加适当的注释和文档
- 确保新功能有相应的单元测试
- 遵循语义保持的设计原则

## 许可证

本项目采用 MIT 许可证。详见 [LICENSE.txt](LICENSE.txt) 文件。

## 联系方式

- 项目主页：https://github.com/devhxj/Jazor
- 问题追踪：https://github.com/devhxj/Jazor/issues
- 邮箱：developerhan@msn.cn

## 致谢

感谢所有为 Jazor 项目做出贡献的开发者和社区成员。

特别感谢以下开源项目：
- [Roslyn](https://github.com/dotnet/roslyn) - C# 编译器平台
- [Acornima](https://github.com/adams85/acornima) - JavaScript 解析器和 AST 库
- [WebRef](https://github.com/w3c/webref) - Web 规范引用
- [WootzJs](https://github.com/kswoll/WootzJs) - C# 到 JavaScript 编译器
- [h5](https://github.com/curiosity-ai/h5) - C# 到 JavaScript 编译器
- [SharpKit](https://github.com/SharpKit/SharpKit) - C# 到 JavaScript 转换器
- [SharpPromise](https://github.com/legacybass/SharpPromise) - C# 的 Promise 实现
- [DenoHost](https://github.com/thomas3577/DenoHost) - .NET 的 Deno 运行时宿主
- [CSharpToJavaScript](https://github.com/TiLied/CSharpToJavaScript) - C# 到 JavaScript 转译器
