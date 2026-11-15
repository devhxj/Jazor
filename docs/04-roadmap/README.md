# 路线图

> 路线图回答两个问题：今天可以依赖什么，以及下一阶段资源投向何处。支持声明以已取得的证据为基础。

这里先呈现当前的产品契约，再说明下一步的投入顺序与完成条件。产品契约、实施细节和历史结论分别归入架构、指南与[历史演进](../05-history/evolution.md)，让每一项判断都有唯一、清楚的权威位置。

| 阅读目的 | 入口 |
| --- | --- |
| 判断某项能力是否已经可以使用 | [当前状态](./current-status.md) |
| 核对语法审计中 35 个 bug 的实现与诊断边界 | [逐项修复记录](./compiler-syntax-bug-fixes.md)（1.3.0） |
| 了解下一阶段的优先级、边界和完成条件 | [下一阶段](./next-development.md) |
| 理解普通源码、class/record 与宿主标注 | [类型指南](../03-guides/type-semantics.md) |
| 使用普通 C# / NuGet DLL 自动导入 | [普通类库导入](./ordinary-nuget-compilation-research.md)（1.4.2 有界切片） |
| 查看 C# 与 RazorVue 的具体支持边界及逐项 GitHub issue | [语法与 issue 索引](./csharp-syntax-gap-audit.md)（英文，区分已关闭 bug 与开放扩展） |
| 推进作者体验和协议扩展 | [下一阶段](./next-development.md)中的责任层与验收条件 |
| 执行 Vue 应用生态绑定扩展 | [P3 Vue 应用生态绑定扩展计划](./p3-vue-application-bindings-plan.md) |
| 讨论 C# + TSX 风格作者格式与智能感知 | [.jazor 提案 #138](https://github.com/devhxj/Jazor/issues/138)（待设计与实现） |
| 使用标准前端项目与已完成的工具链 | [类库产物契约](../02-architecture/library-artifact-contract.md)、[NodeApi 工具链指南](../03-guides/toolchain-migration.md) |
| 追溯已完成计划、旧审计和测试数字 | [历史演进](../05-history/evolution.md)；详细记录由 Git 历史保留 |

## 维护准则

路线图以辅助判断为目标，写入和迁出均以明确依据为准。

- 只有目标、责任归属、依赖和验收条件均已明确的工作，才进入[下一阶段](./next-development.md)。
- 新能力只有在 C# 作者面、官方 Razor SG、编译/运行时、真实浏览器与适用的发布消费者均具备证据后，才能从计划写入[当前状态](./current-status.md)。
- 完成、废弃或被替代的实施记录只沉淀稳定结论；过程材料由 Git 历史承担追溯责任。
