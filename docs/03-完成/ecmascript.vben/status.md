# ECMAScript.Vben 状态（2026-05-15）

> Status: 当前状态快照  
> Positioning: `src/ECMAScript.Vben/` 后台壳层抽象与原生 RazorVue 实现的仓库级状态快照  
> Scope: 原生壳层组件、公共后台语义模型、Vue authoring contract 收口、独立测试基线与产品边界

## 总结

`ECMAScript.Vben` 已完成第一轮仓库级落地，但当前仍应被视为 **Phase 1 原生壳层闭环已形成、后续进入稳定化和扩展期**，而不是完整后台框架产品已经做完。

这一轮已经明确并落地的核心事实是：

- `ECMAScript.Vben` 的定位是 **抽象层 + 自有原生实现**，不是某个第三方 UI 库的 adapter 命名空间；
- 原生组件继续走 RazorVue 用户组件路径：`ComponentBase + IVueComponent + [ECMAScriptModule(...)]`；
- 公开壳层组件现在同时承担 `IVueContainerComponent` 容器契约角色，可被应用层通过 `[VueInject]` 编译期替换到具体实现；
- `CssClass` / `CssStyle` / `AdditionalAttributes` 等 authoring 基底统一建立在 `ECMAScript.Vue3` 与通用 Vue contract 上；
- 通用 prop/slot 元数据已经收口到 `VueProp` / `VueSlot`，不再把 `VueLibraryProp` / `VueLibrarySlot` 兼容别名当作现行契约；
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

当前 Vben 线与 RazorVue/VueContract 的关系已经从“库专属元数据”收口为“通用 authoring metadata”：

- 组件 prop/slot 覆盖走 `VueProp` / `VueSlot`
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
  - `[VueInject]` 可以把 `VbenAdminLayout` / `VbenSidebarMenu` / `VbenHeaderBar` / `VbenPageContainer` 切换到第三方 library implementation，同时保留 `Vben` authoring contract；
  - `VbenPageContainer` 的 injected runtime shape 已进入 Vue SFC artifact 与 pipeline artifact 回归；
  - `VbenAdminLayout` 的 injected runtime shape 已进入 Vue SFC artifact 与 pipeline artifact 回归，覆盖 model prop / model emit / kebab-case slot 映射；
  - `VbenHeaderBar` 的 injected runtime shape 已进入 Vue SFC artifact 与 pipeline artifact 回归；
  - `VbenSidebarMenu` 的 injected runtime shape 已进入 Vue SFC artifact 与 pipeline artifact 回归；
  - 多壳层组合页已进入 Vue SFC artifact 与 pipeline artifact 回归，覆盖 `VbenAdminLayout` / `VbenHeaderBar` / `VbenSidebarMenu` / `VbenPageContainer` 同时注入时的 import 聚合、style/plugin 去重聚合、嵌套 slot 转译与 model prop / emit 映射稳定性；
  - `missing prop` / `prop type mismatch` / `emit payload mismatch` / `default slot mismatch` / `CaptureUnmatchedValues mismatch` / `duplicate [VueInject]` / `mismatched IVueContainerImplementation<TContainer>` 都已有 focused failure-path 回归；
- `VueAuthoringMetadataTests` 校验：
  - 通用 `VueProp` / `VueSlot` 元数据可被 RazorVue descriptor extraction 正常识别；
  - `ECMAScript.VueContract` 当前只保留 canonical 的 `VuePropAttribute` / `VueSlotAttribute`。

2026-05-15 当前基线复核：

- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~VbenContainerInjectTests' -v minimal`：已通过（23/23）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~VbenContainerInjectTests|FullyQualifiedName~VbenAuthoringSurfaceTests|FullyQualifiedName~VueAuthoringMetadataTests' -v minimal`：已通过；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj --filter 'FullyQualifiedName~Vben_MultiShell_ContainerInject' -v minimal`：已通过（2/2）；
- `dotnet test src/ECMAScript.Vben.Test/ECMAScript.Vben.Test.csproj -v minimal`：已通过（30/30）；
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
