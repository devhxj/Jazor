# Jazor.Admin 抽象架构

## 结论

`Jazor.Admin` 应该是**后台框架抽象层 + 自有原生组件层**，而不是第三方组件库适配层。

因此：

- `JazorAdmin` 不应作为正式产品项目存在
- `Jazor.Admin.ElementPlus` 也不应作为正式产品项目存在
- `TDesign` / `ElementPlus` 这类第三方库，应该位于 `Vben` 体系之外
- `Vben` 自己只维护两类东西：
  - 后台框架抽象 contract
  - 自己可控的原生实现组件

如果需要和第三方库协同，正确位置应该是：

- sample
- 外部集成示例
- 应用层组合代码
- 独立实验/社区扩展包

而不是进入 `Jazor.Admin` 主产品线。

## 为什么 `JazorAdmin` 是错误分层

`Vben` 的职责如果是“后台框架”，那它的稳定边界应该是：

- 后台布局语义
- 导航语义
- 页面容器语义
- 工具栏/动作区语义
- 状态与插槽边界
- 容器 contract / implementation 注入边界

这些都不该依赖 `TLayout`、`TMenu`、`TButton` 这样的具体组件名。

一旦出现 `JazorAdmin` 这种命名，就意味着框架层已经默认：

1. `Vben` 需要为某个第三方库做专门工程
2. `Vben` 的可用性依赖某个第三方库的能力面
3. `Vben` 的 API 演进会被第三方库 props 反向牵引

这会直接带来三个问题：

1. 抽象层失焦  
   `Vben` 不再是后台框架，而开始退化成“后台框架 x 某 UI 库”的组合包。

2. 依赖方向错误  
   上层框架语义本该稳定，第三方 UI 库本该是可替换项；一旦出现专属 adapter project，第三方库就会反向定义框架边界。

3. 扩展成本失控  
   后面每接一个库，就要再造一条 `Jazor.Admin.Xxx` 线，最后维护的是 N 条耦合支线，而不是一个稳定框架内核。

## 正确分层

推荐拆成三层，但只有前两层属于 `Vben` 主产品线。

### 1. 基础 Vue authoring 层

位置：

- `ECMAScript.Vue3`

职责：

- 通用 `class` / `style` / slot / props authoring contract
- 不带后台语义
- 不带 UI 库语义

这类能力应继续下沉在 `Vue3`：

- `VueClassValue`
- `VueStyleValue`
- `RenderFragment`
- `RenderFragment<T>`
- 通用 attribute / listener / props bag

这里是所有上层模块共享的 authoring 基础。

### 2. Vben 核心层

位置：

- `Jazor.Admin`

职责：

- 后台框架抽象 contract
- 自有原生后台组件
- 状态模型、插槽模型、导航模型、布局模型

它应该只暴露 `Vben` 自己的稳定 public surface，例如：

- `AdminLayoutMode`
- `AdminNavItem`
- `AdminBreadcrumbItem`
- `AdminPageAction`
- `VbenRouteLocation`
- `AdminLayout`
- `VbenSidebar`
- `HeaderBar`
- `PageContainer`

并且这些公开壳层组件本身应直接承担容器 contract 角色：

- authored public component：`ComponentBase + IVueComponent + IVueContainerComponent`
- injected implementation：`IVueContainerImplementation<TContainer>`
- implementation 选择：`[assembly: VueInject(...)]`

这里不再额外保留 `IVbenUiAdapter` 一类平行抽象。`Vben` 的可替换能力统一收口到 `VueContract` 容器机制。

这里的组件如果存在，应该是：

- `Vben` 自己实现的原生组件
- 基于 HTML / Vue / 自有样式系统
- 不依赖 `ElementPlus` / `TDesign` / `Vuetify` 组件名

### 3. 第三方库协同层

这一层**不属于 `Jazor.Admin` 主产品线**。

正确形态应该是：

- sample
- app-level composition
- 外部集成指南
- 非主线扩展包

例如：

- `samples/VbenWithElementPlus`
- `samples/VbenWithTDesign`
- `docs/04-补充/xxx-integration.md`

这里可以展示：

- 如何把 `AdminNavItem` 投影到第三方菜单
- 如何在 `PageContainer` 的 slot 里放入第三方按钮/表单/表格
- 如何让应用层主题和 `Vben` 布局一起工作

但这些都不应回流为 `Jazor.Admin.*第三方库名*` 的正式核心项目。

## Vben 应该抽象什么

`Vben` 不是抽象“第三方组件库”，而是抽象“后台框架结构”。

应该优先抽象以下能力：

### 1. 布局结构

- 主框架壳层
- 侧栏区
- 顶栏区
- 页面主内容区
- 页面头区
- 工具区

### 2. 导航模型

- 菜单树
- 面包屑
- 当前选中项
- 展开项
- 路由目标
- 外链目标

### 3. 页面容器语义

- 页面标题
- 副标题
- 操作按钮区
- 额外插槽区
- 正文容器

### 4. 插槽与模板边界

为了和第三方库解耦，`Vben` 应该优先暴露插槽/模板扩展点，而不是第三方 props：

- `Logo`
- `HeaderActions`
- `UserRegion`
- `Extra`
- `NavItemTemplate`
- `BreadcrumbItemTemplate`

这样应用层要用 `ElementPlus` 或 `TDesign` 时，是在 slot 内组合，而不是把第三方 props 引进 `Vben` 核心。

### 5. 原生主题 token

`Vben` 可以有自己的 theme/token contract，但应保持在框架自有语义层，例如：

- spacing
- shell width
- sidebar collapsed width
- header height
- page padding
- color token

而不是暴露第三方库 theme 对象。

## 第三方库如何接入

正确方式不是做 `JazorAdmin` 这类正式适配项目，而是：

1. `Vben` 提供稳定结构组件和 slot
2. 应用层决定某个 slot 里放什么第三方组件
3. 如需替换整个结构组件，则通过 `IVueContainerImplementation<T>` + `[VueInject]` 做编译期实现注入
4. 应用层自己完成第三方 props 映射

也就是说：

- `Vben` 负责“框架结构”
- 第三方库负责“具体控件”
- 应用层负责“二者组合”和“容器实现注入”

这个依赖方向才是稳定的。

## 容器机制为什么要放在 `VueContract`

你前面提出的目标本质是：

- authored 代码面对稳定容器组件
- 转译/编译时再查出当前配置的具体实现组件
- 这个实现可以来自 Element Plus，也可以来自 Vuetify 或应用自定义组件

这正是 `IVueContainerComponent + IVueContainerImplementation<T> + [VueInject]` 机制的职责。  
因此正确动作不是在 `Vben` 再造一套 adapter/interface，而是让 `Vben` 公开组件直接使用这套通用容器机制。

这样有三个直接收益：

1. `Vben` 不发明第二套扩展协议
2. RazorVue descriptor / inject / identity / lowering 可以复用同一条生产链路
3. 第三方库替换能力存在，但不会反向污染 `Vben` 的 public authoring surface

截至 2026-05-15，这个机制在 `Vben` 公开壳层上已经不是停留在概念层：

- 四个公开壳层组件都已按 container contract 参与编译期注入回归；
- 默认原生解析与 injected library runtime shape 均已验证；
- 主要非法注入声明也已经有 focused 失败诊断回归，容器机制本身已进入可依赖的生产约束面。

## Phase 1 正确交付物

第一阶段应该交付的是：

1. `Jazor.Admin` 抽象 contract
2. `Jazor.Admin` 自有原生壳层组件
3. 面向应用层的 slot / template 扩展点
4. focused verification
5. 一个第三方库组合 sample

第一阶段**不应该**把下列事项作为主交付物：

- `JazorAdmin`
- `Jazor.Admin.ElementPlus`
- 某个 UI 库的整套 adapter project

## 当前纠偏结论

基于当前目标，仓库里的 `samples/JazorAdmin/` 应视为：

- 一次错误分层的临时 spike
- 不是正式目标架构
- 不应继续作为产品主线推进

后续正确动作应是：

1. 保留其中少量可复用的后台语义认知
2. 把正式设计收回到 `Jazor.Admin`
3. 将第三方库协同下沉到 sample / app composition / 外部扩展
4. 在文档确认后，移除或下线该错误方向项目
