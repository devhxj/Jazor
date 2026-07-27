# RazorVue 代码优先 WBS

> Parent: [Jazor 架构转型开发计划](../Jazor%20架构转型开发计划.md)
> Size rule: keep under 10KB.

## 当前策略

先实现功能代码，再做性能测试。

性能相关任务只保留已有 benchmark protocol 和采样入口；在核心功能闭环前，不继续把吞吐、gzip、heap、旧线 baseline 阈值作为阻塞项。

## P0：RenderTreeBuilder surface

目标：把 official Razor SG 常见 generated-code shape 快速落地。

优先完成：

- `OpenElement` / `CloseElement`
- `OpenComponent<T>` / `OpenComponent(int, Type)` / `CloseComponent`
- `AddContent` 全常见 overload
- `AddMarkupContent` / `MarkupString`
- `AddAttribute` / `AddComponentParameter`
- `AddMultipleAttributes`
- `OpenRegion` / `CloseRegion`
- `SetKey`
- reference capture
- `RenderFragment` / `RenderFragment<T>` 支持矩阵
- `Clear` / `GetFrames` / `Dispose` / constructor 已接线 surface 的一致性测试

验收：

- 每个 accepted call shape 有 compiler emission test；
- runtime 行为测试覆盖 frame balance、root、attribute、event、component parameter；
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

## 推迟到功能闭环后

- browser heap retained；
- render/update throughput；
- gzip 阈值优化；
- retired-line baseline；
- performance ADR 阈值判定；
- 大样例性能调优。
