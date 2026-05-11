# ECMAScript.Vuetify 生产化清单

> Status: current production-readiness snapshot  
> Date: 2026-05-10  
> Scope: `src/ECMAScript.Vuetify` as a Vuetify 3.8.0 binding and RazorVue authoring proxy layer.
> Source: local Vuetify 3.8.0 d.ts surface under `node_modules/vuetify/lib/components/index.d.ts` and `node_modules/vuetify/lib/labs/components.d.ts`.

## 结论

`ECMAScript.Vuetify` 可以作为 RazorVue Vuetify 代理层进入生产集成验证。当前不是“Vuetify 全量 props 强类型完整代理”，而是分层覆盖：

- Runtime registry 覆盖当前支持的 Vuetify component export surface：`VuetifyComponents` 暴露 105 个 `vuetify/components` 导出，`VuetifyLabsComponents` 暴露 3 个 `vuetify/labs/components` 导出；对应 `VuetifyComponentRegistry` / `VuetifyLabsComponentRegistry` 可用于 `CreateVuetify(...)` 的 `components` 配置。
- RazorVue authoring 组件覆盖全部 108 个当前支持的 runtime component exports。108 个组件均具备专用强类型 props。
- 所有 RazorVue authoring 组件都提供 `[Parameter(CaptureUnmatchedValues = true)] AdditionalAttributes`，用于 `class`、`style`、`data-*`、`aria-*` 和尚未强类型建模的 Vuetify 长尾 props。
- 公共强类型主路径不使用通用二选一封装，也不新增 `object` / `object?` catch-all；唯一允许的 `object?` 是 Blazor/RazorVue 约定的 `AdditionalAttributes` sink。

## Authoring 覆盖

当前专用强类型 RazorVue authoring 组件：

`VAlert`, `VApp`, `VAppBar`, `VAutocomplete`, `VAvatar`, `VBadge`, `VBanner`, `VBottomNavigation`, `VBottomSheet`, `VBreadcrumbs`, `VBtn`, `VBtnGroup`, `VBtnToggle`, `VCalendar`, `VCard`, `VCardActions`, `VCardItem`, `VCardSubtitle`, `VCardText`, `VCardTitle`, `VCarousel`, `VCheckbox`, `VChip`, `VChipGroup`, `VCode`, `VCol`, `VColorPicker`, `VCombobox`, `VConfirmEdit`, `VContainer`, `VCounter`, `VDataIterator`, `VDataTable`, `VDatePicker`, `VDefaultsProvider`, `VDialog`, `VDivider`, `VEmptyState`, `VExpansionPanel`, `VFab`, `VField`, `VFileInput`, `VFooter`, `VForm`, `VHover`, `VIcon`, `VImg`, `VInfiniteScroll`, `VInput`, `VItemGroup`, `VKbd`, `VLabel`, `VLayout`, `VLazy`, `VList`, `VListItem`, `VLocaleProvider`, `VMain`, `VMenu`, `VMessages`, `VNavigationDrawer`, `VNoSsr`, `VNumberInput`, `VOtpInput`, `VOverlay`, `VPagination`, `VParallax`, `VProgressCircular`, `VProgressLinear`, `VRadio`, `VRadioGroup`, `VRangeSlider`, `VRating`, `VResponsive`, `VRow`, `VSelect`, `VSelectionControl`, `VSelectionControlGroup`, `VSheet`, `VSkeletonLoader`, `VSlider`, `VSlideGroup`, `VSnackbar`, `VSnackbarQueue`, `VSpacer`, `VSparkline`, `VSpeedDial`, `VStepper`, `VSwitch`, `VSystemBar`, `VTab`, `VTable`, `VTabs`, `VTabsWindow`, `VTabsWindowItem`, `VTextarea`, `VTextField`, `VThemeProvider`, `VTimePicker`, `VTimeline`, `VToolbar`, `VToolbarItems`, `VToolbarTitle`, `VTooltip`, `VTreeview`, `VValidation`, `VVirtualScroll`, `VWindow`.

## 强类型策略

已补齐或约束的关键类型：

- Field common props: `VuetifyFieldVariant`, `VuetifyDensity`, `VuetifyHideDetailsValue`, `VuetifyMessagesValue`, `VuetifyValidateOn`。
- App shell props: `VuetifyAppBarLocation`, `VuetifyNavigationDrawerLocation`, `VuetifyScrimValue`。
- Button/group/rating contracts: `VuetifyBorderValue`, `VuetifyIconValue`, `VuetifyMandatoryValue`, `VuetifyMandatoryMode`, `VuetifyGroupModelValue`, `VuetifyGroupModelValues`, `VuetifyShowArrowsValue`, `VuetifyValueComparator`, `VuetifyItemLabelPosition`, `VItemGroupDefaultSlotContext`, `VSlideGroupSlotContext`, `VRatingItemSlotContext`, `VRatingItemLabelSlotContext`。
- Select/autocomplete/combobox contracts: `VuetifySelectItems`, `VuetifySelectItemValue`, `VuetifySelectItem`, `VuetifyListItem`, `VuetifySelectItemKey`, `VuetifySelectItemPropsSelector`, `VuetifySelectValueComparator`, `VuetifyFilterFunction`, `VuetifyFilterKeyFunctions`, `VuetifyFilterKeys`, `VuetifyFilterMatch`, `VuetifyFilterMode`, `VuetifyItemProps`, `VuetifySelectModelValue`, `VuetifySelectModelValues`, `VSelectItemSlotContext`, `VSelectChipSlotContext`, `VSelectSelectionSlotContext`。
- Breadcrumb item contracts: `VuetifyBreadcrumbItems`, `VuetifyBreadcrumbItemValue`, `VuetifyBreadcrumbItem`。
- Data table contracts: `VuetifyDataTableHeaders`, `VuetifyDataTableHeader`, `VuetifyDataTableHeaderAlign`, `VuetifyDataTableItems`, `VuetifyDataTableItem`, `VuetifyDataTableSelectedValues`, `VuetifyDataTableSortItems`, `VuetifyDataTableSortItem`, `VuetifyDataTableSortOrder`, `VuetifyDataTableSelectStrategy`, `VuetifyDataTableOptions`, `VuetifyDataTableItemsPerPageOptions`, `VuetifyDataTableRowProps`, `VuetifyDataTableCellProps`, `VDataTableSlotContext`, `VDataTableHeadersSlotContext`, `VDataTableHeaderCellSlotContext`, `VDataTableItemSlotContext`, `VDataTableGroupHeaderSlotContext`。
- List/card/overlay composition contracts: `VuetifyListLines`, `VuetifyListLineMode`, `VuetifyRippleValue`, `VuetifyLocationStrategy`, `VuetifyAttachTarget`, `VListItemSlotContext`, `VListItemTitleSlotContext`, `VListItemSubtitleSlotContext`, `VOverlayActivatorContext`。
- Form/input control contracts: `VuetifyAutoSelectFirstValue`, `VuetifyFileShowSizeValue`, `VuetifyFileModelValue`, `VuetifyNumberInputControlVariant`, `VuetifyBooleanAlwaysValue`, `VuetifyBooleanStringValue`, `VuetifyNullableBoolean`, `VuetifyIconColorValue`, `VuetifyValidationResult`, `VuetifyValidationRule`, `VuetifyValidationRuleResolver`, `VuetifyAsyncValidationRuleResolver`, `VuetifyCounterValue`, `VuetifyCounterValueSource`, `VuetifyTextModelModifiers`, `VuetifyProgressCircularIndeterminateValue`, `VuetifyAlwaysMode`, `VuetifySliderDirection`, `VuetifyRangeSliderModelValue`, `VuetifyTransitionValue`, `VFieldSlotContext`, `VInputSlotContext`, `VInputDetailsSlotContext`, `VCounterSlotContext`, `VCounterDefaultSlotContext`, `VMessagesMessageSlotContext`, `VSelectionControlDefaultSlotContext`, `VSelectionControlInputDefaultSlotContext`, `VSelectionControlLabelSlotContext`, `VSelectionControlInputSlotContext`, `VSelectionControlInputProps`, `VSwitchSlotContext`, `VuetifyLoaderSlotContext`, `VuetifyCssProperties`。
- Display/options contracts: `VuetifyDisplayBreakpoint`, `VuetifyDisplayThresholds`, `VuetifyIntersectionObserverOptions`, `VuetifyIntersectionObserverRoot`, `VuetifyIntersectionObserverThreshold`, `VHoverDefaultSlotContext`。
- Visual/value contracts: `VuetifyRoundedValue`, `VuetifyTextValue`，以及尺寸、长度、进度、海拔、分页长度等 `Number | String` props 统一使用 `VueStringNumberValue`。
- Time picker contracts: `VuetifyTimePickerModelValue`, `VuetifyTimePickerAllowedUnits`, `VuetifyTimePickerAllowedUnitValue`, `VuetifyTimePickerAllowedUnitResolver`, `VuetifyTimePickerFormat`, `VuetifyTimePickerViewMode`, `VuetifyTimePickerPeriod`。
- Calendar contracts: `VuetifyCalendarDateValue`, `VuetifyCalendarDateValues`, `VuetifyCalendarAllowedDatesValue`, `VuetifyCalendarEvents`, `VuetifyCalendarEventItem`, `VuetifyCalendarIntervalFormatValue`, `VuetifyCalendarViewMode`, `VuetifyCalendarWeeksInMonth`, `VCalendarHeaderSlotContext`, `VCalendarEventSlotContext`。
- Treeview contracts: `VuetifyTreeviewValues`, `VuetifyTreeviewItems`, `VuetifyTreeviewItem`, `VuetifyTreeviewItemValue`, `VuetifyTreeviewActiveStrategyValue`, `VuetifyTreeviewSelectStrategyValue`, `VuetifyTreeviewActiveStrategyDefinition`, `VuetifyTreeviewSelectStrategyDefinition`, `VuetifyTreeviewClickPayload`, `VuetifyStyleValue`, `VTreeviewNodeSlotContext`, `VTreeviewTitleSlotContext`, `VTreeviewSubtitleSlotContext`, `VTreeviewItemSlotContext`。
- Skeleton loader contracts: `VuetifySkeletonLoaderType`, `VuetifySkeletonLoaderTypeValue`, `VuetifySkeletonLoaderTypes`, `VuetifySkeletonLoaderTypeSetting`，覆盖 Vuetify 3.8.0 官方 root type、自定义 string type、单值和数组 type 场景。

设计约束：

- `string | number` 类 props 使用现有 `VueStringNumberValue`。
- `bool | string` 类 Vuetify props 使用专用 union 类型，例如 `VuetifyScrimValue` / `VuetifyBooleanStringValue`；新代码优先 native `union`，需要保留精确 tagged projection 时使用 `[System.Runtime.CompilerServices.Union]` + `IUnion` fallback。
- `bool | number | string` 类 props 按语义拆分为专用 union，例如 `VuetifyCounterValue` 和 `VuetifyRoundedValue`，避免一个过宽的通用类型掩盖 prop 语义。
- `transition` 类 props 使用 `VuetifyTransitionValue` 覆盖 `bool | string | VueTransitionProps`，支持禁用 transition、命名 transition 和对象形式 transition props，不退回到弱 `object`。
- 集合 item 对象使用 `[ECMAScript]` class 和 collection initializer 友好的 indexer / `Add(...)`，避免弱化为 `object`。
- `Record<string, any>` 风格配置对象优先使用 `VueProps` / `VueDictionary` 作为强类型 object bag，例如 `VLocaleProvider.Messages`，不引入弱类型公共参数。
- `VDefaultsProvider.Defaults` 使用 `VueProps` / `VueDictionary` 表达 Vuetify defaults object；`root` 使用 `VuetifyBooleanStringValue`，不暴露 `object`/`any`。
- `VSelect` / `VAutocomplete` / `VCombobox` 保留 `ModelValue string?` 兼容主路径，同时提供 `SelectedValue` / `SelectedValueChanged` 并行强类型入口覆盖 Vuetify `modelValue` 的 string、number、bool、symbol、object 和 multiple array 场景。三者共享 select-like authoring 基类，覆盖 field/common props、menu/open/focused 模型、item/value/comparator、`item` / `chip` / `selection` / `prepend-item` / `append-item` / `no-data` 官方槽位；`VAutocomplete` / `VCombobox` 额外覆盖 `search` 模型和 filter props。RazorVue 会拒绝同一组件上同时使用两个映射到 `modelValue` 或 `update:modelValue` 的 authoring 参数，避免生成重复 Vue prop/event。
- `VInput` / `VField` / `VSelectionControl` / `VSelectionControlGroup` 已从透传桩提升为强类型基础设施组件，直接覆盖 Vuetify input、field、selection-control 的官方 props、model update、validation、loader、label/input/details/message slots。
- `VValidation` 已从透传桩提升为强类型 validation 组件，覆盖 `focused`、nullable disabled/readonly、error messages、rules、model/validation value、validate-on、typed default slot、computed/shallow ref 状态和 Promise-returning reset/validate 方法。
- `VConfirmEdit` 已从透传桩提升为强类型 confirm-edit 组件，覆盖 model update、`save`/`cancel` emit、`bool | ("save"|"cancel")[]` disabled union、默认 scoped slot 的 editable model/actions 和 `AdditionalAttributes`。
- `VWindow` / `VTabsWindow` / `VTabsWindowItem` 已从透传桩提升为强类型 window/tabs panel 组件，覆盖 group model、selected class、navigation arrows、touch handler bag、transition、`group:selected` emit，以及 `default` / `additional` / `prev` / `next` scoped slots。
- `VSlideGroup` 已从透传桩提升为强类型 slide-group 组件，覆盖 group model、mandatory/max/multiple、mobile/display、arrow controls、show-arrows union，以及 `default` / `prev` / `next` scoped slots。
- `VCarousel` 已从透传桩提升为强类型 carousel 组件，覆盖 group model/update、cycle/interval/progress、delimiter icon、`bool | "left" | "right"` vertical delimiter union、VWindow navigation props，以及 default/prev/next/item scoped slots。
- `VEmptyState` 已从透传桩提升为强类型 empty-state 组件，覆盖 action/media/text/display/router string props、`click:action` emit，以及 `actions` scoped slot 和 `headline` / `title` / `media` / `text` 命名槽。
- `VSkeletonLoader` 已从透传桩提升为强类型 skeleton-loader 组件，覆盖 theme、elevation、dimensions、boilerplate、color、loading、loadingText、官方/custom `type` 单值与数组，以及默认 slot。
- `VToolbarItems` 已从透传桩提升为强类型 toolbar-items 组件，覆盖 color、variant 和默认 slot。
- `VParallax` 已从透传桩提升为强类型 parallax 组件，覆盖 scale、default / placeholder / error / sources 槽和透传图片属性。
- `VCode` 已从透传桩提升为强类型 code 组件，覆盖 tag、默认 slot 和 class/style/data fallthrough。
- `VTimeline` 已从透传桩提升为强类型 timeline 组件，覆盖 theme/tag/density、size、dot/icon color、line inset/thickness/color、align/direction/justify/side/truncateLine 枚举和默认 slot。
- `VLocaleProvider` 已从透传桩提升为强类型 locale-provider 组件，覆盖 locale、fallbackLocale、messages object、nullable RTL、默认 slot 和 class/style/data fallthrough。
- `VFab` 已从透传桩提升为强类型 floating action button 组件，覆盖 `modelValue`、layout/app/absolute/position、transition、location、order 和 VBtn-like 外观/路由 props；官方只声明 default slot，因此不额外承诺 prepend/append/loader slots。
- `VSpeedDial` 已从透传桩提升为强类型 speed-dial 组件，覆盖 `modelValue`、overlay/menu location/origin/offset/transition/dimensions、activator/content props、default/activator scoped slots 和 `AdditionalAttributes`。
- `VSnackbarQueue` 已从透传桩提升为强类型 snackbar-queue 组件，覆盖 `string | SnackbarMessage` 队列、消息 option object、snackbar 外观/位置/timer/closable props、default/text/actions scoped slots 和 `update:modelValue`。
- `VSparkline` 已从透传桩提升为强类型 sparkline 组件，覆盖 trend/bar 类型、auto-draw、gradient、labels/modelValue、`string | number | { value }` item union、smooth union 和 label scoped slot。
- `VVirtualScroll` 已从透传桩提升为强类型 virtual-scroll 组件，覆盖 dimensions、`items`、`itemHeight`、`itemKey`、`renderless` 和 typed default slot context。
- `VInfiniteScroll` 已从透传桩提升为强类型 infinite-scroll 组件，覆盖 direction、side、mode、margin、load texts、`load` payload、loading/error/empty/load-more scoped slots 和 default slot。
- `VDefaultsProvider` 已从透传桩提升为强类型 defaults-provider 组件，覆盖 defaults object、disabled、reset、root、scoped 和默认 slot。
- `VDatePicker` 已从透传桩提升为强类型 date-picker 组件，覆盖 `modelValue`、multiple/range、min/max、year/month/viewMode update、active/allowedDates、weekdays/weeksInMonth、picker display props、`HeaderText` 映射到 Vuetify `header` prop，以及 `HeaderContent` / `TitleContent` / `Actions` slots。
- `VCalendar`、`VTimePicker`、`VTreeview` 是 Vuetify 3.8.0 labs exports，authoring 桩明确导入 `vuetify/labs/components`；三者均具备专用强类型 props/events/slots；`VHotkey` 不存在于本地 Vuetify 3.8.0，已从 runtime 与 authoring surface 移除。
- Vuetify 官方 slot 中的 `ref` / `computed` / `writable computed` 不会被抹平成 `bool` 或 `string`；C# slot 上下文以 `IVueRef<T>`、`VueComputedRef<T>`、`VueWritableComputedRef<T>` 表达，业务代码读取或写入时使用 `.Value`，例如 `ctx.IsValid.Value`、`ctx.Model.Value`、`ctx.IsFocused.Value`。
- 长尾 Vuetify props 通过 `AdditionalAttributes` 透传，不作为强类型主 API 的替代。
- `VCardTitle` 对齐 Vuetify 3.8.0 `createSimpleFunctional("v-card-title")` 合同：不暴露 `Text` prop，标题文本必须通过默认 `ChildContent` 输出。该规则已由 SFC 单元回归和真实浏览器 smoke 覆盖，避免生成不可见的 `<VCardTitle text="...">`。

## CSS 和自定义参数

使用方式：

- 强类型 props：直接使用组件属性，例如 `Variant`, `Density`, `Items`, `Headers`, `Location`, `Width`。
- CSS class/style：通用写法是通过 `AdditionalAttributes` 透传 `class` / `style`；已显式建模的组件可直接用 `Class` / `Style`，例如 `VTreeview.Class` 和 `VTreeview.Style`。
- 自定义属性：通过 `AdditionalAttributes` 透传 `data-*` / `aria-*`。
- 尚未显式建模的 Vuetify props：通过 `AdditionalAttributes` 透传原始 prop 名。
- 显式 fallthrough 属性：库组件有 `AdditionalAttributes` sink 时，RazorVue 允许直接书写 `class`、`style`、`data-*`、`aria-*`、Vue directive-like attrs、kebab-case raw attrs 和 lower-camel raw Vuetify props；未知 PascalCase 参数仍然报错，用于捕获 C# authoring 拼写错误或漏建模参数。

示例：

```csharp
builder.OpenComponent<VBtn>(0);
builder.AddAttribute(1, nameof(VBtn.Text), "Save");
builder.AddAttribute(2, nameof(VBtn.Variant), VuetifyVariant.Flat);
builder.AddAttribute(3, nameof(VBtn.Loading), "primary");
builder.AddAttribute(4, nameof(VBtn.Rounded), "xl");
builder.AddAttribute(5, "class", "primary-action");
builder.AddAttribute(6, "data-tracking-id", "save-order");
builder.AddAttribute(7, "ripple", false);
builder.AddMultipleAttributes(8, new Dictionary<string, object?>
{
    ["aria-label"] = "Save order",
    ["viewMode"] = "month"
});
builder.CloseComponent();
```

## 质量门

当前已加入的自动化保护：

- `RazorVue_Context_DiscoversVuetifyPackageLibraryDescriptors_FromReferencedAssembly`：验证 authoring 组件集合与 normal/labs runtime exports 的 108 个当前支持组件完全一致，且全部有 capture-unmatched `AdditionalAttributes`。
- `RazorVue_Registry_CreateFromCompilationContext_ResolvesVuetifyPackageComponents`：验证全部 108 个 authoring 组件可从 `ECMAScript.Vuetify` 命名空间解析，并区分 `vuetify/components` 与 `vuetify/labs/components` import specifier。
- `Vuetify_ComponentExports_MatchLocalVuetifyPackageEntrypoints` / `Vuetify_AuthoringComponents_UseMatchingPackageEntrypoints`：验证 C# runtime exports 是本地 Vuetify 3.8.0 d.ts 的真实导出，且 authoring 组件 import specifier 与 normal/labs 入口匹配。
- `RazorVue_Pipeline_LowersVuetifyApplicationShellComponents`：验证 `VApp` / `VNavigationDrawer` / `VAppBar` / `VMain` 可正确降级到 Vuetify import、props 和 update events。
- `RazorVue_Pipeline_LowersVuetifyPackageAdditionalAttributesAndExtendedProps`：验证 `VBtn`、`VTextField`、`VTextarea`、`VCheckbox`、`VSwitch` 的生产主路径 props、`update:focused`、field/details/counter/label/thumb 等官方 slot 和 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifyFabWithStrongProps`：验证 `VFab` 的 `modelValue`、layout/app/absolute、transition object、location/order、VBtn-like 外观/路由 props、default slot 和 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifySpeedDialSnackbarQueueAndSparklineWithStrongProps`：验证 `VSpeedDial` 的 overlay/menu props 与 activator/default slots、`VSnackbarQueue` 的消息 union/options、text/actions slots 和 model update、`VSparkline` 的 item/smooth union、gradient/type/label slot，以及三者的 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifyDefaultsVirtualAndInfiniteScrollWithStrongProps`：验证 `VDefaultsProvider` defaults object、`VVirtualScroll` items/itemKey/renderless/typed default slot、`VInfiniteScroll` load payload、side/mode、status slots、`load-more` 槽名和 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifyInputInfrastructureWithStrongProps`：验证 `VInput`、`VField`、`VSelectionControlGroup`、`VSelectionControl` 的强类型 props、nullable boolean、validation rules、icon-color、ripple、model update、loader/default/details/message/label/input slots 和 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifyGroupedAndDisplayComponentsWithStrongProps`：验证 `VBtnGroup`、`VBtnToggle`、`VCardItem`、`VFooter`、`VRating`、`VTable` 的强类型 props、分组 model update、rating item/item-label scoped slots 和样式/plugin 依赖输出。
- `RazorVue_Pipeline_LowersVuetifyNavigationSurfacesWithStrongProps`：验证 `VBanner`、`VBottomNavigation`、`VBottomSheet`、`VExpansionPanel`、`VEmptyState` 的强类型 props、selected/group model update、bottom-sheet activator slot、expansion title scoped slot、empty-state action emit/named slots 和样式/plugin 依赖输出。
- `RazorVue_Pipeline_LowersVuetifyOverlayAndUtilityComponentsWithStrongProps`：验证 `VOverlay`、`VHover`、`VLazy`、`VResponsive`、`VItemGroup`、`VChipGroup`、`VSlideGroup` 的 overlay events、activator/default scoped slots、intersection observer options、show-arrows union、slide-group arrows、value comparator 与 group model update 输出。
- `RazorVue_Pipeline_LowersVuetifySkeletonLoaderWithStrongProps`：验证 `VSkeletonLoader` 的 theme、elevation、dimensions、loading、boilerplate、loadingText、type 单值/数组、default slot、`AdditionalAttributes`、样式/plugin 依赖输出。
- `RazorVue_Pipeline_LowersVuetifyTimelineWithStrongProps`：验证 `VTimeline` 的 theme/tag/density、size、dot/icon color、line props、align/direction/justify/side/truncateLine 枚举、default slot、`AdditionalAttributes`、样式/plugin 依赖输出。
- `RazorVue_Pipeline_LowersVuetifyShellAndMessageComponentsWithStrongProps`：验证 `VThemeProvider`、`VLayout`、`VSystemBar`、`VLabel`、`VKbd`、`VCounter`、`VMessages`、`VNoSsr`、`VLocaleProvider` 的 shell/layout/locale props、click payload event、counter/message typed slots、transition union、messages object 和 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifyWindowComponentsWithStrongProps`：验证 `VWindow`、`VTabsWindow`、`VTabsWindowItem` 的 group model/update、navigation/touch props、window/tabs scoped slots、transition union、`group:selected` emit 和 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifyWindowComponentsWithStrongProps` 同时验证 `VCarousel` 的 group model/update、cycle/interval/progress、delimiter props、vertical delimiter union、VWindow navigation props、default/prev/next/item scoped slots 和 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifyConfirmEditAndValidationWithStrongProps`：验证 `VConfirmEdit` 的 model update、`save`/`cancel` emits、disabled action union、typed default slot 和 `VValidation` 的 focused/model update、nullable boolean、validation rules、computed/shallow ref slot context、Promise-returning methods 与 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifyCollectionComponentsWithStrongItemContracts`：验证 select/breadcrumb/data-table 强 item/header 合同输出；同时覆盖 `VSelect` / `VAutocomplete` 的 field/menu/filter props、`update:menu`、`update:focused`、`update:search` 与 select-like 官方命名 slot；并覆盖 `VDataTable` 的 `modelValue`、`page`、`itemsPerPage`、`sortBy`、`groupBy`、`expanded`、`update:options`、`update:currentItems`、选择/展开/loading/no-data/table 外观 props，以及 `top`、`headers`、`header.data-table-select`、`item`、`footer.prepend`、`no-data` 等命名 slot。
- `RazorVue_Pipeline_LowersVuetifyDatePickerWithStrongProps`：验证 `VDatePicker` 的 date model、month/year/view-mode updates、multiple/range、allowed dates、calendar options、picker display props、header/title/actions slots 和 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifyCalendarWithStrongProps`：验证 labs `VCalendar` 的 date model、allowed dates、events、interval options、next/prev emits、header/event scoped slots 和 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifyTimePickerWithStrongProps`：验证 labs `VTimePicker` 的 time model、allowed hours/minutes/seconds、format/view-mode/period/hour/minute/second updates、picker display props、default/title/actions slots 和 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifyTreeviewWithStrongProps`：验证 labs `VTreeview` 的 model/activated/selected/opened values、items、active/select strategies、lazy load children、filter props、class/style、icons、click payload emits、tree item scoped slots 和 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersPromotedVuetifyInputComponentsWithStrongContracts`：验证 combobox/file-input/number-input/otp/radio/slider/range-slider 的强类型 props、union 值、filter/search props、select-like slots 和 update events。
- `RazorVue_Pipeline_LowersVuetifySelectLikeComponentsWithStrongSelectedValueModel`：验证 `VSelect` / `VAutocomplete` / `VCombobox` 的强类型 `SelectedValue` 降级到 `modelValue`，并覆盖 object 与 multiple array model。
- `RazorVue_Pipeline_WithDuplicateVuetifyModelValueAuthoringAliases_ReportsUnknownParameter` / `RazorVue_Pipeline_WithDuplicateVuetifyModelUpdateAuthoringAliases_ReportsUnknownParameter`：验证 `ModelValue` 与 `SelectedValue`、`ModelValueChanged` 与 `SelectedValueChanged` 不会重复映射到同一个 Vue 目标。
- `RazorVue_SfcArtifactFactory_WithDuplicateLibraryMappedModelProp_ThrowsUnknownParameter` / `RazorVue_SfcArtifactFactory_WithDuplicateLibraryMappedModelUpdateEmit_ThrowsUnknownParameter` / `RazorVue_SfcArtifactFactory_WithInvalidLibraryBindTarget_ThrowsInvalidBindTarget`：验证 SFC/canonical 输出路径与 BuildRenderTree 输出路径使用一致的库组件 authoring 校验，不会在 SFC 模式静默生成重复 `modelValue` / `update:modelValue` 或错误双向绑定。
- `RazorVue_SfcArtifactFactory_LowersExplicitLibraryFallthroughAttributes_WhenTargetHasCaptureUnmatchedValues` / `RazorVue_Pipeline_LowersExplicitLibraryFallthroughAttributes_WhenTargetHasCaptureUnmatchedValues`：验证显式 `class`、`style`、`data-*`、`aria-*`、kebab-case 和 lower-camel raw attrs 可通过库组件 `AdditionalAttributes` sink 透传，且 PascalCase 未知参数仍失败。
- `RazorVue_SfcArtifactFactory_WithImplicitLibraryDefaultSlotOnComponentWithoutChildContent_ThrowsUnknownSlot` / `RazorVue_SfcArtifactFactory_WithUnknownRenderFragmentLibraryAttribute_ThrowsUnknownSlot` / `RazorVue_SfcArtifactFactory_WithUnknownLibrarySlotTemplate_ThrowsUnknownSlot` / `RazorVue_SfcArtifactFactory_WithImplicitTypedLibraryDefaultSlot_ThrowsSlotContextMisuse` / `RazorVue_SfcArtifactFactory_WithDuplicateLibraryDefaultSlotAssignment_ThrowsDuplicateSlotValue` / `RazorVue_SfcArtifactFactory_WithDuplicateLibraryNamedSlotAssignment_ThrowsDuplicateSlotValue`：验证 SFC/canonical 输出路径不会绕过库组件 slot authoring 合同，未声明 slot、typed slot 误用、重复 slot 赋值都会失败。
- `RazorVue_Pipeline_LowersVuetifyCollectionComponentsWithStrongItemContracts` 同时验证 RazorVue h() 输出会正确引用包含 `.` / `-` 的 Vuetify slot 名；slot object key 会输出为字符串 key，避免生成无效 JavaScript。
- `RazorVue_SfcArtifactFactory_LowersLibrarySlotNamesWithDots_ToVueSlotTemplates`：验证 SFC 输出路径对 `header.data-table-select` / `footer.prepend` 等带点号 Vuetify slot 使用 Vue 动态 slot 参数形式，避免普通 `#name.modifier` 语法误解。
- `RazorVue_Pipeline_LowersVuetifyFeedbackAndListComposition` / `RazorVue_Pipeline_LowersVuetifyNavigationAndFeedbackComposition`：验证 chip/snackbar 等反馈组件的 `Boolean | String`、`Boolean | Number | String`、`Number | String` prop 降级输出。
- `RazorVue_Pipeline_LowersVuetifyCardPromotedPropsAndNamedSlots`：验证 `VCard` 的 title/subtitle/text、视觉/尺寸/路由 props 与 `title` / `text` / `actions` 等命名槽输出。
- `RazorVue_SfcArtifactFactory_LowersVuetifyCardTitle_DefaultSlot`：验证 `VCardTitle` 标题文本走默认 slot，且不会生成无效 `text` prop。
- `RazorVue_Pipeline_LowersVuetifyFeedbackAndListComposition`：同时验证 `VList` 的 items/item-title/item-value/item-children/item-props/lines/slim/bgColor/variant，以及 `VListItem` 的 icon/active/link/ripple/typed append slot 输出。
- `RazorVue_Pipeline_LowersVuetifyTooltipStrongTypedLocationAndAdditionalAttributes` / `RazorVue_Pipeline_LowersVuetifyNavigationAndFeedbackComposition`：验证 `VTooltip` / `VMenu` 的 model update、typed activator slot、location/origin/offset/delay/dimension/transition、`activatorProps` / `contentProps` 等 overlay authoring 面。
- `Vuetify_AuthoringComponents_ExposeOnlyAdditionalAttributesAsObjectSink`：验证 authoring 组件公开属性除 `AdditionalAttributes` 外不使用 `object`。
- `Vuetify_ValueAndUnionTypes_ExposeStronglyTypedContracts`：验证核心 union/value 类型公开合同。
- `Vuetify_InputInfrastructureSlotContexts_PreserveOfficialRefContracts`：验证 input/field/selection-control/switch slot context 保留 Vuetify 官方 `Ref`、`ComputedRef`、`WritableComputedRef` 合同，不退化为标量值。
- `Build_LocalPackages_RazorVueTodoListSample_PureDenoPipeline_PassesInIsolatedWorkspace` / `Build_LocalPackages_WithExternalRazorSgSfcConsumer_PureDenoPipeline_PassesInIsolatedWorkspace`：验证本地 NuGet 包消费、Razor SG SFC 输出、纯 Deno SFC 预编译、`deno bundle`、`Deno.bundle()`、SSR smoke 和真实浏览器 mount smoke。浏览器 smoke 要求无 console warning/error、无网络失败、加载生成 CSS/JS、存在 Vuetify `.v-application` root，并覆盖 TodoList 核心交互或外部 consumer 的可见文本。

## 剩余风险

- 当前 108 个 authoring 组件均已有强类型主路径，但这仍不是 Vuetify 官方每个 prop/event/slot 的逐项完整镜像；低频实验 props 继续通过 `AdditionalAttributes` 或后续按业务提升。
- 本程序集当前只显式支持 3 个历史兼容 labs runtime exports。Vuetify 3.8.0 labs 入口还包含 `VDateInput`、`VFileUpload`、`VIconBtn`、`VPicker`、`VStepperVertical`、`VPullToRefresh` 等实验导出；这些未纳入当前 production surface，避免未经建模的实验 API 被误认为稳定代理。
- `VSelect` / `VAutocomplete` / `VCombobox` 当前以 `SelectedValue` 并行入口覆盖复杂 `modelValue`，没有把 `ModelValue` 破坏性改为泛型。后续如果 RazorVue 组件解析安全支持泛型库组件，可再评估把这组三组件升级为泛型 authoring API；升级前必须保持现有 `ModelValue string?` 兼容入口。
- 部分布局枚举类 props 为了保持 Razor 字面量易用性仍保留 `string?`，例如 `VRow.Align` / `VRow.Justify`。后续如要强约束，应设计不破坏 `<VRow Justify="center">` 的 authoring 迁移策略。
- 本清单确认的是代理层、编译降级和类型合同。最终业务上线仍需要目标应用跑完整 RazorVue/Jolt/emit 集成验证。
