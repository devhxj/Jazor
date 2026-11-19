# 示例

> 面向：希望从可运行项目理解 Jazor 组合方式的读者。

示例用于展示集成方式和提供真实验证场景，不替代核心 API 或库的公共契约。阅读或修改示例时，应先确定它验证的是核心能力、框架集成还是应用层选择。

| 路径 | 作用 | 边界 |
| --- | --- | --- |
| `samples/Jazor.MultiProject/` | 多项目 ECMAScript 模块发射 | 核心 C# -> ECMAScript 路径 |
| [Jolt Counter](../../samples/Jolt.Counter/README.md) | `.jazor` 组件与 npm 包 | Reka UI、父子组件回调、开发服务与生产构建 |
| `samples/ECMAScript.Pinia.Counter/` | Vue 3 与 Pinia 状态管理 | Pinia 绑定与浏览器运行时 |
| `samples/ECMAScript.Vue.Devtools.Plugin/` | Vue Devtools plugin authoring | custom inspector、timeline 和 typed settings |
| `samples/ECMAScript.VueRoute.MemorySmoke/` | Vue Router 模块与浏览器验证 | Router 绑定与导航运行时 |
| `samples/Wiki/` | 端到端示例应用 | 使用与验证参考 |

部分示例拥有专用的构建、浏览器或 smoke 脚本。可从 [scripts/csharp README](../../scripts/csharp/README.md) 查找当前验证入口；`bin/`、`obj/`、`node_modules/`、`.tmp/` 和测试结果归属构建与验证输出。
