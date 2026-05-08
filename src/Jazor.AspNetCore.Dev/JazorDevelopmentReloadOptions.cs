using Microsoft.AspNetCore.Http;

namespace Jazor.AspNetCore.Dev;

public sealed class JazorDevelopmentReloadOptions
{
    public PathString ClientScriptPath { get; set; } = new("/@jazor/client");

    public PathString WebSocketPath { get; set; } = new("/@jazor/reload");

    public IList<string> WatchRootPaths { get; } = new List<string>
    {
        "jazor",
        "wwwroot"
    };

    public TimeSpan FileChangeDebounceInterval { get; set; } = TimeSpan.FromMilliseconds(100);

    public TimeSpan FileChangePollingInterval { get; set; } = TimeSpan.FromMilliseconds(750);

    public TimeSpan WebSocketKeepAliveInterval { get; set; } = TimeSpan.FromSeconds(15);

    public bool InjectHtmlResponses { get; set; } = true;

    public bool SuppressReloadOnReconnectWhenExternalBrowserRefreshIsActive { get; set; } = true;

    public bool SuppressWatchRootsHandledByExternalBrowserRefresh { get; set; } = true;
}
