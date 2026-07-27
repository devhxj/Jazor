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
    -> render-context v1
    -> Vue render-function/VNode .mjs
```

直接目标是尽快把功能代码闭环落地，复用 Vuetify、Vue Router、Pinia 和手写 SFC 生态。性能采样和调优排在功能闭环之后。

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

1. 补齐 `RenderTreeBuilder` public surface 的快速功能实现。
2. 保持 render-context v1 行为确定，补 focused tests。
3. 补 component props/events/slots/bind/lifecycle 的 generated C# lowering。
4. 让 Deno 和 Netpack 都消费同一个 manifest 完成最小 production build smoke。
5. 功能闭环后再做 browser heap、throughput、gzip 和旧线 baseline。

## 工作流状态

| 工作流 | 当前判断 | 下一步 |
|---|---|---|
| Razor-to-Vue | G0/G1 已通过，当前主攻 G2-F 功能 Gate | 继续 RenderTreeBuilder 和 component surface |
| Compiler | 已接 RenderTreeBuilder host 与 current-component host | 补 accepted overload breadth 和 diagnostics |
| RazorVue | 已接 generated C# binder、component module framing、runtime assets | 补 component catalog/slot/bind/lifecycle breadth |
| Emit | 已接 VueRenderCatalog、manifest、runtime materialization | 保持 deterministic output，补 toolchain smoke |
| Toolchain | Deno/Netpack 都保留为显式 lane | 冻结共同 request/result contract |
| Vuetify | 当前选择传统 Vue 的核心理由 | 先保证 component boundary 能承载 Vuetify wrapper |

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
| G2-F | 进行中：功能 surface、component contract、lifecycle |
| G2-P | 推迟：功能闭环后执行性能采样和阈值判定 |
| G3 | 待做：Deno production + mixed SFC |
| G4 | 待做：Deno dev/HMR + Netpack production smoke |
| G5 | 待做：package consumer、sample、platform matrix |

## 维护规则

- 本文件只保留仓库级状态，不记录长命令输出。
- 单文件必须小于 10KB。
- 长表格或专项细节放入 `razorvue-transition/` 分片。
- 历史 Jolt、Razor IR、Razor-to-SFC 和 Component Runtime 文件不属于当前生产路线。
