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
| `ECMAScript.Lucide` | `lucide-vue-next` 的完整生成式 RazorVue/Blazor 图标 binding；按 named export 保持 tree shaking |
| `ECMAScript.Vuetify`、`ECMAScript.ElementPlus`、`ECMAScript.TDesign` | UI 组件库绑定 |
| `ECMAScript.Style` | 强类型、确定性的 CSS-in-JS |

这些包按需显式引用。绑定库声明 npm/JSR package identity 与标准 ESM specifier；最终宿主的 Emit 将依赖写入根 `package.json`，Deno 恢复 `node_modules`，项目配置的 JavaScript 构建工具从同一 `jazor/` 项目解析模块、样式和其他资源。

`ECMAScript.VueDataUi` 以 71 个组件 binding 对应上游 `vue-data-ui/vue-ui-*` entry，并为需要全局样式的入口声明标准 stylesheet import。图表模块、样式和 PDF export 的 `jspdf` 依赖由恢复后的 package 与项目构建工具按标准 ESM 图解析。

`ECMAScript.Lucide` 的每个图标类型直接对应 `lucide-vue-next` 的 named export，生成器同时保留 XML 文档、官方图标链接和 Razor 参数类型。应用只导入实际使用的图标，Deno/Vite 可按正常 ESM 图执行 tree shaking。

## Blazor framework 绑定

| 包/程序集 | 用途 | 交付边界 |
| --- | --- | --- |
| `Jazor.CLR` | Blazor framework CLR 类型的生成 module/doc、`[Jazor]` mapping、carrier 与 runtime helper | runtime JavaScript 由 `ECMAScript` 的 `src/ECMAScript/clr/**` 源码提供；唯一 CLR mapping owner |

`Jazor.CLR` 面向 Blazor framework CLR mapping 与 runtime helper；所有进入 runtime-sensitive lowering 的 Blazor 类型都先由 `Jazor.CLR.Generator` 从真实 reference symbol 生成 module/doc，再由 `Jazor.CLR` 完善。核心 runtime JavaScript 从 `src/ECMAScript/clr/**` 写入 `jazor/clr/**`；其他 `ECMAScriptModule` 生成结果按各自模块路径写入项目。Vue listener/component framing 仍由 `Jazor.Vue`/`Jazor.RazorVue` 负责。

## 名称与作者契约

作者声明的 C# 符号保持原有名称。JavaScript ABI 所需的名称差异通过成员级 `ECMAScriptName` 或约定的元数据显式声明，并以 metadata 解析。

Razor 组件的参数、事件、双向绑定和 slot 由正常 C# 与 Razor 类型系统表达。JavaScript ABI 名称差异通过显式映射声明。

项目级 API 和示例位于各绑定项目的 README；包选择与配置见 [安装与配置](../03-guides/installation-and-configuration.md)。
