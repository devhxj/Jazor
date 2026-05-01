# ECMAScript

> Status: active reference
> Positioning: core JavaScript host projection assembly for Jazor module authoring.

`ECMAScript` 提供 Jazor 的基础 JavaScript 运行时投影，包括：

- 全局宿主表面（`Global.cs`）
- Vue 运行时 authoring surface（`Vue3.cs`）
- `Either<...>`、DOM、Web API 等核心契约

## Responsibilities

- 把 JavaScript / Web API 表面以稳定的 C# host contract 形式暴露出来。
- 保持 `ECMAScript("npm:...")` / `ECMAScriptModule("...")` 上声明的包 specifier 与模块路径原样进入 emitted JS。
- 为上层模块 authoring 提供 Vue `createApp` / `defineComponent` / `h` / `ref` / `computed` 等编译时可投影 API。

## Vue Authoring Surface

`Vue3.cs` 当前提供两条组件 authoring 通道：

1. `VueComponentOptions`
   - 面向无 props 的简单组件。
   - 支持 `Render` 或无参 `Setup`。

2. `VueComponentOptions<TProps>`
   - 面向 typed props 的组件。
   - 可通过 `PropNames` 显式声明运行时 `props` 合同；未显式提供时，编译器会从 `TProps` 的公共实例属性自动推断。
   - 可通过 `EmitNames` 显式声明运行时 `emits` 合同；未显式提供时，编译器会从 `setup` 中的 `context.Emit("...")` 稳定字面量调用自动推断。
   - `Setup` 形状为 `VueTypedSetupCallback<TProps>`，对应 Vue 的 `setup(props, context)`.

3. `VueComponentOptions<TProps, TSlots>`
   - 面向同时需要 typed props 和 typed slots 的组件。
   - `Setup` 形状为 `VueTypedSetupCallback<TProps, TSlots>`。
   - `context` 形状为 `VueSetupContext<TSlots>`，`context.Slots` 会以 `TSlots` 的 authoring contract 暴露出来。
   - `DefineComponent(...)` 返回 `IVueComponent<TProps, TSlots>`，`H(component, props, slots)` 可把读写两侧合同接起来。

4. `VueSlotComponentOptions<TSlots>`
   - 面向无 props、但需要 typed slots 的组件。
   - `Setup` 形状为 `VueTypedSlotSetupCallback<TSlots>`。
   - `DefineComponent(...)` 返回 `IVueSlotComponent<TSlots>`。
   - `H(component, slots)` 可直接走 typed slot-only authoring 路径，不需要占位 props 类型。

`TProps` 的自动推断规则当前为：

- 只收集公共实例属性；
- 命名继续走 `Description("@#...")` / `ECMAScriptNameAttribute` 的统一映射；
- 继承链按 base-first 稳定展开并按最终公开名去重；
- 即使 `TProps` 没有任何公共属性，也会稳定发出 `props: []`，不会静默省略；
- 一旦显式设置 `PropNames`，显式声明优先，不再做推断补写。
- `EmitNames` 的自动推断只接受稳定字符串字面量事件名；如果 `context.Emit(...)` 的第一个参数不是可静态确定的非空字符串，应显式设置 `EmitNames`。

这些推断规则不是编译器里的 Vue 名字硬编码，而是由 `ECMAScript.Contract.PropsAttribute` / `ECMAScript.Contract.EmitsAttribute` 直接声明推导入口；需要偏移来源时，分别通过 `TypeArgumentIndex` 和 `SourceMemberName` 配置。

`VueSetupContext` 当前暴露：

- `Emit(...)`
- `Expose(...)`
- `Attrs`
- `Slots`

`H(...)` 的 component authoring 现在还支持显式 slot object：

- `H(tagOrComponent, singleVNodeChild)` 可直接表达单个 vnode child，不必再手动包 `IVNode[]`；
- `H(...)` 的常用 children authoring 已切到 overload-first：`string`、`Number`、`bool`、`IVNode`、`IVNode[]` 直接用 C# 重载表达，不再依赖 `Either<...>`；
- 当目标是 `component` 时，这些 child overload 都会按 default slot authoring 收敛；编译输出会保持 `component / props / child` 左到右、单次求值，再映射成标准 Vue slot 形状；
- 这层 default-slot sugar 不通过外部库侧 `[Jazor]` / `Op.Compile` 声明驱动，而是由编译器基于既定 `H(...)` overload surface 做内部识别；
- 当目标是 `IVueSlotComponent<TSlots>` 或 `IVueComponent<TProps, TSlots>` 时，implicit child/default-slot sugar 只在 `TSlots` 暴露且仅暴露一个 parameterless default slot 时成立；缺少 default slot、声明了多个 default slot，或 default slot 使用 `VueSlotCallback<TScope>` 时，必须回到显式 `H(component, slots)` / `H(component, props, slots)` authoring；
- typed default slot 通过 slot 属性最终映射名是否为 `default` 识别；推荐直接用 `Description("@#default")` 明确声明；
- `H(component, slots)`
- `H(component, props, slots)`
- 若 `component` 是 `IVueSlotComponent<TSlots>`，可直接使用 typed `H(component, slots)` overload；
- 若 `component` 是 `IVueComponent<TProps, TSlots>`，可直接使用 typed `H(component, props, slots)` overload；
- `slots` 使用 `VueSlots` record surface 建模；
- 每个 slot 属性继续走 `Description("@#...")` 名称映射；
- slot 回调使用 `VueSlotCallback` / `VueSlotCallback<TScope>`；
- 这层是“写侧” authoring contract，和 `VueSetupContext.Slots` 的“读侧”运行时 bag 保持分离。

这层合同的目标是让 emitted JS 维持标准 Vue 3 组件形状，而不是引入 Jazor 私有运行时包装。

### `VueObject` / `VueObject<TProps>`

除组件定义对象外，`Vue3.cs` 还提供了面向 `h(...)` props / root props authoring 的 record surface：

- `VueObject`
- `VueObject<TProps>`

它们本质上不是新的 runtime shape，而是普通 `record` structural lowering 的便捷 authoring contract：

- `VueObject` 继承自 `VueProps`，因此仍然走 record -> plain JS object 的既有 lowering；
- `Class`、`Style`、`Id`、`Title` 提供常用 Vue/DOM props 的强类型入口；
- `Attrs`、`Dataset`、`Raw` 通过 `[Spread]` 直接扁平化进当前对象；
- `VueObject<TProps>.Props` 也通过 `[Spread]` 扁平化 typed props bag；
- `Dataset` 当前不提供额外 prefix/format 规则，属性名应直接映射到最终 `data-*` key，例如 `Description("@#data-user-id")`。

也就是说，`VueObject` 不是“编译器里的 Vue 特路”，而是建立在通用 record structural lowering 之上的 authoring sugar。

### `[Spread]` 语法糖

`ECMAScript.SpreadAttribute` 是通用 record property flattening 标记，不是 Vue 专属特性。

当前规则是：

- 仅对参与 structural lowering 的 record 实例属性生效；
- 若被标记成员 lower 后是 object literal，会内联展开其成员；
- 若不是 object literal，则生成标准 JS `...expr`；
- 不能与显式 JS 属性命名约定同时使用，因为两者语义自相矛盾；
- 这层能力的目标是“把一个 record 成员 flatten 到外层对象”，而不是引入新的 runtime 类型系统。

### record 的静态 `null` 省略

对 structural-lowered record，编译器会做一个刻意的静态优化：

- 若 record 主构造参数的实参能被 Roslyn 静态证明为 `null`，该成员不生成；
- 若 record object initializer 的赋值值能被 Roslyn 静态证明为 `null`，该成员不生成；
- 这条规则只针对静态可证明的 `null`，不会把一般运行时 `null` 检查扩展成隐式省略协议。

因此：

- 未赋值或静态 `null` 的 authoring 成员可以被干净省略；
- 非常量 `null` 流值仍会按普通值成员生成；
- 这条优化适用于通用 record structural lowering，不是 `VueObject` 专属行为。

## Boundaries

- `ECMAScript` 不承载 RazorVue 描述符提取、组件目录生成或 Roslyn 分析逻辑；这些能力位于 `Jazor.Common` / `Jazor.Analyzer`。
- `ECMAScript` 不负责产物物化；`.mjs` / source map / manifest 输出由 `Jazor.Emit` 负责。
- `ECMAScript` 不应引入与命名空间无关的额外依赖污染其公共 host surface。

## Key Files

- `Global.cs`: JavaScript 全局对象与基础函数投影。
- `Vue3.cs`: Vue 运行时 authoring 合同。
- `internal/Either.cs`: JS union-like host contract。

## Read Next

- [../Jazor.Common/README.md](../Jazor.Common/README.md)
- [../ECMAScript.Vuetify/README.md](../ECMAScript.Vuetify/README.md)
- [../../docs/01-目标/razorvue/README.md](../../docs/01-目标/razorvue/README.md)
