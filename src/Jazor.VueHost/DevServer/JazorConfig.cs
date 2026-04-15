namespace Jazor.VueHost.DevServer;

internal sealed class JazorConfig
{
    public JazorServerConfig? Server { get; init; }

    public Dictionary<string, JazorProxyConfig>? Proxy { get; init; }
}

internal sealed class JazorServerConfig
{
    public int? Port { get; init; }

    public string? Host { get; init; }

    public bool? Open { get; init; }

    public bool? Hmr { get; init; }
}

internal sealed class JazorProxyConfig
{
    public string? Target { get; init; }

    public bool? Secure { get; init; }

    public bool? WebSocket { get; init; }

    public string? RewritePath { get; init; }
}
