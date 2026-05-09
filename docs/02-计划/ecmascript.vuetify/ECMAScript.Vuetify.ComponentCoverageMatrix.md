# ECMAScript.Vuetify 组件覆盖矩阵

> Status: 活跃矩阵  
> Updated: 2026-05-09  
> Positioning: 基于 `src/ECMAScript.Vuetify/` 当前 Razor authoring 组件桩、`src/Jazor.RazorVue.Test/RazorVueDescriptorExtractionTests.cs` 现有守护，以及 `samples/RazorVue.TodoList/` 当前示例使用方式整理的组件覆盖矩阵。  
> Scope: 本文档只讨论 RazorVue authoring 组件桩，不讨论 `VuetifyComponents` / `VuetifyDirectives` runtime export 的全量覆盖。

## 1. 读法

本矩阵回答四个问题：

1. 当前有哪些 authoring 组件桩
2. 每个组件当前暴露了哪些参数
3. 哪些组件已经适合常规业务使用
4. 哪些组件还停留在“最小可演示壳”

判定等级：

- `A`
  - 高频基础场景基本可用
- `B`
  - 常见演示可用，但真实业务还会很快撞到缺口
- `C`
  - 只有最小 authoring 表面，仍主要用于证明链路

注意：

- `A/B/C` 不是运行时可用性评分；
- 它只表示当前 Razor authoring surface 的完整度和顺手程度。

## 2. 当前总览

当前 `ECMAScript.Vuetify` authoring 组件桩共 `39` 个：

- `VAlert`
- `VAutocomplete`
- `VAvatar`
- `VBadge`
- `VBreadcrumbs`
- `VBtn`
- `VCard`
- `VCardText`
- `VCardTitle`
- `VCheckbox`
- `VChip`
- `VCol`
- `VContainer`
- `VDataTable`
- `VDialog`
- `VDivider`
- `VForm`
- `VIcon`
- `VImg`
- `VList`
- `VListItem`
- `VMenu`
- `VPagination`
- `VProgressCircular`
- `VProgressLinear`
- `VRadioGroup`
- `VRow`
- `VSelect`
- `VSheet`
- `VSnackbar`
- `VSpacer`
- `VSwitch`
- `VTab`
- `VTabs`
- `VTextarea`
- `VTextField`
- `VToolbar`
- `VToolbarTitle`
- `VTooltip`

当前所有这些组件都已经接入：

- `VueLibraryComponent`
- `VueLibraryStyle("vuetify/styles")`
- `VueLibraryPluginRequirement("vuetify")`

同时已由 `RazorVueDescriptorExtractionTests` 守护其基本 descriptor 提取与关键 props/slots 映射。

## 3. 高频组件矩阵

### 3.1 表单与输入

| 组件 | 当前参数 | 当前判断 | 建议优先补齐 |
|------|---------|----------|-------------|
| `VTextField` | `Label`, `Disabled`, `ModelValue`, `ModelValueChanged` | `B` | `Placeholder`, `Hint`, `PersistentHint`, `Readonly`, `Clearable`, `Variant`, `Density`, `Type`, `Error`, `Messages` |
| `VTextarea` | `Label`, `Disabled`, `Rows`, `ModelValue`, `ModelValueChanged` | `B` | `Placeholder`, `Readonly`, `AutoGrow`, `Counter`, `Variant`, `Density`, `Hint`, `PersistentHint` |
| `VCheckbox` | `Label`, `Disabled`, `ModelValue`, `ModelValueChanged` | `B` | `Color`, `Density`, `Readonly`, `HideDetails`, `Messages` |
| `VSwitch` | `Label`, `Disabled`, `ModelValue`, `ModelValueChanged` | `B` | `Color`, `Density`, `Readonly`, `Inset`, `HideDetails` |
| `VRadioGroup` | `Label`, `Disabled`, `Inline`, `ModelValue`, `ModelValueChanged`, `ChildContent` | `B` | `Color`, `Density`, `Readonly`, `HideDetails`, `Messages` |
| `VSelect` | `Label`, `Disabled`, `Multiple`, `ModelValue`, `ModelValueChanged` | `C` | `Items`, `ItemTitle`, `ItemValue`, `ReturnObject`, `Chips`, `Clearable`, `Readonly`, `MenuProps`, `Density`, `Variant` |
| `VAutocomplete` | `Label`, `Disabled`, `Multiple`, `Chips`, `ModelValue`, `ModelValueChanged` | `C` | `Items`, `ItemTitle`, `ItemValue`, `ReturnObject`, `Clearable`, `Readonly`, `MenuProps`, `Density`, `Variant`, `NoDataText` |
| `VForm` | `Disabled`, `FastFail`, `ChildContent` | `B` | `Readonly`, `ValidateOn`, `ModelValue`, `ModelValueChanged` 评估是否需要 |

判断：

- `VTextField` / `VTextarea` / `VCheckbox` / `VSwitch` 已能支撑简单 CRUD 表单；
- `VSelect` / `VAutocomplete` 仍明显偏弱，是当前表单 authoring 的主要短板。

### 3.2 按钮、反馈与浮层

| 组件 | 当前参数 | 当前判断 | 建议优先补齐 |
|------|---------|----------|-------------|
| `VBtn` | `Text`, `Disabled`, `OnClick`, `ChildContent` | `C` | `Color`, `Variant`, `Size`, `Icon`, `Loading`, `Block`, `Href`, `Target`, `Density` |
| `VChip` | `Text`, `Color`, `Closable`, `OnClick`, `ChildContent` | `B` | `Variant`, `Size`, `Label`, `Disabled`, `Filter` |
| `VAlert` | `Type`, `Variant`, `Closable`, `Text`, `ChildContent` | `B` | `Color`, `Density`, `Prominent`, `Border`, `Icon`, `Title` |
| `VSnackbar` | `ModelValue`, `ModelValueChanged`, `Color`, `Timeout`, `ChildContent` | `B` | `Location`, `Variant`, `MultiLine`, `Rounded`, `Actions` 槽 |
| `VDialog` | `ModelValue`, `ModelValueChanged`, `Activator`, `ChildContent` | `B` | `Persistent`, `MaxWidth`, `Width`, `ScrollStrategy`, `Location`, `Transition` |
| `VMenu` | `ModelValue`, `ModelValueChanged`, `CloseOnContentClick`, `ChildContent` | `B` | `Location`, `Offset`, `OpenOnHover`, `Activator` 槽强类型化 |
| `VTooltip` | `Text`, `Location`, `OpenOnHover`, `Activator`, `ChildContent` | `B` | `Color`, `OpenDelay`, `CloseDelay`, `Disabled`, `MaxWidth` |

判断：

- `VDialog.Activator` 已经证明 scoped slot 路径可行；
- `VBtn` 是当前最需要补齐的单个组件之一；
- 浮层类组件已经可演示，但离“复杂业务稳定 authoring”仍有距离。

### 3.3 布局、导航与展示

| 组件 | 当前参数 | 当前判断 | 建议优先补齐 |
|------|---------|----------|-------------|
| `VContainer` | `Fluid`, `ChildContent` | `A` | 低优先级增量即可 |
| `VRow` | `Align`, `Justify`, `ChildContent` | `B` | `Dense`, `NoGutters`, `Class/Style/AdditionalAttributes` |
| `VCol` | `Cols`, `Md`, `Lg`, `ChildContent` | `B` | `Sm`, `Xl`, `Offset*`, `AlignSelf`, `Class/Style/AdditionalAttributes` |
| `VCard` | `Disabled`, `ChildContent` | `C` | `Title`, `Subtitle`, `Text`, `Variant`, `Elevation`, `Rounded`, `Color` |
| `VCardTitle` | `Text`, `ChildContent` | `A` | 低优先级增量即可 |
| `VCardText` | `ChildContent` | `A` | 低优先级增量即可 |
| `VSheet` | `Color`, `Rounded`, `Elevation`, `ChildContent` | `B` | `Border`, `Height`, `Width`, `Position`, `Class/Style` |
| `VToolbar` | `Color`, `Density`, `Flat`, `ChildContent` | `B` | `Height`, `Prominent`, `Extended`, `Collapse` |
| `VToolbarTitle` | `Text`, `ChildContent` | `A` | 低优先级增量即可 |
| `VTabs` | `Color`, `Grow`, `ModelValue`, `ModelValueChanged`, `ChildContent` | `B` | `Direction`, `AlignTabs`, `Density`, `BgColor` |
| `VTab` | `Text`, `Value`, `ChildContent` | `B` | `Disabled`, `SelectedClass`, `Stacked` |
| `VSpacer` | 无 | `A` | 无 |

判断：

- 布局类组件已足够支撑当前 sample；
- `VCard` 当前表面过窄，虽然能组合使用，但 authoring 感受不够完整。

### 3.4 列表、表格与数据展示

| 组件 | 当前参数 | 当前判断 | 建议优先补齐 |
|------|---------|----------|-------------|
| `VList` | `Density`, `Nav`, `ChildContent` | `B` | `Lines`, `Slim`, `SelectStrategy`, 命名槽 |
| `VListItem` | `Title`, `Subtitle`, `Value`, `ChildContent` | `B` | `PrependIcon`, `AppendIcon`, `Active`, `Disabled`, `Link`, 命名槽 |
| `VBreadcrumbs` | `Items`, `Divider`, `Disabled`, `ChildContent` | `B` | `ItemTitle`, `ItemValue`, `Density`, `ActiveClass` |
| `VDataTable` | `Headers`, `Items`, `Dense`, `ItemKey`, `ChildContent` | `C` | 强类型 `Header`/`Item` surface，排序、分页、命名槽、row/header slot |
| `VPagination` | `ModelValue`, `ModelValueChanged`, `Length`, `TotalVisible`, `Disabled` | `B` | `Density`, `Rounded`, `Color`, `Variant` |

判断：

- `VDataTable` 当前更像“证明最小输入面存在”，距离复杂业务表格还很远；
- `VList` / `VListItem` 已能满足简单展示，但槽能力仍偏弱。

### 3.5 媒体与状态展示

| 组件 | 当前参数 | 当前判断 | 建议优先补齐 |
|------|---------|----------|-------------|
| `VImg` | `Src`, `Alt`, `Height`, `Width`, `Cover` | `B` | `LazySrc`, `AspectRatio`, `Gradient`, `Position`, `ChildContent` |
| `VAvatar` | `Color`, `Image`, `Size`, `ChildContent` | `B` | `Rounded`, `Density`, `Variant` |
| `VBadge` | `Color`, `Content`, `Dot`, `ChildContent` | `B` | `Floating`, `Location`, `Inline`, `Max` |
| `VIcon` | `Icon` | `C` | `Color`, `Size`, `Tag`, `Start`, `End` |
| `VProgressCircular` | `Color`, `Indeterminate`, `ModelValue` | `B` | `Size`, `Width`, `Rotate` |
| `VProgressLinear` | `Color`, `Indeterminate`, `ModelValue` | `B` | `Height`, `Rounded`, `BufferValue`, `Striped` |
| `VDivider` | `Inset`, `Thickness`, `Vertical` | `A` | 低优先级增量即可 |

## 4. 统一缺口

这些缺口不是单个组件问题，而是整组 authoring contract 的系统性问题。

### 4.1 缺少统一 arbitrary props 透传

当前几乎所有组件都没有：

- `[Parameter(CaptureUnmatchedValues = true)] AdditionalAttributes`

这导致以下场景会系统性受阻：

- `class`
- `style`
- `id`
- `data-*`
- `aria-*`
- 尚未建模的 Vuetify props

这也是为什么当前多个组件虽然“看起来已经有壳”，但真实 authoring 仍不够顺手。

### 4.2 缺少统一 `Class` / `Style` 策略

当前没有形成统一规则：

- 是否只走 `AdditionalAttributes`
- 是否对高频组件显式提供 `Class` / `Style`
- 冲突优先级如何定义

这需要先设计，再大批量补组件。

### 4.3 集合值和复杂值类型偏弱

当前若干组件仍保留较弱类型：

- `Headers: IEnumerable<object>?`
- `Items: IEnumerable<object>?`
- 多数组件的 `ModelValue` 仍是 `string?` 或简单标量

这在 sample 阶段可接受，但会明显限制生产 authoring。

### 4.4 命名槽与作用域槽仍偏少

当前亮点主要集中在：

- `VDialog.Activator`
- `VTooltip.Activator`

但高价值组件如：

- `VMenu`
- `VSelect`
- `VAutocomplete`
- `VDataTable`
- `VListItem`

还没有形成足够可用的槽建模。

## 5. 推荐推进批次

### 批次 1

- `AdditionalAttributes` contract
- `VBtn`
- `VTextField`
- `VTextarea`
- `VCheckbox`
- `VSwitch`

### 批次 2

- `VSelect`
- `VAutocomplete`
- `VDialog`
- `VMenu`
- `VTooltip`

### 批次 3

- `VCard`
- `VList`
- `VListItem`
- `VDataTable`
- `VImg`

### 批次 4

- `VTabs`
- `VTab`
- `VBreadcrumbs`
- `VPagination`
- `VAvatar`
- `VBadge`

## 6. 当前结论

当前 `ECMAScript.Vuetify` 的 authoring surface 已经适合：

- sample 级验证
- 简单表单/列表/布局页面
- 组件集受控的内部业务

当前还不适合默认宣称：

- 完整 Vuetify C# authoring 代理
- 任意官方 props 都能自然 authoring
- 复杂 select/table/slot-heavy 页面能无摩擦落地

下一阶段最关键的不是继续“加更多组件名”，而是：

- 补统一透传 contract
- 补高频组件高价值 props
- 补复杂集合值和高频槽

只有这三件事先补起来，`ECMAScript.Vuetify` 才会从“能演示”走向“顺手可生产”。
