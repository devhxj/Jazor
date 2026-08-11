# 管理壳库

> 适用范围：`Jazor.Admin` 包及其与 `samples/JazorAdmin` 的边界。

## `Jazor.Admin` 的职责

`Jazor.Admin` 是可发布的管理壳库，提供导航、面包屑、页面操作、布局模式、共享 authoring 基类和原生 RazorVue 管理壳组件。它的公共契约不泄露特定 UI 组件库的 props，应用可通过容器契约与 `VueInject` 选择具体实现。

库负责壳层结构和类型化交互，例如布局、导航、路由目标、受控折叠状态和通用 slot。表单、认证、业务页面、权限模型、应用配置和具体 UI 库组合属于应用层，不属于该包。

## 示例应用的职责

`samples/JazorAdmin` 是 dogfood 示例应用，用于验证管理壳库在真实 RazorVue、Vue Router、样式和浏览器环境中的组合效果。它可以包含业务功能和特定 UI 库选择，但这些内容不构成 `Jazor.Admin` 的公共 API 承诺。

文档或发布说明引用两者时应明确区分“库行为”和“示例行为”，不得以示例页面或应用策略替代库的架构说明。

## 使用方式

应用在已有 RazorVue 基础上显式引用 `Jazor.Admin`。版本应与 `Jazor` 和 `Jazor.Vue` 保持一致，具体包配置见 [安装与配置](../03-guides/installation-and-configuration.md)。库的完整 public surface 与组件说明见 [Jazor.Admin README](../../src/Jazor.Admin/README.md)。
