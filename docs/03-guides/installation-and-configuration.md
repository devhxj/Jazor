# 安装与配置

> 面向：使用 Jazor 核心平台、当前 Razor-to-Vue 集成或可选生态绑定的应用开发者。
>
> 说明：ECMAScript 自有 MJS 与 `Jazor.Generated.ModuleCatalog` 提供项目源码，`ECMAScript.*` binding 提供 npm/JSR identity 与标准 ESM specifier。最终宿主在 MSBuild 中调用 `Jazor.Emit`，生成标准 `jazor/` 项目并恢复依赖。
>
> 从 1.1.0 起使用进程内 NodeApi/pnpm/Vite，该工具链已通过四平台组合与 Windows SPA/SSR 发布消费者验收。以下说明描述新工具链；1.0.0 的旧运行时说明应查看对应 tag。迁移步骤与限制见[迁移方案](../04-roadmap/nodejs-pnpm-migration-plan.md)。

## 前置条件

当前正式版本为 **1.2.0（2026-11-13）**。版本与发布说明以[主仓库 Release](https://github.com/devhxj/Jazor/releases/tag/v1.2.0)和对应 tag 为准。安装时显式指定版本，NuGet UI 无需启用“包括预发行版”。从预览版升级时，将全部 Jazor/ECMAScript 包统一更新到 `1.2.0`，并安装 .NET SDK `11.0.100` 或 `global.json` 允许的更新正式 SDK；普通源码无需 `[ECMAScript]` 许可，class 保留类语义、record 承担对象字面量；受影响的 class 声明、继承及宿主配置迁移见下文和 CHANGELOG。更早预览版的迁移路径见 [CHANGELOG](../../CHANGELOG.md)。

preview.5 的 NuGet 发布目录包含 23 个 lockstep 公共包。P3-A/B/C 与 Monaco 的十个绑定包已可安装，但在补齐 browser smoke 与真实 RazorVue consumer 证据前仍属于 Guidance，不进入当前 Support 矩阵。

preview.8 的 live DOM、BigInt JSON、原生文件/拖放及 Razor 语法示例见[浏览器互操作](./browser-interop.md)；升级时统一使用下文的 lockstep 包版本。

- 使用仓库 [global.json](../../global.json) 指定的 .NET SDK；当前基线为 .NET SDK `11.0.100`，项目目标为 `net11.0`；使用原生 `union` 的项目仍须设置 `<LangVersion>preview</LangVersion>`。
- 所有 Jazor 与 `ECMAScript.*` 包应使用同一版本。
- ECMAScript 源码库通过 NuGet carrier 写入项目源码，绑定库通过 npm/JSR dependency 接入项目。

## 选择包

| 需求 | 必需包 | 可选包 |
| --- | --- | --- |
| C# -> ECMAScript 模块 | `Jazor` | 对应的 `ECMAScript.*` 绑定 |
| 普通 C# -> ECMAScript 类库 | `Jazor` | 不需要 Vue 依赖 |
| 当前 Razor-to-Vue 集成 | `Jazor`、`Jazor.Vue` | Vue authoring、Razor hook、Vue runtime 与基础 Vue bindings |
| RazorVue 的 Blazor framework CLR mapping | `Jazor`、`Jazor.Vue` | mapping 由 `Jazor.CLR.Generator` 生成；核心运行时由 `ECMAScript` manifest 与 `src/ECMAScript/clr/**` 提供，Vue bridge 由 `Jazor.Vue/runtime/vue/**` 提供 |
| Vue Router | `Jazor`、`Jazor.Vue`、`ECMAScript.VueRoute` | `ECMAScript.VueRoute` 显式提供 Router bindings |
| Pinia | `Jazor`、`Jazor.Vue`、`ECMAScript.Pinia` | `ECMAScript.Pinia.Testing` |
| Vue Devtools 自定义插件 | `Jazor`、`Jazor.Vue`、`ECMAScript.Vue.Devtools` | `Jazor.Vue` 提供 Vue runtime 闭包 |
| Vue Data UI 图表 | `Jazor`、`Jazor.Vue`、`ECMAScript.VueDataUi` | 无 |
| Lucide 图标 | `Jazor`、`Jazor.Vue`、`ECMAScript.Lucide` | 无 |
| 日期、浏览器工具与浮动定位 | `Jazor`、`Jazor.Vue`、`ECMAScript.DateFns` / `ECMAScript.VueUse` / `ECMAScript.FloatingUi` | 按应用选择 |
| 表单、国际化与服务端状态 | `Jazor`、`Jazor.Vue`、`ECMAScript.VeeValidate` / `ECMAScript.VueI18n` / `ECMAScript.VueQuery` | 按应用选择 |
| 拖拽、文件上传与富文本 | `Jazor`、`Jazor.Vue`、`ECMAScript.VueDraggable` / `ECMAScript.FilePond` / `ECMAScript.WangEditor` | 按应用选择 |
| Monaco 编辑器 | `Jazor`、`Jazor.Vue`、`ECMAScript.Monaco` | bundler/worker 配置按应用提供 |
| UI 组件库 | `Jazor`、`Jazor.Vue`、对应 `ECMAScript.*` 包 | `ECMAScript.Style` |
| 管理壳 | `Jazor`、`Jazor.Vue`、`Jazor.Admin` | 路由、样式和应用选择的 UI 绑定 |

核心包示例：

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="1.2.0" />
</ItemGroup>
```

Razor-to-Vue 通过显式引用 `Jazor.Vue` 启用：

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Jazor" Version="1.2.0" />
    <PackageReference Include="Jazor.Vue" Version="1.2.0" PrivateAssets="all" />
  </ItemGroup>
</Project>
```

按需添加生态包，并通过强类型 binding 声明 JavaScript import：

```xml
<ItemGroup>
  <PackageReference Include="ECMAScript.Style" Version="1.2.0" />
  <PackageReference Include="ECMAScript.Vue.Devtools" Version="1.2.0" />
  <PackageReference Include="ECMAScript.VueDataUi" Version="1.2.0" />
  <PackageReference Include="ECMAScript.Lucide" Version="1.2.0" />
  <PackageReference Include="ECMAScript.Pinia" Version="1.2.0" />
  <PackageReference Include="ECMAScript.VueRoute" Version="1.2.0" />
  <PackageReference Include="ECMAScript.DateFns" Version="1.2.0" />
  <PackageReference Include="ECMAScript.VueUse" Version="1.2.0" />
  <PackageReference Include="ECMAScript.FloatingUi" Version="1.2.0" />
  <PackageReference Include="ECMAScript.VeeValidate" Version="1.2.0" />
  <PackageReference Include="ECMAScript.VueI18n" Version="1.2.0" />
  <PackageReference Include="ECMAScript.VueQuery" Version="1.2.0" />
  <PackageReference Include="ECMAScript.Monaco" Version="1.2.0" />
  <PackageReference Include="ECMAScript.VueDraggable" Version="1.2.0" />
  <PackageReference Include="ECMAScript.FilePond" Version="1.2.0" />
  <PackageReference Include="ECMAScript.WangEditor" Version="1.2.0" />
  <PackageReference Include="ECMAScript.Vuetify" Version="1.2.0" />
  <PackageReference Include="ECMAScript.ElementPlus" Version="1.2.0" />
  <PackageReference Include="ECMAScript.TDesign" Version="1.2.0" />
</ItemGroup>
```

## 配置产物输出

preview.9 允许最终宿主选择浏览器启动根：

```xml
<ItemGroup>
  <JazorBrowserEntry Include="app.js" />
</ItemGroup>
```

路径必须是生成 `ModuleCatalog` 中的模块路径，可以声明多个启动根。省略时沿用原有入口集合；显式选择仅影响浏览器入口，所有作者模块仍被物化并执行语法与 Vite 模块图检查，SSR/hydration 根独立。RazorVue 路由页面通过 literal `import()` 按导航加载，布局仍静态导入。页面或编辑器若仍被启动模块静态导入，就仍属于首屏闭包。

输出配置放在最终可执行项目或 Web 宿主中。类库通常保留默认的 `JazorMode=none`；源码类库通过 `ModuleCatalog` 或源码 locator 传递模块，binding 通过 metadata 传递 npm/JSR identity 与入口。最终宿主调用 Emit 写出源码、入口和 `package.json`，再通过进程内 pnpm API 恢复 `node_modules` 并生成或验证 `pnpm-lock.yaml`。

多项目和 NuGet 类库遵循“谁使用，谁直接引用”：定义模块或 RazorVue 组件的类库直接引用相应工具；最终宿主直接引用并配置 Emit。工具资产在类库包中使用 `PrivateAssets="all"` 隔离。生成模块随 `ModuleCatalog` 传播，binding declaration 随 manifest locator 传播。完整规则见[类库与标准前端项目契约](../02-architecture/library-artifact-contract.md)。

```xml
<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)\jazor\</JazorDir>
</PropertyGroup>
```

| 属性 | 默认值 | 说明 |
| --- | --- | --- |
| `JazorMode` | `none` | `none` 不输出；`debug` 生成标准 JS 项目、模块和 source map；`release` 另调用 Vite build API；preview.8 将默认值延后至 targets，允许项目体用空值条件设置模式 |
| `JazorDir` | `$(MSBuildProjectDirectory)\jazor\` | 最终输出目录；相对路径以项目目录解析；preview.8 保留此目录中的作者 `.cs` / `.razor` 输入，只排除生成资源 |
| `JazorSSR` | `false` | 生成开发 SSR 入口；Release 另构建 `ssr/` 运行闭包 |
| `JazorPnpmStoreDirectory` | 用户本地 Jazor store | 共享 pnpm store；不要求生产部署恢复依赖 |
| `JazorPublishSourceMaps` | `false` | preview.8 默认发布 `dist/**` 与启用 SSR 时的 `ssr/**`；设置为 `true` 可额外发布这两个目录内的 source map |
| `RuntimeIdentifier` | 未设置 | preview.8 优先传入指定 RID，未设置时使用 SDK 当前 RID；同时传入 SDK runtime graph，先选择兼容 runtime asset 再对程序集身份去重 |

`debug` 与 `release` 是互斥输出模式。`release` 从 `jazor/` 入口调用 Vite build API；构建定制放入 Vite 配置。任意 shell scripts 不在进程内执行契约中。

Folder Publish 使用 Release 构建运行闭包，发布后的 `jazor/` 包含 `dist/` 和可选 `ssr/`；源码、根 lock 和 `node_modules` 保留在本地生成项目中。

## 一次性切换边界

资源契约采用一次性破坏性收敛。最终版本在一次 lockstep 构建中升级所有 Jazor/生态包，并使用清理后的 `JazorDir` 重新构建。历史 API 名称和旧目录说明归档于[历史演进](../05-history/evolution.md)。

开发时使用 `dotnet watch run` 触发最终宿主重新构建；启用 `AddJazorReload()` 时，reload 服务消费本次 Emit 成功物化的 HMR 元数据和模块输出。生成资源不参与 MSBuild 作者输入，`JazorDir` 中的作者 `.cs` / `.razor` 文件保留；更新缺少安全热替换证据时，服务执行整页刷新。

`ECMAScript.Style` 的 DSL 使用 `lower_snake_case`，例如 CSS 声明使用 `background_color`，并生成 CSS `background-color`。WebIDL 生成的 DOM 对象按规范使用 `backgroundColor`。两套 C# 表面分别维持既定命名：`CssRule`、`CssDeclarations`、`CssAtRule`、`CssShadow`、`CssChild` 和 `CssOptions` 等 CLR 模型采用 PascalCase，生成 CSS、`style.js` 与浏览器 HMR 协议。

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

ASP.NET Core 负责路由、静态文件与响应。`Jazor.AspNetCore` 在开发时通过 NodeApi 加载项目 SSR 入口；Release 从独立发布目录加载 `ssr/ssr-entry.js`、本地 chunks 与 `ssr/package.json`，无需恢复依赖或访问 pnpm store。`WorkerCount` 定义单应用实例的持久 NodeApi environment 数和 SSR 并发数，必须为正数，默认 `min(Environment.ProcessorCount, 4)`；每次渲染创建独立 Vue app。state 通过 `JazorSsrRequest.Providers` 的字符串 key 与 JSON value 显式传递，业务状态通过 props 或应用 payload 传递。

从旧运行时迁移时移除 `--deno` / `JazorDenoPath` 和 Vite/SSR `TaskName` 设置，审阅作者 scripts 与 Vite 配置，改用 `pnpm-lock.yaml`。不要把 Deno 路径解释为 Node 路径。共享 store 使用 `JazorPnpmStoreDirectory`；Windows 宿主须启动当前构建的 `CETCompat=false` apphost。安装不执行 shell lifecycle scripts，取消 pnpm 恢复会等待不可中断的原生安装实际结束。

## 后续阅读

- 核心语义与支持边界：[编译器](../02-architecture/compiler.md)
- Razor 应用方向：[Razor-to-Vue](../02-architecture/razor-to-vue.md)
- 产物归属：[产物管线](../02-architecture/artifact-pipeline.md)
- 多项目类库、直接引用与源码/绑定传播：[类库与标准前端项目契约](../02-architecture/library-artifact-contract.md)
- 管理壳库：[管理壳](../02-architecture/admin-shell.md)
