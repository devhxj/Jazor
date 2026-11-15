# 下一阶段

> 已交付能力以[当前状态](./current-status.md)为准。本页只安排尚需推进的工作，不把已完成的迁移或既有支持切片重复列为待开发能力。

## 投入准则

从真实消费者复现、高频作者写法和明确的行为缺口选择任务。每个切片先定义 C# 契约、责任层和验收，再实现、回归并更新指南；缺少需求或可复现收益时不增加运行时协议。

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

35 个语法 bug 的运行行为和诊断修复已随 1.3.0 交付。下一步从[语法 issue 索引](./csharp-syntax-gap-audit.md)中的开放 enhancement 选择高频切片，例如构造链、参数绑定、源码运算符、接口分派和自定义解构/枚举；不把关闭 bug 等同于实现完整运行时协议。

普通 NuGet DLL 的实现恢复仍是[研究方案](./ordinary-nuget-compilation-research.md)，对应 [issue #4](https://github.com/devhxj/Jazor/issues/4)。自动依赖闭包、原始/恢复符号对应、compiler hooks 与产物集成完成前，不声明安装任意普通 NuGet 后即可用于前端。

每个新增 lowering 先明确求值顺序、副作用、结果、使用点语义、确定性与失败边界，再由 Compiler / CLR / Analyzer 按职责实现。仅为擦除类型注解增加 blanket rejection 不属于实现路线。

## Vue 应用生态绑定

DateFns、VueUse、FloatingUi、VeeValidate、VueI18n、VueQuery、VueDraggable、FilePond、WangEditor、Monaco 均已有源码、测试 lane 和 lockstep 发布。当前待办是各绑定的 browser smoke、真实 RazorVue/package consumer、适用 SSR 与 worker/资源验证，完成前保持 Guidance。交付清单见 [P3 验收计划](./p3-vue-application-bindings-plan.md)。

## .jazor 作者格式提案

[Issue #138](https://github.com/devhxj/Jazor/issues/138)提出在 C# 中使用类似 TSX 的标记表达式，并交付语义智能感知。这是待设计与实现的独立作者入口；当前已实现的 RazorVue 仍使用官方 Razor SG。

提案按语法与类型投影、MSBuild/Compiler 接入、语言服务与编辑器、组件组合、发布消费者分阶段验收。C# 语义复用核心编译器；首个 UI 目标建议 Vue ESM。智能感知覆盖 C# 与组件参数补全、上下文类型、导航、诊断、重命名和不完整语法恢复；编辑器范围与组件 setup/render 发现规则需先经设计验证，尚不构成可安装的支持声明。

## 写入当前状态的门槛

1. C# API 通过正常类型检查和官方 Razor SG 绑定。
2. compiler lowering、CLR/runtime 或 binding 有针对性的行为回归与相邻失败诊断。
3. 隔离 Release package consumer 与真实浏览器完成目标交互。
4. 涉及 SSR、hydration、生命周期或宿主状态时，证明对应 profile 的所有权和错误传播。
5. 同步架构、指南和适用发布说明，使用正式门禁保留提交与环境对应的报告。

已完成计划的过程和旧测试快照只保留在[历史演进](../05-history/evolution.md)，详细原始记录通过 Git 历史追溯。
