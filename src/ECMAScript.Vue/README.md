# ECMAScript.Vue

> 定位：独立的 Vue 3 强类型 C# binding，也是框架无关 Jazor 核心上的第一个外部库映射样例。

该项目通过通用 C# 类型系统和 `[ECMAScript]`、`[Description]`、`[ECMAScriptInline]` 等映射表达 Vue 3 API，不在 compiler 中硬编码 `ECMAScript.Vue` 专用规则。

## 代码结构

- `Vue.cs`：模块映射特性与顶层委托/handle 类型。
- `Api/Vue.Api.cs`：App、component、custom element、VNode utility 等 API。
- `Api/Vue.Api.Render.cs`：`BindThis(...)` 与 `H(...)` overload。
- `Api/Vue.Api.Reactivity.cs`：`reactive`、`ref`、`computed`、`watch` 等响应式 API。
- `Api/Vue.Api.Composition.cs`：composition API。
- `Api/Vue.Api.Lifecycle.cs`：生命周期、scope 与 hook API。
- `Types/Vue.Types.*.cs`：嵌套 runtime shape 与 options contract。

## 映射与 authoring 规则

- 默认保留 Vue 官方 API 词根，只做 C# 命名投影；runtime ABI 差异通过显式 `Description("@#...")` 或 `ECMAScriptName` 声明。
- props、emits、slots 和组件上下文必须以明确 C# contract 表示，避免 `object` / `object?` 兜底。
- 类型擦除不是降低 authoring contract 的理由。已能由 assignment、implicit conversion、overload 或 native `union` 表达的场景，不新增弱 factory。
- 组件/生命周期/响应式 API 只定义 host binding；Razor-to-Vue 的组件绑定与 render-function framing 不属于本项目。

## 交付边界

Vue browser runtime 由 `Jazor.Vue` 包内的本地资源与 manifest 提供。应用不需要自行引入 CDN 或将 Vue 安装到项目 `node_modules`；输出模式和实际物化仍由 `Jazor.Emit` 持有。`ECMAScript.Vue` 源项目作为 `Jazor.Vue` 的 payload 输入，不单独作为应用安装包发布。

`@vue/devtools-api` 是 Vue Router、Pinia 和可选 `ECMAScript.Vue.Devtools` 的本地 logical import。它与浏览器安装的 Vue Devtools 扩展通过官方插件桥接协作；仅在需要自定义 inspector、timeline、component hook、custom tab 或 command 时引用 `ECMAScript.Vue.Devtools`，普通应用和 Pinia 自动注册不需要直接调用它。

## 相关文档

- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
- [框架集成层](../../docs/02-architecture/framework-integrations.md)
