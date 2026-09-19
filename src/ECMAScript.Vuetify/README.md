# ECMAScript.Vuetify

> 定位：`vuetify` 的强类型 C# binding 与 Razor-to-Vue authoring 组件契约。

`ECMAScript.Vuetify` 为当前已建模的 Vuetify runtime export、组件 props、events 和 slots 提供 C# 表达。它不拥有 Razor Source Generator、C# lowering 或产物物化。

本包属于 JS resource library：`manifest.json` 与 `inventory.json` 记录锁定的 Vuetify npm
package、组件入口、样式和依赖元数据；Emit 生成标准 `jazor/package.json`，Deno 将运行时
恢复到 `jazor/node_modules`，NetPack 按 Vuetify 的 `exports` 与 `sideEffects` 解析实际闭包。
C# 程序集只提供映射和组件 authoring contract。消费方编写的 RazorVue 组件生成到消费程序集
的 `Jazor.Generated.ModuleCatalog`。

## 安装

在 Razor-to-Vue 应用中，与核心包一起引用：

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="1.0.0-preview.3" />
  <PackageReference Include="Jazor.Vue" Version="1.0.0-preview.3" PrivateAssets="all" />
  <PackageReference Include="ECMAScript.Vuetify" Version="1.0.0-preview.3" />
</ItemGroup>
```

应用启动代码仍负责导入 Vuetify CSS、创建 `createVuetify()` 实例并安装到 Vue application。组件 binding 不会隐式注入这些 bootstrap 行为。

## 职责

- 提供 `Vuetify`、`VuetifyPlugin` 和 `VuetifyOptions` 等 runtime host。
- 提供当前支持的 normal 与 labs component authoring 类型，以及对应的 props、events、slots 和 value contracts。
- 使用命名 union、overload 和具体类型表达多值 props；不以 `object` 作为公共 API 的兜底类型。
- 通过 `AdditionalAttributes` 保留未建模 attrs 的显式透传入口，适用于 `class`、`style`、`data-*`、`aria-*` 与必要的原始 Vuetify 属性。

## 边界

- 组件导入路径和异常 Vue 命名元数据由 `ECMAScript.VueContract` 提供。
- scoped-slot 中的 ref/computed-ref 值使用 `IVueRef<T>`、`VueComputedRef<T>` 或 `VueWritableComputedRef<T>` 表达，应通过 `.Value` 读写。
- 当前支持范围以 `V*.cs`、`VuetifyCore.cs` 与绑定回归测试为准；README 不维护容易过期的组件数量或覆盖率快照。

## 如何阅读 API 注释

组件参数的 XML 注释优先保留 Vuetify 锁定版本 web-types 的官方原文；已有手写 binding 注释继续作为
补充。IDE 悬停 `VBtn.Color`、`VTextField.ModelValue` 或 `VAlert.ChildContent` 可查看语义，
`[ECMAScriptName]` 展示对应的 Vue prop、event 或 slot 名称。

## 验证

```bash
dotnet build src/ECMAScript.Vuetify/ECMAScript.Vuetify.csproj
dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs
```

## 相关文档

- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
- [Razor-to-Vue](../../docs/02-architecture/razor-to-vue.md)
- [安装与配置](../../docs/03-guides/installation-and-configuration.md)
