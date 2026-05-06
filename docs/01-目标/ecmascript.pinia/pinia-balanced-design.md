# ECMAScript.Pinia 平衡式目标设计

## 定位

`ECMAScript.Pinia` 是独立于 `ECMAScript` 平台内核和 `ECMAScript.Vue3` 外部库线的第三条 authoring surface。  
它的职责不是重新发明状态管理模型，也不是在 compiler 内对 `Pinia` 做名称特判，而是把 Pinia 官方运行时暴露成稳定、可测试、可扩展的 C# host binding。

## 目标

1. 保持与 Pinia 官方运行时 API 的一一映射关系，让用户看到的 C# surface 能直接对应到 `pinia` 包的真实能力。
2. 保持与 `ECMAScript.Vue3` 的依赖方向清晰：`ECMAScript.Pinia` 依赖 Vue3 绑定，但不把 Pinia 语义反向污染回 `ECMAScript` 核心层。
3. 对 compiler 保持“普通外部库”姿态：通过 `[ECMAScript("pinia")]`、`[Description("@#...")]`、`[ECMAScriptInline(...)]` 等现有 host 机制完成映射。
4. 保持 public API 不暴露 `object`，优先用 `PiniaValue`、`VueProps`、`StoreDefinition<TStore>`、`Store<TState>` 这类桥接类型承接 JS 的 unknown-like 形态。
5. 让测试边界独立：Pinia 绑定的结构/导入/降级验证应在 `ECMAScript.Pinia.Test` 中完成，而不是继续混在 `Jazor.CompilerTest` 中。

## 非目标

- 不在 compiler 中新增 `Pinia` 关键字、名字表或 host 特判。
- 不复刻 Pinia 全部 TypeScript 工具类型；像 `_Spread`、`_MapStateReturn`、`MapStoresCustomization` 这类 TS 类型级辅助不要求逐个镜像到 C#。
- 不试图把 Pinia 的“函数对象”体验原样搬进 C#。C# 侧更强调可发现性，因此允许使用显式包装方法。
- 不为低频 helper 或测试包提前铺满覆盖；优先主运行时和高频 authoring path。

## 当前推荐边界

### 1. 模块导入边界

- `Pinia` 模块入口固定使用裸导入名 `pinia`。
- 版本解析、import-map、bundler alias 不在 `ECMAScript.Pinia` 中编码。
- `Pinia.cs` 只保留模块入口标记和共享委托声明，不承载静态 API。

### 2. Store 定义边界

- `defineStore()` 仍然映射到 Pinia 官方 API。
- 因为 Pinia 返回的是可调用函数对象，C# 侧不依赖“把对象当函数调用”的隐式魔法，而是显式暴露 `StoreDefinition<TStore>.Use(...)`。
- 为了承接 `mapStores(...)` 这类异构 store-definition 列表，允许存在非泛型 `StoreDefinition` 基类，再由 `StoreDefinition<TStore>` 扩展出真实可调用面。

### 3. 状态与 unknown-like 值边界

- Store state 统一投影到 `PiniaStateTree`。
- 动态 action 参数、action 结果、helper selector 返回值等 unknown-like 位置统一经过 `PiniaValue`。
- `PiniaValue` 是 compile-time bridge，不试图重建 CLR 运行时 object graph。

### 4. Options API helper 边界

- 高优先级 helper 直接暴露：`mapState`、`mapGetters`、`mapWritableState`、`mapActions`、`mapStores`、`setMapStoreSuffix`。
- object-form helper 使用显式 mapper 类型承接：
  - `PiniaKeyMapper`
  - `PiniaStateMapper<TStore>`
  - `PiniaStateMapValue<TStore>`
- `mapState()` / `mapGetters()` 的自定义 selector 当前只显式建模“第一个参数是 store”；不额外建模未类型化的组件实例 `this`。

### 5. Setup store 边界

- 当前 setup-store authoring 先支持参数为空的 `Func<TStore>` 形式。
- Pinia 官方 `storeSetup(helpers)` 里的 helper 参数目前没有单独投影类型；在真实需求出现前，不提前发明一套 C# only helpers surface。
- 这意味着当前实现优先覆盖常见 setup-store authoring，而不是追求 TS 类型系统级等价。

### 6. 验证边界

- 结构和反射合同验证放在 `ECMAScript.Pinia.Test`。
- 编译降级验证仍可引用 `Jazor.Compiler`，但测试所有权不再属于 `Jazor.CompilerTest`。
- 统一测试入口通过 `pwsh ./scripts/test-dotnet.ps1 -Project pinia` 暴露。

## 设计取舍

### 显式 `Use(...)` 优于函数对象幻觉

Pinia 在 JS 中把 `defineStore()` 返回值设计成函数对象。  
在 C# 里如果也强行追求“看起来像函数”，要么依赖编译器魔法，要么牺牲 API 可发现性。当前路线选择显式 `Use(...)`，用一个可读、可反射、可测试的表面换掉函数对象幻觉。

### store-only selector 优于伪造 typed `this`

Pinia 官方 `mapState()` / `mapGetters()` 的自定义函数虽然能访问组件实例 `this`，但官方类型本身也不会把它做成强类型。  
在 `ECMAScript.Pinia` 中继续追求 typed `this` 只会把 surface 复杂度拉高，因此当前刻意收口到“只保证 store 参数强类型”。

### 独立测试工程优于混入编译器回归集

Pinia 绑定当然会依赖 compiler 做 import/lowering 验证，但这不等于它应该成为 compiler 工作流的一部分。  
独立测试工程能保持：

- 所有权清晰
- CI 入口清晰
- 外部库表面扩展与 compiler 语义回归分离

## 后续补齐方向

- setup-store helpers 参数的正式投影
- plugin 扩展属性的更强类型化建模
- 更完整的使用示例与文档索引
- 视需求决定是否覆盖 `@pinia/testing`

