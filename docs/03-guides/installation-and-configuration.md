# 安装与配置

> 面向：使用 Jazor 核心平台、当前 Razor-to-Vue 集成或可选生态绑定的应用开发者。
>
> 说明：类库资源只使用两种 carrier：已有 JavaScript 使用 `manifest.json + dist/**`，Jazor
> 编译结果使用程序集内的 `Jazor.Generated.ModuleCatalog`。最终宿主在构建后由 MSBuild 调用
> `Jazor.Emit`，一次性物化选中的依赖闭包到 `JazorDir`。

## 前置条件

- 使用仓库 [global.json](../../global.json) 指定的 .NET SDK；当前项目目标为 `net11.0`。
- 所有 Jazor 与 `ECMAScript.*` 包应使用同一版本。
- 普通 ECMAScript 模块库不需要 Node、CDN 或全局 JavaScript 工具链。

## 选择包

| 需求 | 必需包 | 可选包 |
| --- | --- | --- |
| C# -> ECMAScript 模块 | `Jazor` | 对应的 `ECMAScript.*` 绑定 |
| 普通 C# -> ECMAScript 类库 | `Jazor` | 不需要 Vue 依赖 |
| 当前 Razor-to-Vue 集成 | `Jazor`、`Jazor.Vue` | Vue authoring、Razor hook、Vue runtime 与基础 Vue bindings |
| RazorVue 的 Blazor framework CLR mapping | `Jazor`、`Jazor.Vue` | mapping 由 `Jazor.CLR.Generator` 生成；运行时 JavaScript 由 `ECMAScript` 的 `manifest.json + dist/**` 提供；`ECMAScript.Blazor` 仅由 `Jazor.Vue` 带入可选的标准 ECMAScript 模拟/投影扩展 |
| Vue Router | `Jazor`、`Jazor.Vue`、`ECMAScript.VueRoute` | `ECMAScript.VueRoute` 显式提供 Router bindings |
| Pinia | `Jazor`、`Jazor.Vue`、`ECMAScript.Pinia` | `ECMAScript.Pinia.Testing` |
| Vue Devtools 自定义插件 | `Jazor`、`Jazor.Vue`、`ECMAScript.Vue.Devtools` | `Jazor.Vue` 提供 Vue runtime 闭包 |
| Vue Data UI 图表 | `Jazor`、`Jazor.Vue`、`ECMAScript.VueDataUi` | 无 |
| Vu Icons 图标 | `Jazor`、`Jazor.Vue`、`ECMAScript.VuIcons` | 无 |
| UI 组件库 | `Jazor`、`Jazor.Vue`、对应 `ECMAScript.*` 包 | `ECMAScript.Style` |
| 管理壳 | `Jazor`、`Jazor.Vue`、`Jazor.Admin` | 路由、样式和应用选择的 UI 绑定 |

核心包示例：

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.26.0" />
</ItemGroup>
```

Razor-to-Vue 是上层 opt-in，不会随 `Jazor` 自动启用：

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Jazor" Version="0.26.0" />
    <PackageReference Include="Jazor.Vue" Version="0.26.0" PrivateAssets="all" />
  </ItemGroup>
</Project>
```

按需添加生态包，不以 `object` 或未声明的 JavaScript import 代替强类型绑定：

```xml
<ItemGroup>
  <PackageReference Include="ECMAScript.Style" Version="0.26.0" />
  <PackageReference Include="ECMAScript.Vue.Devtools" Version="0.26.0" />
  <PackageReference Include="ECMAScript.VueDataUi" Version="0.26.0" />
  <PackageReference Include="ECMAScript.VuIcons" Version="0.26.0" />
  <PackageReference Include="ECMAScript.Pinia" Version="0.26.0" />
  <PackageReference Include="ECMAScript.VueRoute" Version="0.26.0" />
  <PackageReference Include="ECMAScript.Vuetify" Version="0.26.0" />
  <PackageReference Include="ECMAScript.ElementPlus" Version="0.26.0" />
  <PackageReference Include="ECMAScript.TDesign" Version="0.26.0" />
</ItemGroup>
```

## 配置产物输出

将输出配置放在最终可执行项目或 Web 宿主中。类库通常保留默认的 `JazorMode=none`；纯 Jazor
类库在程序集内携带 `ModuleCatalog`，JS resource library 通过传递的 manifest locator 提供
`manifest.json + dist/**`。最终宿主的 MSBuild target 在 `Build` 后调用 Emit，读取这两种输入
并直接写出 `JazorDir`。

多项目和 NuGet 类库遵循“谁使用，谁直接引用”：定义模块或 RazorVue 组件的类库直接引用相应工具，
最终宿主直接引用并配置 Emit；只消费上游类库的中间项目不因资源传递增加 `Jazor`/`Jazor.Vue`。
工具资产应在类库包中使用 `PrivateAssets="all"` 隔离，生成模块则随 `ModuleCatalog`、ESM/CSS
则随 manifest 的显式依赖传播。完整规则见[类库产物与引用契约](../02-architecture/library-artifact-contract.md)。

```xml
<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)\jazor\</JazorDir>
</PropertyGroup>
```

| 属性 | 默认值 | 说明 |
| --- | --- | --- |
| `JazorMode` | `none` | `none` 不输出；`debug` 直接物化模块、source map、manifest 与 import map；`release` 生成生产 bundle 和所需资源 |
| `JazorDir` | `$(MSBuildProjectDirectory)\jazor\` | 最终输出目录；Emit 通过 staging 校验后原子替换该目录 |
| `JazorSSR` | `false` | 启用受支持 SSR 时在同一依赖闭包下额外物化 SSR runner、Vue 和 server-renderer 所需资源 |

`debug` 与 `release` 是互斥输出模式。`release` 通过内置 Netpack 路径完成浏览器打包；不要求应用自行维护 `node_modules` 或 CDN import。

## 一次性切换边界

资源契约是一次性破坏性收敛，不提供旧 carrier 的迁移 API、双读 reader、目录 fallback 或
中间 NuGet。采用最终版本时，必须在一次 lockstep 构建中升级所有 Jazor/生态包，并清空或新建
`JazorDir` 后重新构建。历史 API 名称和旧目录说明只保留在
[历史演进](../05-history/evolution.md)，不属于当前配置契约。

开发时使用 `dotnet watch run` 让最终宿主重新构建；启用 `AddJazorReload()` 时，reload 服务只
消费本次 Emit 成功物化的 HMR 元数据和模块输出。生成目录被排除在 MSBuild 输入项外，无法证明
更新可安全热替换时执行整页刷新，不扫描资源目录猜测“最新”文件。

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

ASP.NET Core 负责路由、静态文件与响应；`Jazor.AspNetCore` 使用 `JazorDir` 中由 Emit
物化的 SSR runner 和本地 Vue 服务器模块，DenoHost 执行这些模块，Netpack 负责浏览器 bundle。
`WorkerCount` 同时限制单应用实例的 Deno worker 数和 SSR 并发数，必须大于零，默认值为
`min(Environment.ProcessorCount, 4)`。宿主不得在 Emit 提交后改写 runner；SSR 不自动传递 Vue
server-prefetch 状态，应用应把需要共享的状态显式放入 props 或自己的 payload。

## 后续阅读

- 核心语义与支持边界：[编译器](../02-architecture/compiler.md)
- Razor 应用方向：[Razor-to-Vue](../02-architecture/razor-to-vue.md)
- 产物归属：[产物管线](../02-architecture/artifact-pipeline.md)
- 多项目类库、直接引用与资源传播：[类库产物与引用契约](../02-architecture/library-artifact-contract.md)
- 管理壳库：[管理壳](../02-architecture/admin-shell.md)
