# RazorVue 概述

> 状态：活跃参考
> 定位：活跃 RazorVue 文档集的主深层文档入口点。

## 1. 文档定位

本文档是 RazorVue 文档集的入口点。

它不重复完整设计。
它回答三个问题：

1. RazorVue 当前处于什么状态
2. 每个 RazorVue 文档的用途
3. 工作恢复时按什么顺序阅读它们

## 2. 当前状态

RazorVue 的当前状态是：

- 核心主管道里程碑已经落地
- 当前阶段是围绕最小路径的阶段一关闭工作
- 阶段一范围仍然有意有限
- HMR 和 sourcemap 保持元数据优先，保留给后续里程碑
- 当前逻辑车道仍然是保守子集，但现在包括生命周期安全子集降低加上最小设置侧逻辑闭合，用于简单字段和辅助方法，其参数可以安全地降低到 `setup()`，包括固定深度两级辅助组合和超出该边界的显式 `JAZORVGA006` 拒绝

截至当前实现车道，仓库已经有：

- `[ECMAScriptModule]` 入口拆分为静态模块和 RazorVue 组件
- `Jazor.Razor` / `Jazor.RazorVue` / `Jazor.RazorVue.Analysis` 拆分，`Jazor.RazorVue` 现在拥有 RazorVue 核心语义车道，`Jazor.RazorVue.Analysis` 保持瘦 Roslyn 主机
- 当前 RazorVue 入口/误用集的 Roslyn 分析器：
  - `JAZORVUE001` 无效入口继承
  - `JAZORVUE002` 直接 `ComponentBase` 入口
  - `JAZORVUE004` `StateHasChanged`
  - `JAZORVUE005` `ShouldRender`
  - `JAZORVUE006` `SetParametersAsync`
- props / emits / slots 的组件描述符提取
- `RazorVueCompilationContext` -> `RazorVueSemanticSnapshot` -> `RazorVuePipeline` -> `RazorVueArtifactFactory` -> `RazorVueCatalog` 主管道
- 真实的 `BuildRenderTree` 提取/降低车道，可以发送 Vue `defineComponent + setup + render`
- 经过验证的组件节点降低，涵盖 props、emit/监听器连线和默认/命名/作用域插槽流
- `if` 和 `foreach` 的最小结构降低
- `OnInitialized*`、`OnParametersSet*` 和 `OnAfterRender*` 的生命周期安全子集降低，包括 `watch(..., { immediate: true })` 和 `firstRender` 桥接
- 简单实例字段和辅助方法的最小设置侧逻辑降低，其参数可以安全地投影到 `setup()` 中，现在包括固定深度两级辅助组合和针对更深层或不支持的设置辅助链的显式 `JAZORVGA006` 投影
- 工件标识/哈希塑造和基本 HMR 边界分类

以下内容仍未完成第一阶段覆盖：

- 超出当前生命周期/事件回调/设置字段/辅助安全子集的更广泛逻辑提取
- 完整的组件实例语义
- 全面的 Razor 语法覆盖
- `Dispose*`、`ShouldRender` 和 `SetParametersAsync` 运行时等效处理
- 最终 `DenoHost` 端到端集成
- 最终 HMR 运行时和 sourcemap 发送

当分析/降低遇到不支持的形状时，`JAZORVGA001`（`RazorVue 目录生成失败`）仍然是一般回退表面。当前瘦 `Jazor.RazorVue.Analysis` 主机路径也为以下内容投影结构化编译器面向问题：

- `JAZORVGA002` 组件未找到
- `JAZORVGA003` 歧义短组件名称
- `JAZORVGA004` 保留内置名称冲突
- `JAZORVGA005` 不支持的生命周期降低
- `JAZORVGA006` 不支持的设置侧逻辑降低

当前阶段备忘录：

- [RazorVue 阶段评估（2026-04-06）](../../../docs/status/2026-04-06-razorvue-stage-assessment.md)

当前共识是：

1. 保持 RazorVue Vue 优先
2. 使用分析器加生成代码分析进行语义提取
3. 保持最终构建所有权与 `DenoHost`
4. 在生态系统扩展之前关闭最小主管道

## 3. 核心结论

主要固定结论是：

1. RazorVue 是 Vue 优先，而非通用多框架 UI 抽象。
2. `[ECMAScriptModule]` 保持统一入口标记。
3. Razor 组件必须继承 `JazorComponent`。
4. 基础层次结构是 `ComponentBase -> JazorComponent -> VueComponent`。
5. 最终公共项目拆分是 `Jazor.Compiler` + `Jazor.Razor` + `Jazor.RazorVue` + `Jazor.RazorVue.Analysis`。
6. RazorVue 不在源生成器排序上构建其主管道。
7. Razor 组件不重用普通静态模块降低。
8. 编译器发送 Vue ESM 工件，`DenoHost` 拥有统一构建。
9. HMR 和 sourcemap 通过元数据在架构中保留，未完全实现为阶段一运行时功能。

## 4. 文档角色

### 4.1 快速结论

- [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)

在以下情况下使用它：

- 您只想要最终决策
- 您不想重新阅读完整设计

### 4.2 完整设计

- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.ProjectResponsibilities.md](./RazorVue.ProjectResponsibilities.md)

在以下情况下使用它们：

- 您需要架构、边界和职责
- 您需要理解为什么设计这样塑造
- 您需要当前项目拆分和扩展接缝

### 4.3 聚焦规范

- [RazorVue.ComponentDescriptorSpec.md](./RazorVue.ComponentDescriptorSpec.md)
- [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)
- [RazorVue.ImplementationSkeleton.md](./RazorVue.ImplementationSkeleton.md)
- [RazorVue.LibraryAuthoring.Design.md](./RazorVue.LibraryAuthoring.Design.md)
- [RazorVue.Vuetify.FirstPackage.md](./RazorVue.Vuetify.FirstPackage.md)

在以下情况下使用它们：

- 您正在实现契约提取
- 您正在实现面向主机的工件/清单流
- 您需要比主设计文档更窄的、面向实现的规范
- 您正在定义第一个库创作包形状

### 4.4 阶段评估

- [RazorVue 阶段评估（2026-04-06）](../../../docs/status/2026-04-06-razorvue-stage-assessment.md)

在以下情况下使用它：

- 您需要当前设计/实现状态的日期检查点
- 您希望在一个地方完成/部分/开放的工作
- 您需要在下一个实现切片之前恢复备忘录

### 4.5 审查备忘录

- [RazorVue.Review.md](./RazorVue.Review.md)

在以下情况下使用它：

- 您想要双重通过审查结果
- 您想要在一个地方获得剩余风险和直接下一步

### 4.6 HMR 包

- [RazorVue.Hmr.Overview.md](./RazorVue.Hmr.Overview.md)
- [RazorVue.Hmr.DecisionSummary.md](./RazorVue.Hmr.DecisionSummary.md)
- [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)
- [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)
- [RazorVue.Hmr.ImplementationChecklist.md](./RazorVue.Hmr.ImplementationChecklist.md)
- [RazorVue.Hmr.Pitfalls.md](./RazorVue.Hmr.Pitfalls.md)

在以下情况下使用它们：

- 您正在准备未来的 HMR 支持
- 您需要保留的标识/更改模型
- 您需要编译器/`DenoHost` HMR 边界

### 4.7 硬约束

- [RazorVue.HardRules.md](./RazorVue.HardRules.md)

在以下情况下使用它：

- 您需要审查规则
- 您需要知道在实现期间不能临时决定什么

### 4.8 实现排序

- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
- [RazorVue.ImplementationSkeleton.md](./RazorVue.ImplementationSkeleton.md)
- [RazorVue.FirstPrPlan.md](./RazorVue.FirstPrPlan.md)（历史上下文）

在以下情况下使用它们：

- 实际开始实现
- 您需要分阶段执行和验收门

### 4.9 常见失败模式

- [RazorVue.Pitfalls.md](./RazorVue.Pitfalls.md)

在以下情况下使用它：

- 您希望避免架构漂移
- 您正在审查更改并希望早期发现熟悉的错误转向

### 4.10 创作产品方向

- [RazorVue.Authoring.ProductDefinition.md](./RazorVue.Authoring.ProductDefinition.md)
- [2026-04-06-razorvue-v1-authoring-roadmap.md](../../../docs/superpowers/plans/2026-04-06-razorvue-v1-authoring-roadmap.md)
- [2026-04-06-razorvue-v1-authoring-pr-breakdown.md](../../../docs/superpowers/plans/2026-04-06-razorvue-v1-authoring-pr-breakdown.md)

在以下情况下使用它们：

- 您正在为 RazorVue 定义 C# 创作体验
- 您正在规划库包装器工作，如 Vuetify
- 您需要创作车道的分阶段执行计划
- 您需要创作车道的执行大小的 PR 范围

## 5. 推荐阅读顺序

### 5.1 如果您只想要最终方向

按以下顺序阅读：

1. [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)
2. [RazorVue.HardRules.md](./RazorVue.HardRules.md)

### 5.2 如果您即将实现

按以下顺序阅读：

1. [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)
2. [RazorVue.Design.md](./RazorVue.Design.md)
3. [RazorVue.ComponentDescriptorSpec.md](./RazorVue.ComponentDescriptorSpec.md)
4. [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)
5. [RazorVue.Hmr.DecisionSummary.md](./RazorVue.Hmr.DecisionSummary.md)
6. [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)
7. [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)
8. [RazorVue.ImplementationSkeleton.md](./RazorVue.ImplementationSkeleton.md)
9. [RazorVue.FirstPrPlan.md](./RazorVue.FirstPrPlan.md)（历史上下文）
10. [RazorVue.HardRules.md](./RazorVue.HardRules.md)
11. [RazorVue.Pitfalls.md](./RazorVue.Pitfalls.md)
12. [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
13. [2026-04-06-razorvue-v1-authoring-roadmap.md](../../../docs/superpowers/plans/2026-04-06-razorvue-v1-authoring-roadmap.md)
14. [2026-04-06-razorvue-v1-authoring-pr-breakdown.md](../../../docs/superpowers/plans/2026-04-06-razorvue-v1-authoring-pr-breakdown.md)

### 5.3 如果您正在审查代码/设计

按以下顺序阅读：

1. [RazorVue.HardRules.md](./RazorVue.HardRules.md)
2. [RazorVue.Pitfalls.md](./RazorVue.Pitfalls.md)
3. [RazorVue.Design.md](./RazorVue.Design.md)
4. [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)
5. [RazorVue.Hmr.Pitfalls.md](./RazorVue.Hmr.Pitfalls.md)
6. [RazorVue.ComponentDescriptorSpec.md](./RazorVue.ComponentDescriptorSpec.md)
7. [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)

## 6. 下一个实现阶段之前必须重新确认的内容

在下一个实现阶段开始之前，重新检查至少：

1. `JazorComponent` / `VueComponent` API 表面仍与当前目标一致
2. 分析器仍接受的主语义提取点
3. 普通静态模块路径不需要首先进行主要入口重构
4. `DenoHost` 清单期望仍与计划的工件模型兼容
5. HMR/sourcemap 保留要求仍在实现计划中
6. HMR 标识和边界分类仍与保留的工件模型一致

## 7. 一行结论

如果您需要稍后恢复 RazorVue，从这里开始，然后阅读：

1. [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)
2. [RazorVue.HardRules.md](./RazorVue.HardRules.md)
3. [RazorVue.Hmr.DecisionSummary.md](./RazorVue.Hmr.DecisionSummary.md)
4. [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
