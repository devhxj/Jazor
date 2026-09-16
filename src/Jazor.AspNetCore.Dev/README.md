# Jazor.AspNetCore.Dev

Development 环境下的 HTML 客户端注入、WebSocket 通知、文件观察和 Vue 模板 HMR。程序集与 XML 文档随 **Jazor** 包交付。它观察构建后的文件，**不会执行 C#/Razor 编译**；用 `dotnet watch` 或构建命令持续产生新产物。完整 SPA/SSR 接入见 [Jazor.AspNetCore](../Jazor.AspNetCore/README.md)。

## 最小接入

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
| `WatchPaths` | `jazor`、`wwwroot`，相对 ContentRootPath；也支持绝对目录 |
| `HmrMappings` | `jazor` → `/jazor`；映射目录自动加入观察范围 |
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
