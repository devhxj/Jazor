# ECMAScript.Pinia 状态（2026-05-07）

> Status: 当前状态快照  
> Positioning: `src/ECMAScript.Pinia/` 外部库线的仓库级状态快照  
> Scope: 项目独立化、API/Types 分层、独立测试工程、核心 Pinia API 覆盖与文档域拆分

## 总结

`ECMAScript.Pinia` 已完成第一轮仓库级落地：

- 源码项目已独立为 `src/ECMAScript.Pinia/`；
- public surface 已按 `Api/` 与 `Types/` 分层拆分；
- `Pinia.cs` 保持模块入口壳文件职责；
- Pinia 相关测试已从 `Jazor.CompilerTest` 拆出，独立为 `src/ECMAScript.Pinia.Test/`；
- 统一测试入口已支持 `pwsh ./scripts/test-dotnet.ps1 -Project pinia`；
- 已补 `samples/ECMAScript.Pinia.Counter/` 真实消费示例，覆盖 Vue 3 + Pinia + emitted modules 的联动路径；
- `@pinia/testing` 已独立为 `src/ECMAScript.Pinia.Testing/` 与 `src/ECMAScript.Pinia.Testing.Test/`，并接入 solution / script / Jazor package build；
- plugin / action listener 的 typed proxy 合同已补齐，覆盖 `PiniaPluginContext<TStore, TOptions>`、`PiniaPluginContext<TStore, TOptions, ...>`、`DefineStoreOptionsInPlugin`、`OnAction<TStore>(...)`、`StoreActionListenerContext.After<TResult>(...)` 的显式结果类型投影，以及 `OnAnyError(PiniaValue?)` / `OnError<TError>(...)` 的错误值建模；
- `$onAction()` 的 action-specific authoring contract 已继续补齐，新增 `ProjectActionContext<TStore, TActionName, TArgs>(...)`、`TryProjectActionContext<TStore, TActionName, TArgs>(..., expectedActionName)`、`ProjectedActionContext<...>`、`ActionArgsView` / `ActionArgsView<T...>` arity family，并已扩到 16 槽位上限，用于把 `context.name` / `context.args[]` 显式投影到更强类型的 action 分支；
- `$subscribe()` mutation 代理已补齐为 base + `Direct` / `PatchFunction` / `PatchObject` 分层，并补回 dev-only `events` 形状；`SubscribeOptions` 也已完整继承 Vue `WatchOptions`，覆盖 `detached + flush + immediate + deep + once + onTrack + onTrigger`；
- `$patch(object)` 与 `SubscriptionMutationPatchObject.Payload` 已收口为显式 `PiniaStatePatch<TState>` 契约；
- `ECMAScript.Pinia.Testing` 已补 `TestingPinia`、`TestingStubActions(bool | predicate)`、`writableComputed` 等长尾 testing contract；
- setup-store helper / options 合同已补齐，覆盖 `PiniaSetupStoreFactory<TStore>`、`SetupStoreHelpers.Action(...)`、`DefineSetupStoreOptions<TActions>`，且 `SetupStoreHelpers.Action(...)` 已覆盖到 .NET 标准 `Action` / `Func` 委托族的 16 输入参数上限；
- plugin merge 投影合同已补齐，覆盖 `ProjectStore(...)`、`ProjectStoreDefinition(...)`、`ProjectedStore<...>`、`ProjectedStoreDefinition<...>`；
- projected store-definition 已统一继承 `StoreDefinition<TStore>` 调用面，`storeToRefs` / HMR / Options API helper 组合路径已补回归；
- `ECMAScript.Pinia` 已补 `PiniaStateMapValue<TStore>.From(...)`，使 `mapState()` / `mapGetters()` 的 object-form mapper 能在对象初始化器中稳定承接 key / selector 分支，同时保持 runtime 仍发出同一个 Pinia mapper object shape；
- `samples/ECMAScript.Pinia.Counter` 已补 object-form `mapState()` cookbook，覆盖 `PiniaStateMapValue<TStore>.From("key")` 与 `PiniaStateMapValue<TStore>.From(selector)` 的真实 generated DOM/runtime 路径；
- sample 已扩到 cookbook 级，覆盖 plugin projection、projected store refs、Options API helper 组合和 testing root module；
- sample 已继续扩到 multi-store / subscription cookbook，覆盖 `mapStores()` + `setMapStoreSuffix("")` 与 `$subscribe()` direct / object-patch / function-patch 组合路径；
- `pinia-consumer` 已补 Vitest smoke example，直接验证生成的 `createTestingPinia()` root 与 generated store module 可在正常前端测试框架中协同运行；
- sample root 已显式走 `createConfiguredPinia().Use(installAuditPlugin)` 真实 plugin 安装路径，projected plugin cookbook 不再依赖“样例 UI 渲染了但 root 没真正装 plugin”的弱约束；
- sample 已补 HMR cookbook，覆盖 `acceptHMRUpdate(...)`、projected store-definition HMR identity、以及 `StoreDefinition<TStore>.Use(pinia, hot)` authoring 路径；
- sample 已补 hydration cookbook，覆盖 setup-store `skipHydrate()` / `shouldHydrate()` 以及 option-store `hydrate(storeState, initialState)`；
- `pinia-consumer` 已补真实 app mount + DOM 交互断言，覆盖 projected plugin、多 store、subscription 与 HMR card 的实际运行面；
- `pinia-consumer` 已补 JS-side HMR host bridge，真实展示 `import.meta.hot.accept(...)` 如何接生成的 `acceptHMRUpdate(...)` handler；
- `pinia-consumer` 已补 Pinia root lifecycle 验证，覆盖 `setActivePinia()` / `setActivePinia(undefined)` / `getActivePinia()` / `disposePinia()` 的真实 runtime 路径；
- `pinia-consumer` 已补 store `$dispose()` 与 root 重建后的干净状态验证，并追加 repeated mount/unmount cleanup 场景；
- `ECMAScript.Pinia.Testing` 已补 `stubActions: string[]` 联合 contract，并追加独立 lowering/proxy 回归；
- `ECMAScript.Pinia.Testing` 已补 `TestingPinia.app` contract，使 `fakeApp` 宿主边界可被显式 authoring / 回归验证；
- `ECMAScript.Pinia.Testing` 已补 `TestingOptions<TDelegate>`，使 `createSpy` 可按显式 delegate 形状 authoring，同时保持 runtime 仍发出同一个 `createSpy` object-form 配置字段；
- `ECMAScript.Pinia.Testing` 已补 `TestingStubActions.From(...)` 与 `TestingStubActions<TStore>.From(...)`，使 `TestingOptions` / `TestingOptions<TDelegate, TStore>` 的对象初始化器可以稳定承接 `bool | string[] | predicate` 分支，同时保持 runtime 仍发出同一个官方 `stubActions` union shape；
- `ECMAScript.Pinia.Testing` 已补 `ProjectStubActionPredicate<TStore>(...)`，使 `stubActions` predicate 可按显式 store 投影 authoring，同时保持 runtime 仍发出同一个 predicate 函数对象；
- `ECMAScript.Pinia.Testing` 已补 `TestingStubActions<TStore>`、`TestingOptions<TDelegate, TStore>` 与 `ProjectStubActions<TStore>(...)`，使 typed `createSpy` 与 typed predicate-style `stubActions` 可以在同一个 testing options object 上组合 authoring，同时保持 runtime 仍发出同一个官方 `@pinia/testing` options shape；
- `ECMAScript.Pinia.Testing` 已补 `ProjectPlugin(...)`，使 testing root 的 `plugins` 列表可以无损复用主包里的 typed / projected Pinia plugin callback，同时保持 runtime 仍发出同一个 plugin 函数对象；
- `samples/ECMAScript.Pinia.Counter` 的 testing root 已切到 `ProjectPlugin(...)` + `ProjectStubActionPredicate<TStore>(...)` 路径，并追加前端运行时断言验证 projected testing plugin 对 custom properties / custom state 的真实生效；
- `samples/ECMAScript.Pinia.Counter` 已补 combined typed testing root，覆盖 `TestingOptions<TDelegate, TStore>` + `ProjectStubActions<TStore>(...)` + typed `createSpy` 的真实 generated runtime / Vitest 路径；
- `samples/ECMAScript.Pinia.Counter` 已补 combined typed testing root 的 explicit union factory 路线，覆盖 `TestingStubActions<TStore>.From(...)` + typed `createSpy` 的真实 generated runtime / Vitest 路径；
- sample/testing consumer 已补 stricter testing-root 验证，覆盖 named `stubActions` + `stubPatch` + `stubReset` 的真实 generated runtime 行为；
- sample/consumer 已补 explicit multi-root isolation cookbook，覆盖 `StoreDefinition.Use(pinia)` / projected store-definition / plugin custom state 在双 root 下的不串扰行为；
- sample root 已补 app-unmount teardown，覆盖 `app.unmount()` -> `disposePinia(...)` 自动回收与 repeated mount/unmount 后的干净 root 语义；
- 文档域已建立 `docs/01-目标/ecmascript.pinia/`，并补齐 `02-计划` / `03-完成` 目录。

当前更准确的状态是：**基础结构与核心运行时绑定已成形，独立测试治理已完成，后续进入增量扩展阶段**。

## 当前状态判断

### 1. 模块结构已稳定

- 入口壳文件：`src/ECMAScript.Pinia/Pinia.cs`
- API 分层：`src/ECMAScript.Pinia/Api/Pinia.Api.cs`
- 类型分层：
  - `src/ECMAScript.Pinia/Types/Pinia.Types.Core.cs`
  - `src/ECMAScript.Pinia/Types/Pinia.Types.Mapping.cs`
  - `src/ECMAScript.Pinia/Types/Pinia.Types.Store.cs`
- 项目命名空间：`ECMAScript.Pinia.csproj` 显式 `RootNamespace=ECMAScript`

这套结构已经和 `ECMAScript.Vue3` 一样，进入“可复用外部库模板”的状态。

### 2. 测试所有权已独立

Pinia 测试不再继续混在 `Jazor.CompilerTest`：

- 新测试项目：`src/ECMAScript.Pinia.Test/`
- 新测试项目：`src/ECMAScript.Pinia.Testing.Test/`
- 统一脚本入口：`pwsh ./scripts/test-dotnet.ps1 -Project pinia`
- 统一脚本入口：`pwsh ./scripts/test-dotnet.ps1 -Project pinia-testing`
- 覆盖率配置：`src/ECMAScript.Pinia.Test/coverlet.runsettings`
- 覆盖率配置：`src/ECMAScript.Pinia.Testing.Test/coverlet.runsettings`

这意味着：

- Pinia 绑定表面的扩展不再污染 compiler 回归工程边界；
- 仍然可以在 Pinia 测试中引用 `Jazor.Compiler` 做 import/lowering 验证；
- “测试依赖 compiler” 与 “测试属于 compiler” 这两个概念已经明确分开。

### 3. 当前 API 覆盖基线可用

当前已进入稳定 public surface 的部分包括：

- root lifecycle
  - `createPinia`
  - `getActivePinia`
  - `setActivePinia`
  - `clearActivePinia`
  - `disposePinia`
- store definition / runtime
  - option store `defineStore`
  - setup store `defineStore`（parameterless + helper-aware 双路径）
  - `StoreDefinition<TStore>.Use(...)`
  - `$state` / `$patch` / `$reset` / `$subscribe`
  - `$onAction` / `OnAction<TStore>(...)` / `$dispose`
- refs / hydration / HMR
  - `storeToRefs`
  - `skipHydrate`
  - `shouldHydrate`
  - `acceptHMRUpdate`
- plugin authoring
  - `PiniaInstance.Use(...)`
  - `PiniaPluginContext<TStore, TOptions>`
  - `PiniaPluginContext<TStore, TOptions, ...>`
  - `DefineStoreOptionsInPlugin`
- `ProjectStore(...)`
- `ProjectStoreDefinition(...)`
- `PiniaTesting.CreateTestingPinia(...)`
- `PiniaTesting.TestingOptions`
- Options API helpers
  - `mapState`
  - `mapGetters`
  - `mapWritableState`
  - `mapActions`
  - `mapStores`
  - `setMapStoreSuffix`

## 当前验证基线

针对这一轮拆分和补齐，已经形成稳定回归基线：

- `src/ECMAScript.Pinia/ECMAScript.Pinia.csproj` 可独立构建
- `src/ECMAScript.Pinia.Test/ECMAScript.Pinia.Test.csproj` 可独立测试
- `src/ECMAScript.Pinia.Testing/ECMAScript.Pinia.Testing.csproj` 可独立构建
- `src/ECMAScript.Pinia.Testing.Test/ECMAScript.Pinia.Testing.Test.csproj` 可独立测试
- `pwsh ./scripts/test-dotnet.ps1 -Project pinia` 可作为统一入口运行
- `pwsh ./scripts/test-dotnet.ps1 -Project pinia-testing` 可作为统一入口运行
- `samples/ECMAScript.Pinia.Counter/build-local.ps1` 可重建本地 sample host
- layout guard 已约束：
  - `Api/` + `Types/` 分层
  - `Pinia.cs` 壳文件边界
  - `ECMAScript.Pinia.csproj` 元数据约束
- proxy/import guard 已覆盖：
  - 裸 `pinia` 导入
  - `StoreDefinition<TStore>.Use(...)` lowering
  - typed `OnAction<TStore>(...)` lowering
- projected action-context + array-slot args lowering
- guarded projected action-context + runtime name-guard lowering
- typed `pinia.use(...)` plugin context lowering
- projected plugin context lowering
- projected store / projected store-definition identity lowering
- projected store + `storeToRefs(...)` 组合 lowering
- projected store-definition + `acceptHMRUpdate(...)` / helper 组合 lowering
- projected plugin cookbook 组合 lowering
- multi-store cookbook lowering
- subscription mutation subtype + `.events` / `.payload` lowering
- subscription cookbook `Subscribe(..., SubscribeOptions)` + patch-variant lowering
- typed `PiniaStatePatch<TState>` object patch lowering
- compiler 侧 `Description("@#[0]")` / `@#["key"]` 计算属性别名访问已补底层支持，可复用于外部库数组/字典式显式投影
- setup-store helpers + third-argument options lowering
- `storeToRefs` / `acceptHMRUpdate`
- `skipHydrate` / `shouldHydrate` / option-store `hydrate`
- `getActivePinia` / `setActivePinia` / `clearActivePinia` / `disposePinia`
- `mapState` / `mapGetters` / `mapWritableState` / `mapActions` / `mapStores`
- `mapState` / `mapGetters` object-form union factory lowering（`PiniaStateMapValue<TStore>.From(...)`）
- `@pinia/testing` `createTestingPinia(...)` / `TestingOptions` lowering
- `@pinia/testing` object-initializer union factory lowering（`TestingStubActions.From(...)` / `TestingStubActions<TStore>.From(...)`）
- `@pinia/testing` named-list/predicate `stubActions` / `plugins` / `writableComputed` cookbook lowering
- `@pinia/testing` combined typed options 的 boolean / named-list / predicate `stubActions` lowering
- `@pinia/testing` typed predicate `stubActions` identity projection lowering
- `@pinia/testing` combined typed `createSpy` + typed predicate `stubActions` options lowering
- `@pinia/testing` fake-app `TestingPinia.app` lowering / proxy contract
- `@pinia/testing` strict root runtime validation for named action stubs and patch/reset stubs
- frontend-side Vitest smoke validation for generated testing/store modules
- frontend-side Vitest DOM validation for generated root app mount/unmount and cookbook interactions
- frontend-side Vitest runtime validation for JS HMR bridge、Pinia root disposal、store `$dispose()` 与 root recreate
- frontend-side Vitest runtime/DOM validation for app-unmount -> `disposePinia(...)` 自动回收
- frontend-side Vitest runtime/DOM validation for explicit multi-root isolation

## 下一步行动

1. plugin 投影模式继续沉淀  
   当前已提供显式 `ProjectStore(...)` / `ProjectStoreDefinition(...)` contract 以及 projected plugin context；后续重点转为 sample、推荐写法和消费约定沉淀。

2. 外部库模板经验沉淀  
   把 Pinia 这一轮项目拆分、测试拆分、文档拆分的经验继续沉淀成可复用模式。

3. sample 继续扩展  
   当前已经有 `samples/ECMAScript.Pinia.Counter/` 作为真实消费入口，并覆盖 projected plugin、multi-store、subscription、hydration、HMR、testing-root 与 root-lifecycle cookbook；后续可视需求再补更复杂的跨组件与重复挂载场景。

## 参考

- [ECMAScript.Pinia 目标索引](../../01-目标/ecmascript.pinia/README.md)
- [ECMAScript.Pinia 平衡式目标设计](../../01-目标/ecmascript.pinia/pinia-balanced-design.md)
- [ECMAScript.Pinia API 覆盖矩阵](../../01-目标/ecmascript.pinia/pinia-api-coverage-matrix.md)
- [ECMAScript.Pinia 剩余完善清单](../../02-计划/ecmascript.pinia/ECMAScript.Pinia.RemainingWorkChecklist.md)
- [src/ECMAScript.Pinia/README.md](../../../src/ECMAScript.Pinia/README.md)
