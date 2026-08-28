# Jazor

> 定位：将受支持 C# 语义编译为确定性 ECMAScript 模块的核心 NuGet 包。

`Jazor` 包含框架无关的 runtime contract、analyzer、source generator、Emit 工具、MSBuild 集成与 ASP.NET Core 集成。它可以用于编写普通 ECMAScript 类库；Vue authoring、Razor-to-Vue、Vue bindings 与 Vue runtime 由独立的 `Jazor.Vue` opt-in 包提供。

## 安装

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.25.0" />
</ItemGroup>
```

所有声明 `[ECMAScriptModule]` 的类库和最终宿主都应引用 `Jazor`。类库保留默认 `JazorMode=none`；最终可执行或 Web 宿主负责收集引用程序集的 catalog 并输出产物。

多项目场景遵循“谁使用，谁直接引用”：只消费上游程序集的中间类库不因 catalog 传递而启用 Jazor 工具链；最终宿主必须自行直接引用 `Jazor` 并配置 Emit。完整的工具资产隔离和资源传播规则见[类库产物与引用契约](../../docs/02-architecture/library-artifact-contract.md)。

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
| `debug` | 输出模块、source map 与 `jazor-manifest.json` |
| `release` | 经内置 Netpack 路径输出生产浏览器 bundle |

`JazorSSR=true` 会在 release 输出中额外保留服务器渲染所需的原始模块图；它与浏览器 bundle 分开维护。

## 可选生态包

Vue Router、Pinia、UI 组件库与 CSS-in-JS 均需按使用场景显式引用对应 `ECMAScript.*` 包：

```xml
<ItemGroup>
  <PackageReference Include="ECMAScript.VueRoute" Version="0.25.0" />
  <PackageReference Include="ECMAScript.Pinia" Version="0.25.0" />
  <PackageReference Include="ECMAScript.Vuetify" Version="0.25.0" />
  <PackageReference Include="ECMAScript.TDesign" Version="0.25.0" />
  <PackageReference Include="ECMAScript.Style" Version="0.25.0" />
</ItemGroup>
```

`ECMAScript.Vue` 随 `Jazor.Vue` 提供；`ECMAScript.Pinia.Testing` 是叠加在 `ECMAScript.Pinia` 之上的测试期 opt-in 包。所有 Jazor、`Jazor.Vue` 与 Vue 生态包应保持相同版本。

Blazor framework CLR mapping 由 `Jazor.CLR.Generator` 生成并由 `Jazor.CLR` 唯一持有，随 `Jazor` 的 `Jazor.Artifacts.RuntimeProviderCatalog` 按统一 provider 管道提供；`Jazor` 不因此引用 ASP.NET Core framework。`ECMAScript.Blazor` 不随核心包安装，只作为 `Jazor.Vue` 可选带入的标准 ECMAScript 模拟/投影扩展，不贡献 whitelist 或 runtime module。

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

Razor SDK 项目需要额外引用 `Jazor.Vue`：

```xml
<ItemGroup>
  <PackageReference Include="Jazor.Vue" Version="0.25.0" PrivateAssets="all" />
</ItemGroup>
```

该集成直接消费官方 Razor Source Generator 完成后的最终 `Compilation`，不需要 `EnableRazorHostOutputs`、Razor IR、`RazorCodeDocument` 或二次解析生成 C#。

## 相关文档

- [安装与配置](../../docs/03-guides/installation-and-configuration.md)
- [快速开始](../../docs/03-guides/quick-start.md)
- [产物管线](../../docs/02-architecture/artifact-pipeline.md)
- [Razor-to-Vue](../../docs/02-architecture/razor-to-vue.md)
