using System.Globalization;

namespace Jolt.DevServer;

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

            if (TryGetOptionValue(arg, "--dev-port", out var portValue))
            {
                options = options with { Port = ParsePortOption("--dev-port", portValue) };
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
                options = options with { VolarCompiler = frontendCompiler.Trim().ToLowerInvariant() };
                continue;
            }

            if (TryGetOptionValue(arg, "--dev-proxy", out var proxyValue))
            {
                if (!TryParseProxyRule(proxyValue, out var proxyPrefix, out var proxyTarget))
                {
                    throw CreateInvalidOptionException(
                        "--dev-proxy",
                        proxyValue,
                        "a rule like /api=http://localhost:5000");
                }

                options = ApplyProxyRule(options, proxyPrefix, proxyTarget);
                continue;
            }

            if (TryGetOptionValue(arg, "--dev-alias", out var aliasValue))
            {
                if (!TryParseAliasRule(aliasValue, out var aliasPrefix, out var aliasTarget))
                {
                    throw CreateInvalidOptionException(
                        "--dev-alias",
                        aliasValue,
                        "a rule like @=/src");
                }

                options = ApplyResolveAlias(options, aliasPrefix, aliasTarget);
                continue;
            }
        }

        return options;
    }

    private static DevServerOptions ApplyConfigFile(DevServerOptions options)
    {
        var config = JoltConfigLoader.Load(options.RootDirectory);

        if (config is null)
        {
            return options;
        }

        if (config.Server is not null)
        {
            if (config.Server.Port is { } port)
            {
                options = options with
                {
                    Port = ValidatePortValue("jolt.config.json server.port", port)
                };
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

        if (config.Resolve?.Alias is not null)
        {
            foreach (var (prefix, target) in config.Resolve.Alias)
            {
                if (string.IsNullOrWhiteSpace(prefix) || string.IsNullOrWhiteSpace(target))
                {
                    throw CreateInvalidOptionException(
                        "jolt.config.json resolve.alias",
                        $"{prefix}={target}",
                        "non-empty alias mappings");
                }

                options = ApplyResolveAlias(options, prefix.Trim(), target.Trim());
            }
        }

        if (config.Proxy is not null)
        {
            foreach (var (prefix, proxyConfig) in config.Proxy)
            {
                if (!TryCreateProxyTarget(proxyConfig, out var proxyTarget))
                {
                    throw CreateInvalidOptionException(
                        "jolt.config.json proxy",
                        $"{prefix}={proxyConfig?.Target}",
                        "proxy targets with an absolute URI");
                }

                if (string.IsNullOrWhiteSpace(prefix) || !prefix.StartsWith("/", StringComparison.Ordinal))
                {
                    throw CreateInvalidOptionException(
                        "jolt.config.json proxy",
                        prefix ?? string.Empty,
                        "proxy prefixes that start with '/'");
                }

                options = ApplyProxyRule(options, prefix, proxyTarget);
            }
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

    private static bool TryParseAliasRule(
        string value,
        out string prefix,
        out string target)
    {
        prefix = string.Empty;
        target = string.Empty;

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
        target = value[(separatorIndex + 1)..].Trim();
        if (string.IsNullOrWhiteSpace(prefix) || string.IsNullOrWhiteSpace(target))
        {
            prefix = string.Empty;
            target = string.Empty;
            return false;
        }

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

    private static DevServerOptions ApplyResolveAlias(
        DevServerOptions options,
        string aliasPrefix,
        string aliasTarget)
    {
        var aliases = new Dictionary<string, string>(options.ResolveAliases, StringComparer.Ordinal)
        {
            [aliasPrefix] = aliasTarget
        };
        return options with { ResolveAliases = aliases };
    }

    internal static int ParsePortOption(string optionName, string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPort))
        {
            throw CreateInvalidOptionException(
                optionName,
                value,
                "an integer between 0 and 65535");
        }

        return ValidatePortValue(optionName, parsedPort);
    }

    private static int ValidatePortValue(string optionName, int port)
    {
        if (port is < 0 or > 65535)
        {
            throw CreateInvalidOptionException(
                optionName,
                port.ToString(CultureInfo.InvariantCulture),
                "an integer between 0 and 65535");
        }

        return port;
    }

    private static InvalidOperationException CreateInvalidOptionException(
        string optionName,
        string value,
        string expected)
        => new($"Invalid value '{value}' for {optionName}. Expected {expected}.");
}
