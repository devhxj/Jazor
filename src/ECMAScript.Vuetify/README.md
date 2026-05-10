# ECMAScript.Vuetify

> Status: active reference
> Positioning: Vuetify binding assembly and RazorVue authoring component stubs.

`ECMAScript.Vuetify` 是 RazorVue 线路中承载 Vuetify 绑定的独立程序集。原先独立的 `Jazor.RazorVue.Vuetify` 已并入这里。

## Responsibilities

- 提供 `Vuetify` / `VuetifyPlugin` / `VuetifyOptions` 等运行时投影类型。
- 提供覆盖全部 `VuetifyComponents` runtime exports 的 RazorVue authoring 组件桩。
- 为 `VBtn`、`VTextField`、`VTextarea`、`VCheckbox`、`VSwitch`、`VSelect`、`VAutocomplete`、`VCombobox`、`VFileInput`、`VNumberInput`、`VSlider`、`VDataTable`、`VCard`、`VList`、`VListItem`、`VMenu`、`VTooltip`、`VImg`、`VProgressLinear` 等高频组件提供专用强类型 props；其余组件先提供 `AdditionalAttributes` 透传桩。
- 所有 authoring 组件都保留 `AdditionalAttributes`，作为 class/style、`data-*` / `aria-*` 和尚未强类型建模 Vuetify props 的生产逃生口；RazorVue 也允许显式书写 `class`、`style`、`data-*`、`aria-*`、Vue directive-like attrs、kebab-case raw attrs 和 lower-camel raw Vuetify props。
- Vuetify `Boolean | String`、`Boolean | Number | String`、`Number | String` 等 prop 使用显式命名 union 或 `VueStringNumberValue`，不使用 `Either` 或弱 `object` 主 API。
- 通过 `VueLibrary*` 特性把库组件元数据暴露给 RazorVue 描述符与生成器。

## Boundaries

- 组件 authoring 元数据特性、`IVueLibraryComponent` 以及相关 authoring 枚举由 `ECMAScript.VueContract` 项目提供；实际代码命名空间统一为 `ECMAScript.VueContract` / `ECMAScript.VueContract.Descriptor`。
- `ECMAScript.Vuetify` 不承载 RazorVue 生成器或分析逻辑；那些逻辑位于 `Jazor.Analyzer`。
- Deno / npm 导入地址通过 `ECMAScript` 特性声明，例如 `npm:vuetify`。

## Key Areas

- `VuetifyCore.cs`: Vuetify 运行时投影与插件选项。
- `V*.cs`: 高频组件的专用强类型 authoring 桩定义。
- `VuetifyItemTypes.cs`: select/autocomplete/combobox、breadcrumb、data-table 等集合 item 与 model value 合同。
- `VListItemSlotContext.cs`: `VListItem` 常用命名槽上下文合同。
- `VOverlaySlotContexts.cs`: `VMenu` / `VTooltip` 等 overlay activator 槽上下文合同。
- `VuetifyRuntimeOnlyAuthoringComponents.cs`: 与 runtime exports 对齐的透传型 authoring 桩。
- `VuetifyComponentRegistry.cs`, `VuetifyDirectiveRegistry.cs`: 组件/指令注册表。
- `VuetifyComponentExports.cs`, `VuetifyDirectiveExports.cs`: 导出聚合。

## Read Next

- [../../docs/03-完成/razorvue/ECMAScript.Vuetify.ProductionChecklist.md](../../docs/03-%E5%AE%8C%E6%88%90/razorvue/ECMAScript.Vuetify.ProductionChecklist.md)
- [../Jazor.Common/README.md](../Jazor.Common/README.md)
- [../../docs/01-目标/razorvue/README.md](../../docs/01-目标/razorvue/README.md)
