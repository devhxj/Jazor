# Jazor 文档

用 Jolt 编写 `.jazor` 组件，在同一文件里使用 Vue 模板和 C#，并从项目的 npm 依赖引入组件。Jazor SDK 也可以将独立 C# 模块编译为 ECMAScript。

## 开始使用

在 Visual Studio 打开 `samples/Jero/src/Jero.slnx` 并启动 Jero.Web，或在 Web 项目目录运行 `dotnet run`。编辑器扩展为 `.jazor` 提供补全和导航。

| 我想做什么 | 从这里开始 |
| --- | --- |
| 运行第一个 `.jazor` 应用 | [Jolt 作者指南](./03-guides/jolt-authoring.md) · [Jero](../samples/Jero/README.md) · [小型 ASP.NET Core 计数器](../samples/Jolt.AspNetCore/README.md) |
| 配置语义高亮、补全和导航 | [VS Code](../editors/jolt/README.md) · [Visual Studio 2026](../editors/jolt-visual-studio/README.md) |
| 核对当前能力和验证命令 | [当前状态](./04-roadmap/current-status.md) · [开发与测试](./03-guides/development-and-testing.md) |

## 编译 C# 模块

在 .NET 项目中引用 Jazor SDK，将带 `[ECMAScriptModule]` 的 C# 类编译成 ECMAScript 模块。

| 我想做什么 | 从这里开始 |
| --- | --- |
| 编译独立 C# 模块 | [C# 模块快速开始](./03-guides/quick-start.md) |
| 配置 MSBuild 输出、绑定包或 ASP.NET Core SSR | [安装与配置](./03-guides/installation-and-configuration.md) |
| 升级旧项目或查看版本变更 | [NodeApi 工具链与迁移](./03-guides/toolchain-migration.md) · [更新日志](../CHANGELOG.md) |

## 文档目录

查阅产品范围、编译器与宿主架构，以及仓库的测试和发布流程。

| 目录 | 内容 |
| --- | --- |
| [产品概览](./01-overview/README.md) | 支持范围、模块职责和系统架构 |
| [架构说明](./02-architecture/README.md) | 编译器、产物管线、类库契约和宿主绑定 |
| [使用与维护指南](./03-guides/README.md) | 作者写法、样例、配置和维护流程 |
| [路线图](./04-roadmap/README.md) | 当前状态和后续验收 |
| [历史演进](./05-history/evolution.md) | 已完成的工作和版本背景 |
