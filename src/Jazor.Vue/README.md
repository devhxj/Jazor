# Jazor.Vue

> 定位：Razor SDK 项目显式启用 Razor-to-Vue 的 NuGet 包。

`Jazor.Vue` 安装消费官方 Razor Source Generator 最终 Roslyn `Compilation` 的 generator-driver hook。Razor 组件的 `BuildRenderTree` 操作会降低为 Vue render-function 模块，并注册到供 `Jazor.Emit` 使用的中性 `Jazor.Generated.ArtifactCatalog`；本包的 build-transitive target 还会注册 RazorVue runtime provider。

## 安装

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.20.0" />
  <PackageReference Include="Jazor.Vue" Version="0.20.0" PrivateAssets="all" />
</ItemGroup>
```

该包是 opt-in，必须与 `Jazor` 一起引用。只引用 `Jazor` 不会安装 Razor hook、扫描 Razor 组件或生成 Vue render catalog。`Jazor.Vue` 只携带合并后的 `Jazor.RazorVue` analyzer，避免重复装载共享 generator。

Blazor framework-to-browser mapping declarations 由独立的 `ECMAScript.Blazor` 程序集提供，并由 `Jazor.Vue` 的 NuGet payload 带入；用户不需要单独引用或复制映射源码。实际 Blazor runtime module/helper 仍由 `Jazor.CLR` 提供，`ECMAScript.Blazor` 不属于 `Jazor` 核心包。当前首批只覆盖 Mouse/Keyboard/Focus event 的只读 getter；`ChangeEventArgs.Value` 在事件时刻捕获协议完成前仍不宣称支持。

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
