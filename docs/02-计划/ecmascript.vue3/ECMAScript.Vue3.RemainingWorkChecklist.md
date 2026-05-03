# ECMAScript.Vue3 剩余完善清单

> Status: 活跃清单  
> Updated: 2026-05-03  
> Positioning: 基于 [ECMAScript.Vue3 Authoring 落地计划](./ECMAScript.Vue3.Authoring.ImplementationPlan.md)、[ECMAScript.Vue3 API 覆盖矩阵](../../01-目标/ecmascript.vue3/vue3-api-coverage-matrix.md) 与当前 `src/ECMAScript.Vue3/` 真实代码状态整理的剩余工作清单。  
> Scope: 只列仍需推进的点；已完成的项目拆分、文档拆分、`ChildrenToSlotIntrinsic` 迁移、`VueObject` 第一批常用 convenience attrs 等不再重复展开。

## 1. 当前判断

`ECMAScript.Vue3` 当前已经完成了 Phase 1 的主体结构收口。最近一轮又补齐了几块此前处于“还差一点”的 authoring surface：

- `VueAttributeBag` 的受控 convenience reads 与 `UseAttrs()` 真实回归；
- `VueModelRef<TValue>` + modifiers projection；
- `VueInjectOptions<T>` / `VueInjectEntry<T>` / `VueInjectRegistry<T>` object-form inject helper；
- `VueObject` / `[Spread]` 的覆盖顺序与静态 `null` omission 回归。

剩余问题不再主要是“还差多少 API 名字”，而是：

1. 哪些 surface 已经足够稳定，可以继续按矩阵补齐；
2. 哪些 surface 仍然缺 authoring 设计，不能机械补 binding；
3. 哪些问题属于 Phase 2 / Phase 3，不能反向塞回 compiler 核心。

## 2. 立即收口项

这些事项仍属于当前 `ECMAScript.Vue3` 主线，且不应引入新的 Vue-specific compiler 特路。

### 2.1 `VueObject` surface 治理

虽然 `VueObject` 已补入一批高频原生 HTML convenience attrs，但它的纳入边界还没有形成稳定规则。

需要继续完成：

- 明确 `VueObject` 内置属性的纳入标准：
  - 高频；
  - 多项目复用概率高；
  - 类型单义；
  - 直接映射到最终 JS key；
  - 不需要额外运行时协议。
- 明确哪些属性不应继续进入 `VueObject`：
  - `aria-*`；
  - `data-*`；
  - 长尾、语义不稳或存在多种 authoring 习惯的属性；
  - 更适合放入 typed props bag 或 `Attrs` / `Dataset` / indexer 的属性。
- 统一便利属性的值类型策略：
  - `bool` / `string` / `int`；
  - `Either<double, string>` 这类真实 union 边界；
  - 哪些地方允许 string fallback，哪些地方不允许。
- 后续只在真实需求出现时增量补高频 convenience attrs，不再回到“看到官方属性名就继续塞进 `VueObject`”的扩张路线。

### 2.2 read-side bag hardening

这一项的当前阶段收口已经完成：

- `VueAttributeBag` 已补齐受控 convenience reads；
- `UseAttrs()` / `UseAttrs<T>()`、listener bridge、`UseSlots<T>()` / scoped slot 读取都已有回归；
- `setup(context)` 与 composition helpers 继续复用同一套 read-side bag contract。

因此这里不再作为主要剩余工作；后续只按真实项目需求小步扩展。

### 2.3 `H(...)` canonical 治理继续收口

`H(...)` overload 现在已经能用，但治理工作还没结束。

需要继续完成：

- 把 element / component / props / slots / direct-child sugar 的 canonical 分类进一步固化到测试。
- 防止后续按 Vue 文档示例继续无节制膨胀 overload 家族。
- 为 default slot sugar 增补边界回归：
  - literal child；
  - single-evaluation IIFE；
  - typed default slot contract；
  - scoped default slot 不允许 direct-child sugar。
- 明确哪些新增场景应复用现有 canonical 分类，哪些确实需要新 surface。

### 2.4 object-literal / dictionary 路线矩阵补齐

当前 `VueObject`、`VueDictionary`、各类 registry、plugin options 都已经走统一 structural object 路线，并且最近补齐了：

- `initializer` / indexer / `Add(string, ...)` 路线的主要回归；
- `[Spread]` 展开顺序与覆盖顺序回归；
- 静态 `null` omission 回归；
- dynamic key 正反向边界回归。

这里剩余的主要工作不再是功能缺口，而是后续新增 object host 时继续复用并守住同一套 lowering 规则。

## 3. 需要先设计再实现的项

这些事项不能机械补 binding；需要先把 C# authoring contract 设计稳定。

### 3.1 `useModel()` 完整 authoring

当前 `UseModel<TValue>(...)` 已覆盖：

- typed model ref；
- get/set transform；
- modifiers projection（`GetModifiers()` / `GetModifiers<T>()`）。

剩余设计点：

- named model 的 authoring contract；
- 与显式 `props` / `emits` 声明的协同规则；
- `update:*` emit 的 typed contract；
- 是否需要单独 helper type，还是继续依赖 overload + options record。

### 3.2 Options API 长尾

基础面已覆盖，但复杂形态还没有闭环。

剩余设计点：

- function-form `provide`；
- inject default / factory default 的完整 contract；
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

## 4. Phase 2 主线事项

这些事项属于 `Razor -> H` 规范层，不应通过反向扩张 `ECMAScript.Vue3` compiler 特路来解决。

### 4.1 Razor -> `H(...)` canonical lowering

需要继续完成：

- Razor authoring 到 Phase 1 `H(...)` 规范层的稳定映射；
- canonical props / children / slots 形状保持一致；
- 避免 Razor 侧重新引入一套偏离 `ECMAScript.Vue3` contract 的特殊路径。

### 4.2 diagnostics 与 contract 对齐

需要继续完成：

- Razor 产物与手写 `H(...)` authoring 的诊断边界一致；
- object-literal / slot / props / attrs / directive 相关错误能落到可解释的 contract；
- 不把 Vue 语义重新散落到 compiler 名称特判中。

## 5. 工程化与样例模板项

### 5.1 外部库模板化

`ECMAScript.Vue3` 已经是第一个官方外部库样例，但还没有完全沉淀成模板。

需要继续完成：

- 目录分层守护规则的可复用化；
- 文档域拆分模式的可复用化；
- proxy surface / layout guard / doc guard 的模板化；
- 为后续外部库抽出“应该照着做什么”的样板约束。

### 5.2 文档同步机制

需要继续完成：

- `VueObject` 新增 convenience attrs 的设计理由写回映射细节文档；
- 覆盖矩阵与真实代码状态持续同步；
- 计划文档与状态快照避免漂移。

## 6. 非当前目标

这些仍然是独立工作流，不应并入当前 `ECMAScript.Vue3` 主线清单：

- SFC macros；
- template directives；
- special elements（`<component>` / `<slot>` / `<template>`）；
- SSR renderer / hydration pipeline；
- custom renderer；
- `.vue` / Jolt / RazorVue 的专用工程化 authoring。

## 7. 推荐推进顺序

建议按以下顺序继续推进：

1. `H(...)` canonical 分类与 default-slot sugar 边界守护
2. `useModel()` named model / higher-level v-model contract
3. Options API 长尾设计
4. custom elements authoring 设计
5. Razor -> `H(...)` Phase 2 规范收口
6. 外部库模板化与文档同步机制

## 8. 完成判断

当以下条件满足时，可以认为 `ECMAScript.Vue3` 的当前收口阶段基本完成：

- `VueObject` 的内置属性纳入边界稳定；
- read-side bags 在真实场景下可预测、可诊断、可测试；
- `H(...)` 不再继续按示例扩张，而是严格复用 canonical 分类；
- object-literal / dictionary 路线具备统一语义和稳定矩阵回归；
- `useModel`、Options API 长尾、custom elements authoring 已明确哪些是下一阶段设计项，哪些已稳定；
- 后续新增 Vue API 不再要求先扩张 compiler Vue 专用分支。
