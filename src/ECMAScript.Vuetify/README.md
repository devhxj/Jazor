# ECMAScript.Vuetify

> Status: active reference
> Positioning: Vuetify binding assembly and RazorVue authoring component stubs.

`ECMAScript.Vuetify` 是 RazorVue 线路中承载 Vuetify 绑定的独立程序集。原先独立的 `Jazor.RazorVue.Vuetify` 已并入这里。

## Responsibilities

- 提供 `Vuetify` / `VuetifyPlugin` / `VuetifyOptions` 等运行时投影类型。
- 提供覆盖当前支持的 `VuetifyComponents` normal exports 与 `VuetifyLabsComponents` labs exports 的 RazorVue authoring 组件桩。
- 为 `VAlert`、`VChip`、`VAvatar`、`VBadge`、`VIcon`、`VBtn`、`VBtnGroup`、`VBtnToggle`、`VBottomNavigation`、`VFab`、`VSpeedDial`、`VSnackbar`、`VStepper`、`VStepperVertical`、`VTextField`、`VTextarea`、`VInput`、`VField`、`VValidation`、`VForm`、`VConfirmEdit`、`VCheckbox`、`VSwitch`、`VSelectionControl`、`VSelectionControlGroup`、`VSelect`、`VAutocomplete`、`VCombobox`、`VFileInput`、`VNumberInput`、`VSlider`、`VDataTable`、`VDataIterator`、`VVirtualScroll`、`VInfiniteScroll`、`VSnackbarQueue`、`VSparkline`、`VColorPicker`、`VDatePicker`、`VDateInput`、`VCalendar`、`VTimePicker`、`VTreeview`、`VFileUpload`、`VIconBtn`、`VPicker`、`VPullToRefresh`、`VCard`、`VCardItem`、`VSheet`、`VToolbar`、`VContainer`、`VRow`、`VCol`、`VSpacer`、`VFooter`、`VRating`、`VTable`、`VList`、`VListItem`、`VDialog`、`VMenu`、`VTooltip`、`VOverlay`、`VHover`、`VLazy`、`VResponsive`、`VItemGroup`、`VChipGroup`、`VSlideGroup`、`VCarousel`、`VEmptyState`、`VSkeletonLoader`、`VWindow`、`VTabsWindow`、`VTabsWindowItem`、`VLayout`、`VSystemBar`、`VThemeProvider`、`VDefaultsProvider`、`VLabel`、`VKbd`、`VCounter`、`VMessages`、`VNoSsr`、`VImg`、`VProgressLinear`、`VProgressCircular`、`VToolbarItems`、`VParallax`、`VCode`、`VTimeline`、`VLocaleProvider` 等组件提供专用强类型 props。
- 所有 authoring 组件都保留 `AdditionalAttributes`，作为 raw `class` / `style`、`data-*` / `aria-*` 和尚未强类型建模 Vuetify props 的生产逃生口；RazorVue 也允许显式书写 `class`、`style`、`data-*`、`aria-*`、Vue directive-like attrs、kebab-case raw attrs 和 lower-camel raw Vuetify props。需要强类型 C# 表达式时，显式建模组件使用 `CssClass` / `CssStyle`，并映射到 Vue runtime 的 `class` / `style`。
- Vuetify `Boolean | String`、`Boolean | Number | String`、`Number | String` 等 prop 使用显式命名 union 或 `VueStringNumberValue`，不使用通用二选一封装或弱 `object` 主 API。
- 分支类型两两不可赋值的多值 prop 使用 C# 原生 `union`，并保留 `AsX` 投影和必要的标量转换；只有必须区分可赋值重叠分支时才保留 tagged fallback。
- 使用 `VueLibraryComponent` 提供 C# 无法表达的 npm import/export 身份；普通 prop、event 和 slot 优先由 C# / Razor 约定推导，仅异常 Vue 名称保留显式映射。

## Production Snapshot

- Runtime component exports: 114 total; 105 from `vuetify/components`, 9 labs from `vuetify/labs/components`.
- Strong RazorVue authoring components: 114.
- Runtime-only pass-through authoring components: 0.
- Public object sink policy: only `AdditionalAttributes : IReadOnlyDictionary<string, object?>?` may accept unmatched values.
- Vuetify CSS import and `createVuetify()` installation are application bootstrap responsibilities; component attributes do not model or inject them.
- Scoped slot ref policy: Vuetify ref/computed ref slot fields are modeled as `IVueRef<T>`, `VueComputedRef<T>`, or `VueWritableComputedRef<T>` and should be read or written through `.Value` in authoring code, for example `ctx.IsValid.Value` or `ctx.Model.Value`.

## Boundaries

- 必要的组件 import 和异常 Vue 名称元数据由 `ECMAScript.VueContract` 项目提供。退役中的 marker、kind 和无 consumer 元数据不得成为新组件库的设计模板。
- `ECMAScript.Vuetify` 不承载 RazorVue 生成器或分析逻辑；那些逻辑位于 `Jazor.Analyzer`。
- Deno / npm 导入地址通过 `ECMAScript` 特性声明，例如 `npm:vuetify`。

## Key Areas

- `VuetifyCore.cs`: Vuetify 运行时投影与插件选项。
- `V*.cs`: 高频组件的专用强类型 authoring 桩定义。
- `VuetifyItemTypes.cs`: select/autocomplete/combobox、breadcrumb、data-table 等集合 item 与 model value 合同。
- `VuetifyGroupTypes.cs`: button toggle、item group、rating 等分组/选择值合同。
- `VInputComponentBase.cs`: `VTextField` / `VTextarea` 共享的 field/input authoring props 和 slots。
- `VAlert.cs`, `VAlertSlotContexts.cs`: alert feedback 组件的 model、close emit、border/icon union、theme/layout/size/CssClass/CssStyle props 和 prepend/title/text/append/close slots。
- `VChip.cs`, `VChipSlotContexts.cs`: chip feedback/list/filter 组件的 model、close/group emits、visual/router/selection props、普通 default 内容、官方 scoped default slot 和 label/prepend/append/close/filter slots。
- `VAvatar.cs`, `VBadge.cs`: avatar/badge 媒体状态组件的 Vuetify composable props、icon/border/transition/location 强类型合同、badge/default slots 和 model update。
- `VSheet.cs`, `VIcon.cs`: sheet/icon 基础展示组件的 theme/tag/dimension、CssClass/CssStyle、position/location/border 和 icon/opacity/start/end 强类型合同。
- `VToolbar.cs`, `VToolbarItems.cs`, `VToolbarTitle.cs`, `VToolbarTypes.cs`: toolbar family 的 Vuetify sheet/toolbar composable props、专用 density union、image/prepend/append/title/extension/default slots，以及 toolbar-title text slot。
- `VContainer.cs`, `VRow.cs`, `VCol.cs`, `VSpacer.cs`, `VGridTypes.cs`: grid family 的 container dimensions、row alignment breakpoint props、col `bool | number | string` span union、order/offset props、spacer tag/CssClass/CssStyle 和默认 slot 合同。
- `VProgressCircular.cs`, `VProgressLinear.cs`, `VProgressSlotContexts.cs`: progress 组件的 theme/tag/CssClass/CssStyle、linear buffer/display/model update props 和官方 default scoped slot context。
- `VSnackbar.cs`, `VSnackbarSlotContexts.cs`: snackbar 组件的 overlay-backed props/events、target/activator unions、content/activator props、text/actions/activator slots 和反馈展示 props。
- `VImg.cs`, `VImgTypes.cs`: image 组件的 `string | srcObject` source、image attrs、dimension/display props、load/error events、default/placeholder/error/sources slots、draggable/crossorigin/referrerpolicy 字符串域合同。
- `VInput.cs`, `VField.cs`: Vuetify input infrastructure 组件的直接 authoring 代理，覆盖 validation、messages、field chrome 和 loader slots。
- `VValidation.cs`, `VValidationSlotContexts.cs`: validation composable 的直接 authoring 代理，覆盖 `focused`、nullable disabled/readonly、rules、model/validation value、typed default slot 和 Promise-returning validation methods。
- `VForm.cs`, `VFormSlotContexts.cs`: form 组件的 nullable model、validate-on、submit validation promise、CssClass/CssStyle、typed default slot 和 validate/reset callbacks。
- `VConfirmEdit.cs`, `VConfirmEditTypes.cs`: confirm-edit 的 model/update、save/cancel emit、action disabled union、default slot model/actions context 和 `AdditionalAttributes` 合同。
- `VSelectionControlComponentBase.cs`: `VCheckbox` / `VSwitch` 共享的 selection-control authoring props。
- `VSelectionControl.cs`, `VSelectionControlGroup.cs`: Vuetify selection-control infrastructure 组件的直接 authoring 代理，覆盖 group model、nullable boolean、ripple、label/input slots。
- `VFieldSlotContexts.cs`: text-field/textarea 的 field、details、counter scoped slot 上下文合同。
- `VSelectionControlSlotContexts.cs`: checkbox/switch/selection-control label、input、thumb、track scoped slot 上下文合同，以及 `VuetifyCssProperties` slot style bag。
- `VSelectLikeComponentBase.cs`: `VSelect` / `VAutocomplete` / `VCombobox` 共享的 field、menu、filter、item 与 validation authoring props。
- `VSelectSlotContexts.cs`: `VSelect` / `VAutocomplete` / `VCombobox` 的 `item`、`chip`、`selection` scoped slot 上下文合同。
- `VRatingSlotContexts.cs`: `VRating` 的 `item`、`item-label` scoped slot 上下文合同。
- `VDataTableSlotContexts.cs`: `VDataTable` 分页、排序、选择、展开、表头/行/页脚等高频命名槽上下文合同。
- `VVirtualScroll.cs`, `VVirtualScrollSlotContexts.cs`: virtual-scroll 尺寸、items、item key、renderless 和 typed default slot 合同。
- `VInfiniteScroll.cs`, `VInfiniteScrollTypes.cs`: infinite-scroll side/mode/status、load event payload、status scoped slots 和 load-more slot 合同。
- `VDateInput.cs`, `VDateInputTypes.cs`: labs date-input 的 text-field 主表面、confirm edit actions、display format、save/cancel emits 和 actions scoped slot 合同。
- `VFileUpload.cs`, `VFileUploadTypes.cs`: labs file-upload 的 file model、browse/input/item slots、visual/drop-zone props 和 upload item context 合同。
- `VIconBtn.cs`, `VIconBtnTypes.cs`: labs icon-btn 的 active model、icon/size maps、variant state、loader/default slots 和 button-like visual props。
- `VPicker.cs`: labs picker shell 的 theme/layout/dimension/header/actions/title slots 合同。
- `VPullToRefresh.cs`, `VPullToRefreshTypes.cs`: labs pull-to-refresh load payload、threshold、default slot 和 pullDownPanel scoped slot 合同。
- `VListItemSlotContext.cs`: `VListItem` 常用命名槽上下文合同。
- `VOverlay.cs`, `VDialog.cs`, `VuetifyOverlayTypes.cs`, `VOverlaySlotContexts.cs`, `VDialogActivatorContext.cs`: overlay/dialog target/activator unions、keydown/click-outside events、overlay props 和 activator scoped slot 合同。
- `VHoverSlotContexts.cs`, `VItemGroupSlotContexts.cs`: display/group utility scoped slot 上下文合同。
- `VSlideGroup.cs`, `VSlideGroupSlotContexts.cs`: slide-group model、mobile/display、arrow controls 和 default/prev/next scoped slot 合同。
- `VCarousel.cs`, `VCarouselSlotContexts.cs`: carousel model、cycle/interval/progress/delimiters、window navigation props、prev/next/item slots 和 `verticalDelimiters` union 合同。
- `VStepper.cs`, `VStepperTypes.cs`: stepper group model、items union/collection、stepper/group/sheet/display/actions props、default/actions/header/item/prev/next scoped slots 和 `AdditionalAttributes` 合同。
- `VStepperVertical.cs`, `VStepperVerticalTypes.cs`: labs vertical stepper group model、items union/collection、expansion-panel/stepper props、actions/icon/title/subtitle/prev/next slots 和 `AdditionalAttributes` 合同；keyed dynamic item slot 需在 C# API 与 lowering 同时具备后再公开。
- `VEmptyState.cs`, `VEmptyStateSlotContexts.cs`: empty-state media/text/action props、click:action 和 actions scoped slot 合同。
- `VSkeletonLoader.cs`, `VuetifySkeletonLoaderTypes.cs`: skeleton-loader 尺寸、loading、boilerplate、官方 root type/custom type 和 type array 合同。
- `VTimeline.cs`, `VuetifyTimelineTypes.cs`: timeline alignment、direction、justify、side、line display 和默认 slot 合同。
- `VLocaleProvider.cs`: locale、fallback locale、RTL、messages object 和默认 slot 合同。
- `VDefaultsProvider.cs`: scoped component defaults object、reset/root/scoped/disabled 和默认 slot 合同。
- `VFab.cs`: floating action button 的 model、layout/position、transition 和 VBtn-like 外观/路由 props 合同。
- `VSpeedDial.cs`, `VSpeedDialSlotContexts.cs`: speed-dial 的 overlay/menu props、model update、activator/default scoped slots 和 `AdditionalAttributes` 合同。
- `VSnackbarQueue.cs`, `VSnackbarQueueTypes.cs`: snackbar-queue 消息数组、消息 option object、default/text/actions scoped slots 和 action props 合同。
- `VSparkline.cs`, `VSparklineTypes.cs`: sparkline trend/bar props、`string | number | { value }` item union、smooth union、label scoped slot 合同。
- `VDatePicker.cs`, `VDatePickerTypes.cs`: date-picker model、multiple/range、calendar navigation、allowed-dates、picker display props、`HeaderText` prop 和 header/title/actions slots 合同。
- `VCalendar.cs`, `VCalendarTypes.cs`: labs calendar date model、allowed dates、events、interval options、next/prev emits 和 header/event scoped slots 合同。
- `VTimePicker.cs`, `VTimePickerTypes.cs`: labs time-picker model、allowed hours/minutes/seconds、format/view-mode/period enums、hour/minute/second update events、picker display props 和 default/title/actions slots 合同。
- `VTreeview.cs`, `VTreeviewTypes.cs`: labs treeview item/model/opened/activated/selected values、active/select strategies、lazy load children、filter/CssClass/CssStyle props、click payload events 和 tree item scoped slots 合同。
- `VCounterSlotContexts.cs`, `VMessagesSlotContexts.cs`: counter/message scoped slot 上下文合同。
- `VWindow.cs`, `VTabsWindow.cs`, `VTabsWindowItem.cs`: window/tabs panel 组件的 group model、navigation、touch handlers、transition 和 group:selected 合同。
- `VWindowSlotContexts.cs`: window default/additional/prev/next scoped slot、touch directive 和 group provide 合同。
- `VuetifyObserverTypes.cs`: `VLazy` intersection observer options 合同。
- `VuetifyComponentRegistry.cs`, `VuetifyLabsComponentRegistry`, `VuetifyDirectiveRegistry.cs`: normal/labs 组件与指令注册表。
- `VuetifyComponentExports.cs`, `VuetifyDirectiveExports.cs`: normal/labs 导出聚合。

## Read Next

- [../../docs/03-完成/razorvue/ECMAScript.Vuetify.ProductionChecklist.md](../../docs/03-%E5%AE%8C%E6%88%90/razorvue/ECMAScript.Vuetify.ProductionChecklist.md)
- [../Jazor.Common/README.md](../Jazor.Common/README.md)
- [../../docs/01-目标/razorvue/README.md](../../docs/01-目标/razorvue/README.md)
