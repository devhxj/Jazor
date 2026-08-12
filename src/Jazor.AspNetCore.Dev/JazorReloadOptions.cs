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
    public PathString ClientScriptPath { get; set; } = new("/@jazor/client");

    /// <summary>Browser endpoint used by the reload WebSocket transport.</summary>
    public PathString WebSocketPath { get; set; } = new("/@jazor/reload");

    /// <summary>Content-root paths observed for generated artifacts and authored static files.</summary>
    public IList<string> WatchPaths { get; }

    /// <summary>
    /// Maps generated artifact roots to browser URLs. A module update is emitted only
    /// when the manifest proves the change stays inside a Vue template-only boundary.
    /// </summary>
    public IList<JazorHmrMapping> HmrMappings { get; }

    /// <summary>Quiet period used to coalesce a single build's file writes.</summary>
    public TimeSpan DebounceInterval { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>Polling fallback interval for file systems that drop watcher events.</summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMilliseconds(750);

    /// <summary>WebSocket heartbeat interval used by the ASP.NET Core transport.</summary>
    public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>Determines whether HTML navigation responses receive the client script.</summary>
    public bool InjectHtml { get; set; } = true;

    /// <summary>Prevents duplicate full reloads when the host already injects browser refresh tooling.</summary>
    public bool SuppressReconnectReloadForExternalRefresh { get; set; } = true;

    /// <summary>Skips web-root paths when an external browser refresh service already watches them.</summary>
    public bool SuppressExternalRefreshPaths { get; set; } = true;
}
