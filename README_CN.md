<div align="center">

![今日诗词](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)

<h1>Jazor</h1>

<p><strong>将受支持的 C# 语义编译为确定性 ECMAScript 模块的强类型 .NET 工具链。</strong></p>

<p>
  <a href="https://dotnet.microsoft.com/"><img alt=".NET 11 Preview" src="https://img.shields.io/badge/.NET-11%20Preview-512BD4?logo=dotnet&amp;logoColor=white" /></a>
  <a href="https://www.nuget.org/packages/Jazor"><img alt="NuGet" src="https://img.shields.io/nuget/v/Jazor?logo=nuget&amp;label=NuGet" /></a>
  <a href="https://github.com/devhxj/Jazor/releases/latest"><img alt="GitHub release" src="https://img.shields.io/github/v/release/devhxj/Jazor?display_name=tag&amp;label=release" /></a>
  <a href="https://github.com/devhxj/Jazor/actions/workflows/razorvue-ci.yml"><img alt="Razor-to-Vue CI" src="https://github.com/devhxj/Jazor/actions/workflows/razorvue-ci.yml/badge.svg?branch=main" /></a>
  <a href="LICENSE.txt"><img alt="MIT 许可证" src="https://img.shields.io/badge/license-MIT-2ea44f" /></a>
</p>

<p>
  <a href="docs/04-roadmap/current-status.md"><img alt="至少 10,000 项编译器测试通过" src="https://img.shields.io/badge/compiler%20tests-10%2C000%2B%20passing-2ea44f" /></a>
  <a href="docs/04-roadmap/current-status.md"><img alt="编译器行覆盖率至少 98%" src="https://img.shields.io/badge/compiler%20line%20coverage-98%25%2B-2ea44f" /></a>
  <a href="docs/04-roadmap/current-status.md"><img alt="编译器分支覆盖率至少 97%" src="https://img.shields.io/badge/compiler%20branch%20coverage-97%25%2B-2ea44f" /></a>
</p>

<p><a href="README.md">English</a> · <strong>简体中文</strong></p>

</div>

> Jazor 仍处于实验阶段，公共 API 与生成产物形态可能继续演进。

Jazor 是一套将受支持 C# 语义转换为确定性 ECMAScript 模块的强类型 .NET 工具链。它的核心不依赖 Vue、React 或其他 UI 框架：Roslyn 提供语义模型，`Jazor.Compiler` 将其降低为 ESTree，`Jazor.Emit` 负责物化浏览器产物。

Razor-to-Vue 是建立在该核心之上的一个应用方向。`Jazor.RazorVue` 绑定官方 Razor Source Generator 的最终输出，再将所有 C# 表达式和成员语义交给同一套 Jazor 编译器，最后组装 Vue render-function 模块。

## 最新更新

### Jazor 0.26.0 - 2026-08-31

- 类库携带 JavaScript 只允许两种形式：已有资源使用包内 `manifest.json + dist/**`；由 Jazor 编译的 C# 写入程序集内部的 `Jazor.Generated.ModuleCatalog`。
- 生成模块、source map、package import、相对依赖、HMR 元数据和所属资源共用唯一的 `ModuleCatalog` 入口，不存在 provider 或并列 artifact/source-map catalog。
- 工具遵循“谁使用，谁直接引用”：编写 Jazor 代码的项目直接引用 `Jazor` 或 `Jazor.Vue`；普通类库引用只传递其声明的模块/资源依赖。
- 宿主完成编译后，MSBuild 只调用一次 `Jazor.Emit`。Emit 解析选中的依赖闭包，并将 Debug、Release 或 SSR 投影原子物化到 `JazorDir`。

完整版本历史见 [CHANGELOG](CHANGELOG.md)。

## 致谢

Jazor 使用了 [Roslyn](https://github.com/dotnet/roslyn)、[Acornima](https://github.com/adams85/acornima)、[Netpack](https://github.com/FlorianRappl/netpack)、[DenoHost](https://github.com/thomas3577/DenoHost)、[WebRef](https://github.com/w3c/webref)，并参考了 [WootzJs](https://github.com/kswoll/WootzJs)、[h5](https://github.com/curiosity-ai/h5)、[SharpKit](https://github.com/SharpKit/SharpKit) 等早期 C# 到 JavaScript 项目。

## 核心模型

```mermaid
flowchart LR
    subgraph Core["Jazor 核心平台：C# -> ECMAScript"]
        CSharp["C# 模块"] --> Roslyn["Roslyn 语义模型"]
        Roslyn --> Compiler["Jazor.Compiler"]
        Bindings["CLR 与 ECMAScript 绑定"] --> Compiler
        Compiler --> Ast["ESTree"] --> Emit["Jazor.Emit"]
        Emit --> Artifacts[".mjs、源映射、manifest、bundle"]
    end

    subgraph Integrations["框架集成层"]
        Razor["Razor 组件"] --> RazorSG["官方 Razor SG"] --> Compilation["最终 Compilation"]
        Compilation --> RazorVue["Jazor.RazorVue"]
        RazorVue -. 调用核心翻译钩子 .-> Compiler
        RazorVue --> Emit
    end
```

`Jazor.RazorVue` 是当前已实现的框架集成。未来的 `Jazor.React`、`Jazor.RazorReact` 等方向可以复用同一核心，但目前不是已支持的 API。

## 质量门槛

顶部徽标展示持续适用的验收门槛，而非会过期的单次构建结果。仓库通过可复现脚本验证以下最低要求：

- 核心编译器：至少 10,000 个通过的 `IOperation` 场景、98% 行覆盖率和 97% 分支覆盖率。
- 当前 Razor-to-Vue 集成：至少 4,000 个通过场景、90% 行覆盖率和 94% 分支覆盖率；该门槛会在集成完善后再提高。
- Vue 生态绑定：每个目标至少 90% 的已审计公共绑定契约覆盖率。

可在 `scripts/csharp/` 下运行 `verify-compiler-coverage.cs`、`verify-razorvue-coverage.cs` 或 `verify-vue-binding-coverage.cs` 复现相应门槛。当前范围与测试入口见[当前状态](docs/04-roadmap/current-status.md)。

## 包组成

| 包 | 职责 |
| --- | --- |
| `Jazor` | 框架无关的编译器、CLR 契约、分析器、emit 工具、MSBuild 与 ASP.NET Core 集成，可用于普通 ECMAScript 类库 |
| `Jazor.Vue` | Vue authoring、Razor-to-Vue opt-in、Vue runtime 资源，以及 `ECMAScript.Vue`、`ECMAScript.VueContract`、`ECMAScript.Blazor` payload |
| `ECMAScript.*` | 框架无关 ECMAScript 绑定、可选 Vue 生态绑定与 CSS-in-JS 类库 |
| `ECMAScript.VueDataUi` | `vue-data-ui` 的强类型 RazorVue 图表与按组件本地 ESM 物化 |
| `ECMAScript.VuIcons` | `vu-icons` 的强类型 RazorVue 图标，支持静态单图标与动态 catalog 路径 |
| `Jazor.Admin` | UI 库无关的管理壳库与 RazorVue 组件 |

`samples/JazorAdmin` 是消费 `Jazor.Admin` 的生产级管理参考应用，不属于该库的公共契约。

## 安装

在声明 ECMAScript 模块的每个项目中安装核心包：

```bash
dotnet add package Jazor --version 0.26.0
```

需要当前 Razor-to-Vue 集成的 Razor SDK 项目，必须显式添加 opt-in 包，并保持版本一致：

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.26.0" />
  <PackageReference Include="Jazor.Vue" Version="0.26.0" PrivateAssets="all" />
</ItemGroup>
```

完整的包选择、输出设置、SSR 配置与生态绑定见[安装与配置](docs/03-guides/installation-and-configuration.md)。

## 第一个模块

使用 `[ECMAScriptModule]` 使 C# 模块进入 JavaScript 发射范围：

```csharp
using ECMAScript;

namespace MyApp;

[ECMAScriptModule("shared/greetings.mjs")]
public static class GreetingModule
{
    public static string Compose(string name) => $"Hello, {name}";
}
```

核心编译器会生成标准的具名导出 ECMAScript 模块。跨模块调用由编译器维护 import，不需要手写 JavaScript。

完整的可运行路径见[快速开始](docs/03-guides/quick-start.md)。

## 输出模式

可执行项目或 Web 宿主通过 MSBuild 选择产物模式：

```xml
<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)\jazor\</JazorDir>
</PropertyGroup>
```

| 模式 | 结果 |
| --- | --- |
| `none` | 默认值，不写入 Jazor 产物 |
| `debug` | 可检查的模块、外部 source map 与 `jazor-manifest.json` |
| `release` | 通过内置 Netpack 路径生成生产浏览器包 |

ASP.NET Core 应用需要 Vue SSR 与 hydration 时，按支持的 SSR 配置设置 `JazorSSR=true`。详见[产物管线](docs/02-architecture/artifact-pipeline.md)。

## 文档

| 需求 | 入口 |
| --- | --- |
| 产品总览 | [docs/README.md](docs/README.md) |
| 核心编译器架构 | [编译器](docs/02-architecture/compiler.md) |
| 框架集成规则 | [框架集成层](docs/02-architecture/framework-integrations.md) |
| 当前 Razor-to-Vue 实现 | [Razor-to-Vue](docs/02-architecture/razor-to-vue.md) |
| 安装、配置与编写 | [使用指南](docs/03-guides/README.md) |
| 示例 | [示例](docs/03-guides/examples.md) |
| 当前范围 | [路线图](docs/04-roadmap/current-status.md) |
| 历史背景 | [演进记录](docs/05-history/evolution.md) |
| 版本历史 | [CHANGELOG.md](CHANGELOG.md) |

## 开发

使用 [global.json](global.json) 指定的 .NET 11 SDK preview。在仓库根目录执行：

```bash
dotnet restore Jazor.slnx
dotnet build Jazor.slnx
dotnet run --file scripts/csharp/test-dotnet.cs
```

常用聚焦测试：

```bash
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj
```

仓库自动化使用 `scripts/csharp/` 下的单文件 C# 入口。完整流程见[开发与测试](docs/03-guides/development-and-testing.md)。

## 许可证与反馈

Jazor 使用 [MIT 许可证](LICENSE.txt)。安全问题请通过 [GitHub Security Advisories](https://github.com/devhxj/Jazor/security/advisories/new) 私下报告；其他问题可使用 [Issues](https://github.com/devhxj/Jazor/issues) 或 [Discussions](https://github.com/devhxj/Jazor/discussions)。
