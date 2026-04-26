# RazorVue 硬规则

本文档修复阶段一期间不能保持模糊的实现规则。

它不重复所有设计讨论。
它存在是为了锁定后续实现和审查不得不断重新协商的边界。

相关文档：

- [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)
- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.ImplementationChecklist.md](../../../02-计划/jolt/razorvue-implementation/RazorVue.ImplementationChecklist.md)
- [RazorVue.Pitfalls.md](./RazorVue.Pitfalls.md)

## 1. 范围

这些规则适用于 RazorVue 阶段一：

- Razor 组件进入 ECMAScript 前端编译
- Vue 优先降低
- Vue ESM 工件发送
- `DenoHost` 移交

## 2. 规则 1. RazorVue 是 Vue 优先

阶段一 RazorVue 是：

- Razor 作为模板语法
- Vue 作为真实运行时语义模型
- `DenoHost` 作为统一构建主机

它并非：

- 通用 UI 抽象层
- 跨框架编译目标
- Blazor 运行时克隆

## 3. 规则 2. `[ECMAScriptModule]` 统一入口，而非降低

`[ECMAScriptModule]` 仍然是单一入口标记。

入口后，编译器必须将输入拆分为：

1. 普通静态模块类
2. Razor 组件

任何尝试对两者使用一条降低路径的实现都超出范围。

## 4. 规则 3. RazorVue 组件必须继承 `JazorComponent`

进入 RazorVue 路径的任何 Razor 组件必须继承 `JazorComponent`。

推荐创作基础：

- `VueComponent : JazorComponent`

必须诊断无效情况：

- `[ECMAScriptModule]` Razor 组件仅继承 `ComponentBase`
- `[ECMAScriptModule]` Razor 组件既不继承 `JazorComponent` 也不继承其后代

## 5. 规则 4. `JazorComponent` 必须继承 `ComponentBase`

阶段一层次结构固定为：

`ComponentBase -> JazorComponent -> VueComponent`

这不是可选的。

基本原理：

- Razor 组件在技术上基于 `ComponentBase`
- C# 不支持多重继承
- 试图绕过 `ComponentBase` 不是稳定的设计路径

## 6. 规则 5. `JazorComponent` 必须保持瘦

`JazorComponent` 是组件标识基础，而不是第二个运行时框架主机。

它不得吸收：

- Vue 组合 API
- 运行时调度
- 打包/构建关注
- 通用状态运行时

Vue 优先创作 API 属于 `VueComponent`。

## 7. 规则 6. `VueComponent` 是 Vue 优先创作 API 的必需主机

Vue 优先 API 必须具有稳定的、编译器可识别的主机。

该主机是 `VueComponent`。

阶段一 API 表面属于那里，包括：

- `Ref`
- `Reactive`
- `Computed`
- `Watch`
- `WatchEffect`
- `NextTick`
- `OnMounted`
- `OnUpdated`
- `OnUnmounted`
- `Emit`
- `Provide`
- `Inject`
- `Expose`

## 8. 规则 7. 阶段一不解析 `.razor`

阶段一不得引入自定义 `.razor` 解析器作为主要输入路径。

编译器应消费：

- 组件符号
- 生成的 `BuildRenderTree` 操作
- 代码隐藏符号/操作

任何将重建 Razor 解析器作为阶段一先决条件的实现都超出范围。

## 9. 规则 8. 不依赖源生成器排序

阶段一不得假设：

- Razor SG 首先运行
- 另一个 SG 可以可靠地消费其输出

源生成器排序不得成为 RazorVue 的架构基础。

## 10. 规则 9. 生成代码分析是必需的

RazorVue 分析器必须启用生成代码分析。

这不是可选增强。
它是使用生成的 Razor 组件代码作为语义提取源的先决条件。

## 11. 规则 10. 分析器模式拆分是必需的

现有的普通 ECMAScript 分析器规则不得不变地对 RazorVue 组件运行。

阶段一需要在以下内容之间进行显式模式拆分：

- 普通静态模块分析
- RazorVue 组件分析

否则，有效的 RazorVue 符号如：

- `ComponentBase`
- `RenderFragment`
- `EventCallback`

将在 RazorVue 降低开始之前被错误的规则集拒绝。

## 12. 规则 11. Razor 组件不重用静态模块降低

RazorVue 不得将生成的 Razor 组件体发送到普通静态模块降低管道。

原因：

- 生成的 Razor 代码基于构建器模式
- 它在结构上与普通用户创作的模块方法不同

因此阶段一需要为以下内容提供单独的路径：

- 渲染树提取
- Razor 到 Vue 降低

## 13. 规则 12. 语义载体必须是显式的

阶段一必须在语义提取和面向主机的发送之间定义显式编译器拥有的载体。

该载体可以通过以下方式实现：

- `RazorVueSemanticSnapshot`
- `VueCompiledArtifact`
- `RazorVueCatalog`

或等效结构。

但它不得被替换为：

- 在后续阶段重复重新分析
- 仅分析器隐藏状态
- 仅原始字符串拼接作为唯一移交

载体生产/消费拆分也必须保持显式：

- 分析器诊断/发现
- 编译器拥有的快照提取
- 编译器拥有的降低
- 编译器拥有的目录/物化
- 主机侧消费

## 14. 规则 13. Vue 定义运行时生命周期语义

阶段一运行时生命周期语义是 Vue 优先。

Blazor 生命周期成员仅是编译时糖。

因此：

- `OnInitialized*`
- `OnParametersSet*`
- `OnAfterRender*`
- `Dispose*`

必须降低为 Vue 概念，如：

- `setup`
- `watch(props, ...)`
- `onMounted`
- `onUpdated`
- `onUnmounted`

阶段一不承诺完整的 Blazor 运行时等效。

## 15. 规则 14. `StateHasChanged`、`ShouldRender` 和 `SetParametersAsync` 保持在主模型之外

这些成员可能在技术上通过 `ComponentBase` 继承，
但阶段一不得接受它们作为 RazorVue 语义模型的一部分。

实现要求：

- 使用它们必须产生诊断
- 它们不得静默影响 Vue 降低行为

## 16. 规则 15. 组件契约必须在渲染降低之前提取

阶段一必须在渲染降低之前提取显式组件契约模型。

该契约至少包括：

- props
- emits
- slots
- 绑定/模型元数据
- 导入/导出标识

在遍历渲染树输出时不要临时猜测组件契约。

## 17. 规则 16. 组件解析必须是 `using` 驱动和显式的

阶段一组件可见性必须从以下内容确定：

- 当前命名空间
- 作用域内 `using` 指令
- 内置组件注册表
- 引用的用户组件描述符
- 库描述符注册表

编译器不得回退到全局短名称搜索。

如果多个可见组件共享相同的短名称，
阶段一必须报告歧义诊断。

阶段一歧义转义限于完全限定组件名称。
除非其降低的语义形式证明稳定，否则不要使别名限定标签语法成为阶段一要求。

## 18. 规则 17. 内置组件名称保留

阶段一保留内置 Vue 组件名称。

示例包括：

- `Teleport`
- `Transition`
- `KeepAlive`
- `Suspense`

用户组件和库组件不得静默隐藏它们。

## 19. 规则 18. 组件属性匹配是严格的

阶段一必须对组件调用严格。

允许：

- HTML 元素的合理属性灵活性

不允许：

- 未知组件 props 的静默传递
- 未知组件事件别名的静默回退
- 未解析的插槽名称被接受为普通组件 props

必须诊断未知组件侧属性。

## 20. 规则 19. 输出必须是标准 Vue ESM

阶段一输出固定为带有以下内容的标准 Vue ESM：

- `defineComponent`
- `setup`
- 渲染函数

阶段一不得转向：

- `.vue` SFC 作为主要格式
- 自定义运行时模块形状
- 打包器拥有的私有模块格式

## 21. 规则 20. 编译器和 `DenoHost` 职责必须保持分离

编译器拥有：

- 语义提取
- 契约生成
- Vue 模块生成
- 工件/清单元数据生成

`DenoHost` 拥有：

- 依赖解析
- 统一编译
- 打包
- 运行时集成

编译器不得增长自己的打包器。
`DenoHost` 不得重新解释 Razor 组件语义。

## 22. 规则 21. 当前主机集成需要显式迁移路径

仓库已经有工作的静态模块目录和发送流。

阶段一 RazorVue 必须定义新 Vue 工件如何与以下内容共存：

- 当前 `ModuleCatalog`
- 当前主机清单处理
- 当前下游打包流

项目不得在将此过渡留作隐式的情况下进入实现。

## 23. 规则 22. 阶段一必须保持最小

阶段一必须只闭合最小可行循环：

- RazorVue 组件发现
- 契约提取
- 最小渲染树恢复
- Vue 降低
- 工件发送
- `DenoHost` 移交

它不得扩展为：

- 完整的生态系统支持
- 完整的 HMR 运行时
- 完整的 sourcemap 发送
- 广泛的 Razor 兼容性
- 通用多框架抽象

## 24. 规则 23. 从阶段一开始必须保留源始元数据

阶段一不需要完整的 sourcemap 输出，
但它必须在管道中保留源始元数据。

至少这适用于：

- 渲染树节点
- 组件逻辑绑定
- 生命周期绑定
- Vue 降低节点
- 工件输出锚点

始类别应至少区分：

- `razor-template`
- `component-logic`
- `generated-render`

不要依赖从最终 JS 文本反向推断。

并且不要仅保留类别。

阶段一源始元数据还必须保留：

- 原始源文件路径（已知时）
- 稳定跨度或稳定段标识
- 精确源不可用时生成的回退跨度
- 显式映射质量

并且阶段一必须保留始记录本身的出处，区分：

- Razor 支持的源映射
- 生成语法派生的映射
- 仅生成回退

## 25. 规则 24. 工件标识必须是稳定和分离的

阶段一工件必须保留稳定的标识和分离的哈希信息。

至少保留：

- `ComponentId`
- `ModuleId`
- `DescriptorHash`
- `TemplateHash`
- `LogicHash`
- `HmrBoundaryKind`

不要将所有更改类别折衷为一个无差异的内容哈希。

## 26. 规则 25. HMR 和 sourcemap 架构上包含，而非静默延迟

阶段一不完全实现 HMR 或 sourcemap，
但架构必须已经为两者保留所需的数据。

这意味着：

- 源始元数据现在是必需的
- 稳定的工件标识现在是必需的
- 后续 HMR/sourcemap 不得需要重新设计主管道

## 27. 结论

RazorVue 阶段一仅在保持这些边界稳定时成功：

- Vue 优先语义
- 统一入口加上拆分降低
- `JazorComponent` / `VueComponent` 层次结构
- 基于分析器的生成代码语义提取
- 专用的 Razor 渲染树降低
- 标准 Vue ESM 输出
- 编译器/`DenoHost` 分离
- HMR/sourcemap 就绪的元数据保留
