# 安装与配置

> 本页说明 Jazor SDK、ECMAScript 绑定与 ASP.NET Core 宿主的包选择及 MSBuild 配置。Jolt CLI 和编辑器的安装步骤见 [Jolt 作者指南](./jolt-authoring.md)。
>
> Jolt 应用用 Vite 管理前端入口、依赖和生产构建。普通 C# 类库可以通过 Emit 将模块写入该应用的生成目录；使用独立 Emit 宿主时可由宿主生成完整的前端项目。
>
> 工具链使用进程内 NodeApi/pnpm/Vite。运行条件与旧项目升级步骤见[工具链指南](./toolchain-migration.md)；历史版本使用对应 tag 的文档。

## 前置条件

当前正式版本为 **2.7.1**。Jazor 与 ECMAScript 包使用同一版本。需要从旧版本迁移 class、record 或宿主配置时，见下文和 CHANGELOG。 [2.7.1 发布页](https://github.com/devhxj/Jazor/releases/tag/v2.7.1)

开发新的 Jolt 组件时，直接在项目的 `package.json` 声明应用所需的 npm 包，在 `.jazor` 中导入投影的类型。Vue、Pinia、Vue Router、Pinia Testing 和 Vue Devtools 的手写绑定与 Jazor、Jolt 和 Style 一起发布。旧的七个应用级绑定仍可按 2.3.0 安装。 [投影用法与示例](./jolt-authoring.md#直接调用-npm-函数)

live DOM、BigInt JSON、原生 DOM 事件示例见[浏览器互操作](./browser-interop.md)；升级时统一使用下文的 lockstep 包版本。

- 使用仓库 [global.json](../../global.json) 指定的 .NET SDK；当前基线为 .NET SDK `11.0.100`，项目目标为 `net11.0`；使用原生 `union` 的项目仍须设置 `<LangVersion>preview</LangVersion>`。
- Jazor 发布的 SDK 和生态绑定使用同一版本。
- ECMAScript 源码库通过 NuGet carrier 写入项目源码，绑定库通过 npm/JSR dependency 接入项目。

## 选择包

| 需求 | 必需包 | 可选包 |
| --- | --- | --- |
| C# -> ECMAScript 模块 | `Jazor` | 对应的 `ECMAScript.*` 绑定 |
| 普通 C# -> ECMAScript 类库 | `Jazor` | 不需要 Vue 依赖 |
| 复用普通 NuGet / ProjectReference | `Jazor` 和原本的库引用 | 按实际使用点自动恢复受支持实现，无需额外导入项；范围见[类库导入](../04-roadmap/ordinary-nuget-compilation-research.md) |
| `.jazor` 组件 | `Jazor.Jolt`，按 [Jolt 作者指南](./jolt-authoring.md) 安装 | 项目选用的 npm 组件与编辑器扩展 |
| Vue Router | `Jazor`、`ECMAScript.VueRoute` | |
| Pinia | `Jazor`、`ECMAScript.Pinia` | |
| 使用测试 Pinia 和 action spy | `Jazor`、`ECMAScript.Pinia.Testing` | 在测试项目中按需引用 |
| 开发 Vue Devtools 插件 | `Jazor`、`ECMAScript.Vue.Devtools` | 在定义插件的项目中按需引用 |
| 在 `.jazor` 使用 npm 函数或 composable | `Jazor.Jolt` 和项目 npm 依赖 | 可投影的调用范围见[作者指南](./jolt-authoring.md#直接调用-npm-函数) |
| 在普通 `.cs` 类库使用应用级 npm 库 | 当前源码或本地候选 Jazor SDK，及项目锁定的 npm 依赖 | 在 VS Code 的 C# 编辑器输入 `using Dates = Npm.DateFns.Exports;` 和 `Dates.` 可补全成员，`dotnet build` 独立生成构建所需声明。[生成与消费](./jolt-authoring.md#普通-c-类库的公开投影) |

核心包示例：

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="2.7.1" />
</ItemGroup>
```

Vue 类型、运行时和组件契约由 `ECMAScript.Vue` 提供。共享绑定通过包依赖引入它。

普通 C# 项目按需选择相同版本的核心绑定与样式包。

```xml
<ItemGroup>
  <PackageReference Include="ECMAScript.Style" Version="2.7.1" />
  <PackageReference Include="ECMAScript.Pinia" Version="2.7.1" />
  <PackageReference Include="ECMAScript.VueRoute" Version="2.7.1" />
</ItemGroup>
```

测试项目需要创建测试 Pinia 时，添加同版本的 Pinia Testing。 [测试用例](../../src/ECMAScript.Pinia.Testing.Test/README.md)

```xml
<PackageReference Include="ECMAScript.Pinia.Testing" Version="2.7.1" />
```

定义 Vue Devtools 插件的项目添加同版本的 Devtools 绑定。 [插件样例](../../samples/ECMAScript.Vue.Devtools.Plugin/README.md)

```xml
<PackageReference Include="ECMAScript.Vue.Devtools" Version="2.7.1" />
```

## 配置产物输出

最终宿主可以选择浏览器启动根：

```xml
<ItemGroup>
  <JazorBrowserEntry Include="app.js" />
</ItemGroup>
```

路径必须是生成 `ModuleCatalog` 中的模块路径，可以声明多个启动根。省略时沿用原有入口集合；显式选择仅影响浏览器入口，所有作者模块仍被物化并执行语法与 Vite 模块图检查，SSR/hydration 根独立。路由页面可通过字面量 `import()` 按需加载。页面或编辑器若仍被启动模块静态导入，就仍属于首屏闭包。

### 独立前端宿主

选择由 Emit 创建前端项目时，在最终可执行项目或 Web 宿主配置 `JazorMode`。其引用的类库可直接通过 DLL 内的 `ModuleCatalog` 传递模块。宿主调用 Emit 写出源码、入口和 `package.json`，再通过进程内 pnpm API 恢复依赖。

多项目和 NuGet 类库遵循“谁使用，谁直接引用”：定义模块的类库直接引用 `Jazor`；最终宿主直接引用并配置 Emit。工具资产在类库包中使用 `PrivateAssets="all"` 隔离。生成模块随 `ModuleCatalog` 传播，binding declaration 随 manifest locator 传播。完整规则见[类库与标准前端项目契约](../02-architecture/library-artifact-contract.md)。

```xml
<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)\jazor\</JazorDir>
</PropertyGroup>
```

| 属性 | 默认值 | 说明 |
| --- | --- | --- |
| `JazorMode` | `none` | 项目留空时使用默认值，模块随 DLL 交付。最终宿主选 `debug` 生成开发项目，选 `release` 生成生产资源。 |
| `JazorDir` | `$(MSBuildProjectDirectory)\jazor\` | 指定宿主启用 Emit 后的输出目录。 |
| `JazorSSR` | `false` | 生成开发 SSR 入口；Release 另构建 `ssr/` 运行闭包 |
| `JazorPnpmStoreDirectory` | 用户本地 Jazor store | 共享 pnpm store；不要求生产部署恢复依赖 |
| `JazorPublishSourceMaps` | `false` | 默认发布 `dist/**` 与启用 SSR 时的 `ssr/**`；设置为 `true` 可额外发布这两个目录内的 source map |
| `RuntimeIdentifier` | 未设置 | 优先传入指定 RID，未设置时使用 SDK 当前 RID；同时传入 SDK runtime graph，先选择兼容 runtime asset 再对程序集身份去重 |

`debug` 与 `release` 是互斥输出模式。`release` 从 `jazor/` 入口调用 Vite build API；构建定制放入 Vite 配置。任意 shell scripts 不在进程内执行契约中。

Folder Publish 使用 Release 构建运行闭包，发布后的 `jazor/` 包含 `dist/` 和可选 `ssr/`；源码、根 lock 和 `node_modules` 保留在本地生成项目中。

## 升级与开发输出

升级时统一所有 Jazor/生态包的版本，并审阅[工具链](./toolchain-migration.md)和[类型语义](./type-semantics.md)的迁移步骤。含作者配置的 `JazorDir` 应先保留和合并；确认可再生后再清理旧安装结果。历史 API 与目录布局见[演进记录](../05-history/evolution.md)。

开发时使用 `dotnet watch run` 触发最终宿主重新构建；启用 `AddJazorReload()` 时，reload 服务消费本次 Emit 成功物化的 HMR 元数据和模块输出。生成资源不参与 MSBuild 作者输入，`JazorDir` 中的作者 `.cs` 文件保留；更新缺少安全热替换证据时，服务执行整页刷新。

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
- 应用开发：[Jolt 作者指南](./jolt-authoring.md)
- 产物归属：[产物管线](../02-architecture/artifact-pipeline.md)
- 多项目类库、直接引用与源码/绑定传播：[类库与标准前端项目契约](../02-architecture/library-artifact-contract.md)
