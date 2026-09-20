# Jazor.AspNetCore

为 ASP.NET Core 提供生成产物托管、SPA 导航 fallback 和 Vue SSR。此程序集及其 XML 文档随 **Jazor** NuGet 包交付；Vue/Razor authoring 另需 **Jazor.Vue**。不需要安装名为 Jazor.AspNetCore 的独立包。项目配置见 [Jazor 包说明](../Jazor/README.md)。

## SPA 接入

在已配置 Jazor 产物输出的 Web 宿主中，`Program.cs` 可采用以下顺序：

```csharp
using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;

var builder = JazorWebApplication.CreateBuilder(args);
builder.Services.AddJazorReload();
var app = builder.Build();

// 部署到子路径时在此调用 app.UsePathBase("/portal")。
app.UseJazorReload(); // Development 才生效；需位于 HTML 响应产生之前。
app.UseJazorHost();
app.MapGet("/api/health", () => new { status = "ok" });
app.UseJazorSpaFallback("index.html");
app.Run();
```

前提是 `wwwroot/index.html` 已包含应用自己的浏览器启动代码，且已构建出 `jazor/` 产物。此入口不生成 HTML shell，也不编译 C#/Razor。需要动态 HTML 时可使用 `UseJazorSpaFallback` 的 writer 重载；writer 自己设置 `ContentType` 并处理 HEAD，传入的取消令牌来自 `RequestAborted`。

`JazorWebApplication.CreateBuilder` 优先使用包含就绪产物的应用输出目录，其次调用源文件所在目录；没有产物时选择含 `wwwroot` 的输出目录或源文件目录。建议在宿主 `Program.cs` 直接调用，避免包装函数的 `CallerFilePath` 指向其他项目。

## 入口与中间件顺序

| 入口 | 职责 |
| --- | --- |
| `UseJazorHost` | 依次注册响应头和 `UseJazorAssets` |
| `UseJazorAssets` | 先挂载产物，再解析默认文件并托管 web root |
| `UseJazorArtifacts` | 只挂载生成产物，默认 `/jazor` |
| `UseJazorStaticFiles` | 托管静态文件，仅补充 `.map` 的 `application/json` 类型 |
| `UseJazorSecurityHeaders` | 响应开始前补充尚未设置的配置头 |
| `UseJazorSpaFallback` | 下游未处理的 HTML 导航使用静态页或自定义 writer |
| `AddJazorSsr` / `UseJazorSsr` | 分别注册 SSR 服务 / 将导航 fallback 改为 SSR |

`UsePathBase` 应先于上述入口；开发 reload 位于静态文件、SPA、SSR 之前。`UseJazorHost` 不自动添加 reload、SPA 或 SSR。常规 ASP.NET Core 路由、异常处理等仍由宿主配置。

SPA fallback 只在下游返回 **404**、响应尚未开始、没有选中 endpoint 时执行，并且只处理 GET/HEAD。默认排除 `/api`、`/assets`、`/health`、`/jazor` 路径段及带扩展名的路径。无 `Accept` 头允许通过；有该头时须包含可接受的 `text/html` 或 `application/xhtml+xml`，仅 `*/*` 不够。已匹配 API endpoint 返回的 404 不会被改成 HTML。

## 目录、缓存与自定义挂载

默认物理目录是 `ContentRootPath/jazor`。**注册中间件时**至少存在 `jazor-manifest.json` 或 `dist/bundle.js` 才挂载；不会在首个请求重新探测。如果先启动空宿主、后首次生成产物，需要重启。探测通过仅代表允许挂载，不保证 SSR 文件完整。

已挂载目录的缺失文件默认直接返回 404。资源默认补充 `nosniff` 和 `Cache-Control: no-cache, must-revalidate`，保留已有头；配置 `ImmutableCachePathPrefixes` 才采用一年 immutable 缓存，应仅用于内容版本化 URL。前缀匹配 `Request.Path`，不含 `PathBase`。`OnPrepareResponse` 最后执行，可覆盖默认头。

```csharp
app.UseJazorHost(options =>
{
    options.Assets.ConfigureArtifacts = artifacts =>
    {
        artifacts.RootPath = "generated"; // 相对 ContentRootPath
        artifacts.RequestPath = "/generated";
    };
});
app.UseJazorSpaFallback("index.html", options =>
    options.ExcludedPathPrefixes.Add("/generated"));
```

`ConfigureArtifacts` 在共享探测路径、缓存前缀及回调复制后执行，产物专用设置优先。修改 URL/目录时，SSR 和 [HMR 映射](../Jazor.AspNetCore.Dev/README.md) 也需与实际挂载对应。

## SSR 接入

以下是替代 SPA 示例的 `Program.cs`。将 `components/app.mjs` 替换为实际生成的根组件模块路径；props 的名称与类型应匹配组件契约。

```csharp
using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;

var builder = JazorWebApplication.CreateBuilder(args);
builder.Services.AddJazorReload();
builder.Services.AddJazorSsr(options => options.WorkerCount = 2);
var app = builder.Build();

app.UseJazorReload();
app.UseJazorHost();
app.UseJazorSsr((context, cancellationToken) =>
    Task.FromResult(new JazorSsrRequest(
        "components/app.mjs",
        Providers: [new JazorSsrProvider("request-path", context.Request.Path.Value)])));
app.Run();
```

Debug 使用物化模块图；Release 构建应配置 `JazorMode=release` 和 `JazorSSR=true`，例如：

```text
dotnet publish YourHost.csproj -c Release -p:JazorMode=release -p:JazorSSR=true
```

默认依次查找 `ContentRootPath/jazor/ssr`、`ContentRootPath/jazor`。SSR 根目录必须同时包含 `jazor-manifest.json`、`importmap.json`、`ssr-importmap.json`、`manifest.json` 及其引用的模块/资源；仅浏览器 bundle 不够。默认浏览器前缀分别为 `/jazor/ssr`、`/jazor`，由 `UseJazorHost` 的 `/jazor` 挂载覆盖。

自定义 SSR 根目录时通过 `AddJazorSsr` 配置 `ArtifactRootPath` 与 `RequestPath`，确保浏览器前缀能访问同一套资源；URL 会自动加上请求 `PathBase`。`MountElementId` 默认 `app`，不得为空或含空白。

SSR 复用 SPA 的导航筛选规则。GET 创建请求并渲染，HEAD 只设置 HTML 响应类型，不运行工厂或 renderer。`IJazorSsrRenderer.RenderAsync` 只返回组件 HTML 和 JSON；`UseJazorSsr` 再包装完整文档、import map、样式和 hydration 引导。

`WorkerCount` 默认 CPU 数限制在 1–4，必须大于零；Deno workers 按需创建、持久复用，由 DI 释放。每次渲染创建新 Vue app，但模块环境可跨请求复用。传递 `RequestAborted` 取消排队/渲染；异常通过正常 ASP.NET Core 异常管线处理。

固定 request/props 重载会跨请求共享对象；请求相关数据应由工厂创建。Props/providers 使用 `System.Text.Json` 序列化，同一快照交给浏览器 hydration，默认保留 CLR 属性名。Provider 键必须非空且唯一（区分大小写）。`Authentication` 自动占用 `jazor:auth-state`；不要同时手动提供该键。`FromPrincipal` 会复制全部 claims；它是快照转换，不执行认证或授权。

完整运行示例见 [Todo 宿主](../../samples/RazorVue.TodoList/Todo.Host/Program.cs)。

## 文档验证

```text
dotnet run --file scripts/csharp/verify-binding-documentation.cs -- --library Jazor.AspNetCore,Jazor.AspNetCore.Dev
```

可附加 `--baseline HEAD` 检查文档变更未改变 C# 代码令牌；`--no-build --packages <目录>` 校验 `Jazor` nupkg 内 XML 与构建输出一致。
