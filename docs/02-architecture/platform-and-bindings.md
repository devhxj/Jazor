# 平台与绑定

> 适用范围：ECMAScript 宿主契约、Vue 生态绑定与强类型 authoring surface。

## 宿主契约

`ECMAScript` 与 `ECMAScript.Contract` 定义可被编译器理解的 JavaScript 宿主类型和标注。公共 API 应优先以明确的 C# 参数、返回类型、重载或闭合 union 表达作者可用的值域；JavaScript 的动态性不是使用 `object` 或无约束泛型弱化 C# 契约的理由。

泛型参数、集合元素与数组元素通常只作为擦除的类型注解。只有具体类型被实例化、执行运行时类型检查或直接访问成员时，编译器才要求它具备可用运行时映射。

## Vue 生态

| 包 | 用途 |
| --- | --- |
| `ECMAScript.Vue` | Vue 3 核心类型、响应式 API、组件与 render-function authoring |
| `ECMAScript.VueContract` | 组件、props、事件、slot 与注入契约 |
| `ECMAScript.VueRoute` | Vue Router 类型绑定 |
| `ECMAScript.Pinia` | Pinia 状态管理绑定 |
| `ECMAScript.Vuetify`、`ECMAScript.ElementPlus`、`ECMAScript.TDesign` | UI 组件库绑定 |
| `ECMAScript.Style` | 强类型、确定性的 CSS-in-JS |

这些包按需显式引用。浏览器模块、样式、许可证和资源 manifest 由包与 Emit 管线管理，应用不应为了使用这些绑定再引入重复的 CDN、`node_modules` 或远程裸模块 import。

## 名称与作者契约

未映射的 C# 符号保持作者声明的名称。JavaScript ABI 所需的名称差异必须通过成员级 `ECMAScriptName` 或约定的元数据显式声明，不依赖大小写、`OnX`、`Changed`、`Content` 等约定反推。

Razor 组件的参数、事件、双向绑定和 slot 首先由正常 C# 与 Razor 类型系统表达。只有 C# 无法表示的 JavaScript 名称才需要显式映射。

项目级 API 和示例位于各绑定项目的 README；包选择与配置见 [安装与配置](../03-guides/installation-and-configuration.md)。
