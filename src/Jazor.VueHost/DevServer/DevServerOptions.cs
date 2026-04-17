namespace Jazor.VueHost.DevServer;

internal sealed record DevServerOptions
{
    public string RootDirectory { get; init; } = Directory.GetCurrentDirectory();

    public int Port { get; init; } = 5173;

    public string Host { get; init; } = "localhost";

    public bool OpenBrowser { get; init; }

    public bool HmrEnabled { get; init; } = true;

    public string FrontendCompiler { get; init; } = "deno";

    public IReadOnlyDictionary<string, ProxyTarget> ProxyRules { get; init; }
        = new Dictionary<string, ProxyTarget>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, string> ResolveAliases { get; init; }
        = new Dictionary<string, string>(StringComparer.Ordinal);
}
