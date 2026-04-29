<div align="center">

![Today's Verse](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)
</div>

# Jazor - C# to JavaScript Compiler and `.jazor` Tooling

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.txt)

> Experimental. Public APIs, generated output shape, and adjacent tooling are still being tightened.

Jazor 是一套以 Roslyn `IOperation -> ECMAScript AST` lowering 为核心的 C# -> JavaScript 工具链。当前仓库有两条活跃技术线路：编译时库模式的 `RazorVue`，以及 `.jazor` 开发时宿主 `Jolt`。

## 当前架构

| 线路 | 对外边界 | 当前物理落点 | 说明 |
|------|---------|-------------|------|
| **RazorVue** | `Jazor.RazorVue` + `Jazor.RazorVue.Analysis` + `ECMAScript.Vuetify` | `src/Jazor.Common/RazorVue/` + `src/Jazor.Analyzer/RazorVue/` + `src/ECMAScript.Vuetify/` | Source Generator 驱动的编译时 Razor-to-JS 路线，不以 `.vue` 作为 authoring 格式 |
| **Jolt** | `Jolt` | `src/Jolt/` | `.jazor` 开发时宿主，承载 LSP、DevServer、HMR、Build、Debug 与 Deno/Volar 前端语义 |

> 迁移说明：`src/Jazor.RazorVue/`、`src/Jazor.RazorVue.Analysis/`、`src/Jazor.RazorVue.Vuetify/`、`src/Jazor.Name/`、`src/ECMAScript.Internal/` 可能仍作为历史兼容目录存在，但它们已经不是当前解决方案里的活跃项目边界。

## 关键模块

| 项目 | 角色 |
|------|------|
| `src/ECMAScript/` | ECMAScript 特性、运行时投影类型与 AST 基础定义 |
| `src/ECMAScript.Contract/` | 依赖零污染的最小契约层，承载 `JazorAttribute`、`Op`、`IUIComponent` |
| `src/ECMAScript.Vuetify/` | Vuetify 绑定与 RazorVue 组件桩 |
| `src/Jazor.Common/` | 共享实现层，承载 `Format`、SourceMap、Emit 共享模型、Vue/Jolt 协议 DTO、RazorVue 共享语义 |
| `src/Jazor.Compiler/` | C# -> JavaScript 编译器主线 |
| `src/Jazor.Compiler.Generator/` | 白名单生成工具，生成 `WhiteList.cs.*` 和 `SemanticWalker.cs.Generate.cs` |
| `src/Jazor.Compiler.Razor/` | Razor 语义前端桥接 |
| `src/Jazor.CLR/` | CLR 映射声明与 JavaScript 语义实现 |
| `src/Jazor.Analyzer/` | 白名单静态分析 + RazorVue 编译时分析/生成器宿主 |
| `src/Jazor.Emit/` | 发射、物化、打包与 RazorVue diff 输出 |
| `src/Jazor.Razor/` | 最薄的 Razor 基础标记层 |
| `src/Jazor/` | NuGet 打包入口（运行时、分析器、生成器、MSBuild 集成） |
| `src/Jolt/` | `.jazor` 全功能开发时宿主 |

## ECMAScript 特性约定

- `[ECMAScript("jsr:@scope/pkg")]`、`[ECMAScript("npm:vue@3")]`、`[ECMAScript("https://...")]` 用于声明 **Deno 可解析的导入地址**。
- `[ECMAScriptModule("features/todo/index.mjs")]` 用于声明 **本类型发射后的模块路径**，它不是包解析地址。
- CLR 和宿主映射的 producer 侧事实由 `[Jazor(...)]` 声明，仓库内不再使用旧的 `[WhiteList]` 特性名。

示例：

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

## 文档入口

| 角色 | 入口 |
|------|------|
| 仓库总览 | [docs/README.md](docs/README.md) |
| 架构设计 | [docs/01-目标/README.md](docs/01-目标/README.md) |
| 实施计划 | [docs/02-计划/README.md](docs/02-计划/README.md) |
| 当前状态 | [docs/03-完成/README.md](docs/03-完成/README.md) |
| 编译器主线 | [src/Jazor.Compiler/README.md](src/Jazor.Compiler/README.md) |
| Jolt | [src/Jolt/README.md](src/Jolt/README.md) |

## 仓库结构

```text
Jazor/
├── src/
│   ├── ECMAScript/
│   ├── ECMAScript.Contract/
│   ├── ECMAScript.Vue/
│   ├── ECMAScript.Vuetify/
│   ├── ECMAScript.WebIDL.Generator/
│   ├── Jazor.Common/
│   ├── Jazor.Compiler/
│   ├── Jazor.Compiler.Generator/
│   ├── Jazor.Compiler.Razor/
│   ├── Jazor.CLR/
│   ├── Jazor.Analyzer/
│   ├── Jazor.Emit/
│   ├── Jazor.Razor/
│   ├── Jazor/
│   ├── Jolt/
│   └── *Test/
├── docs/
├── samples/
└── scripts/
```

## 构建与测试

```powershell
dotnet restore Jazor.slnx
dotnet build Jazor.slnx
pwsh ./scripts/test-dotnet.ps1

dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj
dotnet test src/Jolt.Test/Jolt.Test.csproj
```

## 许可证

本项目采用 MIT 许可证。详见 [LICENSE.txt](LICENSE.txt)。
