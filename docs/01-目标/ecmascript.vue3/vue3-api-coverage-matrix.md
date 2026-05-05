# ECMAScript.Vue3 API 覆盖矩阵

> Status: active inventory
> Updated: 2026-05-03
> Source baseline: Vue 官方 API 索引与分组，目标对象是 `src/ECMAScript.Vue3/Vue3.cs` 的 host binding / authoring surface，而不是 `.vue` SFC 或模板编译器。

## 1. 结论

当前 `ECMAScript.Vue3` 不是 Vue3 全功能映射。

它已经覆盖了 Jazor render-authoring 所需的核心路径：

- component definition 的 `defineComponent(...)` 基础形态；
- `h(...)` render function authoring；
- props / object / slot / directive / plugin 的 plain object authoring；
- `createApp(...)` / `createSSRApp(...)` 的基础应用入口；
- reactivity / lifecycle 的常用最小子集。

但它没有完整覆盖 Vue 官方 API 的以下大块：

- Application config、`app.mixin(...)`、`app.runWithContext(...)`、`version`；
- async component、render function helper、built-in component/directive/helper；
- Composition API 的 helpers、utilities、advanced reactivity、dependency injection、完整 lifecycle；
- Options API 的更复杂 this-bound 长尾等；
- SFC、template syntax、compiler macros、SSR renderer、custom renderer。

因此后续目标不应该直接喊“全功能映射”，而应改成：

1. 完整盘点 Vue 官方 API 面。
2. 把能通过基础 host binding 表达的 runtime API 补齐。
3. 把需要 C# authoring 设计的 API 单独建 surface。
4. 把不属于 `ECMAScript.Vue3` 的能力明确移出目标。

## 2. 覆盖状态定义

| 状态 | 含义 |
|------|------|
| Covered | 当前 surface 已有对应 C# API，且映射规则在 `vue3-mapping-details.md` 中可解释 |
| Partial | 当前有一部分 API 或 authoring 形态，但未覆盖官方完整能力 |
| Target Gap | 应纳入 `ECMAScript.Vue3`，可以通过普通 binding / record / overload / delegate 补齐 |
| Design Gap | 应纳入目标，但需要先设计 C# authoring surface，不能机械绑定 |
| Separate Workstream | 属于 Jolt、RazorVue、SFC、SSR、custom renderer 等独立工作流，不应塞进 `Vue3.cs` |
| Non-goal | 不适合作为 Jazor 当前目标，或会破坏 C# 到 JS 的边界 |

## 3. 官方分类覆盖概览

| Vue 官方分类 | 当前覆盖 | 目标判断 | 说明 |
|--------------|----------|----------|------|
| Application API | Covered | Target Gap | app 创建、mount、use、component、directive、provide、runWithContext、version、config、mixin 已覆盖；`app.mixin` 是低层兼容 binding，不作为推荐 authoring path |
| General API | Partial | Target Gap / Design Gap | `nextTick` promise/callback、`defineComponent`、`defineAsyncComponent` loader/options 核心路径已有；剩余主要是 Options API 长尾语义（如 provide/inject 复杂形态） |
| Composition API: setup | Partial | Design Gap | setup function 已通过 component options 表达；`defineProps` 等 SFC macro 不属于当前 surface |
| Composition API: Helpers | Partial | Target Gap / Design Gap | `useAttrs`、`useSlots` 已覆盖基础 bag 与 typed projection；`useTemplateRef`、`useId` 已覆盖；`useModel` 已覆盖 typed ref、get/set options、modifiers projection，以及 `VueModelName<TProps,TValue>` + `ModelName/ModelPropName/ModelUpdateEventName` named-model contract；更高层 v-model 约定仍保持显式 authoring |
| Composition API: Reactivity Core | Covered | Target Gap | 常用核心、writable computed、watch handle、watch options、debugger event options、reactive object source 与同类 multi-source watch 已覆盖 |
| Composition API: Reactivity Utilities | Covered | Target Gap | `isRef`、`unref`、`toRef`、`toValue`、`toRefs`、proxy/reactivity predicates 已覆盖 |
| Composition API: Reactivity Advanced | Covered | Target Gap | `shallowRef`、`customRef`、`triggerRef`、`shallowReactive`、`markRaw`、`effectScope` 等核心 advanced API 已覆盖 |
| Composition API: Lifecycle Hooks | Covered | Target Gap | mounted/updated/unmounted、before*、errorCaptured、render debug、activated/deactivated、serverPrefetch 已覆盖 |
| Composition API: Dependency Injection | Covered | Target Gap / Design Gap | composition-level string key 与 typed `VueInjectionKey<T>` provide/inject 已覆盖；Options API inject object-form source/default/factory helper、object/function-form provide、symbol-key object authoring 已覆盖；更复杂 this-bound 长尾另行设计 |
| Options API | Partial | Target Gap / Design Gap | component option 基础、显式 array/object-form `props` / `emits`、`BindThis<TThis,...>` this-bound `data`/`computed`/`methods`/`watch`/lifecycle、`inheritAttrs`、`expose`、object-form `provide`、function-form `provide`、array/object-form `inject`、inject object-form helper surface、低层 `mixins`/`extends` binding 已覆盖；更复杂 this-bound 长尾仍需设计 |
| Built-in Directives | Gap | Separate Workstream | 主要属于 template/SFC 语法，不应映射成 `Vue3.cs` 普通方法 |
| Built-in Components | Partial | Design Gap / Separate Workstream | `Transition` / `TransitionGroup` / `KeepAlive` / `Teleport` / `Suspense` render-function binding 已覆盖；template-only 体验另行设计 |
| Built-in Special Elements | Gap | Separate Workstream | `<component>`、`<slot>`、`<template>` 属于 template/SFC 语义 |
| Built-in Special Attributes | Gap | Separate Workstream | `key`、`ref`、`is` 可通过 props object 部分表达，但完整语义属于 renderer/template authoring |
| SFC CSS Features | Gap | Separate Workstream | 属于 `.vue` / Jolt / RazorVue 处理，不属于 `ECMAScript.Vue3` |
| SFC Script Setup / Compiler Macros | Gap | Separate Workstream | compile-time macro，不适合硬塞进 C# host binding |
| Advanced: Custom Elements | Covered | Target Gap | `defineCustomElement`（一参/二参）、`VueCustomElementComponentOptions*` merged options、`useHost`/`useHost<T>()`、`useShadowRoot` 已覆盖 |
| Advanced: SSR | Partial | Separate Workstream | 只有 `createSSRApp`；SSR renderer / hydration / manifest 属于 emit/Jolt/SSR 工作流 |
| Advanced: Custom Renderer | Gap | Separate Workstream | host renderer 合同过大，应单独设计 |

## 4. Application API

Source: <https://vuejs.org/api/application.html>

| API | 当前状态 | 目标 |
|-----|----------|------|
| `createApp()` | Covered | 保持 |
| `createSSRApp()` | Covered | 保持；SSR renderer 另见独立工作流 |
| `app.mount()` | Covered | 保持 |
| `app.unmount()` | Covered | 保持 |
| `app.onUnmount()` | Covered | `VueApp.OnUnmount(Action callback)` |
| `app.component()` | Covered | 保持 |
| `app.directive()` | Covered | 保持 object-form / function shorthand |
| `app.use()` | Covered | 保持 object-form / function-form / typed options |
| `app.mixin()` | Covered | `VueApp.Mixin(VueComponentDefinition)`；官方不推荐应用代码使用全局 mixin，仅作为低层兼容 binding |
| `app.provide()` | Covered | 保持 |
| `app.runWithContext()` | Covered | `VueApp.RunWithContext<TResult>(Func<TResult>)` |
| `app.version` | Covered | `VueApp.Version` |
| `app.config` | Covered | `VueAppConfig` typed surface，覆盖 error/warn handler、runtime compiler options、globalProperties、optionMergeStrategies、idPrefix 等基础路径 |

## 5. General API

Source: <https://vuejs.org/api/general.html>

| API | 当前状态 | 目标 |
|-----|----------|------|
| `version` | Covered | `Vue3.Version` |
| `nextTick()` | Covered | `NextTick()` / `NextTick(Action)` 覆盖 promise 与 callback 形态 |
| `defineComponent()` | Partial | 当前覆盖 setup/render 基础 record；缺 Options API 完整面 |
| `defineAsyncComponent()` | Covered | `VueAsyncComponentLoader` / `VueAsyncComponentOptions`，含 typed `VueAsyncComponentOptions<TComponent>` |

`defineComponent` 不应该继续靠增加零散 overload 追逐所有 Options API。更合理的路径是先设计完整 `VueComponentOptions` 分层，再由 record lowering 输出 Vue options object。

## 6. Composition API: setup

Source: <https://vuejs.org/api/composition-api-setup.html>

| 官方能力 | 当前状态 | 目标 |
|----------|----------|------|
| `setup(props, context)` | Covered | 通过 `VueTypedSetupCallback<TProps>` / `VueSetupContext` 表达 |
| `context.attrs` | Covered | `VueAttributeBag` 已有 indexer / class / style / id / title / for / name / type / placeholder / disabled / readonly / required / tabindex / role 读取面；`UseAttrs<T>()` 可投影到 `VueAttributeListeners*` 以读取 callable listener key |
| `context.slots` | Covered | `VueSlotBag` 已有 indexer / default slot 读取面；typed slots 由 `VueSetupContext<TSlots>` 表达；`UseSlots<T>()` 可投影到 `VueScopedSlots<TScope>` 读取 scoped slots |
| `context.emit` | Covered | 已有 0-4 payload overload；不使用 `params object[]`，超过 4 个 payload 时按实际需求继续加 overload |
| `context.expose` | Covered | 保持 |
| `<script setup>` macros | Separate Workstream | `defineProps`、`defineEmits` 等是 SFC compiler macro，不属于 `Vue3.cs` |

## 7. Composition API: Helpers

Source: <https://vuejs.org/api/composition-api-helpers.html>

| API | 当前状态 | 目标 |
|-----|----------|------|
| `useAttrs()` | Covered | `UseAttrs()` 返回 `VueAttributeBag`；`UseAttrs<TAttrs>()` 返回 typed projection（含 `VueAttributeListeners*` callable bridge） |
| `useSlots()` | Covered | `UseSlots()` 返回 `VueSlotBag`；`UseSlots<TSlots>()` 返回 typed projection（含 `VueScopedSlots<TScope>` scoped helper） |
| `useTemplateRef()` | Covered | `UseTemplateRef<TElement>(string key)` 返回 `VueReadonlyRef<TElement?>` |
| `useId()` | Covered | `UseId()` 返回 string |
| `useModel()` | Partial | `UseModel<TValue>(props,key[,options])` + `UseModel<TProps,TValue>(props,model[,options])` 覆盖 typed model ref、get/set transform、modifiers projection（`VueModelRef<TValue>.GetModifiers()` / `GetModifiers<TModifiers>()`）与 named-model helper contract（`VueModelName<TProps,TValue>`、`ModelName`、`ModelPropName`、`ModelUpdateEventName`）；`VueSetupContext.Emit(model, value)` 覆盖 typed `update:*` emit helper；调用者仍需显式声明 prop 与 emits |

`useAttrs` / `useSlots` 与 `setup(context)` 的 read-side bags 共享类型，避免创建另一套不一致的读取面。

## 8. Composition API: Reactivity Core

Source: <https://vuejs.org/api/reactivity-core.html>

| API | 当前状态 | 目标 |
|-----|----------|------|
| `ref()` | Covered | 保持 `IVueRef<T>` |
| `computed()` | Covered | getter + writable computed options |
| `reactive()` | Covered | 保持 |
| `readonly()` | Covered | 保持 |
| `watchEffect()` | Covered | `VueWatchEffectOptions`、cleanup callback、pause/resume/stop handle、debugger event options |
| `watchPostEffect()` | Covered | `Vue3.WatchPostEffect(Action)` |
| `watchSyncEffect()` | Covered | `Vue3.WatchSyncEffect(Action)` |
| `watch()` | Covered | getter/ref/readonly-ref/reactive object source、options、cleanup callback、debugger event options、同类 ref/readonly-ref/getter source array 已覆盖 |

建议先补低风险 binding：

- `WatchPostEffect(Action effect)`
- `WatchSyncEffect(Action effect)`
- `Watch<T>(IVueRef<T> source, Action<T,T> callback)`
- `Watch<T>(Func<T> source, Action<T,T> callback, VueWatchOptions options)`
- `Watch<T>(..., VueWatchCleanupCallback<T> callback, ...)`

异构 multi-source watch 不引入 `object` 或不可靠 union array；推荐用 getter source 返回 typed record / tuple projection，继续走现有 `Watch<T>(Func<T>, ...)`。

## 9. Composition API: Reactivity Utilities

Source: <https://vuejs.org/api/reactivity-utilities.html>

| API | 当前状态 | 目标 |
|-----|----------|------|
| `isRef()` | Covered | `bool IsRef<T>(T value)`，用泛型承接 unknown-like 输入，避免 public `object` |
| `unref()` | Covered | `Unref<T>(T)` + `Unref<T>(IVueRef<T>)` overload |
| `toRef()` | Covered | value/ref/getter normalization overload、object property overload、default value overload、dictionary key overload |
| `toValue()` | Covered | getter/ref/value normalization 通过 overload 表达，不需要 `Either` |
| `toRefs()` | Covered | `VueRefs<TSource>` indexer bag + user-declared `VueRefs<TSource>` typed projection；缺失属性使用 `toRef(source,key)` |
| `isProxy()` | Covered | `bool IsProxy<T>(T value)` |
| `isReactive()` | Covered | `bool IsReactive<T>(T value)` |
| `isReadonly()` | Covered | `bool IsReadonly<T>(T value)` |

这里最适合利用未来 C# union。当前版本可以用 overload 优先，`Either` 只用于无法 overload 的 object member / normalization boundary。

## 10. Composition API: Reactivity Advanced

Source: <https://vuejs.org/api/reactivity-advanced.html>

| API | 当前状态 | 目标 |
|-----|----------|------|
| `shallowRef()` | Covered | 保持 |
| `triggerRef()` | Covered | `TriggerRef<T>(IVueRef<T>)` |
| `customRef()` | Covered | `VueCustomRefFactory<T>` + `VueCustomRefHandlers<T>` |
| `shallowReactive()` | Covered | `ShallowReactive<T>(T)` |
| `shallowReadonly()` | Covered | `ShallowReadonly<T>(T)` |
| `toRaw()` | Covered | `ToRaw<T>(T)` |
| `markRaw()` | Covered | `MarkRaw<T>(T)` |
| `effectScope()` | Covered | `VueEffectScope` + `Run` / `Stop` |
| `getCurrentScope()` | Covered | 返回 `VueEffectScope?` |
| `onScopeDispose()` | Covered | `OnScopeDispose(Action)` + `OnScopeDispose(Action, bool)` |

## 11. Composition API: Lifecycle Hooks

Source: <https://vuejs.org/api/composition-api-lifecycle.html>

| API | 当前状态 | 目标 |
|-----|----------|------|
| `onMounted()` | Covered | 保持 |
| `onUpdated()` | Covered | 保持 |
| `onUnmounted()` | Covered | 保持 |
| `onBeforeMount()` | Covered | `OnBeforeMount(Action)` |
| `onBeforeUpdate()` | Covered | `OnBeforeUpdate(Action)` |
| `onBeforeUnmount()` | Covered | `OnBeforeUnmount(Action)` |
| `onErrorCaptured()` | Covered | `OnErrorCaptured(VueErrorCapturedHandler)` / `OnErrorCaptured(VueErrorCapturedCallback)` |
| `onRenderTracked()` | Covered | `OnRenderTracked(VueDebuggerCallback)` |
| `onRenderTriggered()` | Covered | `OnRenderTriggered(VueDebuggerCallback)` |
| `onActivated()` | Covered | `OnActivated(Action)` |
| `onDeactivated()` | Covered | `OnDeactivated(Action)` |
| `onServerPrefetch()` | Covered | `OnServerPrefetch(VueServerPrefetchPromiseCallback)` / `OnServerPrefetch(VueServerPrefetchCallback)` |

低风险 hooks 可以直接补；带 event object 或 async 返回的 hooks 先设计 typed delegate。

## 12. Composition API: Dependency Injection

Source: <https://vuejs.org/api/composition-api-dependency-injection.html>

| API | 当前状态 | 目标 |
|-----|----------|------|
| `provide()` | Covered | composition-level `Provide<T>(string key, T value)` |
| `inject()` | Covered | string key、typed `VueInjectionKey<T>`、missing/default/factory default 已覆盖 |
| `hasInjectionContext()` | Covered | `HasInjectionContext()` |

`app.Provide(...)` 已覆盖 application-level provide，不等于 composition-level `provide(...)`。

## 13. Options API

Sources:

- <https://vuejs.org/api/options-state.html>
- <https://vuejs.org/api/options-rendering.html>
- <https://vuejs.org/api/options-lifecycle.html>
- <https://vuejs.org/api/options-composition.html>
- <https://vuejs.org/api/options-misc.html>

| 分类 | API | 当前状态 | 目标 |
|------|-----|----------|------|
| State | `data` | Covered | `VueDataCallback` + `BindThis<TThis>(VueThisDataCallback<TThis>)` 已覆盖无 `this` 与 this-bound `data(vm)` authoring |
| State | `props` | Covered | `Props` 覆盖 array-form 与 object-form；object-form 继续通过 `VuePropOptions<T>` / `VuePropRegistry<T>` / 自定义 `VueProps` record 表达 validators/defaults；typed generic 只提供 C# authoring contract，不自动生成 runtime declaration |
| State | `computed` | Covered | `VueComputedRegistry<T>` / writable options / custom `VueProps` record 已覆盖；this-bound getter/setter 通过 `BindThis<TThis,...>(VueThisFunc<...>/VueThisAction<...>)` 覆盖 |
| State | `methods` | Covered | `VueMethodRegistry<TDelegate>` / custom `VueProps` record 已覆盖；this-bound method delegates 通过 `BindThis<TThis,...>(VueThisAction<...>/VueThisFunc<...>)` 覆盖 |
| State | `watch` | Covered | `VueWatchRegistry<T>` + `VueWatchEntry<T>` / `VueWatchEntries<T>` 覆盖基础与数组声明；this-bound callback / cleanup callback 通过 `BindThis<TThis,...>` 覆盖 |
| State | `emits` | Covered | `Emits` 覆盖 array-form 与 object-form；object-form 继续通过 `VueEmitRegistry` / `VueEmitRegistry<T0...T3>` 表达 validators；超过 4 payload 再按需求加 overload |
| State | `expose` | Covered | `VueComponentDefinition.Expose` |
| Rendering | `template` | Separate Workstream | template compiler 不属于 `Vue3.cs` |
| Rendering | `render` | Covered | 当前 `VueRenderCallback` |
| Rendering | `compilerOptions` | Separate Workstream | 组件/SFC compiler 配置属于 build/Jolt/SFC pipeline；`app.config.compilerOptions` 已由 Application API surface 覆盖 |
| Lifecycle | created/mounted/updated 等 | Covered | lifecycle callback surface 已覆盖；this-bound lifecycle callback 通过 `BindThis<TThis,...>` 覆盖 |
| Composition | `provide` / `inject` | Partial | `Provide = VueProps` object form、`ProvideFactory = VueDataCallback` function form、`Inject = string[] / VueProps` array/object form、`VueInjectOptions<T>` / `VueInjectEntry<T>` / `VueInjectRegistry<T>` helper 已覆盖；object-form `provide` 与 inject source 已支持 string / `Symbol` / typed `VueInjectionKey<T>` authoring；更复杂 this-bound 形态仍需设计 |
| Composition | `mixins` / `extends` | Covered | 低层兼容 binding 已覆盖为 `VueComponentDefinition[]` / `VueComponentDefinition`；不作为新代码推荐复用模型 |
| Misc | `name` | Covered | 当前 `Name` |
| Misc | `inheritAttrs` | Covered | `VueComponentDefinition.InheritAttrs` |
| Misc | `components` | Covered | registry |
| Misc | `directives` | Covered | registry |

建议：

- 不把完整 Options API 作为当前第一优先级。
- 先补不需要 compiler 的 option members：`InheritAttrs`、`Expose`、Options API lifecycle callback、provide/inject 基础声明式形态、computed/methods/watch registry、watch array declarations、this-bound callback bridge（`BindThis<TThis,...>`，由 `ECMAScriptInline` 降级）已落地。
- 更细粒度 this contract 仍需按真实需求继续收敛。

## 14. Built-ins

Sources:

- <https://vuejs.org/api/built-in-directives.html>
- <https://vuejs.org/api/built-in-components.html>
- <https://vuejs.org/api/built-in-special-attributes.html>
- <https://vuejs.org/api/built-in-special-elements.html>

| 分类 | 当前状态 | 目标 |
|------|----------|------|
| Built-in Directives | Gap | Template/SFC workstream；render function 下部分可通过 props / helper 表达 |
| Built-in Components | Partial | `Transition`、`TransitionGroup`、`KeepAlive`、`Teleport`、`Suspense` 已作为 `IVueComponent` binding 落地 |
| Special Attributes | Partial | `key`、`ref`、`is` 可通过 props key 表达；强类型 convenience 需单独设计 |
| Special Elements | Gap | `<component>`、`<slot>`、`<template>` 属于 template/SFC semantics |

推荐先把 built-in components 作为普通 component binding 设计，不要把模板指令语义塞进 compiler。

## 15. Render Function API

Source: <https://vuejs.org/api/render-function.html>

| API | 当前状态 | 目标 |
|-----|----------|------|
| `h()` | Covered | 主路径已覆盖；`VueObject` 已覆盖 string `is` / `key` / named `ref` / class / style / events / attrs / dataset / raw，并提供一组高频原生 HTML convenience attrs（如 `id` / `title` / `for` / `name` / `type` / `placeholder` / `href` / `src` 等）；`H(...)` 已按 element/component/props/slots/direct-child canonical 家族收敛，direct-child 统一通过 `IVNode` + `VueChild` 表达 |
| `mergeProps()` | Covered | `MergeProps(params VueProps[])` |
| `cloneVNode()` | Covered | `CloneVNode(IVNode)` + `CloneVNode(IVNode, VueProps)` |
| `isVNode()` | Covered | `bool IsVNode<T>(T value)` |
| `resolveComponent()` | Covered | `ResolveComponent(string)` 返回 `IVueComponent` |
| `resolveDirective()` | Covered | `ResolveDirective(string)` 返回 `VueDirectiveValue?` |
| `withDirectives()` | Covered | `WithDirectives(vnode, [PreserveParamsArray] params VueDirectiveArguments[])`；支持 `WithDirectives(vnode, d1, d2)` 且保持 runtime 第二参数为数组 |
| `withModifiers()` | Covered | `WithModifiers(Action, [PreserveParamsArray] params string[])` 与 typed `VueEventHandler<T>` overload；支持 `WithModifiers(handler, "stop", "prevent")` 且保持 runtime 第二参数为数组 |

`H(...)` 的 default slot sugar 已从 Vue 命名分块迁移到 `ChildrenToSlotIntrinsic`，后续按 `vue3-module-mapping-rules.md` 与 `vue3-mapping-details.md` 保持为稳定 children-to-slot contract。
当前这条 contract 的 canonical 分类、typed default-slot 校验、literal fast-path 与 single-evaluation IIFE 基线已经在 Phase 1 收口完成。

## 16. SFC 与 Compiler Macros

Sources:

- <https://vuejs.org/api/sfc-spec.html>
- <https://vuejs.org/api/sfc-script-setup.html>
- <https://vuejs.org/api/sfc-css-features.html>

| API 面 | 当前状态 | 目标 |
|--------|----------|------|
| SFC syntax spec | Gap | Separate Workstream |
| `<script setup>` | Gap | Separate Workstream |
| compiler macros | Gap | Separate Workstream |
| scoped CSS / CSS modules / `v-bind()` in CSS | Gap | Separate Workstream |

这些能力不应进入 `src/ECMAScript.Vue3/Vue3.cs`。如果 Jolt 未来支持 `.vue` 或类 SFC authoring，应在 Jolt/RazorVue/SFC pipeline 里设计，而不是扩张 `SemanticWalker` 的 Vue hardcoding。

## 17. Advanced APIs

Sources:

- <https://vuejs.org/api/custom-elements.html>
- <https://vuejs.org/api/ssr.html>
- <https://vuejs.org/api/custom-renderer.html>

| 分类 | 当前状态 | 目标 |
|------|----------|------|
| Custom Elements | Covered | `DefineCustomElement(VueComponentDefinition[, VueCustomElementOptions])`、`VueCustomElementComponentOptions*` merged options、`UseHost()` / `UseHost<THost>()`、`UseShadowRoot()` 已覆盖 runtime + authoring 主路径 |
| SSR | Partial | `createSSRApp` 已有；`renderToString` 等 SSR renderer API 属于 separate workstream |
| Custom Renderer | Gap | Separate Workstream；host renderer contract 过大，不应混入基础 Vue3 binding |

## 18. 补齐优先级

### P0: 不扩大 compiler 的低风险 binding

Status: 第一批已落地到 `src/ECMAScript.Vue3/Vue3.cs`，并由 `EcmaScriptVueProxyTests` 与 `AstConverterTests.Convert_ClassUsingVueP0CoverageBindings_GeneratesPlainVueImports` 覆盖。

- `version`
- `app.version`
- `app.onUnmount`
- `app.runWithContext`
- `watchPostEffect`
- `watchSyncEffect`
- `isRef`
- `unref`
- `isProxy`
- `isReactive`
- `isReadonly`
- `triggerRef`
- `shallowReactive`
- `shallowReadonly`
- `toRaw`
- `markRaw`
- `hasInjectionContext`
- `onBeforeMount`
- `onBeforeUpdate`
- `onBeforeUnmount`
- `onActivated`
- `onDeactivated`
- `mergeProps`
- `cloneVNode`
- `isVNode`
- `resolveComponent`
- `resolveDirective`

### P1: 需要小型 helper surface

- `VueAppConfig`（已落地核心路径；`compilerOptions` 指 Vue runtime compiler config，不替代 Jolt/SFC 编译配置）
- `VueWatchOptions` / `VueWatchEffectOptions`（核心路径、debugger event options、reactive object source、同类 multi-source watch 已落地）
- writable computed options（已落地）
- `VueEffectScope`（已落地）
- composition `provide` / `inject`（string key 与 typed injection key 已落地）
- `defineAsyncComponent`（loader/options 核心路径已落地）
- `customRef`（factory + handlers 已落地）
- `toRef` / `toRefs`（normalization、source key、typed refs projection 已落地）
- built-in components as `IVueComponent`（核心 built-ins 已落地）

### P2: 需要单独 authoring 设计

- Options API full object surface

### P3: 不放进 `ECMAScript.Vue3.cs` 的独立工作流

- SFC syntax / script setup / compiler macros
- template directives and special elements
- SSR renderer and hydration pipeline
- custom renderer
- compiler options for template compilation

## 19. 下一步落地建议

1. 先做 P0。它们大多是纯 `[Description("@#...")]` host binding，不需要 compiler Vue 特路。
2. 下一步做 Options API 基础 object surface 等剩余设计面。
3. 然后把后续新增场景继续压回现有 `H(...)` canonical 分类与 object-literal contract，不再把它们视为 Phase 1 缺口。
4. 最后再决定 Options API full surface、built-in components / custom elements 更高层 authoring convenience 是否进入当前版本目标。

## 20. 参考

- Vue API index: <https://vuejs.org/api/>
- Application API: <https://vuejs.org/api/application.html>
- General API: <https://vuejs.org/api/general.html>
- Composition API setup: <https://vuejs.org/api/composition-api-setup.html>
- Reactivity Core: <https://vuejs.org/api/reactivity-core.html>
- Reactivity Utilities: <https://vuejs.org/api/reactivity-utilities.html>
- Reactivity Advanced: <https://vuejs.org/api/reactivity-advanced.html>
- Lifecycle Hooks: <https://vuejs.org/api/composition-api-lifecycle.html>
- Dependency Injection: <https://vuejs.org/api/composition-api-dependency-injection.html>
- Render Function API: <https://vuejs.org/api/render-function.html>
- Built-ins: <https://vuejs.org/api/built-in-components.html>
- SFC Spec: <https://vuejs.org/api/sfc-spec.html>
- SSR API: <https://vuejs.org/api/ssr.html>
- Custom Renderer API: <https://vuejs.org/api/custom-renderer.html>
- [ECMAScript.Vue3 映射细节设计](./vue3-mapping-details.md)
- [ECMAScript.Vue3 模块映射规则](./vue3-module-mapping-rules.md)
- [src/ECMAScript.Vue3/Vue3.cs](../../../src/ECMAScript.Vue3/Vue3.cs)

