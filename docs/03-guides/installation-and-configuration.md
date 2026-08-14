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
| Vue Devtools 自定义插件 | `Jazor`、`ECMAScript.Vue.Devtools` | `ECMAScript.Vue` 已由 `Jazor` 提供 runtime 闭包 |
| UI 组件库 | `Jazor`、对应 `ECMAScript.*` 包 | `ECMAScript.Style` |
| 管理壳 | `Jazor`、`Jazor.Vue`、`Jazor.Admin` | 路由、样式和应用选择的 UI 绑定 |

核心包示例：

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.14.0" />
</ItemGroup>
```

Razor-to-Vue 是上层 opt-in，不会随 `Jazor` 自动启用：

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Jazor" Version="0.14.0" />
    <PackageReference Include="Jazor.Vue" Version="0.14.0" PrivateAssets="all" />
  </ItemGroup>
</Project>
```

按需添加生态包，不以 `object` 或未声明的 JavaScript import 代替强类型绑定：

```xml
<ItemGroup>
  <PackageReference Include="ECMAScript.Style" Version="0.14.0" />
  <PackageReference Include="ECMAScript.Vue.Devtools" Version="0.14.0" />
  <PackageReference Include="ECMAScript.Pinia" Version="0.14.0" />
  <PackageReference Include="ECMAScript.VueRoute" Version="0.14.0" />
  <PackageReference Include="ECMAScript.Vuetify" Version="0.14.0" />
</ItemGroup>
```

## 配置产物输出

将输出配置放在最终可执行项目或 Web 宿主中。类库通常保留默认的 `JazorMode=none`；宿主负责收集引用程序集中的 catalog 并写出最终产物。

```xml
<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)\jazor\</JazorDir>
</PropertyGroup>
```

| 属性 | 默认值 | 说明 |
| --- | --- | --- |
| `JazorMode` | `none` | `none` 不输出；`debug` 输出模块、source map 和 manifest；`release` 输出生产浏览器包 |
| `JazorDir` | `$(MSBuildProjectDirectory)\jazor\` | debug 模块或 release bundle 的输出目录；发布时复制到 `<publish>/jazor/` |
| `JazorSSR` | `false` | 启用受支持 SSR 时保留服务器渲染需要的原始模块图 |

`debug` 与 `release` 是互斥输出模式。`release` 通过内置 Netpack 路径完成浏览器打包；不要求应用自行维护 `node_modules` 或 CDN import。

## 从 0.11 升级

`0.12.0` 将默认输出从 `wwwroot/jazor/` 移到项目根 `jazor/`。将已有生成物、手工检查脚本和自定义部署复制规则一并更新；不要保留 `wwwroot/jazor/` 作为回退目录。`UseJazorHost()` 会优先从项目根或发布根的 `jazor/` 提供 `/jazor/*`，发布时目标会显式复制到 `<publish>/jazor/`。

| 旧 API | 新 API |
| --- | --- |
| `AddJazorSSR()` / `UseJazorSSR()` | `AddJazorSsr()` / `UseJazorSsr()` |
| `AddJazorDevelopmentReload()` / `UseJazorDevelopmentReload()` | `AddJazorReload()` / `UseJazorReload()` |
| `UseJazorDevelopmentAssets()` | `UseJazorArtifacts()` |
| `UseJazorWebAssets()` | `UseJazorAssets()` |
| `JazorDevelopmentAssetOptions` / `JazorWebAssetOptions` | `JazorArtifactOptions` / `JazorAssetOptions` |
| `JazorDevelopmentReloadOptions` / `JazorDevelopmentHmrModuleMapping` | `JazorReloadOptions` / `JazorHmrMapping` |

开发时使用 `dotnet watch run` 让宿主重新构建；`AddJazorReload()` 与 `UseJazorReload()` 默认观察项目根 `jazor/` 和 `wwwroot/`。生成输出被排除在 MSBuild 的输入项外，避免生成模块自身触发下一次编译；Jazor reload 服务在构建完成后观察这些输出并选择模板热更新或整页刷新。

`ECMAScript.Style` 的 DSL 应使用 `lower_snake_case`，例如 CSS 声明使用 `background_color`。它会生成 CSS `background-color`；WebIDL 生成的 DOM 对象则继续按规范使用 `backgroundColor`。这是两个独立的 C# 表面，不会发生自动大小写转换；`CssRule`、`CssDeclarations`、`CssAtRule`、`CssShadow`、`CssChild` 和 `CssOptions` 等 CLR 模型保持 PascalCase，生成 CSS、`style.mjs` 以及浏览器 HMR 协议不变。

## 启用 SSR

在 ASP.NET Core Web 项目中设置 release 输出和 SSR 标志：

```xml
<PropertyGroup>
  <JazorMode>release</JazorMode>
  <JazorSSR>true</JazorSSR>
</PropertyGroup>
```

随后在应用启动代码中注册并使用 SSR 服务：

```csharp
builder.Services.AddJazorSsr(options =>
{
    // 默认 min(Environment.ProcessorCount, 4)；按 SSR CPU/内存 profile 调整。
    options.WorkerCount = 4;
});

var app = builder.Build();
app.UseJazorHost();
app.UseJazorSsr("components/app.mjs", new { Title = "Jazor" });
```

ASP.NET Core 负责路由、静态文件与响应；DenoHost 通过 generation-aware persistent worker pool 执行本地 Vue 服务器模块；Netpack 负责浏览器 bundle。`WorkerCount` 同时限制单应用实例的 Deno worker 数和 SSR 并发数，必须大于零，默认值为 `min(Environment.ProcessorCount, 4)`。artifact manifest 或 SSR import map 内容变化时，旧 generation 停止接收新请求；进行中的请求完成后其 worker 被释放。SSR 不自动传递 Vue server-prefetch 状态，应用应把需要共享的状态显式放入 props 或自己的 payload。

## 后续阅读

- 核心语义与支持边界：[编译器](../02-architecture/compiler.md)
- Razor 应用方向：[Razor-to-Vue](../02-architecture/razor-to-vue.md)
- 产物归属：[产物管线](../02-architecture/artifact-pipeline.md)
- 管理壳库：[管理壳](../02-architecture/admin-shell.md)
