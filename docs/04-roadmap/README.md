# 当前方向与状态

本目录只记录当前产品范围、已接受的工作方向和可复现的验证入口。它不是任务日志，也不保存已经结束的 WBS、审计报告或历史测试快照。

| 文档 | 说明 |
| --- | --- |
| [当前状态](./current-status.md) | 核心平台、当前框架集成、交付能力和质量门槛 |
| [宿主生产就绪](./host-production-readiness.md) | SPA、HMR、SSR 与 Debug 的当前验收路线和 Windows 优先门禁 |
| [RazorVue Direct Render 性能评审](./razorvue-direct-h-performance.md) | direct render、static hoist、block/patch 与 handler cache 的当前边界和 G2 验收 |
| [RazorVue 极致性能路线图](./razorvue-extreme-performance.md) | 从真实 benchmark、child block tree、list/slot lowering 到 CLR payload、SSR 与生成吞吐的实施顺序和门禁 |
| [RazorVue 作者面诊断与支持决策路线图](./razorvue-authoring-diagnostics.md) | Razor/C# 合法作者面与支持切片差值的分级清单、final-pipeline 诊断契约、支持/拒绝决策与静默劣化清零计划 |
| [RazorVue 开发者体验完善路线图](./razorvue-developer-experience.md) | Blazor-first 作者面兼容：标准 Razor/C# 直接工作；无法保真时由作者源码分析在代码位置解释并给出替代，另含组件适配、运行时证明和发布门禁 |
| [RazorVue “零摩擦”执行计划](./razorvue-zero-friction-plan.md) | 以 TDesign 自然 Razor authoring 为优先，按 compiler/runtime、binding/API、证据/交付三类缺口分阶段执行；Microsoft/Blazor 内置 UI 组件明确范围外 |
| [RazorVue v0.28.0 之后的下一步开发计划](./razorvue-next-development-plan.md) | 基于 v0.28.0 实际支持边界排序下一轮工作：优先作者诊断、强类型表单、导航常用闭环和 binding 收敛；认证/SSR/构造注入先做协议，固定 Reject 边界不扩张 |
| [RazorVue Blazor CLR 类型支持计划](./blazor-clr-support-plan.md) | 浏览器运行时类型与服务的专项 ledger：`Jazor.CLR.Generator -> Jazor.CLR` 的唯一 mapping owner、导航拦截、事件参数、元素引用、Blazor JS interop 的 Reject 边界、认证、表单与文件输入的依赖、顺序和验收 |
| [CLR Runtime 健壮性与性能强化计划](./clr-runtime-hardening-plan.md) | 保留现有 `J*` 值 carrier 形态，收敛运行时行为正确性与开销：比较运算符 lowering 确认、第二布局写入方消除、不变量缓存与生成器护栏 |
| [ECMAScriptAttribute 统一协议调整计划](./ecmascript-attribute-unification.md) | 统一外部 ESM binding 与组件描述 Attribute，固定 `Allow` / `Import` / `Component` 三类 Transform，规划编译器、RazorVue、生成器和兼容层迁移 |
| [CLR runtime 模块资源交付计划](./clr-artifact-provider-unification.md) | 将 `Jazor.CLR` 生成的 runtime JavaScript 交付为 `ECMAScript` 的 `manifest.json + dist`，并验证资源依赖、引用传递和最终 Emit 物化 |
| [Artifact Graph 一次性统一机制设计与实施计划](./artifact-graph-stabilization-plan.md) | 固定 `ECMAScript` 外部资源与纯 Jazor `ModuleCatalog` 两种并列输入，统一依赖、引用、物化和 Debug/Release/SSR/HMR 验收；不发布第三种 carrier 或兼容阶段 |
| [JazorAdmin 生产级参考应用路线图](./admin-reference-app.md) | 示例应用的定位收敛、IAM 边界规矩、TDesign + VueDataUi + VuIcons 界面栈分工、审计日志与 SSO 演示客户端的里程碑与验收 |

新增路线图前，应先明确目标、依赖、验收标准和归属；完成或废弃后，应将结论收敛到当前状态或 [历史演进](../05-history/evolution.md)，而不是继续保留阶段清单。
