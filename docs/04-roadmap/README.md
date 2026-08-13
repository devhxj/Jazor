# 当前方向与状态

本目录只记录当前产品范围、已接受的工作方向和可复现的验证入口。它不是任务日志，也不保存已经结束的 WBS、审计报告或历史测试快照。

| 文档 | 说明 |
| --- | --- |
| [当前状态](./current-status.md) | 核心平台、当前框架集成、交付能力和质量门槛 |
| [宿主生产就绪](./host-production-readiness.md) | SPA、HMR、SSR 与 Debug 的当前验收路线和 Windows 优先门禁 |
| [RazorVue Direct Render 性能评审](./razorvue-direct-h-performance.md) | direct render、static hoist、block/patch 与 handler cache 的当前边界和 G2 验收 |
| [RazorVue 极致性能路线图](./razorvue-extreme-performance.md) | 从真实 benchmark、child block tree、list/slot lowering 到 CLR payload、SSR 与生成吞吐的实施顺序和门禁 |

新增路线图前，应先明确目标、依赖、验收标准和归属；完成或废弃后，应将结论收敛到当前状态或 [历史演进](../05-history/evolution.md)，而不是继续保留阶段清单。
