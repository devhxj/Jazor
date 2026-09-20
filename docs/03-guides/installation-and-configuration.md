# 安装与配置

> 面向：使用 Jazor 核心平台、当前 Razor-to-Vue 集成或可选生态绑定的应用开发者。
>
> 说明：ECMAScript 自有 MJS 与 `Jazor.Generated.ModuleCatalog` 提供项目源码，`ECMAScript.*` binding 提供 npm/JSR identity 与标准 ESM specifier。最终宿主在 MSBuild 中调用 `Jazor.Emit`，生成标准 `jazor/` 项目并恢复依赖。

## 前置条件

当前版本为 **1.0.0-preview.3（2026-09-16）**，属于预发布。版本与发布说明以[主仓库 Release](https://github.com/devhxj/Jazor/releases/tag/v1.0.0-preview.3)和对应 tag 为准；镜像同步及预发布筛选可能导致旧版仍显示在顶部。安装时显式指定版本；NuGet UI 需启用“包括预发行版”。主分支新增能力是否已发布，应核对 [CHANGELOG](../../CHANGELOG.md) 中对应版本的记录。

- 使用仓库 [global.json](../../global.json) 指定的 .NET SDK；当前项目目标为 `net11.0`。
- 所有 Jazor 与 `ECMAScript.*` 包应使用同一版本。
- ECMAScript 源码库通过 NuGet carrier 写入项目源码，绑定库通过 npm/JSR dependency 接入项目。

## 选择包

| 需求 | 必需包 | 可选包 |
| --- | --- | --- |
| C# -> ECMAScript 模块 | `Jazor` | 对应的 `ECMAScript.*` 绑定 |
| 普通 C# -> ECMAScript 类库 | `Jazor` | 不需要 Vue 依赖 |
| 当前 Razor-to-Vue 集成 | `Jazor`、`Jazor.Vue` | Vue authoring、Razor hook、Vue runtime 与基础 Vue bindings |
| RazorVue 的 Blazor framework CLR mapping | `Jazor`、`Jazor.Vue` | mapping 由 `Jazor.CLR.Generator` 生成；核心运行时由 `ECMAScript` manifest 与 `src/ECMAScript/clr/**` 提供，Vue bridge 由 `Jazor.Vue/dist/**` 提供 |
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
  <PackageReference Include="Jazor" Version="1.0.0-preview.3" />
</ItemGroup>
```

Razor-to-Vue 通过显式引用 `Jazor.Vue` 启用：

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Jazor" Version="1.0.0-preview.3" />
    <PackageReference Include="Jazor.Vue" Version="1.0.0-preview.3" PrivateAssets="all" />
  </ItemGroup>
</Project>
```

按需添加生态包，并通过强类型 binding 声明 JavaScript import：

```xml
<ItemGroup>
  <PackageReference Include="ECMAScript.Style" Version="1.0.0-preview.3" />
  <PackageReference Include="ECMAScript.Vue.Devtools" Version="1.0.0-preview.3" />
  <PackageReference Include="ECMAScript.VueDataUi" Version="1.0.0-preview.3" />
  <PackageReference Include="ECMAScript.VuIcons" Version="1.0.0-preview.3" />
  <PackageReference Include="ECMAScript.Pinia" Version="1.0.0-preview.3" />
  <PackageReference Include="ECMAScript.VueRoute" Version="1.0.0-preview.3" />
  <PackageReference Include="ECMAScript.Vuetify" Version="1.0.0-preview.3" />
  <PackageReference Include="ECMAScript.ElementPlus" Version="1.0.0-preview.3" />
  <PackageReference Include="ECMAScript.TDesign" Version="1.0.0-preview.3" />
</ItemGroup>
```

## 配置产物输出

输出配置放在最终可执行项目或 Web 宿主中。类库通常保留默认的 `JazorMode=none`；源码类库通过 `ModuleCatalog` 或源码 locator 传递模块，binding 通过 metadata 传递 npm/JSR identity 与入口。最终宿主的 MSBuild target 调用 Emit 写出源码、入口和 `package.json`，再使用 Deno 2.9.7 恢复 `node_modules` 并生成或验证 `deno.lock`。

多项目和 NuGet 类库遵循“谁使用，谁直接引用”：定义模块或 RazorVue 组件的类库直接引用相应工具��最终宿主直接引用并配置 Emit；工具资产在类库包中使用 `PrivateAssets="all"` 隔离。生成模块随 `ModuleCatalog` 传播，binding declaration 随 manifest locator 传播。完整规则见[类库与标准前端项目契约](../02-architecture/library-artifact-contract.md)。

```xml
<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)\jazor\</JazorDir>
</PropertyGroup>
```

| 属性 | 默认值 | 说明 |
| --- | --- | --- |
| `JazorMode` | `none` | `none` 不输出；`debug` 直接物化模块、source map、manifest 与 import map；`release` 生成生产 bundle 和所需资源 |
| `JazorDir` | `$(MSBuildProjectDirectory)\jazor\` | 最终输出目录；Emit 就地写入并按清单差异清理过期文件 |
| `JazorSSR` | `false` | 启用受支持 SSR 时生成 SSR 入口，并从同一项目根使用已恢复的依赖 |

`debug` 与 `release` 是互斥输出模式。`release` 通过内置 NetPack 路径从 `jazor/` 项目入口完成浏览器打包。

## 一次性切换边界

资源契约采用一次性破坏性收敛。最终版本在一次 lockstep 构建中升级所有 Jazor/生态包，并使用清理后的 `JazorDir` 重新构建。历史 API 名称和旧目录说明归档于[历史演进](../05-history/evolution.md)。

开发时使用 `dotnet watch run` 触发最终宿主重新构建；启用 `AddJazorReload()` 时，reload 服务消费本次 Emit 成功物化的 HMR 元数据和模块输出。生成目录位于 MSBuild 输入项范围之外；更新缺少安全热替换证据时，服务执行整页刷新。

`ECMAScript.Style` 的 DSL 使用 `lower_snake_case`，例如 CSS 声明使用 `background_color`，并生成 CSS `background-color`。WebIDL 生成的 DOM 对象按规范使用 `backgroundColor`。两套 C# 表面分别维持既定命名：`CssRule`、`CssDeclarations`、`CssAtRule`、`CssShadow`、`CssChild` 和 `CssOptions` 等 CLR 模型采用 PascalCase，生成 CSS、`style.mjs` 与浏览器 HMR 协议。

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
app.UseJazorSsr(new JazorSsrRequest(
    "components/app.mjs",
    new { Title = "Jazor" },
    [new JazorSsrProvider("jazor:service:MyApp.BrowserClient", new { BaseUrl = "/api" })]));
```

如果页面需要在 SSR 首屏和 hydration 中读取认证快照，宿主可以转换当前请求的 `ClaimsPrincipal`，得到 `JazorAuthenticationState`。该状态以保留键 `jazor:auth-state` 进入同一份 `jazor-ssr-state` v1 envelope；不要再手工添加同名 provider：

```csharp
app.UseJazorSsr(
    (context, _) => Task.FromResult(
        new JazorSsrRequest(
            "components/app.mjs",
            Authentication: JazorAuthenticationState.FromPrincipal(context.User))));
```

该快照表达匿名、已认证、过期或访问受限状态及只读 claims；授权事实由服务端 endpoint 决定。认证、表单防伪、token 存储和服务器 circuit 分别采用相应宿主协议。

### 显式 typed bootstrap

业务首屏数据建议使用应用自己的 DTO，由 endpoint 同时返回业务版本。版本失配时重新读取数据；提交出现错误时维持客户端草稿，防伪、权限和最终写入由 endpoint 负责：

```csharp
public sealed record EditorBootstrap(int Version, IReadOnlyList<EditorRow> Rows);
public sealed record EditorRow(string Id, string Name);
public sealed record EditorCommand(int Version, IReadOnlyList<EditorRow> Rows);

app.MapGet("/api/editor/bootstrap", async (EditorService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.ReadAsync(cancellationToken)));

app.MapPost("/api/editor/commit", async (EditorCommand command, EditorService service,
    CancellationToken cancellationToken) =>
{
    // Endpoint owns antiforgery, authorization, and optimistic-version checks.
    var result = await service.CommitAsync(command, cancellationToken);
    return result.IsVersionConflict
        ? Results.Conflict(await service.ReadAsync(cancellationToken))
        : result.IsValid
            ? Results.Ok(result)
            : Results.UnprocessableEntity(result.Errors);
});
```

页面通过 `JazorSsrRequest.Props` 交接 `EditorBootstrap`。收到 `409` 时刷新 bootstrap 并请求用户确认覆盖；收到验证错误或网络错误时继续显示当前编辑草稿。该协议通过版本化 bootstrap 交接状态。

浏览器交互使用 `@jazor/vue-runtime/authentication.mjs` 的显式 typed provider。登录、刷新和登出回调由应用 endpoint 提供，并返回 `JazorAuthenticationEnvelope.Create(state)` 生成的 `jazor-auth-state` v1 载荷；provider 以 endpoint 响应作为授权结果来源。endpoint 异常通过 `provider.error` 暴露，当前状态保持可观察；并发请求按最新请求生效。该 provider 定义 Jazor 的 browser contract。

ASP.NET Core 负责路由、静态文件与响应；`Jazor.AspNetCore` 使用 `JazorDir` 中由 Emit 物化的 SSR runner 和本地 Vue 服务器模块，DenoHost 执行这些模块，Netpack 负责浏览器 bundle。`WorkerCount` 定义单应用实例的 Deno worker 数和 SSR 并发数，取正整数，默认值为 `min(Environment.ProcessorCount, 4)`。Emit 提交后的 runner 保持字节稳定；SSR state 通过 `JazorSsrRequest.Providers` 显式传递字符串 key 和 JSON value，共享业务状态通过 props 或应用自有 payload 传递。

## 后续阅读

- 核心语义与支持边界：[编译器](../02-architecture/compiler.md)
- Razor 应用方向：[Razor-to-Vue](../02-architecture/razor-to-vue.md)
- 产物归属：[产物管线](../02-architecture/artifact-pipeline.md)
- 多项目类库、直接引用与源码/绑定传播：[类库与标准前端项目契约](../02-architecture/library-artifact-contract.md)
- 管理壳库：[管理壳](../02-architecture/admin-shell.md)
