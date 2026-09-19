# 产品范围

> 本页定义 Jazor 已公开承诺的产品边界，以及阅读当前文档时应遵循的依据。

## 产品定位

Jazor 是一套 C# 至 ECMAScript 的 .NET 工具链。它以 Roslyn `IOperation` 为语义输入，以标准 ECMAScript AST 为中间表示，编译受支持的 C# 语义，产出确定性的浏览器模块，并提供模块物化、source map、浏览器打包与 ASP.NET Core 集成。

它的价值在于：作者在编写阶段依然保留类型检查、符号绑定与明确的宿主 API 边界，同时得到可被标准 JavaScript 工具链直接消费的模块。编译期负责判断，浏览器负责执行，两者之间只保留已声明的语义，杜绝未经声明的语义跳跃。

Razor-to-Vue 是建立在这项核心能力之上的应用方向：它先绑定官方 Razor SG 的组件语义，得到可编译的 C# 操作，再调用 Jazor 核心生成 Vue render-function 产物。它与核心共享同一条语义与交付链路，承担框架集成的角色。

## 层级与产品路径

核心平台、框架集成与应用方向依次建立，彼此协作，又各司其职。

| 层级 | 路径 | 输入 | 输出 | 启用方式 |
| --- | --- | --- | --- | --- |
| 核心平台 | ECMAScript 模块 | 标注为 `[ECMAScriptModule]` 的 C# 模块 | `.mjs`、源映射与 manifest | 引用 `Jazor` |
| 核心平台 | 浏览器交付 | 程序集内 `ModuleCatalog`、npm/JSR package metadata 与明确声明的 embedded carrier 组成的显式依赖图 | debug 模块或 Netpack 生产包 | 在最终宿主项目设置 `JazorMode` |
| 框架集成层 | Razor-to-Vue（当前实现） | 官方 Razor Source Generator 生成的最终 C# 语义 | Vue render-function `.mjs` | 在 Razor 项目中额外引用 `Jazor.Vue` |
| 应用方向 | ASP.NET Core SSR | 物化后的 Vue 模块图 | 服务器渲染 HTML 与客户端 hydration | 设置 `JazorSSR` 并注册 SSR 服务 |

Vue 3、Vue Router、Pinia、Vue Devtools、Vue Data UI、Vuetify、Element Plus、TDesign、CSS-in-JS 与管理壳，均以强类型绑定或可选库形态服务于 Jazor 核心平台，框架集成沿用统一编译路线。

## 范围边界

Jazor 聚焦受支持 C# 语义在浏览器中的确定性执行。外部类型与成员进入运行时前，须具备明确的宿主映射；映射缺失或语义表达条件不足时，编译器在实际使用点报告诊断。每项可用能力均由实现、映射和可复现证据共同定义。

当前生产 Razor-to-Vue 路径以官方 Razor Source Generator 完成后的最终 `Compilation` 为唯一输入，并直接产出 Vue render-function 模块。Razor IR、生成 SFC、二次解析生成 C#、中间 wrapper-JS 协议、Jolt 与 CSX 均属于历史路线。未来框架集成沿用同一核心平台，并遵循已发布的公开契约。

## 产品组成

下列项目共同构成这条工程路径；每一层只承担本层应当承担的责任。

| 层级 | 主要项目 | 责任 |
| --- | --- | --- |
| 编译核心 | `Jazor.Compiler`、`Jazor.Common`、`ECMAScript` | Roslyn 语义降低、ESTree、命名和公共契约 |
| 宿主映射 | `Jazor.CLR`、`Jazor.Analyzer`、生成器项目 | CLR 白名单、静态诊断和映射生成 |
| Razor 集成 | `Jazor.RazorVue`、`Jazor.Vue` | Razor SG 绑定、Vue 产物封装与显式 opt-in |
| 交付与宿主 | `Jazor.Emit`、`Jazor.AspNetCore`、`Jazor.AspNetCore.Dev` | 物化、打包、SSR、开发期集成 |
| 生态与 UI | `ECMAScript.*`、`Jazor.Admin` | Vue 生态绑定、样式和管理壳库 |
| 示例 | `samples/` | 真实集成、浏览器验证和使用参考 |

`Jazor.Admin` 是可发布的管理壳库，`samples/JazorAdmin` 是消费该库的生产级管理参考应用。库定义可复用的壳与模型契约，示例选择 TDesign、页面结构和领域流程；两者各自承担独立职责。示例入口见[示例](../03-guides/examples.md)。
