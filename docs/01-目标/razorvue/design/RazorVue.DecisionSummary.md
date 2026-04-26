# RazorVue 决策摘要

## 1. 本文档解决的问题

这是一个简短文档，仅保留 RazorVue 方向的最终决策，以便未来工作可以快速重启而无需重新打开已解决的问题。

完整设计位于：

- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.HardRules.md](./RazorVue.HardRules.md)
- [RazorVue.ImplementationChecklist.md](../../../02-计划/jolt/razorvue-implementation/RazorVue.ImplementationChecklist.md)

## 2. 最终决策

### 2.1 Vue 优先，而非跨框架 UI 抽象

RazorVue 不是 React/Vue/Svelte 统一工作。

目标是：

- Razor 作为模板语法
- Vue 作为真实组件/运行时模型
- `DenoHost` 作为统一构建主机

### 2.2 `[ECMAScriptModule]` 保持统一入口

`[ECMAScriptModule]` 仍然是前端编译的单一入口标记，
但它不再意味着"始终编译为普通静态 ECMAScript 模块"。

入口后，输入必须拆分为：

1. 普通静态模块类
2. Razor 组件

### 2.3 Razor 组件必须继承 `JazorComponent`

进入 RazorVue 管道的所有 Razor 组件必须继承：

- `JazorComponent`

推荐创作基础类型是：

- `VueComponent : JazorComponent`

### 2.4 基础类层次结构固定

基础层次结构是：

`ComponentBase -> JazorComponent -> VueComponent`

含义：

- `ComponentBase` 是技术 Razor 基础
- `JazorComponent` 为 Jazor 前端编译定义组件标识
- `VueComponent` 携带 Vue 优先创作 API

### 2.5 不要在源生成器排序上构建主管道

RazorVue 不得假设：

- Razor 源生成器首先运行
- RazorVue 源生成器可以然后消费其输出

主要语义提取基于：

- 分析器
- 生成代码分析

### 2.6 第一阶段不解析 `.razor`

第一阶段不引入自定义 `.razor` 解析器。

相反，它使用：

- 组件符号
- 生成的 `BuildRenderTree(RenderTreeBuilder)` 操作
- 代码隐藏符号和操作

### 2.7 Razor 组件不重用静态模块降低

即使分析器可以看到生成的代码，Razor 组件也不是普通用户创作的方法体。

因此：

- 不要将 Razor 组件发送到现有的静态模块降低路径
- 构建专用的 Razor 渲染树提取和 Vue 降低路径

### 2.8 运行时语义是 Vue 优先

Vue 是真实的运行时语义模型。

Blazor 生命周期成员仅保留为编译时糖：

- `OnInitialized*`
- `OnParametersSet*`
- `OnAfterRender*`
- `Dispose*`

它们降低为 Vue 概念，如：

- `setup`
- `watch(props, ...)`
- `onMounted`
- `onUpdated`
- `onUnmounted`

### 2.9 Vue 优先创作 API 位于 `VueComponent`

`VueComponent` 是 Vue 优先 API 的主机，如：

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

Razor 侧糖仍然可用：

- `[Parameter]`
- `EventCallback`
- `RenderFragment`
- `@bind`
- `@ref`
- `@key`

### 2.10 输出目标是标准 Vue ESM

第一阶段输出固定为：

- 标准 ESM
- `defineComponent`
- `setup`
- 渲染函数

非第一阶段目标：

- `.vue` SFC 输出
- 通用 UI 运行时
- 打包器拥有的自定义模块格式

### 2.11 `DenoHost` 是统一构建所有者

编译器职责：

- 组件语义
- Vue 模块生成
- 清单元数据生成

`DenoHost` 职责：

- 依赖解析
- 统一编译
- 打包
- 运行时集成

### 2.12 HMR 和 sourcemap 现在架构，稍后实现

第一阶段不需要完整的 HMR 或 sourcemap 支持，
但它必须保留：

- 源始元数据
- 稳定的工件标识
- 分离的模板/逻辑/描述符哈希

这些是架构要求，而不是可选的未来润色。

## 3. 第一阶段范围

第一阶段只需要闭合此循环：

1. 发现 `[ECMAScriptModule]` Razor 组件
2. 强制执行 `JazorComponent` / `VueComponent` 契约
3. 提取 props / emits / slots / bind 元数据
4. 从 `BuildRenderTree` 恢复最小渲染树模型
5. 降低为 Vue `defineComponent + render`
6. 为 `DenoHost` 发送清单

第一阶段不需要：

- 完整的生态系统集成
- 深度 SSR/hydration 策略
- sourcemap 输出
- HMR 运行时
- 通用多框架抽象

## 4. 验收摘要

RazorVue 第一阶段仅在以下所有情况为真时完成：

1. Razor 组件入口检测稳定。
2. 组件契约可提取。
3. 最小渲染树恢复工作。
4. Vue ESM 工件确定性发送。
5. `DenoHost` 可以消费清单。
6. 源始和 HMR 标识元数据已在管道中保留。

## 5. 一行结论

RazorVue 是一个 Vue 优先管道，其中 Razor 是模板语法，分析器从生成的 Razor 组件代码中提取语义，编译器发送 Vue ESM 工件，`DenoHost` 拥有最终统一构建。
