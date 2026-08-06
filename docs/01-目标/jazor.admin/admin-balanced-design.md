# Jazor.Admin 平衡式目标设计

## 定位

`Jazor.Admin` 不是对 `src/vben/` 源码的直接搬运，也不是某个 UI 组件库的又一套薄绑定。
它的职责是把“后台管理框架”这一层抽成稳定的 C# authoring surface，让 Jazor / RazorVue 用户可以在 Razor 中表达：

- 后台壳层布局
- 侧边导航与面包屑
- 页面容器与工具栏
- 常见后台页面的结构性骨架

同时把具体 UI 组件库差异隔离到应用层组合或外部集成层，而不是把 `Element Plus`、`TDesign` 甚至未来其他库的 props 直接灌进同一个核心库。

## 目标

1. 为后台管理场景建立独立于 `ECMAScript.Vue3` 基础 binding 的上层 authoring surface。
2. 保持核心库描述的是“后台框架语义”，而不是某个具体 UI 库的 props 镜像。
3. 优先使用 Razor 组件表达布局与结构，少量动态桥接再由 `h(...)` 承担。
4. 与现有 `ECMAScript.TDesign`、未来 `ECMAScript.ElementPlus` 保持解耦：`Jazor.Admin` 不反向污染底层组件绑定，也不为第三方库建立正式 adapter 产品线。
5. 让第一阶段先跑通最小后台壳层，不把表单、表格、schema、权限系统等大块能力一次性塞进首包。

## 非目标

- 不直接复刻 `src/vben/` 全部工程结构、脚手架、构建逻辑与前端运行时。
- 不把 `Jazor.Admin` 设计成 `Element Plus` 或 `TDesign` 的完整组件代理层。
- 不在第一阶段引入完整的表单 schema、表格 schema、动态权限、国际化、主题系统全量能力。
- 不在 compiler 中为 `vben` 增加名字级特判。
- 不为了“同时支持多个 UI 库”而把公共 API 弱化成 `object` catch-all。

## 推荐分层

### 1. 核心壳层：`Jazor.Admin`

只承载后台框架语义：

- `AdminLayout`
- `VbenSidebar`
- `HeaderBar`
- `PageContainer`
- `VbenBreadcrumb`
- `AdminNavItem`

这层不依赖某个 UI 库的菜单项、按钮、布局 props 细节。  
它只描述后台框架需要的稳定结构与交互边界。

### 1.1 容器化扩展边界

`Vben` 不再额外定义 `IVbenUiAdapter` 这类自有 adapter 空接口。  
统一扩展边界直接复用 `VueContract` 的容器机制：

- authored 原生壳层组件实现 `IVueContainerComponent`
- 第三方或应用层具体实现组件实现 `IVueContainerImplementation<TContainer>`
- 最终装配选择通过 `[assembly: VueInject(typeof(TContainer), typeof(TImplementation))]`

这样做的直接收益是：

- 不引入第二套与 RazorVue 平行的扩展协议
- `Vben` 保持“稳定 authoring contract + 可替换 runtime implementation”模型
- 应用层可按需把 `PageContainer`、`HeaderBar` 等映射到 Element Plus / Vuetify / 自定义实现，而不需要新增 `Jazor.Admin.*第三方库*` 主线项目

### 2. 第三方组件绑定：`ECMAScript.TDesign` / `ECMAScript.ElementPlus`

这一层负责：

- npm 模块导入
- 插件 requirement
- style requirement
- 单个组件的强类型 props / slots / emits

当前仓库里：

- `ECMAScript.TDesign` 已经形成可验证的首批后台壳层基础面；
- `ECMAScript.ElementPlus` 虽然已经起了项目骨架和第一批组件，但仍处在**刚开始做**的阶段，还不能当作成熟适配基线。

因此这些项目最多只能作为：

- 参考能力面
- 应用层组合对象
- sample 集成对象

它们不是 `Vben` 主产品线的一部分，也不应反向决定 `Vben` 的 package 结构。

### 3. Vben 自有原生实现：`Jazor.Admin`

`Vben` 自己真正应该承载的，是原生后台壳层实现：

- `AdminLayout`
- `SidebarMenu`
- `HeaderBar`
- `PageContainer`
- `VbenBreadcrumb`

这批组件应该由 `Vben` 自己实现，依赖：

- `Vue3` 通用 authoring contract
- 自有样式/token
- 自有 slot/template 扩展点

而不是落到某个第三方库的组件名上。

### 4. 第三方库集成：sample / app composition / 外部扩展

如果需要与 `TDesign` / `ElementPlus` 协同，正确位置应该是：

- sample
- 应用层组合代码
- 外部扩展包

不应该再建立：

- `JazorAdmin`
- `Jazor.Admin.ElementPlus`

## Razor 与 `h(...)` 的职责边界

### Razor 优先

后台壳层的主体更适合用 Razor 组件表达：

- 布局层级清晰
- 组合关系稳定
- 命名槽和 `RenderFragment` authoring 更自然
- 与当前 `ComponentBase + [Parameter] + VueLibrary*` 模式一致

因此以下能力应优先走 Razor：

- 页面壳层
- 侧栏 / 顶栏 / 主体区
- 面包屑与页面头
- 常见容器类结构组件

### `h(...)` 作为补充

`h(...)` 更适合处理少量运行时动态桥接：

- 递归菜单节点渲染
- 根据配置选择具体组件
- 需要在 adapter 中做动态 slot / props 拼装的场景
- 极少量 schema 驱动区域

也就是说，`h(...)` 是壳层实现细节的一部分，而不是整个后台框架的主要 authoring 方式。

## 公共 contract 设计原则

### 1. 描述后台语义，不泄露 UI 库细节

例如：

- 公共层可以有 `Collapsed`、`Logo`、`Actions`、`NavItems`
- 公共层不应直接出现 `TMenuExpandType` 这类底层专有类型

### 2. 公共层保持强类型

不要为了适配多套 UI 库，把公共入口退化成：

- `object? MenuProps`
- `object? HeaderProps`
- `object? LayoutOptions`

更合理的方向是：

- 使用命名 record / enum / union 表达稳定后台语义
- 把底层差异收口到 adapter 内部

这里的 adapter 指应用层或具体实现组件内部映射，不是 `Vben` 再公开一套新的 adapter 接口。

### 3. 结构优先于细节

第一阶段最重要的是先定义这些结构是否存在：

- 布局区块
- 导航树
- 页面标题 / 副标题 / extra actions
- collapsed / selected / expanded 这类状态

而不是先追求把每个库的长尾参数铺满。

## 第一阶段推荐收口面

第一阶段只做后台壳层最小闭环：

1. `AdminNavItem`
2. `AdminBreadcrumbItem`
3. `AdminLayout`
4. `SidebarMenu`
5. `HeaderBar`
6. `PageContainer`
7. slot / template 扩展点
8. 一个第三方库组合 sample（不是正式 adapter project）

这套最小闭环已经足够支持：

- 后台主布局
- 左侧菜单
- 顶部区域
- 页面容器

它还不足以替代完整 `vben admin`，但足以验证框架方向是否正确。

## 为什么不直接复刻 `src/vben`

`src/vben` 当前是一个完整前端后台框架工程，里面混合了：

- 应用工程结构
- 前端构建工具
- 多组件库适配
- 业务示例
- 文档站与 mock 服务

这些内容并不都属于 Jazor 里的“可复用后台壳层库”。  
如果直接照搬，结果会是：

- 代码量巨大
- C# authoring 边界失焦
- UI 库细节与框架语义混杂
- 很快退化成另一套难维护的前端镜像层

因此更合理的策略是：**参考其后台布局与交互模型，而不是照搬其源码组织。**

## 与现有仓库能力的关系

### 与 `ECMAScript.Vue3`

`Jazor.Admin` 建立在 `ECMAScript.Vue3` 之上，但不扩张 Vue3 核心绑定边界。
它是外部库线里的更高层 authoring surface。

### 与 `Jazor.RazorVue`

`Jazor.Admin` 的组件 authoring 需要继续遵守 RazorVue 当前原生组件规则：

- 使用 `ComponentBase`
- 实现 `IVueComponent`
- 使用 `[ECMAScriptModule(...)]`
- 使用 `[Parameter]`
- 通过 `RenderFragment` / `RenderFragment<T>` 表达插槽

其中需要明确两点：

1. `Vben` 的原生壳层组件不是 `IVueLibraryComponent`，也不依赖 `VueLibraryComponent` 这条 stub 描述链。
2. RazorVue 需要保证**被引用程序集里的原生 `IVueComponent` 组件**也能进入组件注册解析；否则 `Vben` 作为独立包时会失去消费能力。

通用 authoring 契约直接使用 Razor 和 Vue 的现行类型系统：prop 使用 `[Parameter]`，只有 Vue 原始名称不同才使用 `[ECMAScriptName]`；slot 使用 `RenderFragment` / `RenderFragment<T>`，事件使用 `EventCallback` / `EventCallback<T>`。

### 与 `ECMAScript.TDesign`

TDesign 当前可以作为能力参考和 sample 组合对象，因为仓库里已经有：

- `Layout`
- `Menu`
- `Button`
- `Card`
- `Space`
- `Breadcrumb` 风格组件基础面

这使得 `Vben` 的应用层组合 sample 可以较快成形，但不构成 `Vben` 正式项目拆分依据。

### 与 `ECMAScript.ElementPlus`

`ECMAScript.ElementPlus` 当前已经有独立项目与首批组件，但现阶段更准确的状态是：

- 初始切片已存在；
- authoring 面和验证面仍在起步；
- 还不应被当作与 `ECMAScript.TDesign` 同成熟度的生产基线。

因此 `Jazor.Admin` 会在公共层设计上继续保证对 Element Plus 友好，但不会为其建立专属主线 adapter project。

## 设计取舍

### Razor 优于纯 `h(...)`

纯 `h(...)` 当然可以实现后台壳层，但 authoring 成本更高，且对组件结构阅读不友好。  
后台壳层不是 vnode 工具库，它更接近结构化页面组件，因此 Razor 更合适。

### 核心层优于源码镜像

把后台框架语义抽成核心层，后续才能：

- 接多个 UI 库
- 做稳定测试
- 维持清晰依赖方向

### 原生实现优于第三方 adapter

如果一开始就沿着 `Vben x 第三方库` 的方向切项目，会把框架层直接做成耦合层。  
因此合理顺序是：

1. 核心层
2. 原生壳层实现
3. sample 级第三方库协同验证

## 后续补齐方向

- `ECMAScript.ElementPlus` 继续作为独立底层绑定项目推进
- `samples` 中补 Element Plus / TDesign 的协同样例
- 页面工具栏、Tab 导航、用户下拉区等高频后台块
- 表单容器 / 查询栏容器等“结构组件”，但不急着先做 schema 引擎
- 后续再评估表格、表单、权限、主题等是否需要进入独立工作流
