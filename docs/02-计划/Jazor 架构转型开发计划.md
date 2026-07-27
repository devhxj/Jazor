# Jazor 架构转型开发计划

> Status: active
> Updated: 2026-07-27
> File size rule: this file and its child plan files must stay under 10KB each.

## 当前结论

当前 RazorVue 主线固定为传统 Vue 3 render-function/VNode：

```text
official Razor SG generated C#
    -> Roslyn IOperation
    -> Jazor.Compiler / SemanticWalker
    -> render-context v1
    -> Vue render-function/VNode .mjs
```

选择传统 Vue 的直接原因是复用 Vuetify、Vue Router、Pinia 和既有 Vue 组件生态。SolidJS fine-grained/direct-DOM 只作为后续独立技术线，不进入当前 WBS。

## 当前执行策略

先快速落地功能代码，再做性能测试。

当前阶段不把性能阈值作为功能实现前置条件。性能相关工作只保留协议、采样脚本和将来验收入口；在 RenderTreeBuilder surface、component props/events/slots/bind、lifecycle、Deno/Netpack toolchain smoke 都形成可运行闭环前，不再追加性能调优任务。

## 硬边界

- 生产输入只来自 official Razor Source Generator generated C# 与 hook compilation 派生链。
- 不消费 Razor DR/IR，不回读 `.razor` 原文，不 nested-run Razor SG。
- RazorVue lowering 只输出传统 Vue render-function `.mjs`、`.mjs.map` 和 manifest。
- RenderTreeBuilder 只走一套 render-context/VNode lowering。
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

1. 补齐 `RenderTreeBuilder` public surface 的快速功能实现与 focused tests。
2. 保持 render-context v1 runtime 行为确定，功能优先于性能调优。
3. 补齐 component parameter、event、slot、bind、lifecycle 的 generated C# lowering。
4. 让 Deno 和 Netpack 都能消费同一个 manifest 完成最小 production build smoke。
5. 功能闭环后再进入 G2 性能采样、browser heap 和阈值判定。

## 维护规则

- 活跃计划文件单文件不得超过 10KB。
- 长状态、历史命令、专项矩阵必须放入分片文件。
- 分片仍过长时继续拆分，不在入口文件追加流水账。
- 文档不得描述第二执行路线。
