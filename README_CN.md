# Jazor - C# 到 JavaScript 编译器（支持模块系统）

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Jazor 是一个高性能的 C# 到 JavaScript 编译器，旨在实现 C# 代码到 JavaScript 代码的语义等价转换。该项目基于 Roslyn 编译器平台，通过 AST（抽象语法树）转换技术，精确地将 C# 代码转换为可在浏览器或 Node.js 环境中运行的 JavaScript 代码。

## 主要特性

- **ECMAScript 模块系统**：支持 `[ECMAScriptModule]` 和 `[ECMAScript]` 特性标记类进行 JavaScript 转换
- **静态分析**：Roslyn 分析器自动对标记的类进行语法校验
- **源代码生成器**：自动生成包含转换后的 ES6+ 模块 JavaScript 内容的 `ECMAScript.g.cs` 文件
- **网页项目集成**：配置输出目标从 `ECMAScript.g.cs` 提取 JavaScript 代码并生成 JS 文件
- **Bun 主机集成**：通过 bunhost 将 JS 文件与其他 npm 包进行打包编译
- **CLI 代理生成**：为 TypeScript 编写的 npm 包生成代理类（使用 `[ECMAScript]` 特性，不转换但可调用）
- **Razor JSX 支持**：基于 `.razor` 文件实现类似 JSX 的功能
- **完整类型映射**：全面支持 C# 类型与自动 JavaScript 类型转换

- **语义等价转换**：确保 C# 和 JavaScript 之间的语义完全等价，避免任何形式的简化处理
- **完整语法支持**：支持变量声明、控制流、函数、类、模式匹配等现代 C# 语法
- **高级模式匹配**：完整支持 C# 8.0+ 的模式匹配功能，包括递归模式、关系模式等
- **异步编程支持**：完整支持 async/await 异步编程模型
- **Web IDL 绑定**：自动生成 Web IDL 的 C# 绑定，支持 DOM、CSS、WebGL 等
- **编译时优化**：利用 C# 强类型系统的编译时信息，生成最优化的 JavaScript 代码
- **可扩展架构**：模块化设计，支持自定义转换规则和扩展
- **CLR 运行时**：为所有支持的原生类型提供 ES6+ 模块实现，支持 tree shaking

## 项目结构

```
Jazor/
├── src/
│   ├── ECMAScript/                 # 核心 ECMAScript 实现
│   │   ├── attribute/             # ECMAScript 特性定义
│   │   │   ├── ECMAScriptAttribute.cs
│   │   │   └── ECMAScriptModuleAttribute.cs
│   │   ├── generate/              # 自动生成的类型绑定
│   │   ├── internal/              # 内部类型实现
│   │   └── Global.cs              # 全局方法和属性
│   ├── ECMAScript.CLR/            # CLR 运行时支持
│   │   ├── StringModule.cs        # String 类型实现
│   │   ├── DateTimeModule.cs      # DateTime 类型实现
│   │   └── ...                    # 其他 CLR 类型模块
│   ├── ECMAScript.Analyzer/       # 静态代码分析器
│   ├── ECMAScript.Compiler/       # C# 到 JavaScript 编译器
│   │   ├── ESGenerator.cs         # ECMAScript.g.cs 的源代码生成器
│   │   └── AstOperationWalker.cs # 核心 AST 转换器
│   ├── ECMAScript.Server/         # 编译服务器
│   ├── ECMAScript.Test/           # 单元测试
│   ├── ECMAScript.ComplierTest/  # 编译器测试
│   ├── ECMAScript.WebIDL/         # Web IDL 绑定生成
│   └── ECMAScript.CLRGenerate/    # CLR 类型生成器
├── PROJECT_RULES.md               # 项目开发规则
└── README.md                      # 项目说明
```

## 核心组件

### 1. ECMAScript.Compiler
核心编译器组件，负责将 C# 的 Roslyn 操作树转换为 JavaScript 的 Acornima AST。主要特性：
- 直接 AST 构造，避免字符串解析开销
- 语义等价性保证，确保转换前后行为一致
- 完整的错误处理和异常报告机制
- **ESGenerator**：源代码生成器，自动创建包含转换后 JavaScript 内容的 `ECMAScript.g.cs` 文件

### 2. ECMAScript.Analyzer
静态代码分析器，为标记了 `[ECMAScriptModule]` 或 `[ECMAScript]` 特性的类提供语法验证：
- 根据支持的类型映射验证类型使用
- 确保只在 ECMAScript 标记的类中使用兼容的成员
- 为不支持的操作提供编译时错误报告

### 3. ECMAScript.WebIDL
Web API 绑定生成器，自动从 Web IDL 规范生成 C# 类型绑定。支持：
- DOM API 绑定
- CSS API 绑定
- WebGL API 绑定
- 现代 Web 标准 API 绑定

### 4. ECMAScript.CLR
CLR 运行时支持，为所有支持的原生 C# 类型提供 ES6+ 模块实现：
- C# 和 JavaScript 之间的类型安全转换
- 完整的方法和属性实现
- 支持优化的打包的 tree shaking

### 5. ECMAScript.Server
编译服务器，提供基于命名管道的编译服务：
- 支持持续编译
- 提供远程编译接口
- 集成到开发工作流

## 支持的 C# 类型和类型映射

### 基础类型
| C# 类型 | JavaScript 类型 |
|---------|-----------------|
| `object` | `object` |
| `string` | `string` |
| `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `decimal`, `double`, `float` | `Number` |
| `long`, `int128`, `uint128`, `TimeSpan` | `BigInt` |
| `DateOnly`, `TimeOnly`, `DateTime`, `DateTimeOffset` | `Date` |
| `bool` | `boolean` |
| `char` | `string` |

### 集合类型
| C# 类型 | JavaScript 类型 |
|---------|-----------------|
| `Array<>`, `List<>`, `IList<>`, `ICollection<>` | `Array` |
| `Dictionary<,>` | `Map` |
| `HashSet<>`, `IEnumerable`（非 IDictionary） | `Set` |
| `ReadOnlyCollection<>`, `ReadOnlyDictionary<,>`, `ReadOnlySet<>` | `Map` |

### 特殊类型
| C# 类型 | JavaScript 类型 |
|---------|-----------------|
| `Exception` | `Error` |
| `StringBuilder` | `StringBuilder 实现` |
| `Nullable<T>` | `可空类型处理` |
| `ValueTuple` | `Array 或 Object` |
| `WeakReference<T>` | `WeakRef` |
| `ConditionalWeakTable<,>` | `WeakMap` |
| `GregorianCalendar`, `CultureInfo` | `国际化 API` |

### 自定义类型
- 标记了 `[ECMAScript]` 或 `[ECMAScriptModule]` 特性的类
- 转换为保留语义的 JavaScript 类

## 支持的 C# 语法

### 基础语法
- 变量声明和初始化
- 运算符（算术、逻辑、位运算）
- 控制流（if/else, switch, for, foreach, while）
- 异常处理（try/catch/finally）

### 高级语法
- Lambda 表达式和本地函数
- 异步编程（async/await）
- 模式匹配（is, switch 表达式）
- 元组和解构
- 插值字符串
- 空合并运算符
- 条件访问运算符

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
let message = "Value is " + x;
let isPositive = x > 0;
```

### 模式匹配转换
```csharp
// C# 代码
string DescribeValue(int value) => value switch
{
    < 0 => "Negative",
    > 0 and < 100 => "Small Positive",
    >= 100 => "Large Positive",
    _ => "Zero"
};
```

```javascript
// 转换后的 JavaScript 代码
function describeValue(value) {
    return (() => {
        if (value < 0) return "Negative";
        if (value > 0 && value < 100) return "Small Positive";
        if (value >= 100) return "Large Positive";
        return "Zero";
    })();
}
```

### 异步编程转换
```csharp
// C# 代码
async Task<string> FetchDataAsync(string url)
{
    var client = new HttpClient();
    var response = await client.GetAsync(url);
    return await response.Content.ReadAsStringAsync();
}
```

```javascript
// 转换后的 JavaScript 代码
async function fetchDataAsync(url) {
    const response = await fetch(url);
    return await response.text();
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
using ECMAScript.Compiler;
using Microsoft.CodeAnalysis;

// 创建编译器实例
var walker = new AstOperationWalker();

// 编译 C# 代码
var compilation = CSharpCompilation.Create("TestAssembly",
    syntaxTrees,
    references,
    options);

// 获取语义模型
var semanticModel = compilation.GetSemanticModel(syntaxTree);

// 转换为 JavaScript AST
var operation = semanticModel.GetOperation(syntaxNode);
var jsAst = walker.Visit(operation, operation);

// 生成 JavaScript 代码
var jsCode = jsAst.ToJavaScript();
```

### 使用编译服务器
```csharp
using ECMAScript.Server;
using System.IO.Pipes;

// 连接到编译服务器
using var client = new NamedPipeClient("ECMAScript");

// 准备编译请求
var request = new PipeMessage(
    CommandType: 1,
    Body: Encoding.UTF8.GetBytes(csharpCode)
);

// 发送请求并获取响应
var response = await client.RequestAsync(request);
var jsCode = Encoding.UTF8.GetString(response.Body);
```

### 网页项目配置

对于网页项目，配置输出目标从 `ECMAScript.g.cs` 提取 JavaScript 代码：

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

### 使用 Bun 主机进行打包

```bash
# 安装 bun
curl -fsSL https://bun.sh/install | bash

# 打包你的 JavaScript 模块
bun build wwwroot/js/mymodule.js --outdir wwwroot/dist --target browser

# 使用 bun 运行
bun run wwwroot/dist/mymodule.js
```

### CLI 工具使用

```bash
# 安装 Jazor CLI
dotnet tool install --global Jazor.CLI

# 为 npm 包生成代理类
jazor generate-proxy --package lodash --output ./Proxies/LodashProxy.cs

# 将 C# 项目转换为 JavaScript
jazor convert --project ./MyProject.csproj --output ./dist

# 与 npm 包一起打包
jazor bundle --input ./dist --packages lodash,axios --output ./bundle.js

## 高级特性

### Razor JSX 支持

Jazor 支持使用 Razor 组件实现类似 JSX 的语法：

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

// 转换为 JavaScript：
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

### Tree Shaking 支持

CLR 模块设计时考虑了 tree shaking：

```javascript
// 只有使用的方法会包含在最终包中
import { StringCompare, StringIndexOf } from './clr/string.mjs';

// 未使用的方法如 StringToUpper 会在打包时被消除
```

### NPM 包的代理生成

为现有 npm 包生成 C# 代理类：

```bash
# 为 lodash 生成代理
jazor generate-proxy --package lodash --output ./Proxies/

# 生成的代理：
[ECMAScript]
public static class LodashProxy
{
    public static T Get<T>(object obj, string path) => default!;
    public static T[] Filter<T>(T[] collection, Func<T, bool> predicate) => default!;
    // ... 其他 lodash 方法
}
```

### 与现有 JavaScript 库集成

```csharp
[ECMAScriptModule]
public static class ChartInterop
{
    public static void CreateChart(string elementId, object chartData)
    {
        // 直接 JavaScript 互操作
        // 生成为：import { Chart } from 'chart.js';
    }
}
```
```

## 开发和构建

### 环境要求
- .NET 10.0 SDK 或更高版本
- Visual Studio 2022 或 Visual Studio Code
- Node.js 和 npm（用于网页开发）
- Bun（可选，用于打包）
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

# 生成 Web API 绑定
cd src/ECMAScript.WebIDL
npm install
npm run build

# 安装 CLI 工具
dotnet pack ./src/Jazor.CLI
dotnet tool install --global --add-source ./nupkg Jazor.CLI
```

## 贡献指南

我们欢迎社区贡献！请阅读 [PROJECT_RULES.md](PROJECT_RULES.md) 了解详细的开发规则和贡献流程。

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

本项目采用 MIT 许可证。详情请参阅 [LICENSE](LICENSE) 文件。

## 联系方式

- 项目主页：https://github.com/devhxj/Jazor
- 问题反馈：https://github.com/devhxj/Jazor/issues
- 邮箱：developerhan@msn.cn

## 致谢

感谢所有为 Jazor 项目做出贡献的开发者和社区成员！

特别感谢以下开源项目：
- [Roslyn](https://github.com/dotnet/roslyn) - C# 编译器平台
- [Acornima](https://github.com/adams85/acornima) - JavaScript 解析器
- [WebRef](https://github.com/w3c/webref) - Web 规范参考
- [WootzJs](https://github.com/kswoll/WootzJs) - C# 到 JavaScript 编译器
- [h5](https://github.com/curiosity-ai/h5) - C# 到 JavaScript 编译器
- [SharpKit](https://github.com/SharpKit/SharpKit) - C# 到 JavaScript 转换器
- [SharpPromise](https://github.com/legacybass/SharpPromise) - C# 的 Promise 实现
- [DenoHost](https://github.com/thomas3577/DenoHost) - .NET 的 Deno 运行时主机
- [CSharpToJavaScript](https://github.com/TiLied/CSharpToJavaScript) - C# 到 JavaScript 转译器