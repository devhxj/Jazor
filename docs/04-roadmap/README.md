# 路线图

> 路线图回答两个问题：今天可以依赖什么，以及下一阶段资源投向何处。支持声明以已取得的证据为基础。

这里先呈现当前的产品契约，再说明下一步的投入顺序与完成条件。产品契约、实施细节和历史结论分别归入架构、指南与[历史演进](../05-history/evolution.md)，让每一项判断都有唯一、清楚的权威位置。

| 阅读目的 | 入口 |
| --- | --- |
| 判断某项能力是否已经可以使用 | [当前状态](./current-status.md) |
| 了解下一阶段的优先级、边界和完成条件 | [下一阶段](./next-development.md) |
| 执行 RazorVue P0 | [P0 执行计划](./p0-plan.md) |
| 执行 RazorVue P1 | [P1 执行计划](./p1-plan.md) |
| 执行 RazorVue P2 | [P2 执行计划](./p2-plan.md) |
| 执行 Vue 应用生态绑定扩展 | [P3 Vue 应用生态绑定扩展计划](./p3-vue-application-bindings-plan.md) |
| 执行绑定包标准化与细粒度 tree shaking 迁移 | [绑定包标准化与细粒度 tree shaking 迁移计划](./npm-jsr-binding-tree-shaking-plan.md) |

## 维护准则

路线图以辅助判断为目标，写入和迁出均以明确依据为准。

- 只有目标、责任归属、依赖和验收条件均已明确的工作，才进入[下一阶段](./next-development.md)。
- 新能力只有在 C# 作者面、官方 Razor SG、编译/运行时、真实浏览器与适用的发布消费者均具备证据后，才能从计划写入[当前状态](./current-status.md)。
- 完成、废弃或被替代的实施记录只沉淀稳定结论；过程材料由 Git 历史承担追溯责任。
