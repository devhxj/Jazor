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
   - 通过 `EmitNames` 明确声明运行时 `emits` 合同。
   - `Setup` 形状为 `VueTypedSetupCallback<TProps>`，对应 Vue 的 `setup(props, context)`.

`TProps` 的自动推断规则当前为：

- 只收集公共实例属性；
- 命名继续走 `Description("@#...")` / `ECMAScriptNameAttribute` 的统一映射；
- 继承链按 base-first 稳定展开并按最终公开名去重；
- 即使 `TProps` 没有任何公共属性，也会稳定发出 `props: []`，不会静默省略；
- 一旦显式设置 `PropNames`，显式声明优先，不再做推断补写。

这条推断规则不是编译器里的 Vue 名字特判，而是通过 `ECMAScript.Contract.PropsAttribute` 在 host contract 上声明。

`VueSetupContext` 当前暴露：

- `Emit(...)`
- `Expose(...)`
- `Attrs`
- `Slots`

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
