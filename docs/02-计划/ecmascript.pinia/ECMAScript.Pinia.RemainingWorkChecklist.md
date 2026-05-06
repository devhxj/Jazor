# ECMAScript.Pinia 剩余完善清单

> Status: 初始落地后的活跃清单  
> Updated: 2026-05-06  
> Positioning: 基于 `docs/01-目标/ecmascript.pinia/` 的设计边界、`src/ECMAScript.Pinia/` 真实代码状态与 `src/ECMAScript.Pinia.Test/` 验证基线整理的下一阶段工作清单。  
> Scope: 当前已完成独立项目、独立测试工程、核心 runtime/helper 绑定；本清单只列增量补齐项，不重复把已落地的基础项目拆分当作待办。

## 1. 当前判断

`ECMAScript.Pinia` 已经完成第一轮“外部库独立化 + 核心运行时绑定 + 独立测试治理”收口：

- `src/ECMAScript.Pinia/` 已作为独立项目存在；
- `Api/` / `Types/` / `Pinia.cs` 壳文件分层已建立；
- `ECMAScript.Pinia.Test` 已独立于 `Jazor.CompilerTest`；
- `pwsh ./scripts/test-dotnet.ps1 -Project pinia` 已成为统一入口；
- 核心 runtime、hydration/HMR、以及常用 Options API helper 已有回归守护。

当前更准确的状态是：**基础落地已完成，后续进入增量覆盖与 authoring contract 继续收口阶段**。

## 2. 当前主线项

### 2.1 plugin surface 更强类型化

当前已覆盖：

- `PiniaInstance.Use(...)`
- `PiniaPluginContext`
- `PiniaPluginContext<TStore, TOptions>`
- `PiniaPluginContext<TStore, TOptions, ...>`
- `DefineStoreOptionsInPlugin`
- `OnAction<TStore>(...)`
- `_customProperties` 扩展面
- `ProjectStore(...)`
- `ProjectStoreDefinition(...)`
- `ProjectedStore<...>` / `ProjectedStoreDefinition<...>`

剩余问题：

- 如何为常见 plugin authoring 给出推荐 projection / record 模式；
- 如何把 plugin-added custom state 的消费写法沉淀成统一示例；
- 在不引入“module augmentation 等价物”的前提下，继续提升 authoring 可读性。

### 2.2 API 覆盖矩阵驱动补齐

当前已覆盖最常见 authoring path，但还有几类缺口：

- 更长尾 helper
- `@pinia/testing`

推荐原则：

- 优先补“官方常见文档路径 + 仓库真实需求”交集；
- 不机械复制 Pinia 全部 TypeScript 类型工具；
- 每补一类 API，同步补结构守护和 import/lowering 验证。

### 2.4 外部库模板化

`ECMAScript.Pinia` 已经成为第二个相对完整的外部库样例。  
后续需要继续沉淀模板经验：

- 独立项目元数据约束
- `Api/` / `Types/` 分层
- 壳文件约束
- 独立测试工程
- docs `01-目标 / 02-计划 / 03-完成` 拆分

这条经验后续可服务于更多 `ECMAScript.*` 外部库。

## 3. 刻意缺口

这些缺口当前属于“明确不急着补”，而不是遗漏：

### 3.1 TypeScript 类型级工具家族

例如：

- `_Spread`
- `_MapStateReturn`
- `MapStoresCustomization`

当前不要求逐个做 C# 等价物。

### 3.2 `@pinia/testing`

测试包已按独立边界落地为：

- `src/ECMAScript.Pinia.Testing/`
- `src/ECMAScript.Pinia.Testing.Test/`

后续剩余工作不再是“要不要拆”，而是：

- 是否继续补更长尾 testing options
- 是否需要补 sample / cookbook 场景
- 是否需要为 testing root 和 plugin 联动追加更多组合回归

### 3.3 组件实例 `this` 的强类型 helper 回调

`mapState()` / `mapGetters()` 的自定义 selector 当前只保证 store 参数强类型。  
组件实例 `this` 不做类型化是刻意取舍。

## 4. 工程化项

### 4.1 文档持续同步

需要继续完成：

- 让 `src/ECMAScript.Pinia/README.md` 与 `docs/01-目标/ecmascript.pinia/*` 不漂移；
- 后续每轮补齐 API 时同步更新覆盖矩阵；
- 视工作量决定是否建立 `docs/03-完成/ecmascript.pinia/status.md` 的定期快照节奏。

### 4.2 示例与消费验证

当前已补：

- `samples/ECMAScript.Pinia.Counter/` 小型 store authoring sample
- Vue3 + Pinia 联动示例
- `storeToRefs()` + `$patch()` + `$reset()` 的最小消费场景

后续仍可按需求追加：

- `mapStores()` / `mapState()` / `mapActions()` 的更偏 Options API sample
- `$subscribe()` mutation subtype 的更完整 sample
- HMR 专门场景 sample

## 5. 推荐推进顺序

建议按以下顺序继续推进：

1. plugin merge 属性收口模式
2. plugin projection 的真实消费示例或 sample
3. 视需求决定是否引入更长尾 helper
4. 继续完善 `ECMAScript.Pinia.Testing` 的 sample / 组合回归 / 文档
