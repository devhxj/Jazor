# ECMAScript.Vuetify

> Status: active reference
> Positioning: Vuetify binding assembly and RazorVue authoring component stubs.

`ECMAScript.Vuetify` 是 RazorVue 线路中承载 Vuetify 绑定的独立程序集。原先独立的 `Jazor.RazorVue.Vuetify` 已并入这里。

## Responsibilities

- 提供 `Vuetify` / `VuetifyPlugin` / `VuetifyOptions` 等运行时投影类型。
- 提供覆盖当前支持的 `VuetifyComponents` normal exports 与 `VuetifyLabsComponents` labs exports 的 RazorVue authoring 组件桩。
- 为 `VBtn`、`VBtnGroup`、`VBtnToggle`、`VFab`、`VSpeedDial`、`VStepper`、`VTextField`、`VTextarea`、`VInput`、`VField`、`VValidation`、`VConfirmEdit`、`VCheckbox`、`VSwitch`、`VSelectionControl`、`VSelectionControlGroup`、`VSelect`、`VAutocomplete`、`VCombobox`、`VFileInput`、`VNumberInput`、`VSlider`、`VDataTable`、`VDataIterator`、`VVirtualScroll`、`VInfiniteScroll`、`VSnackbarQueue`、`VSparkline`、`VColorPicker`、`VDatePicker`、`VCalendar`、`VTimePicker`、`VTreeview`、`VCard`、`VCardItem`、`VFooter`、`VRating`、`VTable`、`VList`、`VListItem`、`VMenu`、`VTooltip`、`VOverlay`、`VHover`、`VLazy`、`VResponsive`、`VItemGroup`、`VChipGroup`、`VSlideGroup`、`VCarousel`、`VEmptyState`、`VSkeletonLoader`、`VWindow`、`VTabsWindow`、`VTabsWindowItem`、`VLayout`、`VSystemBar`、`VThemeProvider`、`VDefaultsProvider`、`VLabel`、`VKbd`、`VCounter`、`VMessages`、`VNoSsr`、`VImg`、`VProgressLinear`、`VToolbarItems`、`VParallax`、`VCode`、`VTimeline`、`VLocaleProvider` 等组件提供专用强类型 props。
- 所有 authoring 组件都保留 `AdditionalAttributes`，作为 class/style、`data-*` / `aria-*` 和尚未强类型建模 Vuetify props 的生产逃生口；RazorVue 也允许显式书写 `class`、`style`、`data-*`、`aria-*`、Vue directive-like attrs、kebab-case raw attrs 和 lower-camel raw Vuetify props。
- Vuetify `Boolean | String`、`Boolean | Number | String`、`Number | String` 等 prop 使用显式命名 union 或 `VueStringNumberValue`，不使用通用二选一封装或弱 `object` 主 API。
- 通过 `VueLibrary*` 特性把库组件元数据暴露给 RazorVue 描述符与生成器。

## Production Snapshot

- Runtime component exports: 108 total; 105 from `vuetify/components`, 3 labs from `vuetify/labs/components`.
- Strong RazorVue authoring components: 108.
- Runtime-only pass-through authoring components: 0.
- Public object sink policy: only `AdditionalAttributes : IReadOnlyDictionary<string, object?>?` may accept unmatched values.
- Scoped slot ref policy: Vuetify ref/computed ref slot fields are modeled as `IVueRef<T>`, `VueComputedRef<T>`, or `VueWritableComputedRef<T>` and should be read or written through `.Value` in authoring code, for example `ctx.IsValid.Value` or `ctx.Model.Value`.

## Boundaries

- 组件 authoring 元数据特性、`IVueLibraryComponent` 以及相关 authoring 枚举由 `ECMAScript.VueContract` 项目提供；实际代码命名空间统一为 `ECMAScript.VueContract` / `ECMAScript.VueContract.Descriptor`。
- `ECMAScript.Vuetify` 不承载 RazorVue 生成器或分析逻辑；那些逻辑位于 `Jazor.Analyzer`。
- Deno / npm 导入地址通过 `ECMAScript` 特性声明，例如 `npm:vuetify`。

## Key Areas

- `VuetifyCore.cs`: Vuetify 运行时投影与插件选项。
- `V*.cs`: 高频组件的专用强类型 authoring 桩定义。
- `VuetifyItemTypes.cs`: select/autocomplete/combobox、breadcrumb、data-table 等集合 item 与 model value 合同。
- `VuetifyGroupTypes.cs`: button toggle、item group、rating 等分组/选择值合同。
- `VInputComponentBase.cs`: `VTextField` / `VTextarea` 共享的 field/input authoring props 和 slots。
- `VInput.cs`, `VField.cs`: Vuetify input infrastructure 组件的直接 authoring 代理，覆盖 validation、messages、field chrome 和 loader slots。
- `VValidation.cs`, `VValidationSlotContexts.cs`: validation composable 的直接 authoring 代理，覆盖 `focused`、nullable disabled/readonly、rules、model/validation value、typed default slot 和 Promise-returning validation methods。
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
- `VListItemSlotContext.cs`: `VListItem` 常用命名槽上下文合同。
- `VOverlaySlotContexts.cs`: `VMenu` / `VTooltip` 等 overlay activator 槽上下文合同。
- `VHoverSlotContexts.cs`, `VItemGroupSlotContexts.cs`: display/group utility scoped slot 上下文合同。
- `VSlideGroup.cs`, `VSlideGroupSlotContexts.cs`: slide-group model、mobile/display、arrow controls 和 default/prev/next scoped slot 合同。
- `VCarousel.cs`, `VCarouselSlotContexts.cs`: carousel model、cycle/interval/progress/delimiters、window navigation props、prev/next/item slots 和 `verticalDelimiters` union 合同。
- `VStepper.cs`, `VStepperTypes.cs`: stepper group model、items union/collection、stepper/group/sheet/display/actions props、default/actions/header/item/prev/next scoped slots 和 `AdditionalAttributes` 合同。
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
- `VTreeview.cs`, `VTreeviewTypes.cs`: labs treeview item/model/opened/activated/selected values、active/select strategies、lazy load children、filter/class/style props、click payload events 和 tree item scoped slots 合同。
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
