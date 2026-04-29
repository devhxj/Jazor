# ECMAScript

> Status: active reference
> Positioning: core JavaScript host projection assembly for Jazor module authoring.

`ECMAScript` 提供 Jazor 的基础 JavaScript 运行时投影，包括：

- 全局宿主表面（`Global.cs`）
- Vue 运行时 authoring surface（`Vue.cs`）
- `Either<...>`、DOM、Web API 等核心契约

## Responsibilities

- 把 JavaScript / Web API 表面以稳定的 C# host contract 形式暴露出来。
- 保持 `ECMAScript("npm:...")` / `ECMAScriptModule("...")` 上声明的包 specifier 与模块路径原样进入 emitted JS。
- 为上层模块 authoring 提供 Vue `createApp` / `defineComponent` / `h` / `ref` / `computed` 等编译时可投影 API。

## Vue Authoring Surface

`Vue.cs` 当前提供两条组件 authoring 通道：

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

这些推断规则不是编译器里的 Vue 名字硬编码，而是通过统一的 `ECMAScript.Contract.RecordLiteralContractAttribute` 核心模型分发，并由 `ECMAScript.Contract.PropsAttribute` / `ECMAScript.Contract.EmitsAttribute` 作为声明侧薄封装暴露出来。

`VueSetupContext` 当前暴露：

- `Emit(...)`
- `Expose(...)`
- `Attrs`
- `Slots`

`H(...)` 的 component authoring 现在还支持显式 slot object：

- `H(tagOrComponent, singleVNodeChild)` 可直接表达单个 vnode child，不必再手动包 `IVNode[]`；
- 当目标是 `component` 时，`IVNode` child 以及 `Either<string, Number, bool, IVNode, IVNode[]>` children 都会按 default slot authoring 收敛；编译输出会保持 `component / props / child` 左到右、单次求值，再映射成标准 Vue slot 形状；
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

## Boundaries

- `ECMAScript` 不承载 RazorVue 描述符提取、组件目录生成或 Roslyn 分析逻辑；这些能力位于 `Jazor.Common` / `Jazor.Analyzer`。
- `ECMAScript` 不负责产物物化；`.mjs` / source map / manifest 输出由 `Jazor.Emit` 负责。
- `ECMAScript` 不应引入与命名空间无关的额外依赖污染其公共 host surface。

## Key Files

- `Global.cs`: JavaScript 全局对象与基础函数投影。
- `Vue.cs`: Vue 运行时 authoring 合同。
- `internal/Either.cs`: JS union-like host contract。

## Read Next

- [../Jazor.Common/README.md](../Jazor.Common/README.md)
- [../ECMAScript.Vuetify/README.md](../ECMAScript.Vuetify/README.md)
- [../../docs/01-目标/razorvue/README.md](../../docs/01-目标/razorvue/README.md)
