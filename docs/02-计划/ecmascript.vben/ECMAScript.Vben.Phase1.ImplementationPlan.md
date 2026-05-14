# ECMAScript.Vben 第一阶段实施计划

> Status: 活跃计划  
> Updated: 2026-05-15  
> Positioning: 基于 `docs/01-目标/ecmascript.vben/` 的设计边界，为 `src/ECMAScript.Vben/` 建立第一阶段可落地实施顺序。当前关注点是后台壳层核心与原生实现闭环，而不是完整复刻前端后台框架，也不是为第三方组件库建立专属 adapter project。

## 当前纠偏状态

当前已经明确：

- `ECMAScript.Vben` 必须回到“抽象层 + 自有原生实现”主线
- `class/style` 这类通用 authoring 值域继续下沉到 `ECMAScript.Vue3`
- `prop/slot` 元数据进一步上提为通用 `VueProp` / `VueSlot`，旧 `VueLibraryProp` / `VueLibrarySlot` 已不再保留
- `src/ECMAScript.Vben.TDesign/` 不是正确产品方向，只能视为临时 spike
- Phase 1 不再把任何 `ECMAScript.Vben.*第三方库名*` 项目作为正式交付物

当前实现状态补充：

- `src/ECMAScript.Vben/` 已形成原生 Razor 壳层组件骨架并可构建
- `src/ECMAScript.Vben.Test/` 已建立独立聚焦验证，不再依赖当前被 `ECMAScript.ElementPlus` 阻塞的共享测试主线
- RazorVue 已补齐“引用程序集中的原生 `IVueComponent` 组件也可进入组件注册解析”的能力，确保 `Vben` 作为独立包时仍可被消费
- `VbenAdminLayout` / `VbenSidebarMenu` / `VbenHeaderBar` / `VbenPageContainer` 已明确承担 `IVueContainerComponent` 容器契约角色
- 原先设想的 `IVbenUiAdapter` 空接口不再保留，统一收口到 `VueContract` 容器注入机制
- 四个公开壳层都已具备 inject success-path 回归，且 Vben 语义下的主要 failure-path 兼容性诊断已进入 focused tests

## 0. 第一阶段目标

第一阶段只解决一个问题：

**让 Jazor / RazorVue 用户可以用 Razor 组件 authoring 出一套最小后台管理壳层。**

这意味着首阶段要能稳定表达：

- 主布局
- 左侧导航
- 顶部栏
- 页面容器
- 菜单数据模型

而不要求一开始就具备：

- 表单 schema
- 表格 schema
- 动态权限系统
- 全量主题系统
- 多套 UI 库同时成熟支持

## 1. 总体路线

第一阶段按三层推进，保持依赖方向清晰：

1. **Phase A: 核心 contract**
   - 新建 `ECMAScript.Vben` 公共语义层
   - 定义后台壳层结构组件和导航模型

2. **Phase B: 原生壳层闭环**
   - 在 `ECMAScript.Vben` 内实现自有原生后台壳层
   - 跑通核心壳层到 slot / template 扩展点的最小闭环

3. **Phase C: 验证与样例**
   - 建立针对性测试或最小 sample
   - 验证 Razor authoring、descriptor、lowering、emit 主路径
   - 如需验证第三方库协同，只放在 sample，不形成正式 adapter project

当前不把 `Element Plus` 或 `TDesign` 的专属适配作为 Phase 1 阻塞项。  
原因很简单：

- `Vben` 的主目标不是适配某个库，而是建立自己的后台框架结构；
- 一旦把第三方库 adapter 纳入主产品线，抽象层会立刻失焦。

## 2. 计划目标

### 2.1 建立核心语义而不是 UI 库镜像

`ECMAScript.Vben` 的 public API 应描述：

- 后台布局区块
- 菜单节点结构
- 页面容器语义
- 头部动作区语义

它不应直接变成：

- TDesign props 汇总
- Element Plus props 汇总
- `src/vben` TS 工程 API 的逐项搬运

### 2.2 Razor 作为主要 authoring 方式

这一阶段默认：

- 布局与结构组件用 Razor / `ComponentBase`
- 少量动态节点桥接使用 `h(...)`

不要把纯 `h(...)` 作为主 authoring 路线。  
这会让后台壳层在可读性和组合性上都退化。

### 2.3 强类型优先

以下反模式在第一阶段就应避免：

- `object? LayoutProps`
- `object? MenuOptions`
- `object? HeaderOptions`

应该优先使用：

- 命名 record
- enum / union
- `RenderFragment`
- 明确的 adapter interface

## 3. Phase A: 核心 contract

### A1. 新建 `src/ECMAScript.Vben/`

项目边界：

- 独立项目
- 依赖 `ECMAScript`
- 依赖 `ECMAScript.Vue3`
- 需要时依赖 `Microsoft.AspNetCore.App`

第一阶段不依赖具体 UI 库项目。

### A2. 建立基础 authoring base types

建议首先建立：

- `VbenComponentBase`
- `VbenContentComponentBase`

目标与 `TDesignComponentBase` 类似：

- `CssClass`
- `CssStyle`
- `AdditionalAttributes`
- `ChildContent`

但注意这里表达的是后台壳层公共 authoring 基底，不直接绑定某个 UI 库的 style value contract。

### A3. 建立核心模型

第一批模型只覆盖后台壳层高频语义：

- `VbenNavItem`
- `VbenBreadcrumbItem`
- `VbenPageAction`
- `VbenLayoutMode`

`VbenNavItem` 第一阶段至少需要：

- `Key`
- `Title`
- `Icon`
- `Href` / `To`
- `Children`
- `Disabled`

### A4. 建立扩展接口

先定义可控的扩展边界，而不是第三方库 adapter：

- slot / template contract
- 应用层可组合的 shell extension point
- `IVueContainerComponent + IVueContainerImplementation<T> + [VueInject]` 容器注入边界

这一层的价值在于让 `Vben` 保持抽象稳定，同时允许应用层把第三方组件填进来。

## 4. Phase B: 原生壳层闭环

### B1. 在 `Vben` 内完成自有壳层实现

理由：

- 这是唯一不受第三方库反向牵引的实现路径
- 可以真实验证 `Vben` 自己的抽象是否足够稳定
- 后续接任意库时，依赖方向都不会反过来

### B2. 先做最小后台壳层实现

建议第一批组件：

1. `VbenAdminLayout`
2. `VbenSidebarMenu`
3. `VbenHeaderBar`
4. `VbenPageContainer`

其中：

- `VbenAdminLayout` 负责区域组织
- `VbenSidebarMenu` 负责导航树与 selected/expanded/collapsed
- `VbenHeaderBar` 负责 logo/title/actions/user-region 这类顶部结构
- `VbenPageContainer` 负责页面标题、副标题、extra actions、正文容器

当前补充约束：

- 以上公开壳层组件默认就是原生实现，同时也是正式容器 contract
- 若应用层要替换整个 `VbenPageContainer` 或 `VbenHeaderBar`，必须通过 `VueInject` 选择实现，而不是引入 `Vben.*第三方库*` 主线工程

### B3. 动态菜单节点渲染允许局部 `h(...)`

递归菜单节点是第一阶段最合理使用 `h(...)` 的地方之一。  
原因：

- 节点层级动态
- icon / title / child route 结构可能需要运行时拼装
- 用少量 `h(...)` 处理递归要比堆很多中间组件更直接

但这只应存在于 `Vben` 自有实现内部，不应把整个后台壳层 authoring 都推向 `h(...)`。

## 5. Phase C: 验证与样例

### C1. 至少要有 focused verification

第一阶段至少需要以下一种或多种验证：

- 项目 build 通过
- RazorVue descriptor / lowering 回归
- 容器 contract 默认解析与 `[VueInject]` 替换回归
- 最小 sample 编译产物验证

如果测试面还不值得独立工程，至少要有 focused build/smoke 验证。

当前这一项的已完成部分：

- 默认原生解析回归
- `VbenAdminLayout` / `VbenSidebarMenu` / `VbenHeaderBar` / `VbenPageContainer` 的 `[VueInject]` 替换回归
- 容器兼容性失败诊断回归（prop / emit / slot / capture-unmatched-values / duplicate inject / wrong contract）

### C2. 建议增加最小 sample

最小 sample 不需要完整业务页，只需要证明：

- 布局可写
- 菜单可渲染
- 页面容器可组合
- 头部 action 与 child content 可落到最终工件
- 第三方库如需协同，也是在 sample 层完成，而不是新建 adapter project

## 6. 第一阶段切片顺序

建议按以下顺序推进：

1. 新建 `docs/01-目标/ecmascript.vben/` 与 `docs/02-计划/ecmascript.vben/`
2. 新建 `src/ECMAScript.Vben/` 项目骨架
3. 建立基础 base types 与 nav/page model
4. 建立 `VbenAdminLayout` / `VbenPageContainer`
5. 建立 `VbenSidebarMenu`
6. 补原生实现验证 sample
7. 增加 focused verification

这个顺序的好处是：

- 先有边界
- 再有核心 contract
- 再有首个适配
- 最后再补验证

避免一开始就在适配细节里打转。

## 7. 当前明确非目标

以下内容不属于第一阶段默认范围：

- `ECMAScript.ElementPlus` 达到与 TDesign 同级别完成度
- 多 UI 库同时 feature parity
- 完整表单 / 表格 / schema 引擎
- 权限、国际化、主题系统的产品级闭环
- 完整复制 `src/vben` 示例站结构

这些事项后续可以进入独立计划，但不应阻塞 Phase 1。

## 8. 风险与控制

| 风险 | 影响 | 控制方式 |
|------|------|---------|
| 直接把 `src/vben` 当源码翻译 | 高 | 强制按“后台语义层 + 适配层”拆分 |
| 公共 API 被底层 UI 库污染 | 高 | 先定义核心模型与 slot/template 扩展边界 |
| 过早引入 `Vben x 第三方库` adapter project | 高 | 明确第三方协同只放 sample / app composition |
| 纯 `h(...)` 导致 authoring 失去结构性 | 中 | Razor 作为主要 authoring，`h(...)` 仅内部桥接 |
| 首阶段范围膨胀到表单/表格/schema | 高 | 明确只做后台壳层闭环 |

## 9. 完成定义

第一阶段可视为完成，至少要满足：

1. `src/ECMAScript.Vben/` 项目建立完成
2. 核心后台壳层组件存在且可编译
3. 有一套自有原生后台壳层主线
4. 有 focused verification 证明主链路可用
5. 文档能清楚说明去第三方库耦合的分层边界、容器注入扩展方式与后续扩展方向
