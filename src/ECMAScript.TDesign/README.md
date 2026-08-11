# ECMAScript.TDesign

> 定位：TDesign Vue Next 的强类型 C# host binding 与 Razor-to-Vue authoring 接口。

发布包携带 `tdesign-vue-next` 1.20.5 的浏览器 ESM、CSS、许可证和 `manifest.json`。应用只需还原 NuGet 包；Jazor 会从本地包资源物化 TDesign 与 Vue 依赖，不要求 `node_modules`、CDN 或额外的 Node.js 安装。

## 维护输入

绑定输入固定在 `../ECMAScript.Vue.Generator/upstream/tdesign-vue-next/1.20.5`。`components.json`、`bindings.json` 和 `contracts.json` 分别记录可导出的组件、实际模块/export 与强类型 props 契约。没有当前 runtime export 的文档标签不是 binding 输入。

## 维护命令

以下命令只供包维护者更新锁定上游快照和验证生成结果；应用构建与发布不会执行它们：

```bash
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign snapshot
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign bindings
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign snapshot --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign bindings --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components --report
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components --check
```

`tdesign components` 是全覆盖门禁：只有每个已声明 props 都具备具体 C# 类型时才生成当前 118 个 runtime 组件。不能为了通过生成而使用 `object`、`VueValue` 或占位契约。

## 类型与产物边界

公开 authoring 类型使用 `T*` 命名，例如 `TMenuValue`、`TButtonThemeValue` 与 `TComponents`；根 host 保留 `TDesign`。字符串域使用 `[String]` enum，因此 `TButtonThemeValue.Primary` 会发射为 `"primary"`，不会变成数值序号。

Razor Source Generator 集成、render-function lowering 和产物物化分别属于 `Jazor.Vue`、`Jazor.RazorVue` 和 `Jazor.Emit`。本包只定义 host binding 与组件契约。

## 相关文档

- [ECMAScript.Vue.Generator](../ECMAScript.Vue.Generator/README.md)
- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
