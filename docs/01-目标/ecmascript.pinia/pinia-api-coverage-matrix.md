# ECMAScript.Pinia API 覆盖矩阵

> Scope: `src/ECMAScript.Pinia/` 当前 public surface  
> Baseline: Pinia 官方运行时 API（`pinia` 包）  
> Interpretation: “已覆盖”表示已有稳定 C# host binding；“部分覆盖”表示保留主 authoring path，但刻意没有镜像全部 TS 细节；“暂不覆盖”表示当前不进入 `ECMAScript.Pinia` 设计边界。

## 总览

| 类别 | 官方 API / 概念 | 当前 C# surface | 状态 | 说明 |
|------|-----------------|----------------|------|------|
| Root lifecycle | `createPinia()` | `Pinia.CreatePinia()` | 已覆盖 | 返回 `PiniaInstance`，同时兼容 Vue plugin 安装路径 |
| Root lifecycle | `getActivePinia()` / `setActivePinia()` / `setActivePinia(undefined)` | `GetActivePinia()` / `SetActivePinia(...)` / `ClearActivePinia()` | 已覆盖 | 激活具体 root 直接映射；清空 active root 使用显式 helper 发出 `setActivePinia(undefined)` |
| Root lifecycle | `disposePinia()` | `DisposePinia(...)` | 已覆盖 | 直接映射 |
| Store definition | option store `defineStore(id, options)` | `DefineStore<TState>(...)` / `DefineStore<TStore, TState>(...)` | 已覆盖 | 支持默认 `Store<TState>` 投影与用户自定义 store 投影 |
| Store definition | setup store `defineStore(id, setup, options?)` | `DefineStore<TStore>(string, Func<TStore>)` / `DefineStore<TStore>(string, PiniaSetupStoreFactory<TStore>)` / `DefineSetupStoreOptions` | 已覆盖 | 同时支持 parameterless 与 helper-aware setup callback；`actions` options contract 已建模 |
| Store definition | callable `useStore(...)` result | `StoreDefinition<TStore>.Use(...)` | 已覆盖 | 用显式方法替代 JS 函数对象调用面 |
| Store runtime | `$id` / `_customProperties` | `StoreProperties` | 已覆盖 | `_customProperties` 保留为 devtools/plugin 扩展面 |
| Store runtime | `$state` | `Store<TState>.State` | 已覆盖 | 强类型状态投影 |
| Store runtime | `$patch(object)` / `$patch(fn)` | `Patch(PiniaStatePatch<TState>)` / `Patch(PiniaStatePatchCallback<TState>)` | 已覆盖 | object patch 改为显式 patch contract，不再误建模为完整状态对象 |
| Store runtime | `$reset()` | `Reset()` | 已覆盖 | option store 主路径 |
| Store runtime | `$subscribe(...)` | `Subscribe(...)` + `SubscribeOptions` + mutation subtype family | 已覆盖 | `SubscribeOptions` 已覆盖 `detached + WatchOptions`，包括 `flush` / `immediate` / `deep` / `once` / `onTrack` / `onTrigger`，同时 mutation 形状已建模 |
| Store runtime | `$onAction(...)` | `OnAction(...)` / `OnAction<TStore>(...)` + action context | 已覆盖 | 支持 untyped 与 typed listener proxy；`After(...)` 同时支持无结果、`PiniaValue` 桥接和显式结果类型投影；`onError(...)` 同时覆盖 `Error` 便利层、`OnAnyError(PiniaValue?)` unknown-like 层和显式泛型错误投影；补充 `ProjectActionContext<TStore, TActionName, TArgs>(...)`、`TryProjectActionContext<TStore, TActionName, TArgs>(..., expectedActionName)` 与 `ActionArgsView` / `ActionArgsView<T...>` 作为显式 action-name / args 投影入口；对泛型方法组通常需显式写类型参数 |
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
| Plugin surface | `pinia.use(...)` / `PiniaPluginContext` | `PiniaInstance.Use(...)` / `PiniaPluginContext` / `PiniaPluginContext<TStore, TOptions>` / `PiniaPluginContext<TStore, TOptions, ...>` / `DefineStoreOptionsInPlugin` | 已覆盖 | 支持 untyped、typed、以及 chained-plugin projected context/options 投影 |
| Plugin surface | plugin-added custom properties / custom state typed propagation | `ProjectStore(...)` / `ProjectStoreDefinition(...)` + `ProjectedStore<...>` / `ProjectedStoreDefinition<...>` | 已覆盖 | 使用显式 identity 投影承接 store / store.$state 的 plugin 扩展，不做 TS module augmentation 等价物 |
| TS utility types | `_Spread` / `_MapStateReturn` / `MapStoresCustomization` 等 | 无 | 暂不覆盖 | C# 不追求镜像 Pinia 的全部类型级工具 |
| Testing package | `@pinia/testing` / `createTestingPinia()` | `ECMAScript.Pinia.Testing` / `PiniaTesting.CreateTestingPinia(...)` / `TestingPinia` / `TestingOptions` / `TestingOptions<TDelegate>` / `ProjectPlugin(...)` | 已覆盖 | 作为独立外部库与独立测试工程落地，不混入 `ECMAScript.Pinia` 主包；`stubActions` 已覆盖 `bool | string[] | predicate`，`createSpy` 同时支持非泛型与显式 typed delegate authoring，typed/projected plugin 可通过显式 identity 投影复用到 testing root |

## 关键差异

### `defineStore(setup)` helpers 参数

Pinia 官方 setup store 允许 `storeSetup(helpers)`。  
`ECMAScript.Pinia` 现在同时支持：

- `Func<TStore>`
- `PiniaSetupStoreFactory<TStore>`
- `SetupStoreHelpers.Action(fn, name?)`
  说明：当前已把 `Action` / `Func` 委托族覆盖到 .NET 标准上限（16 输入参数），不再停留在低参数子集。
- 对 4 参数及以上的方法组，C# 通常需要显式 generic 参数或先赋给委托局部变量；这是语言推断边界，不是 host binding 缺口。
- `DefineSetupStoreOptions` / `DefineSetupStoreOptions<TActions>`

这条 authoring path 不再停留在“无参 callback”的简化模型。

### `mapState()` / `mapGetters()` 自定义函数里的 `this`

Pinia 官方允许 object-form mapper 使用自定义函数，并在运行时访问组件实例 `this`。  
当前 `ECMAScript.Pinia` 只显式暴露 store 参数：

- `PiniaMapStateSelector<TStore>`
- `PiniaStateMapValue<TStore>`

组件实例 `this` 不做类型化建模。

### plugin 扩展属性的类型传播

Pinia plugin 现在已经支持：

- `PiniaPluginContext<TStore>`
- `PiniaPluginContext<TStore, TOptions>`
- `PiniaPluginContext<TStore, TOptions, TCustomProperties>`
- `PiniaPluginContext<TStore, TOptions, TCustomProperties, TCustomState>`
- `DefineStoreOptionsInPlugin`
- typed plugin return record

但 `ECMAScript.Pinia` 不会尝试把它做成 TypeScript `module augmentation` 的一比一镜像。  
当前改为显式投影路线：

- `ProjectStore(...)`
- `ProjectStoreDefinition(...)`
- `ProjectedStore<...>.AsStore()`
- `ProjectedStore<...>.AsCustomProperties()`
- `ProjectedStore<...>.AsCustomState()`

这些 helper 只做类型级 identity 投影，不引入额外 `pinia` runtime API。

对链式 plugin authoring，`PiniaInstance.Use(...)` 还提供更高阶 typed overload，允许 plugin callback 在 `context.store` 上直接读取前置插件加进来的 custom properties / custom state，而不是回退到手写混合 store 类型。

### `$onAction()` 的 action-specific typing

Pinia 官方 TypeScript 类型把 `$onAction()` 的 context 建模成按 action name 分发的 union，`name` 与 `args` 会随 action 分支一起收窄。  
`ECMAScript.Pinia` 不尝试在 C# 里伪造同等 union/type-level 魔法，而是改成显式投影路线：

- `ProjectActionContext<TStore, TActionName, TArgs>(...)`
- `TryProjectActionContext<TStore, TActionName, TArgs>(..., expectedActionName)`
- `ProjectedActionContext<TStore, TActionName, TArgs>.ActionName`
- `ProjectedActionContext<TStore, TActionName, TArgs>.ActionArgs`
- `ActionArgsView`
- `ActionArgsView<T...>`

其中：

- `ProjectActionContext(...)` 只做类型级 identity 投影；
- `TryProjectActionContext(..., expectedActionName)` 会额外发出显式 runtime name guard，只有 `context.name === expectedActionName` 时才返回非空投影。

这些 helper 不创建新的 runtime context，也不改变 Pinia 原始 `context.name` / `context.args[]` 结构。  
当前 `ActionArgsView` / `ActionArgsView<T...>` 采用稳定的数组槽位语义，并提供逐层扩展的 arity family，当前已补到 16 槽位上限；适合在生产代码里把高频 action 的参数面显式收口为可读的命名 contract，而不是继续在业务层散落 `PiniaValue[]`。

### `@pinia/testing` 独立边界

`ECMAScript.Pinia.Testing` 当前已覆盖：

- `createTestingPinia()`
- `TestingPinia`
- `TestingPinia.App`
- `TestingOptions`
- `TestingInitialState`
- `TestingStubActions`（`bool | string[] | predicate`）
- `PiniaTestingSpyFactory`
- `TestingOptions<TDelegate>`（typed `createSpy` authoring，不改变 runtime `createSpy` field shape）
- `ProjectPlugin(...)`（typed / projected Pinia plugin 到 testing `plugins` 列表的显式 identity 投影）
- `writableComputed` / `stubPatch` / `stubReset` / `fakeApp` / `plugins`

同时已补 cookbook 级 lowering 回归，覆盖：

- object-form `initialState`
- plugin install list
- named-action `stubActions`
- predicate-style `stubActions`
- `writableComputed`
- testing root sample module
- `fakeApp` + `TestingPinia.app` runtime seam
- consumer-side Vitest smoke example against generated testing/store modules

### `$subscribe()` mutation shape

Pinia 官方 subscription callback 不是一个单薄对象，而是：

- base mutation metadata
- `Direct`
- `PatchFunction`
- `PatchObject`

其中 `payload` 只稳定存在于 object-patch 形状上，且其类型应为 deep-partial patch payload，`events` 也会因 mutation kind 呈现单事件或事件数组。  
`ECMAScript.Pinia` 现在按这个分层建模，并把 object-patch payload 收口为 `PiniaStatePatch<TState>`，而不是继续把所有字段塞回一个“总有完整 state payload”的简化类型。

### JS 函数对象 vs C# 可发现性

Pinia 把 `defineStore()` 返回值设计为函数对象。  
`ECMAScript.Pinia` 刻意改成：

- `StoreDefinition<TStore>`
- `StoreDefinition<TStore>.Use()`
- `StoreDefinition<TStore>.Use(pinia)`
- `StoreDefinition<TStore>.Use(pinia, hot)`

这是当前最重要的 authoring surface 取舍之一。

其中 `Use(pinia, hot)` 已进入 cookbook/回归覆盖，用于承接真实 HMR host 在热更新过程中把旧热态 store 重新解析回 typed store 的路径。

## 结论

当前 `ECMAScript.Pinia` 已覆盖 Pinia 主包里最常见的 authoring 路径：

- root instance
- store definition
- store runtime
- refs / hydration / HMR
- 常用 Options API helpers

剩余缺口主要是：

- plugin 投影模式的进一步 sample / cookbook 化
- TS-only utility 家族
- `ECMAScript.Pinia.Testing` 的更长尾 options / sample / 回归扩展

这些缺口目前都属于“已建立主线后继续扩展的增量项”，不是偶然遗漏。
