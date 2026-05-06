# ECMAScript.Pinia 状态（2026-05-06）

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
- plugin / action listener 的 typed proxy 合同已补齐，覆盖 `PiniaPluginContext<TStore, TOptions>`、`PiniaPluginContext<TStore, TOptions, ...>`、`DefineStoreOptionsInPlugin`、`OnAction<TStore>(...)`；
- `$subscribe()` mutation 代理已补齐为 base + `Direct` / `PatchFunction` / `PatchObject` 分层，并补回 dev-only `events` 形状；
- setup-store helper / options 合同已补齐，覆盖 `PiniaSetupStoreFactory<TStore>`、`SetupStoreHelpers.Action(...)`、`DefineSetupStoreOptions<TActions>`；
- plugin merge 投影合同已补齐，覆盖 `ProjectStore(...)`、`ProjectStoreDefinition(...)`、`ProjectedStore<...>`、`ProjectedStoreDefinition<...>`；
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
- 统一脚本入口：`pwsh ./scripts/test-dotnet.ps1 -Project pinia`
- 覆盖率配置：`src/ECMAScript.Pinia.Test/coverlet.runsettings`

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
- `pwsh ./scripts/test-dotnet.ps1 -Project pinia` 可作为统一入口运行
- `samples/ECMAScript.Pinia.Counter/build-local.ps1` 可重建本地 sample host
- layout guard 已约束：
  - `Api/` + `Types/` 分层
  - `Pinia.cs` 壳文件边界
  - `ECMAScript.Pinia.csproj` 元数据约束
- proxy/import guard 已覆盖：
  - 裸 `pinia` 导入
  - `StoreDefinition<TStore>.Use(...)` lowering
  - typed `OnAction<TStore>(...)` lowering
  - typed `pinia.use(...)` plugin context lowering
  - projected plugin context lowering
  - projected store / projected store-definition identity lowering
  - subscription mutation subtype + `.events` / `.payload` lowering
  - setup-store helpers + third-argument options lowering
  - `storeToRefs` / `acceptHMRUpdate`
  - `mapState` / `mapGetters` / `mapWritableState` / `mapActions` / `mapStores`

## 下一步行动

1. plugin 投影模式继续沉淀  
   当前已提供显式 `ProjectStore(...)` / `ProjectStoreDefinition(...)` contract 以及 projected plugin context；后续重点转为 sample、推荐写法和消费约定沉淀。

2. 外部库模板经验沉淀  
   把 Pinia 这一轮项目拆分、测试拆分、文档拆分的经验继续沉淀成可复用模式。

3. sample 继续扩展  
   当前已经有 `samples/ECMAScript.Pinia.Counter/` 作为真实消费入口；后续可视需求再补 helper/HMR 更专门的 sample。

## 参考

- [ECMAScript.Pinia 目标索引](../../01-目标/ecmascript.pinia/README.md)
- [ECMAScript.Pinia 平衡式目标设计](../../01-目标/ecmascript.pinia/pinia-balanced-design.md)
- [ECMAScript.Pinia API 覆盖矩阵](../../01-目标/ecmascript.pinia/pinia-api-coverage-matrix.md)
- [ECMAScript.Pinia 剩余完善清单](../../02-计划/ecmascript.pinia/ECMAScript.Pinia.RemainingWorkChecklist.md)
- [src/ECMAScript.Pinia/README.md](../../../src/ECMAScript.Pinia/README.md)
