# 安装与配置

> 面向：使用 Jazor 核心平台、当前 Razor-to-Vue 集成或可选生态绑定的应用开发者。

## 前置条件

- 使用仓库 [global.json](../../global.json) 指定的 .NET SDK；当前项目目标为 `net11.0`。
- 所有 Jazor 与 `ECMAScript.*` 包应使用同一版本。
- 普通 ECMAScript 模块库不需要 Node、CDN 或全局 JavaScript 工具链。

## 选择包

| 需求 | 必需包 | 可选包 |
| --- | --- | --- |
| C# -> ECMAScript 模块 | `Jazor` | 对应的 `ECMAScript.*` 绑定 |
| 当前 Razor-to-Vue 集成 | `Jazor`、`Jazor.Vue` | UI、路由、状态与样式绑定 |
| Vue Router | `Jazor`、`ECMAScript.VueRoute` | `Jazor.Vue`，仅 Razor 组件项目需要 |
| Pinia | `Jazor`、`ECMAScript.Pinia` | `ECMAScript.Pinia.Testing` |
| UI 组件库 | `Jazor`、对应 `ECMAScript.*` 包 | `ECMAScript.Style` |
| 管理壳 | `Jazor`、`Jazor.Vue`、`Jazor.Admin` | 路由、样式和应用选择的 UI 绑定 |

核心包示例：

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.8.4" />
</ItemGroup>
```

Razor-to-Vue 是上层 opt-in，不会随 `Jazor` 自动启用：

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Jazor" Version="0.8.4" />
    <PackageReference Include="Jazor.Vue" Version="0.8.4" PrivateAssets="all" />
  </ItemGroup>
</Project>
```

按需添加生态包，不以 `object` 或未声明的 JavaScript import 代替强类型绑定：

```xml
<ItemGroup>
  <PackageReference Include="ECMAScript.Style" Version="0.8.4" />
  <PackageReference Include="ECMAScript.Pinia" Version="0.8.4" />
  <PackageReference Include="ECMAScript.VueRoute" Version="0.8.4" />
  <PackageReference Include="ECMAScript.Vuetify" Version="0.8.4" />
</ItemGroup>
```

## 配置产物输出

将输出配置放在最终可执行项目或 Web 宿主中。类库通常保留默认的 `JazorMode=none`；宿主负责收集引用程序集中的 catalog 并写出最终产物。

```xml
<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorDir>
</PropertyGroup>
```

| 属性 | 默认值 | 说明 |
| --- | --- | --- |
| `JazorMode` | `none` | `none` 不输出；`debug` 输出模块、source map 和 manifest；`release` 输出生产浏览器包 |
| `JazorDir` | `$(MSBuildProjectDirectory)\wwwroot\jazor\` | debug 模块或 release bundle 的输出目录 |
| `JazorSsrEnabled` | `false` | 启用受支持 SSR 时保留服务器渲染需要的原始模块图 |

`debug` 与 `release` 是互斥输出模式。`release` 通过内置 Netpack 路径完成浏览器打包；不要求应用自行维护 `node_modules` 或 CDN import。

## 启用 SSR

在 ASP.NET Core Web 项目中设置 release 输出和 SSR 标志：

```xml
<PropertyGroup>
  <JazorMode>release</JazorMode>
  <JazorSsrEnabled>true</JazorSsrEnabled>
</PropertyGroup>
```

随后在应用启动代码中注册并使用 SSR 服务：

```csharp
builder.Services.AddJazorSsr();

var app = builder.Build();
app.UseStaticFiles();
app.UseJazorSsr("components/app.mjs", new { Title = "Jazor" });
```

ASP.NET Core 负责路由、静态文件与响应；DenoHost 执行本地 Vue 服务器模块；Netpack 负责浏览器 bundle。SSR 不自动传递 Vue server-prefetch 状态，应用应把需要共享的状态显式放入 props 或自己的 payload。

## 后续阅读

- 核心语义与支持边界：[编译器](../02-architecture/compiler.md)
- Razor 应用方向：[Razor-to-Vue](../02-architecture/razor-to-vue.md)
- 产物归属：[产物管线](../02-architecture/artifact-pipeline.md)
- 管理壳库：[管理壳](../02-architecture/admin-shell.md)
