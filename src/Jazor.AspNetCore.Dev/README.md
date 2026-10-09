# Jazor.AspNetCore.Dev

统一管理标准前端项目的开发启动、Vite 代理和发布产物托管。程序集与 XML 文档随 **Jazor** 包交付，不需要额外 NuGet 包。Development 环境由 DenoHost 使用随包运行时启动 Vite；其他环境直接托管 Release 产物。完整 SPA/SSR 接入见 [Jazor.AspNetCore](../Jazor.AspNetCore/README.md)。

## 标准前端宿主

宿主只配置一次项目目录、公开路径和 Vite origin：

```csharp
using Jazor.AspNetCore.Dev;

var builder = WebApplication.CreateBuilder(args);
builder.AddJazorFrontend(options =>
{
    options.PathBase = "/portal";       // 可选
    options.ProjectRootPath = "jazor";  // 相对 ContentRootPath 或绝对路径
    options.Vite.ServerOrigin = new Uri("http://127.0.0.1:5173");
});

var app = builder.Build();
app.UseJazorPathBase();

// 认证、授权和业务中间件按应用需要放置。
app.UseJazorFrontend();
app.Run();
```

`UseJazorPathBase` 独立存在，让应用决定 PathBase 在转发头、认证和重定向之前的准确位置；`PathBase` 为空时可省略。配置非空 `PathBase` 时必须调用它。`UseJazorFrontend` 负责开发代理或发布产物托管，并组合标准 `UseJazorHost` 资源管线。

通过 Startup、`IStartupFilter` 或 Furion 等框架组合宿主时，在服务阶段调用 `services.AddJazorFrontend(...)`，在框架传入的 `IApplicationBuilder` 上调用中间件：

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddJazorFrontend(options => options.PathBase = "/portal");
}

public void Configure(IApplicationBuilder app)
{
    app.UseJazorPathBase();
    app.UseJazorFrontend();
    // 继续注册业务路由或 SPA/SSR shell。
}
```

这些重载与 `WebApplication` 入口共用相同的配置、HTTP/WebSocket 代理和发布产物管线。必须使用框架传入的实际 builder；捕获另一个 `WebApplication` 并在其上注册无法保证中间件进入框架执行的管线。

Development 启动顺序如下：

1. 探测配置的 `DevelopmentEntryRelativePath`；已有 Vite 可用时复用它，不接管其生命周期。
2. 未就绪且 `LaunchServer=true` 时，通过 `DenoHost.Core.DenoProcess` 在项目根运行 `deno task dev --host ... --port ... --strictPort --base ...`。
3. 等待入口可访问，再开放 ASP.NET Core 宿主；HTTP、query 和 Vite HMR WebSocket 透明转发到同一 origin。
4. 宿主停止时只停止自己启动的进程树。

非 Development 环境不会探测或启动 Vite，而是从 `ProjectRootPath` 静态提供 `ReleaseEntryRelativePath` 所在的产物图。Visual Studio F5、Ctrl+F5 和 Folder Publish 都直接走该宿主契约；不需要启动脚本、全局 Deno、`PATH`/`DENO_DIR`、SpaProxy 或 Hosting Startup。

## 配置与浏览器 URL

| 选项 | 默认值与含义 |
| --- | --- |
| `RequestPath` | `/jazor`，不含应用 PathBase |
| `PathBase` | 空；配置后由 `UseJazorPathBase` 应用 |
| `ProjectRootPath` | `jazor`，相对 ContentRootPath 或绝对路径 |
| `DevelopmentEntryRelativePath` | `entry.js`，启动就绪探针与开发浏览器入口 |
| `ReleaseEntryRelativePath` | `dist/bundle.js`，非 Development 浏览器入口 |
| `Vite.ServerOrigin` | `http://127.0.0.1:5173` |
| `Vite.TaskName` | `dev` |
| `Vite.LaunchServer` | true；false 表示只使用外部服务器 |
| `Vite.StartupTimeout` | 2 分钟 |
| `Vite.ShutdownTimeout` | 5 秒 |

HTML shell 使用 `JazorFrontendUrls.GetDevelopmentClient(context)` 和 `JazorFrontendUrls.GetBrowserEntry(context)` 生成 URL，避免重复拼接 PathBase、`/jazor`、Vite client 和 Release bundle 路径。Vite 的 `base`、启动参数、代理目标和浏览器 URL 均来自同一份 options。

### 下一轮 Release 样式契约（尚未公开发布）

当前源码与配套本地候选包的标准 Vite build 使用相对 `base: './'`，分包和资源从入口 URL 所在目录解析。默认入口仍是 `/jazor/dist/bundle.js`，其资源使用 `/jazor/dist/assets/**`；非空 PathBase 或自定义 `RequestPath` 无需再次构建或添加静态别名。Development 继续使用配置的公共 base 与 Vite/HMR 代理。

`JazorFrontendUrls.GetStylesheets(context)` 返回 Release 入口静态依赖的 CSS URL，按依赖优先顺序去重。Development 返回空列表，样式由 Vite 提供。HTML shell 可逐个生成 `<link rel="stylesheet">`，不再扫描 `dist/assets`；dynamic imports 的 CSS 在加载模块时由 Vite 加载，不提前注入。

标准 build 在 Release 入口旁写 `manifest.json`，其中的 `file`、`imports` 与 `css` 是样式 helper 的唯一事实源，随 `dist/**` 一起 publish。元数据每个宿主只读取一次；部署后重启宿主。缺失或格式错误直接传播，不能退回目录扫描。

从 preview.8 升级时，Emit 保留既有作者配置。若 `vite.config.js` 是未修改的旧 SDK 默认文件，先移出生成目录，再由新 SDK 重新生成；若有作者改动，合并相对 build base 和 `build.manifest: 'manifest.json'`，保留自己的配置。替代 bundler 也应输出上述 manifest 契约。删除消费者的旧 `dist` 别名与 CSS 扫描后验证 entry、chunks、CSS、HEAD 和 PathBase。

如需连接由其他工具管理的 Vite：

```csharp
builder.AddJazorFrontend(options =>
{
    options.Vite.ServerOrigin = new Uri("http://127.0.0.1:4300");
    options.Vite.LaunchServer = false;
});
```

服务器未就绪时宿主会启动失败并给出明确错误，不会回退到其他端口、全局命令或静态开发文件。

## 可选宿主 reload

```csharp
using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;

var builder = JazorWebApplication.CreateBuilder(args);
builder.Services.AddJazorReload();
var app = builder.Build();

// 子路径部署先调用 app.UsePathBase("/portal")。
app.UseJazorReload();
app.UseJazorHost();
app.UseJazorSpaFallback("index.html");
app.Run();
```

`AddJazorReload` 注册配置和宿主服务，`UseJazorReload` 注册端点及 HTML 注入；两者均需要。Production 不启用观察或暴露 reload 端点。Development 未注册服务却调用中间件会抛出明确异常；同一管线重复调用中间件只注册一次。

reload 必须位于会产生 HTML 的静态文件、SPA fallback、SSR 之前。注入会缓冲适合检查的导航响应，仅修改 `text/html`，跳过 HEAD、204、304 和已经含 `Content-Encoding` 的响应。若使用响应压缩，把 `UseResponseCompression` 放在 `UseJazorReload` 前，使返回路径先注入、后压缩。

## 默认选项

| 选项 | 默认值与含义 |
| --- | --- |
| `ClientScriptPath` | `/@jazor/client`，GET/HEAD 加载 reload 模块 |
| `WebSocketPath` | `/@jazor/reload`，只接受 WebSocket 升级 |
| `WatchPaths` | 仅 `wwwroot`，相对 ContentRootPath；也支持绝对目录 |
| `HmrMappings` | 空；标准项目由 Vite 管理，旧协议须显式配置 |
| `DebounceInterval` | 100 ms，合并构建连续写入 |
| `PollingInterval` | 750 ms，与文件监听一起运行，补充遗漏事件 |
| `KeepAliveInterval` | 15 s，WebSocket transport 心跳 |
| `InjectHtml` | true，自动注入客户端模块 |
| `SuppressReconnectReloadForExternalRefresh` | true，外部刷新工具存在时抑制重复重连刷新 |
| `SuppressExternalRefreshPaths` | true，不重复观察外部刷新工具负责的 web root |

三个时间间隔必须大于零。客户端及 WebSocket 路径必须以 `/` 开头、不能为根路径且不能相同。配置路径不含 `PathBase`，实际请求 URL 自动加应用前缀。

## 自定义产物目录

服务配置与资源挂载需指向同一物理目录和 URL：

```csharp
builder.Services.AddJazorReload(options =>
{
    options.HmrMappings.Clear();
    options.HmrMappings.Add(new JazorHmrMapping
    {
        ArtifactRootPath = "generated",
        RequestPath = "/generated"
    });
    options.WatchPaths.Clear();
    options.WatchPaths.Add("wwwroot");
    // generated 已由映射自动加入观察，无需重复添加。
});
```

构建后输出必须确实位于 `ContentRootPath/generated`。同时设置 `UseJazorHost` 的 `Assets.ConfigureArtifacts`：`RootPath = "generated"`、`RequestPath = "/generated"`；SPA fallback 排除该前缀。SSR 自定义路径配置也应保持一致。

只有 manifest 确认变化处于 Vue **template-only** 边界时才发送模块更新；逻辑变更、无法确定边界或没有映射时使用整页刷新。配置 HMR 映射并不意味着任意 C# 改动都能保留组件状态。

## 排查刷新不生效

1. 确认环境为 Development，且 `AddJazorReload`、`UseJazorReload` 都已调用。
2. 检查 HTML 是否包含 reload script；若开启压缩，确认先注入后压缩。禁用 `InjectHtml` 时需自己加载客户端。
3. 检查浏览器的客户端模块请求和 WebSocket 升级；子路径部署 URL 必须带应用前缀。
4. 确认编译已更新观察目录中的文件，而不只是源 `.razor` 文件发生变化。
5. 确认 HMR 映射、产物 HTTP 挂载和 ContentRootPath 一致；逻辑改动发生整页刷新是预期行为。

手动注入且部署到 `/portal` 时，客户端标签使用：

```html
<script type="module" src="/portal/@jazor/client" data-jazor-path-base="/portal"></script>
```

XML 生成、公开 API 注释及包内文档验证命令见 [宿主文档验证](../Jazor.AspNetCore/README.md#文档验证)。
