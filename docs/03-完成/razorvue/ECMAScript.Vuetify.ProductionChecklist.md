# ECMAScript.Vuetify 生产化清单

> Status: current production-readiness snapshot  
> Date: 2026-05-12
> Scope: `src/ECMAScript.Vuetify` as a Vuetify 3.8.0 binding and RazorVue authoring proxy layer.
> Source: local Vuetify 3.8.0 d.ts surface under `node_modules/vuetify/lib/components/index.d.ts` and `node_modules/vuetify/lib/labs/components.d.ts`.

## 结论

`ECMAScript.Vuetify` 可以作为 RazorVue Vuetify 代理层进入生产集成验证。当前不是“Vuetify 全量 props 强类型完整代理”，而是分层覆盖：

- Runtime registry 覆盖当前支持的 Vuetify component export surface：`VuetifyComponents` 暴露 105 个 `vuetify/components` 导出，`VuetifyLabsComponents` 暴露 9 个 `vuetify/labs/components` 导出；对应 `VuetifyComponentRegistry` / `VuetifyLabsComponentRegistry` 可用于 `CreateVuetify(...)` 的 `components` 配置。
- RazorVue authoring 组件覆盖全部 114 个当前支持的 runtime component exports。114 个组件均具备专用强类型 props。
- 所有 RazorVue authoring 组件都提供 `[Parameter(CaptureUnmatchedValues = true)] AdditionalAttributes`，用于 `class`、`style`、`data-*`、`aria-*` 和尚未强类型建模的 Vuetify 长尾 props。
- 公共强类型主路径不使用通用二选一封装，也不新增 `object` / `object?` catch-all；唯一允许的 `object?` 是 Blazor/RazorVue 约定的 `AdditionalAttributes` sink。

## Authoring 覆盖

当前专用强类型 RazorVue authoring 组件：

`VAlert`, `VApp`, `VAppBar`, `VAutocomplete`, `VAvatar`, `VBadge`, `VBanner`, `VBottomNavigation`, `VBottomSheet`, `VBreadcrumbs`, `VBtn`, `VBtnGroup`, `VBtnToggle`, `VCalendar`, `VCard`, `VCardActions`, `VCardItem`, `VCardSubtitle`, `VCardText`, `VCardTitle`, `VCarousel`, `VCheckbox`, `VChip`, `VChipGroup`, `VCode`, `VCol`, `VColorPicker`, `VCombobox`, `VConfirmEdit`, `VContainer`, `VCounter`, `VDataIterator`, `VDataTable`, `VDateInput`, `VDatePicker`, `VDefaultsProvider`, `VDialog`, `VDivider`, `VEmptyState`, `VExpansionPanel`, `VFab`, `VField`, `VFileInput`, `VFileUpload`, `VFooter`, `VForm`, `VHover`, `VIcon`, `VIconBtn`, `VImg`, `VInfiniteScroll`, `VInput`, `VItemGroup`, `VKbd`, `VLabel`, `VLayout`, `VLazy`, `VList`, `VListItem`, `VLocaleProvider`, `VMain`, `VMenu`, `VMessages`, `VNavigationDrawer`, `VNoSsr`, `VNumberInput`, `VOtpInput`, `VOverlay`, `VPagination`, `VParallax`, `VPicker`, `VProgressCircular`, `VProgressLinear`, `VPullToRefresh`, `VRadio`, `VRadioGroup`, `VRangeSlider`, `VRating`, `VResponsive`, `VRow`, `VSelect`, `VSelectionControl`, `VSelectionControlGroup`, `VSheet`, `VSkeletonLoader`, `VSlider`, `VSlideGroup`, `VSnackbar`, `VSnackbarQueue`, `VSpacer`, `VSparkline`, `VSpeedDial`, `VStepper`, `VStepperVertical`, `VSwitch`, `VSystemBar`, `VTab`, `VTable`, `VTabs`, `VTabsWindow`, `VTabsWindowItem`, `VTextarea`, `VTextField`, `VThemeProvider`, `VTimePicker`, `VTimeline`, `VToolbar`, `VToolbarItems`, `VToolbarTitle`, `VTooltip`, `VTreeview`, `VValidation`, `VVirtualScroll`, `VWindow`.

## 强类型策略

已补齐或约束的关键类型：

- Field common props: `VuetifyFieldVariant`, `VuetifyDensity`, `VuetifyHideDetailsValue`, `VuetifyMessagesValue`, `VuetifyValidateOn`。
- App shell props: `VuetifyAppBarLocation`, `VuetifyNavigationDrawerLocation`, `VuetifyScrimValue`。
- Button/group/rating contracts: `VuetifyBorderValue`, `VuetifyIconValue`, `VuetifyMandatoryValue`, `VuetifyMandatoryMode`, `VuetifyGroupModelValue`, `VuetifyGroupModelValues`, `VuetifyShowArrowsValue`, `VuetifyValueComparator`, `VuetifyItemLabelPosition`, `VItemGroupDefaultSlotContext`, `VSlideGroupSlotContext`, `VRatingItemSlotContext`, `VRatingItemLabelSlotContext`。
- Select/autocomplete/combobox contracts: `VuetifySelectItems`, `VuetifySelectItemValue`, `VuetifySelectItem`, `VuetifyListItem`, `VuetifySelectItemKey`, `VuetifySelectItemPropsSelector`, `VuetifySelectValueComparator`, `VuetifyFilterFunction`, `VuetifyFilterKeyFunctions`, `VuetifyFilterKeys`, `VuetifyFilterMatch`, `VuetifyFilterMode`, `VuetifyItemProps`, `VuetifySelectModelValue`, `VuetifySelectModelValues`, `VSelectItemSlotContext`, `VSelectChipSlotContext`, `VSelectSelectionSlotContext`。
- Breadcrumb item contracts: `VuetifyBreadcrumbItems`, `VuetifyBreadcrumbItemValue`, `VuetifyBreadcrumbItem`。
- Data table contracts: `VuetifyDataTableHeaders`, `VuetifyDataTableHeader`, `VuetifyDataTableHeaderAlign`, `VuetifyDataTableItems`, `VuetifyDataTableItem`, `VuetifyDataTableSelectedValues`, `VuetifyDataTableSortItems`, `VuetifyDataTableSortItem`, `VuetifyDataTableSortOrder`, `VuetifyDataTableSelectStrategy`, `VuetifyDataTableOptions`, `VuetifyDataTableItemsPerPageOptions`, `VuetifyDataTableRowProps`, `VuetifyDataTableCellProps`, `VDataTableSlotContext`, `VDataTableHeadersSlotContext`, `VDataTableHeaderCellSlotContext`, `VDataTableItemSlotContext`, `VDataTableGroupHeaderSlotContext`。
- List/card/overlay composition contracts: `VuetifyListLines`, `VuetifyListLineMode`, `VuetifyRippleValue`, `VuetifyLocationStrategy`, `VuetifyAttachTarget`, `VuetifyOverlayTarget`, `VuetifyOverlayActivatorTarget`, `VuetifyDialogTarget`, `VuetifyDialogActivatorTarget`, `VuetifyOverlayCoordinateTarget`, `VListItemSlotContext`, `VListItemTitleSlotContext`, `VListItemSubtitleSlotContext`, `VOverlayActivatorContext`, `VDialogActivatorContext`, `VSnackbarActionsSlotContext`。
- Alert contracts: `VuetifyAlertBorderValue`, `VuetifyAlertBorderSide`, `VuetifyAlertIconValue`, `VAlertCloseSlotContext`，覆盖 Vuetify `border: bool | "top" | "end" | "bottom" | "start"`、`icon: false | IconValue` 和 close slot props。
- Chip contracts: `VChipDefaultSlotContext`, `VChipSelectedClassValue`, `VChipSelectCallback`，覆盖 Vuetify chip default slot 的 `isSelected`、`selectedClass`、`select`、`toggle`、`value`、`disabled` 上下文，以及 `selectedClass: bool | string[]` 槽字段。
- Avatar/badge contracts: `VAvatar` 复用 `VuetifyIconValue`、`VuetifyBorderValue`、`VueClassValue`、`VuetifyStyleValue` 覆盖 Vuetify media identity 主路径；`VBadge` 复用 `VuetifyTransitionValue`、`VuetifyLocation`、`VuetifyIconValue` 并声明 `BadgeContent` -> Vue `badge` slot。
- Sheet/icon contracts: `VSheet` 复用 `VuetifyRoundedValue`、`VuetifyPosition`、`VuetifyLocation`、`VuetifyBorderValue`、`VueClassValue`、`VuetifyStyleValue` 覆盖 Vuetify sheet display surface；`VIcon` 复用 `VuetifyIconValue`、`VueStringNumberValue`、`VueClassValue`、`VuetifyStyleValue` 覆盖 icon visual/content surface。
- Toolbar contracts: `VToolbar` 使用 `VuetifyToolbarDensityValue` 覆盖 Vuetify toolbar 专用 `null | prominent | default | comfortable | compact` density 域，并复用 rounded/border/CssClass/CssStyle/dimension contracts 覆盖 image/prepend/append/title/extension/default slots；`VToolbarItems` 覆盖 color/variant/CssClass/CssStyle/default slot；`VToolbarTitle` 覆盖 tag/CssClass/CssStyle/text/default/text slots。
- Grid contracts: `VContainer` 覆盖 tag/dimensions/CssClass/CssStyle/fluid/default slot；`VRow` 覆盖 tag/CssClass/CssStyle、align/justify/alignContent 与 breakpoint variants、dense/noGutters/default slot；`VCol` 使用 `VuetifyGridSpanValue` 覆盖 `bool | number | string` 的 cols/sm/md/lg/xl/xxl span 域，并覆盖 order/offset breakpoint variants、alignSelf/CssClass/CssStyle/default slot；`VSpacer` 覆盖 tag/CssClass/CssStyle/default slot。`VRow`/`VCol` 的 alignment 字符串域保留 `string?`，避免破坏 Razor 静态属性 authoring。
- Progress contracts: `VProgressCircularDefaultSlotContext`、`VProgressLinearDefaultSlotContext`，覆盖 Vuetify progress default slot 的 `value` / `buffer` 数字上下文；`VProgressLinear.ModelValueChanged` 对齐官方 `update:modelValue` 的 numeric payload。
- Image contracts: `VImgSource`, `VImgSourceObject`, `VImgDraggableValue`, `VImgCrossOrigin`, `VImgReferrerPolicy`，覆盖 Vuetify `src: string | srcObject`、必填 `srcObject.aspect: number`、`draggable: bool | "true" | "false"`、`crossorigin` 和 `referrerpolicy` 字符串域。
- Form/input control contracts: `VuetifyAutoSelectFirstValue`, `VuetifyFileShowSizeValue`, `VuetifyFileModelValue`, `VuetifyNumberInputControlVariant`, `VuetifyBooleanAlwaysValue`, `VuetifyBooleanStringValue`, `VuetifyNullableBoolean`, `VuetifyIconColorValue`, `VuetifyValidationResult`, `VuetifyValidationRule`, `VuetifyValidationRuleResolver`, `VuetifyAsyncValidationRuleResolver`, `VuetifyCounterValue`, `VuetifyCounterValueSource`, `VuetifyTextModelModifiers`, `VuetifyProgressCircularIndeterminateValue`, `VuetifyAlwaysMode`, `VuetifySliderDirection`, `VuetifyRangeSliderModelValue`, `VuetifyTransitionValue`, `VFieldSlotContext`, `VInputSlotContext`, `VInputDetailsSlotContext`, `VCounterSlotContext`, `VCounterDefaultSlotContext`, `VMessagesMessageSlotContext`, `VSelectionControlDefaultSlotContext`, `VSelectionControlInputDefaultSlotContext`, `VSelectionControlLabelSlotContext`, `VSelectionControlInputSlotContext`, `VSelectionControlInputProps`, `VSwitchSlotContext`, `VuetifyLoaderSlotContext`, `VuetifyCssProperties`。
- Form contracts: `VFormSubmitEvent`, `VuetifyFormValidationResult`, `VuetifyFormFieldValidationResult`, `VuetifyFormField`, `VFormDefaultSlotContext`，覆盖 Vuetify `submit` 事件的 native `SubmitEvent` + validation promise 双合同，以及 default slot 的 validation refs、field registry、reset/resetValidation/validate callbacks。
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
- `VAlert` 已从窄表面提升为强类型反馈组件，覆盖 `modelValue`、`click:close`、theme/tag/rounded/tile/position/location/dimensions/CssClass/CssStyle、`border`/`borderColor`、close/icon/prominent/title/text、prepend/title/text/append/close/default slots；`Icon = VuetifyAlertIconValue.None()` 明确降级为 Vuetify 所需的 `false`。
- `VSelect` / `VAutocomplete` / `VCombobox` 保留 `ModelValue string?` 兼容主路径，同时提供 `SelectedValue` / `SelectedValueChanged` 并行强类型入口覆盖 Vuetify `modelValue` 的 string、number、bool、symbol、object 和 multiple array 场景。三者共享 select-like authoring 基类，覆盖 field/common props、menu/open/focused 模型、item/value/comparator、`item` / `chip` / `selection` / `prepend-item` / `append-item` / `no-data` 官方槽位；`VAutocomplete` / `VCombobox` 额外覆盖 `search` 模型和 filter props。RazorVue 会拒绝同一组件上同时使用两个映射到 `modelValue` 或 `update:modelValue` 的 authoring 参数，避免生成重复 Vue prop/event。
- `VInput` / `VField` / `VSelectionControl` / `VSelectionControlGroup` 已从透传桩提升为强类型基础设施组件，直接覆盖 Vuetify input、field、selection-control 的官方 props、model update、validation、loader、label/input/details/message slots。
- `VValidation` 已从透传桩提升为强类型 validation 组件，覆盖 `focused`、nullable disabled/readonly、error messages、rules、model/validation value、validate-on、typed default slot、computed/shallow ref 状态和 Promise-returning reset/validate 方法。
- `VForm` 已从窄表单桩提升为强类型 form 组件，覆盖 disabled/readonly/fast-fail、validate-on、nullable model/update、CssClass/CssStyle、`submit` emit、typed default slot，以及 validate/reset/resetValidation authoring 回调；`submit` payload 保留 native `SubmitEvent` 能力并实现 `IPromise<VuetifyFormValidationResult>`。
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
- `VSnackbar` 已从基础消息条组件提升为 overlay-backed snackbar 组件，覆盖 `modelValue`、overlay target/activator、location/origin/offset/transition/dimensions、content/activator props、variant/text/timer/timeout/rounded/tile/multiLine/vertical props、activator/text/actions slots，以及 `afterEnter` / `afterLeave` / `click:outside` / `keydown` emits。
- `VImg` 已从窄图片组件提升为强类型 image 组件，覆盖 `src` 的 string/object union、`alt`/`lazySrc`/`srcset`/`sizes`、dimensions/aspect/transition/display props、CssClass/CssStyle/contentClass、color/gradient/intersection options、position、draggable/crossorigin/referrerpolicy 字符串域、load/loadstart/error emits，以及 default/placeholder/error/sources slots。
- `VAvatar` / `VBadge` 已从窄媒体状态组件提升为强类型主路径组件：`VAvatar.Icon` 与 `VBadge.Icon` 均使用 `VuetifyIconValue`，覆盖 theme/tag/rounded/tile/CssClass/CssStyle 等 Vuetify composable props；`VBadge` 同时覆盖 transition/location/model/offset/textColor 和 `badge` 命名槽。
- `VSheet` / `VIcon` 已对齐 Vuetify 3.8.0 display/media 基础组件主路径：`VSheet` 覆盖 theme/tag/rounded/tile/position/location/dimensions/CssClass/CssStyle/border/color/default slot，`VIcon` 覆盖 theme/tag/size/CssClass/CssStyle/color/disabled/start/end/icon/opacity/default slot。
- `VToolbar` 已从窄 toolbar 组件提升为强类型 toolbar 主路径组件，覆盖 theme/tag/rounded/tile/elevation/CssClass/CssStyle/border、absolute/collapse/extended/floating、height/extensionHeight/image/title、专用 density union，以及 image/prepend/append/title/extension/default slots。
- `VContainer` / `VRow` / `VCol` / `VSpacer` 已对齐 Vuetify 3.8.0 grid family 主路径：container 覆盖 tag/dimensions/CssClass/CssStyle/fluid，row 覆盖 align/justify/alignContent 及 breakpoint variants、dense/noGutters，col 覆盖 bool/number/string span union、order/offset breakpoint variants 和 alignSelf，spacer 覆盖 tag/CssClass/CssStyle/default slot。
- `VProgressCircular` / `VProgressLinear` 已从窄进度组件提升为强类型进度主路径组件，覆盖 theme/tag/CssClass/CssStyle、size/dimension、linear location/absolute/active/buffer/clickable/reverse/roundedBar、`update:modelValue` numeric emit，以及官方 typed default slot。
- `VSnackbarQueue` 已从透传桩提升为强类型 snackbar-queue 组件，覆盖 `string | SnackbarMessage` 队列、消息 option object、snackbar 外观/位置/timer/closable props、default/text/actions scoped slots 和 `update:modelValue`。
- `VSparkline` 已从透传桩提升为强类型 sparkline 组件，覆盖 trend/bar 类型、auto-draw、gradient、labels/modelValue、`string | number | { value }` item union、smooth union 和 label scoped slot。
- `VVirtualScroll` 已从透传桩提升为强类型 virtual-scroll 组件，覆盖 dimensions、`items`、`itemHeight`、`itemKey`、`renderless` 和 typed default slot context。
- `VInfiniteScroll` 已从透传桩提升为强类型 infinite-scroll 组件，覆盖 direction、side、mode、margin、load texts、`load` payload、loading/error/empty/load-more scoped slots 和 default slot。
- `VDefaultsProvider` 已从透传桩提升为强类型 defaults-provider 组件，覆盖 defaults object、disabled、reset、root、scoped 和默认 slot。
- `VDatePicker` 已从透传桩提升为强类型 date-picker 组件，覆盖 `modelValue`、multiple/range、min/max、year/month/viewMode update、active/allowedDates、weekdays/weeksInMonth、picker display props、`HeaderText` 映射到 Vuetify `header` prop，以及 `HeaderContent` / `TitleContent` / `Actions` slots。
- `VDialog` 已从早期 scoped-slot 示例提升为 overlay-backed dialog 组件，覆盖 Vuetify overlay props/events、dialog props、`target`/`activator` 专用 union、`Activator` scoped slot 和 `keydown` / `click:outside` / transition lifecycle emits。
- `VOverlay` 覆盖 `keydown` emit，和 `click:outside`、after-enter/leave、activator/default scoped slots 一起形成 overlay 事件主路径。
- `VBottomNavigation` 对齐 Vuetify 3.8.0：`modelValue` 表示 group selection，`active` 表示可见状态；旧的 `selectedValue` / `activeColor` / `shift` authoring 面不再作为当前合同。
- `VChip` 已从窄反馈标签组件提升为强类型 chip 组件，覆盖 `modelValue`、`click:close`、`group:selected`、selection value、variant/theme/tag/router/rounded/tile/elevation/density/CssClass/CssStyle/border、active/selected class、avatar/icon/base/close/filter/label/link/pill/ripple/text props，以及普通 `ChildContent` 和官方 scoped `DefaultContent`。`DefaultContent` 与 `ChildContent` 都映射到 Vue `default`，重复赋值在 BuildRenderTree pipeline 和 SFC/canonical lowering 中都会报错。
- `VDateInput`、`VFileUpload`、`VIconBtn`、`VPicker`、`VPullToRefresh`、`VCalendar`、`VTimePicker`、`VTreeview`、`VStepperVertical` 是 Vuetify 3.8.0 labs exports，authoring 桩明确导入 `vuetify/labs/components`；9 个 labs 组件均具备专用强类型 props/events/slots，但 labs API 仍按 Vuetify 版本变化跟进；`VHotkey` 不存在于本地 Vuetify 3.8.0，已从 runtime 与 authoring surface 移除。
- Vuetify 官方 slot 中的 `ref` / `computed` / `writable computed` 不会被抹平成 `bool` 或 `string`；C# slot 上下文以 `IVueRef<T>`、`VueComputedRef<T>`、`VueWritableComputedRef<T>` 表达，业务代码读取或写入时使用 `.Value`，例如 `ctx.IsValid.Value`、`ctx.Model.Value`、`ctx.IsFocused.Value`。
- 长尾 Vuetify props 通过 `AdditionalAttributes` 透传，不作为强类型主 API 的替代。
- `VCardTitle` 对齐 Vuetify 3.8.0 `createSimpleFunctional("v-card-title")` 合同：不暴露 `Text` prop，标题文本必须通过默认 `ChildContent` 输出。该规则已由 SFC 单元回归和真实浏览器 smoke 覆盖，避免生成不可见的 `<VCardTitle text="...">`。

## CSS 和自定义参数

使用方式：

- 强类型 props：直接使用组件属性，例如 `Variant`, `Density`, `Items`, `Headers`, `Location`, `Width`。
- CSS class/style：Razor 组件标签上的 lowercase `class` / `style` 通过 `AdditionalAttributes` 透传，保持官方 Razor Source Generator 可编译；已显式建模的组件在 C# authoring 中使用 `CssClass` / `CssStyle`，例如 `VTreeview.CssClass` 和 `VTreeview.CssStyle`，并映射到 Vue runtime 的 `class` / `style`。
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

- `RazorVue_Context_DiscoversVuetifyPackageLibraryDescriptors_FromReferencedAssembly`：验证 authoring 组件集合与 normal/labs runtime exports 的 114 个当前支持组件完全一致，且全部有 capture-unmatched `AdditionalAttributes`。
- `RazorVue_Registry_CreateFromCompilationContext_ResolvesVuetifyPackageComponents`：验证全部 114 个 authoring 组件可从 `ECMAScript.Vuetify` 命名空间解析，并区分 `vuetify/components` 与 `vuetify/labs/components` import specifier。
- `Vuetify_ComponentExports_MatchLocalVuetifyPackageEntrypoints` / `Vuetify_AuthoringComponents_UseMatchingPackageEntrypoints`：验证 C# runtime exports 是本地 Vuetify 3.8.0 d.ts 的真实导出，且 authoring 组件 import specifier 与 normal/labs 入口匹配。
- `RazorVue_Pipeline_LowersVuetifyApplicationShellComponents`：验证 `VApp` / `VNavigationDrawer` / `VAppBar` / `VMain` 可正确降级到 Vuetify import、props 和 update events。
- `RazorVue_Pipeline_LowersVuetifyPackageAdditionalAttributesAndExtendedProps`：验证 `VBtn`、`VTextField`、`VTextarea`、`VCheckbox`、`VSwitch` 的生产主路径 props、`update:focused`、field/details/counter/label/thumb 等官方 slot 和 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifyFabWithStrongProps`：验证 `VFab` 的 `modelValue`、layout/app/absolute、transition object、location/order、VBtn-like 外观/路由 props、default slot 和 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifySpeedDialSnackbarQueueAndSparklineWithStrongProps`：验证 `VSpeedDial` 的 overlay/menu props 与 activator/default slots、`VSnackbarQueue` 的消息 union/options、text/actions slots 和 model update、`VSparkline` 的 item/smooth union、gradient/type/label slot，以及三者的 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifyDefaultsVirtualAndInfiniteScrollWithStrongProps`：验证 `VDefaultsProvider` defaults object、`VVirtualScroll` items/itemKey/renderless/typed default slot、`VInfiniteScroll` load payload、side/mode、status slots、`load-more` 槽名和 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifyInputInfrastructureWithStrongProps`：验证 `VInput`、`VField`、`VSelectionControlGroup`、`VSelectionControl` 的强类型 props、nullable boolean、validation rules、icon-color、ripple、model update、loader/default/details/message/label/input slots 和 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifyGroupedAndDisplayComponentsWithStrongProps`：验证 `VBtnGroup`、`VBtnToggle`、`VCardItem`、`VFooter`、`VRating`、`VTable` 的强类型 props、分组 model update、rating item/item-label scoped slots 和样式/plugin 依赖输出。
- `RazorVue_Pipeline_LowersVuetifyNavigationSurfacesWithStrongProps`：验证 `VBanner`、`VBottomNavigation`、`VBottomSheet`、`VExpansionPanel`、`VEmptyState` 的强类型 props、`VBottomNavigation` 的 `modelValue` group selection 与 `active` 可见状态 update、bottom-sheet activator slot、expansion title scoped slot、empty-state action emit/named slots 和样式/plugin 依赖输出。
- `RazorVue_Pipeline_LowersVuetifyOverlayAndUtilityComponentsWithStrongProps`：验证 `VOverlay`、`VHover`、`VLazy`、`VResponsive`、`VItemGroup`、`VChipGroup`、`VSlideGroup` 的 overlay events（含 `keydown`）、activator/default scoped slots、intersection observer options、show-arrows union、slide-group arrows、value comparator 与 group model update 输出。
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
- `RazorVue_Pipeline_LowersVuetifyTreeviewWithStrongProps`：验证 labs `VTreeview` 的 model/activated/selected/opened values、items、active/select strategies、lazy load children、filter props、CssClass/CssStyle、icons、click payload emits、tree item scoped slots 和 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifyLabsInputUtilityComponentsWithStrongProps`：验证 labs `VDateInput`、`VFileUpload`、`VIconBtn`、`VPicker`、`VPullToRefresh` 的 model/update、events、slot context、dimension/visual props 和 `AdditionalAttributes` 合并输出。
- `RazorVue_Pipeline_LowersVuetifyStepperVerticalWithDynamicSlots`：验证 labs `VStepperVertical` 的 group model、items、actions/icon/title/subtitle/prev/next slots，以及 pattern-only dynamic `header-item.${string}` / `item.${string}` 槽名输出。
- `RazorVue_Pipeline_LowersPromotedVuetifyInputComponentsWithStrongContracts`：验证 combobox/file-input/number-input/otp/radio/slider/range-slider 的强类型 props、union 值、filter/search props、select-like slots 和 update events。
- `RazorVue_Pipeline_LowersVuetifySelectLikeComponentsWithStrongSelectedValueModel`：验证 `VSelect` / `VAutocomplete` / `VCombobox` 的强类型 `SelectedValue` 降级到 `modelValue`，并覆盖 object 与 multiple array model。
- `RazorVue_Pipeline_WithDuplicateVuetifyModelValueAuthoringAliases_ReportsUnknownParameter` / `RazorVue_Pipeline_WithDuplicateVuetifyModelUpdateAuthoringAliases_ReportsUnknownParameter`：验证 `ModelValue` 与 `SelectedValue`、`ModelValueChanged` 与 `SelectedValueChanged` 不会重复映射到同一个 Vue 目标。
- `RazorVue_SfcArtifactFactory_WithDuplicateLibraryMappedModelProp_ThrowsUnknownParameter` / `RazorVue_SfcArtifactFactory_WithDuplicateLibraryMappedModelUpdateEmit_ThrowsUnknownParameter` / `RazorVue_SfcArtifactFactory_WithInvalidLibraryBindTarget_ThrowsInvalidBindTarget`：验证 SFC/canonical 输出路径与 BuildRenderTree 输出路径使用一致的库组件 authoring 校验，不会在 SFC 模式静默生成重复 `modelValue` / `update:modelValue` 或错误双向绑定。
- `RazorVue_SfcArtifactFactory_LowersExplicitLibraryFallthroughAttributes_WhenTargetHasCaptureUnmatchedValues` / `RazorVue_Pipeline_LowersExplicitLibraryFallthroughAttributes_WhenTargetHasCaptureUnmatchedValues`：验证显式 `class`、`style`、`data-*`、`aria-*`、kebab-case 和 lower-camel raw attrs 可通过库组件 `AdditionalAttributes` sink 透传，且 PascalCase 未知参数仍失败。
- `Vuetify_AuthoringComponents_ExposeExplicitCssPropsAsCssClassAndCssStyle`：验证 Vuetify top-level authoring 组件不暴露 `[Parameter] Class` / `[Parameter] Style`，并要求 `CssClass` / `CssStyle` 映射到 Vue runtime `class` / `style`，避免 lowercase raw attribute 再被 Razor SG 绑定到非字符串组件参数。
- `RazorVue_SfcArtifactFactory_WithImplicitLibraryDefaultSlotOnComponentWithoutChildContent_ThrowsUnknownSlot` / `RazorVue_SfcArtifactFactory_WithUnknownRenderFragmentLibraryAttribute_ThrowsUnknownSlot` / `RazorVue_SfcArtifactFactory_WithUnknownLibrarySlotTemplate_ThrowsUnknownSlot` / `RazorVue_SfcArtifactFactory_WithImplicitTypedLibraryDefaultSlot_EmitsParameterizedDefaultTemplate` / `RazorVue_SfcArtifactFactory_WithNonCallableScopedSlotAttribute_ThrowsSlotContextMisuse` / `RazorVue_SfcArtifactFactory_WithDuplicateLibraryDefaultSlotAssignment_ThrowsDuplicateSlotValue` / `RazorVue_SfcArtifactFactory_WithImplicitAndExplicitLibraryDefaultSlotAssignment_ThrowsDuplicateSlotValue` / `RazorVue_SfcArtifactFactory_WithDuplicateLibraryNamedSlotAssignment_ThrowsDuplicateSlotValue`：验证 SFC/canonical 输出路径不会绕过库组件 slot authoring 合同，未声明 slot 继续失败，typed default slot 的普通隐式 child content 会生成带内部参数的 `#default` 模板且避免遮蔽同名业务局部，显式非 callable scoped slot 和重复 slot 赋值仍会失败。
- `RazorVue_Pipeline_LowersVuetifyCollectionComponentsWithStrongItemContracts` 同时验证 RazorVue h() 输出会正确引用包含 `.` / `-` 的 Vuetify slot 名；slot object key 会输出为字符串 key，避免生成无效 JavaScript。
- `RazorVue_SfcArtifactFactory_LowersLibrarySlotNamesWithDots_ToVueSlotTemplates`：验证 SFC 输出路径对 `header.data-table-select` / `footer.prepend` 等带点号 Vuetify slot 使用 Vue 动态 slot 参数形式，避免普通 `#name.modifier` 语法误解。
- `RazorVue_Pipeline_LowersVuetifyFeedbackAndListComposition` / `RazorVue_Pipeline_LowersVuetifyNavigationAndFeedbackComposition`：验证 alert/chip/snackbar/avatar/badge/progress 等反馈/状态组件的 `Boolean | String`、`Boolean | Number | String`、`Number | String` prop 降级输出；其中 `VAlert` 覆盖 `modelValue`、`click:close`、`border`、`icon: false`、CssClass/CssStyle 和 prepend/title/text/append/close slots，`VChip` 覆盖 model/close/group emits、visual/router/selection props、default scoped slot 和 label/prepend/append/close/filter slots，`VSnackbar` 覆盖 overlay props/events、target/activator unions、activator/text/actions slots 和 snackbar display props，`VAvatar` / `VBadge` 覆盖 icon/CssClass/CssStyle/rounded/transition/location/model 和 badge slot 输出，`VProgressCircular` / `VProgressLinear` 覆盖 progress slot context、linear model update、buffer/display/position props。
- `RazorVue_Pipeline_LowersVuetifyImgProductionSurface`：验证 `VImg` 的 object/string source、image attrs、dimension/display props、string enum literal 输出、load/error emits、default/placeholder/error/sources slots、样式/plugin 依赖输出。
- `RazorVue_Pipeline_LowersVuetifyCardPromotedPropsAndNamedSlots`：验证 `VCard` 的 title/subtitle/text、视觉/尺寸/路由 props 与 `title` / `text` / `actions` 等命名槽输出。
- `RazorVue_Pipeline_LowersVuetifyLayoutComposition`：验证 `VContainer` / `VRow` / `VCol` / `VSpacer` 的 grid family import、container dimensions、CssClass/CssStyle、row alignment breakpoint props、col span/order/offset union props 和 nested default slot composition 输出。
- `RazorVue_SfcArtifactFactory_LowersVuetifyCardTitle_DefaultSlot`：验证 `VCardTitle` 标题文本走默认 slot，且不会生成无效 `text` prop。
- `RazorVue_Pipeline_LowersVuetifyFeedbackAndListComposition`：同时验证 `VList` 的 items/item-title/item-value/item-children/item-props/lines/slim/bgColor/variant，以及 `VListItem` 的 icon/active/link/ripple/typed append slot 输出。
- `RazorVue_Pipeline_LowersVuetifyTooltipStrongTypedLocationAndAdditionalAttributes` / `RazorVue_Pipeline_LowersVuetifyNavigationAndFeedbackComposition`：验证 `VTooltip` / `VMenu` 的 model update、typed activator slot、location/origin/offset/delay/dimension/transition、`activatorProps` / `contentProps` 等 overlay authoring 面。
- `RazorVue_Pipeline_LowersVuetifyOverlayAndUtilityComponentsWithStrongProps` / `RazorVue_Pipeline_LowersVuetifyAdvancedForm` / `RazorVue_Pipeline_LowersVuetifyDialogActivatorScopedSlot`：共同验证 `VDialog` 的 overlay-backed props、target/activator union、official activator slot context、`click:outside` 和 `keydown` emits；`ActivatorTarget` prop 与 `Activator` slot 同时映射 Vue `activator` 时按 Vue prop/slot 独立命名空间处理，不再作为重复参数误报。
- `Vuetify_AuthoringComponents_ExposeOnlyAdditionalAttributesAsObjectSink`：验证 authoring 组件公开属性除 `AdditionalAttributes` 外不使用 `object`。
- `Vuetify_ValueAndUnionTypes_ExposeStronglyTypedContracts` / `Vuetify_ImgSourceObject_MatchesVuetifySourceContract` / `Vuetify_AvatarAndBadge_MatchVuetifySourceContracts` / `Vuetify_ProgressComponents_MatchVuetifySourceContracts`：验证核心 union/value 类型公开合同，守护 `VImgSourceObject.Aspect` 与 Vuetify `srcObject.aspect: number` 保持非 nullable 对齐，并守护 avatar/badge/progress 的 icon、border、transition、location、model update payload、scoped slot context 等关键类型不退化。
- `Vuetify_InputInfrastructureSlotContexts_PreserveOfficialRefContracts`：验证 input/field/selection-control/switch slot context 保留 Vuetify 官方 `Ref`、`ComputedRef`、`WritableComputedRef` 合同，不退化为标量值。
- `Build_LocalPackages_RazorVueTodoListSample_PureDenoPipeline_PassesInIsolatedWorkspace` / `Build_LocalPackages_WithExternalRazorSgSfcConsumer_PureDenoPipeline_PassesInIsolatedWorkspace`：验证本地 NuGet 包消费、Razor SG SFC 输出、纯 Deno SFC 预编译、`deno bundle`、`Deno.bundle()`、SSR smoke 和真实浏览器 mount smoke。浏览器 smoke 要求无 console warning/error、无网络失败、加载生成 CSS/JS、存在 Vuetify `.v-application` root，并覆盖 TodoList 核心交互或外部 consumer 的可见文本。

## 剩余风险

- 当前 114 个 authoring 组件均已有强类型主路径，但这仍不是 Vuetify 官方每个 prop/event/slot 的逐项完整镜像；低频实验 props 继续通过 `AdditionalAttributes` 或后续按业务提升。
- 本程序集当前显式支持 Vuetify 3.8.0 labs 入口的 9 个 runtime exports：`VCalendar`、`VDateInput`、`VFileUpload`、`VIconBtn`、`VPicker`、`VPullToRefresh`、`VStepperVertical`、`VTimePicker`、`VTreeview`。这些已纳入当前 production surface，但 labs API 本身仍是版本敏感区域，不承诺等同 Vuetify 稳定组件的全量 prop/event/slot 镜像。
- `VSelect` / `VAutocomplete` / `VCombobox` 当前以 `SelectedValue` 并行入口覆盖复杂 `modelValue`，没有把 `ModelValue` 破坏性改为泛型。后续如果 RazorVue 组件解析安全支持泛型库组件，可再评估把这组三组件升级为泛型 authoring API；升级前必须保持现有 `ModelValue string?` 兼容入口。
- 部分布局枚举类 props 为了保持 Razor 字面量易用性仍保留 `string?`，例如 `VRow.Align` / `VRow.Justify`。后续如要强约束，应设计不破坏 `<VRow Justify="center">` 的 authoring 迁移策略。
- 本清单确认的是代理层、编译降级和类型合同。最终业务上线仍需要目标应用跑完整 RazorVue/Jolt/emit 集成验证。
