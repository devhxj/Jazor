<div align="center">

![今日诗词](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)

<h1>Jazor</h1>

<p><strong>将强类型 C# 与 Razor 编译为确定性 ECMAScript 模块和 Vue 渲染函数的 .NET 工具链。</strong></p>

<p>
  <a href="https://dotnet.microsoft.com/"><img alt=".NET 11 Preview 6" src="https://img.shields.io/badge/.NET-11%20Preview%206-512BD4?logo=dotnet&amp;logoColor=white" /></a>
  <a href="https://www.nuget.org/packages/Jazor"><img alt="NuGet" src="https://img.shields.io/nuget/v/Jazor?logo=nuget&amp;label=NuGet" /></a>
  <a href="https://github.com/devhxj/Jazor/releases/latest"><img alt="GitHub release" src="https://img.shields.io/github/v/release/devhxj/Jazor?display_name=tag&amp;label=release" /></a>
  <a href="https://github.com/devhxj/Jazor/actions/workflows/razorvue-ci.yml"><img alt="RazorVue CI" src="https://github.com/devhxj/Jazor/actions/workflows/razorvue-ci.yml/badge.svg?branch=main" /></a>
  <a href="LICENSE.txt"><img alt="MIT 许可证" src="https://img.shields.io/badge/license-MIT-2ea44f" /></a>
</p>

<p>
  <a href="src/Jazor.CompilerTest/README.md"><img alt="8285 项编译器测试通过" src="https://img.shields.io/badge/compiler%20tests-8285%20passed-2ea44f" /></a>
  <a href="docs/03-%E5%AE%8C%E6%88%90/compiler/status.md"><img alt="编译器行覆盖率 96.28%" src="https://img.shields.io/badge/compiler%20line%20coverage-96.28%25-2ea44f" /></a>
  <a href="docs/03-%E5%AE%8C%E6%88%90/compiler/status.md"><img alt="编译器分支覆盖率 90.01%" src="https://img.shields.io/badge/compiler%20branch%20coverage-90.01%25-2ea44f" /></a>
</p>

<p><a href="README.md">English</a> · <strong>简体中文</strong></p>

</div>

> [!IMPORTANT]
> Jazor 仍处于实验阶段。公共 API、生成产物形态和工具链可能继续演进；编译器核心、`Jazor.Emit` 管线和 Razor SG 绑定边界是当前最稳定的基础。

Jazor 是一套使用 C# 和 Razor 构建 JavaScript 与 Vue 应用的 .NET 工具链。

核心包提供编译器、运行时契约、分析器、emit 工具和 MSBuild 集成。Razor-to-Vue 转换通过 `Jazor.Vue` 显式启用：官方 Razor 源生成器的输出会绑定为 Roslyn `IOperation`，并降低为 Vue render-function `.mjs` 产物。

实现由 `Jazor.Compiler`、`Jazor.CLR`、`Jazor.Analyzer`、`Jazor.Emit`、`Jazor.Common` 以及 ECMAScript / Vue 绑定程序集组成。

## 已验证的编译器基线

`Jazor.Compiler` 使用真实 Roslyn `IOperation` 操作图进行验证，并通过 Acornima ESTree 生成 JavaScript。当前可复验基线记录于 2026-08-04：

| 指标 | 验证结果 | 强制阈值 |
|------|----------|----------|
| 编译器回归测试 | 8285 / 8285 通过 | 至少通过 8000 项 |
| 行覆盖率 | 15536 / 16137（96.28%） | 95% |
| 分支覆盖率 | 6219 / 6909（90.01%） | 90% |

在仓库根目录运行正式覆盖率门禁：

```bash
dotnet run --file scripts/csharp/verify-compiler-coverage.cs
```

该门禁会运行完整编译器测试套件，读取本次 TRX 与 Cobertura 报告；测试数量或覆盖率未达到阈值时以非零状态退出。当前范围和统计方法见[编译器状态](docs/03-%E5%AE%8C%E6%88%90/compiler/status.md)与[编译器测试指南](src/Jazor.CompilerTest/README.md)。

## RazorVue 验证基线

官方 Razor SG 输出会经过 Roslyn 绑定、直接 Vue render-function 生成、源映射、catalog 产物与 Deno.host 运行时场景验证。当前可复验基线为：

| 指标 | 验证结果 | 强制阈值 |
|------|----------|----------|
| RazorVue SG 场景 | 4482 / 4482 通过 | 至少通过 4000 项 |
| 行覆盖率 | 8147 / 8719（93.44%） | 90% |
| 分支覆盖率 | 3569 / 4265（83.68%） | 80% |

在仓库根目录运行 RazorVue 覆盖率门禁：

```bash
dotnet run --file scripts/csharp/verify-razorvue-coverage.cs
```

## 架构

- **语义降低**：Roslyn `IOperation` 被转换为 Acornima ESTree，并保持明确的支持边界与确定性输出。
- **Razor 集成**：`Jazor.Vue` 从 `GeneratorDriver.RunGeneratorsAndUpdateCompilation` 取得最终 `Compilation`，并绑定生成的 `BuildRenderTree` 操作。Razor DR/IR、宿主输出文档和生成 C# 的二次解析不属于生产边界。
- **产物契约**：Razor 组件生成 Vue render-function `.mjs` 模块；`Jazor.Emit` 负责物化模块、源映射、清单、运行时资产和生产包。
- **类型化绑定**：Vue 3 核心绑定随 `Jazor` 提供；Pinia、Vue Router、Vuetify 和其他生态绑定以独立包方式引用。

## 能力

- **语义级 C# 降低**：基于 Roslyn `IOperation`，而非语法字符串替换。
- **快速失败的宿主边界**：不支持的外部运行时语义会在实际降低使用点明确报错，不会静默生成近似 JavaScript。
- **白名单约束的 CLR API**：常用 CLR API 由 `Jazor.CLR` 与生成的白名单元数据映射；分析器可提前诊断大量不支持的用法。
- **ECMAScript 模块输出**：`[ECMAScriptModule]` 类生成具名导出的 `.mjs` 模块，并提供稳定的导入收集、源位置跟踪和源映射载体。
- **Razor-to-Vue 产物生成**：Razor 组件语义从官方 Razor SG 生成的 C# 出发，经 Roslyn 绑定和编译器持有的 `IOperation` 降低。
- **类型化 Vue 编写**：`ECMAScript.Vue3` 提供 Vue 3 `defineComponent`、`h`、响应式引用、生命周期、props、slots 和组件契约绑定。
- **面向宿主的构建支持**：MSBuild 为 ECMAScript 与 RazorVue 产物选择一种输出模式：不输出、`debug` 模块与清单，或通过 Deno / Netpack 工具链生成 `release` 生产包。

## 最新更新

### 2026-08-04

- `DateTime` 与 `DateTimeOffset` 的 GregorianCalendar 构造函数族现通过共享日期运行时执行，并保留 null 参数优先级、kind、微秒和 offset 校验。
- `StringBuilder` 现包含容量感知的构造与扩容、容量 API、字符串追加/换行追加和内容相等比较，并统一遵循同一状态载体契约。
- `string.Intern` 现通过字符串载体执行；表达式树和 `IQueryable` lambda 转换会明确失败，不再被误作可执行 delegate。
- 原生 `ECMAScript.Array`、`Set` 与 `Map` 支持标准 C# 集合初始化；`Map` 的索引器和双参数元素会复用既有 `set` 契约。
- C# `for` 循环内创建的闭包现保留控制变量的 C# 生命周期，不再继承 JavaScript `let` 每轮创建绑定的行为。
- Compiler 门禁已验证 8,285 个场景，行覆盖率 96.28%、分支覆盖率 90.01%；RazorVue 门禁已验证 4,482 个官方 SG 场景，行覆盖率 93.44%、分支覆盖率 83.68%。

完整历史见 [release notes](docs/releases/release-notes.md)。

## 安装

```bash
dotnet add package Jazor --version 0.1.41
```

`Jazor` 包包含核心运行时契约、`ECMAScript`、`ECMAScript.Vue3`、`ECMAScript.VueContract`、`Jazor.Compiler`、`Jazor.Analyzer`、ASP.NET Core 集成程序集、emit 工具和 MSBuild props/targets。Razor-to-Vue 生成由独立的 `Jazor.Vue` 包提供。

Razor SDK 项目需显式启用：

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.40" />
  <PackageReference Include="Jazor.Vue" Version="0.1.40" PrivateAssets="all" />
</ItemGroup>
```

按需显式添加生态包：

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.40" />
  <PackageReference Include="ECMAScript.Style" Version="0.1.40" />
  <PackageReference Include="ECMAScript.Pinia" Version="0.1.40" />
  <PackageReference Include="ECMAScript.VueRoute" Version="0.1.40" />
  <PackageReference Include="ECMAScript.Vuetify" Version="0.1.40" />
</ItemGroup>
```

## 编写方式

### ECMAScript 模块

使用 `[ECMAScriptModule]` 将普通 C# 生成 JavaScript 模块：

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

编译器会生成具名导出的 ECMAScript 模块。其他模块调用 `GreetingModule.Compose(...)` 时，跨模块导入会自动解析。

### Vue 3 `h()` 组件

直接使用 C# 编写 Vue 组件时，引用 `ECMAScript.Vue3`：

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

### Razor-to-Vue 组件

Razor 组件仅以最终 Roslyn 编译结果作为生产输入：

- 在声明 `.razor` 或 `.razor.cs` 组件的项目中引用 `Jazor.Vue`。
- 集成层从完成后的 Razor 源生成器编译结果中绑定生成的 `BuildRenderTree` 操作。
- `Jazor.Compiler` 降低已绑定的语义，`Jazor.Emit` 物化 Vue render-function 产物。
- 不需要 `EnableRazorHostOutputs`、Razor 宿主输出设置、Razor IR/文档模型或生成 C# 的二次解析。

实现细节见 [Razor-to-Vue 设计](docs/01-%E7%9B%AE%E6%A0%87/razorvue/README.md)。

### 确定性 CSS-in-JS

应用需要结构化运行时样式时，显式引用 `ECMAScript.Style`：

```csharp
using ECMAScript.Style;
using static ECMAScript.Style.css;

var actionClass = style(new CssRule
{
    Display = inlineFlex,
    Gap = rem(0.5),
    Width = percent(100) - rem(2),
    Color = varOr("--action-color", color("white")),
    BackgroundColor = hex("1769aa"),
    Children =
    [
        new(CssChildKind.Selector, "&:hover", new CssRule
        {
            BackgroundColor = hex("125486")
        })
    ]
});
```

该包依据锁定的 Webref 语法快照生成 705 个标准属性。C# 原生 union 区分长度、百分比、颜色、时间、display 等值域；`raw(...)` 显式承载未来或尚未建模的语法。稳定内容命名、支持 nonce 的 `document` / `ShadowRoot` 所有权、detached 提取与水合共享同一运行时合同。`style(...)` 返回普通字符串，可直接用于常规模块和 RazorVue `class` 属性；构建不增加 CSS 专用 MSBuild 属性，debug 模式在 `JazorDir` 下物化 `style.mjs`。

详细合同见 [ECMAScript.Style 包指南](src/ECMAScript.Style/README.md) 与 [目标边界](docs/01-%E7%9B%AE%E6%A0%87/ecmascript.style/README.md)。

## MSBuild 属性

由于 `JazorMode` 默认值为 `none`，类库无需配置输出。

开发构建使用 debug 产物时，配置如下：

```xml
<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorDir>
</PropertyGroup>
```

生产发布生成 bundle 时，配置如下：

```xml
<PropertyGroup>
  <JazorMode>release</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorDir>
  <JazorTool>Deno</JazorTool>
</PropertyGroup>
```

`debug` 与 `release` 互斥。`release` 在内部完成中间物化，清空 `JazorDir` 后仅在该目录写出 `bundle.js` 和 `bundle.js.map`。

| 属性 | 默认值 | 说明 |
|------|--------|------|
| `JazorMode` | `none` | `none` 不输出；`debug` 写出模块和清单；`release` 写出生产包。 |
| `JazorDir` | `$(MSBuildProjectDirectory)\wwwroot\jazor\` | debug 模块或 release 生产包的输出根目录。 |
| `JazorTool` | `Deno` | 选择 release 工具链，目前支持 `Deno` 或 `Netpack`。 |

包和 emit 细节见 [src/Jazor/README.md](src/Jazor/README.md) 与 [src/Jazor.Emit/README.md](src/Jazor.Emit/README.md)。

## 仓库结构

```text
Jazor/
├── src/
│   ├── Jazor.Compiler/              # C# -> JavaScript 编译器核心
│   ├── Jazor.CLR/                   # CLR runtime 映射和 JavaScript helper
│   ├── Jazor.Analyzer/              # 静态分析诊断
│   ├── Jazor.RazorVue/              # Generator 集成、SG 结果绑定与 Vue render framing
│   ├── Jazor.Emit/                  # 物化、manifest、source map 与打包
│   ├── Jazor.Admin/                 # UI 无关的管理后台外壳契约与 Razor 组件
│   ├── JazorAdmin/                  # TDesign 集成示例与包消费冒烟验证
│   ├── ECMAScript.Style/            # 强类型、确定的 CSS-in-JS runtime
│   ├── Jazor.Common/                # 共享格式化 / source-map 工具和契约
│   ├── Jazor.AspNetCore*/           # ASP.NET Core runtime 与开发期集成
│   ├── Jazor/                       # NuGet 包，打包核心 SDK 资产
│   ├── Jazor.Vue/                   # 显式启用的 Razor-to-Vue NuGet 包
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
dotnet run --file scripts/csharp/verify-compiler-coverage.cs
dotnet run --file scripts/csharp/test-dotnet.cs -- --project style
dotnet run --file scripts/csharp/test-dotnet.cs -- --project style-browser
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
| ECMAScript.Style 设计与状态 | [docs/01-目标/ecmascript.style/README.md](docs/01-%E7%9B%AE%E6%A0%87/ecmascript.style/README.md)、[docs/03-完成/ecmascript.style/status.md](docs/03-%E5%AE%8C%E6%88%90/ecmascript.style/status.md) |
| 架构转型计划 | [docs/02-计划/Jazor 架构转型开发计划.md](docs/02-%E8%AE%A1%E5%88%92/Jazor%20%E6%9E%B6%E6%9E%84%E8%BD%AC%E5%9E%8B%E5%BC%80%E5%8F%91%E8%AE%A1%E5%88%92.md) |
| G0 决策记录 | [docs/02-计划/RazorSgFinalDocument.G0.DecisionRecord.md](docs/02-%E8%AE%A1%E5%88%92/RazorSgFinalDocument.G0.DecisionRecord.md) |
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
