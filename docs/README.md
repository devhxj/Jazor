# Jazor

> **Jazor 是面向 .NET 的 C# 至 ECMAScript 工具链。受支持的 C# 语义经它编译，成为可验证、可交付的浏览器模块，类型、依赖与发布由此沿一条清晰的工程路径抵达运行环境。**

当 .NET 应用的部分能力需要运行于浏览器，相较于语法表面的相似，真正需要维系的是语义本身：跨越语言边界之后，它依然清晰、可追溯。Jazor 为这条路径确立了一份明确的契约——作者以 C# 表达已受支持的行为，编译器生成确定性的标准模块，浏览器执行事先声明的运行时语义。

## Jazor 是什么

Jazor 是一条从 Roslyn 语义到浏览器模块的编译与交付链路，专注于受支持 C# 语义的忠实转译；在浏览器中复现完整 CLR 运行时，则始终位于其目标之外。

`Jazor.Compiler` 负责降低受支持的 `IOperation`，产出 ECMAScript AST；`Jazor.Emit` 负责物化模块、source map 与发布产物；CLR 与 ECMAScript 绑定则定义能够跨越语言边界的运行时语义。

Razor-to-Vue 建立在这条核心路径之上。它只接收官方 Razor Source Generator 完成后的最终 `Compilation`，直接产出 Vue render-function 模块；C# 表达式、成员与调用仍由同一套编译主线处理，与核心共享同一条语义转换路线。

## 它解决什么

类型检查、符号绑定、宿主映射与模块交付由此汇聚于同一条链路，C# 作者体验与浏览器模块交付之间的断裂随之消解。

| 需要面对的问题 | Jazor 的回答 |
| --- | --- |
| 用 C# 编写浏览器模块 | 受支持的 C# 语义经 Roslyn `IOperation` 与 ESTree lowering 转化为确定性的 ECMAScript 模块，`Jazor.Emit` 再物化 source map、import map 与发布产物。 |
| 在 Razor 项目中使用 Vue 组件生态 | Razor-to-Vue 读取官方 Razor SG 的最终 C# 语义，直接生成 Vue render-function `.mjs`；生产链路以最终 `Compilation` 和 render-function 产物为标准。 |
| 让跨语言边界保持可验证 | CLR 与 ECMAScript API 由强类型映射和白名单定义。导入、模块名、临时变量与 source map 锚点保持确定性，错误回到作者实际使用的位置。 |
| 让模块可靠地进入真实应用 | Debug 模块、Release bundle 以及已声明范围内的 ASP.NET Core SSR 与 hydration 使用同一显式资源闭包交付。 |

## 适用边界

它适合需要受控浏览器语义与可追溯交付链路的 .NET 团队。当前产品契约收录已完成宿主映射并具备浏览器、发布证据的运行时能力。

明确的边界应当在采用 Jazor 之前先行确立，成为选型阶段即可依据的前提。下列对照说明当前的承诺，也标示出留在范围之外的部分。

| 适合采用 Jazor | 当前范围边界 |
| --- | --- |
| 以 C# 类型系统编写受控的浏览器模块或组件库 | 浏览器运行时聚焦受支持的 C# 语义与显式宿主映射 |
| 组合自定义 Razor 组件与 Vue 3、TDesign、Vue Router、Pinia 等已声明 binding | Microsoft/Blazor 内置 UI 组件沿用应用自定义或已声明 binding 的接入方式 |
| 对模块、资源、发布与诊断边界有明确要求 | 运行时语义通过强类型 binding、显式资源和诊断契约进入浏览器 |
| 用生产级参考应用验证 RazorVue authoring 与交付链路 | `samples/JazorAdmin` 以生产级真实应用覆盖浏览器、Release、资源闭包和多页面 typed authoring；认证状态、SSR 状态交接和浏览器历史按已验证子集运行 |

完整的产品范围、支持边界与非目标见[产品范围](./01-overview/product-scope.md)。边界一旦发生变化，先由实现与验证给出证明，再更新至本页。

## 选择阅读路径

从一个明确的问题开始阅读，能让后续的技术细节始终落在同一份产品判断之内。

| 读者与目标 | 建议入口 |
| --- | --- |
| 第一次评估 Jazor | [产品范围](./01-overview/product-scope.md) -> [系统架构](./01-overview/system-architecture.md) |
| 第一次使用 Jazor | [安装与配置](./03-guides/installation-and-configuration.md) -> [快速开始](./03-guides/quick-start.md) |
| 在 Razor 项目中启用 Vue | [Razor-to-Vue 架构](./02-architecture/razor-to-vue.md) -> [包配置](./03-guides/installation-and-configuration.md) |
| 理解 Jazor 核心 | [系统架构](./01-overview/system-architecture.md) -> [编译器](./02-architecture/compiler.md) -> [产物管线](./02-architecture/artifact-pipeline.md) |
| 设计多项目类库与资源交付 | [类库产物与引用契约](./02-architecture/library-artifact-contract.md) |
| 在核心平台上使用框架集成、Vue 生态、SSR 或管理壳 | [框架集成层](./02-architecture/framework-integrations.md) -> [Razor-to-Vue](./02-architecture/razor-to-vue.md) -> [平台与绑定](./02-architecture/platform-and-bindings.md) |
| 参与仓库开发 | [开发与测试](./03-guides/development-and-testing.md) -> [文档规范](./03-guides/documentation-style.md) |
| 查找样例 | [示例](./03-guides/examples.md) |

## 文档职责

每类文档各自承担一种事实，同一结论因此只在唯一的页面呈现。

| 目录 | 责任 |
| --- | --- |
| [`01-overview`](./01-overview/README.md) | 产品定位、阅读地图与系统全景 |
| [`02-architecture`](./02-architecture/README.md) | 当前架构、模块职责和稳定边界 |
| [`03-guides`](./03-guides/README.md) | 安装、配置、编写、开发与验证指南 |
| [`04-roadmap`](./04-roadmap/README.md) | 当前工作方向和状态说明 |
| [`05-history`](./05-history/README.md) | 已退役路线与关键演进背景 |

## 阅读原则

文档以当前可验证的事实为准，并在产品、架构、指南与历史之间保持清楚的分工。

- 当前行为以源码、自动化测试和本目录的架构文档为准。
- 实现细节优先维护在源码旁的 README 或项目文档中；本目录只保留跨模块契约、使用方式和必要的阅读入口。
- 已交付能力、下一阶段投入与历史材料各有唯一位置，同一事实因此只有一种表述。
- 版本变更记录位于仓库根目录的 [CHANGELOG.md](../CHANGELOG.md)。
