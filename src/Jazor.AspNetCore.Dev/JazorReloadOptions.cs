using Microsoft.AspNetCore.Http;

namespace Jazor.AspNetCore.Dev;

/// <summary>Configures the development-only Jazor reload transport and file observation.</summary>
public sealed class JazorReloadOptions
{
    /// <summary>Initializes development reload with project-root artifacts and web-root assets observed.</summary>
    public JazorReloadOptions()
    {
        WatchPaths =
        [
            "jazor",
            "wwwroot"
        ];

        HmrMappings =
        [
            new JazorHmrMapping()
        ];
    }

    /// <summary>Browser endpoint that serves the reload client module.</summary>
    /// <remarks>默认 /@jazor/client，支持 GET/HEAD；必须为非根路径，且不同于 WebSocketPath。不包含 PathBase。</remarks>
    public PathString ClientScriptPath { get; set; } = new("/@jazor/client");

    /// <summary>Browser endpoint used by the reload WebSocket transport.</summary>
    /// <remarks>默认 /@jazor/reload，只接受 WebSocket 升级；必须为非根路径且不同于 ClientScriptPath。不包含 PathBase。</remarks>
    public PathString WebSocketPath { get; set; } = new("/@jazor/reload");

    /// <summary>Content-root paths observed for generated artifacts and authored static files.</summary>
    /// <remarks>默认 jazor 与 wwwroot。相对路径基于 ContentRootPath，也可使用绝对目录；文件监听与周期轮询共同观察变化。</remarks>
    public IList<string> WatchPaths { get; }

    /// <summary>
    /// Maps generated artifact roots to browser URLs. A module update is emitted only
    /// when the manifest proves the change stays inside a Vue template-only boundary.
    /// </summary>
    /// <remarks>默认将 jazor 目录映射到 /jazor。映射目录自动加入观察范围，无需重复添加到 WatchPaths。仅 manifest 确认的 template-only 变化使用 Vue HMR，其余变化整页刷新。</remarks>
    public IList<JazorHmrMapping> HmrMappings { get; }

    /// <summary>Quiet period used to coalesce a single build's file writes.</summary>
    /// <remarks>默认 100 毫秒，必须大于零；用于合并一次构建的连续写入。</remarks>
    public TimeSpan DebounceInterval { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>Polling interval used alongside filesystem watcher events.</summary>
    /// <remarks>默认 750 毫秒，必须大于零；轮询与文件监听同时运行，用于发现遗漏事件。</remarks>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMilliseconds(750);

    /// <summary>WebSocket heartbeat interval used by the ASP.NET Core transport.</summary>
    /// <remarks>默认 15 秒，必须大于零；传递给 ASP.NET Core WebSocket transport。</remarks>
    public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>Determines whether HTML navigation responses receive the client script.</summary>
    /// <remarks>默认 true；为适合注入的 HTML 导航响应加入客户端模块。跳过 HEAD、204、304 及已编码响应；压缩中间件应先于 reload 注册，使返回时先注入后压缩。禁用时需自行加载 ClientScriptPath，并提供应用 PathBase。</remarks>
    public bool InjectHtml { get; set; } = true;

    /// <summary>Prevents duplicate full reloads when the host already injects browser refresh tooling.</summary>
    /// <remarks>默认 true；检测到外部 browser refresh 工具时，抑制连接恢复引发的重复整页刷新。</remarks>
    public bool SuppressReconnectReloadForExternalRefresh { get; set; } = true;

    /// <summary>Skips web-root paths when an external browser refresh service already watches them.</summary>
    /// <remarks>默认 true；外部 browser refresh 已观察 web root 时，不重复观察这些路径。</remarks>
    public bool SuppressExternalRefreshPaths { get; set; } = true;
}
