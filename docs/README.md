# Jazor 文档中心

本目录是 Jazor 的中文技术文档入口。文档按照读者的阅读目的组织：先了解产品与架构，再选择使用或维护路径；当前计划与历史记录不会混入产品契约。

## 推荐阅读

| 读者与目标 | 建议入口 |
| --- | --- |
| 第一次使用 Jazor | [安装与配置](./03-guides/installation-and-configuration.md) -> [快速开始](./03-guides/quick-start.md) |
| 在 Razor 项目中启用 Vue | [Razor-to-Vue 架构](./02-architecture/razor-to-vue.md) -> [包配置](./03-guides/installation-and-configuration.md) |
| 理解 Jazor 核心 | [系统架构](./01-overview/system-architecture.md) -> [编译器](./02-architecture/compiler.md) -> [产物管线](./02-architecture/artifact-pipeline.md) |
| 设计多项目类库与资源交付 | [类库产物与引用契约](./02-architecture/library-artifact-contract.md) |
| 在核心平台上使用框架集成、Vue 生态、SSR 或管理壳 | [框架集成层](./02-architecture/framework-integrations.md) -> [Razor-to-Vue](./02-architecture/razor-to-vue.md) -> [平台与绑定](./02-architecture/platform-and-bindings.md) |
| 参与仓库开发 | [开发与测试](./03-guides/development-and-testing.md) -> [文档规范](./03-guides/documentation-style.md) |
| 查找样例 | [示例](./03-guides/examples.md) |

## 目录说明

| 目录 | 责任 |
| --- | --- |
| [`01-overview`](./01-overview/README.md) | 产品定位、阅读地图与系统全景 |
| [`02-architecture`](./02-architecture/README.md) | 当前架构、模块职责和稳定边界 |
| [`03-guides`](./03-guides/README.md) | 安装、配置、编写、开发与验证指南 |
| [`04-roadmap`](./04-roadmap/README.md) | 当前工作方向和状态说明 |
| [`05-history`](./05-history/README.md) | 已退役路线与关键演进背景 |

## 文档原则

- 当前行为以源码、自动化测试和本目录的架构文档为准。
- 实现细节优先维护在源码旁的 README 或项目文档中；本目录只保留跨模块契约、使用方式和必要的阅读入口。
- 计划、测试结果和审计结论只在其有效范围内陈述，不在长期设计文档中重复快照数据。
- 版本变更记录位于仓库根目录的 [CHANGELOG.md](../CHANGELOG.md)。
