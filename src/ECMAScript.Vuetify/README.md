# ECMAScript.Vuetify

> Status: active reference
> Positioning: Vuetify binding assembly and RazorVue authoring component stubs.

`ECMAScript.Vuetify` 是 RazorVue 线路中承载 Vuetify 绑定的独立程序集。原先独立的 `Jazor.RazorVue.Vuetify` 已并入这里。

## Responsibilities

- 提供 `Vuetify` / `VuetifyPlugin` / `VuetifyOptions` 等运行时投影类型。
- 提供 `VBtn`、`VCard`、`VDialog`、`VDataTable` 等 RazorVue authoring 组件桩。
- 通过 `VueLibrary*` 特性把库组件元数据暴露给 RazorVue 描述符与生成器。

## Boundaries

- 组件 authoring 元数据特性与 `IVueLibraryComponent` 来自 `Jazor.RazorVue` 命名空间，其物理实现位于 `Jazor.Common`。
- `ECMAScript.Vuetify` 不承载 RazorVue 生成器或分析逻辑；那些逻辑位于 `Jazor.Analyzer`。
- Deno / npm 导入地址通过 `ECMAScript` 特性声明，例如 `npm:vuetify`。

## Key Areas

- `VuetifyCore.cs`: Vuetify 运行时投影与插件选项。
- `V*.cs`: 组件桩定义。
- `VuetifyComponentRegistry.cs`, `VuetifyDirectiveRegistry.cs`: 组件/指令注册表。
- `VuetifyComponentExports.cs`, `VuetifyDirectiveExports.cs`: 导出聚合。

## Read Next

- [../Jazor.Common/README.md](../Jazor.Common/README.md)
- [../../docs/01-目标/razorvue/README.md](../../docs/01-目标/razorvue/README.md)
