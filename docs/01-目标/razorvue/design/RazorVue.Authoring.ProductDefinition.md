# RazorVue 创作产品定义


## 1. 目的

本文档定义 RazorVue 的创作产品方向。

目标是让 RazorVue 对 C# 开发者感觉自然，同时仍然编译为 Vue 优先的运行时输出。

## 2. 定位

RazorVue v1 是：

类 Blazor 创作，Vue 优先运行时。

这意味着：

- 创作者保持在 Razor 和 C# 中
- 运行时输出保持标准 Vue ESM
- Vue 生态系统库通过 C# 友好的包装器使用
- 创作模型对 Blazor 用户保持熟悉

RazorVue v1 不是：

- Vue SFC 替代品
- Volar 等效项目
- 通用多框架 UI 抽象
- 面向业务创作者的完整 Vue Composition API 表面

## 3. 目标用户

RazorVue v1 面向的开发者：

- 主要在 C# 和 Razor 中工作
- 希望保持 `.razor + .razor.cs` 作为主要创作表面
- 希望访问 Vue 运行时和生态系统，无需切换到 TS/SFC 工作流
- 偏好 Blazor 风格的组件创作语义

## 4. 核心承诺

RazorVue v1 应该让创作者：

- 使用 `[Parameter]`、`EventCallback` 和 `RenderFragment` 编写组件
- 以类 Blazor 方式使用 `@bind-*`
- 通过 C# 组件包装器使用选定的 Vue 库
- 生成标准 Vue 工件，无需手动 JS/TS 包装器代码

## 5. 创作模型

主要创作模型保持：

- `.razor + .razor.cs`
- `[Parameter]`
- `EventCallback` / `EventCallback<T>`
- `RenderFragment` / `RenderFragment<T>`
- `@bind-*`
- 熟悉的生命周期方法，如 `OnInitialized*`、`OnParametersSet*` 和 `OnAfterRender*`

业务创作者不应该需要考虑：

- `defineComponent`
- `setup`
- `h`
- 原始导入说明符
- 主机插件安装细节

## 6. 生态系统策略

Vue 生态系统包应该通过稳定的 C# 友好契约集成：

`C# 存根 + 描述符 + 主机要求`

这意味着：

- 创作者看到正常的 C# 组件
- 编译器看到稳定的库描述符
- 主机看到明确的运行时要求

此策略的第一个生态系统目标是 Vuetify。

## 7. 体验目标

RazorVue v1 应该优先考虑：

- 组件标签完成
- 参数完成
- 参数类型检查
- 导航到组件和参数定义
- 绑定目标验证
- 插槽名称验证
- 插槽上下文验证
- 从 Razor/C# 角点表述的诊断

## 8. 非目标

RazorVue v1 不尝试：

- 匹配 Vue SFC + Volar 语义
- 向业务创作者暴露完整的 Vue Composition API
- 支持所有 Razor 和 Vue 语法及运行时组合
- 解决完整的样式层创作语义
- 一次深度集成多个生态系统包

## 9. 运行时边界

RazorVue 编译器层负责：

- 语义提取
- 描述符构造
- 渲染树降低
- Vue 工件生成
- 面向主机的依赖和插件要求声明

主机负责：

- 依赖解析
- 插件安装
- 最终打包
- 运行时组装

## 10. 成功标准

RazorVue v1 成功，如果：

- C# 开发者可以在 Razor 中构建典型 UI 流程
- 至少一个 Vue UI 库闭合创作到运行时循环
- 生成的输出保持标准 Vue ESM
- 业务创作者不需要编写自定义 JS/TS 包装器
- 设计时诊断是早期、有用且面向 C# 的
