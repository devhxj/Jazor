# RazorVue 极致性能路线图

> 范围：official Razor Source Generator generated C# -> Roslyn `IOperation` -> RazorVue direct Vue render-function `.mjs`。本路线图把已完成的 G2 优化作为基线，记录下一轮端到端性能工作；它不是重新引入 RenderTreeBuilder、SFC 或 render-context 协议的理由。

## 目标与边界

目标是让 RazorVue 在真实 Vue 3 runtime 中接近 Vue SFC compiler 的关键优化收益，同时保持 Jazor 的 C# 语义和产物契约：

```text
Razor source
  -> official Razor SG generated C#
  -> Roslyn IOperation
  -> RenderEmitter Vue framing + Jazor.Compiler SemanticWalker lowering
  -> direct Vue render-function .mjs
  -> Jazor.Emit / browser bundle / SSR artifact
```

“极致”在这里不是只压缩一次 mock render call，而是同时优化并测量以下四个面：

| 面向 | 关注指标 | 当前已知状态 |
| --- | --- | --- |
| 浏览器 render / patch | VNode allocation、DOM patch、handler identity、keyed reorder | direct `h(...)` 已消除 builder bridge；即时 children 还不能形成 block tree |
| hydration | static VNode node cardinality、client/server VNode 一致性 | `createStaticVNode(markup, 1)` 只在当前单 frame 假设下使用，multi-root 必须先实测 |
| 交付与 SSR | release JS payload、模块解析、SSR cold/warm TTFB、并发 | package ESM 保持 external；SSR 每请求启动一个 Deno process |
| 生成与 HMR | RazorVue artifact build、增量生成、reload latency | catalog 生成当前按稳定顺序串行执行 |

以下约束不可为性能让步：

1. 不恢复 retired `render-context`、runtime builder、SFC、wrapper marker 或手拼 C# expression JavaScript 的 fallback。
2. C# expression、member、call、import 和 CLR/union 语义仍须经 `Jazor.Compiler` / `SemanticWalker`。RenderPlan 只能承载 RazorVue VNode framing metadata，不能变成第二个 C# compiler。
3. 必须保留 evaluation order、side-effect count、最终值、loop/slot closure identity、reference capture、异常传播、稳定 import/temp 命名和 source-map origin。
4. .NET 11 / C# preview 的 `IOperation` 与 native `union` 演进不得被 RazorVue 私有语法探测绑定；需要新语义时应先由 compiler lowering hook 给出稳定的已降低表达式。
5. 没有真实 runtime 证据的优化不扩大默认 lowering 范围。性能 benchmark 不能替代 correctness regression。

## 已确认结论

当前 direct `h(...)` 路径是正确的性能基线，G2 已安全实现：

- module-scope static props/static VNode hoist；
- 无即时 children 的 element/component block tree + 已证明安全的 patch flags；
- setup-instance stable event handler cache。

这不是 Vue compiler 全量等价。当前最重要的限制是 `RenderState` 主要保存 ESTree expression，而没有为每个 child 保存 VNode kind、动态更新面、fragment stability 和 hoist scope。因此有即时 children 的 element/component 有意回退为普通 `h(...)`：若错误地打开空 block，Vue 会跳过尚未被记录为 dynamic child 的更新。

本轮审计还确认了下列具体机会，按依赖而不是“看起来最容易”排序：

| 优先级 | 机会 | 根因 / 现状 | 预期收益 | 前置条件 |
| --- | --- | --- | --- | --- |
| P0 | static VNode cardinality | `createStaticVNode(markup, 1)` 对多根 static markup 的 hydration 语义尚未验证 | correctness gate；避免扩大 hoist 后的 hydration 风险 | 真实 mount/patch/hydration tests |
| P1 | child-level block tree | immediate child metadata 在 expression-only state 中丢失 | 减少稳定子树 traversal 与普通 children diff | RazorVue-only RenderPlan/VNodePlan |
| P2 | list lowering | `foreach` 目前走 `Array.from(..., mapper)` | Vue `renderList`、keyed fragment 和 block collection 快路径 | P1 的 fragment / key metadata |
| P3 | slot classification | slots 一律标为 `DYNAMIC_SLOTS` | stable slot fast path、少建 object/closure | slot scope / conditional metadata |
| P4 | event/bind leaf lowering | simple DOM bind 仍有 event-time wrapper/IIFE 成本 | 高频 input update 降低 closure 与 call 层数 | 严格证明为直接赋值 |
| P5 | parameter lifecycle watch | `OnParametersSet*` 使用 `watch(() => props, ..., { deep: true })` | 避免每次 nested traversal | 明确参数变更兼容契约 |
| P6 | CLR release payload | Netpack 保留 package ESM external，较大 CLR module 会整体被解析 | 减少 parse/transfer/eval | Jazor-owned member/family split 或安全 DCE |
| P7 | SSR process lifecycle | 每个请求写 temp JSON 并启动新的 Deno | 大幅降低 warm TTFB | generation-aware worker protocol |
| P8 | generation / asset delivery | Vue artifact 生成串行，release asset cache/preload 未形成数据驱动策略 | build/HMR 与 navigation 吞吐 | P0 benchmark matrix 和 deterministic parallel design |

## 实施里程碑

### E0: 真实基准与 static VNode 正确性

先建立 production Vue 的可复现测量，而不是根据生成字符串推断性能。

- 扩展 `scripts/csharp/benchmark-razorvue-g2.cs` 或拆出同目录单文件 C# runner；它负责启动并收集真实浏览器/SSR 测量，不能以 PowerShell wrapper 代替。
- 建立代表性 fixture：static multi-root markup、dynamic text/class/style、component prop、`@bind` input、keyed/unkeyed `@foreach`、conditional slot、nested slot、large CLR import、SSR + hydration。
- 在 Chrome/Edge 的 production Vue runtime 记录 mount、steady-state patch、keyed reorder、hydration、heap/allocation（可用时）、artifact gzip 和 parsed module bytes；SSR 分开记录 cold、warm、并发和 cancellation。
- 对 static markup 计算或显式携带真实 top-level node count；无法证明时不 hoist 为一般 `createStaticVNode`。增加 multi-root mount/patch/hydration regression。
- 基准输出必须包含机器/runtime/version/fixture/iterations，并保留 baseline 与 candidate 的原始样本；只有同环境对照才可用作 gate。

**完成门槛：** real Vue DOM 与 hydration 测试覆盖静态一根、多根、相邻文本/element/comment 情况；benchmark 同时能跑 browser、SSR 和 artifact-size lane；没有把 mock render 数字写成页面性能结论。

**状态：已完成。** `VueRawMarkup` 已使用 HTML5 fragment parser 计算顶层 node cardinality；production Vue gate 覆盖单/多根、text+element、table、SVG/MathML、leading comment、动态 `MarkupString` patch、SSR 与 hydration。动态 raw markup 已迁移为按需 `@jazor/vue-runtime/raw-markup.mjs` provider，Emit 只在 artifact 引用时保留并物化该 runtime 与 import-map 条目。

### E1: RenderPlan v1 与 child block tree

在 `RenderEmitter` 内引入 RazorVue 专用的 `RenderPlan` / `VNodePlan`，让 lowering 在最终 ESTree expression 之外保留最小必要 metadata：

- node kind（element/component/text/fragment/static/opaque expression）；
- dynamic surface（text、class、style、props、full props、need patch）；
- child / fragment stability、key presence、slot/loop/render-fragment scope；
- module-hoist eligibility 与 source origin。

plan 不能保存或重新翻译 `IOperation`；其 expression 仍来自现有 compiler lowering。先覆盖 dynamic text 和无控制流的安全 child block，再逐层扩展 element/component immediate children。只有当 `openBlock()` 收集到完整 dynamic children 时才发射 `createElementBlock` / `createBlock` 和 `TEXT`；否则保留 `h(...)`。

**完成门槛：** dynamic text、nested dynamic child、conditional child、reference capture、nested component 的 evaluation order 和 VNode 更新均有真实 runtime regression；同一输入的 module text/source map/import order 保持 deterministic。

**状态：已完成。** `RenderEmitter` 现在以 RazorVue-only `RenderPlan` / `VNodePlan` 保存已降低 ESTree expression 的 VNode 分类和更新事实，不持有或重译 `IOperation`。单一已证明为 `string` 的动态 text child 使用 `TEXT`，静态与动态文本混合时发射 `createTextVNode(..., TEXT)`，完整的静态/dynamic-text/nested-block direct-child 集合使用 `openBlock()` / `createElementBlock(...)`。conditional、slot、sequence、dynamic raw markup 与普通 component children 仍为 opaque，因此继续用 `h(...)` 的完整 children diff。artifact/Deno 回归和 production Vue DOM gate 已覆盖单文本、混合文本、嵌套 block，以及不错误提升 opaque children 的边界。

### E2: `@foreach`、fragment 和 list fast path

把已证明安全的 `foreach` 从通用 `Array.from(..., mapper)` 演进为 Vue `renderList` lowering：

- 使用 `openBlock(true)` 表达 list 内 block collection；
- 依据 Razor key/稳定性发射 keyed、stable 或 unkeyed fragment patch flag；
- 保持 source collection、key expression、iteration aliases 和 body 的 C# 求值顺序；
- 保留没有 key、dynamic key、nested loop、loop-local handler / ref / slot 的保守路径，直到 plan 可证明其 closure 与 identity。

**完成门槛：** keyed reorder 保留 DOM/component identity，unkeyed 行为不被错误标为 stable，loop-local bind/handler/slot 不越过实例或迭代捕获；真实 DOM benchmark 显示目标 list fixture 的 patch 成本改善且非目标 fixture 无显著回归。

**状态：已完成。** 只有 simple loop local、single direct element/component VNode root 且没有事件、`@bind`、ref、slot、解构或 nested loop capture 的 `foreach` 采用 `openBlock(true) + createElementBlock(Fragment, null, renderList(source, mapper), flag)`。显式 Razor `@key` 采用 `KEYED_FRAGMENT (128)`，没有 key 采用 `UNKEYED_FRAGMENT (256)`；`STABLE_FRAGMENT (64)` 没有被猜测性使用。`renderList` 接收原 collection，保留 Vue 对 array、iterable、object record 和 numeric range 的协议，避免旧 `Array.from(source ?? [])` 对 source 语义的改写。production Vue gate 已验证 keyed `c,a,b` reorder 保留原 DOM identity，且 unkeyed fragment flag 为 `256`。其他 foreach 继续保守使用旧的 `Array.from(... ?? [], mapper)` 路径。

### E3: stable slot 与精确 dynamic slots

slot object 需要区分稳定、conditional、loop-generated 和动态名字：

- 固定 slot 使用 `withCtx`、稳定 slot flag (`_: 1`)；
- conditional / loop / dynamic-name slots 用 `createSlots` 和精确 `DYNAMIC_SLOTS`；
- slot 函数的 closure 永远属于正确的 setup/render/loop scope，不作跨实例或跨 iteration hoist。

**完成门槛：** parent update、child update、conditional slot enable/disable、nested slot 和 loop slot 全部经 Vue DOM regression；稳定 slot 路径不再无条件设置 `1024`。

### E4: 高频更新与 lifecycle contract

先收紧语义，再做 leaf optimization：

- `@bind` 只对 Roslyn 已证明的 single direct assignment 省去 event-time wrapper；复杂 setter、format/parse、modifier、await 或可观察副作用继续走现有安全 lowering。
- 为 `OnParametersSet` / `OnParametersSetAsync` 定义并测试精确契约：父组件传入新 prop value/reference 时触发；是否把“同一 prop 引用的 nested mutation”视为 parameter change 必须先决定。只有契约允许时，才用 shallow / per-prop watch 替代 deep watch。
- 保留异步 parameter callback 的 serial ordering、generation stale-completion suppression 和 `StateHasChanged` 行为。

**完成门槛：** direct bind、复杂 bind、named/dynamic handler、sync/async lifecycle、nested object prop、rapid prop updates 均有 regression；优化不靠改变 Razor-visible callback 时机来获得数字。

### E5: release payload、SSR 与 build/HMR 吞吐

这些工作与 RenderPlan 独立，但决定真实首屏和服务器成本。

- 对 Jazor-owned CLR runtime 实施 member/family splitting，或在不改变 runtime module semantics 的前提下实现可证明安全的 production DCE；不能把 Netpack 的非 lossless printer 风险隐藏为“tree shaking”。
- 将 SSR 改为有界、generation-aware 的 persistent Deno worker pool：stdin/stdout request protocol、artifact generation invalidation、worker crash recovery、cancellation、bounded concurrency 与 graceful disposal。每次 request 不再落 temp JSON/起新 Deno。
- 在 profiling 证明瓶颈后，保持 catalog 顺序确定性的前提下评估有界并行 RazorVue artifact construction；release asset cache/preload 只为已测量的 critical graph 引入。

**完成门槛：** release bundle 保持可运行且 source-map/manifest/import rewrite 正确；未使用 CLR surface 不增加首屏解析负担；SSR warm request 复用 worker，reload 后不使用旧 artifact，取消/故障不泄漏 process；并行生成产物 byte-for-byte deterministic。

## 通用验证与发布门禁

每一个里程碑在进入下一步前都必须满足：

1. 对应 RazorVue SG regression、compiler regression、Emit regression 通过；涉及 SSR 时增加 ASP.NET Core integration coverage。
2. 新的 Vue helper import、patch flag、fragment flag、hoist 或 cache 都有 AST/artifact shape assertion 和真实 Vue behavior assertion，两者缺一不可。
3. 对比 benchmark 使用相同 fixture、Vue build、browser/runtime、iteration/warm-up 规则；报告 median、p95 和 variance，而非单次最优值。
4. 对静态、动态、slot、loop、exception 与 cancellation 路径检查 source map 与 artifact deterministic 输出。
5. 先跑 focused suite，再在交付切片完成后运行 `dotnet run --file scripts/csharp/test-dotnet.cs`；性能提升不能通过跳过测试、全局 mutable state 或 `[DoNotParallelize]` 获得。

当前基线与已完成 G2 的细节在 [RazorVue Direct Render 性能评审](./razorvue-direct-h-performance.md)。本文件是后续实现顺序和验收契约；任何完成的阶段应将稳定结论收敛回 [当前状态](./current-status.md)，详细演进记录归档到 `docs/05-history/evolution.md`。
