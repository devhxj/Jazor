# Jazor 架构转型开发计划

> Status: active
> Updated: 2026-07-27
> File size rule: this file and its child plan files must stay under 10KB each.

## 当前结论

当前 Razor-to-Vue 主线固定为传统 Vue 3 render-function/VNode，并通过 `Jazor.Vue` 显式启用：

```text
official Razor SG generated C#
    -> Roslyn IOperation
    -> Jazor.Compiler / SemanticWalker
    -> direct Vue h()/Fragment VNode emitter
    -> Vue render-function .mjs
```

`Jazor` 负责通用编译、分析、Emit 和 MSBuild 输出支持；`Jazor.Vue` 负责安装 Razor generator-driver Hook 及其 RazorVue analyzer payload。`JazorMode` 仅决定是否物化以及物化方式，不决定 Hook 是否启用。

选择传统 Vue 的直接原因是复用 Vuetify、Vue Router、Pinia 和既有 Vue 组件生态。SolidJS fine-grained/direct-DOM 只作为后续独立技术线，不进入当前 WBS。

render-context v1 现在定位为 oracle/过渡层：覆盖尚未 direct lowering 的 supported surface、保持行为对照和负向诊断测试，不作为长期 production lowering 形态。

## 当前执行策略

功能闭环和首份 G2 baseline 已落地，当前阶段转入 traditional Vue direct VNode emitter。

先把 supported `BuildRenderTree` 调用从 compiler 产出的 builder 协议调用降成 setup-scoped Vue `h(...)` render function，减少 runtime frame stack replay 与 frame-to-VNode materialization；后续再扩 component、slot、markup、bulk attrs、patch flags / block-level optimization。

## 硬边界

- 生产输入只来自 official Razor Source Generator generated C# 与 hook compilation 派生链。
- 不消费 Razor DR/IR，不回读 `.razor` 原文，不 nested-run Razor SG。
- RazorVue lowering 只输出传统 Vue render-function `.mjs`、`.mjs.map` 和 manifest。
- supported RenderTreeBuilder 调用优先走 direct VNode lowering；render-context 只保留为 oracle/过渡覆盖。
- 无法表达的 generated-code shape 必须 diagnostic 或进入明确未实现清单。
- 工具链选择必须显式；用户选择 `Deno` 或 `Netpack` 后只执行该实现。
- 不引入第二执行模型、手写 DOM diff、template/SFC 反推、wrapper marker transport。

## 分片索引

| 文件 | 内容 |
|---|---|
| [01-路线与边界.md](./razorvue-transition/01-路线与边界.md) | 架构路线、输入输出合同、禁止事项 |
| [02-代码优先WBS.md](./razorvue-transition/02-代码优先WBS.md) | 当前功能落地顺序 |
| [03-Gate与验收.md](./razorvue-transition/03-Gate与验收.md) | Gate、完成定义、测试入口 |
| [04-工具链.md](./razorvue-transition/04-工具链.md) | Deno / Netpack 显式 lane |
| [05-状态快照.md](./razorvue-transition/05-状态快照.md) | 已完成、进行中、推迟项 |

## 当前最高优先级

1. 推进 direct VNode emitter P0：线性 `OpenElement` / `AddAttribute` / `AddContent` / `CloseElement` 不再 replay runtime frame stack。
2. 继续维护 `scripts/csharp/benchmark-razorvue-g2.cs`，用 baseline 对比 direct emitter 的 runtime、browser、generated artifact 和 release performance report。
3. 根据 direct P0 report 拆分 component、slot、markup、bulk attrs、patch flags / block-level optimization。
4. 保持 Deno / Netpack production 主线稳定，性能采样不得引入第二 artifact contract。
5. Dev/HMR、跨平台旧线复跑和性能阈值优化在 baseline report 后单独排期。

## 维护规则

- 活跃计划文件单文件不得超过 10KB。
- 长状态、历史命令、专项矩阵必须放入分片文件。
- 分片仍过长时继续拆分，不在入口文件追加流水账。
- 文档不得描述第二执行路线。
