# ECMAScript.Pinia

`ECMAScript.Pinia` 是参照 `ECMAScript.Vue3` 建立的独立外部库项目，用于承载 Pinia 运行时绑定，而不是把状态库语义重新塞回 compiler 特判。

## Responsibilities

- 提供 `createPinia()`、`defineStore()`、`storeToRefs()`、HMR/hydration 相关的 C# host binding。
- 提供 `PiniaInstance`、`StoreDefinition<TStore>`、`Store<TState>` 等运行时投影类型。
- 保持对 Vue 侧依赖的边界明确：Pinia 绑定依赖 `ECMAScript.Vue3`，不反向污染 `ECMAScript` 核心模块。

## Boundaries

- 不在 compiler 中新增 `Pinia` 名称特判。
- 不把 `mapState` / `mapActions` 这类更高层 Options API helper 一次性全部铺满；当前先收核心 store/runtime 面。
- 运行时导入使用裸模块名 `pinia`，交给宿主 import-map / bundler 决定最终版本解析。

## Layout

- `Pinia.cs`
  - 模块入口，只保留导入标记和委托声明。
- `Api/Pinia.Api.cs`
  - `createPinia`、`defineStore`、`storeToRefs`、hydration/HMR 入口。
- `Types/Pinia.Types.*.cs`
  - store/runtime 形状、options bag、callback context、辅助值类型。
