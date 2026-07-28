# Jazor 工作流总览

> Status: active
> Updated: 2026-07-27
> File size rule: keep under 10KB.

## 当前主线

当前唯一 Razor-to-Vue 主线是传统 Vue render-function/VNode：

```text
official Razor SG generated C#
    -> Roslyn IOperation
    -> Jazor.Compiler / SemanticWalker
    -> direct Vue h()/Fragment VNode emitter
    -> Vue render-function .mjs
```

功能代码闭环和首份 G2 baseline 已落地。当前直接目标是稳定 Vue render-function emitter；render-context v1 仅作为验证参照，Vuetify、Vue Router 与 Pinia 继续复用统一的 host binding 与 production module 主线。

## 快速入口

| 入口 | 说明 |
|---|---|
| [Jazor 架构转型开发计划](./Jazor%20架构转型开发计划.md) | 当前 RazorVue 主计划入口 |
| [代码优先 WBS](./razorvue-transition/02-代码优先WBS.md) | 当前功能实现顺序 |
| [路线与边界](./razorvue-transition/01-路线与边界.md) | 输入/输出/禁止事项 |
| [Gate 与验收](./razorvue-transition/03-Gate与验收.md) | 功能 Gate、性能 Gate、测试入口 |
| [工具链](./razorvue-transition/04-工具链.md) | Deno / Netpack 显式 lane |
| [状态快照](./razorvue-transition/05-状态快照.md) | 已完成、缺口、推迟项 |
| [ECMAScript.Vue3 / Vuetify](../01-目标/ecmascript.vue3/README.md) | Vue3 与 Vuetify authoring 目标入口 |

## 当前优先级

1. 推进 direct VNode emitter P0：线性 element/content/attribute lowering 直接生成 setup-scoped `h(...)`。
2. 用 G2 benchmark 对比 direct emitter 的 runtime/browser/generated artifact/release performance report。
3. 根据报告拆分 component、slot、markup、bulk attrs、patch flags / block-level optimization。
4. 保持 Deno/Netpack production toolchain 主线稳定。
5. Dev/HMR 与阈值优化在 baseline 后单独排期。

## 工作流状态

| 工作流 | 当前判断 | 下一步 |
|---|---|---|
| Razor-to-Vue | G2-F 功能 Gate 和首份 G2-P baseline 已闭环，当前主攻 direct VNode emitter P0 | 扩 direct element/content/attribute coverage |
| Compiler | 已接 RenderTreeBuilder host 与 current-component host | 继续让 C# expression/member semantics 走 SemanticWalker |
| RazorVue | 已接 generated C# binder、component module framing、direct P0 emitter、runtime assets | 对比 direct 与 oracle 行为/性能 |
| Emit | 已接 VueRenderCatalog、manifest、runtime materialization | 保持 artifact contract 稳定 |
| Toolchain | Deno/Netpack 都保留为显式 lane | 保持共同 request/result contract |
| Vuetify | 当前选择传统 Vue 的核心理由 | 保持 package import smoke，暂不扩生态面 |

## 路线规则

- 只走 official Razor SG generated C# -> `IOperation`。
- 不消费 Razor DR/IR，不回读 `.razor` 原文。
- 只输出传统 Vue render-function/VNode `.mjs`。
- runtime、lowering 和 toolchain 都只执行显式主线。
- 用户显式选择 `Deno` 或 `Netpack`。
- unsupported shape 必须 diagnostic 或进入未实现清单。

## Gate 摘要

| Gate | 当前判断 |
|---|---|
| G0 | 通过：SG generated C# + hook compilation derivation |
| G1 | 通过：真实 Counter browser 首切片 |
| G2-F | 通过：功能 surface、component contract、lifecycle、production toolchain smoke |
| G2-P | 当前：执行性能采样和阈值判定 |
| G3 | 已并入 production toolchain smoke：Deno Bundle 与 render-function module |
| G4 | 部分通过：Netpack production 已过；Deno dev/HMR 待单独排期 |
| G5 | 部分通过：package consumer 已过；platform matrix 待单独排期 |

## 维护规则

- 本文件只保留仓库级状态，不记录长命令输出。
- 单文件必须小于 10KB。
- 长表格或专项细节放入 `razorvue-transition/` 分片。
- Razor IR 与 Razor-to-SFC 不属于当前生产路线。
