# ECMAScript.Vue.Generator

> 定位：Element Plus、Vuetify 和 TDesign binding 的维护期生成器，不参与应用构建或运行时。

该项目维护锁定的上游输入，并生成或校验 binding catalog。各 binding 包保留其 authoring
contract、生成的 C#、`manifest.json`、`inventory.json` 和 `licenses/` 元数据；运行时由
npm/JSR package 提供，只有明确声明的 embedded binding 才随包携带本地 carrier。应用构建
不会引用本项目。

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
dotnet run --project src/ECMAScript.Vue.Generator -- authoring
dotnet run --project src/ECMAScript.Vue.Generator -- authoring --check
```

## 输入与边界

`authoring` 扫描当前 `ECMAScript.*` C# 声明，维护数值/union 参数的 XML 提示、嵌套 union
的直接标量投影，以及 Number 分支缺少的强类型数值转换。转换输入来自 `ECMAScript.Number`
已有隐式转换；不引入 `object`、开放泛型或组件名名单。生成区域和 `data-authoring="types"`
说明由该命令维护；重复运行保留原文、换行和既有接口，`--check` 检测漂移。
Element Plus、Vuetify、TDesign 和 WebIDL 生成器共享相同处理，Lucide 生成脚本最后执行该维护步骤。
新增 numeric/union 绑定后运行这两个 authoring 命令即可；应用构建期间不扫描或重写源码。

- `upstream/element-plus/2.14.5` 只冻结 Element Plus 生成实际需要的上游文件。
- `element-plus-runtime/package.json` 与 `package-lock.json` 锁定 Element Plus 的校验输入；更新脚本通过 `npm ci` 验证上游 exports、样式和依赖元数据，不把绑定库自己的 dist 作为运行时 carrier。
- `upstream/tdesign-vue-next/1.20.7` 保存可复现 TDesign contract 所需的声明快照与外部类型输入。
- Vuetify catalog 由当前 `[ECMAScript(import, Transform.Component, exportName)]` 声明经 Roslyn 生成 `VuetifyCatalog.g.cs`；它不是完整的上游类型镜像。
- Vuetify 的 `V*.cs`、`VuetifyCatalog.g.cs` 与 `manifest.json` 是生成器受控产物；修改组件契约时应先修改 `VuetifyCatalogGenerator` 或锁定的 upstream 输入，再运行 `vuetify` 并用 `vuetify --check` 校验，禁止把生成文件作为独立手工源码维护。
- 生成器不得用 `object`、`VueValue` 或占位类型伪造组件覆盖率。

TDesign 文档由相同版本的上游源码 JSDoc 和本地枚举释义共同生成。刷新步骤与源码归档地址见
[TDesign 维护命令](../ECMAScript.TDesign/README.md#维护命令)。`documentation.json` 只提供说明，
类型签名仍来自 npm 声明快照；共享类型使用自身说明，继承成员沿类型别名和基类查找原始说明。
修改注释应编辑 `TDesignDocumentation.Text.cs` 或更新上游文档快照，再运行 `tdesign components`，
不要手改 `TBasic.g.cs`。新增字符串枚举取值必须同时补充中文释义，否则生成命令会报出缺失项。

## 相关文档

- [ECMAScript.ElementPlus](../ECMAScript.ElementPlus/README.md)
- [ECMAScript.TDesign](../ECMAScript.TDesign/README.md)
- [ECMAScript.Vuetify](../ECMAScript.Vuetify/README.md)
