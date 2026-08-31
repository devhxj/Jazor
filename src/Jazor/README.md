# Jazor

> 定位：将受支持 C# 语义编译为确定性 ECMAScript 模块的核心 NuGet 包。

`Jazor` 包含框架无关的 runtime contract、analyzer、source generator、Emit 工具、MSBuild 集成与 ASP.NET Core 集成。它可以用于编写普通 ECMAScript 类库；Vue authoring、Razor-to-Vue、Vue bindings 与 Vue runtime 由独立的 `Jazor.Vue` opt-in 包提供。

> 资源契约：已有 JavaScript 由 `manifest.json + dist/**` 携带，Jazor 编译模块由程序集内的
> `Jazor.Generated.ModuleCatalog` 携带。两者由最终宿主构建后的 Emit 一次性物化。

类库携带 JavaScript 只有这两种形式；RazorVue 是纯 Jazor 类库的 authoring 场景，不是第三种
carrier。`ModuleCatalog`（`ECMAScriptCode`）是开发者编写的 C#/RazorVue 模块的程序集内生成载体；
`manifest.json + dist/**` 是 `ECMAScript`、Vue、Vuetify 等已有 JavaScript 的包内资源载体。
`ModuleCatalog` 的存在是因为 analysis/source generator 的标准输出是 C#；它是纯 Jazor 的正式
生成格式，不是遗留兼容载体。

## 安装

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.26.3" />
</ItemGroup>
```

纯 Jazor 类库（开发者编写 C# 并声明 `[ECMAScriptModule]`）直接引用 `Jazor`。类库保留默认
`JazorMode=none`，自身生成模块写入 DLL 内的 `ModuleCatalog`；最终可执行或 Web 宿主负责
收集程序集 catalog 与传递的资源 manifest 并输出产物。编写 RazorVue 组件的项目属于纯 Jazor
类库，必须直接引用 `Jazor` 和 `Jazor.Vue`；只消费上游类库的项目不会因普通引用获得工具资格。

定义 module 的类库应隔离这项工具引用：

```xml
<PackageReference Include="Jazor" Version="0.26.3" PrivateAssets="all" />
```

最终 `Exe`/`WinExe` 宿主需要 Emit 时直接引用 `Jazor`，不设置 `PrivateAssets`。包内
`build/Jazor.props` 和 `build/Jazor.targets` 只对直接引用激活 compiler/analyzer/Emit；
`buildTransitive/Jazor.Resources.targets` 只传递 ECMAScript resource manifest locator。

多项目场景遵循“谁使用，谁直接引用”：只消费上游程序集的中间类库不因资源传递而启用 Jazor
工具链；最终宿主必须自行直接引用 `Jazor` 并配置 Emit。完整的工具资产隔离和资源传播规则见
[类库产物与引用契约](../../docs/02-architecture/library-artifact-contract.md)。

## 宿主输出

```xml
<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)\jazor\</JazorDir>
</PropertyGroup>
```

| `JazorMode` | 行为 |
| --- | --- |
| `none` | 默认值，不写入产物 |
| `debug` | 直接物化模块、source map、`jazor-manifest.json` 与 import map |
| `release` | 经内置 Netpack 路径输出生产浏览器 bundle 与所需资源 |

`JazorDir` 是最终输出目录。`Jazor.targets` 在最终 `Exe`/`WinExe` 的 `Build` 后调用
`Jazor.Emit`，Emit 先在同卷 staging 目录完成校验，再原子替换该目录。`JazorSSR=true` 会在
同一依赖闭包下额外物化 SSR runner、Vue 和 server-renderer 所需资源；开发 reload 消费本次
成功 Emit 的模块与 HMR 元数据。输出目录不是类库 carrier，也不会成为下一次资源发现的输入。

## 可选生态包

Vue Router、Pinia、UI 组件库与 CSS-in-JS 均需按使用场景显式引用对应 `ECMAScript.*` 包：

```xml
<ItemGroup>
  <PackageReference Include="ECMAScript.VueRoute" Version="0.26.3" />
  <PackageReference Include="ECMAScript.Pinia" Version="0.26.3" />
  <PackageReference Include="ECMAScript.Vuetify" Version="0.26.3" />
  <PackageReference Include="ECMAScript.TDesign" Version="0.26.3" />
  <PackageReference Include="ECMAScript.Style" Version="0.26.3" />
</ItemGroup>
```

`ECMAScript.Vue` 随 `Jazor.Vue` 提供；`ECMAScript.Pinia.Testing` 是叠加在 `ECMAScript.Pinia` 之上的测试期 opt-in 包。所有 Jazor、`Jazor.Vue` 与 Vue 生态包应保持相同版本。

Blazor framework CLR mapping 由 `Jazor.CLR.Generator` 生成并由 `Jazor.CLR` 唯一持有；其 runtime
JavaScript 由 `ECMAScript` 的 `manifest.json + dist/**` 提供。`Jazor` 不因此引用 ASP.NET Core
framework。`ECMAScript.Blazor` 不随核心包安装，只作为 `Jazor.Vue` 可选带入的标准 ECMAScript
模拟/投影扩展，不贡献 whitelist 或 runtime module。

## SSR

在 ASP.NET Core Web 项目中配置 release 与 SSR：

```xml
<PropertyGroup>
  <JazorMode>release</JazorMode>
  <JazorSSR>true</JazorSSR>
</PropertyGroup>
```

```csharp
builder.Services.AddJazorSsr();

var app = builder.Build();
app.UseJazorHost();
app.UseJazorSsr("components/app.mjs", new { Title = "Jazor" });
```

ASP.NET Core 持有请求管线、静态资源与响应文档，DenoHost 执行本地 Vue 服务器模块，Netpack 只负责浏览器构建。应用不需要全局 Deno、`node_modules`、CDN 或远程 import。

## Razor-to-Vue

编写 RazorVue 组件的 Razor SDK 项目必须直接引用两个包：

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.26.3" />
  <PackageReference Include="Jazor.Vue" Version="0.26.3" PrivateAssets="all" />
</ItemGroup>
```

该集成直接消费官方 Razor Source Generator 完成后的最终 `Compilation`，不需要 `EnableRazorHostOutputs`、Razor IR、`RazorCodeDocument` 或二次解析生成 C#。

## 相关文档

- [安装与配置](../../docs/03-guides/installation-and-configuration.md)
- [快速开始](../../docs/03-guides/quick-start.md)
- [产物管线](../../docs/02-architecture/artifact-pipeline.md)
- [Razor-to-Vue](../../docs/02-architecture/razor-to-vue.md)
