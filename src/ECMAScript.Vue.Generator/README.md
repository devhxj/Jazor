# ECMAScript.Vue.Generator

> 定位：Element Plus、Vuetify 和 TDesign binding 的维护期生成器，不参与应用构建或运行时。

该项目维护锁定的上游输入，并生成或校验 binding catalog。各 binding 包只保留其 authoring contract、生成的 C#、`manifest.json`、`dist/` 和 `licenses/`，不会在应用构建时引用本项目。

## 运行

在仓库根目录执行：

```bash
dotnet run --project src/ECMAScript.Vue.Generator -- elementplus
dotnet run --project src/ECMAScript.Vue.Generator -- elementplus --check
dotnet run --project src/ECMAScript.Vue.Generator -- vuetify
dotnet run --project src/ECMAScript.Vue.Generator -- vuetify --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign snapshot --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign bindings --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components --report
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components --check
```

## 输入与边界

- `upstream/element-plus/2.14.4` 只冻结 Element Plus 生成实际需要的上游文件。
- `upstream/tdesign-vue-next/1.20.5` 保存可复现 TDesign contract 所需的声明快照与外部类型输入。
- Vuetify catalog 由当前 `[ECMAScript(import, Transform.Component, exportName)]` 声明经 Roslyn 生成 `VuetifyCatalog.g.cs`；它不是完整的上游类型镜像。
- Vuetify 的 `V*.cs`、`VuetifyCatalog.g.cs`、`manifest.json` 与 `dist/` 是生成器受控产物；修改组件契约时应先修改 `VuetifyCatalogGenerator` 或锁定的 upstream 输入，再运行 `vuetify` 并用 `vuetify --check` 校验，禁止把生成文件作为独立手工源码维护。
- 生成器不得用 `object`、`VueValue` 或占位类型伪造组件覆盖率。

## 相关文档

- [ECMAScript.ElementPlus](../ECMAScript.ElementPlus/README.md)
- [ECMAScript.TDesign](../ECMAScript.TDesign/README.md)
- [ECMAScript.Vuetify](../ECMAScript.Vuetify/README.md)
