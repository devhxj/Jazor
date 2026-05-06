# ECMAScript.Pinia API 覆盖矩阵

> Scope: `src/ECMAScript.Pinia/` 当前 public surface  
> Baseline: Pinia 官方运行时 API（`pinia` 包）  
> Interpretation: “已覆盖”表示已有稳定 C# host binding；“部分覆盖”表示保留主 authoring path，但刻意没有镜像全部 TS 细节；“暂不覆盖”表示当前不进入 `ECMAScript.Pinia` 设计边界。

## 总览

| 类别 | 官方 API / 概念 | 当前 C# surface | 状态 | 说明 |
|------|-----------------|----------------|------|------|
| Root lifecycle | `createPinia()` | `Pinia.CreatePinia()` | 已覆盖 | 返回 `PiniaInstance`，同时兼容 Vue plugin 安装路径 |
| Root lifecycle | `getActivePinia()` / `setActivePinia()` | `GetActivePinia()` / `SetActivePinia(...)` | 已覆盖 | 直接映射 |
| Root lifecycle | `disposePinia()` | `DisposePinia(...)` | 已覆盖 | 直接映射 |
| Store definition | option store `defineStore(id, options)` | `DefineStore<TState>(...)` / `DefineStore<TStore, TState>(...)` | 已覆盖 | 支持默认 `Store<TState>` 投影与用户自定义 store 投影 |
| Store definition | setup store `defineStore(id, setup, options?)` | `DefineStore<TStore>(string, Func<TStore>)` / `DefineStore<TStore>(..., Func<TStore>, DefineSetupStoreOptions)` | 部分覆盖 | 当前 setup callback 不接收 helpers 参数 |
| Store definition | callable `useStore(...)` result | `StoreDefinition<TStore>.Use(...)` | 已覆盖 | 用显式方法替代 JS 函数对象调用面 |
| Store runtime | `$id` / `_customProperties` | `StoreProperties` | 已覆盖 | `_customProperties` 保留为 devtools/plugin 扩展面 |
| Store runtime | `$state` | `Store<TState>.State` | 已覆盖 | 强类型状态投影 |
| Store runtime | `$patch(object)` / `$patch(fn)` | `Patch(TState)` / `Patch(PiniaStatePatchCallback<TState>)` | 已覆盖 | function patch 保持同步 callback 形态 |
| Store runtime | `$reset()` | `Reset()` | 已覆盖 | option store 主路径 |
| Store runtime | `$subscribe(...)` | `Subscribe(...)` + `SubscribeOptions` | 已覆盖 | `flush` / `detached` 已建模 |
| Store runtime | `$onAction(...)` | `OnAction(...)` + action context | 已覆盖 | 参数/结果使用 `PiniaValue` 桥接 |
| Store runtime | `$dispose()` | `Dispose()` | 已覆盖 | 直接映射 |
| Refs / hydration / HMR | `storeToRefs()` | `StoreToRefs(...)` | 已覆盖 | 支持默认 refs bag 和用户自定义 typed refs |
| Refs / hydration / HMR | `skipHydrate()` / `shouldHydrate()` | `SkipHydrate(...)` / `ShouldHydrate(...)` | 已覆盖 | 直接映射 |
| Refs / hydration / HMR | `acceptHMRUpdate()` | `AcceptHMRUpdate(...)` | 已覆盖 | 返回 `PiniaHotUpdateHandler` |
| Options API helpers | `mapState()` | `MapState(...)` | 已覆盖 | 支持 array-form 和 object-form mapper |
| Options API helpers | `mapGetters()` | `MapGetters(...)` | 已覆盖 | 明确标记为 `MapState()` 别名 |
| Options API helpers | `mapWritableState()` | `MapWritableState(...)` | 已覆盖 | 支持 array-form 和 object-form mapper |
| Options API helpers | `mapActions()` | `MapActions(...)` | 已覆盖 | 支持 array-form 和 object-form mapper |
| Options API helpers | `mapStores()` | `MapStores(...)` | 已覆盖 | 依赖非泛型 `StoreDefinition` 基类承接异构列表 |
| Options API helpers | `setMapStoreSuffix()` | `SetMapStoreSuffix(...)` | 已覆盖 | 可设置为空字符串 |
| Plugin surface | `pinia.use(...)` / `PiniaPluginContext` | `PiniaInstance.Use(...)` / `PiniaPluginContext` | 已覆盖 | 先覆盖核心 plugin callback 语义 |
| Plugin surface | plugin-added custom properties typed merge | 无 | 部分覆盖 | 运行时扩展面存在，但未尝试做 TS module augmentation 等价物 |
| TS utility types | `_Spread` / `_MapStateReturn` / `MapStoresCustomization` 等 | 无 | 暂不覆盖 | C# 不追求镜像 Pinia 的全部类型级工具 |
| Testing package | `@pinia/testing` / `createTestingPinia()` | 无 | 暂不覆盖 | 当前只做 `pinia` 主包，不引入测试包子线 |

## 关键差异

### `defineStore(setup)` helpers 参数

Pinia 官方 setup store 允许 `storeSetup(helpers)`。  
当前 `ECMAScript.Pinia` 只建模参数为空的 `Func<TStore>`，这是一个明确的“部分覆盖”点，而不是漏写。

### `mapState()` / `mapGetters()` 自定义函数里的 `this`

Pinia 官方允许 object-form mapper 使用自定义函数，并在运行时访问组件实例 `this`。  
当前 `ECMAScript.Pinia` 只显式暴露 store 参数：

- `PiniaMapStateSelector<TStore>`
- `PiniaStateMapValue<TStore>`

组件实例 `this` 不做类型化建模。

### JS 函数对象 vs C# 可发现性

Pinia 把 `defineStore()` 返回值设计为函数对象。  
`ECMAScript.Pinia` 刻意改成：

- `StoreDefinition<TStore>`
- `StoreDefinition<TStore>.Use()`
- `StoreDefinition<TStore>.Use(pinia)`
- `StoreDefinition<TStore>.Use(pinia, hot)`

这是当前最重要的 authoring surface 取舍之一。

## 结论

当前 `ECMAScript.Pinia` 已覆盖 Pinia 主包里最常见的 authoring 路径：

- root instance
- store definition
- store runtime
- refs / hydration / HMR
- 常用 Options API helpers

剩余缺口主要是：

- setup-store helpers 参数
- plugin 扩展属性的更强类型化
- TS-only utility 家族
- `@pinia/testing`

这些缺口目前都属于“明确未纳入设计边界”，不是偶然遗漏。

