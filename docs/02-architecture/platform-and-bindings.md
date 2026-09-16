# 平台与绑定

> 适用范围：ECMAScript 宿主契约、Vue 生态绑定与强类型 authoring surface。

## 宿主契约

`ECMAScript` 与 `ECMAScript.Contract` 定义可被编译器理解的 JavaScript 宿主类型和标注。公共 API 应优先以明确的 C# 参数、返回类型、重载或闭合 union 表达作者可用的值域；JavaScript 的动态性并不构成以 `object` 或无约束泛型弱化 C# 契约的理由。

泛型参数、集合元素与数组元素通常只作为擦除的类型注解。只有具体类型被实例化、执行运行时类型检查或直接访问成员时，编译器才要求它具备可用的运行时映射。

## Vue 生态

| 包 | 用途 |
| --- | --- |
| `ECMAScript.Vue` | Vue 3 核心类型、响应式 API、组件与 render-function authoring |
| `ECMAScript.VueContract` | 组件、props、事件、slot 与注入契约 |
| `ECMAScript.VueRoute` | Vue Router 类型绑定 |
| `ECMAScript.Pinia` | Pinia 状态管理绑定 |
| `ECMAScript.Pinia.Testing` | `@pinia/testing` 绑定：测试期 Pinia root、spy 与 initial-state 的 authoring contract |
| `ECMAScript.Vue.Devtools` | Vue Devtools Plugin API 绑定：custom inspector、timeline、component hook、tab 与 command |
| `ECMAScript.VueDataUi` | `vue-data-ui` 3.23.4 binding：完整 71 个公开 `Vd*` 组件、强类型 dataset/config 与按组件 ESM entry |
| `ECMAScript.VuIcons` | `vu-icons` 1.5.4 binding：完整 1,821 个 `Vu*` 图标组件与闭合动态 icon enum |
| `ECMAScript.Vuetify`、`ECMAScript.ElementPlus`、`ECMAScript.TDesign` | UI 组件库绑定 |
| `ECMAScript.Style` | 强类型、确定性的 CSS-in-JS |

这些包按需显式引用。浏览器模块、样式、许可证和其他资源由各包的 `manifest.json + dist/**` 与显式 module/package dependency 管理；最终宿主的 Emit 物化所选闭包。应用通过 package manifest 管理资源来源。

`ECMAScript.VueDataUi` 以 71 个组件 binding 对应上游 `dist/components/vue-ui-*.js`，直接声明 `vue-data-ui/vue-ui-*` entry。`Jazor.Emit` 从实际 generated import 沿 manifest 的显式 module/package edge 物化相对 ESM closure；图表 chunk 按实际 entry 进入发布输出。PDF export runtime 的 entry 解析同包携带的本地 `jspdf`。

`ECMAScript.VuIcons` 同时提供静态与动态路径。已知图标应直接使用生成的 `VuUser` 等 component，其 binding 指向独立 `vu-icons/VuUser` entry，Emit 仅物化该 SVG module、共享 runtime 和样式；运行时名称选择使用 `VuIcon` 与 `VuIconName`，它需要完整 `icons-data.js` catalog 才能解析任意名称。这是运行时动态性的必要载荷，独立于静态路径而存在。

## Blazor framework 绑定

| 包/程序集 | 用途 | 交付边界 |
| --- | --- | --- |
| `Jazor.CLR` | Blazor framework CLR 类型的生成 module/doc、`[Jazor]` mapping、carrier 与 runtime helper | runtime JavaScript 由 `ECMAScript` 的 `manifest.json + dist/**` 提供；唯一 CLR mapping owner |

`Jazor.CLR` 面向 Blazor framework CLR mapping 与 runtime helper；第二个 Razor renderer 的角色并不在其职责之内。所有进入 runtime-sensitive lowering 的 Blazor 类型都先由 `Jazor.CLR.Generator` 从真实 reference symbol 生成 module/doc，再由 `Jazor.CLR` 完善；生成的 runtime JavaScript 作为 `ECMAScript` JS resource library 的 manifest/dist 内容交付。Vue listener/component framing 仍由 `Jazor.Vue`/`Jazor.RazorVue` 负责。

## 名称与作者契约

作者声明的 C# 符号保持原有名称。JavaScript ABI 所需的名称差异通过成员级 `ECMAScriptName` 或约定的元数据显式声明，并以 metadata 解析。

Razor 组件的参数、事件、双向绑定和 slot 由正常 C# 与 Razor 类型系统表达。JavaScript ABI 名称差异通过显式映射声明。

项目级 API 和示例位于各绑定项目的 README；包选择与配置见 [安装与配置](../03-guides/installation-and-configuration.md)。
