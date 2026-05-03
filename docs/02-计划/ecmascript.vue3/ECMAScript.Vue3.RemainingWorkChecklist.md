# ECMAScript.Vue3 剩余完善清单

> Status: Post-Phase1 活跃清单  
> Updated: 2026-05-03  
> Positioning: 基于 [ECMAScript.Vue3 Authoring 落地计划](./ECMAScript.Vue3.Authoring.ImplementationPlan.md)、[ECMAScript.Vue3 API 覆盖矩阵](../../01-目标/ecmascript.vue3/vue3-api-coverage-matrix.md) 与当前 `src/ECMAScript.Vue3/` 真实代码状态整理的剩余工作清单。  
> Scope: Phase 1 已完成闭环；本清单只列 Phase 2/3 与后续设计项，不再把已收口的 `H(...)` / `VueObject` / read-side bag / object-literal contract 当作剩余事项重复展开。

## 1. 当前判断

`ECMAScript.Vue3` 当前已经完成了 Phase 1 的闭环收口。最近一轮补齐后，以下关键 authoring surface 已经进入“完成并受守护测试保护”的状态：

- `ChildrenToSlotIntrinsic` 默认插槽语法糖迁移与 typed slot contract 校验；
- `H(...)` canonical 分类、literal fast-path 与 single-evaluation IIFE 守护；
- `VueAttributeBag` / `VueSlotBag` / `UseAttrs()` / `UseSlots()` read-side bag；
- `VueModelRef<TValue>`、modifiers projection、`VueModelName<TProps,TValue>` named-model contract 与 typed `update:*` emit helper；
- `VueInjectOptions<T>` / `VueInjectEntry<T>` / `VueInjectRegistry<T>` object-form inject helper（含 symbol-key source）；
- `VueObject` / `[Spread]` 的覆盖顺序、静态 `null` omission 与 object-literal/dictionary 统一 lowering。

剩余问题已经不再属于 Phase 1，而主要是：

1. 如何让 Razor / Jolt 等上层工作流复用 Phase 1 contract；
2. 哪些高层 authoring convenience 需要先设计再实现；
3. 如何把 `ECMAScript.Vue3` 沉淀成可复制的外部库模板。

## 2. Phase 2 主线项

这些事项承接已完成的 Phase 1 规范层，不应通过反向扩张 compiler Vue 特路来解决。

### 2.1 Razor -> `H(...)` canonical lowering

需要继续完成：

- 继续扩展 Razor authoring 到 Phase 1 `H(...)` 规范层的稳定映射；
- 已完成 RazorVue `h(...)` 最小 arity / `null` 占位清理；继续统一剩余 props / children / slots / default-slot sugar 的 canonical shape；
- 禁止 Razor 侧重新引入一套偏离 `ECMAScript.Vue3` contract 的特殊路径。

### 2.2 diagnostics 与 contract 对齐

需要继续完成：

- Razor 产物与手写 `H(...)` authoring 的诊断边界一致；
- object-literal / slot / props / attrs / directive 相关错误能落到可解释的 contract；
- 不把 Vue 语义重新散落到 compiler 名称特判中。

### 2.3 外部库模板化

`ECMAScript.Vue3` 已经是第一个官方外部库样例，但还没有完全沉淀成模板。

需要继续完成：

- 已完成 Vue3 目录分层 / project metadata / doc-split / source backflow guard；继续把这些守护抽成更可复用的外部库样板；
- proxy surface / layout guard / doc guard 的模板化；
- 文档域拆分模式的可复用化；
- 为后续外部库抽出“应该照着做什么”的样板约束。

## 3. 需要先设计再实现的项

这些事项不能机械补 binding；需要先把 C# authoring contract 设计稳定。

### 3.1 `useModel()` 完整 authoring

当前 `UseModel<TValue>(...)` 已覆盖：

- typed model ref；
- get/set transform；
- modifiers projection（`GetModifiers()` / `GetModifiers<T>()`）。

最近一轮又补入：

- `VueModelName<TProps,TValue>` typed named-model contract；
- `ModelName(...)` / `ModelPropName(...)` / `ModelUpdateEventName(...)` helper；
- `UseModel<TProps,TValue>(props, model[,options])` authoring 路线。
- `VueSetupContext.Emit(model, value)` typed `update:*` emit helper。

剩余设计点：

- 与显式 `props` / `emits` 声明的协同规则；
- higher-level `v-model` convenience 是否需要继续在这个 typed contract 之上补 helper。

### 3.2 Options API 长尾

基础面已覆盖，但复杂形态还没有闭环。

剩余设计点：

- inject default / factory default 的更深 this-bound contract；
- 更细粒度 this-bound provide/inject 行为；
- `defineComponent(...)` 更完整 object surface 如何分层；
- 哪些 Options API 只是低层兼容 binding，哪些是推荐 authoring path。

### 3.3 custom elements 完整 authoring 策略

runtime binding 已覆盖，但 authoring 体验还不完整。

剩余设计点：

- CE props/events authoring 的推荐 contract；
- light DOM / shadow DOM authoring 边界；
- 与 component options 的 props/emits surface 复用策略；
- 真实业务场景下需要补哪些 convenience，而不是单纯补 Vue 官方字段名。

## 4. 工程化与文档项

### 4.1 文档同步机制

需要继续完成：

- `VueObject` 新增 convenience attrs 的设计理由写回映射细节文档；
- 覆盖矩阵与真实代码状态持续同步；
- 计划文档与状态快照避免漂移。

## 5. 非当前目标

这些仍然是独立工作流，不应并入当前 `ECMAScript.Vue3` 主线清单：

- SFC macros；
- template directives；
- special elements（`<component>` / `<slot>` / `<template>`）；
- SSR renderer / hydration pipeline；
- custom renderer；
- `.vue` / Jolt / RazorVue 的专用工程化 authoring。

## 6. 推荐推进顺序

建议按以下顺序继续推进：

1. Razor -> `H(...)` Phase 2 规范收口
2. diagnostics 与 contract 对齐
3. 外部库模板化
4. `useModel()` higher-level v-model contract
5. Options API 长尾设计
6. custom elements authoring 设计

## 7. Phase 1 完成判断

以下条件已经满足，因此可以认为 `ECMAScript.Vue3` 的 Phase 1 收口阶段已完成：

- `VueObject` 的内置属性纳入边界稳定；
- read-side bags 在真实场景下可预测、可诊断、可测试；
- `H(...)` 不再继续按示例扩张，而是严格复用 canonical 分类；
- object-literal / dictionary 路线具备统一语义和稳定矩阵回归；
- `useModel` named-model / typed emit / modifiers contract 已闭环；
- 后续新增 Vue API 不再要求先扩张 compiler Vue 专用分支。
