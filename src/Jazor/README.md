# Jazor

> 定位：将受支持 C# 语义编译为确定性 ECMAScript 模块的核心 NuGet 包。

`Jazor` 包含核心 runtime contract、analyzer、source generator、Emit 工具、MSBuild 集成、ASP.NET Core 集成与基础 Vue 3 authoring 类型。Razor-to-Vue 是独立的 `Jazor.Vue` opt-in，不属于核心平台定义。

## 安装

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.8.4" />
</ItemGroup>
```

所有声明 `[ECMAScriptModule]` 的类库和最终宿主都应引用 `Jazor`。类库保留默认 `JazorMode=none`；最终可执行或 Web 宿主负责收集引用程序集的 catalog 并输出产物。

## 宿主输出

```xml
<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorDir>
</PropertyGroup>
```

| `JazorMode` | 行为 |
| --- | --- |
| `none` | 默认值，不写入产物 |
| `debug` | 输出模块、source map 与 `jazor-manifest.json` |
| `release` | 经内置 Netpack 路径输出生产浏览器 bundle |

`JazorSsrEnabled=true` 会在 release 输出中额外保留服务器渲染所需的原始模块图；它与浏览器 bundle 分开维护。

## 可选生态包

Vue Router、Pinia、UI 组件库与 CSS-in-JS 均需按使用场景显式引用对应 `ECMAScript.*` 包：

```xml
<ItemGroup>
  <PackageReference Include="ECMAScript.VueRoute" Version="0.8.4" />
  <PackageReference Include="ECMAScript.Pinia" Version="0.8.4" />
  <PackageReference Include="ECMAScript.Vuetify" Version="0.8.4" />
  <PackageReference Include="ECMAScript.TDesign" Version="0.8.4" />
  <PackageReference Include="ECMAScript.Style" Version="0.8.4" />
</ItemGroup>
```

`ECMAScript.Vue3` 随 `Jazor` 提供；`ECMAScript.Pinia.Testing` 是叠加在 `ECMAScript.Pinia` 之上的测试期 opt-in 包。所有 Jazor 与 `ECMAScript.*` 包应保持相同版本。

## SSR

在 ASP.NET Core Web 项目中配置 release 与 SSR：

```xml
<PropertyGroup>
  <JazorMode>release</JazorMode>
  <JazorSsrEnabled>true</JazorSsrEnabled>
</PropertyGroup>
```

```csharp
builder.Services.AddJazorSsr();

var app = builder.Build();
app.UseStaticFiles();
app.UseJazorSsr("components/app.mjs", new { Title = "Jazor" });
```

ASP.NET Core 持有请求管线、静态资源与响应文档，DenoHost 执行本地 Vue 服务器模块，Netpack 只负责浏览器构建。应用不需要全局 Deno、`node_modules`、CDN 或远程 import。

## Razor-to-Vue

Razor SDK 项目需要额外引用 `Jazor.Vue`：

```xml
<ItemGroup>
  <PackageReference Include="Jazor.Vue" Version="0.8.4" PrivateAssets="all" />
</ItemGroup>
```

该集成直接消费官方 Razor Source Generator 完成后的最终 `Compilation`，不需要 `EnableRazorHostOutputs`、Razor IR、`RazorCodeDocument` 或二次解析生成 C#。

## 相关文档

- [安装与配置](../../docs/03-guides/installation-and-configuration.md)
- [快速开始](../../docs/03-guides/quick-start.md)
- [产物管线](../../docs/02-architecture/artifact-pipeline.md)
- [Razor-to-Vue](../../docs/02-architecture/razor-to-vue.md)
