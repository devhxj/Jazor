[English](README.md) | **中文**

<div align="center">

![今日诗词](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)
</div>

# Jazor

[![.NET](https://img.shields.io/badge/.NET-11.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.txt)
[![NuGet](https://img.shields.io/nuget/v/Jazor.svg)](https://www.nuget.org/packages/Jazor)

> 实验性项目。公共 API、生成产物形态和工具链仍在演进中。当前最稳定的基础是编译器核心、emit 管线和 SG 结果绑定边界。

Jazor 是一套用 C# 和 Razor 编写 JavaScript / Vue 应用的 .NET 工具链。

转型分支当前只有一条活跃的 Razor-to-Vue 方向。原 Jolt 宿主仅保留在 Git 历史中：

| 线路 | 模式 | 主要项目 | 作用 |
|------|------|----------|------|
| **Razor-to-Vue 架构转型** | 活跃 | `Jazor.RazorVue`、`Jazor.Analyzer`、`Jazor.Compiler`、`Jazor.Emit` | 官方 Razor SG 生成 C# -> Roslyn `IOperation` -> Vue render-function `.mjs`。 |
| **Jolt** | 本分支已退役 | 基线 `d68aecbb00b23aa35735c9a269b2e987c7815b05` | 历史 `.jazor` LSP/DAP/DevServer/build 宿主；不在当前项目图中。 |

当前主线复用 `Jazor.Compiler`、`Jazor.CLR`、`Jazor.Analyzer`、`Jazor.Emit`、`Jazor.Common` 和 ECMAScript / Vue 绑定程序集，不继承 Jolt 协议或状态机。

## 当前重点

- **编译器核心**：Roslyn `IOperation` 到 Acornima ESTree 的 lowering，强调明确支持边界和确定性发射。
- **SG 结果输入**：受控 Razor SG tail 消费官方生成的 C# 文档并复用 callback compilation 派生链；Razor DR/IR 不是生产输入。
- **单一产物方向**：Razor 组件目标是 Vue render-function `.mjs`；转型分支不维护 Razor-to-SFC 或 Jolt 兼容路径。
- **Emit 与物化**：`Jazor.Emit` 写出 `.mjs`、manifest、source map 和 bundle 产物。
- **Vue 生态绑定**：Vue 3 核心绑定随 `Jazor` 包提供；Pinia、Vue Router、Vuetify 和其他 UI / 库绑定作为显式生态项目维护。

判断当前状态时，优先看 `docs/03-完成/` 下的状态页和当前测试结果，不要依赖 README 中写死的历史测试数量。

## 能力概览

- **语义级 C# lowering**：基于 Roslyn `IOperation`，不是语法字符串替换。
- **fail-fast 宿主边界**：不支持的外部运行时语义会在实际 lowering 使用点显式失败，而不是静默发射近似 JavaScript。
- **白名单 CLR surface**：常见 CLR API 通过 `Jazor.CLR` 和生成的 whitelist metadata 映射；Analyzer 会提前诊断很多不支持的用法。
- **ECMAScript 模块输出**：`[ECMAScriptModule]` 类发射为 named-export `.mjs` 模块，并带稳定 import 收集、source-origin 跟踪和 source-map carrier。
- **RazorVue artifact 生成**：Razor 组件语义从官方 Razor SG 生成 C# 流经 Roslyn 绑定和 compiler-owned `IOperation` lowering。
- **类型化 Vue authoring**：`ECMAScript.Vue3` 提供 Vue 3 `defineComponent`、`h`、ref/reactivity、生命周期、props、slots 和组件契约绑定。
- **面向宿主的构建支持**：MSBuild target 当前可发射并物化通用 ECMAScript 模块、发布资产，并通过内置 Deno runtime 打包。RazorVue `.mjs` consumer build 属于当前转型计划，相关 gate 通过前不得假定已完成。
- **已退役 Jolt 边界**：`.jazor` authoring、Jolt LSP/DAP、DevServer/HMR、调试和构建协议均有意不进入本分支。

## 安装

```bash
dotnet add package Jazor
```

`Jazor` 包包含核心运行时契约、`ECMAScript`、`ECMAScript.Vue3`、`ECMAScript.VueContract`、`Jazor.Compiler`、`Jazor.RazorVue`、`Jazor.Analyzer`、ASP.NET Core 集成程序集、emit 工具和 MSBuild props/targets。

按需显式添加生态包：

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.26" />
  <PackageReference Include="ECMAScript.Pinia" Version="0.1.26" />
  <PackageReference Include="ECMAScript.VueRoute" Version="0.1.26" />
  <PackageReference Include="ECMAScript.Vuetify" Version="0.1.26" />
</ItemGroup>
```

## Authoring 路线

### ECMAScript 模块 authoring

用 `[ECMAScriptModule]` 发射普通 C# 到 JavaScript 模块：

```csharp
using ECMAScript;

namespace MyApp;

[ECMAScriptModule("shared/greetings.mjs")]
public static class GreetingModule
{
    public static string Prefix() => "Hello";
    public static string Compose(string name) => $"{Prefix()}, {name}";
}
```

编译器会发射 named-export ECMAScript 模块。其他模块调用 `GreetingModule.Compose(...)` 时，跨模块 import 会自动解析。

### Vue 3 h() authoring

直接用 C# 编写 Vue 组件时使用 `ECMAScript.Vue3`：

```csharp
using ECMAScript;
using static ECMAScript.Vue3;

namespace MyApp;

[ECMAScriptModule("app/counter.mjs")]
public static class CounterModule
{
    public static IVueComponent Counter
        => DefineComponent(new VueComponentOptions
        {
            Setup = () =>
            {
                var count = Ref(0);
                return () => H("button", new VueObject
                {
                    Events = new VueDictionary
                    {
                        ["click"] = (Action)(() => count.Value++)
                    }
                }, $"Count: {count.Value}");
            }
        });
}
```

### Razor-to-Vue 架构转型

当前工作流保留 Razor 组件 authoring，并收窄生产边界：

- 组件库编写 `.razor` / `.razor.cs` 组件；
- 受控 tail hook 消费官方 Razor SG 生成 C# 和 callback compilation 上下文；
- `Jazor.Compiler` 降低已绑定组件语义，`Jazor.Emit` 物化版本化 render-function artifact；
- Razor DR/IR、生成 SFC 和 Jolt 协议都不是 fallback 路径。

当前 gate 和实现顺序见 [架构转型开发计划](docs/02-%E8%AE%A1%E5%88%92/Jazor%20%E6%9E%B6%E6%9E%84%E8%BD%AC%E5%9E%8B%E5%BC%80%E5%8F%91%E8%AE%A1%E5%88%92.md)。

### 已退役 Jolt 宿主

Jolt 已在 `3ee18679fbdf43c13e05d7bfac8857ddcebd19f9` 从转型分支移除。维护或比较时使用基线 `d68aecbb00b23aa35735c9a269b2e987c7815b05` 或原分支；不要在此重新引入 `.jazor`、LSP/DAP、DevServer 或协议面。

## MSBuild 属性

| 属性 | 默认值 | 说明 |
|------|--------|------|
| `JazorCompile` | `true` | 启用 `[ECMAScriptModule]` 类型编译。 |
| `JazorEmit` | 可执行宿主为 `true`，库项目为 `false` | 构建后发射生成模块 / artifact。 |
| `JazorRazorVueEnableRazorSgIntegration` | `false` | 为显式选择的 RazorVue 路径启用 Razor Source Generator 集成。 |
| `JazorDevOutDir` | `$(MSBuildProjectDirectory)\jazor\` | compiler-owned artifact 的默认开发期输出根目录。 |
| `JazorPublishOutDir` | `$(MSBuildProjectDirectory)\wwwroot\jazor\` | 未启用 publish materialization 时的默认发布期浏览器资产根目录。 |
| `JazorOutDir` | `$(JazorDevOutDir)` | compiler-owned artifact 的当前选定输出目录。 |
| `JazorBundle` | `false` | 通过内置 Deno runtime 打包已发射模块。 |
| `JazorBundleOut` | `$(OutDir)jazor\app.js` | 打包 JavaScript 输出路径。 |
| `JazorCleanEmit` | `true` | 清理输出目录中过期的发射文件。 |
| `JazorFailOnPathConflict` | `true` | 两个模块声明同一输出路径时构建失败。 |
| `JazorPublishMaterializeEnabled` | `false` | 发布时把 compiler-owned RazorVue 输出物化到发布资产。 |

包和 emit 细节见 [src/Jazor/README.md](src/Jazor/README.md) 与 [src/Jazor.Emit/README.md](src/Jazor.Emit/README.md)。

## 仓库结构

```text
Jazor/
├── src/
│   ├── Jazor.Compiler/              # C# -> JavaScript 编译器核心
│   ├── Jazor.CLR/                   # CLR runtime 映射和 JavaScript helper
│   ├── Jazor.Analyzer/              # Analyzer 与 RazorVue source-generator 宿主
│   ├── Jazor.RazorVue/              # SG 结果绑定与 Vue render artifact framing
│   ├── Jazor.Emit/                  # 物化、manifest、source map 与打包
│   ├── Jazor.Common/                # 共享格式化 / source-map 工具和契约
│   ├── Jazor.AspNetCore*/           # ASP.NET Core runtime 与开发期集成
│   ├── Jazor/                       # NuGet 包，打包核心 SDK 资产
│   ├── ECMAScript*/                 # ECMAScript AST/contract 与 Vue 生态绑定
│   └── *Test/                       # MSTest 回归项目
├── samples/
│   ├── Jazor.MultiProject/          # 多项目模块发射基线示例
│   ├── ECMAScript.Pinia.Counter/    # Vue 3 + Pinia 示例
│   └── RazorVue.TodoList/           # 待转型的旧示例
├── docs/                            # 目标、计划、状态快照、补充规则、遗弃材料
└── scripts/csharp/                  # 仓库自动化脚本
```

## 开发

环境要求：

- 与 [global.json](global.json) 匹配的 .NET 11 SDK preview
- Windows、Linux 或 macOS
- 只有 `src/ECMAScript.WebIDL` 下已归档的 WebIDL TypeScript generator 需要 Node/npm

从仓库根目录运行常用命令：

```bash
dotnet restore Jazor.slnx
dotnet build Jazor.slnx

# 仓库主测试入口
dotnet run --file scripts/csharp/test-dotnet.cs

# 聚焦测试套件
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj

# 单个测试类示例
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest"
```

仓库自动化脚本应使用 `scripts/csharp/` 下的单文件 C# 入口；避免新增 PowerShell build/test wrapper。

## 文档

| 需求 | 入口 |
|------|------|
| 仓库文档中心 | [docs/README.md](docs/README.md) |
| 当前工作流总览 | [docs/02-计划/workstream-dashboard.md](docs/02-%E8%AE%A1%E5%88%92/workstream-dashboard.md) |
| 编译器实现原则 | [src/Jazor.Compiler/ImplementationPrinciples.md](src/Jazor.Compiler/ImplementationPrinciples.md) |
| 编译器状态 | [docs/03-完成/compiler/status.md](docs/03-%E5%AE%8C%E6%88%90/compiler/status.md) |
| RazorVue 设计 | [docs/01-目标/razorvue/README.md](docs/01-%E7%9B%AE%E6%A0%87/razorvue/README.md) |
| 架构转型计划 | [docs/02-计划/Jazor 架构转型开发计划.md](docs/02-%E8%AE%A1%E5%88%92/Jazor%20%E6%9E%B6%E6%9E%84%E8%BD%AC%E5%9E%8B%E5%BC%80%E5%8F%91%E8%AE%A1%E5%88%92.md) |
| G0 决策记录 | [docs/02-计划/RazorSgFinalDocument.G0.DecisionRecord.md](docs/02-%E8%AE%A1%E5%88%92/RazorSgFinalDocument.G0.DecisionRecord.md) |
| 已退役 Jolt 历史 | [docs/01-目标/jolt/README.md](docs/01-%E7%9B%AE%E6%A0%87/jolt/README.md) |
| Emit 状态 | [docs/03-完成/emit/status.md](docs/03-%E5%AE%8C%E6%88%90/emit/status.md) |

文档按以下目录组织：

- `docs/01-目标/`：目标和设计理由
- `docs/02-计划/`：计划、里程碑和工作拆分
- `docs/03-完成/`：状态快照和评审结果
- `docs/04-补充/`：治理规则和补充约束
- `docs/05-遗弃/`：已遗弃历史材料

`docs/03-完成/compiler/testing/` 应视为历史审计材料。判断当前 compiler 事实时，优先阅读 `src/Jazor.Compiler/ImplementationPrinciples.md`、`docs/03-完成/compiler/status.md` 和当前 compiler / test README。

## 贡献

欢迎贡献。请保持改动范围清晰，遵守仓库约定；当工作流边界或公共契约变化时，同步更新相关文档 / 状态页。

## 许可证

本项目采用 MIT 许可证。详见 [LICENSE.txt](LICENSE.txt)。

## 致谢

- [Roslyn](https://github.com/dotnet/roslyn) — C# 编译器平台
- [Acornima](https://github.com/adams85/acornima) — JavaScript 解析器和 AST 库
- [WebRef](https://github.com/w3c/webref) — Web 规范引用
- [DenoHost](https://github.com/thomas3577/DenoHost) — .NET 的 Deno runtime host
- [WootzJs](https://github.com/kswoll/WootzJs)、[h5](https://github.com/curiosity-ai/h5)、[SharpKit](https://github.com/SharpKit/SharpKit) — 早期 C# 到 JavaScript 编译器

## 安全策略

如果你发现安全漏洞，请通过 [GitHub Security Advisories](https://github.com/devhxj/Jazor/security/advisories/new) 私下报告。不要为安全问题创建公开 Issue。

## 反馈

- [报告 Bug](https://github.com/devhxj/Jazor/issues/new?template=bug_report.md)
- [功能请求](https://github.com/devhxj/Jazor/issues/new?template=feature_request.md)
- [讨论区](https://github.com/devhxj/Jazor/discussions)
