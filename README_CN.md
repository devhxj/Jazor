[English](README.md) | **中文**

<div align="center">

![今日诗词](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)
</div>

# Jazor - C# 到 JavaScript 编译器

[![.NET](https://img.shields.io/badge/.NET-11.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.txt)
[![NuGet](https://img.shields.io/nuget/v/Jazor.svg)](https://www.nuget.org/packages/Jazor)

> ⚠️ **实验性项目** ⚠️
> 公共 API、生成的输出形态以及工具链仍在演进中。编译器核心和 ECMAScript 模块发射是项目中最稳定的部分。

Jazor 是一个基于 Roslyn 的 C# 到 JavaScript 编译器，核心能力是将 `IOperation` 语义降低为 ECMAScript AST。用 `[ECMAScriptModule]` 标注 C# 类，构建时自动生成 `.mjs` 文件。内含 `ECMAScript.Vue3` 提供类型化的 Vue 3 `h()` 渲染函数绑定，也提供 `ECMAScript.Pinia` 的 Pinia store 绑定，以及 `ECMAScript.VueRoute` 的 Vue Router 绑定。

## 特性

- **C# 语法覆盖** — 基本支持 C# 15 全部语言特性：变量声明、基础类型、模式匹配、可空类型、async/await、字符串插值、对象和集合初始化、元组与解构、switch 语句/表达式，以及循环（for/foreach/while/do-while）
- **Roslyn `IOperation` lowering** — 通过 `IOperation` → ECMAScript AST 的语义驱动编译，而非语法层面的翻译
- **静态分析安全** — `Jazor.Analyzer` 在编译期强制执行白名单边界，在不支持的类型和成员进入发射之前即诊断报错
- **CLR 运行时模块** — 常见 BCL 类型（`string`、`int`、`double`、`List<T>`、`Dictionary<TKey, TValue>`、`Task<T>` 等）通过 `Jazor.CLR` 映射到 JavaScript 运行时实现
- **ECMAScript 模块发射** — `[ECMAScriptModule]` 标注的类编译为 `.mjs` 文件，自动处理跨模块 import 解析和 source map
- **Vue 3 集成** — `ECMAScript.Vue3` 提供 Vue 3 Composition API 和 `h()` 渲染函数的类型化 C# 绑定，支持纯 C# 编写组件
- **Pinia 集成** — `ECMAScript.Pinia` 提供 Pinia 根实例和 store authoring 的类型化 C# 绑定，包括 `createPinia()`、`defineStore()`、`storeToRefs()`，以及 `mapState()` / `mapActions()` 等常用 Options API helper
- **Vue Router 集成** — `ECMAScript.VueRoute` 提供 Vue Router 4 authoring 的类型化 C# 绑定，包括 `createRouter()`、history 创建函数、`useRouter()`、`useRoute()`、`RouterLink` 与 `RouterView`
- **Vuetify 集成** — `ECMAScript.Vuetify` 提供 Vuetify 3 组件 authoring 的生产级类型化 C# 绑定，包括强类型 props、命名槽、事件绑定和完整运行时导出覆盖
- **MSBuild 集成** — 通过标准 MSBuild 属性配置发射、打包和输出路径

## 项目状态

<table>
<tr><th nowrap>层级</th><th>组件</th><th>状态</th></tr>
<tr><td nowrap><strong>已可用</strong></td><td>编译器核心（SemanticWalker、AstConverter）— 1,891 测试</td><td>稳定 — 仓库中最成熟的部分</td></tr>
<tr><td nowrap><strong>已可用</strong></td><td>ECMAScript 模块发射（<code>[ECMAScriptModule]</code> → <code>.mjs</code>）— 111 测试</td><td>稳定</td></tr>
<tr><td nowrap><strong>已可用</strong></td><td>ECMAScript.Vue3 绑定（h、ref、reactive、生命周期、createApp）</td><td>稳定</td></tr>
<tr><td nowrap><strong>已可用</strong></td><td>MSBuild 集成（JazorEmit、JazorBundle、JazorOutDir）</td><td>稳定</td></tr>
<tr><td nowrap><strong>已可用</strong></td><td>Jazor.Analyzer（白名单编译时验证）</td><td>稳定</td></tr>
<tr><td nowrap><strong>已可用</strong></td><td>ECMAScript.Vuetify 绑定（117 个组件，强类型 props/槽/事件）— 117 组件</td><td>稳定 — 生产级 Vuetify 3 authoring</td></tr>
<tr><td nowrap><strong>已可用</strong></td><td>CLR 运行时模块 — 70 测试</td><td>稳定</td></tr>
<tr><td nowrap><strong>进行中</strong></td><td>RazorVue（SFC 管线、规范 h() 模型、Vuetify 生产级覆盖）— 772 测试</td><td>生产级 Vuetify authoring、管线验证、槽/fallthrough 支持</td></tr>
<tr><td nowrap><strong>进行中</strong></td><td>ECMAScript.Pinia + ECMAScript.VueRoute — 205 测试</td><td>API 覆盖持续扩展，Pinia Testing 工具</td></tr>
<tr><td nowrap><strong>进行中</strong></td><td>SourceMap</td><td>局部可用 — 模块级 <code>.mjs.map</code>，尚未全覆盖</td></tr>
<tr><td nowrap><strong>进行中</strong></td><td>Deno 打包</td><td><code>JazorBundle</code> 目标支持基本场景</td></tr>
<tr><td nowrap><strong>进行中</strong></td><td>调试</td><td>设计和里程碑代码已就位，尚未面向用户</td></tr>
<tr><td nowrap><strong>远期</strong></td><td>Jolt（LSP、HMR、DevServer、调试、构建）— 778 测试</td><td>Phase 1–6 收口中，<a href="docs/01-目标/jolt/README.md">设计文档</a></td></tr>
</table>

当前用户应基于**已可用**层进行开发。32 项目 · ~290K 生产代码 · ~190K 测试代码 · ~3,850+ 总测试。

---

## 快速开始

### 安装

```
dotnet add package Jazor
```

该包包含运行时（`ECMAScript`、`ECMAScript.Vue3`、`ECMAScript.Pinia`、`ECMAScript.VueRoute`、`ECMAScript.Vuetify`）、编译器（`Jazor.Compiler`）、静态分析器（`Jazor.Analyzer`）、发射工具（`Jazor.Emit`）以及 MSBuild props/targets。

### 多项目配置

Jazor 推荐使用多项目布局：库项目声明模块，宿主项目负责发射。

**库项目** — 声明模块，不触发发射：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
    <JazorEmit>false</JazorEmit>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Jazor" Version="*" />
  </ItemGroup>
</Project>
```

**宿主项目** — 触发发射，输出 `.mjs` 文件：

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net11.0</TargetFramework>
    <JazorEmit>true</JazorEmit>
    <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorOutDir>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Jazor" Version="*" />
    <ProjectReference Include="..\MyApp.Features\MyApp.Features.csproj" />
  </ItemGroup>
</Project>
```

宿主项目会扫描自身程序集及所有引用程序集中带有 `[ECMAScriptModule]` 标注的类型，将 `.mjs` 文件输出到 `JazorOutDir`。基线发射示例见 [multi-project sample](samples/Jazor.MultiProject/)，Vue 3 + Pinia 消费示例见 [ECMAScript.Pinia.Counter](samples/ECMAScript.Pinia.Counter/)。

### MSBuild 属性

| 属性 | 默认值 | 说明 |
|------|--------|------|
| `JazorCompile` | `true` | 启用 `[ECMAScriptModule]` 类型的编译。 |
| `JazorEmit` | `Exe` 为 `true`，`Library` 为 `false` | 构建后发射 `.mjs` 文件。 |
| `JazorOutDir` | `$(IntermediateOutputPath)jazor\$(TargetFramework)\modules\` | 发射的 `.mjs` 文件输出目录。 |
| `JazorBundle` | `false` | 将所有模块打包为单个 JS 文件（使用内置 Deno 运行时）。 |
| `JazorBundleOut` | `$(OutDir)jazor\app.js` | 打包后的 JS 文件输出路径。 |
| `JazorCleanEmit` | `true` | 清理输出目录中的过期 `.mjs` 文件。 |
| `JazorFailOnPathConflict` | `true` | 两个模块映射到同一路径时构建失败。 |

---

## 模块编写

### 基本模块

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

生成 `shared/greetings.mjs`：

```javascript
export function prefix() {
  return "Hello";
}
export function compose(name) {
  return `${prefix()}, ${name}`;
}
```

跨模块引用自动解析 — 当另一个模块调用 `GreetingModule.Compose(name)` 时，编译器自动生成对应的 `import` 语句。

### Vue 3 h() 函数编写

`ECMAScript.Vue3` 提供类型化的 C# 绑定，用于 Vue 3 的 Composition API 和 `h()` 渲染函数：

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
                return () => H("div", new VueObject { Class = "counter" },
                [
                    H("p", $"Count: {count.Value}"),
                    H("button", new VueObject
                    {
                        Events = new VueDictionary
                        {
                            ["click"] = (Action)(() => count.Value++)
                        }
                    }, "Increment")
                ]);
            }
        });
}
```

### 编译能力

编译器支持变量声明、基础类型、模式匹配、可空类型、async/await、字符串插值、对象和集合初始化、元组与解构、switch 语句/表达式，以及循环（for/foreach/while/do-while）。详见 [编译器文档](src/Jazor.Compiler/README.md)。

---

## ECMAScript 特性约定

- `[ECMAScript("npm:vue@3")]` — 声明**运行时导入依赖**。编译器生成 `import { ... } from "npm:vue@3"`。
- `[ECMAScript("jsr:@scope/pkg")]` 或 `[ECMAScript("https://...")]` — Deno 可解析的导入地址。
- `[ECMAScriptModule("features/todo/index.mjs")]` — 声明发射后的**输出模块路径**，不是包解析地址。
- `[Jazor(...)]` — CLR 和宿主映射的生产侧声明。

---

## 项目结构

```
Jazor/
├── src/
│   ├── ECMAScript/                  # ECMAScript AST 核心类型与特性
│   ├── ECMAScript.Contract/         # 最小契约层（JazorAttribute、Op）
│   ├── ECMAScript.Pinia/            # Pinia 运行时绑定面
│   ├── ECMAScript.Pinia.Test/       # Pinia 绑定测试
│   ├── ECMAScript.Vue3/             # Vue 3 运行时绑定面
│   ├── ECMAScript.VueRoute/         # Vue Router 运行时绑定面
│   ├── ECMAScript.VueRoute.Test/    # Vue Router 绑定测试
│   ├── ECMAScript.Vuetify/          # Vuetify 3 生产级组件绑定
│   ├── Jazor.Compiler/              # C# → JS 编译器核心
│   ├── Jazor.Analyzer/              # 静态分析器（白名单验证）
│   ├── Jazor.CLR/                   # CLR 运行时模块支持
│   ├── Jazor.Emit/                  # 发射管线和打包物化
│   ├── Jazor.Common/                # 共享契约和工具
│   ├── Jazor/                       # NuGet 包（打包以上所有内容）
│   ├── Jolt/                        # 【远期】开发工具链
│   ├── Wiki/                        # 基于 Jazor 构建的文档站点
│   └── samples/                     # 可运行的 host / consumer 示例
├── docs/                            # 文档中心
└── scripts/                         # 构建与工具脚本
```

## 文档

| 角色 | 入口 |
|------|------|
| **新访客** | [文档中心](docs/README.md) — 项目全貌与导航 |
| **维护者** | [工作流总览](docs/02-计划/workstream-dashboard.md) — 恢复工作唯一入口 |
| **架构设计** | [编译器架构](docs/01-目标/compiler/ArchitectureOverview.Simplified.md) · [ECMAScript.Vue3 设计](docs/01-目标/ecmascript.vue3/README.md) · [ECMAScript.Pinia 设计](docs/01-目标/ecmascript.pinia/README.md) · [ECMAScript.VueRoute 设计](docs/01-目标/ecmascript.vueroute/README.md) |

文档按五类组织：[目标](docs/01-目标/README.md) · [计划](docs/02-计划/README.md) · [完成](docs/03-完成/README.md) · [补充](docs/04-补充/README.md) · [遗弃](docs/05-遗弃/README.md)

---

## 开发和构建

### 环境要求
- .NET 11 SDK（预览版）
- PowerShell 7+（用于测试脚本）
- Windows、Linux 或 macOS

### 构建步骤

```bash
git clone https://github.com/devhxj/Jazor.git
cd Jazor
dotnet restore
dotnet build

# 运行所有测试
pwsh ./scripts/test-dotnet.ps1

# 仅运行编译器测试
pwsh ./scripts/test-dotnet.ps1 -Project compiler

# 仅运行 Pinia 绑定测试
pwsh ./scripts/test-dotnet.ps1 -Project pinia

# 仅运行 Vue Router 绑定测试
pwsh ./scripts/test-dotnet.ps1 -Project vueroute

# 运行单个测试类
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest"
```

---

## 贡献

欢迎社区贡献。请在提交 Pull Request 前查阅仓库文档并遵循代码库中描述的约定。

## 许可证

本项目采用 MIT 许可证。详见 [LICENSE.txt](LICENSE.txt) 文件。

## 致谢

- [Roslyn](https://github.com/dotnet/roslyn) — C# 编译器平台
- [Acornima](https://github.com/adams85/acornima) — JavaScript 解析器和 AST 库
- [WebRef](https://github.com/w3c/webref) — Web 规范引用
- [WootzJs](https://github.com/kswoll/WootzJs) · [h5](https://github.com/curiosity-ai/h5) · [SharpKit](https://github.com/SharpKit/SharpKit) — C# 到 JavaScript 编译器
- [DenoHost](https://github.com/thomas3577/DenoHost) — .NET 的 Deno 运行时宿主
- [CSharpToJavaScript](https://github.com/TiLied/CSharpToJavaScript) — C# 到 JavaScript 转译器

---

## 安全策略

如果你发现安全漏洞，请通过 [GitHub 安全公告](https://github.com/devhxj/Jazor/security/advisories/new)私下报告。不要为安全问题创建公开 Issue。

## 反馈

- [报告 Bug](https://github.com/devhxj/Jazor/issues/new?template=bug_report.md)
- [功能请求](https://github.com/devhxj/Jazor/issues/new?template=feature_request.md)
- [讨论区](https://github.com/devhxj/Jazor/discussions)
