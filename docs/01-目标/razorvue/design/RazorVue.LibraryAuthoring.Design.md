# RazorVue 库创作设计

> 状态：活跃参考
> 定位：RazorVue 中活跃库创作车道的设计参考。

## 1. 目的

本文档定义第三方 Vue 库如何作为 C# 开发人员的一流创作表面进入 RazorVue。

设计目标是让 Vue 生态系统库作为正常的 Razor/C# 组件库出现，同时仍然编译为标准 Vue 运行时使用。

## 2. 现有基础

RazorVue 已经提供：

- 组件描述符
- 组件注册表和解析
- props/emits/slots 提取
- 库组件源种类
- Vue 工件降低
- 清单发送

此设计扩展这些机制，而不是替换它们。

## 3. 核心规则

库组件必须有一个创作真理源：

C# 存根类型。

存根服务于：

- Razor 创作
- IDE 工具
- 描述符提取
- 验证
- 组件解析

描述符数据必须从存根及其元数据派生，而不是作为单独的手动真理源维护。

## 4. 库组件模型

库组件由以下表示：

- `VueLibraryComponent`
- `VueLibraryComponentAttribute`
- `VueLibraryStyleAttribute`

库组件是一个 C# 类型，声明：

- Vue 运行时组件从哪里导入
- 使用哪个导出名称
- 需要哪些样式依赖

## 5. 描述符提取规则

如果组件类型具有 `VueLibraryComponentAttribute`，描述符提取必须生产：

- `SourceKind = LibraryComponent`
- 来自属性的 `ImportSpecifier`
- 来自属性的 `ExportName`
- 来自 `VueLibraryStyleAttribute` 的 `StyleDependencies`

所有 prop/emit/slot 提取仍应重用标准 RazorVue 规则：

- `[Parameter]` -> prop
- `EventCallback` -> emit
- `RenderFragment` -> slot
- `RenderFragment<T>` -> 作用域插槽参数

## 6. 发现规则

默认注册表创建必须包括：

- 内置组件
- 用户组件
- 从 `Compilation` 发现的库组件

正常用法不应需要外部注册表文件。

## 7. 解析规则

库组件遵循与用户组件相同的解析模型：

- 首先完全限定名称
- 内置名称保持保留
- 可见性由当前命名空间和 `using` 控制
- 歧义必须产生诊断，而不是启发式

## 8. 绑定和插槽规则

创作保持 C# 友好：

- 事件的 `EventCallback`
- 模型风格绑定的 `Xxx + XxxChanged`
- 作用域插槽的 `RenderFragment` / `RenderFragment<T>`

业务创作者不应编写原始 Vue 事件或插槽有效负载形状。

## 9. 清单和主机规则

库集成必须显式声明运行时要求。

至少，面向主机的层必须能够观察到：

- 导入
- 样式
- 插件要求

编译器声明它们。
主机消费它们。

## 10. 拒绝的方向

v1 拒绝以下方向：

- 分离的手动描述符真理
- 库特定的降低分支
- 原始 Vue 运行时 API 作为主要业务创作表面
- 绕过标准注册表和解析规则
