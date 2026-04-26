# RazorVue HMR 概览

> Status: 活跃参考
> Positioning: 未来 RazorVue HMR 工作的预留通道概览；不代表运行时 HMR 已处于活跃实现中。

## 1. 文档定位

本文档是 RazorVue HMR 文档集的入口点。

它不重复整个 RazorVue 架构。
它回答四个问题：

1. RazorVue 目前处于什么 HMR 状态
2. 每个 HMR 文档的用途
3. HMR 工作恢复时应按什么顺序阅读
4. 在实现开始前仍须重新确认什么

## 2. 当前状态

当前 HMR 状态为：

- HMR 已在架构上预留
- 运行时 HMR 未实现
- 编译器侧的身份和变更分类已优先设计
- `DenoHost` 仍然是最终的 HMR 宿主所有者

当前共识：

1. HMR 是编译器加宿主的契约，而非 bundler 的事后补充
2. 编译器拥有稳定身份和变更类别元数据
3. `DenoHost` 拥有实际的热更新编排
4. 第一阶段可以在安全性不明确时过度分类为完全重载

## 3. 核心结论

主要固定结论：

1. HMR 是 RazorVue 架构的一部分，即使运行时支持延后落地。
2. HMR 必须基于稳定的 `ComponentId` 和 `ModuleId`。
3. HMR 必须保留描述符、模板和逻辑的分离哈希。
4. HMR 分类属于编译器拥有的产物，而非仅基于最终 JS 差异比较。
5. `DenoHost` 拥有更新的运行时应用。
6. 允许保守回退到完全重载；不允许静默的不安全热补丁。

## 4. 文档角色

### 4.1 快速结论

- [RazorVue.Hmr.DecisionSummary.md](./RazorVue.Hmr.DecisionSummary.md)

适用场景：

- 你只需要已确定的 HMR 决策
- 你需要快速恢复工作

### 4.2 完整设计

- [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)

适用场景：

- 你需要 HMR 责任划分
- 你需要身份和变更分类模型
- 你需要编译器/宿主边界

### 4.3 硬性约束

- [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)

适用场景：

- 你正在评审一个实现
- 你需要知道什么不能临时决定

### 4.4 实现排序

- [RazorVue.Hmr.ImplementationChecklist.md](./RazorVue.Hmr.ImplementationChecklist.md)

适用场景：

- 实现即将开始
- 你需要分阶段的执行和验收门

### 4.5 常见失败模式

- [RazorVue.Hmr.Pitfalls.md](./RazorVue.Hmr.Pitfalls.md)

适用场景：

- 你想避免不安全的 HMR 设计漂移
- 你正在检查某个实现是否变得过于乐观

## 5. 建议阅读顺序

### 5.1 如果你只想要最终方向

按此顺序阅读：

1. [RazorVue.Hmr.DecisionSummary.md](./RazorVue.Hmr.DecisionSummary.md)
2. [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)

### 5.2 如果你即将开始实现

按此顺序阅读：

1. [RazorVue.DecisionSummary.md](../../../01-目标/razorvue/design/RazorVue.DecisionSummary.md)
2. [RazorVue.Hmr.DecisionSummary.md](./RazorVue.Hmr.DecisionSummary.md)
3. [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)
4. [RazorVue.DenoHostContract.md](../../../01-目标/razorvue/design/RazorVue.DenoHostContract.md)
5. [RazorVue.ComponentDescriptorSpec.md](../../../01-目标/razorvue/design/RazorVue.ComponentDescriptorSpec.md)
6. [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)
7. [RazorVue.Hmr.Pitfalls.md](./RazorVue.Hmr.Pitfalls.md)
8. [RazorVue.Hmr.ImplementationChecklist.md](./RazorVue.Hmr.ImplementationChecklist.md)

### 5.3 如果你正在评审代码/设计

按此顺序阅读：

1. [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)
2. [RazorVue.Hmr.Pitfalls.md](./RazorVue.Hmr.Pitfalls.md)
3. [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)
4. [RazorVue.DenoHostContract.md](../../../01-目标/razorvue/design/RazorVue.DenoHostContract.md)

## 6. 实现前必须重新确认的事项

在真正的 HMR 工作开始之前，至少重新检查：

1. `ComponentId` 和 `ModuleId` 身份规则是否已经足够稳定
2. 描述符/模板/逻辑哈希是否在宿主交接之前产出
3. `DenoHost` 是否仍然拥有运行时更新编排
4. sourcemap/source-origin 数据是否仍然与 HMR 诊断需求对齐
5. Vue 库集成是否暴露了足够的元数据来保守地分类重载安全性

## 7. 一句话结论

RazorVue HMR 从编译器拥有的身份加变更元数据开始，成长为 `DenoHost` 拥有的运行时热更新行为，无需重新设计 RazorVue 主管线。
