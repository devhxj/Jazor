# Jazor.AspNetCore.Dev

这是 ASP.NET Core 宿主的前端集成层，程序集与 XML 文档随 **Jazor** 包交付。编写和发布普通 C# ES 模块类库只需 Jazor SDK。需要让浏览器从一个 ASP.NET Core 地址访问页面、模块和开发期热更新时，宿主可以接入本程序集。

| 项目 | 谁管理前端服务 | ASP.NET Core 如何接入 |
| --- | --- | --- |
| 普通 C# 模块应用，如 Wiki | Development 中，宿主默认通过 NodeApi 启动和关闭 Vite。已有服务就绪时直接连接 | 开发时代理 HTTP 和 HMR WebSocket，部署后提供已发布的文件。[Wiki 入口](../../samples/Wiki/Program.cs) |
| 含 `.jazor` 的 Web 应用，如 Jero | F5 或 `dotnet run` 启动 Web 宿主，由宿主管理 Jolt 开发会话。Jolt 编译组件并驱动 Vite | Startup 直接注册 SPA 宿主。开发期转发 Vite 流量，发布后提供静态资源与 SPA 深链接回退。[Jero Startup](../../samples/Jero/src/Jero.Web/Startup.cs) |

编辑器直接连接 Jolt 语言服务，补全和跳转无需启动 Web 宿主。[工具链指南](../../docs/03-guides/toolchain-migration.md) · [ASP.NET Core 的 SPA 与 SSR](../Jazor.AspNetCore/README.md)

## 标准前端宿主

在 Web 宿主中注册 SPA 集成。部署在子路径时可以指定 `PathBase`。

```csharp
using Jazor.AspNetCore.Dev;

var builder = WebApplication.CreateBuilder(args);
builder.AddJazorSPA(options =>
{
    options.PathBase = "/portal";       // 可选
});

var app = builder.Build();
app.UseJazorPathBase();

// 认证、授权和业务中间件按应用需要放置。
app.UseJazorSPA();
app.Run();
```

`UseJazorPathBase` 独立存在，让应用决定 PathBase 在转发头、认证和重定向之前的准确位置；`PathBase` 为空时可省略。配置非空 `PathBase` 时必须调用它。`UseJazorSPA` 负责开发代理或发布产物托管，并组合标准 `UseJazorHost` 资源管线。

通过 Startup、`IStartupFilter` 或 Furion 等框架组合宿主时，在服务阶段调用 `services.AddJazorSPA(...)`，在框架传入的 `IApplicationBuilder` 上调用中间件：

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddJazorSPA(options => options.PathBase = "/portal");
}

public void Configure(IApplicationBuilder app)
{
    app.UseJazorPathBase();
    app.UseJazorSPA();
    // 继续注册业务路由或 SPA/SSR shell。
}
```

这些重载与 `WebApplication` 入口共用相同的配置、HTTP/WebSocket 代理和发布产物管线。必须使用框架传入的实际 builder；捕获另一个 `WebApplication` 并在其上注册无法保证中间件进入框架执行的管线。

Development 启动顺序如下：

1. 按 F5 或运行 `dotnet run` 时，Jolt Web 宿主启动自己的 Jolt 会话。使用默认配置时，Vite 选择可用端口，宿主读取这次启动实际绑定的地址。并行运行两个项目时各使用自己的会话。
2. 显式设置 `Vite.LaunchServer=false` 时，宿主探测 `Vite.ServerOrigin` 的应用入口和 Vite 客户端，连接已启动的外部会话。普通 ES 模块宿主支持复用已有 Vite，也可以在进程内通过 NodeApi 启动 Vite。
3. 入口就绪后，宿主将 HTTP、query 和 HMR WebSocket 转发到同一 origin。停止宿主时释放它启动的开发服务。

发布后的宿主从 `wwwroot/` 提供客户端构建产物。在项目目录中运行 Release 构建时也可以读取 `$(JazorDir)/dist/`。Visual Studio 的 F5、Ctrl+F5 和 Folder Publish 共用这些入口。

## 配置与浏览器 URL

宿主根据应用程序集已有的 `JoltHost` 或 `JoltTool` 元数据选择 Jolt Web 默认值。配置回调可以覆盖这些默认值。

| 选项 | 普通 ES 模块默认值 | Jolt Web 默认值 |
| --- | --- | --- |
| `RequestPath` | `/jazor` | 空，Development 代理使用 `/__jolt` |
| `ProjectRootPath` | `JazorDir`，默认 `.jazor` | `.` |
| `DevelopmentEntryRelativePath` | `entry.js` | `index.html` |
| `ReleaseEntryRelativePath` | `dist/bundle.js` | `dist/index.html` |

`ProjectRootPath` 可使用相对 ContentRootPath 的路径或绝对路径。两种宿主共用以下设置：

| 选项 | 默认值与含义 |
| --- | --- |
| `PathBase` | 空。Jero 读取 `SPA:PathBase`，并通过 `UseJazorPathBase` 应用 |
| `Vite.ServerOrigin` | 显式附接的默认地址为 `http://127.0.0.1:5173`。Jolt 自启时使用自己报告的地址。 |
| `Vite.LaunchServer` | true；false 表示只使用外部服务器 |
| `Vite.StartupTimeout` | 2 分钟 |
| `Vite.ShutdownTimeout` | 5 秒 |

Jolt Web 宿主在服务端路由之后用 HTML 入口响应符合条件的浏览器导航，`JazorSPAOptions.Fallback` 提供回退匹配配置。普通 ES 模块宿主可在自己的 HTML shell 中用 `JazorSPAUrls.GetDevelopmentClient(context)` 和 `JazorSPAUrls.GetBrowserEntry(context)` 生成 URL。Vite 的 `base`、启动参数、代理目标和浏览器 URL 均来自同一份 options。

`Fallback.ExcludedPathPrefixes` 相对于 SPA 挂载点。例如挂载 `/frontend` 时，排除 `/api` 会保留 `/frontend/api` 下的服务端响应。HTML 中作者显式提供的 `<base>` 优先；未提供时，宿主以当前请求的 PathBase 和 SPA 挂载点生成 base。生产 HTML 模板在首次读取后缓存，部署新产物后重启宿主。

### Release 样式契约

标准 Vite build 使用相对 `base: './'`，分包和资源从入口 URL 所在目录解析。默认入口是 `/jazor/bundle.js`，其资源使用 `/jazor/assets/**`；非空 PathBase 或自定义 `RequestPath` 无需再次构建或添加静态别名。Development 使用配置的公共 base 与 Vite/HMR 代理。

`JazorSPAUrls.GetStylesheets(context)` 返回 Release 入口静态依赖的 CSS URL，按依赖优先顺序去重。Development 返回空列表，样式由 Vite 提供。HTML shell 可逐个生成 `<link rel="stylesheet">`，不再扫描 `dist/assets`；dynamic imports 的 CSS 在加载模块时由 Vite 加载，不提前注入。

标准 build 在 Release 入口旁写 `manifest.json`，其中的 `file`、`imports` 与 `css` 是样式 helper 的唯一事实源，随 `.jazor/dist/` 中的浏览器文件发布到 `wwwroot/`。元数据每个宿主只读取一次；部署后重启宿主。缺失或格式错误直接传播，不能退回目录扫描。

升级旧构建布局时，Emit 保留既有作者配置。若 `vite.config.js` 是未修改的旧 SDK 默认文件，先移出生成目录，再由新 SDK 重新生成；若有作者改动，合并相对 build base 和 `build.manifest: 'manifest.json'`，保留自己的配置。构建定制使用 Vite 配置和上述 manifest 契约。删除消费者的旧 `dist` 别名与 CSS 扫描后验证 entry、chunks、CSS、HEAD 和 PathBase。

如需连接由其他工具管理的 Vite：

```csharp
builder.AddJazorSPA(options =>
{
    options.Vite.ServerOrigin = new Uri("http://127.0.0.1:4300");
    options.Vite.LaunchServer = false;
});
```

服务器未就绪时宿主会启动失败并给出明确错误，不会回退到其他端口、全局命令或静态开发文件。

XML 生成、公开 API 注释及包内文档验证命令见 [宿主文档验证](../Jazor.AspNetCore/README.md#文档验证)。
