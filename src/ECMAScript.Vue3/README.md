# ECMAScript.Vue3

`ECMAScript.Vue3` 是独立外部库项目，不属于 `ECMAScript` 平台内核模块。  
它作为“官方第一个外部库映射样例”，展示如何在不增加 compiler 框架白名单特路的前提下，通过 C# 类型系统与通用特性完成 Vue 3 绑定。

## Design Boundary

- 不依赖 `Jazor` 专属特性做外部库语义（除平台通用映射特性外）。
- 不在 compiler 中硬编码 `ECMAScript.Vue3` 命名空间规则。
- 运行时映射通过通用 `[ECMAScript]` / `[Description]` / `[ECMAScriptInline]` 等机制表达。

## File Layout (Partial Class Pattern)

- `Vue3.cs`
  - 入口文件，仅保留 `Vue3` 的模块映射特性与顶层委托/handle 类型。
- `Api/Vue3.Api.cs`
  - App/Component/CustomElement/Builtin Component/VNode Utility 等核心 API。
- `Api/Vue3.Api.Render.cs`
  - `BindThis(...)` 与 `H(...)` overload 家族（渲染构建面）。
- `Api/Vue3.Api.Reactivity.cs`
  - `reactive/ref/computed/watch/...` 响应式 API。
- `Api/Vue3.Api.Composition.cs`
  - `useAttrs/useSlots/useTemplateRef/useModel/useHost/...` 组合式 API。
- `Api/Vue3.Api.Lifecycle.cs`
  - `onMounted/onUpdated/onErrorCaptured/...` 生命周期与 scope/hook API。
- `Types/Vue3.Types.*.cs`
  - `Vue3` 嵌套类型分组（Core/Props/Component/Directive/PluginApp/Structural/ReactivityOptions）。

该拆分策略的目标是：

- 保持 API 可读性与发现性；
- 控制单文件复杂度；
- 为后续外部库（如状态库、路由库、UI 库）提供可复制模板。

## Mapping Rules (Summary)

- C# 类型系统优先：`record`/`generic`/`overload`/`nullable`/`delegate`。
- `Either` 用于真实 union 桥接；方法边界优先 overload 体验。
- props/object 字面量遵循通用 record lowering，不额外引入 Vue 专用 compiler 特路。
- `VueObject` 承载 Vue 核心 props 与一组高频原生 HTML convenience attrs；长尾属性继续通过 `Attrs` / indexer / typed props bag 表达。
- `UseModel(...)` 返回 `VueModelRef<TValue>`，`.Value` 对应 `model.value`，modifiers 通过 `GetModifiers()` / `GetModifiers<T>()` 读取。
- Options API object-form inject 可通过 `VueInjectOptions<T>` / `VueInjectEntry<T>` / `VueInjectRegistry<T>` 表达，不新增 compiler 特路。
- `[Spread]` 等语法糖由通用属性机制驱动，不绑定 Vue 命名空间。

## Roadmap (3 Phases)

1. Phase 1 (`H` mapping): 建立 `H(...)` / `VueObject` / slot contract 的稳定映射层（当前已完成主体收口）。
2. Phase 2 (Razor -> `H`): 把 Razor authoring 映射到上述规范层，收敛 canonical shape 与诊断边界。
3. Phase 3 (Jolt): 在 Jolt 中承接工程化 authoring/build/debug，但不把 Vue 语义反向硬编码进 compiler。
