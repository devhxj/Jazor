using System.Text.Json;

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
            }
        }

        if (string.IsNullOrWhiteSpace(options.RootDirectory))
        {
            options = options with { RootDirectory = Directory.GetCurrentDirectory() };
        }

        options = ApplyConfigFile(options);

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

            if (TryGetOptionValue(arg, "--dev-frontend", out var frontendCompiler)
                && !string.IsNullOrWhiteSpace(frontendCompiler))
            {
                options = options with { FrontendCompiler = frontendCompiler.Trim().ToLowerInvariant() };
                continue;
            }

            if (TryGetOptionValue(arg, "--dev-proxy", out var proxyValue) &&
                TryParseProxyRule(proxyValue, out var proxyPrefix, out var proxyTarget))
            {
                options = ApplyProxyRule(options, proxyPrefix, proxyTarget);
            }
        }

        return options;
    }

    private static DevServerOptions ApplyConfigFile(DevServerOptions options)
    {
        var configPath = Path.Combine(options.RootDirectory, "jazor.config.json");
        if (!File.Exists(configPath))
        {
            return options;
        }

        JazorConfig? config;
        try
        {
            config = JsonSerializer.Deserialize<JazorConfig>(
                File.ReadAllText(configPath),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse dev-server config '{configPath}'.", ex);
        }

        if (config is null)
        {
            return options;
        }

        if (config.Server is not null)
        {
            if (config.Server.Port is { } port)
            {
                options = options with { Port = port };
            }

            if (!string.IsNullOrWhiteSpace(config.Server.Host))
            {
                options = options with { Host = config.Server.Host };
            }

            if (config.Server.Open is { } openBrowser)
            {
                options = options with { OpenBrowser = openBrowser };
            }

            if (config.Server.Hmr is { } hmrEnabled)
            {
                options = options with { HmrEnabled = hmrEnabled };
            }
        }

        if (config.Proxy is null)
        {
            return options;
        }

        foreach (var (prefix, proxyConfig) in config.Proxy)
        {
            if (!TryCreateProxyTarget(proxyConfig, out var proxyTarget))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(prefix) || !prefix.StartsWith("/", StringComparison.Ordinal))
            {
                continue;
            }

            options = ApplyProxyRule(options, prefix, proxyTarget);
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

    private static bool TryCreateProxyTarget(
        JazorProxyConfig proxyConfig,
        out ProxyTarget target)
    {
        target = default!;
        if (proxyConfig is null
            || string.IsNullOrWhiteSpace(proxyConfig.Target)
            || !Uri.TryCreate(proxyConfig.Target, UriKind.Absolute, out _))
        {
            return false;
        }

        target = new ProxyTarget
        {
            Target = proxyConfig.Target,
            Secure = proxyConfig.Secure ?? false,
            WebSocket = proxyConfig.WebSocket ?? true,
            RewritePath = proxyConfig.RewritePath
        };
        return true;
    }

    private static DevServerOptions ApplyProxyRule(
        DevServerOptions options,
        string proxyPrefix,
        ProxyTarget proxyTarget)
    {
        var proxyRules = new Dictionary<string, ProxyTarget>(options.ProxyRules, StringComparer.OrdinalIgnoreCase)
        {
            [proxyPrefix] = proxyTarget
        };
        return options with { ProxyRules = proxyRules };
    }
}
