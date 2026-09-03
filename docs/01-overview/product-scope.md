# 产品范围

> 适用范围：Jazor 的公共产品边界与仓库级阅读规则。

## 产品定位

Jazor 首先是一套 C# 到 ECMAScript 的 .NET 工具链。它将受支持的 C# 语义转换为确定性的 ECMAScript 模块，以 Roslyn `IOperation` 为语义输入，以标准 ECMAScript AST 为中间表示，并提供模块物化、源映射、浏览器打包和 ASP.NET Core 集成。

Jazor 的核心价值是让 C# 作者在编写阶段保留类型检查、符号绑定和明确的宿主 API 边界，同时生成可以被标准 JavaScript 工具链消费的模块。Razor-to-Vue 是建立在这套核心能力之上的一个应用方向：它先把官方 Razor SG 的组件语义绑定为可编译的 C# 操作，再调用 Jazor 核心生成 Vue render-function 产物。

## 层级与产品路径

| 层级 | 路径 | 输入 | 输出 | 启用方式 |
| --- | --- | --- | --- |
| 核心平台 | ECMAScript 模块 | 标注为 `[ECMAScriptModule]` 的 C# 模块 | `.mjs`、源映射与 manifest | 引用 `Jazor` |
| 核心平台 | 浏览器交付 | 程序集内 `ModuleCatalog` 与资源包 `manifest.json + dist/**` 的显式依赖闭包 | debug 模块或 Netpack 生产包 | 在最终宿主项目设置 `JazorMode` |
| 框架集成层 | Razor-to-Vue（当前实现） | 官方 Razor Source Generator 生成的最终 C# 语义 | Vue render-function `.mjs` | 在 Razor 项目中额外引用 `Jazor.Vue` |
| 应用方向 | ASP.NET Core SSR | 物化后的 Vue 模块图 | 服务器渲染 HTML 与客户端 hydration | 设置 `JazorSSR` 并注册 SSR 服务 |

Vue 3、Vue Router、Pinia、Vue Devtools、Vue Data UI、Vuetify、Element Plus、TDesign、CSS-in-JS 与管理壳均是围绕 Jazor 核心平台提供的强类型绑定或可选库，不构成独立编译路线。

## 非目标

Jazor 不试图成为任意 .NET 程序的完整 CLR 运行时，也不把不受支持的外部 API 静默降级为原始 JavaScript。对于需要运行时语义的外部类型和成员，必须存在明确的宿主映射；无法忠实表达的能力应在实际使用点明确失败。

当前生产 Razor-to-Vue 路径只接受官方 Razor Source Generator 完成后的最终 `Compilation`。Razor IR、生成 SFC、二次解析生成 C#、中间 wrapper-JS 协议，以及已退役的 Jolt 和 CSX 路线都不是当前产品路径。未来的框架集成可建立在同一核心平台上，但不会因尚未实现的方向改变当前公开契约。

## 产品组成

| 层级 | 主要项目 | 责任 |
| --- | --- | --- |
| 编译核心 | `Jazor.Compiler`、`Jazor.Common`、`ECMAScript` | Roslyn 语义降低、ESTree、命名和公共契约 |
| 宿主映射 | `Jazor.CLR`、`Jazor.Analyzer`、生成器项目 | CLR 白名单、静态诊断和映射生成 |
| Razor 集成 | `Jazor.RazorVue`、`Jazor.Vue` | Razor SG 绑定、Vue 产物封装与显式 opt-in |
| 交付与宿主 | `Jazor.Emit`、`Jazor.AspNetCore`、`Jazor.AspNetCore.Dev` | 物化、打包、SSR、开发期集成 |
| 生态与 UI | `ECMAScript.*`、`Jazor.Admin` | Vue 生态绑定、样式和管理壳库 |
| 示例 | `samples/` | 真实集成、浏览器验证和使用参考 |

`Jazor.Admin` 是可发布的管理壳库；`samples/JazorAdmin` 是消费该库的生产级管理参考应用。前者定义可复用的壳与模型契约，后者选择 TDesign、页面结构和领域流程，两者的职责不可混用。示例入口见[示例](../03-guides/examples.md)。
