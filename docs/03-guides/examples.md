# 示例

> 面向：希望从可运行项目理解 Jazor 组合方式的读者。

示例用于展示集成方式和提供真实验证场景，不替代核心 API 或库的公共契约。阅读或修改示例时，应先确定它验证的是核心能力、框架集成还是应用层选择。

| 路径 | 作用 | 边界 |
| --- | --- | --- |
| `samples/Jazor.MultiProject/` | 多项目 ECMAScript 模块发射 | 核心 C# -> ECMAScript 路径 |
| `samples/RazorVue.TodoList/` | Razor-to-Vue 组件与宿主组合 | 当前 RazorVue 集成 |
| `samples/ECMAScript.Pinia.Counter/` | Vue 3 与 Pinia 状态管理 | Pinia 绑定与浏览器运行时 |
| `samples/ECMAScript.Vue.Devtools.Plugin/` | Vue Devtools plugin authoring | custom inspector、timeline 和 typed settings |
| `samples/ECMAScript.VueDataUi.Dashboard/` | Vue Data UI Razor dashboard | typed dataset/config、per-chart ESM import 与本地 NuGet consumer 验证 |
| `samples/ECMAScript.VueRoute.MemorySmoke/` | Vue Router 模块与浏览器验证 | Router 绑定与导航运行时 |
| `samples/Wiki/` | 端到端示例应用 | 使用与验证参考，不是独立产品线 |
| `samples/JazorAdmin/` | 生产级管理应用参考（门户/IAM/运营） | 消费 `Jazor.Admin`，不定义库 API |
| `samples/JazorAdmin.DemoClient/` | 独立 confidential OIDC 下游客户端 | 授权码 + PKCE、Bearer API 与单点登出；不承载下游业务权限模型 |
| `samples/JazorAdmin.Test/` | JazorAdmin 的 API 与审计回归测试 | 随示例演进，不定义库 API |

`Jazor.Admin` 的公共契约位于 `src/Jazor.Admin/`；`samples/JazorAdmin` 可以选择具体的应用框架、业务功能和 UI 组合。不得从示例的页面、认证或部署策略反推出 `Jazor.Admin` 库的必需行为。

部分示例拥有专用的构建、浏览器或 smoke 脚本。可从 [scripts/csharp README](../../scripts/csharp/README.md) 查找当前验证入口；不要把示例产生的 `bin/`、`obj/`、`node_modules/`、`.tmp/` 或测试结果当作文档或源代码。
