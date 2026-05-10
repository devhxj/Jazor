# ECMAScript.Vuetify 生产化清单

> Status: current production-readiness snapshot  
> Date: 2026-05-10  
> Scope: `src/ECMAScript.Vuetify` as a Vuetify 3.8.0 binding and RazorVue authoring proxy layer.
> Source: Vuetify 3.8.0 component surface, <https://github.com/vuetifyjs/vuetify/tree/v3.8.0/packages/vuetify/src/components>.

## 结论

`ECMAScript.Vuetify` 可以作为 RazorVue Vuetify 代理层进入生产集成验证。当前不是“Vuetify 全量 props 强类型完整代理”，而是分层覆盖：

- Runtime registry 覆盖 Vuetify component export surface：`VuetifyComponents` 暴露 109 个组件导出，`VuetifyComponentRegistry` 可用于 `CreateVuetify(...)`。
- RazorVue authoring 组件覆盖全部 109 个 runtime component exports。50 个高频组件具备专用强类型 props；其余 59 个组件提供生产可用透传桩。
- 所有 RazorVue authoring 组件都提供 `[Parameter(CaptureUnmatchedValues = true)] AdditionalAttributes`，用于 `class`、`style`、`data-*`、`aria-*` 和尚未强类型建模的 Vuetify 长尾 props。
- 公共强类型主路径不使用 `Either<>` / `IEither`，也不新增 `object` / `object?` catch-all；唯一允许的 `object?` 是 Blazor/RazorVue 约定的 `AdditionalAttributes` sink。

## Authoring 覆盖

当前专用强类型 RazorVue authoring 组件：

`VAlert`, `VApp`, `VAppBar`, `VAutocomplete`, `VAvatar`, `VBadge`, `VBreadcrumbs`, `VBtn`, `VCard`, `VCardText`, `VCardTitle`, `VCheckbox`, `VChip`, `VCol`, `VCombobox`, `VContainer`, `VDataTable`, `VDialog`, `VDivider`, `VFileInput`, `VForm`, `VIcon`, `VImg`, `VList`, `VListItem`, `VMain`, `VMenu`, `VNavigationDrawer`, `VNumberInput`, `VOtpInput`, `VPagination`, `VProgressCircular`, `VProgressLinear`, `VRadio`, `VRadioGroup`, `VRangeSlider`, `VRow`, `VSelect`, `VSheet`, `VSlider`, `VSnackbar`, `VSpacer`, `VSwitch`, `VTab`, `VTabs`, `VTextarea`, `VTextField`, `VToolbar`, `VToolbarTitle`, `VTooltip`.

当前透传型 RazorVue authoring 组件：

`VBanner`, `VBottomNavigation`, `VBottomSheet`, `VBtnGroup`, `VBtnToggle`, `VCalendar`, `VCardActions`, `VCardItem`, `VCardSubtitle`, `VCarousel`, `VChipGroup`, `VCode`, `VColorPicker`, `VConfirmEdit`, `VCounter`, `VDataIterator`, `VDatePicker`, `VDefaultsProvider`, `VEmptyState`, `VExpansionPanel`, `VFab`, `VField`, `VFooter`, `VHotkey`, `VHover`, `VInfiniteScroll`, `VInput`, `VItemGroup`, `VKbd`, `VLabel`, `VLayout`, `VLazy`, `VLocaleProvider`, `VMessages`, `VNoSsr`, `VOverlay`, `VParallax`, `VRating`, `VResponsive`, `VSelectionControl`, `VSelectionControlGroup`, `VSkeletonLoader`, `VSlideGroup`, `VSnackbarQueue`, `VSparkline`, `VSpeedDial`, `VStepper`, `VSystemBar`, `VTable`, `VTabsWindow`, `VTabsWindowItem`, `VThemeProvider`, `VTimeline`, `VTimePicker`, `VToolbarItems`, `VTreeview`, `VValidation`, `VVirtualScroll`, `VWindow`.

## 强类型策略

已补齐或约束的关键类型：

- Field common props: `VuetifyFieldVariant`, `VuetifyDensity`, `VuetifyHideDetailsValue`, `VuetifyMessagesValue`, `VuetifyValidateOn`。
- App shell props: `VuetifyAppBarLocation`, `VuetifyNavigationDrawerLocation`, `VuetifyScrimValue`。
- Select/autocomplete/combobox contracts: `VuetifySelectItems`, `VuetifySelectItemValue`, `VuetifySelectItem`, `VuetifySelectItemKey`, `VuetifySelectItemPropsSelector`, `VuetifyItemProps`, `VuetifySelectModelValue`, `VuetifySelectModelValues`。
- Breadcrumb item contracts: `VuetifyBreadcrumbItems`, `VuetifyBreadcrumbItemValue`, `VuetifyBreadcrumbItem`。
- Data table contracts: `VuetifyDataTableHeaders`, `VuetifyDataTableHeader`, `VuetifyDataTableHeaderAlign`, `VuetifyDataTableItems`, `VuetifyDataTableItem`, `VuetifyDataTableSelectedValues`, `VuetifyDataTableSortItems`, `VuetifyDataTableSortItem`, `VuetifyDataTableSortOrder`, `VuetifyDataTableSelectStrategy`, `VuetifyDataTableOptions`, `VuetifyDataTableItemsPerPageOptions`, `VuetifyDataTableRowProps`, `VuetifyDataTableCellProps`, `VDataTableSlotContext`, `VDataTableHeadersSlotContext`, `VDataTableHeaderCellSlotContext`, `VDataTableItemSlotContext`, `VDataTableGroupHeaderSlotContext`。
- List/card/overlay composition contracts: `VuetifyListLines`, `VuetifyListLineMode`, `VuetifyRippleValue`, `VListItemSlotContext`, `VListItemTitleSlotContext`, `VListItemSubtitleSlotContext`, `VOverlayActivatorContext`。
- Form/input control contracts: `VuetifyAutoSelectFirstValue`, `VuetifyFileShowSizeValue`, `VuetifyFileModelValue`, `VuetifyNumberInputControlVariant`, `VuetifyBooleanAlwaysValue`, `VuetifyBooleanStringValue`, `VuetifyCounterValue`, `VuetifyProgressCircularIndeterminateValue`, `VuetifyAlwaysMode`, `VuetifySliderDirection`, `VuetifyRangeSliderModelValue`。
- Display/options contracts: `VuetifyDisplayBreakpoint`, `VuetifyDisplayThresholds`。
- Visual/value contracts: `VuetifyRoundedValue`, `VuetifyTextValue`，以及尺寸、长度、进度、海拔、分页长度等 `Number | String` props 统一使用 `VueStringNumberValue`。

设计约束：

- `string | number` 类 props 使用现有 `VueStringNumberValue`。
- `bool | string` 类 Vuetify props 使用专用 `[ECMAScriptUnion]` 类型，例如 `VuetifyScrimValue` / `VuetifyBooleanStringValue`。
- `bool | number | string` 类 props 按语义拆分为专用 union，例如 `VuetifyCounterValue` 和 `VuetifyRoundedValue`，避免一个过宽的通用类型掩盖 prop 语义。
- 集合 item 对象使用 `[ECMAScript]` class 和 collection initializer 友好的 indexer / `Add(...)`，避免弱化为 `object`。
- `VSelect` / `VAutocomplete` / `VCombobox` 保留 `ModelValue string?` 兼容主路径，同时提供 `SelectedValue` / `SelectedValueChanged` 并行强类型入口覆盖 Vuetify `modelValue` 的 string、number、bool、symbol、object 和 multiple array 场景。RazorVue 会拒绝同一组件上同时使用两个映射到 `modelValue` 或 `update:modelValue` 的 authoring 参数，避免生成重复 Vue prop/event。
- 长尾 Vuetify props 通过 `AdditionalAttributes` 透传，不作为强类型主 API 的替代。
- 透传型 authoring 桩只承诺组件导入、样式依赖、插件依赖、默认 slot 和 `AdditionalAttributes`；业务常用后再提升为专用强类型 props，避免一次性引入不准确的弱类型 API。

## CSS 和自定义参数

使用方式：

- 强类型 props：直接使用组件属性，例如 `Variant`, `Density`, `Items`, `Headers`, `Location`, `Width`。
- CSS class/style：通过 `AdditionalAttributes` 透传 `class` / `style`。
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

- `RazorVue_Context_DiscoversVuetifyLibraryComponents`：验证 authoring 组件集合与 `VuetifyComponents` 109 个 runtime exports 完全一致，且全部有 capture-unmatched `AdditionalAttributes`。
- `RazorVue_Registry_ResolvesBuiltInVuetifyLibraryComponents`：验证全部 109 个 authoring 组件可从 `ECMAScript.Vuetify` 命名空间解析。
- `RazorVue_Pipeline_LowersVuetifyApplicationShellComponents`：验证 `VApp` / `VNavigationDrawer` / `VAppBar` / `VMain` 可正确降级到 Vuetify import、props 和 update events。
- `RazorVue_Pipeline_LowersVuetifyCollectionComponentsWithStrongItemContracts`：验证 select/breadcrumb/data-table 强 item/header 合同输出；同时覆盖 `VDataTable` 的 `modelValue`、`page`、`itemsPerPage`、`sortBy`、`groupBy`、`expanded`、`update:options`、`update:currentItems`、选择/展开/loading/no-data/table 外观 props，以及 `top`、`headers`、`header.data-table-select`、`item`、`footer.prepend`、`no-data` 等命名 slot。
- `RazorVue_Pipeline_LowersPromotedVuetifyInputComponentsWithStrongContracts`：验证 combobox/file-input/number-input/otp/radio/slider/range-slider 的强类型 props、union 值和 update events。
- `RazorVue_Pipeline_LowersVuetifySelectLikeComponentsWithStrongSelectedValueModel`：验证 `VSelect` / `VAutocomplete` / `VCombobox` 的强类型 `SelectedValue` 降级到 `modelValue`，并覆盖 object 与 multiple array model。
- `RazorVue_Pipeline_WithDuplicateVuetifyModelValueAuthoringAliases_ReportsUnknownParameter` / `RazorVue_Pipeline_WithDuplicateVuetifyModelUpdateAuthoringAliases_ReportsUnknownParameter`：验证 `ModelValue` 与 `SelectedValue`、`ModelValueChanged` 与 `SelectedValueChanged` 不会重复映射到同一个 Vue 目标。
- `RazorVue_SfcArtifactFactory_WithDuplicateLibraryMappedModelProp_ThrowsUnknownParameter` / `RazorVue_SfcArtifactFactory_WithDuplicateLibraryMappedModelUpdateEmit_ThrowsUnknownParameter` / `RazorVue_SfcArtifactFactory_WithInvalidLibraryBindTarget_ThrowsInvalidBindTarget`：验证 SFC/canonical 输出路径与 BuildRenderTree 输出路径使用一致的库组件 authoring 校验，不会在 SFC 模式静默生成重复 `modelValue` / `update:modelValue` 或错误双向绑定。
- `RazorVue_SfcArtifactFactory_LowersExplicitLibraryFallthroughAttributes_WhenTargetHasCaptureUnmatchedValues` / `RazorVue_Pipeline_LowersExplicitLibraryFallthroughAttributes_WhenTargetHasCaptureUnmatchedValues`：验证显式 `class`、`style`、`data-*`、`aria-*`、kebab-case 和 lower-camel raw attrs 可通过库组件 `AdditionalAttributes` sink 透传，且 PascalCase 未知参数仍失败。
- `RazorVue_SfcArtifactFactory_WithImplicitLibraryDefaultSlotOnComponentWithoutChildContent_ThrowsUnknownSlot` / `RazorVue_SfcArtifactFactory_WithUnknownRenderFragmentLibraryAttribute_ThrowsUnknownSlot` / `RazorVue_SfcArtifactFactory_WithUnknownLibrarySlotTemplate_ThrowsUnknownSlot` / `RazorVue_SfcArtifactFactory_WithImplicitTypedLibraryDefaultSlot_ThrowsSlotContextMisuse` / `RazorVue_SfcArtifactFactory_WithDuplicateLibraryDefaultSlotAssignment_ThrowsDuplicateSlotValue` / `RazorVue_SfcArtifactFactory_WithDuplicateLibraryNamedSlotAssignment_ThrowsDuplicateSlotValue`：验证 SFC/canonical 输出路径不会绕过库组件 slot authoring 合同，未声明 slot、typed slot 误用、重复 slot 赋值都会失败。
- `RazorVue_Pipeline_LowersVuetifyCollectionComponentsWithStrongItemContracts` 同时验证 RazorVue h() 输出会正确引用包含 `.` / `-` 的 Vuetify slot 名；slot object key 会输出为字符串 key，避免生成无效 JavaScript。
- `RazorVue_SfcArtifactFactory_LowersLibrarySlotNamesWithDots_ToVueSlotTemplates`：验证 SFC 输出路径对 `header.data-table-select` / `footer.prepend` 等带点号 Vuetify slot 使用 Vue 动态 slot 参数形式，避免普通 `#name.modifier` 语法误解。
- `RazorVue_Pipeline_LowersVuetifyFeedbackAndListComposition` / `RazorVue_Pipeline_LowersVuetifyNavigationAndFeedbackComposition`：验证 chip/snackbar 等反馈组件的 `Boolean | String`、`Boolean | Number | String`、`Number | String` prop 降级输出。
- `RazorVue_Pipeline_LowersVuetifyCardPromotedPropsAndNamedSlots`：验证 `VCard` 的 title/subtitle/text、视觉/尺寸/路由 props 与 `title` / `text` / `actions` 等命名槽输出。
- `RazorVue_Pipeline_LowersVuetifyFeedbackAndListComposition`：同时验证 `VList` 的 items/item-title/item-value/item-children/item-props/lines/slim/bgColor/variant，以及 `VListItem` 的 icon/active/link/ripple/typed append slot 输出。
- `RazorVue_Pipeline_LowersVuetifyTooltipStrongTypedLocationAndAdditionalAttributes` / `RazorVue_Pipeline_LowersVuetifyNavigationAndFeedbackComposition`：验证 `VTooltip` / `VMenu` 的 model update、typed activator slot、location/origin/offset/delay/dimension/transition、`activatorProps` / `contentProps` 等 overlay authoring 面。
- `Vuetify_AuthoringComponents_ExposeOnlyAdditionalAttributesAsObjectSink`：验证 authoring 组件公开属性除 `AdditionalAttributes` 外不使用 `object`。
- `Vuetify_ValueAndUnionTypes_ExposeStronglyTypedContracts`：验证核心 union/value 类型公开合同。

## 剩余风险

- 透传型的 59 个 Vuetify 组件还没有专用强类型 props；如果业务频繁使用这些组件，应按同样规则提升为强类型桩并补降级测试。
- `VSelect` / `VAutocomplete` / `VCombobox` 当前以 `SelectedValue` 并行入口覆盖复杂 `modelValue`，没有把 `ModelValue` 破坏性改为泛型。后续如果 RazorVue 组件解析安全支持泛型库组件，可再评估把这组三组件升级为泛型 authoring API；升级前必须保持现有 `ModelValue string?` 兼容入口。
- 部分布局枚举类 props 为了保持 Razor 字面量易用性仍保留 `string?`，例如 `VRow.Align` / `VRow.Justify`。后续如要强约束，应设计不破坏 `<VRow Justify="center">` 的 authoring 迁移策略。
- 本清单确认的是代理层、编译降级和类型合同。最终业务上线仍需要目标应用跑完整 RazorVue/Jolt/emit 集成验证。
