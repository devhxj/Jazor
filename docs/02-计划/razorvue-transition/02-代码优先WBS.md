# RazorVue 代码优先 WBS

> Parent: [Jazor 架构转型开发计划](../Jazor%20架构转型开发计划.md)
> Size rule: keep under 10KB.

## 当前策略

功能代码和首份 G2 性能 baseline 已经闭环，当前 WBS 转入 traditional Vue direct VNode emitter。

direct emitter 仍然走 official Razor SG generated C# -> Roslyn `IOperation` -> `Jazor.Compiler` / `SemanticWalker`。supported `BuildRenderTree` 调用直接降成 Vue `h(...)` / `Fragment` / 后续 `createStaticVNode`、`mergeProps` 和 slot function code。render-context v1 只作为 oracle/过渡层，不是长期 production frame stack。

不得为 direct emitter 引入第二 artifact contract、SFC 输出、目录扫描或工具链 fallback。

## P0-D：Direct VNode emitter

目标：把最常见线性 RenderTreeBuilder 元素输出从 runtime frame replay 降成 setup-scoped Vue render function。

优先完成：

- `OpenElement` / `CloseElement` -> `h(tag, props, children)`；
- `AddAttribute` DOM attribute/event name normalization；
- `AddContent` child expression 保持 compiler source-map projection；
- `ShouldRender` cached VNode gate 调用 direct render；
- render-context oracle parity fixture；
- direct lane benchmark 标记与报告字段。

验收：

- simple generated component `.mjs` 不含 `createRenderContext`、`scope.buildRenderTree(builder)`、`builder.finish()`；
- setup factory 内 direct render function 闭包住 props/state/member handler；
- source-map 仍能链回 Razor source mappings；
- focused `RazorSgComponentMemberClosureTests` 通过。

## P1-D：Direct surface expansion

目标：把已有 accepted render-context surface 分批迁移到 direct VNode lowering。

优先完成：

- `OpenComponent<T>` / `OpenComponent(int, Type)` / `CloseComponent`；（静态组件类型已接线）
- `AddComponentParameter`；（普通参数、EventCallback/bind 参数、descriptor name-map 已接线）
- `AddMultipleAttributes` / `SetAttributeValue`；（已接线，静态可解析 bulk attrs 直接生成 props）
- `AddMarkupContent` / `MarkupString` -> `createStaticVNode` 或等价 direct static VNode；（简单 markup 已接线）
- `OpenRegion` / `CloseRegion` -> `Fragment`；（已接线）
- `SetKey`；（已接线）
- reference capture；（element/component ref callback 已接线）
- `RenderFragment` / `RenderFragment<T>` slot function lowering；（非泛型 named/default slot 与 typed scoped slot 已接线）
- event prevent/stop、named event、bind metadata direct normalization；（DOM bind metadata、prevent/stop 与 named event metadata 已接线）
- `Clear` / `GetFrames` / `Dispose` / constructor 已接线 surface 的 oracle 对照。

验收：

- 每个 migrated call shape 有 generated-module emission test；
- runtime 行为测试覆盖 direct 与 oracle 等价的 root、attribute、event、component parameter；
- unsupported overload 明确 diagnostic。

## P1：Component contract

目标：让父子组件、Vuetify wrapper 和手写 SFC interop 需要的 component boundary 可用。

优先完成：

- `[Parameter]` -> Vue `props`;
- `EventCallback` / `EventCallback<T>` -> Vue listener / emits metadata;
- DOM event 与 component emit 分离规范化；
- component parameter lower-camel / descriptor name-map；
- default / named non-generic `RenderFragment` slot；
- `RenderFragment<T>` scoped slot 基础传输；
- typed slot descriptor 支持矩阵；
- `@bind` DOM 与 component 常见 generated shape；
- sibling generated component import rebasing。

验收：

- parent/child focused tests；
- generated child props/emits catalog 不被 closure 裁剪；
- unknown/required/type mismatch 交给 Razor/C# 编译链路，不在 lowering 重复校验。

## P2：State 与 lifecycle

目标：闭合可用组件行为，而不是复制完整 Blazor renderer。

优先完成：

- field/property initializer；
- `OnInitialized` / `OnInitializedAsync`;
- `OnParametersSet` / `OnParametersSetAsync`;
- `OnAfterRender` / `OnAfterRenderAsync`;
- `StateHasChanged`;
- `ShouldRender`;
- `IDisposable` / `IAsyncDisposable`;
- dispose 后 event/lifecycle 调用诊断规则。

验收：

- focused lifecycle ordering tests；
- browser fixture 覆盖 mount、prop update、event update、unmount；
- async completion 的 invalidation 次数有明确规则。

## P3：Toolchain smoke

目标：功能闭环可在真实 consumer 里跑起来。

优先完成：

- Deno production build smoke；
- Netpack production build smoke；
- mixed `.mjs + .vue` manifest 消费；
- Vuetify import smoke；
- source-map smoke；
- package consumer clean build。

验收：

- Deno 与 Netpack 使用同一 manifest/request/result contract；
- 显式选择哪个工具链就只执行哪个；
- 不扫描目录猜入口，不改写 compiler-owned `.mjs`。

## P4：G2 性能 baseline

- browser heap retained；
- render/update throughput；
- gzip 阈值优化；
- retired-line baseline；
- performance ADR 阈值判定；
- 大样例性能调优。

当前先执行前三类采样和 old-line baseline availability probe。若 report 出现阈值 warn，再按 fixture 和指标拆后续优化任务。
