# ECMAScript.Vuetify 剩余完善清单

> Status: 初始可用后的活跃清单  
> Updated: 2026-05-09  
> Positioning: 基于 `src/ECMAScript.Vuetify/` 当前真实代码、`src/Jazor.RazorVue.Test/` / `src/Jazor.CompilerTest/` / `src/Jazor.EmitTest/` 现有守护，以及 `docs/01-目标/razorvue/design/RazorVue.Vuetify.FirstPackage.md` 的设计边界整理的下一阶段执行清单。  
> Scope: 当前关注点不是“是否存在 Vuetify 代理”，而是“作为 `ECMAScript.Vue3` 环境下的 Vuetify 代理层，代理是否足够完整、authoring 是否足够易用、自定义参数与 CSS 路径是否足够清晰可生产使用”。

## 1. 当前判断

`ECMAScript.Vuetify` 现在已经具备“两层能力”：

- `ECMAScript.Vue3` 之上的 Vuetify runtime proxy；
- RazorVue authoring 组件桩与对应的描述符/emit/host-requirement 接入。

当前更准确的状态是：

- **runtime proxy 基础可用**
- **Razor authoring 层仍是第一包、窄表面积、精选 props 的薄桩**

当前已经被验证的能力包括：

- `Vuetify.CreateVuetify()` / `CreateVuetify(VuetifyOptions)` 的强类型 runtime 代理；
- `vuetify/components` / `vuetify/directives` 的导入与 registry lowering；
- `vuetify/styles` 与 `vuetify` plugin requirement 的 host requirement 汇总；
- 一批高频组件的 RazorVue library component lowering；
- `ModelValue + ModelValueChanged`、`EventCallback`、默认槽、`RenderFragment<TContext>` 作用域槽等 Blazor 风格 authoring。

但当前也有明确边界：

- authoring 组件桩并不是 Vuetify 全量 props/emits/slots 镜像；
- 当前 authoring 层没有统一 arbitrary props 透传入口；
- `class` / `style` / `href` / `variant` / `size` / `color` 这类真实项目高频参数在多个组件上仍未统一补齐；
- sample 证明了“宿主手动导入 Vuetify 样式和安装插件”的工作流，但这套约定还没有沉淀成正式产品级 contract 文档。

## 2. 当前主线项

### 2.1 先把“运行时代理完整”和“authoring 桩完整”分开治理

当前最大风险不是 runtime import 失效，而是两层职责混在一起后，容易误以为：

- 既然 `VuetifyComponents` / `VuetifyDirectives` 很全，
- 那么 Razor authoring 组件桩也应该同样全。

后续需要明确分层：

- runtime proxy coverage
- RazorVue authoring coverage
- host bootstrap contract

建议后续所有补齐项都明确标注属于哪一层，避免继续用“Vuetify 支持了没有”这种模糊表述。

### 2.2 提升高频 authoring surface 的完整度

当前 authoring 组件数已经不算太少，但多个组件仍然只是“最小可演示表面”。

下一阶段应优先补高频真实参数，而不是平均摊薄到长尾组件。

推荐优先级：

- `VBtn`
  - `Color`
  - `Variant`
  - `Size`
  - `Icon`
  - `Href`
  - `Target`
  - `Loading`
- `VTextField`
  - `Placeholder`
  - `Hint`
  - `PersistentHint`
  - `Type`
  - `Clearable`
  - `Readonly`
  - `Density`
  - `Variant`
- `VSelect` / `VAutocomplete`
  - `Items`
  - `ItemTitle`
  - `ItemValue`
  - `ReturnObject`
  - `Chips`
  - `Closables` / `Clearable`
  - `MenuProps`
- `VDialog`
  - `Persistent`
  - `MaxWidth`
  - `Width`
  - `ScrollStrategy`
  - `Location`
- `VDataTable`
  - 更强类型的 header/item surface
  - 常用分页/排序 props
  - 关键命名槽

补齐原则：

- 优先补“业务真实高频 + 官方常见文档路径 + 当前 sample/test 已经接近”的交集；
- 不机械复制 Vuetify 全字段；
- 每补一批 props/slots，都补 descriptor extraction、pipeline lowering、emit/manifest 守护。

### 2.3 建立统一 arbitrary props 透传 contract

这是当前 authoring 可用性最大的短板之一。

现状：

- 对未声明参数，RazorVue 会报 `UnknownParameter`；
- 当前 `ECMAScript.Vuetify` 组件桩里没有统一的 `[Parameter(CaptureUnmatchedValues = true)] AdditionalAttributes` sink；
- 因此真实项目里常见的：
  - `class`
  - `style`
  - `id`
  - `data-*`
  - `aria-*`
  - 尚未建模的 Vuetify props
  都无法先透传再逐步建模。

建议下一阶段补齐：

1. 为高频组件引入统一 `AdditionalAttributes`
2. 明确透传优先级与覆盖规则
3. 为 `AddMultipleAttributes(...)` authoring 加专项回归
4. 对已显式建模的组件提供 `CssClass` / `CssStyle` 强类型入口，并保持 lowercase `class` / `style` 走 `AdditionalAttributes`

注意：

- arbitrary props 透传是过渡层，不应成为永远不补强类型 props 的借口；
- 但没有这层过渡，真实项目 authoring 体验会非常僵硬。

### 2.4 梳理样式 contract：Vuetify 样式、自定义组件样式、消费端样式职责

当前样式链路能工作，但“职责边界”只体现在 sample 和测试里，还不够产品化。

当前事实：

- `ECMAScript.Vuetify` 组件桩通过 `VueLibraryStyle("vuetify/styles")` 把 Vuetify 样式声明为 host requirement；
- 生成的 `.vue` 工件不会自动注入 `<style src="vuetify/styles">`；
- consumer 侧需要显式 `import "vuetify/styles"` 并安装 `createVuetify()`；
- `.razor.css` / style-hash / styles manifest 路径在底层是存在的，但 `ECMAScript.Vuetify` 文档里没有把“业务 CSS 怎么写、谁来 import、谁来收口”讲清楚。

下一阶段需要补齐的不是样式能力本身，而是正式 contract：

1. 明确 `vuetify/styles` 由宿主负责导入
2. 明确 `createVuetify()` 由宿主负责安装
3. 明确 `.razor.css` / library-local style 的推荐路径
4. 明确“Vuetify global theme/style”与“业务组件局部样式”的边界
5. 明确 consumer 不遵守 host requirement 时的失败模式

### 2.5 提升复杂值与集合 authoring 的强类型程度

当前多个组件为了先跑通 authoring，用了较宽但不够友好的类型：

- `IEnumerable<object>?`
- `string? ModelValue`
- 若干未分层 union 的简化版 surface

这些类型在早期是合理的，但继续停留会影响生产 authoring：

- IntelliSense 弱；
- 真实 items/header/options 场景表达能力差；
- 容易逼用户退回手写低层 `ECMAScript.Vue3` object authoring。

建议增量设计：

- `VDataTableHeader` / `VDataTableItemKey` 等小型 host types
- `VSelectItem<T>` / `VAutocompleteItem<T>` 一类强类型集合入口
- 只在 C# 无法自然表达的地方再引入 `From(...)` 工厂，而不是默认回退到 `object`

### 2.6 组件槽建模继续从“默认槽”走向“命名槽/作用域槽”

当前 `VDialog.Activator` 已经证明作用域槽路径可行，但大多数组件仍停留在：

- 默认槽
- 极少量命名槽

下一阶段建议优先补：

- `VTooltip.Activator`
- `VMenu.Activator`
- `VDataTable` 常用 item/header slot
- `VSelect` / `VAutocomplete` item/selection slot
- `VListItem` prepend/append/default 相关常用槽

原则：

- 先补真实高频命名槽；
- 对需要上下文的槽，优先提供 `RenderFragment<TContext>`；
- 不为了“槽覆盖率”而一次性引入大批没有验证价值的上下文类型。

## 3. 需要先设计再实现的项

### 3.1 `class` / `style` 与 `CssClass` / `CssStyle` 的分层规则

这是 authoring 体验里的关键设计决策。

当前规则：

- Razor 组件标签上的 lowercase `class` / `style` 走 `AdditionalAttributes` fallthrough
- 需要强类型 C# 表达式时，已显式建模组件提供 `CssClass` / `CssStyle`
- `CssClass` / `CssStyle` 通过 `VueProp` 映射到 Vue runtime 的 `class` / `style`

不要在 top-level Vuetify authoring component 上提供 `[Parameter] Class` / `[Parameter] Style`。这些名称会与官方 Razor SG 对 lowercase `class` / `style` 的组件参数绑定规则冲突，导致 raw attribute 不能自然 fallthrough。

### 3.2 高级 Vuetify props 的类型策略

很多 Vuetify props 在 JS 侧是：

- 字符串字面量联合
- 对象/数组/函数联合
- 支持多种 item source shape

这里不能机械用 `object?` 收口。

需要先设计：

- 哪些用 enum/常量字符串 host type
- 哪些用小 union struct/record
- 哪些用专门 wrapper type
- 哪些暂时只开放透传，不在强类型层建模

### 3.3 “authoring stub 是否允许明显偏离官方 prop 名”

当前整体风格倾向：

- 保持接近 Blazor
- 但最终仍映射到 Vue/Vuetify prop 名

后续仍要继续统一：

- 是否保持 `ModelValue + ModelValueChanged` 这类 authoring-friendly alias
- 其余 props 是否尽量保留 Vuetify 原名
- 是否允许为少量高频场景增加更 C# 化的便捷别名

## 4. 工程化与文档项

### 4.1 增加 `ECMAScript.Vuetify` 自身状态文档

当前状态散落在：

- `src/ECMAScript.Vuetify/README.md`
- RazorVue 设计文档
- 测试
- sample

建议补：

- `docs/01-目标/ecmascript.vuetify/` 目录
- 覆盖矩阵
- authoring contract 文档
- host bootstrap contract 文档

这样后续“运行时代理完整度”和“authoring 完整度”才有稳定对照基线。

### 4.2 增加针对高频组件 surface 的矩阵测试

当前测试已经证明“若干关键路径可以工作”，但还不够回答：

- 哪些组件支持哪些 props
- 哪些支持哪些 emits
- 哪些支持哪些 slots

建议新增或整理：

- authoring coverage matrix
- props/emits/slots 快照测试
- unknown-parameter/invalid-bind target 的针对性回归

### 4.3 建立 sample 分层

当前 `RazorVue.TodoList` 能证明最小链路，但对 `ECMAScript.Vuetify` 还不够。

后续建议增加：

- “纯 runtime proxy” sample
- “authoring 高频表单组件” sample
- “样式与主题” sample
- “slot-heavy 组件” sample

这样可以更快识别“代理存在”与“authoring 好用”之间的差距。

## 5. 非当前目标

这些事项当前不应混入本清单当作默认目标：

- 完整复制 Vuetify 全量组件/全量 props/emits/slots
- 把所有 TS 类型工具逐一翻译成 C# 类型工具
- 让 `ECMAScript.Vuetify` 自己安装插件或自己注入全局样式
- 为每个长尾组件都提供一步到位的高保真 authoring
- 在没有真实消费场景前就补齐所有高级实验性 API

## 6. 推荐推进顺序

建议按以下顺序推进：

1. 建立 `AdditionalAttributes` arbitrary props 透传 contract
2. 补齐高频组件的高价值 props surface
3. 明确 CSS / plugin / host bootstrap 正式 contract
4. 补强复杂集合值与常用 union 的强类型 surface
5. 继续扩展高频命名槽/作用域槽
6. 建立 `ECMAScript.Vuetify` 自身覆盖矩阵与状态文档

## 7. 当前可用性结论

当前 `ECMAScript.Vuetify` 更适合以下场景：

- 组件集合可控；
- 团队愿意按需补 authoring stub；
- 宿主能明确负责 `vuetify/styles` 导入与 `createVuetify()` 安装；
- 项目主要使用已覆盖的高频组件和常见绑定模式。

当前不应默认宣称适合以下场景：

- 把 Vuetify 当成“已经被完整代理”的通用 C# 设计系统；
- 期望任意官方 props/class/style/data/aria 都能自然 authoring；
- 期望复杂 item/header/slot 场景已经完全强类型建模。

因此，下一阶段的目标不是“证明它能代理 Vuetify”，而是：

- 让它在高频真实项目中足够顺手；
- 让自定义参数和 CSS 路径足够清楚；
- 让缺失边界可预测、可补齐、可测试。
