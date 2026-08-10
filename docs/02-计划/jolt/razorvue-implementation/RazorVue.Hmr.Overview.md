# RazorVue HMR 概览

> Status: 活跃参考
> Positioning: RazorVue HMR 的当前保守运行时通道和后续扩展边界概览。

## 1. 文档定位

本文档是 RazorVue HMR 文档集的入口点。

它不重复整个 RazorVue 架构。
它回答四个问题：

1. RazorVue 目前处于什么 HMR 状态
2. 每个 HMR 文档的用途
3. HMR 工作恢复时应按什么顺序阅读
4. 已交付通道的边界与后续工作

## 2. 当前状态

当前 HMR 状态为：

- HMR 身份、分离哈希和边界元数据已经由编译器产物和 manifest 携带
- 开发宿主已实现狭窄的 `template-only` 模块更新通道
- 浏览器必须通过 `JazorHmr.accept(moduleId, handler)` 显式接管更新
- 缺少处理器、动态导入失败、描述符/逻辑变化、身份变化和其他不明确变化均完整刷新
- 不自动替换 Vue 实例，也不承诺组件状态保留

当前共识：

1. HMR 是编译器加宿主的契约，而非 bundler 的事后补充
2. 编译器拥有稳定身份和变更类别元数据
3. `DenoHost` 拥有实际的热更新编排
4. 当前运行时通道只接受可证明的模板变更，安全性不明确时完整重载

## 3. 核心结论

主要固定结论：

1. HMR 是 RazorVue 架构的一部分，0.7 已交付一条受限的运行时通道。
2. HMR 必须基于稳定的 `ComponentId` 和 `ModuleId`。
3. HMR 必须保留描述符、模板和逻辑的分离哈希。
4. HMR 分类属于编译器拥有的产物，而非仅基于最终 JS 差异比较。
5. `DenoHost` 拥有更新的运行时应用。
6. 允许保守回退到完全重载；不允许静默的不安全热补丁或自动状态保留声明。

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

## 6. 后续扩展前必须重新确认的事项

在扩大当前仅模板通道之前，至少重新检查：

1. `ComponentId` 和 `ModuleId` 身份规则是否已经足够稳定
2. 描述符/模板/逻辑哈希是否在宿主交接之前产出
3. `DenoHost` 是否仍然拥有运行时更新编排
4. sourcemap/source-origin 数据是否仍然与 HMR 诊断需求对齐
5. Vue 库集成是否暴露了足够的元数据来保守地分类重载安全性

## 7. 一句话结论

RazorVue HMR 已从编译器拥有的身份和变更元数据落地为受限运行时通道；后续扩展仍必须由宿主消费同一契约，不能绕过保守回退。
