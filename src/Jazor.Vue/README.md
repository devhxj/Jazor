# Jazor.Vue

> 定位：Razor SDK 项目显式启用 Razor-to-Vue 的 NuGet 包。

> 资源契约：RazorVue 生成的组件模块进入程序集内的 `Jazor.Generated.ModuleCatalog`；Vue、
> CLR runtime 与其他已有 JavaScript 资源通过 `manifest.json + dist/**` 提供。

`Jazor.Vue` 安装消费官方 Razor Source Generator 最终 Roslyn `Compilation` 的 generator-driver hook。Razor 组件的 `BuildRenderTree` 操作会降低为 Vue render-function 模块，并生成供 `Jazor.Emit` 读取的 `Jazor.Generated.ModuleCatalog`；本包唯一的 `buildTransitive/Jazor.Vue.targets` 始终传递资源 manifest locator，并仅在当前项目直接声明 `Jazor.Vue` 时注册 RazorVue analyzer。

## 安装

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.26.3" />
  <PackageReference Include="Jazor.Vue" Version="0.26.3" PrivateAssets="all" />
</ItemGroup>
```

该包是 opt-in，必须与 `Jazor` 一起引用。只引用 `Jazor` 不会安装 Razor hook、扫描 Razor 组件或生成 Vue 模块。`Jazor.Vue` 只携带合并后的 `Jazor.RazorVue` analyzer，避免重复装载共享 generator。

编写 RazorVue 组件的类库或应用必须在当前项目直接声明 `Jazor` 和 `Jazor.Vue`；组件模块属于
纯 Jazor carrier，生成后进入程序集内的 `Jazor.Generated.ModuleCatalog`。如果项目发布可复用
类库，工具引用通常使用 `PrivateAssets="all"`；只消费该类库的下游项目不会因传递引用获得
RazorVue analyzer、generator 或 Emit 资格。

`Jazor`/`Jazor.Vue` 的 analyzer、generator、build target 和 Emit 资格遵循“谁使用，谁直接引用”；它们不应因为中间类库引用而成为下游的隐式工具依赖。组件库的 ESM/CSS 等运行时资源通过 manifest 的显式依赖传播。跨项目的完整规则见[类库产物与引用契约](../../docs/02-architecture/library-artifact-contract.md)。

RazorVue analyzer 与 `AngleSharp` 位于 `tools/net11.0/analyzers/`，由当前项目直接声明
`Jazor.Vue` 时通过该包的 `buildTransitive/Jazor.Vue.targets` 条件注册；它们不放入 NuGet
自动导入的 `analyzers/dotnet/cs`，也不随组件库引用激活。

Blazor framework CLR mapping 由 `Jazor.CLR.Generator` 从真实 ASP.NET Core reference symbol 生成，再由 `Jazor.CLR` 唯一持有 module、mapping、helper；生成的 runtime JavaScript 进入 `ECMAScript/manifest.json + dist/**`，用户不需要复制映射源码或手工注册资源。`ECMAScript.Blazor` 只是随本包带入的可选标准 ECMAScript 模拟/投影扩展，不贡献 CLR whitelist 或 runtime resource。当前 InProof 覆盖 Mouse/Keyboard/Focus/Change，以及 Pointer/Wheel/Drag/Clipboard/Touch/Error/Progress 的原生事件 getter；TouchList 在属性访问时惰性转换为数组 carrier。统一 Release 包边界和基本 Razor/Vue 消费路径已由 `SdkIntegrationTests.Build_LocalReleasePackages_CoreAndVueConsumers_RespectBlazorClrPackageBoundary` 验证，但各事件切片仍缺真实 BrowserSmoke、reference oracle 和事件特定 package consumer，因此尚未宣称 Support；file input、合成 `EventArgs` payload、DataTransfer files/items、TouchList 非 getter 操作仍不支持。

## 产物输出

| `JazorMode` | 结果 |
| --- | --- |
| `none` | 默认值，不输出产物 |
| `debug` | 直接物化模块、source map、manifest 与 import map |
| `release` | 生产浏览器 bundle、source map 与所需资源 |

`JazorDir` 默认是 `$(MSBuildProjectDirectory)\jazor\` 的最终输出目录。MSBuild 只在最终
`Exe`/`WinExe` 构建后调用 Emit；它读取程序集 ModuleCatalog 与资源 manifest，完成校验后直接
物化到该目录，发布时再由 SDK 复制到 `<publish>\jazor\`。该集成不需要
`EnableRazorHostOutputs`、`RazorCodeDocument`、`RazorCSharpDocument` 或二次解析生成 C#；
`release` 使用 Netpack 进行浏览器打包。

## 相关文档

- [Jazor.RazorVue](../Jazor.RazorVue/README.md)
- [安装与配置](../../docs/03-guides/installation-and-configuration.md)
- [Razor-to-Vue](../../docs/02-architecture/razor-to-vue.md)
