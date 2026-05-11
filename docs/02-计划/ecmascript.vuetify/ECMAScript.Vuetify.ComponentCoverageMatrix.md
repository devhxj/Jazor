# ECMAScript.Vuetify 组件覆盖矩阵

> Status: 活跃矩阵  
> Updated: 2026-05-10
> Positioning: 历史覆盖矩阵，当前生产快照以 `docs/03-完成/razorvue/ECMAScript.Vuetify.ProductionChecklist.md` 和代码测试为准。
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

当前 `ECMAScript.Vuetify` authoring 组件桩已覆盖当前支持的 runtime exports 共 `108` 个组件：`VuetifyComponents` 的 `105` 个 normal exports，以及 `VuetifyLabsComponents` 的 `3` 个 labs exports。108 个组件全部为专用强类型 authoring 组件；当前不再保留 runtime-only / labs 透传型 authoring 桩。

当前专用强类型 authoring 组件包括：

`VAlert`, `VApp`, `VAppBar`, `VAutocomplete`, `VAvatar`, `VBadge`, `VBanner`, `VBottomNavigation`, `VBottomSheet`, `VBreadcrumbs`, `VBtn`, `VBtnGroup`, `VBtnToggle`, `VCalendar`, `VCard`, `VCardActions`, `VCardItem`, `VCardSubtitle`, `VCardText`, `VCardTitle`, `VCarousel`, `VCheckbox`, `VChip`, `VChipGroup`, `VCode`, `VCol`, `VColorPicker`, `VCombobox`, `VConfirmEdit`, `VContainer`, `VCounter`, `VDataIterator`, `VDataTable`, `VDatePicker`, `VDefaultsProvider`, `VDialog`, `VDivider`, `VEmptyState`, `VExpansionPanel`, `VFab`, `VField`, `VFileInput`, `VFooter`, `VForm`, `VHover`, `VIcon`, `VImg`, `VInfiniteScroll`, `VInput`, `VItemGroup`, `VKbd`, `VLabel`, `VLayout`, `VLazy`, `VList`, `VListItem`, `VLocaleProvider`, `VMain`, `VMenu`, `VMessages`, `VNavigationDrawer`, `VNoSsr`, `VNumberInput`, `VOtpInput`, `VOverlay`, `VPagination`, `VParallax`, `VProgressCircular`, `VProgressLinear`, `VRadio`, `VRadioGroup`, `VRangeSlider`, `VRating`, `VResponsive`, `VRow`, `VSelect`, `VSelectionControl`, `VSelectionControlGroup`, `VSheet`, `VSkeletonLoader`, `VSlider`, `VSlideGroup`, `VSnackbar`, `VSnackbarQueue`, `VSpacer`, `VSparkline`, `VSpeedDial`, `VStepper`, `VSwitch`, `VSystemBar`, `VTab`, `VTable`, `VTabs`, `VTabsWindow`, `VTabsWindowItem`, `VTextarea`, `VTextField`, `VThemeProvider`, `VTimePicker`, `VTimeline`, `VToolbar`, `VToolbarItems`, `VToolbarTitle`, `VTooltip`, `VTreeview`, `VValidation`, `VVirtualScroll`, `VWindow`.

当前所有 authoring 组件都已经接入：

- `VueLibraryComponent`
- `VueLibraryStyle("vuetify/styles")`
- `VueLibraryPluginRequirement("vuetify")`

同时已由 `RazorVueDescriptorExtractionTests` 守护其基本 descriptor 提取与关键 props/slots 映射。

## 3. 高频组件矩阵

### 3.1 表单与输入

| 组件 | 当前参数 | 当前判断 | 建议优先补齐 |
|------|---------|----------|-------------|
| `VTextField` | `ModelValue`, `ModelValueChanged`, `Focused`, `FocusedChanged`, field/input common props, `CounterValue`, `ModelModifiers`, `Prepend`, `Append`, `PrependInner`, `AppendInner`, `Clear`, `LabelContent`, `Details`, `CounterContent` | `A` | 长尾 validation rules 和 browser-native event payload 按业务继续建模 |
| `VTextarea` | `VTextField` 主表面 + `AutoGrow`, `Rows`, `MaxRows`, `NoResize`, `Autofocus` | `A` | 长尾 validation rules 和 native textarea attrs 按业务继续建模 |
| `VInput` | `Id`, `Name`, `Label`, `Theme`, `Density`, `Direction`, dimensions, icons/colors, `HideDetails`, `Messages`, `Focused`, `FocusedChanged`, nullable `Disabled`/`Readonly`, `ErrorMessages`, `MaxErrors`, `Rules`, `ValidateOn`, `ValidationValue`, `ModelValue`, `ModelValueChanged`, `Prepend`, `Append`, `Details`, `Message`, typed default slot | `A` | 业务直接组合 Vuetify input infrastructure 时可用，长尾事件继续按需补 |
| `VField` | `Id`, `Theme`, `Rounded`, `Loading`, field icons/colors, `Clearable`, `Active`, `Dirty`, `Disabled`, `Error`, `Flat`, `Label`, `Variant`, `Focused`, `FocusedChanged`, `ModelValue`, `ModelValueChanged`, `PrependInner`, `AppendInner`, `Clear`, `LabelContent`, `Loader`, typed default slot | `A` | 复杂自定义 input chrome 可用，native event payload 按需补 |
| `VCheckbox` | `ModelValue`, `ModelValueChanged`, `Focused`, `FocusedChanged`, `Label`, `Color`, `BaseColor`, `BgColor`, `Density`, `Readonly`, `HideDetails`, `Messages`, `TrueValue`, `FalseValue`, `TrueIcon`, `FalseIcon`, `Indeterminate`, `LabelContent` | `A` | 多选数组模型和 payload event 如有真实需求再补 |
| `VSwitch` | `VCheckbox` 主表面 + `Inset`, `Loading`, `Flat`, `Thumb`, `TrackTrue`, `TrackFalse` | `A` | 多选数组模型和 payload event 如有真实需求再补 |
| `VSelectionControl` | `Id`, `Name`, `Type`, `Label`, `Theme`, `DefaultsTarget`, `Color`, `BaseColor`, `Density`, nullable `Disabled`/`Readonly`/`Multiple`, `Error`, `Inline`, `FalseIcon`, `TrueIcon`, `Ripple`, `ValueComparator`, `ModelValue`, `ModelValueChanged`, `Value`, `TrueValue`, `FalseValue`, `LabelContent`, `Input`, typed default slot | `A` | 自定义 checkbox/radio/switch primitive 可用，多选集合模型按业务继续细化 |
| `VSelectionControlGroup` | `Id`, `Name`, `Type`, `Theme`, `DefaultsTarget`, `Color`, `Density`, nullable `Disabled`/`Readonly`/`Multiple`, `Error`, `Inline`, `FalseIcon`, `TrueIcon`, `Ripple`, `ValueComparator`, `ModelValue`, `ModelValueChanged`, `ChildContent` | `A` | 与低层 selection-control 组合可用，group 长尾 slots 按需补 |
| `VValidation` | `Focused`, `FocusedChanged`, nullable `Disabled`/`Readonly`, `Error`, `ErrorMessages`, `MaxErrors`, `Name`, `Label`, `Rules`, `ModelValue`, `ModelValueChanged`, `ValidateOn`, `ValidationValue`, typed default slot with computed/shallow ref state and Promise-returning reset/validate methods | `A` | 作为自定义 validation primitive 可用，复杂 rule payload 按业务继续扩展 |
| `VRadioGroup` | `Label`, `Disabled`, `Inline`, `ModelValue`, `ModelValueChanged`, `ChildContent` | `B` | `Color`, `Density`, `Readonly`, `HideDetails`, `Messages` |
| `VSelect` | `ModelValue`/`SelectedValue`, `Items`, `ItemTitle`, `ItemValue`, `ItemChildren`, `ItemProps`, `ValueComparator`, `Multiple`, `ReturnObject`, `Chips`, `ClosableChips`, `Clearable`, `Readonly`, field/validation props, menu props/model, `Item`, `Chip`, `Selection`, `PrependItem`, `AppendItem`, `NoData` | `A` | 长尾 validation rules、counter-value callback 等按业务继续建模 |
| `VAutocomplete` | `VSelect` 主表面 + `Search`, `SearchChanged`, `AutoSelectFirst`, `ClearOnSelect`, `CustomFilter`, `CustomKeyFilter`, `FilterKeys`, `FilterMode`, `NoFilter` | `A` | 高级 filter match 场景已强类型，后续按真实页面补 loader/field 长尾 slot |
| `VCombobox` | `VSelect` 主表面 + `Search`, `SearchChanged`, `AutoSelectFirst`, `ClearOnSelect`, `Delimiters`, filter props | `A` | 自由输入复杂模型继续通过 `SelectedValue` 并行入口覆盖，泛型库组件支持成熟前不破坏 `ModelValue string?` |
| `VForm` | `Disabled`, `FastFail`, `ChildContent` | `B` | `Readonly`, `ValidateOn`, `ModelValue`, `ModelValueChanged` 评估是否需要 |
| `VDatePicker` | `ModelValue`, `ModelValueChanged`, `Multiple`, `Min`, `Max`, `Year`, `YearChanged`, `Month`, `MonthChanged`, `ViewMode`, `ViewModeChanged`, `Active`, `AllowedDates`, calendar display props, picker display props, `HeaderText`, `HeaderContent`, `TitleContent`, `Actions`, `ChildContent` | `A` | 更复杂 date adapter object 和业务日期类型如有需求再补 |

判断：

- `VTextField` / `VTextarea` / `VInput` / `VField` / `VCheckbox` / `VSwitch` / `VSelectionControl` / `VSelectionControlGroup` 已覆盖 CRUD 表单主路径、input infrastructure、focused model、常用视觉/校验 props 和官方高频 slots；
- `VValidation` 已覆盖 validation composable 的直接 authoring 主路径，且保留官方 ref/computed ref 槽合同；
- `VSelect` / `VAutocomplete` / `VCombobox` 已补到生产主路径：items/model/menu/search/filter/scoped slots 均有强类型入口，长尾仍走 `AdditionalAttributes`。

### 3.2 按钮、反馈与浮层

| 组件 | 当前参数 | 当前判断 | 建议优先补齐 |
|------|---------|----------|-------------|
| `VBtn` | `Text`, `PrependIcon`, `AppendIcon`, `Active`, `ActiveColor`, `ActiveReadonly`, `BaseColor`, `Color`, `Variant`, `Size`, `Loading`, `Block`, `Border`, dimensions, `Rounded`, `Elevation`, router props, `Disabled`, `Flat`, `Icon`, `Slim`, `Stacked`, `Symbol`, `Density`, `Location`, `Position`, `Tag`, `Type`, `Value`, `Ripple`, `OnClick`, `Prepend`, `Append`, `Loader`, `ChildContent` | `A` | payload event 和 router object-form `To` 按业务继续建模 |
| `VFab` | `ModelValue`, `ModelValueChanged`, `App`, `Appear`, `Extended`, `Layout`, `Offset`, `Transition`, `Location`, `Name`, `Order`, `Absolute`, VBtn-like visual/router props, `ChildContent` | `A` | 官方只声明 default slot；`symbol` 注入键等内部组合协议继续通过 `AdditionalAttributes` 或后续专门建模 |
| `VBtnGroup` | `BaseColor`, `Border`, `Color`, `Density`, `Divided`, dimensions, `Elevation`, `Rounded`, `Tag`, `Variant`, `ChildContent` | `A` | 低优先级增量即可 |
| `VBtnToggle` | `VBtnGroup` 主表面 + `ModelValue`, `ModelValueChanged`, `Mandatory`, `Max`, `Multiple`, `SelectedClass` | `A` | 复杂 value object 与 selected strategy 按业务继续扩展 |
| `VConfirmEdit` | `ModelValue`, `ModelValueChanged`, `Color`, `CancelText`, `OkText`, `Disabled` as `bool | ("save"|"cancel")[]`, `HideActions`, `Save`, `Cancel`, typed default slot with editable model/actions | `A` | action slot 返回 `IVNode`，复杂 props 继续通过 `VueProps`/`AdditionalAttributes` 表达 |
| `VChip` | `Text`, `Color`, `Closable`, `OnClick`, `ChildContent` | `B` | `Variant`, `Size`, `Label`, `Disabled`, `Filter` |
| `VAlert` | `Type`, `Variant`, `Closable`, `Text`, `ChildContent` | `B` | `Color`, `Density`, `Prominent`, `Border`, `Icon`, `Title` |
| `VSnackbar` | `ModelValue`, `ModelValueChanged`, `Color`, `Timeout`, `ChildContent` | `B` | `Location`, `Variant`, `MultiLine`, `Rounded`, `Actions` 槽 |
| `VSnackbarQueue` | `ModelValue`, `ModelValueChanged`, `Variant`, `Color`, `Timeout`, `Timer`, `Closable`, `CloseText`, location/origin/offset/dimensions, typed default/text/actions slots | `A` | 更深层 snackbar prop 长尾和业务消息 record 可按真实通知系统继续细化 |
| `VDialog` | `ModelValue`, `ModelValueChanged`, `Activator`, `ChildContent` | `B` | `Persistent`, `MaxWidth`, `Width`, `ScrollStrategy`, `Location`, `Transition` |
| `VMenu` | `ModelValue`, `ModelValueChanged`, `CloseOnContentClick`, `CloseOnBack`, `CloseOnClick`, `OpenOnClick`, `OpenOnHover`, `OpenOnFocus`, `OpenDelay`, `CloseDelay`, `Location`, `Origin`, `Offset`, `ScrollStrategy`, `Persistent`, `Disabled`, `MinWidth`, `MaxWidth`, `Width`, `Transition`, `ActivatorProps`, `ContentProps`, typed `Activator`, `ChildContent` | `A` | 高级 selected/opened nested 状态按业务补齐 |
| `VTooltip` | `ModelValue`, `ModelValueChanged`, `Id`, `Interactive`, `Text`, `Location`, `Origin`, `Offset`, `OpenOnClick`, `OpenOnHover`, `OpenOnFocus`, `OpenDelay`, `CloseDelay`, `Disabled`, `Eager`, `MinWidth`, `MaxWidth`, `Width`, `Transition`, `ActivatorProps`, `ContentProps`, typed `Activator`, `ChildContent` | `A` | `scrim` 等长尾 overlay props 继续走 `AdditionalAttributes` 或按需建模 |
| `VSpeedDial` | `ModelValue`, `ModelValueChanged`, overlay/menu location/origin/offset/transition/dimensions, `ActivatorProps`, `ContentProps`, typed default/activator slots | `A` | 复杂 target/activator 实例等长尾 overlay props 继续通过 `AdditionalAttributes` 或后续专门建模 |
| `VOverlay` | `ModelValue`, `ModelValueChanged`, `Attach`, `Absolute`, `Contained`, `Disabled`, `Eager`, `Persistent`, close/open strategy props, delay props, `ActivatorProps`, `ContentProps`, `Location`, `Origin`, `Offset`, `LocationStrategy`, `ScrollStrategy`, `Scrim`, dimensions, `AfterEnter`, `AfterLeave`, `ClickOutside`, typed `Activator`, `ChildContent` | `A` | 函数型 location/scroll strategy 与复杂 target 实例按业务继续建模 |
| `VEmptyState` | `ActionText`, `BgColor`, `Color`, `Icon`, `Image`, `Justify`, `Headline`, `Title`, `Text`, `TextWidth`, dimensions, `Href`, `To`, `ActionClick`, `Actions`, `HeadlineContent`, `TitleContent`, `Media`, `TextContent`, `ChildContent` | `A` | router object-form `To` 继续通过 `AdditionalAttributes` 或后续专门建模 |
| `VHover` | `ModelValue`, `ModelValueChanged`, `Disabled`, `OpenDelay`, `CloseDelay`, typed default slot | `A` | 无 |
| `VLazy` | `ModelValue`, `ModelValueChanged`, `MinHeight`, `Options`, dimensions, `Tag`, `Transition`, `ChildContent` | `A` | 更完整 IntersectionObserverInit 长尾字段按业务扩展 |
| `VCounter` | `Active`, `Disabled`, `Max`, `Value`, `Transition`, typed default slot | `A` | 低优先级增量即可 |
| `VMessages` | `Active`, `Color`, `Messages`, `Transition`, typed `message` slot | `A` | 更复杂 transition component 对象如有业务需求继续建模 |

判断：

- `VDialog.Activator`、`VConfirmEdit` default slot 等已证明 scoped slot 路径可行；
- `VBtn` 已覆盖按钮高频生产主路径，复杂 payload event 与 router object-form `To` 继续按真实业务提升；
- 浮层类组件已经可演示，但离“复杂业务稳定 authoring”仍有距离。

### 3.3 布局、导航与展示

| 组件 | 当前参数 | 当前判断 | 建议优先补齐 |
|------|---------|----------|-------------|
| `VContainer` | `Fluid`, `ChildContent` | `A` | 低优先级增量即可 |
| `VLayout` | `Overlaps`, `FullHeight`, dimensions, `ChildContent` | `A` | layout item 子组件尚未专用建模，必要时提升 `VLayoutItem` |
| `VRow` | `Align`, `Justify`, `ChildContent` | `B` | `Dense`, `NoGutters`, `Class/Style/AdditionalAttributes` |
| `VCol` | `Cols`, `Md`, `Lg`, `ChildContent` | `B` | `Sm`, `Xl`, `Offset*`, `AlignSelf`, `Class/Style/AdditionalAttributes` |
| `VCard` | `Title`, `Subtitle`, `Text`, `PrependIcon`, `AppendIcon`, `PrependAvatar`, `AppendAvatar`, `Image`, `Color`, `Variant`, `Density`, `Elevation`, `Rounded`, `Height/Width/Min*/Max*`, `Disabled`, `Flat`, `Hover`, `Link`, `Href`, `To`, `Replace`, `Exact`, `TextContent`, `TitleContent`, `SubtitleContent`, `ImageContent`, `Prepend`, `Append`, `Actions`, `Item`, `ChildContent` | `A` | 后续按业务补更深层 router object-form `To` |
| `VCardItem` | `Title`, `Subtitle`, `PrependIcon`, `AppendIcon`, `PrependAvatar`, `AppendAvatar`, `Prepend`, `Append`, `TitleContent`, `SubtitleContent`, `ChildContent` | `A` | 低优先级增量即可 |
| `VCardTitle` | `ChildContent` | `A` | Vuetify 3.8.0 `VCardTitle` 是 simple functional 标题容器，不暴露 `text` prop；标题文本必须走默认 slot |
| `VCardSubtitle` | `ChildContent` | `A` | simple functional 容器，文本走默认 slot |
| `VCardText` | `ChildContent` | `A` | 低优先级增量即可 |
| `VCardActions` | `ChildContent` | `A` | simple functional 容器，内容走默认 slot |
| `VCode` | `Tag`, `ChildContent` | `A` | simple code 容器，class/style/data 与低频 attrs 通过 `AdditionalAttributes` |
| `VSheet` | `Color`, `Rounded`, `Elevation`, `ChildContent` | `B` | `Border`, `Height`, `Width`, `Position`, `Class/Style` |
| `VToolbar` | `Color`, `Density`, `Flat`, `ChildContent` | `B` | `Height`, `Prominent`, `Extended`, `Collapse` |
| `VToolbarItems` | `Color`, `Variant`, `ChildContent` | `A` | simple toolbar action group，低优先级增量即可 |
| `VToolbarTitle` | `Text`, `ChildContent` | `A` | 低优先级增量即可 |
| `VTimeline` | `Theme`, `Tag`, `Density`, `Size`, `IconColor`, `DotColor`, `FillDot`, `HideOpposite`, `LineInset`, `Align`, `Direction`, `Justify`, `Side`, `LineThickness`, `LineColor`, `TruncateLine`, `ChildContent` | `A` | `VTimelineItem` 子项如业务频繁使用可继续提升为专用强类型组件 |
| `VFooter` | `App`, `Border`, `Color`, dimensions, `Elevation`, `Rounded`, `Tag`, layout props, `Theme`, `ChildContent` | `A` | 低优先级增量即可 |
| `VSystemBar` | `Color`, `Height`, `Window`, `Theme`, `Tag`, `Rounded`, `Tile`, layout item props, `Elevation`, `ChildContent` | `A` | 低优先级增量即可 |
| `VThemeProvider` | `WithBackground`, `Theme`, `Tag`, `ChildContent` | `A` | 低优先级增量即可 |
| `VLabel` | `Text`, `Theme`, `OnClick`, `ChildContent` | `A` | label native attrs 继续走 `AdditionalAttributes` |
| `VKbd` | `Tag`, `ChildContent` | `A` | simple functional 容器，内容走默认 slot |
| `VNoSsr` | `ChildContent` | `A` | 无 |
| `VLocaleProvider` | `Locale`, `FallbackLocale`, `Messages`, `Rtl`, `ChildContent` | `A` | `Messages` 使用 `VueProps` / `VueDictionary`，复杂 locale message tree 可继续通过嵌套 typed record 或 dictionary 表达 |
| `VDefaultsProvider` | `Defaults`, `Disabled`, `Reset`, `Root`, `Scoped`, `ChildContent` | `A` | defaults object 使用 `VueProps` / `VueDictionary`；长尾 defaults shape 可按业务增加 typed records |
| `VTabs` | `Color`, `Grow`, `ModelValue`, `ModelValueChanged`, `ChildContent` | `B` | `Direction`, `AlignTabs`, `Density`, `BgColor` |
| `VTab` | `Text`, `Value`, `ChildContent` | `B` | `Disabled`, `SelectedClass`, `Stacked` |
| `VWindow` | `ModelValue`, `ModelValueChanged`, `Continuous`, `NextIcon`, `PrevIcon`, `Reverse`, `ShowArrows`, `Touch`, `Direction`, `Disabled`, `SelectedClass`, `Mandatory`, `Tag`, `Theme`, `Additional`, `Prev`, `Next`, typed default slot | `A` | 长尾 group internals 和复杂 touch data 继续按业务扩展 |
| `VTabsWindow` | `ModelValue`, `ModelValueChanged`, `Reverse`, `Direction`, `Disabled`, `SelectedClass`, `Tag`, `Theme`, `ChildContent` | `A` | 与 tabs group 深层联动按业务扩展 |
| `VTabsWindowItem` | `Value`, `Disabled`, `SelectedClass`, `Eager`, `Transition`, `ReverseTransition`, `GroupSelected`, `ChildContent` | `A` | 更复杂 transition component object 如有业务需求继续建模 |
| `VSpacer` | 无 | `A` | 无 |

判断：

- 布局类组件已足够支撑当前 sample；
- `VCard` 当前表面过窄，虽然能组合使用，但 authoring 感受不够完整。

### 3.4 列表、表格与数据展示

| 组件 | 当前参数 | 当前判断 | 建议优先补齐 |
|------|---------|----------|-------------|
| `VList` | `Items`, `ItemTitle`, `ItemValue`, `ItemChildren`, `ItemProps`, `ItemType`, `BaseColor`, `ActiveColor`, `ActiveClass`, `BgColor`, `Color`, `ExpandIcon`, `CollapseIcon`, `Lines`, `Slim`, `Density`, `Nav`, `Disabled`, `Variant`, `Rounded`, `Elevation`, `Height/Width/Min*/Max*`, `ChildContent` | `A` | `selectStrategy` / opened/selected 等高级嵌套状态按真实业务继续建模 |
| `VListItem` | `Title`, `Subtitle`, `Value`, `PrependIcon`, `AppendIcon`, `PrependAvatar`, `AppendAvatar`, `Active`, `ActiveClass`, `BaseColor`, `Color`, `Disabled`, `Lines`, `Link`, `Nav`, `Ripple`, `Slim`, `Density`, `Height/Width/Min*/Max*`, `Elevation`, `Rounded`, `Href`, `To`, `Replace`, `Exact`, `Variant`, `OnClick`, `Prepend`, `Append`, `TitleContent`, `SubtitleContent`, `ChildContent` | `A` | router object-form `To` 与更多 slot context 字段按需求扩展 |
| `VTreeview` | `ModelValue`, `ModelValueChanged`, `Activated`, `ActivatedChanged`, `Selected`, `SelectedChanged`, `Opened`, `OpenedChanged`, `Items`, `ItemTitle`, `ItemValue`, `ItemChildren`, `ItemProps`, `ReturnObject`, `Mandatory`, `Activatable`, `Selectable`, `ActiveStrategy`, `SelectStrategy`, `LoadChildren`, filter props, icon/color/list/dimension props, `Class`, `Style`, click payload events, tree item scoped slots | `A` | labs API 仍按 Vuetify 版本变化跟进；低频策略 object 继续通过专用 definition record 或 `AdditionalAttributes` 扩展 |
| `VBreadcrumbs` | `Items`, `Divider`, `Disabled`, `ChildContent` | `B` | `ItemTitle`, `ItemValue`, `Density`, `ActiveClass` |
| `VDataTable` | `ModelValue`, `ModelValueChanged`, `Headers`, `Items`, `ItemValue`, `ItemSelectable`, `ReturnObject`, `Page`, `PageChanged`, `ItemsPerPage`, `ItemsPerPageChanged`, `ItemsPerPageOptions`, `SortBy`, `SortByChanged`, `GroupBy`, `GroupByChanged`, `Expanded`, `ExpandedChanged`, `OptionsChanged`, `CurrentItemsChanged`, `Search`, `ShowSelect`, `SelectStrategy`, `ShowExpand`, `ExpandOnClick`, `HideDefaultBody`, `HideDefaultFooter`, `HideDefaultHeader`, `HideNoData`, `NoDataText`, `Loading`, `LoadingText`, `DisableSort`, `MultiSort`, `MustSort`, `SortAscIcon`, `SortDescIcon`, `Color`, `Density`, `Dense`, `FixedHeader`, `FixedFooter`, `Hover`, `Height`, `Width`, `ItemKey`, `HeaderProps`, `RowProps`, `CellProps`, pagination icon/text props, `Top`, `Colgroup`, `HeadersContent`, `HeaderSelect`, `HeaderExpand`, `BodyContent`, `BodyPrepend`, `BodyAppend`, `ItemContent`, `GroupHeader`, `ExpandedRow`, `Tbody`, `Thead`, `Tfoot`, `Bottom`, `FooterPrepend`, `LoadingContent`, `NoData`, `ChildContent` | `A` | server-side table、virtual table、深度分组和每列 `item.<key>` 动态 slot 仍按业务继续建模 |
| `VTable` | `FixedHeader`, `FixedFooter`, `Height`, `Hover`, `Density`, `Tag`, `Theme`, `ChildContent` | `A` | 原生 table 内容由默认 slot 组合 |
| `VPagination` | `ModelValue`, `ModelValueChanged`, `Length`, `TotalVisible`, `Disabled` | `B` | `Density`, `Rounded`, `Color`, `Variant` |
| `VItemGroup` | `ModelValue`, `ModelValueChanged`, `Mandatory`, `Max`, `Multiple`, `SelectedClass`, `Tag`, `ValueComparator`, typed default slot | `A` | 低频 group internals 继续按业务扩展 |
| `VChipGroup` | `ModelValue`, `ModelValueChanged`, `BaseColor`, `CenterActive`, `Color`, `Column`, `Filter`, `Direction`, `Mandatory`, `Max`, `Mobile`, `Multiple`, `NextIcon`, `PrevIcon`, `ShowArrows`, `SelectedClass`, `Variant`, `Tag`, `ValueComparator`, `ChildContent` | `A` | 低频 slide-group display props 继续按业务扩展 |
| `VSlideGroup` | `ModelValue`, `ModelValueChanged`, `Multiple`, `Mandatory`, `Max`, `SelectedClass`, `Disabled`, `Tag`, `Mobile`, `MobileBreakpoint`, `CenterActive`, `Direction`, `NextIcon`, `PrevIcon`, `ShowArrows`, typed default/prev/next slots | `A` | `symbol` injection key 暂未暴露，作为内部组合协议保留 |
| `VVirtualScroll` | dimensions, `ItemHeight`, `ItemKey`, `Items`, `Renderless`, typed default slot | `A` | 当前 `Items` 使用 `VueValue[]` 覆盖常见标量/object bag；泛型库组件支持成熟后可评估泛型 authoring |
| `VInfiniteScroll` | `Tag`, dimensions, `Color`, `Direction`, `Side`, `Mode`, `Margin`, `LoadMoreText`, `EmptyText`, `Load`, default/loading/error/empty/load-more slots | `A` | 复杂按钮 props 通过 status slot context 的 `Props` 或 `AdditionalAttributes` 继续表达 |

判断：

- `VDataTable` 已覆盖常规业务表格主路径：headers/items、选择、展开、分页、排序、loading/no-data、表头/行/页脚命名槽；服务端数据表、虚拟表格与动态列级 slot 后续按业务推进；
- `VTreeview` 已覆盖 labs treeview 的模型、items、opened/activated/selected 状态、异步 children、过滤、策略、点击 payload 与核心 scoped slots；
- `VList` / `VListItem` 已能满足简单展示，但槽能力仍偏弱。

### 3.5 媒体与状态展示

| 组件 | 当前参数 | 当前判断 | 建议优先补齐 |
|------|---------|----------|-------------|
| `VImg` | `Src`, `Alt`, `Height`, `Width`, `Cover` | `B` | `LazySrc`, `AspectRatio`, `Gradient`, `Position`, `ChildContent` |
| `VAvatar` | `Color`, `Image`, `Size`, `ChildContent` | `B` | `Rounded`, `Density`, `Variant` |
| `VBadge` | `Color`, `Content`, `Dot`, `ChildContent` | `B` | `Floating`, `Location`, `Inline`, `Max` |
| `VIcon` | `Icon` | `C` | `Color`, `Size`, `Tag`, `Start`, `End` |
| `VCarousel` | `ModelValue`, `ModelValueChanged`, `Color`, `Cycle`, `DelimiterIcon`, `Height`, `HideDelimiters`, `HideDelimiterBackground`, `Interval`, `Progress`, `VerticalDelimiters`, VWindow navigation/group props, `ChildContent`, `Prev`, `Next`, `Item` | `A` | `VCarouselItem` 不在当前 runtime export surface 内，carousel slide 内容先通过默认 slot 和业务组合表达 |
| `VParallax` | `Scale`, `ChildContent`, `Placeholder`, `Error`, `Sources` | `A` | image attrs 通过 `AdditionalAttributes` 透传，复杂 image props 可按 `VImg` 后续合并策略推进 |
| `VRating` | `ModelValue`, `ModelValueChanged`, `ActiveColor`, `Color`, `Clearable`, `Density`, `Name`, `ItemLabelPosition`, `ItemLabels`, `ItemAriaLabel`, `HalfIncrements`, `EmptyIcon`, `FullIcon`, `HalfIcon`, `Length`, `Hover`, `Readonly`, `Disabled`, `Ripple`, `Size`, `Tag`, `ItemContent`, `ItemLabel` | `A` | 自定义 icon component 值可通过 `VuetifyIconValue`/`AdditionalAttributes` 扩展 |
| `VProgressCircular` | `Color`, `Indeterminate`, `ModelValue` | `B` | `Size`, `Width`, `Rotate` |
| `VProgressLinear` | `Color`, `Indeterminate`, `ModelValue` | `B` | `Height`, `Rounded`, `BufferValue`, `Striped` |
| `VSparkline` | `AutoDraw`, `AutoDrawDuration`, `Gradient`, `GradientDirection`, `Labels`, `ModelValue`, `Smooth`, `Type`, `Label` | `A` | 复杂 object item shape 先覆盖 `{ value }` 主路径，其他自定义字段可经 `AdditionalAttributes` 或后续 typed record 扩展 |
| `VSkeletonLoader` | `Theme`, `Elevation`, dimensions, `Boilerplate`, `Color`, `Loading`, `LoadingText`, `Type`, `ChildContent` | `A` | 复杂自定义 skeleton bone/VNode 组合先继续通过默认 slot 或后续专门建模 |
| `VDivider` | `Inset`, `Thickness`, `Vertical` | `A` | 低优先级增量即可 |

## 4. 统一缺口

这些缺口不是单个组件问题，而是整组 authoring contract 的系统性问题。

### 4.1 arbitrary props 透传已统一

当前所有 authoring 组件都有：

- `[Parameter(CaptureUnmatchedValues = true)] AdditionalAttributes`

以下场景统一走透传：

- `class`
- `style`
- `id`
- `data-*`
- `aria-*`
- 尚未建模的 Vuetify props

PascalCase 未知参数仍保持失败，用于捕获 C# authoring 拼写错误；kebab-case、lower-camel raw props、`class`、`style`、`data-*`、`aria-*` 允许透传。

### 4.2 `Class` / `Style` 策略

当前统一规则是：

- 通用 CSS class/style 通过 `AdditionalAttributes` 透传 `class` / `style`
- 已显式建模的组件可以直接使用 `Class` / `Style`，例如 `VTreeview.Class` 和 `VTreeview.Style`
- 未显式建模的组件不为了 CSS 逃生口新增弱类型参数，继续使用 `AdditionalAttributes`

如果后续要把 `Class` / `Style` 扩展到更多组件，应先定义冲突优先级，再分批提升。

### 4.3 集合值和复杂值类型偏弱

当前若干组件仍保留较弱类型：

- `VSelect` / `VAutocomplete` / `VCombobox` 为兼容保留 `ModelValue string?`，复杂模型走并行 `SelectedValue`
- 低频实验 props 和 Vuetify 内部组合协议仍通过 `AdditionalAttributes`，不作为强类型主 API

这在 sample 阶段可接受，但会明显限制生产 authoring。

### 4.4 slot ref 合同必须按 ref 使用

Vuetify 官方 scoped slot 中的 `Ref`、`ComputedRef`、`WritableComputedRef` 已在 C# 合同中保留为 `IVueRef<T>`、`VueComputedRef<T>`、`VueWritableComputedRef<T>`。使用这些 slot context 时不要把字段当成直接标量读取，应通过 `.Value`：

- 读取：`ctx.IsValid.Value`、`ctx.IsFocused.Value`、`ctx.Id.Value`
- 写入 writable model：`ctx.Model.Value = true`
- color style bag：`ctx.BackgroundColorStyles.Value`

### 4.5 命名槽与作用域槽仍偏少

当前亮点主要集中在：

- `VDialog.Activator`
- `VTooltip.Activator`
- `VSpeedDial` default/activator scoped slots
- `VSnackbarQueue` default/text/actions scoped slots
- `VSparkline` label scoped slot
- `VOverlay.Activator`
- `VHover` / `VItemGroup` scoped default slot
- `VSlideGroup` default/prev/next scoped slots
- `VWindow` default/additional/prev/next scoped slots
- `VInput` / `VField` / `VSelectionControl` input infrastructure scoped slots
- `VValidation` default scoped slot
- `VConfirmEdit` default scoped slot
- `VCounter` scoped default slot
- `VMessages` scoped `message` slot

但高价值组件如：

- `VMenu`
- `VListItem`
仍会按业务继续补深层 slot context 和 payload event。

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

### 批次 5

- `VBanner`
- `VBottomNavigation`
- `VBottomSheet`
- `VExpansionPanel`
- `VOverlay`
- `VSpeedDial`
- `VHover`
- `VLazy`
- `VResponsive`
- `VItemGroup`
- `VSnackbarQueue`
- `VSparkline`
- `VChipGroup`
- `VWindow`
- `VTabsWindow`
- `VTabsWindowItem`
- `VConfirmEdit`
- `VValidation`

## 6. 当前结论

当前 `ECMAScript.Vuetify` 的 authoring surface 已经适合：

- 进入生产集成验证
- 常规表单/列表/布局页面
- 组件集受控的内部业务
- 常见 overlay、hover、lazy、responsive 和 group/chip-group 组合
- 有清晰逃生口的长尾 Vuetify props 透传

当前仍不适合默认宣称：

- 完整 Vuetify C# authoring 代理
- 任意官方 props 都能自然 authoring
- 复杂 slot-heavy 页面能无摩擦落地

下一阶段最关键的是继续按本地 Vuetify 3.8.0 源码和真实业务页面分批提升长尾合同，并在不弱化公共 API 的前提下补齐：

- 长尾组件的复杂集合值
- 高频 slot context 和 payload event

当前结论是：代理层已具备生产集成验证条件，但还不是 Vuetify 官方 API 的全量强类型镜像。上线前仍应在目标业务应用中跑完整 RazorVue/Jolt/emit 集成验证。
