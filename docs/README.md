# Jazor

> **Jazor 是面向 .NET 的 C# 到 ECMAScript 工具链。它将受支持的 C# 语义转换为可验证、可交付的浏览器模块，让类型、依赖与发布产物沿同一条工程链路抵达运行环境。**

Jazor 不以语法替换语义。它以 Roslyn 的语义模型为基础，将 C# 编译为确定性的 ECMAScript 模块；在 Razor 项目中，则以官方 Razor Source Generator 生成的最终编译结果为输入，直接产出 Vue render function。C# 的类型检查、符号绑定和宿主 API 边界因此仍由编译过程承担，而非留给浏览器在运行时猜测。

## Jazor 是什么

Jazor 的核心是一条从 C# 语义到浏览器模块的工程路径：`Jazor.Compiler` 负责 lowering，`Jazor.Emit` 负责模块、source map 与发布产物的物化，CLR 与 ECMAScript 绑定负责定义可以跨越语言边界的运行时语义。Razor-to-Vue 建立在这条路径之上，是当前的框架集成方向，而不是另一套编译路线。

它不试图把任意 .NET 程序搬进浏览器。需要运行时语义的外部类型和成员必须有明确映射；无法忠实表达的能力会在实际使用点明确失败，不以原始 JavaScript 或静默 fallback 掩盖差异。

## 它解决什么

| 需要面对的问题 | Jazor 的回答 |
| --- | --- |
| 如何用 C# 编写真正可交付的浏览器模块 | 通过 Roslyn `IOperation` 与 ESTree lowering 生成确定性的 ECMAScript 模块，并由 `Jazor.Emit` 物化 source map、import map 与发布产物。 |
| 如何在 Razor 应用中采用 Vue 组件生态 | 以官方 Razor SG 的最终 C# 语义为输入，经 Razor-to-Vue 直接生成 Vue render-function `.mjs`，不依赖 Razor IR、生成 SFC 或中间 wrapper 协议。 |
| 如何让跨语言边界保持可验证 | CLR 与 ECMAScript API 由强类型映射和白名单定义；导入、模块名、临时变量与 source map 锚点保持确定性。 |
| 如何把模块可靠地带到真实应用 | 支持 Debug 模块、Release bundle，以及已声明范围内的 ASP.NET Core SSR 与 hydration；资源按显式依赖闭包交付。 |

## 适用边界

Jazor 的价值来自明确边界，而不是对所有运行时形态作出模糊承诺。它适合需要受控浏览器语义和可追溯交付链路的 .NET 团队；下列场景不在当前产品契约内。

| 适合采用 Jazor | 不属于当前产品契约 |
| --- | --- |
| 希望以 C# 类型系统编写受控浏览器模块或组件库 | 在浏览器中运行完整 CLR，或调用任意未映射 .NET API |
| 需要将自定义 Razor 组件与 Vue 3、TDesign、Vue Router、Pinia 等绑定组合 | 将 Microsoft/Blazor 内置 UI 组件自动替换为 Vue 组件 |
| 需要明确模块、资源、发布与诊断边界 | 通过 `IJSRuntime`、反射或弱类型 `object?` 逃逸到未经验证的运行时语义 |
| 希望用生产级参考应用验证 RazorVue authoring 与交付链路 | 未建立浏览器和发布证据的认证状态、SSR 状态交接或复杂浏览器历史协议 |

完整的产品范围、支持边界与非目标见[产品范围](./01-overview/product-scope.md)。

## 从这里开始

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

## 文档地图

| 目录 | 责任 |
| --- | --- |
| [`01-overview`](./01-overview/README.md) | 产品定位、阅读地图与系统全景 |
| [`02-architecture`](./02-architecture/README.md) | 当前架构、模块职责和稳定边界 |
| [`03-guides`](./03-guides/README.md) | 安装、配置、编写、开发与验证指南 |
| [`04-roadmap`](./04-roadmap/README.md) | 当前工作方向和状态说明 |
| [`05-history`](./05-history/README.md) | 已退役路线与关键演进背景 |

## 阅读原则

- 当前行为以源码、自动化测试和本目录的架构文档为准。
- 实现细节优先维护在源码旁的 README 或项目文档中；本目录只保留跨模块契约、使用方式和必要的阅读入口。
- 计划、测试结果和审计结论只在其有效范围内陈述，不在长期设计文档中重复快照数据。
- 版本变更记录位于仓库根目录的 [CHANGELOG.md](../CHANGELOG.md)。
