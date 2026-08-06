# ECMAScript.Vben 状态（2026-05-17）

> Status: historical pre-G0 status snapshot
> Positioning: `src/ECMAScript.Vben/` 后台壳层抽象与原生 RazorVue 实现的历史仓库级状态快照
> Scope: 原生壳层组件、公共后台语义模型、Vue authoring contract 收口、独立测试基线与产品边界
> Current boundary: `src/ECMAScript.Vben.Test/` 和旧 RazorVue SFC pipeline 已在当前转型分支退役；本文的测试通过记录和 Deno consumer 主链说明仅作迁移参考，不作为当前 `.mjs` 主线完成证据。

## 总结

`ECMAScript.Vben` 已完成第一轮仓库级落地，但当前仍应被视为 **Phase 1 原生壳层闭环已形成、后续进入稳定化和扩展期**，而不是完整后台框架产品已经做完。

这一轮已经明确并落地的核心事实是：

- `ECMAScript.Vben` 的定位是 **抽象层 + 自有原生实现**，不是某个第三方 UI 库的 adapter 命名空间；
- 原生组件继续走 RazorVue 用户组件路径：`ComponentBase + IVueComponent + [ECMAScriptModule(...)]`；
- 公开壳层组件现在同时承担 `IVueContainerComponent` 容器契约角色，可被应用层通过 `[VueInject]` 编译期替换到具体实现；
- `CssClass` / `CssStyle` / `AdditionalAttributes` 等 authoring 基底统一建立在 `ECMAScript.Vue3` 与通用 Vue contract 上；
- props 使用 `[Parameter]` 与按需的 `[ECMAScriptName]`，slots 使用 `RenderFragment` / `RenderFragment<T>`，事件使用 `EventCallback` / `EventCallback<T>`；
- 早期设想的 `IVbenUiAdapter` 空接口已被移除，Vben 不再维护与 `VueContract` 平行的第二套 adapter 协议；
- `src/ECMAScript.Vben.Test/` 已建立独立聚焦测试，不再把 Vben 演进绑死在其他外部库线的在制状态上。
- `samples/ECMAScript.Vben.ElementPlusInject/` 已建立真实 sample，验证 `ECMAScript.Vben` 容器 contract 在应用层通过 `[VueInject]` 切换到 sample-local Element Plus 实现，同时维持 Deno consumer 主链。

当前更准确的判断是：**产品边界已经纠偏，原生壳层主线已经可构建、可测试、可被 RazorVue 发现与消费。**

## 当前状态判断

### 1. 产品边界已经回到正确方向

当前 `Vben` 线已经明确拒绝以下错误演进方向：

- 把 `ECMAScript.Vben` 做成 `src/vben` 的逐项 API 搬运；
- 为 `TDesign`、`ElementPlus`、`Vuetify` 分别建立 `ECMAScript.Vben.*第三方库*` 正式产品线；
- 在 Vben 公共 contract 中直接泄漏第三方 UI 库的 props/value types。

这意味着：

- `Vben` 本身负责后台壳层语义；
- 第三方组件库协同只应放在 sample、应用层组合或未来明确定义的集成层；
- `Vben` 的 public surface 必须保持对第三方库解耦。

### 2. 原生壳层骨架已经形成

当前已存在的核心实现包括：

- `VbenComponentBase` / `VbenContentComponentBase`
- `VbenAdminLayout`
- `VbenSidebarMenu`
- `VbenHeaderBar`
- `VbenPageContainer`

这些组件已经足以表达第一阶段的最小后台壳层闭环：

- 主布局组织
- 侧边导航
- 顶部栏
- 页面容器

并且从 2026-05-15 起，这四个公开壳层组件已经正式进入“默认原生实现 + 容器 contract”双重角色：

- 默认情况下直接解析到 `ECMAScript.Vben` 原生实现
- 如应用层声明 `[VueInject]`，则 RazorVue 会保留 `Vben` authoring contract，同时切换到具体 implementation 的 runtime import / prop / slot 名称

当前对 `VbenSidebarMenu` 的原生行为补充已经进入主线：

- `ExpandedKeys` 现在有明确优先级：显式传入优先于基于 `SelectedKey` 的祖先展开回退；
- 当 `ExpandedKeys = []` 时，显式空数组会正确覆盖“按选中项自动展开祖先”的默认回退；
- 原生菜单项已区分 `selected`、`ancestor-selected`、`expanded`、`disabled` 等状态，不再只有叶子节点 `is-active`；
- 禁用分支不会再被误判为可导航或可展开子树；
- 即使显式 `ExpandedKeys` 包含禁用分支 key，原生菜单也不会再把禁用子树展开出来，避免外部状态把非法交互语义重新注入回 DOM；
- `OnBranchToggled` 回写 `ExpandedKeysChanged` 时现在会基于当前菜单树做归一化，只保留当前仍有效、仍可展开的 branch key，不再把禁用分支或不存在的 key 持续回传给外部状态；
- 带 `Target` 的禁用叶子节点不再保留真实 `href`，避免“逻辑上 disabled、浏览器上仍可跳转”的错误语义；
- `VbenRouteLocation` 在原生菜单中已收口到统一导航目标解析，不再只支持纯字符串 `href`；
- 原生菜单现在明确区分两类导航目标：字符串 `Target` 继续输出原始 `href`，`VbenRouteLocation` 则统一输出原生 `router-link` / `to`；
- `VbenRouteLocation.Path/Name/Hash` 的原生解析规则已经稳定：`Path` 优先于 `Name`，`Hash` 会统一补 `#` 前缀，非空 `href/path/name/hash` 会先做首尾空白归一化，空白 `href`/route 字段不会再被当作可导航目标；
- 菜单 key 现在也进入统一规范化主链：空白 `Key` 菜单项会在渲染前被忽略，首尾空格 `Key` 不会再污染选中态、展开态、DOM `data-key` 或 `SelectedKeyChanged` / `ExpandedKeysChanged` payload；
- 原生 sidebar 现在还会基于规范化后的 key 构建一棵“有效唯一菜单树”：同一棵菜单树中，后出现的重复 key 项会被整体忽略，不再制造重复 `data-key`、歧义选中态、冲突展开态或重复回调来源；
- nav item `Title` 现在也会在原生输出前裁剪首尾空格，避免后台菜单文案把脏 authoring 文本直接落进链接或按钮节点；
- 当 `Logo` 与有效菜单项都不存在时，原生 `SidebarMenu` 不再输出空的根 `nav` / `ul` wrapper；
- 当仅提供 `Logo` 时，原生 `SidebarMenu` 会保留 logo region，但不会附带空的菜单列表；
- 当 `Logo` slot 实际只产出空内容、纯空白文本或仅 HTML 注释 markup 时，原生 `SidebarMenu` 也会把它视为无效 branding 内容，不再制造空的根 `nav` 或 logo wrapper；
- `Items` 与 `Children` 现在会在渲染前统一过滤 `null` 项，默认 native 主链不再因脏菜单数组输入而抛出 `NullReferenceException`。
- 当前 native 菜单只把标题非空白的 `VbenNavItem` 视为有效可渲染项；空白标题菜单项会在渲染前被忽略，不再制造空链接、空按钮或空侧栏结构。

当前对 `VbenPageContainer` 的原生行为补充也已进入主线：

- breadcrumb 不再退化为纯文本；当 `Target` 是字符串 `href` 时会输出真实可导航锚点；
- breadcrumb 的 `VbenRouteLocation.Path/Name/Hash` 现在会收口到和 sidebar 相同的原生 `router-link` 导航目标解析规则；
- 禁用 breadcrumb 即使携带 `Target`，也不会再输出真实 `href`，而是输出带 `aria-disabled` 的非导航节点；
- action 不再一律退化为朴素按钮；当 `Target` 可导航且未禁用时会按目标语义输出真实锚点或 `router-link`；
- action `Kind` 已稳定映射为原生语义类（`default/primary/secondary/link/danger`），方便后续样式与容器实现保持语义一致；
- 禁用 action 即使携带 `Target`，也不会保留可跳转 `href`，并会显式带上禁用语义状态。
- `Title` / `Subtitle` / breadcrumb `Title` / action `Text` 在原生输出前都会统一裁剪首尾空格，避免 page header 把带 padding 的显示文本直接带进 DOM；
- 仅包含空白 `Title` 的 breadcrumb 项和仅包含空白 `Text` 的 action 项现在会在渲染前被忽略，不再把脏输入误判为有效 page header 语义，也不会再生成空链接、空按钮或空 header 区域；
- 当页面没有任何 header 语义内容时，原生 `PageContainer` 不再输出空的 `vben-page__header` / `vben-page__titles` wrapper；
- 当只有 `Extra` 区域时，原生 `PageContainer` 会保留 actions region，但不会附带空的 titles 容器；
- 当 `Extra` slot 实际只产出空内容、纯空白文本或仅 HTML 注释 markup 时，原生 `PageContainer` 会把它视为无效 header 内容，不再额外挂出空的 actions/header wrapper；
- `BreadcrumbItems` / `Actions` 现在会在渲染前过滤 `null` 项，默认 native 主链不再因脏数组输入而抛出 `NullReferenceException`。

当前对 `VbenAdminLayout` 的原生模式语义也已收口：

- `Mode.Top` 不再错误输出侧栏区域，也不会再默认挂出 `VbenSidebarMenu`；
- `Mode.Sidebar` 与 `Mode.Mixed` 继续保留侧栏区域，保持后台壳层与混合布局的结构能力；
- 当既没有自定义 `Sidebar`，也没有 `Logo` 或有效 `NavItems` 时，原生 `AdminLayout` 不再输出空的 `vben-shell__sidebar` 区域；
- 当 `NavItems` 只包含空白标题菜单项时，也会被视为“没有有效 sidebar 内容”，不再错误挂出默认侧栏区域；
- 当 `Logo` 存在时，即使没有 `NavItems`，默认 native 主链也会继续挂出 `VbenSidebarMenu`，保持后台壳层 branding 入口；
- 共享 `Logo` 的默认布局语义已经明确：`Top` 模式把它视为 header branding，`Sidebar/Mixed` 模式把它视为 sidebar branding，不再把同一个 fragment 同时重复渲染到默认 sidebar 与默认 header；
- 当没有任何 header 内容时，原生 `AdminLayout` 不再输出空的 `vben-shell__header` 区域，也不会默认挂出空 `VbenHeaderBar`；
- 当显式提供 `Header` 或默认 header props 真正存在时，`AdminLayout` 才会输出 header region；
- 当 `Header` / `Sidebar` / `HeaderActions` / `UserRegion` slot 实际只产出空内容、纯空白文本或仅 HTML 注释 markup 时，原生 `AdminLayout` 不会再把它们误判为“有内容”，也不会挂出空的 shell header / sidebar 区域；
- 原生 `AdminLayout` 的模式分支现在与现有参考实现保持一致，不再出现同一 public contract 在 native / reference implementation 之间的结构漂移。

当前对 `VbenHeaderBar` 的原生 DOM 语义也已收口：

- 当 `Title` / `Subtitle` 都为空时，不再输出空的 titles 容器；
- `Title` / `Subtitle` 在原生输出前会先裁剪首尾空格，不再把带 padding 的 header 文本直接落进 DOM；
- 当 `Actions` / `UserRegion` 都为空时，不再输出空的右侧 actions 容器；
- 当 `Logo` / `Title` / `Subtitle` 都为空时，不再输出空的 `vben-header__main` 左侧主区域；
- 当整个 `HeaderBar` 没有任何可见内容时，原生默认实现不再输出空的根 wrapper；
- 当 `Logo` / `Actions` / `UserRegion` slot 实际只产出空内容、纯空白文本或仅 HTML 注释 markup 时，也会被视为“无有效内容”，不会再泄漏空的 logo / toolbar / user-region wrapper；
- `Actions` 与 `UserRegion` 现在有独立的原生语义 wrapper，后续样式、对齐与壳层组合可以稳定落在明确 DOM 边界上；
- 原生 `HeaderBar` 不再把所有右侧内容直接平铺到同一个裸容器里，减少了样式耦合和空结构噪音。

当前对 `ECMAScript.Vben.Test` 的 native 回归测试基础设施也已进一步收口：

- `VbenNativeRenderTreeTestHelper` 已集中承担 native 组件参数注入、反射渲染与 render-tree 断言扫描；
- `VbenNativeAdminLayoutTests` / `VbenNativeHeaderBarTests` 不再通过对象初始化器直接写 `[Parameter]`，消除了 `BL0005` 噪音；
- native render-tree 测试对 `Microsoft.AspNetCore.Components.RenderTree` 的使用已收口在共享 helper 中，不再让 `BL0006` 漏到套件输出；
- 当前 `ECMAScript.Vben.Test` 全量通过时已不再伴随这两类已知测试警告。

### 3. 公共语义模型已经建立

当前已进入 `src/ECMAScript.Vben/` 的核心语义包括：

- `VbenLayoutMode`
- `VbenPageActionKind`
- `VbenRouteLocation`
- `VbenNavTarget`
- `VbenPageAction`
- `VbenBreadcrumbItem`
- `VbenNavItem`
- `VbenNavItems`
- `VbenAdminLayoutState`

这些模型的价值不在于“数量已经很多”，而在于：

- 后台壳层最常用的数据面已经有强类型表达；
- 没有退化回 `object?`/`Dictionary<string, object?>` 这类弱 contract；
- 菜单目标、布局状态、页面动作这些常用语义已经可直接被 Razor authoring 消费。

### 4. Vue authoring contract 已完成一次关键收口

当前 Vben 线与 RazorVue/VueContract 的关系已经收口为通用 authoring contract：

- 组件 prop 使用 `[Parameter]`，仅在 Vue 名称不同时使用 `[ECMAScriptName]`
- 组件 slot 使用 `RenderFragment` / `RenderFragment<T>`，事件使用 `EventCallback` / `EventCallback<T>`
- `CssClass` / `CssStyle` 走 Vue3 通用值域
- `AdditionalAttributes` 是唯一允许的宽口透传入口

这件事的意义很直接：

- Vben 不需要再拥有一套“自己专用但本质重复”的 prop/slot 元数据命名；
- 后续 native component、library component、RazorVue descriptor extraction 可以共享同一套通用 authoring 语义；
- 这条线对未来更多组件库协同是减耦，而不是增耦。

### 5. 容器注入扩展边界已开始落地

当前已经落地的判断是：

- `Vben` 不需要额外定义 `IVbenUiAdapter`
- 容器 contract 统一复用 `ECMAScript.VueContract`
- 第三方库或应用层想替换整个结构组件时，应实现 `IVueContainerImplementation<TContainer>`
- 当前装配选择通过 `[assembly: VueInject(...)]`

这意味着“可替换实现”已经进入正式主线，但仍然没有把 `ECMAScript.Vben` 重新做成第三方库 adapter 包。

### 6. 真实消费路径已经补到 sample 层

截至 2026-05-15，`Vben` 线不再只有 focused tests，也已经有了真实 sample 级消费路径：

- sample 名称：`samples/ECMAScript.Vben.ElementPlusInject/`
- authoring 面：根组件继续只写 `VbenAdminLayout` / `VbenHeaderBar` / `VbenSidebarMenu` / `VbenPageContainer`
- 具体实现面：四个容器都由 sample-local 用户组件实现 `IVueContainerImplementation<TContainer>`
- 第三方库协同面：具体实现内部组合 `ECMAScript.ElementPlus` library component，而不是把 `Element Plus` 直接泄漏进 `ECMAScript.Vben` 公共 contract
- consumer 面：使用 `deno.json` + `scripts/run-deno.cs` + 官方 `razorvue-consumer-entry`
- smoke 面：SSR / `Deno.bundle()` / browser build / browser smoke 全部在 Deno 链路完成

这条路径的意义是：

- 它证明 Vben 当前的抽象不是“只能在测试里成立”；
- 它证明 sample-local user component 实现足以承接第三方库协同，不需要正式 `ECMAScript.Vben.ElementPlus` 产品包；
- 它把 sample 的前端消费路径固定在 `deno.json` + `scripts/run-deno.cs` + `razorvue-consumer-entry`，和当前仓库对前端运行时主线的判断保持一致。

## 当前验证基线

当前已形成的聚焦验证包括：

- `src/ECMAScript.Vben/ECMAScript.Vben.csproj` 可独立构建；
- `src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj` 可独立测试；
- `VbenAuthoringSurfaceTests` 校验：
  - 仅 `AdditionalAttributes` 允许承接 `object?` 风格宽口；
  - 所有原生组件统一暴露 `CssClass` / `CssStyle`；
  - `Vben` public contracts 不泄漏 `TDesign` / `ElementPlus` / `Vuetify` 类型；
  - RazorVue 组件注册器可以把 `ECMAScript.Vben` 中的原生组件作为用户组件发现并解析；
- `VbenContainerInjectTests` 校验：
  - 公开壳层组件确实声明为 `IVueContainerComponent`；
  - 默认解析路径仍使用 `ECMAScript.Vben` 原生实现；
  - `VbenAdminLayout` / `VbenHeaderBar` / `VbenSidebarMenu` / `VbenPageContainer` 四个公开壳层组件都已有默认 native descriptor / registry resolution 回归；
  - `[VueInject]` 可以把 `VbenAdminLayout` / `VbenSidebarMenu` / `VbenHeaderBar` / `VbenPageContainer` 切换到第三方 library implementation，同时保留 `Vben` authoring contract；
  - `VbenPageContainer` 的 injected runtime shape 已进入 Vue SFC artifact 与 pipeline artifact 回归；
  - `VbenAdminLayout` 的 injected runtime shape 已进入 Vue SFC artifact 与 pipeline artifact 回归，覆盖 model prop / model emit / kebab-case slot 映射；
  - `VbenHeaderBar` 的 injected runtime shape 已进入 Vue SFC artifact 与 pipeline artifact 回归；
  - `VbenSidebarMenu` 的 injected runtime shape 已进入 Vue SFC artifact 与 pipeline artifact 回归；
  - 多壳层组合页已进入 Vue SFC artifact 与 pipeline artifact 回归，覆盖 `VbenAdminLayout` / `VbenHeaderBar` / `VbenSidebarMenu` / `VbenPageContainer` 同时注入时的 import 聚合、style/plugin 去重聚合、嵌套 slot 转译与 model prop / emit 映射稳定性；
  - `missing prop` / `prop type mismatch` / `emit payload mismatch` / `default slot mismatch` / `CaptureUnmatchedValues mismatch` / `duplicate [VueInject]` / `mismatched IVueContainerImplementation<TContainer>` 都已有 focused failure-path 回归；
- `VbenNativeSidebarMenuTests` 校验：
  - 无 `Logo` / `Items` 时不会再输出空的 sidebar 根与列表容器；
  - 仅 `Logo` 场景会保留 logo region，但不会附带空列表；
  - `Logo` slot 仅输出空内容或纯空白文本时，也不会再制造空的 sidebar 根或 logo wrapper；
  - `Logo` slot 仅输出 HTML 注释 markup 时，也不会再被误判为有效 branding 内容；
  - `Items` / `Children` 中的 `null` 项会被忽略，不会导致默认 native 渲染抛异常；
  - 空白 `Key` 菜单项会被忽略，不会制造空的 sidebar 根、列表或导航元素；
  - 首尾空格 `Key` 会在选中态、展开态、DOM `data-key` 与回调 payload 上统一裁剪，不再让脏 key 把内部状态与输出结构带偏；
  - 规范化后重复的菜单 key 会按 first-win 构建有效唯一菜单树，后出现的冲突项不会再参与渲染、选中、展开或回调；
  - nav item `Title` 在原生输出前会先裁剪首尾空格，不再把带 padding 的菜单文案直接落进链接或按钮文本节点；
  - `ExpandedKeys` 显式状态与 `SelectedKey` 祖先展开回退的优先级；
  - 显式空 `ExpandedKeys` 覆盖默认回退；
  - 选中节点与祖先选中链状态；
  - 禁用分支不会被计为可导航或可展开子树；
  - 禁用分支即使出现在显式 `ExpandedKeys` 中，也不会渲染展开态或子列表；
  - `OnBranchToggled` 产生的 `ExpandedKeysChanged` payload 会自动剔除禁用分支 key 与当前菜单树不存在的 key；
  - 空白标题菜单项不会被计为有效菜单内容，也不会制造空的 sidebar 根、列表或导航元素；
  - 禁用 `href` 叶子节点最终输出为禁用按钮而不是可导航链接；
  - `VbenRouteLocation` 叶子节点会输出原生 `router-link` / `to`，不再退化成字符串 `href`；
- `VbenNativePageContainerTests` 校验：
  - 无 header 内容时不会再输出空的 `header` / `titles` 容器；
  - 仅 `Extra` 内容时不会再附带空的 titles 容器；
  - `Extra` slot 仅输出空内容或纯空白文本时，也不会再制造空的 `header` / `actions` 容器；
  - `Extra` slot 仅输出 HTML 注释 markup 时，也不会再被误判为有效 header 内容；
  - breadcrumb `href` 目标会输出可导航锚点；
  - breadcrumb `route path/name/hash` 目标会输出原生 `router-link` / `to`；
  - 禁用 breadcrumb 不会再输出可导航锚点；
  - action `href` 目标会输出可导航锚点；
  - action `route path/name/hash` 目标会输出原生 `router-link` / `to`；
  - action `Kind` 会落为稳定原生语义类；
  - 禁用 action 即使携带 `Target` 也不会再输出可跳转链接；
  - `BreadcrumbItems` / `Actions` 中的 `null` 项会被忽略，不会导致默认 native 渲染抛异常；
  - `Title` / `Subtitle` / breadcrumb `Title` / action `Text` 在原生输出前会统一裁剪首尾空格，不再把带 padding 的显示文本直接带进 page header；
  - 仅包含空白标题/文本的 breadcrumb/action 项会被忽略，不会制造空的 page header 内容；
- `VbenNavigationTargetResolverTests` 校验：
  - `Path` 分支会规范化 `Hash`；
  - `Path + Name` 并存时原生解析稳定采用 `Path` 分支；
  - `Name` / `Hash` 相对目标会保留为可导航 `to` 对象；
  - 仅 `Hash` 的相对目标仍保持可导航；
  - 非空 `href/path/name/hash` 会在原生解析阶段统一裁剪首尾空白，避免脏导航目标把空格直接落进 DOM / route object；
  - 空白字符串 `href` 不会再被误判为可导航目标；
- `VbenNativeArtifactTests` 当前额外覆盖：
  - 原生 `router-link` 元素携带 route-object 参数时，Vue SFC artifact 会稳定保留 `:to="props.target"`；
  - 同一构造在 pipeline artifact 中会稳定保留 `h("router-link", { "to": props.target, ... })`；
- `VbenNativeAdminLayoutTests` 校验：
  - `Mode.Top` 不会再渲染侧栏区域或默认 `VbenSidebarMenu`；
  - `Mode.Sidebar` 在没有任何有效 sidebar 内容时不会再输出空的 `aside` 区域；
  - `Mode.Sidebar` 在 `NavItems` 只包含空白标题菜单项时也不会再输出默认 sidebar 区域；
  - 无 header 内容时不会再输出空的 shell header 区域或默认 `VbenHeaderBar`；
  - `Mode.Sidebar` 在存在 `NavItems` 时会继续渲染默认 `VbenSidebarMenu`；
  - `Mode.Sidebar` 在仅存在 `Logo` 时也会继续渲染默认 `VbenSidebarMenu`；
  - `Mode.Sidebar` 在仅存在共享 `Logo` 时不会再重复渲染默认 header region；
  - `Mode.Mixed` 会继续保留侧栏区域并承接自定义 `Sidebar` 内容；
  - 显式 `Header` 与默认 header props 场景下仍会正确输出 header region；
  - `Header` / `Sidebar` / `HeaderActions` / `UserRegion` 仅输出空内容或纯空白文本时，不会再制造空的 shell header / sidebar 区域；
  - `Header` / `Sidebar` / `HeaderActions` / `UserRegion` 仅输出 HTML 注释 markup 时，也不会再制造空的 shell header / sidebar 区域；
- `VbenNativeHeaderBarTests` 校验：
  - 整个组件无内容时不会再输出空根 wrapper；
  - 无 logo/titles 时不会再输出空的 `main` 区域；
  - 无标题时不会再输出空的 titles 容器；
  - `Title` / `Subtitle` 在原生输出前会统一裁剪首尾空格，不再把带 padding 的 header 文本直接带进 DOM；
  - 无 actions / user-region 时不会再输出空的右侧容器；
  - `Logo` / `Actions` / `UserRegion` 仅输出空内容或纯空白文本时，也不会再制造空的 main/logo/actions/toolbar/user-region wrapper；
  - `VbenRenderFragmentHelper` 会把 comment-only markup 归一化为无效内容，并保证可见 slot 内容只捕获一次、回放一次，不会为判空重复执行原始 fragment；
  - `Actions` 与 `UserRegion` 会落到独立语义 wrapper 中；
  - 仅右侧区域场景不会再附带空的 `main` wrapper；
- `VbenNativeArtifactTests` 校验：
  - `VbenAdminLayout` 在无 sidebar 内容场景下不会 lower 出空的 shell sidebar 区域；
  - `VbenAdminLayout` 在无 header 内容场景下不会 lower 出空的 shell header 区域；
  - `VbenHeaderBar` 在仅右侧区域场景下不会 lower 出空的 `main` wrapper；
  - 默认 native 多壳层组合在无 `[VueInject]` 情况下可稳定 lower 为 Vue SFC artifact；
  - 默认 native 多壳层组合在无 `[VueInject]` 情况下可稳定 lower 为 pipeline artifact；
  - `VbenAdminLayout` / `VbenHeaderBar` / `VbenSidebarMenu` / `VbenPageContainer` 的默认 native import 路径、slot 映射与 typed callback 透传已进入回归；
  - `VbenPageContainer` 在“仅 `Extra`、无标题区”场景下不会 lower 出空的 titles wrapper；
  - 默认 native 主链不会意外引入第三方 style/plugin requirement；
- Vue authoring 回归覆盖 `[Parameter]`、按需的 `[ECMAScriptName]`、`RenderFragment` 和 `EventCallback` 的 descriptor extraction。

2026-05-17 当前基线复核：

- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~VbenNativeSidebarMenuTests|FullyQualifiedName~VbenNativeAdminLayoutTests|FullyQualifiedName~Vben_AdminLayout_DefaultNativeComponent_WithoutSidebarContent_DoesNotLowerEmptySidebarRegion' -v minimal`：已通过（16/16）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~VbenNativeHeaderBarTests|FullyQualifiedName~VbenNativeAdminLayoutTests|FullyQualifiedName~Vben_AdminLayout_DefaultNativeComponent_WithoutHeaderContent_DoesNotLowerEmptyHeaderRegion|FullyQualifiedName~Vben_HeaderBar_DefaultNativeComponent_WithOnlyUserRegion_DoesNotLowerEmptyMainRegion' -v minimal`：已通过（11/11）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~VbenNativePageContainerTests|FullyQualifiedName~Vben_PageContainer_DefaultNativeComponent_WithOnlyExtra_DoesNotLowerEmptyTitlesRegion' -v minimal`：已通过（10/10）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~VbenNativeAdminLayoutTests|FullyQualifiedName~VbenNativeHeaderBarTests|FullyQualifiedName~VbenNativePageContainerTests|FullyQualifiedName~VbenNativeSidebarMenuTests|FullyQualifiedName~DefaultComponentRegistryResolution' -v minimal`：已通过（32/32）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~VbenContainerInjectTests' -v minimal`：已通过（23/23）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~VbenContainerInjectTests|FullyQualifiedName~VbenAuthoringSurfaceTests|FullyQualifiedName~VueAuthoringMetadataTests' -v minimal`：已通过；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~Vben_MultiShell_ContainerInject' -v minimal`：已通过（2/2）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~VbenNavigationTargetResolverTests|FullyQualifiedName~VbenNativePageContainerTests|FullyQualifiedName~VbenNativeSidebarMenuTests' -v minimal`：已通过（25/25）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~VbenNativeSidebarMenuTests' -v minimal`：已通过（27/27）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~VbenNativePageContainerTests' -v minimal`：已通过（13/13）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~VbenNativeAdminLayoutTests' -v minimal`：已通过（9/9）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~VbenNativeHeaderBarTests' -v minimal`：已通过（7/7）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~Vben_RouterLinkElement_WithRouteObject_LowersIntoVueSfcArtifact|FullyQualifiedName~Vben_RouterLinkElement_WithRouteObject_LowersIntoPipelineArtifact' -v minimal`：已通过（2/2）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~Vben_MultiShell_DefaultNativeComponents' -v minimal`：已通过（2/2）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~VbenNativeSidebarMenuTests|FullyQualifiedName~VbenAuthoringSurfaceTests|FullyQualifiedName~VbenContainerInjectTests' -v minimal`：已通过（49/49）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~VbenNativeSidebarMenuTests|FullyQualifiedName~VbenNavigationTargetResolverTests|FullyQualifiedName~VbenNativeAdminLayoutTests' -v minimal`：已通过（29/29）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~VbenNativeSidebarMenuTests|FullyQualifiedName~VbenNativeAdminLayoutTests|FullyQualifiedName~VbenNativePageContainerTests|FullyQualifiedName~VbenNavigationTargetResolverTests' -v minimal`：已通过（43/43）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~VbenNativePageContainerTests|FullyQualifiedName~VbenNavigationTargetResolverTests|FullyQualifiedName~VbenNativeArtifactTests' -v minimal`：已通过（17/17）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj -v minimal`：已通过（123/123）；测试层当前输出未再出现 `BL0005` / `BL0006`；
- `samples/ECMAScript.Vben.ElementPlusInject/verify-smoke.cs`：已纳入当前阶段的真实 sample 验证入口，目标覆盖本地 package pack、host rebuild、host requirements 断言、Deno SSR smoke、Deno bundle smoke、browser build、browser smoke；
- `RazorVue` 聚焦回归已覆盖 Vben 相关 descriptor / authoring contract 收口，不再依赖旧兼容别名。

## 当前仍未完成的部分

以下内容当前不应被误判为“Vben 已经具备”：

- 完整业务后台框架级表单/表格/schema 引擎；
- 权限系统、主题系统、国际化闭环；
- 多 UI 库 feature parity；
- 与现有 TS `src/vben` 工程等价的产品面积；
- 大面积 sample 级第三方容器实现矩阵。

这不是缺陷陈述，而是当前阶段边界。

## 下一步行动

### 1. 稳定原生壳层行为

优先补足：

- 布局状态与交互行为的更细粒度验证；
- 导航项层级/选中/展开的边界测试；
- 原生壳层组合行为的更细粒度 lowering 回归。

### 2. 扩展容器注入覆盖面

优先补足：

- sample 级第三方容器实现映射，验证真实应用组合方式；
- 继续扩展 sample 中的多壳层同时注入场景与交互细节，补真实目录结构和消费方式；

### 3. 补状态文档与实现对应关系

后续每次推进都应继续维护三层文档一致性：

- `docs/01-目标/ecmascript.vben/`：为什么这样设计；
- `docs/02-计划/ecmascript.vben/`：下一步怎么做；
- `docs/03-完成/ecmascript.vben/`：当前已经落到了哪里。

### 4. 把第三方库协同留在应用层或 sample 层

如果后续要验证 `ElementPlus`/`TDesign` 协同，优先策略应是：

- 在 sample 中做组合验证；
- 在应用层引入第三方组件；
- 不把 `ECMAScript.Vben.*第三方库*` 再推回正式产品边界。

## 参考

- [ECMAScript.Vben README](../../../src/ECMAScript.Vben/README.md)
- [ECMAScript.Vben 平衡式设计](../../01-目标/ecmascript.vben/vben-balanced-design.md)
- [ECMAScript.Vben 第一阶段实施计划](../../02-计划/ecmascript.vben/ECMAScript.Vben.Phase1.ImplementationPlan.md)
- [docs/03-完成/README.md](../README.md)
