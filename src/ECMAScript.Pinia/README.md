# ECMAScript.Pinia

`ECMAScript.Pinia` 是参照 `ECMAScript.Vue3` 建立的独立外部库项目，用于承载 Pinia 运行时绑定，而不是把状态库语义重新塞回 compiler 特判。

## Responsibilities

- 提供 `createPinia()`、`defineStore()`、`storeToRefs()`、HMR/hydration 相关的 C# host binding。
- 提供 `mapState()`、`mapGetters()`、`mapWritableState()`、`mapActions()`、`mapStores()`、`setMapStoreSuffix()` 的 Options API helper 绑定。
- 提供 `PiniaInstance`、`StoreDefinition<TStore>`、`Store<TState>`、`PiniaPluginContext<TStore, TOptions>`、`PiniaPluginContext<TStore, TOptions, ...>`、`DefineStoreOptionsInPlugin`、`SetupStoreHelpers`、`DefineSetupStoreOptions`、`ProjectedStore<...>`、`ProjectedStoreDefinition<...>` 等运行时投影类型。
- 为 `$onAction(...)` 与 `pinia.use(...)` 提供 untyped + typed 双轨 authoring proxy，便于在生产代码里逐步收口到更强类型的 store/plugin contract。
- 为 `$subscribe(...)` 提供 base mutation + `Direct` / `PatchFunction` / `PatchObject` 子类型代理，而不是把三种运行时形状压成一个简化类型。
- 为 setup-store `defineStore(id, setup, options?)` 提供 helper-aware callback overload 与 `actions` options contract，而不是继续停留在“无参 callback + 空 options 壳类型”。
- 为 plugin-added custom properties / custom state 提供显式投影 helper：`ProjectStore(...)` / `ProjectStoreDefinition(...)` 只做类型级 identity 投影，不引入伪 runtime API。
- 为链式 plugin authoring 提供 projected plugin context：高阶 `PiniaPluginContext<TStore, TOptions, ...>` 与 `PiniaInstance.Use(...)` overload 可以直接在 plugin callback 中读取前置插件加进来的 store/store.$state 扩展。
- 保持对 Vue 侧依赖的边界明确：Pinia 绑定依赖 `ECMAScript.Vue3`，不反向污染 `ECMAScript` 核心模块。

## Boundaries

- 不在 compiler 中新增 `Pinia` 名称特判。
- 当前覆盖核心 store/runtime 面和常用 Options API helper；更长尾的 helper 仍按需求增量补充。
- 运行时导入使用裸模块名 `pinia`，交给宿主 import-map / bundler 决定最终版本解析。

## Layout

- `Pinia.cs`
  - 模块入口，只保留导入标记和委托声明。
- `Api/Pinia.Api.cs`
  - `createPinia`、`defineStore`、`storeToRefs`、Options API helper、hydration/HMR 入口。
- `Types/Pinia.Types.*.cs`
  - store/runtime 形状、options bag、callback context、helper mapper、辅助值类型。
