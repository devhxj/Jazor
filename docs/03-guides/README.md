# 使用与维护指南

本目录面向两类读者：使用 Jazor 构建应用的开发者，以及维护 Jazor 仓库的贡献者。每份指南只说明当下可执行的路径，让读者从配置、编写到验证都能找到明确入口，而不承担历史背景或阶段任务记录。

| 文档 | 面向读者 | 说明 |
| --- | --- | --- |
| [安装与配置](./installation-and-configuration.md) | 应用开发者 | 包选择、MSBuild 输出、SSR 和可选绑定 |
| [快速开始](./quick-start.md) | 初次使用者 | 从 C# 模块到本地 `.mjs` 产物的最小路径 |
| [RazorVue 快速开始](./razorvue-quickstart.md) | RazorVue 应用开发者 | 从 typed CRUD 页面、TDesign 表单和应用自有 route host 开始，并附作者诊断与验证入口 |
| [RazorVue Golden Path](./razorvue-golden-path.md) | RazorVue 应用开发者 | 从推荐样本到独立 package consumer、Release 和浏览器验收的可复制路径 |
| [RazorVue 作者指南](./razorvue-authoring.md) | RazorVue 应用开发者 | 完整组件 C# 边界、`@code`/`.razor.cs`、direct-render 限制、JAZORVGA 诊断、Proxy-safe class 与升级门禁 |
| [RazorVue 诊断矩阵](./razorvue-diagnostic-matrix.md) | RazorVue 应用开发者与维护者 | 按作者场景查找稳定诊断、最小替代写法和验收规则 |
| [RazorVue 开发范式](../02-architecture/razorvue-paradigm.md) | RazorVue 应用开发者与架构设计者 | Razor/C# JSX-like 规则、支持决策等级、明确边界与 P0/P1 后续工作 |
| [RazorVue 范式调试](./razorvue-debugging.md) | RazorVue 应用开发者 | 从 `.razor` 追踪到 generated C#、render module 与 source map |
| [开发与测试](./development-and-testing.md) | 仓库维护者 | 构建、测试、覆盖率门槛与脚本约定 |
| [发版与版本规则](./release-and-versioning.md) | 仓库维护者 | 版本通道语义、1.0 条件、发版门禁与 CHANGELOG 规则 |
| [示例](./examples.md) | 应用开发者与维护者 | 示例项目的用途和验证范围 |
| [文档规范](./documentation-style.md) | 文档维护者 | 目录、命名、写作和历史资料规则 |

涉及框架集成时，请先阅读[框架集成层](../02-architecture/framework-integrations.md)，确认边界后再进入具体操作。
