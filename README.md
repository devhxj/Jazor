<div align="center">

![Today's Verse](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)
</div>

# Jazor - C# to JavaScript Compiler with Module-Oriented Tooling

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.txt)

> ⚠️ **EXPERIMENTAL** ⚠️\
> Jazor is still evolving. Public APIs, generated output shapes, and adjacent toolchains may change as the repository continues to stabilize.

Jazor 是一个基于 Roslyn 的 C# → JavaScript 编译器。核心能力是将 C# 操作树（IOperation）语义保持地降低为 JavaScript AST。当前以编译器主线和 `Jolt`（`.jazor` 全功能开发时宿主）为活跃开发边界。

## 两条技术线路

| 线路 | 模式 | 核心项目 | 说明 |
|------|------|---------|------|
| **RazorVue** | 库模式 | `Jazor.RazorVue` + Analysis + Vuetify | Source Generator 驱动，不使用 .vue SFC，编译时直接输出 JS/TS 模块 |
| **Jolt** | 全功能模式 | `Jolt` | 类似 Vite，支持 .jazor + .vue SFC，LSP + DevServer/HMR + Debug + Build |

两条线路共享同一套编译器基础设施（SemanticWalker、WhiteList、AstConverter）。

## 文档入口

| 角色 | 入口 |
|------|------|
| **新访客** | [文档中心](docs/README.md) — 项目全貌与导航 |
| **维护者** | [工作流总览](docs/02-计划/workstream-dashboard.md) — 恢复工作唯一入口 |
| **架构设计** | [编译器架构](docs/01-目标/compiler/ArchitectureOverview.Simplified.md) · [Jolt 设计](docs/01-目标/jolt/README.md) · [RazorVue 设计](docs/01-目标/razorvue/README.md) |

文档按四类组织：[目标](docs/01-目标/README.md) · [计划](docs/02-计划/README.md) · [完成](docs/03-完成/README.md) · [补充](docs/04-补充/README.md) · [遗弃](docs/05-遗弃/README.md)

## 项目状态

详见 [工作流总览](docs/02-计划/workstream-dashboard.md)。

- ✅ **Compiler 主线** — 接近稳定，是仓库最成熟的部分
- 🔄 **Jolt** — Phase 1–6 收口中，Phase 7 扩展系统规划中
- 🔄 **Emit / Materialisation** — 持续承接，产物输出和打包管道
- 🔄 **SourceMap** — 局部活跃（narrow lane），支撑 Jolt / Deno 物化链路

## 项目结构

```
Jazor/
├── src/
│   ├── ECMAScript/                         # ECMAScript AST 核心类型与特性
│   ├── ECMAScript.WebIDL/                  # WebIDL 绑定生成器（TypeScript，已归档）
│   ├── ECMAScript.WebIDL.Generator/        # WebIDL 绑定生成器（.NET）
│   ├── ECMAScript.WebIDL.GeneratorTest/    # WebIDL 生成器测试（MSTest）
│   ├── Jazor.Compiler/                     # C# → JS 编译器核心（SemanticWalker 分文件组织）
│   ├── Jazor.Compiler.Generator/           # 白名单 Source Generator
│   ├── Jazor.Compiler.Razor/               # 编译器 Razor 集成层
│   ├── Jazor.CompilerTest/                 # 编译器 + Jolt + LSP 测试（MSTest）
│   ├── Jazor.CLR/                          # CLR 运行时支持（白名单声明 + JS 实现）
│   ├── Jazor.CLR.Generator/                # CLR 类型映射与绑定代码生成器
│   ├── Jazor.CLR.Test/                     # CLR 测试（MSTest）
│   ├── Jazor.Common/                       # 跨项目共享契约
│   ├── Jazor.Name/                         # 符号格式化与哈希命名工具
│   ├── Jazor.Emit/                         # 发射管线、打包物化、SourceMap 输出
│   ├── Jazor.EmitTest/                     # 发射测试（MSTest）
│   ├── Jazor.Analyzer/                     # 静态代码分析器（白名单编译时验证）
│   ├── Jazor.Razor/                        # Razor 基础层（桥接 ASP.NET Core）
│   ├── Jazor.RazorVue/                     # 【RazorVue 线路】核心语义
│   ├── Jazor.RazorVue.Analysis/            # 【RazorVue 线路】编译时分析
│   ├── Jazor.RazorVue.Test/                # 【RazorVue 线路】测试（MSTest）
│   ├── Jazor.RazorVue.Vuetify/             # 【RazorVue 线路】Vuetify 3 组件库
│   ├── Jazor.Test/                         # Jazor 集成测试（MSTest）
│   ├── Jolt/                               # 【Jolt 线路】LSP + DevServer + HMR + Debug + Build
│   ├── Jolt.Test/                          # Jolt 测试（MSTest）
│   ├── Jolt.VSCodeExtension/               # VS Code Language Client 扩展
│   └── Jazor/                              # NuGet 包（运行时 + 分析器 + 生成器 + MSBuild）
├── samples/                                # 示例项目
├── docs/                                   # 文档中心（01-目标/02-计划/03-完成/04-补充/05-遗弃）
├── scripts/                                # 构建与工具脚本
└── README.md
```

> **注**：`ECMAScript.WebIDL`（TypeScript 版）已归档，非当前活跃方向。

## 核心组件

- **Jazor.Compiler** — C# 到 JavaScript 的编译器核心 [→ 文档](src/Jazor.Compiler/README.md)
- **Jazor.Analyzer** — 静态分析和白名单验证，确保编译时类型安全
- **Jazor.CLR** — .NET 类型的运行时模块支持，提供 JavaScript 运行时实现
- **Jazor.Emit** — 产物输出和打包管道，处理 host-facing 输出 [→ 文档](src/Jazor.Emit/README.md)
- **Jazor.RazorVue** — Vue 导向的 Razor 编译路径，支持 Blazor 风格的组件编写 [→ 设计](docs/01-目标/razorvue/README.md)
- **Jolt** — `.jazor` 全功能开发时边界：LSP + DevServer + HMR + Debug + Build [→ 设计](docs/01-目标/jolt/README.md) [→ 状态](docs/03-完成/jolt/status.md)

`.jazor` 以 Razor 作为源码语法，Vue 相关产物仅作为内部投影或桥接工件。

## 核心能力

Jazor 支持将 C# 代码转换为 JavaScript，包括：

- 变量声明和基础类型转换
- 模式匹配和条件表达式
- 可空类型处理
- 异步编程（async/await）
- 字符串插值（模板字符串）
- 对象和集合初始化
- 元组与解构
- switch 语句和表达式
- 循环（for / foreach / while / do-while）

详见 [编译器文档](src/Jazor.Compiler/README.md) 了解支持的完整特性。

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
