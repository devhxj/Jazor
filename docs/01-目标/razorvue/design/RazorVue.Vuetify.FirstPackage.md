# RazorVue Vuetify 第一包


## 1. 文档定位

本文档定义 Vuetify 的第一个 RazorVue 生态系统包形状。

它有意窄。
它描述第一个创作包，而不是完整的 Vuetify 表面积。

相关文档：

- [RazorVue.Overview.md](./RazorVue.Overview.md)
- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.ComponentDescriptorSpec.md](./RazorVue.ComponentDescriptorSpec.md)
- [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)
- [RazorVue.ImplementationChecklist.md](../../../02-计划/jolt/razorvue-implementation/RazorVue.ImplementationChecklist.md)

## 2. 目的

`Jazor.RazorVue.Vuetify` 是 RazorVue 库创作模型的第一个具体测试。

其目的是证明 Vue 生态系统库可以作为正常的 Razor 组件库暴露给 C# 创作者，同时仍然降低为标准 Vue 运行时导入。

包必须支持：

- C# 友好的组件创作
- 描述符驱动的降低
- 显式样式依赖
- 面向主机的插件要求

## 3. 定位

该包不是运行时包装器库。

它是一个创作包。

这意味着：

- 真理源是 C# 存根类型
- 编译器从存根元数据派生描述符数据
- 主机拥有插件安装和打包
- 业务创作者不编写 JavaScript 或 TypeScript 包装器

## 4. 包契约

包应该使用命名空间：

`ECMAScript.UI.Vue.Vuetify`

包应该将其组件定义为继承 `VueLibraryComponent` 的瘦存根。

存根表面应保持接近 Blazor 约定：

- props 的 `[Parameter]`
- 事件的 `EventCallback`
- 默认和命名插槽的 `RenderFragment`
- 需要时作用域插槽的 `RenderFragment<TContext>`

## 5. 第一波组件

第一波组件集应该小而高价值：

- `VBtn`
- `VTextField`
- `VCard`
- `VIcon`
- `VDialog`

`VDialog` 应被视为第一个插槽上下文示例，可能在更简单的组件之后落地。

## 6. 组件建模规则

每个组件存根必须声明足够的元数据，以便编译器派生：

- `SourceKind = LibraryComponent`
- 运行时导入说明符
- 运行时导出名称
- 样式依赖

推荐的元数据形状：

- `VueLibraryComponentAttribute`
- `VueLibraryStyleAttribute`

推荐创作规则：

- 存根应保持瘦
- 存根中不应存在运行时行为
- 核心管道中不应添加组件特定的降低分支

## 7. 绑定规则

绑定应保持 C# 形状。

推荐模型：

- `ModelValue`
- `ModelValueChanged`

这保持了 Blazor 用户的创作体验熟悉，同时仍然干净地降低为 Vue 模型更新语义。

## 8. 插槽规则

插槽应尽可能保持强类型。

推荐映射：

- `RenderFragment` -> 默认插槽或简单命名插槽
- `RenderFragment<TContext>` -> 带有 C# 上下文类型的作用域插槽

`VDialog.Activator` 应该是第一个强类型作用域插槽示例。

## 9. 样式规则

Vuetify 组件应显式声明样式依赖。

第一个包应至少声明：

- `vuetify/styles`

编译器应将此保留为主机侧消费的元数据。

## 10. 插件规则

Vuetify 还需要面向主机的插件要求。

包不应自己安装插件。
它应该只使要求显式，以便主机可以 later 消费它。

## 11. 延迟范围

第一个包不应尝试覆盖：

- 完整的 Vuetify 组件覆盖
- 所有 Vuetify props 和 emits
- 所有插槽上下文变体
- 图标集和主题配置
- Router 或 Pinia 集成
- 自定义 Vuetify 特定降低路径

## 12. 验收标准

当以下所有情况为真时，包有用：

- C# 创作者可以导入 `ECMAScript.UI.Vue.Vuetify`
- Vuetify 组件被识别为库组件
- 描述符提取与现有 RazorVue 模型保持统一
- 生成的工件声明正确的导入、样式和插件要求
- 创作模型仍然感觉像 Blazor
