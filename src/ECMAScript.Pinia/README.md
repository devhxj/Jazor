# ECMAScript.Pinia

`ECMAScript.Pinia` 是参照 `ECMAScript.Vue3` 建立的独立外部库项目，用于承载 Pinia 运行时绑定，而不是把状态库语义重新塞回 compiler 特判。

## Install

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.22" />
  <PackageReference Include="ECMAScript.Pinia" Version="0.1.22" />
</ItemGroup>
```

如果需要 `@pinia/testing` 绑定，再额外引用：

```xml
<ItemGroup>
  <PackageReference Include="ECMAScript.Pinia.Testing" Version="0.1.22" />
</ItemGroup>
```

`ECMAScript.Pinia` 与 `ECMAScript.Pinia.Testing` 都作为独立前端库发布，不再由 `Jazor` 主包默认捆绑。

## Responsibilities

- 提供 `createPinia()`、`getActivePinia()`、`setActivePinia()`、`setActivePinia(undefined)`、`disposePinia()`、`defineStore()`、`storeToRefs()`、HMR/hydration 相关的 C# host binding。
- 提供 `mapState()`、`mapGetters()`、`mapWritableState()`、`mapActions()`、`mapStores()`、`setMapStoreSuffix()` 的 Options API helper 绑定。
- 提供 `PiniaInstance`、`StoreDefinition<TStore>`、`Store<TState>`、`PiniaPluginContext<TStore, TOptions>`、`PiniaPluginContext<TStore, TOptions, ...>`、`DefineStoreOptionsInPlugin`、`SetupStoreHelpers`、`DefineSetupStoreOptions`、`ProjectedStore<...>`、`ProjectedStoreDefinition<...>` 等运行时投影类型。
- 为 `$onAction(...)` 与 `pinia.use(...)` 提供 untyped + typed 双轨 authoring proxy，便于在生产代码里逐步收口到更强类型的 store/plugin contract；`StoreActionListenerContext.After(...)` 同时支持无结果、`PiniaValue` 桥接和显式结果类型投影，`onError(...)` 同时覆盖 `Error` 便利层、`OnAnyError(PiniaValue?)` unknown-like 层和显式泛型错误投影。
- 为 `$onAction()` 的 action-specific authoring 再补显式投影 helper：`ProjectActionContext<TStore, TActionName, TArgs>(...)` + `TryProjectActionContext<TStore, TActionName, TArgs>(..., expectedActionName)` + `ProjectedActionContext<...>` + `ActionArgsView` / `ActionArgsView<T...>` arity family。前者保留类型级 identity 投影，后者在 runtime 上附带显式 `context.name === expectedActionName` guard；二者都不伪造 TypeScript union 魔法。当前槽位族已补到 16 参数上限，与 setup-store action helper 的生产面保持一致。
- 对 `StoreActionListenerContext.After<TResult>(...)` 的方法组用法，当前 C# 通常需要显式写出 `<TResult>`；这是语言推断边界，不是 Pinia runtime 缺口。
- 为 `$subscribe(...)` 提供 base mutation + `Direct` / `PatchFunction` / `PatchObject` 子类型代理，而不是把三种运行时形状压成一个简化类型；`SubscribeOptions` 继承 Vue `WatchOptions`，保留 `detached + flush + immediate + deep + once + onTrack + onTrigger` 的完整 authoring 面。
- 为 setup-store `defineStore(id, setup, options?)` 提供 helper-aware callback overload 与 `actions` options contract，而不是继续停留在“无参 callback + 空 options 壳类型”。
- `SetupStoreHelpers.Action(fn, name?)` 覆盖 `Action` / `Func` 委托族直到 .NET 框架上限（16 输入参数），避免把 setup-store action helper 再裁成低参数数目的样例级接口。
- 对 4 参数及以上的方法组，当前 C# 语言推断通常需要显式 generic 参数或先落到委托局部变量；这是语言绑定特性，不是 Pinia runtime 缺口。
- 为 `mapState()` / `mapGetters()` 的 object-form mapper 提供 `PiniaStateMapValue<TStore>.From(...)`，让对象初始化器里的 key / selector 分支都能稳定承接方法组与显式 mapper value，而不依赖多跳隐式转换。
- 为 plugin-added custom properties / custom state 提供显式投影 helper：`ProjectStore(...)` / `ProjectStoreDefinition(...)` 只做类型级 identity 投影，不引入伪 runtime API。
- 为链式 plugin authoring 提供 projected plugin context：高阶 `PiniaPluginContext<TStore, TOptions, ...>` 与 `PiniaInstance.Use(...)` overload 可以直接在 plugin callback 中读取前置插件加进来的 store/store.$state 扩展。
- 保持对 Vue 侧依赖的边界明确：Pinia 绑定依赖 `ECMAScript.Vue3`，不反向污染 `ECMAScript` 核心模块。

## Boundaries

- 不在 compiler 中新增 `Pinia` 名称特判。
- 当前覆盖核心 store/runtime 面和常用 Options API helper；更长尾的 helper 仍按需求增量补充。
- 运行时导入使用裸模块名 `pinia`，交给宿主 import-map / bundler 决定最终版本解析。
- `@pinia/testing` 已拆分到独立项目 `src/ECMAScript.Pinia.Testing/`，不再继续塞进主包边界。

## Layout

- `Pinia.cs`
  - 模块入口，只保留导入标记和委托声明。
- `Api/Pinia.Api.cs`
  - `createPinia`、`defineStore`、`storeToRefs`、Options API helper、hydration/HMR 入口。
- `Types/Pinia.Types.*.cs`
  - store/runtime 形状、options bag、callback context、helper mapper、辅助值类型。

## Related Projects

- `src/ECMAScript.Pinia.Test/`
  - `pinia` 主包回归测试。
- `src/ECMAScript.Pinia.Testing/`
  - `@pinia/testing` 独立绑定线。
- `src/ECMAScript.Pinia.Testing.Test/`
  - `@pinia/testing` 独立回归测试工程。
