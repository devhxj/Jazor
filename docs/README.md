# Jazor

> **Jazor 是面向 .NET 的 C# 至 ECMAScript 工具链：它以受支持的 C# 语义为起点，生成可验证、可交付的浏览器模块。**

Jazor 让 .NET 团队在浏览器侧继续使用经过类型检查、符号绑定和边界约束的 C# 作者体验。浏览器最终接收的是标准 ECMAScript 模块；类型、依赖、source map 与发布产物沿同一条工程链路抵达运行环境。未被映射的运行时语义会在使用处明确失败，不由浏览器替作者猜测。

## Jazor 是什么

Jazor 以 Roslyn 的语义模型为依据，将受支持的 C# 操作编译为确定性的 ECMAScript 模块。它不是语法替换工具，也不试图把完整 CLR 搬进浏览器。

`Jazor.Compiler` 负责将受支持的 `IOperation` 降低为 ECMAScript AST，`Jazor.Emit` 负责将模块、source map 与发布产物物化，CLR 与 ECMAScript 绑定则定义能够跨越语言边界的运行时语义。

Razor-to-Vue 建立在这条核心路径之上。它只接收官方 Razor Source Generator 完成后的最终 `Compilation`，直接产出 Vue render-function 模块；C# 表达式、成员与调用的语义仍由同一套编译主线负责，而不是另起一条转换路线。

## 它解决什么

它让类型检查、符号绑定、宿主映射与模块交付保持在同一条链路中，解决 C# 作者体验与浏览器模块交付之间的断裂。

| 需要面对的问题 | Jazor 的回答 |
| --- | --- |
| 用 C# 编写浏览器模块 | 受支持的 C# 语义经 Roslyn `IOperation` 与 ESTree lowering 转化为确定性的 ECMAScript 模块，`Jazor.Emit` 再物化 source map、import map 与发布产物。 |
| 在 Razor 项目中使用 Vue 组件生态 | Razor-to-Vue 读取官方 Razor SG 的最终 C# 语义，直接生成 Vue render-function `.mjs`；不依赖 Razor IR、生成 SFC 或中间 wrapper 协议。 |
| 让跨语言边界保持可验证 | CLR 与 ECMAScript API 由强类型映射和白名单定义。导入、模块名、临时变量与 source map 锚点保持确定性，错误回到作者实际使用的位置。 |
| 将模块可靠地带入真实应用 | Debug 模块、Release bundle 以及已声明范围内的 ASP.NET Core SSR 与 hydration 使用同一显式资源闭包交付。 |

## 适用边界

它适合需要受控浏览器语义与可追溯交付链路的 .NET 团队；未映射或尚无浏览器、发布证据的运行时能力，不属于当前产品契约。

明确边界是采用 Jazor 的前提，不是项目后期才发现的限制。下列对照说明当前承诺，也说明不应期待它替代什么。

| 适合采用 Jazor | 不属于当前产品契约 |
| --- | --- |
| 以 C# 类型系统编写受控的浏览器模块或组件库 | 在浏览器中运行完整 CLR，或调用任意未映射的 .NET API |
| 将自定义 Razor 组件与 Vue 3、TDesign、Vue Router、Pinia 等已声明 binding 组合 | 将 Microsoft/Blazor 内置 UI 组件自动替换为 Vue 组件 |
| 对模块、资源、发布与诊断边界有明确要求 | 通过 `IJSRuntime`、反射或弱类型 `object?` 逃逸到未经验证的运行时语义 |
| 用生产级参考应用验证 RazorVue authoring 与交付链路 | 尚未建立浏览器与发布证据的认证状态、SSR 状态交接或复杂浏览器历史协议 |

完整的产品范围、支持边界与非目标见[产品范围](./01-overview/product-scope.md)。边界一旦改变，应先由实现与验证证明，再写入本页。

## 选择阅读路径

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
- 已交付能力、下一阶段投入与历史材料各有唯一位置，避免同一事实在不同页面形成不同表述。
- 版本变更记录位于仓库根目录的[CHANGELOG.md](../CHANGELOG.md)。
