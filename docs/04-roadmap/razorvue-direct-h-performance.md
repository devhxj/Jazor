# RazorVue Direct Render 性能评审

> 范围：官方 Razor Source Generator C# 到 Vue render-function `.mjs` 的当前路径。本文记录当前评审结论、已接受的 G2 优化边界与可复现验证，不把一次机器上的吞吐数字当作通用性能承诺。

## 结论

RazorVue 已不再通过 RenderTreeBuilder runtime bridge 组装 VNode。生产主线是：

```text
.razor
  -> official Razor Source Generator generated C#
  -> Roslyn IOperation / BuildRenderTree
  -> RenderEmitter Vue framing + Jazor.Compiler SemanticWalker expression lowering
  -> Vue render-function .mjs
  -> Jazor.Emit artifact / source map / bundle
```

直接 `h(...)` 产物已经避免了额外 builder 对象、协议调用和最终 `finish()` 汇总的分配/间接层；这是正确的基线。但是只使用通用 `h` 仍有三个可观察的成本来源：

1. 静态 props object 与静态 markup 在每次 render 中重新分配。
2. 可证明的动态 prop 仍会走 Vue 的通用 patch 路径，runtime 无法只检查实际变化字段。
3. render 中新建的 inline event handler 会改变函数 identity，也会制造无必要分配。

本轮 G2 已实现 Vue compiler 思路的保守子集：static hoist、block tree + patch flags、以及 setup-instance stable handler cache。它不宣称与 Vue SFC compiler 的全量 transform 等价。C# 的求值顺序、Razor slot/loop 闭包和 source-map 可追踪性优先于更激进的静态推断。

## 当前优化契约

### Static hoist

`RenderEmitter` 仅提升不可观察的模块常量：

- plain props object，所有 key 是非计算属性，所有 value 是 `null`、string、boolean、number 或 bigint；
- static markup，发射为 `createStaticVNode(...)`；
- hoist 不接受 `key`、`ref`、event listener、spread/conditional props、动态表达式或 reference capture；
- `foreach`、slot/render-fragment 等 render-local scope 也不会产生 module hoist。

`VueModuleBuilder` 将这些常量置于 Vue import 之后、setup factory 之前。因此同一组件的多次 render 与多个 setup instance 可以共享它们，且不会捕获组件 state、props 或 loop local。

`createStaticVNode(markup, 1)` 中的 `1` 继承当前单个 Razor markup frame 的 lowering 约定。Deno contract tests 覆盖了产物形状与复用身份，但任意多根 raw HTML 的 Vue DOM mount/patch cardinality 尚未在真实 Vue runtime 中单独验证；在该验证完成前，不把它扩展为一般 multi-root static-markup 优化承诺。

### Block tree 与 patch flags

patch metadata 根据最终 Vue props surface 构造：

| 形状 | 当前 flag / 行为 |
| --- | --- |
| dynamic text | 保留 `h(...)` children diff，当前不发射 `TEXT` |
| dynamic element `class` / `style` | `2` / `4` (`CLASS` / `STYLE`) |
| dynamic component prop named `class` / `style` | `8` (`PROPS`) with `dynamicProps: ["class"]` / `["style"]` |
| 已知动态 props | `8` (`PROPS`) 与稳定 dynamic-prop name array |
| spread、conditional attributes 或 dynamic `key` | `16` (`FULL_PROPS`) |
| `ref` / reference capture | `512` (`NEED_PATCH`) |
| slots | `1024` (`DYNAMIC_SLOTS`) |

element 只有在**没有即时 children**且有明确动态更新面时才发射 `openBlock(), createElementBlock(...)`。component 则只在没有普通即时 children 且有明确动态更新面时发射 `createBlock(...)`；slot object 可以获得 dynamic-slots flag。含即时 child 的 element/component 继续用 `h(...)`，因为当前 lowering 尚未为每个子节点建立完整的 `dynamicChildren` 合同，伪造空 block 会让 Vue 错过子树 diff。

`CLASS` / `STYLE` 是 Vue DOM element 的专用 patch 快路径，不能直接用于 component VNode。组件把参数映射到 runtime `class` 或 `style` 时，必须使用 `PROPS` 和精确的 `dynamicProps` 名称，否则 Vue 的 component-update gate 可能跳过子组件更新。

因此 `TEXT` 虽是 Vue runtime 的标准 flag，但不是当前 direct lowering 的已实现契约。动态文本通常正是即时 child；在 child-level metadata、动态 child 收集和 C# 求值顺序能够一起证明前，保持 `h(...)` 的普通 children diff 是有意的保守选择。

### Stable event handler cache

inline arrow/function handler 仅在以下前提下改写为 `__jazor$handlerCache[index] || (...)`：

- 不在 foreach、slot 或 render-fragment 的 non-hoistable scope；
- handler 不引用 render-local alias；
- event modifier wrapper 不缓存；
- `@bind` adapter 只在其底层 binder 本身是稳定 inline closure 时缓存。

cache array 在 setup factory 内创建，而不是 module scope。这样同一 setup instance 的 handler identity 稳定，跨组件实例仍各自保留 props/state closure，不会把第一实例的状态泄漏给后续实例。已稳定的 named member function 不需要 cache，因为 render 不会为它分配新函数。

这里的 “named” 不等于所有 `Identifier`。render prelude 的 local handler 也会以 identifier 形式出现，但每轮 render 可能重新赋值；它必须继续进入 patch flags 的 dynamic-prop 列表，不能被误当作稳定 listener 而跳过 Vue listener 更新。

## 没有回退路径

生产输入固定为 official Razor SG generated C#，最终输出固定为 Vue render-function `.mjs`。direct lowering 失败时提供带 source 位置的诊断；不回退到 runtime builder、SFC、wrapper marker 或字符串拼接的 JavaScript。

这个约束很重要：性能优化不能通过重引入第二套运行时协议来换取“可运行”。Vue framing 可直接构造 AST，但所有 C# expression/member/call/import 语义仍经 `Jazor.Compiler` / `SemanticWalker`。

## Benchmark

`scripts/csharp/benchmark-razorvue-g2.cs` 以最终 direct render-function call shape 为输入，和同进程 handwritten `h(...)` 对照，记录 render/update ops、retained heap delta 与 gzip body bytes。

```bash
dotnet run --file scripts/csharp/benchmark-razorvue-g2.cs -- --dry-run
dotnet run --file scripts/csharp/benchmark-razorvue-g2.cs -- --measure-runtime --samples 5 --iterations 10000
dotnet run --file scripts/csharp/benchmark-razorvue-g2.cs -- --measure-browser --iterations 10000
```

该 lane 不 mount DOM、不执行 Vue `patch()`、不测 hydration、layout、浏览器网络或 compiler cold/incremental 时间。它适合防止 direct artifact shape 的额外分配/体积回归，不能单独证明真实页面端到端更快。需要发布性能结论时，必须在目标浏览器、真实 Vue runtime 和代表性页面上补充 profile。

## G2 验收

1. 官方 Razor SG -> direct `.mjs` 是唯一生产路径，仓库不保留 retired runtime bridge/import token。
2. 静态 props 与 static VNode 在 module scope 只创建一次；slot/loop/render-local 不可提升。
3. 仅已证明安全的叶节点和 component shape 使用 block/patch flag；有即时 children 时保留 `h`。
4. handler 在同一 setup instance 内保持 identity，跨 setup instance 不共享。
5. `RazorSgVueCompilerOptimizationTests` 覆盖以上身份与 scope 约束；RazorVue、Emit、Compiler suites 保持通过。
6. benchmark protocol 可独立执行并明确记录其测量边界。

## 后续演进

下一步不是机械扩大发射 `createElementBlock` 的范围。只有在 direct lowering 能为即时 children 建立稳定、完整的 dynamic-child contract，并通过真实 Vue runtime regression 后，才考虑更深的 block tree。可能的候选包括静态子树分段、keyed list 的 fragment flag、以及按 C# 求值顺序保留的 child-level patch metadata。

每一项都应先证明：evaluation order、side-effect count、最终 VNode、slot/loop closure identity 与 source-map anchoring 不退化；否则继续使用 `h(...)` 是正确的保守行为。
