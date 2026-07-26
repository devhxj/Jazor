# Jazor 工作流总览

> Status: active plan
> Updated: 2026-07-27
> Positioning: 仓库级恢复入口，用于查看当前转型主线、依赖顺序与并行策略。

当前唯一 Razor-to-Vue 主线是：

```text
official Razor SG generated C#
    -> Roslyn IOperation
    -> Jazor.Compiler / SemanticWalker
    -> Vue render-function .mjs
```

Jazor Component Runtime、旧 RazorVue SFC/library-mode、Jolt 和 CSX 计划只保留历史或旁路参考价值，不覆盖 `docs/02-计划/Jazor 架构转型开发计划.md`。

## 快速导航

| 工作流 | 当前阶段 | 下一步行动 | 状态/计划 |
|--------|---------|-----------|-----------|
| Razor-to-Vue 架构转型 | G0 已通过，Phase 0 清理已验证；Task 1.1/1.2 已完成；Task 1.3 compiler closure/rewrite、SG bridge seam、最小内存 `.mjs` framing、dynamic dispatch/source-origin diagnostic 与 Node runtime unit 切片已通过；Task 1.4 carrier/Emit/runtime asset/canonical manifest、`.mjs.map` 表达式级首切片、stale component cleanup、materialization repeat-write determinism 与 Counter 外部 consumer clean-build artifact hash 覆盖已通过；Task 1.5 真实 browser Counter 首切片已通过；Task 2.2 已补 `RenderFragment<T>` component slot fail-fast diagnostic 首切片；Task 2.5 benchmark protocol、runtime partial measurement、三 fixture official SG generated artifact size/hash + handwritten baseline 首切片与 active-line incremental p95 已加入；已完成多轮单一路径生成体积优化 | 继续补 G2 render semantics、多 fixture determinism、browser heap/retired baseline 与阈值判定 | [WBS](./Jazor%20架构转型开发计划.md) |
| Razor SG final-document G0 | 已接受 | 保持 adapter/binder tests 和 SDK compatibility evidence | [ADR](./RazorSgFinalDocument.G0.DecisionRecord.md) |
| Compiler | 主线依赖 | 已承接 RenderTreeBuilder render-context v1、current-component closure/rewrite v1、state default initializer、EventCallback factory method-group lowering 与动态 dispatch/source-origin diagnostic；下一步补 G2 render surface | [原则](../../src/Jazor.Compiler/ImplementationPrinciples.md) |
| Emit | Task 1.4/1.5 首切片完成 | 已读取 VueRenderCatalog、物化 RazorVue runtime assets、写 `.mjs.map` 与 canonical schema-v1 manifest，通过真实 browser Counter smoke，接上 RazorVue 表达式级 source-map 首切片，并补齐 Vue render component stale cleanup、repeat-write determinism 与 Counter 外部 consumer clean-build determinism 覆盖；下一步补多 fixture determinism / performance evidence | [状态](../03-完成/emit/status.md) |
| ECMAScript.Vue3 / Vuetify | 外部库绑定层 | 保持 host binding/API surface，不把 SFC/toolchain 协议塞回 Vue binding | [Vue3](../01-目标/ecmascript.vue3/README.md) |
| Jolt | 历史退役 | 仅通过 Git 基线维护和比较，不进入当前项目图 | [历史](../01-目标/jolt/README.md) |
| Jazor Component Runtime | 历史探索 | 不作为当前转型执行主线 | [历史计划](./jazor-component-runtime-plan-2026-07-06.md) |

## 依赖顺序

1. Phase 0: SG-result hook G0、ADR、旧入口清理、full build/focused tests。
2. Phase 1: render-context v1、RenderTreeBuilder Compile hooks、minimal component `.mjs` artifact。
3. Phase 2: render semantics、state、lifecycle、determinism、source-map/performance gates。
4. Phase 3: Deno production toolchain 和 `DynamicVueComponent<TProps>` 单向 SFC interop。
5. Phase 4: dev server/HMR；Netpack 仅 experimental。
6. Phase 5: TodoList sample、NuGet/package consumer、platform matrix、release docs。

## 并行策略

- 文档治理、遗留扫描、focused tests 和只读 code review 可以与主线实现并行。
- Compiler Task 1.2 已完成，Task 1.3 closure/rewrite、SG bridge seam、最小内存 `.mjs` framing、dynamic dispatch/source-origin diagnostic 与 Node runtime unit 子切片已通过 focused tests；Task 1.4 carrier/Emit/runtime asset/canonical manifest/表达式级 `.mjs.map` 首切片、stale cleanup、materialization repeat-write determinism 与 Counter 外部 consumer clean-build artifact hash 覆盖已通过 focused tests；Task 1.5 真实 browser Counter 首切片已通过；Task 2.5 benchmark protocol、runtime partial measurement、三 fixture official SG generated artifact size/hash + handwritten baseline 与 active-line incremental p95 首切片已加入，并已完成按需 lifecycle/invalidation/reactive/setup 参数/scope return 体积优化。下一主线缺口是 G2 render semantics、多 fixture determinism、browser heap/retired baseline 与阈值判定。
- Toolchain/Deno/Netpack、DynamicVueComponent 和 TodoList sample 依赖 Phase 1/2 的 artifact and manifest contract，不应提前实现伪协议。
- 历史 Jolt、Razor IR、Razor-to-SFC 和 Component Runtime 文件不能作为当前生产 fallback。

## Gates

| Gate | 当前判断 |
|------|----------|
| G0 | official Razor SG final document + hook compilation derivation 已通过；Task 0.5 清理已完成并通过 clean build / focused tests 验证 |
| G1 | 真实 `.razor -> .mjs -> Deno bundle -> browser` Counter 首切片已通过；不能用手工 fixture 替代真实 browser regression |
| G2 | source-map、Counter determinism、benchmark protocol、runtime partial measurement、三 fixture official SG generated artifact size/hash 与 active-line incremental p95 首切片已接通；最新 generated artifact sample：plain-text 628/333、counter 811/387、keyed-list-100 2348/672 component bytes/gzip；Task 2.1 已扩展 OpenRegion/静态 markup/OpenComponent/RenderFragment/AddMultipleAttributes；Task 2.2 props 声明 + EventCallback.Invoke→`props.onX?.()` 首切片、EventCallback 参数 `emits: [...]` metadata 首切片、DOM multiple-attribute event normalization / component parameter name preservation、DOM `@bind` v1（`EventCallback.Factory.CreateBinder` 简单 state assignment + `SetUpdatesAttributeName` hint）、component `@bind-X` 常见 `X`/`XChanged` 首切片、default/named 非泛型 `RenderFragment` slot 首切片、`RenderFragment<T>` component slot fail-fast diagnostic 首切片、父子 component parameter lower-camel bridge 与 sibling component import rebasing 已接线；Razor-side unknown/required/type mismatch 由 Razor/C# 编译链路负责，lowering 不重复校验；Task 2.3 `onInitialized`+`StateHasChanged`+同步 `OnParametersSet` + 同步 `OnAfterRender` + 同步 dispose→`onUnmounted` + 同步 `ShouldRender` + `OnInitializedAsync` + `OnParametersSetAsync` + `OnAfterRenderAsync` 已接线；仍需要 typed slot descriptor/lowering matrix、component bind descriptor breadth/full emits catalog、browser heap/retired baseline 与阈值判定 |
| G3 | Deno production build + mixed `.mjs + .vue` browser tests |
| G4 | Deno dev/HMR protocol/browser tests；Netpack 不阻塞 |
| G5 | package consumer、TodoList、platform matrix 和 full regression |

## 维护规则

- 当前执行状态以 `Jazor 架构转型开发计划.md` 为准。
- 修改主线边界时同步更新本 dashboard、`docs/README.md` 和相关 project README。
- 旧计划若没有迁移到当前 WBS，应移动、删除或标成历史，不得留在活跃索引中。
