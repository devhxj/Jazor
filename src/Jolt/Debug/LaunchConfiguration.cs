using System.Text.Json;

namespace Jolt.Debug;

internal sealed class LaunchConfiguration
{
    public string? Name { get; init; }

    public string? Type { get; init; }

    public string? Request { get; init; }

    public string? Url { get; init; }

    public string? WebRoot { get; init; }

    public string? CdpWebSocketUrl { get; init; }

    public static LaunchConfiguration? ResolveFromArgs(string[] args, string workingDirectory)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);

        var launchFilePath = GetOptionValue(args, "--launch-config");
        if (!string.IsNullOrWhiteSpace(launchFilePath))
        {
            var resolvedPath = Path.IsPathRooted(launchFilePath)
                ? launchFilePath
                : Path.Combine(workingDirectory, launchFilePath);
            var loadedConfiguration = TryLoad(resolvedPath);
            if (loadedConfiguration is not null)
            {
                return loadedConfiguration;
            }
        }

        var cdpEndpoint = GetOptionValue(args, "--dap-cdp-ws");
        return string.IsNullOrWhiteSpace(cdpEndpoint)
            ? null
            : new LaunchConfiguration
            {
                Type = "jolt",
                Request = "launch",
                CdpWebSocketUrl = cdpEndpoint
            };
    }

    public static LaunchConfiguration? TryLoad(string launchJsonPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(launchJsonPath);
        if (!File.Exists(launchJsonPath))
        {
            return null;
        }

        return TryParse(File.ReadAllText(launchJsonPath));
    }

    internal static LaunchConfiguration? TryParse(string launchJson)
    {
        if (string.IsNullOrWhiteSpace(launchJson))
        {
            return null;
        }

        using var document = JsonDocument.Parse(launchJson);
        if (!TryGetProperty(document.RootElement, "configurations", out var configurations)
            || configurations.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        JsonElement? fallback = null;
        JsonElement? chromeFallback = null;
        foreach (var configuration in configurations.EnumerateArray())
        {
            fallback ??= configuration;
            var type = TryGetString(configuration, "type");
            if (string.Equals(type, "jolt", StringComparison.OrdinalIgnoreCase)
            )
            {
                return Create(configuration);
            }

            if (chromeFallback is null
                && (string.Equals(type, "pwa-chrome", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(type, "chrome", StringComparison.OrdinalIgnoreCase)))
            {
                chromeFallback = configuration;
            }
        }

        if (chromeFallback is not null)
        {
            return Create(chromeFallback.Value);
        }

        return fallback is null ? null : Create(fallback.Value);
    }

    private static LaunchConfiguration Create(JsonElement configuration)
        => new()
        {
            Name = TryGetString(configuration, "name"),
            Type = TryGetString(configuration, "type"),
            Request = TryGetString(configuration, "request"),
            Url = TryGetString(configuration, "url"),
            WebRoot = TryGetString(configuration, "webRoot"),
            CdpWebSocketUrl =
                TryGetString(configuration, "cdpWebSocketUrl")
                ?? TryGetString(configuration, "cdpWsUrl")
                ?? TryGetString(configuration, "webSocketUrl")
                ?? TryGetString(configuration, "cdpEndpoint")
        };

    private static string? GetOptionValue(string[] args, string optionName)
    {
        foreach (var arg in args)
        {
            if (arg.StartsWith(optionName + "=", StringComparison.OrdinalIgnoreCase))
            {
                return arg[(optionName.Length + 1)..];
            }
        }

        return null;
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(propertyName, out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    private static string? TryGetString(JsonElement element, string propertyName)
        => TryGetProperty(element, propertyName, out var value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
