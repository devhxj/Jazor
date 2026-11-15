# 下一阶段

> 已交付能力以[当前状态](./current-status.md)为准。本页只安排尚需推进的工作，不把已完成的迁移或既有支持切片重复列为待开发能力。

## 投入准则

从真实消费者复现、高频作者写法和明确的行为缺口选择任务。每个切片先定义 C# 契约、责任层和验收，再实现、回归并更新指南；缺少需求或可复现收益时不增加运行时协议。

## 三项提案的实施优先级

以下 P0/P1/P2 表示三项提案的主线投入顺序；已发布能力的回归缺陷仍按实际影响处理。

| 优先级 | 提案 | 排序依据 | 首个交付门槛 |
| --- | --- | --- | --- |
| **P0：首批切片已交付** | [#4：ILSpy 按需导入普通 C# / NuGet DLL](https://github.com/devhxj/Jazor/issues/4) | 解除普通托管类库只有签名、没有可编译实现的核心采用障碍，直接扩大现有 C# 与 RazorVue 的代码复用范围；1.4.2 已实现普通 DLL/NuGet 自动导入、成员闭包与共享模块 | 实现资产与 ref/lib 配对、原始/恢复符号对应、静态直接调用的成员闭包；无关不支持成员不阻断；真实 DLL/NuGet 的 JS 与 .NET 结果对照及隔离 consumer 验证 |
| **P1：其次推进** | [#139：Jazor.LibNode LTS 原生包](https://github.com/devhxj/Jazor/issues/139) | 掌握 LTS 原生构建与更新，验证现有 pnpm/Vite/SSR 和后续 npm 类型工具的兼容性 | 移植匹配微软托管层的 C ABI，复用上游测试，通过已声明平台的原生包与现有工具链门禁 |
| **P2：随后推进** | [#138：.jazor 组件文件与语义智能感知](https://github.com/devhxj/Jazor/issues/138) | 改善组件作者体验、接入 npm Vue 组件；现有 RazorVue 可继续承担组件编写 | 确认 `<template>` + `@code`，验证 Razor/Roslyn 与 Vue 类型工具的复用；可构建组件、真实语义编辑器和直接 npm consumer 分阶段交付 |

这不是三项提案之间的强制技术依赖。ILSpy 首个切片使用现有宿主验证，无需等待 LibNode 升级；后续 `.jazor` 的 npm 工具如需要更高 Node 版本，由 LibNode 切片提供已验证环境。优先完成每项有界切片及其交付证据，再扩大下一条主线的投入，不以“支持任意 NuGet”作为开始后续工作的门槛。

## 作者体验与交付证据

Golden Path、诊断矩阵、标准 `jazor/` 项目、NodeApi 工具链及发布门禁已经建立。后续在这些入口上持续验收：

| 任务 | 责任层 | 完成条件 |
| --- | --- | --- |
| 高频表格、slot、表单和回调自然写法 | 对应 binding、必要的 RazorVue compiler hooks | 独立 authoring fixture 与真实页面通过官方 SG、隔离 package 和浏览器验证；公共 API 保持强类型 |
| 诊断与修改反馈 | RazorVue、作者指南、诊断工具 | 稳定 ID、作者源码位置、HelpLink、最小替代写法及源码/package 一致性回归 |
| Debug/HMR/SPA/SSR 一致性 | Emit、ASP.NET Core、NodeHost | 同一资源闭包上的模块、source map、PathBase、浏览器交互和适用 hydration 行为可复现，错误明确传播 |
| 真实应用联调 | 参考应用与消费者 | 补齐真实后端保存、插件启用后的业务流程和修改到可见的 HMR 长尾证据 |
| 可测量优化 | Compiler、RazorVue、CLR、Emit | 固定机器、提交、输入、warm-up 和多轮参数；比较耗时、p50/p95、吞吐、原始/gzip 体积与行为，达到预先记录的收益门槛 |

使用[Golden Path](../03-guides/razorvue-golden-path.md)、[诊断矩阵](../03-guides/razorvue-diagnostic-matrix.md)和[开发与测试](../03-guides/development-and-testing.md)中的现有入口。测试仿真、真实浏览器和 package consumer 分别说明证明范围；产物不变不等于内部缓存命中。

## 协议与支持范围扩展

SSR state envelope、认证快照/browser provider、单一显式构造函数的普通引用服务参数，以及 history handler 的已验证子集已经实现。扩展必须在现有协议基础上明确新边界：

| 方向 | 待证明的扩展 | 责任层与验收 |
| --- | --- | --- |
| SSR 状态交接 | 更复杂的请求/组件所有权、过期 payload、一次性副作用与 hydration identity | ASP.NET Core / Emit；独立 SSR/package/browser 场景验证失配、重复入口与失败传播 |
| 认证与应用 bootstrap | 新 endpoint 状态和应用生命周期的组合 | 应用 endpoint / ASP.NET Core；服务端保持授权事实来源，typed consumer 验证匿名、刷新、403、过期、错误和竞态 |
| 构造与 activation | 多构造、继承和更复杂服务生命周期 | RazorVue / Compiler；先定义 Roslyn 符号绑定的 selector、初始化顺序和 SSR/browser lifetime，再以独立消费者和完整矩阵验收 |
| 复杂 browser history | 超出现有 handler、取消恢复和释放切片的完整历史语义 | CLR / 应用 route host；以真实浏览器和 reference 明确 URL、竞态、用户确认与释放协议 |
| localization 与复杂 validation | 有明确需求的 typed 应用/组件协议 | 对应应用或 binding；当前保持 Guidance，取得独立作者面和运行时证据后再评审 |

Microsoft/Blazor 内置 UI、`IJSRuntime` 字符串互操作、`StreamRendering` 和完整 CLR parity 是当前明确边界，不因出现在路线图中成为实现承诺。具体替代路径见[RazorVue 作者指南](../03-guides/razorvue-authoring.md)。

## 编译器与普通类库

35 个语法 bug 的运行行为和诊断修复已随 1.3.0 交付。[语法 issue 索引](./csharp-syntax-gap-audit.md)中的开放 enhancement 优先选择真实消费者及 P0 普通类库导入所需的切片，再扩展构造链、参数绑定、源码运算符、接口分派和自定义解构/枚举；不把关闭 bug 等同于实现完整运行时协议。

普通 NuGet DLL 的自动实现恢复已形成[有界交付切片](./ordinary-nuget-compilation-research.md)，对应 [issue #4](https://github.com/devhxj/Jazor/issues/4)。后续按真实库需求扩展初始化、运行时类型和分派协议；不承诺任意 NuGet/CLR 兼容。下一条主线为 LibNode LTS，随后推进 `.jazor`。

每个新增 lowering 先明确求值顺序、副作用、结果、使用点语义、确定性与失败边界，再由 Compiler / CLR / Analyzer 按职责实现。仅为擦除类型注解增加 blanket rejection 不属于实现路线。

## Vue 应用生态绑定

DateFns、VueUse、FloatingUi、VeeValidate、VueI18n、VueQuery、VueDraggable、FilePond、WangEditor、Monaco 均已有源码、测试 lane 和 lockstep 发布。当前待办是各绑定的 browser smoke、真实 RazorVue/package consumer、适用 SSR 与 worker/资源验证，完成前保持 Guidance。交付清单见 [P3 验收计划](./p3-vue-application-bindings-plan.md)。

## LibNode LTS 原生包提案

[Issue #139](https://github.com/devhxj/Jazor/issues/139)提出自行构建 `Jazor.LibNode` 总包及 RID 原生包：以实施时最新 Node.js LTS 为基线，参考微软 LibNode 的分包方式，移植与所选 `Microsoft.JavaScript.NodeApi` 匹配的 `node_embedding_*` C API，继续复用微软托管互操作层。2026-11-15 查询官方索引的首个候选为 Node `v26.13.1`；当前实际依赖仍是微软 LibNode `20.1800.215`，本提案不代表已经升级。

验收要求复用微软现有嵌入、GC 与适用互操作测试，以及对应原生 C embedding 测试，保留断言与夹具，仅适配包选择、路径及运行入口；报告记录上游修订和实际加载的候选库，区分真实原生运行与 mock 覆盖。先验证 C ABI、已有四个平台组合和隔离 NuGet consumer，再通过现有 NodeApi、pnpm、Vite、WebIDL 与 SSR 发布闭包门禁。额外 RID、旧宿主 workaround 的移除及新 addon 兼容性必须分别取得证据；发布沿用 lockstep 版本与官方工作流。

## .jazor 作者格式提案

[Issue #138](https://github.com/devhxj/Jazor/issues/138)提出 `.jazor` 组件文件：顶层 `<template>...</template>` 与同级 `@code { ... }`，文件隐式定义组件，作者无需声明静态类、`Setup()` 或模块特性。模板以 Razor 的 C# 表达式和控制流为起点，同时要求直接导入有类型声明的 npm Vue 组件及语义智能感知。这是待设计与实现的作者入口；当前已实现的 RazorVue 仍使用官方 Razor SG。

优先验证薄适配层复用微软 Razor 的解析、官方 SG 及源码映射，C# 语义复用 Roslyn 与核心编译器；Vue 组件类型提取对比官方 `vue-component-meta` 和 Verter 的公开接口。微软 Razor 持续开发已迁到 [`dotnet/roslyn`](https://github.com/dotnet/roslyn/tree/main/src/Razor)。先证明公开 API、投影/生成顺序、编辑器接入与嵌入宿主兼容性，再决定依赖；Razor 编辑器内部实现和 Verter 的 TypeScript 投影不自动成为 `.jazor` 的 C# 语言服务。

提案按复用验证、可构建组件、语义编辑器、npm 组件组合和发布消费者分阶段验收。首个导入器聚焦 npm，JSR 专用接入不在初始范围；运行时复用现有 NodeApi、pnpm、Vue ESM 与 Emit 链路。智能感知覆盖 C# 与组件参数补全、上下文类型、导航、诊断、重命名和不完整语法恢复；具体导入指令、组件参数/生命周期规则及编辑器范围需验证后确定，尚不构成可安装的支持声明。

## 写入当前状态的门槛

1. C# API 通过正常类型检查和官方 Razor SG 绑定。
2. compiler lowering、CLR/runtime 或 binding 有针对性的行为回归与相邻失败诊断。
3. 隔离 Release package consumer 与真实浏览器完成目标交互。
4. 涉及 SSR、hydration、生命周期或宿主状态时，证明对应 profile 的所有权和错误传播。
5. 同步架构、指南和适用发布说明，使用正式门禁保留提交与环境对应的报告。

已完成计划的过程和旧测试快照只保留在[历史演进](../05-history/evolution.md)，详细原始记录通过 Git 历史追溯。
