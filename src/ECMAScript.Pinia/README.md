# ECMAScript.Pinia

> 定位：Pinia runtime 的独立强类型 C# binding，不向 compiler 引入 Pinia 特判。

本包属于 JS resource library：Pinia 的已有 runtime ESM 位于包内
`manifest.json + dist/**`，许可证等附属文件由 manifest 显式声明；C# 程序集只提供映射和
authoring contract。消费方编写的 RazorVue 模块生成到消费程序集的
`Jazor.Generated.ModuleCatalog`，不会改写本包资源。

## 安装

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.26.2" />
  <PackageReference Include="Jazor.Vue" Version="0.26.2" PrivateAssets="all" />
  <PackageReference Include="ECMAScript.Pinia" Version="0.26.2" />
</ItemGroup>
```

需要 `@pinia/testing` 时额外引用 `ECMAScript.Pinia.Testing`，并保持所有包版本一致。

## 职责

- 提供 `createPinia()`、`defineStore()`、`storeToRefs()`、active Pinia、HMR 与 hydration binding。
- 提供 Options API helper：`mapState()`、`mapGetters()`、`mapWritableState()`、`mapActions()`、`mapStores()` 与 `setMapStoreSuffix()`。
- 提供 store、definition、plugin context、setup-store options 与 projected store 的强类型 runtime shape。
- 通过显式 projection helper 支持 `$onAction(...)`、`$subscribe(...)`、`pinia.use(...)`、plugin-added state 与 setup-store action authoring。

## authoring 边界

- 公开 API 优先表达为具体 C# 类型、overload 或 named union；不使用 `object` 模拟 JavaScript `any`。
- `StoreActionListenerContext.After<TResult>(...)` 和四参数以上的方法组有 C# 推断限制时，调用方应显式声明泛型参数或先落到委托局部变量。
- `TestingOptions`、`TestingStubActions` 与测试 runner 相关 contract 位于独立 `ECMAScript.Pinia.Testing` 包。
- runtime import 使用 `pinia`，最终版本解析由资源 manifest 的 package dependency 与宿主 Emit 依赖闭包处理。
- Pinia 4 的 development entry 会按官方依赖闭包带入 `nostics` 与 `@vue/devtools-api`。调用 `app.Use(pinia)` 后，Pinia 自行向已安装的 Vue Devtools 浏览器扩展注册面板；这不是额外的 C# authoring API，也不会进入 production entry。

## 代码结构

- `Pinia.cs`：模块入口与委托声明。
- `Api/Pinia.Api.cs`：Pinia API、Options API helper 与 HMR/hydration 入口。
- `Types/Pinia.Types.*.cs`：store/runtime shape、options、callback context 与 helper value type。

## 相关文档

- [ECMAScript.Pinia.Testing](../ECMAScript.Pinia.Testing/README.md)
- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
