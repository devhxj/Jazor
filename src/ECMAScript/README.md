# ECMAScript

> 定位：Jazor 的框架无关 JavaScript、Web API 与基础 host projection 程序集。

`ECMAScript` 将可支持的 JavaScript runtime surface 表达为稳定的 C# contract，并通过 `[ECMAScript]`、`[ECMAScriptModule]`、`[Description]` 和白名单映射进入 compiler。它不是 JavaScript 的弱类型镜像：未建模的 runtime 语义不能通过 `object` 或手写 import 绕过。

## 职责

- 提供 global host、DOM/Web API、命名 union 与基础 runtime contract。
- 保留 authoring 中声明的 npm specifier 与模块路径，按 compiler 的标准 import 路径发射。
- 提供 ECMAScript global、DOM/Web API、union 和 runtime contract；Vue authoring API 由 `ECMAScript.Vue`（随 `Jazor.Vue` 交付）提供。
- 为 record、`[Spread]` 与 typed object literal 提供可预测的结构化 lowering 契约。

## Host authoring 接口

Vue component、props、reactivity 和 render-function authoring 位于独立的 `ECMAScript.Vue` 项目，并随 `Jazor.Vue` 交付。`ECMAScript` 只提供通用 compiler/host contract；普通 ECMAScript 类库无需安装 Vue binding。

## 边界

- 本项目不为任何框架加入 compiler 专用分支；框架绑定使用通用 host mapping contract。
- `ECMAScript.Vue`、Vue Router、Pinia 与 UI 库是独立生态 binding，不应回流到本项目的通用核心。
- 编译器只接受有明确映射的外部类型和成员，并在 runtime-sensitive 使用点拒绝未支持语义。

## 关键区域

- `Global.cs`：全局 JavaScript host。
- `Vue.cs`：基础 Vue authoring contract。
- `DOM/`、`Web/`：浏览器与 Web API 投影。
- `Runtime/` 与 union 类型：可支持的 runtime value domain。

## 生成边界

WebIDL 生成内容由 `ECMAScript.WebIDL.Generator` 和已提交 inventory 管理。不要手工编辑明显由生成器拥有的文件；生成器与当前 inventory 才是可复现来源。

## 相关文档

- [ECMAScript.Vue](../ECMAScript.Vue/README.md)
- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
- [编译器架构](../../docs/02-architecture/compiler.md)
