<div align="center">

![今日诗词](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)
</div>

# Jazor - C# 到 JavaScript 编译器（支持模块系统）

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.txt)

Jazor 是一个高性能的 C# 到 JavaScript 编译器，旨在实现 C# 代码到 JavaScript 代码的语义等价转换。该项目基于 Roslyn 编译器平台，通过 AST（抽象语法树）转换技术，精确地将 C# 代码转换为可在浏览器或 Node.js 环境中运行的 JavaScript 代码。

## 文档导航

- [仓库文档中心](docs/README.md)
- [当前项目阶段评审](docs/status/2026-04-04-project-stage-assessment.md)
- [文档治理规则](docs/guides/documentation-governance.md)
- [Compiler 深度文档入口](src/Jazor.Compiler/doc/README.md)

## 主要特性

- **语义等价转换**：确保 C# 和 JavaScript 之间的语义完全等价，避免任何形式的简化处理
- **完整语法支持**：支持变量声明、控制流、函数、类、模式匹配等现代 C# 语法
- **高级模式匹配**：完整支持 C# 8.0+ 的模式匹配功能，包括递归模式、关系模式、列表模式等
- **异步编程支持**：完整支持 async/await 异步编程模型

## 计划支持的功能

- **ECMAScript 模块系统**：支持 `[ECMAScriptModule]` 和 `[ECMAScript]` 特性标记类进行 JavaScript 转换
- **静态分析**：Roslyn 分析器自动对标记的类进行语法校验
- **源代码生成器**：自动生成包含转换后的 ES6+ 模块 JavaScript 内容的 `ECMAScript.g.cs` 文件
- **网页项目集成**：配置输出目标从 `ECMAScript.g.cs` 提取 JavaScript 代码并生成 JS 文件
- **Bun/Deno 主机集成**：通过 bun/denohost 将 JS 文件与其他 npm 包进行打包编译
- **CLI 代理生成**：为 TypeScript 编写的 npm 包生成代理类（使用 `[ECMAScript]` 特性，不转换但可调用）
- **Razor JSX 支持**：基于 `.razor` 文件实现类似 JSX 的功能
- **完整类型映射**：全面支持 C# 类型与自动 JavaScript 类型转换
- **Source Map 及调试支持**：源映射生成和调试支持

## 项目结构

```
Jazor/
├── src/
│   ├── Jazor.Compiler/              # C# 到 JavaScript 编译器核心
│   ├── Jazor.Analyzer/              # 静态分析与白名单校验
│   ├── Jazor.CLR/                   # CLR 运行时支持模块
│   ├── Jazor.Emit/                  # Emit 与打包输出管线
│   ├── Jazor.RazorVue/              # RazorVue 集成层
│   ├── Jazor.RazorVue.Analysis/     # RazorVue 分析与 lowering
│   ├── Jazor.RazorVue.Vuetify/      # Vuetify 组件桩
│   ├── Jazor.Vue/                   # Vue 桥接编译器（.jazor）
│   ├── Jazor.Vue.Analysis/          # Vue 桥接分析与生成
│   ├── Jazor.Razor/                 # Razor 模板支持
│   ├── Jazor.Common/                # 共享工具和类型
│   ├── ECMAScript/                  # ECMAScript 核心类型和特性
│   ├── ECMAScript.WebIDL/           # WebIDL 绑定生成器
│   ├── Jazor.CompilerTest/          # 编译器测试（MSTest）
│   └── Jazor.EmitTest/              # Emit 与 bundle 测试（MSTest）
├── docs/                            # 仓库级文档中心
├── README.md                        # 英文版本文档
└── README_CN.md                     # 本文件
```

## 核心组件

### 1. Jazor.Compiler

核心编译器组件，采用两层转换架构：

**AstConverter（类级别转换）**：
- 将整个 C# 类转换为 ES6 模块
- 处理静态字段、属性、方法、嵌套类和枚举
- 根据可访问性管理导出声明

**SemanticWalker（操作级别转换）**：
- 将 C# Roslyn 操作树转换为 JavaScript Acornima AST
- 直接 AST 构造，避免字符串解析开销
- 语义等价性保证，确保转换前后行为一致
- 支持通过 `IInvalidOperation` 回退到 SyntaxNode 转换
- **ESGenerator**：源代码生成器，自动创建包含转换后 JavaScript 内容的 `ECMAScript.g.cs` 文件

详见 [Jazor.Compiler README](src/Jazor.Compiler/README.md) 了解最新的模块状态与详细说明。

### 2. Jazor.Analyzer

静态代码分析器，为标记了 `[ECMAScriptModule]` 或 `[ECMAScript]` 特性的类提供语法验证：
- 根据支持的类型映射验证类型使用
- 通过白名单确保只在 ECMAScript 标记的类中使用兼容的成员
- 为不支持的操作提供编译时错误报告

### 3. Jazor.CLR

CLR 运行时支持，为所有支持的原生 C# 类型提供 ES6+ 模块实现：
- 使用 C# 编写（语法贴合 JavaScript）但编译为 ES6 模块
- C# 和 JavaScript 之间的类型安全转换
- 完整的方法和属性实现
- 通过 `[WhiteList]` 特性映射支持优化的 tree shaking

**模块状态**（共 39 个模块）：
- ✅ 完善 (9/10)：27 个模块 (69%)
- ⚠️ 部分完善 (7-8/10)：12 个模块 (31%)
- 🔴 需完善 (< 7/10)：0 个模块

详见 [Jazor.CLR README](src/Jazor.CLR/README.md) 了解最新的模块状态与详细说明。

### 4. ECMAScript.WebIDL

Web API 绑定生成器，自动从 Web IDL 规范生成 C# 类型绑定。支持：
- DOM API 绑定
- CSS API 绑定
- WebGL API 绑定
- 现代 Web 标准 API 绑定

当前正在迁移为分层架构：
- `src/ECMAScript.WebIDL` 保留 `webref` / `webidl2` 采集层
- `src/ECMAScript.WebIDL.Generator` 通过 `DenoHost` 承载 Deno，并为后续 C# emitter 落稳定 JSON IR

### 5. Jazor.Emit

Emit 与打包输出管线，负责把生成结果整理为宿主可消费的输出：
- 组织运行时与模块输出资产
- 支持面向 bundle 的输出流程
- 由独立的 emit / bundle 测试覆盖

### 6. Jazor.Vue

Vue 桥接编译器，负责 `.jazor` 文档模型和 Vue SFC 桥接：
- `.jazor` 文档模型和解析器
- Vue SFC 桥接编译器
- 虚拟外部符号合约 (VESL)
- 外部声明发射器

## 支持的 C# 类型和类型映射

### 基础类型
| C# 类型 | JavaScript 类型 |
|---------|-----------------|
| `object` | `object` |
| `string` | `string` |
| `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `decimal`, `double`, `float` | `Number` |
| `long`, `ulong`, `Int128`, `UInt128`, `TimeSpan`, `BigInteger` | `BigInt` |
| `DateOnly`, `TimeOnly`, `DateTime`, `DateTimeOffset` | `Date` |
| `bool` | `boolean` |
| `char` | `string` |

### 集合类型
| C# 类型 | JavaScript 类型 |
|---------|-----------------|
| `Array<>`, `List<>`, `IList<>`, `IEnumerable<>` | `Array` |
| `Dictionary<,>`, `IDictionary<,>` | `Map` |
| `HashSet<>`, `ISet<>` | `Set` |

### 特殊类型
| C# 类型 | JavaScript 类型 |
|---------|-----------------|
| `Exception` | `Error` |
| `StringBuilder` | StringBuilder 实现 |
| `Nullable<T>` | 可空类型处理 |
| `ValueTuple` | Array 或 Object |
| `WeakReference<T>` | `WeakRef` |
| `ConditionalWeakTable<,>` | `WeakMap` |
| `GregorianCalendar`, `CultureInfo` | 国际化 API |

### 自定义类型
- 标记了 `[ECMAScript]` 或 `[ECMAScriptModule]` 特性的类
- 转换为保留语义的 JavaScript 类

## 支持的 C# 语法

### 基础语法
- 变量声明和初始化
- 运算符（算术、逻辑、位运算、复合赋值）
- 控制流（if/else, switch, for, foreach, while, do-while）
- 异常处理（try/catch/finally）

### 高级语法
- Lambda 表达式和本地函数
- 异步编程（async/await）
- 模式匹配（is 表达式、switch 表达式、递归模式、列表模式等）
- 元组和解构
- 插值字符串（模板字符串）
- 空合并运算符（`??`, `??=`）
- 条件访问运算符（`?.`, `?[]`, `?..`）
- 索引范围（`array[1..^4]`, `array[..]`）

### 面向对象编程
- 类和结构体
- 属性和字段
- 方法和构造函数
- 继承和多态
- 接口实现

## 转换示例

### 基础代码转换
```csharp
// C# 代码
int x = 42;
string message = $"Value is {x}";
bool isPositive = x > 0;
```

```javascript
// 转换后的 JavaScript 代码
let x = 42;
let message = `Value is ${x}`;
let isPositive = x > 0;
```

### 模式匹配转换
```csharp
// C# 代码
string DescribeValue(int value) => value switch
{
    < 0 => "负数",
    > 0 and < 100 => "小正数",
    >= 100 => "大正数",
    _ => "零"
};
```

```javascript
// 转换后的 JavaScript 代码
function describeValue(value) {
    return (() => {
        if (value < 0) return "负数";
        if (value > 0 && value < 100) return "小正数";
        if (value >= 100) return "大正数";
        return "零";
    })();
}
```

### 可空类型处理
```csharp
// C# 代码
void Process(string? input)
{
    if (input is string actual)
    {
        Console.WriteLine(actual.Length);
    }
}
```

```javascript
// 转换后的 JavaScript 代码
function process(input) {
    if (typeof input === "string" || input === null) {
        if (input !== null) {
            console.log(input.length);
        }
    }
}
```

## 使用方法

### 使用 ECMAScriptModule 特性

```csharp
using ECMAScript;

[ECMAScriptModule]
public static class MyMathModule
{
    public static int Add(int a, int b) => a + b;

    public static string Greet(string name) => $"你好，{name}！";
}
```

### 基本编译

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
- .NET 10.0 SDK 或更高版本
- Visual Studio 2022 或 Visual Studio Code
- Windows、Linux 或 macOS

### 构建步骤

```bash
# 克隆仓库
git clone https://github.com/your-repo/Jazor.git
cd Jazor

# 还原依赖
dotnet restore

# 构建解决方案
dotnet build

# 运行测试
dotnet test

# 运行特定测试项目
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj

# 运行单个测试类
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest"

# 运行单个测试方法
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest.Visit_IsPattern_Constant"
```

## 贡献指南

我们欢迎社区贡献！请在提交 Pull Request 前先阅读仓库文档，并遵循代码库中现有的约定。

### 开发流程
1. Fork 项目仓库
2. 创建功能分支
3. 实现功能并添加测试
4. 确保所有测试通过
5. 提交 Pull Request

### 代码规范
- 遵循 C# 编码约定
- 添加适当的注释和文档
- 确保新功能有相应的单元测试
- 遵循语义等价性原则

## 许可证

本项目采用 MIT 许可证。详情请参阅 [LICENSE.txt](LICENSE.txt) 文件。

## 联系方式

- 项目主页：https://github.com/devhxj/Jazor
- 问题反馈：https://github.com/devhxj/Jazor/issues
- 邮箱：developerhan@msn.cn

## 致谢

感谢所有为 Jazor 项目做出贡献的开发者和社区成员！

特别感谢以下开源项目：
- [Roslyn](https://github.com/dotnet/roslyn) - C# 编译器平台
- [Acornima](https://github.com/adams85/acornima) - JavaScript 解析器和 AST 库
- [WebRef](https://github.com/w3c/webref) - Web 规范参考
- [WootzJs](https://github.com/kswoll/WootzJs) - C# 到 JavaScript 编译器
- [h5](https://github.com/curiosity-ai/h5) - C# 到 JavaScript 编译器
- [SharpKit](https://github.com/SharpKit/SharpKit) - C# 到 JavaScript 转换器
- [SharpPromise](https://github.com/legacybass/SharpPromise) - C# 的 Promise 实现
- [DenoHost](https://github.com/thomas3577/DenoHost) - .NET 的 Deno 运行时主机
- [CSharpToJavaScript](https://github.com/TiLied/CSharpToJavaScript) - C# 到 JavaScript 转译器
