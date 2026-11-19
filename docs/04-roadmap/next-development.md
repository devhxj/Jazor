# 下一阶段

> 已交付能力以[当前状态](./current-status.md)为准。本页只安排尚需推进的工作，不把已完成的迁移或既有支持切片重复列为待开发能力。

## 投入准则

从真实消费者复现、高频作者写法和明确的行为缺口选择任务。每个切片先定义 C# 契约、责任层和验收，再实现、回归并更新指南；缺少需求或可复现收益时不增加运行时协议。

## 主线投入

| 优先级 | 提案 | 排序依据 | 首个交付门槛 |
| --- | --- | --- | --- |
| **P0：首批切片已交付** | [#4：ILSpy 按需导入普通 C# / NuGet DLL](https://github.com/devhxj/Jazor/issues/4) | 解除普通托管类库只有签名、没有可编译实现的核心采用障碍，直接扩大现有 C# 与 RazorVue 的代码复用范围；1.4.2 已实现普通 DLL/NuGet 自动导入、成员闭包与共享模块 | 实现资产与 ref/lib 配对、原始/恢复符号对应、静态直接调用的成员闭包；无关不支持成员不阻断；真实 DLL/NuGet 的 JS 与 .NET 结果对照及隔离 consumer 验证 |
| **当前主线** | [#138：Jolt 组件和语义编辑](../03-guides/jolt-authoring.md) | 以顶层标记和 C# 编写 Vue 组件，直接使用项目依赖中的 npm 组件 | 用同一应用验证组件组合、编辑器、浏览器和隔离工具包消费者 |

## 作者体验与交付证据

| 任务 | 责任层 | 完成条件 |
| --- | --- | --- |
| npm 组件组合、插槽和回调 | Jolt.Compiler、Jolt.Npm | 真实组件包的类型检查、渲染和浏览器交互一致 |
| 诊断与修改反馈 | Jolt.LanguageServer、编辑器扩展 | 构建和编辑器定位一致，补全和导航使用同一份组件声明 |
| Debug/HMR/SPA/SSR 一致性 | Emit、ASP.NET Core、NodeHost | 同一资源闭包上的模块、source map、PathBase、浏览器交互和适用 hydration 行为可复现，错误明确传播 |
| 真实应用联调 | 参考应用与消费者 | 补齐真实后端保存、插件启用后的业务流程和修改到可见的 HMR 长尾证据 |
| 可测量优化 | Compiler、Jolt、CLR、Emit | 固定机器、提交、输入、warm-up 和多轮参数；比较耗时、p50/p95、吞吐、原始/gzip 体积与行为 |

复现入口见 [Jolt 作者指南](../03-guides/jolt-authoring.md) 和 [开发与测试](../03-guides/development-and-testing.md)。

## 协议与支持范围扩展

SSR state envelope、认证快照/browser provider、单一显式构造函数的普通引用服务参数，以及 history handler 的已验证子集已经实现。扩展必须在现有协议基础上明确新边界：

| 方向 | 待证明的扩展 | 责任层与验收 |
| --- | --- | --- |
| SSR 状态交接 | 更复杂的请求/组件所有权、过期 payload、一次性副作用与 hydration identity | ASP.NET Core / Emit；独立 SSR/package/browser 场景验证失配、重复入口与失败传播 |
| 认证与应用 bootstrap | 新 endpoint 状态和应用生命周期的组合 | 应用 endpoint / ASP.NET Core；服务端保持授权事实来源，typed consumer 验证匿名、刷新、403、过期、错误和竞态 |
| 构造与 activation | 多构造、继承和更复杂服务生命周期 | Compiler、应用集成；验证 Roslyn 绑定、初始化顺序和 SSR/browser lifetime |
| 复杂 browser history | 超出现有 handler、取消恢复和释放切片的完整历史语义 | CLR / 应用 route host；以真实浏览器和 reference 明确 URL、竞态、用户确认与释放协议 |
| localization 与复杂 validation | 有明确需求的 typed 应用/组件协议 | 对应应用或 binding；当前保持 Guidance，取得独立作者面和运行时证据后再评审 |

Microsoft/Blazor 内置 UI、`IJSRuntime` 字符串互操作、`StreamRendering` 和完整 CLR parity 是当前明确边界，不因出现在路线图中成为实现承诺。具体替代路径见[RazorVue 作者指南](https://github.com/devhxj/RazorVue/blob/main/docs/03-guides/razorvue-authoring.md)。

## 编译器与普通类库

35 个语法 bug 的运行行为和诊断修复已随 1.3.0 交付。[语法 issue 索引](./csharp-syntax-gap-audit.md)中的开放 enhancement 优先选择真实消费者及 P0 普通类库导入所需的切片，再扩展构造链、参数绑定、源码运算符、接口分派和自定义解构/枚举；不把关闭 bug 等同于实现完整运行时协议。

普通 NuGet DLL 的自动实现恢复已形成[有界交付切片](./ordinary-nuget-compilation-research.md)，对应 [issue #4](https://github.com/devhxj/Jazor/issues/4)。后续按真实库需求扩展初始化、运行时类型和分派协议。

每个新增 lowering 先明确求值顺序、副作用、结果、使用点语义、确定性与失败边界，再由 Compiler / CLR / Analyzer 按职责实现。仅为擦除类型注解增加 blanket rejection 不属于实现路线。

## Vue 应用生态绑定

DateFns、VueUse、FloatingUi、VeeValidate、VueI18n、VueQuery 和 Monaco 已有源码、测试 lane 和 lockstep 发布。各绑定需要补齐浏览器交互、真实包消费者，以及适用的 SSR、worker 和资源验证。交付清单见 [P3 验收计划](./p3-vue-application-bindings-plan.md)。

## Jolt 组件

当前源码候选已实现后台 npm 初始化、跨文件引用与改名、格式化和快捷修复，以及选定的泛型组件和 `.jazor` 样式隔离场景。平台、编辑器版本和复现入口见候选验收。 [Jolt 编辑器与组件能力实施计划](./jolt-editor-implementation-plan.md#候选验收)

一个 `.jazor` 文件定义一个 Vue 组件，HTML 与组件标签直接写在顶层，`@using` 引入命名空间，`@code` 声明 C# 成员。构建从 npm 包的公开类型声明检查调用，从公开 ESM 入口导入组件。 [Issue #138](https://github.com/devhxj/Jazor/issues/138)

Jolt 在 Razor 语言层取得带原文位置的标记与 C# 片段，通过 Roslyn 和 Jazor.Compiler 生成 Vue 渲染模块。项目加载器读取 npm 锁文件和声明，构建与编辑器共用相同的输入。 [Jolt 作者指南](../03-guides/jolt-authoring.md)

验收应用通过父子回调驱动 Reka UI 更新，开发服务器与生产资源均使用浏览器验证。编辑器验证涵盖作者文件诊断和跳转到 npm 声明。本地打包验证检查安装后的 CLI 在隔离项目中构建与打包。正式发布以版本发行记录为准。 [浏览器样例](../../samples/Jolt.Counter/README.md) · [工具包验收](../../scripts/csharp/verify-jolt-package.cs)

## 写入当前状态的门槛

1. C# API 通过正常类型检查，组件绑定通过 Jolt 编译。
2. compiler lowering、CLR/runtime 或 binding 有针对性的行为回归与相邻失败诊断。
3. 隔离 Release package consumer 与真实浏览器完成目标交互。
4. 涉及 SSR、hydration、生命周期或宿主状态时，证明对应 profile 的所有权和错误传播。
5. 同步架构、指南和适用发布说明，使用正式门禁保留提交与环境对应的报告。

已完成计划的过程和旧测试快照只保留在[历史演进](../05-history/evolution.md)，详细原始记录通过 Git 历史追溯。
