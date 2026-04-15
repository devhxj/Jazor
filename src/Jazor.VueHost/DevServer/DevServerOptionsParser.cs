namespace Jazor.VueHost.DevServer;

internal static class DevServerOptionsParser
{
    public static DevServerOptions Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var options = new DevServerOptions();
        foreach (var arg in args)
        {
            if (TryGetOptionValue(arg, "--dev-root", out var rootDirectory) &&
                !string.IsNullOrWhiteSpace(rootDirectory))
            {
                options = options with { RootDirectory = Path.GetFullPath(rootDirectory) };
                continue;
            }

            if (TryGetOptionValue(arg, "--dev-port", out var portValue) &&
                int.TryParse(portValue, out var port))
            {
                options = options with { Port = port };
                continue;
            }

            if (TryGetOptionValue(arg, "--dev-host", out var hostValue) &&
                !string.IsNullOrWhiteSpace(hostValue))
            {
                options = options with { Host = hostValue };
                continue;
            }

            if (string.Equals(arg, "--open-browser", StringComparison.OrdinalIgnoreCase))
            {
                options = options with { OpenBrowser = true };
                continue;
            }

            if (string.Equals(arg, "--no-hmr", StringComparison.OrdinalIgnoreCase))
            {
                options = options with { HmrEnabled = false };
                continue;
            }

            if (TryGetOptionValue(arg, "--dev-proxy", out var proxyValue) &&
                TryParseProxyRule(proxyValue, out var proxyPrefix, out var proxyTarget))
            {
                var proxyRules = new Dictionary<string, ProxyTarget>(options.ProxyRules, StringComparer.OrdinalIgnoreCase)
                {
                    [proxyPrefix] = proxyTarget
                };
                options = options with { ProxyRules = proxyRules };
            }
        }

        if (string.IsNullOrWhiteSpace(options.RootDirectory))
        {
            options = options with { RootDirectory = Directory.GetCurrentDirectory() };
        }

        return options;
    }

    private static bool TryGetOptionValue(string arg, string optionName, out string value)
    {
        var prefix = optionName + "=";
        if (arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            value = arg[prefix.Length..];
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static bool TryParseProxyRule(
        string value,
        out string prefix,
        out ProxyTarget target)
    {
        prefix = string.Empty;
        target = default!;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var separatorIndex = value.IndexOf('=');
        if (separatorIndex <= 0 || separatorIndex == value.Length - 1)
        {
            return false;
        }

        prefix = value[..separatorIndex].Trim();
        var targetValue = value[(separatorIndex + 1)..].Trim();
        if (string.IsNullOrWhiteSpace(prefix)
            || !prefix.StartsWith("/", StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(targetValue)
            || !Uri.TryCreate(targetValue, UriKind.Absolute, out _))
        {
            prefix = string.Empty;
            return false;
        }

        target = new ProxyTarget
        {
            Target = targetValue
        };
        return true;
    }
}
