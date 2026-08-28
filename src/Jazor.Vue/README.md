# Jazor.Vue

> 定位：Razor SDK 项目显式启用 Razor-to-Vue 的 NuGet 包。

`Jazor.Vue` 安装消费官方 Razor Source Generator 最终 Roslyn `Compilation` 的 generator-driver hook。Razor 组件的 `BuildRenderTree` 操作会降低为 Vue render-function 模块，并注册到供 `Jazor.Emit` 使用的中性 `Jazor.Generated.ArtifactCatalog`；本包的 build-transitive target 还会注册 RazorVue runtime provider。

## 安装

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.25.0" />
  <PackageReference Include="Jazor.Vue" Version="0.25.0" PrivateAssets="all" />
</ItemGroup>
```

该包是 opt-in，必须与 `Jazor` 一起引用。只引用 `Jazor` 不会安装 Razor hook、扫描 Razor 组件或生成 Vue render catalog。`Jazor.Vue` 只携带合并后的 `Jazor.RazorVue` analyzer，避免重复装载共享 generator。

`Jazor`/`Jazor.Vue` 的 analyzer、generator、build target 和 Emit 资格遵循“谁使用，谁直接引用”；它们不应因为中间类库引用而成为下游的隐式工具依赖。组件库的 ESM/CSS 等运行时资源则通过 artifact/provider manifest 传播。跨项目的完整规则见[类库产物与引用契约](../../docs/02-architecture/library-artifact-contract.md)。

Blazor framework CLR mapping 由 `Jazor.CLR.Generator` 从真实 ASP.NET Core reference symbol 生成，再由 `Jazor.CLR` 唯一持有 module、mapping、helper 和 `Jazor.Artifacts.RuntimeProviderCatalog`；用户不需要复制映射源码或手工注册 provider。`ECMAScript.Blazor` 只是随本包带入的可选标准 ECMAScript 模拟/投影扩展，不贡献 CLR whitelist 或 runtime module。当前 InProof 覆盖 Mouse/Keyboard/Focus/Change，以及 Pointer/Wheel/Drag/Clipboard/Touch/Error/Progress 的原生事件 getter；TouchList 在属性访问时惰性转换为数组 carrier。统一 Release 包边界和基本 Razor/Vue 消费路径已由 `SdkIntegrationTests.Build_LocalReleasePackages_CoreAndVueConsumers_RespectBlazorClrPackageBoundary` 验证，但各事件切片仍缺真实 BrowserSmoke、reference oracle 和事件特定 package consumer，因此尚未宣称 Support；file input、合成 `EventArgs` payload、DataTransfer files/items、TouchList 非 getter 操作仍不支持。

## 产物输出

| `JazorMode` | 结果 |
| --- | --- |
| `none` | 默认值，不输出产物 |
| `debug` | 模块、source map 与 manifest |
| `release` | 生产浏览器 bundle 与 source map |

`JazorDir` 默认是 `$(MSBuildProjectDirectory)\jazor\`。Web 宿主通过 `UseJazorHost()` 将它挂载为浏览器 `/jazor/*`，发布时复制到 `<publish>/jazor/`。该集成不需要 `EnableRazorHostOutputs`、`RazorCodeDocument`、`RazorCSharpDocument` 或二次解析生成 C#；`release` 使用 Netpack 进行浏览器打包。

## 相关文档

- [Jazor.RazorVue](../Jazor.RazorVue/README.md)
- [安装与配置](../../docs/03-guides/installation-and-configuration.md)
- [Razor-to-Vue](../../docs/02-architecture/razor-to-vue.md)
